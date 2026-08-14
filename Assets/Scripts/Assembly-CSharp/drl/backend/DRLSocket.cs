using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Authentication;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;
using WebSocketSharp;

namespace drl.backend
{
	public class DRLSocket<T>
	{
		public WebSocket m_socket;

		private Dictionary<string, List<Action<T>>> m_handlers = new Dictionary<string, List<Action<T>>>();

		public Action Connected;

		private object eventQueueLock = new object();

		private object connectQueueLock = new object();

		private Queue<Tuple<string, string>> eventsQueue = new Queue<Tuple<string, string>>();

		private Queue<Tuple<string, string>> connectEventsQueue = new Queue<Tuple<string, string>>();

		private int m_pingInterval = 25;

		private int m_pingTimeout = 5;

		private int m_reconnectDelay = 3;

		private volatile bool m_pinging;

		private volatile bool m_hasPong;

		private volatile bool m_connected;

		private volatile bool m_connecting;

		private volatile bool m_disconnecting;

		private Thread m_hearbeatThread;

		private Thread m_reconnectThread;

		private Stopwatch debugTimer = new Stopwatch();

		private string socketUrl = "";

		private bool m_forceReconnect;

		private bool m_needsCleanup;

		private float m_connectEventDelayTimer;

		private float m_reconnectDelayTimer;

		public void StartConnect(string p_url)
		{
			socketUrl = p_url;
			lock (connectQueueLock)
			{
				connectEventsQueue.Enqueue(new Tuple<string, string>("socket-connect", p_url));
			}
		}

		public void StartDisconnect()
		{
			lock (connectQueueLock)
			{
				connectEventsQueue.Enqueue(new Tuple<string, string>("socket-disconnect", ""));
			}
		}

		private void Connect(string p_url)
		{
			string text = p_url;
			if (string.IsNullOrEmpty(text))
			{
				UnityEngine.Debug.LogWarning("DRLSocket> Error: URL not defined.");
				return;
			}
			if (text.StartsWith("http"))
			{
				text = text.Replace("http", "ws");
			}
			if (text.StartsWith("https"))
			{
				text = text.Replace("https", "ws");
			}
			if (m_connected)
			{
				m_socket?.Close();
			}
			m_connected = false;
			try
			{
				m_connecting = true;
				StartConnectionAsync(p_url);
			}
			catch (Exception ex)
			{
				m_connecting = false;
				UnityEngine.Debug.LogWarning("DRLSocket> Error: error occurred while connecting - " + ex.Message);
			}
		}

		private void Disconnect()
		{
			if (!m_connected)
			{
				return;
			}
			try
			{
				m_disconnecting = true;
				SendMessage(EnginePacketType.MESSAGE, SocketPacketType.DISCONNECT, delegate
				{
					m_socket.CloseAsync(CloseStatusCode.Normal);
				});
			}
			catch (Exception ex)
			{
				m_disconnecting = false;
				UnityEngine.Debug.LogWarning("DRLSocket> Error: error occurred while disconnecting - " + ex.Message);
			}
		}

		private void StartConnectionAsync(string p_url)
		{
			if (m_socket != null)
			{
				UnityEngine.Debug.LogWarning("DRLSocket> Error: Socket already exists, can't start new connection.");
			}
			m_socket = new WebSocket(p_url);
			m_socket.SslConfiguration.EnabledSslProtocols = SslProtocols.Default | SslProtocols.Tls11 | SslProtocols.Tls12;
			m_socket.ConnectAsync();
			m_socket.OnOpen += OnOpen;
			m_socket.OnClose += OnClose;
			m_socket.OnError += OnError;
			m_socket.OnMessage += OnMessage;
		}

		private void OnOpen(object sender, EventArgs e)
		{
			UnityEngine.Debug.Log("DRLSocket> Socket connection established.\n" + DateTime.Now.ToString());
			debugTimer.Start();
			m_connected = true;
			m_connecting = false;
			m_disconnecting = false;
			m_hearbeatThread = new Thread(StartHeartbeat);
			m_hearbeatThread.Start();
			m_reconnectThread = new Thread(HandleReconnect);
			m_reconnectThread.Start();
			if (Connected != null)
			{
				Connected();
			}
		}

		private void OnError(object sender, ErrorEventArgs e)
		{
			UnityEngine.Debug.LogWarning("DRLSocket> Error: " + e.Message);
		}

		private void OnClose(object sender, CloseEventArgs e)
		{
			m_connected = false;
			m_disconnecting = false;
			m_connecting = false;
			UnityEngine.Debug.Log($"DRLSocket> Socket connection closed. Connection duration:[{debugTimer.Elapsed.TotalSeconds}]" + DateTime.Now.ToString());
			UnsubscribeAll();
			m_socket.OnOpen -= OnOpen;
			m_socket.OnError -= OnError;
			m_socket.OnMessage -= OnMessage;
			m_socket.OnClose -= OnClose;
			m_socket = null;
			if (e != null && !e.WasClean && !string.IsNullOrEmpty(socketUrl))
			{
				UnityEngine.Debug.LogWarning("DRLSocket> Connection dropped unexpectedly - trying to reconnect...");
				m_forceReconnect = true;
				m_reconnectDelayTimer = 3.5f;
				m_needsCleanup = false;
			}
			else
			{
				m_forceReconnect = false;
				m_needsCleanup = true;
			}
		}

		public void OnUpdate()
		{
			if (m_needsCleanup)
			{
				m_needsCleanup = false;
				m_connected = false;
				m_hearbeatThread.Abort();
				m_reconnectThread.Abort();
			}
			if (m_forceReconnect)
			{
				if (m_reconnectDelayTimer > 0f)
				{
					m_reconnectDelayTimer -= Time.deltaTime;
				}
				else
				{
					m_forceReconnect = false;
					if (!string.IsNullOrEmpty(socketUrl))
					{
						StartConnect(socketUrl);
					}
				}
			}
			lock (eventQueueLock)
			{
				while (eventsQueue.Count > 0)
				{
					Tuple<string, string> tuple = eventsQueue.Dequeue();
					RaiseEvent(tuple.Item1, tuple.Item2);
				}
			}
			lock (connectQueueLock)
			{
				if (m_connecting || m_disconnecting)
				{
					return;
				}
				if (m_connectEventDelayTimer > 0f)
				{
					m_connectEventDelayTimer -= Time.deltaTime;
				}
				else if (connectEventsQueue.Count > 0)
				{
					Tuple<string, string> tuple2 = connectEventsQueue.Dequeue();
					if (tuple2.Item1 == "socket-connect")
					{
						Connect(tuple2.Item2);
						m_connectEventDelayTimer = 3f;
					}
					if (tuple2.Item1 == "socket-disconnect")
					{
						Disconnect();
						m_connectEventDelayTimer = 3f;
					}
				}
			}
		}

		private void OnMessage(object sender, MessageEventArgs e)
		{
			if (string.IsNullOrEmpty(e.Data))
			{
				UnityEngine.Debug.LogWarning("DRLSocket> Message arrived with no data!");
				return;
			}
			DRLSocketPacket dRLSocketPacket = DRLSocketParser.Decode(e.Data);
			if (dRLSocketPacket == null)
			{
				UnityEngine.Debug.LogWarning("DRLSocket> Failed to decode the message: " + e.Data);
			}
			if (string.IsNullOrEmpty(dRLSocketPacket.message))
			{
				dRLSocketPacket.message = "::";
			}
			switch (dRLSocketPacket.enginePacketType)
			{
			case EnginePacketType.PING:
				HandlePing();
				break;
			case EnginePacketType.PONG:
				m_pinging = false;
				m_hasPong = true;
				break;
			case EnginePacketType.MESSAGE:
				if (dRLSocketPacket.socketPacketType == SocketPacketType.EVENT)
				{
					lock (eventQueueLock)
					{
						eventsQueue.Enqueue(new Tuple<string, string>(dRLSocketPacket.eventName, dRLSocketPacket.message));
						break;
					}
				}
				break;
			}
		}

		private void HandlePing()
		{
			SendMessage(EnginePacketType.PONG);
		}

		private void HandleReconnect()
		{
			int millisecondsTimeout = m_reconnectDelay * 1000;
			while (m_socket != null && m_connected)
			{
				try
				{
					if (!m_socket.IsAlive && !m_disconnecting)
					{
						StartConnect(socketUrl);
					}
					Thread.Sleep(millisecondsTimeout);
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.Log("DRLSocket> Reconnect thread exception ocurred: " + ex.Message);
					m_reconnectThread?.Join();
				}
			}
		}

		private void StartHeartbeat()
		{
			int millisecondsTimeout = m_pingInterval * 1000;
			int num = m_pingTimeout * 1000;
			int millisecondsTimeout2 = m_reconnectDelay * 1000 + 50;
			Stopwatch stopwatch = new Stopwatch();
			while (m_connected)
			{
				if (m_socket == null || !m_socket.IsAlive)
				{
					Thread.Sleep(millisecondsTimeout2);
					continue;
				}
				m_pinging = true;
				m_hasPong = false;
				SendMessage(EnginePacketType.PING);
				stopwatch.Start();
				while (m_socket != null && m_socket.IsAlive && m_pinging && stopwatch.ElapsedMilliseconds < num)
				{
					Thread.Sleep(1000);
				}
				if (!m_hasPong)
				{
					UnityEngine.Debug.LogWarning("DRLSocket> No response from server - closing socket connection.\nConnection duration: " + debugTimer.Elapsed.TotalSeconds + "\n" + DateTime.Now.ToString());
					StartDisconnect();
					Thread.Sleep(3500);
					StartConnect(socketUrl);
				}
				stopwatch.Reset();
				stopwatch.Stop();
				Thread.Sleep(millisecondsTimeout);
			}
		}

		private void RaiseEvent(string p_event, string p_message = null)
		{
			if (m_handlers == null || !m_handlers.ContainsKey(p_event) || string.IsNullOrEmpty(p_event))
			{
				return;
			}
			List<Action<T>> list = m_handlers[p_event];
			if (list == null || list.Count == 0)
			{
				UnityEngine.Debug.LogWarning("DRLSocket> Error: no callbacks assigned for - " + p_event);
			}
			foreach (Action<T> item in list)
			{
				T obj = default(T);
				if (!string.IsNullOrEmpty(p_message))
				{
					try
					{
						obj = JsonConvert.DeserializeObject<T>(p_message);
					}
					catch (Exception ex)
					{
						UnityEngine.Debug.LogWarning("DRLSocket> Message arrived in invalid format: " + p_message + "\n" + ex.Message);
					}
				}
				item?.Invoke(obj);
			}
		}

		public void Subscribe(string p_event, Action<T> p_callback)
		{
			if (!m_handlers.ContainsKey(p_event))
			{
				m_handlers[p_event] = new List<Action<T>>();
			}
			if (!m_handlers[p_event].Contains(p_callback))
			{
				m_handlers[p_event].Add(p_callback);
			}
		}

		public void UnsubscribeAll()
		{
			if (m_handlers != null)
			{
				m_handlers.Clear();
			}
		}

		public void UnsubscribeAll(string p_event)
		{
			if (m_handlers != null && m_handlers.ContainsKey(p_event))
			{
				m_handlers.Remove(p_event);
			}
		}

		public void Unsubscribe(string p_event, Action<T> p_callback)
		{
			if (m_handlers.ContainsKey(p_event))
			{
				List<Action<T>> list = m_handlers[p_event];
				if (list.Contains(p_callback))
				{
					list.Remove(p_callback);
				}
			}
		}

		public void SendMessage(EnginePacketType p_type, Action<bool> p_callback = null)
		{
			DRLSocketPacket p_packet = new DRLSocketPacket(p_type);
			SendMessageAsync(p_packet, p_callback);
		}

		public void SendMessage(EnginePacketType p_etype, SocketPacketType p_stype, Action<bool> p_callback = null)
		{
			DRLSocketPacket p_packet = new DRLSocketPacket(p_etype, p_stype);
			SendMessageAsync(p_packet, p_callback);
		}

		public void SendMessage(string p_event, T p_data, Action<bool> p_callback = null)
		{
			if (p_data != null)
			{
				string p_message = JsonConvert.SerializeObject(p_data, Formatting.None);
				DRLSocketPacket p_packet = new DRLSocketPacket(p_event, p_message);
				SendMessageAsync(p_packet, p_callback);
			}
		}

		public void SendMessage(string p_event, Action<bool> p_callback = null)
		{
			DRLSocketPacket p_packet = new DRLSocketPacket(p_event);
			SendMessageAsync(p_packet, p_callback);
		}

		private void SendMessageAsync(DRLSocketPacket p_packet, Action<bool> p_callback = null)
		{
			if (!IsConnected())
			{
				return;
			}
			try
			{
				string data = DRLSocketParser.Encode(p_packet);
				m_socket.SendAsync(data, p_callback);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogWarning("DRLSocket> Failed to send message: " + ex.Message);
			}
		}

		public bool IsConnected()
		{
			if (m_socket != null)
			{
				return m_socket.ReadyState == WebSocketState.Open;
			}
			return false;
		}

		public bool IsConnecting()
		{
			if (m_socket != null)
			{
				return m_socket.ReadyState == WebSocketState.Connecting;
			}
			return false;
		}

		public void ForceClose()
		{
			m_socket?.Close();
			m_connected = false;
			m_connecting = false;
			m_disconnecting = false;
		}
	}
}

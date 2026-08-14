using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using thelab.core;

namespace drl
{
	public class NetworkByteStream<T>
	{
		public Action<NetworkByteStreamEventType, object, string> OnEvent;

		private Socket m_socket;

		private Socket m_socket_rcv;

		private IPAddress m_socket_ip;

		private IPEndPoint m_socket_ep;

		private IPEndPoint m_socket_rcv_ep;

		private byte[] m_socket_rcv_buffer;

		private List<byte[]> m_data_snd_buffer;

		private int m_data_snd_idx;

		private Thread m_socket_loop;

		public string ip
		{
			get
			{
				if (m_socket_ip != null)
				{
					return m_socket_ip.ToString();
				}
				return "";
			}
		}

		public int port
		{
			get
			{
				if (m_socket_ep != null)
				{
					return m_socket_ep.Port;
				}
				return 0;
			}
		}

		public bool active => m_socket != null;

		public ProtocolType protocol { get; private set; }

		public NetworkByteStreamMode mode { get; private set; }

		public NetworkByteStream(NetworkByteStreamMode p_mode, bool p_use_udp)
		{
			mode = p_mode;
			protocol = (p_use_udp ? ProtocolType.Udp : ProtocolType.Tcp);
		}

		public NetworkByteStream(NetworkByteStreamMode p_mode)
			: this(p_mode, true)
		{
		}

		public void Start(string p_ip, int p_port)
		{
			if (active)
			{
				return;
			}
			m_socket_ip = IPAddress.Parse(p_ip);
			m_socket_ep = new IPEndPoint(m_socket_ip, p_port);
			try
			{
				SocketType socketType = ((protocol == ProtocolType.Tcp) ? SocketType.Stream : SocketType.Dgram);
				m_socket = new Socket(AddressFamily.InterNetwork, socketType, protocol);
			}
			catch (SocketException ex)
			{
				Debug.LogError($"NetworkByteStream> SocketException\ncode: {ex.ErrorCode}\nmessage:\n{ex.Message}");
				return;
			}
			switch (protocol)
			{
			case ProtocolType.Udp:
				m_socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, optionValue: true);
				break;
			case ProtocolType.Tcp:
				m_socket.NoDelay = true;
				break;
			}
			switch (mode)
			{
			case NetworkByteStreamMode.Send:
				m_data_snd_buffer = new List<byte[]>();
				break;
			case NetworkByteStreamMode.Receive:
				m_socket.Bind(m_socket_ep);
				if (protocol == ProtocolType.Tcp)
				{
					m_socket.Listen(100);
				}
				m_socket_rcv_ep = new IPEndPoint(IPAddress.Parse("0.0.0.0"), p_port);
				m_socket_rcv_buffer = new byte[m_socket.ReceiveBufferSize];
				break;
			}
			if (m_socket_loop != null && m_socket_loop.IsAlive)
			{
				m_socket_loop.Abort();
			}
			m_socket_loop = new Thread(SocketLoop);
			m_socket_loop.Priority = System.Threading.ThreadPriority.Highest;
			m_socket_loop.Start();
		}

		public void Start(int p_port)
		{
			IPAddress selfIP = GetSelfIP();
			if (selfIP != null)
			{
				Start(selfIP.ToString(), p_port);
			}
		}

		public static IPAddress GetSelfIP()
		{
			IPAddress result = null;
			try
			{
				IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
				foreach (IPAddress iPAddress in addressList)
				{
					if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
					{
						result = iPAddress;
						break;
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("NetworkByteStream> GetSelfIP / Error\n" + ex.Message);
				return null;
			}
			return result;
		}

		protected void SocketLoop()
		{
			switch (mode)
			{
			case NetworkByteStreamMode.Receive:
				Dispatch(NetworkByteStreamEventType.Listening);
				break;
			case NetworkByteStreamMode.Send:
				Dispatch(NetworkByteStreamEventType.Connect);
				break;
			}
			bool flag = true;
			while (flag)
			{
				switch (mode)
				{
				case NetworkByteStreamMode.Receive:
					try
					{
						SocketReceiveLoop();
					}
					catch (Exception ex2)
					{
						Dispatch(NetworkByteStreamEventType.Error, ex2.Message);
						flag = false;
					}
					break;
				case NetworkByteStreamMode.Send:
					try
					{
						SocketSendLoop();
					}
					catch (Exception ex)
					{
						Dispatch(NetworkByteStreamEventType.Error, ex.Message);
						flag = false;
					}
					break;
				}
			}
			if (m_socket_loop != null && m_socket_loop.IsAlive)
			{
				m_socket_loop.Abort();
			}
			m_socket_loop = null;
			Disconnect();
		}

		protected void SocketReceiveLoop()
		{
			Socket socket = null;
			switch (protocol)
			{
			case ProtocolType.Udp:
				socket = m_socket;
				break;
			case ProtocolType.Tcp:
				if (m_socket_rcv == null)
				{
					m_socket_rcv = m_socket.Accept();
				}
				socket = m_socket_rcv;
				break;
			}
			if (socket == null)
			{
				return;
			}
			int num = 0;
			num = socket.Receive(m_socket_rcv_buffer);
			if (num > 0)
			{
				byte b = m_socket_rcv_buffer[0];
				byte[] array = new byte[num - 1];
				Array.Copy(m_socket_rcv_buffer, 1, array, 0, array.Length);
				switch (b)
				{
				case 1:
					OnData(array);
					Dispatch(NetworkByteStreamEventType.Data, array);
					break;
				case 2:
				{
					T val = Serialize.FromBytes<T>(array);
					OnDecode(val);
					Dispatch(NetworkByteStreamEventType.Decode, val);
					break;
				}
				case 0:
					break;
				}
			}
		}

		protected virtual void OnData(byte[] p_data)
		{
		}

		protected virtual void OnDecode(T p_data)
		{
		}

		protected void SocketSendLoop()
		{
			if (protocol == ProtocolType.Tcp && !m_socket.Connected)
			{
				try
				{
					m_socket.Connect(m_socket_ep);
				}
				catch (Exception)
				{
					Thread.Sleep(500);
					return;
				}
			}
			if (m_data_snd_buffer.Count <= m_data_snd_idx)
			{
				return;
			}
			byte[] array = m_data_snd_buffer[m_data_snd_idx];
			if (array != null)
			{
				m_data_snd_idx++;
				if (m_data_snd_buffer.Count > 10)
				{
					m_data_snd_buffer.RemoveAt(0);
					m_data_snd_idx--;
				}
				switch (protocol)
				{
				case ProtocolType.Tcp:
					m_socket.Send(array);
					break;
				case ProtocolType.Udp:
					m_socket.SendTo(array, m_socket_ep);
					break;
				}
			}
		}

		public void Disconnect()
		{
			if (m_socket_loop != null && m_socket_loop.IsAlive)
			{
				m_socket_loop.Abort();
			}
			m_socket_loop = null;
			Socket socket = m_socket;
			if (socket != null)
			{
				try
				{
					socket.Shutdown(SocketShutdown.Both);
				}
				catch (Exception message)
				{
					Debug.LogWarning(message);
				}
				finally
				{
					socket.Close();
				}
			}
			socket = m_socket_rcv;
			if (socket != null)
			{
				try
				{
					socket.Shutdown(SocketShutdown.Both);
				}
				catch (Exception message2)
				{
					Debug.LogWarning(message2);
				}
				finally
				{
					socket.Close();
				}
			}
			m_socket = null;
			m_socket_rcv = null;
			Dispatch(NetworkByteStreamEventType.Disconnect, null);
		}

		public bool Send(byte[] p_data)
		{
			return BaseSend(1, p_data);
		}

		public bool Send(T p_data)
		{
			byte[] p_data2 = ((p_data == null) ? null : Serialize.ToBytes(p_data));
			return BaseSend(2, p_data2);
		}

		protected bool BaseSend(byte p_mode, byte[] p_data)
		{
			if (p_data == null)
			{
				return false;
			}
			if (p_data.Length == 0)
			{
				return false;
			}
			byte[] array = new byte[p_data.Length + 1];
			array[0] = p_mode;
			Array.Copy(p_data, 0, array, 1, p_data.Length);
			if (m_data_snd_buffer == null)
			{
				return false;
			}
			m_data_snd_buffer.Add(array);
			return true;
		}

		protected void Dispatch(NetworkByteStreamEventType p_event, object p_data, string p_error = "")
		{
			if (OnEvent != null)
			{
				OnEvent(p_event, p_data, p_error);
			}
		}

		protected void Dispatch(NetworkByteStreamEventType p_event, string p_error = "")
		{
			if (OnEvent != null)
			{
				OnEvent(p_event, null, p_error);
			}
		}
	}
}

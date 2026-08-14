using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using drl.sim.rci;
using thelab.core;

namespace drl
{
	public class NetworkRewiredReceiver : MonoBehaviour
	{
		public float pairingTimeout = 5f;

		public NetworkByteStreamMode mode;

		public NetworkChannelStream ncs;

		public string ip;

		public int port = 30123;

		public int serverListenerPort = 30303;

		public int clientListenerPort = 33333;

		private bool m_paired;

		private string m_magicWord = "DRLSIM";

		private Thread m_thread;

		private string m_clientIP;

		private UdpClient serverSender;

		private UdpClient serverReceiver;

		private bool m_canConnect;

		private bool m_canDisconnect;

		private bool m_canUpdateInterfaceData;

		private bool m_running;

		private float m_heartbeatFreq = 3f;

		private float m_hearbeatTimer;

		[Header("Controller interface:")]
		public string controllerSimplifiedName = "";

		public string controllerName = "NO CONTROLLER";

		public int axisCount;

		public int buttonCount;

		public float[] axisData;

		public float[] buttonChangedData;

		public float[] axisDeltaData;

		private bool m_connectController;

		private float[] axisRawData = new float[10];

		public bool paired => m_paired;

		private void Start()
		{
		}

		public void Initialize()
		{
			if (!m_running)
			{
				IPAddress selfIP = NetworkByteStream<NCPacket>.GetSelfIP();
				if (selfIP != null)
				{
					ip = selfIP.ToString();
					Pair();
				}
			}
		}

		public void Pair()
		{
			if (m_thread != null && m_thread.IsAlive)
			{
				m_thread.Abort();
			}
			m_thread = null;
			m_running = false;
			if (serverSender != null)
			{
				serverSender.Close();
				serverSender = null;
			}
			if (serverReceiver != null)
			{
				serverReceiver.Close();
				serverReceiver = null;
			}
			try
			{
				Debug.Log("NetworkRewiredReceiver> Pair / Server Sender UDP Bind");
				serverSender = new UdpClient();
				Debug.Log($"NetworkRewiredReceiver> Pair / Server Receiver UDP Bind - port[{serverListenerPort}]");
				serverReceiver = new UdpClient(serverListenerPort);
				Debug.Log("NetworkRewiredReceiver> Pair / Bind Complete!");
				m_thread = new Thread((ThreadStart)delegate
				{
					Debug.Log("NetworkRewiredReceiver> Pair / Start Pair Thread Run...");
					StartPairing();
				});
				m_running = true;
				m_thread.Start();
			}
			catch (Exception message)
			{
				Debug.LogWarning(message);
				if (m_thread != null && m_thread.IsAlive)
				{
					m_thread.Abort();
				}
				m_thread = null;
			}
		}

		public void Unpair()
		{
			if (ncs != null)
			{
				ncs.Disconnect();
				NetworkChannelStream networkChannelStream = ncs;
				networkChannelStream.OnEvent = (Action<NetworkByteStreamEventType, object, string>)Delegate.Remove(networkChannelStream.OnEvent, new Action<NetworkByteStreamEventType, object, string>(OnNetworkStreamEvent));
			}
			m_running = false;
			if (m_thread != null)
			{
				m_thread.Abort();
			}
			if (!string.IsNullOrEmpty(m_clientIP))
			{
				if (serverSender != null)
				{
					serverSender.Close();
					serverSender = null;
				}
				if (serverReceiver != null)
				{
					serverReceiver.Close();
					serverReceiver = null;
				}
				m_canDisconnect = true;
				IPAddress address = IPAddress.Parse("192.168.0.255");
				IPAddress address2 = IPAddress.Parse("10.0.0.255");
				serverSender = new UdpClient();
				serverSender.EnableBroadcast = true;
				IPEndPoint endPoint = new IPEndPoint(address, clientListenerPort);
				IPEndPoint endPoint2 = new IPEndPoint(address2, clientListenerPort);
				byte[] bytes = Encoding.ASCII.GetBytes("STOP");
				serverSender.Send(bytes, bytes.Length, endPoint);
				serverSender.Send(bytes, bytes.Length, endPoint2);
				m_clientIP = null;
			}
		}

		public void SendCalibrationUpdate()
		{
			if (serverSender != null)
			{
				serverSender.Close();
				serverSender = null;
			}
			IPAddress address = IPAddress.Parse("192.168.0.255");
			IPAddress address2 = IPAddress.Parse("10.0.0.255");
			serverSender = new UdpClient();
			serverSender.EnableBroadcast = true;
			IPEndPoint endPoint = new IPEndPoint(address, clientListenerPort);
			IPEndPoint endPoint2 = new IPEndPoint(address2, clientListenerPort);
			byte[] bytes = Encoding.ASCII.GetBytes("CALIBRATED#" + Serialize.ToJson(RCI.GetSavedProfile()));
			serverSender.Send(bytes, bytes.Length, endPoint);
			serverSender.Send(bytes, bytes.Length, endPoint2);
		}

		private void SendHeartbeat()
		{
			if (!string.IsNullOrEmpty(m_clientIP))
			{
				if (serverSender != null)
				{
					serverSender.Close();
					serverSender = null;
				}
				IPAddress address = IPAddress.Parse(m_clientIP);
				serverSender = new UdpClient();
				IPEndPoint endPoint = new IPEndPoint(address, clientListenerPort);
				byte[] bytes = Encoding.ASCII.GetBytes("PING");
				serverSender.Send(bytes, bytes.Length, endPoint);
			}
		}

		private void StartConnection()
		{
			if (ncs != null)
			{
				ncs.Disconnect();
			}
			ncs = new NetworkChannelStream(16, mode);
			ncs.OnSync = OnChannelSync;
			NetworkChannelStream networkChannelStream = ncs;
			networkChannelStream.OnEvent = (Action<NetworkByteStreamEventType, object, string>)Delegate.Combine(networkChannelStream.OnEvent, new Action<NetworkByteStreamEventType, object, string>(OnNetworkStreamEvent));
			if (mode == NetworkByteStreamMode.Receive)
			{
				StartCoroutine(StartChannelDataListener());
			}
		}

		private void OnNetworkStreamEvent(NetworkByteStreamEventType p_type, object arg2, string arg3)
		{
			if (p_type == NetworkByteStreamEventType.Error)
			{
				m_canDisconnect = true;
			}
			if (p_type == NetworkByteStreamEventType.Disconnect)
			{
				RCI.DisconnectControllerMobile(this);
			}
		}

		private IEnumerator StartChannelDataListener()
		{
			ncs.Start(port);
			yield return 0;
		}

		private void StartPairing()
		{
			IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
			while (m_running)
			{
				byte[] bytes = serverReceiver.Receive(ref remoteEP);
				string text = Encoding.ASCII.GetString(bytes);
				string[] array = text.Split('#');
				if (array.Length == 0)
				{
					continue;
				}
				string obj = array[0];
				string text2 = "";
				if (array.Length > 1)
				{
					text2 = array[1];
				}
				string text3 = "";
				if (array.Length > 2)
				{
					text3 = array[2];
				}
				string value = "";
				if (array.Length > 3)
				{
					value = array[3];
				}
				if (array.Length > 4)
				{
					int result = 0;
					int.TryParse(array[4], out result);
					axisCount = result;
					axisData = new float[result];
					axisDeltaData = new float[result];
				}
				if (array.Length > 5)
				{
					int result2 = 0;
					int.TryParse(array[5], out result2);
					buttonCount = result2;
					buttonChangedData = new float[result2];
				}
				if (obj == m_magicWord)
				{
					m_clientIP = text2;
					IPAddress.TryParse(text2, out var address);
					if (address != null)
					{
						m_canConnect = true;
						bool flag = false;
						string text4 = "";
						if (!string.IsNullOrEmpty(text3))
						{
							flag = RCI.HasSavedProfile(text3 + "-app");
							controllerName = text3;
							if (flag)
							{
								RCDeviceData savedProfile = RCI.GetSavedProfile(text3 + "-app");
								if (savedProfile != null)
								{
									text4 = Serialize.ToJson(savedProfile);
								}
							}
						}
						if (!string.IsNullOrEmpty(value))
						{
							controllerSimplifiedName = value;
						}
						byte[] bytes2 = Encoding.ASCII.GetBytes(ip + "#" + flag + "#" + text4);
						serverSender.Send(bytes2, bytes2.Length, new IPEndPoint(GetBroadcastIP(GetLocalIP().Item2), clientListenerPort));
						m_connectController = true;
						axisRawData = new float[axisCount * 2 + buttonCount];
					}
				}
				if (text.StartsWith("STOP"))
				{
					Debug.Log("Client disconnected..");
					m_canDisconnect = true;
				}
			}
		}

		protected void OnChannelSync(NCPacket p_packet)
		{
			uint[] array = p_packet.Get();
			for (int i = 0; i < array.Length; i++)
			{
				DecodeData(array[i], ref axisRawData);
			}
			m_canUpdateInterfaceData = true;
		}

		protected void DecodeData(uint p_value, ref float[] p_rawDataArray)
		{
			uint num = p_value >> 24;
			uint num2 = 16777215u;
			uint num3 = p_value & num2;
			float num4 = 16777215f;
			float num5 = (float)num3 / num4;
			num5 = -1f + 2f * num5;
			num5 = (((double)Mathf.Abs(num5) < 1E-06) ? 0f : num5);
			if (num < p_rawDataArray.Length)
			{
				p_rawDataArray[num] = num5;
			}
		}

		private void Update()
		{
			if (m_canConnect)
			{
				m_canConnect = false;
				m_paired = true;
				StartConnection();
			}
			if (m_canDisconnect)
			{
				m_canDisconnect = false;
				m_running = false;
				m_paired = false;
				if (m_thread != null)
				{
					m_thread.Abort();
				}
				if (ncs != null)
				{
					ncs.Disconnect();
					NetworkChannelStream networkChannelStream = ncs;
					networkChannelStream.OnEvent = (Action<NetworkByteStreamEventType, object, string>)Delegate.Remove(networkChannelStream.OnEvent, new Action<NetworkByteStreamEventType, object, string>(OnNetworkStreamEvent));
				}
				RCI.DisconnectControllerMobile(this);
				Pair();
			}
			if (m_connectController)
			{
				m_connectController = false;
				RCI.SetActiveControllerMobile(this);
			}
			if (m_canUpdateInterfaceData)
			{
				m_canUpdateInterfaceData = false;
				if (axisRawData.Length == 2 * axisCount + buttonCount)
				{
					for (int i = 0; i < 2 * axisCount + buttonCount; i++)
					{
						if (i < axisCount)
						{
							axisData[i] = axisRawData[i];
						}
						else if (i < axisCount + buttonCount)
						{
							buttonChangedData[i - axisCount] = axisRawData[i];
						}
						else
						{
							axisDeltaData[i - (axisCount + buttonCount)] = axisRawData[i];
						}
					}
				}
			}
			if (!m_paired)
			{
				return;
			}
			m_hearbeatTimer += Time.deltaTime;
			if (m_hearbeatTimer > m_heartbeatFreq)
			{
				m_hearbeatTimer = 0f;
				try
				{
					SendHeartbeat();
				}
				catch (Exception ex)
				{
					Debug.LogWarning(ex.Message);
					m_canDisconnect = true;
				}
			}
		}

		private void OnApplicationQuit()
		{
			Unpair();
		}

		private Tuple<string, IPAddress> GetLocalIP()
		{
			IPAddress iPAddress = null;
			IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
			foreach (IPAddress iPAddress2 in addressList)
			{
				if (iPAddress2.AddressFamily == AddressFamily.InterNetwork)
				{
					iPAddress = iPAddress2;
					break;
				}
			}
			if (iPAddress == null)
			{
				return null;
			}
			string text = "";
			byte[] addressBytes = iPAddress.GetAddressBytes();
			foreach (byte b in addressBytes)
			{
				text = text + b + ".";
			}
			text = text.Substring(0, text.Length - 2);
			return new Tuple<string, IPAddress>(text, iPAddress);
		}

		private IPAddress GetBroadcastIP(IPAddress p_ip)
		{
			if (p_ip == null)
			{
				return null;
			}
			byte[] addressBytes = p_ip.GetAddressBytes();
			addressBytes[addressBytes.Length - 1] = byte.MaxValue;
			return new IPAddress(addressBytes);
		}
	}
}

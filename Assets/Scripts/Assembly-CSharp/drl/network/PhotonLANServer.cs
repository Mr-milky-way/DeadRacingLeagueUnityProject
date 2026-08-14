using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using UnityEngine;
using drl.game;

namespace drl.network
{
	public class PhotonLANServer : MonoBehaviour
	{
		public enum ServerState
		{
			Offline = 0,
			Starting = 1,
			Online = 2
		}

		public static readonly string PhotonProcessName = "PhotonSocketServer";

		[SerializeField]
		private ServerState m_state;

		[SerializeField]
		private string m_local_ip = string.Empty;

		public Action<ServerState> OnState;

		private Process m_server_process;

		private float m_online_elapsed;

		public bool running => m_server_process != null;

		public ServerState state
		{
			get
			{
				return m_state;
			}
			private set
			{
				m_state = value;
			}
		}

		public bool supported
		{
			get
			{
				if (Application.platform != RuntimePlatform.WindowsEditor)
				{
					return Application.platform == RuntimePlatform.WindowsPlayer;
				}
				return true;
			}
		}

		public string localIp
		{
			get
			{
				m_local_ip = "";
				IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
				foreach (IPAddress iPAddress in addressList)
				{
					if (iPAddress.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(iPAddress))
					{
						m_local_ip = iPAddress.ToString();
					}
				}
				return m_local_ip;
			}
		}

		public bool Run()
		{
			if (!supported)
			{
				UnityEngine.Debug.LogWarning("PhotonLANServer> Server is not supported on: " + Application.platform);
				return false;
			}
			if (m_server_process != null)
			{
				m_server_process.Kill();
				m_server_process = null;
				state = ServerState.Offline;
			}
			m_online_elapsed = 0f;
			state = ServerState.Starting;
			if (OnState != null)
			{
				OnState(ServerState.Starting);
			}
			UpdateIpAddress();
			m_server_process = CallPhotonSocketServer("/run LoadBalancing");
			return m_server_process != null;
		}

		public void Stop()
		{
			state = ServerState.Offline;
			CallPhotonSocketServer("/stop");
		}

		protected void Update()
		{
			if (!running)
			{
				return;
			}
			ServerState serverState = state;
			if (serverState == ServerState.Offline || serverState != ServerState.Starting)
			{
				return;
			}
			Process server_process = m_server_process;
			if (server_process == null)
			{
				state = ServerState.Offline;
				UnityEngine.Debug.LogWarning("PhotonLANServer> Process is null, entering Offline state.");
				if (OnState != null)
				{
					OnState(ServerState.Offline);
				}
				return;
			}
			if (server_process.HasExited)
			{
				state = ServerState.Offline;
				m_server_process = null;
				if (OnState != null)
				{
					OnState(ServerState.Offline);
				}
				return;
			}
			m_online_elapsed += Time.deltaTime;
			if (!(m_online_elapsed < 120f))
			{
				m_online_elapsed = 0f;
				state = ServerState.Online;
				if (OnState != null)
				{
					OnState(ServerState.Online);
				}
			}
		}

		protected Process CallPhotonSocketServer(string p_command)
		{
			string text = DRLPaths.streamingAssetsRoot + "PhotonServer/bin_Win64/";
			Process process = null;
			try
			{
				ProcessStartInfo processStartInfo = new ProcessStartInfo();
				processStartInfo.FileName = text + PhotonProcessName + ".exe";
				UnityEngine.Debug.Log("PhotonLANServer> CallPhotonSocketServer at [" + processStartInfo.FileName + "] [" + p_command + "]");
				processStartInfo.RedirectStandardError = false;
				processStartInfo.RedirectStandardOutput = true;
				processStartInfo.UseShellExecute = false;
				processStartInfo.CreateNoWindow = true;
				processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
				processStartInfo.WorkingDirectory = text;
				processStartInfo.Arguments = p_command;
				process = new Process();
				process.StartInfo = processStartInfo;
				process.OutputDataReceived += delegate(object s, DataReceivedEventArgs e)
				{
					string data = e.Data;
					UnityEngine.Debug.Log("PhotonLANServer> [SERVER] " + data);
					if (((data == null) ? "" : data.ToLower().Replace(" ", "")).Contains("outgoingmasterserverpeersuccess"))
					{
						m_online_elapsed = 120f;
					}
				};
				process.Start();
				process.BeginOutputReadLine();
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError("PhotonLANServer> CallPhotonSocketServer / Error\n" + ex.Message);
				process = null;
			}
			return process;
		}

		protected bool UpdateIpAddress()
		{
			UpdateIpAddress("GameServer");
			return true;
		}

		protected bool UpdateIpAddress(string p_service_target)
		{
			string text = DRLPaths.streamingAssetsRoot + "PhotonServer/Loadbalancing/" + p_service_target + "/bin/Photon.LoadBalancing.dll.config";
			UnityEngine.Debug.Log("PhotonLANServer> UpdateIpAddress / target[" + p_service_target + "] ip[" + localIp + "] path[" + text + "]");
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.PreserveWhitespace = true;
				xmlDocument.Load(text);
				XmlNode xmlNode = xmlDocument.SelectSingleNode("//setting[@name='PublicIPAddress']/value");
				if (xmlNode != null)
				{
					xmlNode.InnerText = localIp;
				}
				xmlDocument.Save(text);
				return true;
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.Log("PhotonLANServer>  UpdateIpAddress / Error\n" + ex.Message);
				return false;
			}
		}
	}
}

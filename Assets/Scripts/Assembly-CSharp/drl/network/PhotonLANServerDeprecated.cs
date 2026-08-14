using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using UnityEngine;
using drl.game;

namespace drl.network
{
	[Serializable]
	public class PhotonLANServerDeprecated
	{
		public enum ServerState
		{
			Starting = 0,
			Online = 1,
			Stopping = 2,
			Offline = 3
		}

		public static readonly string PhotonProcessName = "PhotonSocketServer";

		public bool Enabled;

		public Action<ServerState> OnStateChanged;

		[SerializeField]
		private string localIP = string.Empty;

		[SerializeField]
		private ServerState state = ServerState.Offline;

		private float statusCheckTime;

		private PhotonService parentService;

		public bool IsRunning => RunningProcess != null;

		public ServerState State
		{
			get
			{
				return state;
			}
			private set
			{
				SetState(value);
			}
		}

		public string StandardOutput { get; private set; }

		public Process RunningProcess { get; private set; }

		public bool IsSupported
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

		public string Address
		{
			get
			{
				localIP = "";
				IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
				foreach (IPAddress iPAddress in addressList)
				{
					if (iPAddress.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(iPAddress))
					{
						localIP = iPAddress.ToString();
					}
				}
				return localIP;
			}
		}

		public void Start(PhotonService service)
		{
			if (!IsSupported)
			{
				UnityEngine.Debug.LogError("PhotonLAN server is not supported on: " + Application.platform);
				return;
			}
			parentService = service;
			Enabled = true;
			State = ServerState.Starting;
			UpdateIpAddress();
			StandardOutput = string.Empty;
			string text = Application.streamingAssetsPath.Replace('/', '\\') + "\\PhotonServer\\bin_Win32";
			try
			{
				ProcessStartInfo processStartInfo = new ProcessStartInfo();
				processStartInfo.FileName = text + "\\PhotonSocketServer.exe";
				processStartInfo.RedirectStandardError = true;
				processStartInfo.RedirectStandardOutput = true;
				processStartInfo.UseShellExecute = false;
				processStartInfo.CreateNoWindow = true;
				processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
				processStartInfo.WorkingDirectory = text;
				processStartInfo.Arguments = "/run LoadBalancing";
				Process process = new Process();
				process.StartInfo = processStartInfo;
				process.OutputDataReceived += delegate(object s, DataReceivedEventArgs e)
				{
					string text2 = $"Photon:LANServer > {e.Data}";
					StandardOutput += text2;
					UnityEngine.Debug.Log(text2);
				};
				process.Start();
				process.BeginOutputReadLine();
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError(ex.Message);
			}
		}

		public void Stop()
		{
			State = ServerState.Stopping;
			try
			{
				Process[] processesByName = Process.GetProcessesByName(PhotonProcessName);
				foreach (Process process in processesByName)
				{
					UnityEngine.Debug.Log("PhotonLANServer > Stopping running LAN server");
					if (process != null)
					{
						process.Exited += delegate
						{
							UnityEngine.Debug.Log("PhotonLANServer > LAN server stopped");
						};
						process.Kill();
					}
				}
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError(ex.Message);
			}
		}

		public void Update()
		{
			if (!Enabled || parentService == null || parentService.State == PhotonService.ServiceState.InRoom)
			{
				return;
			}
			statusCheckTime += Time.deltaTime;
			if (!(statusCheckTime >= 10f))
			{
				return;
			}
			statusCheckTime = 0f;
			Process[] processesByName = Process.GetProcessesByName(PhotonProcessName);
			if (processesByName.Length != 0)
			{
				RunningProcess = processesByName[0];
				State = ServerState.Online;
				return;
			}
			RunningProcess = null;
			if (State == ServerState.Stopping)
			{
				Enabled = false;
			}
			State = ServerState.Offline;
		}

		protected bool UpdateIpAddress()
		{
			UpdateIpAddress("GameServer");
			return true;
		}

		protected bool UpdateIpAddress(string p_server)
		{
			string filename = DRLPaths.Assert(DRLPaths.streamingAssetsRoot + "PhotonServer/LoadBalancing/" + p_server + "/bin/Photon.LoadBalancing.dll.config");
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.PreserveWhitespace = true;
				xmlDocument.Load(filename);
				XmlNode xmlNode = xmlDocument.SelectSingleNode("//setting[@name='PublicIPAddress']/value");
				if (xmlNode != null)
				{
					xmlNode.InnerText = Address;
				}
				xmlDocument.Save(filename);
				return true;
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.Log("PhotonLANServer>  UpdateIpAddress / Error\n" + ex.Message);
				return false;
			}
		}

		private void SetState(ServerState newState)
		{
			if (State != newState)
			{
				state = newState;
				if (OnStateChanged != null)
				{
					OnStateChanged(State);
				}
			}
		}
	}
}

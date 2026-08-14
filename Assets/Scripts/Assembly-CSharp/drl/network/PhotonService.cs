using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using Photon;
using UnityEngine;
using thelab.core;

namespace drl.network
{
	public class PhotonService : PunBehaviour
	{
		public enum ServiceState
		{
			Disconnected = 0,
			InProgress = 1,
			InMaster = 2,
			InLobby = 3,
			InRoom = 4
		}

		public enum EventType
		{
			OnJoinLobby = 0,
			OnLobbyUpdated = 1,
			OnJoinRoom = 2,
			OnJoinRoomFailed = 3,
			OnCreateRoomFailed = 4,
			OnRoomUpdated = 5,
			OnPlayerJoin = 6,
			OnPlayerUpdated = 7,
			OnPlayerLeft = 8,
			OnLeftRoom = 9,
			OnLeftLobby = 10,
			OnMasterChanged = 11,
			OnJoinLobbyFailed = 12,
			OnRoomFull = 13,
			OnRoomNotActive = 14
		}

		public enum DisconnectionCode
		{
			ByUser = 0,
			ByServerLogic = 1,
			Timeout = 2,
			Exception = 3,
			Unknown = 4
		}

		public Action<ServiceState> OnStateChanged;

		public Action<EventType, object> OnNetworkEvent;

		public Action<NetworkRoom.GameEvent> OnGameEvent;

		public Action<PhotonLANServerDeprecated.ServerState> OnLANServerChanged;

		public readonly Lobby CurrentLobby = new Lobby();

		[SerializeField]
		protected ServiceState m_state;

		[SerializeField]
		private string m_game_version;

		private List<Region> m_available_regions;

		[SerializeField]
		private PhotonLANServer m_lan_server;

		public readonly TypedLobby ServerLobby = new TypedLobby();

		private const string PortNumber = "5055";

		public string AuthToken;

		public string AuthUser;

		private SlackDebugTool m_slack;

		private float kickedCooldown;

		private QuickMatchResult QuickMatch;

		private Action<QuickMatchResult> OnQuickMatchResult;

		private Activity m_join_room_loop;

		private Activity m_disconnect_poll;

		public DisconnectionCode DisconnectionReason { get; private set; }

		public bool IsConnectedAndReady => PhotonNetwork.connectedAndReady;

		public int OnlinePlayers => PhotonNetwork.countOfPlayers;

		public CloudRegionCode CurrentRegionCode => PhotonNetwork.CloudRegion;

		public string CurrentRegionName => PhotonUtils.GetRegionName(CurrentRegionCode);

		public NetworkRoom CurrentRoom { get; private set; }

		public int PingTime => PhotonNetwork.GetPing();

		public string PlayerName
		{
			get
			{
				return PhotonNetwork.playerName;
			}
			set
			{
				PhotonNetwork.playerName = value;
			}
		}

		public Dictionary<int, NetworkActor> Players
		{
			get
			{
				if (CurrentRoom != null)
				{
					return CurrentRoom.Players;
				}
				return new Dictionary<int, NetworkActor>();
			}
		}

		public bool IsMaster
		{
			get
			{
				if (CurrentRoom != null)
				{
					return CurrentRoom.IsMaster;
				}
				return false;
			}
		}

		public ServiceState State
		{
			get
			{
				return m_state;
			}
			private set
			{
				m_state = value;
				if (OnStateChanged != null)
				{
					OnStateChanged(m_state);
				}
			}
		}

		public string ServerIP => PhotonNetwork.ServerAddress;

		public string LocalIPAddress { get; set; }

		public string KickedFromRoom { get; set; }

		public bool IsConnectedToLocal
		{
			get
			{
				if (string.IsNullOrEmpty(ServerIP))
				{
					return false;
				}
				if (string.IsNullOrEmpty(LocalIPAddress))
				{
					return false;
				}
				bool flag = State == ServiceState.Disconnected;
				bool flag2 = PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated;
				bool flag3 = ServerIP.Split(':')[0] == LocalIPAddress.Split(':')[0];
				return (!flag || flag2) && flag3;
			}
		}

		public string GameVersion
		{
			get
			{
				return m_game_version;
			}
			set
			{
				m_game_version = value;
			}
		}

		public bool RegionPingActive { get; private set; }

		public List<Region> AvailableRegions
		{
			get
			{
				if (m_available_regions != null)
				{
					return m_available_regions;
				}
				return m_available_regions = new List<Region>();
			}
		}

		public string UserId
		{
			get
			{
				return PhotonNetwork.player?.UserId;
			}
			set
			{
				PhotonNetwork.AuthValues = new AuthenticationValues(value);
			}
		}

		public PhotonLANServer LanServer
		{
			get
			{
				if (!m_lan_server)
				{
					return m_lan_server = GetComponent<PhotonLANServer>();
				}
				return m_lan_server;
			}
		}

		public string APIKEY => "28c108ec-052d-4900-863c-3c5aad81d945";

		public SlackDebugTool slack
		{
			get
			{
				if (!m_slack)
				{
					return m_slack = UnityEngine.Object.FindObjectOfType<SlackDebugTool>();
				}
				return m_slack;
			}
		}

		protected void Awake()
		{
			PhotonNetwork.PhotonServerSettings.AppID = APIKEY;
			PhotonNetwork.OnEventCall += OnEvent;
			PhotonNetwork.autoJoinLobby = false;
			PhotonNetwork.logLevel = PhotonLogLevel.ErrorsOnly;
			CurrentLobby.Reset();
			LocalIPAddress = string.Empty;
			KickedFromRoom = "";
			PhotonNetwork.sendRate = 8;
			PhotonNetwork.sendRateOnSerialize = 8;
			PhotonNetwork.BackgroundTimeout = 360f;
			ServerSettings.ResetBestRegionCodeInPreferences();
		}

		protected void RefreshAuth()
		{
			Debug.Log("PhotonService> RefreshAuth");
			PhotonNetwork.AuthValues = null;
		}

		protected void Start()
		{
			PhotonNetwork.CacheSendMonoMessageTargets(typeof(PunBehaviour));
		}

		protected void OnDestroy()
		{
			PhotonNetwork.OnEventCall -= OnEvent;
		}

		protected void OnApplicationQuit()
		{
			if ((bool)LanServer)
			{
				LanServer.Stop();
			}
		}

		protected void OnEvent(byte eventCode, object content, int senderId)
		{
			PhotonPlayer sender = PhotonPlayer.Find(senderId);
			if (CurrentRoom != null)
			{
				CurrentRoom.OnIncomingGameEvent((NetworkRoom.GameEventCode)eventCode, content, sender);
			}
		}

		protected void DispatchNetworkEvent(EventType eventCode, object content = null)
		{
			if (OnNetworkEvent != null)
			{
				OnNetworkEvent(eventCode, content);
			}
		}

		public void TryConnectToLAN(string p_server_ip, string p_tournamentId = null)
		{
			if (string.IsNullOrEmpty(p_server_ip))
			{
				Debug.LogWarning("PhotonService> TryConnectToLAN / Invalid IP!");
			}
			else
			{
				ConnectToLANAsync(p_server_ip, p_tournamentId);
			}
		}

		protected IEnumerator ConnectToLAN(string p_server_ip, string p_tournamentId = null)
		{
			if (State != ServiceState.Disconnected)
			{
				Debug.Log("PhotonService> ConnectToLAN / Disconnecting from " + CurrentRegionName);
				TryDisconnect();
				while (State != ServiceState.Disconnected)
				{
					yield return null;
				}
			}
			if (string.IsNullOrEmpty(p_server_ip))
			{
				Debug.LogError("PhotonService> ConnectToLAN / Empty IP");
				yield break;
			}
			PhotonNetwork.gameVersion = GameVersion;
			PhotonNetwork.networkingPeer.AppId = PhotonNetwork.PhotonServerSettings.AppID;
			ServerLobby.Name = ((!string.IsNullOrEmpty(p_tournamentId)) ? p_tournamentId : "");
			LocalIPAddress = p_server_ip + ":5055";
			Debug.Log("PhotonService> ConnectToLAN  / address[" + LocalIPAddress + "]");
			bool serverIsready = false;
			int retry = 0;
			while (!serverIsready)
			{
				PhotonNetwork.networkingPeer.MasterServerAddress = LocalIPAddress;
				PhotonNetwork.networkingPeer.Connect(LocalIPAddress, ServerConnection.MasterServer);
				Debug.Log($"PhotonService> ConnectToLAN / Connecting - address[{LocalIPAddress}] retry[{retry}]");
				if (State != ServiceState.InLobby && State != ServiceState.InMaster)
				{
					State = ServiceState.InProgress;
				}
				while (State == ServiceState.InProgress)
				{
					yield return null;
				}
				Debug.Log($"PhotonService> ConnectToLAN / state[{State}]");
				while (State == ServiceState.InMaster)
				{
					yield return null;
				}
				serverIsready = State == ServiceState.InLobby;
				if (serverIsready && IsConnectedToLocal)
				{
					break;
				}
				if (serverIsready)
				{
					yield return null;
				}
				retry++;
				serverIsready = retry > 5;
				yield return new WaitForSeconds(3f);
			}
			if (!string.IsNullOrEmpty(p_tournamentId))
			{
				ServerLobby.Name = p_tournamentId;
			}
			Debug.Log("PhotonService> ConnectToLAN / Connected! Server[" + ServerLobby.Name + "]");
		}

		protected async Task ConnectToLANAsync(string p_server_ip, string p_tournamentId = null)
		{
			if (State != ServiceState.Disconnected)
			{
				Debug.Log("PhotonService> ConnectToLAN / Disconnecting from " + CurrentRegionName);
				TryDisconnect();
				while (State != ServiceState.Disconnected)
				{
					await Task.Delay(100);
				}
			}
			if (string.IsNullOrEmpty(p_server_ip))
			{
				Debug.LogError("PhotonService> ConnectToLANAsync / Empty IP");
				return;
			}
			PhotonNetwork.gameVersion = GameVersion;
			PhotonNetwork.networkingPeer.AppId = PhotonNetwork.PhotonServerSettings.AppID;
			ServerLobby.Name = ((!string.IsNullOrEmpty(p_tournamentId)) ? p_tournamentId : "");
			LocalIPAddress = p_server_ip + ":5055";
			Debug.Log("PhotonService> ConnectToLANAsync  / address[" + LocalIPAddress + "]");
			int p_retries = 5;
			float p_timeout = 3f;
			await TryConnectLANAsync(p_retries, p_timeout, p_tournamentId);
		}

		private async Task TryConnectLANAsync(int p_retries, float p_timeout, string p_tournamentId = null)
		{
			float timeout = p_timeout;
			if (p_retries <= 0)
			{
				Debug.Log("PhotonService> TryConnectLANAsync / Failed to connect!");
				return;
			}
			Debug.Log("PhotonService> TryConnectLANAsync / Trying to connect...");
			PhotonNetwork.networkingPeer.MasterServerAddress = LocalIPAddress;
			PhotonNetwork.networkingPeer.Connect(LocalIPAddress, ServerConnection.MasterServer);
			while (State != ServiceState.InLobby)
			{
				await Task.Delay(1000);
				timeout -= 1f;
				if (timeout <= 0f)
				{
					p_retries--;
					TryConnectLANAsync(p_retries, p_timeout, p_tournamentId);
					break;
				}
			}
			if (!string.IsNullOrEmpty(p_tournamentId))
			{
				ServerLobby.Name = p_tournamentId;
			}
			Debug.Log("PhotonService> TryConnectLANAsync / Connected! Server[" + ServerLobby.Name + "]");
		}

		public void TryConnectBestRegionLobby(string lobbyId = null)
		{
			RefreshAuth();
			LocalIPAddress = string.Empty;
			StopAllCoroutines();
			StartCoroutine(ConnectToBestMaster(lobbyId));
		}

		public void TryConnectRandomRoom(NetworkRoomOptions options, Action<QuickMatchResult> onResult)
		{
			StopAllCoroutines();
			QuickMatch = new QuickMatchResult();
			OnQuickMatchResult = onResult;
			StartCoroutine(ConnectRandom(options));
		}

		public void TryJoinRoom(string roomName)
		{
			if (KickedFromRoom == roomName)
			{
				Debug.LogWarning("PhotonService> TryJoinRoom / Already kicked from [" + roomName + "]. Cooldown ongoing.");
				return;
			}
			if (!PhotonNetwork.insideLobby)
			{
				Debug.LogWarning("PhotonService> TryJoinRoom - couldn't connect to lobby.");
				DispatchNetworkEvent(EventType.OnJoinLobbyFailed, roomName);
				return;
			}
			int retry = 0;
			float retry_elapsed = 0f;
			Debug.LogWarning($"PhotonService> TryJoinRoom / Polling Start - state[{State}]");
			if (m_join_room_loop != null)
			{
				m_join_room_loop.Stop();
			}
			m_join_room_loop = Activity.Run((Func<bool>)delegate
			{
				if (retry_elapsed > 0f)
				{
					retry_elapsed -= Time.unscaledDeltaTime;
					return true;
				}
				RoomInfo[] roomList = PhotonNetwork.GetRoomList();
				RoomInfo roomInfo = null;
				Debug.LogWarning($"PhotonService> TryJoinRoom / Searching PhotonNetwork Rooms - count[{roomList.Length}]");
				foreach (RoomInfo roomInfo2 in roomList)
				{
					Debug.LogWarning("PhotonService>    room[" + roomInfo2.Name + "]");
					if (!(roomInfo2.Name != roomName))
					{
						roomInfo = roomInfo2;
					}
				}
				Lobby.NetworkRoomInfo[] array = CurrentLobby.Rooms.ToArray();
				Lobby.NetworkRoomInfo networkRoomInfo = null;
				Debug.LogWarning($"PhotonService> TryJoinRoom / Searching CurrentLobby Rooms - count[{array.Length}]");
				foreach (Lobby.NetworkRoomInfo networkRoomInfo2 in array)
				{
					Debug.LogWarning("PhotonService>    room[" + networkRoomInfo2.Name + "] title[" + networkRoomInfo2.RoomTitle + "]");
					if (!(networkRoomInfo2.Name != roomName))
					{
						networkRoomInfo = networkRoomInfo2;
					}
				}
				string text = "";
				if (networkRoomInfo != null)
				{
					text = networkRoomInfo.Name;
				}
				if (roomInfo != null)
				{
					text = roomInfo.Name;
				}
				if (string.IsNullOrEmpty(text))
				{
					retry++;
					retry_elapsed = 0.5f;
					Debug.LogWarning($"PhotonService> TryJoinRoom / Room Not Found - retry[{retry}]!");
					if (retry < 3)
					{
						return true;
					}
					Debug.LogWarning("PhotonService> TryJoinRoom / Dispatch Room Not Active - room[" + text + "]");
					DispatchNetworkEvent(EventType.OnRoomNotActive, text);
					return false;
				}
				bool flag = false;
				if (networkRoomInfo != null && networkRoomInfo.PlayerCount >= networkRoomInfo.MaxPlayers)
				{
					flag = true;
				}
				if (roomInfo != null && roomInfo.PlayerCount >= roomInfo.MaxPlayers)
				{
					flag = true;
				}
				if (flag)
				{
					Debug.LogWarning("PhotonService> TryJoinRoom - Room is Full!");
					DispatchNetworkEvent(EventType.OnRoomFull, text);
					return false;
				}
				PhotonNetwork.JoinRoom(text);
				return false;
			}, 0f, false);
			State = ServiceState.InProgress;
		}

		protected void TryJoinRandomRoom(NetworkRoomOptions options)
		{
			PhotonNetwork.JoinRandomRoom(options.QuickMatchProperties, 0);
			State = ServiceState.InProgress;
		}

		public void TryConnectToRegionMaster(CloudRegionCode region, string lobbyId = null)
		{
			ServerLobby.Name = lobbyId;
			StopAllCoroutines();
			StartCoroutine(ConnectRegion(region));
		}

		public void TryDisconnect()
		{
			if (State == ServiceState.Disconnected)
			{
				return;
			}
			PhotonNetwork.Disconnect();
			State = ServiceState.InProgress;
			if (m_disconnect_poll != null)
			{
				m_disconnect_poll.Stop();
			}
			float t = 0f;
			m_disconnect_poll = Activity.Run((Func<bool>)delegate
			{
				if (State == ServiceState.Disconnected)
				{
					m_disconnect_poll = null;
					return false;
				}
				if (PhotonNetwork.networkingPeer != null)
				{
					switch (PhotonNetwork.networkingPeer.PeerState)
					{
					case PeerStateValue.Connecting:
					case PeerStateValue.Connected:
						State = ServiceState.Disconnected;
						break;
					case PeerStateValue.Disconnected:
					case PeerStateValue.Disconnecting:
						State = ServiceState.Disconnected;
						break;
					}
				}
				t += Time.unscaledDeltaTime;
				if (t < 5f)
				{
					return true;
				}
				Debug.Log("PhotonService> TryDisconnect / Timeout for Disconnection...");
				State = ServiceState.Disconnected;
				return true;
			}, 0f, false);
		}

		public void TryLeaveRoom()
		{
			PhotonNetwork.LeaveRoom(becomeInactive: false);
			State = ServiceState.InProgress;
		}

		public void ConnectToNameServer()
		{
			PhotonNetwork.gameVersion = GameVersion;
			PhotonNetwork.networkingPeer.AppId = PhotonNetwork.PhotonServerSettings.AppID;
			PhotonNetwork.networkingPeer.ConnectToNameServer();
		}

		private IEnumerator ConnectToBestMaster(string lobbyId)
		{
			if (State != ServiceState.Disconnected)
			{
				TryDisconnect();
				while (State != ServiceState.Disconnected)
				{
					yield return null;
				}
			}
			if (State == ServiceState.Disconnected)
			{
				PhotonNetwork.gameVersion = GameVersion;
				PhotonNetwork.networkingPeer.AppId = PhotonNetwork.PhotonServerSettings.AppID;
				ServerLobby.Name = lobbyId;
				NetworkingPeer pnp = PhotonNetwork.networkingPeer;
				pnp.ConnectToNameServer();
				State = ServiceState.InProgress;
				while (State == ServiceState.InProgress && pnp.State != ClientState.ConnectedToNameServer)
				{
					yield return null;
				}
				Debug.Log("PhotonService> ConnectToBestMaster / GetRegions");
				pnp.OpGetRegions(pnp.AppId);
				if (!RegionPingActive)
				{
					RegionPingActive = true;
					StartCoroutine(PhotonHandler.SP.PingAvailableRegionsCoroutine(connectToBest: false));
				}
				List<Region> rl = new List<Region>();
				int c = 0;
				bool has_ping = false;
				while (true)
				{
					List<Region> availableRegions = pnp.AvailableRegions;
					rl.Clear();
					if (availableRegions != null)
					{
						rl.AddRange(availableRegions);
					}
					rl.Sort(delegate(Region a, Region b)
					{
						if (a.Ping > 0)
						{
							has_ping = true;
						}
						if (b.Ping > 0)
						{
							has_ping = true;
						}
						if (a.Ping == b.Ping)
						{
							if (a.Code != CloudRegionCode.us)
							{
								if (b.Code != CloudRegionCode.us)
								{
									return 0;
								}
								return 1;
							}
							return -1;
						}
						return (a.Ping >= b.Ping) ? 1 : (-1);
					});
					int num = (has_ping ? 2 : 8);
					int num2 = c + 1;
					c = num2;
					if (num2 >= num)
					{
						break;
					}
					yield return new WaitForSeconds(1f);
				}
				if (has_ping)
				{
					AvailableRegions.Clear();
					AvailableRegions.AddRange(rl);
				}
				rl = AvailableRegions;
				if (rl.Count >= 1)
				{
					Debug.Log($"PhotonService> ConnectToBestMaster / Found Region [{rl[0].Code}]");
					pnp.ConnectToRegionMaster(rl[0].Code);
				}
				else
				{
					Debug.Log($"PhotonService> ConnectToBestMaster / Default Region [{CloudRegionCode.us}]");
					pnp.ConnectToRegionMaster(CloudRegionCode.us);
				}
				c = 0;
				while (State != ServiceState.InLobby && c++ < 10)
				{
					yield return new WaitForSeconds(1f);
				}
			}
			if (State != ServiceState.InLobby)
			{
				DisconnectionReason = DisconnectionCode.Unknown;
			}
		}

		private IEnumerator ConnectRandom(NetworkRoomOptions options)
		{
			RefreshAuth();
			OnQuickMatchChanged(QuickMatchState.FindingBestServer);
			yield return StartCoroutine(ConnectToBestMaster(null));
			OnQuickMatchChanged(QuickMatchState.ConnectedBestServer);
			TryJoinRandomRoom(options);
			while (State == ServiceState.InProgress)
			{
				yield return null;
			}
			if (State == ServiceState.InLobby)
			{
				Debug.Log("PhotonService: Could not join random room. Will try to create one ");
				TryCreateRoom(options);
			}
		}

		private IEnumerator ConnectRegion(CloudRegionCode newRegion)
		{
			TryDisconnect();
			while (State != ServiceState.Disconnected)
			{
				yield return null;
			}
			State = ServiceState.InProgress;
			RefreshAuth();
			bool flag = PhotonNetwork.ConnectToRegion(newRegion, GameVersion);
			Debug.Log($"PhotonService> ConnectRegion / region[{newRegion}] result[{flag}]");
		}

		public void TryJoinQuickMatchInRegion(CloudRegionCode region, string roomId, Action<QuickMatchResult> onResult)
		{
			StopAllCoroutines();
			QuickMatch = new QuickMatchResult();
			OnQuickMatchResult = onResult;
			StartCoroutine(JoinRoomInRegion(region, roomId));
		}

		public void TryJoinRoomInRegion(CloudRegionCode region, string roomId)
		{
			StopAllCoroutines();
			StartCoroutine(JoinRoomInRegion(region, roomId));
		}

		private IEnumerator JoinRoomInRegion(CloudRegionCode region, string roomId)
		{
			Debug.Log($"PhotonService> JoinRoomInRegion / Disconnecting - state[{State}]");
			if (State != ServiceState.Disconnected)
			{
				TryDisconnect();
				while (State != ServiceState.Disconnected)
				{
					yield return null;
				}
			}
			ServerLobby.Name = null;
			Debug.Log($"PhotonService> JoinRoomInRegion / Connecting to Region - region[{region}]");
			yield return StartCoroutine(ConnectRegion(region));
			Debug.Log($"PhotonService> JoinRoomInRegion / Wating... - state[{State}]");
			while (State != ServiceState.InLobby)
			{
				yield return 0;
			}
			Debug.Log("PhotonService> JoinRoomInRegion / InLobby - Joining Room - room-id[" + roomId + "]");
			TryJoinRoom(roomId);
			Debug.Log($"PhotonService> JoinRoomInRegion / InLobby - Waiting for Room... - state[{State}]");
			while (State != ServiceState.InRoom)
			{
				yield return 0;
			}
			Debug.Log($"PhotonService> JoinRoomInRegion / Complete - state[{State}]");
		}

		public void TryCreateRoom(NetworkRoomOptions options)
		{
			PhotonNetwork.CreateRoom(null, options.GetPhotonRoomOptions(), null);
			State = ServiceState.InProgress;
			if (options.MatchmakingType == NetworkRoom.MatchmakingFlow.Normal)
			{
				QuickMatch = null;
			}
		}

		public void TryJoinOrCreateCustomRoom(string matchId, NetworkRoomOptions options)
		{
			StartCoroutine(TryJoinOrCreateRoom(matchId, options));
		}

		private IEnumerator TryJoinOrCreateRoom(string matchId, NetworkRoomOptions options)
		{
			if (State == ServiceState.InRoom)
			{
				TryLeaveRoom();
				while (State != ServiceState.InLobby)
				{
					yield return null;
				}
			}
			PhotonNetwork.JoinOrCreateRoom(ServerLobby.Name + "-" + matchId, options.GetPhotonRoomOptions(), ServerLobby, options.ExpectedPlayers);
			State = ServiceState.InProgress;
			QuickMatch = null;
		}

		public bool TryGetPlayer(int ActorId, out NetworkActor player)
		{
			player = null;
			if (CurrentRoom != null)
			{
				player = CurrentRoom.TryGetPlayer(ActorId);
			}
			return player != null;
		}

		public override void OnConnectedToMaster()
		{
			Debug.Log("PhotonService> OnConnectedToMaster / Joining lobby[" + ServerLobby.Name + "]");
			State = ServiceState.InMaster;
			PhotonNetwork.JoinLobby(ServerLobby);
		}

		public override void OnJoinedLobby()
		{
			CurrentLobby.Region = CurrentRegionCode.ToString();
			CurrentLobby.IsConnected = true;
			CurrentLobby.PingTime = PingTime;
			State = ServiceState.InLobby;
			DispatchNetworkEvent(EventType.OnJoinLobby, CurrentLobby);
			Debug.Log($"PhotonService > OnJoinedLobby {ServerLobby.Name} isLAN[{IsConnectedToLocal}]");
		}

		public override void OnJoinedRoom()
		{
			DisconnectionReason = DisconnectionCode.ByUser;
			CurrentRoom = null;
			Room room = PhotonNetwork.room;
			if (room != null)
			{
				NetworkRoom.GameType gamemode = (NetworkRoom.GameType)room.CustomProperties["g"];
				CurrentRoom = new NetworkRoom(this, room, gamemode);
			}
			State = ServiceState.InRoom;
			DispatchNetworkEvent(EventType.OnJoinRoom, CurrentRoom);
			CurrentRoom.OnEvent = delegate(NetworkRoom.GameEvent gameEvent)
			{
				if (OnGameEvent != null)
				{
					OnGameEvent(gameEvent);
				}
			};
			KickedFromRoom = "";
			OnQuickMatchChanged(QuickMatchState.JoinedRoom, CurrentRoom);
		}

		public override void OnReceivedRoomListUpdate()
		{
			CurrentLobby.Rooms.Clear();
			CurrentLobby.PingTime = PingTime;
			RoomInfo[] roomList = PhotonNetwork.GetRoomList();
			foreach (RoomInfo roomInfo in roomList)
			{
				if (roomInfo != null)
				{
					CurrentLobby.Rooms.Add(new Lobby.NetworkRoomInfo(roomInfo));
				}
			}
			DispatchNetworkEvent(EventType.OnLobbyUpdated, CurrentLobby);
		}

		public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
		{
			if (CurrentRoom != null)
			{
				NetworkActor content = CurrentRoom.OnPlayerJoin(newPlayer);
				DispatchNetworkEvent(EventType.OnPlayerJoin, content);
				OnQuickMatchChanged(QuickMatchState.MatchmakingChanged);
			}
		}

		public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
		{
			if (otherPlayer == null)
			{
				Debug.LogWarning("PhotonService> OnPhotonPlayerDisconnected / other-player is null");
			}
			if (CurrentRoom == null)
			{
				Debug.LogWarning("PhotonService> OnPhotonPlayerDisconnected / current-room is null");
			}
			if (CurrentRoom != null)
			{
				CurrentRoom.OnPlayerLeft(otherPlayer);
			}
			DispatchNetworkEvent(EventType.OnPlayerLeft, otherPlayer?.ID ?? (-1));
			OnQuickMatchChanged(QuickMatchState.MatchmakingChanged);
		}

		public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
		{
			if (CurrentRoom != null)
			{
				CurrentRoom.OnMasterClientSwitched(newMasterClient);
				DispatchNetworkEvent(EventType.OnMasterChanged, newMasterClient?.ID ?? (-1));
			}
		}

		public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
		{
			if (playerAndUpdatedProps != null && playerAndUpdatedProps.Length != 0)
			{
				if (CurrentRoom != null)
				{
					CurrentRoom.OnPlayerPropertiesChanged(playerAndUpdatedProps);
				}
				PhotonPlayer photonPlayer = playerAndUpdatedProps[0] as PhotonPlayer;
				NetworkActor player = null;
				if (photonPlayer != null && TryGetPlayer(photonPlayer.ID, out player))
				{
					DispatchNetworkEvent(EventType.OnPlayerUpdated, player);
				}
			}
		}

		public override void OnConnectionFail(DisconnectCause cause)
		{
			switch (cause)
			{
			case DisconnectCause.DisconnectByServerUserLimit:
			case DisconnectCause.DisconnectByServerLogic:
			case DisconnectCause.AuthenticationTicketExpired:
			case DisconnectCause.InvalidRegion:
			case DisconnectCause.MaxCcuReached:
			case DisconnectCause.InvalidAuthentication:
				DisconnectionReason = DisconnectionCode.ByServerLogic;
				break;
			case DisconnectCause.DisconnectByClientTimeout:
			case DisconnectCause.DisconnectByServerTimeout:
				DisconnectionReason = DisconnectionCode.Timeout;
				break;
			case DisconnectCause.SecurityExceptionOnConnect:
			case DisconnectCause.ExceptionOnConnect:
			case DisconnectCause.Exception:
			case DisconnectCause.InternalReceiveException:
				DisconnectionReason = DisconnectionCode.Exception;
				break;
			default:
				DisconnectionReason = DisconnectionCode.Unknown;
				break;
			}
			if ((bool)slack)
			{
				List<string> list = new List<string>();
				list.Add("<Photon Error>");
				list.Add("Type: OnConnectionFail");
				list.Add($"Cause: {cause}");
				bool flag = PhotonNetwork.AuthValues != null;
				list.Add($"Auth: {flag}");
				if (flag)
				{
					list.Add("AuthToken: " + PhotonNetwork.AuthValues.Token);
					list.Add("AuthUserId: " + PhotonNetwork.AuthValues.UserId);
				}
				slack.ReportToSlack(string.Join("\n", list), "");
			}
			Debug.LogError($"PhotonService> OnConnectionFail / cause[{cause}]");
		}

		public override void OnFailedToConnectToPhoton(DisconnectCause cause)
		{
			if ((bool)slack)
			{
				List<string> list = new List<string>();
				list.Add("<Photon Error>");
				list.Add("Type: OnFailedToConnectToPhoton");
				list.Add($"Cause: {cause}");
				bool flag = PhotonNetwork.AuthValues != null;
				list.Add($"Auth: {flag}");
				if (flag)
				{
					list.Add("AuthToken: " + PhotonNetwork.AuthValues.Token);
					list.Add("AuthUserId: " + PhotonNetwork.AuthValues.UserId);
				}
				slack.ReportToSlack(string.Join("\n", list), "");
			}
		}

		public override void OnCustomAuthenticationFailed(string cause)
		{
			if ((bool)slack)
			{
				List<string> list = new List<string>();
				list.Add("<Photon Error>");
				list.Add("Type: OnFailedToConnectToPhoton");
				list.Add("Cause: " + cause);
				bool flag = PhotonNetwork.AuthValues != null;
				list.Add($"Auth: {flag}");
				if (flag)
				{
					list.Add("AuthToken: " + PhotonNetwork.AuthValues.Token);
					list.Add("AuthUserId: " + PhotonNetwork.AuthValues.UserId);
				}
				slack.ReportToSlack(string.Join("\n", list), "");
			}
		}

		public override void OnDisconnectedFromPhoton()
		{
			Debug.Log("PhotonService> OnDisconnectedFromPhoton");
			CurrentLobby.Reset();
			State = ServiceState.Disconnected;
			CurrentRoom = null;
		}

		public override void OnLeftRoom()
		{
			Debug.Log("PhotonService> OnLeftRoom");
			State = ServiceState.InProgress;
			if (CurrentRoom != null)
			{
				CurrentRoom.OnRoomLeft();
			}
			DispatchNetworkEvent(EventType.OnLeftRoom);
			CurrentRoom = null;
		}

		public override void OnLeftLobby()
		{
			CurrentLobby.Reset();
			State = ServiceState.InMaster;
			DispatchNetworkEvent(EventType.OnLeftLobby);
		}

		public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
		{
			State = ServiceState.InLobby;
			DispatchNetworkEvent(EventType.OnJoinRoomFailed, codeAndMsg);
		}

		public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
		{
			State = ServiceState.InLobby;
			OnQuickMatchChanged(QuickMatchState.CreatingRoom);
		}

		public override void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
		{
			if (CurrentRoom != null)
			{
				CurrentRoom.OnRoomPropertiesChanged(propertiesThatChanged);
			}
			DispatchNetworkEvent(EventType.OnRoomUpdated, propertiesThatChanged);
			OnQuickMatchChanged(QuickMatchState.MatchmakingChanged);
		}

		public override void OnPhotonCreateRoomFailed(object[] codeAndMsg)
		{
			DispatchNetworkEvent(EventType.OnCreateRoomFailed, codeAndMsg);
			OnQuickMatchChanged(QuickMatchState.Failed);
		}

		protected void OnQuickMatchChanged(QuickMatchState state, NetworkRoom joinedRoom = null)
		{
			if (OnQuickMatchResult == null || QuickMatch == null)
			{
				return;
			}
			QuickMatch.State = state;
			if (joinedRoom != null)
			{
				QuickMatch.JoinedRoom = joinedRoom;
				if (!joinedRoom.IsQuickMatch)
				{
					QuickMatch = null;
				}
			}
			OnQuickMatchResult(QuickMatch);
		}

		public override void OnWebRpcResponse(OperationResponse response)
		{
		}

		public void ForceStartMatch()
		{
			if (CurrentRoom != null)
			{
				CurrentRoom.ForceStartMatch();
			}
		}

		public void SendLevelLoaded()
		{
			if (CurrentRoom != null)
			{
				CurrentRoom.SendLevelLoaded();
			}
		}

		public void TryKickPlayer(NetworkActor playerToKick)
		{
			if (CurrentRoom != null && playerToKick != null)
			{
				CurrentRoom.Outgoing.SendPlayerKick(playerToKick.ID);
			}
		}

		public void TrySetMaster(NetworkActor newMaster)
		{
			if (CurrentRoom == null)
			{
				Debug.LogWarning("PhotonService> TryChangeMaster / Room is null");
				return;
			}
			if (newMaster == null)
			{
				Debug.LogWarning("PhotonService> TryChangeMaster / NewMaster is null");
				return;
			}
			if (newMaster.IsMaster)
			{
				Debug.LogWarning("PhotonService> TryChangeMaster / user[" + newMaster.ProfileName + "] is already Master");
				return;
			}
			Debug.Log("PhotonService> TryChangeMaster / user[" + newMaster.ProfileName + "] will be set to master now");
			PhotonNetwork.SetMasterClient(newMaster.RawData);
		}

		protected void Update()
		{
			if (PhotonNetwork.connectedAndReady && CurrentRoom != null)
			{
				CurrentRoom.Update();
			}
			if (CurrentRoom == null && !string.IsNullOrEmpty(KickedFromRoom))
			{
				kickedCooldown += Time.deltaTime;
				if (kickedCooldown > 30f)
				{
					kickedCooldown = 0f;
					KickedFromRoom = "";
				}
			}
		}
	}
}

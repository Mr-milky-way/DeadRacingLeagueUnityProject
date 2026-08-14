using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using Photon.Chat;
using UnityEngine;
using drl.backend;
using drl.core;
using drl.network;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class NetworkModel : Model<DRLApp>
	{
		[Header("[-- Server data --]")]
		[SerializeField]
		private FloatProperty pingQuality;

		[SerializeField]
		private IntProperty pingTime;

		[SerializeField]
		private BoolProperty isLAN;

		[SerializeField]
		private StringProperty regionCodeName;

		[SerializeField]
		private LobbyProperty lobbyData;

		[Header("[-- Room data --]")]
		[SerializeField]
		private RoomProperty roomData;

		[SerializeField]
		public DRLTournamentMatchData tournamentMatchData;

		public bool isOnline = true;

		public bool debugKeepAliveThread;

		public List<NetworkReplayRequest> replayLoadQueue;

		private DateTime LastReadTime;

		private Dictionary<int, Tuple<float, float[]>> m_damageData = new Dictionary<int, Tuple<float, float[]>>();

		private bool m_photon_keepalive_active;

		private int m_photon_keepalive_max_steps;

		private Thread m_photon_keepalive_loop;

		private Stopwatch debugTimer = new Stopwatch();

		private Task m_keepAliveTask;

		private CancellationTokenSource m_tokenSource;

		public PhotonService photon => AssertLocal<PhotonService>("photon");

		public PhotonService.ServiceState connectionState => photon.State;

		public Dictionary<int, NetworkActor> players => photon.Players;

		public NetworkRoom room
		{
			get
			{
				if (!photon)
				{
					return null;
				}
				return photon.CurrentRoom;
			}
		}

		public bool InRoom
		{
			get
			{
				if (room != null)
				{
					return room.Local != null;
				}
				return false;
			}
		}

		public bool isMaster => photon.IsMaster;

		public Lobby lobby => photon.CurrentLobby;

		public string LobbyId => photon.ServerLobby.Name;

		public string ServerIP => photon.ServerIP;

		public string LocalIPAddress
		{
			get
			{
				return photon.LocalIPAddress;
			}
			set
			{
				photon.LocalIPAddress = value;
			}
		}

		public List<Lobby.NetworkRoomInfo> rooms
		{
			get
			{
				if (lobby != null)
				{
					return lobby.Rooms;
				}
				return new List<Lobby.NetworkRoomInfo>();
			}
		}

		public NetworkRoom.StateCode gameState
		{
			get
			{
				if (photon.CurrentRoom != null)
				{
					return photon.CurrentRoom.State;
				}
				return NetworkRoom.StateCode.None;
			}
		}

		public CloudRegionCode region => photon.CurrentRegionCode;

		public bool CanChat
		{
			get
			{
				if (room != null)
				{
					return connectionState == PhotonService.ServiceState.InRoom;
				}
				return false;
			}
		}

		public string regionName
		{
			get
			{
				if (!IsConnectedToLAN)
				{
					return base.app.model.storage.locale.Get<string>(photon.CurrentRegionName);
				}
				return "LAN";
			}
		}

		public bool connected
		{
			get
			{
				if (!photon)
				{
					return false;
				}
				return photon.IsConnectedAndReady;
			}
		}

		public Queue<NetworkRoomChat.Message> ChatHistory
		{
			get
			{
				if (photon.CurrentRoom != null)
				{
					return photon.CurrentRoom.Chat.History;
				}
				return new Queue<NetworkRoomChat.Message>();
			}
		}

		public bool HasUnreadChatMessages
		{
			get
			{
				if (photon.CurrentRoom != null)
				{
					return photon.CurrentRoom.Chat.HasUnreadMessages;
				}
				return false;
			}
		}

		public DateTime LastChatMessageTime
		{
			get
			{
				if (photon.CurrentRoom != null)
				{
					return photon.CurrentRoom.Chat.LastMessageTime;
				}
				return DateTime.UtcNow;
			}
		}

		public PhotonLANServer LanServer => photon.LanServer;

		public bool IsConnectedToLAN
		{
			get
			{
				if (!photon)
				{
					return false;
				}
				return photon.IsConnectedToLocal;
			}
		}

		public bool IsTournamentMatch
		{
			get
			{
				if (photon.CurrentRoom != null)
				{
					return photon.CurrentRoom.IsTournamentMatch;
				}
				return false;
			}
		}

		public int OnlinePlayers => photon.OnlinePlayers;

		public bool fetchingBots { get; set; }

		public bool isLANAllowed
		{
			get
			{
				if (!base.validContext)
				{
					return false;
				}
				if ((bool)base.app && (bool)base.app.model && (bool)base.app.model.storage)
				{
					return base.app.model.storage.state.player.profile.isDeveloper;
				}
				return false;
			}
		}

		protected void Start()
		{
			PhotonService photonService = photon;
			photonService.OnStateChanged = (Action<PhotonService.ServiceState>)Delegate.Remove(photonService.OnStateChanged, new Action<PhotonService.ServiceState>(OnPhotonServiceStateChanged));
			PhotonService photonService2 = photon;
			photonService2.OnStateChanged = (Action<PhotonService.ServiceState>)Delegate.Combine(photonService2.OnStateChanged, new Action<PhotonService.ServiceState>(OnPhotonServiceStateChanged));
			PhotonService photonService3 = photon;
			photonService3.OnNetworkEvent = (Action<PhotonService.EventType, object>)Delegate.Remove(photonService3.OnNetworkEvent, new Action<PhotonService.EventType, object>(OnPhotonServiceNetworkEvent));
			PhotonService photonService4 = photon;
			photonService4.OnNetworkEvent = (Action<PhotonService.EventType, object>)Delegate.Combine(photonService4.OnNetworkEvent, new Action<PhotonService.EventType, object>(OnPhotonServiceNetworkEvent));
			PhotonService photonService5 = photon;
			photonService5.OnGameEvent = (Action<NetworkRoom.GameEvent>)Delegate.Remove(photonService5.OnGameEvent, new Action<NetworkRoom.GameEvent>(OnPhotonRoomEvent));
			PhotonService photonService6 = photon;
			photonService6.OnGameEvent = (Action<NetworkRoom.GameEvent>)Delegate.Combine(photonService6.OnGameEvent, new Action<NetworkRoom.GameEvent>(OnPhotonRoomEvent));
			PhotonService photonService7 = photon;
			photonService7.OnLANServerChanged = (Action<PhotonLANServerDeprecated.ServerState>)Delegate.Remove(photonService7.OnLANServerChanged, new Action<PhotonLANServerDeprecated.ServerState>(OnLANServerChanged));
			PhotonService photonService8 = photon;
			photonService8.OnLANServerChanged = (Action<PhotonLANServerDeprecated.ServerState>)Delegate.Combine(photonService8.OnLANServerChanged, new Action<PhotonLANServerDeprecated.ServerState>(OnLANServerChanged));
			lobbyData.Value = photon.CurrentLobby;
			photon.GameVersion = DRLVersion.server;
		}

		public void StartKeepAliveLoop()
		{
			if (m_keepAliveTask != null)
			{
				m_photon_keepalive_active = false;
				m_tokenSource.Cancel();
				m_keepAliveTask.Dispose();
			}
			if (room == null)
			{
				return;
			}
			((Component)this).TimerRun((Func<bool>)delegate
			{
				if (m_keepAliveTask != null && m_keepAliveTask.Status == TaskStatus.Running)
				{
					return true;
				}
				m_tokenSource?.Dispose();
				m_tokenSource = new CancellationTokenSource();
				CancellationToken t = m_tokenSource.Token;
				m_photon_keepalive_active = true;
				m_keepAliveTask = Task.Run(delegate
				{
					KeepAliveLoopTask(t);
				}, t);
				return false;
			}, 0f);
		}

		private async void KeepAliveLoopTask(CancellationToken p_token)
		{
			m_photon_keepalive_max_steps = (room?.GameLoadingTimeout ?? 300) * 10;
			UnityEngine.Debug.Log("NetworkModel> StartKeepAliveLoop: steps " + m_photon_keepalive_max_steps);
			ChatClient cc = base.app.model.chat.service.Client;
			if (debugKeepAliveThread)
			{
				debugTimer.Reset();
				debugTimer.Start();
			}
			while (!p_token.IsCancellationRequested)
			{
				m_photon_keepalive_max_steps--;
				if (m_photon_keepalive_max_steps <= 0 || !m_photon_keepalive_active)
				{
					UnityEngine.Debug.Log("NetworkModel> StopKeepAliveLoop Complete");
					m_photon_keepalive_active = false;
					m_photon_keepalive_loop = null;
					if (debugKeepAliveThread)
					{
						debugTimer.Reset();
						debugTimer.Stop();
					}
					break;
				}
				NetworkingPeer networkingPeer = PhotonNetwork.networkingPeer;
				if (networkingPeer != null)
				{
					networkingPeer.SendAcksOnly();
					if (debugKeepAliveThread)
					{
						UnityEngine.Debug.Log("NetworkModel> KeepAliveThread:\nMax steps " + m_photon_keepalive_max_steps + " at : " + DateTime.UtcNow.ToString() + "\n" + debugTimer.Elapsed.TotalSeconds);
					}
				}
				cc?.SendAcksOnly();
				await Task.Delay(80, p_token);
			}
		}

		public void StopKeepAliveLoop(float p_delay)
		{
			UnityEngine.Debug.Log($"NetworkModel> StopKeepAliveLoop Start / steps[{m_photon_keepalive_max_steps}]");
			m_photon_keepalive_max_steps = Mathf.Min((int)(p_delay * 1000f) / 80, m_photon_keepalive_max_steps);
			this.TimerRunOnce(delegate
			{
				if (m_keepAliveTask != null && m_keepAliveTask.Status == TaskStatus.Running)
				{
					m_tokenSource?.Cancel();
				}
			}, p_delay);
		}

		protected void RefreshPhotonAuth()
		{
		}

		public void CreateRoom(NetworkRoomOptions roomSettings)
		{
			photon.TryCreateRoom(roomSettings);
		}

		public void CreateRoom(GameFlag p_type)
		{
			NetworkRoom.GameType gamemode = NetworkRoom.GameType.Freestyle;
			switch (p_type)
			{
			case GameFlag.Freestyle:
				gamemode = NetworkRoom.GameType.Freestyle;
				break;
			case GameFlag.Race:
				gamemode = NetworkRoom.GameType.Race;
				break;
			}
			UnityEngine.Debug.Log("NetworkModel> CreateRoom - game-type[" + gamemode.ToString() + "/" + p_type.ToString() + "]");
			NetworkRoomOptions networkRoomOptions = new NetworkRoomOptions(gamemode, NetworkRoom.MatchmakingFlow.Normal);
			networkRoomOptions.Crossplay = base.app.model.storage.state.player.settings.game.crossplay;
			networkRoomOptions.MasterPlatform = OS.GetPlatformByContext();
			CreateRoom(networkRoomOptions);
		}

		public GameFlag GetGameType(NetworkRoom.GameType p_type)
		{
			return p_type switch
			{
				NetworkRoom.GameType.Race => GameFlag.Race, 
				NetworkRoom.GameType.Tournament => GameFlag.Race, 
				NetworkRoom.GameType.Freestyle => GameFlag.Freestyle, 
				_ => GameFlag.None, 
			};
		}

		public void ConnectToTournamentLobby(DRLTournamentData data)
		{
			if (data == null)
			{
				UnityEngine.Debug.LogError("NetworkModel> ConnectToTournamentLobby - Can't connect to tournament, tournmanet data is null");
				return;
			}
			if (string.IsNullOrEmpty(data.id))
			{
				UnityEngine.Debug.LogError("NetworkModel> ConnectToTournamentLobby - Can't connect to tournament, tournmanet Id is null or empty");
				return;
			}
			string guid = data.guid;
			CloudRegionCode cloudRegionCode = CloudRegionCode.none;
			switch (data.region)
			{
			case "us":
				cloudRegionCode = CloudRegionCode.us;
				break;
			case "eu":
				cloudRegionCode = CloudRegionCode.eu;
				break;
			case "asia":
				cloudRegionCode = CloudRegionCode.asia;
				break;
			default:
				UnityEngine.Debug.LogWarning("NetworkModel> ConnectToTournamentLobby / " + data.region + " not implemented, falling back to US region");
				cloudRegionCode = CloudRegionCode.us;
				break;
			}
			PlayerStateModel player = base.app.model.storage.state.player;
			photon.UserId = player.profile.playerId;
			RefreshPhotonAuth();
			if (data.enabledLAN)
			{
				ConnectToLAN(data.serverLAN, guid);
			}
			else
			{
				photon.TryConnectToRegionMaster(cloudRegionCode, guid);
			}
		}

		public void JoinTournamentMatch(DRLTournamentMatchData matchData)
		{
			if (matchData == null)
			{
				UnityEngine.Debug.LogError("NetworkModel> JoinTournamentMatch - MatchData is null");
				return;
			}
			if (matchData.state != TournamentMatchState.active)
			{
				UnityEngine.Debug.LogWarning($"NetworkModel>JoinTournamentMatch - The match requested is {matchData.state} and not active. Aborting...");
				return;
			}
			string[] array = Array.ConvertAll(matchData.players, (DRLTournamentPlayerData player) => player.playerId.ToString());
			NetworkRoomOptions networkRoomOptions = new NetworkRoomOptions(NetworkRoom.GameType.Tournament, NetworkRoom.MatchmakingFlow.Normal);
			networkRoomOptions.ExpectedPlayers = array;
			networkRoomOptions.ServerTime = matchData.currentTime;
			networkRoomOptions.MaxRacers = array.Length;
			networkRoomOptions.MaxSpectators = 15;
			networkRoomOptions.MapGUID = matchData.mapId;
			networkRoomOptions.MatchId = matchData.Id;
			networkRoomOptions.LobbyCountdownAllowed = matchData.roomTimerAllowed;
			networkRoomOptions.TrackGUID = (matchData.isCustomMap ? string.Empty : matchData.trackId);
			networkRoomOptions.CustomMapId = matchData.customMapId;
			networkRoomOptions.HeatIdx = matchData.currentHeat - 1;
			networkRoomOptions.MaxHeats = matchData.heatCount;
			networkRoomOptions.DroneClass = matchData.droneClass;
			networkRoomOptions.HasDelayedStart = base.app.inTournament && base.app.tournament.hasCountdown;
			tournamentMatchData = matchData;
			UnityEngine.Debug.Log("UITournamentBracketsController> Creating room: heat - " + networkRoomOptions.HeatIdx + " current-heat-backend - " + matchData.currentHeat + " active-heat-backend - " + matchData.activeHeat);
			photon.TryJoinOrCreateCustomRoom(matchData.Id, networkRoomOptions);
		}

		public void ConnectToLobby()
		{
			RefreshPhotonAuth();
			photon.TryConnectBestRegionLobby();
		}

		public void SwitchRegion(CloudRegionCode regionCode)
		{
			RefreshPhotonAuth();
			photon.TryConnectToRegionMaster(regionCode);
		}

		public void SearchQuickMatch(GameFlag gameType, string mapGUID = null, string trackGUID = null, Action<QuickMatchResult> onResult = null)
		{
			NetworkRoom.GameType gameType2 = NetworkRoom.GameType.Freestyle;
			switch (gameType)
			{
			case GameFlag.Freestyle:
				gameType2 = NetworkRoom.GameType.Freestyle;
				break;
			case GameFlag.Race:
				gameType2 = NetworkRoom.GameType.Race;
				break;
			}
			UnityEngine.Debug.Log("NetworkModel> QuickMatch / gameType[" + gameType2.ToString() + "/" + gameType.ToString() + "]");
			NetworkRoomOptions networkRoomOptions = new NetworkRoomOptions(gameType2, NetworkRoom.MatchmakingFlow.Quick);
			networkRoomOptions.QuickMatchProperties.Add("g", gameType2);
			networkRoomOptions.QuickMatchProperties.Add("mt", NetworkRoom.MatchmakingFlow.Quick);
			networkRoomOptions.QuickMatchProperties.Add("cr", true);
			if (!string.IsNullOrEmpty(mapGUID))
			{
				networkRoomOptions.QuickMatchProperties.Add("m", mapGUID);
			}
			if (!string.IsNullOrEmpty(trackGUID))
			{
				networkRoomOptions.QuickMatchProperties.Add("t", trackGUID);
			}
			networkRoomOptions.Crossplay = base.app.model.storage.state.player.settings.game.crossplay;
			networkRoomOptions.MasterPlatform = OS.GetPlatformByContext();
			photon.TryConnectRandomRoom(networkRoomOptions, onResult);
		}

		public void TryJoinQuickMatchInRegion(CloudRegionCode region, string roomId)
		{
			RefreshPhotonAuth();
			photon.TryJoinQuickMatchInRegion(region, roomId, delegate(QuickMatchResult result)
			{
				if (result != null)
				{
					Notify("network.qm.state.changed", result);
				}
			});
		}

		public void QuickFreestyle(Action<QuickMatchResult> onResult = null)
		{
			SearchQuickMatch(GameFlag.Freestyle, null, null, onResult);
		}

		public void QuickRace(Action<QuickMatchResult> onResult = null)
		{
			SearchQuickMatch(GameFlag.Race, null, null, onResult);
		}

		public void StartMatchmaking()
		{
			if (photon.CurrentRoom != null)
			{
				photon.CurrentRoom.StartMatchmaking();
			}
		}

		public void ResetMatchmakingTimeout()
		{
			if (photon.CurrentRoom != null)
			{
				photon.CurrentRoom.SendResetMatchmakingTimeout();
			}
		}

		public void ConnectToLAN(string p_server_ip, string p_tournamentId = null)
		{
			photon.TryConnectToLAN(p_server_ip, p_tournamentId);
		}

		[ContextMenu("Start LAN server")]
		public void StartLANServer()
		{
			photon.LanServer.Run();
		}

		[ContextMenu("Stop LAN server")]
		public void StopLANServer()
		{
			photon.LanServer.Stop();
		}

		public void Disconnect()
		{
			photon.TryDisconnect();
		}

		public void JoinRoom(string roomName)
		{
			photon.TryJoinRoom(roomName);
		}

		public void TryJoinRoomInRegion(CloudRegionCode region, string roomId)
		{
			RefreshPhotonAuth();
			photon.TryJoinRoomInRegion(region, roomId);
		}

		public void LeaveRoom()
		{
			photon.TryLeaveRoom();
		}

		public void SendLevelLoaded()
		{
			photon.SendLevelLoaded();
		}

		public void SendPlayerReady()
		{
			if (photon.CurrentRoom != null)
			{
				photon.CurrentRoom.Outgoing.SendPlayerReady();
			}
		}

		public void SendPlayerCountdownReady()
		{
			if (photon.CurrentRoom != null)
			{
				photon.CurrentRoom.Outgoing.SendPlayerCountdownReady();
			}
		}

		public void SendPlayerSkippedIntro()
		{
			if (photon.CurrentRoom != null)
			{
				photon.CurrentRoom.Outgoing.SendPlayerSkippedIntro();
			}
		}

		public void TrySetMaster(NetworkActor newMaster)
		{
			photon.TrySetMaster(newMaster);
		}

		public void SwitchToRacer()
		{
			if (photon.CurrentRoom != null)
			{
				photon.CurrentRoom.TrySwitchToRacer(photon.CurrentRoom.Local);
			}
		}

		public void SwitchToSpectator()
		{
			if (photon.CurrentRoom != null)
			{
				photon.CurrentRoom.TrySwitchToSpectator(photon.CurrentRoom.Local);
			}
		}

		public void ForceToSpectator(NetworkActor playerForcedToSpectate)
		{
			if (photon.CurrentRoom != null && photon.IsMaster)
			{
				photon.CurrentRoom.TrySwitchToSpectator(playerForcedToSpectate);
			}
		}

		public void ForceToRacer(NetworkActor playerForcedToRace)
		{
			if (photon.CurrentRoom != null && photon.IsMaster)
			{
				photon.CurrentRoom.TrySwitchToRacer(playerForcedToRace);
			}
		}

		public void SwapPlayers(NetworkActor playerA, NetworkActor playerB)
		{
			if (photon.CurrentRoom == null || !photon.IsMaster)
			{
				return;
			}
			int order = playerA.Order;
			int order2 = playerB.Order;
			playerA.Order = order2;
			playerB.Order = order;
			if (playerA.IsSpectator != playerB.IsSpectator)
			{
				playerA.IsSpectator = !playerA.IsSpectator;
				if (playerA.IsSpectator)
				{
					room.Outgoing.SendSwitchToSpectator(playerA.ID);
				}
				else
				{
					room.Outgoing.SendSwitchToRacer(playerA.ID);
				}
				playerB.IsSpectator = !playerB.IsSpectator;
				if (playerB.IsSpectator)
				{
					room.Outgoing.SendSwitchToSpectator(playerB.ID);
				}
				else
				{
					room.Outgoing.SendSwitchToRacer(playerB.ID);
				}
			}
		}

		public void SwapPlayerToCard(NetworkActor player, bool isSpectator, int uiOrder)
		{
			if (room == null || !photon.IsMaster)
			{
				return;
			}
			player.Order = uiOrder;
			if (player.IsSpectator != isSpectator)
			{
				player.IsSpectator = isSpectator;
				if (isSpectator)
				{
					room.Outgoing.SendSwitchToSpectator(player.ID);
				}
				else
				{
					room.Outgoing.SendSwitchToRacer(player.ID);
				}
			}
		}

		public void SetRoomReady(bool isReady)
		{
			if (photon.CurrentRoom != null)
			{
				photon.CurrentRoom.Local.IsRoomReady = isReady;
			}
		}

		public void SetBadgeLevel(int badgeLevel)
		{
			if (photon.CurrentRoom != null)
			{
				photon.CurrentRoom.Local.BadgeLevel = badgeLevel;
			}
		}

		public void SetPlatform(string p_platform)
		{
			if (photon.CurrentRoom != null)
			{
				photon.CurrentRoom.Local.Platform = p_platform;
			}
		}

		public void SendChatMessage(string messageText)
		{
			if (photon.CurrentRoom != null)
			{
				photon.CurrentRoom.Chat.SendChatMessage(messageText);
			}
		}

		public void MarkChatAsRead()
		{
			if (photon.CurrentRoom != null)
			{
				photon.CurrentRoom.Chat.MarkAsRead();
			}
			Notify("social.badges.clear", "room-chat");
		}

		public void SendGateEvent(int gateId)
		{
			if (photon.CurrentRoom != null)
			{
				photon.CurrentRoom.Outgoing.SendGateEvent(gateId);
			}
		}

		[ContextMenu("Test Map voting generation")]
		public void GenerateVotingTrackList()
		{
			List<DRLMapTrack> list = base.app.model.storage.library.FindAll<DRLMapTrack>();
			for (int i = 0; i < list.Count; i++)
			{
				GameFlagTag component = list[i].GetComponent<GameFlagTag>();
				if ((bool)component && component.Contains(GameFlag.MapEditorOnly))
				{
					list.RemoveAt(i--);
				}
				if ((bool)component && !component.Contains(GameFlag.Race))
				{
					list.RemoveAt(i--);
				}
				if ((bool)component && component.Contains(GameFlag.Development))
				{
					list.RemoveAt(i--);
				}
			}
			List<MapData> collection = base.app.model.storage.maps.Find(true, GameFlag.MapDRL);
			List<MapData> collection2 = base.app.model.storage.maps.Find(true, GameFlag.MapMultiGP);
			List<MapData> list2 = new List<MapData>();
			list2.AddRange(collection);
			list2.AddRange(collection2);
			list2.RemoveAll((MapData map) => !map.isPublic);
			switch (room.MapVotingCategory)
			{
			case MapCategory.Basic:
				list.RemoveAll((DRLMapTrack m) => m.difficulty != 0);
				list2.RemoveAll((MapData m) => m.mapDifficulty != 0);
				break;
			case MapCategory.Easy:
				list.RemoveAll((DRLMapTrack m) => m.difficulty != 1);
				list2.RemoveAll((MapData m) => m.mapDifficulty != 1);
				break;
			case MapCategory.Medium:
				list.RemoveAll((DRLMapTrack m) => m.difficulty != 2);
				list2.RemoveAll((MapData m) => m.mapDifficulty != 2);
				break;
			case MapCategory.Hard:
				list.RemoveAll((DRLMapTrack m) => m.difficulty != 3);
				list2.RemoveAll((MapData m) => m.mapDifficulty != 3);
				break;
			case MapCategory.MultiGP:
				list.Clear();
				list2.Clear();
				list2.AddRange(collection2);
				break;
			case MapCategory.DRL:
				list2.Clear();
				list2.AddRange(collection);
				break;
			case MapCategory.Featured:
			{
				list.Clear();
				list2.Clear();
				List<MapData> list3 = base.app.model.storage.maps.Find(true, GameFlag.MapFeatured);
				if (list3 != null && list3.Count > 0)
				{
					list2.AddRange(list3);
				}
				break;
			}
			}
			List<string> list4 = new List<string>();
			list4.AddRange(list.ConvertAll((DRLMapTrack el) => el.guid));
			list4.AddRange(list2.ConvertAll((MapData el) => el.guid));
			list4.Shuffle();
			List<string> list5 = new List<string>();
			string item = (room.UsingCustomMap ? room.CustomMapId : room.TrackId);
			list5.Add(item);
			list4.Remove(item);
			int num = Math.Min(list4.Count, 4);
			for (int num2 = 0; num2 < num; num2++)
			{
				list5.Add(list4[num2]);
			}
			room.Outgoing.SendTrackListGenerated(list5.ToArray());
		}

		public void ChangeDroneRig(string newDroneRigData)
		{
			if (room != null)
			{
				room.Local.DroneRigData = newDroneRigData;
				room.Outgoing.SendDroneRigChanged(newDroneRigData);
			}
		}

		public void SendTrackVote(string p_guid)
		{
			if (photon.CurrentRoom != null)
			{
				photon.CurrentRoom.SendPlayerVotedTrack(p_guid);
			}
		}

		private int RandomTrackSort(DRLMapTrack a, DRLMapTrack b)
		{
			if (!(UnityEngine.Random.value < 0.5f))
			{
				return 1;
			}
			return -1;
		}

		public bool TryGetPlayer(int actorId, out NetworkActor player)
		{
			return photon.TryGetPlayer(actorId, out player);
		}

		public NetworkActor GetPlayer(int actorId)
		{
			if (photon.CurrentRoom == null)
			{
				return null;
			}
			return photon.CurrentRoom.TryGetPlayer(actorId);
		}

		public void TryKickPlayer(NetworkActor playerToKick)
		{
			photon.TryKickPlayer(playerToKick);
		}

		[ContextMenu("Test Ghosts")]
		public void TestGhosts()
		{
			DRLMap map = base.app.model.storage.library.FindByGUID<DRLMap>(room.MapId);
			DRLMapTrack track = base.app.model.storage.library.FindByGUID<DRLMapTrack>(room.TrackId);
			TryFetchGhosts(map, track);
		}

		private void GetGhostsData()
		{
			if (fetchingBots)
			{
				UnityEngine.Debug.Log("NetworkModel> GetGhostsData / Currently Fetching bots");
				return;
			}
			room.GhostsData = string.Empty;
			if (!room.AllowGhosts)
			{
				return;
			}
			room.Local.GhostsProcessing = true;
			int num = 101;
			string p_drone_guid = null;
			if (room.DroneClass == num)
			{
				p_drone_guid = base.app.model.storage.state.player.garage.currentRigData.guid;
			}
			if (room.UsingCustomMap)
			{
				TryFetchGhosts(room.CustomMapId, p_drone_guid);
				return;
			}
			DRLMap map = base.app.model.storage.library.FindByGUID<DRLMap>(room.MapId);
			DRLMapTrack dRLMapTrack = base.app.model.storage.library.FindByGUID<DRLMapTrack>(room.TrackId);
			if ((bool)dRLMapTrack)
			{
				map = dRLMapTrack.map;
			}
			TryFetchGhosts(map, dRLMapTrack, p_drone_guid);
		}

		private void TryFetchGhosts(string p_custom_map_id, string p_drone_guid = null)
		{
			if (fetchingBots)
			{
				UnityEngine.Debug.Log("NetworkModel> TryFetchGhosts / Currently Fetching bots");
			}
			else
			{
				if (room == null || !photon.IsMaster || !room.AllowGhosts)
				{
					return;
				}
				fetchingBots = true;
				MapData mapData = base.app.model.storage.maps.FindByGUID(p_custom_map_id);
				if (mapData != null)
				{
					DRLMap dRLMap = base.app.model.storage.library.FindByGUID<DRLMap>(mapData.mapId);
					dRLMap.data = mapData;
					TryFetchGhosts(dRLMap, null, p_drone_guid);
					return;
				}
				StartKeepAliveLoop();
				base.app.model.service.GetCommunityMaps(p_custom_map_id, delegate(DRLCommunityMapResult p_result)
				{
					if ((bool)this && room != null)
					{
						DRLCommunityMapData d = ((p_result.data.Length == 0) ? null : p_result.data[0]);
						if (d == null)
						{
							UnityEngine.Debug.LogWarning("NetworkModel> TryFetchGhosts / Failed to Load DRLCommunityMapData - guid[" + p_custom_map_id + "]");
						}
						else
						{
							new Thread((ThreadStart)delegate
							{
								MapData md = d.Convert<MapData>();
								if (md == null)
								{
									UnityEngine.Debug.LogWarning("NetworkModel> TryFetchGhosts / Failed to Parse MapData - guid[" + p_custom_map_id + "]");
								}
								else
								{
									Activity.RunOnce(delegate
									{
										StopKeepAliveLoop(1f);
										DRLMap dRLMap2 = base.app.model.storage.library.FindByGUID<DRLMap>(md.mapId);
										dRLMap2.data = md;
										TryFetchGhosts(dRLMap2, null, p_drone_guid);
									}, 1f / 12f);
								}
							}).Start();
						}
					}
				});
			}
		}

		private void TryFetchGhosts(DRLMap map, DRLMapTrack track, string p_drone_guid = null)
		{
			if (room == null || !photon.IsMaster || !room.AllowGhosts)
			{
				return;
			}
			List<string> p_exclude = room.Racers.ConvertAll((NetworkActor el) => el.PlayerId);
			int num = room.DroneClass;
			if (num > 7)
			{
				num = -1;
			}
			Notify("network.ghosts.update-ui");
			base.app.model.service.GetMultiplayerBots(map, track, 6, num, p_custom_physics: false, delegate(DRLLeaderboardData[] botsResult)
			{
				Notify("network.ghosts.refreshed-ui");
				if (!(this == null) && room != null && botsResult != null)
				{
					UnityEngine.Debug.Log($"[NetworkModel.TryUpdateGhosts] - Found {botsResult.Length} new ghosts");
					room.Ghosts.Clear();
					foreach (DRLLeaderboardData dRLLeaderboardData in botsResult)
					{
						if (room.Ghosts.Count >= room.MaxRacers - room.RacersCount)
						{
							break;
						}
						NetworkGhost item = new NetworkGhost
						{
							PlayerBackendId = dRLLeaderboardData.playerId.ToString(),
							ProfileName = dRLLeaderboardData.profileName + " (BOT)",
							ProfilePhoto = dRLLeaderboardData.profileThumbURL,
							ProfileColorHex = dRLLeaderboardData.profileColorHex,
							ReplayURL = dRLLeaderboardData.replayURL,
							DronePhoto = dRLLeaderboardData.droneThumb,
							DroneRig = dRLLeaderboardData.droneRig
						};
						room.Ghosts.Add(item);
					}
					UpdateGhostsCount();
					fetchingBots = false;
				}
			}, -1, p_exclude, p_drone_guid);
			this.TimerRunOnce(delegate
			{
				fetchingBots = false;
			}, 5f);
		}

		private void UpdateGhostsCount()
		{
			room.UpdateGhostsCount();
		}

		[ContextMenu("Debug Ghost Loading")]
		public void DebugLoadGhosts()
		{
			if (room == null)
			{
				UnityEngine.Debug.LogWarning("NetworkModel> DebugLoadGhosts / Not in a room!");
				return;
			}
			if (room.Ghosts == null)
			{
				UnityEngine.Debug.LogWarning("NetworkModel> DebugLoadGhosts / Invalid Ghost List!");
				return;
			}
			if (room.Ghosts.Count <= 0)
			{
				UnityEngine.Debug.LogWarning("NetworkModel> DebugLoadGhosts / No Ghosts Available!");
				return;
			}
			List<string> replay_urls = new List<string>();
			for (int i = 0; i < room.Ghosts.Count; i++)
			{
				string replayURL = room.Ghosts[i].ReplayURL;
				if (!string.IsNullOrEmpty(replayURL))
				{
					replay_urls.Add(replayURL);
				}
			}
			if (replay_urls.Count <= 0)
			{
				UnityEngine.Debug.LogWarning("NetworkModel> DebugLoadGhosts / No Replays Available!");
				return;
			}
			UnityEngine.Debug.Log("NetworkModel> DebugLoadGhost / URLs\n" + string.Join("\n", replay_urls));
			int idx = 0;
			GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
			Action on_complete = null;
			on_complete = delegate
			{
				if (idx >= replay_urls.Count)
				{
					UnityEngine.Debug.Log("============= FINISHED =============");
					GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.Default;
					Activity.RunOnce(delegate
					{
						DRLApp.LogMemStats("After Finished 5s", p_show_delta: true);
					}, 5f);
				}
				else
				{
					DebugGhostLoad(idx, replay_urls[idx], on_complete);
					idx++;
				}
			};
			on_complete();
		}

		protected void DebugGhostLoad(int p_index, string p_url, Action p_oncomplete)
		{
			DRLApp.ClearMemStats();
			DRLApp.LogMemStats($"Load Start {p_index}", p_show_delta: false);
			DateTime t0;
			TimeSpan dt;
			Web.Get(p_url, delegate(byte[] d, float p, WebAsyncRequest req)
			{
				if (!(p < 1f))
				{
					if (d == null)
					{
						UnityEngine.Debug.LogWarning("NetworkModel> DebugLoadGhosts / Invalid Replay Data!");
					}
					else
					{
						float num = (float)d.Length / 1024f / 1024f;
						UnityEngine.Debug.Log(string.Format("NetworkModel> DebugLoadGhost / Replay {0} Loaded - [{1}mb]", p_index, num.ToString("0.00")));
						DRLApp.LogMemStats("Load Complete", p_show_delta: true);
						req.loader.Dispose();
						DRLApp.LogMemStats("Dispose", p_show_delta: true);
						Thread thread = new Thread((ThreadStart)delegate
						{
							BlackboxRecord blackboxRecord = Serialize.FromBytes<BlackboxRecord>(d, p_unsafe: true);
							blackboxRecord.Decompress();
							blackboxRecord.Prune();
							Activity.RunOnce(delegate
							{
								DRLApp.LogMemStats("Deserialize/Decompress", p_show_delta: true);
								Activity.RunOnce(delegate
								{
									t0 = DateTime.Now;
									GC.Collect(2);
									dt = DateTime.Now - t0;
									DRLApp.LogMemStats($"GC LOH {dt.TotalMilliseconds}ms", p_show_delta: true);
									if (p_oncomplete != null)
									{
										p_oncomplete();
									}
								}, 1f);
							});
						});
						thread.Priority = System.Threading.ThreadPriority.Highest;
						thread.Start();
					}
				}
			});
		}

		private void TryLoadGhosts()
		{
			if (room == null)
			{
				UnityEngine.Debug.LogWarning("NetworkModel> TryLoadGhosts / Room is Null");
				return;
			}
			List<string> list = new List<string>();
			for (int i = 0; i < room.Ghosts.Count; i++)
			{
				string replayURL = room.Ghosts[i].ReplayURL;
				if (!string.IsNullOrEmpty(replayURL))
				{
					list.Add(replayURL);
				}
			}
			StartKeepAliveLoop();
			ServiceModel sm = base.app.model.service;
			sm.opponent.Cancel();
			base.app.model?.service?.opponent?.ForceResetLoadedReplays();
			RunOnce(2f, delegate
			{
				GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
			});
			sm.opponent.Load(list.ToArray(), list.Count, delegate
			{
				if (room != null && base.validContext)
				{
					switch (sm.opponent.status)
					{
					case OpponentModel.Status.Error:
						base.app.model.service.opponent.Cancel();
						Notify("network.ghosts.status", sm.opponent.status, sm.opponent.progress);
						room.Local.GhostsProcessed = true;
						break;
					case OpponentModel.Status.NoResults:
						base.app.model.service.opponent.Cancel();
						Notify("network.ghosts.status", sm.opponent.status, sm.opponent.progress);
						room.Local.GhostsProcessed = true;
						break;
					case OpponentModel.Status.Progress:
						Notify("network.ghosts.status", sm.opponent.status, sm.opponent.progress);
						break;
					case OpponentModel.Status.Complete:
						UnityEngine.Debug.Log($"NetworkModel>TryLoadGhosts > Ghosts loaded with progress[{sm.opponent.progress}]");
						Notify("network.ghosts.status", sm.opponent.status, sm.opponent.progress);
						room.Local.GhostsProcessed = true;
						break;
					case OpponentModel.Status.ManifestSuccess:
						base.app.view.audio.PlayUIGenericSuccess();
						break;
					case OpponentModel.Status.ByPass:
						break;
					}
				}
			});
		}

		public void FlushReplayRequests()
		{
			string text = "NetworkModel> FlushReplayRequests\n";
			for (int i = 0; i < replayLoadQueue.Count; i++)
			{
				string url = replayLoadQueue[i].url;
				int sender_id = replayLoadQueue[i].senderId;
				text = text + "player[" + sender_id + "] url[" + url + "]\n";
				if (sender_id == room.Local.ID)
				{
					Notify("network.race.replay.incoming", sender_id, null);
					continue;
				}
				Web.Get(url, delegate(byte[] data, float progress, WebAsyncRequest request)
				{
					if (!(progress < 1f))
					{
						Notify("network.race.replay.incoming", sender_id, data);
					}
				});
			}
			replayLoadQueue.Clear();
			UnityEngine.Debug.Log(text);
		}

		[ContextMenu("Send Game Invite")]
		public void SendGameInvite()
		{
			if (room == null)
			{
				UnityEngine.Debug.LogWarning("NetworkModel> SendGameInvite - Can't send GameInvite if not inside an MP Room");
				return;
			}
			bool crossplay = base.app.model.storage.state.player.settings.game.crossplay;
			Notify("network.room.invite", region, room.Id, room.IsQuickMatch, room.RoomTitle, room.GameMode == NetworkRoom.GameType.Race, crossplay);
		}

		protected void OnPhotonServiceStateChanged(PhotonService.ServiceState p_state)
		{
			UnityEngine.Debug.Log("NetworkModel> OnPhotonServiceStateChanged: " + p_state.ToString() + "\n" + StackTraceUtility.ExtractStackTrace());
			switch (p_state)
			{
			case PhotonService.ServiceState.Disconnected:
				Notify("network.disconnect", photon.DisconnectionReason);
				Notify("multiplayer.lan.disconnected");
				break;
			case PhotonService.ServiceState.InLobby:
				Notify("network.connection@complete");
				if (IsConnectedToLAN)
				{
					string text = photon.ServerIP.Split(':')[0];
					Notify("multiplayer.lan.connected", text);
				}
				break;
			case PhotonService.ServiceState.InProgress:
				Notify("network.connection@start");
				break;
			case PhotonService.ServiceState.InMaster:
				break;
			}
		}

		protected void OnPhotonServiceNetworkEvent(PhotonService.EventType p_event, object p_content)
		{
			switch (p_event)
			{
			case PhotonService.EventType.OnJoinLobby:
				isLAN.Value = IsConnectedToLAN;
				regionCodeName.Value = regionName;
				Notify("network.lobby@enter");
				break;
			case PhotonService.EventType.OnLeftLobby:
				isLAN.Value = IsConnectedToLAN;
				regionCodeName.Value = regionName;
				Notify("network.lobby@exit");
				break;
			case PhotonService.EventType.OnLobbyUpdated:
				Notify("network.lobby@update");
				Notify("network.lobby.room-list", photon.CurrentLobby.Rooms);
				pingTime.Value = photon.CurrentLobby.PingTime;
				pingQuality.Value = photon.CurrentLobby.PingQuality;
				Notify("network.ping.update", photon.CurrentLobby.PingTime, photon.CurrentLobby.PingQuality);
				break;
			case PhotonService.EventType.OnJoinRoom:
			{
				PlayerStateModel player = base.app.model.storage.state.player;
				NetworkActor local = photon.CurrentRoom.Local;
				local.Set(player);
				UpdateAutoColor();
				roomData.Value.Enter(photon.CurrentRoom);
				if (!room.IsTournamentMatch)
				{
					tournamentMatchData = null;
				}
				room.SetupLocalPlayer();
				Notify("network.player.room@enter", local);
				Notify("network.room@enter", photon.CurrentRoom.Id);
				break;
			}
			case PhotonService.EventType.OnRoomUpdated:
			{
				if (room == null)
				{
					break;
				}
				roomData.Value.Update(photon.CurrentRoom);
				ExitGames.Client.Photon.Hashtable hashtable = (ExitGames.Client.Photon.Hashtable)p_content;
				Notify(1f / 60f, "network.room.update", hashtable);
				if (room.State != NetworkRoom.StateCode.GameRunning)
				{
					int num3 = photon.PingTime;
					float pingQualityLevel = PhotonUtils.GetPingQualityLevel(num3);
					Notify("network.ping.update", num3, pingQualityLevel);
				}
				if (hashtable == null || base.app == null || base.app.model == null)
				{
					break;
				}
				if (base.app.model.game != null && hashtable.ContainsKey("av") && room.AllowMapVoting)
				{
					UpdateVotedTrack();
				}
				if (hashtable.ContainsKey("gd"))
				{
					room.TryUpdateLocalGhosts();
					Notify(0.1f, "network.ghosts.count", room.Ghosts);
				}
				if ((hashtable.ContainsKey("t") && !string.IsNullOrEmpty(room.TrackId)) || (hashtable.ContainsKey("cmi") && !string.IsNullOrEmpty(room.CustomMapId)) || hashtable.ContainsKey("rc") || hashtable.ContainsKey("mr") || hashtable.ContainsKey("d") || hashtable.ContainsKey("ag") || hashtable.ContainsKey("dp"))
				{
					bool flag = true;
					if (hashtable.ContainsKey("t") && room.UsingCustomMap)
					{
						flag = false;
					}
					if (flag)
					{
						GetGhostsData();
					}
				}
				if (hashtable.ContainsKey("auc") || hashtable.ContainsKey("rc"))
				{
					UpdateAutoColor();
				}
				bool num4 = hashtable.ContainsKey("crc");
				int racersCount = room.RacersCount;
				int activeRacersCount = room.ActiveRacersCount;
				int forfeitRacersCount = room.ForfeitRacersCount;
				int completeRacersCount = room.CompleteRacersCount;
				Mathf.Max(0, racersCount - activeRacersCount - forfeitRacersCount);
				if (num4 && completeRacersCount == 1 && !room.IsTournamentMatch && room.State == NetworkRoom.StateCode.GameRunning)
				{
					UnityEngine.Debug.Log($"NetworkModel> OnRoomUpdated / First Racer finished\nTotal Racers: {racersCount}\nSuccess Racers: {activeRacersCount}\nForfeit Racers: {forfeitRacersCount}");
					Notify("network.room.first-racer-finshed");
				}
				break;
			}
			case PhotonService.EventType.OnJoinRoomFailed:
			{
				tournamentMatchData = null;
				string text = "";
				if (p_content != null)
				{
					IEnumerator enumerator = ((p_content == null) ? null : (p_content.GetType().IsArray ? ((IEnumerator)p_content) : null));
					text = ((p_content is string) ? p_content.ToString().ToLower() : ((enumerator == null) ? "" : string.Join(" ", enumerator).ToLower()));
				}
				UnityEngine.Debug.Log("NetworkModel> OnJoinRoomFailed / " + text);
				Notify("network.room-enter@error", text);
				break;
			}
			case PhotonService.EventType.OnCreateRoomFailed:
				tournamentMatchData = null;
				Notify("network.room-create@error");
				break;
			case PhotonService.EventType.OnLeftRoom:
				if (roomData != null && roomData.Value != null)
				{
					roomData.Value.Left();
				}
				tournamentMatchData = null;
				if ((bool)photon)
				{
					Notify("network.room@exit", photon.DisconnectionReason);
				}
				break;
			case PhotonService.EventType.OnPlayerJoin:
			{
				NetworkActor networkActor2 = p_content as NetworkActor;
				Notify("network.player.room@enter", networkActor2);
				break;
			}
			case PhotonService.EventType.OnPlayerUpdated:
			{
				NetworkActor networkActor = p_content as NetworkActor;
				Notify("network.player@update", networkActor);
				break;
			}
			case PhotonService.EventType.OnPlayerLeft:
			{
				int num2 = (int)p_content;
				Notify("network.player.room@exit", num2);
				if (room != null && room.Racers.Count == 0)
				{
					Notify("network.room.no.racers");
				}
				break;
			}
			case PhotonService.EventType.OnMasterChanged:
			{
				int num = (int)p_content;
				Notify("network.room.master.changed", num);
				break;
			}
			case PhotonService.EventType.OnJoinLobbyFailed:
				if (base.validContext)
				{
					Notify("network.lobby.join-failed", p_content);
				}
				break;
			case PhotonService.EventType.OnRoomFull:
				if (base.validContext)
				{
					Notify("network.room.full", p_content);
				}
				break;
			case PhotonService.EventType.OnRoomNotActive:
				if (base.validContext)
				{
					Notify("network.room.not-active", p_content);
				}
				break;
			}
		}

		protected void OnPhotonRoomEvent(NetworkRoom.GameEvent eventData)
		{
			if (room?.Local == null)
			{
				return;
			}
			switch (eventData.EventCode)
			{
			case NetworkRoom.GameEventCode.OnMatchmaking:
				UpdateAutoColor();
				break;
			case NetworkRoom.GameEventCode.OnMatchLocked:
				Notify("network.room@lock");
				if (room.IsUsingGhosts)
				{
					TryLoadGhosts();
				}
				break;
			case NetworkRoom.GameEventCode.OnLoadLevel:
			{
				NetworkRoom.LoadGameData loadGameData = (NetworkRoom.LoadGameData)eventData.Content;
				if (loadGameData != null)
				{
					Notify("network.room.load-game", loadGameData);
				}
				break;
			}
			case NetworkRoom.GameEventCode.OnPlayerMarkedReady:
			{
				NetworkActor networkActor5 = (NetworkActor)eventData.Content;
				if (networkActor5 != null)
				{
					Notify("network.player.marked.ready", networkActor5);
				}
				break;
			}
			case NetworkRoom.GameEventCode.OnGameWarmup:
				if (replayLoadQueue != null)
				{
					replayLoadQueue.Clear();
				}
				else
				{
					replayLoadQueue = new List<NetworkReplayRequest>();
				}
				Notify("network.player.all.ready");
				break;
			case NetworkRoom.GameEventCode.OnGameWarmupStep:
			{
				float num2 = (float)eventData.Content;
				Notify("network.race.count", 3f - num2, 3f);
				break;
			}
			case NetworkRoom.GameEventCode.OnGameStart:
				Notify("network.race.count@complete");
				PhotonNetwork.sendRate = 12;
				PhotonNetwork.sendRateOnSerialize = 12;
				break;
			case NetworkRoom.GameEventCode.OnGameEnd:
			{
				NetworkRoom.GameFinishedData gameFinishedData = (NetworkRoom.GameFinishedData)eventData.Content;
				Notify("network.race.end", gameFinishedData);
				PhotonNetwork.sendRate = 5;
				PhotonNetwork.sendRateOnSerialize = 5;
				break;
			}
			case NetworkRoom.GameEventCode.OnChatMessage:
			{
				NetworkRoomChat.Message message = (NetworkRoomChat.Message)eventData.Content;
				Notify("network.room.chat.incoming", message);
				if (room.Chat.HasUnreadMessages)
				{
					Notify("social.badges.dirty", "room-chat");
				}
				break;
			}
			case NetworkRoom.GameEventCode.OnLoadPlayer:
				UnityEngine.Debug.Log("NetworkModel > OnLoadPlayer");
				Notify("network.instantiate.local");
				break;
			case NetworkRoom.GameEventCode.OnPlayerSpawned:
			{
				UnityEngine.Debug.Log("NetworkModel > OnPlayerSpawned");
				NetworkActor networkActor4 = (NetworkActor)eventData.Content;
				if (networkActor4 != null && !networkActor4.IsLocal)
				{
					Notify("network.instantiate.remote", networkActor4);
				}
				break;
			}
			case NetworkRoom.GameEventCode.OnSwitchedToRacer:
			{
				NetworkActor networkActor2 = (NetworkActor)eventData.Content;
				if (networkActor2 != null)
				{
					UnityEngine.Debug.Log("NetworkModel > OnSwitchedToRacer playerId[" + networkActor2.ID + "]");
					Notify("network.player.racer", networkActor2);
				}
				break;
			}
			case NetworkRoom.GameEventCode.OnSwitchedToSpectator:
			{
				NetworkActor networkActor = (NetworkActor)eventData.Content;
				if (networkActor != null)
				{
					UnityEngine.Debug.Log("NetworkModel > OnSwitchedToSpectator playerId[" + networkActor.ID + "]");
					if (eventData.Notify)
					{
						Notify("network.player.spectator", networkActor);
					}
				}
				break;
			}
			case NetworkRoom.GameEventCode.OnGateEvent:
			{
				int num = (int)eventData.Content;
				Notify("network.race.gate@hit", eventData.PlayerId, num);
				break;
			}
			case NetworkRoom.GameEventCode.OnPlayerCompletedGame:
				Notify("network.player.completed.race", eventData.PlayerId);
				break;
			case NetworkRoom.GameEventCode.OnPlayerForfeitGame:
				Notify("network.player.forfeit.race", eventData.PlayerId);
				break;
			case NetworkRoom.GameEventCode.OnPlayerCrashed:
			{
				NetworkRoom.DroneState droneState = (NetworkRoom.DroneState)eventData.Content;
				Notify("network.player.crashed", droneState);
				break;
			}
			case NetworkRoom.GameEventCode.OnPlayerDamage:
			{
				NetworkRoom.DamageData damageData = (NetworkRoom.DamageData)eventData.Content;
				Notify("network.player.damage", damageData);
				break;
			}
			case NetworkRoom.GameEventCode.OnPlayerRecovered:
				if (room != null && room.Local.ID != eventData.PlayerId)
				{
					Notify("network.player.recovered", eventData.PlayerId);
				}
				break;
			case NetworkRoom.GameEventCode.OnReplayDataReady:
			{
				string text4 = (string)eventData.Content;
				int playerId2 = eventData.PlayerId;
				if (string.IsNullOrEmpty(text4))
				{
					UnityEngine.Debug.LogWarning("NetworkModel> Failed to load replay for [" + playerId2 + "]");
					break;
				}
				NetworkReplayRequest networkReplayRequest = new NetworkReplayRequest();
				networkReplayRequest.senderId = playerId2;
				networkReplayRequest.url = text4;
				replayLoadQueue.Add(networkReplayRequest);
				if (base.app.model.tournament.isTournamentActive && room != null)
				{
					Notify("tournament.replay.incoming", room.MatchId, room.HeatIdx, playerId2, text4);
				}
				if (replayLoadQueue.Count >= room.RacersCount)
				{
					FlushReplayRequests();
				}
				break;
			}
			case NetworkRoom.GameEventCode.OnTrackListGenerated:
			{
				List<string> list = (List<string>)eventData.Content;
				Notify("network.room.vote-track.generated", list);
				break;
			}
			case NetworkRoom.GameEventCode.OnPlayerVotedTrack:
			{
				NetworkActor networkActor3 = (NetworkActor)eventData.Content;
				UpdateVotedTrack();
				Notify("network.player.voted.track", networkActor3);
				break;
			}
			case NetworkRoom.GameEventCode.OnPlayerKicked:
				Notify("network.player-kicked");
				break;
			case NetworkRoom.GameEventCode.OnDroneRigChanged:
			{
				string text3 = (string)eventData.Content;
				int playerId = eventData.PlayerId;
				Notify("network.drone.changed", playerId, text3);
				break;
			}
			case NetworkRoom.GameEventCode.OnPullUsersIn:
			{
				string text2 = (string)eventData.Content;
				if (base.app.inTournament)
				{
					Notify("tournament.action.match-starting", text2);
				}
				break;
			}
			case NetworkRoom.GameEventCode.OnOrderUpdate:
				UnityEngine.Debug.Log("NetworkModel > OnOrderUpdate");
				Notify("network.player.order@update");
				this.TimerRunOnce(delegate
				{
					if (base.validContext && room != null && room.State == NetworkRoom.StateCode.MatchMaking)
					{
						room.Outgoing.SendMatchLocked();
					}
				}, 5f);
				break;
			case NetworkRoom.GameEventCode.OnRaceReady:
				if (base.validContext && room != null && room.State == NetworkRoom.StateCode.MatchMaking && room.IsMaster)
				{
					string text = (string)eventData.Content;
					if (!string.IsNullOrEmpty(text))
					{
						UnityEngine.Debug.Log("NetworkModel > OnRaceReady");
						Notify("tournament.action.start-match", text);
					}
				}
				break;
			case NetworkRoom.GameEventCode.OnPlayerLoadedLevel:
			case NetworkRoom.GameEventCode.OnPlayerReady:
			case NetworkRoom.GameEventCode.OnWebHookTest:
			case NetworkRoom.GameEventCode.OnPlayerSkippedIntro:
			case NetworkRoom.GameEventCode.OnPlayerCountdownReady:
				break;
			}
		}

		protected void OnLANServerChanged(PhotonLANServerDeprecated.ServerState newState)
		{
			UnityEngine.Debug.Log($"NetworkModel> OnLANServerChanged / state[{newState}]");
			switch (newState)
			{
			case PhotonLANServerDeprecated.ServerState.Offline:
				Notify("network.LAN.offline");
				break;
			case PhotonLANServerDeprecated.ServerState.Starting:
				Notify("network.LAN.starting");
				break;
			case PhotonLANServerDeprecated.ServerState.Online:
				Notify("network.LAN.online");
				break;
			case PhotonLANServerDeprecated.ServerState.Stopping:
				Notify("network.LAN.stopping");
				break;
			}
		}

		public void OnPersistency()
		{
			base.app.model.network = this;
		}

		protected void OnDestroy()
		{
			m_photon_keepalive_active = false;
		}

		private void UpdateVotedTrack()
		{
			if (room == null || !room.IsMaster)
			{
				return;
			}
			string mostVotedTrack = room.GetMostVotedTrack();
			if (mostVotedTrack == room.TrackId || mostVotedTrack == room.CustomMapId || string.IsNullOrEmpty(mostVotedTrack))
			{
				return;
			}
			DRLMapTrack dRLMapTrack = base.app.model.storage.library.FindByGUID<DRLMapTrack>(mostVotedTrack);
			if (dRLMapTrack != null)
			{
				room.CustomMapId = string.Empty;
				room.CustomMapName = string.Empty;
				room.MapId = dRLMapTrack.map.guid;
				room.TrackId = dRLMapTrack.guid;
				return;
			}
			MapData mapData = base.app.model.storage.maps.FindByGUID(mostVotedTrack);
			if (mapData != null)
			{
				room.MapId = mapData.mapId;
				room.TrackId = string.Empty;
				room.CustomMapId = mapData.guid;
				room.CustomMapName = mapData.mapTitle;
			}
			else
			{
				UnityEngine.Debug.LogWarning("NetworkModel > OnPlayerVotedTrack - TrackGUID not found: " + mostVotedTrack);
			}
		}

		public void UpdateAutoColor()
		{
			if (room?.Local == null)
			{
				return;
			}
			if (room.AutoColor)
			{
				if (IsTournamentMatch && tournamentMatchData != null)
				{
					DRLTournamentPlayerData[] array = tournamentMatchData.players;
					foreach (DRLTournamentPlayerData dRLTournamentPlayerData in array)
					{
						if (dRLTournamentPlayerData.playerId.ToString() == room.Local.PlayerId)
						{
							UnityEngine.Debug.Log("NetworkModel>Updating user color for " + dRLTournamentPlayerData.profileName + " to color " + dRLTournamentPlayerData.profileColor.ToString() + " and secondary color: " + dRLTournamentPlayerData.profileColor2.ToString());
							room.Local.MainColor = dRLTournamentPlayerData.profileColor;
							room.Local.SecondaryColor = dRLTournamentPlayerData.profileColor2;
						}
					}
				}
				else
				{
					room.Local.MainColor = DRLColor.profileTournamentColors[Mathf.Min(room.Local.Order, DRLColor.profileTournamentColors.Length)];
				}
			}
			else
			{
				room.Local.MainColor = room.Local.ProfileColor;
			}
		}

		public void SetDamage(int p_networkId, float p_bodyDamage, float p_prop0Damage, float p_prop1Damage, float p_prop2Damage, float p_prop3Damage)
		{
			float[] array = new float[4] { p_prop0Damage, p_prop1Damage, p_prop2Damage, p_prop3Damage };
			if (m_damageData.ContainsKey(p_networkId))
			{
				Tuple<float, float[]> tuple = m_damageData[p_networkId];
				float item = tuple.Item1 + p_bodyDamage;
				if (tuple.Item2 != null && tuple.Item2.Length == 4)
				{
					array[0] += tuple.Item2[0];
					array[1] += tuple.Item2[1];
					array[2] += tuple.Item2[2];
					array[3] += tuple.Item2[3];
				}
				m_damageData[p_networkId] = new Tuple<float, float[]>(item, array);
			}
			else
			{
				m_damageData.Add(p_networkId, new Tuple<float, float[]>(p_bodyDamage, array));
			}
		}

		public Tuple<float, float[]> GetDamage(int p_networkId)
		{
			if (m_damageData == null || !m_damageData.ContainsKey(p_networkId))
			{
				return null;
			}
			return m_damageData[p_networkId];
		}

		public bool HasDamage(int p_networkId)
		{
			return GetDamage(p_networkId) != null;
		}

		public void ResetDamage()
		{
			m_damageData?.Clear();
		}
	}
}

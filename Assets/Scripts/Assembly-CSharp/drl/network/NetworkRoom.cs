using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Newtonsoft.Json;
using UnityEngine;
using drl.sim;
using thelab.core;

namespace drl.network
{
	public class NetworkRoom
	{
		public enum GameEventCode
		{
			OnMatchmaking = 0,
			OnMatchLocked = 1,
			OnLoadLevel = 2,
			OnPlayerLoadedLevel = 3,
			OnLoadPlayer = 4,
			OnPlayerReady = 5,
			OnPlayerMarkedReady = 6,
			OnGameWarmup = 7,
			OnGameWarmupStep = 8,
			OnGameStart = 9,
			OnGameEnd = 10,
			OnChatMessage = 11,
			OnPlayerSpawned = 12,
			OnWebHookTest = 13,
			OnSwitchedToRacer = 14,
			OnSwitchedToSpectator = 15,
			OnGateEvent = 16,
			OnPlayerCompletedGame = 17,
			OnReplayDataReady = 18,
			OnPlayerVotedTrack = 19,
			OnTrackListGenerated = 20,
			OnPlayerSkippedIntro = 21,
			OnPlayerKicked = 22,
			OnPlayerCrashed = 23,
			OnDroneRigChanged = 24,
			OnPullUsersIn = 25,
			OnPlayerForfeitGame = 26,
			OnOrderUpdate = 27,
			OnPlayerCountdownReady = 28,
			OnRaceReady = 29,
			OnPlayerRecovered = 30,
			OnPlayerDamage = 31,
			OnPlayerSubmittedLeaderboard = 32,
			ErrorInfo = 251
		}

		public enum GameType
		{
			Freestyle = 0,
			Race = 1,
			Tournament = 2
		}

		public enum StateCode
		{
			None = 0,
			MatchMaking = 1,
			MatchLocked = 2,
			GameLoading = 3,
			GameWarmup = 4,
			GameRunning = 5,
			GameFinished = 6
		}

		public enum MatchmakingFlow
		{
			Normal = 0,
			Quick = 1
		}

		[Serializable]
		public class GameEvent
		{
			public GameEventCode EventCode;

			public object Content;

			public int PlayerId;

			public bool Notify;
		}

		public class LoadGameData
		{
			public string Map;

			public string Track;

			public int Order;

			public string Campaign;

			public GameType GameType;

			public string CustomMapId;

			public bool IsCustomMap => !string.IsNullOrEmpty(CustomMapId);

			public Hashtable ToHashTable()
			{
				return new Hashtable
				{
					{ "map", Map },
					{ "track", Track },
					{ "order", Order },
					{ "campaign", Campaign },
					{
						"game_type",
						(int)GameType
					},
					{ "custom_map_id", CustomMapId }
				};
			}

			public void UpdateData(Hashtable data)
			{
				Map = (string)data["map"];
				Track = (string)data["track"];
				Order = (int)data["order"];
				Campaign = (string)data["campaign"];
				GameType = (GameType)(int)data["game_type"];
				CustomMapId = (string)data["custom_map_id"];
			}
		}

		public class GameFinishedData
		{
			public enum Reason
			{
				Timeout = 0,
				Completed = 1,
				NotEnoughPlayers = 2,
				Crash = 3
			}

			public Reason FinishedReason;

			public byte WinnerId;

			public float TimeElapsed;

			public Hashtable ToHashTable()
			{
				return new Hashtable
				{
					{ "reason", FinishedReason },
					{ "winner", WinnerId },
					{ "time", TimeElapsed }
				};
			}

			public void UpdateData(Hashtable data)
			{
				FinishedReason = (Reason)data["reason"];
				WinnerId = (byte)data["winner"];
				TimeElapsed = (float)data["time"];
			}
		}

		public class DroneState
		{
			public int PlayerId;

			public Vector3 Position = Vector3.zero;

			public Vector3 Rotation = Vector3.zero;

			public Vector3 Velocity = Vector3.zero;

			public float CrashEnergy;

			public Vector3 ContactNormal = Vector3.zero;

			public Vector3 ImpactVelocity = Vector3.zero;

			public Vector3 ContactPoint = Vector3.zero;

			public Hashtable ToHashTable()
			{
				return new Hashtable
				{
					{ "player_id", PlayerId },
					{ "position", Position },
					{ "rotation", Rotation },
					{ "velocity", Velocity },
					{ "energy", CrashEnergy },
					{ "contact_normal", ContactNormal },
					{ "impact_velocity", ImpactVelocity },
					{ "contact_point", ContactPoint }
				};
			}

			public void UpdateData(Hashtable data)
			{
				PlayerId = (int)data["player_id"];
				Position = (Vector3)data["position"];
				Rotation = (Vector3)data["rotation"];
				Velocity = (Vector3)data["velocity"];
				CrashEnergy = (float)data["energy"];
				ContactNormal = (Vector3)data["contact_normal"];
				ImpactVelocity = (Vector3)data["impact_velocity"];
				ContactPoint = (Vector3)data["contact_point"];
			}
		}

		public class DamageData
		{
			public int NetworkID;

			public float bodyDamage;

			public float prop0Damage;

			public float prop1Damage;

			public float prop2Damage;

			public float prop3Damage;

			public bool isCrash
			{
				get
				{
					if (bodyDamage < 1f)
					{
						return false;
					}
					if (prop0Damage < 1f)
					{
						return false;
					}
					if (prop1Damage < 1f)
					{
						return false;
					}
					if (prop2Damage < 1f)
					{
						return false;
					}
					if (prop3Damage < 1f)
					{
						return false;
					}
					return true;
				}
			}

			public Hashtable ToHashTable()
			{
				return new Hashtable
				{
					{ "network_id", NetworkID },
					{ "body_damage", bodyDamage },
					{ "prop0", prop0Damage },
					{ "prop1", prop1Damage },
					{ "prop2", prop2Damage },
					{ "prop3", prop3Damage }
				};
			}

			public void UpdateData(Hashtable data)
			{
				NetworkID = (int)data["network_id"];
				bodyDamage = (float)data["body_damage"];
				prop0Damage = (float)data["prop0"];
				prop1Damage = (float)data["prop1"];
				prop2Damage = (float)data["prop2"];
				prop3Damage = (float)data["prop3"];
			}
		}

		public bool IsLoadingLevel;

		public LoadGameData CachedLevelData;

		public Action<GameEvent> OnEvent;

		public bool MapRandomSet;

		public List<NetworkActor> lastPlayers = new List<NetworkActor>();

		public Dictionary<string, int> FixedColorsLocal = new Dictionary<string, int>();

		private readonly Hashtable cachedPropertyHashtable = new Hashtable();

		private NetworkActor[] m_racerOrderSlots;

		public Room PhotonRoom { get; protected set; }

		public IGamePlugin GamePlugin { get; private set; }

		public string Id => PhotonRoom.Name;

		public int PlayerCount => PhotonRoom.PlayerCount;

		public int MaxPlayers
		{
			get
			{
				return PhotonRoom.MaxPlayers;
			}
			set
			{
				if (IsMaster && value >= 0)
				{
					PhotonRoom.MaxPlayers = value;
				}
			}
		}

		public Dictionary<int, NetworkActor> Players { get; set; }

		public List<NetworkActor> PlayerList => new List<NetworkActor>(Players.Values);

		public List<NetworkActor> Racers => new List<NetworkActor>(Players.Values).FindAll((NetworkActor el) => !el.IsSpectator);

		public List<NetworkActor> Spectators => new List<NetworkActor>(Players.Values).FindAll((NetworkActor el) => el.IsSpectator);

		public List<NetworkGhost> Ghosts { get; private set; }

		public bool HasGhosts
		{
			get
			{
				if (Ghosts != null)
				{
					return Ghosts.Count > 0;
				}
				return false;
			}
		}

		public NetworkRoomOperations Outgoing { get; private set; }

		public NetworkRoomEvents Incoming { get; private set; }

		public NetworkRoomState StateMachine { get; private set; }

		public virtual int MatchmakingTimeout => 45;

		public bool HasDelayedStart => GetCustom<bool>("ds");

		public virtual int GameLoadingTimeout
		{
			get
			{
				if (HasDelayedStart)
				{
					return int.MaxValue;
				}
				if (!HasGhosts)
				{
					return 300;
				}
				return 600;
			}
		}

		public virtual float WarmupTimeout => 3f;

		public PhotonService Service { get; private set; }

		public NetworkActor Local => TryGetPlayer((PhotonNetwork.player == null) ? (-1) : PhotonNetwork.player.ID);

		public NetworkActor Master => TryGetPlayer((PhotonNetwork.masterClient == null) ? (-1) : PhotonNetwork.masterClient.ID);

		public bool IsMaster
		{
			get
			{
				if (Local != null && Master != null)
				{
					return Local.ID == Master.ID;
				}
				return false;
			}
		}

		public bool IsSpectator
		{
			get
			{
				if (Local != null)
				{
					return Local.IsSpectator;
				}
				return true;
			}
		}

		public StateCode State
		{
			get
			{
				return StateMachine.State;
			}
			set
			{
				OnStateChanged(value);
			}
		}

		public List<string> VoteTrackList { get; set; }

		public Dictionary<string, int> VoteTrackTable
		{
			get
			{
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				if (VoteTrackList == null)
				{
					return dictionary;
				}
				List<string> list = Racers.ConvertAll((NetworkActor el) => el.VotedTrackGUID);
				foreach (string voteTrack in VoteTrackList)
				{
					if (!dictionary.ContainsKey(voteTrack))
					{
						dictionary.Add(voteTrack, 0);
					}
				}
				foreach (string item in list)
				{
					if (!string.IsNullOrEmpty(item) && dictionary.ContainsKey(item))
					{
						dictionary[item]++;
					}
				}
				return dictionary;
			}
		}

		public float ElapsedTime
		{
			get
			{
				float num = (float)(PhotonNetwork.time - InGameTimeStarted);
				if (!(num > 0f))
				{
					return 0f;
				}
				return num;
			}
		}

		public float ServerTimeElapsed
		{
			get
			{
				float num = (float)(PhotonNetwork.time - ServerTimeStarted);
				if (!(num > 0f))
				{
					return 0f;
				}
				return num;
			}
		}

		public DateTime ServerTime
		{
			get
			{
				if (ServerTimeStarted == 0.0)
				{
					ServerTimeStarted = PhotonNetwork.time;
				}
				if (ServerTimeUTCTicks == 0L)
				{
					ServerTimeUTCTicks = DateTime.UtcNow.Ticks;
				}
				return new DateTime(ServerTimeUTCTicks).AddSeconds(ServerTimeElapsed);
			}
		}

		public float TimeLeft => (float)(TimeLimit - (double)ElapsedTime);

		public float WarmupTimeLeft
		{
			get
			{
				float num = (float)(PhotonNetwork.time - ServerWarmupStarted);
				num = ((num > 0f) ? num : 0f);
				return WarmupTimeout - num;
			}
		}

		public bool UsingCustomMap => !string.IsNullOrEmpty(CustomMapId);

		public bool IsQuickMatch => MatchmakingType == MatchmakingFlow.Quick;

		public bool IsTournamentMatch => GameMode == GameType.Tournament;

		public DateTime StartTimeUTC => new DateTime(StartTimeUTCTicks);

		public bool LastHeat => HeatIdx == MaxHeats;

		public bool HeatAllowed => HeatIdx < MaxHeats;

		public bool IsRoundComplete
		{
			get
			{
				if (ServerState == StateCode.GameFinished)
				{
					return LastHeat;
				}
				return false;
			}
		}

		public bool AllowMapVoting => MapVotingCategory != MapCategory.None;

		public bool MapRandom => MapVotingCategory == MapCategory.Random;

		public bool IsUsingGhosts
		{
			get
			{
				if (AllowGhosts)
				{
					return Ghosts.Count > 0;
				}
				return false;
			}
		}

		public Dictionary<int, NetworkRacer> SceneRacers { get; private set; }

		public Dictionary<int, int> CachedRemoteRacers { get; private set; }

		public NetworkRoomChat Chat { get; private set; }

		public bool ArmAndTurtle
		{
			get
			{
				return GetCustom<bool>("aat");
			}
			set
			{
				SetCustom<bool>("aat", value);
			}
		}

		public int ActiveRacersCount
		{
			get
			{
				return GetCustom<int>("arc");
			}
			set
			{
				SetCustom<int>("arc", value);
			}
		}

		public int ForfeitRacersCount
		{
			get
			{
				return GetCustom<int>("frc");
			}
			set
			{
				SetCustom<int>("frc", value);
			}
		}

		public int CompleteRacersCount
		{
			get
			{
				return GetCustom<int>("crc");
			}
			set
			{
				SetCustom<int>("crc", value);
			}
		}

		public bool AllowGhosts
		{
			get
			{
				return GetCustom<bool>("ag");
			}
			set
			{
				SetCustom<bool>("ag", value);
			}
		}

		public bool DRLPilotMode
		{
			get
			{
				return GetCustom<bool>("dp");
			}
			set
			{
				SetCustom<bool>("dp", value);
			}
		}

		public bool AutoColor
		{
			get
			{
				return GetCustom<bool>("auc");
			}
			set
			{
				SetCustom<bool>("auc", value);
			}
		}

		public bool CanRace
		{
			get
			{
				return GetCustom<bool>("cr");
			}
			set
			{
				SetCustom<bool>("cr", value);
			}
		}

		public bool CanSpectate
		{
			get
			{
				return GetCustom<bool>("cs");
			}
			set
			{
				SetCustom<bool>("cs", value);
			}
		}

		public string CustomMapId
		{
			get
			{
				return GetCustom<string>("cmi") ?? "";
			}
			set
			{
				SetCustom<string>("cmi", value);
			}
		}

		public string CustomMapName
		{
			get
			{
				return GetCustom<string>("cmn") ?? "";
			}
			set
			{
				SetCustom<string>("cmn", value);
			}
		}

		public int DroneClass
		{
			get
			{
				return GetCustom<int>("d");
			}
			set
			{
				SetCustom<int>("d", value);
			}
		}

		public string SelectedDrone
		{
			get
			{
				return GetCustom<string>("sd") ?? "";
			}
			set
			{
				SetCustom<string>("sd", value);
			}
		}

		public string FixedColors
		{
			get
			{
				return GetCustom<string>("fc") ?? "";
			}
			set
			{
				SetCustom<string>("fc", value);
			}
		}

		public GameType GameMode
		{
			get
			{
				return GetCustom<GameType>("g");
			}
			set
			{
				SetCustom<GameType>("g", value);
			}
		}

		public string GhostsData
		{
			get
			{
				return GetCustom<string>("gd") ?? "";
			}
			set
			{
				SetCustom<string>("gd", value);
			}
		}

		public int HeatIdx
		{
			get
			{
				return GetCustom<int>("hid");
			}
			set
			{
				SetCustom<int>("hid", value);
			}
		}

		public bool IsCustomPhysics
		{
			get
			{
				return GetCustom<bool>("icf");
			}
			set
			{
				SetCustom<bool>("icf", value);
			}
		}

		public double InGameTimeStarted
		{
			get
			{
				return GetCustom<double>("igs");
			}
			set
			{
				SetCustom<double>("igs", value);
			}
		}

		public bool IsPrivate
		{
			get
			{
				return GetCustom<bool>("ip");
			}
			set
			{
				SetCustom<bool>("ip", value);
			}
		}

		public int LobbyCountdown
		{
			get
			{
				return GetCustom<int>("lc");
			}
			set
			{
				SetCustom<int>("lc", value);
			}
		}

		public bool LobbyCountdownAllowed
		{
			get
			{
				return GetCustom<bool>("lca");
			}
			set
			{
				SetCustom<bool>("lca", value);
			}
		}

		public string MapId
		{
			get
			{
				return GetCustom<string>("m") ?? "";
			}
			set
			{
				SetCustom<string>("m", value);
			}
		}

		public int MatchCount
		{
			get
			{
				return GetCustom<int>("mc");
			}
			set
			{
				SetCustom<int>("mc", value);
			}
		}

		public MatchmakingFlow MatchmakingType
		{
			get
			{
				return GetCustom<MatchmakingFlow>("mt");
			}
			set
			{
				SetCustom<MatchmakingFlow>("mt", value);
			}
		}

		public Color MasterProfileColour
		{
			get
			{
				return Colorf.RGBToColor((uint)GetCustom<int>("mpc"));
			}
			set
			{
				SetCustom<int>("mpc", (int)Colorf.ColorToRGB(value));
			}
		}

		public string MasterProfilePhoto
		{
			get
			{
				return GetCustom<string>("mph") ?? "";
			}
			set
			{
				SetCustom<string>("mph", value);
			}
		}

		public string MasterId
		{
			get
			{
				return GetCustom<string>("mi") ?? "";
			}
			set
			{
				SetCustom<string>("mi", value);
			}
		}

		public string MasterBlockList
		{
			get
			{
				return GetCustom<string>("mblist") ?? "";
			}
			set
			{
				SetCustom<string>("mblist", value);
			}
		}

		public string MatchId
		{
			get
			{
				return GetCustom<string>("mid") ?? "";
			}
			set
			{
				SetCustom<string>("mid", value);
			}
		}

		public string RaceId
		{
			get
			{
				return GetCustom<string>("rid") ?? "";
			}
			set
			{
				SetCustom<string>("rid", value);
			}
		}

		public int MaxHeats
		{
			get
			{
				return GetCustom<int>("mh");
			}
			set
			{
				SetCustom<int>("mh", value);
			}
		}

		public int MaxRacers
		{
			get
			{
				return GetCustom<int>("mr");
			}
			set
			{
				SetCustom<int>("mr", value);
			}
		}

		public int MaxSpectators
		{
			get
			{
				return GetCustom<int>("ms");
			}
			set
			{
				SetCustom<int>("ms", value);
			}
		}

		public int MinRequiredRacers
		{
			get
			{
				return GetCustom<int>("mps");
			}
			set
			{
				SetCustom<int>("mps", value);
			}
		}

		public string Password
		{
			get
			{
				return GetCustom<string>("p") ?? "";
			}
			set
			{
				SetCustom<string>("p", value);
			}
		}

		public string PlayersIdsData
		{
			get
			{
				return GetCustom<string>("pi") ?? "";
			}
			set
			{
				SetCustom<string>("pi", value);
			}
		}

		public float Progress
		{
			get
			{
				return GetCustom<float>("pr");
			}
			set
			{
				SetCustom<float>("pr", value);
			}
		}

		public int RacersCount
		{
			get
			{
				return GetCustom<int>("rc");
			}
			set
			{
				SetCustom<int>("rc", value);
			}
		}

		public string RoomTitle
		{
			get
			{
				return GetCustom<string>("rt") ?? "";
			}
			set
			{
				SetCustom<string>("rt", value);
			}
		}

		public long StartTimeUTCTicks
		{
			get
			{
				return GetCustom<long>("st");
			}
			set
			{
				SetCustom<long>("st", value);
			}
		}

		public int SpectatorsCount
		{
			get
			{
				return GetCustom<int>("sc");
			}
			set
			{
				SetCustom<int>("sc", value);
			}
		}

		public StateCode ServerState
		{
			get
			{
				return GetCustom<StateCode>("s");
			}
			set
			{
				SetCustom<StateCode>("s", value);
			}
		}

		public double ServerWarmupStarted
		{
			get
			{
				return GetCustom<double>("sws");
			}
			set
			{
				SetCustom<double>("sws", value);
			}
		}

		public double ServerTimeStarted
		{
			get
			{
				return GetCustom<double>("sts");
			}
			set
			{
				SetCustom<double>("sts", value);
			}
		}

		public long ServerTimeUTCTicks
		{
			get
			{
				return GetCustom<long>("stu");
			}
			set
			{
				SetCustom<long>("stu", value);
				ServerTimeStarted = PhotonNetwork.time;
			}
		}

		public double TimeLimit
		{
			get
			{
				return GetCustom<double>("tl");
			}
			set
			{
				SetCustom<double>("tl", value);
			}
		}

		public string TournamentId
		{
			get
			{
				return GetCustom<string>("tid") ?? "";
			}
			set
			{
				SetCustom<string>("tid", value);
			}
		}

		public string TrackId
		{
			get
			{
				return GetCustom<string>("t") ?? "";
			}
			set
			{
				SetCustom<string>("t", value);
			}
		}

		public float TrackLenght
		{
			get
			{
				return GetCustom<float>("trl");
			}
			set
			{
				SetCustom<float>("trl", value);
			}
		}

		public MapCategory MapVotingCategory
		{
			get
			{
				return GetCustom<MapCategory>("mvc");
			}
			set
			{
				SetCustom<MapCategory>("mvc", value);
			}
		}

		public TimeoutMode TimeoutMode
		{
			get
			{
				return GetCustom<TimeoutMode>("tm");
			}
			set
			{
				SetCustom<TimeoutMode>("tm", value);
			}
		}

		public bool Crossplay
		{
			get
			{
				return GetCustom<bool>("cp");
			}
			set
			{
				SetCustom<bool>("cp", value);
			}
		}

		public string MasterPlatform
		{
			get
			{
				return GetCustom<string>("mp") ?? "undefined";
			}
			set
			{
				SetCustom<string>("mp", value);
			}
		}

		public string GetMostVotedTrack()
		{
			List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>();
			foreach (KeyValuePair<string, int> item in VoteTrackTable)
			{
				list.Add(item);
			}
			list.Sort((KeyValuePair<string, int> a, KeyValuePair<string, int> b) => (a.Value <= b.Value) ? 1 : (-1));
			int value = list[0].Value;
			for (int num = 0; num < list.Count; num++)
			{
				if (list[num].Value < value)
				{
					list.RemoveAt(num--);
				}
			}
			int value2 = UnityEngine.Random.Range(0, list.Count);
			value2 = Mathf.Clamp(value2, 0, list.Count - 1);
			if (list.Count > 0)
			{
				return list[value2].Key;
			}
			return "";
		}

		public NetworkRoom(PhotonService service, Room photonRoom, GameType gamemode)
		{
			Service = service;
			PhotonRoom = photonRoom;
			GameMode = gamemode;
			Outgoing = new NetworkRoomOperations(this);
			Incoming = new NetworkRoomEvents(this);
			StateMachine = new NetworkRoomState(this);
			Chat = new NetworkRoomChat(this);
			CachedRemoteRacers = new Dictionary<int, int>();
			Players = new Dictionary<int, NetworkActor>();
			Ghosts = new List<NetworkGhost>();
			PhotonPlayer[] playerList = PhotonNetwork.playerList;
			foreach (PhotonPlayer photonPlayer in playerList)
			{
				NetworkActor networkActor = new NetworkActor(photonPlayer);
				if (networkActor.IsMaster)
				{
					MasterPlatform = (MasterPlatform.Equals("undefined") ? OS.GetPlatformByContext() : MasterPlatform);
				}
				Players.Add(photonPlayer.ID, networkActor);
			}
			SetupPlugin();
			UpdateRestrictions();
			SetupLocalPlayer();
			SceneRacers = new Dictionary<int, NetworkRacer>();
			MapVotingCategory = MapCategory.All;
			TimeoutMode = TimeoutMode.Fixed;
			UpdatePlayersOrders();
			StartMatchmaking();
			UpdatePlayerIds();
			Debug.Log("NetworkRoom gamemode[" + gamemode.ToString() + "] matchmaking[" + MatchmakingType.ToString() + "]");
		}

		public void Reset()
		{
			SceneRacers.Clear();
			CachedRemoteRacers.Clear();
			Progress = 0f;
			Outgoing.Reset();
		}

		public void SetupLocalPlayer()
		{
			if (GamePlugin != null)
			{
				GamePlugin.LocalPlayerSetup();
			}
		}

		public void SetupPlugin()
		{
			switch (GameMode)
			{
			case GameType.Freestyle:
				GamePlugin = new FreestyleRoom(this);
				break;
			case GameType.Race:
				GamePlugin = new RaceRoom(this);
				break;
			case GameType.Tournament:
				GamePlugin = new TournamentRoom(this);
				break;
			default:
				Debug.LogError("PhotonService > GameRoom not implemented for gamemode: " + GameMode);
				break;
			}
		}

		public void RoomSetup()
		{
			if (GamePlugin != null)
			{
				GamePlugin.RoomSetup();
			}
		}

		public void ForceStartMatch()
		{
			if (GamePlugin != null)
			{
				GamePlugin.StartMatch();
			}
		}

		public void StartMatchmaking()
		{
			if (State != StateCode.MatchMaking)
			{
				RoomSetup();
				UpdateComposedProperties();
				if (IsMaster)
				{
					Outgoing.SendMatchmakingStarted();
				}
			}
		}

		public void StartLevelLoading()
		{
			if (IsMaster)
			{
				PlayerList.ForEach(delegate(NetworkActor el)
				{
					el.Reset();
				});
				SendLoadGame();
			}
		}

		public void SendLoadGame()
		{
			if (IsMaster)
			{
				LoadGameData loadGameData = new LoadGameData();
				loadGameData.Map = MapId;
				loadGameData.Track = TrackId;
				loadGameData.GameType = GameMode;
				loadGameData.CustomMapId = CustomMapId;
				Outgoing.SendLoadLevel(loadGameData);
			}
		}

		public void SendLevelLoaded()
		{
			if (CachedLevelData != null)
			{
				IsLoadingLevel = false;
				OnIncomingGameEvent(GameEventCode.OnLoadLevel, CachedLevelData.ToHashTable(), Local.RawData);
			}
			else
			{
				Outgoing.SendLevelLoaded();
			}
		}

		public void SendGameTimeout()
		{
			GameFinishedData gameFinishedData = new GameFinishedData();
			gameFinishedData.FinishedReason = GameFinishedData.Reason.Timeout;
			gameFinishedData.TimeElapsed = (float)TimeLimit;
			foreach (NetworkActor racer in Racers)
			{
				if (racer.RaceState == NetworkActor.RacerState.Running)
				{
					racer.RaceState = NetworkActor.RacerState.Timeout;
				}
			}
			Outgoing.SendEndGame(gameFinishedData);
		}

		public void SendGameCompleted()
		{
			GameFinishedData gameFinishedData = new GameFinishedData();
			gameFinishedData.FinishedReason = GameFinishedData.Reason.Completed;
			gameFinishedData.TimeElapsed = ElapsedTime;
			Outgoing.SendEndGame(gameFinishedData);
		}

		public void SendPlayerCompletedRace(float raceTime)
		{
			Local.RaceTime = raceTime;
			Local.RaceState = NetworkActor.RacerState.Complete;
			Outgoing.SendPlayerCompletedGame();
		}

		public void SendPlayerForfeit(float raceTime)
		{
			Local.RaceTime = raceTime;
			Local.RaceState = NetworkActor.RacerState.Forfeit;
			Outgoing.SendPlayerForfeitGame();
		}

		public void SendPlayerCrashed(float raceTime, Vector3 pos, Quaternion rot, Vector3 vel, CrashData p_crashData)
		{
			Local.RaceTime = raceTime;
			Local.RaceState = NetworkActor.RacerState.Crash;
			DroneState droneState = new DroneState();
			droneState.PlayerId = Local.ID;
			droneState.Position = pos;
			droneState.Rotation = rot.eulerAngles;
			droneState.Velocity = vel;
			droneState.CrashEnergy = p_crashData.crashEnergy;
			droneState.ContactNormal = p_crashData.contactNormal;
			droneState.ImpactVelocity = p_crashData.impactVelocity;
			droneState.ContactPoint = p_crashData.contactPoint;
			Outgoing.SendPlayerCrashed(droneState);
			Debug.Log("NetworkRoom> Sent PlayerCrashed network event.");
		}

		public void SendPlayerSubmittedLeaderboard()
		{
			Outgoing.SendPlayerSubmittedLeaderboard();
		}

		public void SendPlayerDamage(CrashData p_crashData)
		{
			if (p_crashData == null)
			{
				return;
			}
			DamageData damageData = new DamageData();
			damageData.NetworkID = Local.ID;
			if (p_crashData.type == DroneEventType.Crash)
			{
				damageData.bodyDamage = 1f;
				damageData.prop0Damage = 1f;
				damageData.prop1Damage = 1f;
				damageData.prop2Damage = 1f;
				damageData.prop3Damage = 1f;
			}
			else
			{
				damageData.bodyDamage = p_crashData.bodyDamage;
				if (p_crashData.propsDamage != null && p_crashData.propsDamage.Length == 4)
				{
					damageData.prop0Damage = p_crashData.propsDamage[0];
					damageData.prop1Damage = p_crashData.propsDamage[1];
					damageData.prop2Damage = p_crashData.propsDamage[2];
					damageData.prop3Damage = p_crashData.propsDamage[3];
				}
			}
			Outgoing.SendPlayerDamage(damageData);
			Debug.Log("NetworkRoom> Sent PlayerDamage network event.");
		}

		public void SendPlayerRecovered()
		{
			Local.RaceState = NetworkActor.RacerState.Running;
			Outgoing.SendPlayerRecovered(Local.ID);
			Debug.Log("NetworkRoom> Sent PlayerRecovered network event.");
		}

		public void SendReplayData(string replayDataUrl)
		{
			Local.IsReplaySent = true;
			Outgoing.SendReplayData(replayDataUrl);
		}

		public void SendPlayerVotedTrack(string trackGUID)
		{
			if (AllowMapVoting)
			{
				Local.VotedTrackGUID = trackGUID;
				Outgoing.SendPlayerVotedTrack();
			}
		}

		public void SendResetMatchmakingTimeout()
		{
			LobbyCountdown = MatchmakingTimeout;
		}

		public NetworkActor OnPlayerJoin(PhotonPlayer newPlayer)
		{
			NetworkActor networkActor = new NetworkActor(newPlayer);
			if (IsMaster)
			{
				int num = 0;
				foreach (NetworkActor value in Players.Values)
				{
					if (!value.IsSpectator && num <= value.Order)
					{
						num = value.Order + 1;
					}
				}
				networkActor.Order = num;
			}
			Players[newPlayer.ID] = networkActor;
			UpdateRestrictions();
			if (IsMaster)
			{
				if (State == StateCode.MatchMaking)
				{
					Outgoing.SendMatchmakingStarted(newPlayer.ID);
				}
				UpdatePlayerIds();
			}
			return networkActor;
		}

		public void UpdatePlayerIds()
		{
			if (IsMaster)
			{
				List<string> value = Racers.ConvertAll((NetworkActor el) => el.PlayerId);
				PlayersIdsData = JsonConvert.SerializeObject(value);
			}
		}

		public void TrySwitchToRacer(NetworkActor playerToPromote)
		{
			if (GamePlugin != null)
			{
				GamePlugin.SwitchToRacer(playerToPromote);
			}
		}

		public void TrySwitchToSpectator(NetworkActor playerToDowngrade, bool forced = false, bool p_notify = true)
		{
			if (GamePlugin != null)
			{
				GamePlugin.SwitchToSpectator(playerToDowngrade, forced, p_notify);
			}
		}

		public NetworkRacerLocal CreateLocalRacer(INetworkObservable observedObject)
		{
			NetworkRacerLocal networkRacerLocal = NetworkRacerLocal.Create(Local, observedObject, this);
			Outgoing.SendLocalPlayerSpawned(networkRacerLocal.Actor.ID);
			SceneRacers[networkRacerLocal.Actor.ID] = networkRacerLocal;
			return networkRacerLocal;
		}

		public NetworkRacerRemote CreateRemoteRacer(NetworkActor remoteActor, INetworkObservable observedObject)
		{
			NetworkRacerRemote networkRacerRemote = NetworkRacerRemote.Create(remoteActor, observedObject, this);
			SceneRacers[networkRacerRemote.Actor.ID] = networkRacerRemote;
			return networkRacerRemote;
		}

		public virtual void OnPlayerLeft(PhotonPlayer playerToRemove)
		{
			NetworkActor networkActor = TryGetPlayer(playerToRemove.ID);
			_ = networkActor.IsSpectator;
			Players.Remove(networkActor.ID);
			UpdateRestrictions();
			if (IsMaster)
			{
				if (RacersCount == 0 && !IsTournamentMatch)
				{
					TrySwitchToRacer(Local);
				}
				UpdatePlayersOrders();
				UpdatePlayerIds();
			}
			if (IsTournamentMatch && RacersCount == 0)
			{
				Service.TryLeaveRoom();
			}
		}

		public void OnRoomLeft()
		{
			OnEvent = null;
			foreach (NetworkRacer value in SceneRacers.Values)
			{
				value.CleanUp();
			}
		}

		public void OnMasterClientSwitched(PhotonPlayer newMasterClient)
		{
			UpdateComposedProperties();
		}

		public void OnPlayerPropertiesChanged(object[] playerAndUpdatedProps)
		{
			if (PhotonNetwork.masterClient == null)
			{
				return;
			}
			UpdateComposedProperties();
			UpdateRestrictions();
			if (playerAndUpdatedProps != null && playerAndUpdatedProps.Length >= 2)
			{
				_ = playerAndUpdatedProps[0];
				Hashtable hashtable = playerAndUpdatedProps[1] as Hashtable;
				if (IsMaster && hashtable.Count > 0 && hashtable.ContainsKey("st"))
				{
					UpdatePlayerIds();
				}
			}
		}

		public void OnRoomPropertiesChanged(Hashtable propertiesThatChanged)
		{
			if (propertiesThatChanged != null)
			{
				if (GamePlugin != null)
				{
					GamePlugin.OnRoomPropertiesUpdated(propertiesThatChanged);
				}
				if (propertiesThatChanged.ContainsKey("mr") || propertiesThatChanged.ContainsKey("ms") || propertiesThatChanged.ContainsKey("lc"))
				{
					UpdateRestrictions();
				}
			}
		}

		public void UpdatePlayersOrders()
		{
			if (IsMaster)
			{
				List<NetworkActor> racers = Racers;
				racers.Sort((NetworkActor a, NetworkActor b) => a.Order.CompareTo(b.Order));
				for (int num = 0; num < racers.Count; num++)
				{
					racers[num].Order = num;
				}
				List<NetworkActor> spectators = Spectators;
				spectators.Sort((NetworkActor a, NetworkActor b) => a.Order.CompareTo(b.Order));
				for (int num2 = 0; num2 < spectators.Count; num2++)
				{
					spectators[num2].Order = num2;
				}
			}
		}

		public bool SetCustomOrder(string[] p_playerIds)
		{
			if (p_playerIds == null || p_playerIds.Length == 0 || !IsMaster)
			{
				return false;
			}
			bool result = false;
			m_racerOrderSlots = new NetworkActor[6];
			for (int i = 0; i < p_playerIds.Length; i++)
			{
				for (int j = 0; j < Racers.Count; j++)
				{
					if (!(Racers[j].PlayerId != p_playerIds[i]))
					{
						if (Racers[j].Order != i)
						{
							result = true;
						}
						Racers[j].Order = i;
						if (i >= 0 && i < m_racerOrderSlots.Length)
						{
							m_racerOrderSlots[i] = Racers[j];
						}
						Debug.Log("NetworkRoom> Setting custom order for user " + Racers[j].ProfileName + " to position:" + Racers[j].Order);
						break;
					}
				}
			}
			for (int k = 0; k < Racers.Count; k++)
			{
				for (int l = k + 1; l < Racers.Count; l++)
				{
					if (Racers[k].Order != Racers[l].Order)
					{
						continue;
					}
					Debug.Log("NetworkRoom>Custom order found duplicate order idx between: " + Racers[k].ProfileName + " and " + Racers[l].ProfileName + " both assigned with order:" + Racers[k].Order);
					for (int m = 0; m < m_racerOrderSlots.Length; m++)
					{
						if (m_racerOrderSlots[m] == null)
						{
							m_racerOrderSlots[m] = Racers[l];
							Racers[l].Order = m;
							Debug.Log("NetworkRoom>Resolving duplicate order - moving player " + Racers[l].ProfileName + " to position: " + Racers[l].Order);
							result = true;
							break;
						}
					}
				}
			}
			return result;
		}

		public void TryUpdateLocalGhosts()
		{
			if (Ghosts == null || PhotonNetwork.masterClient == null || Local == null)
			{
				return;
			}
			if (string.IsNullOrEmpty(GhostsData))
			{
				Ghosts.Clear();
				return;
			}
			List<NetworkGhost> list = JsonConvert.DeserializeObject<List<NetworkGhost>>(GhostsData);
			if (list != null)
			{
				Ghosts.Clear();
				Ghosts.AddRange(list);
			}
			Local.GhostsProcessing = false;
		}

		public void UpdateGhostsCount()
		{
			if (!IsMaster || !AllowGhosts)
			{
				return;
			}
			int num = MaxRacers - RacersCount;
			if (Ghosts.Count > 0)
			{
				while (Ghosts.Count > num)
				{
					int num2 = Ghosts.Count - 1;
					if (num2 < 0 || num2 >= Ghosts.Count)
					{
						break;
					}
					Ghosts.RemoveAt(num2);
				}
			}
			string text = JsonConvert.SerializeObject(Ghosts);
			Debug.Log("[NetworkRoom.TryUpdateGhosts] - Json:\n " + text + " ");
			GhostsData = text;
		}

		protected void UpdateComposedProperties()
		{
			NetworkActor networkActor = TryGetPlayer(PhotonNetwork.masterClient.ID);
			if (networkActor != null)
			{
				MasterProfileColour = networkActor.MainColor;
				MasterProfilePhoto = networkActor.ProfilePhoto;
				MasterId = networkActor.PlatformId;
				MasterBlockList = networkActor.BlockList;
			}
		}

		private void UpdateRestrictions()
		{
			if (GamePlugin != null)
			{
				GamePlugin.UpdateRestrictions();
			}
		}

		public void SetInterestGroupEnabled(byte groupId, bool isEnabled)
		{
			PhotonNetwork.SetInterestGroups(groupId, isEnabled);
		}

		public void OnIncomingGameEvent(GameEventCode eventCode, object content, PhotonPlayer sender)
		{
			GameEvent gameEvent = Incoming.OnEvent(eventCode, content, sender);
			if (GamePlugin != null && GamePlugin.OnGameEvent(gameEvent) && OnEvent != null)
			{
				OnEvent(gameEvent);
			}
		}

		public void OnStateChanged(StateCode newState)
		{
			StateMachine.SetState(newState);
			UpdateRestrictions();
		}

		public T GetCustom<T>(string key)
		{
			T result = default(T);
			if (PhotonRoom != null && PhotonRoom.CustomProperties.ContainsKey(key))
			{
				return (T)PhotonRoom.CustomProperties[key];
			}
			return result;
		}

		public void SetCustom<T>(string key, object newValue)
		{
			if (PhotonRoom != null && PhotonNetwork.isMasterClient && newValue != null && !EqualityComparer<T>.Default.Equals(GetCustom<T>(key), (T)newValue))
			{
				cachedPropertyHashtable.Clear();
				cachedPropertyHashtable.Add(key, newValue);
				PhotonRoom.SetCustomProperties(cachedPropertyHashtable);
			}
		}

		public NetworkActor TryGetPlayer(int ActorId)
		{
			NetworkActor value = null;
			Players.TryGetValue(ActorId, out value);
			return value;
		}

		public void AddTolastPlayersList(NetworkActor player)
		{
			if (player != null)
			{
				if (lastPlayers == null)
				{
					lastPlayers = new List<NetworkActor>();
				}
				if (lastPlayers.Contains(player))
				{
					lastPlayers.Remove(player);
				}
				lastPlayers.Add(player);
				if (lastPlayers.Count > 5)
				{
					lastPlayers.RemoveAt(0);
				}
				Debug.LogWarning("NetworkRoom.AddPlayerTolastPlayers> Added player " + player.ProfileName + " to lastPlayers list - count:" + lastPlayers.Count);
			}
		}

		public virtual void Update()
		{
			StateMachine.Update(this);
		}
	}
}

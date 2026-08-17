using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using drl.backend;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class ServiceModel : Model<DRLApp>
	{
		public float stateRefresh = 60f;

		public bool stateAutoRefresh;

		public float timeRefresh = 120f;

		internal float state_elapsed;

		internal float time_elapsed;

		internal bool state_lock;

		internal bool time_lock;

		internal bool logged;

		private float m_reconnectTimer = 1f;

		private Activity m_reconnectActivity;

		private float m_socketReconnectTimer = 1500f;

		private DRLTournamentSocketData m_tournamentSocketData;

		protected SteamService steam => AssertLocal<SteamService>("steam");

		protected XboxLiveService xboxlive => null;

		protected PlaystationService playstation => null;

		protected EpicService epic => null;

		public PlatformService platform
		{
			get
			{
				if ((bool)xboxlive)
				{
					UnityEngine.Object.Destroy(xboxlive);
				}
				if ((bool)playstation)
				{
					UnityEngine.Object.Destroy(playstation);
				}
				if ((bool)epic)
				{
					UnityEngine.Object.Destroy(epic);
				}
				return steam;
			}
		}

		public DRLService backend => AssertLocal<DRLService>("backend");

		public SocialModel social => base.transform.Find("social").GetComponent<SocialModel>();

		public OpponentModel opponent => Assert<OpponentModel>("opponent");

		public TournamentSocketService tournamentSocket => Assert<TournamentSocketService>("tournamentSocket");

		public static DRLLeaderboardData CreateLeaderboardData(int p_order, int p_score, int p_crashes, GameFlag p_game_type, bool p_multiplayer, ScoreType p_score_type, DRLMap p_map, DRLMapTrack p_track, DRLCampaign p_campaign, DRLMission p_mission, float p_topSpeed = 0f)
		{
			DRLLeaderboardData dRLLeaderboardData = new DRLLeaderboardData();
			if ((bool)p_map)
			{
				dRLLeaderboardData.map = p_map.guid;
			}
			if ((bool)p_track)
			{
				dRLLeaderboardData.track = p_track.guid;
			}
			AssertCommunityMap(dRLLeaderboardData, p_map, p_track);
			if ((bool)p_campaign)
			{
				dRLLeaderboardData.group = p_campaign.guid;
			}
			if ((bool)p_mission)
			{
				dRLLeaderboardData.mission = p_mission.guid;
			}
			dRLLeaderboardData.order = p_order;
			dRLLeaderboardData.score = p_score;
			dRLLeaderboardData.crashCount = p_crashes;
			dRLLeaderboardData.topSpeed = p_topSpeed;
			dRLLeaderboardData.gameType = p_game_type.ToString();
			dRLLeaderboardData.multiplayer = p_multiplayer;
			dRLLeaderboardData.scoreTypeFlag = p_score_type;
			dRLLeaderboardData.controllerType = RCI.GetControllerStateType(ControllerStateType.XBox).ToString();
			return dRLLeaderboardData;
		}

		public static void AssertCommunityMap(DRLLeaderboardData p_data, DRLMap p_map, DRLMapTrack p_track)
		{
			if (p_data != null)
			{
				DRLMap dRLMap = (p_map ? p_map : (p_track ? p_track.map : null));
				if (p_data.isCustomMap = dRLMap != null && dRLMap.data != null)
				{
					p_data.customMap = dRLMap.data.guid;
				}
			}
		}

		public static void AssertCommunityMap(DRLReplayData p_data, DRLMap p_map, DRLMapTrack p_track)
		{
			if (p_data != null)
			{
				DRLMap dRLMap = (p_map ? p_map : (p_track ? p_track.map : null));
				if (p_data.isCustomMap = dRLMap != null && dRLMap.data != null)
				{
					MapData data = p_map.data;
					p_data.customMap = data.guid;
				}
			}
		}

		public static void AssertCustomFlags(DRLLeaderboardData p_data, int p_drlOfficial, int p_customPhysics)
		{
			if (p_data != null)
			{
				p_data.drlOfficial = p_drlOfficial == 1;
				p_data.customPhysics = p_customPhysics != 0;
				if (p_data.customPhysics)
				{
					p_data.drlOfficial = false;
				}
			}
		}

		public static void AssertCustomFlags(DRLCircuitLeaderboardData p_data, int p_drlOfficial, int p_customPhysics)
		{
			if (p_data != null)
			{
				p_data.drlOfficial = p_drlOfficial == 1;
				p_data.customPhysics = p_customPhysics != 0;
				if (p_data.customPhysics)
				{
					p_data.drlOfficial = false;
				}
			}
		}

		public static void AssertCustomFlags(DRLLeaderboardData p_data, bool p_drlOfficial, int p_customPhysics)
		{
			AssertCustomFlags(p_data, p_drlOfficial ? 1 : 0, p_customPhysics);
		}

		public static void AssertCustomFlags(DRLCircuitLeaderboardData p_data, bool p_drlOfficial, int p_customPhysics)
		{
			AssertCustomFlags(p_data, p_drlOfficial ? 1 : 0, p_customPhysics);
		}

		public static void AssertCustomFlags(DRLLeaderboardData p_data, bool p_drlOfficial, bool p_customPhysics)
		{
			AssertCustomFlags(p_data, p_drlOfficial ? 1 : 0, p_customPhysics ? 1 : 0);
		}

		public static DRLLeaderboardData CreateRaceLeaderboardData(int p_order, float p_time, int p_crashes, DRLMapTrack p_track, float p_topSpeed = 0f)
		{
			float num = Mathf.Round(p_time * 1000f) / 1000f;
			return CreateLeaderboardData(p_order, Mathf.FloorToInt(num * 1000f), p_crashes, GameFlag.Race, p_multiplayer: false, ScoreType.TimeMin, p_track ? p_track.map : null, p_track, null, null, p_topSpeed);
		}

		public static DRLLeaderboardData CreateCollectablesLeaderboardData(int p_order, float p_time, int p_crashes, DRLMapTrack p_track, float p_topSpeed = 0f)
		{
			float num = Mathf.Round(p_time * 1000f) / 1000f;
			return CreateLeaderboardData(p_order, Mathf.FloorToInt(num * 1000f), p_crashes, GameFlag.Collectable, p_multiplayer: false, ScoreType.TimeMin, p_track ? p_track.map : null, p_track, null, null, p_topSpeed);
		}

		public static DRLLeaderboardData CreateRaceLeaderboardData(float p_time, int p_crashes, DRLMapTrack p_track)
		{
			return CreateRaceLeaderboardData(0, p_time, p_crashes, p_track);
		}

		public static DRLLeaderboardData CreateMissionLeaderboardData(float p_score, DRLMission p_mission, int p_crashes = 0)
		{
			float num = Mathf.Round(p_score * 1000f) / 1000f;
			return CreateLeaderboardData(0, Mathf.FloorToInt(num * 1000f), p_crashes, GameFlag.Mission, p_multiplayer: false, ScoreType.ScoreMax, null, null, null, p_mission);
		}

		protected void Awake()
		{
			state_lock = true;
			time_lock = true;
			state_elapsed = 0f;
			time_elapsed = 0f;
		}

		public void Login(Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string platform_id = platform.id.ToString();
			string p_auth_ticket = (steam ? steam.session : ("DRL-" + GUID.Create(32, "", 0, 0, 15, "x")));
			string versionString = DRLApp.GetVersionString();
			backend.Login(p_auth_ticket, platform_id, versionString, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("ServiceModel> Login / Error");
					p_result.id = platform_id;
					Notify("service.login@error", p_result);
				}
				else
				{
					backend.token = p_result.token;
					SerializedData data = p_result.GetData<SerializedData>();
					backend.loginData = data;
					if (data == null)
					{
						Debug.LogWarning("ServiceModel> Login / Invalid Login Data!");
					}
					logged = true;
					if (p_callback != null)
					{
						p_callback(p_result);
					}
					Notify("service.login@success");
				}
			}, p_timeout);
		}

		public void StateGame(Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			backend.State(null, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("ServiceModel> StateGame - Error\n" + p_result.message);
					Notify("service.state.game@error", p_result);
				}
				else
				{
					state_lock = false;
					Notify("service.state.game@refresh", p_result);
					if (p_callback != null)
					{
						p_callback(p_result);
					}
				}
			}, null, p_timeout);
		}

		public void State(Action<DRLServiceResult> p_callback, Dictionary<string, string> p_data = null, int p_timeout = -1)
		{
			backend.State(backend.token, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("ServiceModel> State - Error\n" + p_result.message);
					Notify("service.state@error", p_result);
				}
				else
				{
					state_lock = false;
					string p_event = ((p_data == null) ? "service.state@refresh" : "service.state.write");
					Notify(p_event, p_result);
					if (p_callback != null)
					{
						p_callback(p_result);
					}
				}
			}, p_data, p_timeout);
		}

		public void License(Action<DRLLicenseResult> p_callback)
		{
			backend.License(delegate(DRLLicenseResult p_result)
			{
				if (p_result == null)
				{
					if (p_callback != null)
					{
						p_callback(null);
					}
					Notify("service.license@error");
				}
				else if (p_callback != null)
				{
					p_callback(p_result);
				}
			});
		}

		public void StatePlayer(string p_id, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			backend.State(p_id, p_callback, null, p_timeout);
		}

		public void GetContentManifest(string p_branch, string p_platform, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			backend.GetContentManifest(p_branch, p_platform, p_callback, p_timeout);
		}

		public WebAsyncRequest GetLeaderboardUser(string p_player_id, DRLLeaderboardData p_query, int p_limit, int p_drone_class, Action<DRLLeaderboardResult> p_callback, int p_timeout = -1, bool p_collectable = false)
		{
			DRLLeaderboardData dRLLeaderboardData = ((p_query == null) ? new DRLLeaderboardData() : p_query);
			dRLLeaderboardData.playerId = (string.IsNullOrEmpty(p_player_id) ? backend.playerId : p_player_id);
			dRLLeaderboardData.limit = p_limit;
			dRLLeaderboardData.diameter = p_drone_class;
			if (dRLLeaderboardData.diameter < 0)
			{
				dRLLeaderboardData.Remove("diameter");
			}
			return backend.GetLeaderboardUser(dRLLeaderboardData, p_callback, p_timeout, p_collectable);
		}

		public WebAsyncRequest GetLeaderboardUser(DRLLeaderboardData p_query, int p_limit, int p_drone_class, Action<DRLLeaderboardResult> p_callback, int p_timeout = -1)
		{
			return GetLeaderboardUser("", p_query, p_limit, p_drone_class, p_callback, p_timeout);
		}

		public WebAsyncRequest GetLeaderboardUser(string p_player_id, DRLCampaign p_campaign, DRLMap p_map, DRLMapTrack p_track, int p_limit, int p_drone_class, bool? p_drone_official, int p_physics, Action<DRLLeaderboardResult> p_callback, string p_platform = null, string p_controller_type = null, string p_circuitId = null, string p_drone_guid = null, int p_timeout = -1, bool p_collectable = false)
		{
			if (!string.IsNullOrEmpty(p_circuitId))
			{
				return backend.GetCircuitLeaderboardUser(backend.playerId, p_circuitId, p_limit, p_drone_class, p_drone_official, p_physics, p_callback, p_platform, p_controller_type, p_drone_guid, p_timeout);
			}
			DRLLeaderboardData dRLLeaderboardData = new DRLLeaderboardData();
			dRLLeaderboardData.playerId = (string.IsNullOrEmpty(p_player_id) ? backend.playerId : p_player_id);
			dRLLeaderboardData.limit = p_limit;
			dRLLeaderboardData.gameType = (p_campaign ? "Campaign" : "Race");
			if (p_collectable)
			{
				dRLLeaderboardData.gameType = "Collectable";
			}
			dRLLeaderboardData.scoreTypeFlag = ScoreType.TimeMin;
			dRLLeaderboardData.diameter = p_drone_class;
			if (dRLLeaderboardData.diameter < 0)
			{
				dRLLeaderboardData.Remove("diameter");
			}
			if ((bool)p_campaign)
			{
				dRLLeaderboardData.group = p_campaign.guid;
			}
			if ((bool)p_map)
			{
				dRLLeaderboardData.map = p_map.guid;
			}
			if ((bool)p_track)
			{
				dRLLeaderboardData.track = p_track.guid;
			}
			if (string.IsNullOrEmpty(p_platform))
			{
				dRLLeaderboardData.Remove("profile-platform");
			}
			else
			{
				dRLLeaderboardData.platform = p_platform;
			}
			if (string.IsNullOrEmpty(p_controller_type))
			{
				dRLLeaderboardData.Remove("controller-type");
			}
			else
			{
				dRLLeaderboardData.controllerType = p_controller_type;
			}
			if (string.IsNullOrEmpty(p_drone_guid))
			{
				dRLLeaderboardData.Remove("drone-guid");
			}
			else
			{
				dRLLeaderboardData.droneGuid = p_drone_guid;
			}
			if (p_drone_official.HasValue)
			{
				AssertCustomFlags(dRLLeaderboardData, p_drone_official.Value, p_physics);
			}
			else
			{
				dRLLeaderboardData.Remove("drl-official");
				dRLLeaderboardData.customPhysics = p_physics != 0;
			}
			AssertCommunityMap(dRLLeaderboardData, p_map, p_track);
			return backend.GetLeaderboardUser(dRLLeaderboardData, p_callback, p_timeout, p_collectable);
		}

		public WebAsyncRequest GetLeaderboardSpecificUser(string p_player_name, DRLCampaign p_campaign, DRLMap p_map, DRLMapTrack p_track, bool isCustomMap, int p_limit, int p_drone_class, bool? p_drone_official, int p_physics, Action<DRLLeaderboardResult> p_callback, string p_platform = null, string p_controller_type = null, string p_circuitId = null, string p_drone_guid = null, int p_timeout = -1, bool p_collectable = false)
		{
			if (!string.IsNullOrEmpty(p_circuitId))
			{
				return backend.GetCircuitLeaderboardSpecificUser(p_player_name, p_circuitId, p_limit, p_drone_class, p_drone_official, p_physics, p_callback, p_platform, p_controller_type, p_drone_guid, p_timeout);
			}
			DRLLeaderboardData dRLLeaderboardData = new DRLLeaderboardData();
			dRLLeaderboardData.username = p_player_name;
			dRLLeaderboardData.limit = p_limit;
			dRLLeaderboardData.gameType = (p_campaign ? "Campaign" : "Race");
			if (p_collectable)
			{
				dRLLeaderboardData.gameType = "Collectable";
			}
			if (p_circuitId != null)
			{
				dRLLeaderboardData.gameType = "Circuit";
			}
			dRLLeaderboardData.isCustomMap = isCustomMap;
			dRLLeaderboardData.scoreTypeFlag = ScoreType.TimeMin;
			dRLLeaderboardData.diameter = p_drone_class;
			if (dRLLeaderboardData.diameter < 0)
			{
				dRLLeaderboardData.Remove("diameter");
			}
			if ((bool)p_campaign)
			{
				dRLLeaderboardData.group = p_campaign.guid;
			}
			if ((bool)p_map)
			{
				dRLLeaderboardData.map = p_map.guid;
			}
			if ((bool)p_track)
			{
				dRLLeaderboardData.track = p_track.guid;
			}
			if (string.IsNullOrEmpty(p_platform))
			{
				dRLLeaderboardData.Remove("profile-platform");
			}
			else
			{
				dRLLeaderboardData.platform = p_platform;
			}
			if (string.IsNullOrEmpty(p_controller_type))
			{
				dRLLeaderboardData.Remove("controller-type");
			}
			else
			{
				dRLLeaderboardData.controllerType = p_controller_type;
			}
			if (string.IsNullOrEmpty(p_drone_guid))
			{
				dRLLeaderboardData.Remove("drone-guid");
			}
			else
			{
				dRLLeaderboardData.droneGuid = p_drone_guid;
			}
			if (p_drone_official.HasValue)
			{
				AssertCustomFlags(dRLLeaderboardData, p_drone_official.Value, p_physics);
			}
			else
			{
				dRLLeaderboardData.Remove("drl-official");
				dRLLeaderboardData.customPhysics = p_physics != 0;
			}
			AssertCommunityMap(dRLLeaderboardData, p_map, p_track);
			return backend.GetLeaderboardSpecificUser(dRLLeaderboardData, p_callback, isCustomMap, p_timeout);
		}

		public WebAsyncRequest GetCircuitLeaderboardUser(string p_circuitId, int p_limit, int p_drone_class, bool? p_drone_official, int p_physics, Action<DRLLeaderboardResult> p_callback, string p_platform = null, string p_controller_type = null, string p_drone_guid = null, int p_timeout = -1)
		{
			if (string.IsNullOrEmpty(p_circuitId))
			{
				return null;
			}
			return backend.GetCircuitLeaderboardUser(backend.playerId, p_circuitId, p_limit, p_drone_class, p_drone_official, p_physics, p_callback, p_platform, p_controller_type, p_drone_guid, p_timeout);
		}

		public WebAsyncRequest GetLeaderboardUser(string p_player_id, DRLCampaign p_campaign, MapData p_custom_map, int p_limit, int p_drone_class, bool? p_drone_official, int p_physics, Action<DRLLeaderboardResult> p_callback, string p_platform = null, string p_controller_type = null, string p_circuitId = null, string p_drone_guid = null, int p_timeout = -1, bool p_collectable = false)
		{
			if (!string.IsNullOrEmpty(p_circuitId))
			{
				return backend.GetCircuitLeaderboardUser(backend.playerId, p_circuitId, p_limit, p_drone_class, p_drone_official, p_physics, p_callback, p_platform, p_controller_type, p_drone_guid, p_timeout);
			}
			DRLLeaderboardData dRLLeaderboardData = new DRLLeaderboardData();
			dRLLeaderboardData.playerId = (string.IsNullOrEmpty(p_player_id) ? backend.playerId : p_player_id);
			dRLLeaderboardData.limit = p_limit;
			dRLLeaderboardData.gameType = (p_campaign ? "Campaign" : "Race");
			dRLLeaderboardData.scoreTypeFlag = ScoreType.TimeMin;
			dRLLeaderboardData.diameter = p_drone_class;
			if (dRLLeaderboardData.diameter < 0)
			{
				dRLLeaderboardData.Remove("diameter");
			}
			if ((bool)p_campaign)
			{
				dRLLeaderboardData.group = p_campaign.guid;
			}
			dRLLeaderboardData.isCustomMap = true;
			dRLLeaderboardData.customMap = p_custom_map.guid;
			if (string.IsNullOrEmpty(p_platform))
			{
				dRLLeaderboardData.Remove("profile-platform");
			}
			else
			{
				dRLLeaderboardData.platform = p_platform;
			}
			if (string.IsNullOrEmpty(p_controller_type))
			{
				dRLLeaderboardData.Remove("controller-type");
			}
			else
			{
				dRLLeaderboardData.controllerType = p_controller_type;
			}
			if (string.IsNullOrEmpty(p_drone_guid))
			{
				dRLLeaderboardData.Remove("drone-guid");
			}
			else
			{
				dRLLeaderboardData.droneGuid = p_drone_guid;
			}
			if (p_drone_official.HasValue)
			{
				AssertCustomFlags(dRLLeaderboardData, p_drone_official.Value, p_physics);
			}
			else
			{
				dRLLeaderboardData.Remove("drl-official");
				dRLLeaderboardData.customPhysics = p_physics != 0;
			}
			return backend.GetLeaderboardUser(dRLLeaderboardData, p_callback, p_timeout, p_collectable);
		}

		public WebAsyncRequest GetLeaderboardUser(string p_player_id, DRLMap p_map, DRLMapTrack p_track, int p_limit, int p_drone_class, bool p_drone_official, int p_physics, Action<DRLLeaderboardResult> p_callback, string p_platform = null, string p_controller_type = null, string p_circuitId = null, string p_drone_guid = null, int p_timeout = -1)
		{
			return GetLeaderboardUser(p_player_id, null, p_map, p_track, p_limit, p_drone_class, p_drone_official, p_physics, p_callback, p_platform, p_controller_type, p_circuitId, p_drone_guid, p_timeout);
		}

		public WebAsyncRequest GetLeaderboardSpecificUser(string p_player_name, DRLMap p_map, DRLMapTrack p_track, bool isCustomMap, int p_limit, int p_drone_class, bool p_drone_official, int p_physics, Action<DRLLeaderboardResult> p_callback, string p_platform = null, string p_controller_type = null, string p_circuitId = null, string p_drone_guid = null, bool p_iscollectable = false, int p_timeout = -1)
		{
			return GetLeaderboardSpecificUser(p_player_name, null, p_map, p_track, isCustomMap, p_limit, p_drone_class, p_drone_official, p_physics, p_callback, p_platform, p_controller_type, p_circuitId, p_drone_guid, p_timeout, p_iscollectable);
		}

		public WebAsyncRequest GetLeaderboardUser(DRLCampaign p_campaign, int p_limit, int p_drone_class, Action<DRLLeaderboardResult> p_callback, string p_platform = null, string p_controller_type = null, string p_circuitId = null, string p_drone_guid = null, int p_timeout = -1)
		{
			return GetLeaderboardUser("", p_campaign, null, null, p_limit, p_drone_class, true, 0, p_callback, p_platform, p_controller_type, p_circuitId, p_drone_guid, p_timeout);
		}

		public WebAsyncRequest GetLeaderboardUser(DRLMap p_map, DRLMapTrack p_track, int p_limit, int p_drone_class, bool? p_drone_official, int p_physics, Action<DRLLeaderboardResult> p_callback, string p_platform = null, string p_controller_type = null, string p_circuitId = null, string p_drone_guid = null, int p_timeout = -1, bool p_collectable = false)
		{
			return GetLeaderboardUser("", null, p_map, p_track, p_limit, p_drone_class, p_drone_official, p_physics, p_callback, p_platform, p_controller_type, p_circuitId, p_drone_guid, p_timeout, p_collectable);
		}

		public WebAsyncRequest GetLeaderboardUser(MapData p_custom_map, int p_limit, int p_drone_class, bool? p_drone_official, int p_physics, Action<DRLLeaderboardResult> p_callback, string p_platform = null, string p_controller_type = null, string p_circuitId = null, string p_drone_guid = null, int p_timeout = -1, bool p_collectable = false)
		{
			return GetLeaderboardUser("", null, p_custom_map, p_limit, p_drone_class, p_drone_official, p_physics, p_callback, p_platform, p_controller_type, p_circuitId, p_drone_guid, p_timeout, p_collectable);
		}

		public WebAsyncRequest GetLeaderboardRivals(string p_player_id, DRLLeaderboardData p_query, int p_limit, Action<DRLLeaderboardRivalsResult> p_callback, int p_timeout = -1, bool p_collectable = false)
		{
			DRLLeaderboardData dRLLeaderboardData = ((p_query == null) ? new DRLLeaderboardData() : p_query);
			dRLLeaderboardData.playerId = (string.IsNullOrEmpty(p_player_id) ? backend.playerId : p_player_id);
			dRLLeaderboardData.limit = p_limit;
			return backend.GetLeaderboardRivals(dRLLeaderboardData, p_callback, p_timeout);
		}

		public WebAsyncRequest GetLeaderboardRivals(DRLLeaderboardData p_query, int p_limit, Action<DRLLeaderboardRivalsResult> p_callback, int p_timeout = -1, bool p_collectable = false)
		{
			return GetLeaderboardRivals("", p_query, p_limit, p_callback, p_timeout);
		}

		public WebAsyncRequest GetLeaderboardRivals(string p_player_id, DRLCampaign p_campaign, DRLMap p_map, DRLMapTrack p_track, int p_limit, int p_drone_class, bool p_official, bool p_customPhysics, Action<DRLLeaderboardRivalsResult> p_callback, int p_timeout = -1, string p_matchId = null, bool p_collectable = false)
		{
			DRLLeaderboardData dRLLeaderboardData = new DRLLeaderboardData();
			dRLLeaderboardData.playerId = (string.IsNullOrEmpty(p_player_id) ? backend.playerId : p_player_id);
			dRLLeaderboardData.limit = p_limit;
			dRLLeaderboardData.gameType = (p_campaign ? GameFlag.Campaign.ToString() : GameFlag.Race.ToString());
			if (p_collectable)
			{
				dRLLeaderboardData.gameType = "Collectable";
			}
			dRLLeaderboardData.scoreTypeFlag = ScoreType.TimeMin;
			dRLLeaderboardData.diameter = p_drone_class;
			if (!string.IsNullOrEmpty(p_matchId))
			{
				dRLLeaderboardData.matchId = p_matchId;
			}
			if (dRLLeaderboardData.diameter < 0)
			{
				dRLLeaderboardData.Remove("diameter");
			}
			if ((bool)p_campaign)
			{
				dRLLeaderboardData.group = p_campaign.guid;
			}
			if ((bool)p_map)
			{
				dRLLeaderboardData.map = p_map.guid;
			}
			if ((bool)p_track)
			{
				dRLLeaderboardData.track = p_track.guid;
			}
			if (!p_campaign)
			{
				AssertCustomFlags(dRLLeaderboardData, p_official, p_customPhysics);
			}
			AssertCommunityMap(dRLLeaderboardData, p_map, p_track);
			return backend.GetLeaderboardRivals(dRLLeaderboardData, p_callback, p_timeout);
		}

		public WebAsyncRequest GetLeaderboardRivals(string p_player_id, MapData p_custom_map, int p_limit, int p_drone_class, bool p_official, bool p_customPhysics, Action<DRLLeaderboardRivalsResult> p_callback, int p_timeout = -1, bool p_collectable = false)
		{
			DRLLeaderboardData dRLLeaderboardData = new DRLLeaderboardData();
			dRLLeaderboardData.playerId = (string.IsNullOrEmpty(p_player_id) ? backend.playerId : p_player_id);
			dRLLeaderboardData.limit = p_limit;
			dRLLeaderboardData.gameType = GameFlag.Race.ToString();
			if (p_collectable)
			{
				dRLLeaderboardData.gameType = "Collectable";
			}
			dRLLeaderboardData.scoreTypeFlag = ScoreType.TimeMin;
			dRLLeaderboardData.diameter = p_drone_class;
			if (dRLLeaderboardData.diameter < 0)
			{
				dRLLeaderboardData.Remove("diameter");
			}
			dRLLeaderboardData.customMap = p_custom_map.guid;
			dRLLeaderboardData.map = p_custom_map.mapId;
			dRLLeaderboardData.track = "";
			dRLLeaderboardData.isCustomMap = true;
			AssertCustomFlags(dRLLeaderboardData, p_official, p_customPhysics);
			return backend.GetLeaderboardRivals(dRLLeaderboardData, p_callback, p_timeout);
		}

		public WebAsyncRequest GetLeaderboardRivals(string p_player_id, DRLMap p_map, DRLMapTrack p_track, int p_limit, int p_drone_class, bool p_official, bool p_customPhysics, Action<DRLLeaderboardRivalsResult> p_callback, int p_timeout = -1, bool p_collectable = false)
		{
			return GetLeaderboardRivals(p_player_id, null, p_map, p_track, p_limit, p_drone_class, p_official, p_customPhysics, p_callback, p_timeout, null, p_collectable);
		}

		public WebAsyncRequest GetLeaderboardRivals(DRLMap p_map, DRLMapTrack p_track, int p_limit, int p_drone_class, bool p_official, bool p_customPhysics, Action<DRLLeaderboardRivalsResult> p_callback, int p_timeout = -1, bool p_collectable = false)
		{
			return GetLeaderboardRivals("", null, p_map, p_track, p_limit, p_drone_class, p_official, p_customPhysics, p_callback, p_timeout, null, p_collectable);
		}

		public WebAsyncRequest GetLeaderboardRivals(DRLMap p_map, DRLMapTrack p_track, int p_limit, int p_drone_class, bool p_official, bool p_customPhysics, string p_matchId, Action<DRLLeaderboardRivalsResult> p_callback, int p_timeout = -1, bool p_collectable = false)
		{
			return GetLeaderboardRivals("", null, p_map, p_track, p_limit, p_drone_class, p_official, p_customPhysics, p_callback, p_timeout, p_matchId, p_collectable);
		}

		public WebAsyncRequest GetLeaderboardRivals(DRLCampaign p_campaign, int p_limit, int p_drone_class, bool p_official, bool p_customPhysics, Action<DRLLeaderboardRivalsResult> p_callback, int p_timeout = -1, bool p_collectable = false)
		{
			return GetLeaderboardRivals("", p_campaign, null, null, p_limit, p_drone_class, p_official, p_customPhysics, p_callback, p_timeout, null, p_collectable);
		}

		protected WebAsyncRequest SetLeaderboardCard(UILeaderboardCardView p_card, bool p_self, DRLCampaign p_campaign, DRLMap p_map, DRLMapTrack p_track, int p_drone_class, bool p_official, bool p_customPhysics, bool p_collectable = false)
		{
			if (!base.app)
			{
				return null;
			}
			if (!this)
			{
				return null;
			}
			return GetLeaderboardRivals("", p_campaign, p_map, p_track, 1, p_drone_class, p_official, p_customPhysics, delegate(DRLLeaderboardRivalsResult p_result)
			{
				SetLeaderboardCard(p_card, p_self, p_result);
			}, -1, null, p_collectable);
		}

		protected WebAsyncRequest SetLeaderboardCard(UILeaderboardCardView p_card, bool p_self, MapData p_custom_map, int p_drone_class, bool p_official, bool p_customPhysics, bool p_collectable)
		{
			return base.app.model.service.GetLeaderboardRivals("", p_custom_map, 1, p_drone_class, p_official, p_customPhysics, delegate(DRLLeaderboardRivalsResult p_result)
			{
				SetLeaderboardCard(p_card, p_self, p_result);
			}, -1, p_collectable);
		}

		public void SetLeaderboardCard(UILeaderboardCardView p_card, bool p_self, DRLLeaderboardRivalsResult p_result)
		{
			if (p_result == null)
			{
				Debug.LogWarning("ServiceModel> GetLeaderboardRivals - Failed!");
			}
			else
			{
				if (this == null || base.gameObject == null || p_card == null)
				{
					return;
				}
				FadeComponent component = p_card.GetComponent<FadeComponent>();
				Debug.Log("ServiceModel> GetLeaderboardRivals - Success - count[" + p_result.top.Length + "]");
				DRLLeaderboardData dRLLeaderboardData = null;
				if (p_self)
				{
					if (p_result.player >= 0)
					{
						dRLLeaderboardData = p_result.rivals[p_result.player];
					}
				}
				else if (p_result.top.Length != 0)
				{
					dRLLeaderboardData = p_result.top[0];
				}
				if (dRLLeaderboardData == null)
				{
					if ((bool)component)
					{
						component.Fade(0.1f, 0.1f);
					}
					return;
				}
				if ((bool)component)
				{
					component.Fade(1f, 0.1f);
				}
				p_card.Set(dRLLeaderboardData);
			}
		}

		public WebAsyncRequest SetLeaderboardCard(UILeaderboardCardView p_card, bool p_self, DRLCampaign p_campaign, int p_drone_class, bool p_official, bool p_customPhysics)
		{
			return SetLeaderboardCard(p_card, p_self, p_campaign, null, null, p_drone_class, p_official, p_customPhysics);
		}

		public WebAsyncRequest SetLeaderboardCard(UILeaderboardCardView p_card, bool p_self, DRLMap p_map, DRLMapTrack p_track, int p_drone_class, bool p_official, bool p_customPhysics, bool p_collectable = false)
		{
			return SetLeaderboardCard(p_card, p_self, null, p_map, p_track, p_drone_class, p_official, p_customPhysics, p_collectable);
		}

		public WebAsyncRequest GetLeaderboard(DRLLeaderboardData p_query, Action<DRLLeaderboardResult> p_callback, bool p_group = false, int p_timeout = -1, bool p_collectable = false)
		{
			return backend.GetLeaderboard(p_query, p_group, p_callback, p_timeout, p_collectable);
		}

		public WebAsyncRequest GetLeaderboard(int p_page, int p_limit, DRLLeaderboardData p_query, Action<DRLLeaderboardResult> p_callback, bool p_group = false, int p_timeout = -1, bool p_collectable = false)
		{
			p_query.page = p_page;
			p_query.limit = p_limit;
			return GetLeaderboard(p_query, p_callback, p_group, p_timeout, p_collectable);
		}

		public WebAsyncRequest GetLeaderboard(DRLCampaign p_campaign, DRLMap p_map, DRLMapTrack p_track, string p_region, int p_page, int p_limit, int p_drone_class, bool? p_drone_official, int p_physics, Action<DRLLeaderboardResult> p_callback, string p_platform = null, string p_controller_type = null, bool p_group = false, string p_drone_guid = null, int p_timeout = -1, bool p_collectable = false)
		{
			DRLLeaderboardData dRLLeaderboardData = new DRLLeaderboardData();
			dRLLeaderboardData.page = p_page;
			dRLLeaderboardData.limit = p_limit;
			dRLLeaderboardData.gameType = (p_campaign ? GameFlag.Campaign.ToString() : GameFlag.Race.ToString());
			if (p_collectable)
			{
				dRLLeaderboardData.gameType = "Collectable";
			}
			dRLLeaderboardData.scoreTypeFlag = ScoreType.TimeMin;
			dRLLeaderboardData.diameter = p_drone_class;
			if (dRLLeaderboardData.diameter < 0)
			{
				dRLLeaderboardData.Remove("diameter");
			}
			if (string.IsNullOrEmpty(p_platform))
			{
				dRLLeaderboardData.Remove("profile-platform");
			}
			else
			{
				dRLLeaderboardData.platform = p_platform;
			}
			if (string.IsNullOrEmpty(p_controller_type))
			{
				dRLLeaderboardData.Remove("controller-type");
			}
			else
			{
				dRLLeaderboardData.controllerType = p_controller_type;
			}
			if (string.IsNullOrEmpty(p_drone_guid))
			{
				dRLLeaderboardData.Remove("drone-guid");
			}
			else
			{
				dRLLeaderboardData.droneGuid = p_drone_guid;
			}
			if ((bool)p_map)
			{
				dRLLeaderboardData.map = p_map.guid;
			}
			if ((bool)p_track)
			{
				dRLLeaderboardData.track = p_track.guid;
			}
			AssertCommunityMap(dRLLeaderboardData, p_map, p_track);
			if ((bool)p_campaign)
			{
				dRLLeaderboardData.group = p_campaign.guid;
			}
			if (!string.IsNullOrEmpty(p_region))
			{
				dRLLeaderboardData.region = p_region;
			}
			if (p_drone_official.HasValue)
			{
				AssertCustomFlags(dRLLeaderboardData, p_drone_official.Value, p_physics);
			}
			else if (!p_collectable)
			{
				dRLLeaderboardData.Remove("drl-official");
				dRLLeaderboardData.customPhysics = p_physics != 0;
			}
			return GetLeaderboard(dRLLeaderboardData, p_callback, p_group, p_timeout, p_collectable);
		}

		public WebAsyncRequest GetLeaderboard(string p_community_map, int p_page, int p_limit, int p_drone_class, bool? p_drone_official, int p_physics, Action<DRLLeaderboardResult> p_callback, string p_platform = null, string p_controller_type = null, bool p_group = false, string p_drone_guid = null, int p_timeout = -1, GameFlag p_game_type = GameFlag.Race, bool p_collectable = false)
		{
			DRLLeaderboardData dRLLeaderboardData = new DRLLeaderboardData();
			dRLLeaderboardData.page = p_page;
			dRLLeaderboardData.limit = p_limit;
			dRLLeaderboardData.gameType = p_game_type.ToString();
			dRLLeaderboardData.scoreTypeFlag = ScoreType.TimeMin;
			dRLLeaderboardData.diameter = p_drone_class;
			if (dRLLeaderboardData.diameter < 0)
			{
				dRLLeaderboardData.Remove("diameter");
			}
			if (string.IsNullOrEmpty(p_platform))
			{
				dRLLeaderboardData.Remove("profile-platform");
			}
			else
			{
				dRLLeaderboardData.platform = p_platform;
			}
			if (string.IsNullOrEmpty(p_controller_type))
			{
				dRLLeaderboardData.Remove("controller-type");
			}
			else
			{
				dRLLeaderboardData.controllerType = p_controller_type;
			}
			if (string.IsNullOrEmpty(p_drone_guid))
			{
				dRLLeaderboardData.Remove("drone-guid");
			}
			else
			{
				dRLLeaderboardData.droneGuid = p_drone_guid;
			}
			dRLLeaderboardData.customMap = p_community_map;
			dRLLeaderboardData.isCustomMap = true;
			if (p_collectable)
			{
				dRLLeaderboardData.gameType = "Collectable";
			}
			if (p_drone_official.HasValue)
			{
				AssertCustomFlags(dRLLeaderboardData, p_drone_official.Value, p_physics);
			}
			else if (!p_collectable)
			{
				dRLLeaderboardData.Remove("drl-official");
				dRLLeaderboardData.customPhysics = p_physics != 0;
			}
			return GetLeaderboard(dRLLeaderboardData, p_callback, p_group, p_timeout);
		}

		public WebAsyncRequest GetLeaderboardCircuit(string p_circuitId, int p_page, int p_limit, int p_drone_class, bool? p_drone_official, int p_physics, Action<DRLCircuitsResult> p_callback, string p_platform = null, string p_controller_type = null, string p_drone_guid = null, int p_timeout = -1)
		{
			DRLCircuitLeaderboardData dRLCircuitLeaderboardData = new DRLCircuitLeaderboardData();
			dRLCircuitLeaderboardData.page = p_page;
			dRLCircuitLeaderboardData.limit = p_limit;
			dRLCircuitLeaderboardData.circuitId = p_circuitId;
			dRLCircuitLeaderboardData.diameter = p_drone_class;
			if (dRLCircuitLeaderboardData.diameter < 0)
			{
				dRLCircuitLeaderboardData.Remove("diameter");
			}
			if (string.IsNullOrEmpty(p_platform))
			{
				dRLCircuitLeaderboardData.Remove("profile-platform");
			}
			else
			{
				dRLCircuitLeaderboardData.platform = p_platform;
			}
			if (string.IsNullOrEmpty(p_controller_type))
			{
				dRLCircuitLeaderboardData.Remove("controller-type");
			}
			else
			{
				dRLCircuitLeaderboardData.controllerType = p_controller_type;
			}
			if (string.IsNullOrEmpty(p_drone_guid))
			{
				dRLCircuitLeaderboardData.Remove("drone-guid");
			}
			else
			{
				dRLCircuitLeaderboardData.droneGuid = p_drone_guid;
			}
			if (p_drone_official.HasValue)
			{
				AssertCustomFlags(dRLCircuitLeaderboardData, p_drone_official.Value, p_physics);
			}
			else
			{
				dRLCircuitLeaderboardData.Remove("drl-official");
				dRLCircuitLeaderboardData.customPhysics = p_physics != 0;
			}
			return backend.GetLeaderboardCircuit(dRLCircuitLeaderboardData, p_callback, p_timeout);
		}

		public WebAsyncRequest GetLeaderboard(DRLMap p_map, DRLMapTrack p_track, int p_page, int p_limit, int p_drone_class, bool? p_drone_official, int p_physics, Action<DRLLeaderboardResult> p_callback, string p_platform = null, string p_controller_type = null, bool p_group = false, string p_drone_guid = null, int p_timeout = -1, bool p_collectable = false)
		{
			return GetLeaderboard(null, p_map, p_track, "", p_page, p_limit, p_drone_class, p_drone_official, p_physics, p_callback, p_platform, p_controller_type, p_group, p_drone_guid, p_timeout, p_collectable);
		}

		public WebAsyncRequest GetLeaderboard(DRLCampaign p_campaign, string p_region, int p_page, int p_limit, int p_drone_class, Action<DRLLeaderboardResult> p_callback, bool p_group = false, string p_drone_guid = null, int p_timeout = -1)
		{
			return GetLeaderboard(p_campaign, null, null, p_region, p_page, p_limit, p_drone_class, false, -1, p_callback, null, null, p_group, p_drone_guid, p_timeout);
		}

		public WebAsyncRequest GetLeaderboard(DRLCampaign p_campaign, int p_page, int p_limit, int p_drone_class, Action<DRLLeaderboardResult> p_callback, bool p_group = false, string p_drone_guid = null, int p_timeout = -1)
		{
			return GetLeaderboard(p_campaign, null, null, "", p_page, p_limit, p_drone_class, false, -1, p_callback, null, null, p_group, p_drone_guid, p_timeout);
		}

		public WebAsyncRequest GetLeaderboard(MapData p_custom_map, int p_page, int p_limit, int p_drone_class, bool p_drone_official, int p_physics, Action<DRLLeaderboardResult> p_callback, bool p_group = false, int p_timeout = -1)
		{
			DRLLeaderboardData dRLLeaderboardData = new DRLLeaderboardData();
			dRLLeaderboardData.page = p_page;
			dRLLeaderboardData.limit = p_limit;
			dRLLeaderboardData.gameType = GameFlag.Race.ToString();
			dRLLeaderboardData.scoreTypeFlag = ScoreType.TimeMin;
			dRLLeaderboardData.diameter = p_drone_class;
			if (dRLLeaderboardData.diameter < 0)
			{
				dRLLeaderboardData.Remove("diameter");
			}
			dRLLeaderboardData.isCustomMap = true;
			dRLLeaderboardData.customMap = p_custom_map.guid;
			dRLLeaderboardData.Remove("drone-guid");
			AssertCustomFlags(dRLLeaderboardData, p_drone_official, p_physics);
			return GetLeaderboard(dRLLeaderboardData, p_callback, p_group, p_timeout);
		}

		public WebAsyncRequest GetLeaderboard(DRLTournamentLeaderboardParams p_leaderboardParams, int p_page, int p_limit, Action<DRLLeaderboardResult> p_callback, int p_timeout = -1)
		{
			return backend.GetLeaderboard(p_leaderboardParams, p_page, p_limit, p_callback, p_timeout);
		}

		public WebAsyncRequest GetLeaderboardQuest(List<DRLMission> p_missions, Action<DRLLeaderboardData[]> p_callback, int p_timeout = -1)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < p_missions.Count; i++)
			{
				list.Add(p_missions[i].guid);
			}
			return backend.GetLeaderboardsQuest(list, p_callback, p_timeout);
		}

		public WebAsyncRequest GetLeaderboardQuest(DRLMission p_mission, Action<DRLLeaderboardData[]> p_callback, int p_timeout = -1)
		{
			return GetLeaderboardQuest(new List<DRLMission>(new DRLMission[1] { p_mission }), p_callback, p_timeout);
		}

		public WebAsyncRequest GetLeaderboardQuest(DRLQuest p_quest, Action<DRLLeaderboardData[]> p_callback, int p_timeout = -1)
		{
			return GetLeaderboardQuest(p_quest.missions, p_callback, p_timeout);
		}

		public void PopulateLeaderboardQuest(List<DRLMission> p_missions, List<Component> p_targets, bool p_clear = true, float p_delay_step = 0.1f)
		{
			List<UICardButtonMission> mcl = new List<UICardButtonMission>();
			List<UICardButtonQuest> qcl = new List<UICardButtonQuest>();
			List<UICardButtonLesson> lcl = new List<UICardButtonLesson>();
			List<UICardButtonDmvTest> tcl = new List<UICardButtonDmvTest>();
			for (int i = 0; i < p_targets.Count; i++)
			{
				Component component = p_targets[i];
				if (component is UICardButtonMission)
				{
					mcl.Add(component as UICardButtonMission);
				}
				if (component is UICardButtonQuest)
				{
					qcl.Add(component as UICardButtonQuest);
				}
				if (component is UICardButtonLesson)
				{
					lcl.Add(component as UICardButtonLesson);
				}
				if (component is UICardButtonDmvTest)
				{
					tcl.Add(component as UICardButtonDmvTest);
				}
			}
			GetLeaderboardQuest(p_missions, delegate(DRLLeaderboardData[] p_result)
			{
				string text = "ServiceModel> PopulateLeaderboardQuest - length[" + p_result.Length + "]";
				List<DRLLeaderboardData> list = new List<DRLLeaderboardData>();
				list.AddRange(p_result);
				if (mcl.Count > 0)
				{
					if (list.Count >= 2)
					{
						for (int j = 0; j < list.Count; j++)
						{
							for (int k = j + 1; k < list.Count; k++)
							{
								DRLLeaderboardData dRLLeaderboardData = list[j];
								DRLLeaderboardData dRLLeaderboardData2 = list[k];
								if (!(dRLLeaderboardData.mission != dRLLeaderboardData2.mission))
								{
									if (dRLLeaderboardData.score > dRLLeaderboardData2.score)
									{
										list.RemoveAt(k--);
									}
									if (dRLLeaderboardData.score <= dRLLeaderboardData2.score)
									{
										list.RemoveAt(j);
									}
								}
							}
						}
					}
					for (int l = 0; l < mcl.Count; l++)
					{
						if ((bool)mcl[l].stars && (bool)mcl[l].stars.fade)
						{
							mcl[l].stars.fade.FadeIn(0.25f, 0.5f);
						}
					}
					for (int m = 0; m < qcl.Count; m++)
					{
						if ((bool)qcl[m].stars && (bool)qcl[m].stars.fade)
						{
							qcl[m].stars.fade.FadeIn(0.25f, 0.5f);
						}
					}
					float num = 5f;
					float num2 = 0f;
					float num3 = p_result.Length;
					for (int n = 0; n < list.Count; n++)
					{
						DRLLeaderboardData dRLLeaderboardData3 = list[n];
						float num4 = Mathf.Clamp01((float)dRLLeaderboardData3.score / 1000f);
						num2 += num4;
						for (int num5 = 0; num5 < mcl.Count; num5++)
						{
							if ((bool)mcl[num5].data && !(mcl[num5].data.guid != dRLLeaderboardData3.mission) && (bool)mcl[num5].stars)
							{
								text = text + "\nMission " + num5 + "> " + mcl[num5].data.title.Replace("\n", "") + " score[" + num4 + "]";
								mcl[num5].stars.FadeProgress(num4 * num, 0.8f, p_delay_step, p_clear);
							}
						}
					}
					float num6 = ((num3 <= 0f) ? 1f : (num2 / num3));
					for (int num7 = 0; num7 < qcl.Count; num7++)
					{
						if ((bool)qcl[num7].stars)
						{
							_ = qcl[num7].data;
							text = text + "\nQuest> " + qcl[num7].data.title.Replace("\n", "") + " score[" + num6 + "]";
							qcl[num7].stars.SetProgress(num6 * num);
						}
					}
					Debug.Log(text);
				}
				if (lcl.Count > 0)
				{
					for (int num8 = 0; num8 < list.Count; num8++)
					{
						DRLLeaderboardData dRLLeaderboardData4 = list[num8];
						float score = (float)dRLLeaderboardData4.score / 1000f;
						for (int num9 = 0; num9 < lcl.Count; num9++)
						{
							if ((bool)lcl[num9].data && !(lcl[num9].data.guid != dRLLeaderboardData4.mission))
							{
								lcl[num9].SetScore(score);
								text = text + "\n Lesson > " + lcl[num9].data.name + " score[" + score + "]";
							}
						}
					}
					Debug.Log(text);
					Notify("missions.scoring.set");
				}
				if (tcl.Count > 0)
				{
					if (list.Count > 0)
					{
						int num10 = 0;
						for (int num11 = 0; num11 < tcl.Count; num11++)
						{
							for (int num12 = 0; num12 < list.Count; num12++)
							{
								DRLLeaderboardData dRLLeaderboardData5 = list[num12];
								if ((bool)tcl[num11].data)
								{
									if (tcl[num11].data.testMission == null)
									{
										break;
									}
									if (!(tcl[num11].data.testMission.guid != dRLLeaderboardData5.mission))
									{
										float num13 = Mathf.Clamp01((float)dRLLeaderboardData5.score / 1000f);
										tcl[num11].SetScore(num13);
										text = text + "\n Test > " + dRLLeaderboardData5.mission + " score[" + num13 + "]";
										if (num13 > (float)MissionController.passingScore / 100f)
										{
											num10++;
											tcl[num11].MarkComplete();
										}
										else if (dRLLeaderboardData5.crashCount > 0 && dRLLeaderboardData5.crashCount % MissionController.testAttempts == 0)
										{
											tcl[num11].MarkComplete(p_active: false);
											float num14 = 172800f - (float)DateTime.UtcNow.Subtract(dRLLeaderboardData5.createDate).TotalSeconds;
											if (num14 > 0f)
											{
												tcl[num11].LockCard(num14);
											}
										}
										break;
									}
								}
							}
						}
						Debug.Log(text);
						PlayerStateModel player = base.app.model.storage.state.player;
						if (player.userRank < num10)
						{
							player.userRank = num10;
						}
						Notify("missions.dmv.total-progress", num10);
					}
					tcl.Clear();
				}
			});
		}

		public void PopulateLeaderboardQuest(DRLMission p_mission, List<Component> p_targets, float p_delay_step = 0.1f)
		{
			PopulateLeaderboardQuest(new List<DRLMission>(new DRLMission[1] { p_mission }), p_targets, p_clear: true, p_delay_step);
		}

		public WebAsyncRequest ResetLeaderboardQuest(List<DRLMission> p_missions, Action<DRLLeaderboardData[]> p_callback, int p_timeout = -1)
		{
			List<string> list = new List<string>();
			foreach (DRLMission p_mission in p_missions)
			{
				list.Add(p_mission.guid);
			}
			return backend.ResetLeaderboardsQuest(list, p_callback, p_timeout);
		}

		public WebAsyncRequest ResetLeaderboardUser(string p_playerID, int p_timeout = -1)
		{
			return backend.ResetLeaderboardUser(p_playerID, p_timeout);
		}

		public WebAsyncRequest ResetTrackLeaderboardUser(string p_mapID, string p_trackID, string p_customMapID, bool p_isCustom, int p_timeout = -1)
		{
			return backend.ResetTrackLeaderboardUser(p_mapID, p_trackID, p_customMapID, p_isCustom, p_timeout);
		}

		public WebAsyncRequest SetLeaderboard(DRLLeaderboardData p_data, Action<DRLLeaderboardData> p_callback, int p_timeout = -1, bool p_collectable = false)
		{
			return backend.SetLeaderboard(p_data, p_callback, p_timeout, p_collectable);
		}

		public WebAsyncRequest SetLeaderboard(DRLLeaderboardData[] p_data, Action<DRLLeaderboardData[]> p_callback, int p_timeout = -1, bool p_collectable = false)
		{
			return backend.SetLeaderboard(p_data, p_callback, p_timeout, p_collectable);
		}

		public WebAsyncRequest SetLeaderboard(DRLLeaderboardData[] p_data, Action<DRLLeaderboardData> p_callback, int p_timeout = -1)
		{
			return backend.SetLeaderboard(p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest SetLeaderboardCampaign(DRLCampaign p_campaign, bool p_force, int p_drone_class, Action<DRLLeaderboardData> p_callback, int p_timeout = -1)
		{
			List<DRLLeaderboardData> leaderboards = base.app.model.storage.state.player.results.campaign.GetLeaderboards(p_campaign);
			if (p_force)
			{
				for (int i = 0; i < leaderboards.Count; i++)
				{
					leaderboards[i].force = true;
					leaderboards[i].diameter = p_drone_class;
				}
			}
			return SetLeaderboard(leaderboards.ToArray(), p_callback);
		}

		public WebAsyncRequest SetLeaderboardCampaign(DRLCampaign p_campaign, int p_drone_class, Action<DRLLeaderboardData> p_callback, int p_timeout = -1)
		{
			return SetLeaderboardCampaign(p_campaign, p_force: false, p_drone_class, p_callback, p_timeout);
		}

		public WebAsyncRequest SetLeaderboardRace(int p_order, DRLMap p_map, DRLMapTrack p_track, float p_time, int p_crashes, int p_drone_class, bool p_force, Action<DRLLeaderboardData> p_callback, int p_timeout = -1)
		{
			DRLLeaderboardData dRLLeaderboardData = new DRLLeaderboardData();
			dRLLeaderboardData = CreateRaceLeaderboardData(p_order, p_time, p_crashes, p_track);
			dRLLeaderboardData.force = p_force;
			dRLLeaderboardData.diameter = p_drone_class;
			return SetLeaderboard(dRLLeaderboardData, p_callback, p_timeout);
		}

		public WebAsyncRequest SetLeaderboardRace(DRLMap p_map, DRLMapTrack p_track, float p_time, int p_crashes, int p_drone_class, Action<DRLLeaderboardData> p_callback, int p_timeout = -1)
		{
			return SetLeaderboardRace(0, p_map, p_track, p_time, p_crashes, p_drone_class, p_force: false, p_callback, p_timeout);
		}

		public WebAsyncRequest SetLeaderboardRace(int p_order, DRLMap p_map, DRLMapTrack p_track, float p_time, int p_crashes, int p_drone_class, Action<DRLLeaderboardData> p_callback, int p_timeout = -1)
		{
			return SetLeaderboardRace(p_order, p_map, p_track, p_time, p_crashes, p_drone_class, p_force: false, p_callback, p_timeout);
		}

		public WebAsyncRequest SetLeaderboardMission(DRLMission p_mission, float p_score, bool p_force, Action<DRLLeaderboardData> p_callback, int p_crashes, int p_timeout = -1)
		{
			DRLLeaderboardData dRLLeaderboardData = new DRLLeaderboardData();
			dRLLeaderboardData = CreateMissionLeaderboardData(p_score, p_mission, p_crashes);
			dRLLeaderboardData.force = p_force;
			return SetLeaderboard(dRLLeaderboardData, p_callback, p_timeout);
		}

		public WebAsyncRequest SetLeaderboardCircuit(DRLCircuitLeaderboardData p_data, Action<bool> p_on_complete = null, int p_timeout = -1)
		{
			return backend.SetLeaderboardCircuit(p_data, p_on_complete, p_timeout);
		}

		public void SyncOfflineLeaderboard(Action p_complete, int p_timeout = -1)
		{
			base.app.model.storage.leaderboards.Load(delegate(List<DRLLeaderboardData> p_results)
			{
				if ((p_results == null || p_results.Count == 0) && p_complete != null)
				{
					p_complete();
				}
				else
				{
					SetLeaderboard(p_results.ToArray(), delegate(DRLLeaderboardData[] p_result)
					{
						if ((p_result == null || p_result.Length == 0) && p_complete != null)
						{
							p_complete();
						}
						else
						{
							base.app.model.storage.leaderboards.Clear();
							if (p_complete != null)
							{
								p_complete();
							}
						}
					});
				}
			});
		}

		public WebAsyncRequest SetCommunityTunes(DRLCommunityTuneData p_query, Action<DRLCommunityTuneData> p_callback, int p_timeout = -1)
		{
			return backend.SetCommunityTunes(p_query, p_callback, p_timeout);
		}

		public WebAsyncRequest GetCommunityTunes(DRLCommunityTuneData p_query, Action<DRLCommunityTuneResult> p_callback, int p_timeout = -1)
		{
			return backend.GetCommunityTunes(p_query, p_callback, p_timeout);
		}

		public WebAsyncRequest GetCommunityTunes(string p_player_id, int p_page, int p_limit, SortType p_sort, string p_search, Action<DRLCommunityTuneResult> p_callback, int p_timeout = -1)
		{
			DRLCommunityTuneData dRLCommunityTuneData = new DRLCommunityTuneData();
			dRLCommunityTuneData.Remove("guid");
			if (!string.IsNullOrEmpty(p_player_id))
			{
				dRLCommunityTuneData.playerId = p_player_id;
			}
			else
			{
				dRLCommunityTuneData.Remove(DRLService.PlatformIdKey);
			}
			dRLCommunityTuneData.page = p_page + 1;
			dRLCommunityTuneData.limit = p_limit;
			dRLCommunityTuneData.sort = p_sort;
			if (!string.IsNullOrEmpty(p_search))
			{
				dRLCommunityTuneData["q"] = p_search;
			}
			return GetCommunityTunes(dRLCommunityTuneData, p_callback, p_timeout);
		}

		public WebAsyncRequest GetCommunityTunes(string p_player_id, int p_page, int p_limit, SortType p_sort, Action<DRLCommunityTuneResult> p_callback, int p_timeout = -1)
		{
			return GetCommunityTunes(p_player_id, p_page, p_limit, p_sort, "", p_callback, p_timeout);
		}

		public WebAsyncRequest GetCommunityTunes(int p_page, int p_limit, SortType p_sort, Action<DRLCommunityTuneResult> p_callback, int p_timeout = -1)
		{
			return GetCommunityTunes("", p_page, p_limit, p_sort, p_callback, p_timeout);
		}

		public WebAsyncRequest GetCommunityTunes(int p_page, int p_limit, Action<DRLCommunityTuneResult> p_callback, int p_timeout = -1)
		{
			return GetCommunityTunes(p_page, p_limit, SortType.ScoreDesc, p_callback, p_timeout);
		}

		public WebAsyncRequest GetCommunityTune(string p_guid, Action<DRLCommunityTuneResult> p_callback, int p_timeout = -1)
		{
			DRLCommunityTuneData dRLCommunityTuneData = new DRLCommunityTuneData();
			dRLCommunityTuneData.guid = p_guid;
			return GetCommunityTunes(dRLCommunityTuneData, p_callback, p_timeout);
		}

		public WebAsyncRequest SetCommunityTuneRating(string p_guid, float p_score, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return backend.SetCommunityTuneRating(p_guid, p_score, p_callback, p_timeout);
		}

		public WebAsyncRequest SetCommunityTuneRating(DRLCommunityTuneData p_tune, float p_score, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string p_guid = ((p_tune == null) ? "" : p_tune.guid);
			return SetCommunityTuneRating(p_guid, p_score, p_callback, p_timeout);
		}

		public WebAsyncRequest RemoveCommunityTune(string p_guid, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return backend.RemoveCommunityTune(p_guid, p_callback, p_timeout);
		}

		public WebAsyncRequest GetCommunityDrones(DRLCommunityDroneData p_query, Action<DRLCommunityDroneResult> p_callback, int p_timeout = -1)
		{
			return backend.GetCommunityDrones(p_query, p_callback, p_timeout);
		}

		public WebAsyncRequest GetCommunityDrones(string p_player_id, string p_guid, int p_page, int p_limit, int p_class, int p_physics, DRLCommunityDroneData.SortType p_sort, string p_search, Action<DRLCommunityDroneResult> p_callback, int p_timeout = -1)
		{
			DRLCommunityDroneData dRLCommunityDroneData = new DRLCommunityDroneData();
			if (!string.IsNullOrEmpty(p_guid))
			{
				dRLCommunityDroneData.guid = p_guid;
			}
			else
			{
				dRLCommunityDroneData.Remove("guid");
			}
			if (!string.IsNullOrEmpty(p_player_id))
			{
				dRLCommunityDroneData.playerId = p_player_id;
				dRLCommunityDroneData.Remove("is-public");
			}
			else
			{
				dRLCommunityDroneData.Remove("player-id");
				dRLCommunityDroneData.isPublic = true;
			}
			if (p_class > 0)
			{
				dRLCommunityDroneData.droneSize = p_class;
			}
			else
			{
				dRLCommunityDroneData.Remove("size");
			}
			if (p_physics > -1)
			{
				dRLCommunityDroneData.isCustomPhysics = p_physics == 1;
			}
			else
			{
				dRLCommunityDroneData.Remove("is-custom-physics");
			}
			if (p_page < 0)
			{
				dRLCommunityDroneData.Remove("page");
			}
			else
			{
				dRLCommunityDroneData.page = p_page + 1;
			}
			if (p_limit < 0)
			{
				dRLCommunityDroneData.Remove("limit");
			}
			else
			{
				dRLCommunityDroneData.limit = p_limit;
			}
			dRLCommunityDroneData.sort = p_sort;
			if (!string.IsNullOrEmpty(p_search))
			{
				dRLCommunityDroneData["q"] = p_search;
			}
			return GetCommunityDrones(dRLCommunityDroneData, p_callback, p_timeout);
		}

		public WebAsyncRequest GetCommunityDrones(string p_guid, Action<DRLCommunityDroneResult> p_callback, int p_timeout = -1)
		{
			return GetCommunityDrones("", p_guid, -1, -1, -1, -1, DRLCommunityDroneData.SortType.None, null, p_callback, p_timeout);
		}

		public WebAsyncRequest GetCommunityDrone(string p_guid, Action<DRLCommunityDroneData> p_callback, int p_timeout = -1)
		{
			return GetCommunityDrones("", p_guid, -1, -1, -1, -1, DRLCommunityDroneData.SortType.None, null, delegate(DRLCommunityDroneResult p_result)
			{
				if (!(this == null) && p_callback != null)
				{
					DRLCommunityDroneData obj = null;
					if (p_result != null && p_result.data != null && p_result.data.Length != 0)
					{
						obj = p_result.data[0];
					}
					p_callback(obj);
				}
			}, p_timeout);
		}

		public WebAsyncRequest GetCommunityDroneRating(string p_guid, Action<float> p_callback, int p_timeout = -1)
		{
			return GetCommunityDrone(p_guid, delegate(DRLCommunityDroneData p_result)
			{
				if (!(this == null) && p_callback != null)
				{
					if (p_result != null)
					{
						p_callback(p_result.rating);
					}
					else
					{
						p_callback(-1f);
					}
				}
			}, p_timeout);
		}

		public WebAsyncRequest GetCommunityDroneScore(string p_guid, Action<float> p_callback, int p_timeout = -1)
		{
			return GetCommunityDrone(p_guid, delegate(DRLCommunityDroneData p_result)
			{
				if (!(this == null) && p_callback != null)
				{
					if (p_result != null)
					{
						p_callback(p_result.score);
					}
					else
					{
						p_callback(-1f);
					}
				}
			}, p_timeout);
		}

		public WebAsyncRequest CloneCommunityDrone(string p_guid, string p_suffix, Action<DRLCommunityDroneData> p_callback, int p_timeout = -1)
		{
			return GetCommunityDrones(p_guid, delegate(DRLCommunityDroneResult p_result)
			{
				DRLCommunityDroneData dRLCommunityDroneData = ((p_result.data.Length == 0) ? null : p_result.data[0]);
				if (dRLCommunityDroneData == null)
				{
					Debug.LogWarning("ServiceModel> CloneCommunityDrone / Failed to Find DRLCommunityDroneData - guid[" + p_guid + "]");
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else if (DroneRigData.FromJson(dRLCommunityDroneData.droneRigData) == null)
				{
					Debug.LogWarning("ServiceModel> CloneCommunityDrone / Failed to Parse DroneRigData - guid[" + p_guid + "]");
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					dRLCommunityDroneData.playerId = "";
					dRLCommunityDroneData.guid = DroneRigData.GenerateGUID();
					dRLCommunityDroneData.droneName += p_suffix;
					SetCommunityDrones(dRLCommunityDroneData, p_callback);
				}
			}, p_timeout);
		}

		public WebAsyncRequest SetCommunityDrones(DRLCommunityDroneData p_data, Action<DRLCommunityDroneData> p_callback, int p_timeout = -1)
		{
			return backend.SetCommunityDrones(p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest SetCommunityDrones(DroneRigData p_data, Action<DRLCommunityDroneData> p_callback, int p_timeout = -1)
		{
			DRLCommunityDroneData dRLCommunityDroneData = new DRLCommunityDroneData();
			dRLCommunityDroneData.Clear();
			if (p_data != null)
			{
				dRLCommunityDroneData.guid = p_data.guid;
				dRLCommunityDroneData.droneThumbURL = p_data.thumb1;
				dRLCommunityDroneData.droneName = p_data.name;
				dRLCommunityDroneData.isDroneOfficial = false;
				dRLCommunityDroneData.isCustomPhysics = p_data.hasCustomPhysics;
				dRLCommunityDroneData.droneSize = p_data.diameter;
				dRLCommunityDroneData.isPublic = false;
				dRLCommunityDroneData.droneFrameId = p_data.frame;
				dRLCommunityDroneData.droneMotorId = p_data.motor;
				dRLCommunityDroneData.dronePropId = p_data.prop;
				dRLCommunityDroneData.droneBatteryId = p_data.battery;
				dRLCommunityDroneData.droneRigData = p_data.ToJson();
				dRLCommunityDroneData.dronePhysicsData = p_data.tune;
				dRLCommunityDroneData.droneProfileData = p_data.profile;
			}
			return SetCommunityDrones(dRLCommunityDroneData, p_callback, p_timeout);
		}

		public WebAsyncRequest SetCommunityDroneRating(string p_guid, float p_score, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return backend.SetCommunityDroneRating(p_guid, p_score, p_callback, p_timeout);
		}

		public WebAsyncRequest SetCommunityDroneRating(DroneRigData p_data, float p_score, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string p_guid = ((p_data == null) ? "" : p_data.guid);
			return SetCommunityDroneRating(p_guid, p_score, p_callback, p_timeout);
		}

		public WebAsyncRequest SetCommunityDroneTime(string p_guid, float p_time, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return backend.SetCommunityDroneTime(p_guid, null, null, null, null, p_time, p_callback, p_timeout);
		}

		public WebAsyncRequest GetCommunityDroneTime(string p_guid, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return backend.GetCommunityDroneTime(p_guid, p_callback, p_timeout);
		}

		public WebAsyncRequest SetCommunityDroneTime(string p_guid, string p_map, string p_track, string p_communityMap, string p_gameType, float p_time, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return backend.SetCommunityDroneTime(p_guid, p_map, p_track, p_communityMap, p_gameType, p_time, p_callback, p_timeout);
		}

		public WebAsyncRequest SetCommunityDroneTime(DroneRigData p_data, float p_time, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string p_guid = ((p_data == null) ? "" : p_data.guid);
			return SetCommunityDroneRating(p_guid, p_time, p_callback, p_timeout);
		}

		public WebAsyncRequest GetThrottleCap(Action<DRLServiceResult> p_callback, string p_raceID = null, int p_timeout = -1)
		{
			return backend.GetThrottleCap(p_callback, p_raceID, p_timeout);
		}

		public WebAsyncRequest GetCrashSettings(Action<DRLCrashPenaltyData> p_callback, int p_timeout = -1)
		{
			return backend.GetCrashSettings(p_callback, p_timeout);
		}

		public WebAsyncRequest RemoveCommunityDrones(string p_guid, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return backend.RemoveCommunityDrones(p_guid, p_callback, p_timeout);
		}

		public WebAsyncRequest GetCommunityMaps(DRLCommunityMapData p_query, Action<DRLCommunityMapResult> p_callback, int p_timeout = -1, bool p_isCollectable = false)
		{
			return backend.GetCommunityMaps(p_query, p_callback, p_timeout, p_isCollectable);
		}

		public WebAsyncRequest GetCommunityMaps(string p_player_id, int p_map_difficulty, int p_allow_race, string p_map_id, int p_page, int p_limit, GameFlag p_category, SortType p_sort, string p_search, Action<DRLCommunityMapResult> p_callback, int p_timeout = -1)
		{
			Debug.Log("ServiceModel> GetCommunityMaps: " + p_category);
			DRLCommunityMapData dRLCommunityMapData = new DRLCommunityMapData();
			dRLCommunityMapData.Remove("guid");
			dRLCommunityMapData.Remove("player-id");
			bool p_isCollectable = false;
			if (p_category == GameFlag.Collectable)
			{
				dRLCommunityMapData.Add("game-type", "Collectable");
				dRLCommunityMapData.Add("map-category", "MapCommon");
				p_isCollectable = true;
			}
			if (p_category == GameFlag.All)
			{
				dRLCommunityMapData.Add("game-type", "All");
			}
			if (!string.IsNullOrEmpty(p_player_id))
			{
				dRLCommunityMapData.playerId = p_player_id;
			}
			dRLCommunityMapData.page = p_page + 1;
			dRLCommunityMapData.limit = p_limit;
			dRLCommunityMapData.sort = p_sort;
			if (p_allow_race >= 0)
			{
				dRLCommunityMapData.isRaceAllowed = p_allow_race != 0;
			}
			if (p_map_difficulty >= 0)
			{
				dRLCommunityMapData.mapDifficulty = p_map_difficulty;
			}
			if (!string.IsNullOrEmpty(p_map_id))
			{
				dRLCommunityMapData.mapId = p_map_id;
			}
			if (!string.IsNullOrEmpty(p_search))
			{
				dRLCommunityMapData["q"] = p_search;
			}
			return GetCommunityMaps(dRLCommunityMapData, p_callback, p_timeout, p_isCollectable);
		}

		public WebAsyncRequest GetCommunityMaps(string p_player_id, int p_map_difficulty, int p_allow_race, string p_map_id, int p_page, int p_limit, SortType p_sort, string p_search, Action<DRLCommunityMapResult> p_callback, int p_timeout = -1)
		{
			return GetCommunityMaps(p_player_id, -1, p_allow_race, p_map_id, p_page, p_limit, GameFlag.None, p_sort, p_search, p_callback, p_timeout);
		}

		public WebAsyncRequest GetCommunityMaps(string p_player_id, int p_allow_race, string p_map_id, int p_page, int p_limit, SortType p_sort, string p_search, Action<DRLCommunityMapResult> p_callback, int p_timeout = -1)
		{
			return GetCommunityMaps(p_player_id, -1, p_allow_race, p_map_id, p_page, p_limit, p_sort, p_search, p_callback, p_timeout);
		}

		public WebAsyncRequest GetCommunityMaps(string p_player_id, string p_map_id, int p_page, int p_limit, SortType p_sort, string p_search, Action<DRLCommunityMapResult> p_callback, int p_timeout = -1)
		{
			return GetCommunityMaps(p_player_id, -1, -1, p_map_id, p_page, p_limit, p_sort, p_search, p_callback, p_timeout);
		}

		public WebAsyncRequest GetCommunityMaps(string p_player_id, int p_page, int p_limit, SortType p_sort, string p_search, Action<DRLCommunityMapResult> p_callback, int p_timeout = -1)
		{
			return GetCommunityMaps(p_player_id, -1, -1, "", p_page, p_limit, p_sort, p_search, p_callback, p_timeout);
		}

		public WebAsyncRequest GetCommunityMaps(int p_map_difficulty, string p_map_id, int p_page, int p_limit, SortType p_sort, string p_search, Action<DRLCommunityMapResult> p_callback, int p_timeout = -1)
		{
			return GetCommunityMaps("", p_map_difficulty, p_map_id, p_page, p_limit, p_sort, p_search, p_callback);
		}

		public WebAsyncRequest GetCommunityMaps(int p_page, int p_limit, Action<DRLCommunityMapResult> p_callback, int p_timeout = -1)
		{
			return GetCommunityMaps(-1, "", p_page, p_limit, SortType.ScoreDesc, "", p_callback, p_timeout);
		}

		public WebAsyncRequest GetCommunityMaps(string p_guid, bool p_has_root, Action<DRLCommunityMapResult> p_callback, int p_timeout = -1, bool p_isCollectable = false)
		{
			DRLCommunityMapData dRLCommunityMapData = new DRLCommunityMapData();
			dRLCommunityMapData.Remove("guid");
			dRLCommunityMapData["guid"] = p_guid;
			return backend.GetCommunityMaps(dRLCommunityMapData, p_has_root, p_callback, p_timeout, p_isCollectable);
		}

		public WebAsyncRequest GetCommunityMap(string p_guid, Action<DRLCommunityMapResult> p_callback, int p_timeout = -1)
		{
			DRLCommunityMapData dRLCommunityMapData = new DRLCommunityMapData();
			dRLCommunityMapData.Remove("guid");
			dRLCommunityMapData["guid"] = p_guid;
			return backend.GetCommunityMap(dRLCommunityMapData, p_callback, p_timeout);
		}

		public WebAsyncRequest GetSDMaps(bool p_has_root, Action<DRLCommunityMapResult> p_callback, int p_timeout = -1)
		{
			DRLCommunityMapData p_query = new DRLCommunityMapData();
			return backend.GetSDMaps(p_query, p_has_root, p_callback, p_timeout);
		}

		public WebAsyncRequest GetCommunityMaps(string p_guid, Action<DRLCommunityMapResult> p_callback, int p_timeout = -1, bool p_isCollectable = false)
		{
			return GetCommunityMaps(p_guid, p_has_root: false, p_callback, p_timeout, p_isCollectable);
		}

		public WebAsyncRequest GetCommunityMapRating(string p_guid, Action<float> p_callback, int p_timeout = -1, bool p_isCollectable = false)
		{
			return GetCommunityMaps(p_guid, p_has_root: false, delegate(DRLCommunityMapResult p_result)
			{
				if (!(this == null) && p_callback != null)
				{
					DRLCommunityMapData dRLCommunityMapData = null;
					if (p_result != null && p_result.data != null && p_result.data.Length != 0)
					{
						dRLCommunityMapData = p_result.data[0];
					}
					if (dRLCommunityMapData != null)
					{
						p_callback(dRLCommunityMapData.rating);
					}
					else
					{
						p_callback(-1f);
					}
				}
			}, p_timeout, p_isCollectable);
		}

		public WebAsyncRequest GetCommunityMapRating(string p_guid, Action<DRLServiceResult> p_callback, int p_timeout = -1, bool p_isCollectable = false)
		{
			return backend.GetCommunityMapRating(p_guid, p_callback, p_timeout, p_isCollectable);
		}

		public WebAsyncRequest GetCommunityMapScore(string p_guid, Action<float> p_callback, int p_timeout = -1, bool p_isCollectable = false)
		{
			return GetCommunityMaps(p_guid, p_has_root: false, delegate(DRLCommunityMapResult p_result)
			{
				if (!(this == null) && p_callback != null)
				{
					DRLCommunityMapData dRLCommunityMapData = null;
					if (p_result != null && p_result.data != null && p_result.data.Length != 0)
					{
						dRLCommunityMapData = p_result.data[0];
					}
					if (dRLCommunityMapData != null)
					{
						p_callback(dRLCommunityMapData.score);
					}
					else
					{
						p_callback(-1f);
					}
				}
			}, p_timeout, p_isCollectable);
		}

		public WebAsyncRequest UpdateLocalMaps(Action<MapData[]> p_callback, bool p_full = false, int p_timeout = -1)
		{
			return backend.UpdateLocalMaps(p_callback, p_full, p_community: false, p_timeout);
		}

		public WebAsyncRequest UpdateLocalCommunityMaps(Action<MapData[]> p_callback, bool p_full = false, int p_timeout = -1)
		{
			return backend.UpdateLocalMaps(p_callback, p_full, p_community: true, p_timeout);
		}

		public WebAsyncRequest DownloadMap(string p_path, string p_url, Action<string, string> p_onComplete, int p_timeout = -1)
		{
			return backend.DownloadMap(p_path, p_url, p_onComplete, p_timeout);
		}

		public void DownloadMapsToFiles(List<MapData> p_maps, string p_mapsRoot, Action<bool, float> p_callback)
		{
			if (p_maps == null || p_maps.Count == 0)
			{
				Debug.Log("ServiceModel> DownloadMapsToFiles - no new maps provided.");
				p_callback?.Invoke(arg1: true, 1f);
				return;
			}
			Dictionary<string, float> progress_dict = new Dictionary<string, float>();
			for (int i = 0; i < p_maps.Count; i++)
			{
				backend.DownloadMapToFile(p_maps[i].guid, p_mapsRoot + p_maps[i].guid + ".cmp", delegate(string guid, bool has_finished, float progress)
				{
					if (progress_dict.ContainsKey(guid))
					{
						progress_dict[guid] = progress;
					}
					else
					{
						progress_dict.Add(guid, progress);
					}
					float num = progress_dict.Values.Average();
					if (num == 1f)
					{
						p_callback?.Invoke(arg1: true, 1f);
					}
					else
					{
						p_callback?.Invoke(arg1: false, num);
					}
				});
			}
		}

		public WebAsyncRequest SyncLocalMapVersions(List<DRLCommunityMapVersionData> p_maps, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return backend.SyncLocalMapVersions(p_maps, p_callback, p_timeout);
		}

		public WebAsyncRequest CloneCommunityMap(string p_guid, Action<DRLCommunityMapData> p_callback, int p_timeout = -1)
		{
			return backend.CloneCommunityMap(p_guid, p_callback, p_timeout);
		}

		public WebAsyncRequest SetCommunityMaps(DRLCommunityMapData p_data, Action<DRLCommunityMapData> p_callback, int p_timeout = -1)
		{
			return backend.SetCommunityMaps(p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest SetCommunityMaps(string p_localFilepath, DRLCommunityMapData p_data, Action<string, DRLCommunityMapData> p_callback, int p_timeout = -1)
		{
			return backend.SetCommunityMaps(p_localFilepath, p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest SetCommunityMaps(MapData p_data, bool p_convert_root, Action<DRLCommunityMapData> p_callback, int p_timeout = -1)
		{
			DRLCommunityMapData dRLCommunityMapData = new DRLCommunityMapData();
			dRLCommunityMapData.Clear();
			if (p_data != null)
			{
				dRLCommunityMapData.Merge(p_data);
				if (p_convert_root)
				{
					dRLCommunityMapData.root = p_data.root.ToJson();
				}
			}
			return SetCommunityMaps(dRLCommunityMapData, p_callback, p_timeout);
		}

		public WebAsyncRequest SetCommunityMaps(string p_localFilepath, MapData p_data, bool p_convert_root, Action<string, DRLCommunityMapData> p_callback, int p_timeout = -1)
		{
			DRLCommunityMapData dRLCommunityMapData = new DRLCommunityMapData();
			dRLCommunityMapData.Clear();
			if (p_data != null)
			{
				dRLCommunityMapData.Merge(p_data);
				if (p_convert_root)
				{
					dRLCommunityMapData.root = p_data.root.ToJson();
				}
			}
			return SetCommunityMaps(p_localFilepath, dRLCommunityMapData, p_callback, p_timeout);
		}

		public WebAsyncRequest SetCommunityMaps(MapData p_data, Action<DRLCommunityMapData> p_callback, int p_timeout = -1)
		{
			return SetCommunityMaps(p_data, p_convert_root: true, p_callback, p_timeout);
		}

		public WebAsyncRequest SetCommunityMaps(string p_localFilepath, MapData p_data, Action<string, DRLCommunityMapData> p_callback, int p_timeout = -1)
		{
			return SetCommunityMaps(p_localFilepath, p_data, p_convert_root: true, p_callback, p_timeout);
		}

		public WebAsyncRequest SetCommunityMapRating(string p_guid, float p_score, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return backend.SetCommunityMapRating(p_guid, p_score, p_callback, p_timeout);
		}

		public WebAsyncRequest SetCommunityMapRating(DRLCommunityMapData p_data, float p_score, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string p_guid = ((p_data == null) ? "" : p_data.guid);
			return SetCommunityMapRating(p_guid, p_score, p_callback, p_timeout);
		}

		public WebAsyncRequest RemoveCommunityMaps(string p_guid, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return backend.RemoveCommunityMaps(p_guid, p_callback, p_timeout);
		}

		public void SyncOfflineMapEditorMaps()
		{
			MapsStorageModel maps = base.app.model.storage.maps;
			maps.GetMapEditorLocalMaps(delegate(List<Tuple<string, MapData>> list)
			{
				if (list != null && list.Count != 0)
				{
					for (int i = 0; i < list.Count; i++)
					{
						SetCommunityMaps(list[i].Item1, list[i].Item2, delegate(string p_filepath, DRLCommunityMapData p_result)
						{
							if (File.Exists(p_filepath))
							{
								File.Delete(p_filepath);
							}
						});
					}
				}
			});
			maps.GetMapEditorImages(delegate(List<Tuple<string, byte[]>> images)
			{
				if (images != null && images.Count != 0)
				{
					for (int i = 0; i < images.Count; i++)
					{
						StorageImage("map-editor-thumb", images[i].Item1, images[i].Item2, delegate(string p_filepath, string p_url)
						{
							if (File.Exists(p_filepath))
							{
								File.Delete(p_filepath);
							}
						});
					}
				}
			});
			maps.ClearMapEditorCache();
		}

		public WebAsyncRequest GetTournaments(string p_guid, Action<DRLTournamentResult> p_callback, int p_timeout = -1)
		{
			return backend.GetTournaments(p_guid, p_callback, p_timeout);
		}

		public WebAsyncRequest GetTournaments(int p_min_skill, Action<DRLTournamentResult> p_callback, int p_timeout = -1, int p_count = 4)
		{
			return backend.GetTournaments(p_min_skill, p_callback, p_timeout, p_count);
		}

		public WebAsyncRequest GetTournaments(bool p_registered_only, Action<DRLTournamentResult> p_callback, int p_timeout = -1)
		{
			return backend.GetTournaments(p_registered_only, p_callback, p_timeout);
		}

		public WebAsyncRequest GetTournaments(Action<DRLTournamentResult> p_callback, int p_timeout = -1)
		{
			return backend.GetTournaments(p_registered_only: false, p_callback, p_timeout);
		}

		public WebAsyncRequest GetTournament(string p_guid, Action<DRLTournamentResult> p_callback, int p_timeout = -1)
		{
			return backend.GetTournament(p_guid, p_callback, p_timeout);
		}

		public WebAsyncRequest GetTryoutsTrack(Action<string> p_result, int p_timeout = -1)
		{
			return backend.GetTryoutsActiveTrack(p_result, p_timeout);
		}

		public WebAsyncRequest GetTryoutsTournamentWinners(Action<string[]> p_result, int p_timeout = -1)
		{
			return backend.GetTryoutsTournamentWinners(p_result, p_timeout);
		}

		public WebAsyncRequest GetTryoutsHeatsFinished(Action<int> p_callback, int p_timeout = -1)
		{
			return backend.GetTryoutsHeatsFinished(p_callback, p_timeout);
		}

		public WebAsyncRequest GetNotifications(Action<DRLNotificationsData[]> p_callback, int p_timeout = -1)
		{
			return backend.GetNotifications(p_callback, p_timeout);
		}

		public WebAsyncRequest MarkNotificationRead(string p_id, int p_timeout = -1)
		{
			return backend.MarkNotificationRead(p_id, p_timeout);
		}

		public WebAsyncRequest RefreshAchievements(Action<DRLAchievementsResult> p_callback, int p_timeout = -1)
		{
			return backend.RefreshAchievements(p_callback, p_timeout);
		}

		public WebAsyncRequest GetAchievements(Action<DRLAchievementResult> p_callback, string p_playerID, int p_timeout = -1)
		{
			return backend.GetAchievements(p_callback, p_playerID, p_timeout);
		}

		public WebAsyncRequest GetAchievementRequirements(Action<DRLAchievementRequirementsResult> p_callback, string p_playerID, string p_achievementID, int p_timeout = -1)
		{
			return backend.GetAchievementRequirements(p_callback, p_playerID, p_achievementID, p_timeout);
		}

		public WebAsyncRequest MarkAchievementRead(string p_id, int p_timeout = -1)
		{
			return backend.MarkAchievementsRead(p_id, p_timeout);
		}

		public WebAsyncRequest GetMatch(string p_guid, string p_matchId, Action<DRLTournamentMatchResult> p_callback, int p_timeout = -1)
		{
			return backend.GetMatch(p_guid, p_matchId, p_callback, p_timeout);
		}

		public WebAsyncRequest GetHeatResults(string p_guid, string p_matchId, int p_heatIdx, Action<DRLTournamentHeatData> p_callback, int p_timeout = -1)
		{
			return backend.GetHeatResults(p_guid, p_matchId, p_heatIdx, p_callback, p_timeout);
		}

		public WebAsyncRequest GetTournamentCountdownState(string p_guid, string p_matchId, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return backend.GetTournamentCountdownState(p_guid, p_matchId, p_callback, p_timeout);
		}

		public WebAsyncRequest GetTournamentPlacements(string p_guid, Action<DRLTournamentPlacementsData> p_callback, int p_timeout = -1)
		{
			return backend.GetTournamentPlacements(p_guid, p_callback, p_timeout);
		}

		public WebAsyncRequest SetMatchHeat(string p_guid, string p_matchId, int p_heatIdx, int p_timeout = -1)
		{
			return backend.SetMatchHeat(p_guid, p_matchId, p_heatIdx, p_timeout);
		}

		public WebAsyncRequest RegisterUser(string p_guid, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return backend.RegisterUser(p_guid, p_callback, p_timeout);
		}

		public WebAsyncRequest UnregisterUser(string p_guid, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return backend.UnregisterUser(p_guid, p_callback, p_timeout);
		}

		public WebAsyncRequest SubscribeUser(string p_guid, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return backend.SubscribeUser(p_guid, p_callback, p_timeout);
		}

		public WebAsyncRequest SubscribeUser(Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return backend.SubscribeUser(p_callback, p_timeout);
		}

		public WebAsyncRequest UnsubscribeUser(string p_guid, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return backend.UnsubscribeUser(p_guid, p_callback, p_timeout);
		}

		public WebAsyncRequest UnsubscribeUser(Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return backend.UnsubscribeUser(p_callback, p_timeout);
		}

		public WebAsyncRequest CheckUserSubscription(string p_guid, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return backend.CheckUserSubscription(p_guid, p_callback, p_timeout);
		}

		public WebAsyncRequest CheckUserSubscription(Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return backend.CheckUserSubscription(p_callback, p_timeout);
		}

		public WebAsyncRequest SetTournamentResults(string p_guid, DRLRaceResultData[] p_results, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return backend.SetTournamentResults(p_guid, p_results, p_callback, p_timeout);
		}

		public WebAsyncRequest GetTournamentResults(string p_guid, string p_roundId, Action<DRLTournamentResultData> p_callback, int p_timeout = -1)
		{
			return backend.GetTournamentResults(p_guid, p_roundId, p_callback, p_timeout);
		}

		public void WatchTournamentRefresh()
		{
			StartTournamentRefresh(delegate
			{
				if (m_reconnectActivity != null)
				{
					m_reconnectActivity.Stop();
					m_reconnectActivity = null;
				}
				m_reconnectActivity = ((Component)this).ActivityRun((Func<bool>)delegate
				{
					if (!base.validContext)
					{
						return false;
					}
					if (!base.app.model.service.tournamentSocket.IsConnected())
					{
						m_reconnectTimer -= Time.deltaTime;
						if (m_reconnectTimer <= 0f)
						{
							Debug.LogWarning("ServiceModel> Socket connection dropped. Attempting to reconnect..");
							base.app.model.service.tournamentSocket.UnsubscribeAll();
							WatchTournamentRefresh();
							m_reconnectTimer = 1f;
							return false;
						}
					}
					return true;
				}, 0f);
			});
		}

		private void StartTournamentRefresh(Action p_callback = null)
		{
			backend.WatchTournamentRefresh(base.app.model.service.tournamentSocket, delegate(DRLTournamentSocketData p_result)
			{
				if (base.validContext && p_result != null)
				{
					switch (p_result.action)
					{
					case TournamentActionEvent.reset_match:
						Notify("tournament.action.reset-match", p_result.id);
						break;
					case TournamentActionEvent.reset_match_heat:
						Notify("tournament.action.reset-heat", p_result.id);
						break;
					case TournamentActionEvent.start_race:
						Notify("tournament.action.start-match", p_result.id);
						break;
					case TournamentActionEvent.starting:
						Notify("tournament.action.match-starting", p_result.id);
						break;
					case TournamentActionEvent.pull:
						Notify("tournament.action.match-pull", p_result.id);
						break;
					case TournamentActionEvent.results_arrived:
						Notify("tournament.match.results-arrived", p_result.id);
						break;
					case TournamentActionEvent.end_race:
						Notify("tournament.action.quit-heat", p_result.id);
						break;
					case TournamentActionEvent.force_quit_single:
						Notify("tournament.action.quit-heat-user", p_result.metaData.playerId);
						break;
					case TournamentActionEvent.restart_sudden_death:
					case TournamentActionEvent.restart_golden_heat:
						Notify("tournament.action.refresh-racers", p_result.id);
						break;
					case TournamentActionEvent.countdown_start:
						Notify("tournament.countdown-start", p_result.id);
						break;
					}
					if (p_result.action == TournamentActionEvent.refresh && p_result.description == "swap players")
					{
						Notify("tournament.action.swapped");
					}
					else
					{
						if (p_result.description == "refresh order")
						{
							Notify("tournament.action.refresh-racers");
						}
						Notify("tournament.refresh.data");
					}
					Debug.Log("ServiceModel> Received action event from server! Action - " + p_result.action.ToString() + "\nDescription - " + p_result.description);
				}
			});
		}

		public void SendWebsocketEvent(DRLTournamentSocketData p_data)
		{
			backend.SocketEmitMessage(p_data);
		}

		public void SendMatchStartingSocketEvent(string p_matchID)
		{
			m_tournamentSocketData = new DRLTournamentSocketData();
			m_tournamentSocketData.classType = "Match";
			m_tournamentSocketData.id = p_matchID;
			m_tournamentSocketData.status = "active";
			m_tournamentSocketData.description = "Match about to start.";
			m_tournamentSocketData.action = TournamentActionEvent.starting;
			SendWebsocketEvent(m_tournamentSocketData);
		}

		public void StopTournamentRefresh(Action p_callback = null)
		{
			if (base.validContext && !(backend == null))
			{
				if (m_reconnectActivity != null)
				{
					m_reconnectActivity.Stop();
					m_reconnectActivity = null;
				}
				backend.StopTournamentRefresh(delegate
				{
					Notify("tournament.refresh-listener.stopped");
					p_callback?.Invoke();
				});
			}
		}

		public WebAsyncRequest GetTournamentsLegacy(string p_guid, Action<DRLTournamentLegacyData[]> p_callback, int p_timeout = -1)
		{
			return backend.GetTournamentsLegacy(p_guid, p_callback, p_timeout);
		}

		public WebAsyncRequest GetTournamentsLegacy(Action<DRLTournamentLegacyData[]> p_callback, int p_timeout = -1)
		{
			return GetTournamentsLegacy("", p_callback, p_timeout);
		}

		public WebAsyncRequest SetTournamentsLegacyResults(string p_guid, DRLRaceResultData[] p_results, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return backend.SetTournamentLegacyResults(p_guid, p_results, p_callback, p_timeout);
		}

		public WebAsyncRequest GetPlayerProgression(Action<DRLProgressionStateData> p_callback, int p_timeout = -1)
		{
			return backend.GetPlayerProgression(p_callback, p_timeout);
		}

		public WebAsyncRequest GetProgressionTracks(Action<DRLProgressionTrackData[]> p_callback, int p_timeout = -1)
		{
			return backend.GetProgressionMaps(p_callback, p_timeout);
		}

		public WebAsyncRequest GetProgressionWeekRank(Action<DRLProgressionRankResult> p_callback, int p_timeout = -1)
		{
			return backend.GetProgressionWeekRank(p_callback, p_timeout);
		}

		public DRLReplayData CreateReplayData(int p_order, byte[] p_data, GameFlag p_game_type, bool p_multiplayer, ScoreType p_score_type, DRLMap p_map, DRLMapTrack p_track, DRLCampaign p_campaign, int p_drone_class, string p_region)
		{
			DRLReplayData dRLReplayData = new DRLReplayData();
			if (p_order >= 0)
			{
				dRLReplayData.order = p_order;
			}
			dRLReplayData.gameType = p_game_type.ToString();
			dRLReplayData.scoreType = p_score_type.ToString();
			dRLReplayData.multiplayer = p_multiplayer;
			dRLReplayData.diameter = p_drone_class;
			if ((bool)p_map)
			{
				dRLReplayData.map = p_map.guid;
			}
			if ((bool)p_track)
			{
				dRLReplayData.track = p_track.guid;
			}
			if ((bool)p_map)
			{
				dRLReplayData.isCustomMap = p_map.data != null;
				if (dRLReplayData.isCustomMap)
				{
					dRLReplayData.customMap = p_map.data.guid;
				}
			}
			if ((bool)p_campaign)
			{
				dRLReplayData.group = p_campaign.guid;
			}
			if (p_data != null)
			{
				dRLReplayData.replayData = p_data;
			}
			if (!string.IsNullOrEmpty(p_region))
			{
				dRLReplayData.region = p_region;
			}
			return dRLReplayData;
		}

		public WebAsyncRequest SetReplayRace(string p_id, int p_order, byte[] p_data, DRLMap p_map, DRLMapTrack p_track, bool p_multiplayer, int p_drone_class, bool p_force, Action<DRLReplayData[]> p_callback, int p_timeout = -1)
		{
			DRLReplayData dRLReplayData = CreateReplayData(p_order, p_data, GameFlag.Race, p_multiplayer, ScoreType.TimeMin, p_map, p_track, null, p_drone_class, "");
			if (!string.IsNullOrEmpty(p_id))
			{
				dRLReplayData.leaderboardId = p_id;
			}
			return backend.SetReplay(dRLReplayData, p_callback, p_timeout);
		}

		public WebAsyncRequest SetReplayRace(string p_id, int p_order, byte[] p_data, DRLMap p_map, DRLMapTrack p_track, int p_drone_class, bool p_force, Action<DRLReplayData[]> p_callback, int p_timeout = -1)
		{
			return SetReplayRace(p_id, p_order, p_data, p_map, p_track, p_multiplayer: false, p_drone_class, p_force, p_callback, p_timeout);
		}

		public WebAsyncRequest SetReplayRace(string p_id, byte[] p_data, DRLMap p_map, DRLMapTrack p_track, bool p_multiplayer, int p_drone_class, bool p_force, Action<DRLReplayData[]> p_callback, int p_timeout = -1)
		{
			return SetReplayRace(p_id, -1, p_data, p_map, p_track, p_multiplayer, p_drone_class, p_force, p_callback, p_timeout);
		}

		public WebAsyncRequest SetReplayRace(string p_id, byte[] p_data, DRLMap p_map, DRLMapTrack p_track, int p_drone_class, bool p_force, Action<DRLReplayData[]> p_callback, int p_timeout = -1)
		{
			return SetReplayRace(p_id, -1, p_data, p_map, p_track, p_multiplayer: false, p_drone_class, p_force, p_callback, p_timeout);
		}

		public WebAsyncRequest GetReplayRace(string p_player_id, int p_order, DRLMap p_map, DRLMapTrack p_track, int p_drone_class, Action<DRLReplayData[]> p_callback, int p_timeout = -1)
		{
			DRLReplayData dRLReplayData = CreateReplayData(p_order, null, GameFlag.Race, p_multiplayer: false, ScoreType.TimeMin, p_map, p_track, null, p_drone_class, "");
			dRLReplayData.playerId = p_player_id;
			dRLReplayData.Remove("multiplayer");
			return backend.GetReplay(dRLReplayData, p_all: false, p_callback, p_timeout);
		}

		public WebAsyncRequest GetReplayRace(string p_player_id, DRLMap p_map, DRLMapTrack p_track, int p_drone_class, Action<DRLReplayData[]> p_callback, int p_timeout = -1)
		{
			return GetReplayRace(p_player_id, -1, p_map, p_track, p_drone_class, p_callback, p_timeout);
		}

		public WebAsyncRequest GetReplayRivals(string p_player_id, DRLReplayData p_query, int p_limit, Action<DRLLeaderboardRivalsResult> p_callback, int p_timeout = -1)
		{
			DRLReplayData dRLReplayData = ((p_query == null) ? new DRLReplayData() : p_query);
			dRLReplayData.playerId = (string.IsNullOrEmpty(p_player_id) ? backend.playerId : p_player_id);
			dRLReplayData.limit = p_limit;
			return backend.GetReplayRivals(dRLReplayData, p_callback, p_timeout);
		}

		public WebAsyncRequest GetReplayRivals(DRLReplayData p_query, int p_limit, Action<DRLLeaderboardRivalsResult> p_callback, int p_timeout = -1)
		{
			return GetReplayRivals("", p_query, p_limit, p_callback, p_timeout);
		}

		public WebAsyncRequest GetReplayRivals(string p_player_id, DRLGameAsset p_campaign, DRLMap p_map, DRLMapTrack p_track, int p_limit, int p_drone_class, bool p_official, bool p_customPhysics, Action<DRLLeaderboardRivalsResult> p_callback, int p_timeout = -1, List<string> p_exclude = null, string p_circuitId = null, int p_circuitDifficulty = -1)
		{
			DRLReplayData dRLReplayData = new DRLReplayData();
			dRLReplayData.playerId = (string.IsNullOrEmpty(p_player_id) ? backend.playerId : p_player_id);
			dRLReplayData.limit = p_limit;
			dRLReplayData.gameType = (p_campaign ? GameFlag.Campaign.ToString() : GameFlag.Race.ToString());
			dRLReplayData.scoreTypeFlag = ScoreType.TimeMin;
			dRLReplayData.diameter = p_drone_class;
			if (dRLReplayData.diameter < 0)
			{
				dRLReplayData.Remove("diameter");
			}
			if (p_exclude != null)
			{
				dRLReplayData.exclude = string.Join(",", p_exclude);
			}
			if (!string.IsNullOrEmpty(p_circuitId))
			{
				dRLReplayData.circuitId = p_circuitId;
				if (p_circuitDifficulty >= 0)
				{
					dRLReplayData.circuitDifficulty = p_circuitDifficulty;
				}
			}
			else
			{
				dRLReplayData.Remove("circuit-id");
				dRLReplayData.Remove("circuit-difficulty");
			}
			if ((bool)p_campaign)
			{
				dRLReplayData.group = p_campaign.guid;
			}
			if ((bool)p_map)
			{
				dRLReplayData.map = p_map.guid;
			}
			if ((bool)p_track)
			{
				dRLReplayData.track = p_track.guid;
			}
			AssertCommunityMap(dRLReplayData, p_map, p_track);
			dRLReplayData.customPhysics = p_customPhysics;
			dRLReplayData.drlOfficial = p_official;
			return backend.GetReplayRivals(dRLReplayData, p_callback, p_timeout);
		}

		public WebAsyncRequest GetReplayRivals(string p_player_id, DRLMap p_map, DRLMapTrack p_track, int p_limit, int p_drone_class, bool p_official, bool p_customPhysics, Action<DRLLeaderboardRivalsResult> p_callback, int p_timeout = -1)
		{
			return GetReplayRivals(p_player_id, null, p_map, p_track, p_limit, p_drone_class, p_official, p_customPhysics, p_callback, p_timeout);
		}

		public WebAsyncRequest GetReplayOnboarding(Action<OnboardingRaceReplayData[]> p_callback, OnboardingCampaignMode mode, int p_timeout = -1)
		{
			return backend.GetOnboardingBotReplay(p_callback, mode, p_timeout);
		}

		public WebAsyncRequest GetReplayRivals(DRLMap p_map, DRLMapTrack p_track, int p_limit, int p_drone_class, bool p_official, bool p_customPhysics, Action<DRLLeaderboardRivalsResult> p_callback, int p_timeout = -1, List<string> p_exclude = null, string p_circuitId = null, int p_circuitDifficulty = -1)
		{
			return GetReplayRivals("", null, p_map, p_track, p_limit, p_drone_class, p_official, p_customPhysics, p_callback, p_timeout, p_exclude, p_circuitId, p_circuitDifficulty);
		}

		public WebAsyncRequest GetReplayRivals(DRLCampaign p_campaign, int p_limit, int p_drone_class, bool p_official, bool p_customPhysics, Action<DRLLeaderboardRivalsResult> p_callback, int p_timeout = -1)
		{
			return GetReplayRivals("", p_campaign, null, null, p_limit, p_drone_class, p_official, p_customPhysics, p_callback, p_timeout);
		}

		public WebAsyncRequest GetMultiplayerBots(DRLMap p_map, DRLMapTrack p_track, int p_limit, int p_drone_class, bool p_custom_physics, Action<DRLLeaderboardData[]> p_callback, int p_timeout = -1, List<string> p_exclude = null, string p_drone_guid = null)
		{
			DRLReplayData dRLReplayData = new DRLReplayData();
			dRLReplayData.playerId = backend.playerId;
			dRLReplayData.limit = p_limit;
			if (p_drone_guid == null)
			{
				dRLReplayData.Remove("drone-guid");
			}
			else
			{
				dRLReplayData.droneGUID = p_drone_guid;
			}
			dRLReplayData.gameType = GameFlag.Race.ToString();
			dRLReplayData.scoreTypeFlag = ScoreType.TimeMin;
			dRLReplayData.diameter = p_drone_class;
			dRLReplayData.Remove("drl-official");
			if (dRLReplayData.diameter < 0)
			{
				dRLReplayData.Remove("diameter");
			}
			if (p_exclude != null)
			{
				dRLReplayData.exclude = string.Join(",", p_exclude);
			}
			if ((bool)p_map)
			{
				dRLReplayData.map = p_map.guid;
			}
			if ((bool)p_track)
			{
				dRLReplayData.track = p_track.guid;
			}
			AssertCommunityMap(dRLReplayData, p_map, p_track);
			dRLReplayData.customPhysics = p_custom_physics;
			return backend.GetMultiplayerBots(dRLReplayData, p_callback, p_timeout);
		}

		public WebAsyncRequest Storage(byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			return backend.Storage(p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest Storage(string p_category, byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			return backend.Storage(p_category, p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest StorageTemp(byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			return backend.StorageTemp(p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest StorageTemp(string p_category, byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			return backend.StorageTemp(p_category, p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest StorageTemp(string p_category, string p_localFilepath, byte[] p_data, Action<string, string> p_callback, int p_timeout = -1)
		{
			return backend.StorageTemp(p_category, p_localFilepath, p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest StorageImage(byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			return backend.StorageImage(p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest StorageImage(string p_category, byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			return backend.StorageImage(p_category, p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest StorageImage(string p_category, string p_localFilepath, byte[] p_data, Action<string, string> p_callback, int p_timeout = -1)
		{
			return backend.StorageImage(p_category, p_localFilepath, p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest StorageReplay(byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			return backend.StorageReplay(p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest StorageReplayCloud(string p_score_id, byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			return backend.StorageReplayCloud(p_score_id, p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest StorageReplayCloud(string p_score_id, byte[] p_data, string p_map, string p_track, string p_custom_map, int p_diameter, float p_score, string p_match_id, Action<string> p_callback, int p_timeout = -1)
		{
			return backend.StorageReplayCloud(p_score_id, p_data, p_map, p_track, p_custom_map, p_diameter, p_score, p_match_id, p_callback, p_timeout);
		}

		public WebAsyncRequest StorageLogs(byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			return backend.StorageLogs(p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest StorageReplay(string p_category, byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			return backend.StorageReplay(p_category, p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest GetImage(string p_url, int p_width, int p_height, Action<Texture2D> p_callback, int p_timeout = -1)
		{
			return backend.GetImage(p_url, p_width, p_height, p_callback, p_timeout);
		}

		public WebAsyncRequest GetImage(string p_url, int p_width, int p_height, Action<Texture> p_callback, int p_timeout = -1)
		{
			return backend.GetImage(p_url, p_width, p_height, p_callback, p_timeout);
		}

		public WebAsyncRequest SetPlayerAvatar(byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			return backend.SetPlayerAvatar(p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest GetPlayerAvatar(string p_id, Action<Texture2D> p_callback, int p_timeout = -1)
		{
			return backend.GetPlayerAvatar(p_id, p_callback, p_timeout);
		}

		public WebAsyncRequest GetPlayerAvatar(Action<Texture2D> p_callback, int p_timeout = -1)
		{
			return backend.GetPlayerAvatar(backend.playerId, p_callback, p_timeout);
		}

		public WebAsyncRequest GetSocialProfile(string[] p_ids, Action<DRLPlayerProfileData[]> p_callback, int p_timeout = -1)
		{
			return backend.GetSocialProfile(p_ids, p_callback, p_timeout);
		}

		public WebAsyncRequest GetSocialProfile(string p_id, Action<DRLPlayerProfileData[]> p_callback, int p_timeout = -1)
		{
			return backend.GetSocialProfile(p_id, p_callback, p_timeout);
		}

		public WebAsyncRequest GetTimers(string[] p_ids, Action<DRLTimerData[]> p_callback, int p_timeout = -1)
		{
			return backend.GetTimers(p_ids, p_callback, p_timeout);
		}

		public WebAsyncRequest GetTimers(string p_id, Action<DRLTimerData[]> p_callback, int p_timeout = -1)
		{
			return backend.GetTimers(p_id, p_callback, p_timeout);
		}

		public WebAsyncRequest StartTimer(string p_id, Action<DRLTimerData> p_callback, int p_timeout = -1)
		{
			return backend.StartTimer(p_id, p_callback, p_timeout);
		}

		public WebAsyncRequest StopTimer(string p_id, Action<DRLTimerData> p_callback, int p_timeout = -1)
		{
			return backend.StopTimer(p_id, p_callback, p_timeout);
		}

		public WebAsyncRequest SendCounterUAVData(DRLCounterUAVData p_data, Action<DRLCounterUAVData> p_callback, int p_timeout = -1)
		{
			return backend.SetCounterUAVCatchData(p_data, p_callback);
		}

		public WebAsyncRequest SendCounterUAVData(float p_x, float p_y, string p_mode, float p_duration, Action<DRLCounterUAVData> p_callback, int p_timeout = -1)
		{
			DRLCounterUAVData dRLCounterUAVData = new DRLCounterUAVData();
			dRLCounterUAVData.x = p_x;
			dRLCounterUAVData.y = p_y;
			dRLCounterUAVData.mode = p_mode;
			dRLCounterUAVData.duration = p_duration;
			return SendCounterUAVData(dRLCounterUAVData, p_callback, p_timeout);
		}

		public WebAsyncRequest ServerTime(Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return backend.ServerTime(delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("ServiceModel> ServerTime - Error");
					Notify("service.time@error");
				}
				else
				{
					time_lock = false;
					if (p_callback != null)
					{
						p_callback(p_result);
					}
					Notify("service.time@refresh", p_result);
				}
			}, p_timeout);
		}

		protected void UpdateState()
		{
			if (!state_lock && stateAutoRefresh)
			{
				state_elapsed += Time.unscaledDeltaTime;
				if (state_elapsed >= stateRefresh)
				{
					state_lock = true;
					state_elapsed = 0f;
					StateGame(null);
				}
			}
		}

		protected void UpdateTime()
		{
			if (Application.isEditor)
			{
				return;
			}
			if (!time_lock && stateAutoRefresh)
			{
				time_elapsed += Time.unscaledDeltaTime;
				if (time_elapsed >= timeRefresh)
				{
					time_lock = true;
					time_elapsed = 0f;
					ServerTime(null);
				}
			}
		}

		public WebAsyncRequest GetStoreProducts(DRLStoreData p_query, Action<DRLStoreResult> p_callback, int p_timeout = -1)
		{
			return backend.GetStoreProducts(p_query, p_callback, p_timeout);
		}

		public void BuyProduct(DRLStoreProductData p_product, Action<bool, string> p_oncomplete)
		{
			DRLStoreProductData pd = p_product;
			if (pd == null)
			{
				if (p_oncomplete != null)
				{
					p_oncomplete(arg1: false, "INVALID PRODUCT");
				}
				Debug.LogWarning("ServiceModel> BuyProduct / Invalid Product");
				return;
			}
			bool isDeveloper = base.app.model.storage.state.player.profile.isDeveloper;
			bool key = Input.GetKey(KeyCode.LeftShift);
			bool flag = false;
			if (isDeveloper && key)
			{
				flag = true;
			}
			bool flag2 = base.app.model.storage.state.player.profile.ContainsInventory(pd.items);
			Debug.Log($"ServiceModel> BuyProduct / BuyProduct [{pd.name}] Start - platform-id[{pd.platformId}] is-debug[{flag}] has-product[{flag2}]");
			if (flag2)
			{
				if (p_oncomplete != null)
				{
					p_oncomplete(arg1: false, "PRODUCT ALREADY OWNED");
				}
				return;
			}
			if (flag)
			{
				base.app.model.service.backend.TransactionComplete(p_product.platformId, delegate
				{
					base.app.model.storage.state.player.profile.RegisterInventoryGUIDs(pd.items);
					if (p_oncomplete != null)
					{
						p_oncomplete(arg1: true, "");
					}
				});
				return;
			}
			base.app.model.service.platform.PurchaseProduct(pd.platformId, delegate(bool p_success, string p_msg)
			{
				Debug.Log($"ServiceModel> BuyProduct / PurchaseProduct - success[{p_success}] msg[{p_msg}]");
				if (p_success)
				{
					base.app.model.service.backend.TransactionComplete(p_product.platformId, delegate
					{
						base.app.model.storage.state.player.profile.RegisterInventoryGUIDs(pd.items);
						if (p_oncomplete != null)
						{
							p_oncomplete(arg1: true, "");
						}
					});
				}
				else if (p_oncomplete != null)
				{
					p_oncomplete(arg1: false, p_msg.ToUpper());
				}
			});
		}

		public WebAsyncRequest GetCircuitsData(Action<DRLCircuitData[]> p_callback, int p_timeout = -1)
		{
			return backend.GetCircuits(p_callback, p_timeout);
		}

		protected void Update()
		{
		}

		public void OnPersistency()
		{
			base.app.model.service = this;
		}
	}
}

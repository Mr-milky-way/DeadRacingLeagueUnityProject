using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using drl.game;
using thelab.core;

namespace drl.backend
{
	public class DRLService : MonoBehaviour
	{
		public static string PlatformIdKey = "steam-id";

		private int retryCount;

		private const int maxRetries = 5;

		public List<DRLServiceResult> results;

		public string token;

		private SerializedData m_loginData;

		private SteamService m_steam;

		public const string achievementsPath = "/achievements/";

		public const string achievementsReadPath = "seen/";

		private int m_refreshAttempts = 3;

		public const string bundlePath = "/bundles/";

		public const string circuitsPath = "/circuits/";

		public const string circuitLeaderboardsPath = "/circuits/leaderboards/";

		public const string circuitLeaderboardsUserPath = "/circuits/leaderboards/user/";

		public const string contentPath = "/bundles/";

		public const string contentManifestPath = "/bundles/manifest/";

		public string cuavPath = "/player/counter-uav/";

		public const string dronesPath = "/drones/";

		public const string dronesRatePath = "rate/";

		public const string dronesRemovePath = "remove/";

		public const string dronesTimePath = "flight-time/";

		private const string LeaderboardsPath = "/leaderboards/";

		private const string LeaderboardsGroupPath = "/leaderboards/group/";

		private const string LeaderboardUserPath = "/leaderboards/user/";

		private const string LeaderboardUserResetPath = "/leaderboards/user/reset/";

		private const string LeaderboardRivalsPath = "/leaderboards/rivals/";

		private const string LeaderboardsQuestPath = "/leaderboards/quest/";

		private const string LeaderboardTrackResetPath = "/leaderboards/user/reset/track/";

		public const string licensePath = "/player/license/";

		public const string loginPath = "/login/";

		public const string loginV2 = "/v2/login";

		private string m_authTicket;

		private string m_platformId;

		private string m_appVersion;

		private bool m_loginInProgress;

		public const string mapsPath = "/maps/";

		public const string mapsRatePath = "rate/";

		public const string mapsRemovePath = "remove/";

		public const string mapsUpdatedPath = "updated/";

		public const string multiplayerPath = "/multiplayer/";

		public const string multiplayerBotsPath = "/multiplayer/bots/";

		public const string notificationsPath = "/notifications/";

		public const string notificationsReadPath = "read/";

		public const string physicsTunesPath = "/physics-tunes/";

		public const string physicsTunesRatePath = "/physics-tunes/rate/";

		public const string physicsTunesRemovePath = "/physics-tunes/remove/";

		public const string profanityAPI = "https://www.purgomalum.com/service/json?text=";

		public const string progressionPath = "/progression/";

		public const string progressionMaps = "/progression/maps/";

		public const string playerProgressionPath = "/experience-points/progression/";

		public const string progressionWeekRankPath = "/experience-points/ranking/";

		public const string replayPath = "/replay/";

		public const string replayRivalsPath = "/replay/rivals/";

		public const string timePath = "/time/";

		public const string socialPath = "/social/";

		public const string socialProfilePath = "/social/profile/";

		public const string socialTwitchStatus = "/social/twitch/";

		public const string socialOnlinePlayers = "/social/online-players/";

		private static Dictionary<string, DRLPlayerProfileData> m_socialProfileCache = new Dictionary<string, DRLPlayerProfileData>();

		private List<DRLPlayerProfileData> m_socialProfileResult = new List<DRLPlayerProfileData>();

		private List<string> m_socialProfileRequestIds = new List<string>();

		public const string playerStatePath = "/state/";

		public const string gameStatePath = "/state/game/";

		public const string storagePath = "/storage/";

		public const string storageTempMode = "temp";

		public const string storageImageMode = "image";

		public const string storageReplayMode = "replay";

		public const string storageLogs = "logs";

		public const string storageReplayCloudMode = "replay-cloud";

		public const string imagesPath = "/images/";

		public const string playerAvatarPath = "/player/avatar/";

		public const string storePath = "/store/";

		public const string timerPath = "/timer/";

		public const string timerStartPath = "/timer/start/";

		public const string timerStopPath = "/timer/stop/";

		public const string tournamentsPath = "/tournaments/";

		public const string registeredTournamentsPath = "/player/tournaments/";

		private TournamentSocketService socket;

		public Thread m_thread;

		private int m_matchHeatRetryAttempts = 5;

		public const string tournamentsLegacyPath = "/tournaments/";

		public const string tournamentsLegacyScoresPath = "/tournaments/scores/";

		public const string transactionsPath = "/transactions/";

		public const string transactionsStartPath = "/transactions/start/";

		public const string transactionsFinalizePath = "/transactions/finalize/";

		public const string transactionsCompletePath = "/transactions/complete/";

		public const string transactionsRefundPath = "/transactions/refund/";

		public const string tryoutsPath = "/tryouts/";

		public const string tryoutsTournamentsPath = "tournaments/";

		private const string OnboardingPath = "/onboarding/bots";

		private const string OnboardingReplayBeginnerPath = "/beginner";

		private const string OnboardingReplayIntermediatePath = "/intermediate";

		private const string OnboardingReplayProPath = "/pro";

		public static string baseUri => "https://api.drlgame.com";

		public static string baseWebsocketUri => "wss://api.drlgame.com/live/?transport=websocket";

		public static string baseStatusPageUri => "https://status.drlgame.com/";

		public static string promoBannerUri => "https://drl-game-api.s3.amazonaws.com/in-game/home-notification.png";

		public string webtoken => token;

		public SerializedData loginData
		{
			get
			{
				if (m_loginData != null)
				{
					return m_loginData;
				}
				return m_loginData = new SerializedData();
			}
			set
			{
				m_loginData = ((value == null) ? new SerializedData() : value);
			}
		}

		public string playerId
		{
			get
			{
				string text = loginData.Get("player-id", "");
				if (string.IsNullOrEmpty(text))
				{
					text = loginData.Get("_id", "");
				}
				return text;
			}
		}

		public bool hasPlayerId => !string.IsNullOrEmpty(playerId);

		protected SteamService steam
		{
			get
			{
				if (!m_steam)
				{
					return m_steam = GetComponent<SteamService>();
				}
				return m_steam;
			}
		}

		public WebAsyncRequest Get(string p_id, string p_endpoint, Action<DRLServiceResult> p_callback, object p_data = null, int p_timeout = -1)
		{
			string text = baseUri;
			if (p_endpoint.StartsWith("@"))
			{
				text = "";
				p_endpoint = p_endpoint.Substring(1);
			}
			UriBuilder uriBuilder = new UriBuilder(text + p_endpoint);
			string url = uriBuilder.Uri.ToString();
			LogDRLServiceRequest("GET", p_id, url, p_data);
			Dictionary<string, string> headers = null;
			if (!string.IsNullOrEmpty(webtoken))
			{
				headers = new Dictionary<string, string>();
				headers.Add("X-Access-JSONWebToken", webtoken);
			}
			float t0 = Time.realtimeSinceStartup;
			WebCallback<string> cb = null;
			cb = delegate(string p_result, float p_progress, WebAsyncRequest p_request)
			{
				if (p_progress >= 1f)
				{
					DRLServiceResult requestResult = GetRequestResult(p_request, p_result);
					bool flag = false;
					if (!requestResult.success)
					{
						if ((string.IsNullOrEmpty(requestResult.message) ? "" : requestResult.message.ToLower()).Contains("timeout"))
						{
							flag = true;
						}
						if (IsTokenError(requestResult) && retryCount < 5)
						{
							retryCount++;
							RefreshToken(delegate
							{
								Get(p_id, p_endpoint, p_callback, p_data, p_timeout);
							}, p_timeout, retryCount);
							return;
						}
					}
					if (Debug.unityLogger.logEnabled)
					{
						int num = Mathf.FloorToInt((Time.realtimeSinceStartup - t0) * 1000f);
						string text2 = (Application.isEditor ? $"<color=#fff>{num}ms</color>" : $"{num}ms");
						string text3 = (Application.isEditor ? ("<color=#ff0>" + (flag ? "[TIMEOUT]" : "") + "</color>") : ((flag ? "[TIMEOUT]" : "") ?? ""));
						Debug.Log($"DRLService> [GET] {p_id} / code[{p_request?.code ?? 0}] {text3} {text2}");
					}
					if (flag)
					{
						Debug.Log("DRLService> Get " + p_id + " / Backend Timeout - Retry...");
						Web.Get(p_id, url, cb, p_data, headers, p_timeout);
					}
					else
					{
						retryCount = 0;
						if (p_callback != null)
						{
							p_callback(requestResult);
						}
					}
				}
			};
			return Web.Get(p_id, url, cb, p_data, headers, p_timeout);
		}

		public WebAsyncRequest GetRaw(string p_id, string p_endpoint, Action<string> p_callback, object p_data = null, int p_timeout = -1)
		{
			string text = baseUri;
			if (p_endpoint.StartsWith("@"))
			{
				text = "";
				p_endpoint = p_endpoint.Substring(1);
			}
			string p_url = new UriBuilder(text + p_endpoint).Uri.ToString();
			LogDRLServiceRequest("GET", p_id, p_url, p_data);
			Dictionary<string, string> dictionary = null;
			if (!string.IsNullOrEmpty(webtoken))
			{
				dictionary = new Dictionary<string, string>();
				dictionary.Add("X-Access-JSONWebToken", webtoken);
			}
			float t0 = Time.realtimeSinceStartup;
			return Web.Get(p_id, p_url, delegate(string p_result, float p_progress, WebAsyncRequest p_request)
			{
				if (p_progress >= 1f)
				{
					if (Debug.unityLogger.logEnabled)
					{
						Debug.Log($"DRLService> [GET] {p_id} / code[{p_request?.code ?? 0}] <color=#fff>{Mathf.FloorToInt((Time.realtimeSinceStartup - t0) * 1000f)}ms</color>");
					}
					if (p_request.code != 200 && retryCount < 5)
					{
						retryCount++;
						RefreshToken(delegate
						{
							GetRaw(p_id, p_endpoint, p_callback, p_data, p_timeout);
						}, p_timeout, retryCount);
					}
					else
					{
						retryCount = 0;
						if (p_callback != null)
						{
							p_callback(p_result);
						}
					}
				}
			}, p_data, dictionary, p_timeout);
		}

		public WebAsyncRequest Post(string p_id, string p_endpoint, Action<DRLServiceResult> p_callback, object p_data = null, int p_timeout = -1)
		{
			string text = baseUri;
			if (p_endpoint.StartsWith("@"))
			{
				text = "";
				p_endpoint = p_endpoint.Substring(1);
			}
			UriBuilder uriBuilder = new UriBuilder(text + p_endpoint);
			string url = uriBuilder.Uri.ToString();
			LogDRLServiceRequest("POST", p_id, url, p_data);
			Dictionary<string, string> headers = null;
			if (!string.IsNullOrEmpty(webtoken))
			{
				headers = new Dictionary<string, string>();
				headers.Add("X-Access-JSONWebToken", webtoken);
			}
			float t0 = Time.realtimeSinceStartup;
			WebCallback<string> cb = null;
			cb = delegate(string p_result, float p_progress, WebAsyncRequest p_request)
			{
				if (p_progress >= 1f)
				{
					DRLServiceResult requestResult = GetRequestResult(p_request, p_result);
					bool flag = false;
					if (!requestResult.success)
					{
						if ((string.IsNullOrEmpty(requestResult.message) ? "" : requestResult.message.ToLower()).Contains("timeout"))
						{
							flag = true;
						}
						if (IsTokenError(requestResult) && retryCount < 5)
						{
							retryCount++;
							RefreshToken(delegate
							{
								Post(p_id, p_endpoint, p_callback, p_data, p_timeout);
							}, p_timeout, retryCount);
							return;
						}
					}
					if (Debug.unityLogger.logEnabled)
					{
						int num = Mathf.FloorToInt((Time.realtimeSinceStartup - t0) * 1000f);
						string text2 = (Application.isEditor ? $"<color=#fff>{num}ms</color>" : $"{num}ms");
						string text3 = (Application.isEditor ? ("<color=#ff0>" + (flag ? "[TIMEOUT]" : "") + "</color>") : ((flag ? "[TIMEOUT]" : "") ?? ""));
						Debug.Log($"DRLService> [POST] {p_id} / code[{p_request?.code ?? 0}] {text3} {text2}");
					}
					if (flag)
					{
						Debug.Log("DRLService> Post " + p_id + " / Backend Timeout - Retry...");
						Web.Post(p_id, url, cb, p_data, headers, p_timeout);
					}
					else
					{
						retryCount = 0;
						if (p_callback != null)
						{
							p_callback(requestResult);
						}
					}
				}
			};
			return Web.Post(p_id, url, cb, p_data, headers, p_timeout);
		}

		protected DRLServiceResult GetRequestResult(WebAsyncRequest p_request, string p_json)
		{
			string p_data = (string.IsNullOrEmpty(p_json) ? "{success:false, message:'Invalid JSON'}" : p_json);
			DRLServiceResult dRLServiceResult = new DRLServiceResult();
			dRLServiceResult.success = false;
			dRLServiceResult.message = "Failed to Parse JSON";
			dRLServiceResult.request = p_request;
			dRLServiceResult.id = p_request.id;
			switch (p_request.code)
			{
			case 0L:
				p_data = "{success:false, code: " + p_request.code + ", message:'" + (p_request.cancelled ? "Request Cancelled." : "Unknown Error.") + "'}";
				break;
			}
			try
			{
				if (p_request.id == "drl.service.time")
				{
					Serialize.jsonParseDateTime = false;
				}
				dRLServiceResult = Serialize.FromJson<DRLServiceResult>(p_data);
				if (p_request.id == "drl.service.time")
				{
					Serialize.jsonParseDateTime = true;
				}
				dRLServiceResult.request = p_request;
				dRLServiceResult.id = p_request.id;
				string value = dRLServiceResult.token;
				if (!string.IsNullOrEmpty(value))
				{
					token = value;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("DRLService> Service [" + p_request.id + "] Result Error\n" + ex.Message);
			}
			results.Add(dRLServiceResult);
			return dRLServiceResult;
		}

		private void RefreshToken(Action p_callback, int p_timeout, int p_retryCount)
		{
			Login(delegate
			{
				p_callback?.Invoke();
			}, p_timeout, p_retryCount);
		}

		protected void LogDRLServiceRequest(string p_method, string p_id, string p_url, object p_data = null)
		{
		}

		public WebAsyncRequest RefreshAchievements(Action<DRLAchievementsResult> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/achievements/";
			SerializedData serializedData = new SerializedData();
			serializedData["token"] = token;
			return Get("drl.service.achievements.get", p_endpoint, delegate(DRLServiceResult p_result)
			{
				DRLAchievementsResult dRLAchievementsResult = new DRLAchievementsResult();
				if (!p_result.success)
				{
					if (m_refreshAttempts > 0)
					{
						m_refreshAttempts--;
						RefreshAchievements(p_callback, p_timeout);
					}
				}
				else
				{
					m_refreshAttempts = 3;
					object data = p_result.data;
					if (data == null)
					{
						Debug.LogWarning("Achievements> GetAchievements / Failed!");
					}
					else
					{
						string p_data = data.ToString();
						dRLAchievementsResult.list = Serialize.FromJson<DRLAchievementsData[]>(p_data);
						if (p_callback != null)
						{
							p_callback(dRLAchievementsResult);
						}
					}
				}
			}, serializedData, p_timeout);
		}

		public WebAsyncRequest MarkAchievementsRead(string p_id, int p_timeout = -1)
		{
			string p_endpoint = "/achievements/seen/" + p_id;
			SerializedData serializedData = new SerializedData();
			serializedData["token"] = token;
			return Get("drl.service.achievements.mark-read", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("Achievements> MarkAchievementRead / Failed!");
				}
				else
				{
					Debug.Log("Achievements> MarkAchievementRead / Success!");
				}
			}, serializedData, p_timeout);
		}

		public WebAsyncRequest GetAchievements(Action<DRLAchievementResult> p_callback, string p_playerID, int p_timeout = -1)
		{
			string p_endpoint = "/achievements/all/" + p_playerID;
			SerializedData serializedData = new SerializedData();
			serializedData["token"] = token;
			return Get("drl.service.achievements.get", p_endpoint, delegate(DRLServiceResult p_result)
			{
				DRLAchievementResult dRLAchievementResult = new DRLAchievementResult();
				if (!p_result.success)
				{
					GetAchievements(p_callback, playerId, p_timeout);
				}
				else
				{
					object data = p_result.data;
					if (data == null)
					{
						Debug.LogWarning("Achievements> GetAchievements / Failed!");
					}
					else
					{
						string p_data = data.ToString();
						dRLAchievementResult.list = Serialize.FromJson<DRLAchievementData[]>(p_data);
						if (p_callback != null)
						{
							p_callback(dRLAchievementResult);
						}
					}
				}
			}, serializedData, p_timeout);
		}

		public WebAsyncRequest GetAchievementRequirements(Action<DRLAchievementRequirementsResult> p_callback, string p_playerID, string p_achievementID, int p_timeout = -1)
		{
			string p_endpoint = "/achievements/" + p_achievementID + "/" + p_playerID;
			SerializedData serializedData = new SerializedData();
			serializedData["token"] = token;
			Debug.Log("Achievements-> GetAchievementRequirements: " + p_achievementID);
			return Get("drl.service.achievementRequirements.get", p_endpoint, delegate(DRLServiceResult p_result)
			{
				DRLAchievementRequirementsResult dRLAchievementRequirementsResult = new DRLAchievementRequirementsResult();
				if (!p_result.success)
				{
					Debug.LogWarning("Achievements->GetAchievementRequirements Failed");
				}
				else
				{
					object data = p_result.data;
					if (data == null)
					{
						Debug.LogWarning("Achievements> GetAchievementRequirementss / Failed!");
					}
					else
					{
						string p_data = data.ToString();
						dRLAchievementRequirementsResult.list = Serialize.FromJson<DRLAchievementRequirementsData[]>(p_data);
						if (p_callback != null)
						{
							p_callback(dRLAchievementRequirementsResult);
						}
					}
				}
			}, serializedData, p_timeout);
		}

		public void Bundles(Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/bundles/?os=" + OS.prefix;
			Get("drl.service.bundles", p_endpoint, p_callback, null, p_timeout);
		}

		public WebAsyncRequest GetCircuits(Action<DRLCircuitData[]> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/circuits/";
			SerializedData serializedData = new SerializedData();
			serializedData["token"] = token;
			return Get("drl.service.circuits.player", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					object data = p_result.data;
					if (data == null)
					{
						Debug.LogWarning("<color=red>Circuits> GetCircuits / Failed!<color>");
						if (p_callback != null)
						{
							p_callback(null);
						}
					}
					else
					{
						DRLCircuitData[] obj = Serialize.FromJson<DRLCircuitData[]>(data.ToString());
						if (p_callback != null)
						{
							p_callback(obj);
						}
					}
				}
			}, serializedData, p_timeout);
		}

		public WebAsyncRequest SetLeaderboardCircuit(DRLCircuitLeaderboardData p_results, Action<bool> p_on_complete = null, int p_timeout = -1)
		{
			DRLCircuitLeaderboardData dRLCircuitLeaderboardData = ((p_results == null) ? new DRLCircuitLeaderboardData() : p_results);
			if (string.IsNullOrEmpty(dRLCircuitLeaderboardData.circuitId))
			{
				Debug.LogWarning("DRLService> Tried to send empty leaderboard.");
				p_on_complete?.Invoke(obj: false);
				return null;
			}
			string text = "/circuits/leaderboards/";
			text = text + "?token=" + token;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string value = Serialize.ToJson(dRLCircuitLeaderboardData);
			dictionary["list"] = value;
			return Post("drl.service.circuit-leaderboards.write", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("DRLService> Failed to send leaderboards!\n" + p_result.message);
				}
				p_on_complete?.Invoke(p_result.success);
			}, dictionary, p_timeout);
		}

		public WebAsyncRequest GetLeaderboardCircuit(DRLCircuitLeaderboardData p_query, Action<DRLCircuitsResult> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/circuits/leaderboards/";
			p_query["token"] = token;
			return Get("drl.service.circuit-leaderboards.read", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					DRLCircuitsResult obj = new DRLCircuitsResult
					{
						success = false
					};
					p_callback?.Invoke(obj);
				}
				else
				{
					DRLCircuitsResult obj = Serialize.FromJson<DRLCircuitsResult>(p_result.data.ToString());
					if (obj.pagging == null)
					{
						obj.pagging = new DRLServicePageData();
					}
					if (obj.leaderboard == null)
					{
						obj.leaderboard = new DRLCircuitLeaderboardData[0];
					}
					p_callback?.Invoke(obj);
				}
			}, p_query, p_timeout);
		}

		public WebAsyncRequest GetCircuitLeaderboardUser(string p_playerId, string p_circuitId, int p_limit, int p_drone_class, bool? p_drone_official, int p_physics, Action<DRLLeaderboardResult> p_callback, string p_platform = null, string p_controller_type = null, string p_drone_guid = null, int p_timeout = -1)
		{
			DRLCircuitLeaderboardData dRLCircuitLeaderboardData = new DRLCircuitLeaderboardData();
			dRLCircuitLeaderboardData.playerId = (string.IsNullOrEmpty(p_playerId) ? playerId : p_playerId);
			dRLCircuitLeaderboardData.limit = p_limit;
			dRLCircuitLeaderboardData.diameter = p_drone_class;
			if (dRLCircuitLeaderboardData.diameter < 0)
			{
				dRLCircuitLeaderboardData.Remove("diameter");
			}
			dRLCircuitLeaderboardData.circuitId = p_circuitId;
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
				ServiceModel.AssertCustomFlags(dRLCircuitLeaderboardData, p_drone_official.Value, p_physics);
			}
			else
			{
				dRLCircuitLeaderboardData.Remove("drl-official");
				dRLCircuitLeaderboardData.customPhysics = p_physics != 0;
			}
			string p_endpoint = "/circuits/leaderboards/user/";
			dRLCircuitLeaderboardData["token"] = token;
			return Get("drl.service.circuit-leaderboards.user.read", p_endpoint, delegate(DRLServiceResult p_result)
			{
				DRLLeaderboardResult dRLLeaderboardResult = new DRLLeaderboardResult();
				if (!p_result.success)
				{
					dRLLeaderboardResult = new DRLLeaderboardResult
					{
						success = false
					};
					p_callback?.Invoke(dRLLeaderboardResult);
				}
				else
				{
					DRLCircuitsResult dRLCircuitsResult = Serialize.FromJson<DRLCircuitsResult>(p_result.data.ToString());
					if (dRLCircuitsResult.pagging == null)
					{
						dRLCircuitsResult.pagging = new DRLServicePageData();
					}
					if (dRLCircuitsResult.leaderboard == null)
					{
						dRLCircuitsResult.leaderboard = new DRLCircuitLeaderboardData[0];
					}
					if (dRLCircuitsResult.leaderboard.Length == 0)
					{
						p_callback(dRLLeaderboardResult);
					}
					else
					{
						DRLCircuitLeaderboardData dRLCircuitLeaderboardData2 = dRLCircuitsResult.leaderboard[0];
						dRLLeaderboardResult.pagging = dRLCircuitsResult.pagging;
						dRLLeaderboardResult.leaderboard = new DRLLeaderboardData[1];
						DRLLeaderboardData dRLLeaderboardData = new DRLLeaderboardData
						{
							playerId = dRLCircuitLeaderboardData2.playerId,
							limit = dRLCircuitLeaderboardData2.limit,
							diameter = dRLCircuitLeaderboardData2.diameter,
							score = dRLCircuitLeaderboardData2.score,
							drlOfficial = dRLCircuitLeaderboardData2.drlOfficial,
							droneGuid = dRLCircuitLeaderboardData2.droneGuid,
							position = dRLCircuitLeaderboardData2.position
						};
						dRLLeaderboardResult.leaderboard[0] = dRLLeaderboardData;
						p_callback?.Invoke(dRLLeaderboardResult);
					}
				}
			}, dRLCircuitLeaderboardData, p_timeout);
		}

		public WebAsyncRequest GetCircuitLeaderboardSpecificUser(string username, string p_circuitId, int p_limit, int p_drone_class, bool? p_drone_official, int p_physics, Action<DRLLeaderboardResult> p_callback, string p_platform = null, string p_controller_type = null, string p_drone_guid = null, int p_timeout = -1)
		{
			DRLCircuitLeaderboardData dRLCircuitLeaderboardData = new DRLCircuitLeaderboardData();
			dRLCircuitLeaderboardData.username = username;
			dRLCircuitLeaderboardData.limit = p_limit;
			dRLCircuitLeaderboardData.diameter = p_drone_class;
			if (dRLCircuitLeaderboardData.diameter < 0)
			{
				dRLCircuitLeaderboardData.Remove("diameter");
			}
			dRLCircuitLeaderboardData.circuitId = p_circuitId;
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
				ServiceModel.AssertCustomFlags(dRLCircuitLeaderboardData, p_drone_official.Value, p_physics);
			}
			else
			{
				dRLCircuitLeaderboardData.Remove("drl-official");
				dRLCircuitLeaderboardData.customPhysics = p_physics != 0;
			}
			dRLCircuitLeaderboardData["token"] = token;
			return Get("drl.service.circuit-leaderboards.user.specific", "/circuits/leaderboards", delegate(DRLServiceResult p_result)
			{
				DRLLeaderboardResult dRLLeaderboardResult = new DRLLeaderboardResult();
				if (!p_result.success)
				{
					dRLLeaderboardResult = new DRLLeaderboardResult
					{
						success = false
					};
					p_callback?.Invoke(dRLLeaderboardResult);
				}
				else
				{
					DRLCircuitsResult dRLCircuitsResult = Serialize.FromJson<DRLCircuitsResult>(p_result.data.ToString());
					if (dRLCircuitsResult.pagging == null)
					{
						dRLCircuitsResult.pagging = new DRLServicePageData();
					}
					if (dRLCircuitsResult.leaderboard == null)
					{
						dRLCircuitsResult.leaderboard = new DRLCircuitLeaderboardData[0];
					}
					if (dRLCircuitsResult.leaderboard.Length == 0)
					{
						p_callback(dRLLeaderboardResult);
					}
					else
					{
						DRLCircuitLeaderboardData dRLCircuitLeaderboardData2 = dRLCircuitsResult.leaderboard[0];
						dRLLeaderboardResult.pagging = dRLCircuitsResult.pagging;
						dRLLeaderboardResult.leaderboard = new DRLLeaderboardData[1];
						DRLLeaderboardData dRLLeaderboardData = new DRLLeaderboardData
						{
							playerId = dRLCircuitLeaderboardData2.playerId,
							limit = dRLCircuitLeaderboardData2.limit,
							diameter = dRLCircuitLeaderboardData2.diameter,
							score = dRLCircuitLeaderboardData2.score,
							drlOfficial = dRLCircuitLeaderboardData2.drlOfficial,
							droneGuid = dRLCircuitLeaderboardData2.droneGuid,
							droneRig = dRLCircuitLeaderboardData2.droneRig,
							droneThumb = dRLCircuitLeaderboardData2.droneThumb,
							droneName = dRLCircuitLeaderboardData2.droneName,
							position = dRLCircuitLeaderboardData2.position,
							profileName = dRLCircuitLeaderboardData2.profileName,
							profileThumbURL = dRLCircuitLeaderboardData2.profileThumbURL,
							profileColorHex = dRLCircuitLeaderboardData2.profileColorHex,
							controllerType = dRLCircuitLeaderboardData2.controllerType,
							flagThumbURL = dRLCircuitLeaderboardData2.flagThumbURL
						};
						dRLLeaderboardResult.leaderboard[0] = dRLLeaderboardData;
						p_callback?.Invoke(dRLLeaderboardResult);
					}
				}
			}, dRLCircuitLeaderboardData, p_timeout);
		}

		public void GetContentManifest(string p_branch, string p_platform, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string text = "https://api.drlgame.com";
			string p_endpoint = "@" + text + "/bundles/";
			SerializedData serializedData = new SerializedData();
			serializedData["token"] = token;
			serializedData["branch"] = p_branch;
			serializedData["platform"] = p_platform;
			Get("drl.service.content.manifest", p_endpoint, p_callback, serializedData, p_timeout);
		}

		public WebAsyncRequest SetCounterUAVCatchData(DRLCounterUAVData p_results, Action<DRLCounterUAVData> p_callback, int p_timeout = -1)
		{
			if (p_results == null)
			{
				Debug.LogWarning("DRLService> Tried to send empty counter uav data.");
				if (p_callback != null)
				{
					p_callback(null);
				}
				return null;
			}
			string text = cuavPath;
			text = text + "?token=" + token;
			return Post("drl.service.counter-uav.write", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("DRLService> Failed to send counter uav data!\n" + p_result.message);
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					DRLCounterUAVData obj = Serialize.FromJson<DRLCounterUAVData>((p_result.data == null) ? "[]" : p_result.data.ToString());
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, p_results, p_timeout);
		}

		public WebAsyncRequest GetCommunityDrones(DRLCommunityDroneData p_query, Action<DRLCommunityDroneResult> p_callback, int p_timeout = -1)
		{
			p_query["token"] = token;
			return Get("drl.service.drones.read", "/drones/", delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					DRLCommunityDroneResult obj = new DRLCommunityDroneResult();
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
				else
				{
					DRLCommunityDroneResult obj = Serialize.FromJson<DRLCommunityDroneResult>(p_result.data.ToString());
					if (obj.pagging == null)
					{
						obj.pagging = new DRLServicePageData();
					}
					if (obj.data == null)
					{
						obj.data = new DRLCommunityDroneData[0];
					}
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, p_query, p_timeout);
		}

		public WebAsyncRequest SetCommunityDrones(DRLCommunityDroneData p_query, Action<DRLCommunityDroneData> p_callback, int p_timeout = -1)
		{
			string text = "/drones/";
			text = text + "?token=" + token;
			return Post("drl.service.drones.write", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					DRLCommunityDroneData obj = null;
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
				else
				{
					DRLCommunityDroneData obj = Serialize.FromJson<DRLCommunityDroneData>(p_result.data.ToString());
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, p_query, p_timeout);
		}

		public WebAsyncRequest RemoveCommunityDrones(string p_guid, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string text = "/drones/";
			text = text + p_guid + "/remove/?token=" + token;
			return Get("drl.service.drones.remove", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					if (p_callback != null)
					{
						p_callback(p_result);
					}
				}
				else if (p_callback != null)
				{
					p_callback(p_result);
				}
			}, null, p_timeout);
		}

		public WebAsyncRequest SetCommunityDroneRating(string p_guid, float p_score, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string text = "/drones/";
			text = text + p_guid + "/rate/?token=" + token;
			DRLCommunityDroneData dRLCommunityDroneData = new DRLCommunityDroneData();
			dRLCommunityDroneData.guid = p_guid;
			dRLCommunityDroneData.score = p_score;
			return Post("drl.service.drones.rate.write", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("DRLService> Failed to send drone rating!\n" + p_result.message);
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else if (p_callback != null)
				{
					p_callback(p_result);
				}
			}, dRLCommunityDroneData, p_timeout);
		}

		public WebAsyncRequest SetCommunityDroneTime(string p_guid, string p_map, string p_track, string p_communityMap, string p_gameType, float p_time, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string text = "/drones/";
			text = text + p_guid + "/flight-time/?token=" + token;
			DRLCommunityDroneData dRLCommunityDroneData = new DRLCommunityDroneData();
			dRLCommunityDroneData.Set("time", p_time);
			if (!string.IsNullOrEmpty(p_map))
			{
				dRLCommunityDroneData.Set("map", p_map);
			}
			if (!string.IsNullOrEmpty(p_track))
			{
				dRLCommunityDroneData.Set("track", p_track);
			}
			if (!string.IsNullOrEmpty(p_communityMap))
			{
				dRLCommunityDroneData.Set("custom-map", p_communityMap);
			}
			if (!string.IsNullOrEmpty(p_gameType))
			{
				dRLCommunityDroneData.Set("game-type", p_gameType);
			}
			return Post("drl.service.drones.time.write", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("DRLService> Failed to send drone time!\n" + p_result.message);
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else if (p_callback != null)
				{
					p_callback(p_result);
				}
			}, dRLCommunityDroneData, p_timeout);
		}

		public WebAsyncRequest GetCommunityDroneTime(string p_guid, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string text = "/player/";
			text = text + "flight-time/?token=" + token + "&drone-guid=" + p_guid;
			return Get("drl.service.drones.time.read", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("DRLService> Failed to send drone time!\n" + p_result.message);
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else if (p_callback != null)
				{
					p_callback(p_result);
				}
			}, p_timeout);
		}

		public WebAsyncRequest GetThrottleCap(Action<DRLServiceResult> p_callback, string p_raceID = null, int p_timeout = -1)
		{
			string text = "/player/throttle";
			text = text + "?token=" + token;
			if (!string.IsNullOrEmpty(p_raceID))
			{
				text = text + "&race-id=" + p_raceID;
			}
			return Get("drl.service.drones.throttle-cap.read", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("DRLService> Failed to get throttle cap!\n" + p_result.message);
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else if (p_callback != null)
				{
					p_callback(p_result);
				}
			}, p_timeout);
		}

		public WebAsyncRequest GetCrashSettings(Action<DRLCrashPenaltyData> p_callback, int p_timeout = -1)
		{
			string text = "/crash-settings";
			text = text + "?token=" + token;
			return Get("drl.service.drones.crash-settings.read", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success || p_result == null || p_result.data == null)
				{
					Debug.LogWarning("DRLService> Failed to get crash settings!\n" + p_result.message);
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					string p_data = p_result.data.ToString();
					DRLCrashPenaltyData[] array = null;
					DRLCrashPenaltyData obj = null;
					try
					{
						array = Serialize.FromJson<DRLCrashPenaltyData[]>(p_data);
						if (array != null && array.Length != 0)
						{
							obj = array[0];
						}
					}
					catch (Exception message)
					{
						Debug.LogError(message);
					}
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, p_timeout);
		}

		public WebAsyncRequest GetLeaderboard(DRLLeaderboardData p_query, bool p_group, Action<DRLLeaderboardResult> p_callback, int p_timeout = -1, bool p_collectable = false)
		{
			string p_endpoint = (p_group ? "/leaderboards/group/" : "/leaderboards/");
			p_query["token"] = token;
			if (p_collectable)
			{
				p_query["game-type"] = "Collectable";
			}
			return Get("drl.service.leaderboards.read", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					DRLLeaderboardResult obj = new DRLLeaderboardResult
					{
						success = false
					};
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
				else
				{
					DRLLeaderboardResult obj = Serialize.FromJson<DRLLeaderboardResult>(p_result.data.ToString());
					if (obj.pagging == null)
					{
						obj.pagging = new DRLServicePageData();
					}
					if (obj.leaderboard == null)
					{
						obj.leaderboard = new DRLLeaderboardData[0];
					}
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, p_query, p_timeout);
		}

		public WebAsyncRequest GetLeaderboard(DRLTournamentLeaderboardParams p_params, int p_page, int p_limit, Action<DRLLeaderboardResult> p_callback, int p_timeout = -1, bool p_collectable = false)
		{
			DRLLeaderboardData dRLLeaderboardData = new DRLLeaderboardData();
			dRLLeaderboardData["token"] = token;
			dRLLeaderboardData["guid"] = p_params.guid;
			dRLLeaderboardData["match"] = p_params.match;
			if (p_collectable)
			{
				dRLLeaderboardData["game-type"] = "Collectable";
			}
			dRLLeaderboardData.page = p_page;
			dRLLeaderboardData.limit = p_limit;
			string p_endpoint = "/leaderboards/";
			return Get("drl.service.leaderboards.read", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					DRLLeaderboardResult obj = new DRLLeaderboardResult();
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
				else
				{
					DRLLeaderboardResult obj = Serialize.FromJson<DRLLeaderboardResult>(p_result.data.ToString());
					if (obj.pagging == null)
					{
						obj.pagging = new DRLServicePageData();
					}
					if (obj.leaderboard == null)
					{
						obj.leaderboard = new DRLLeaderboardData[0];
					}
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, dRLLeaderboardData, p_timeout);
		}

		public WebAsyncRequest GetLeaderboard(DRLLeaderboardData p_query, Action<DRLLeaderboardResult> p_callback, int p_timeout = -1, bool p_collectable = false)
		{
			return GetLeaderboard(p_query, p_group: false, p_callback, p_timeout, p_collectable);
		}

		public WebAsyncRequest GetLeaderboardsQuest(List<string> p_queries, Action<DRLLeaderboardData[]> p_callback, int p_timeout = -1)
		{
			string text = "/leaderboards/quest/";
			string text2 = "";
			text2 = text2 + "?token=" + token;
			for (int i = 0; i < p_queries.Count; i++)
			{
				text2 += "&";
				text2 = text2 + "mission[]=" + p_queries[i];
			}
			text += text2;
			return Get("drl.service.leaderboards-quest.read", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					DRLLeaderboardData[] obj = new DRLLeaderboardData[0];
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
				else
				{
					DRLLeaderboardData[] obj = Serialize.FromJson<DRLLeaderboardData[]>(p_result.data.ToString());
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, p_timeout);
		}

		public WebAsyncRequest SetLeaderboard(DRLLeaderboardData[] p_results, Action<DRLLeaderboardData[]> p_callback, int p_timeout = -1, bool p_collectable = false)
		{
			DRLLeaderboardData[] array = ((p_results == null) ? new DRLLeaderboardData[0] : p_results);
			if (array.Length == 0)
			{
				DRLLeaderboardData[] obj = new DRLLeaderboardData[0];
				Debug.LogWarning("DRLService> Tried to send empty leaderboard.");
				if (p_callback != null)
				{
					p_callback(obj);
				}
				return null;
			}
			string text = "/leaderboards/";
			text = text + "?token=" + token;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string value = Serialize.ToJson(array);
			dictionary["list"] = value;
			return Post("drl.service.leaderboards.write", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("DRLService> Failed to send leaderboards!\n" + p_result.message);
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					DRLLeaderboardData[] obj2 = Serialize.FromJson<DRLLeaderboardData[]>((p_result.data == null) ? "[]" : p_result.data.ToString());
					if (p_callback != null)
					{
						p_callback(obj2);
					}
				}
			}, dictionary, p_timeout);
		}

		public WebAsyncRequest SetLeaderboard(DRLLeaderboardData p_result, Action<DRLLeaderboardData> p_callback, int p_timeout = -1, bool p_collectable = false)
		{
			DRLLeaderboardData[] p_results = ((p_result == null) ? new DRLLeaderboardData[0] : new DRLLeaderboardData[1] { p_result });
			return SetLeaderboard(p_results, delegate(DRLLeaderboardData[] p_rl)
			{
				p_rl = ((p_rl == null) ? new DRLLeaderboardData[0] : p_rl);
				if (p_callback != null)
				{
					p_callback((p_rl.Length == 0) ? null : p_rl[0]);
				}
			}, p_timeout, p_collectable);
		}

		public WebAsyncRequest SetLeaderboard(DRLLeaderboardData[] p_result, Action<DRLLeaderboardData> p_callback, int p_timeout = -1)
		{
			DRLLeaderboardData[] p_results = ((p_result == null) ? new DRLLeaderboardData[0] : p_result);
			return SetLeaderboard(p_results, delegate(DRLLeaderboardData[] p_rl)
			{
				p_rl = ((p_rl == null) ? new DRLLeaderboardData[0] : p_rl);
				if (p_callback != null)
				{
					p_callback((p_rl.Length == 0) ? null : p_rl[0]);
				}
			}, p_timeout);
		}

		public WebAsyncRequest GetLeaderboardUser(DRLLeaderboardData p_query, Action<DRLLeaderboardResult> p_callback, int p_timeout = -1, bool p_collectable = false)
		{
			p_query["token"] = token;
			string p_endpoint = "/leaderboards/user/";
			if (p_collectable)
			{
				p_query["game-type"] = "Collectable";
			}
			return Get("drl.service.leaderboards.get-user", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					DRLLeaderboardResult obj = new DRLLeaderboardResult();
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
				else
				{
					DRLLeaderboardResult obj = Serialize.FromJson<DRLLeaderboardResult>(p_result.data.ToString());
					if (obj.pagging == null)
					{
						obj.pagging = new DRLServicePageData();
					}
					if (obj.leaderboard == null)
					{
						obj.leaderboard = new DRLLeaderboardData[0];
					}
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, p_query, p_timeout);
		}

		public WebAsyncRequest GetLeaderboardSpecificUser(DRLLeaderboardData p_query, Action<DRLLeaderboardResult> p_callback, bool isCustomMap, int p_timeout = -1, bool p_collectable = false)
		{
			p_query["token"] = token;
			if (p_collectable)
			{
				p_query["game-type"] = "Collectable";
			}
			if (isCustomMap)
			{
				p_query["is-custom-map"] = isCustomMap;
			}
			return Get(" drl.service.leaderboards.get-user.specific", "/leaderboards", delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					DRLLeaderboardResult obj = new DRLLeaderboardResult();
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
				else
				{
					DRLLeaderboardResult obj = Serialize.FromJson<DRLLeaderboardResult>(p_result.data.ToString());
					if (obj.pagging == null)
					{
						obj.pagging = new DRLServicePageData();
					}
					if (obj.leaderboard == null)
					{
						obj.leaderboard = new DRLLeaderboardData[0];
					}
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, p_query, p_timeout);
		}

		public WebAsyncRequest ResetLeaderboardUser(string p_playerID, int p_timeout = -1)
		{
			string text = "/leaderboards/user/reset/";
			text = text + "?token=" + token;
			return Post("drl.service.leaderboards.reset-user", text, delegate(DRLServiceResult p_result)
			{
				_ = p_result.success;
			}, p_playerID, p_timeout);
		}

		public WebAsyncRequest ResetTrackLeaderboardUser(string p_mapID, string p_trackID, string p_customMapID, bool p_isCustom, int p_timeout = -1)
		{
			string text = "/leaderboards/user/reset/track/";
			text = text + "?token=" + token;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("mapID", p_mapID);
			dictionary.Add("isCustom", p_isCustom.ToString());
			if (p_isCustom)
			{
				dictionary.Add("customMapId", p_customMapID);
				dictionary.Add("trackID", "");
			}
			else
			{
				dictionary.Add("customMapID", "");
				dictionary.Add("trackID", p_trackID);
			}
			return Post("drl.service.leaderboards.reset-track-user", text, delegate(DRLServiceResult p_result)
			{
				_ = p_result.success;
			}, dictionary, p_timeout);
		}

		public WebAsyncRequest GetLeaderboardRivals(DRLLeaderboardData p_query, Action<DRLLeaderboardRivalsResult> p_callback, int p_timeout = -1)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["token"] = token;
			dictionary.Merge(p_query);
			return Get("drl.service.leaderboards.get-rivals", "/leaderboards/rivals/", delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					DRLLeaderboardRivalsResult obj = new DRLLeaderboardRivalsResult();
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
				else
				{
					DRLLeaderboardRivalsResult obj = Serialize.FromJson<DRLLeaderboardRivalsResult>(p_result.data.ToString());
					if (obj == null)
					{
						obj = new DRLLeaderboardRivalsResult();
					}
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, dictionary, p_timeout);
		}

		public WebAsyncRequest ResetLeaderboardsQuest(List<string> p_queries, Action<DRLLeaderboardData[]> p_callback, int p_timeout = -1)
		{
			string text = "/leaderboards/quest/reset/";
			string text2 = "";
			text2 = text2 + "?token=" + token;
			for (int i = 0; i < p_queries.Count; i++)
			{
				text2 += "&";
				text2 = text2 + "mission[]=" + p_queries[i];
			}
			text += text2;
			return Get("drl.service.leaderboards-quest.read", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					DRLLeaderboardData[] obj = new DRLLeaderboardData[0];
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
				else
				{
					DRLLeaderboardData[] obj = Serialize.FromJson<DRLLeaderboardData[]>(p_result.data.ToString());
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, p_timeout);
		}

		public WebAsyncRequest License(Action<DRLLicenseResult> p_callback)
		{
			string text = "/player/license/";
			text = text + "?token=" + token;
			return Get("drl.service.license", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("DRLService> Failed to retrieve the license!\n" + p_result.message);
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					DRLLicenseResult data = p_result.GetData<DRLLicenseResult>();
					if (p_callback != null)
					{
						p_callback(data);
					}
				}
			});
		}

		public void Login(string p_auth_ticket, string p_platform_id, string p_app_version, Action<DRLServiceResult> p_callback, int p_timeout = -1, int retryCount = 0)
		{
			if (m_loginInProgress)
			{
				return;
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string text = ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
			string ivString = text.PadLeft(16, '0');
			dictionary.Add("UserID", p_platform_id);
			string prefix = OS.prefix;
			dictionary.Add(PlatformIdKey, p_platform_id);
			dictionary.Add("os", prefix);
			dictionary.Add("device-graphics-id", SystemInfo.graphicsDeviceID.ToString());
			dictionary.Add("device-unique-id", SystemInfo.deviceUniqueIdentifier);
			dictionary.Add("system-language", DRLApp.systemLocale);
			string text2 = SystemInfo.deviceName;
			string context = OS.context;
			switch (context)
			{
			case "xb":
				text2 = "XBoxOne";
				break;
			case "xbs":
				text2 = "XBoxOneS";
				break;
			case "xbx":
				text2 = "XBoxOneX";
				break;
			case "xbss":
				text2 = "XBoxSeriesS";
				break;
			case "xbsx":
				text2 = "XBoxSeriesX";
				break;
			case "ps4base":
				text2 = "Playstation4";
				break;
			case "ps4pro":
				text2 = (OS.IsPS5 ? "Playstation5" : "Playstation4Pro");
				break;
			case "standalone":
			case "editor":
				switch (OS.prefix)
				{
				case "win":
					text2 = "Windows";
					break;
				case "osx":
					text2 = "MacOS";
					break;
				case "linux":
					text2 = "Linux";
					break;
				}
				break;
			}
			if (!context.StartsWith("xb") && !context.StartsWith("ps4"))
			{
				text2 = text2 + " (" + SystemInfo.deviceName + ")";
			}
			string value = Convert.ToBase64String(AESCrypto.Encrypt(Serialize.ToJson(new DRLLoginData
			{
				platform = OS.GetPlatform(),
				uid = p_platform_id,
				version = p_app_version,
				time = text,
				checksum = OS.checksum
			}), ivString));
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			dictionary2.Add("token", value);
			dictionary2.Add("version", p_app_version);
			dictionary2.Add("time", text);
			dictionary2.Add("os", text2);
			Post("drl.service.login.v2", "/v2/login", delegate(DRLServiceResult result)
			{
				m_loginInProgress = false;
				if (result.success)
				{
					Debug.Log("Login > Login successful.");
					token = result.token;
					m_authTicket = p_auth_ticket;
					m_platformId = p_platform_id;
					m_appVersion = p_app_version;
					p_callback?.Invoke(result);
				}
				else if (IsTokenError(result) && retryCount < 5)
				{
					Login(p_auth_ticket, p_platform_id, p_app_version, p_callback, p_timeout, retryCount + 1);
				}
				else
				{
					p_callback?.Invoke(result);
				}
			}, dictionary2, p_timeout);
		}

		private bool IsTokenError(DRLServiceResult result)
		{
			if (string.IsNullOrEmpty(result.message))
			{
				return false;
			}
			if (result.message.Contains("not present") || result.message.Contains("invalid"))
			{
				return true;
			}
			return false;
		}

		public void Login(Action<DRLServiceResult> p_callback, int p_timeout, int p_retryCount)
		{
			if (string.IsNullOrEmpty(m_authTicket) || string.IsNullOrEmpty(m_platformId) || string.IsNullOrEmpty(m_appVersion))
			{
				p_callback?.Invoke(null);
			}
			else
			{
				Login(m_authTicket, m_platformId, m_appVersion, p_callback, p_timeout, p_retryCount);
			}
		}

		public WebAsyncRequest GetCommunityMaps(DRLCommunityMapData p_query, bool p_has_root, Action<DRLCommunityMapResult> p_callback, int p_timeout = -1, bool p_isCollectable = false)
		{
			p_query["token"] = token;
			if (!p_query.ContainsKey("root"))
			{
				p_query["root"] = p_has_root;
			}
			if (p_isCollectable)
			{
				p_query["game-type"] = "Collectable";
			}
			return Get("drl.service.maps.read", "/maps/", delegate(DRLServiceResult p_result)
			{
				DRLCommunityMapResult res;
				if (!p_result.success)
				{
					res = new DRLCommunityMapResult();
					res.success = p_result.success;
					if (p_callback != null)
					{
						p_callback(res);
					}
				}
				else
				{
					Debug.Log("DRLService> GetCommunityMaps Success");
					int parse_state = 0;
					string json_data = "";
					new Thread((ThreadStart)delegate
					{
						while (true)
						{
							string obj;
							switch (parse_state)
							{
							default:
								return;
							case 0:
								obj = ((p_result.data is string) ? ((string)p_result.data) : p_result.data.ToString());
								break;
							case 1:
								res = Serialize.FromJson<DRLCommunityMapResult>(json_data);
								if (res.pagging == null)
								{
									res.pagging = new DRLServicePageData();
								}
								if (res.data == null)
								{
									res.data = new DRLCommunityMapData[0];
								}
								Activity.RunOnce(delegate
								{
									if (p_callback != null)
									{
										p_callback(res);
									}
								}, 1f / 60f);
								return;
							}
							json_data = obj;
							parse_state = 1;
							Thread.Sleep(10);
						}
					}).Start();
				}
			}, p_query, p_timeout);
		}

		public WebAsyncRequest GetCommunityMap(DRLCommunityMapData p_query, Action<DRLCommunityMapResult> p_callback, int p_timeout = -1)
		{
			p_query["token"] = token;
			string p_endpoint = "/maps/" + p_query["guid"];
			p_query.Remove("guid");
			return Get("drl.service.map.load", p_endpoint, delegate(DRLServiceResult p_result)
			{
				DRLCommunityMapResult res;
				if (!p_result.success)
				{
					res = new DRLCommunityMapResult();
					if (p_callback != null)
					{
						p_callback(res);
					}
				}
				else
				{
					Debug.Log("DRLService> GetCommunityMap OK");
					new Thread((ThreadStart)delegate
					{
						string p_data = p_result.data.ToString();
						res = Serialize.FromJson<DRLCommunityMapResult>(p_data);
						if (res.pagging == null)
						{
							res.pagging = new DRLServicePageData();
						}
						if (res.data == null)
						{
							res.data = new DRLCommunityMapData[0];
						}
						Activity.RunOnce(delegate
						{
							if (p_callback != null)
							{
								p_callback(res);
							}
						}, 1f / 60f);
					}).Start();
				}
			}, p_query, p_timeout);
		}

		public WebAsyncRequest GetCommunityMaps(DRLCommunityMapData p_query, Action<DRLCommunityMapResult> p_callback, int p_timeout = -1, bool p_isCollectable = false)
		{
			return GetCommunityMaps(p_query, p_has_root: false, p_callback, p_timeout, p_isCollectable);
		}

		public void DownloadMapToFile(string p_guid, string p_fileLocation, Action<string, bool, float> p_callback)
		{
			string url = baseUri + "/maps/" + p_guid + "/?token=" + token + "&root=true";
			if (File.Exists(p_fileLocation))
			{
				File.Delete(p_fileLocation);
			}
			StartCoroutine(DownloadToFile(p_guid, url, p_callback, p_fileLocation));
		}

		private IEnumerator DownloadToFile(string guid, string url, Action<string, bool, float> p_callback, string p_fileLocation)
		{
			UnityWebRequest dlreq = new UnityWebRequest(url);
			using (DownloadHandlerFile dh = new DownloadHandlerFile(p_fileLocation))
			{
				dlreq.downloadHandler = dh;
				UnityWebRequestAsyncOperation op = dlreq.SendWebRequest();
				Debug.Log("Maps> Started downloading offline map..\n" + url);
				while (!op.isDone)
				{
					p_callback?.Invoke(guid, arg2: false, op.progress);
					yield return null;
				}
				if (dlreq.result != UnityWebRequest.Result.Success)
				{
					Debug.Log("Maps> Download error: " + dlreq.error);
				}
				dlreq.Dispose();
				p_callback?.Invoke(guid, arg2: true, 1f);
			}
			yield return null;
		}

		public WebAsyncRequest UpdateLocalMaps(Action<MapData[]> p_callback, bool p_full, bool p_community = false, int p_timeout = -1)
		{
			string text = (p_community ? "/maps/user/updated/" : "/maps/updated/");
			text = text + "?token=" + token;
			if (p_full)
			{
				text += "&full=true";
			}
			string p_id = (p_community ? "drl.service.map.local.user.update" : "drl.service.map.local.update");
			return GetRaw(p_id, text, delegate(string p_result)
			{
				if (string.IsNullOrEmpty(p_result))
				{
					p_callback?.Invoke(null);
				}
				else
				{
					Debug.Log("DRLService> GetMapUpdates OK");
					Thread thread = new Thread((ThreadStart)delegate
					{
						string text2 = p_result;
						string[] array = text2.Split(new char[1] { '[' }, 2);
						if (array.Length <= 1)
						{
							this.TimerRunOnce(delegate
							{
								p_callback?.Invoke(null);
							}, 1f / 30f);
						}
						else
						{
							text2 = array[1];
							text2 = text2.TrimEnd('}');
							text2 = text2.TrimEnd('}');
							text2 = "[" + text2;
							MapData[] m_data = null;
							try
							{
								m_data = Serialize.FromJson<MapData[]>(text2);
							}
							catch
							{
								Debug.Log("DRLService> GetMapUpdates failed to parse data.");
							}
							this.TimerRunOnce(delegate
							{
								p_callback?.Invoke(m_data);
							}, 1f / 60f);
						}
					});
					thread.Start();
					thread.Priority = System.Threading.ThreadPriority.AboveNormal;
				}
			}, p_timeout);
		}

		public WebAsyncRequest DownloadMap(string p_path, string p_url, Action<string, string> p_onComplete, int p_timeout = -1)
		{
			WebAsyncRequest webAsyncRequest = null;
			try
			{
				webAsyncRequest = Web.Get("drl.service.map.local.download", p_url, delegate(byte[] data, float progress, WebAsyncRequest request)
				{
					if (progress >= 1f)
					{
						p_onComplete?.Invoke(p_path, p_url);
					}
				});
				webAsyncRequest.loader.downloadHandler = new DownloadHandlerFile(p_path);
			}
			catch
			{
				p_onComplete?.Invoke(p_path, p_url);
			}
			return webAsyncRequest;
		}

		public WebAsyncRequest SyncLocalMapVersions(List<DRLCommunityMapVersionData> p_maps, Action<DRLServiceResult> p_callback, int p_timeout)
		{
			if (p_maps == null || p_maps.Count == 0)
			{
				if (p_callback != null)
				{
					p_callback(null);
				}
				return null;
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string value = Serialize.ToJson(p_maps.ToArray());
			dictionary["maps"] = value;
			string text = "/maps/updated/";
			text = text + "?token=" + token;
			return Post("drl.service.maps.local.sync", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					if (p_callback != null)
					{
						p_callback(p_result);
					}
				}
				else if (p_callback != null)
				{
					p_callback(p_result);
				}
			}, dictionary, p_timeout);
		}

		public WebAsyncRequest SetCommunityMaps(DRLCommunityMapData p_query, Action<DRLCommunityMapData> p_callback, int p_timeout = -1)
		{
			string text = "/maps/";
			text = text + "?token=" + token;
			return Post("drl.service.maps.write", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					DRLCommunityMapData obj = null;
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
				else
				{
					DRLCommunityMapData obj = Serialize.FromJson<DRLCommunityMapData>(p_result.data.ToString());
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, p_query, p_timeout);
		}

		public WebAsyncRequest CloneCommunityMap(string p_guid, Action<DRLCommunityMapData> p_callback, int p_timeout = -1)
		{
			string text = "/maps/";
			string text2 = p_guid + "/duplicate";
			text = text + text2 + "?token=" + token;
			return Post("drl.service.maps.write", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					DRLCommunityMapData obj = null;
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
				else
				{
					DRLCommunityMapData obj = Serialize.FromJson<DRLCommunityMapData>(p_result.data.ToString());
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, p_timeout);
		}

		public WebAsyncRequest SetCommunityMaps(string p_localFilepath, DRLCommunityMapData p_query, Action<string, DRLCommunityMapData> p_callback, int p_timeout = -1)
		{
			string text = "/maps/";
			text = text + "?token=" + token;
			return Post("drl.service.maps.write", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					DRLCommunityMapData arg = null;
					if (p_callback != null)
					{
						p_callback(p_localFilepath, arg);
					}
				}
				else
				{
					DRLCommunityMapData arg = Serialize.FromJson<DRLCommunityMapData>(p_result.data.ToString());
					if (p_callback != null)
					{
						p_callback(p_localFilepath, arg);
					}
				}
			}, p_query, p_timeout);
		}

		public WebAsyncRequest RemoveCommunityMaps(string p_guid, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string text = "/maps/";
			text = text + p_guid + "/remove/?token=" + token;
			Debug.Log("DRLService> RemoveCommunityMaps [" + text + "]");
			return Get("drl.service.maps.remove", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					if (p_callback != null)
					{
						p_callback(p_result);
					}
				}
				else if (p_callback != null)
				{
					p_callback(p_result);
				}
			}, null, p_timeout);
		}

		public WebAsyncRequest SetCommunityMapRating(string p_guid, float p_score, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string text = "/maps/";
			text = text + p_guid + "/rate/";
			text = text + "?token=" + token;
			Debug.Log("DRLService> SetCommunityMapRating [" + text + "]");
			DRLCommunityMapData dRLCommunityMapData = new DRLCommunityMapData();
			dRLCommunityMapData.guid = p_guid;
			dRLCommunityMapData.score = p_score;
			return Post("drl.service.maps.rate.write", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("DRLService> Failed to send rating!\n" + p_result.message);
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else if (p_callback != null)
				{
					p_callback(p_result);
				}
			}, dRLCommunityMapData, p_timeout);
		}

		public WebAsyncRequest GetCommunityMapRating(string p_guid, Action<DRLServiceResult> p_callback, int p_timeout = -1, bool p_isCollectable = false)
		{
			string text = "/maps/";
			text = text + p_guid + "/rate/";
			text = text + "?token=" + token;
			if (p_isCollectable)
			{
				text += "?game-type=Collectable";
			}
			Debug.Log("DRLService> SetCommunityMapRating [" + text + "]");
			return Get("drl.service.maps.rate.read", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("DRLService> Failed to get rating!\n" + p_result.message);
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else if (p_callback != null)
				{
					p_callback(p_result);
				}
			}, null, p_timeout);
		}

		public WebAsyncRequest GetSDMaps(DRLCommunityMapData p_query, bool p_has_root, Action<DRLCommunityMapResult> p_callback, int p_timeout = -1)
		{
			p_query["token"] = token;
			if (!p_query.ContainsKey("root"))
			{
				p_query["root"] = p_has_root;
			}
			p_query["game-type"] = "Collectable";
			p_query["map-category"] = "MapDRL";
			return Get("drl.service.maps.read", "/maps/", delegate(DRLServiceResult p_result)
			{
				DRLCommunityMapResult res;
				if (!p_result.success)
				{
					res = new DRLCommunityMapResult();
					if (p_callback != null)
					{
						p_callback(res);
					}
				}
				else
				{
					int parse_state = 0;
					string json_data = "";
					new Thread((ThreadStart)delegate
					{
						while (true)
						{
							string obj;
							switch (parse_state)
							{
							default:
								return;
							case 0:
								obj = ((p_result.data is string) ? ((string)p_result.data) : p_result.data.ToString());
								break;
							case 1:
								res = Serialize.FromJson<DRLCommunityMapResult>(json_data);
								if (res.pagging == null)
								{
									res.pagging = new DRLServicePageData();
								}
								if (res.data == null)
								{
									res.data = new DRLCommunityMapData[0];
								}
								Activity.RunOnce(delegate
								{
									if (p_callback != null)
									{
										p_callback(res);
									}
								}, 1f / 60f);
								return;
							}
							json_data = obj;
							parse_state = 1;
							Thread.Sleep(10);
						}
					}).Start();
				}
			}, p_query, p_timeout);
		}

		public WebAsyncRequest GetMultiplayerBots(DRLReplayData p_query, Action<DRLLeaderboardData[]> p_callback, int p_timeout = -1)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["token"] = token;
			dictionary.Merge(p_query);
			return Get("drl.service.multiplayer.get-bots", "/multiplayer/bots/", delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("DRLService> Failed to get replay bots!\n" + p_result.message);
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					DRLLeaderboardData[] obj = Serialize.FromJson<DRLLeaderboardData[]>((p_result.data == null) ? "[]" : p_result.data.ToString());
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, dictionary, p_timeout);
		}

		public WebAsyncRequest GetNotifications(Action<DRLNotificationsData[]> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/notifications/";
			SerializedData serializedData = new SerializedData();
			serializedData["token"] = token;
			return Get("drl.service.notifications.get", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					object data = p_result.data;
					if (data == null)
					{
						Debug.LogWarning("Notifications> GetNotifications / Failed!");
					}
					else
					{
						DRLNotificationsData[] obj = Serialize.FromJson<DRLNotificationsData[]>(data.ToString());
						if (p_callback != null)
						{
							p_callback(obj);
						}
					}
				}
			}, serializedData, p_timeout);
		}

		public WebAsyncRequest MarkNotificationRead(string p_id, int p_timeout = -1)
		{
			string p_endpoint = "/notifications/read/" + p_id;
			SerializedData serializedData = new SerializedData();
			serializedData["token"] = token;
			return Get("drl.service.notifications.mark-read", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("Notifications> MarkNotificationRead / Failed!");
				}
				else
				{
					Debug.Log("Notifications> MarkNotificationRead / Success!");
				}
			}, serializedData, p_timeout);
		}

		public WebAsyncRequest GetCommunityTunes(DRLCommunityTuneData p_query, Action<DRLCommunityTuneResult> p_callback, int p_timeout = -1)
		{
			p_query["token"] = token;
			return Get("drl.service.physics-tunes.read", "/physics-tunes/", delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					DRLCommunityTuneResult obj = new DRLCommunityTuneResult();
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
				else
				{
					string[] array = p_result.data.ToString().Split(new string[1] { "\"data\": {" }, StringSplitOptions.None);
					string text = array[0];
					for (int i = 1; i < array.Length; i++)
					{
						text = text + "\"data\": \"{" + array[i].Substring(0, array[i].IndexOf('}') + 1).Replace("\"", "\\\"") + "\"" + array[i].Substring(array[i].IndexOf('}') + 1);
					}
					DRLCommunityTuneResult obj = Serialize.FromJson<DRLCommunityTuneResult>(text);
					if (obj.pagging == null)
					{
						obj.pagging = new DRLServicePageData();
					}
					if (obj.data == null)
					{
						obj.data = new DRLCommunityTuneData[0];
					}
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, p_query, p_timeout);
		}

		public WebAsyncRequest SetCommunityTunes(DRLCommunityTuneData p_query, Action<DRLCommunityTuneData> p_callback, int p_timeout = -1)
		{
			string text = "/physics-tunes/";
			text = text + "?token=" + token;
			return Post("drl.service.physics-tunes.write", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					DRLCommunityTuneData obj = null;
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
				else
				{
					DRLCommunityTuneData obj = Serialize.FromJson<DRLCommunityTuneData>(p_result.data.ToString());
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, p_query, p_timeout);
		}

		public WebAsyncRequest RemoveCommunityTune(string p_guid, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string text = "/physics-tunes/remove/";
			text = text + p_guid + "/?token=" + token;
			Debug.Log("DRLService> RemoveCommunityTune [" + text + "]");
			return Get("drl.service.physics-tunes.remove", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					if (p_callback != null)
					{
						p_callback(p_result);
					}
				}
				else if (p_callback != null)
				{
					p_callback(p_result);
				}
			}, null, p_timeout);
		}

		public WebAsyncRequest SetCommunityTuneRating(string p_guid, float p_score, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string text = "/physics-tunes/rate/";
			text = text + "?token=" + token;
			DRLCommunityTuneData dRLCommunityTuneData = new DRLCommunityTuneData();
			dRLCommunityTuneData.guid = p_guid;
			dRLCommunityTuneData.score = p_score;
			return Post("drl.service.physics-tune.rate.write", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("DRLService> Failed to send tune rating!\n" + p_result.message);
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else if (p_callback != null)
				{
					p_callback(p_result);
				}
			}, dRLCommunityTuneData, p_timeout);
		}

		public static WebAsyncRequest ValidateMessage(string p_message, Action<string> p_onComplete, int p_timeout = -1)
		{
			string p_url = "https://www.purgomalum.com/service/json?text=" + p_message;
			return Web.Get("drl.service.chat.profanity", p_url, delegate(string data, float progress, WebAsyncRequest request)
			{
				if (progress >= 1f)
				{
					if (!request.hasError && request.code == 200)
					{
						try
						{
							DRLChatProfanityResult dRLChatProfanityResult = Serialize.FromJson<DRLChatProfanityResult>(data);
							if (dRLChatProfanityResult == null || string.IsNullOrEmpty(dRLChatProfanityResult.result))
							{
								p_onComplete?.Invoke(p_message);
							}
							else
							{
								p_onComplete?.Invoke(dRLChatProfanityResult.result);
							}
							return;
						}
						catch
						{
							p_onComplete?.Invoke(p_message);
							return;
						}
					}
					Debug.Log("ProfanityFiltering> Error: " + request.error);
					p_onComplete?.Invoke(p_message);
				}
			}, p_timeout);
		}

		public WebAsyncRequest GetPlayerProgression(Action<DRLProgressionStateData> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/experience-points/progression/";
			SerializedData serializedData = new SerializedData();
			serializedData["token"] = token;
			return Get("drl.service.progression.player", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					object data = p_result.data;
					if (data == null)
					{
						Debug.LogWarning("Progression> GetPlayerProgression / Failed!");
						if (p_callback != null)
						{
							p_callback(null);
						}
					}
					else
					{
						DRLProgressionStateData obj = Serialize.FromJson<DRLProgressionStateData>(data.ToString());
						if (p_callback != null)
						{
							p_callback(obj);
						}
					}
				}
			}, serializedData, p_timeout);
		}

		public WebAsyncRequest GetProgressionMaps(Action<DRLProgressionTrackData[]> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/progression/maps/";
			SerializedData serializedData = new SerializedData();
			serializedData["token"] = token;
			return Get("drl.service.progression.tracks", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					object data = p_result.data;
					if (data == null)
					{
						Debug.LogWarning("Progression> GetProgressionMaps / Failed!");
						if (p_callback != null)
						{
							p_callback(null);
						}
					}
					else
					{
						DRLProgressionTrackData[] obj = Serialize.FromJson<DRLProgressionTrackData[]>(data.ToString());
						if (p_callback != null)
						{
							p_callback(obj);
						}
					}
				}
			}, serializedData, p_timeout);
		}

		public WebAsyncRequest GetProgressionWeekRank(Action<DRLProgressionRankResult> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/experience-points/ranking/";
			SerializedData serializedData = new SerializedData();
			serializedData["token"] = token;
			return Get("drl.service.progression.week-rank", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					object data = p_result.data;
					if (data == null)
					{
						Debug.LogWarning("Progression> GetProgressionWeekRank / Failed!");
						if (p_callback != null)
						{
							p_callback(null);
						}
					}
					else
					{
						string text = data.ToString();
						Debug.Log("Progression> GetProgressionWeekRank / result\n" + text);
						DRLProgressionRankResult obj = Serialize.FromJson<DRLProgressionRankResult>(text);
						if (p_callback != null)
						{
							p_callback(obj);
						}
					}
				}
			}, serializedData, p_timeout);
		}

		public WebAsyncRequest SetReplay(DRLReplayData p_query, Action<DRLReplayData[]> p_callback, int p_timeout = -1, int p_retry = 4)
		{
			string text = "/replay/";
			text = text + "?token=" + token;
			WWWForm p_data = p_query.ToForm();
			return Post("drl.service.replay.write", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning($"DRLService> SetReplay / Failed to send replay - Retry[{p_retry}]!\n{p_result.message}");
					if (p_retry <= 0)
					{
						if (p_callback != null)
						{
							p_callback(null);
						}
					}
					else
					{
						SetReplay(p_query, p_callback, p_timeout, p_retry - 1);
					}
				}
				else
				{
					DRLReplayData[] obj = Serialize.FromJson<DRLReplayData[]>((p_result.data == null) ? "[]" : p_result.data.ToString());
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, p_data, p_timeout);
		}

		public WebAsyncRequest GetReplay(DRLReplayData p_query, bool p_all, Action<DRLReplayData[]> p_callback, int p_timeout = -1)
		{
			string text = "/replay/";
			if (p_all)
			{
				text += "all/";
			}
			text = text + "?token=" + token;
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Merge(p_query);
			return Post("drl.service.replay.read", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("DRLService> Failed to get replay!\n" + p_result.message);
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					DRLReplayData[] obj = Serialize.FromJson<DRLReplayData[]>((p_result.data == null) ? "[]" : p_result.data.ToString());
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, dictionary, p_timeout);
		}

		public WebAsyncRequest GetReplayRivals(DRLReplayData p_query, Action<DRLLeaderboardRivalsResult> p_callback, int p_timeout = -1)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["token"] = token;
			dictionary.Merge(p_query);
			return Get("drl.service.replay.get-rivals", "/replay/rivals/", delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("DRLService> Failed to get replay rivals!\n" + p_result.message);
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					string obj = ((p_result.data == null) ? "[]" : p_result.data.ToString());
					Debug.Log(obj);
					DRLLeaderboardRivalsResult obj2 = Serialize.FromJson<DRLLeaderboardRivalsResult>(obj);
					if (p_callback != null)
					{
						p_callback(obj2);
					}
				}
			}, dictionary, p_timeout);
		}

		public WebAsyncRequest GetReplayOnboarding(string p_replay_id, OnboardingCampaignMode onboardingCampaignMode, Action<OnboardingRaceReplayData[]> p_callback, int p_timeout = -1)
		{
			Dictionary<string, object> p_data = new Dictionary<string, object>();
			string replayUrl = GetReplayUrl(onboardingCampaignMode);
			return Get("drl.service.replay-onboarding.get-replays", replayUrl, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("DRLService> Failed to get replay rivals!\n" + p_result.message);
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					string obj = ((p_result.data == null) ? "[]" : p_result.data.ToString());
					Debug.Log(obj);
					OnboardingRaceReplayData[] obj2 = Serialize.FromJson<OnboardingRaceReplayData[]>(obj);
					if (p_callback != null)
					{
						p_callback(obj2);
					}
				}
			}, p_data, p_timeout);
		}

		public WebAsyncRequest ServerTime(Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			return Get("drl.service.time", "/time/", p_callback, null, p_timeout);
		}

		public WebAsyncRequest GetSocialProfile(string[] p_ids, Action<DRLPlayerProfileData[]> p_callback, int p_timeout = -1)
		{
			bool flag = true;
			m_socialProfileResult.Clear();
			m_socialProfileRequestIds.Clear();
			for (int i = 0; i < p_ids.Length; i++)
			{
				if (m_socialProfileCache.ContainsKey(p_ids[i]))
				{
					m_socialProfileResult.Add(m_socialProfileCache[p_ids[i]]);
					continue;
				}
				m_socialProfileRequestIds.Add(p_ids[i]);
				flag = false;
			}
			Debug.Log("Social.Profile> Retrieving social profile from cache - success[" + flag + "]");
			if (flag)
			{
				p_callback?.Invoke(m_socialProfileResult.ToArray());
				return null;
			}
			SerializedData serializedData = new SerializedData();
			serializedData["token"] = token;
			serializedData.SetWebArray("steam-ids", m_socialProfileRequestIds.ToArray());
			return Get("drl.service.social.profile", "/social/profile/", delegate(DRLServiceResult p_result)
			{
				DRLPlayerProfileData[] array = new DRLPlayerProfileData[0];
				if (p_result == null)
				{
					Debug.LogWarning("DRLService> GetSocialProfile - Error");
					if (p_callback != null)
					{
						p_callback(array);
					}
				}
				else
				{
					array = (p_result.success ? p_result.GetData<DRLPlayerProfileData[]>() : array);
					for (int j = 0; j < array.Length; j++)
					{
						if (array[j] != null && !string.IsNullOrEmpty(array[j].platformId))
						{
							if (!m_socialProfileCache.ContainsKey(array[j].platformId))
							{
								m_socialProfileCache.Add(array[j].platformId, array[j]);
							}
							m_socialProfileResult.Add(array[j]);
						}
					}
					if (p_callback != null)
					{
						p_callback(m_socialProfileResult.ToArray());
					}
				}
			}, serializedData, p_timeout);
		}

		public WebAsyncRequest GetSocialProfile(string p_id, Action<DRLPlayerProfileData[]> p_callback, int p_timeout = -1)
		{
			return GetSocialProfile(new string[1] { p_id }, p_callback, p_timeout);
		}

		public WebAsyncRequest GetTwitchLiveStatus(Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			SerializedData serializedData = new SerializedData();
			serializedData["token"] = token;
			return Get("drl.service.social.twitch", "/social/twitch/", delegate(DRLServiceResult p_result)
			{
				if (p_result == null)
				{
					Debug.LogWarning("DRLService> GetTwitchLiveStatus - Error");
					if (p_callback != null)
					{
						p_callback(p_result);
					}
				}
				else if (p_callback != null)
				{
					p_callback(p_result);
				}
			}, serializedData, p_timeout);
		}

		public WebAsyncRequest GetOnlineUserCount(Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			SerializedData serializedData = new SerializedData();
			serializedData["token"] = token;
			return Get("drl.service.social.online-users", "/social/online-players/", delegate(DRLServiceResult p_result)
			{
				if (p_result == null)
				{
					Debug.LogWarning("DRLService> GetOnlineUserCount - Error");
					if (p_callback != null)
					{
						p_callback(p_result);
					}
				}
				else if (p_callback != null)
				{
					p_callback(p_result);
				}
			}, serializedData, p_timeout);
		}

		public void State(object p_query, Action<DRLServiceResult> p_callback, Dictionary<string, string> p_data = null, int p_timeout = -1)
		{
			if (p_data != null)
			{
				SetPlayerState(p_callback, p_data, p_timeout);
			}
			else if (((p_query is string text) ? text : null) == null)
			{
				GetGameState(p_callback, null, p_timeout);
			}
			else
			{
				GetPlayerState(p_callback, null, p_timeout);
			}
		}

		protected void GetGameState(Action<DRLServiceResult> p_callback, Dictionary<string, string> p_query = null, int p_timeout = -1)
		{
			string text = "/state/game/";
			text = text + "?token=" + token;
			Get("drl.service.game-state", text, p_callback, null, p_timeout);
		}

		protected void GetPlayerState(Action<DRLServiceResult> p_callback, Dictionary<string, string> p_query = null, int p_timeout = -1)
		{
			string text = "/state/";
			text = text + "?token=" + token;
			Get("drl.service.player-state.read", text, p_callback, null, p_timeout);
		}

		protected void SetPlayerState(Action<DRLServiceResult> p_callback, Dictionary<string, string> p_data = null, int p_timeout = -1)
		{
			string text = "/state/";
			text = text + "?token=" + token;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("state", Serialize.ToJson(p_data));
			Post("drl.service.player-state.write", text, p_callback, dictionary, p_timeout);
		}

		public WebAsyncRequest Storage(string p_mode, string p_category, byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			string text = "/storage/" + p_mode + "/";
			text = text + "?token=" + token;
			byte[] contents = ((p_data == null) ? new byte[1] : p_data);
			WWWForm wWWForm = new WWWForm();
			wWWForm.AddField("category", p_category);
			wWWForm.AddBinaryData("file", contents);
			return Post("drl.service.storage" + (string.IsNullOrEmpty(p_mode) ? "" : ("." + p_mode)), text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("DRLService> Failed to send data!\n" + p_result.message);
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					string obj = ((p_result.data == null) ? "" : p_result.data.ToString());
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, wWWForm, p_timeout);
		}

		public WebAsyncRequest Storage(string p_mode, string p_category, string p_localFilepath, byte[] p_data, Action<string, string> p_callback, int p_timeout = -1)
		{
			string text = "/storage/" + p_mode + "/";
			text = text + "?token=" + token;
			byte[] contents = ((p_data == null) ? new byte[1] : p_data);
			WWWForm wWWForm = new WWWForm();
			wWWForm.AddField("category", p_category);
			wWWForm.AddBinaryData("file", contents);
			return Post("drl.service.storage" + (string.IsNullOrEmpty(p_mode) ? "" : ("." + p_mode)), text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("DRLService> Failed to send data!\n" + p_result.message);
					if (p_callback != null)
					{
						p_callback(null, null);
					}
				}
				else
				{
					string arg = ((p_result.data == null) ? "" : p_result.data.ToString());
					if (p_callback != null)
					{
						p_callback(p_localFilepath, arg);
					}
				}
			}, wWWForm, p_timeout);
		}

		public WebAsyncRequest StorageReplayCloud(string p_score_id, byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			string text = "/storage/replay-cloud/";
			text = text + "?token=" + token;
			byte[] contents = ((p_data == null) ? new byte[1] : p_data);
			WWWForm wWWForm = new WWWForm();
			wWWForm.AddField("category", "");
			wWWForm.AddField("score-id", p_score_id);
			wWWForm.AddBinaryData("file", contents);
			return Post("drl.service.storage" + (string.IsNullOrEmpty("replay") ? "" : ".replay"), text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("DRLService> Failed to send data!\n" + p_result.message);
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					string obj = ((p_result.data == null) ? "" : p_result.data.ToString());
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, wWWForm, p_timeout);
		}

		public WebAsyncRequest StorageReplayCloud(string p_score_id, byte[] p_data, string p_map, string p_track, string p_customMap, int p_diameter, float p_score, string p_matchId, Action<string> p_callback, int p_timeout = -1, int p_retry = 4)
		{
			string text = "/storage/replay-cloud/";
			text = text + "?token=" + token;
			byte[] contents = ((p_data == null) ? new byte[1] : p_data);
			WWWForm wWWForm = new WWWForm();
			wWWForm.AddField("category", "");
			wWWForm.AddField("score-id", p_score_id);
			wWWForm.AddBinaryData("file", contents);
			wWWForm.AddField("map", p_map);
			wWWForm.AddField("track", p_track);
			wWWForm.AddField("custom-map", p_customMap);
			wWWForm.AddField("diameter", p_diameter.ToString());
			wWWForm.AddField("score", p_score.ToString());
			wWWForm.AddField("match-id", p_matchId);
			return Post("drl.service.storage" + (string.IsNullOrEmpty("replay") ? "" : ".replay"), text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning($"DRLService> SetReplay / Failed to send replay - Retry[{p_retry}]!\n{p_result.message}");
					if (p_retry <= 0)
					{
						if (p_callback != null)
						{
							p_callback(null);
						}
					}
					else
					{
						StorageReplayCloud(p_score_id, p_data, p_map, p_track, p_customMap, p_diameter, p_score, p_matchId, p_callback, p_timeout, p_retry - 1);
					}
				}
				else
				{
					string obj = ((p_result.data == null) ? "" : p_result.data.ToString());
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, wWWForm, p_timeout);
		}

		public WebAsyncRequest Storage(string p_category, byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			return Storage("", p_category, p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest Storage(byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			return Storage("", "", p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest StorageTemp(string p_category, byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			return Storage("temp", p_category, p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest StorageTemp(string p_category, string p_localFilepath, byte[] p_data, Action<string, string> p_callback, int p_timeout = -1)
		{
			return Storage("temp", p_category, p_localFilepath, p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest StorageTemp(byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			return Storage("temp", "", p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest StorageImage(string p_category, byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			return Storage("image", p_category, p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest StorageImage(string p_category, string p_localFilepath, byte[] p_data, Action<string, string> p_callback, int p_timeout = -1)
		{
			return Storage("image", p_category, p_localFilepath, p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest StorageImage(byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			return Storage("image", "", p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest StorageReplay(string p_category, byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			return Storage("replay", p_category, p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest StorageReplay(byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			return Storage("replay", "", p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest StorageLogs(byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			return Storage("logs", "", p_data, p_callback, p_timeout);
		}

		public WebAsyncRequest GetImage(string p_image_url, int p_width, int p_height, Action<Texture2D> p_callback, int p_timeout = -1)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["token"] = token;
			dictionary["url"] = p_image_url;
			if (p_width > 0)
			{
				dictionary["w"] = p_width;
			}
			if (p_height > 0)
			{
				dictionary["h"] = p_height;
			}
			Dictionary<string, string> dictionary2 = null;
			if (!string.IsNullOrEmpty(webtoken))
			{
				dictionary2 = new Dictionary<string, string>();
				dictionary2["webtoken"] = webtoken;
			}
			string p_url = baseUri + "/images/";
			return Web.Get("drl.service.images", p_url, delegate(Texture2D p_data, float p_progress, WebAsyncRequest p_req)
			{
				if (p_progress >= 1f && p_callback != null)
				{
					p_callback(p_data);
				}
			}, dictionary, dictionary2);
		}

		public WebAsyncRequest SetPlayerAvatar(byte[] p_data, Action<string> p_callback, int p_timeout = -1)
		{
			string text = "/player/avatar/";
			text = text + "?token=" + token;
			byte[] contents = ((p_data == null) ? new byte[1] : p_data);
			WWWForm wWWForm = new WWWForm();
			wWWForm.AddBinaryData("file", contents);
			return Post("drl.service.player.avatar.write", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("DRLService> Failed to send data!\n" + p_result.message);
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					string obj = ((p_result.data == null) ? "" : p_result.data.ToString());
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, wWWForm, p_timeout);
		}

		public string GetPlayerAvatarURL(string p_id)
		{
			return baseUri + "/player/avatar/" + p_id + "/?token=" + token;
		}

		public WebAsyncRequest GetPlayerAvatar(string p_id, Action<Texture2D> p_callback, int p_timeout = -1)
		{
			if (string.IsNullOrEmpty(p_id))
			{
				Debug.LogWarning("DRLService> Storage.GetPlayerAvatar / Player Id is <empty>\n" + StackTraceUtility.ExtractStackTrace());
				return null;
			}
			if (p_id.Contains("http"))
			{
				Debug.LogWarning("DRLService> Storage.GetPlayerAvatar / Player Id is invalid\n" + StackTraceUtility.ExtractStackTrace());
				return null;
			}
			string playerAvatarURL = GetPlayerAvatarURL(p_id);
			return Web.Get("drl.service.player.avatar.read", playerAvatarURL, delegate(Texture2D p_data, float p_progress, WebAsyncRequest p_req)
			{
				if (p_progress >= 1f && p_callback != null)
				{
					p_callback(p_data);
				}
			}, null, null, p_timeout);
		}

		public WebAsyncRequest GetPlayerAvatarOnboarding(string p_id, Action<Texture2D> p_callback, int p_timeout = -1)
		{
			if (string.IsNullOrEmpty(p_id))
			{
				Debug.LogWarning("DRLService> Storage.GetPlayerAvatar / Player Id is <empty>\n" + StackTraceUtility.ExtractStackTrace());
				return null;
			}
			return Web.Get(p_id, delegate(Texture2D p_data, float p_progress, WebAsyncRequest p_req)
			{
				if (p_progress >= 1f && p_callback != null)
				{
					p_callback(p_data);
				}
			}, null, null, p_timeout);
		}

		public WebAsyncRequest GetStoreProducts(DRLStoreData p_query, Action<DRLStoreResult> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/store/";
			SerializedData serializedData = new SerializedData();
			serializedData["token"] = token;
			serializedData.Merge(p_query);
			return Get("drl.service.store.products", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					DRLStoreResult obj = new DRLStoreResult
					{
						success = false
					};
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
				else
				{
					DRLStoreResult obj = Serialize.FromJson<DRLStoreResult>(p_result.data.ToString());
					if (obj.pagging == null)
					{
						obj.pagging = new DRLServicePageData();
						obj.pagging.page = 1;
					}
					if (obj.data == null)
					{
						obj.data = new DRLStoreProductData[0];
					}
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, serializedData, p_timeout);
		}

		public WebAsyncRequest GetTimers(string[] p_ids, Action<DRLTimerData[]> p_callback, int p_timeout = -1)
		{
			SerializedData serializedData = new SerializedData();
			serializedData["token"] = token;
			serializedData.SetWebArray("timers", p_ids);
			return Get("drl.service.timer", "/timer/", delegate(DRLServiceResult p_result)
			{
				DRLTimerData[] array = new DRLTimerData[0];
				if (p_result == null)
				{
					Debug.LogWarning("DRLService> GetTimers - Error");
					if (p_callback != null)
					{
						p_callback(array);
					}
				}
				else
				{
					array = (p_result.success ? p_result.GetData<DRLTimerData[]>() : array);
					if (p_callback != null)
					{
						p_callback(array);
					}
				}
			}, serializedData, p_timeout);
		}

		public WebAsyncRequest GetTimers(string p_id, Action<DRLTimerData[]> p_callback, int p_timeout = -1)
		{
			return GetTimers(new string[1] { p_id }, p_callback, p_timeout);
		}

		public WebAsyncRequest StartTimer(string p_id, Action<DRLTimerData> p_callback, int p_timeout = -1)
		{
			SerializedData serializedData = new SerializedData();
			serializedData["token"] = token;
			string p_endpoint = "/timer/start/" + p_id + "/";
			return Get("drl.service.timer.start", p_endpoint, delegate(DRLServiceResult p_result)
			{
				DRLTimerData obj = null;
				if (p_result == null)
				{
					Debug.LogWarning("DRLService> StartTimer - Error");
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
				else
				{
					obj = (p_result.success ? p_result.GetData<DRLTimerData>() : null);
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, serializedData, p_timeout);
		}

		public WebAsyncRequest StopTimer(string p_id, Action<DRLTimerData> p_callback, int p_timeout = -1)
		{
			SerializedData serializedData = new SerializedData();
			serializedData["token"] = token;
			string p_endpoint = "/timer/stop/" + p_id + "/";
			return Get("drl.service.timer.stop", p_endpoint, delegate(DRLServiceResult p_result)
			{
				DRLTimerData obj = null;
				if (p_result == null)
				{
					Debug.LogWarning("DRLService> StopTimer - Error");
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
				else
				{
					obj = (p_result.success ? p_result.GetData<DRLTimerData>() : null);
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, serializedData, p_timeout);
		}

		public WebAsyncRequest GetTournaments(DRLTournamentData p_query, bool p_registered_only, Action<DRLTournamentResult> p_callback, int p_timeout = -1, int p_count = 4)
		{
			string text = (p_registered_only ? "/player/tournaments/" : "/tournaments/");
			string guid = p_query.guid;
			p_query.Remove("guid");
			if (!string.IsNullOrEmpty(guid))
			{
				text = text + guid + "/";
			}
			p_query["limit"] = p_count;
			p_query["token"] = token;
			return Get("drl.service.tournaments.read", text, delegate(DRLServiceResult p_result)
			{
				DRLTournamentResult res = new DRLTournamentResult();
				if (!p_result.success)
				{
					if (p_callback != null)
					{
						p_callback(res);
					}
				}
				else
				{
					if (m_thread != null)
					{
						m_thread.Abort();
					}
					m_thread = new Thread((ThreadStart)delegate
					{
						res.tournaments = Serialize.FromJson<DRLTournamentData[]>(p_result.data.ToString());
						for (int i = 0; i < res.tournaments.Length; i++)
						{
							res.tournaments[i].WarmUp();
						}
						this.TimerRunOnce(delegate
						{
							if (p_callback != null)
							{
								p_callback(res);
							}
						}, 1f / 60f);
					});
					m_thread.Start();
				}
			}, p_query, p_timeout);
		}

		public WebAsyncRequest GetTournaments(DRLTournamentData p_query, Action<DRLTournamentResult> p_callback, int p_timeout = -1, int p_count = 4)
		{
			return GetTournaments(p_query, p_registered_only: false, p_callback, p_timeout, p_count);
		}

		public WebAsyncRequest GetTournaments(string p_guid, Action<DRLTournamentResult> p_callback, int p_timeout = -1)
		{
			DRLTournamentData dRLTournamentData = new DRLTournamentData();
			dRLTournamentData.guid = p_guid;
			return GetTournaments(dRLTournamentData, p_callback, p_timeout);
		}

		public WebAsyncRequest GetTournaments(int p_min_skill, Action<DRLTournamentResult> p_callback, int p_timeout = -1, int p_count = 4)
		{
			DRLTournamentData dRLTournamentData = new DRLTournamentData();
			dRLTournamentData.minimumSkill = p_min_skill;
			return GetTournaments(dRLTournamentData, p_callback, p_timeout, p_count);
		}

		public WebAsyncRequest GetTournaments(bool p_registered_only, Action<DRLTournamentResult> p_callback, int p_timeout = -1)
		{
			DRLTournamentData p_query = new DRLTournamentData();
			return GetTournaments(p_query, p_registered_only, p_callback, p_timeout);
		}

		public WebAsyncRequest GetTournament(string p_guid, Action<DRLTournamentResult> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/tournaments/" + p_guid + "?token=" + token;
			return Get("drl.service.tournaments.read", p_endpoint, delegate(DRLServiceResult p_result)
			{
				DRLTournamentResult res = new DRLTournamentResult();
				if (!p_result.success)
				{
					if (p_callback != null)
					{
						p_callback(res);
					}
				}
				else
				{
					if (m_thread != null)
					{
						m_thread.Abort();
					}
					m_thread = new Thread((ThreadStart)delegate
					{
						res.tournaments = Serialize.FromJson<DRLTournamentData[]>(p_result.data.ToString());
						for (int i = 0; i < res.tournaments.Length; i++)
						{
							res.tournaments[i].WarmUp();
						}
						this.TimerRunOnce(delegate
						{
							if (p_callback != null)
							{
								p_callback(res);
							}
						}, 1f / 60f);
					});
					m_thread.Start();
				}
			}, null, p_timeout);
		}

		public WebAsyncRequest GetMatch(string p_guid, string p_matchId, Action<DRLTournamentMatchResult> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/tournaments/" + p_guid + "/matches/" + p_matchId + "?token=" + token;
			return Get("drl.service.tournaments.matches", p_endpoint, delegate(DRLServiceResult p_result)
			{
				DRLTournamentMatchResult res = new DRLTournamentMatchResult();
				if (!p_result.success || p_result.data == null)
				{
					if (p_callback != null)
					{
						p_callback(res);
					}
				}
				else
				{
					if (m_thread != null)
					{
						if (m_thread.ThreadState == ThreadState.Running)
						{
							return;
						}
						m_thread.Abort();
						m_thread = null;
					}
					m_thread = new Thread((ThreadStart)delegate
					{
						string text = p_result.data.ToString();
						bool flag = !string.IsNullOrEmpty(text);
						if (!flag)
						{
							Debug.LogWarning("DRLService> Tournaments.GetMatch / Result Data is <null> or empty!");
						}
						try
						{
							res.matches = ((!flag) ? new DRLTournamentMatchData[0] : Serialize.FromJson<DRLTournamentMatchData[]>(text));
						}
						catch (Exception ex)
						{
							Debug.LogWarning("DRLService> Tournaments.GetMatch / Error Parsing the Match\n" + ex.Message);
							if (flag)
							{
								Debug.LogWarning("DRLService> Tournaments.GetMatch / Invalid Data\n" + text);
							}
						}
						for (int i = 0; i < res.matches.Length; i++)
						{
							res.matches[i].WarmUp();
						}
						this.TimerRunOnce(delegate
						{
							if (p_callback != null)
							{
								p_callback(res);
							}
						}, 1f / 60f);
					});
					m_thread.Start();
				}
			}, null, p_timeout);
		}

		public WebAsyncRequest GetHeatResults(string p_guid, string p_matchId, int p_heatIdx, Action<DRLTournamentHeatData> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/tournaments/" + p_guid + "/matches/" + p_matchId + "/heat/" + p_heatIdx + "?token=" + token;
			return Get("drl.service.tournaments.heats.results", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					DRLTournamentHeatData data = Serialize.FromJson<DRLTournamentHeatData>(p_result.data.ToString());
					data.WarmUp();
					this.TimerRunOnce(delegate
					{
						if (p_callback != null)
						{
							p_callback(data);
						}
					}, 1f / 60f);
				}
			}, null, p_timeout);
		}

		public WebAsyncRequest SetMatchHeat(string p_guid, string p_matchId, int p_heatIdx, int p_timeout = -1)
		{
			string p_endpoint = "/tournaments/" + p_guid + "/matches/" + p_matchId + "/" + p_heatIdx + "?token=" + token;
			return Post("drl.service.tournaments.matches.heat-idx", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					if (m_matchHeatRetryAttempts > 0)
					{
						this.TimerRunOnce(delegate
						{
							SetMatchHeat(p_guid, p_matchId, p_heatIdx, p_timeout);
							m_matchHeatRetryAttempts--;
						}, 1f);
					}
				}
				else
				{
					m_matchHeatRetryAttempts = 5;
				}
			}, null, p_timeout);
		}

		public WebAsyncRequest GetTournamentCountdownState(string p_guid, string p_matchId, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/tournaments/" + p_guid + "/matches/" + p_matchId + "/countdown?token=" + token;
			return Get("drl.service.tournaments.countdown.state", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					this.TimerRunOnce(delegate
					{
						if (p_callback != null)
						{
							p_callback(p_result);
						}
					}, 1f / 60f);
				}
			}, null, p_timeout);
		}

		public WebAsyncRequest GetTournamentPlacements(string p_guid, Action<DRLTournamentPlacementsData> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/tournaments/" + p_guid + "/placements?token=" + token;
			return Get("drl.service.tournaments.placements", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					this.TimerRunOnce(delegate
					{
						DRLTournamentPlacementsData dRLTournamentPlacementsData = new DRLTournamentPlacementsData();
						dRLTournamentPlacementsData = Serialize.FromJson<DRLTournamentPlacementsData>(p_result.data.ToString());
						if (p_callback != null)
						{
							p_callback(dRLTournamentPlacementsData);
						}
					}, 1f / 60f);
				}
			}, null, p_timeout);
		}

		public WebAsyncRequest RegisterUser(string p_guid, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/tournaments/" + p_guid + "/register?token=" + token;
			return Get("drl.service.tournaments.read", p_endpoint, delegate(DRLServiceResult p_result)
			{
				p_callback(p_result);
			}, null, p_timeout);
		}

		public WebAsyncRequest UnregisterUser(string p_guid, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/tournaments/" + p_guid + "/unregister?token=" + token;
			return Get("drl.service.tournaments.read", p_endpoint, delegate(DRLServiceResult p_result)
			{
				p_callback(p_result);
			}, null, p_timeout);
		}

		public WebAsyncRequest SubscribeUser(string p_guid, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/tournaments/" + p_guid + "/subscribe?token=" + token;
			return Get("drl.service.tournaments.read", p_endpoint, delegate(DRLServiceResult p_result)
			{
				p_callback(p_result);
			}, null, p_timeout);
		}

		public WebAsyncRequest SubscribeUser(Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/tournaments/subscribe?token=" + token;
			return Get("drl.service.tournaments.read", p_endpoint, delegate(DRLServiceResult p_result)
			{
				p_callback(p_result);
			}, null, p_timeout);
		}

		public WebAsyncRequest UnsubscribeUser(string p_guid, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/tournaments/" + p_guid + "/unsubscribe?token=" + token;
			return Get("drl.service.tournaments.read", p_endpoint, delegate(DRLServiceResult p_result)
			{
				p_callback(p_result);
			}, null, p_timeout);
		}

		public WebAsyncRequest UnsubscribeUser(Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/tournaments/unsubscribe?token=" + token;
			return Get("drl.service.tournaments.read", p_endpoint, delegate(DRLServiceResult p_result)
			{
				p_callback(p_result);
			}, null, p_timeout);
		}

		public WebAsyncRequest CheckUserSubscription(string p_guid, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/tournaments/" + p_guid + "/subscription?token=" + token;
			return Get("drl.service.tournaments.read", p_endpoint, delegate(DRLServiceResult p_result)
			{
				p_callback(p_result);
			}, null, p_timeout);
		}

		public WebAsyncRequest CheckUserSubscription(Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/tournaments/subscription?token=" + token;
			return Get("drl.service.tournaments.read", p_endpoint, delegate(DRLServiceResult p_result)
			{
				p_callback(p_result);
			}, null, p_timeout);
		}

		public WebAsyncRequest SetTournamentResults(string p_guid, DRLRaceResultData[] p_results, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/tournaments/" + p_guid + "/scores?token=" + token;
			SerializedData serializedData = new SerializedData();
			serializedData.Set("guid", p_guid);
			serializedData.Set("scores", Serialize.ToJson(p_results));
			return Post("drl.service.tournaments.write", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("DRLService> Failed to send Tournament [" + p_guid + "] results!\n" + p_result.message);
					if (p_callback != null)
					{
						p_callback(p_result);
					}
				}
				else if (p_callback != null)
				{
					p_callback(p_result);
				}
			}, serializedData, p_timeout);
		}

		public WebAsyncRequest GetTournamentResults(string p_guid, string p_roundId, Action<DRLTournamentResultData> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/tournaments/" + p_guid + "/results/" + p_roundId + "?token=" + token;
			return Get("drl.service.tournaments.results", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					if (m_thread != null)
					{
						m_thread.Abort();
					}
					m_thread = new Thread((ThreadStart)delegate
					{
						DRLTournamentResultData rd = Serialize.FromJson<DRLTournamentResultData>(p_result.data.ToString());
						rd.WarmUp();
						this.TimerRunOnce(delegate
						{
							if (p_callback != null)
							{
								p_callback(rd);
							}
						}, 1f / 60f);
					});
					m_thread.Start();
				}
			}, null, p_timeout);
		}

		public void WatchTournamentRefresh(TournamentSocketService p_socket, Action<DRLTournamentSocketData> p_callback, Action p_connected = null)
		{
			if (p_socket == null)
			{
				return;
			}
			socket = p_socket;
			if (socket.IsConnected())
			{
				return;
			}
			try
			{
				socket.Connect();
				socket.OnConnected(delegate
				{
					socket.On("tournament-event", delegate(DRLTournamentSocketData p_data)
					{
						if (p_data != null && p_callback != null)
						{
							p_callback(p_data);
						}
					});
				});
				p_connected?.Invoke();
			}
			catch (Exception message)
			{
				Debug.LogWarning(message);
				StopTournamentRefresh();
			}
		}

		public void SocketEmitMessage(DRLTournamentSocketData p_data)
		{
			if (!(socket == null))
			{
				socket.Send("tournament-event", p_data);
			}
		}

		public void StopTournamentRefresh(Action p_callback = null)
		{
			if (socket == null || !socket.IsConnected())
			{
				return;
			}
			try
			{
				socket.Disconnect();
				p_callback?.Invoke();
			}
			catch (Exception message)
			{
				Debug.LogWarning(message);
			}
		}

		public WebAsyncRequest GetTournamentsLegacy(string p_guid, Action<DRLTournamentLegacyData[]> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/tournaments/" + (string.IsNullOrEmpty(p_guid) ? "" : (p_guid + "/"));
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["token"] = token;
			return Get("drl.service.tournaments.read", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					DRLTournamentLegacyData[] obj = new DRLTournamentLegacyData[0];
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
				else
				{
					string p_data = p_result.data.ToString();
					DRLTournamentLegacyData[] obj;
					if (string.IsNullOrEmpty(p_guid))
					{
						obj = Serialize.FromJson<DRLTournamentLegacyData[]>(p_data);
					}
					else
					{
						DRLTournamentLegacyData dRLTournamentLegacyData = Serialize.FromJson<DRLTournamentLegacyData>(p_data);
						obj = ((dRLTournamentLegacyData == null) ? new DRLTournamentLegacyData[0] : new DRLTournamentLegacyData[1] { dRLTournamentLegacyData });
					}
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, dictionary, p_timeout);
		}

		public WebAsyncRequest GetTournamentsLegacy(Action<DRLTournamentLegacyData[]> p_callback, int p_timeout = -1)
		{
			return GetTournamentsLegacy("", p_callback, p_timeout);
		}

		public WebAsyncRequest SetTournamentLegacyResults(string p_guid, DRLRaceResultData[] p_results, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string text = "/tournaments/scores/";
			text = text + "?token=" + token;
			SerializedData serializedData = new SerializedData();
			serializedData.Set("guid", p_guid);
			serializedData.Set("scores", Serialize.ToJson(p_results));
			return Post("drl.service.tournaments.write", text, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					Debug.LogWarning("DRLService> Failed to send Tournament [" + p_guid + "] results!\n" + p_result.message);
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else if (p_callback != null)
				{
					p_callback(p_result);
				}
			}, serializedData, p_timeout);
		}

		public WebAsyncRequest Transaction(List<DRLTransactionItem> p_items, Action<DRLTransactionResult> p_callback, int p_timeout = -1)
		{
			string text = "/transactions/start/";
			text = text + "?token=" + token;
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			int num = 0;
			for (int i = 0; i < p_items.Count; i++)
			{
				DRLTransactionItem dRLTransactionItem = p_items[i];
				if (dRLTransactionItem != null)
				{
					dictionary["itemid[" + num + "]"] = dRLTransactionItem.id;
					dictionary["qty[" + num + "]"] = dRLTransactionItem.count;
					num++;
				}
			}
			dictionary["currency"] = steam.currencyISO;
			dictionary["system-language"] = DRLApp.systemLocale;
			string order_id = "";
			DRLTransactionResult rd = new DRLTransactionResult();
			steam.WatchTransactionEvent(delegate(bool p_succes)
			{
				Debug.Log($"DRLService> WatchTransactionEvent / Complete - success[{p_succes}] order-id[{order_id}]");
				if (!p_succes)
				{
					rd.result = "CANCEL";
					if (p_callback != null)
					{
						p_callback(rd);
					}
				}
				else if (rd.parameters != null)
				{
					TransactionFinalize(rd.parameters.orderId, rd.parameters.transactionId, p_callback);
				}
			});
			return Post("drl.service.transactions.start", text, delegate(DRLServiceResult p_start_result)
			{
				if (!p_start_result.success)
				{
					Debug.LogWarning("DRLService> Failed to start transaction!\n" + p_start_result.message);
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					string p_data = ((p_start_result.data == null) ? "{}" : p_start_result.data.ToString());
					rd = Serialize.FromJson<DRLTransactionResult>(p_data);
					order_id = rd.parameters.orderId;
					string transactionId = rd.parameters.transactionId;
					Debug.Log("DRLService> Transaction Start - result[" + rd.result + "] order-id[" + order_id + "] transaction-id[" + transactionId + "]");
				}
			}, dictionary, p_timeout);
		}

		public WebAsyncRequest TransactionFinalize(string p_order_id, string p_transaction_id, Action<DRLTransactionResult> p_callback, int p_timeout = -1)
		{
			string text = "/transactions/finalize/";
			text = text + "?token=" + token;
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["orderid"] = p_order_id;
			dictionary["transid"] = p_transaction_id;
			return Post("drl.service.transactions.finalize", text, delegate(DRLServiceResult p_result)
			{
				Debug.Log("DRLService> Transaction Finalize - success[" + p_result.success + "]");
				DRLTransactionResult dRLTransactionResult = Serialize.FromJson<DRLTransactionResult>((p_result.data == null) ? "{}" : p_result.data.ToString());
				dRLTransactionResult.result = (p_result.success ? "OK" : "ERROR");
				if (p_callback != null)
				{
					p_callback(dRLTransactionResult);
				}
			}, dictionary);
		}

		public WebAsyncRequest Transaction(DRLTransactionItem p_item, Action<DRLTransactionResult> p_callback, int p_timeout = -1)
		{
			List<DRLTransactionItem> list = new List<DRLTransactionItem>();
			list.Add(p_item);
			return Transaction(list, p_callback, p_timeout);
		}

		public WebAsyncRequest Transaction(string p_item_id, int p_count, Action<DRLTransactionResult> p_callback, int p_timeout = -1)
		{
			return Transaction(new DRLTransactionItem(p_item_id, p_count), p_callback, p_timeout);
		}

		public WebAsyncRequest TransactionComplete(string p_product_id, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string text = "/transactions/complete/";
			text = text + "?token=" + token;
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["platformId"] = p_product_id;
			return Post("drl.service.transactions.complete", text, delegate(DRLServiceResult p_result)
			{
				Debug.Log($"DRLService> Transaction Complete - success[{p_result.success}] product-id[{p_product_id}]");
				if (p_callback != null)
				{
					p_callback(p_result);
				}
			}, dictionary);
		}

		public WebAsyncRequest TransactionRefund(string p_product_id, Action<DRLServiceResult> p_callback, int p_timeout = -1)
		{
			string text = "/transactions/refund/";
			text = text + "?token=" + token;
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["platformId"] = p_product_id;
			return Post("drl.service.transactions.refund", text, delegate(DRLServiceResult p_result)
			{
				Debug.Log($"DRLService> Transaction Refund - success[{p_result.success}] product-id[{p_product_id}]");
				if (p_callback != null)
				{
					p_callback(p_result);
				}
			}, dictionary);
		}

		public WebAsyncRequest GetTryoutsActiveTrack(Action<string> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/tryouts/track/?token=" + token;
			return Get("drl.service.tryouts.track", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (p_result.success && p_callback != null)
				{
					p_callback(p_result.data.ToString());
				}
			}, null, p_timeout);
		}

		public WebAsyncRequest GetTryoutsTournamentWinners(Action<string[]> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/tryouts/tournaments/winners/?token=" + token;
			return Get("drl.service.tryouts.winners", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success || p_result == null || p_result.data == null)
				{
					p_callback(null);
				}
				else
				{
					string[] obj = Serialize.FromJson<string[]>(p_result.data.ToString());
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, null, p_timeout);
		}

		public WebAsyncRequest GetTryoutsHeatsFinished(Action<int> p_callback, int p_timeout = -1)
		{
			string p_endpoint = "/tryouts/track/counter/?token=" + token;
			return Get("drl.service.tryouts.heats-counter", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success || p_result == null || p_result.data == null)
				{
					p_callback(-1);
				}
				else
				{
					int heatCount = Serialize.FromJson<DRLTryoutsHeatsData>(p_result.data.ToString()).heatCount;
					if (p_callback != null)
					{
						p_callback(heatCount);
					}
				}
			}, null, p_timeout);
		}

		public WebAsyncRequest GetOnboardingBotReplay(Action<OnboardingRaceReplayData[]> p_callback, OnboardingCampaignMode mode, int p_timeout = -1)
		{
			string p_endpoint = GetReplayUrl(mode) + "?token=" + token;
			return Get("drl.service.onboarding.get-opponent", p_endpoint, delegate(DRLServiceResult p_result)
			{
				if (!p_result.success)
				{
					OnboardingRaceReplayData[] obj = new OnboardingRaceReplayData[0];
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
				else
				{
					OnboardingRaceReplayData[] obj = Serialize.FromJson<OnboardingRaceReplayData[]>(p_result.data.ToString());
					if (p_callback != null)
					{
						p_callback(obj);
					}
				}
			}, p_timeout);
		}

		public static string GetReplayUrl(OnboardingCampaignMode mode)
		{
			string text = "/onboarding/bots";
			switch (mode)
			{
			case OnboardingCampaignMode.Beginner:
				text += "/beginner";
				break;
			case OnboardingCampaignMode.Intermediate:
				text += "/intermediate";
				break;
			case OnboardingCampaignMode.Pro:
				text += "/pro";
				break;
			}
			return text;
		}
	}
}

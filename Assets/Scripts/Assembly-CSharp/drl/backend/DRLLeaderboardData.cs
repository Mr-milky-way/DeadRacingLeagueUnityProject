using System;
using System.Collections.Generic;
using UnityEngine;
using drl.game;
using thelab.core;

namespace drl.backend
{
	public class DRLLeaderboardData : SerializedData
	{
		public string playerId
		{
			get
			{
				return Get("player-id", "");
			}
			set
			{
				Set("player-id", value);
			}
		}

		public string platformPlayerId
		{
			get
			{
				return Get("profile-platform-id", "");
			}
			set
			{
				Set("profile-platform-id", value);
			}
		}

		public string username
		{
			get
			{
				return Get("username", "");
			}
			set
			{
				Set("username", value);
			}
		}

		public string profileColorHex
		{
			get
			{
				return Get("profile-color", "000000");
			}
			set
			{
				Set("profile-color", value);
			}
		}

		public Color profileColor
		{
			get
			{
				if (!ContainsKey("profile-color"))
				{
					return Color.magenta;
				}
				return Colorf.ParseRGB(profileColorHex, Color.yellow);
			}
		}

		public string profileThumbURL
		{
			get
			{
				return Get("profile-thumb", "");
			}
			set
			{
				Set("profile-thumb", value);
			}
		}

		public string profileName
		{
			get
			{
				return Get("profile-name", "");
			}
			set
			{
				Set("profile-name", value);
			}
		}

		public string platform
		{
			get
			{
				return Get("profile-platform", "");
			}
			set
			{
				Set("profile-platform", value);
			}
		}

		public string map
		{
			get
			{
				return Get("map", "");
			}
			set
			{
				Set("map", value);
			}
		}

		public string track
		{
			get
			{
				return Get("track", "");
			}
			set
			{
				Set("track", value);
			}
		}

		public bool isCustomMap
		{
			get
			{
				return Get("is-custom-map", d: false);
			}
			set
			{
				Set("is-custom-map", value);
			}
		}

		public string customMap
		{
			get
			{
				return Get("custom-map", "");
			}
			set
			{
				Set("custom-map", value);
			}
		}

		public string mission
		{
			get
			{
				return Get("mission", "");
			}
			set
			{
				Set("mission", value);
			}
		}

		public string group
		{
			get
			{
				return Get("group-id", "");
			}
			set
			{
				Set("group-id", value);
			}
		}

		public string region
		{
			get
			{
				return Get("region", "");
			}
			set
			{
				Set("region", value);
			}
		}

		public string replayURL
		{
			get
			{
				return Get("replay-url", "");
			}
			set
			{
				Set("replay-url", value);
			}
		}

		public string gameType
		{
			get
			{
				return Get("game-type", "");
			}
			set
			{
				Set("game-type", value);
			}
		}

		public int diameter
		{
			get
			{
				return Get("diameter", 6);
			}
			set
			{
				Set("diameter", value);
			}
		}

		public string droneName
		{
			get
			{
				return Get("drone-name", "");
			}
			set
			{
				Set("drone-name", value);
			}
		}

		public string droneThumb
		{
			get
			{
				return Get("drone-thumb", "");
			}
			set
			{
				Set("drone-thumb", value);
			}
		}

		public bool multiplayer
		{
			get
			{
				return Get("multiplayer", d: false);
			}
			set
			{
				Set("multiplayer", value);
			}
		}

		public string multiplayerRoomId
		{
			get
			{
				return Get("multiplayer-room-id", "");
			}
			set
			{
				Set("multiplayer-room-id", value);
			}
		}

		public int multiplayerRoomSize
		{
			get
			{
				return Get("multiplayer-room-size", 1);
			}
			set
			{
				Set("multiplayer-room-size", value);
			}
		}

		public string multiplayerPlayerId
		{
			get
			{
				return Get("multiplayer-player-id", "");
			}
			set
			{
				Set("multiplayer-player-id", value);
			}
		}

		public string multiplayerMasterId
		{
			get
			{
				return Get("multiplayer-master-id", "");
			}
			set
			{
				Set("multiplayer-master-id", value);
			}
		}

		public int multiplayerPlayerPosition
		{
			get
			{
				return Get("multiplayer-player-position", -1);
			}
			set
			{
				Set("multiplayer-player-position", value);
			}
		}

		public string flagThumbURL
		{
			get
			{
				return Get("flag-url", "");
			}
			set
			{
				Set("flag-url", value);
			}
		}

		public string scoreType
		{
			get
			{
				return Get("score-type", "");
			}
			set
			{
				Set("score-type", value);
			}
		}

		public string raceStatus
		{
			get
			{
				return Get("race-status", "");
			}
			set
			{
				Set("race-status", value);
			}
		}

		public string createDateString
		{
			get
			{
				object obj = Get<object>("created-at", null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return "";
			}
		}

		public string updateDateString
		{
			get
			{
				object obj = Get<object>("updated-at", null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return "";
			}
		}

		public string matchId
		{
			get
			{
				return Get("match-id", "");
			}
			set
			{
				Set("match-id", value);
			}
		}

		public bool tryouts
		{
			get
			{
				return Get("tryouts", d: false);
			}
			set
			{
				Set("tryouts", value);
			}
		}

		public float batteryResistance
		{
			get
			{
				return Get("battery-resistance", 0f);
			}
			set
			{
				Set("battery-resistance", value);
			}
		}

		public DateTime createDate
		{
			get
			{
				DateTime result = DateTime.MinValue;
				string text = createDateString;
				if (string.IsNullOrEmpty(text))
				{
					return result;
				}
				if (DateTime.TryParse(text, out result))
				{
					return result;
				}
				return DateTime.MinValue;
			}
		}

		public DateTime updateDate
		{
			get
			{
				DateTime result = DateTime.MinValue;
				string text = updateDateString;
				if (string.IsNullOrEmpty(text))
				{
					return result;
				}
				if (DateTime.TryParse(text, out result))
				{
					return result;
				}
				return DateTime.MinValue;
			}
		}

		public ScoreType scoreTypeFlag
		{
			get
			{
				string value = scoreType;
				if (string.IsNullOrEmpty(value))
				{
					return ScoreType.None;
				}
				return (ScoreType)Enum.Parse(typeof(ScoreType), value);
			}
			set
			{
				string text = value.ToString();
				scoreType = text;
			}
		}

		public RaceStatusType raceStatusFlag
		{
			get
			{
				string value = raceStatus;
				if (string.IsNullOrEmpty(value))
				{
					return RaceStatusType.None;
				}
				return (RaceStatusType)Enum.Parse(typeof(RaceStatusType), value);
			}
			set
			{
				string text = value.ToString();
				raceStatus = text;
			}
		}

		public string controllerType
		{
			get
			{
				return Get("controller-type", "");
			}
			set
			{
				Set("controller-type", value);
			}
		}

		public int position
		{
			get
			{
				return Get("position", -1);
			}
			set
			{
				Set("position", value);
			}
		}

		public int score
		{
			get
			{
				return Get("score", 0);
			}
			set
			{
				Set("score", value);
			}
		}

		public int scoreCheck
		{
			get
			{
				return Get("score-check", 0);
			}
			set
			{
				Set("score-check", value);
			}
		}

		public int scoreDoubleCheck
		{
			get
			{
				return Get("score-double-check", 0);
			}
			set
			{
				Set("score-double-check", value);
			}
		}

		public bool scoreCheat
		{
			get
			{
				return Get("score-cheat", d: false);
			}
			set
			{
				Set("score-cheat", value);
			}
		}

		public float scoreCheatRatio
		{
			get
			{
				return Get("score-cheat-ratio", 1f);
			}
			set
			{
				Set("score-cheat-ratio", value);
			}
		}

		public string scoreCheatSamples
		{
			get
			{
				return Get("score-cheat-samples", "");
			}
			set
			{
				Set("score-cheat-samples", value);
			}
		}

		public float scoreSeconds => (float)score / 1000f;

		public string scoreTime => Format.SecondsToTime(scoreSeconds, 2, p_use_ms: true);

		public int crashCount
		{
			get
			{
				return Get("crash-count", 0);
			}
			set
			{
				Set("crash-count", value);
			}
		}

		public float topSpeed
		{
			get
			{
				return Get("top-speed", 0f);
			}
			set
			{
				Set("top-speed", value);
			}
		}

		public float timeInFirst
		{
			get
			{
				return Get("time-in-first", 0f);
			}
			set
			{
				Set("time-in-first", value);
			}
		}

		public float[] lapTimes
		{
			get
			{
				return Get<float[]>("lap-times");
			}
			set
			{
				Set("lap-times", value);
			}
		}

		public List<float> gateTimes
		{
			get
			{
				return Get<List<float>>("gate-times");
			}
			set
			{
				Set("gate-times", value);
			}
		}

		public int fastestLap
		{
			get
			{
				return Get("fastest-lap", 0);
			}
			set
			{
				Set("slowest-lap", value);
			}
		}

		public int slowestLap
		{
			get
			{
				return Get("slowest-lap", 0);
			}
			set
			{
				Set("slowest-lap", value);
			}
		}

		public float totalDistance
		{
			get
			{
				return Get("total-distance", 0f);
			}
			set
			{
				Set("total-distance", value);
			}
		}

		public float percentile
		{
			get
			{
				return Get("percentile", 0f);
			}
			set
			{
				Set("percentile", value);
			}
		}

		public int order
		{
			get
			{
				return Get("order", 0);
			}
			set
			{
				Set("order", value);
			}
		}

		public bool highscore => Get("high-score", d: false);

		public string id => Get("id", "");

		public string raceId
		{
			get
			{
				return Get("race-id", "");
			}
			set
			{
				Set("race-id", value);
			}
		}

		public int page
		{
			get
			{
				return Get("page", 0);
			}
			set
			{
				Set("page", value);
			}
		}

		public int limit
		{
			get
			{
				return Get("limit", 0);
			}
			set
			{
				Set("limit", value);
			}
		}

		public bool force
		{
			get
			{
				return Get("force", d: false);
			}
			set
			{
				Set("force", value);
			}
		}

		public int heatIdx
		{
			get
			{
				return Get("heat", 1);
			}
			set
			{
				Set("heat", value);
			}
		}

		public DRLReplayData[] replays
		{
			get
			{
				object obj = Get<object>("replays", null);
				string p_data = "[]";
				if (obj != null)
				{
					p_data = obj.ToString();
				}
				return Serialize.FromJson<DRLReplayData[]>(p_data);
			}
		}

		public DRLLeaderboardData[] races
		{
			get
			{
				object obj = Get<object>("races", null);
				string p_data = "[]";
				if (obj != null)
				{
					p_data = obj.ToString();
				}
				return Serialize.FromJson<DRLLeaderboardData[]>(p_data);
			}
		}

		public DRLProgressionStateData progression => GetCast<DRLProgressionStateData>("progression", null);

		public bool customPhysics
		{
			get
			{
				return Get("custom-physics", d: false);
			}
			set
			{
				Set("custom-physics", value);
			}
		}

		public bool drlOfficial
		{
			get
			{
				return Get("drl-official", d: false);
			}
			set
			{
				Set("drl-official", value);
			}
		}

		public bool drlPilotMode
		{
			get
			{
				return Get("drl-pilot-mode", d: false);
			}
			set
			{
				Set("drl-pilot-mode", value);
			}
		}

		public string droneGuid
		{
			get
			{
				return Get("drone-guid", "");
			}
			set
			{
				Set("drone-guid", value);
			}
		}

		public string droneRig
		{
			get
			{
				return Get("drone-rig", "");
			}
			set
			{
				Set("drone-rig", value);
			}
		}

		public string hash
		{
			get
			{
				return Get("drone-hash", "");
			}
			set
			{
				Set("drone-hash", value);
			}
		}
	}
}

using System;
using thelab.core;

namespace drl.backend
{
	public class DRLReplayData : SerializedData
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

		public string droneGUID
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

		public int diameter
		{
			get
			{
				return Get("diameter", 0);
			}
			set
			{
				Set("diameter", value);
			}
		}

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

		public string leaderboardId
		{
			get
			{
				return Get("leaderboard-id", "");
			}
			set
			{
				Set("leaderboard-id", value);
			}
		}

		public string circuitId
		{
			get
			{
				return Get("circuit-id", "");
			}
			set
			{
				Set("circuit-id", value);
			}
		}

		public int circuitDifficulty
		{
			get
			{
				return Get("circuit-bot-difficulty", 1);
			}
			set
			{
				Set("circuit-bot-difficulty", value);
			}
		}

		public string exclude
		{
			get
			{
				object obj = Get<object>("exclude", null);
				string result = "[]";
				if (obj != null)
				{
					result = obj.ToString();
				}
				return result;
			}
			set
			{
				Set("exclude", value);
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

		public byte[] replayData
		{
			get
			{
				return Get("replay-data", new byte[0]);
			}
			set
			{
				Set("replay-data", value);
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
	}
}

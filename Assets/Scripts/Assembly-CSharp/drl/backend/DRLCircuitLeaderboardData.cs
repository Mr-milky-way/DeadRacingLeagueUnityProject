using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using thelab.core;

namespace drl.backend
{
	public class DRLCircuitLeaderboardData : SerializedData
	{
		private List<float> m_times;

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

		public int position => Get("position", -1);

		public string profileColorHex => Get("profile-color", "000000");

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

		public string profileThumbURL => Get("profile-thumb", "");

		public string profileName => Get("profile-name", "");

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

		public string trackTimes
		{
			get
			{
				return Get("track-times", "");
			}
			set
			{
				Set("track-times", value);
			}
		}

		public List<float> times
		{
			get
			{
				if (m_times == null)
				{
					m_times = new List<float>();
				}
				if (string.IsNullOrEmpty(trackTimes))
				{
					return m_times;
				}
				m_times = Serialize.FromJson<float[]>(trackTimes).ToList();
				return m_times;
			}
			set
			{
				m_times = value;
				if (m_times != null)
				{
					trackTimes = Serialize.ToJson(m_times.ToArray());
				}
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

		public string flagThumbURL => Get("flag-url", "");

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

		public float scoreSeconds => (float)score / 1000f;

		public string scoreTime => Format.SecondsToTime(scoreSeconds, 2, p_use_ms: true);
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.backend
{
	public class DRLRaceResultData : SerializedData
	{
		private List<float> m_gateTimes;

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

		public int heat
		{
			get
			{
				return Get("heat", 0);
			}
			set
			{
				Set("heat", value);
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

		public string profileThumbURL => Get("profile-thumb", "");

		public string profileName
		{
			get
			{
				return Get("profile-name", "Player");
			}
			set
			{
				Set("profile-name", value);
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

		public GameFlag type
		{
			get
			{
				return Reflection<object>.GetEnum<GameFlag>(Get("type", "None"));
			}
			set
			{
				Set("type", value.ToString());
			}
		}

		public GameFlag mode
		{
			get
			{
				return Reflection<object>.GetEnum<GameFlag>(Get("mode", "None"));
			}
			set
			{
				Set("mode", value.ToString());
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

		public string campaign
		{
			get
			{
				return Get("campaign", "");
			}
			set
			{
				Set("campaign", value);
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

		public string replay
		{
			get
			{
				return Get("replay", "");
			}
			set
			{
				Set("replay", value);
			}
		}

		public string tournament
		{
			get
			{
				return Get("tournament", "");
			}
			set
			{
				Set("tournament", value);
			}
		}

		public int crashes
		{
			get
			{
				return Get("crashes", 0);
			}
			set
			{
				Set("crashes", value);
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

		public string gateTimesData
		{
			get
			{
				return Get("gate-times", "");
			}
			set
			{
				Set("gate-times", value);
			}
		}

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

		public List<float> gateTimes
		{
			get
			{
				if (m_gateTimes == null)
				{
					m_gateTimes = new List<float>();
				}
				return m_gateTimes;
			}
			set
			{
				m_gateTimes = value;
				if (m_gateTimes != null)
				{
					string text = Serialize.ToJson(m_gateTimes.ToArray());
					gateTimesData = text;
				}
			}
		}

		public ResultStatusType status
		{
			get
			{
				return Reflection<object>.GetEnum<ResultStatusType>(Get("status", "None"));
			}
			set
			{
				Set("status", value.ToString());
			}
		}

		public float time => (float)score / 1000f;

		public string guid
		{
			get
			{
				string text = Get("guid", "");
				if (string.IsNullOrEmpty(text) || text.Length < 24)
				{
					text = GUID.Create(24, "", 200, 0, 15, "x1");
					Set("guid", text);
				}
				return text;
			}
			set
			{
				string text = value;
				if (string.IsNullOrEmpty(text) || text.Length < 24)
				{
					text = GUID.Create(24, "", 200, 0, 15, "x1");
				}
				Set("guid", text);
			}
		}

		public DRLRaceResultData()
		{
			guid = "";
		}

		public string GetTimeString()
		{
			return Format.SecondsToTime(time, 2, p_use_ms: true);
		}

		public static Comparison<DRLRaceResultData> SortByScore(ScoreType p_type)
		{
			return delegate(DRLRaceResultData a, DRLRaceResultData b)
			{
				int result = 0;
				ResultStatusType resultStatusType = a.status;
				ResultStatusType resultStatusType2 = b.status;
				if (resultStatusType == resultStatusType2)
				{
					if (resultStatusType == ResultStatusType.Crash)
					{
						result = ((a.score <= b.score) ? 1 : (-1));
					}
					if (resultStatusType == ResultStatusType.Timeout)
					{
						result = string.Compare(a.profileName, b.profileName);
					}
					if (resultStatusType == ResultStatusType.Quit)
					{
						result = string.Compare(a.profileName, b.profileName);
					}
					if (resultStatusType == ResultStatusType.Success || resultStatusType == ResultStatusType.None)
					{
						switch (p_type)
						{
						case ScoreType.TimeMin:
						case ScoreType.ScoreMin:
							result = ((a.score >= b.score) ? 1 : (-1));
							break;
						case ScoreType.TimeMax:
						case ScoreType.ScoreMax:
							result = ((a.score <= b.score) ? 1 : (-1));
							break;
						}
					}
				}
				else
				{
					bool num = resultStatusType == ResultStatusType.Timeout || resultStatusType == ResultStatusType.Crash || resultStatusType == ResultStatusType.Quit;
					bool flag = resultStatusType2 == ResultStatusType.Timeout || resultStatusType2 == ResultStatusType.Crash || resultStatusType2 == ResultStatusType.Quit;
					if (num && resultStatusType2 == ResultStatusType.Success)
					{
						result = 1;
					}
					if (flag && resultStatusType == ResultStatusType.Success)
					{
						result = -1;
					}
					if (num && resultStatusType2 == ResultStatusType.None)
					{
						result = 1;
					}
					if (flag && resultStatusType == ResultStatusType.None)
					{
						result = -1;
					}
				}
				return result;
			};
		}
	}
}

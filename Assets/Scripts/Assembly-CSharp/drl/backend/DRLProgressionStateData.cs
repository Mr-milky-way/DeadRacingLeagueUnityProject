using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using thelab.core;

namespace drl.backend
{
	public class DRLProgressionStateData : SerializedData
	{
		private DRLProgressionLeagueData m_default_league = new DRLProgressionLeagueData();

		public int xp => Get("xp", 0);

		public int previousLevelXP => Get("previous-level-xp", 0);

		public int nextLevelXP => Get("next-level-xp", 0);

		public float xpProgression => GetXPProgresion(xp);

		public int level => Get("level", -1);

		public string rankName => Get("rank-name", "");

		public int rankIndex => Get("rank-index", 0);

		public int rankPosition => Get("rank-position", -1);

		public string rankRoundStartString
		{
			get
			{
				object obj = Get<object>("rank-round-start", null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return "";
			}
		}

		public string rankRoundEndString
		{
			get
			{
				object obj = Get<object>("rank-round-end", null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return "";
			}
		}

		public DateTime rankRoundStart
		{
			get
			{
				DateTime result = DateTime.MinValue;
				DateTime.TryParse(rankRoundStartString, out result);
				return result;
			}
		}

		public DateTime rankRoundEnd
		{
			get
			{
				DateTime result = DateTime.MinValue;
				DateTime.TryParse(rankRoundEndString, out result);
				return result;
			}
		}

		public DRLProgressionLeagueData league => GetCast("league", m_default_league);

		public string streakDateStartString
		{
			get
			{
				object obj = Get<object>("streak-date-start", null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return "";
			}
		}

		public string streakDateEndString
		{
			get
			{
				object obj = Get<object>("streak-date-end", null);
				if (obj != null)
				{
					return obj.ToString();
				}
				return "";
			}
		}

		public DateTime streakDateStart
		{
			get
			{
				DateTime result = DateTime.MinValue;
				DateTime.TryParse(streakDateStartString, out result);
				return result;
			}
		}

		public DateTime streakDateEnd
		{
			get
			{
				DateTime result = DateTime.MinValue;
				DateTime.TryParse(streakDateEndString, out result);
				return result;
			}
		}

		public int streak => Get("streak-points", 0);

		public int streakMapIndex => Get("daily-completed-maps", 0);

		public int streakMapCount => Get("goal-daily-completed-maps", 0);

		public float streakProgression
		{
			get
			{
				float num = streakMapCount;
				if (num <= 0f)
				{
					return 1f;
				}
				return Mathf.Clamp01((float)streakMapIndex / num);
			}
		}

		public DRLProgressionPrizeData[] prizes
		{
			get
			{
				JArray jArray = (JArray)Get<object>("prizes", null);
				if (jArray != null)
				{
					return jArray.ToObject<DRLProgressionPrizeData[]>();
				}
				return new DRLProgressionPrizeData[0];
			}
		}

		public float GetXPProgresion(int p_xp)
		{
			int num = previousLevelXP;
			float num2 = nextLevelXP - num;
			if (num2 <= 0f)
			{
				return 1f;
			}
			return Mathf.Clamp01((float)p_xp / num2);
		}
	}
}

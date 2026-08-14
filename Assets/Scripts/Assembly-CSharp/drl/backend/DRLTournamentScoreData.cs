using System;
using thelab.core;

namespace drl.backend
{
	public class DRLTournamentScoreData : SerializedData
	{
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

		public int heat => Get("heat", 0);

		public int score => Get("score", 0);

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

		public string matchId => Get("match-id", "");

		public int points => Get("points", 0);

		public string status => Get("status", "");

		public int position => Get("position", 1);

		public static Comparison<DRLTournamentScoreData> SortByScore()
		{
			return delegate(DRLTournamentScoreData a, DRLTournamentScoreData b)
			{
				int result = 0;
				if (a.crashes == b.crashes)
				{
					result = ((a.crashes != 1) ? ((a.score > b.score) ? 1 : (-1)) : ((a.score <= b.score) ? 1 : (-1)));
				}
				else
				{
					bool num = a.crashes == 1;
					bool flag = b.crashes == 1;
					if (num)
					{
						result = 1;
					}
					if (flag)
					{
						result = -1;
					}
				}
				return result;
			};
		}

		public static Comparison<DRLTournamentScoreData> SortByUserId()
		{
			return delegate(DRLTournamentScoreData a, DRLTournamentScoreData b)
			{
				int num = 0;
				return (a.playerId == b.playerId) ? ((a.score < b.score) ? 1 : (-1)) : string.Compare(a.playerId, b.playerId);
			};
		}
	}
}

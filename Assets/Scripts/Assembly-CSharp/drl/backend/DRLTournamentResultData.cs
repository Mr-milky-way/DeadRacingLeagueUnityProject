using Newtonsoft.Json.Linq;
using thelab.core;

namespace drl.backend
{
	public class DRLTournamentResultData : SerializedData
	{
		public const string StatusPending = "waiting";

		public const string StatusSuccess = "success";

		private DRLTournamentScoreData[] m_matches;

		private DRLTournamentPlayerData[] m_leaderboard;

		public DRLTournamentScoreData[] matches
		{
			get
			{
				if (m_matches != null)
				{
					return m_matches;
				}
				return new DRLTournamentScoreData[0];
			}
		}

		public DRLTournamentPlayerData[] leaderboard
		{
			get
			{
				if (m_leaderboard != null)
				{
					return m_leaderboard;
				}
				return new DRLTournamentPlayerData[0];
			}
		}

		public DRLTournamentLeaderboardParams[] leaderboardParams
		{
			get
			{
				JArray jArray = (JArray)Get<object>("leaderboard-params", null);
				if (jArray != null)
				{
					return jArray.ToObject<DRLTournamentLeaderboardParams[]>();
				}
				return new DRLTournamentLeaderboardParams[0];
			}
		}

		public string status => Get("status", "waiting");

		public void WarmUp()
		{
			JArray jArray = (JArray)Get<object>("matches", null);
			m_matches = ((jArray == null) ? new DRLTournamentScoreData[0] : jArray.ToObject<DRLTournamentScoreData[]>());
			jArray = (JArray)Get<object>("leaderboard", null);
			m_leaderboard = ((jArray == null) ? new DRLTournamentPlayerData[0] : jArray.ToObject<DRLTournamentPlayerData[]>());
			for (int i = 0; i < m_leaderboard.Length; i++)
			{
				m_leaderboard[i].WarmUp();
			}
		}
	}
}

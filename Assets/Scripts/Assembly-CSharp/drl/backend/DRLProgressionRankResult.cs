using System;
using System.Collections.Generic;
using thelab.core;

namespace drl.backend
{
	public class DRLProgressionRankResult : SerializedData
	{
		private DRLProgressionLeagueData m_default_league = new DRLProgressionLeagueData();

		private DRLProgressionRankData[] m_default_ranking = new DRLProgressionRankData[0];

		private List<DRLProgressionRankData> m_full_ranking;

		public DRLProgressionLeagueData league => GetCast("league", m_default_league);

		public DateTime rankingStartDate => Get("start-at", DateTime.UtcNow);

		public DateTime rankingEndDate => Get("end-at", DateTime.UtcNow.AddDays(30.0));

		public DRLProgressionRankData[] ranking => GetCast("ranking", m_default_ranking);

		public List<DRLProgressionRankData> GetRankingList()
		{
			List<DRLProgressionRankData> list = ((m_full_ranking == null) ? new List<DRLProgressionRankData>() : m_full_ranking);
			list.Clear();
			List<DRLProgressionRankData> list2 = new List<DRLProgressionRankData>();
			List<DRLProgressionRankData> list3 = new List<DRLProgressionRankData>();
			List<DRLProgressionRankData> list4 = new List<DRLProgressionRankData>();
			for (int i = 0; i < ranking.Length; i++)
			{
				DRLProgressionRankData dRLProgressionRankData = ranking[i];
				dRLProgressionRankData.type = "player";
				if (dRLProgressionRankData.isTop)
				{
					list2.Add(dRLProgressionRankData);
				}
				else if (dRLProgressionRankData.isBottom)
				{
					list4.Add(dRLProgressionRankData);
				}
				else
				{
					list3.Add(dRLProgressionRankData);
				}
			}
			DRLProgressionRankData dRLProgressionRankData2 = new DRLProgressionRankData();
			dRLProgressionRankData2.type = "promotion-separator";
			DRLProgressionRankData dRLProgressionRankData3 = new DRLProgressionRankData();
			dRLProgressionRankData3.type = "demotion-separator";
			list2.Add(dRLProgressionRankData2);
			list3.Add(dRLProgressionRankData3);
			list.AddRange(list2);
			list.AddRange(list3);
			list.AddRange(list4);
			m_full_ranking = list;
			return list;
		}
	}
}

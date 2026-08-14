using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class DRLTournamentStandings : MonoBehaviour
	{
		public DRLTournamentStandingsRanksHeader ranksHeader;

		public DRLTournamentStandingsHeatsHeader heatsHeader;

		public ListComponent userRankList;

		public ListComponent heatsResultsList;

		public FadeComponent fade;

		public void Set(List<DRLTournamentStandingsItem> p_data, int p_heatCount, int p_activeHeat, bool p_suddenDeath = false, bool p_goldenHeat = false, List<DRLTournamentReplayData> p_replayURLs = null, bool p_fromBrackets = false, UINavigation p_leftNav = null, UINavigation p_rightNav = null)
		{
			if (p_data == null || p_data.Count == 0)
			{
				Debug.LogWarning("DRLTournamentStandings> No data provided!");
				return;
			}
			Clear();
			heatsHeader.Set(p_heatCount, p_activeHeat, p_suddenDeath, p_goldenHeat);
			ranksHeader.SetLayout(p_goldenHeat || p_suddenDeath);
			for (int i = 0; i < p_data.Count; i++)
			{
				DRLTournamentStandingsItem dRLTournamentStandingsItem = p_data[i];
				DRLTournamentStandingsUserItem dRLTournamentStandingsUserItem = userRankList.Push<DRLTournamentStandingsUserItem>();
				DRLTournamentStandingsHeats dRLTournamentStandingsHeats = heatsResultsList.Push<DRLTournamentStandingsHeats>();
				dRLTournamentStandingsUserItem.Set(dRLTournamentStandingsItem.playerId, dRLTournamentStandingsItem.rank, dRLTournamentStandingsItem.username, dRLTournamentStandingsItem.color, dRLTournamentStandingsItem.totalWins, dRLTournamentStandingsItem.isWinner && p_goldenHeat);
				dRLTournamentStandingsHeats.Set(p_heatCount, dRLTournamentStandingsItem.results, dRLTournamentStandingsItem.color, p_suddenDeath, p_goldenHeat, dRLTournamentStandingsItem.isWinner, dRLTournamentStandingsItem.isWinnerSecond, dRLTournamentStandingsItem.playerBestIndex, dRLTournamentStandingsItem.overallBestIndex);
			}
			if (p_replayURLs != null)
			{
				heatsResultsList.Push<DRLTournamentStandingsHeats>().SetReplayStandings(p_heatCount, p_replayURLs, p_fromBrackets, p_leftNav, p_fromBrackets ? null : p_rightNav);
			}
		}

		public void SetPlayerFlagIcon(string p_playerId, Texture p_flagIcon)
		{
			DRLTournamentStandingsUserItem dRLTournamentStandingsUserItem = userRankList.GetList<DRLTournamentStandingsUserItem>().Find((DRLTournamentStandingsUserItem o) => o.playerId == p_playerId);
			if (!(dRLTournamentStandingsUserItem == null))
			{
				dRLTournamentStandingsUserItem.SetFlag(p_flagIcon);
			}
		}

		public void Clear()
		{
			userRankList.Clear();
			heatsResultsList.Clear();
			heatsHeader.list.Clear();
		}
	}
}

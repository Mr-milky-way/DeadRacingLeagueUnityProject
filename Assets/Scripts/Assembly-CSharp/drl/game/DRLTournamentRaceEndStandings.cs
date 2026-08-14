using System;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class DRLTournamentRaceEndStandings : MonoBehaviour
	{
		public ListComponent list;

		public FadeComponent fade;

		public void Set(DRLTournamentHeatResultData[] p_results)
		{
			list.Clear();
			for (int i = 0; i < p_results.Length; i++)
			{
				DRLTournamentRaceEndStandingsItem dRLTournamentRaceEndStandingsItem = list.Push<DRLTournamentRaceEndStandingsItem>();
				DRLTournamentHeatResultData dRLTournamentHeatResultData = p_results[i];
				dRLTournamentRaceEndStandingsItem.Set((i + 1).ToString(), dRLTournamentHeatResultData.username, dRLTournamentHeatResultData.crashes.ToString(), FormatTime(dRLTournamentHeatResultData.score), dRLTournamentHeatResultData.color, dRLTournamentHeatResultData.success ? RaceStatusType.Success : RaceStatusType.Timeout);
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)base.transform);
		}

		public string FormatTime(float p_miliseconds)
		{
			if (p_miliseconds <= 0f)
			{
				return "-:-:-";
			}
			return new TimeSpan(0, 0, 0, 0, (int)p_miliseconds).ToString("m\\:ss\\.fff");
		}
	}
}

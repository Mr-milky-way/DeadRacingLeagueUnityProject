using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class DRLTournamentStandingsHeats : MonoBehaviour
	{
		public ListComponent heatBaseList;

		public ListComponent heatContentsList;

		public GameObject advancedGradient;

		public Image advancedGradientImage;

		public void Set(int p_totalHeatCount, List<Tuple<int, float, int>> p_results, Color p_userColor, bool p_suddenDeathMode = false, bool p_goldenHeatMode = false, bool p_isWinner = false, bool p_isWinnerSecond = false, int p_playerBestIndex = -1, int p_overallBestIndex = -1)
		{
			if (p_totalHeatCount <= 0)
			{
				return;
			}
			int num = ((p_results != null) ? (p_results.Count - 1) : (-1));
			bool flag = p_suddenDeathMode || p_goldenHeatMode;
			heatBaseList.Clear();
			heatContentsList.Clear();
			int num2 = 0;
			if (p_isWinner)
			{
				advancedGradientImage.color = p_userColor;
			}
			int num3 = 0;
			for (int i = 0; i < p_totalHeatCount; i++)
			{
				DRLTournamentStandingsHeatItem dRLTournamentStandingsHeatItem = heatBaseList.Push<DRLTournamentStandingsHeatItem>();
				DRLTournamentStandingsHeatItemContent dRLTournamentStandingsHeatItemContent = heatContentsList.Push<DRLTournamentStandingsHeatItemContent>();
				bool flag2 = false;
				if (i > num)
				{
					ResetHeatItem(dRLTournamentStandingsHeatItem, dRLTournamentStandingsHeatItemContent);
				}
				dRLTournamentStandingsHeatItemContent.ClearReplayContent();
				if (i == p_totalHeatCount - 1 && flag)
				{
					if (p_isWinner)
					{
						dRLTournamentStandingsHeatItem.SetLayoutActive();
						dRLTournamentStandingsHeatItemContent.SetAdvance(p_goldenHeatMode);
					}
					dRLTournamentStandingsHeatItemContent.SetUserColorStripe(p_userColor);
				}
				else if (p_results != null && p_results.Count > num3 && p_results[num3] != null && p_results[num3].Item3 == i + 1)
				{
					dRLTournamentStandingsHeatItem.SetLayoutActive();
					string p_rank = FormatRank(p_results[num3].Item1, flag);
					string p_time = FormatTime(p_results[num3].Item2);
					flag2 = p_goldenHeatMode && p_results[num3].Item1 == 2;
					dRLTournamentStandingsHeatItemContent.Set(p_rank, p_time, p_userColor, flag2, p_playerBestIndex == num3, (p_overallBestIndex == num3) ? true : false);
					if (flag2)
					{
						num2++;
					}
					num3++;
				}
			}
		}

		public void SetReplayStandings(int p_totalHeatCount, List<DRLTournamentReplayData> p_replayURLs, bool p_fromBrackets, UINavigation p_leftNav = null, UINavigation p_rightNav = null)
		{
			if (p_totalHeatCount <= 0)
			{
				return;
			}
			heatBaseList.Clear();
			heatContentsList.Clear();
			int i;
			for (i = 0; i < p_totalHeatCount; i++)
			{
				DRLTournamentStandingsHeatItem dRLTournamentStandingsHeatItem = heatBaseList.Push<DRLTournamentStandingsHeatItem>();
				DRLTournamentStandingsHeatItemContent dRLTournamentStandingsHeatItemContent = heatContentsList.Push<DRLTournamentStandingsHeatItemContent>();
				bool flag = false;
				if (p_replayURLs != null)
				{
					DRLTournamentReplayData dRLTournamentReplayData = p_replayURLs.Find((DRLTournamentReplayData o) => o.heat == i + 1);
					flag = dRLTournamentReplayData != null && !string.IsNullOrEmpty(dRLTournamentReplayData.URLs) && ((p_fromBrackets && dRLTournamentReplayData.backendReplaysReady) || dRLTournamentReplayData.replaysReady);
				}
				if (!flag)
				{
					ResetHeatItem(dRLTournamentStandingsHeatItem, dRLTournamentStandingsHeatItemContent);
				}
				dRLTournamentStandingsHeatItem.SetLayoutActive(p_active: false);
				dRLTournamentStandingsHeatItemContent.SetReplayContent(flag);
			}
			List<DRLTournamentStandingsHeatItemContent> list = heatContentsList.GetList<DRLTournamentStandingsHeatItemContent>();
			for (int num = 0; num < list.Count; num++)
			{
				DRLTournamentStandingsHeatItemContent dRLTournamentStandingsHeatItemContent2 = list[num];
				if (num == 0)
				{
					if (p_leftNav != null)
					{
						p_leftNav.right = dRLTournamentStandingsHeatItemContent2.watchButtonNavigation;
						dRLTournamentStandingsHeatItemContent2.watchButtonNavigation.left = p_leftNav;
					}
					if (list.Count > 1)
					{
						dRLTournamentStandingsHeatItemContent2.watchButtonNavigation.right = list[num + 1].watchButtonNavigation;
					}
				}
				else if (num == p_totalHeatCount - 1)
				{
					if (p_rightNav != null)
					{
						p_rightNav.left = dRLTournamentStandingsHeatItemContent2.watchButtonNavigation;
						dRLTournamentStandingsHeatItemContent2.watchButtonNavigation.right = p_rightNav;
					}
					if (list.Count > 1)
					{
						dRLTournamentStandingsHeatItemContent2.watchButtonNavigation.left = list[num - 1].watchButtonNavigation;
					}
				}
				else
				{
					dRLTournamentStandingsHeatItemContent2.watchButtonNavigation.left = list[num - 1].watchButtonNavigation;
					dRLTournamentStandingsHeatItemContent2.watchButtonNavigation.right = list[num + 1].watchButtonNavigation;
				}
			}
		}

		private void ResetHeatItem(DRLTournamentStandingsHeatItem p_heatBase, DRLTournamentStandingsHeatItemContent p_heatContent)
		{
			if (!(p_heatBase == null) && !(p_heatContent == null))
			{
				p_heatBase.gameObject.SetActive(value: true);
				p_heatBase.SetLayoutActive(p_active: false);
				p_heatContent.Clear();
			}
		}

		private string FormatRank(int p_rank, bool p_isSimCup)
		{
			if (p_rank < 0)
			{
				return "DNF";
			}
			if (!p_isSimCup)
			{
				return p_rank.ToString();
			}
			string text = "";
			return p_rank switch
			{
				1 => "1ST", 
				2 => "2ND", 
				3 => "3RD", 
				_ => p_rank + "TH", 
			};
		}

		private string FormatTime(float p_time)
		{
			if (p_time <= 0f)
			{
				return "DNF";
			}
			return new TimeSpan(0, 0, 0, 0, (int)p_time).ToString("m\\:ss\\.fff");
		}
	}
}

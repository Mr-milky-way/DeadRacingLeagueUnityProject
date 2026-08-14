using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UITournamentResultsView : UIScreenView
	{
		[Header("Game")]
		[HideInInspector]
		public DRLTournamentMatchData matchData;

		public DRLTournamentResultData data;

		private string m_title;

		[Header("Screen")]
		public GameObject promo;

		public GameObject promoSlash;

		public Text titleField;

		public Text roundField;

		public RectTransform headerRect;

		public FadeComponent feedbackFade;

		public GameObject feedbackNoResults;

		public GameObject feedbackLoading;

		public GameObject feedbackPending;

		public GameObject feedbackFailed;

		public FadeComponent headerFade;

		[Header("Stats")]
		public DRLTournamentStandings standings;

		[Header("Nav")]
		public UIElementView nextButton;

		public RectTransform replayButton;

		public UINavigation backButtonNav;

		public UINavigation nextButtonNav;

		[HideInInspector]
		public bool openedFromTheBrackets;

		[HideInInspector]
		public RaceController race;

		private Dictionary<string, Texture> m_flagsCache = new Dictionary<string, Texture>();

		public string title
		{
			set
			{
				m_title = value;
			}
		}

		public void SetPromoEnabled(bool p_flag)
		{
			if ((bool)promo)
			{
				promo.SetActive(p_flag);
			}
			if ((bool)promoSlash)
			{
				promoSlash.SetActive(p_flag);
			}
		}

		public void Clear()
		{
			titleField.text = "";
			roundField.text = "";
		}

		public void SetReplayEnabled(bool p_flag)
		{
			replayButton.GetComponent<UIElementView>().interactable = p_flag;
		}

		public void SetReplayActive(bool p_flag)
		{
			replayButton.gameObject.SetActive(value: false);
		}

		public void SetTitle(string p_roundName, string p_matchTitle)
		{
			titleField.text = p_matchTitle;
			roundField.text = p_roundName;
			LayoutRebuilder.ForceRebuildLayoutImmediate(headerRect);
			if (headerFade.alpha < 0.4f)
			{
				headerFade.FadeIn();
			}
		}

		public void ClearTable(bool p_animate = true)
		{
			standings.fade.FadeOut(0f);
			standings.Clear();
		}

		public void SetStandings(List<DRLTournamentStandingsItem> p_data, int p_heatCount, int p_activeHeat, bool p_suddenDeath = false, bool p_goldenHeat = false, List<DRLTournamentReplayData> p_replayURLs = null)
		{
			standings.Set(p_data, p_heatCount, p_activeHeat, p_suddenDeath, p_goldenHeat, p_replayURLs, openedFromTheBrackets, backButtonNav, nextButtonNav);
		}

		public void SetFeedback(UITournamentLeaderboardFeedbackType p_feedback)
		{
			switch (p_feedback)
			{
			case UITournamentLeaderboardFeedbackType.None:
				feedbackFade.FadeOut();
				feedbackLoading.SetActive(value: false);
				feedbackNoResults.SetActive(value: false);
				feedbackFailed.SetActive(value: false);
				break;
			case UITournamentLeaderboardFeedbackType.Loading:
				feedbackLoading.SetActive(value: true);
				feedbackNoResults.SetActive(value: false);
				feedbackPending.SetActive(value: false);
				feedbackFailed.SetActive(value: false);
				feedbackFade.FadeIn();
				break;
			case UITournamentLeaderboardFeedbackType.NoResult:
				feedbackLoading.SetActive(value: false);
				feedbackNoResults.SetActive(value: true);
				feedbackPending.SetActive(value: false);
				feedbackFailed.SetActive(value: false);
				feedbackFade.FadeIn();
				break;
			case UITournamentLeaderboardFeedbackType.Pending:
				feedbackLoading.SetActive(value: false);
				feedbackNoResults.SetActive(value: false);
				feedbackPending.SetActive(value: true);
				feedbackFailed.SetActive(value: false);
				feedbackFade.FadeIn();
				break;
			case UITournamentLeaderboardFeedbackType.Failed:
				feedbackLoading.SetActive(value: false);
				feedbackNoResults.SetActive(value: false);
				feedbackPending.SetActive(value: false);
				feedbackFailed.SetActive(value: true);
				feedbackFade.FadeIn();
				break;
			}
		}

		public void SetMatchData(DRLTournamentMatchData p_matchData, TournamentRoundGameMode p_roundMode = TournamentRoundGameMode.matchPoints)
		{
			UISecondaryHeaderView headerSecondary = base.app.view.ui.headerSecondary;
			bool p_is_under_review = p_matchData?.isUnderReview ?? false;
			if (headerSecondary != null)
			{
				headerSecondary.Refresh(this, p_is_under_review);
			}
			Debug.Log($"UITournamentResultView>@@ TournamentRefreshState match_data == null:{p_matchData == null} is_under_review: {p_matchData.isUnderReview}");
			if (p_matchData == null || p_roundMode == TournamentRoundGameMode.leaderboard)
			{
				SetFeedback(UITournamentLeaderboardFeedbackType.NoResult);
				return;
			}
			bool flag = p_roundMode == TournamentRoundGameMode.suddenDeath;
			bool flag2 = p_roundMode == TournamentRoundGameMode.goldenHeat;
			List<DRLTournamentReplayData> p_replayURLs = base.app.model.tournament.FetchMatchReplays(p_matchData.Id);
			SetStandings(GetSortedPlayers(p_matchData.players, p_matchData.scores, flag || flag2), p_matchData.heatCount, p_matchData.activeHeat, flag, flag2, p_replayURLs);
			standings.fade.FadeIn();
		}

		private string FormatTime(float p_time)
		{
			if (p_time <= 0f)
			{
				return "-:-:-";
			}
			return new TimeSpan(0, 0, 0, 0, (int)p_time).ToString("m\\:ss\\.fff");
		}

		private List<DRLTournamentStandingsItem> GetSortedPlayers(DRLTournamentPlayerData[] players, DRLTournamentScoreData[] results, bool p_isSimCup)
		{
			List<DRLTournamentStandingsItem> list = new List<DRLTournamentStandingsItem>();
			float num = float.PositiveInfinity;
			int num2 = -1;
			int num3 = -1;
			List<int> list2 = new List<int>();
			int num4 = 0;
			for (int i = 0; i < players.Length; i++)
			{
				list2.Add(-1);
				float num5 = float.PositiveInfinity;
				num4 = 0;
				for (int j = 0; j < results.Length; j++)
				{
					float num6 = results[j].score;
					if (results[j].status != "Success")
					{
						num6 = -1f;
					}
					if (players[i].playerId == results[j].playerId)
					{
						if (num6 < num5 && num6 != -1f)
						{
							num5 = num6;
							list2[i] = num4;
						}
						if (num6 < num && num6 != -1f)
						{
							num3 = i;
							num = num6;
							num2 = num4;
						}
						num4++;
					}
				}
			}
			for (int k = 0; k < players.Length; k++)
			{
				DRLTournamentStandingsItem dRLTournamentStandingsItem = new DRLTournamentStandingsItem();
				dRLTournamentStandingsItem.playerId = players[k].playerId;
				dRLTournamentStandingsItem.username = players[k].profileName.ToUpper();
				dRLTournamentStandingsItem.color = players[k].profileColor;
				GetPlayerFlag(players[k].flagThumbURL, dRLTournamentStandingsItem.playerId);
				dRLTournamentStandingsItem.isWinner = players[k].isWinner;
				dRLTournamentStandingsItem.isWinnerSecond = players[k].isWinnerSecond && !players[k].isWinner;
				dRLTournamentStandingsItem.totalWins = (p_isSimCup ? players[k].totalWins : players[k].points);
				dRLTournamentStandingsItem.rank = k + 1;
				dRLTournamentStandingsItem.overallBestIndex = -1;
				List<Tuple<int, float, int>> list3 = new List<Tuple<int, float, int>>();
				num4 = 0;
				for (int l = 0; l < results.Length; l++)
				{
					int item = (p_isSimCup ? results[l].position : results[l].points);
					float item2 = results[l].score;
					if (results[l].status != "Success")
					{
						item2 = -1f;
					}
					if (players[k].playerId == results[l].playerId)
					{
						if (num4 == num2 && k == num3)
						{
							dRLTournamentStandingsItem.overallBestIndex = num4;
						}
						list3.Add(new Tuple<int, float, int>(item, item2, results[l].heat));
						num4++;
					}
				}
				dRLTournamentStandingsItem.results = list3;
				dRLTournamentStandingsItem.playerBestIndex = list2[k];
				list.Add(dRLTournamentStandingsItem);
			}
			return list;
		}

		private void GetPlayerFlag(string p_url, string p_playerID)
		{
			if (string.IsNullOrEmpty(p_url))
			{
				Debug.LogWarning("TournamentResultsView> no flag URL provided!");
				return;
			}
			if (m_flagsCache.ContainsKey(p_url))
			{
				SetPlayerFlag(p_playerID, m_flagsCache[p_url]);
				return;
			}
			Texture flagTexture = null;
			Web.Get(p_url, delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
			{
				if (!(p_progress < 1f) && p_req.code == 200 && !(this == null))
				{
					if ((bool)p_result)
					{
						flagTexture = p_result;
					}
					if (flagTexture != null && !m_flagsCache.ContainsKey(p_url))
					{
						m_flagsCache.Add(p_url, flagTexture);
					}
					SetPlayerFlag(p_playerID, flagTexture);
				}
			});
		}

		private void SetPlayerFlag(string p_playerId, Texture p_flagIcon)
		{
			standings.SetPlayerFlagIcon(p_playerId, p_flagIcon);
		}

		public void SetCapturedBackground(RenderTexture p_capturedBackground)
		{
			if (!(p_capturedBackground == null))
			{
				base.app.view.ui.screens.SetStaticBackground(p_capturedBackground);
			}
		}
	}
}

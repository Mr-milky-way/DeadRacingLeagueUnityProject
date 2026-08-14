using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UITournamentWinnersController : Controller<DRLApp>
	{
		protected bool m_ignore_page_click;

		private int m_max_pages = 1;

		public int autoRetryAttempts = 5;

		public float autoRetryDelay = 2f;

		private int m_autoAttempts;

		private bool m_resultsPopulated;

		public UITournamentWinnersView view => AssertLocal<UITournamentWinnersView>("view");

		public TournamentModel model => base.app.model.tournament;

		public DRLTournamentData tournament => model.tournament;

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				if (!(p_data[0] as UIScreen != view.screen) && tournament != null)
				{
					view.listFade.Fade(0f, 0f);
					string text = "TOURNAMENT " + tournament.title.ToUpper();
					if (text.Length > 40)
					{
						text = text.Substring(0, 41) + "...";
					}
					view.title.text = text + " <color=red>/</color> WINNERS";
					m_max_pages = 1;
					m_autoAttempts = autoRetryAttempts;
					view.pageField.fade.FadeOut(0f);
					int num = tournament.rankings.Length - view.flexboxItems.Count;
					if (num > 0)
					{
						m_max_pages += num / view.regularList.Count + 1;
					}
					m_resultsPopulated = false;
					view.pageField.Set(m_max_pages);
					RefreshList(0);
					if ((bool)view.container && (bool)view.content)
					{
						LayoutRebuilder.ForceRebuildLayoutImmediate(view.content);
						LayoutRebuilder.ForceRebuildLayoutImmediate(view.container);
					}
					view.nextButton.SetActive(view.allowNext);
				}
				break;
			case "ui.screen.return@click":
				view.listFade.Fade(0f, 0f);
				view.pageField.fade.FadeOut(0f);
				base.app.view.ui.screens.Return();
				break;
			case "leaderboards.page@select":
				if (!m_ignore_page_click && !(view.pageField == null))
				{
					int p_page2 = (int)p_data[0];
					RefreshList(p_page2);
				}
				break;
			case "ui.screen.nav-right@click":
				if (tournament != null)
				{
					model.isPastTournament = true;
					base.app.view.ui.screens.Open<UITournamentBracketsView>("tournament-brackets-screen");
				}
				break;
			case "leaderboards.page-next@click":
				if (!m_ignore_page_click && !(view.pageField == null))
				{
					int index = view.pageField.index;
					int count = view.pageField.listField.Count;
					if (index + 1 != count)
					{
						view.pageField.index = index + 1;
						RefreshList(view.pageField.index);
					}
				}
				break;
			case "leaderboards.page-previous@click":
				if (!m_ignore_page_click && !(view.pageField == null))
				{
					int index2 = view.pageField.index;
					_ = view.pageField.listField.Count;
					if (index2 != 0)
					{
						view.pageField.index = index2 - 1;
						RefreshList(view.pageField.index);
					}
				}
				break;
			case "tournament.action.refresh":
				if (!m_resultsPopulated)
				{
					int p_page = ((view.pageField != null) ? view.pageField.index : 0);
					RefreshList(p_page);
				}
				break;
			}
		}

		protected void RefreshList(int p_page)
		{
			if (tournament == null || p_page < 0)
			{
				return;
			}
			view.Clear();
			if (tournament.rankings == null || tournament.rankings.Length == 0)
			{
				view.SetFeedback(UILeaderboardFeedbackType.Loading);
				base.app.model.service.GetTournament(tournament.guid, delegate(DRLTournamentResult result)
				{
					if (base.validContext && result != null && result.tournaments.Length != 0 && (result.tournaments[0].rankings == null || result.tournaments[0].rankings.Length == 0))
					{
						this.TimerRunOnce(delegate
						{
							if (base.validContext && (tournament.rankings == null || tournament.rankings.Length == 0))
							{
								m_autoAttempts--;
								if (m_autoAttempts <= 0)
								{
									view.SetFeedback(UILeaderboardFeedbackType.NoResult);
									m_resultsPopulated = false;
								}
								else
								{
									RefreshList(0);
								}
							}
						}, autoRetryDelay);
					}
				});
			}
			m_resultsPopulated = true;
			PopulateResults(tournament.rankings, (p_page == 0) ? view.flexboxItems : view.regularList, p_page, m_max_pages);
		}

		public void PopulateResults(DRLTournamentPlayerData[] p_players, List<UILeaderboardItemView> p_itemList, int p_page, int p_total_pages)
		{
			List<DRLTournamentPlayerData> list = new List<DRLTournamentPlayerData>(p_players);
			DRLPagePickerView pageField = view.pageField;
			if ((bool)pageField)
			{
				pageField.GetComponent<UINavigation>();
			}
			float num = 0f;
			int i = 0;
			int j = 0;
			if (p_page > 0)
			{
				i = view.flexboxItems.Count + view.regularList.Count * (p_page - 1);
			}
			view.flexbox.SetActive(p_page == 0);
			view.regular.SetActive(p_page != 0);
			for (; i < list.Count; i++)
			{
				if (j >= p_itemList.Count)
				{
					break;
				}
				UILeaderboardItemView uILeaderboardItemView = p_itemList[j];
				DRLTournamentPlayerData dRLTournamentPlayerData = list[i];
				uILeaderboardItemView.Set(dRLTournamentPlayerData, num);
				uILeaderboardItemView.selected = dRLTournamentPlayerData.playerId == base.app.model.service.backend.playerId;
				if (i < 9)
				{
					uILeaderboardItemView.positionField.text = "0" + (i + 1);
				}
				else
				{
					uILeaderboardItemView.positionField.text = (i + 1).ToString();
				}
				if (i < 6)
				{
					uILeaderboardItemView.profileName = "0" + (i + 1) + " - " + uILeaderboardItemView.profileNameField.text;
				}
				uILeaderboardItemView.flagContainer.SetActive(value: false);
				j++;
			}
			for (; j < p_itemList.Count; j++)
			{
				p_itemList[j].Clear(num, fadeOut: false);
				num += 0.02f;
			}
			UILeaderboardFeedbackType feedback = UILeaderboardFeedbackType.None;
			if (list.Count <= 0)
			{
				feedback = UILeaderboardFeedbackType.NoResult;
			}
			view.SetFeedback(feedback);
			if ((bool)pageField)
			{
				m_ignore_page_click = true;
				FadeComponent fade = pageField.fade;
				if (fade.alpha < 0f)
				{
					fade.alpha = 0f;
				}
				if (p_total_pages > 1)
				{
					fade.FadeIn(0.3f);
				}
				else
				{
					fade.FadeOut(0.3f);
				}
				pageField.Set(p_total_pages);
				pageField.index = p_page;
				m_ignore_page_click = false;
			}
			if ((bool)view.container && (bool)view.content)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(view.content);
				LayoutRebuilder.ForceRebuildLayoutImmediate(view.container);
			}
		}
	}
}

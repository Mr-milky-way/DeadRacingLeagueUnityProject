using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UITryoutsLeadersController : Controller<DRLApp>
	{
		public int pageLength = 24;

		public DRLLeaderboardData campaignSelected;

		protected WebAsyncRequest m_loader;

		protected WebAsyncRequest m_user_search;

		protected WebAsyncRequest m_replay_loader;

		protected Activity m_load_timer;

		protected bool m_ignore_page_click;

		protected bool m_ignore_replay_click;

		public UITryoutsLeadersView view => AssertLocal<UITryoutsLeadersView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
			{
				if (p_data[0] as UIScreen != view.screen)
				{
					break;
				}
				view.listFade.Fade(0f, 0f);
				DRLCampaign data = view.data;
				view.Set(data);
				if ((bool)view.pageField)
				{
					DRLAppArguments.Leaderboards leaderboardsCampaign = base.app.arguments.leaderboardsCampaign;
					int p_page2 = 0;
					if (leaderboardsCampaign != null)
					{
						p_page2 = leaderboardsCampaign.racePage;
					}
					if (view.overridePage > 0)
					{
						p_page2 = view.overridePage;
					}
					view.overridePage = -1;
					RefreshList(p_page2);
				}
				else
				{
					RefreshList(0);
				}
				break;
			}
			case "campaign.open.leaders@click":
			{
				UITryoutsLeadersView uITryoutsLeadersView = base.app.view.ui.screens.Open<UITryoutsLeadersView>("tryouts-leaderboard-screen");
				uITryoutsLeadersView.data = view.data;
				uITryoutsLeadersView.AllowNext(p_flag: false);
				uITryoutsLeadersView.overridePage = 1;
				break;
			}
			case "campaign.tryouts.leaders.item@click":
			{
				UIElementView uIElementView = p_target as UIElementView;
				if ((bool)uIElementView && view.tournamentData == null)
				{
					UILeaderboardItemView componentInParent = uIElementView.GetComponentInParent<UILeaderboardItemView>();
					SaveArgs(componentInParent.data);
					view.listFade.Fade(0f, 0f);
					UILeaderboardsView uILeaderboardsView = base.app.view.ui.screens.Open<UILeaderboardsView>("leaderboards-screen");
					uILeaderboardsView.SetGameType(GameFlag.Campaign);
					uILeaderboardsView.SetCampaignRaceMode(p_flag: true);
					uILeaderboardsView.SetTryouts(view.data, componentInParent.data.profileName);
				}
				break;
			}
			case "ui.screen.nav-right@click":
				view.listFade.Fade(0f, 0f);
				base.app.view.ui.screens.Open<UICampaignOverviewView>("campaign-overview-screen").data = view.data;
				break;
			case "ui.screen.return@click":
				view.listFade.Fade(0f, 0f);
				view.tournamentData = null;
				base.app.view.ui.screens.Return();
				break;
			case "leaderboards.page@select":
				if (!m_ignore_page_click && !(view.pageField == null))
				{
					int p_page = (int)p_data[0];
					RefreshList(p_page);
				}
				break;
			case "leaderboards.page-next@click":
				if (!m_ignore_page_click && !(view.pageField == null))
				{
					int index2 = view.pageField.index;
					int count = view.pageField.listField.Count;
					if (index2 + 1 != count)
					{
						view.pageField.index = index2 + 1;
						RefreshList(index2);
					}
				}
				break;
			case "leaderboards.page-previous@click":
				if (!m_ignore_page_click && !(view.pageField == null))
				{
					int index = view.pageField.index;
					_ = view.pageField.listField.Count;
					if (index != 0)
					{
						view.pageField.index = index - 1;
						RefreshList(index);
					}
				}
				break;
			case "tournament.action.refresh":
				RefreshList(0);
				break;
			}
		}

		protected void SaveArgs(DRLLeaderboardData p_leaderboardData)
		{
			DRLAppArguments.Leaderboards leaderboards = base.app.arguments.leaderboardsCampaign;
			if (leaderboards == null)
			{
				leaderboards = new DRLAppArguments.Leaderboards();
			}
			leaderboards.map = null;
			leaderboards.track = null;
			leaderboards.customMap = null;
			leaderboards.campaign = view.data;
			leaderboards.campaignIndex = 1;
			leaderboards.mission = null;
			leaderboards.missionIndex = 0;
			leaderboards.racePage = (view.pageField ? view.pageField.index : 0);
			leaderboards.campaignSelectd = p_leaderboardData;
			leaderboards.isCampaignRaceMode = true;
			leaderboards.campaignRaceModePage = 0;
			leaderboards.gameType = GameFlag.Campaign;
			base.app.arguments.leaderboardsCampaign = leaderboards;
			base.app.arguments.lastLeaderboard = DRLAppArguments.LeaderboardType.campaign;
		}

		protected void CancelReplayLoad()
		{
			m_ignore_replay_click = false;
			if (m_replay_loader != null)
			{
				m_replay_loader.Cancel();
			}
		}

		protected void RefreshList(int p_page)
		{
			view.Clear();
			if (view.tournamentData == null)
			{
				if (view.header != null)
				{
					view.header.gameObject.SetActive(value: false);
				}
				GetLeaderboard(p_page);
			}
			else
			{
				view.title.text = "TOURNAMENT " + view.tournamentData.title.ToUpper() + " <color=red>/</color> WINNERS";
				view.header.gameObject.SetActive(value: true);
				PopulateResults(view.tournamentData.rankings, view.listField);
			}
		}

		protected void GetLeaderboard(int p_page)
		{
			if (m_loader != null)
			{
				m_loader.Cancel();
			}
			view.SetFeedback(UILeaderboardFeedbackType.Loading, p_hide_list: true);
			Debug.Log("UILeaderboardController> RefreshList - game-type[" + GameFlag.Campaign.ToString() + "] campaign[" + view.data.label + "]");
			DRLLeaderboardData dRLLeaderboardData = new DRLLeaderboardData();
			dRLLeaderboardData.gameType = GameFlag.Campaign.ToString();
			dRLLeaderboardData.page = p_page + 1;
			dRLLeaderboardData.limit = pageLength;
			dRLLeaderboardData.scoreTypeFlag = ScoreType.TimeMin;
			dRLLeaderboardData.diameter = 6;
			dRLLeaderboardData.group = view.data.guid;
			m_loader = base.app.model.service.GetLeaderboard(dRLLeaderboardData, delegate(DRLLeaderboardResult p_result)
			{
				OnLeaderboardRacesLoad(p_result);
			});
		}

		protected void OnLeaderboardRacesLoad(DRLLeaderboardResult p_result)
		{
			if (!(this == null))
			{
				if (p_result == null)
				{
					Debug.LogWarning("UILeaderboardController> OnLeaderboardRacesLoad - Error Loading the Results!");
					m_loader = null;
					return;
				}
				DRLLeaderboardData[] leaderboard = p_result.leaderboard;
				int p_page = p_result.pagging.page - 1;
				int pageTotal = p_result.pagging.pageTotal;
				PopulateResults(leaderboard, view.listField, p_campaign_race: false, p_page, pageTotal);
			}
		}

		public void PopulateResults(DRLTournamentPlayerData[] p_players, List<UILeaderboardItemView> p_itemList)
		{
			List<DRLTournamentPlayerData> list = new List<DRLTournamentPlayerData>(p_players);
			float num = 0f;
			int i;
			for (i = 0; i < list.Count && i < p_itemList.Count; i++)
			{
				UILeaderboardItemView uILeaderboardItemView = p_itemList[i];
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
			}
			for (; i < p_itemList.Count; i++)
			{
				p_itemList[i].Clear(num);
				num += 0.02f;
			}
			UILeaderboardFeedbackType feedback = UILeaderboardFeedbackType.None;
			if (list.Count <= 0)
			{
				feedback = UILeaderboardFeedbackType.NoResult;
			}
			view.SetFeedback(feedback);
		}

		public void PopulateResults(DRLLeaderboardData[] p_races, List<UILeaderboardItemView> p_itemList, bool p_campaign_race, int p_page, int p_pageTotal)
		{
			List<DRLLeaderboardData> list = new List<DRLLeaderboardData>(p_races);
			float num = 0f;
			bool p_allow_replay = false;
			DRLPagePickerView pageField = view.pageField;
			if ((bool)pageField)
			{
				pageField.GetComponent<UINavigation>();
			}
			Debug.Log("UILeaderboardController> PopulateResults - count[" + list.Count + "] campaign-race[" + p_campaign_race + "]");
			if (p_campaign_race)
			{
				list.Sort((DRLLeaderboardData lba, DRLLeaderboardData lbb) => (lba.order >= lbb.order) ? 1 : (-1));
			}
			int num2 = 0;
			for (int num3 = 0; num3 < list.Count; num3++)
			{
				num2++;
				UILeaderboardItemView uILeaderboardItemView = p_itemList[num3];
				DRLLeaderboardData dRLLeaderboardData = list[num3];
				uILeaderboardItemView.Set(dRLLeaderboardData, p_allow_replay, p_allow_save: false, num);
				uILeaderboardItemView.SetCampaignRaceMode(p_campaign_race);
				uILeaderboardItemView.selected = dRLLeaderboardData.playerId == base.app.model.service.backend.playerId && !p_campaign_race;
				DRLCampaign dRLCampaign = (p_campaign_race ? view.data : null);
				if ((bool)dRLCampaign)
				{
					int p_phase = 0;
					int p_heat = 0;
					DRLCampaignRace race = dRLCampaign.GetRace(dRLLeaderboardData.order, out p_phase, out p_heat);
					string text = race.phaseNames[p_phase];
					DRLMapTrack track = race.track;
					DRLMap map = track.map;
					uILeaderboardItemView.SetCampaignRaceTitle(map.label, track.label, text + " - Heat " + (p_heat + 1).ToString("00"));
				}
				num += 0.02f;
			}
			for (int num4 = num2; num4 < 24; num4++)
			{
				p_itemList[num4].Clear(num);
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
				if (p_pageTotal > 1)
				{
					fade.FadeIn(0.3f);
				}
				else
				{
					fade.FadeOut(0.3f);
				}
				pageField.Set(p_pageTotal);
				pageField.index = p_page;
				m_ignore_page_click = false;
			}
			if ((bool)view.allResultsButton)
			{
				view.allResultsButton.gameObject.SetActive(p_pageTotal > 1);
			}
		}
	}
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UICampaignOverviewController : Controller<DRLApp>
	{
		public bool resetAllowed;

		public List<DRLRaceResultData> results;

		public int heatId;

		public int phaseId;

		public string phaseName;

		public int raceId;

		public int raceOrder;

		private MonoActivity m_autofocus_timer;

		public UICampaignOverviewView view => AssertLocal<UICampaignOverviewView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event != null)
			{
				_ = p_event == "ui.screen@close";
			}
			if (base.app.view.ui.screens.current != view.screen)
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
				DRLCampaign d = view.data;
				if (!d)
				{
					Debug.LogWarning("UICampaignOverviewController> Invalid Campaign Data");
					break;
				}
				Debug.Log("UICampaignOverviewController> Open - campaign[" + d.label + "]");
				Init();
				view.SetLeadersEnabled(p_flag: true);
				view.SetQualifySuccess(0);
				view.SetQualifyTime(d.qualifyTime);
				ServiceModel sm = base.app.model.service;
				sm.GetLeaderboardRivals("", d, null, null, 1, 6, p_official: true, p_customPhysics: false, delegate(DRLLeaderboardRivalsResult p_result)
				{
					sm.SetLeaderboardCard(view.userCard, p_self: true, p_result);
					if (p_result != null)
					{
						float qualifyTime = d.qualifyTime;
						DRLLeaderboardData player = p_result.GetPlayer();
						int p_state = ((player != null) ? ((player.scoreSeconds <= qualifyTime) ? 1 : 2) : 0);
						view.SetQualifySuccess(p_state, 1f);
					}
				});
				sm.SetLeaderboardCard(view.leaderCard, p_self: false, d, 6, p_official: true, p_customPhysics: false);
				RunOnce(0.5f, LoadResults);
				break;
			}
			case "campaign.campaign-map-card@click":
			{
				UICardButtonCampaignMap uICardButtonCampaignMap = (UICardButtonCampaignMap)p_target;
				if (!uICardButtonCampaignMap)
				{
					break;
				}
				if (uICardButtonCampaignMap.locked)
				{
					base.app.view.audio.PlayUIGenericError();
					break;
				}
				if (uICardButtonCampaignMap.complete)
				{
					base.app.view.audio.PlayUIGenericError();
					break;
				}
				DRLCampaignRace race = uICardButtonCampaignMap.race;
				string text = (race.isCustomMap ? "" : uICardButtonCampaignMap.race.track.map.label);
				string text2 = (race.isCustomMap ? race.customMap.mapTitle.ToUpper() : uICardButtonCampaignMap.race.track.label);
				string text3 = (race.isCustomMap ? text2 : (text + "/" + text2));
				Debug.Log("UICampaignOverviewController> Race Click - track[" + text3 + "]");
				UIMapTrackOverviewView uIMapTrackOverviewView = base.app.view.ui.screens.Open<UIMapTrackOverviewView>("track-overview-screen");
				uIMapTrackOverviewView.screen.title = text2 + "/" + phaseName.ToUpper() + " HEAT " + (heatId + 1).ToString("00");
				if (race.isCustomMap)
				{
					uIMapTrackOverviewView.Set(race.customMap);
				}
				else
				{
					uIMapTrackOverviewView.Set(uICardButtonCampaignMap.race.track.map);
					uIMapTrackOverviewView.Set(uICardButtonCampaignMap.race.track);
				}
				uIMapTrackOverviewView.campaign = view.data;
				break;
			}
			case "campaign.restart@click":
			{
				if (!resetAllowed)
				{
					base.app.view.audio.PlayUIGenericError();
					break;
				}
				CampaignResultsModel campaign = base.app.model.storage.state.player.results.campaign;
				GameObject g = view.resetConfirmationField.gameObject;
				if (g.activeInHierarchy)
				{
					g.SetActive(value: false);
					campaign.Clear(view.data, p_increment_attempts: true);
					view.attemptsCount = campaign.GetAttempts(view.data);
					_ = (bool)base.app.model.game;
					Init();
					LoadResults();
					base.app.view.audio.PlayUIGenericSuccess();
				}
				else
				{
					g.SetActive(value: true);
					RunOnce(2f, delegate
					{
						g.SetActive(value: false);
					});
				}
				break;
			}
			case "ui.screen.return@click":
				base.app.model.storage.state.player.garage.currentRigData = null;
				base.app.controller.RefreshFooterDrone();
				base.app.view.ui.screens.Return();
				break;
			case "campaign.open.results@click":
				if (view.rightNavigation.enabled)
				{
					UICampaignResultView uICampaignResultView = base.app.view.ui.screens.Open<UICampaignResultView>("campaign-results-screen");
					uICampaignResultView.screen.title = view.data.label + " RESULT";
					uICampaignResultView.m_data = view.data;
				}
				break;
			case "campaign.open.leaders@click":
			{
				UITryoutsLeadersView uITryoutsLeadersView = base.app.view.ui.screens.Open<UITryoutsLeadersView>("tryouts-leaderboard-screen");
				uITryoutsLeadersView.data = view.data;
				uITryoutsLeadersView.AllowNext(p_flag: false);
				break;
			}
			}
		}

		public void Init()
		{
			DRLCampaign data = view.data;
			List<DRLCampaignRace> races = data.races;
			view.Clear();
			bool tournament = data.tournament;
			for (int i = 0; i < races.Count; i++)
			{
				DRLCampaignRace dRLCampaignRace = races[i];
				if (tournament && dRLCampaignRace.isCustomMap)
				{
					dRLCampaignRace.customMap.mapThumbURL = "";
				}
				view.Add(dRLCampaignRace);
			}
			view.time = 0f;
			view.attemptsCount = 0;
			view.SetProgress(0, races.Count);
			SetResetAllowed(f: false);
			SetResultsAvailable(f: false);
			UINavigation.Link(view.listField.GetComponent<LayoutGroup>(), view.resetButtonNav, view.rightNavigation);
		}

		public void LoadResults()
		{
			DRLCampaign d = view.data;
			CampaignResultsModel m = base.app.model.storage.state.player.results.campaign;
			string playerId = base.app.model.service.backend.playerId;
			results = m.FindAll(playerId, d);
			int count = results.Count;
			int p_phase = 0;
			int p_heat = 0;
			int raceIndex = d.GetRaceIndex(count, out p_phase, out p_heat);
			int num = raceIndex;
			bool campaign_complete = d.IsComplete(count);
			SetResetAllowed(count > 0);
			raceIndex = Mathf.Max(0, raceIndex - 1);
			DRLCampaignRace dRLCampaignRace = d.races[raceIndex];
			Debug.Log("UICampaignOverviewController> LoadResults - count[" + count + "] race_id[" + raceIndex + "] phase[" + p_phase + "/" + dRLCampaignRace.phases + "] heat[" + p_heat + "/" + dRLCampaignRace.heats + "]");
			float num2 = 0f;
			int lock_id = raceIndex;
			for (int i = 0; i < view.listField.Count; i++)
			{
				UICardButtonCampaignMap uICardButtonCampaignMap = view.listField.Get<UICardButtonCampaignMap>(i);
				bool flag = d.IsRaceComplete(count, i);
				bool flag2 = d.IsRaceComplete(count, i - 1);
				if (i > lock_id && (flag2 || campaign_complete))
				{
					lock_id++;
				}
				if (i > lock_id)
				{
					uICardButtonCampaignMap.SetLocked(p_flag: true, num2, p_show_result: false);
				}
				else
				{
					uICardButtonCampaignMap.complete = flag || campaign_complete;
					dRLCampaignRace = d.races[i];
					uICardButtonCampaignMap.SetLocked(p_flag: false, num2);
					if (uICardButtonCampaignMap.complete)
					{
						float raceTime = m.GetRaceTime(d, i);
						uICardButtonCampaignMap.time = 0f;
						uICardButtonCampaignMap.SetResult(raceTime, 0.5f + num2);
					}
					else
					{
						string text = dRLCampaignRace.phaseNames[p_phase];
						string text2 = (i + 1).ToString();
						string text3 = view.listField.Count.ToString();
						uICardButtonCampaignMap.SetResult(text.ToUpper() + " HEAT " + text2 + " OF " + text3);
						raceId = i;
						raceOrder = num;
						phaseId = p_phase;
						phaseName = text;
						heatId = p_heat;
					}
				}
				num2 += 0.1f;
			}
			int p_count = Mathf.Max(lock_id, 0);
			if (campaign_complete)
			{
				p_count = d.races.Count - 1;
			}
			view.SetProgress(p_count, d.races.Count);
			int attempts = m.GetAttempts(d);
			view.attemptsCount = attempts;
			float raceTime2 = m.GetRaceTime(d);
			Tween.Add(view, "time", raceTime2, 0.3f, Mathf.Abs(num2 - 0.3f), Cubic.Out);
			if (m_autofocus_timer != null)
			{
				m_autofocus_timer.Stop();
			}
			m_autofocus_timer = RunOnce(delegate
			{
				if (view.current)
				{
					UINavigation uINavigation = view.listField.Get<UINavigation>(lock_id);
					if (campaign_complete)
					{
						bool newHighScore = m.GetNewHighScore(d);
						SetResultsAvailable(f: true);
						if (!newHighScore)
						{
							return;
						}
						m.SetNewHighScore(d, p_flag: false);
						base.app.view.audio.PlayUINewRecord();
						uINavigation = view.rightNavigation;
					}
					if ((bool)uINavigation)
					{
						UINavigation.focus = uINavigation;
					}
				}
			}, num2);
		}

		public void SetResultsAvailable(bool f)
		{
			view.rightNavigation.enabled = f;
			view.navRightFade.Fade(f ? 1f : 0.1f, 1f, 0.2f);
		}

		public void SetResetAllowed(bool f)
		{
			resetAllowed = f;
			view.resetButtonNav.GetComponent<FadeComponent>().Fade(f ? 1f : 0.1f, 1f, 0.2f);
		}
	}
}

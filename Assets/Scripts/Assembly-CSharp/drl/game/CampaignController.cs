using System;
using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class CampaignController : RaceController
	{
		protected bool lockNextRaceCommand;

		private int m_delay_replay_update_tries;

		public new CampaignModel model => base.model as CampaignModel;

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			base.OnNotification(p_event, p_target, p_data);
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "game.restart":
				break;
			case "game.race-complete.restart@click":
				break;
			case "game.race-overview.restart@click":
				break;
			case "game.ready":
				GetCurrentRace();
				break;
			case "game.race-overview.next@click":
				if (!lockNextRaceCommand)
				{
					lockNextRaceCommand = true;
					base.game.NextCampaignRace(model.campaign);
				}
				break;
			case "game.count@complete":
				model.replayUploadComplete = false;
				lockNextRaceCommand = false;
				model.results.SetNextCampaignResultTime(model.campaign, 360f, 0, RaceStatusType.Timeout);
				break;
			case "game.race-overview.campaign@click":
			{
				UICampaignOverviewView uICampaignOverviewView = base.app.view.ui.screens.Open<UICampaignOverviewView>("campaign-overview-screen");
				uICampaignOverviewView.screen.title = model.campaign.title;
				uICampaignOverviewView.data = model.campaign;
				break;
			}
			case "game.pause.exit@click":
			{
				DRLCampaign campaign = model.campaign;
				Debug.LogWarning("CampaignController> Exit during game! campaign[" + campaign.guid + "]");
				model.results.Clear(campaign, p_increment_attempts: true);
				break;
			}
			}
		}

		protected override void SetTitle()
		{
			UIHUDTitle gameTitle = base.ui.hud.gameTitle;
			DRLMap map = base.app.scene.map;
			string p_caption_left = base.app.scene.track.label;
			if ((bool)map && map.data != null)
			{
				p_caption_left = map.data.mapTitle;
			}
			DRLCampaign campaign = model.campaign;
			int phase = model.phase;
			int heat = model.heat;
			int count = model.count;
			string phaseName = model.phaseName;
			string text = ((!string.IsNullOrEmpty(phaseName)) ? ("HEAT " + (heat + 1).ToString("00")) : ("HEAT " + (count + 1).ToString("00")));
			Debug.Log("CampaignController> SetTitle - count[" + count + "] phase[" + phase + "] heat[" + heat + "]");
			string text2 = phaseName.ToUpper();
			text2 = (string.IsNullOrEmpty(text2) ? text2 : (text2 + " - "));
			gameTitle.Set(map.label, p_caption_left, campaign.label, text2 + text);
		}

		public override string GetRaceTitle()
		{
			string title = model.campaign.title;
			int heat = model.heat;
			string phaseName = model.phaseName;
			return title + " / " + phaseName + " - Heat " + (heat + 1).ToString("00") + " Complete!";
		}

		public int GetCurrentRaceIndex()
		{
			List<DRLRaceResultData> list = model.results.FindAll(model.campaign);
			model.count = list.Count;
			int p_phase = 0;
			int p_heat = 0;
			int num = model.campaign.GetRaceIndex(model.count, out p_phase, out p_heat);
			if (num < 0)
			{
				num = 0;
			}
			return num;
		}

		protected void GetCurrentRace()
		{
			List<DRLRaceResultData> list = model.results.FindAll(model.campaign);
			model.count = list.Count;
			if (model.campaign.IsComplete(list.Count))
			{
				Debug.LogWarning("CampaignController> Campaign already complete!");
				return;
			}
			model.data = ((list.Count <= 0) ? null : list[list.Count - 1]);
			int p_phase = 0;
			int p_heat = 0;
			int num = model.campaign.GetRaceIndex(model.count, out p_phase, out p_heat);
			if (num < 0)
			{
				num = 0;
			}
			DRLCampaignRace dRLCampaignRace = model.campaign.races[num];
			model.phase = p_phase;
			model.heat = p_heat;
			model.race = num;
			model.phaseName = dRLCampaignRace.phaseNames[p_phase];
			Debug.Log("CampaignController> GetCurrentRace - count[" + model.count + "] race[" + num + "] phase[" + p_phase + "/" + model.phaseName + "] heat[" + p_heat + "]");
		}

		protected override void OnRaceComplete(float p_race_time, RaceStatusType p_status)
		{
			if (!model.raceComplete)
			{
				base.OnRaceComplete(p_race_time, p_status);
			}
		}

		public void UpdateRaceResult(Action p_callback = null)
		{
			RaceStatusType status = model.status;
			float time = model.time;
			if ((status != RaceStatusType.Crash && status != RaceStatusType.Success) || !base.validContext || !model.campaign)
			{
				return;
			}
			ServiceModel sm = base.app.model.service;
			Debug.Log("CampaignController> OnRaceComplete - " + status.ToString() + " - SetNextCampaignResultTime");
			DRLRaceResultData res = null;
			string storage_category = (model.campaign.tournament ? "tryouts" : "campaign");
			res = model.results.SetNextCampaignResultTime(model.campaign, time, model.crashes, status, GameFlag.SinglePlayer, p_remove_last: true);
			string text = "race" + res.order.ToString("00") + "complete";
			Notify("analytics.tryouts.completed-step", text);
			base.game.replay.recorder.model.ToBytesAsync(delegate(byte[] p_replay_data)
			{
				Activity.RunOnce(delegate
				{
					Debug.Log("CampaignController> Replay Parse success [" + p_replay_data.Length + " bytes] category[" + storage_category + "]");
					sm.StorageTemp(storage_category, p_replay_data, delegate(string p_link)
					{
						m_delay_replay_update_tries = 5;
						DelayUpdateReplayData(res, p_link, p_callback);
					});
				});
			}, 2f);
			List<DRLRaceResultData> list = model.results.FindAll(model.campaign);
			model.campaignComplete = model.campaign.IsComplete(list.Count);
		}

		protected void DelayUpdateReplayData(DRLRaceResultData p_result, string p_link, Action p_callback = null)
		{
			if (m_delay_replay_update_tries <= 0)
			{
				return;
			}
			m_delay_replay_update_tries--;
			DRLRaceResultData res = p_result;
			Activity.RunOnce(delegate
			{
				Debug.Log("CampaignController> Refreshing replay information for result[" + p_result.guid + "]");
				if (!base.app)
				{
					Debug.LogWarning("CampaignController> Failed to update replay for result[" + p_result.guid + "] / Retry!");
					DelayUpdateReplayData(p_result, p_link, p_callback);
				}
				else
				{
					res.replay = p_link;
					base.app.model.storage.state.player.results.campaign.UpdateResult(res);
					if ((bool)model)
					{
						model.replayUploadComplete = true;
					}
					if (p_callback != null)
					{
						p_callback();
					}
					if ((bool)model)
					{
						Notify(1f / 60f, "campaign.result.replay@complete");
					}
				}
			}, 2f);
		}

		protected void OnCampaignRaceReset()
		{
			if ((bool)model.campaign)
			{
				List<DRLRaceResultData> list = model.results.FindAll(model.campaign);
				bool flag = false;
				if (model.count >= 0 && model.count < list.Count)
				{
					list.RemoveAt(model.count);
					model.results.parent.list = list;
					flag = true;
				}
				Debug.Log("CampaignController> RaceRestart - Remove RaceResult - guid[" + model.campaign.guid + "] remove[" + model.count + "/" + list.Count + "] success[" + flag + "]");
			}
			else
			{
				Debug.LogWarning("CampaignController> No ResultModel to remove.");
			}
		}

		protected override void RequestRaceReset()
		{
		}

		public override void SetLeaderboard(Action<DRLLeaderboardData> p_callback, DroneRigData p_rig = null)
		{
			UpdateRaceResult(delegate
			{
				base.SetLeaderboard((Action<DRLLeaderboardData>)delegate(DRLLeaderboardData p_race_result)
				{
					SetCampaignLeaderboard(p_race_result, p_callback);
				}, p_rig);
			});
		}

		public void SetCampaignLeaderboard(DRLLeaderboardData p_race_result, Action<DRLLeaderboardData> p_callback)
		{
			if (p_race_result == null)
			{
				Debug.LogWarning("CampaignController> SetLeaderboard - Failed to send race results!");
				if (p_callback != null)
				{
					p_callback(null);
				}
				return;
			}
			if (p_callback != null)
			{
				p_callback(p_race_result);
			}
			DRLCampaign campaign = model.campaign;
			bool flag = false;
			if (!model.results.IsCampaignComplete(campaign))
			{
				Debug.LogWarning("CampaignController> SetLeaderboard - Campaign not complete!");
				return;
			}
			DroneRigData rig = base.game.model.playerData.rig;
			List<DRLLeaderboardData> leaderboards = base.app.model.storage.state.player.results.campaign.GetLeaderboards(campaign);
			for (int i = 0; i < leaderboards.Count; i++)
			{
				if (flag)
				{
					leaderboards[i].force = true;
				}
				leaderboards[i].diameter = rig.diameter;
				leaderboards[i].droneName = rig.name;
				leaderboards[i].droneThumb = rig.thumb0;
			}
			base.app.model.service.SetLeaderboard(leaderboards.ToArray(), delegate(DRLLeaderboardData p_result)
			{
				if (p_result == null)
				{
					Debug.LogWarning("CampaignController> SetLeaderboard - Failed to send results!");
					if (p_callback != null)
					{
						p_callback(null);
					}
				}
				else
				{
					Debug.Log("CampaignController> SetLeaderboard - Success!\n" + p_result.profileName + "\n" + p_result.position + "\nhighscore: " + p_result.highscore);
					model.results.SetNewHighScore(campaign, p_result.highscore);
					if (p_callback != null)
					{
						p_callback(p_result);
					}
				}
			});
		}
	}
}

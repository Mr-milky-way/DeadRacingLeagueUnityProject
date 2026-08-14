using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLOnboardingController : Controller<DRLApp>
	{
		private GameController m_game;

		public OnboardingCampaignMode selectedDifficulty;

		private bool startLoading;

		public bool is_custom;

		public DRLOnboardingModel model => AssertLocal<DRLOnboardingModel>("model");

		public int progress
		{
			get
			{
				return model.GetProgress();
			}
			set
			{
				model.SetProgress(value);
			}
		}

		public int stepIndex
		{
			get
			{
				return model.currentStep;
			}
			private set
			{
				model.SetActiveStep(value);
			}
		}

		protected DRLOnboarding activeOnboarding => model.activeOnboarding;

		public GameController game
		{
			get
			{
				if (!m_game)
				{
					return m_game = (base.app ? base.app.controller.game : null);
				}
				return m_game;
			}
		}

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (!base.app.inOnboarding || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "onboarding.start.beginner@click":
				OnSelectedDifficulty(OnboardingCampaignMode.Beginner);
				break;
			case "onboarding.start.intermediate@click":
				OnSelectedDifficulty(OnboardingCampaignMode.Intermediate);
				break;
			case "onboarding.start.pro@click":
				OnSelectedDifficulty(OnboardingCampaignMode.Pro);
				break;
			case "onboarding.step.next@start":
				StartStep(stepIndex + 1);
				break;
			case "onboarding.step.previous@start":
				StartStep(stepIndex - 1);
				break;
			case "onboarding.step.current@click":
				if (base.app.model.onboarding.IsMissionStep(base.app.model.onboarding.currentStep, activeOnboarding))
				{
					StartStep(model.currentStep);
				}
				StartStep(model.currentStep);
				break;
			case "onboarding.start.race":
				if (!model.IsCompleted())
				{
					stepIndex = progress;
				}
				else
				{
					stepIndex = ((activeOnboarding.mode != OnboardingCampaignMode.Pro) ? (stepIndex + 1) : 0);
				}
				StartStep(stepIndex);
				break;
			case "onboarding.finish@click":
				StopOnboarding();
				break;
			case "onboarding.stop":
				StopOnboarding();
				break;
			case "onboarding.skip@click":
				model.skipOnboarding = true;
				StopOnboarding();
				break;
			case "onboarding.missions-complete.next@click":
			{
				int currentStep = base.app.model.onboarding.currentStep;
				Notify("onboarding.progress@increase");
				base.app.arguments.Clear();
				if (currentStep > activeOnboarding.steps.Count)
				{
					base.app.view.ui.fade.FadeIn(1.5f);
					base.app.view.ui.screens.Close("onboarding-steps-menu-screen");
					base.app.view.ui.screens.manager.ClearHistory();
					base.app.view.ui.game.hud.Hide();
					Activity.RunOnce(delegate
					{
						base.app.view.ui.footer.Hide(0f);
					}, 1f / 30f);
					UIOnboardingCompleteView uIOnboardingCompleteView = base.app.view.ui.screens.Open<UIOnboardingCompleteView>("onboarding-complete-screen", 0.3f);
					uIOnboardingCompleteView.isMissionCompleted = true;
					uIOnboardingCompleteView.SetNextButtonRaceText();
				}
				else if (base.app.model.onboarding.IsMissionStep(base.app.model.onboarding.currentStep, base.app.model.onboarding.activeOnboarding))
				{
					base.app.controller.onboarding.StartStep(model.currentStep);
				}
				break;
			}
			case "game.race.leaderboard-complete":
				if (base.app.inOnboarding && !model.IsStepComplete(stepIndex) && model.GetStepType() == OnboardingStep.OnboardingStepType.Race && base.app.inGame)
				{
					List<GamePlayerData> rankings = base.app.controller.game.race.model.Rankings;
					if (rankings.Count != 0 && !(rankings[0].playerId != base.app.model.storage.state.player.profile.playerId))
					{
						model.IncreaseProgress();
						RefreshProgressUI();
					}
				}
				break;
			case "onboarding.progress@increase":
				if (!base.app.model.onboarding.hasFailed)
				{
					model.SetOnboardingActive(activeOnboarding.mode);
					model.currentStep++;
				}
				break;
			case "onboarding.mission.complete@increase":
				if (!base.app.model.onboarding.hasFailed)
				{
					model.SetOnboardingActive(activeOnboarding.mode);
					model.SetStepComplete(model.activeOnboarding.mode, model.currentStep);
					RefreshProgressUI();
				}
				break;
			case "onboarding.progress@decrease":
				if (!model.IsStepComplete(stepIndex))
				{
					model.SetOnboardingActive(activeOnboarding.mode);
					model.DecreaseProgress();
					RefreshProgressUI();
				}
				break;
			case "onboarding.restart.training":
				stepIndex = 0;
				StartStep();
				break;
			case "onboarding.progress.reset.all@click":
				base.app.view.ui.dialog.Open(DialogType.Warning, "RESET ONBOARDING PROGRESS", "DO YOU WISH TO RESET ONBOARDING PROGRESS?", new string[2] { "RESET", "CLOSE" }, null, null, delegate(string p_id, int p_option)
				{
					if (p_option == 1)
					{
						base.app.view.ui.fade.FadeIn(0.5f);
						this.TimerRunOnce(delegate
						{
							model.ResetAllProgress();
							base.app.view.ui.screens.Open<UIOnobardingMenuView>("onboarding-home-screen", 0.3f);
							base.app.view.ui.fade.FadeOut(0.5f, 0.7f);
						}, 0.6f);
					}
				});
				break;
			case "onboarding.pro.mission@click":
				Notify("onboarding.back.overview@click");
				break;
			case "onboarding.back.overview@click":
				base.app.view.ui.screens.Open<UIOnboardingOverviewView>("onboarding-overview-screen", 0.3f).onboardingData = model.activeOnboarding;
				break;
			}
		}

		public void GetBotData(OnboardingCampaignMode difficulty)
		{
			model.replayData = GetBotReplay(difficulty);
			if (model.replayData != null)
			{
				model.activeOnboarding.steps[model.step].opponentReplayId = model.replayData[model.GetCurrentRaceStep(activeOnboarding, model.currentStep)].Get<string>("replay-url");
			}
		}

		private void OnSelectedDifficulty(OnboardingCampaignMode difficulty)
		{
			model.skipOnboarding = true;
			selectedDifficulty = difficulty;
			model.SetOnboardingActive(difficulty);
			int activeStep = model.GetProgress();
			model.SetActiveStep(activeStep);
			OpenOnboardingProgress(fromStartOnboarding: true);
		}

		public void StartStep(int p_step = -1)
		{
			if (model.inProgress)
			{
				model.SetOnboardingActive(activeOnboarding.mode);
				if (p_step >= 0)
				{
					stepIndex = p_step;
				}
				Load(activeOnboarding.steps[stepIndex]);
			}
		}

		public int GetFirstRaceIndex(DRLOnboarding onboarding)
		{
			for (int i = 0; i < onboarding.steps.Count; i++)
			{
				if (onboarding.steps[i].type == OnboardingStep.OnboardingStepType.Race)
				{
					return i;
				}
			}
			Debug.LogWarning("No race found");
			return -1;
		}

		public void LoadBots(OnboardingStep stepModel, UIStatusView roomStatusField)
		{
			roomStatusField.fade.FadeOut(0f, 0.5f);
			Load(stepModel);
			OpponentModel om = base.app.model.service.opponent;
			om.Cancel();
			string text = "onboarding-replay-";
			string text2 = model.activeOnboarding.mode.ToString() + model.GetSelectedRaceStep(activeOnboarding, model.currentStep);
			text2 = text2.Replace("-", "");
			ReplayFile replayCache = base.app.model.storage.replays.GetReplayCache(text + text2);
			if (replayCache != null && replayCache.duration > 0f)
			{
				replayCache.header.profileName = "NPC (BOT)";
				base.app.arguments.game.AddGhostPlayer(replayCache);
				Activity.RunOnce(delegate
				{
					base.app.arguments.game.players[1].photo = model.botAvatar;
				}, 1f);
				Debug.LogWarning("Cached");
				if (base.app.model.game == null)
				{
					base.app.view.audio.SceneMainToGame(1.5f);
				}
				if (is_custom)
				{
					base.app.scene.LoadCommunityMap(stepModel.trackGuid);
				}
				else
				{
					RunOnce(base.app.scene.Load, 3f);
				}
				base.app.view.ui.fade.FadeIn();
				return;
			}
			if (!base.app.online)
			{
				base.app.view.ui.dialog.Open(DialogType.Warning, "MAP IS NOT DOWNLOADED", "YOU NEED TO START THE GAME ONLINE FIRST TO DOWNLOAD THE MAP", new string[1] { "OK" }, null, null, delegate(string p_id, int p_option)
				{
					if (p_option == 1)
					{
						base.app.view.ui.screens.mouseWheelScrollingEnabled = true;
						roomStatusField.fade.FadeOut(0f);
						base.app.view.ui.screens.Return();
					}
				});
				return;
			}
			om.Load(stepModel.opponentReplayId, selectedDifficulty, delegate
			{
				switch (om.status)
				{
				case OpponentModel.Status.Complete:
					if (!startLoading)
					{
						startLoading = true;
						Debug.LogWarning("NO Cache");
						if (ReplayFile.EnableVersion2)
						{
							ReplayRecord ghostRecordsV = om.ghostRecordsV2;
							ghostRecordsV.replays[0].header.profileName = "NPC (BOT)";
							base.app.arguments.game.AddGhostPlayer(ghostRecordsV);
						}
						else
						{
							BlackboxRecord blackboxRecord = Reflection<object>.Get<BlackboxRecord>((IList)new List<BlackboxRecord> { om.ghostRecords }, 0);
							blackboxRecord.clips[0].header["profile-name"] = "NPC";
							base.app.arguments.game.AddGhostPlayer(blackboxRecord);
						}
						base.app.arguments.game.players[1].photo = model.botAvatar;
						if (base.app.model.game == null)
						{
							base.app.view.audio.SceneMainToGame(1.5f);
						}
						if (is_custom)
						{
							base.app.scene.LoadCommunityMap(stepModel.trackGuid);
						}
						else
						{
							RunOnce(base.app.scene.Load, 3f);
						}
					}
					break;
				case OpponentModel.Status.Error:
					roomStatusField.SetWarning("LOADING FAILED!");
					roomStatusField.fade.FadeOut(0.2f, 0.5f);
					base.app.view.audio.PlayUIGenericError();
					break;
				case OpponentModel.Status.NoResults:
					roomStatusField.SetWarning("NO OPPONENTS FOUND!");
					break;
				case OpponentModel.Status.Progress:
				{
					roomStatusField.fade.FadeIn(0.3f);
					float loading = om.progress * 100f;
					roomStatusField.SetLoading(loading);
					break;
				}
				case OpponentModel.Status.ManifestSuccess:
					roomStatusField.SetLoading(0f);
					base.app.view.audio.PlayUIGenericSuccess();
					break;
				case OpponentModel.Status.None:
					roomStatusField.fade.FadeOut(0.3f);
					break;
				}
				base.app.view.ui.screens.mouseWheelScrollingEnabled = true;
			});
		}

		private void Load(OnboardingStep stepModel)
		{
			if (!base.validContext)
			{
				return;
			}
			if (stepIndex >= activeOnboarding.steps.Count || stepIndex < 0)
			{
				Debug.LogWarning($"DRLOnboardingController> Can't find step {stepIndex} for onboarding {activeOnboarding.mode}. Aborting..");
				return;
			}
			base.app.view.ui.screens.mouseWheelScrollingEnabled = false;
			DRLAppArguments arguments = base.app.arguments;
			arguments.Clear();
			if (stepModel == null)
			{
				stepModel = activeOnboarding.steps[stepIndex];
			}
			DRLMap dRLMap = null;
			DRLMapTrack dRLMapTrack = null;
			arguments.game.allowCrash = false;
			is_custom = false;
			switch (stepModel.type)
			{
			case OnboardingStep.OnboardingStepType.Mission:
				is_custom = false;
				startLoading = false;
				if (activeOnboarding.steps[stepIndex].mission.map != null)
				{
					dRLMap = activeOnboarding.steps[stepIndex].mission.map;
				}
				else
				{
					dRLMap = base.app.model.storage.GetMapByGUID(stepModel.mapGuid);
					if (dRLMap == null)
					{
						Debug.LogWarning("DRLOnboardingController>Couldn't find a map with GUID: " + stepModel.mapGuid);
						break;
					}
				}
				if (activeOnboarding.steps[stepIndex].mission != null && activeOnboarding.steps[stepIndex].mission.track != null)
				{
					dRLMapTrack = activeOnboarding.steps[stepIndex].mission.track;
				}
				if (dRLMap == null)
				{
					Debug.LogWarning("DRLOnboardingController>No map GUID provided! Aborting..");
					break;
				}
				base.app.arguments.game.players.Clear();
				base.app.arguments.game.AddPlayer(base.app.model.storage.state.player.playerData);
				arguments.game.type = GameFlag.Mission;
				arguments.game.mode = GameFlag.SinglePlayer;
				arguments.game.mission = stepModel.mission;
				arguments.game.quest = stepModel.quest;
				arguments.game.map = dRLMap;
				if (dRLMapTrack != null)
				{
					arguments.game.track = dRLMapTrack;
				}
				switch (activeOnboarding.mode)
				{
				case OnboardingCampaignMode.Beginner:
				{
					DRLAppArguments.Game obj6 = arguments.game;
					FCMode fcMode = (base.app.model.storage.state.player.activeFCMode = FCMode.Beginner);
					obj6.fcMode = fcMode;
					base.app.model.storage.state.player.activeFCMode = FCMode.Beginner;
					base.app.model.storage.state.player.activeFCModeMissions = FCMode.Beginner;
					break;
				}
				case OnboardingCampaignMode.Intermediate:
				{
					DRLAppArguments.Game obj5 = arguments.game;
					FCMode fcMode = (base.app.model.storage.state.player.activeFCMode = FCMode.Intermediate);
					obj5.fcMode = fcMode;
					base.app.model.storage.state.player.activeFCMode = FCMode.Intermediate;
					base.app.model.storage.state.player.activeFCModeMissions = FCMode.Intermediate;
					break;
				}
				default:
				{
					DRLAppArguments.Game obj4 = arguments.game;
					FCMode fcMode = (base.app.model.storage.state.player.activeFCMode = FCMode.Pro);
					obj4.fcMode = fcMode;
					base.app.model.storage.state.player.activeFCMode = FCMode.Pro;
					base.app.model.storage.state.player.activeFCModeMissions = FCMode.Pro;
					break;
				}
				}
				base.app.model.storage.state.player.garage.currentRigData = base.app.model.storage.state.player.garage.officialRigs[0];
				base.app.view.ui.screens.mouseWheelScrollingEnabled = true;
				RunOnce(base.app.scene.Load, 1.6f);
				break;
			case OnboardingStep.OnboardingStepType.Race:
			{
				is_custom = false;
				startLoading = false;
				dRLMap = base.app.model.storage.GetMapByGUID(stepModel.mapGuid);
				if (dRLMap == null)
				{
					Debug.LogWarning("DRLOnboardingController>Couldn't find a map with GUID: " + stepModel.mapGuid);
					break;
				}
				dRLMapTrack = base.app.model.storage.GetMapTrack(dRLMap.guid, stepModel.trackGuid, p_freestyle: false);
				if (dRLMapTrack == null)
				{
					dRLMapTrack = base.app.model.storage.GetMapTracks(dRLMap, GameFlag.Freestyle)[0];
					dRLMap.data = base.app.model.storage.maps.FindByGUID(stepModel.trackGuid);
					is_custom = true;
				}
				if (dRLMapTrack == null)
				{
					Debug.LogWarning("DRLOnboardingController>Couldn't find a track with GUID: " + stepModel.trackGuid);
					break;
				}
				if (dRLMap == null)
				{
					Debug.LogWarning("DRLOnboardingController>No map GUID provided! Aborting..");
					break;
				}
				_ = base.app.model.service.opponent;
				OpponentModeType opponentType = OpponentModeType.Leader;
				arguments.game.players.Clear();
				arguments.game.AddPlayer(base.app.model.storage.state.player.playerData);
				arguments.game.type = GameFlag.Race;
				arguments.game.mode = GameFlag.SinglePlayer;
				arguments.game.map = dRLMap;
				arguments.game.track = dRLMapTrack;
				arguments.game.podium = dRLMapTrack.podium;
				switch (activeOnboarding.mode)
				{
				case OnboardingCampaignMode.Beginner:
				{
					DRLAppArguments.Game obj3 = arguments.game;
					FCMode fcMode = (base.app.model.storage.state.player.activeFCMode = FCMode.Beginner);
					obj3.fcMode = fcMode;
					break;
				}
				case OnboardingCampaignMode.Intermediate:
				{
					DRLAppArguments.Game obj2 = arguments.game;
					FCMode fcMode = (base.app.model.storage.state.player.activeFCMode = FCMode.Intermediate);
					obj2.fcMode = fcMode;
					break;
				}
				default:
				{
					DRLAppArguments.Game obj = arguments.game;
					FCMode fcMode = (base.app.model.storage.state.player.activeFCMode = FCMode.Pro);
					obj.fcMode = fcMode;
					break;
				}
				}
				arguments.game.opponentType = opponentType;
				base.app.model.storage.state.player.garage.currentRigData = base.app.model.storage.state.player.garage.officialRigs[0];
				break;
			}
			}
		}

		public void OpenOnboardingProgress(bool fromStartOnboarding)
		{
			UIOnboardingOverviewController uIOnboardingOverviewController = base.app.view.ui.screens.Open<UIOnboardingOverviewController>("onboarding-overview-screen", 0.3f);
			uIOnboardingOverviewController.view.onboardingData = model.activeOnboarding;
			uIOnboardingOverviewController.view.fromStartOnboarding = fromStartOnboarding;
		}

		public void StopOnboarding()
		{
			model.SetOnboardingInactive();
			if (base.app.inGame)
			{
				base.app.view.ui.screens.manager.ClearHistory();
				base.app.controller.game.RestartWithoutLoad();
			}
			else
			{
				base.app.view.ui.screens.CloseAllScreens();
				base.app.view.ui.screens.manager.ClearHistory();
				base.app.view.ui.screens.Open<UIHomeView>("home-screen-grid", 0.3f);
			}
		}

		private void RefreshProgressUI()
		{
			Notify("onboarding.header.refresh");
		}

		public void OnPersistency()
		{
			base.app.controller.onboarding = this;
		}

		public OnboardingRaceReplayData[] GetBotReplay(OnboardingCampaignMode difficulty)
		{
			OnboardingRaceReplayData[] currentReplayData = null;
			OnboardingStep stepModel = activeOnboarding.steps[model.currentStep];
			if (base.app.model.onboarding.IsMissionStep(stepIndex, activeOnboarding))
			{
				return null;
			}
			base.app.model.service.GetReplayOnboarding(delegate(OnboardingRaceReplayData[] data)
			{
				if (data != null)
				{
					currentReplayData = data;
					int num = 0;
					num = model.GetCurrentRaceStep(activeOnboarding, model.currentStep);
					stepModel.opponentReplayId = currentReplayData[num].Get<string>("replay-url");
				}
			}, difficulty, 0);
			return currentReplayData;
		}
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using drl.backend;
using drl.sim;
using drl.sim.rci;
using thelab.core;

namespace drl.game
{
	public class MissionController : GameTypeController
	{
		private bool m_resetInProgress;

		public bool hideUI;

		private bool m_failed;

		public static int passingScore = 70;

		public static int testAttempts = 3;

		[HideInInspector]
		public float crashVelocity = 150f;

		public MissionModel model => AssertLocal<MissionModel>("model");

		public DRLOnboardingModel onboardingModel => base.app.model.onboarding;

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			base.OnNotification(p_event, p_target, p_data);
			if (p_event.StartsWith("fn.") && model.module.main != null)
			{
				model.module.main.Message(p_event, p_data);
			}
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "scene.start":
				model.mission = base.app.arguments.game.mission;
				model.quest = base.app.arguments.game.quest;
				break;
			case "scene.track.build@complete":
			{
				GameObject gameObject = null;
				DroneSimulation missionRoot = base.app.scene.GetMissionRoot<DroneSimulation>();
				GameObject missionRoot2 = base.app.scene.GetMissionRoot("core");
				if ((bool)missionRoot2)
				{
					gameObject = missionRoot2;
				}
				if ((bool)missionRoot)
				{
					gameObject = missionRoot.gameObject;
				}
				model.root = gameObject;
				if (!gameObject)
				{
					Debug.LogWarning("MissionController> TrackBuildComplete / Mission Root is <null>");
				}
				break;
			}
			case "game.level.load@complete":
			{
				Debug.Log("MissionController> LevelLoadComplete / Fetching PPB");
				DRLQualityGroup component3 = base.game.model.level.root.GetComponent<DRLQualityGroup>();
				List<PostProcessingBehaviour> ppbl = new List<PostProcessingBehaviour>();
				if ((bool)component3)
				{
					if (component3.postProcessing != null)
					{
						ppbl.AddRange(component3.postProcessing);
					}
					if ((bool)model.root)
					{
						Hierarchy.Traverse(model.root.transform, delegate(PostProcessingBehaviour it)
						{
							ppbl.Add(it);
						});
					}
					component3.postProcessing = ppbl.ToArray();
					component3.Apply();
				}
				Debug.Log("MissionController> Found [" + ppbl.Count + "] PPB - Apply Quality");
				break;
			}
			case "game.simulation.load@complete":
			{
				DroneSimulation simulation = base.game.model.simulation;
				base.app.controller.settings.ApplySimulationCameras();
				base.app.controller.game.level.ApplyLevelSettings(simulation.cameras.list);
				break;
			}
			case "game.simulation.drone@add":
			{
				Drone drone = p_data[0] as Drone;
				if (!drone)
				{
					break;
				}
				int playerId = base.game.model.playerId;
				if (playerId < 0)
				{
					break;
				}
				GamePlayerData gamePlayerData = base.game.model.players[playerId];
				if (gamePlayerData != null)
				{
					Debug.Log("MissionController> Assigned Drone [" + drone?.ToString() + "] to Player");
					if (!gamePlayerData.drone)
					{
						gamePlayerData.drone = drone;
					}
				}
				break;
			}
			case "ui.screen@open":
			{
				UIScreen uIScreen = p_data[0] as UIScreen;
				if ((bool)uIScreen && (bool)uIScreen.GetComponent<UINPCOverlayView>())
				{
					Activity.RunOnce(delegate
					{
						base.app.view.ui.footer.Hide(0.5f);
					}, 1f / 30f);
				}
				break;
			}
			case "game.ready":
			{
				ControllerStateType controllerStateType = RCI.GetControllerStateType(ControllerStateType.Taranis);
				if (onboardingModel.activeOnboarding == null)
				{
					UINPCOverlayView v = base.app.view.ui.screens.Open<UINPCOverlayView>("npc-overlay-screen", 0.3f);
					v.description = ((model.mission.shortDescription == "") ? model.mission.description : model.mission.shortDescription);
					v.image = model.mission.descriptionImage;
					v.missionText = model.mission.title.ToUpper();
					if (model.mission.gameObject.CompareTag("Intro"))
					{
						v.questTitle.enabled = false;
						v.missionTitle.enabled = false;
						v.backButton.SetActive(value: false);
					}
					if (model.mission.CompareTag("DiagnosticTest"))
					{
						v.backButton.SetActive(value: false);
						v.exitButton.SetActive(value: false);
					}
					if (model.mission.name == "cuav-test")
					{
						v.backButton.SetActive(value: false);
					}
					string title = model.quest.title;
					title = title.Replace("\n", " ");
					if (!(model.quest.testMission == model.mission) && !model.quest.tags.Contains(GameFlag.DMVQuest))
					{
						v.questText = title + " <color=red> / </color> " + base.app.model.storage.locale.Get("mission-overview.mission-title", "MISSION") + " " + model.mission.order.ToString("00");
					}
					else
					{
						v.questText = title;
					}
					v.SetState(NPCStateType.Controller0, p_is_left: false, controllerStateType);
					Activity.RunOnce(delegate
					{
						UINavigation.focus = v.rightNavigation;
					}, 0.8f);
				}
				else
				{
					_ = onboardingModel.activeOnboarding.steps[base.app.controller.onboarding.stepIndex];
					UIOnboardingStepsView uIOnboardingStepsView = base.app.view.ui.screens.Open<UIOnboardingStepsView>("onboarding-steps-menu-screen", 0.3f);
					uIOnboardingStepsView.nextButton.gameObject.SetActive(value: false);
					uIOnboardingStepsView.Set(onboardingModel.activeOnboarding, onboardingModel.currentStep);
					model.quest.title.Replace("\n", " ");
					base.app.view.ui.footer.Hide(0f, 0.1f);
				}
				base.app.view.ui.footer.Hide(0f, 0.1f);
				base.app.view.ui.game.hud.marker.margins.bottom = 185;
				Activity.RunOnce(base.app.controller.settings.ApplySimulationCameras, 0.1f);
				base.app.view.ui.game.hud.marker.fade.Fade(1f);
				ControllerTypeTag component = model.mission.GetComponent<ControllerTypeTag>();
				if ((bool)component)
				{
					controllerStateType = component.tags[0];
				}
				m_failed = false;
				break;
			}
			case "game.npc-overlay.next@click":
			{
				if (!model.module)
				{
					Debug.LogWarning("MissionController> SimulationFlowMdule is null!");
					base.app.scene.ExitGame();
					break;
				}
				if (model.mission.gameObject.CompareTag("Intro"))
				{
					base.app.view.ui.screens.Open<UIPhysicsIntroView01>("physics-intro-screen-01");
					base.app.view.ui.SetDark(p_flag: false);
					Activity.RunOnce(delegate
					{
						base.app.view.ui.footer.Hide(0f);
					}, 1f / 30f);
				}
				else if (onboardingModel.activeOnboarding == null)
				{
					base.app.view.ui.screens.Close("npc-overlay-screen");
				}
				else
				{
					base.app.view.ui.screens.Close("onboarding-steps-menu-screen");
					Activity.RunOnce(delegate
					{
						base.app.view.ui.footer.Hide(0f);
					}, 1f / 30f);
				}
				base.ui.hud.training.Clear();
				if (!hideUI)
				{
					base.ui.hud.training.fade.alpha = 1f;
					base.ui.hud.training.content.alpha = 1f;
					if (model.mission.name == "cuav-test")
					{
						base.ui.hud.training.counterUAVHUD.fade.FadeIn();
					}
				}
				ControllerStateType controller = RCI.GetControllerStateType(ControllerStateType.Taranis);
				ControllerTypeTag component2 = model.mission.GetComponent<ControllerTypeTag>();
				if ((bool)component2)
				{
					controller = component2.tags[0];
				}
				base.ui.hud.controller.SetController(controller);
				base.ui.hud.training.npc.controller = controller;
				base.ui.hud.Show();
				Flow main = model.module.main;
				if ((bool)main)
				{
					model.completed = false;
					Activity.RunOnce(main.Run, 0.3f);
					ServiceModel service = base.app.model.service;
					if ((bool)service && (model.quest.testMission == model.mission || model.quest.tags.Contains(GameFlag.DMVQuest)))
					{
						service.StartTimer(model.mission.guid, delegate
						{
						});
					}
				}
				else
				{
					Debug.LogWarning("GameController> Flow not found [" + model.module?.ToString() + "]");
				}
				break;
			}
			case "game.npc-overlay.back@click":
				base.app.view.ui.game.hud.Hide();
				if (model.quest.testMission == model.mission || model.quest.tags.Contains(GameFlag.DMVQuest))
				{
					base.app.view.ui.screens.Open("dmv-tests-screen");
				}
				else if (base.app.controller.onboarding.model.inProgress)
				{
					Debug.Log($"222 MissionController.OnNotification(): app.controller.onboarding.model.GetProgress(): {base.app.controller.onboarding.model.GetProgress()}");
					base.app.controller.onboarding.OpenOnboardingProgress(fromStartOnboarding: false);
					DRLQuest quest = model.quest;
					base.app.arguments.game.quest = quest;
				}
				else
				{
					base.app.view.ui.screens.Open("train-menu-screen");
				}
				break;
			case "game.npc-overlay.exit@click":
				base.app.view.audio.PlayUIGenericSuccess();
				base.game.Exit();
				break;
			case "fn.mission@complete":
			{
				if (base.app.inOnboarding)
				{
					base.app.view.ui.game.hud.Fade(0f);
					base.app.view.ui.screens.manager.ClearHistory();
					base.app.view.ui.game.hud.Hide();
					base.ui.hud.training.fade.alpha = 0f;
					base.ui.hud.training.content.alpha = 0f;
					Activity.RunOnce(delegate
					{
						base.app.view.ui.footer.Hide(0f);
					}, 1f / 30f);
					base.app.view.ui.game.hud.marker.fade.Fade(0f);
					SimulationFlowModule simulationFlowModule = Hierarchy.Find<SimulationFlowModule>(model.root.transform);
					if ((bool)simulationFlowModule)
					{
						simulationFlowModule.StopTimer(0);
						simulationFlowModule.StopTimer(1);
					}
					base.app.view.audio.StopAllGameAudio();
					base.app.view.audio.PlayMusicPostGame();
					this.TimerRunOnce(delegate
					{
						Notify("onboarding.mission.complete@increase");
						int currentStep = onboardingModel.currentStep;
						if (onboardingModel.activeOnboarding.steps.Count < currentStep + 1 && onboardingModel.activeOnboarding.steps[currentStep + 1].type == OnboardingStep.OnboardingStepType.Race && (onboardingModel.activeOnboarding.mode == OnboardingCampaignMode.Beginner || onboardingModel.activeOnboarding.mode == OnboardingCampaignMode.Intermediate))
						{
							base.app.view.ui.screens.Close("onboarding-steps-menu-screen");
							UIOnboardingCompleteView uIOnboardingCompleteView = base.app.view.ui.screens.Open<UIOnboardingCompleteView>("onboarding-complete-screen", 0.3f);
							uIOnboardingCompleteView.isMissionCompleted = true;
							uIOnboardingCompleteView.isLastRace = false;
							uIOnboardingCompleteView.SetNextButtonRaceText();
						}
						else
						{
							UIOnboardingStepsView uIOnboardingStepsView2 = base.app.view.ui.screens.Open<UIOnboardingStepsView>("onboarding-steps-menu-screen", 0.3f);
							uIOnboardingStepsView2.nextButton.gameObject.SetActive(value: true);
							uIOnboardingStepsView2.startButton.gameObject.SetActive(value: false);
							uIOnboardingStepsView2.playAgainButton.gameObject.SetActive(value: true);
							uIOnboardingStepsView2.Set(onboardingModel.activeOnboarding, currentStep);
							uIOnboardingStepsView2.SetMarkers(missionComplete: true);
							uIOnboardingStepsView2.titleCompleteText.gameObject.SetActive(value: true);
						}
					}, 1f);
					break;
				}
				if (model.mission.name == "cuav-test")
				{
					model.completed = true;
					base.app.view.ui.game.hud.Hide();
					base.app.view.ui.screens.manager.ClearHistory();
					model.score = (float)p_data[0];
					UIMissionCompleteView uIMissionCompleteView = base.app.view.ui.screens.Open<UIMissionCompleteView>("lesson-complete-screen");
					uIMissionCompleteView.screen.title = model.mission.title;
					uIMissionCompleteView.Set(model.quest, model.mission);
					uIMissionCompleteView.SetScore(model.score, 0.01f);
					uIMissionCompleteView.questsNavBtn.gameObject.SetActive(value: false);
					break;
				}
				bool flag = model.quest.tags.Contains(GameFlag.DMVQuest);
				bool flag2 = model.quest.testMission == model.mission;
				model.completed = !m_failed && !(flag || flag2);
				Activity.RunOnce(delegate
				{
					if (!m_failed || (!(model.quest.testMission == model.mission) && !model.quest.tags.Contains(GameFlag.DMVQuest)))
					{
						model.completed = true;
						ServiceModel sm = base.app.model.service;
						if ((bool)sm && model.quest.tags.Contains(GameFlag.DMVQuest))
						{
							sm.StopTimer(model.mission.guid, delegate(DRLTimerData data)
							{
								base.app.model.storage.state.player.dmvUserTotalTime += data.elapsed;
							});
						}
						base.app.view.ui.screens.manager.ClearHistory();
						base.app.view.ui.game.hud.Hide();
						DroneSimulation simulation2 = base.game.model.simulation;
						SimulationFlowModule simulationFlowModule2 = Hierarchy.Find<SimulationFlowModule>(model.root.transform);
						if ((bool)simulation2)
						{
							simulation2.drones.SetArmed(p_flag: false);
							simulation2.drones.SetRigidbodyConstraint(RigidbodyConstraints.FreezeAll);
						}
						if ((bool)simulationFlowModule2)
						{
							simulationFlowModule2.StopTimer(0);
							simulationFlowModule2.StopTimer(1);
						}
						model.EvaluateScore();
						float num2 = model.module.data.Get<float>("miss-gates-negative-score");
						if (num2 < 0f)
						{
							num2 = Mathf.Clamp01(0f - num2);
							model.score -= num2 * 0.5f;
						}
						model.score = Mathf.Clamp01(model.score);
						Debug.Log("MissionController> Mission [" + model.mission.name + "] score[" + model.score + "] negative score [" + num2 + "]");
						UIMissionCompleteView scr = null;
						if (model.mission.tag == "DiagnosticTest")
						{
							UIDMVDiagnosticCompleteScreenView uIDMVDiagnosticCompleteScreenView = base.app.view.ui.screens.Open<UIDMVDiagnosticCompleteScreenView>("dmv-diagnostic-complete-screen");
							int rankFromScore = GetRankFromScore(model.score);
							uIDMVDiagnosticCompleteScreenView.SetRank(rankFromScore);
							base.app.model.storage.state.player.userRank = rankFromScore;
							SetScoresForRank(rankFromScore);
							Debug.Log("DIAGNOSTIC TEST COMPLETE > RANK [" + rankFromScore + "] SCORE [" + model.score + "]");
						}
						else
						{
							if (model.quest.testMission == model.mission)
							{
								if (model.score >= (float)passingScore / 100f)
								{
									scr = base.app.view.ui.screens.Open<UIMissionCompleteView>("test-complete-screen");
									scr.SetTestCompleteFeedback(model.quest.order, model.score);
									if (model.mission.tag == "FinalTest")
									{
										scr.questsNavBtn.gameObject.SetActive(value: false);
										scr.nextNavBtn.gameObject.SetActive(value: true);
										scr.exitNavBtn.gameObject.SetActive(value: false);
									}
								}
								else
								{
									scr = base.app.view.ui.screens.Open<UIMissionCompleteView>("test-fail-screen");
								}
								if ((bool)sm)
								{
									sm.GetLeaderboardQuest(model.mission, delegate(DRLLeaderboardData[] p_result)
									{
										int crashCount = p_result[0].crashCount;
										scr.SetAttempts(crashCount, testAttempts, p_result[0].score);
									});
								}
							}
							else if (model.quest.tags.Contains(GameFlag.DMVQuest))
							{
								scr = base.app.view.ui.screens.Open<UIMissionCompleteView>((model.score >= (float)passingScore / 100f) ? "lesson-complete-screen" : "lesson-fail-screen");
							}
							else
							{
								scr = base.app.view.ui.screens.Open<UIMissionCompleteView>("mission-complete-screen");
							}
							scr.screen.title = model.mission.title;
							scr.Set(model.quest, model.mission);
							scr.SetScore(model.score, 0.01f);
							Activity.RunOnce(delegate
							{
								UINavigation.focus = scr.nextNavBtn;
							}, 0.5f);
						}
						if ((bool)sm && model.quest.testMission != null && model.quest.testMission == model.mission && model.score < (float)passingScore / 100f)
						{
							sm.GetLeaderboardQuest(model.mission, delegate(DRLLeaderboardData[] p_result)
							{
								if ((float)p_result[0].score < (float)passingScore * 10f)
								{
									int crashCount = p_result[0].crashCount;
									crashCount++;
									if (crashCount < testAttempts && scr != null && scr.restartNavBtn != null)
									{
										scr.restartNavBtn.gameObject.SetActive(value: true);
										if (scr.menuNavBtn != null)
										{
											scr.menuNavBtn.down = scr.restartNavBtn;
											scr.restartNavBtn.up = scr.menuNavBtn;
										}
									}
									if (crashCount > 0 && crashCount % testAttempts == 0)
									{
										sm.ResetLeaderboardQuest(model.quest.missions, delegate
										{
										});
									}
									if (scr != null)
									{
										scr.SetAttempts(crashCount, testAttempts, p_result[0].score);
									}
									ProcessLeaderboards(is_fail: true, crashCount);
								}
								else
								{
									ProcessLeaderboards();
								}
							});
						}
						else
						{
							ProcessLeaderboards();
						}
						base.app.view.audio.StopAllGameAudio();
						base.app.view.audio.PlayMusicPostGame();
						Notify("missions.mission-complete");
					}
				}, 0.5f);
				break;
			}
			case "fn.mission@fail":
				if (!(model.mission.name == "cuav-test"))
				{
					m_failed = true;
					MissionFail();
				}
				break;
			case "missions.certificate-acquired@click":
				base.app.view.ui.screens.Open("dmv-certificate-screen");
				break;
			case "ui.screen.return@click":
			case "ui.screen.nav-right@click":
			case "settings.controller.profile.calibration.form.event@click":
				if (!model.mission.CompareTag("Intro") || (model.module.simulation.isPaused && base.app.view.ui.screens.manager.InHistory("game-pause-screen")) || (p_event == "settings.controller.profile.calibration.form.event@click" && base.app.view.ui.screens.current.name == "settings-controller-calibration-screen" && !(p_target.name == "nav") && !(p_target.name == "nav-delete") && !(p_target.name == "nav-calib-exit")))
				{
					break;
				}
				if (base.app.view.ui.screens.current.name == "settings-system-screen" && base.app.view.ui.screens.manager.InHistory("physics-intro-screen-03") && !base.app.view.ui.screens.manager.InHistory("physics-intro-screen-04"))
				{
					Notify("game.unpause");
					model.module.ui.ShowFooter();
					base.game.model.simulation.cameras.Get(0).follow.enabled = true;
					base.game.model.simulation.cameras.Get(0).orbit.enabled = true;
				}
				else if (!(base.app.view.ui.screens.current.name != "settings-controller-calibration-screen"))
				{
					if (base.app.view.ui.screens.manager.InHistory("physics-intro-screen-01"))
					{
						model.module.main.Message("fn.intro.step01.next@click");
						base.app.view.ui.screens.CloseAllScreens();
						model.module.ui.HideFooter();
					}
					else if (base.app.view.ui.screens.manager.InHistory("physics-intro-screen-02"))
					{
						Notify("game.unpause");
						model.module.ui.ShowFooter();
						base.game.model.simulation.cameras.Get(0).follow.enabled = true;
						base.game.model.simulation.cameras.Get(0).orbit.enabled = true;
					}
				}
				break;
			case "fn.mission.physicsintrostep1@start":
				base.app.view.ui.screens.Open<UIPhysicsIntroView01>("physics-intro-screen-01");
				base.app.view.ui.SetDark(p_flag: false);
				Activity.RunOnce(delegate
				{
					base.app.view.ui.footer.Hide(0f);
				}, 1f / 30f);
				break;
			case "fn.mission.physicsintrostep2@start":
				base.app.view.ui.screens.Open<UIPhysicsIntroView02>("physics-intro-screen-02");
				base.app.view.ui.SetDark(p_flag: false);
				Activity.RunOnce(delegate
				{
					base.app.view.ui.footer.Hide(0f);
				}, 1f / 30f);
				break;
			case "fn.mission.physicsintrostep3@start":
				base.app.view.ui.screens.Open<UIPhysicsIntroView03>("physics-intro-screen-03");
				base.app.view.ui.SetDark(p_flag: false);
				Activity.RunOnce(delegate
				{
					base.app.view.ui.footer.Hide(0f);
				}, 1f / 30f);
				break;
			case "fn.mission.physicsintrostep4@start":
				base.app.view.ui.screens.Open<UIPhysicsIntroView04>("physics-intro-screen-04");
				break;
			case "game.change-mission@click":
				if (base.app.arguments.game.quest.tags.Contains(GameFlag.DMVQuest))
				{
					base.app.view.ui.screens.Open<UIDMVTestsView>("dmv-tests-screen");
					ServiceModel service2 = base.app.model.service;
					if ((bool)service2)
					{
						service2.StopTimer(base.app.arguments.game.mission.guid, delegate
						{
						});
					}
				}
				else
				{
					base.app.view.ui.screens.Open<UIQuestsView>("train-menu-screen");
				}
				break;
			case "game.pause":
				model.module.simulation.pause = DroneSimulationPauseMode.Pause;
				model.module.PauseSplineActor();
				break;
			case "game.unpause":
				model.module.simulation.pause = DroneSimulationPauseMode.Unpause;
				model.module.UnpauseSplineActor();
				if (p_data.Length != 0)
				{
					string text = (string)p_data[0];
					if (!string.IsNullOrEmpty(text) && model.mission.CompareTag("Intro") && text == "pause-menu")
					{
						base.app.view.ui.screens.Return();
						base.app.view.ui.SetDark(p_flag: false);
					}
				}
				break;
			case "game.simulation.drone@collision":
				if (!(model.module.simulation.drones.Get(0).fc.sensor.inertial.actualVelocity.sqrMagnitude < 30f))
				{
					if (model.quest.testMission == model.mission || model.mission.tag == "DiagnosticTest")
					{
						Notify("fn.mission.drone@collision");
					}
					else if (model.quest.tags.Contains(GameFlag.DMVQuest) && model.module.simulation.drones.Get(0).fc.sensor.inertial.actualVelocity.sqrMagnitude > crashVelocity)
					{
						float num = model.module.data.Get<float>("miss-gates-negative-score");
						num -= 0.2f;
						model.module.data.SetFloat("miss-gates-negative-score", num);
					}
				}
				break;
			case "ui.screen.video-player@end":
				Notify("fn.mission.video-player@end", p_data);
				break;
			case "missions.mission-complete.soft-reset@click":
				if (model.mission.name == "cuav-test")
				{
					base.app.view.audio.PlayUIGenericSuccess();
					base.game.Restart();
					break;
				}
				base.app.view.ui.screens.Close("lesson-fail-screen");
				base.app.view.ui.screens.Close("lesson-complete-screen");
				Hierarchy.Find<SimulationFlowModule>(model.root.transform).resetAvailable = true;
				model.completed = false;
				base.app.view.audio.StopAllGameAudio();
				SoftResetMission();
				base.app.view.audio.PlayMusicGame();
				break;
			}
		}

		protected void ProcessLeaderboards(bool is_fail = false, int p_crashCount = 0)
		{
			bool flag = false;
			ServiceModel service = base.app.model.service;
			flag = is_fail;
			if (is_fail)
			{
				model.score = 0f;
			}
			service.SetLeaderboardMission(model.mission, model.score, flag, delegate(DRLLeaderboardData p_result)
			{
				if (this == null || p_result == null)
				{
					Debug.LogWarning("MissionController> SetLeaderboard - Failed to send results!");
				}
				else
				{
					Debug.Log("MissionController> Leaderboard success!\n" + p_result.profileName + "\n" + p_result.position + "\nhighscore: " + p_result.highscore);
					if (p_result.highscore)
					{
						base.app.view.audio.PlayUINewRecord();
					}
				}
			}, p_crashCount);
		}

		protected override DroneSimulation GetSimulation()
		{
			if (!model.mission)
			{
				Debug.LogWarning("MissionController> Tried to run null mission!");
				base.app.scene.LoadMain();
				return null;
			}
			if (!model.root)
			{
				Debug.LogWarning("MissionController> Mission root not found!");
				base.app.scene.LoadMain();
				return null;
			}
			DroneSimulation component = model.root.GetComponent<DroneSimulation>();
			if (!component)
			{
				Debug.LogWarning("MissionController> Simulation not found!");
				base.app.scene.LoadMain();
				return null;
			}
			DRLGameFlowUI component2 = base.app.view.ui.GetComponent<DRLGameFlowUI>();
			SimulationFlowModule simulationFlowModule = Hierarchy.Find<SimulationFlowModule>(model.root.transform);
			simulationFlowModule.factory = base.app.model.storage.factory;
			simulationFlowModule.ui = component2;
			model.module = simulationFlowModule;
			model.module.CameraStart();
			model.module.SetObjectives(model.mission.objectives);
			return component;
		}

		protected override void LoadPodiums(string p_guid)
		{
		}

		protected override void LoadCameras()
		{
			base.game.model.camera = base.game.model.simulation.cameras.Get(0);
			base.LoadCameras();
		}

		protected override void LoadDrones()
		{
		}

		protected override void LoadPlayerTuning(FCProfileData p_data = null)
		{
			Drone playerDrone = base.game.model.playerDrone;
			if (!playerDrone)
			{
				Debug.LogWarning("GameTypeController> Player Drone not found!");
				return;
			}
			ControllerStateType controllerStateType = RCI.GetControllerStateType(ControllerStateType.XBox);
			playerDrone.fc.profile.SetPreset(FCProfileData.Betaflight.TrainingPresets[controllerStateType]);
		}

		public override bool OnGameCommand(GameCommand p_command)
		{
			if (p_command == null)
			{
				return true;
			}
			switch (p_command.type)
			{
			case GameCommandType.Pause:
				if (model.completed)
				{
					return false;
				}
				break;
			case GameCommandType.ResetDrone:
				if (model.completed)
				{
					return false;
				}
				if (base.app.model.game.playerDrone.fc.mode == FlightControllerMode.Beginner || base.app.model.game.playerDrone.fc.mode == FlightControllerMode.DJI)
				{
					return false;
				}
				break;
			case GameCommandType.ResetGame:
				if (model.completed)
				{
					return false;
				}
				break;
			case GameCommandType.ResetDronePodium:
				SoftResetMission();
				break;
			}
			return true;
		}

		protected void MissionFail()
		{
			if (!model.quest.tags.Contains(GameFlag.DMVQuest))
			{
				return;
			}
			model.completed = true;
			base.app.view.ui.screens.manager.ClearHistory();
			base.app.view.ui.game.hud.Hide();
			DroneSimulation simulation = base.game.model.simulation;
			SimulationFlowModule simulationFlowModule = Hierarchy.Find<SimulationFlowModule>(model.root.transform);
			if ((bool)simulation)
			{
				simulation.drones.SetArmed(p_flag: false);
				simulation.drones.SetRigidbodyConstraint(RigidbodyConstraints.FreezeAll);
			}
			if ((bool)simulationFlowModule)
			{
				simulationFlowModule.StopTimer(0);
				simulationFlowModule.StopTimer(1);
			}
			Debug.Log("MissionController> FAILED Mission [" + model.mission.name + "] score[" + model.score + "]");
			if (base.app.inOnboarding)
			{
				int currentStep = onboardingModel.currentStep;
				UIOnboardingStepsView uIOnboardingStepsView = base.app.view.ui.screens.Open<UIOnboardingStepsView>("onboarding-steps-menu-screen", 0.3f);
				uIOnboardingStepsView.nextButton.gameObject.SetActive(value: true);
				uIOnboardingStepsView.startButton.gameObject.SetActive(value: false);
				uIOnboardingStepsView.playAgainButton.gameObject.SetActive(value: true);
				uIOnboardingStepsView.Set(onboardingModel.activeOnboarding, currentStep);
				uIOnboardingStepsView.SetFailUI(onboardingModel);
				uIOnboardingStepsView.avatarsGroup.SetActive(value: false);
			}
			else if (model.mission.tag == "DiagnosticTest")
			{
				base.app.view.ui.screens.Open<UIDMVDiagnosticCompleteScreenView>("dmv-diagnostic-complete-screen").rank = 0;
				base.app.model.storage.state.player.userRank = 0;
			}
			else
			{
				UIMissionCompleteView scr = null;
				if (model.quest.testMission == model.mission)
				{
					scr = base.app.view.ui.screens.Open<UIMissionCompleteView>("test-fail-screen");
					scr.restartNavBtn.gameObject.SetActive(value: false);
				}
				else if (model.quest.tags.Contains(GameFlag.DMVQuest))
				{
					scr = base.app.view.ui.screens.Open<UIMissionCompleteView>("lesson-fail-screen");
				}
				else
				{
					scr = base.app.view.ui.screens.Open<UIMissionCompleteView>("mission-complete-screen");
				}
				scr.screen.title = model.mission.title;
				scr.Set(model.quest, model.mission);
				ServiceModel sm = base.app.model.service;
				if ((bool)sm)
				{
					sm.GetLeaderboardQuest(model.mission, delegate(DRLLeaderboardData[] p_result)
					{
						if (model.quest.testMission == model.mission)
						{
							scr.SetScore(0f, 0.01f);
							int crashCount = p_result[0].crashCount;
							crashCount++;
							if (crashCount < testAttempts)
							{
								scr.restartNavBtn.gameObject.SetActive(value: true);
								if (scr.menuNavBtn != null)
								{
									scr.menuNavBtn.down = scr.restartNavBtn;
									scr.restartNavBtn.up = scr.menuNavBtn;
								}
							}
							if (crashCount > 0 && crashCount % testAttempts == 0)
							{
								sm.ResetLeaderboardQuest(model.quest.missions, delegate
								{
								});
							}
							scr.SetAttempts(crashCount, testAttempts, p_result[0].score);
							if ((float)p_result[0].score < (float)passingScore * 10f)
							{
								ProcessLeaderboards(is_fail: true, crashCount);
							}
						}
					});
					sm.StopTimer(model.mission.guid, delegate(DRLTimerData data)
					{
						base.app.model.storage.state.player.dmvUserTotalTime += data.elapsed;
					});
				}
				Activity.RunOnce(delegate
				{
					UINavigation.focus = scr.nextNavBtn;
				}, 0.5f);
			}
			base.app.view.audio.StopAllGameAudio();
			base.app.view.audio.PlayMusicPostGame();
		}

		protected override void OnGameReady()
		{
			base.OnGameReady();
			if (!(model.mission.guid == "MS-bcd") && !(model.mission.guid == "MS-620"))
			{
				UnfreezeDrones();
				base.app.view.ui.game.hud.marker.fade.Fade(1f);
				if (base.game.model.simulation != null)
				{
					base.game.model.simulation.drones.SetReceiver(p_flag: true);
				}
			}
		}

		protected void SoftResetMission(bool p_resetToFirst = false)
		{
			if ((model.quest.testMission != null && (model.quest.testMission == model.mission || model.mission.tag == "DiagnosticTest")) || model.completed)
			{
				return;
			}
			SimulationFlowModule simulation_flow = Hierarchy.Find<SimulationFlowModule>(model.root.transform);
			if (!simulation_flow.resetAvailable || model.module.simulation.pause == DroneSimulationPauseMode.Pause)
			{
				return;
			}
			Drone drone = simulation_flow.simulation.drones.Get(0);
			int activeStepIdx = (p_resetToFirst ? 1 : simulation_flow.ActiveStepIdx());
			if (activeStepIdx <= 0 || m_resetInProgress)
			{
				return;
			}
			m_resetInProgress = true;
			drone.rigidbody.rb.constraints = RigidbodyConstraints.FreezeAll;
			drone.fc.allowPitch = false;
			drone.fc.allowRoll = false;
			drone.fc.allowYaw = false;
			drone.fc.allowThrottle = false;
			model.module.ui.FadeIn(0f, 0.5f);
			Flow steps = simulation_flow.GetSteps();
			steps.ProgressUpdate = (Action)Delegate.Remove(steps.ProgressUpdate, new Action(simulation_flow.ProgressUpdate));
			RunOnce(delegate
			{
				UnityEngine.Object.Destroy(simulation_flow.missionModule.gameObject);
				simulation_flow.simulation.PlaceDrone(drone, simulation_flow.droneStart.transform);
				simulation_flow.missionModule = simulation_flow.missionModuleDuplicate;
				simulation_flow.missionModule.SetActive(value: true);
				simulation_flow.DuplicateMission();
				simulation_flow.StopTimer(0);
				simulation_flow.StopTimer(1);
				simulation_flow.ui.ClearMissionUI();
				if (simulation_flow.data != null)
				{
					simulation_flow.data.Set("mission-time", simulation_flow.data.Contains("mission-time") ? simulation_flow.ui.GetStepTimes(0) : 0f);
					simulation_flow.data.Set("miss-gates-negative-score", 0f);
				}
				simulation_flow.ui.SoftResetTimers();
				DroneCameraModeType activeCameraMode = simulation_flow.activeCameraMode;
				if (activeCameraMode == DroneCameraModeType.FPV || activeCameraMode == DroneCameraModeType.FPVSmooth)
				{
					simulation_flow.simulation.cameras.SetFPVSmooth(0, 0, 0.01f);
				}
				else
				{
					simulation_flow.simulation.cameras.Get(0).SetTPVMissions(drone);
				}
				RunOnce(delegate
				{
					Flow steps2 = simulation_flow.GetSteps();
					steps2.ProgressUpdate = (Action)Delegate.Combine(steps2.ProgressUpdate, new Action(simulation_flow.ProgressUpdate));
					steps2.pointer = activeStepIdx;
					steps2.Run();
					base.app.view.audio.StopGameRadar();
					if (simulation_flow.simulation.UIRequirements.progressBarRequired)
					{
						simulation_flow.ui.SetProgressBar(activeStepIdx - 1, simulation_flow.simulation.UIRequirements.progressBarTotal);
					}
				}, 0.02f);
				model.module.ui.FadeOut(0.2f, 0.5f);
				model.module.ui.ShowContent();
				RunOnce(delegate
				{
					m_resetInProgress = false;
					m_failed = false;
					ServiceModel service = base.app.model.service;
					if ((bool)service && model.quest.tags.Contains(GameFlag.DMVQuest))
					{
						service.StartTimer(model.mission.guid, delegate
						{
						});
					}
				}, 0.5f);
			}, 0.5f);
		}

		public int GetRankFromScore(float p_score)
		{
			float num = 0f;
			if (model.module.data.Get("diagnostic.test.hover", 0) == 0)
			{
				num += 0.05f;
			}
			if (model.module.data.Get("diagnostic.test.up-and-down", 0) == 0)
			{
				num += 0.05f;
			}
			if (model.module.data.Get("diagonstic.test.slalom", 0) == 0)
			{
				num += 0.05f;
			}
			if (model.module.data.Get("diagnostic.test.large-turn", 0) == 0)
			{
				num += 0.05f;
			}
			if (model.module.data.Get("diagnostic.test.path", 0) == 0)
			{
				num += 0.15f;
			}
			if (model.module.data.Get("diagnostic.test.stairs", 0) == 0)
			{
				num += 0.15f;
			}
			if (model.module.data.Get("diagnostic.test.power-loop", 0) == 0)
			{
				num += 0.2f;
			}
			if (model.module.data.Get("diagnostic.test.small-turn", 0) == 0)
			{
				num += 0.15f;
			}
			if (model.module.data.Get("diagnostic.test.split-s", 0) == 0)
			{
				num += 0.15f;
			}
			Debug.Log("DIAGNOSTIC TEST [OBSTACLES SCORE] >>> " + num);
			num -= (1f - p_score) * 0.33f;
			num = Mathf.Clamp01(num);
			if (num < 0.2f)
			{
				return 0;
			}
			if (num < 0.3f)
			{
				return 1;
			}
			if (num < 0.4f)
			{
				return 2;
			}
			if (num < 0.6f)
			{
				return 3;
			}
			if (num < 0.8f)
			{
				return 4;
			}
			if (num < 0.9f)
			{
				return 5;
			}
			return 6;
		}

		private void SetScoresForRank(int rank)
		{
			ServiceModel service = base.app.model.service;
			List<DRLQuest> quests = base.app.model.storage.GetQuests(GameFlag.DMVQuest);
			for (int i = 0; i < rank; i++)
			{
				if (quests[i].testMission == null)
				{
					continue;
				}
				service.SetLeaderboardMission(quests[i].testMission, 1000f, p_force: true, delegate(DRLLeaderboardData p_result)
				{
					if (this == null || p_result == null)
					{
						Debug.LogWarning("MissionController> SetLeaderboard - Failed to send results!");
					}
					else
					{
						Debug.Log("MissionController> Leaderboard success!\n" + p_result.profileName + "\n" + p_result.position + "\nhighscore: " + p_result.highscore);
					}
				}, 0);
			}
		}
	}
}

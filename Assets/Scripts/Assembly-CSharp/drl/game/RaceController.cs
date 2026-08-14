using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Scripting;
using drl.backend;
using drl.sim;
using drl.sim.rci;
using thelab.core;

namespace drl.game
{
	public class RaceController : GameTypeController
	{
		private Activity m_race_complete_timer;

		private bool m_customPhysics;

		private int m_droneClass;

		private bool m_official;

		private bool m_resetAfterPause;

		private bool m_raceFinished;

		public bool replayUploadCompleted;

		private bool m_replayUploadStarted;

		private bool m_requestedRestart;

		private bool m_usingDRLPilotMode;

		private List<float> tempGatesTime = new List<float>();

		protected int tournamentPostRetry;

		private Activity tournamentMatchTimer;

		public new static bool ignoreCount = false;

		[HideInInspector]
		public bool restartLocked;

		private UIDialogView dialog;

		public List<WebAsyncRequest> ghostRequests;

		public WebAsyncRequest ghostDataRequest;

		public List<Thread> ghostParsers;

		public int ghostParsersComplete;

		public List<byte[]> ghostReplays;

		public MonoActivity ghostProcessingLoop;

		public List<BlackboxRecord> ghostRecords;

		public Mutex ghostParsersMtx;

		public float ghostProcessTimeout;

		private int m_throttleCapRetry = 10;

		private float ghost_speed = 1f;

		private static Dictionary<Component, string> m_gate_collider_lut = new Dictionary<Component, string>();

		private static Dictionary<Component, Drone> m_transform_drone_lut;

		private static Dictionary<Component, MAGate> m_transform_gate_lut;

		private List<object> act_data_list = new List<object>();

		private bool m_has_sent_tournament_results;

		public RaceModel model => AssertLocal<RaceModel>("model");

		public bool customPhysics => m_customPhysics;

		public bool tournamentMatchComplete
		{
			get
			{
				if (base.app.arguments.game.tournamentData != null)
				{
					return !base.app.view.ui.game.hud.timeout.countdownContainer.activeInHierarchy;
				}
				return false;
			}
		}

		protected override void Start()
		{
			base.Start();
		}

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (p_event != null && p_event == "game.ui.dashboard@show" && resetInProgress)
			{
				return;
			}
			base.OnNotification(p_event, p_target, p_data);
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "game.ui.dashboard@show":
				m_customPhysics = false;
				base.ui.hud.race.fade.alpha = -0.1f;
				base.ui.hud.marker.fade.alpha = -0.1f;
				base.ui.hud.controller.fade.alpha = -0.1f;
				base.ui.hud.standingsFade.alpha = -0.1f;
				base.ui.hud.physics.view.raceHudVisible = false;
				break;
			case "game.ui.dashboard@hide":
				if (base.ui.hud.dashboard.openingAnotherScreen)
				{
					break;
				}
				base.ui.hud.physics.view.raceHudVisible = true;
				RequestRaceReset();
				base.ui.hud.physics.Hide();
				if (base.app.inMultiplayer && !base.app.model.network.room.IsSpectator)
				{
					base.ui.hud.race.fade.alpha = 1f;
					base.ui.hud.marker.fade.alpha = 1f;
					base.ui.hud.controller.fade.alpha = (base.app.model.storage.state.player.settings.game.controllerOverlay ? 1f : (-0.1f));
					if (base.app.model.network.room.RacersCount > 1)
					{
						base.ui.hud.standingsFade.alpha = (base.app.model.storage.state.player.settings.game.raceAutoStandings ? 1f : (-0.1f));
					}
				}
				break;
			case "game.simulation.drone.all@ready":
				m_droneClass = ((base.app.model.game.playerDrone != null) ? base.app.model.game.playerDrone.rig.diameter : 0);
				m_official = base.app.model.game.playerDrone != null && base.app.model.storage.state.player.garage.IsOfficial(base.app.model.game.playerDrone.rig);
				m_customPhysics = base.app.model.game.playerDrone != null && base.app.model.game.playerDrone.rig.hasCustomPhysics;
				break;
			case "game.pause":
				base.game.model.simulation.transmitters.SetEnabled<DroneGhostTransmitter>(p_flag: false);
				break;
			case "game.unpause":
			{
				if (base.ui.hud.dashboard.openedFromPause)
				{
					base.ui.hud.dashboard.openedFromPause = false;
					base.ui.hud.physics.view.raceHudVisible = true;
					RequestRaceReset();
				}
				else if (m_resetAfterPause)
				{
					RequestRaceReset();
				}
				m_resetAfterPause = false;
				base.game.model.simulation.transmitters.SetEnabled<DroneGhostTransmitter>(p_flag: true);
				bool hotkeys = base.app.model.storage.state.player.settings.game.hotkeys;
				base.app.view.ui.game.hud.SetHotkeysEnabled(hotkeys);
				break;
			}
			case "game.ready":
			{
				InitializePath();
				InitializeGates();
				tempGatesTime.Clear();
				bool num = base.app.arguments.game.tournamentData != null;
				if (num)
				{
					base.app.model.service.WatchTournamentRefresh();
				}
				if (!num)
				{
					SetThrottleCap();
				}
				m_raceFinished = false;
				replayUploadCompleted = false;
				m_replayUploadStarted = false;
				m_has_sent_tournament_results = false;
				break;
			}
			case "game.count@step":
			{
				int num5 = Reflection<object>.Get<int>(p_data, 0);
				int p_max = Reflection<object>.Get<int>(p_data, 1);
				Reflection<object>.Get((IList)p_data, 2, true);
				bool p_hide_title = Reflection<object>.Get((IList)p_data, 3, num5 == 2);
				if (!ignoreCount)
				{
					ApplyCount(num5, p_max, p_play_audio: true, p_hide_title);
				}
				break;
			}
			case "game.count@complete":
				if (!ignoreCount)
				{
					OnCountComplete();
				}
				break;
			case "game.race.gate@step":
			{
				Drone drone = Reflection<object>.Get<Drone>(p_data, 3);
				if (base.app.model.game.playerDrone == drone)
				{
					base.app.view.audio.PlayGameGateValid();
				}
				model.RefreshStandings();
				break;
			}
			case "game.race.lap@step":
			{
				int num2 = Reflection<object>.Get<int>(p_data, 0);
				int num3 = Reflection<object>.Get<int>(p_data, 1);
				float num4 = Reflection<object>.Get<float>(p_data, 4);
				Drone drone2 = Reflection<object>.Get<Drone>(p_data, 3);
				if (!(base.app.model.game.playerDrone != drone2))
				{
					if (num2 - 1 > model.currentLap)
					{
						Notify(1f / 60f, "game.race.lap@change", num2 - 1, num3, drone2, num4);
						base.game.model.replay.recorder.PushEvent(7, drone2, num2 - 1);
						model.UpdateLapTimes(num2 - 1);
					}
					base.ui.hud.race.SetLap(num2, num3);
				}
				break;
			}
			case "game.standings@update":
				RefreshStandings();
				break;
			case "game.race.gate@complete":
				if (!resetInProgress)
				{
					Drone drone3 = Reflection<object>.Get<Drone>(p_data, 3);
					if (base.game.model.playerDrone == drone3)
					{
						restartLocked = true;
					}
					drone3.invulnerable = 60f;
					base.game.model.GetPlayerData(drone3);
					float p_race_time = Reflection<object>.Get<float>(p_data, 4);
					_ = base.game.model.simulation;
					OnRaceDroneComplete(drone3, p_race_time, RaceStatusType.Success);
					model.RefreshStandings();
				}
				break;
			case "game.simulation.drone@crash":
			{
				Drone d = Reflection<object>.Get<Drone>(p_data, 0);
				if (base.game.model.playerDrone != d || d == null)
				{
					break;
				}
				if (!base.validContext || resetInProgress)
				{
					d.Fix();
					break;
				}
				GamePlayerData pd = base.game.model.GetPlayerData(d);
				float race_time = model.time;
				if (ReplayFile.EnableVersion2)
				{
					base.game.model.replay.recorder.PushEvent(3, d);
				}
				else
				{
					base.game.model.replay.recorder.fps = 300;
					base.game.model.replay.recorder.PushEvent(3, d);
				}
				if (!base.app.inMultiplayer)
				{
					this.TimerRunOnce(delegate
					{
						RaceStatusType raceStatusType = (model.IsComplete(d) ? RaceStatusType.Success : RaceStatusType.Crash);
						pd.raceStatus = raceStatusType;
						OnRaceDroneComplete(d, race_time, raceStatusType);
						model.RefreshStandings();
					}, 0.1f);
				}
				break;
			}
			case "game.simulation.drone@ready":
			{
				Drone drone4 = Reflection<object>.Get<Drone>(p_data, 0);
				if ((bool)drone4)
				{
					drone4.renderer.SetTrailsDuration(0.2f);
					if (!(base.game.model == null) && !(base.game.model.playerDrone != drone4))
					{
						drone4.rigidbody.rb.constraints = RigidbodyConstraints.FreezeRotation;
					}
				}
				break;
			}
			case "garage.edit.fly.ready":
				RequestRaceReset();
				break;
			case "garage.isClosed":
				if ((string)p_data[0] == "pause")
				{
					m_resetAfterPause = true;
				}
				break;
			case "tournament.action.start-match":
			{
				DRLTournamentData tournamentData = base.app.arguments.game.tournamentData;
				if (!base.validContext || tournamentData == null)
				{
					break;
				}
				string mid = (string)p_data[0];
				if (string.IsNullOrEmpty(mid))
				{
					break;
				}
				_ = base.app.model.storage.state.player.profile.playerId;
				DRLTournamentRoundData activeRound2 = tournamentData.GetActiveRound();
				if (activeRound2 == null || activeRound2.gameMode != TournamentRoundGameMode.leaderboard)
				{
					break;
				}
				TournamentModel tm = base.app.model.tournament;
				tm.RefreshData(delegate
				{
					if (base.validContext && tm.tournament != null)
					{
						string text = base.app.view.ui.screens.current?.name;
						if (string.IsNullOrEmpty(text) || (text != "tournament-brackets-screen" && tm.IsRacerInMatch(mid)))
						{
							base.app.view.ui.screens.Open<UITournamentBracketsView>("tournament-brackets-screen").backButtonEnabled = false;
						}
					}
				});
				break;
			}
			case "tournament.action.refresh":
			{
				DRLTournamentData tournament = base.app.model.tournament.tournament;
				if (base.validContext && tournament != null)
				{
					DRLTournamentRoundData activeRound = base.app.model.tournament.activeRound;
					if (m_raceFinished)
					{
						Notify("tournament.action.refresh-racers");
					}
					if (tournament.status == TournamentState.canceled && (!(base.app.view.ui.screens.current != null) || !(base.app.view.ui.screens.current.name == "tournament-brackets-screen")))
					{
						string p_roundName = ((activeRound != null) ? activeRound.title : "TOURNAMENT");
						QuitTournamentMatch(p_roundName, 0f, p_forceQuit: true);
					}
				}
				break;
			}
			case "garage.drone.fc-changed":
			{
				if (p_data.Length == 0)
				{
					m_usingDRLPilotMode = false;
					break;
				}
				FCMode fCMode = (FCMode)p_data[0];
				m_usingDRLPilotMode = fCMode == FCMode.DRLPilot && m_usingDRLPilotMode;
				break;
			}
			}
		}

		protected void InitializePath()
		{
			ResetDronePathGuide();
		}

		protected void ResetDronePathGuide()
		{
			Drone playerDrone = base.game.model.playerDrone;
			if ((bool)playerDrone)
			{
				int progress = model.GetProgress(playerDrone);
				base.game.model.level.track.pathTrace.RefreshTrace(progress, playerDrone.position, p_force: true);
			}
		}

		protected override void PlayIntroAnimation()
		{
			base.app.view.ui.game.hud.damage.Show(p_flag: false);
			if (!PlayPodiumAnimation() && !PlayTrackAnimation())
			{
				Debug.LogWarning("RaceController> Failed to play intro animation!");
			}
		}

		protected override void OnIntroAnimationComplete()
		{
			StopIntroAnimations();
			if (base.validContext && !(base.game == null) && !(base.game.model == null))
			{
				if (base.game.model.level != null)
				{
					base.game.model.level.radio.boundsSignal = 1f;
				}
				if (base.game.model.camera != null)
				{
					base.game.model.camera.fx.radio = 1f;
				}
			}
		}

		protected virtual void OnRaceDroneComplete(Drone p_drone, float p_race_time, RaceStatusType p_status)
		{
			DroneSimulation simulation = base.game.model.simulation;
			GamePlayerData playerData = base.game.model.GetPlayerData(p_drone);
			p_drone.invulnerable = 60f;
			switch (playerData.type)
			{
			case GamePlayerType.Network:
				if (p_drone != null)
				{
					if (p_drone.hasFc)
					{
						p_drone.fc.armed = false;
					}
					p_drone.SetMotorSpinSpeed(0f, 1f);
				}
				break;
			case GamePlayerType.Ghost:
			{
				DroneGhostTransmitter byDrone = simulation.transmitters.GetByDrone<DroneGhostTransmitter>(p_drone);
				if ((bool)byDrone)
				{
					byDrone.usePhysics = true;
				}
				if (p_drone.hasFc)
				{
					p_drone.fc.armed = false;
				}
				p_drone.SetMotorSpinSpeed(0f, 1f);
				break;
			}
			case GamePlayerType.Human:
				if (base.game.model.IsPlayer(p_drone))
				{
					OnRaceComplete(p_race_time, p_status);
				}
				break;
			case GamePlayerType.Spectator:
				break;
			}
		}

		protected virtual void OnRaceComplete(float p_race_time, RaceStatusType p_status)
		{
			if (model.raceComplete || resetInProgress)
			{
				return;
			}
			Notify("game.race.complete");
			model.CalculateLapTimes(model.currentLap);
			Physics.autoSyncTransforms = true;
			Physics.SyncTransforms();
			float p_delay = ((p_status != RaceStatusType.Crash) ? 0f : 1.8f);
			GamePlayerData pd = base.game.model.playerData;
			Drone d = base.game.model.playerDrone;
			DroneCamera camera = base.game.model.camera;
			RaceStatusType raceStatusType = p_status;
			if (((uint)(raceStatusType - 2) <= 2u || raceStatusType == RaceStatusType.Forfeit) && (bool)camera)
			{
				camera.fx.ExposureGrayscale(p_flag: true, 3f);
			}
			ReplayRecorderController recorder = base.game.replay.recorder;
			if (ReplayFile.EnableVersion2)
			{
				pd.replayV2 = recorder.model.GetReplay(d);
			}
			else
			{
				pd.replay = recorder.model.GetData(d);
			}
			bool flag = base.game.model.racerCount > 1;
			EnableRaceEndSlowmotion(d, 2f, !flag, p_status);
			Notify("game.race.slowmo@start", d, 4f);
			if (d != null)
			{
				Notify("game.simulation.drone.flight-time@update", d.rig);
			}
			this.TimerRunOnce(delegate
			{
				if (!m_requestedRestart)
				{
					base.app.model.service.stateAutoRefresh = true;
					model.raceComplete = true;
					model.status = p_status;
					if (!base.game.model.multiplayer)
					{
						ForceCompleteGhostDrones();
					}
					if (pd == null)
					{
						base.game.model.TryFetchSpectatorData();
					}
					base.ui.hud.race.fade.alpha = -0.1f;
					base.ui.hud.marker.fade.alpha = -0.1f;
					base.ui.hud.controller.fade.alpha = -0.1f;
					base.ui.hud.standingsFade.alpha = -0.1f;
					base.game.model.level.track.pathTrace.rendererEnabled = false;
					base.ui.hud.dashboard.gameObject.SetActive(value: false);
					base.ui.hud.physics.gameObject.SetActive(value: false);
					switch (p_status)
					{
					case RaceStatusType.Success:
						base.app.view.audio.PlayGameGateFinalValid();
						break;
					case RaceStatusType.Timeout:
						base.app.view.audio.PlayGameRaceFailure();
						break;
					case RaceStatusType.Forfeit:
						base.app.view.audio.PlayGameRaceFailure();
						break;
					case RaceStatusType.Crash:
						base.app.view.audio.PlayGameRaceFailure();
						break;
					case RaceStatusType.Quit:
						base.app.view.audio.PlayGameRaceFailure();
						break;
					}
					model.time = p_race_time;
					if (ReplayFile.EnableVersion2)
					{
						pd.replayV2 = base.game.replay.recorder.model.GetReplay(d);
					}
					else
					{
						pd.replay = base.game.replay.recorder.model.GetData(d);
					}
					CheatAssert();
					this.TimerRunOnce(delegate
					{
						if (base.validContext)
						{
							model.RefreshStandings();
							RefreshStandings();
						}
					}, 1f / 12f);
					OnReplayHeaderWrite();
					if (m_race_complete_timer != null)
					{
						m_race_complete_timer.Stop();
					}
					if (m_replay_stop_timer != null)
					{
						m_replay_stop_timer.Stop();
					}
					if (d != null && d.hasFc)
					{
						d.fc.allowThrottle = false;
						d.fc.allowPitch = false;
						d.fc.allowRoll = false;
						d.fc.allowYaw = false;
					}
				}
			}, p_delay);
		}

		protected override void OnRaceSlowmotionStop()
		{
			if (!model.raceComplete || !base.validContext || resetInProgress)
			{
				return;
			}
			Debug.Log("RaceController> OnRaceSlowmotionStop");
			base.app.view.ui.screens.SetStaticBackground();
			Drone d = base.game.model.playerDrone;
			bool raceComplete = model.raceComplete;
			bool multiplayer = base.game.model.multiplayer;
			bool flag = base.app.arguments.game.tournamentData != null;
			bool flag2 = model.GetRacerRankingCount() > 1 || (multiplayer && flag);
			GamePlayerData gamePlayerData = base.game.model.playerData;
			if (gamePlayerData == null)
			{
				gamePlayerData = base.game.model.TryFetchSpectatorData();
			}
			string text = "";
			for (int i = 0; i < model.Rankings.Count; i++)
			{
				GamePlayerData gamePlayerData2 = model.Rankings[i];
				text = ((gamePlayerData2 != null) ? (text + $"  [{i}] {gamePlayerData2.type} {gamePlayerData2.name}\n") : (text + $"  [{i}] <null>\n"));
			}
			Debug.Log("RaceController> OnRaceSlowmotionStop / Ranking\n" + text);
			float p_time = 0f;
			if (model.status == RaceStatusType.Success)
			{
				p_time = gamePlayerData?.raceTime ?? 0f;
			}
			if (!multiplayer)
			{
				base.app.view.audio.StopAllGameAudio();
			}
			base.app.view.audio.PlayMusicPostGame();
			PlayerStateModel player = base.app.model.storage.state.player;
			if (base.app.inOnboarding)
			{
				raceComplete = model.Rankings[0].type == GamePlayerType.Human;
				if (raceComplete)
				{
					Notify("onboarding.mission.complete@increase");
				}
				base.app.model.onboarding.ResetPreset(d);
				base.app.model.onboarding.hasFailed = !raceComplete;
				int currentStep = base.app.model.onboarding.currentStep;
				int num = base.app.model.onboarding.activeOnboarding.steps.Count - 1;
				if (num >= currentStep && currentStep == num && raceComplete)
				{
					base.app.view.ui.fade.Fade(0f, 1f, 1f, 0f);
					base.app.view.ui.screens.Close("onboarding-steps-menu-screen");
					UIOnboardingCompleteView uIOnboardingCompleteView = base.app.view.ui.screens.Open<UIOnboardingCompleteView>("onboarding-complete-screen", 0.3f);
					uIOnboardingCompleteView.isMissionCompleted = false;
					uIOnboardingCompleteView.isLastRace = true;
					uIOnboardingCompleteView.SetNextButtonNextText();
				}
				else
				{
					base.app.view.ui.fade.Fade(0f, 1f, 1f, 0f);
					UIOnboardingStepsView uIOnboardingStepsView = base.app.view.ui.screens.Open<UIOnboardingStepsView>("onboarding-steps-menu-screen", 0.3f);
					string text2 = Format.SecondsToMMSSFFF(model.GetPlayerRankings().raceTime);
					base.app.model.onboarding.hasFailed = !raceComplete;
					uIOnboardingStepsView.playerTime.text = text2.ToString();
					uIOnboardingStepsView.nextButton.gameObject.SetActive(raceComplete);
					uIOnboardingStepsView.retryButton.gameObject.SetActive(!raceComplete);
					uIOnboardingStepsView.startButton.gameObject.SetActive(!raceComplete);
					if (base.app.model.onboarding.activeOnboarding.mode == OnboardingCampaignMode.Pro)
					{
						uIOnboardingStepsView.missionsButton.gameObject.SetActive(!raceComplete);
					}
					uIOnboardingStepsView.Set(base.app.model.onboarding.activeOnboarding, base.app.model.onboarding.currentStep);
					uIOnboardingStepsView.avatarsGroup.SetActive(value: true);
					uIOnboardingStepsView.SetMarkers(raceComplete);
				}
			}
			else
			{
				UIRaceCompleteView uIRaceCompleteView = base.app.view.ui.screens.Open<UIRaceCompleteView>("game-race-complete-screen");
				if (flag2)
				{
					uIRaceCompleteView.standings.Refresh(model.Rankings, p_clear: true, p_dnf: false, p_displayDNF: false);
				}
				uIRaceCompleteView.race = this;
				uIRaceCompleteView.headerPhoto = player.profile.photo;
				uIRaceCompleteView.showStandings = flag2;
				uIRaceCompleteView.replayUploadStarted = false;
				uIRaceCompleteView.Fade(p_flag: true, player.profile.username.ToUpper(), p_time, 0.25f);
				uIRaceCompleteView.willSetLeaderboard = raceComplete && !base.game.model.fromEditor;
				uIRaceCompleteView.willUpdateCircuits = base.app.inCircuits && model.status == RaceStatusType.Success;
			}
			base.game.SetGCEnabled(p_flag: true);
			int num2 = (int)(Profiler.GetMonoHeapSizeLong() / 1024 / 1024);
			int num3 = (int)(Profiler.GetTotalReservedMemoryLong() / 1024 / 1024);
			Debug.Log("RaceController> OnRaceSlowmotionStop - allocated heap: " + num2 + "MB allocated total reserved: " + num3 + "MB used heap " + (int)(Profiler.usedHeapSizeLong / 1024 / 1024));
			if (model.status != RaceStatusType.Success)
			{
				p_time = 0f;
			}
			this.TimerRunOnce(delegate
			{
				if (base.validContext)
				{
					DisableRaceEndSlowmotion(d);
					DroneCamera camera = base.game.model.camera;
					if ((bool)camera)
					{
						camera.fx.ExposureGrayscale(p_flag: false, 0f);
					}
					ProcessReplays();
					RunOnce(delegate
					{
						if (!(this == null) && !(d == null) && base.validContext)
						{
							Debug.Log("RaceController> OnRaceSlowmotionStop - Disable local drone. Assume it's landed");
							d.fc.armed = false;
							d.SetMotorSpinSpeed(0f);
							m_raceFinished = true;
							Notify("game.race.slowmo@stop");
						}
					}, 2f, unscaledTime: true);
				}
			}, 2f);
		}

		protected override void LoadDrones()
		{
			List<GamePlayerData> players = base.game.model.players;
			for (int i = 0; i < players.Count; i++)
			{
				GamePlayerData p_player = players[i];
				CreatePlayer(p_player, model.rig);
			}
			ResetGhostDrones();
		}

		protected override void OnGameReady()
		{
			base.ui.hud.Fade(1f, 0.5f, 1f);
			InitializeRace();
			StartCount();
		}

		public virtual bool WillShowTopRacers()
		{
			if (model.Rankings.Count >= 2)
			{
				return base.game.model.hasDifferentPlayers;
			}
			return false;
		}

		protected virtual void RequestRaceReset()
		{
			if (model.raceComplete || resetInProgress)
			{
				return;
			}
			resetInProgress = true;
			Drone playerDrone = base.app.model.game.playerDrone;
			if (playerDrone != null)
			{
				playerDrone.fc.Reset();
				playerDrone.receiver.enabled = false;
				playerDrone.fc.armed = false;
				playerDrone.ClearForces();
				playerDrone.receiver.ClearSignal();
				SetDroneForBeginnerOnboarding(playerDrone);
			}
			m_requestedRestart = true;
			Notify("game.ui.dashboard@hide");
			base.app.view.ui.game.hud.dashboard.openedFromPause = false;
			base.app.view.ui.game.hud.dashboard.openingAnotherScreen = false;
			if (playerDrone != null && (playerDrone.rig.diameter != m_droneClass || base.app.model.storage.state.player.garage.IsOfficial(playerDrone.rig) != m_official))
			{
				m_droneClass = playerDrone.rig.diameter;
				m_official = base.app.model.storage.state.player.garage.IsOfficial(playerDrone.rig);
				m_customPhysics = playerDrone.rig.hasCustomPhysics;
				base.game.model.simulation.transmitters.RemoveGhostDrones();
				if (base.app.arguments.game.opponentType == OpponentModeType.Off)
				{
					RunOnce(0.02f, delegate
					{
						RaceReset();
					});
					return;
				}
				if (m_race_complete_timer != null)
				{
					m_race_complete_timer.Stop();
				}
				if (m_replay_stop_timer != null)
				{
					m_replay_stop_timer.Stop();
				}
				model.countActive = true;
				model.raceActive = false;
				base.ui.hud.race.fade.alpha = -0.1f;
				base.ui.hud.controller.fade.alpha = -0.1f;
				base.ui.hud.counter.fade.alpha = -0.1f;
				base.ui.hud.counter.Clear();
				base.game.replay.recorder.model.Clear();
				List<GamePlayerData> players = base.app.model.game.players;
				for (int num = 0; num < players.Count; num++)
				{
					if (players[num].type != GamePlayerType.Human)
					{
						base.app.model.game.simulation.RemoveDrone(players[num].drone);
						players.RemoveAt(num--);
					}
				}
				base.app.arguments.game.players = base.app.model.game.players;
				Debug.Log("RaceController> reloading opponents: " + base.app.arguments.game.opponentType.ToString() + " class [" + m_droneClass + "]");
				CancelOpponentLoad();
				dialog = base.app.view.ui.screens.Open<UIDialogView>("dialog-screen");
				dialog.Clear();
				dialog.SetNav(p_left: false, p_right: false);
				RunOnce(4f, delegate
				{
					if ((bool)dialog && (bool)dialog.gameObject)
					{
						dialog.SetNav(null, base.app.model.storage.locale.Get("ui.common.start", "START"));
					}
				});
				UIDialogController controller = dialog.controller;
				controller.OnNavRight = (Action)Delegate.Combine(controller.OnNavRight, (Action)delegate
				{
					CancelOpponentLoad();
					RaceReset();
				});
				dialog.status.SetLoading(0f);
				ServiceModel service = base.app.model.service;
				switch (base.app.arguments.game.opponentType)
				{
				case OpponentModeType.Leader:
					ghostDataRequest = service.GetReplayRivals(base.app.arguments.game.map, base.app.arguments.game.track, 1, m_droneClass, m_official, m_customPhysics, OnReplayManifest);
					break;
				case OpponentModeType.Top5:
					ghostDataRequest = service.GetReplayRivals(base.app.arguments.game.map, base.app.arguments.game.track, 5, m_droneClass, m_official, m_customPhysics, OnReplayManifest);
					break;
				case OpponentModeType.Self:
					ghostDataRequest = service.GetReplayRivals(base.app.arguments.game.map, base.app.arguments.game.track, 3, m_droneClass, m_official, m_customPhysics, OnReplayManifest);
					break;
				case OpponentModeType.Random5:
					ghostDataRequest = service.GetReplayRivals(base.app.arguments.game.map, base.app.arguments.game.track, 5, m_droneClass, m_official, m_customPhysics, OnReplayManifest);
					break;
				case OpponentModeType.Random50:
					ghostDataRequest = service.GetReplayRivals(base.app.arguments.game.map, base.app.arguments.game.track, 50, m_droneClass, m_official, m_customPhysics, OnReplayManifest);
					break;
				case OpponentModeType.Off:
				case OpponentModeType.Rival5:
					break;
				}
			}
			else
			{
				RunOnce(0.02f, delegate
				{
					RaceReset();
				});
			}
		}

		private void SetDroneForBeginnerOnboarding(Drone d)
		{
			if (base.app.inOnboarding && base.app.model.onboarding.activeOnboarding.mode == OnboardingCampaignMode.Beginner)
			{
				base.app.model.onboarding.GetDroneProfile(d);
				base.app.model.onboarding.SetPresetHigh(d);
			}
		}

		protected void OnReplayManifest(DRLLeaderboardRivalsResult p_list)
		{
			if (p_list == null)
			{
				if ((bool)dialog && dialog.visible)
				{
					dialog.status.SetWarning("LOADING FAILED!");
					RunOnce(1f, delegate
					{
						RaceReset();
					});
					base.app.view.audio.PlayUIGenericError();
					CancelOpponentLoad();
				}
				return;
			}
			Debug.Log("RaceController> OnReplayManifest\n" + p_list);
			base.app.view.audio.PlayUIGenericSuccess();
			string[] array = new string[0];
			int num = 0;
			switch (base.app.arguments.game.opponentType)
			{
			case OpponentModeType.Top5:
				array = p_list.GetTopReplays();
				num = 5;
				break;
			case OpponentModeType.Leader:
				array = p_list.GetTopReplays(p_include_player: true);
				num = 1;
				break;
			case OpponentModeType.Rival5:
				array = p_list.GetRivalReplays();
				num = 5;
				break;
			case OpponentModeType.Self:
				array = p_list.GetPastReplays();
				num = 1;
				break;
			case OpponentModeType.Random5:
			case OpponentModeType.Random50:
			{
				List<string> list = new List<string>();
				array = p_list.GetTopReplays();
				if (((p_list.rivals != null && p_list.rivals.Length != 0) ? 1 : 0) > (false ? 1 : 0) && base.app.arguments.game.opponentType == OpponentModeType.Random50)
				{
					array = new string[0];
				}
				list.AddRange(array);
				array = p_list.GetRivalReplays();
				list.AddRange(array);
				for (int num2 = 0; num2 < list.Count; num2++)
				{
					list.Sort((string sa, string sb) => (!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
				}
				int num3 = Mathf.Min(list.Count, 5);
				array = new string[num3];
				for (int num4 = 0; num4 < num3; num4++)
				{
					array[num4] = list[num4];
				}
				num = 5;
				break;
			}
			}
			if (array.Length > num)
			{
				Array.Resize(ref array, num);
			}
			Debug.Log("RaceController> OnReplayManifest - List\n" + string.Join("\n", array));
			if (array.Length == 0)
			{
				if ((bool)dialog && dialog.visible)
				{
					dialog.status.SetWarning("NO OPPONENTS FOUND!");
				}
				RunOnce(1f, delegate
				{
					RaceReset();
				});
				CancelOpponentLoad();
				return;
			}
			for (int num5 = 0; num5 < array.Length; num5++)
			{
				WebAsyncRequest item = Web.Get(array[num5], delegate(byte[] p_result, float p_progress, WebAsyncRequest p_request)
				{
					if (p_progress >= 1f && p_result == null)
					{
						Debug.LogWarning("RaceController> OnReplayManifest - replay[" + p_request.path + "] failed!");
					}
				});
				ghostRequests.Add(item);
			}
			float c = 0f;
			float t = ghostRequests.Count;
			ghostProcessingLoop = Run((Func<bool>)delegate
			{
				bool flag = true;
				for (int i = 0; i < ghostRequests.Count; i++)
				{
					c += ghostRequests[i].progress;
					if (!ghostRequests[i].completed)
					{
						flag = false;
					}
				}
				float num6 = ((t <= 0f) ? 1f : Mathf.Clamp01(c / t));
				if ((bool)dialog && dialog.visible)
				{
					dialog.status.SetLoading(num6 * 0.5f);
				}
				if (flag)
				{
					Debug.Log("RaceController> OnReplayManifest - Replays Download Complete!");
					if ((bool)dialog && dialog.visible)
					{
						dialog.status.SetLoading(0.5f);
					}
					for (int j = 0; j < ghostRequests.Count; j++)
					{
						ghostReplays.Add(ghostRequests[j].Get<byte[]>());
					}
					RunOnce(ProcessReplayFiles, 1f / 30f);
					return false;
				}
				return true;
			}, 0f, false);
		}

		protected void ProcessReplayFiles()
		{
			List<byte[]> dl = ghostReplays;
			int num = 0;
			for (int i = 0; i < dl.Count; i++)
			{
				if (dl[i] == null)
				{
					dl.RemoveAt(i--);
				}
				else
				{
					num += dl[i].Length;
				}
			}
			Debug.Log("RaceController> ProcessReplayFiles - count[" + dl.Count + "] length[" + num + " bytes]");
			if (dl.Count <= 0)
			{
				RaceReset();
				return;
			}
			float rstart = UnityEngine.Random.Range(0.5f, 0.6f);
			float thread_progress = rstart;
			ghostParsersComplete = 0;
			CreateProcessThread(dl);
			ghostProcessingLoop = Run((Func<bool>)delegate
			{
				bool flag = ghostParsersComplete >= dl.Count;
				float num2 = (float)ghostParsersComplete / (float)dl.Count;
				ghostProcessTimeout += Time.deltaTime;
				if (ghostProcessTimeout >= 10f)
				{
					flag = true;
				}
				if (flag)
				{
					AbortProcessingThreads();
					if ((bool)dialog && dialog.visible)
					{
						dialog.status.SetLoading(1f);
					}
					while (ghostRecords.Count > 5)
					{
						ghostRecords.RemoveAt(ghostRecords.Count - 1);
					}
					BlackboxRecord p_data = BlackboxRecord.Merge(ghostRecords.ToArray());
					base.app.arguments.game.AddGhostPlayer(p_data);
					base.app.model.game.players = new List<GamePlayerData>(base.app.arguments.game.players);
					List<GamePlayerData> players = base.app.model.game.players;
					for (int j = 0; j < players.Count; j++)
					{
						GamePlayerData gamePlayerData = players[j];
						if (gamePlayerData.type != GamePlayerType.Human)
						{
							CreatePlayer(gamePlayerData, model.rig);
						}
					}
					ResetGhostDrones();
					RunOnce(RaceReset, 0.5f);
					return false;
				}
				float num3 = Mathf.Lerp(0.1f, 0.05f, Mathf.Clamp01((thread_progress - rstart) / rstart)) * 0.333f;
				thread_progress += Time.deltaTime * 0.5f * num3;
				if ((bool)dialog && dialog.visible)
				{
					dialog.status.SetLoading(thread_progress + num2 * 0.8f);
				}
				return true;
			}, 0f, false);
		}

		protected Thread CreateProcessThread(List<byte[]> p_data)
		{
			Thread thread = new Thread((ThreadStart)delegate
			{
				BlackboxRecord blackboxRecord = null;
				for (int i = 0; i < p_data.Count; i++)
				{
					blackboxRecord = Serialize.FromBytes<BlackboxRecord>(p_data[i], p_unsafe: false);
					Debug.Log("RaceController> Replay [" + i + "] [" + blackboxRecord?.ToString() + "]");
					if (blackboxRecord != null)
					{
						ghostRecords.Add(blackboxRecord);
					}
					ghostParsersComplete++;
					ghostProcessTimeout = 0f;
				}
				Debug.Log("RaceController> CreateProcessThread - Replay [" + ghostParsersComplete + "/" + ghostParsers.Count + "] Parse Complete!");
			});
			thread.Start();
			return thread;
		}

		protected Thread CreateProcessThread(byte[] p_data)
		{
			int retries = 3;
			Thread thread = new Thread((ThreadStart)delegate
			{
				BlackboxRecord blackboxRecord = null;
				while (retries > 0)
				{
					blackboxRecord = Serialize.FromBytes<BlackboxRecord>(p_data, p_unsafe: true);
					if (blackboxRecord != null)
					{
						break;
					}
					retries--;
					Thread.Sleep(50);
				}
				ghostParsersMtx.WaitOne(2000);
				if (blackboxRecord != null)
				{
					ghostRecords.Add(blackboxRecord);
				}
				ghostParsersComplete++;
				ghostParsersMtx.ReleaseMutex();
				ghostProcessTimeout = 0f;
				Debug.Log("RaceController> CreateProcessThread - Replay [" + ghostParsersComplete + "/" + ghostParsers.Count + "] Parse Complete!");
			});
			thread.Start();
			return thread;
		}

		public void CancelOpponentLoad()
		{
			if (ghostDataRequest != null)
			{
				ghostDataRequest.Cancel();
			}
			ghostDataRequest = null;
			AbortProcessingThreads();
			if (ghostRequests == null)
			{
				ghostRequests = new List<WebAsyncRequest>();
			}
			for (int i = 0; i < ghostRequests.Count; i++)
			{
				ghostRequests[i].Cancel();
			}
			ghostRequests.Clear();
			if (ghostReplays == null)
			{
				ghostReplays = new List<byte[]>();
			}
			ghostReplays.Clear();
			if (ghostRecords == null)
			{
				ghostRecords = new List<BlackboxRecord>();
			}
			ghostRecords.Clear();
			if (ghostProcessingLoop != null)
			{
				ghostProcessingLoop.Stop();
			}
		}

		protected void AbortProcessingThreads()
		{
			if (ghostParsers == null)
			{
				ghostParsers = new List<Thread>();
			}
			for (int i = 0; i < ghostParsers.Count; i++)
			{
				ghostParsers[i].Abort();
			}
			ghostParsers.Clear();
			if (ghostParsersMtx == null)
			{
				ghostParsersMtx = new Mutex();
			}
		}

		public void RaceReset()
		{
			CancelOpponentLoad();
			if ((bool)dialog && dialog.visible)
			{
				base.app.view.ui.screens.Close("dialog-screen");
			}
			base.app.view.ui.footer.Hide(0.05f);
			Drone d = base.game.model.playerDrone;
			if (!d)
			{
				return;
			}
			GameStateModel gsm = base.app.model.storage.state.player.settings.game;
			if (m_race_complete_timer != null)
			{
				m_race_complete_timer.Stop();
			}
			if (m_replay_stop_timer != null)
			{
				m_replay_stop_timer.Stop();
			}
			d.receiver.ClearSignal();
			d.receiver.enabled = false;
			d.fc.armed = false;
			SetDroneForBeginnerOnboarding(d);
			base.app.view.ui.fade.FadeIn(0.5f);
			this.TimerRunOnce(delegate
			{
				if (m_raceSlowmotionStarted)
				{
					DisableRaceEndSlowmotion(d);
					base.game.model.camera?.fx.ExposureGrayscale(p_flag: false, 0f);
				}
			}, 0.51f);
			model.countActive = true;
			model.raceActive = false;
			base.app.view.audio.PlayUILevelRestart();
			base.game.SetTabScreenEnabled(p_flag: false);
			base.ui.hud.physics.Hide();
			this.TimerRunOnce(delegate
			{
				if (model.raceComplete)
				{
					base.app.view.ui.screens.Close("game-race-complete-screen");
					DisableRaceEndSlowmotion(d);
					base.app.view.audio.PlayMusicGame();
				}
				base.game.model.level.track.pathTrace.RefreshTrace(0, d.position, p_force: true);
				d.Fix();
				base.app.model?.service?.opponent?.ForceResetLoadedReplays();
				GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true);
				InitializeRace();
				StartCount(gsm.raceFastReset);
				base.app.view.ui.fade.FadeOut(0.5f, 1f / 30f);
				if (d.body != null && d.body.frame != null && d.body.frame.batteries != null)
				{
					foreach (DroneBattery battery in d.body.frame.batteries)
					{
						if (battery != null)
						{
							battery.Recharge();
						}
					}
				}
			}, 0.52f);
		}

		public new virtual string GetRaceTitle()
		{
			return "Race Complete!";
		}

		public override void OnDroneScrape(DroneEvent p_event)
		{
			if ((bool)base.game.model.replay && (bool)base.game.model.replay.recorder)
			{
				base.game.model.replay.recorder.PushEvent(2, p_event.target);
			}
		}

		public void InitializeRace()
		{
			if (!base.validContext)
			{
				return;
			}
			DroneSimulation simulation = base.game.model.simulation;
			Drone playerDrone = base.game.model.playerDrone;
			model.playerDrone = playerDrone;
			DroneCamera droneCamera = simulation.cameras.Get(0);
			base.app.view.audio.ResetGameRadioSignal(playerDrone ? playerDrone.gameObject : null);
			base.game.PodiumResetAll();
			ResetGhostDrones();
			UnfreezeDrones();
			FCMode activeFCMode = base.app.model.storage.state.player.activeFCMode;
			SetDroneFCMode(playerDrone, activeFCMode);
			SetDroneForBeginnerOnboarding(playerDrone);
			if (activeFCMode == FCMode.DRLPilot)
			{
				m_usingDRLPilotMode = true;
			}
			if ((bool)droneCamera)
			{
				droneCamera.SetFPV(playerDrone);
			}
			if (playerDrone != null)
			{
				playerDrone.rigidbody.rb.constraints = RigidbodyConstraints.FreezeAll;
			}
			simulation.drones.SetArmed(p_flag: true);
			simulation.drones.SetReceiver(p_flag: false);
			this.TimerRunOnce(delegate
			{
				m_requestedRestart = false;
			}, 2.1f);
			base.ui.hud.race.fade.alpha = -0.1f;
			base.ui.hud.controller.fade.alpha = -0.1f;
			base.ui.hud.counter.fade.alpha = -0.1f;
			base.ui.hud.counter.Clear();
			base.ui.hud.counter.fade.FadeIn(0.8f);
			base.ui.hud.gameTitle.Clear();
			base.ui.hud.gameTitle.fade.alpha = 1f;
			SetTitle();
			base.ui.hud.race.Clear();
			int p_current = 1;
			int laps = base.game.model.level.track.laps;
			base.ui.hud.race.SetLap(p_current, laps);
			base.game.model.level.track.SetActionsMode(MapAssetActionMode.Auto);
			base.game.model.level.track.RestoreActions();
			base.game.model.level.track.ClearActionEvents();
			base.game.model.level.track.AddActionListener(OnActionEvent);
			bool promo = false;
			if (base.game.model.type == GameFlag.Campaign)
			{
				CampaignController campaignController = this as CampaignController;
				promo = (bool)campaignController.model.campaign && campaignController.model.campaign.tournament;
			}
			if (base.app.arguments.game.tournamentPromo)
			{
				promo = true;
			}
			if (base.app.arguments.game.promo)
			{
				promo = true;
			}
			base.ui.hud.race.SetPromo(promo);
			_ = base.app.model.storage.state.player.settings.game;
			_ = base.game.model.racerCount;
			_ = 1;
			base.ui.hud.standingsFade.alpha = -0.1f;
			base.ui.hud.SetStandingsCount(base.game.model.racerCount, p_has_positions: true);
			model.RefreshStandings();
			RefreshStandings();
			base.ui.hud.race.SetPosition(1, base.game.model.racerCount);
			base.app.model.storage.saveComplete = false;
			model.countActive = true;
			model.status = RaceStatusType.Idle;
			model.ClearData();
			model.AddDrones(simulation);
			base.game.ClearAllActivities();
			ResetGhostDrones();
			ReplayRecorderController recorder = base.game.replay.recorder;
			recorder.model.Clear();
			for (int num = 0; num < simulation.drones.list.Count; num++)
			{
				Drone drone = simulation.drones.list[num];
				if (drone != base.game.model.playerDrone)
				{
					continue;
				}
				if (drone.rig.hasCustomPhysics && !base.app.inTournament)
				{
					m_customPhysics = true;
					base.ui.hud.dashboard.Init();
					base.ui.hud.dashboard.Hide(p_all: true);
				}
				if (ReplayFile.EnableVersion2)
				{
					recorder.model.AddReplay(drone, p_is_player: true).header.isCustomPhysics = m_customPhysics;
				}
				else
				{
					BlackboxData blackboxData = recorder.model.Add(drone, p_is_player: true);
					if (m_customPhysics)
					{
						blackboxData.SetPhysicsFlag(p_flag: true);
					}
				}
				m_customPhysics = false;
			}
			Debug.Log("MARenderer> Cached material count " + MARenderer.GetCachedMaterials().Count);
			base.ui.hud.marker.fade.alpha = -0.1f;
			this.TimerRunOnce(delegate
			{
				base.ui.hud.marker.Clear();
				for (int i = 0; i < model.gates.Count; i++)
				{
					ColliderEventComponent colliderEventComponent = model.gates[i];
					Component component = colliderEventComponent.transform;
					if (colliderEventComponent.colliders.Count > 0)
					{
						component = colliderEventComponent.colliders[0];
					}
					UIHUDMarker uIHUDMarker = base.ui.hud.marker.Add(component);
					MAGate gateParent = GetGateParent(colliderEventComponent);
					if ((bool)gateParent)
					{
						uIHUDMarker.targetForward = -gateParent.trigger.transform.up;
						uIHUDMarker.bidirectional = gateParent.gateMode == MapGateMode.Bidirectional;
						uIHUDMarker.reverse = gateParent.gateMode == MapGateMode.BackToFront;
					}
					uIHUDMarker.selfUpdate = false;
					uIHUDMarker.alpha = 0f;
					if (i == 0)
					{
						base.ui.hud.marker.SetSelection(component);
					}
				}
			}, 0.5f);
			if (base.app.arguments.game.tournamentData != null)
			{
				m_throttleCapRetry = 10;
				RefreshMatchData();
			}
		}

		private void RefreshMatchData()
		{
			DRLTournamentData td = base.app.arguments.game.tournamentData;
			if (td == null)
			{
				Debug.LogWarning("RaceController> Can't fetch throttle cap, no active tournament!");
				return;
			}
			DRLTournamentRoundData activeRound = td.GetActiveRound();
			if (activeRound == null)
			{
				Debug.LogWarning("RaceController> Can't fetch throttle cap, no active round!");
				return;
			}
			DRLTournamentMatchData tmd = activeRound.GetPlayerMatch(base.app.model.storage.state.player.profile.playerId);
			if (tmd == null)
			{
				Debug.LogWarning("RaceController> Can't fetch throttle cap, no active match or user not racer!");
			}
			else if (string.IsNullOrEmpty(td.guid) || string.IsNullOrEmpty(tmd.Id))
			{
				Debug.LogWarning("RaceController> Can't fetch throttle cap, no active match!");
			}
			else
			{
				if (base.app.model.network.room != null && base.app.model.network.room.Local.IsSpectator)
				{
					return;
				}
				base.app.model.tournament.RefreshMatchData(tmd.Id, delegate(DRLTournamentMatchData result)
				{
					if (base.validContext)
					{
						if (result == null && m_throttleCapRetry > 0)
						{
							m_throttleCapRetry--;
							RefreshMatchData();
						}
						else if (result != null)
						{
							tmd = result;
							float throttleCap = tmd.throttleCap;
							if (throttleCap <= 0f)
							{
								SetThrottleCap();
							}
							else
							{
								RCI.SetThrottleCap(throttleCap);
							}
							if (td.progression != TournamentProgression.manual || td.rounds[tmd.roundIndex].gameMode == TournamentRoundGameMode.leaderboard)
							{
								DateTime completeDate = result.completeDate;
								DateTime currentTime = result.currentTime;
								float num = (float)(completeDate - currentTime).TotalSeconds;
								if (num <= 1f)
								{
									QuitTournamentMatch(td.rounds[tmd.roundIndex].title);
								}
								bool is_spectator = !result.ContainsPlayer(base.app.model.storage.state.player.profile.playerId);
								StartCountdownTimer(num, td.rounds[tmd.roundIndex].title, is_spectator);
							}
						}
					}
				});
			}
		}

		protected new virtual void StartCount(bool p_fast = false)
		{
			Notify("game.count@start");
			float num = (p_fast ? 0.4f : 1f);
			float num2 = 0.8f * num;
			base.ui.hud.gameTitle.Show(num2);
			num2 += 1.5f * num;
			Activity.RunOnce(delegate
			{
				SetCount(1, 3, p_play_audio: true, p_hide_title: false);
			}, num2);
			num2 += 1f * num;
			Activity.RunOnce(delegate
			{
				SetCount(2, 3, p_play_audio: true, p_hide_title: true);
			}, num2);
			num2 += 1f * num;
			Activity.RunOnce(delegate
			{
				SetCount(3, 3, p_play_audio: true, p_hide_title: false);
			}, num2);
		}

		protected new virtual void SetCount(int p_current, int p_max, bool p_play_audio, bool p_hide_title)
		{
			Notify("game.count@step", p_current, p_max, p_play_audio, p_hide_title);
			if (p_current >= p_max)
			{
				Notify("game.count@complete");
			}
		}

		protected new virtual void ApplyCount(int p_current, int p_max, bool p_play_audio = true, bool p_hide_title = false)
		{
			if (p_current >= p_max)
			{
				if (p_play_audio)
				{
					base.app.view.audio.PlayGameCountdownFinish();
				}
			}
			else if (p_play_audio)
			{
				base.app.view.audio.PlayGameCountdownTick();
			}
			base.app.view.audio.UpdateRaceGatesPercentage(0f);
			base.ui.hud.counter.FadeLamp(p_current - 1, p_on: true);
			if (base.game.model.mode != GameFlag.NetworkMultiplayer && p_hide_title)
			{
				base.ui.hud.gameTitle.Hide(0.6f);
			}
		}

		protected new virtual void OnCountComplete()
		{
			float num = 0.8f;
			base.ui.hud.marker.fade.FadeIn(0.25f, num);
			base.ui.hud.race.fade.FadeIn(0.25f, num);
			base.ui.hud.counter.fade.FadeOut(0.25f, num);
			if (m_customPhysics && !base.app.inTournament)
			{
				base.ui.hud.physics.Show();
			}
			GameStateModel gameStateModel = base.app.model.storage.state.player.settings.game;
			float p_alpha = ((model.Rankings.Count <= 1 || !gameStateModel.raceAutoStandings) ? (-0.1f) : 1f);
			base.ui.hud.standingsFade.Fade(p_alpha, 0.25f, num);
			bool controllerOverlay = gameStateModel.controllerOverlay;
			base.ui.hud.controller.fade.Fade(controllerOverlay ? 1f : 0f, 0.25f);
			this.TimerRunOnce(base.ui.hud.counter.Clear, 0.3f + num);
			this.TimerRunOnce(EnableRace, num * 0.3f);
			bool hotkeys = base.app.model.storage.state.player.settings.game.hotkeys;
			base.app.view.ui.game.hud.SetHotkeysEnabled(hotkeys);
			base.game.SetGCEnabled(p_flag: false);
			base.app.model.game.simulation.drones.SetArmed(p_flag: true);
			model.UpdateLapTimes(0);
		}

		protected new virtual void SetTitle()
		{
			UIHUDTitle gameTitle = base.ui.hud.gameTitle;
			DRLMap map = base.app.scene.map;
			DRLMapTrack track = base.app.scene.track;
			string p_caption_left = (track ? track.label : "");
			if ((bool)map && map.data != null)
			{
				p_caption_left = map.data.mapTitle.ToUpper();
			}
			gameTitle.Set(map.label, p_caption_left, base.app.model.storage.locale.Get("race-hud.title.get", "GET"), base.app.model.storage.locale.Get("race-hud.title.ready", "READY!"));
		}

		private void SetThrottleCap()
		{
			bool num = base.app.model.storage.state.player.garage.IsOfficial();
			bool flag = base.app.model.storage.state.player.garage.CanUseDamage();
			RCI.SetThrottleCap((num || flag) ? 80f : (-1f));
		}

		public void EnableRace()
		{
			if (base.game.model == null || base.game.model.simulation == null)
			{
				return;
			}
			Drone playerDrone = base.game.model.playerDrone;
			DroneSimulation simulation = base.game.model.simulation;
			model.countActive = false;
			model.raceComplete = false;
			model.raceActive = true;
			model.raceId = GUID.Create(24, "", 200, 0, 15, "x1");
			model.gateTimes.Clear();
			tempGatesTime.Clear();
			Physics.autoSyncTransforms = false;
			model.timeStart = GetGlobalTime();
			int laps = base.game.model.level.track.laps;
			if (laps > 0)
			{
				for (int i = 0; i < laps; i++)
				{
					model.lapTimes.Add(0f);
				}
			}
			else
			{
				model.lapTimes.Add(0f);
			}
			if ((bool)base.app.acs)
			{
				this.TimerRunOnce(delegate
				{
					if (base.validContext)
					{
						base.app.acs.Clear();
						base.app.acs.Handcap(3);
					}
				}, 2f);
			}
			model.racersCount = GetRacerCount();
			model.ghostsCount = base.game.model.GetPlayerCount(GamePlayerType.Ghost);
			model.status = RaceStatusType.Running;
			ReplayRecorderController recorder = base.game.replay.recorder;
			simulation.drones.SetReceiver(p_flag: true);
			recorder.Stop();
			recorder.Record();
			base.app.model.service.stateAutoRefresh = false;
			base.ui.hud.gameTitle.Clear();
			base.game.model.simulation.transmitters.SetEnabled<DroneGhostTransmitter>(p_flag: true);
			base.game.model.simulation.transmitters.SetGhostDronesSpeed((base.app.inCircuits && base.app.model.storage.state.player.garage.CanUseDamage()) ? 0.8f : 1f);
			resetInProgress = false;
			model.RefreshStandings();
			Notify("game.race.enabled");
			if (playerDrone != null)
			{
				playerDrone.rigidbody.rb.constraints = RigidbodyConstraints.None;
			}
		}

		protected void RefreshStandings()
		{
			GamePlayerData playerData = base.game.model.playerData;
			int count = model.Rankings.Count;
			if (playerData != null)
			{
				int num = model.Rankings.IndexOf(playerData);
				base.ui.hud.race.SetPosition(num + 1, count);
			}
			base.ui.hud.standings.Refresh(model.Rankings, p_clear: false);
			if (m_customPhysics)
			{
				base.ui.hud.physics.view.raceStandingsCount = count;
				base.ui.hud.physics.view.raceStandingsVisible = base.ui.hud.standingsFade.alpha > 0.5f;
			}
		}

		protected virtual bool CanTabScreen()
		{
			if (model.countActive)
			{
				return false;
			}
			if (model.raceComplete)
			{
				return false;
			}
			if (base.game.model.racerCount <= 1)
			{
				return false;
			}
			return true;
		}

		public override bool OnGameCommand(GameCommand p_command)
		{
			if (p_command == null)
			{
				return true;
			}
			if (!base.validContext || DRLApp.isLoading)
			{
				return false;
			}
			if (!base.game)
			{
				return false;
			}
			if (!model)
			{
				return false;
			}
			if (base.game.model == null)
			{
				return false;
			}
			int num;
			int num2;
			if (!base.app.view.ui)
			{
				num = 0;
			}
			else
			{
				num = (((bool)base.app.view.ui.game) ? 1 : 0);
				if (num != 0)
				{
					num2 = (((bool)base.app.view.ui.screens) ? 1 : 0);
					goto IL_009f;
				}
			}
			num2 = 0;
			goto IL_009f;
			IL_009f:
			bool flag = (byte)num2 != 0;
			bool flag2 = num != 0 && (bool)base.app.view.ui.game.hud.dashboard;
			Debug.Log("RaceController> " + p_command.type);
			GameStateModel gameStateModel = base.app.model.storage.state.player.settings.game;
			Drone playerDrone = base.game.model.playerDrone;
			if (playerDrone == null)
			{
				return false;
			}
			bool flag3 = base.app.arguments.game.tournamentData != null;
			bool flag4 = (bool)gameStateModel && gameStateModel.armAndTurtle;
			bool flag5 = flag && base.app.view.ui.screens.current != null;
			bool flag6 = flag2 && base.app.view.ui.game.hud.dashboard.isShowing;
			LevelModel level = base.game.model.level;
			RadioQuality radioQuality = (level ? level.radio : null);
			bool flag7 = !radioQuality || radioQuality.boundsSignal >= 1f;
			switch (p_command.type)
			{
			case GameCommandType.ResetDronePodium:
				if (base.game.model.paused)
				{
					return false;
				}
				if (model.countActive)
				{
					return false;
				}
				if (!flag7)
				{
					return false;
				}
				if (restartLocked)
				{
					return false;
				}
				if (!flag4 || flag3)
				{
					Debug.Log($"RaceController> ResetDronePodium / arm-turtle[{flag4}] tournament[{flag3}] has-screen[{flag5}] replay-upload-active[{m_replayUploadStarted}] restart-locked[{restartLocked}]");
					if (!flag5)
					{
						RequestRaceReset();
					}
					else if (m_replayUploadStarted || model.status == RaceStatusType.Crash)
					{
						Notify("game.race.request-restart");
					}
				}
				else
				{
					base.game.DroneArmDisarm(playerDrone);
				}
				return false;
			case GameCommandType.ResetDrone:
				if (base.game.model == null || model == null || playerDrone == null)
				{
					return false;
				}
				if (base.game.model.paused)
				{
					return false;
				}
				if (model.countActive)
				{
					return false;
				}
				if (!model.raceActive)
				{
					return false;
				}
				if (model.IsComplete())
				{
					return false;
				}
				if (playerDrone == null)
				{
					return false;
				}
				if (playerDrone.isBroken)
				{
					return false;
				}
				if (DRLApp.isLoading || !playerDrone.ready)
				{
					return false;
				}
				SetDroneForBeginnerOnboarding(playerDrone);
				if (!flag4 || flag3)
				{
					if (base.game.model.replay == null)
					{
						return false;
					}
					if ((bool)base.game.model.replay && (bool)base.game.model.replay.recorder)
					{
						base.game.model.replay.recorder.PushEvent(4, playerDrone);
					}
					if (base.game.DroneReset(playerDrone, p_snapToPath: true))
					{
						model.crashes++;
					}
				}
				else
				{
					base.game.DroneTurtle(playerDrone);
				}
				return false;
			case GameCommandType.Pause:
				if (model.countActive)
				{
					return false;
				}
				if (model.raceComplete)
				{
					return false;
				}
				base.game.SetGCEnabled(!base.game.model.paused);
				if (flag6)
				{
					Notify("game.ui.dashboard@hide");
					base.app.view.ui.game.hud.dashboard.openedFromPause = false;
					base.app.view.ui.game.hud.dashboard.openingAnotherScreen = false;
					return false;
				}
				break;
			case GameCommandType.TabScreenEnable:
			case GameCommandType.TabScreenDisable:
				if (!CanTabScreen())
				{
					return false;
				}
				if (p_command.type == GameCommandType.TabScreenEnable)
				{
					base.game.SwitchTabScreen();
				}
				break;
			case GameCommandType.SwitchCameraMode:
				if ((bool)model && model.raceComplete)
				{
					return false;
				}
				break;
			}
			if (p_command.type == GameCommandType.SwitchPhysicsDashboard && resetInProgress)
			{
				return false;
			}
			return base.OnGameCommand(p_command);
		}

		protected void UpdateDroneGuide()
		{
			if (!base.game)
			{
				Debug.LogWarning("RaceController> UpdateDroneGuide - 'game' is null!");
			}
			else if (!base.game.model)
			{
				Debug.LogWarning("RaceController> UpdateDroneGuide - 'game.model' is null!");
			}
			else
			{
				if (base.game.model.paused)
				{
					return;
				}
				Drone playerDrone = base.game.model.playerDrone;
				if (!playerDrone)
				{
					return;
				}
				if (!base.game.model.level)
				{
					Debug.LogWarning("RaceController> UpdateDroneGuide - 'game.model.level' is null!");
					return;
				}
				if (!base.game.model.level.track)
				{
					Debug.LogWarning("RaceController> UpdateDroneGuide - 'game.model.level.track' is null!");
					return;
				}
				SplineTracerComponent pathTrace = base.game.model.level.track.pathTrace;
				if ((bool)pathTrace)
				{
					int progress = model.GetProgress(playerDrone);
					pathTrace.RefreshTrace(progress, playerDrone.position);
				}
			}
		}

		public virtual float GetDeltaTime()
		{
			float deltaTime = Time.deltaTime;
			if (!base.game.model.paused)
			{
				return deltaTime;
			}
			if (!model.stopTimeOnPause)
			{
				return deltaTime;
			}
			return 0f;
		}

		public virtual float GetGlobalTime()
		{
			return Time.time;
		}

		public virtual int GetRacerCount()
		{
			return 1;
		}

		protected virtual void UpdateRace()
		{
			if (base.ui.hud.dashboard.isShowing && !base.app.inMultiplayer)
			{
				base.ui.hud.race.fade.alpha = -0.1f;
				base.ui.hud.marker.fade.alpha = -0.1f;
				base.ui.hud.controller.fade.alpha = -0.1f;
				base.ui.hud.standingsFade.alpha = -0.1f;
			}
			else if (model.raceActive)
			{
				Drone playerDrone = base.game.model.playerDrone;
				if ((bool)playerDrone && playerDrone.ready)
				{
					model.topSpeed = (playerDrone ? Mathf.Max(model.topSpeed, playerDrone.fc.sensor.inertial.groundSpeedKph) : 0f);
				}
				float deltaTime = GetDeltaTime();
				model.time += deltaTime;
				model.currentLapTime += deltaTime;
				if (model.GetPlayerPosition(base.game.model.playerData) == 0)
				{
					model.timeInFirstPlace += deltaTime;
				}
				base.ui.hud.race.time = model.time;
				if (model.time >= 240f && GarbageCollector.GCMode == GarbageCollector.Mode.Disabled)
				{
					Debug.LogWarning("RaceController> UpdateRace / GC Offline Timeout Enable and Collect!");
					base.game.SetGCEnabled(p_flag: true);
				}
			}
		}

		protected override void Update()
		{
			UpdateDroneGuide();
			UpdateRace();
			base.Update();
		}

		public void InitializeGates()
		{
			List<ColliderEventComponent> list = model.gates;
			if (list == null)
			{
				list = new List<ColliderEventComponent>();
			}
			list.Clear();
			List<Collider> gates = base.game.model.level.track.gates;
			for (int i = 0; i < gates.Count; i++)
			{
				Collider collider = gates[i];
				ColliderEventComponent colliderEventComponent = collider.GetComponent<ColliderEventComponent>();
				if (!colliderEventComponent)
				{
					colliderEventComponent = collider.gameObject.AddComponent<ColliderEventComponent>();
				}
				colliderEventComponent.callback.RemoveAllListeners();
				colliderEventComponent.callback.AddListener(GateEvent);
				list.Add(colliderEventComponent);
			}
			Debug.Log("RaceController> InitializeGates - Found [" + list.Count + "] gates.");
		}

		protected void GateEvent(ColliderEvent p_event)
		{
			if (model.IsComplete())
			{
				return;
			}
			string text3;
			if (!m_gate_collider_lut.ContainsKey(p_event.collider))
			{
				string text = (m_gate_collider_lut[p_event.collider] = p_event.collider.name);
				text3 = text;
			}
			else
			{
				text3 = m_gate_collider_lut[p_event.collider];
			}
			if (text3 != "gate" || p_event.type == ColliderEvent.Type.Stay)
			{
				return;
			}
			int num = model.gates.IndexOf(p_event.target);
			bool flag = num >= model.gates.Count - 1;
			ColliderEventComponent p_target = model.gates[num];
			if ((flag && p_event.type == ColliderEvent.Type.Exit) || (!flag && p_event.type == ColliderEvent.Type.Enter))
			{
				return;
			}
			Drone droneParent = GetDroneParent(p_event.collider.transform);
			MAGate gateParent = GetGateParent(p_target);
			MapGateMode mapGateMode = ((!gateParent) ? MapGateMode.Bidirectional : gateParent.gateMode);
			if (mapGateMode != MapGateMode.Bidirectional && (uint)(mapGateMode - 2) <= 1u)
			{
				Vector3 lhs = -gateParent.trigger.transform.up;
				Vector3 velocity = droneParent.fc.sensor.inertial.velocity;
				float num2 = Vector3.Dot(lhs, velocity);
				if ((num2 > 0f && mapGateMode == MapGateMode.FrontToBack) || (num2 < 0f && mapGateMode == MapGateMode.BackToFront))
				{
					return;
				}
			}
			OnGateEvent(p_event.type, num, droneParent);
		}

		public Drone GetDroneParent(Transform p_target)
		{
			if (m_transform_drone_lut == null)
			{
				m_transform_drone_lut = new Dictionary<Component, Drone>();
			}
			if (m_transform_drone_lut.ContainsKey(p_target))
			{
				return m_transform_drone_lut[p_target];
			}
			Transform transform = p_target;
			Drone drone = null;
			while ((bool)transform)
			{
				drone = transform.GetComponent<Drone>();
				if ((bool)drone)
				{
					break;
				}
				transform = transform.parent;
				if (!transform)
				{
					break;
				}
			}
			if ((bool)drone)
			{
				m_transform_drone_lut[p_target] = drone;
			}
			return drone;
		}

		public MAGate GetGateParent(Component p_target)
		{
			if (m_transform_gate_lut == null)
			{
				m_transform_gate_lut = new Dictionary<Component, MAGate>();
			}
			if (m_transform_gate_lut.ContainsKey(p_target))
			{
				return m_transform_gate_lut[p_target];
			}
			Transform parent = p_target.transform;
			MAGate mAGate = null;
			int num = 8;
			while ((bool)parent)
			{
				mAGate = parent.GetComponent<MAGate>();
				if ((bool)mAGate)
				{
					break;
				}
				parent = parent.parent;
				if (!parent)
				{
					break;
				}
				num--;
				if (num <= 0)
				{
					break;
				}
			}
			if ((bool)mAGate)
			{
				m_transform_gate_lut[p_target] = mAGate;
			}
			return mAGate;
		}

		protected virtual void OnActionEvent(AssetActionEvent p_event)
		{
			MapAssetAction target = p_event.target;
			Drone playerDrone = base.game.model.playerDrone;
			int num = base.game.model.level.track.actions.IndexOf(target);
			_ = p_event.data;
			act_data_list.Clear();
			act_data_list.Add(num);
			if (num < 0)
			{
				Debug.LogWarning($"RaceController> OnActionEvent / Action [{target}] not found in track data!.", target);
				return;
			}
			if (target.tag == GameFlag.ActionBreakGlass)
			{
				act_data_list.Add(p_event.data);
				base.app.view.audio.PlayGlassBreak(target.gameObject);
			}
			base.game.model.replay.recorder.PushEvent(6, playerDrone, act_data_list.ToArray());
		}

		protected virtual void OnGateEvent(ColliderEvent.Type p_type, int p_gate_id, Drone p_drone)
		{
			ProcessGate(p_type, p_gate_id, p_drone);
		}

		public void ProcessGate(ColliderEventComponent p_gate, Drone p_drone)
		{
			if (model.IsComplete())
			{
				return;
			}
			bool flag = base.game.model.playerDrone == p_drone;
			int num = model.gates.IndexOf(p_gate);
			if (model.IsComplete(p_drone))
			{
				p_drone.invulnerable = 60f;
				Notify(1f / 12f, "game.race.gate@complete", num, model.gates.Count, p_gate, p_drone, model.time);
				return;
			}
			bool num2 = model.IncrementProgress(p_drone, p_gate);
			model.RefreshStandings();
			if (!num2)
			{
				return;
			}
			Notify(1f / 60f, "game.race.gate@step", num, model.gates.Count, p_gate, p_drone, model.time);
			int laps = base.game.model.level.track.laps;
			int value = base.game.model.level.track.GetLapIndex(num) + 1;
			value = Mathf.Clamp(value, 1, laps);
			if (laps > 1)
			{
				Notify(1f / 60f, "game.race.lap@step", value, laps, p_gate, p_drone, model.time);
			}
			if (flag)
			{
				base.game.model.replay.recorder.PushEvent(1, p_drone, num);
				tempGatesTime.Add(model.time);
				model.gateTimes = tempGatesTime;
				if (model.IsComplete(p_drone))
				{
					base.ui.hud.marker.Clear();
					model.raceActive = false;
					p_drone.invulnerable = 60f;
					Notify(1f / 12f, "game.race.gate@complete", num, model.gates.Count, p_gate, p_drone, model.time);
				}
				else
				{
					int progress = model.GetProgress(p_drone);
					MarkGate(progress);
				}
			}
		}

		public void ProcessGate(int p_gate_id, Drone p_drone)
		{
			if (p_gate_id >= 0 && p_gate_id < model.gates.Count)
			{
				ColliderEventComponent colliderEventComponent = model.gates[p_gate_id];
				if ((bool)colliderEventComponent)
				{
					ProcessGate(colliderEventComponent, p_drone);
				}
			}
		}

		protected void ProcessGate(ColliderEventComponent p_gate, Drone p_drone, bool p_process_last)
		{
			int progress = model.GetProgress(p_drone);
			if ((!p_process_last || progress >= model.gates.Count - 1) && (p_process_last || progress < model.gates.Count - 1))
			{
				ProcessGate(p_gate, p_drone);
			}
		}

		protected void ProcessGate(int p_gate_id, Drone p_drone, bool p_process_last)
		{
			_ = model.gates[p_gate_id];
			ProcessGate(p_gate_id, p_drone, p_process_last);
		}

		protected void ProcessGate(ColliderEvent.Type p_event_type, int p_gate_id, Drone p_drone)
		{
			ColliderEventComponent p_gate = model.gates[p_gate_id];
			switch (p_event_type)
			{
			case ColliderEvent.Type.Exit:
				ProcessGate(p_gate, p_drone, p_process_last: false);
				break;
			case ColliderEvent.Type.Enter:
				ProcessGate(p_gate, p_drone, p_process_last: true);
				break;
			}
		}

		public void MarkGate(int p_id)
		{
			if (p_id < 0)
			{
				base.ui.hud.marker.Clear();
				return;
			}
			if (p_id >= model.gates.Count)
			{
				base.ui.hud.marker.Clear();
				return;
			}
			ColliderEventComponent colliderEventComponent = model.gates[p_id];
			Component selection = colliderEventComponent.transform;
			if (colliderEventComponent.colliders.Count > 0)
			{
				selection = colliderEventComponent.colliders[0];
			}
			base.ui.hud.marker.SetSelection(selection);
		}

		public override void SetLeaderboard(Action<DRLLeaderboardData> p_callback, DroneRigData p_rig = null)
		{
			if (!model.raceComplete)
			{
				Debug.LogWarning("RaceControlller> SetLeaderboard - Trying to send leaderboard without completing the race.");
				return;
			}
			if (!base.app.model.storage.state.license.exists)
			{
				Debug.LogWarning("RaceControlller> SetLeaderboard - Demo Mode can't send leaderboards!");
				return;
			}
			bool has_tournament = base.app.inTournament;
			if (model.status != RaceStatusType.Success && !has_tournament)
			{
				Debug.LogWarning("RaceControlller> SetLeaderboard - Race Status Invalid [" + model.status.ToString() + "]");
				return;
			}
			if (DRLApp.offline)
			{
				Notify("game.race.leaderboard-set");
				return;
			}
			DRLMap map = base.app.scene.map;
			DRLMapTrack track = base.app.scene.track;
			MapData custom_map = (track ? track.map.data : null);
			Drone playerDrone = base.game.model.playerDrone;
			bool flag = base.app.model.network.room != null && base.app.model.network.room.Local != null;
			float t = (flag ? base.app.model.network.room.Local.RaceTime : base.game.model.playerData.raceTime);
			int crashes = model.crashes;
			float topSpeed = model.topSpeed;
			float[] lapTimes = model.lapTimes.ToArray();
			int fastestLapIndex = model.fastestLapIndex;
			int slowestLapIndex = model.slowestLapIndex;
			float distanceTraveled = model.distanceTraveled;
			float timeInFirstPlace = model.timeInFirstPlace;
			List<float> gateTimes = model.gateTimes;
			bool is_force = false;
			_ = base.app.arguments.game.mode;
			bool valid_context = base.game != null && base.game.model != null && base.game.model.playerData != null;
			if ((base.app.arguments.game.mode != GameFlag.NetworkMultiplayer || base.app.model.network.room == null || !base.app.model.network.room.IsTournamentMatch) && model.status != RaceStatusType.Success && model.status != RaceStatusType.Crash)
			{
				t = 180f;
			}
			ServiceModel sm = base.app.model.service;
			DroneRigData rd = p_rig;
			if (rd == null && valid_context)
			{
				rd = base.game.model.playerData.rig;
			}
			if (rd == null)
			{
				Debug.LogWarning("RaceControlller> SetLeaderboard - No drone rig!");
				return;
			}
			if (p_rig == null)
			{
				m_customPhysics = !playerDrone.IsCurrentPhysicsDefault && !has_tournament;
				rd.tune = (m_customPhysics ? playerDrone.physics.ToJson() : null);
				rd.profile = ((playerDrone.profile != null) ? playerDrone.profile.ToJson() : null);
			}
			else
			{
				m_customPhysics = rd.tune != null && !has_tournament;
			}
			DRLLeaderboardData lbd = new DRLLeaderboardData();
			lbd = ServiceModel.CreateRaceLeaderboardData(0, t, crashes, track);
			lbd.scoreCheat = (bool)base.app.acs && base.app.acs.cheatEver;
			lbd.scoreCheatRatio = (base.app.acs ? base.app.acs.avgRatio : 1f);
			lbd.scoreCheatSamples = (base.app.acs ? base.app.acs.GetSamplesString() : "");
			lbd.multiplayer = false;
			lbd.raceStatusFlag = model.status;
			lbd.batteryResistance = base.app.model.storage.state.player.settings.game.batteryResistance;
			lbd.raceId = model.raceId;
			if (base.app.arguments.game.mode == GameFlag.NetworkMultiplayer)
			{
				lbd.multiplayer = true;
				NetworkModel network = base.app.model.network;
				if ((bool)network && network.room != null)
				{
					lbd.multiplayerRoomId = network.room.Id;
					lbd.multiplayerRoomSize = model.racersCount;
					lbd.raceId = network.room.RaceId;
					if (network.room.Master != null)
					{
						lbd.multiplayerMasterId = network.room.Master.PlayerId;
					}
					if (network.room.Local != null)
					{
						lbd.multiplayerPlayerId = network.room.Local.ID.ToString();
						string playerId = network.room.Local.PlayerId;
						int playerPosition = model.GetPlayerPosition(playerId, p_ignore_ghosts: true);
						if (playerPosition >= 0)
						{
							lbd.multiplayerPlayerPosition = playerPosition + 1;
						}
					}
					if (network.room.IsTournamentMatch)
					{
						lbd.heatIdx = network.room.HeatIdx;
					}
				}
			}
			lbd.force = is_force;
			lbd.diameter = rd.diameter;
			lbd.droneName = rd.name;
			lbd.droneThumb = rd.thumb1;
			lbd.customPhysics = m_customPhysics && !has_tournament;
			lbd.droneRig = rd.ToJson();
			lbd.droneGuid = rd.guid;
			lbd.drlOfficial = (!m_customPhysics && base.app.model.storage.state.player.garage.IsOfficial(rd)) || has_tournament;
			lbd.drlPilotMode = base.app.model.storage.state.player.activeFCMode == FCMode.DRLPilot;
			lbd.tryouts = base.app.arguments.game.tryouts;
			lbd.topSpeed = topSpeed;
			lbd.crashCount = crashes;
			lbd.timeInFirst = timeInFirstPlace;
			lbd.totalDistance = distanceTraveled;
			lbd.lapTimes = lapTimes;
			lbd.fastestLap = fastestLapIndex;
			lbd.slowestLap = slowestLapIndex;
			lbd.gateTimes = gateTimes;
			if ((bool)base.game.model && (bool)playerDrone)
			{
				lbd.hash = base.game.model.playerDroneHash;
			}
			if (has_tournament)
			{
				lbd.matchId = base.app.arguments.game.tournamentMatchData.Id;
			}
			if (custom_map != null)
			{
				Debug.Log("RaceController> SetLeaderboard / Sending Custom Map / guid[" + custom_map.guid + "] title[" + custom_map.mapTitle.ToUpper() + "]");
			}
			if (1 == 0)
			{
				return;
			}
			sm.SetLeaderboard(lbd, delegate(DRLLeaderboardData p_result)
			{
				if (base.validContext)
				{
					if (p_result == null)
					{
						Debug.LogWarning("RaceController> SetLeaderboard - Failed to send results!");
						if (p_callback != null)
						{
							p_callback(null);
						}
					}
					else
					{
						DRLLeaderboardData lbr = p_result;
						DRLProgressionStateData progression = lbr.progression;
						List<string> list = new List<string>();
						list.AddRange(new string[10]
						{
							"Result: " + lbr.id,
							"Map: " + map.guid + "." + map.title,
							"Track: " + track.guid + "." + track.title,
							"CommunityMap: " + ((custom_map == null) ? "No" : (custom_map.mapTitle + "." + custom_map.guid)),
							"ProfileName: " + lbr.profileName,
							"Position: " + lbr.position,
							"Highscore: " + lbr.highscore,
							"Drone: " + rd.guid + "." + rd.name + "@" + lbr.diameter,
							"DroneCustom: " + lbr.customPhysics,
							"Multiplayer: " + lbr.multiplayer
						});
						if (lbr.multiplayer)
						{
							list.AddRange(new string[2]
							{
								"  RoomId: " + lbr.multiplayerRoomId,
								"  PlayerId: " + lbr.multiplayerPlayerId
							});
						}
						if (progression != null)
						{
							list.AddRange(new string[1] { "Progression:\n" + progression.ToJson(p_indented: true) });
						}
						model.playerPercentile = p_result.percentile;
						Debug.Log("RaceController> Leaderboard Success!\n" + string.Join("\n", list));
						byte[] parsed_replay = null;
						string id = lbr.id;
						Action finalize_leaderboard_result = null;
						bool will_parse_replay = true;
						ReplayRecorderController rrc = base.game.replay.recorder;
						Activity.RunOnce(delegate
						{
							if (valid_context && !(rrc == null))
							{
								if (will_parse_replay)
								{
									rrc.model.ToBytesAsync(delegate(byte[] p_replay_data)
									{
										parsed_replay = p_replay_data;
										finalize_leaderboard_result();
									});
								}
								else
								{
									finalize_leaderboard_result();
								}
							}
						}, 5f);
						finalize_leaderboard_result = delegate
						{
							OnReplayWriteData(parsed_replay);
							m_replayUploadStarted = true;
							Notify("game.race.replay-upload@start");
							int num = ((parsed_replay != null) ? parsed_replay.Length : 0);
							Debug.Log($"RaceController> Replay Parse success [{num} bytes] at {DateTime.Now.ToString()}");
							int replay_upload_count = ((parsed_replay != null) ? ((!lbr.highscore) ? 1 : 2) : 0);
							if (lbr.highscore && parsed_replay != null && parsed_replay.Length != 0)
							{
								sm.SetReplayRace(id, parsed_replay, map, track, rd.diameter, is_force, delegate(DRLReplayData[] p_replay_result)
								{
									Debug.Log("RaceController> SetReplaySuccess\n" + p_replay_result);
									replay_upload_count--;
									if (replay_upload_count <= 0)
									{
										replayUploadCompleted = true;
										Notify("game.race.replay-upload@complete");
										parsed_replay = null;
										GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
									}
									if (!lbr.multiplayer && !base.app.inCircuits && !base.app.inOnboarding)
									{
										Debug.Log("RaceContoller> SetLeaderboard - Refreshing Opponents");
										base.app.model.service.opponent.Refresh(delegate(bool p_success)
										{
											Debug.Log($"RaceContoller> SetLeaderboard / Refresh Complete - success[{p_success}]");
											if (p_success && ReplayFile.EnableVersion2)
											{
												base.app.arguments.game.RemoveGhostPlayers();
												ReplayRecord ghostRecordsV = base.app.model.service.opponent.ghostRecordsV2;
												int num2 = ((ghostRecordsV != null) ? ((ghostRecordsV.replays != null) ? ghostRecordsV.replays.Count : 0) : 0);
												Debug.Log($"RaceContoller> SetLeaderboard / Refresh Opponent Complete - ghosts-found[{num2}]");
												if (num2 > 0)
												{
													base.app.arguments.game.AddGhostPlayer(ghostRecordsV);
												}
											}
										});
									}
								});
							}
							if (!has_tournament)
							{
								if (parsed_replay != null)
								{
									sm.StorageReplayCloud(id, parsed_replay, delegate(string p_replay_url)
									{
										Debug.Log("RaceController> Replay Cloud Submission Complete [" + p_replay_url + "]");
										replay_upload_count--;
										if (replay_upload_count <= 0)
										{
											replayUploadCompleted = true;
											Notify("game.race.replay-upload@complete");
											parsed_replay = null;
											GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
											if (base.app.inCircuits)
											{
												base.app.model.storage.state.player.circuits.SetCircuitReplay(p_replay_url);
											}
										}
									});
								}
							}
							else
							{
								string p_custom_map = ((custom_map != null) ? custom_map.guid : "");
								if (parsed_replay != null)
								{
									sm.StorageReplayCloud(id, parsed_replay, map.guid, track.guid, p_custom_map, rd.diameter, t, lbd.matchId, delegate(string p_replay_url)
									{
										Debug.Log("RaceController> Replay Cloud Submission Complete [" + p_replay_url + "] Tournament[" + lbd.id + "] Match[" + lbd.matchId + "]");
										replay_upload_count--;
										if (replay_upload_count <= 0)
										{
											replayUploadCompleted = true;
											Notify("game.race.replay-upload@complete");
											parsed_replay = null;
											GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
										}
									});
								}
							}
							if (replay_upload_count <= 0)
							{
								replayUploadCompleted = true;
								Notify("game.race.replay-upload@complete");
								parsed_replay = null;
								GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
							}
						};
						if (p_callback != null)
						{
							p_callback(lbr);
						}
						Notify("game.race.leaderboard-set");
					}
				}
			});
		}

		protected override void OnReplayHeaderWrite()
		{
			base.OnReplayHeaderWrite();
			if (ReplayFile.EnableVersion2)
			{
				ReplayHeaderWriteV2();
			}
			else
			{
				ReplayHeaderWriteV1();
			}
		}

		private void ReplayHeaderWriteV2()
		{
			GameModel gameModel = base.game.model;
			Drone playerDrone = gameModel.playerDrone;
			ReplayFile replay = base.game.replay.recorder.model.GetReplay(playerDrone);
			if (replay == null)
			{
				Debug.LogWarning("RaceController> ReplayHeaderWriteV2 / Replay filenot found!");
				return;
			}
			Debug.Log("ReplayPrototypeController> ReplayHeaderWriteV2 / Header Write");
			DRLMap map = base.app.scene.map;
			DRLMapTrack track = base.app.scene.track;
			bool flag = map.data != null;
			PlayerStateModel player = base.app.model.storage.state.player;
			GamePlayerData playerData = base.game.model.playerData;
			FCProfileData active = player.settings.tuning.GetActive();
			int num = playerData?.order ?? 0;
			ReplayHeader header = replay.header;
			header.Clear();
			header.title = "Clip";
			header.isMultiplayer = false;
			header.isPlayer = true;
			header.mapGUID = map.guid;
			header.trackGUID = track.guid;
			header.isCustomMap = flag;
			header.customMapGUID = (flag ? map.data.guid : "");
			header.platformId = player.profile.platformId;
			header.playerId = player.profile.playerId;
			header.profileName = player.profile.username;
			header.profileColorHex = player.profile.colorHex;
			header.profilePhoto = player.profile.photoURL;
			header.raceTime = model.time;
			header.gameTypeFlag = gameModel.type;
			header.controllerTypeFlag = RCI.GetControllerStateType(ControllerStateType.Taranis);
			header.cameraTilt = playerDrone.body.frame.camera.tilt;
			header.cameraFOV = playerDrone.body.frame.camera.fov;
			header.podiumGUID = player.podiumId;
			header.SetDroneRig(playerData.rig);
			header.SetFCProfile(active);
			header.SetPhysicsTune(playerDrone.physics);
			header.order = ((num >= 0) ? num : 0);
			header.profileTournamentColor2Hex = player.profile.colorHex;
			if (!base.app.arguments.game.isTournamentActive)
			{
				return;
			}
			DRLTournamentData tournamentData = base.app.arguments.game.tournamentData;
			bool isTournamentMatchActive = base.app.arguments.game.isTournamentMatchActive;
			if (isTournamentMatchActive)
			{
				string playerId = base.game.model.playerData.playerId;
				DRLTournamentMatchData tournamentMatchData = base.app.arguments.game.tournamentMatchData;
				DRLTournamentPlayerData playerById = tournamentMatchData.GetPlayerById(playerId);
				Debug.Log($"RaceController> ReplayHeaderWriteV2 / Tournament Active - guid[{tournamentData.guid}] name[{tournamentData.title}] has-match[{isTournamentMatchActive}]" + " " + playerById.profileColorHex + " " + player.profile.colorHex);
				if (playerById == null)
				{
					Debug.LogWarning("RaceController> ReplayHeaderWriteV2 / Player Color Not Found / player-id[" + playerId + "] match-id[" + tournamentMatchData.Id + "] round-id[" + tournamentMatchData.roundId + "]");
				}
				header.profileTournamentColorHex = ((playerById == null) ? player.profile.colorHex : playerById.profileColorHex);
				header.profileTournamentColor2Hex = ((playerById == null) ? Colorf.ToRGBHex(playerData.color2) : playerById.profileColor2Hex);
			}
		}

		private void ReplayHeaderWriteV1()
		{
			Drone playerDrone = base.game.model.playerDrone;
			BlackboxData data = base.game.replay.recorder.model.GetData(playerDrone);
			if (data == null)
			{
				Debug.LogWarning("RaceController> Drone BlackboxData not found!");
				return;
			}
			Debug.Log("RaceController> OnReplayComplete");
			PlayerStateModel player = base.app.model.storage.state.player;
			GamePlayerData playerData = base.game.model.playerData;
			SerializedData header = data.header;
			string value = "Clip";
			switch (base.game.model.type)
			{
			case GameFlag.Campaign:
			{
				value = GetRaceTitle();
				CampaignController campaignController = this as CampaignController;
				header["campaign"] = (campaignController ? campaignController.model.campaign.guid : "");
				header["campaign-race"] = (campaignController ? campaignController.GetCurrentRaceIndex() : (-1));
				break;
			}
			case GameFlag.Race:
				value = "Clip";
				break;
			case GameFlag.Mission:
				value = "Clip";
				break;
			}
			DRLMap map = base.app.scene.map;
			DRLMapTrack track = base.app.scene.track;
			bool flag = map.data != null;
			header["title"] = value;
			header["multiplayer"] = base.game.model.mode == GameFlag.NetworkMultiplayer;
			header["player"] = true;
			header["map"] = map.guid;
			header["track"] = track.guid;
			header["is-custom-map"] = flag;
			header["custom-map"] = (flag ? map.data.guid : "");
			header[DRLService.PlatformIdKey] = player.profile.platformId;
			header["player-id"] = player.profile.playerId;
			header["profile-name"] = player.profile.username;
			header["profile-color"] = player.profile.colorHex;
			header["profile-photo"] = player.profile.photoURL;
			header["race-time"] = model.time;
			header["race-cheat"] = (bool)base.app.acs && base.app.acs.cheatEver;
			header["race-cheat-ratio"] = (base.app.acs ? base.app.acs.avgRatio : 1f);
			header["race-cheat-samples"] = (base.app.acs ? base.app.acs.GetSamplesString() : "");
			header["game-type"] = (int)base.game.model.type;
			header["controller-type"] = (int)RCI.GetControllerStateType(ControllerStateType.Taranis);
			header["camera-tilt"] = playerDrone.body.frame.camera.tilt;
			header["camera-fov"] = playerDrone.body.frame.camera.fov;
			header["podium-id"] = player.podiumId;
			header["drone-rig"] = ((playerData.rig == null) ? "" : playerData.rig.ToJson());
			if (base.app.arguments.game.isTournamentActive)
			{
				DRLTournamentData tournamentData = base.app.arguments.game.tournamentData;
				bool isTournamentMatchActive = base.app.arguments.game.isTournamentMatchActive;
				Debug.Log($"RaceController> OnReplayHeaderWrite / Tournament Active - guid[{tournamentData.guid}] name[{tournamentData.title}] has-match[{isTournamentMatchActive}]");
				if (isTournamentMatchActive)
				{
					string playerId = base.game.model.playerData.playerId;
					DRLTournamentMatchData tournamentMatchData = base.app.arguments.game.tournamentMatchData;
					DRLTournamentPlayerData playerById = tournamentMatchData.GetPlayerById(playerId);
					if (playerById == null)
					{
						Debug.LogWarning("RaceController> OnReplayHeaderWrite / Player Color Not Found / player-id[" + playerId + "] match-id[" + tournamentMatchData.Id + "] round-id[" + tournamentMatchData.roundId + "]");
					}
					header["profile-tournament-color"] = ((playerById == null) ? player.profile.colorHex : playerById.profileColorHex);
				}
			}
			FCProfileData active = player.settings.tuning.GetActive();
			header["fc-profile"] = ((active == null) ? "" : active.ToJson());
			header["physics-tune"] = ((playerDrone.physics == null) ? "" : playerDrone.physics.ToJson());
			int num = playerData?.order ?? 0;
			header["order"] = ((num >= 0) ? num : 0);
			data.header = header;
		}

		protected new virtual void OnReplayComplete()
		{
		}

		protected override void OnReplayWrite()
		{
			if (!base.app.model.game)
			{
				return;
			}
			string folder = (base.app.model.game.fromEditor ? DRLPaths.Storage.replaysMapEditorRoot : DRLPaths.Storage.replaysRoot);
			string hash = base.app.hash;
			if (base.game.model.type == GameFlag.Campaign)
			{
				CampaignController campaignController = this as CampaignController;
				if ((bool)campaignController && (bool)campaignController.model.campaign)
				{
					hash = hash + "_" + campaignController.model.campaign.guid;
				}
			}
			Debug.Log("RaceController> OnReplayWrite [" + folder + hash + "]");
			_ = base.app.model.service;
			base.game.replay.recorder.model.ToBytesAsync(delegate(byte[] fd)
			{
				string text = (ReplayFile.EnableVersion2 ? "rpl2" : "replay");
				File.WriteAllBytes(folder + hash + "." + text + ".bytes", fd);
			});
		}

		protected new virtual void OnReplayWriteData(byte[] p_replay_data)
		{
		}

		protected virtual void ResetGhostDrones(bool p_write_podium)
		{
			DroneSimulation simulation = base.game.model.simulation;
			simulation.transmitters.ResetGhostDrones();
			simulation.transmitters.SetPhysicsOnComplete(p_flag: true);
			if (!p_write_podium)
			{
				return;
			}
			for (int i = 0; i < simulation.drones.list.Count; i++)
			{
				Drone p_drone = simulation.drones.list[i];
				DroneGhostTransmitter byDrone = simulation.transmitters.GetByDrone<DroneGhostTransmitter>(p_drone);
				if ((bool)byDrone)
				{
					if ((bool)byDrone.drone)
					{
						byDrone.drone.rigidbody.SetCollisionEnabled(p_flag: false);
					}
					if (i < simulation.podiums.list.Count)
					{
						byDrone.podium = simulation.podiums.list[i].spawn.position;
					}
				}
			}
		}

		protected void ResetGhostDrones()
		{
			ResetGhostDrones(p_write_podium: true);
		}

		protected void ForceCompleteGhostDrones()
		{
			DroneSimulation simulation = base.game.model.simulation;
			simulation.transmitters.SetEnabled<DroneGhostTransmitter>(p_flag: false);
			simulation.transmitters.SetPhysicsOnComplete(p_flag: false);
			simulation.transmitters.FindAll<DroneGhostTransmitter>().Sort((DroneGhostTransmitter a, DroneGhostTransmitter b) => (!(a.raceTime < b.raceTime)) ? 1 : (-1));
			for (int num = 0; num < model.Rankings.Count; num++)
			{
				if (model.Rankings[num].type == GamePlayerType.Ghost)
				{
					model.Rankings[num].raceStatus = RaceStatusType.Success;
				}
			}
		}

		public void TournamentUpdate(Action p_callback = null)
		{
			if (base.app.arguments.game.mode != GameFlag.SinglePlayer || base.app.inMultiplayer || base.app.arguments.game.tournamentData == null)
			{
				return;
			}
			string guid = base.app.arguments.game.tournamentData.guid;
			if (string.IsNullOrEmpty(guid))
			{
				Debug.Log("NetworkRaceController> No Tournament available - skip");
				return;
			}
			int p_order = 0;
			DRLMap map = base.app.arguments.game.map;
			DRLMapTrack track = base.app.arguments.game.track;
			string text = "";
			GamePlayerData playerDataById = base.game.model.GetPlayerDataById(base.game.model.playerData.playerId);
			List<DRLRaceResultData> list = new List<DRLRaceResultData>();
			string p_player_id = (string.IsNullOrEmpty(playerDataById.playerId) ? "" : playerDataById.playerId);
			int p_score = Mathf.FloorToInt(playerDataById.raceTime * 1000f);
			int p_crashes = 0;
			DRLRaceResultData dRLRaceResultData = base.app.model.storage.state.player.results.Create(guid, map, track, p_order, p_player_id, p_score, p_crashes);
			dRLRaceResultData.status = ResultStatusType.Success;
			dRLRaceResultData.matchId = base.app.arguments.game.tournamentMatchData.Id;
			dRLRaceResultData.heat = 0;
			dRLRaceResultData.gateTimes = model.gateTimes;
			dRLRaceResultData.raceId = model.raceId;
			switch (playerDataById.raceStatus)
			{
			case RaceStatusType.Success:
				dRLRaceResultData.status = ResultStatusType.Success;
				break;
			case RaceStatusType.Timeout:
				dRLRaceResultData.status = ResultStatusType.Timeout;
				break;
			case RaceStatusType.Crash:
				dRLRaceResultData.status = ResultStatusType.Crash;
				break;
			case RaceStatusType.Quit:
				dRLRaceResultData.status = ResultStatusType.Quit;
				break;
			}
			if (dRLRaceResultData.status != ResultStatusType.Success && dRLRaceResultData.status != ResultStatusType.Crash)
			{
				playerDataById.raceTime = 180f;
				dRLRaceResultData.score = Mathf.FloorToInt(playerDataById.raceTime * 1000f);
			}
			text = text + dRLRaceResultData.ToJson() + "\n";
			list.Add(dRLRaceResultData);
			Debug.Log("RaceController> SetResults - tournament[" + guid + "]\n" + text);
			SendTournamentResults(guid, list.ToArray(), p_callback);
		}

		protected void SendTournamentResults(string p_guid, DRLRaceResultData[] p_results, Action p_callback = null)
		{
			if (m_has_sent_tournament_results)
			{
				return;
			}
			base.app.model.service.SetTournamentResults(p_guid, p_results, delegate(DRLServiceResult p_result)
			{
				if (base.validContext)
				{
					if (!p_result.success && tournamentPostRetry < 5)
					{
						tournamentPostRetry++;
						Debug.LogWarning($"RaceController> SendTournamentResults / FAIL - Retry {tournamentPostRetry}...");
						SendTournamentResults(p_guid, p_results);
					}
					else
					{
						Debug.Log("RaceController> SendTournamentResults / SUCCESS!");
						Notify(1f / 6f, "game.tournament.results@submit");
						m_has_sent_tournament_results = true;
						if (p_callback != null)
						{
							p_callback();
						}
					}
				}
			});
		}

		private void StartCountdownTimer(float f, string p_roundName = "", bool is_spectator = false)
		{
			if (!(f <= 0f))
			{
				if (!is_spectator && IsQualifierRound())
				{
					base.ui.hud.timeout.StartCountdown(f);
				}
				QuitTournamentMatch(p_roundName, f);
			}
		}

		private void ClearTournamentTimer()
		{
			if (tournamentMatchTimer != null)
			{
				tournamentMatchTimer.Stop();
				tournamentMatchTimer.manager.Remove(tournamentMatchTimer);
				tournamentMatchTimer = null;
			}
		}

		public bool IsQualifierRound()
		{
			if (!base.app.inTournament)
			{
				return false;
			}
			if (base.app.model.tournament == null || base.app.model.tournament.activeRound == null)
			{
				return false;
			}
			return base.app.model.tournament.activeRound.gameMode == TournamentRoundGameMode.leaderboard;
		}

		protected void QuitTournamentMatch(string p_roundName, float p_delay = 0f, bool p_forceQuit = false)
		{
			UIGame gmUI = base.app.view.ui.game;
			tournamentMatchTimer = this.TimerRunOnce(delegate
			{
				Notify("tournament.match.complete");
				if (!(gmUI == null))
				{
					gmUI.hud.timeout.StopCountdown();
					if (!model.raceComplete || p_forceQuit)
					{
						model.raceComplete = true;
						model.raceActive = false;
						model.status = RaceStatusType.Timeout;
						if (!(base.app.view.ui.screens.current != null) || p_forceQuit)
						{
							Pause(p_flag: true, p_pause_physics: true, p_open_pause_screen: false);
							base.app.view.ui.screens.Open<UITournamentRaceEndsView>("tournament-race-ends-screen").SetTitle(p_roundName, p_forceQuit);
							this.TimerRunOnce(delegate
							{
								base.app.view.ui.screens.Open<UITournamentBracketsView>("tournament-brackets-screen").backButtonEnabled = false;
							}, 3f);
						}
					}
				}
			}, p_delay + 0.1f);
		}

		private void OnDisable()
		{
			ClearTournamentTimer();
		}
	}
}

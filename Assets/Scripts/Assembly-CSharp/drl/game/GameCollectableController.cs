using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using drl.backend;
using drl.sim;
using drl.sim.rci;
using thelab.core;

namespace drl.game
{
	public class GameCollectableController : GameTypeController
	{
		private bool m_customPhysics;

		public bool replayUploadCompleted;

		public bool pauseOnCollect;

		private bool m_replayUploadStarted;

		private Activity m_game_complete_timer;

		private new MonoActivity m_replay_stop_timer;

		private bool canReset;

		public GameCollectableModel model => AssertLocal<GameCollectableModel>("model");

		public bool customPhysics => m_customPhysics;

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			base.OnNotification(p_event, p_target, p_data);
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "game.simulation.drone.all@ready":
				break;
			case "game.pause":
				_ = base.game.model.simulation;
				break;
			case "game.unpause":
				_ = base.game.model.simulation;
				break;
			case "game.ready":
				model.status = RaceStatusType.Idle;
				break;
			case "game.boot":
				base.game.effects.Warmup(20);
				model.BuildTrack(base.app.model.game.level.track.rootMap);
				break;
			case "game.count@step":
			{
				int num = Reflection<object>.Get<int>(p_data, 0);
				int p_max = Reflection<object>.Get<int>(p_data, 1);
				Reflection<object>.Get((IList)p_data, 2, true);
				bool p_hide_title = Reflection<object>.Get((IList)p_data, 3, num == 2);
				if (!GameTypeController.ignoreCount)
				{
					ApplyCount(num, p_max, p_play_audio: true, p_hide_title);
				}
				break;
			}
			case "game.count@complete":
				if (!GameTypeController.ignoreCount)
				{
					OnCountComplete();
				}
				break;
			case "game.simulation.drone@ready":
			{
				Drone drone = Reflection<object>.Get<Drone>(p_data, 0);
				if ((bool)drone)
				{
					drone.renderer.SetTrailsDuration(0.2f);
					if (!(base.game.model == null) && !(base.game.model.playerDrone != drone))
					{
						drone.rigidbody.rb.constraints = RigidbodyConstraints.FreezeRotation;
					}
				}
				break;
			}
			case "game.simulation.drone@flip":
				model.crashes++;
				Debug.Log("GameCollectableController> Flip");
				break;
			case "game.simulation.drone@crash":
				Debug.Log("GameCollectableController> Crash");
				OnGameComplete(model.time, RaceStatusType.Crash);
				break;
			case "garage.edit.fly.ready":
				RequestRaceReset();
				break;
			}
		}

		protected override void LoadDrones()
		{
			List<GamePlayerData> players = base.game.model.players;
			for (int i = 0; i < players.Count; i++)
			{
				GamePlayerData p_player = players[i];
				CreatePlayer(p_player, model.rig);
			}
		}

		protected override void PlayIntroAnimation()
		{
			if (!PlayTrackAnimation() && !PlayPodiumAnimation())
			{
				Debug.LogWarning("GameCollectableController> Failed to play intro animation!");
			}
		}

		protected override void OnIntroAnimationComplete()
		{
			StopIntroAnimations();
			bool controllerOverlay = base.app.model.storage.state.player.settings.game.controllerOverlay;
			base.ui.hud.controller.fade.Fade(controllerOverlay ? 1f : (-0.1f));
			bool hotkeys = base.app.model.storage.state.player.settings.game.hotkeys;
			base.app.view.ui.game.hud.SetHotkeysEnabled(hotkeys);
		}

		public void InitializeGame()
		{
			if (base.validContext)
			{
				DroneSimulation simulation = base.game.model.simulation;
				Drone playerDrone = base.game.model.playerDrone;
				model.playerDrone = playerDrone;
				DroneCamera droneCamera = simulation.cameras.Get(0);
				base.app.view.audio.ResetGameRadioSignal(playerDrone ? playerDrone.gameObject : null);
				base.game.PodiumResetAll();
				UnfreezeDrones();
				SetDroneFCMode(playerDrone, base.app.model.storage.state.player.activeFCMode);
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
				Restore();
				base.ui.hud.controller.fade.alpha = -0.1f;
				base.ui.hud.counter.fade.alpha = -0.1f;
				base.ui.hud.counter.Clear();
				base.ui.hud.counter.fade.FadeIn(0.8f);
				base.ui.hud.gameTitle.Clear();
				base.ui.hud.gameTitle.fade.alpha = 1f;
				SetTitle();
				base.ui.hud.collectables.Clear();
				model.ResetScore();
				model.ResetTime();
				base.ui.hud.collectables.SetScoreTotal(model.total);
				_ = base.app.model.storage.state.player.settings.game;
				model.countActive = true;
				model.status = RaceStatusType.Idle;
				base.game.ClearAllActivities();
				ReplayInit();
			}
		}

		protected override void OnGameReady()
		{
			DroneSimulation simulation = base.game.model.simulation;
			DroneCamera droneCamera = simulation.cameras.Get(0);
			simulation.drones.SetArmed(p_flag: true);
			Drone drone = simulation.drones.Get(0);
			SetDroneFCMode(drone, base.app.model.storage.state.player.activeFCMode);
			droneCamera.SetFPV(drone);
			base.app.view.audio.ResetGameRadioSignal(drone.gameObject);
			base.game.model.level.radio.boundsSignal = 1f;
			droneCamera.fx.radio = 1f;
			base.ui.hud.Fade(1f, 0.5f, 1f);
			base.ui.hud.Fade(1f, 0.5f, 1f);
			InitializeGame();
			StartCount();
		}

		protected override void OnCountComplete()
		{
			base.OnCountComplete();
			DroneSimulation simulation = base.game.model.simulation;
			UnfreezeDrones();
			simulation.drones.SetReceiver(p_flag: true);
			base.game.ui.hud.alpha = 1f;
			base.game.ui.hud.collectables.fade.alpha = 1f;
			resetInProgress = false;
			model.ResetTime();
			model.countActive = false;
			model.gameActive = true;
			model.gameComplete = false;
			model.status = RaceStatusType.Running;
			base.game.replay.recorder.Stop();
			base.game.replay.recorder.Record();
			canReset = true;
		}

		public override bool OnGameCommand(GameCommand p_command)
		{
			if (p_command == null)
			{
				return true;
			}
			switch (p_command.type)
			{
			case GameCommandType.ResetDronePodium:
				Debug.Log("GameCollectableController> " + p_command.type);
				if (!base.game.model.paused)
				{
					if (base.app.view.ui.screens.current == null)
					{
						RequestRaceReset();
					}
					else if (m_replayUploadStarted || model.status == RaceStatusType.Crash)
					{
						Notify("game.race.request-restart");
					}
				}
				break;
			case GameCommandType.Pause:
				Debug.Log("Count Active: " + model.countActive);
				if (model.countActive)
				{
					return false;
				}
				Debug.Log("Game Complete: " + model.gameComplete);
				if (model.gameComplete)
				{
					return false;
				}
				base.game.SetGCEnabled(!base.game.model.paused);
				if (base.app.view.ui.game.hud.dashboard.isShowing)
				{
					Notify("game.ui.dashboard@hide");
					base.app.view.ui.game.hud.dashboard.openedFromPause = false;
					base.app.view.ui.game.hud.dashboard.openingAnotherScreen = false;
					return false;
				}
				break;
			}
			return base.OnGameCommand(p_command);
		}

		public void OnCollectableEvent(ColliderEvent p_event, CollectableView p_target, Drone p_drone)
		{
			if (p_event.type != ColliderEvent.Type.Enter)
			{
				return;
			}
			switch (p_target.collectable.collectableMode)
			{
			case MapCollectableMode.Regular:
			{
				Collect(p_target, p_drone);
				int num = 0;
				int num2 = 0;
				for (int i = 0; i < model.trackCollectables.Count; i++)
				{
					CollectableView collectableView = model.trackCollectables[i];
					if (collectableView.collectable.collectableMode == MapCollectableMode.Regular)
					{
						num++;
						if (collectableView.isDestroyed)
						{
							num2++;
						}
					}
				}
				if (model.IsCollectableComplete())
				{
					OnGameComplete(model.time, RaceStatusType.Success);
				}
				break;
			}
			case MapCollectableMode.Kill:
				Bogey(p_target, p_drone);
				base.game.model.playerDrone.Crash();
				OnGameComplete(model.time, RaceStatusType.Crash);
				break;
			}
			MapCollectableMode collectableMode = p_target.collectable.collectableMode;
			if ((uint)(collectableMode - 1) <= 1u && !(model.playerDrone != p_drone))
			{
				base.game.model.replay.recorder.PushEvent(5, p_drone, p_target.collectable.index);
			}
		}

		private void Collect(CollectableView p_target, Drone p_drone)
		{
			PlayEffect(p_target, p_drone);
			model.UpdateScore(1);
			base.ui.hud.collectables.SetScore(model.score);
			p_target.Destroy();
		}

		private void Bogey(CollectableView p_target, Drone p_drone)
		{
			PlayEffect(p_target, p_drone);
			p_target.Destroy();
		}

		private void Restore()
		{
			for (int i = 0; i < model.trackCollectables.Count; i++)
			{
				model.trackCollectables[i].Restore();
			}
		}

		public void PlayEffect(CollectableView p_target, Drone p_drone)
		{
			if (!p_drone)
			{
				return;
			}
			if (!p_target)
			{
				return;
			}
			DroneCamera target = p_drone.body.frame.camera.target;
			if ((bool)target && (bool)target.main)
			{
				float magnitude = p_drone.rigidbody.rb.velocity.magnitude;
				_ = target.main.transform.position + target.main.transform.forward * magnitude * 0.1f;
				switch (p_target.collectable.collectableMode)
				{
				case MapCollectableMode.Regular:
					p_target.PlayEffects(target.main);
					base.app.view.audio.PlayGameBalloon(p_target.effects.gameObject);
					break;
				case MapCollectableMode.Kill:
					p_target.PlayEffects(target.main);
					base.app.view.audio.PlayGameBalloon(p_target.effects.gameObject);
					break;
				}
			}
		}

		protected void OnGameComplete(float p_time, RaceStatusType p_state)
		{
			canReset = false;
			if (model.gameComplete)
			{
				return;
			}
			model.gameComplete = true;
			model.gameActive = false;
			base.game.model.playerData.raceTime = p_time;
			model.status = p_state;
			Drone playerDrone = base.game.model.playerDrone;
			GamePlayerData playerData = base.game.model.GetPlayerData(playerDrone);
			playerData.replay = base.game.replay.recorder.model.GetData(playerDrone);
			playerData.replayV2 = base.game.replay.recorder.model.GetReplay(playerDrone);
			switch (p_state)
			{
			case RaceStatusType.Crash:
				base.app.view.audio.PlayGameRaceFailure();
				break;
			case RaceStatusType.Success:
				base.app.view.audio.PlayGameGateFinalValid();
				break;
			}
			Debug.Log($"GameCollectableController> OnGameComplete\nstate[{p_state}] time[{model.time}][{Format.SecondsToTime(model.time, 2, p_use_ms: true)}]");
			CheatAssert();
			DroneCamera camera = base.game.model.camera;
			switch (p_state)
			{
			case RaceStatusType.Success:
				model.status = RaceStatusType.Success;
				Debug.Log("GameCollectableController> RaceStatusType.Success");
				break;
			case RaceStatusType.Crash:
				if ((bool)camera)
				{
					camera.fx.ExposureSaturation(4f, 0.5f, 0.5f, 2f, 0.5f);
				}
				break;
			default:
				if ((bool)camera)
				{
					camera.fx.ExposureGrayscale(p_flag: true, 3f);
				}
				break;
			}
			EnableRaceEndSlowmotion(playerDrone, 2f, p_centered: true, p_state);
			Notify("game.race.slowmo@start", playerDrone, 4f);
			if (playerDrone != null)
			{
				Notify("game.simulation.drone.flight-time@update", playerDrone.rig);
				if (!playerDrone.hasFc)
				{
					return;
				}
				playerDrone.fc.allowThrottle = false;
				playerDrone.fc.allowPitch = false;
				playerDrone.fc.allowRoll = false;
				playerDrone.fc.allowYaw = false;
			}
			base.ui.hud.collectables.fade.alpha = -0.1f;
			base.ui.hud.controller.fade.alpha = -0.1f;
			base.app.view.ui.game.hud.Hide();
			OnReplayHeaderWrite();
		}

		protected override void OnRaceSlowmotionStop()
		{
			if (!base.validContext || !model.gameComplete)
			{
				return;
			}
			Debug.Log("GameCollectableController> OnRaceSlowmotionStop");
			PlayerStateModel player = base.app.model.storage.state.player;
			Drone d = base.game.model.playerDrone;
			GamePlayerData playerData = base.game.model.playerData;
			if (playerData == null)
			{
				Debug.LogWarning("GameCollectableController> OnRaceSlowmotionStop / PlayerData is <null>");
			}
			if (model.status == RaceStatusType.Success)
			{
				_ = playerData?.raceTime;
			}
			_ = model.status;
			_ = 1;
			bool flag = model.status == RaceStatusType.Success;
			base.app.view.audio.StopAllGameAudio();
			base.app.view.audio.PlayMusicPostGame();
			base.game.SetGCEnabled(p_flag: true);
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
						if (base.validContext && (bool)d)
						{
							Debug.Log("GameCollectableController> OnRaceSlowmotionStop / Disable local drone. Assume it's landed");
							d.fc.armed = false;
							d.SetMotorSpinSpeed(0f);
							Notify("game.race.slowmo@stop");
						}
					}, 2f, unscaledTime: true);
				}
			}, 2f);
			UICollectablesCompleteView uICollectablesCompleteView = base.app.view.ui.screens.Open<UICollectablesCompleteView>("collectables-complete-screen");
			if ((bool)uICollectablesCompleteView)
			{
				uICollectablesCompleteView.collectable = this;
				uICollectablesCompleteView.headerPhoto = player.profile.photo;
				uICollectablesCompleteView.showStandings = false;
				uICollectablesCompleteView.replayUploadStarted = false;
				uICollectablesCompleteView.Fade(p_flag: true, player.profile.username.ToUpper(), playerData.raceTime, model.score, model.total, 0.25f, flag);
				uICollectablesCompleteView.willSetLeaderboard = flag && !base.game.model.fromEditor;
			}
		}

		protected virtual void UpdateRace()
		{
			if (base.ui.hud.dashboard.isShowing && !base.app.inMultiplayer)
			{
				base.ui.hud.collectables.fade.alpha = -0.1f;
				base.ui.hud.marker.fade.alpha = -0.1f;
				base.ui.hud.controller.fade.alpha = -0.1f;
				return;
			}
			Drone playerDrone = base.game.model.playerDrone;
			if ((bool)playerDrone && playerDrone.ready)
			{
				model.topSpeed = (playerDrone ? Mathf.Max(model.topSpeed, playerDrone.fc.sensor.inertial.groundSpeedKph) : 0f);
			}
			float deltaTime = GetDeltaTime();
			model.time += deltaTime;
			base.ui.hud.collectables.time = model.time;
			if (base.ui.hud.collectables != null && base.ui.hud.collectables.speed != null)
			{
				base.ui.hud.collectables.SetSpeed(playerDrone.fc.sensor.inertial.groundSpeedKph);
			}
			if (model.time >= 240f && GarbageCollector.GCMode == GarbageCollector.Mode.Disabled)
			{
				Debug.LogWarning("RaceController> UpdateRace / GC Offline Timeout Enable and Collect!");
				base.game.SetGCEnabled(p_flag: true);
			}
		}

		protected virtual void RequestRaceReset()
		{
			if (!canReset || resetInProgress)
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
			}
			Notify("game.ui.dashboard@hide");
			base.app.view.ui.game.hud.dashboard.openedFromPause = false;
			base.app.view.ui.game.hud.dashboard.openingAnotherScreen = false;
			base.game.ui.hud.collectables.fade.alpha = 0f;
			base.game.model.simulation.transmitters.RemoveGhostDrones();
			model.ResetScore();
			base.ui.hud.collectables.SetScore(model.score);
			if (base.app.arguments.game.opponentType == OpponentModeType.Off)
			{
				RunOnce(0.02f, delegate
				{
					RaceReset();
				});
				return;
			}
			model.countActive = true;
			model.gameActive = false;
			model.gameComplete = false;
			base.ui.hud.collectables.fade.alpha = -0.1f;
			base.ui.hud.controller.fade.alpha = -0.1f;
			base.ui.hud.counter.fade.alpha = -0.1f;
			base.ui.hud.counter.Clear();
			base.game.replay.recorder.model.Clear();
			_ = base.app.model.service;
			RunOnce(0.02f, delegate
			{
				RaceReset();
			});
		}

		public void RaceReset()
		{
			Drone d = base.game.model.playerDrone;
			if (!d)
			{
				return;
			}
			GameStateModel gsm = base.app.model.storage.state.player.settings.game;
			d.receiver.ClearSignal();
			d.receiver.enabled = false;
			d.fc.armed = false;
			base.app.view.ui.fade.FadeIn(0.5f);
			model.countActive = true;
			model.gameActive = false;
			model.gameComplete = false;
			base.app.view.audio.PlayUILevelRestart();
			base.game.SetTabScreenEnabled(p_flag: false);
			base.ui.hud.physics.Hide();
			RunOnce(delegate
			{
				InitializeGame();
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

		public override string GetRaceTitle()
		{
			return "Game Complete!";
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

		public override void OnDroneScrape(DroneEvent p_event)
		{
			if ((bool)base.game.model.replay && (bool)base.game.model.replay.recorder)
			{
				base.game.model.replay.recorder.PushEvent(2, p_event.target);
			}
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
				Debug.LogWarning("GameCollectableController> ReplayHeaderWriteV2 / Replay filenot found!");
				return;
			}
			Debug.Log("GameCollectableController> ReplayHeaderWriteV2 / Header Write");
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
			FCProfileData active = player.settings.tuning.GetActive();
			header["fc-profile"] = ((active == null) ? "" : active.ToJson());
			header["physics-tune"] = ((playerDrone.physics == null) ? "" : playerDrone.physics.ToJson());
			int num = playerData?.order ?? 0;
			header["order"] = ((num >= 0) ? num : 0);
			data.header = header;
		}

		public override void SetLeaderboard(Action<DRLLeaderboardData> p_callback, DroneRigData p_rig = null)
		{
			if (!model.gameComplete)
			{
				Debug.LogWarning("GameCollectableController> SetLeaderboard - Trying to send leaderboard without completing the game.");
				return;
			}
			if (!base.app.model.storage.state.license.exists)
			{
				Debug.LogWarning("GameCollectableController> SetLeaderboard - Demo Mode can't send leaderboards!");
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
			float p_time = 0f;
			if (model.status == RaceStatusType.Success)
			{
				p_time = model.time;
			}
			int crashes = model.crashes;
			float topSpeed = model.topSpeed;
			float distanceTraveled = model.distanceTraveled;
			bool is_force = false;
			_ = base.app.arguments.game.mode;
			bool valid_context = base.game != null && base.game.model != null && base.game.model.playerData != null;
			ServiceModel sm = base.app.model.service;
			DroneRigData rd = p_rig;
			if (rd == null && valid_context)
			{
				rd = base.game.model.playerData.rig;
			}
			if (rd == null)
			{
				Debug.LogWarning("GameCollectableController> SetLeaderboard - No drone rig!");
				return;
			}
			if (p_rig == null)
			{
				m_customPhysics = !playerDrone.IsCurrentPhysicsDefault;
				rd.tune = (m_customPhysics ? playerDrone.physics.ToJson() : null);
				rd.profile = ((playerDrone.profile != null) ? playerDrone.profile.ToJson() : null);
			}
			else
			{
				m_customPhysics = rd.tune != null;
			}
			DRLLeaderboardData dRLLeaderboardData = new DRLLeaderboardData();
			dRLLeaderboardData = ServiceModel.CreateCollectablesLeaderboardData(0, p_time, crashes, track);
			dRLLeaderboardData.scoreCheat = (bool)base.app.acs && base.app.acs.cheatEver;
			dRLLeaderboardData.scoreCheatRatio = (base.app.acs ? base.app.acs.avgRatio : 1f);
			dRLLeaderboardData.scoreCheatSamples = (base.app.acs ? base.app.acs.GetSamplesString() : "");
			if (!playerDrone.IsCurrentPhysicsDefault || rd.diameter != 7)
			{
				dRLLeaderboardData.drlOfficial = false;
			}
			else
			{
				dRLLeaderboardData.drlOfficial = true;
			}
			dRLLeaderboardData.topSpeed = topSpeed;
			dRLLeaderboardData.crashCount = crashes;
			dRLLeaderboardData.totalDistance = distanceTraveled;
			dRLLeaderboardData.diameter = rd.diameter;
			dRLLeaderboardData.customPhysics = rd.hasCustomPhysics;
			dRLLeaderboardData.droneThumb = rd.thumb1;
			dRLLeaderboardData.droneName = rd.name;
			if (model.status == RaceStatusType.Success)
			{
				dRLLeaderboardData.raceStatusFlag = RaceStatusType.Success;
			}
			if ((bool)base.game.model && (bool)playerDrone)
			{
				dRLLeaderboardData.hash = base.game.model.playerDroneHash;
			}
			if (custom_map != null)
			{
				Debug.Log("GameCollectableController> SetLeaderboard / Sending Custom Map / guid[" + custom_map.guid + "] title[" + custom_map.mapTitle.ToUpper() + "]");
			}
			sm.SetLeaderboard(dRLLeaderboardData, delegate(DRLLeaderboardData p_result)
			{
				if (base.validContext)
				{
					if (p_result == null)
					{
						Debug.LogWarning("GameCollectableController> SetLeaderboard - Failed to send results!");
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
						if (progression != null)
						{
							list.AddRange(new string[1] { "Progression:\n" + progression.ToJson(p_indented: true) });
						}
						Debug.Log("GameCollectableController> Leaderboard Success!\n" + string.Join("\n", list));
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
							Debug.Log($"GameCollectableController> Replay Parse success [{num} bytes] at {DateTime.Now.ToString()}");
							int replay_upload_count = ((parsed_replay != null) ? ((!lbr.highscore) ? 1 : 2) : 0);
							if (lbr.highscore && parsed_replay != null)
							{
								sm.SetReplayRace(id, parsed_replay, map, track, rd.diameter, is_force, delegate(DRLReplayData[] p_replay_result)
								{
									Debug.Log("GameCollectableController> SetReplaySuccess\n" + p_replay_result);
									replay_upload_count--;
									if (replay_upload_count <= 0)
									{
										replayUploadCompleted = true;
										Notify("game.race.replay-upload@complete");
									}
								});
							}
							if (parsed_replay != null)
							{
								sm.StorageReplayCloud(id, parsed_replay, delegate(string p_replay_url)
								{
									Debug.Log("GameCollectableController> Replay Cloud Submission Complete [" + p_replay_url + "]");
									replay_upload_count--;
									if (replay_upload_count <= 0)
									{
										replayUploadCompleted = true;
										Notify("game.race.replay-upload@complete");
									}
								});
							}
							if (replay_upload_count <= 0)
							{
								replayUploadCompleted = true;
								Notify("game.race.replay-upload@complete");
							}
						};
						if (p_callback != null)
						{
							p_callback(lbr);
						}
						Notify("game.race.leaderboard-set");
					}
				}
			}, -1, p_collectable: true);
		}

		protected override void Update()
		{
			if (!resetInProgress && model.gameActive)
			{
				UpdateRace();
				base.Update();
			}
		}
	}
}

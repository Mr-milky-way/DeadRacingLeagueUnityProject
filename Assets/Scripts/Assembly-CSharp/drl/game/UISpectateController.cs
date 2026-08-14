using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using drl.network;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;
using vrecorder;

namespace drl.game
{
	public class UISpectateController : Controller<DRLApp>
	{
		public bool ignoreChange;

		private bool m_disableExit;

		private bool m_isControlEnabled;

		private const float LeaderPollingRate = 1f;

		private float m_LeaderFocusTimer;

		private bool m_targetsReady;

		private RectTransform m_standingsRect;

		private const float m_extraCooldownForSwappingLeaders = 3f;

		private bool is_right_panel_over;

		private Activity m_right_panel_loop;

		private float m_stored_speed = -1f;

		private Activity m_panelToggle;

		public UISpectateView view => AssertLocal<UISpectateView>("view");

		public UISpectateModel model => AssertLocal<UISpectateModel>("model");

		public GameController game => base.app.controller.game;

		public ReplayPlayerController replay => base.app.controller.game.replay.player;

		private bool isPlayerObserver => base.app.model.storage.state.player.profile.isObserver;

		private bool isPlayerCommentator => base.app.model.storage.state.player.profile.isCommentator;

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			GameFlag type = base.app.arguments.game.type;
			switch (p_event)
			{
			case "ui.screen@close":
				if (p_data.Length != 0)
				{
					UIScreen uIScreen = p_data[0] as UIScreen;
					if (!(uIScreen == null) && uIScreen == view.screen)
					{
						view.tournamentContext = false;
						m_disableExit = false;
					}
				}
				break;
			case "ui.screen@open":
			{
				DroneCamera camera4 = base.app.model.game.camera;
				if ((bool)camera4)
				{
					camera4.wasd.enabled = view.current && camera4.mode == DroneCameraModeType.Free;
				}
				if (view.current && view.enabled)
				{
					Debug.Log($"UISpectateController> ScreenOpen | isPlayerCommentator: {isPlayerCommentator}");
					Debug.Log($"UISpectateController> ScreenOpen | isPlayerObserver: {isPlayerObserver}");
					view.trailWidthContainer.SetActive(value: true);
					if (!isPlayerObserver)
					{
						view.EnableControls(p_focus: false);
					}
					base.app.controller.game.FadeBlur(0f, 0f, 0.05f);
					base.app.view.ui.SetDark(p_flag: false);
					base.app.view.ui.footer.Hide(0f);
					base.app.model.game.camera.main.GetComponent<Camera>().enabled = true;
					this.TimerRunOnce(delegate
					{
						base.app.controller.game.FadeBlur(0f, 0f, 0.05f);
						base.app.view.ui.SetDark(p_flag: false);
						base.app.view.ui.footer.Hide(0f);
					}, 0.41f);
					view.SetControllerType(model.GetFocusController());
					this.TimerRunOnce(delegate
					{
						UINavigation.Focus(view.cameraModeButtons[0]);
					}, 2f);
					this.TimerRunOnce(delegate
					{
						base.app.controller.settings.ApplySimulationCameras(p_force: true);
					}, 5f);
					if (type != GameFlag.Replay)
					{
						game.ui.hud.Show();
						view.leaderFocusEnabled = (model.keepFocusOnLeader = false);
					}
					if (type == GameFlag.Replay)
					{
						game.SetTabScreenEnabled(p_flag: false);
					}
					UISecondaryHeaderView headerSecondary = base.app.view.ui.headerSecondary;
					if (headerSecondary != null)
					{
						headerSecondary.Refresh(view, p_is_under_review: false);
					}
					base.app.view.audio.ResumeAllGameAudio();
					view.tournamentContext = view.tournamentContext || base.app.inTournament;
				}
				break;
			}
			case "game.simulation.load@complete":
				Debug.Log("UISpectateController> SimulationAllDroneReady / type[" + base.app.arguments.game.type.ToString() + "] mode[" + base.app.arguments.game.mode.ToString() + "]");
				Initialize(type);
				break;
			case "game.ready":
				this.TimerRunOnce(delegate
				{
					if (base.validContext)
					{
						base.app.view.ui.fade.Kill();
					}
				}, 1f / 60f);
				break;
			case "game.race.lap@step":
			{
				int p_count = Reflection<object>.Get<int>(p_data, 0);
				int p_total = Reflection<object>.Get<int>(p_data, 1);
				Drone drone = Reflection<object>.Get<Drone>(p_data, 3);
				if (!(drone == null))
				{
					Drone focus4 = model.GetFocus<Drone>();
					if (focus4 == null)
					{
						return;
					}
					if (!(drone != focus4))
					{
						view.SetLapCount(p_count, p_total);
					}
				}
				break;
			}
			case "network.player.spectator":
				if (view.current)
				{
					NetworkActor networkActor = (NetworkActor)p_data[0];
					if (base.app.model.network.room != null && !networkActor.IsLocal)
					{
						model.RemoveTargetById(networkActor.PlayerId);
					}
				}
				break;
			case "network.remote.transmitter.added":
			{
				GamePlayerData p_data2 = (GamePlayerData)p_data[0];
				model.AddTarget(p_data2);
				break;
			}
			case "network.player.room@exit":
			{
				int p_network_id = (int)p_data[0];
				model.RemoveTargetByNetworkId(p_network_id);
				break;
			}
			case "network.race.end":
				if (view.current)
				{
					Exit();
				}
				break;
			case "spectate.targets.ready":
				Debug.Log("UISpectateController> Targets Ready");
				SetTargetsReady();
				break;
			case "spectate.targets@change":
			{
				List<string> upperCaseNames = model.GetUpperCaseNames();
				Debug.Log("UISpectateController> Targets Change\n" + string.Join("\n", upperCaseNames));
				view.SetTargets(upperCaseNames, p_full_size: false);
				RefreshCameraToolActiveHint();
				break;
			}
			case "spectate.focus@change":
			{
				GamePlayerData focus = model.GetFocus<GamePlayerData>();
				int focus2 = model.focus;
				view.SetUser(focus);
				view.SetControllerType(model.GetFocusController());
				view.SetEvents(model.GetFocus<List<ReplayEvent>>());
				ignoreChange = true;
				view.targetStepper.Set(focus2);
				view.targetStepper.Refresh();
				ignoreChange = false;
				SpectateCameraModeType cameraMode = model.cameraMode;
				if ((uint)(cameraMode - 3) <= 1u)
				{
					model.SetCameraToolFocus(model.cameraToolTargetFocus[focus2]);
				}
				ignoreChange = true;
				if (view.targetStepper.index != focus2)
				{
					view.targetStepper.index = focus2;
					view.targetStepper.Refresh();
				}
				ignoreChange = false;
				if ((bool)game)
				{
					int focusDroneGate = model.GetFocusDroneGate();
					int laps = game.model.level.track.laps;
					int lapLoopCount = game.model.level.track.lapLoopCount;
					int value = ((laps > 0) ? ((focusDroneGate + 1) / lapLoopCount + 1) : 0);
					value = Mathf.Clamp(value, 1, laps);
					view.SetLapCount(value, laps);
				}
				break;
			}
			case "spectate.camera-tools@change":
			{
				bool flag = base.app.arguments.game.type == GameFlag.Freestyle;
				view.SetCameraTools(model.cameraTools.Count, !flag);
				model.ResetCameraToolFocus();
				break;
			}
			case "spectate.course-cameras@change":
				view.SetCourseCameras(model.courseCameras.Count);
				break;
			case "spectate.course-camera-mode@change":
			{
				DroneCamera camera = base.app.model.game.camera;
				model.ApplyCameraCourse(camera);
				break;
			}
			case "spectate.camera-mode@change":
			{
				ignoreChange = true;
				view.SetCameraMode(model.cameraMode);
				ignoreChange = false;
				Drone focus3 = model.GetFocus<Drone>();
				DroneCamera camera3 = base.app.model.game.camera;
				model.ApplyCameraMode(model.cameraMode, camera3, focus3);
				camera3.wasd.useJoystick = model.cameraMode == SpectateCameraModeType.FreeCamera || model.cameraMode == SpectateCameraModeType.Orbit;
				if (!model.isReplay)
				{
					DroneNetworkTransmitter byDrone = base.app.model.game.simulation.transmitters.GetByDrone<DroneNetworkTransmitter>(focus3);
					if ((model.cameraMode != SpectateCameraModeType.FPV && model.cameraMode != SpectateCameraModeType.Orbit) || byDrone == null || byDrone.Actor == null)
					{
						base.app.view.ui.game.hud.damage.Show(p_flag: false);
						base.app.view.ui.game.hud.damage.Reset();
					}
					else
					{
						RefreshDamageIndicator(byDrone.Actor.ID);
					}
				}
				break;
			}
			case "spectate.camera-tool.focus-list@change":
				RefreshCameraToolActiveHint();
				break;
			case "spectate.camera-tool.focus@change":
			{
				int p_index2 = (int)p_data[0];
				view.ClearCameraToolFocus();
				view.ClearCourseCameraActive();
				view.SetCameraToolFocus(p_index2, p_flag: true);
				model.UpdateCameraTool(game.model.camera, p_smooth: false);
				model.RefreshEffects();
				break;
			}
			case "spectate.course-camera.focus@change":
			{
				int p_index = (int)p_data[0];
				DroneCamera camera2 = base.app.model.game.camera;
				view.ClearCameraToolFocus();
				view.ClearCourseCameraActive();
				model.SetCourseCameraActive(p_flag: true);
				view.SetCourseCameraActive(p_index, p_flag: true);
				model.ApplyCameraCourse(camera2);
				model.RefreshEffects();
				break;
			}
			case "spectate.drone-trail-mode@change":
				ignoreChange = true;
				view.SetDroneTrailMode(model.trailMode);
				ignoreChange = false;
				break;
			case "spectate.drone-trail-width-mode@change":
				ignoreChange = true;
				view.SetDroneTrailWidthMode(model.trailWidthMode);
				ignoreChange = false;
				break;
			case "tournament.action.start-match":
				if (base.app.inTournament && p_data.Length != 0)
				{
					string text = p_data[0] as string;
					if (!string.IsNullOrEmpty(text) && base.app.model.network.room != null && !(base.app.model.network.room.MatchId != text) && type == GameFlag.Replay)
					{
						Exit();
					}
				}
				break;
			case "spectate.form.event@change":
			case "spectate.form.event@click":
			case "spectate.form.event@over":
			case "spectate.form.event@out":
				OnFieldsFormNotification(p_event, p_target, p_data);
				break;
			}
			if (model.isReplay)
			{
				OnReplayNotification(p_event, p_target, p_data);
			}
			else
			{
				OnSpectatorNotification(p_event, p_target, p_data);
			}
		}

		protected void OnFieldsFormNotification(string p_notification, UnityEngine.Object p_target, params object[] p_data)
		{
			string text = p_target.name;
			bool flag = p_notification.Contains("@change");
			bool flag2 = p_notification.Contains("@end-edit");
			bool flag3 = p_notification.Contains("@click");
			bool flag4 = p_notification.Contains("@over");
			p_notification.Contains("@out");
			if (flag && ignoreChange)
			{
				return;
			}
			if (flag3)
			{
				switch (text)
				{
				case "playback-skip-end":
				{
					float nextEventTime = model.GetNextEventTime(view.time);
					UpdateReplayTime(nextEventTime);
					break;
				}
				case "playback-skip-start":
				{
					float prevEventTime = model.GetPrevEventTime(view.time);
					UpdateReplayTime(prevEventTime);
					break;
				}
				case "playback-play":
					SwitchReplayPlayback();
					break;
				case "playback-stop":
					StopReplay();
					model.ResetCameraToolFocus();
					model.ResetTargetRays();
					model.UpdateCameraTool(game.model.camera, p_smooth: false);
					break;
				case "camera-fpv":
					model.SetCameraMode(SpectateCameraModeType.FPV);
					break;
				case "camera-orbit":
					model.SetCameraMode(SpectateCameraModeType.Orbit);
					break;
				case "camera-free":
					model.SetCameraMode(SpectateCameraModeType.FreeCamera);
					break;
				case "camera-auto":
					model.SetCameraMode(SpectateCameraModeType.Auto);
					break;
				case "camera-manual":
					model.SetCameraMode(SpectateCameraModeType.Manual);
					break;
				case "course-camera-item":
				case "camera-tool-item":
				{
					UISpectateCTButton uISpectateCTButton = p_target as UISpectateCTButton;
					bool num9 = text == "course-camera-item";
					if (num9)
					{
						model.SetCourseCameraFocus(uISpectateCTButton.index);
					}
					if (!num9)
					{
						SpectateCameraModeType cameraMode = model.cameraMode;
						if ((uint)(cameraMode - 3) <= 1u)
						{
							model.SetCameraToolFocus(uISpectateCTButton.index);
						}
					}
					break;
				}
				case "leader-focus":
				{
					DRLToggleView dRLToggleView3 = p_target as DRLToggleView;
					model.keepFocusOnLeader = dRLToggleView3.toggle.isOn;
					break;
				}
				case "trail-off":
					model.SetDroneTrailMode(SpectateDroneTrailModeType.Off);
					break;
				case "trail-small":
					model.SetDroneTrailMode(SpectateDroneTrailModeType.Small);
					break;
				case "trail-medium":
					model.SetDroneTrailMode(SpectateDroneTrailModeType.Medium);
					break;
				case "trail-large":
					model.SetDroneTrailMode(SpectateDroneTrailModeType.Large);
					break;
				case "trail-auto":
					model.SetDroneTrailMode(SpectateDroneTrailModeType.Auto);
					break;
				case "trail-width-small":
					model.SetDroneTrailWidthMode(SpectateDroneTrailWidthModeType.Small);
					break;
				case "trail-width-medium":
					model.SetDroneTrailWidthMode(SpectateDroneTrailWidthModeType.Medium);
					break;
				case "trail-width-large":
					model.SetDroneTrailWidthMode(SpectateDroneTrailWidthModeType.Large);
					break;
				case "trail-width-auto":
					model.SetDroneTrailWidthMode(SpectateDroneTrailWidthModeType.Auto);
					break;
				case "spectate-target-item":
				{
					UISpectateTargetButton uISpectateTargetButton = p_target as UISpectateTargetButton;
					Notify("spectate.target.select", uISpectateTargetButton.index);
					break;
				}
				case "info-name":
				{
					DRLToggleView dRLToggleView2 = p_target as DRLToggleView;
					view.SetUserVisible(dRLToggleView2.toggle.isOn);
					break;
				}
				case "info-race-stats":
				{
					DRLToggleView dRLToggleView = p_target as DRLToggleView;
					view.SetRaceStatsVisible(dRLToggleView.toggle.isOn);
					break;
				}
				case "info-controller":
				{
					DRLToggleView dRLToggleView5 = p_target as DRLToggleView;
					if (dRLToggleView5.toggle.isOn)
					{
						view.SetControllerType(model.GetFocusController());
					}
					view.controller.fade.alpha = (dRLToggleView5.toggle.isOn ? 1f : (-0.1f));
					break;
				}
				case "vc-crop":
				{
					DRLToggleView dRLToggleView4 = p_target as DRLToggleView;
					model.videoCaptureCropEnabled = dRLToggleView4.toggle.isOn;
					break;
				}
				case "video-capture":
				{
					ignoreChange = true;
					view.SetVideoRecordEnabled(p_flag: true);
					view.isHelpDataVisible = false;
					view.SetVideoSizeMode(model.videoCaptureSizeMode);
					view.SetVideoAspectMode(model.videoCaptureApectMode);
					view.SetVideoFPSMode(model.videoCaptureFPSMode);
					view.SetVideoQualityMode(model.videoCaptureQualityMode);
					view.videoCaptureCropEnabled = model.videoCaptureCropEnabled;
					int[] captureAspect2 = GetCaptureAspect(model.videoCaptureApectMode);
					ResetCamerasViewport();
					SetCaptureAspect(captureAspect2[0], captureAspect2[1]);
					view.videoCaptureFolderPath = model.videoCaptureOutputFolderPath;
					ignoreChange = false;
					game.model.camera.video.Clear();
					RefreshTempCaptureDiskSpace();
					break;
				}
				case "video-capture-back":
					view.SetVideoRecordEnabled(p_flag: false);
					ResetCamerasViewport();
					game.model.camera.video.Clear();
					break;
				case "video-capture-record":
				{
					VideoCapture video = game.model.camera.video;
					if (video.isRecording || video.isGenerating)
					{
						break;
					}
					string p_target_folder = DRLPaths.Assert(model.videoCaptureOutputFolderPath);
					string hash = base.app.hash;
					float num2 = Mathf.Min(model.videoCaptureRangeStart, model.videoCaptureRangeEnd);
					float num3 = Mathf.Max(model.videoCaptureRangeStart, model.videoCaptureRangeEnd);
					if (!(Mathf.Abs(num3 - num2) <= 0.001f))
					{
						Mathf.Ceil(num3 - num2 + 0.5f);
						int num4 = (int)(model.videoCaptureQualityMode - 50);
						int[] array = new int[9] { 36, 32, 28, 24, 20, 16, 12, 8, 4 };
						int compression = ((model.videoCaptureQualityMode != UISpectateVideoFlags.QualityMax) ? array[num4] : 0);
						int p_framerate = 60;
						switch (model.videoCaptureFPSMode)
						{
						case UISpectateVideoFlags.FPS240:
							p_framerate = 240;
							break;
						case UISpectateVideoFlags.FPS120:
							p_framerate = 120;
							break;
						case UISpectateVideoFlags.FPS60:
							p_framerate = 60;
							break;
						case UISpectateVideoFlags.FPS30:
							p_framerate = 30;
							break;
						case UISpectateVideoFlags.FPS24:
							p_framerate = 24;
							break;
						}
						int num5 = 1080;
						switch (model.videoCaptureSizeMode)
						{
						case UISpectateVideoFlags.Size2160:
							num5 = 2160;
							break;
						case UISpectateVideoFlags.Size1080:
							num5 = 1080;
							break;
						case UISpectateVideoFlags.Size720:
							num5 = 720;
							break;
						case UISpectateVideoFlags.Size540:
							num5 = 540;
							break;
						case UISpectateVideoFlags.Size480:
							num5 = 480;
							break;
						case UISpectateVideoFlags.Size240:
							num5 = 240;
							break;
						}
						float num6 = Screen.width;
						float num7 = Screen.height;
						int[] captureAspect = GetCaptureAspect(model.videoCaptureApectMode);
						SetCaptureAspect(captureAspect[0], captureAspect[1]);
						video.SetCompression(compression);
						int num8 = (int)(Mathf.Floor(num6 * ((float)num5 / num7) / 2f) * 2f);
						num6 = num8;
						num7 = num5;
						Rect captureAspectRect = GetCaptureAspectRect(captureAspect[0], captureAspect[1]);
						captureAspectRect.x *= num6;
						captureAspectRect.y *= num7;
						captureAspectRect.width *= num6;
						captureAspectRect.height *= num7;
						video.ResetCropArea();
						if (model.videoCaptureCropEnabled)
						{
							video.SetCropArea(captureAspectRect);
						}
						if ((bool)game)
						{
							game.SetGCEnabled(p_flag: true);
						}
						view.SetVideoEncodingState(p_encoding: true);
						video.recorder.path = DRLPaths.Storage.videoRecorderExecutable;
						bool key = Input.GetKey(KeyCode.LeftShift);
						video.recorder.output.codecFlag = (key ? VideoRecorder.CodecModeFlag.H264 : VideoRecorder.CodecModeFlag.VP9);
						base.app.model.storage.replays.WriteRecorderApp();
						video.Record(600f, p_framerate, num8, num5, p_target_folder, hash, p_piped: true);
					}
					break;
				}
				case "vc-size-4k":
					model.videoCaptureSizeMode = UISpectateVideoFlags.Size2160;
					view.SetVideoSizeMode(model.videoCaptureSizeMode);
					RefreshTempCaptureDiskSpace();
					break;
				case "vc-size-1080":
					model.videoCaptureSizeMode = UISpectateVideoFlags.Size1080;
					view.SetVideoSizeMode(model.videoCaptureSizeMode);
					RefreshTempCaptureDiskSpace();
					break;
				case "vc-size-720":
					model.videoCaptureSizeMode = UISpectateVideoFlags.Size720;
					view.SetVideoSizeMode(model.videoCaptureSizeMode);
					RefreshTempCaptureDiskSpace();
					break;
				case "vc-size-540":
					model.videoCaptureSizeMode = UISpectateVideoFlags.Size540;
					view.SetVideoSizeMode(model.videoCaptureSizeMode);
					RefreshTempCaptureDiskSpace();
					break;
				case "vc-size-480":
					model.videoCaptureSizeMode = UISpectateVideoFlags.Size480;
					view.SetVideoSizeMode(model.videoCaptureSizeMode);
					RefreshTempCaptureDiskSpace();
					break;
				case "vc-size-240":
					model.videoCaptureSizeMode = UISpectateVideoFlags.Size240;
					view.SetVideoSizeMode(model.videoCaptureSizeMode);
					RefreshTempCaptureDiskSpace();
					break;
				case "vc-fps-240":
					model.videoCaptureFPSMode = UISpectateVideoFlags.FPS240;
					view.SetVideoFPSMode(model.videoCaptureFPSMode);
					RefreshTempCaptureDiskSpace();
					break;
				case "vc-fps-120":
					model.videoCaptureFPSMode = UISpectateVideoFlags.FPS120;
					view.SetVideoFPSMode(model.videoCaptureFPSMode);
					RefreshTempCaptureDiskSpace();
					break;
				case "vc-fps-60":
					model.videoCaptureFPSMode = UISpectateVideoFlags.FPS60;
					view.SetVideoFPSMode(model.videoCaptureFPSMode);
					RefreshTempCaptureDiskSpace();
					break;
				case "vc-fps-30":
					model.videoCaptureFPSMode = UISpectateVideoFlags.FPS30;
					view.SetVideoFPSMode(model.videoCaptureFPSMode);
					RefreshTempCaptureDiskSpace();
					break;
				case "vc-fps-24":
					model.videoCaptureFPSMode = UISpectateVideoFlags.FPS24;
					view.SetVideoFPSMode(model.videoCaptureFPSMode);
					RefreshTempCaptureDiskSpace();
					break;
				case "vc-aspect-w:h":
					model.videoCaptureApectMode = UISpectateVideoFlags.AspectWH;
					view.SetVideoAspectMode(model.videoCaptureApectMode);
					break;
				case "vc-aspect-h:w":
					model.videoCaptureApectMode = UISpectateVideoFlags.AspectHW;
					view.SetVideoAspectMode(model.videoCaptureApectMode);
					break;
				case "vc-aspect-21:9":
					model.videoCaptureApectMode = UISpectateVideoFlags.Aspect21_9;
					view.SetVideoAspectMode(model.videoCaptureApectMode);
					break;
				case "vc-aspect-16:10":
					model.videoCaptureApectMode = UISpectateVideoFlags.Aspect16_10;
					view.SetVideoAspectMode(model.videoCaptureApectMode);
					break;
				case "vc-aspect-16:9":
					model.videoCaptureApectMode = UISpectateVideoFlags.Aspect16_9;
					view.SetVideoAspectMode(model.videoCaptureApectMode);
					break;
				case "vc-aspect-4:3":
					model.videoCaptureApectMode = UISpectateVideoFlags.Aspect4_3;
					view.SetVideoAspectMode(model.videoCaptureApectMode);
					break;
				case "vc-aspect-1:1":
					model.videoCaptureApectMode = UISpectateVideoFlags.Aspect1_1;
					view.SetVideoAspectMode(model.videoCaptureApectMode);
					break;
				case "vc-quality-mode":
				{
					UIElementView uIElementView = p_target as UIElementView;
					int num = (uIElementView ? uIElementView.transform.GetSiblingIndex() : (-1));
					if (num < 0)
					{
						return;
					}
					num = num - 3 + 50;
					UISpectateVideoFlags uISpectateVideoFlags = (UISpectateVideoFlags)num;
					model.videoCaptureQualityMode = uISpectateVideoFlags;
					view.SetVideoQualityMode(uISpectateVideoFlags);
					break;
				}
				case "nav-settings":
				{
					UIScreen uIScreen = base.app.view.ui.screens.Open("settings-screen", 0f);
					if (model.isReplay)
					{
						replay.model.paused = true;
						view.SetPlaybackPause(p_flag: false);
					}
					else if ((bool)uIScreen)
					{
						game.SetTabScreenEnabled(p_flag: false);
					}
					break;
				}
				case "nav-exit":
					if (m_disableExit)
					{
						break;
					}
					Exit(p_fromUI: true);
					m_disableExit = true;
					this.TimerRunOnce(delegate
					{
						if (base.validContext)
						{
							m_disableExit = false;
						}
					}, 3f);
					break;
				case "nav-help":
					view.isHelpDataVisible = !view.isHelpDataVisible;
					view.SetVideoRecordEnabled(p_flag: false);
					break;
				}
			}
			if (flag || flag2)
			{
				switch (text)
				{
				case "playback-speed":
				{
					float replaySpeed = Mathf.Floor((p_target as DRLSliderView).slider.value * 100f) / 100f;
					SetReplaySpeed(replaySpeed);
					break;
				}
				case "playback-time":
				{
					float value = (p_target as DRLSliderView).slider.value;
					UpdateReplayTime(value);
					break;
				}
				case "spectate-name":
				{
					int index = (p_target as DRLStepperView).index;
					model.SetFocus(index);
					break;
				}
				case "vc-output-folder":
				{
					DRLInputFieldView dRLInputFieldView = p_target as DRLInputFieldView;
					model.videoCaptureOutputFolderPath = dRLInputFieldView.text;
					Debug.Log("UISpectateController> OnFieldsFormNotification / " + text + "[" + model.videoCaptureOutputFolderPath + "]");
					break;
				}
				case "vc-range-start":
				case "vc-range-end":
				{
					ignoreChange = true;
					DRLNumberFieldView videoRecordRangeStartField = view.videoRecordRangeStartField;
					DRLNumberFieldView videoRecordRangeEndField = view.videoRecordRangeEndField;
					view.SetVideoRecordRange(videoRecordRangeStartField.value, videoRecordRangeEndField.value, 0f, replay.model.duration);
					model.videoCaptureRangeStart = videoRecordRangeStartField.value;
					model.videoCaptureRangeEnd = videoRecordRangeEndField.value;
					ignoreChange = false;
					RefreshTempCaptureDiskSpace();
					break;
				}
				}
			}
			if (text == null)
			{
				return;
			}
			switch (text)
			{
			case "right-panel-trigger":
			{
				if (!flag4 || isPlayerObserver || game.model.camera.video.isRecording || Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1))
				{
					break;
				}
				_ = view.panelTriggerField.data.position;
				if (m_right_panel_loop != null)
				{
					m_right_panel_loop.Stop();
				}
				RectTransform rt = (RectTransform)view.panelTriggerField.transform;
				float trigger_timeout = 0f;
				Vector2 ts = rt.sizeDelta;
				m_right_panel_loop = Activity.Run((Func<bool>)delegate
				{
					if (!base.validContext)
					{
						return false;
					}
					Vector3 vector = base.app.view.ui.GetMousePosition(rt);
					bool flag5 = true;
					if (vector.x < 0f - ts.x)
					{
						flag5 = false;
					}
					else if (vector.y > ts.y)
					{
						flag5 = false;
					}
					if (flag5)
					{
						trigger_timeout = 3f;
						return true;
					}
					trigger_timeout -= Time.unscaledDeltaTime;
					if (trigger_timeout > 0f)
					{
						return true;
					}
					view.panelFade.Fade(1f, 0.3f, 0f);
					this.TimerRunOnce(delegate
					{
						UINavigation.Focus(view.cameraModeButtons[0]);
					}, 0.5f);
					return false;
				}, 0f, false);
				view.panelFade.Fade(0f, 0.15f, 0f);
				RefreshTempCaptureDiskSpace();
				break;
			}
			case "vc-aspect-w:h":
			case "vc-aspect-h:w":
			case "vc-aspect-21:9":
			case "vc-aspect-16:10":
			case "vc-aspect-16:9":
			case "vc-aspect-4:3":
			case "vc-aspect-1:1":
			{
				int[] captureAspect3 = GetCaptureAspect(text);
				if (flag3)
				{
					SetCaptureAspect(captureAspect3[0], captureAspect3[1]);
				}
				break;
			}
			}
		}

		private void UpdateReplayTime(float p_time)
		{
			SetReplayTime(p_time, GetReplayDuration());
			model.ResetCameraToolFocus();
			model.ResetTargetRays();
			model.UpdateCameraTool(game.model.camera, p_smooth: false);
		}

		public void Initialize()
		{
			GameFlag p_game_type = (base.app.model.game ? base.app.model.game.type : GameFlag.None);
			Initialize(p_game_type);
		}

		public void Initialize(GameFlag p_game_type)
		{
			if (!base.app.model.game.simulation)
			{
				Debug.LogWarning("UISpectateController> Initialize - Failed to find the simulation");
				base.app.scene.LoadMain();
				return;
			}
			if ((bool)base.app.model.game.level.track.pathTrace)
			{
				base.app.model.game.level.track.pathTrace.gameObject.SetActive(value: false);
			}
			base.app.controller.settings.ApplySimulationCameras(p_force: true);
			TransformVector averageTransform = base.app.model.game.simulation.podiums.GetAverageTransform();
			DroneCamera camera = base.app.model.game.camera;
			if ((bool)camera)
			{
				camera.transform.position = averageTransform.position + Vector3.up;
				camera.transform.localRotation = averageTransform.rotation;
				camera.main.enabled = true;
			}
			model.isReplay = p_game_type == GameFlag.Replay;
			_ = base.app.arguments.game.tournamentData;
			model.changeFocusUponFinish = isPlayerCommentator;
			view.SetHelpData(model.helpDataList, model.isReplay ? 999 : 11);
			view.isHelpDataVisible = false;
			view.SetVideoWatermark(p_flag: false);
			GameFlag gameFlag = p_game_type;
			bool isCustomMap = base.app.arguments.game.isCustomMap;
			MapData mapData = (isCustomMap ? base.app.arguments.game.map.data : null);
			if ((gameFlag == GameFlag.Freestyle || gameFlag == GameFlag.Race) && !base.app.model.game.multiplayer)
			{
				gameFlag = GameFlag.None;
			}
			ignoreChange = true;
			view.contentFade.alpha = 0f;
			view.panelFade.transition = 1f;
			this.TimerRunOnce(delegate
			{
				((Component)this).ActivityRun((Func<bool>)delegate
				{
					if (!base.validContext || UINavigation.focus != null)
					{
						return true;
					}
					UINavigation.Focus(view.cameraModeButtons[0]);
					return false;
				}, 0f);
			}, 0.8f);
			view.playerNameVisible = true;
			view.leaderFocusEnabled = (model.keepFocusOnLeader = false);
			view.controllerVisible = false;
			view.controller.fade.alpha = -0.1f;
			switch (gameFlag)
			{
			case GameFlag.Freestyle:
				view.raceStatsVisible = false;
				view.raceStatsAllowed = false;
				break;
			case GameFlag.Collectable:
				view.raceStatsVisible = true;
				view.raceStatsAllowed = true;
				view.lapCountAllowed = false;
				break;
			case GameFlag.Race:
				view.raceStatsVisible = true;
				view.raceStatsAllowed = true;
				view.lapCountAllowed = isCustomMap && mapData.mode.race.lapCount > 1;
				break;
			}
			view.SetLapCountEnabled(p_flag: false);
			view.SetRaceStatsVisible(view.raceStatsAllowed && view.raceStatsVisible);
			view.SetDroneTrailMode(SpectateDroneTrailModeType.Auto);
			ignoreChange = false;
			List<MACameraTool> cameraTools = new List<MACameraTool>(base.app.model.game.level.track.cameraTools);
			List<MASpline> courseCameras = new List<MASpline>(base.app.model.game.level.track.courseCameras);
			model.SetCameraTools(cameraTools);
			TrackModel track = base.app.model.game.level.track;
			for (int num = 0; num < track.actions.Count; num++)
			{
				MapAssetAction mapAssetAction = track.actions[num];
				switch (mapAssetAction.tag)
				{
				case GameFlag.ActionNone:
					mapAssetAction.mode = MapAssetActionMode.Auto;
					break;
				case GameFlag.ActionBreakGlass:
					mapAssetAction.mode = MapAssetActionMode.Manual;
					break;
				}
			}
			track.RestoreActions();
			model.SetCourseCameras(courseCameras);
			switch (gameFlag)
			{
			case GameFlag.None:
				Debug.LogWarning("UISpectateController> Initialize - Failed to initialize non multiplayer game!");
				base.app.scene.LoadMain();
				return;
			case GameFlag.Replay:
				InitializeReplay();
				break;
			case GameFlag.Freestyle:
			case GameFlag.Race:
			case GameFlag.Collectable:
				InitializeSpectator();
				break;
			}
			base.app.view.ui.screens.ClearStaticBackground();
		}

		public void SetTargetsReady()
		{
			if (model.isReplay)
			{
				model.SetFocus(0);
			}
			else
			{
				model.SetFocusAvailable();
			}
			if (isPlayerCommentator)
			{
				model.SetCameraMode(SpectateCameraModeType.Auto);
			}
			else
			{
				model.SetCameraMode(SpectateCameraModeType.FPV);
			}
			model.ResetTargetRays();
			base.app.view.ui.fade.Fade(base.app.view.ui.fade.transition, 1f, 0f, 0f);
			view.contentFade.Fade(1f, 0.5f);
			view.panelFade.Fade(1f, 0f, 0.5f, 0.8f);
			this.TimerRunOnce(delegate
			{
				UINavigation.Focus(view.cameraModeButtons[0]);
			}, 0.6f);
			model.ResetCameraToolFocus(0);
			m_targetsReady = true;
		}

		public void PauseReplay()
		{
			if (view.IsControlsEnabled())
			{
				base.app.view.ui.navigation.enabled = false;
				view.DisableControls();
			}
		}

		public void UnpauseReplay()
		{
			if (!view.IsControlsEnabled())
			{
				base.app.view.ui.navigation.enabled = true;
				view.EnableControls(p_focus: false);
			}
		}

		public void StopReplay()
		{
			replay.model.paused = false;
			replay.model.playing = false;
			replay.model.Seek(0f);
			view.SetPlaybackPause(p_flag: false);
			UpdateReplayClip();
			model.ClearDroneTrails();
		}

		public void SetReplaySpeed(float p_speed)
		{
			replay.model.speed = p_speed;
			model.SetDroneTrailScale(p_speed);
			base.app.view.audio.UpdateTimescale(Mathf.Abs(p_speed));
		}

		public void SetReplayTime(float p_time, float p_max_time = -1f)
		{
			replay.model.Seek(p_time);
			if (p_max_time >= 0f)
			{
				p_time = Mathf.Min(p_time, p_max_time);
			}
			view.time = p_time;
			UpdateReplayClip();
			model.ClearDroneTrails();
		}

		public void SwitchReplayPlayback()
		{
			bool flag = replay.model.playing && !replay.model.paused;
			SetReplayPlayback(!flag);
		}

		public void SetReplayPlayback(bool p_flag)
		{
			GameAudioController audio = base.app.controller.game.audio;
			if (p_flag)
			{
				audio.PlayDroneMotor(model.drones);
			}
			else
			{
				audio.StopDroneMotor(model.drones);
			}
			if ((!(replay.model.speed > 0f) || !(replay.model.elapsed >= replay.model.duration)) && (!(replay.model.speed < 0f) || !(replay.model.elapsed <= 0f)))
			{
				replay.model.paused = !p_flag;
				view.SetPlaybackPause(p_flag);
				replay.model.playing = true;
				model.ClearDroneTrails();
			}
		}

		public float GetReplayRaceTime()
		{
			ReplayClipPlayerModel focus = model.GetFocus<ReplayClipPlayerModel>();
			if (!focus)
			{
				return 0f;
			}
			return focus.raceTime;
		}

		public float GetReplayDuration()
		{
			ReplayClipPlayerModel focus = model.GetFocus<ReplayClipPlayerModel>();
			if (!focus)
			{
				return 0f;
			}
			return focus.duration;
		}

		public Rect GetCaptureAspectRect(int p_width, int p_height)
		{
			Rect result = default(Rect);
			float num = 0f;
			float num2 = 0f;
			float num3 = 1f;
			float num4 = 1f;
			float num5 = p_width;
			float num6 = p_height;
			float num7 = Screen.width;
			float num8 = Screen.height;
			float num9 = 1f;
			float num10 = num8 / num7;
			float num11 = 1f;
			float num12 = num6 / num5;
			if (num10 < num12)
			{
				num9 = num7 / num8;
				num10 = 1f;
				num11 = num5 / num6;
				num12 = 1f;
			}
			float num13 = num11 / num9;
			float num14 = num12 / num10;
			num3 = num13;
			num4 = num14;
			float num15 = 1f - num3;
			num += num15 * 0.5f;
			num15 = 1f - num4;
			num2 += num15 * 0.5f;
			result.x = num;
			result.y = num2;
			result.width = num3;
			result.height = num4;
			return result;
		}

		public void SetCaptureAspect(int p_width, int p_height)
		{
			Rect captureAspectRect = GetCaptureAspectRect(p_width, p_height);
			SetCamerasViewport(captureAspectRect.x, captureAspectRect.y, captureAspectRect.width, captureAspectRect.height);
		}

		public int[] GetCaptureAspect(string p_field_id)
		{
			int num = Screen.width;
			int num2 = Screen.height;
			switch (p_field_id)
			{
			case "vc-aspect-h:w":
				num = Screen.height;
				num2 = Screen.width;
				break;
			case "vc-aspect-21:9":
				num = 21;
				num2 = 9;
				break;
			case "vc-aspect-16:10":
				num = 16;
				num2 = 10;
				break;
			case "vc-aspect-16:9":
				num = 16;
				num2 = 9;
				break;
			case "vc-aspect-4:3":
				num = 4;
				num2 = 3;
				break;
			case "vc-aspect-1:1":
				num = 1;
				num2 = 1;
				break;
			}
			return new int[2] { num, num2 };
		}

		protected void RefreshTempCaptureDiskSpace()
		{
			int width = Screen.width;
			int height = Screen.height;
			int p_fps = 60;
			int num = ((model.videoCaptureSizeMode != UISpectateVideoFlags.Size2160) ? 1 : Mathf.CeilToInt(2160f / (float)height));
			width *= num;
			height *= num;
			switch (model.videoCaptureFPSMode)
			{
			case UISpectateVideoFlags.FPS240:
				p_fps = 240;
				break;
			case UISpectateVideoFlags.FPS120:
				p_fps = 120;
				break;
			case UISpectateVideoFlags.FPS60:
				p_fps = 60;
				break;
			case UISpectateVideoFlags.FPS30:
				p_fps = 30;
				break;
			case UISpectateVideoFlags.FPS24:
				p_fps = 24;
				break;
			}
			float videoCaptureRangeStart = model.videoCaptureRangeStart;
			float videoCaptureRangeEnd = model.videoCaptureRangeEnd;
			float p_duration = Mathf.Max(0f, videoCaptureRangeEnd - videoCaptureRangeStart);
			ulong tempDiskSpaceRequirements = game.model.camera.video.GetTempDiskSpaceRequirements(p_duration, width, height, p_fps);
			view.SetTempDiskSpace(tempDiskSpaceRequirements);
		}

		public int[] GetCaptureAspect(UISpectateVideoFlags p_flag)
		{
			int num = Screen.width;
			int num2 = Screen.height;
			switch (model.videoCaptureApectMode)
			{
			case UISpectateVideoFlags.AspectHW:
				num = Screen.height;
				num2 = Screen.width;
				break;
			case UISpectateVideoFlags.Aspect21_9:
				num = 21;
				num2 = 9;
				break;
			case UISpectateVideoFlags.Aspect16_10:
				num = 16;
				num2 = 10;
				break;
			case UISpectateVideoFlags.Aspect16_9:
				num = 16;
				num2 = 9;
				break;
			case UISpectateVideoFlags.Aspect4_3:
				num = 4;
				num2 = 3;
				break;
			case UISpectateVideoFlags.Aspect1_1:
				num = 1;
				num2 = 1;
				break;
			}
			return new int[2] { num, num2 };
		}

		public void SetCamerasViewport(float p_x, float p_y, float p_w, float p_h)
		{
			DroneCamera camera = game.model.camera;
			CanvasScaler canvasScaler = base.app.view.ui.canvasScaler;
			Camera worldCamera = base.app.view.ui.canvas.worldCamera;
			float num = Screen.width;
			float num2 = Screen.height;
			if (p_w < p_h)
			{
				num2 = Mathf.Lerp(num2 * 2f, num2, p_w);
			}
			else
			{
				num = Mathf.Lerp(num * 2f, num, p_h);
			}
			canvasScaler.referenceResolution = new Vector2(num, num2);
			Rect rect = new Rect(p_x, p_y, p_w, p_h);
			camera.SetBackgroundEnabled(p_flag: true);
			if (p_x <= 0f && p_y <= 0f && p_w >= 1f && p_h >= 1f)
			{
				camera.SetBackgroundEnabled(p_flag: false);
			}
			camera.main.rect = rect;
			worldCamera.rect = rect;
		}

		public void ResetCamerasViewport()
		{
			SetCamerasViewport(0f, 0f, 1f, 1f);
		}

		protected void UpdateReplayClip()
		{
			ReplayClipPlayerModel focus = model.GetFocus<ReplayClipPlayerModel>();
			if ((bool)focus)
			{
				view.time = Mathf.Min(replay.model.elapsed, focus.duration);
				view.controller.leftStick = focus.leftInput;
				view.controller.rightStick = focus.rightInput;
				view.raceTime = Mathf.Min(view.time, focus.raceTime);
			}
		}

		protected void OnReplayNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "spectate.focus@change":
			{
				view.TargetBlink(model.focus, 0.8f, 0.5f);
				Drone focus = model.GetFocus<Drone>();
				DroneCamera camera = base.app.model.game.camera;
				model.ApplyCameraMode(model.cameraMode, camera, focus);
				model.GetFocus<ReplayClipPlayerModel>();
				UpdateReplayClip();
				break;
			}
			case "spectate.target.select":
			{
				int p_index = (int)p_data[0];
				view.targetStepper.Set(p_index);
				break;
			}
			case "spectate.pause-command":
				if (view.IsControlsEnabled())
				{
					PauseReplay();
				}
				else
				{
					UnpauseReplay();
				}
				break;
			}
		}

		protected void OnReplayVideoState(VideoCapture p_video_capture, VideoCapture.StateType p_state)
		{
			VideoCapture.StateType stateType = p_state;
			switch (p_state)
			{
			case VideoCapture.StateType.RecordStart:
			{
				Debug.Log("UISpectateController> VideoCapture / " + stateType);
				float p_time = Mathf.Clamp(view.videoRecordRangeStartField.value, 0f, replay.model.duration);
				SetReplayTime(p_time);
				model.ResetCameraToolFocus();
				model.ResetTargetRays();
				model.UpdateCameraTool(game.model.camera, p_smooth: false);
				SetReplayPlayback(p_flag: true);
				view.panelFade.transition = 1f;
				this.TimerRunOnce(delegate
				{
					UINavigation.Focus(view.cameraModeButtons[0]);
				}, 0.6f);
				view.SetVideoEncodingProgress(0f);
				view.SetVideoWatermark(p_flag: true);
				if ((bool)base.app.view.ui.game)
				{
					base.app.view.ui.game.hud.lowFPSWarning.gameObject.SetActive(value: false);
				}
				break;
			}
			case VideoCapture.StateType.RecordStep:
			{
				float elapsed = replay.model.elapsed;
				_ = model.videoCaptureRangeStart;
				float videoCaptureRangeEnd2 = model.videoCaptureRangeEnd;
				if (elapsed >= videoCaptureRangeEnd2)
				{
					p_video_capture.Stop();
				}
				break;
			}
			case VideoCapture.StateType.GenerateProgress:
			{
				float videoCaptureRangeStart = model.videoCaptureRangeStart;
				float videoCaptureRangeEnd = model.videoCaptureRangeEnd;
				float num = p_video_capture.processedFrame;
				float num2 = p_video_capture.GetEstimatedFrameCount(videoCaptureRangeEnd - videoCaptureRangeStart);
				float videoEncodingProgress = ((num2 <= 0f) ? 1f : (num / num2));
				view.SetVideoEncodingProgress(videoEncodingProgress);
				break;
			}
			case VideoCapture.StateType.RecordStop:
				view.SetVideoWatermark(p_flag: false);
				SetReplayPlayback(p_flag: false);
				view.panelFade.transition = 0f;
				if ((bool)base.app.view.ui.game)
				{
					base.app.view.ui.game.hud.lowFPSWarning.gameObject.SetActive(value: true);
				}
				break;
			case VideoCapture.StateType.RecordEnd:
				Debug.Log("UISpectateController> VideoCapture / " + stateType);
				break;
			case VideoCapture.StateType.GenerateStart:
				Debug.Log("UISpectateController> VideoCapture / " + stateType);
				break;
			case VideoCapture.StateType.GenerateEnd:
				Debug.Log("UISpectateController> VideoCapture / " + stateType);
				p_video_capture.Clear();
				view.SetVideoEncodingState(p_encoding: false);
				try
				{
					OS.OpenFolder(p_video_capture.recorder.output.path);
					break;
				}
				catch (Exception)
				{
					break;
				}
			}
		}

		protected void ReplayUpdate()
		{
			UpdateReplayKeyboard();
			if (replay.model.playing)
			{
				UpdateReplayClip();
			}
		}

		protected void UpdateReplayKeyboard()
		{
			if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Escape))
			{
				SwitchReplayPlayback();
			}
			if (Input.GetKey(KeyCode.P))
			{
				model.SetDroneMotorRPM(1f);
			}
			float num = 0f;
			if (Input.GetKey(KeyCode.Comma))
			{
				num += -1f;
			}
			if (Input.GetKey(KeyCode.Period))
			{
				num += 1f;
			}
			if (Mathf.Abs(num) > 0f)
			{
				float num2 = view.duration * 0.01f;
				float num3 = Mathf.Clamp(view.time + num * num2 * Time.deltaTime, 0f, view.duration);
				view.time = num3;
				SetReplayTime(num3, GetReplayDuration());
			}
			num = 0f;
			if (Input.GetKey(KeyCode.KeypadPlus))
			{
				num += 1f;
			}
			if (Input.GetKey(KeyCode.KeypadMinus))
			{
				num += -1f;
			}
			if (Mathf.Abs(num) > 0f)
			{
				float num4 = 0.12f;
				float num5 = view.speed + num * num4 * Time.unscaledDeltaTime;
				view.speed = num5;
				SetReplaySpeed(num5);
			}
			if (Input.GetKeyDown(KeyCode.M))
			{
				view.speed = 0.05f;
				SetReplaySpeed(view.speed);
			}
			if (Input.GetKeyDown(KeyCode.N))
			{
				view.speed = 0.25f;
				SetReplaySpeed(view.speed);
			}
			if (Input.GetKeyDown(KeyCode.L))
			{
				view.speed = 0.5f;
				SetReplaySpeed(view.speed);
			}
			if (Input.GetKeyDown(KeyCode.K))
			{
				view.speed = 0.66f;
				SetReplaySpeed(view.speed);
			}
			if (Input.GetKeyDown(KeyCode.J))
			{
				view.speed = 1f;
				SetReplaySpeed(view.speed);
			}
			if (game.model.camera.video.isRecording && Input.GetKeyDown(KeyCode.Escape) && game.model.camera.video.isRecording)
			{
				game.model.camera.video.Stop();
				game.model.camera.video.Clear();
				view.SetVideoEncodingState(p_encoding: false);
				view.SetVideoEncodingProgress(0f);
			}
		}

		public void SetReplayClips(GameModel p_game, bool p_excludeBots = false)
		{
			if (ReplayFile.EnableVersion2)
			{
				List<ReplayFile> replaysV = p_game.GetReplaysV2(p_excludeBots);
				SetReplayClips(replaysV);
			}
			else
			{
				List<BlackboxData> replays = p_game.GetReplays();
				SetReplayClips(replays);
			}
		}

		public void SetReplayClips(List<BlackboxData> p_clips)
		{
			replay.model.Clear();
			replay.model.SetClips(p_clips);
		}

		public void SetReplayClips(List<ReplayFile> p_replays)
		{
			replay.model.Clear();
			replay.model.SetClips(p_replays);
		}

		public void InitializeReplay()
		{
			Debug.Log("UISpectateController> InitializeReplay");
			model.SetTargets(replay.model.clips);
			replay.model.Seek(0f);
			model.videoCaptureSizeMode = UISpectateVideoFlags.Size1080;
			model.videoCaptureFPSMode = UISpectateVideoFlags.FPS60;
			model.videoCaptureQualityMode = UISpectateVideoFlags.Quality4;
			view.SetFocusLeaderEnabled(p_flag: false);
			view.SetPlaybackEnabled(p_flag: true);
			view.SetVideoRecordEnabled(p_flag: false);
			view.SetVideoEncodingState(game.model.camera.video.isGenerating);
			model.videoCaptureCropEnabled = false;
			game.model.camera.video.OnState = OnReplayVideoState;
			ignoreChange = true;
			view.SetPlaybackPause(p_flag: false);
			view.speed = 1f;
			view.time = 0f;
			view.raceTime = 0f;
			view.duration = replay.model.duration;
			view.lapCountAllowed = false;
			view.SetLapCountEnabled(p_flag: false);
			model.videoCaptureRangeStart = 0f;
			model.videoCaptureRangeEnd = replay.model.duration;
			view.SetVideoRecordRange(0f, replay.model.duration, 0f, replay.model.duration);
			ignoreChange = false;
			model.ResetTargetRays();
			if (isPlayerObserver)
			{
				view.DisableControls(0f, p_disable_visually: true);
			}
		}

		protected void OnSpectatorNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "spectate.focus@change":
			{
				GamePlayerData focus = model.GetFocus<GamePlayerData>();
				Drone drone = focus.drone;
				DroneCamera camera = base.app.model.game.camera;
				model.ApplyCameraMode(model.cameraMode, camera, drone);
				view.TargetBlink(model.focus, 0.8f, 0.5f);
				RefreshDamageIndicator(focus.id);
				break;
			}
			case "spectate.target.select":
			{
				int p_index = (int)p_data[0];
				p_index = model.GetTargetStandingsIndex(p_index);
				view.targetStepper.Set(p_index);
				break;
			}
			case "network.drone-damage.update":
				if (p_data.Length >= 1)
				{
					int p_networkId = (int)p_data[0];
					RefreshDamageIndicator(p_networkId);
					CameraShake(p_networkId);
				}
				break;
			}
		}

		protected void SpectateUpdate()
		{
			UpdateSpectatorKeyboard();
			UpdateSpectatorTarget();
		}

		public void RefreshDamageIndicator(int p_networkId)
		{
			this.TimerRunOnce(delegate
			{
				GamePlayerData playerByNetworkId = model.GetPlayerByNetworkId(p_networkId);
				GamePlayerData focus = model.GetFocus<GamePlayerData>();
				UIHUDDamageIndicator damage = base.app.view.ui.game.hud.damage;
				if (!(playerByNetworkId.playerId != focus.playerId))
				{
					Tuple<float, float[]> damage2 = base.app.model.network.GetDamage(p_networkId);
					if (base.app.model.network.room == null || !base.app.model.network.room.DRLPilotMode || (model.cameraMode != SpectateCameraModeType.FPV && model.cameraMode != SpectateCameraModeType.Orbit))
					{
						damage.Show(p_flag: false);
						damage.Reset();
					}
					else
					{
						D.Log("UISpecateController> Showing damage for: " + playerByNetworkId.upperName + " FOCUSED: " + focus.upperName + " " + p_networkId);
						damage.Show(view.raceStatsToggle.toggle.isOn);
						if (damage2 == null)
						{
							damage.Reset();
						}
						else
						{
							damage.SetDamageSpectator(damage2.Item1, damage2.Item2);
						}
					}
				}
			}, 1f / 30f);
		}

		private void CameraShake(int p_networkId)
		{
			if (!model.isReplay && model.cameraMode == SpectateCameraModeType.FPV)
			{
				GamePlayerData playerByNetworkId = model.GetPlayerByNetworkId(p_networkId);
				GamePlayerData focus = model.GetFocus<GamePlayerData>();
				if (!(playerByNetworkId.playerId != focus.playerId) && !(base.app.model.game.camera == null))
				{
					base.app.model.game.camera.fx.shake.Shake();
				}
			}
		}

		protected void UpdateSpectatorKeyboard()
		{
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				game.SwitchTabScreen();
			}
			bool flag = RCI.HasNavigationController && RCI.GetButtonUp(ConsoleButtons.Center2);
			if (RCI.HasNavigationController && RCI.GetButtonUp(ConsoleButtons.ActionBottomRow1) && !view.IsControlsEnabled())
			{
				flag = true;
			}
			if (Input.GetKeyUp(KeyCode.Return) && !view.IsControlsEnabled())
			{
				flag = true;
			}
			if (Input.GetKeyUp(KeyCode.Space) && !view.IsControlsEnabled())
			{
				flag = true;
			}
			if (flag)
			{
				bool flag2 = true;
				if (view.IsControlsEnabled())
				{
					view.DisableControls();
					flag2 = false;
				}
				else
				{
					view.EnableControls(p_focus: false);
					flag2 = true;
				}
				base.app.view.ui.navigation.enabled = flag2;
				UINavigation.focus = (flag2 ? view.targetStepper.GetComponent<UINavigation>() : null);
			}
			if (Input.GetKeyDown(KeyCode.F) && isPlayerCommentator)
			{
				view.leaderFocusEnabled = !view.leaderFocusEnabled;
			}
		}

		public void UpdateSpectatorTarget()
		{
			if (!base.enabled)
			{
				return;
			}
			DroneInputTransmitter focus = model.GetFocus<DroneInputTransmitter>();
			if (focus is DroneGhostTransmitter p_target)
			{
				UpdateSpectatorGhost(p_target);
			}
			if (focus is DroneNetworkTransmitter p_target2)
			{
				UpdateSpectatorNetwork(p_target2);
			}
			if (!model.keepFocusOnLeader)
			{
				return;
			}
			m_LeaderFocusTimer += Time.deltaTime;
			if (m_LeaderFocusTimer >= 1f)
			{
				bool num = model.SetFocusAvailable();
				m_LeaderFocusTimer = 0f;
				if (num)
				{
					Debug.Log($"UISpectateController> LateUpdate / Leader changed -  {model.focus}");
				}
			}
		}

		protected void UpdateSpectatorGhost(DroneGhostTransmitter p_target)
		{
			if ((bool)p_target)
			{
				view.raceTime = Mathf.Min(model.GetGameTime(), model.GetGameTime(p_target));
				view.controller.leftStick = p_target.leftInput;
				view.controller.rightStick = p_target.rightInput;
				if (model.changeFocusUponFinish && p_target.elapsed >= p_target.raceTime)
				{
					model.SetFocusAvailable();
				}
			}
		}

		protected void UpdateSpectatorNetwork(DroneNetworkTransmitter p_target)
		{
			if ((bool)p_target)
			{
				Vector4 input = p_target.Input;
				bool flag = !(base.app.model.network == null) && base.app.model.network.room != null && base.app.model.network.room.State == NetworkRoom.StateCode.GameRunning;
				view.raceTime = (flag ? model.GetGameTime(p_target) : 0f);
				view.controller.leftStick = new Vector2(input.x, input.y);
				view.controller.rightStick = new Vector2(input.z, input.w);
				if (model.changeFocusUponFinish && p_target.Actor != null && p_target.Actor.RaceState == NetworkActor.RacerState.Complete)
				{
					model.SetFocusAvailable();
				}
			}
		}

		public void InitializeSpectator()
		{
			Debug.Log("UISpectateController> InitializeSpectator");
			view.SetFocusLeaderEnabled(p_flag: false);
			view.SetPlaybackEnabled(p_flag: false);
			if (isPlayerObserver)
			{
				view.DisableControls(0f, p_disable_visually: true);
				base.app.view.ui.game.hud.Hide(0f);
			}
			if (base.app.inVirtualSeason)
			{
				base.app.view.ui.game.hud.damage.Show(view.raceStatsToggle.toggle.isOn && base.app.model.network.room != null && base.app.model.network.room.DRLPilotMode);
			}
			base.app.view.audio.PlayMusicGame();
			model.ResetDamageData();
			model.SetTargets(base.app.model.game.players);
			ignoreChange = true;
			List<string> upperCaseNames = model.GetUpperCaseNames();
			view.AssertTargetStepper(upperCaseNames);
			view.raceTime = 0f;
			if (view.lapCountAllowed)
			{
				MapData data = base.app.arguments.game.map.data;
				view.SetLapCount(1, data.mode.race.lapCount);
				view.SetLapCountEnabled(p_flag: true);
			}
			ignoreChange = false;
			model.ResetTargetRays();
			GameAudioController gameAudioController = (base.app.inGame ? base.app.controller.game.audio : null);
			if ((bool)gameAudioController)
			{
				gameAudioController.PlayDroneMotor(model.drones);
			}
		}

		public void Exit(bool p_fromUI = false)
		{
			base.app.view.ui.navigation.enabled = true;
			switch (base.app.model.game.type)
			{
			case GameFlag.Replay:
			{
				if (!base.app.model.game)
				{
					break;
				}
				base.app.view.audio.UpdateTimescale(1f);
				SetReplayPlayback(p_flag: false);
				StopReplay();
				if (view.tournamentContext)
				{
					base.app.view.ui.game.preventFooter = true;
					DRLTournamentMatchData tournamentMatchData = base.app.arguments.game.tournamentMatchData;
					if (base.app.view.ui.screens.manager.history.Count > 1)
					{
						base.app.model.game.type = GameFlag.Race;
						base.app.arguments.game.type = GameFlag.Race;
						if (view.current)
						{
							base.app.view.ui.screens.Return();
						}
					}
					else if (tournamentMatchData == null || base.app.controller.network.model.room == null || base.app.arguments.game.isFromBrackets || tournamentMatchData.state == TournamentMatchState.complete || tournamentMatchData.state == TournamentMatchState.fail || tournamentMatchData.state == TournamentMatchState.canceled)
					{
						base.app.view.ui.screens.Open<UITournamentBracketsView>("tournament-brackets-screen");
						base.app.arguments.game.isFromBrackets = false;
					}
					else
					{
						base.app.view.ui.screens.Open<UITournamentResultsView>("tournament-results-screen").matchData = tournamentMatchData;
					}
					break;
				}
				if (base.app.model.game.fromEditor)
				{
					base.app.controller.game.BackToEditor();
					break;
				}
				bool num = base.app.arguments.leaderboardsCampaign != null;
				bool flag = base.app.arguments.lastLeaderboard == DRLAppArguments.LeaderboardType.campaign;
				float p_delay = 0f;
				if (num && flag)
				{
					UITryoutsLeadersView uITryoutsLeadersView = base.app.view.ui.screens.Open<UITryoutsLeadersView>("tryouts-leaders-screen");
					uITryoutsLeadersView.data = base.app.arguments.leaderboardsCampaign.campaign;
					uITryoutsLeadersView.AllowNext(p_flag: false);
					p_delay = 0.2f;
				}
				if (base.app.view.ui.screens.manager.history.Count > 1)
				{
					if (model.game.name == "collectables")
					{
						base.app.model.game.type = GameFlag.Collectable;
						base.app.arguments.game.type = GameFlag.Collectable;
					}
					else
					{
						base.app.model.game.type = GameFlag.Race;
						base.app.arguments.game.type = GameFlag.Race;
					}
					base.app.view.ui.screens.Return();
				}
				else
				{
					base.app.view.ui.screens.Open("leaderboards-screen", p_delay);
				}
				break;
			}
			case GameFlag.Freestyle:
			case GameFlag.Race:
			case GameFlag.Campaign:
			case GameFlag.Sandbox:
				if (base.app.model.game.mode != GameFlag.NetworkMultiplayer)
				{
					base.app.view.ui.screens.Return();
					break;
				}
				base.app.view.audio.PauseAllGameAudio();
				base.app.view.audio.PlayMusicPostGame();
				if (base.app.model.network.room == null && !view.tournamentContext)
				{
					base.app.controller.game.Exit();
					break;
				}
				switch (base.app.model.game.type)
				{
				case GameFlag.Freestyle:
					if ((bool)game.OpenNetworkRoomScreen())
					{
						game.SetTabScreenEnabled(p_flag: false);
					}
					break;
				case GameFlag.Race:
				{
					_ = base.app.model.storage.state.player.profile.isDeveloper;
					if (view.tournamentContext)
					{
						if (view.current)
						{
							if (base.app.controller.network.model.room != null)
							{
								base.app.view.ui.screens.Open<UITournamentRaceCompleteView>("tournament-race-complete-screen").race = game.networkRace;
							}
							else
							{
								base.app.view.ui.screens.Open<UITournamentBracketsView>("tournament-brackets-screen");
							}
						}
						break;
					}
					int count = base.app.view.ui.screens.manager.history.Count;
					game.SetTabScreenEnabled(p_flag: false);
					if (count > 1)
					{
						base.app.view.ui.screens.Return();
					}
					else if ((bool)game.OpenNetworkRoomScreen())
					{
						game.SetTabScreenEnabled(p_flag: false);
					}
					break;
				}
				}
				break;
			case GameFlag.FreeCamera:
				break;
			}
		}

		protected void RefreshCameraToolActiveHint()
		{
			view.ClearCameraToolActive();
			view.ClearCameraToolHints();
			for (int i = 0; i < model.cameraToolTargetFocus.Count; i++)
			{
				MACameraTool cameraToolFocus = model.GetCameraToolFocus(i);
				int cameraToolIndex = model.GetCameraToolIndex(cameraToolFocus);
				view.SetCameraToolActive(cameraToolIndex, p_flag: true);
				Color p_color = (model.isReplay ? model.replays[i].player : model.players[i])?.color ?? DRLColor.gray3;
				view.SetCameraToolHint(cameraToolIndex, i, p_flag: true, p_color);
			}
		}

		protected void LateUpdate()
		{
			if (!base.enabled || !view.enabled || !view.current)
			{
				return;
			}
			UpdateKeyboard();
			Drone focus = model.GetFocus<Drone>();
			Transform focus2 = model.GetFocus<Transform>();
			if ((!focus || !focus2) && m_targetsReady)
			{
				model.SetFocusAvailable();
			}
			switch (model.cameraMode)
			{
			case SpectateCameraModeType.FPV:
				model.UpdateTargetCameraToolCheck(p_notify_focus_change: false);
				break;
			case SpectateCameraModeType.Orbit:
				model.UpdateTargetCameraToolCheck(p_notify_focus_change: false);
				break;
			case SpectateCameraModeType.FreeCamera:
				model.UpdateTargetCameraToolCheck(p_notify_focus_change: false);
				break;
			case SpectateCameraModeType.Manual:
				model.UpdateTargetCameraToolCheck(p_notify_focus_change: false);
				model.UpdateCameraTool(game.model.camera, p_smooth: true);
				break;
			case SpectateCameraModeType.Auto:
				model.UpdateTargetCameraToolCheck(p_notify_focus_change: true);
				model.UpdateCameraTool(game.model.camera, p_smooth: true);
				break;
			}
			if (model.cameraCourseActive)
			{
				model.UpdateCourseCamera(game.model.camera);
			}
			if (model.isReplay)
			{
				ReplayUpdate();
			}
			else
			{
				SpectateUpdate();
			}
			if (m_standingsRect == null)
			{
				GameObject gameObject = game.ui.hud.standings.gameObject;
				if (gameObject != null)
				{
					m_standingsRect = gameObject.GetComponent<RectTransform>();
				}
			}
			if (m_standingsRect != null)
			{
				if (view.playerNameVisible)
				{
					m_standingsRect.anchoredPosition = new Vector2(m_standingsRect.anchoredPosition.x, -175f);
				}
				else
				{
					m_standingsRect.anchoredPosition = new Vector2(m_standingsRect.anchoredPosition.x, -55f);
				}
			}
		}

		protected void UpdateKeyboard()
		{
			if (!DRLUINavigationSystem.IsTyping)
			{
				UpdateKeyboardTargetSelection();
				UpdateKeyboardCameraControls();
				UpdateKeyboardControls();
			}
		}

		protected void UpdateKeyboardTargetSelection()
		{
			int num = -1;
			bool num2 = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
			bool flag = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
			if (!(num2 || flag))
			{
				bool flag2 = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
				if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
				{
					num = 0;
				}
				if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
				{
					num = 1;
				}
				if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
				{
					num = 2;
				}
				if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
				{
					num = 3;
				}
				if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
				{
					num = 4;
				}
				if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
				{
					num = 5;
				}
				if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
				{
					num = 6;
				}
				if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8))
				{
					num = 7;
				}
				if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9))
				{
					num = 8;
				}
				if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
				{
					num = 9;
				}
				if (num >= 0 && flag2)
				{
					num += 10;
				}
				if (num >= 0 && num < model.targets.Count)
				{
					Notify("spectate.target.select", num);
				}
			}
		}

		protected void UpdateKeyboardCameraControls()
		{
			if (Input.GetKeyDown(KeyCode.Z))
			{
				model.SetCameraMode(SpectateCameraModeType.FPV);
			}
			if (Input.GetKeyDown(KeyCode.X))
			{
				model.SetCameraMode(SpectateCameraModeType.Orbit);
			}
			if (Input.GetKeyDown(KeyCode.C))
			{
				model.SetCameraMode(SpectateCameraModeType.FreeCamera);
			}
			if (Input.GetKeyDown(KeyCode.V))
			{
				model.SetCameraMode(SpectateCameraModeType.Auto);
			}
			if (Input.GetKeyDown(KeyCode.B))
			{
				model.SetCameraMode(SpectateCameraModeType.Manual);
			}
			DroneCamera camera = game.model.camera;
			if ((bool)camera && camera.mode == DroneCameraModeType.TPVFree)
			{
				float distanceMin = camera.orbit.constraint.distanceMin;
				float distanceMax = camera.orbit.constraint.distanceMax;
				if (Input.GetKeyDown(KeyCode.A))
				{
					camera.orbit.distance = Mathf.Lerp(distanceMin, distanceMax, 0.7f);
				}
				if (Input.GetKeyDown(KeyCode.S))
				{
					camera.orbit.distance = Mathf.Lerp(distanceMin, distanceMax, 0.1f);
				}
			}
		}

		protected void UpdateKeyboardControls()
		{
			if (!game.model.camera.video.isRecording)
			{
				Input.GetKeyDown(KeyCode.Escape);
			}
			bool keyDown = Input.GetKeyDown(KeyCode.H);
			if (RCI.GetButtonDown(ConsoleButtons.ActionBottomRow2))
			{
				if (view.panelFade.transition > 0.5f || view.panelFade.transition < -0.5f)
				{
					view.panelFade.Fade(0f, 0f, 0f);
				}
				this.TimerRunOnce(delegate
				{
					UINavigation.Focus(view.cameraModeButtons[0]);
				}, 0.5f);
			}
			if (keyDown)
			{
				if (m_right_panel_loop != null)
				{
					m_right_panel_loop.Stop();
					m_right_panel_loop = null;
				}
				if (m_panelToggle != null)
				{
					m_panelToggle.Stop();
					m_panelToggle = null;
				}
				m_panelToggle = this.TimerRunOnce(delegate
				{
					if (view.IsControlsEnabled())
					{
						view.DisableControls();
						UINavigation.focus = null;
						base.app.controller.SetMouseVisible(p_flag: false);
					}
					else
					{
						view.EnableControls(p_focus: true);
						base.app.controller.SetMouseVisible(p_flag: true);
					}
				}, 0.16f);
			}
			if (Input.GetKeyUp(KeyCode.T))
			{
				model.SetDroneTrailMode(SpectateDroneTrailModeType.Off);
			}
			if (Input.GetKeyUp(KeyCode.Y))
			{
				model.SetDroneTrailMode(SpectateDroneTrailModeType.Small);
			}
			if (Input.GetKeyUp(KeyCode.U))
			{
				model.SetDroneTrailMode(SpectateDroneTrailModeType.Medium);
			}
			if (Input.GetKeyUp(KeyCode.I))
			{
				model.SetDroneTrailMode(SpectateDroneTrailModeType.Large);
			}
			if (Input.GetKeyUp(KeyCode.O))
			{
				model.SetDroneTrailMode(SpectateDroneTrailModeType.Auto);
			}
			if (Input.GetKeyUp(KeyCode.Q))
			{
				view.ToggleUserInfo();
			}
			if (Input.GetKeyUp(KeyCode.G))
			{
				view.ToggleUsername();
			}
			if (Input.GetKeyDown(KeyCode.R))
			{
				view.controllerVisible = !view.controllerVisible;
				view.controller.fade.alpha = (view.controllerVisible ? 1f : (-0.1f));
			}
			int num = -1;
			bool flag = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
			bool flag2 = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
			bool flag3 = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
			if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
			{
				num = 0;
			}
			if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
			{
				num = 1;
			}
			if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
			{
				num = 2;
			}
			if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
			{
				num = 3;
			}
			if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
			{
				num = 4;
			}
			if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
			{
				num = 5;
			}
			if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
			{
				num = 6;
			}
			if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8))
			{
				num = 7;
			}
			if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9))
			{
				num = 8;
			}
			if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
			{
				num = 9;
			}
			if (num >= 0)
			{
				if (flag)
				{
					if (flag2)
					{
						num += 10;
					}
				}
				else if (flag3)
				{
					num += 10;
				}
				if (num < model.cameraTools.Count && flag2)
				{
					model.SetCameraToolFocus(num);
				}
				if (num < model.courseCameras.Count && flag)
				{
					model.SetCourseCameraFocus(num);
				}
			}
			if (flag && Input.GetKeyDown(KeyCode.LeftArrow))
			{
				float prevEventTime = model.GetPrevEventTime(view.time);
				UpdateReplayTime(prevEventTime);
			}
			if (flag && Input.GetKeyDown(KeyCode.RightArrow))
			{
				float nextEventTime = model.GetNextEventTime(view.time);
				UpdateReplayTime(nextEventTime);
			}
		}
	}
}

using UnityEngine;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIGameViewerController : Controller<DRLApp>
	{
		public GameController game => base.app.controller.game;

		public UIGameViewerView view => AssertLocal<UIGameViewerView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (!view.current)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				game.FadeBlur(0f, 0f);
				base.app.view.ui.SetDark(p_flag: false);
				base.app.view.ui.footer.Hide(0f);
				Activity.RunOnce(delegate
				{
					UINavigation.focus = view.controls.playback.targetStepper.GetComponent<UINavigation>();
				}, 1f / 30f);
				break;
			case "viewer.controls.nav.settings@click":
				if ((bool)base.app.view.ui.screens.Open("settings-screen", 0f))
				{
					game.SetTabScreenEnabled(p_flag: false);
				}
				break;
			case "viewer.controls.nav.exit@click":
				if ((bool)base.app.model.game)
				{
					Exit();
				}
				break;
			case "input.mouse-cursor.show":
			{
				UIGameViewerControlsPlaybackPanel playback = view.controls.playback;
				bool flag = view.ControlsEnabled();
				DroneCamera camera = game.model.camera;
				if ((bool)camera)
				{
					_ = camera.mode;
				}
				if (playback.oldCameraMode == ViewerCameraModeType.Orbit && !flag)
				{
					base.app.controller.SetMouseVisible(p_flag: false);
				}
				break;
			}
			case "viewer.form.event@click":
				OnControlsFormEvent(p_event, p_target, p_is_change: false);
				break;
			case "viewer.form.event@change":
				OnControlsFormEvent(p_event, p_target, p_is_change: true);
				break;
			}
		}

		public void Exit()
		{
			base.app.view.ui.navigation.enabled = true;
			GameFlag type = base.app.model.game.type;
			if ((uint)(type - 13) > 2u && type != GameFlag.Sandbox)
			{
				return;
			}
			if (base.app.model.game.mode != GameFlag.NetworkMultiplayer)
			{
				base.app.view.ui.screens.Return();
				return;
			}
			if (base.app.model.network.room == null)
			{
				base.app.controller.game.Exit();
				return;
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
				if (base.app.arguments.game.tournamentData != null && (base.app.controller.network.model.room == null || base.app.controller.network.model.room.HeatIdx == base.app.controller.network.model.room.MaxHeats))
				{
					base.app.view.ui.screens.Open<UITournamentBracketsView>("tournament-brackets-screen");
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
		}

		protected virtual void OnControlsFormEvent(string p_event, Object p_target, bool p_is_change)
		{
			bool flag = p_is_change;
			string text = (p_target ? p_target.name : "");
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			_ = view.controls.playback;
			UIGameViewerInformationLayer info = view.info;
			switch (text)
			{
			case "playback-camera":
				if (flag)
				{
					ViewerCameraModeType value = (ViewerCameraModeType)(p_target as DRLIntStepperView).value;
					SetCameraMode(value);
				}
				break;
			case "playback-target":
				if (flag)
				{
					int index = (p_target as DRLStepperView).index;
					OnTargetChange(index);
				}
				break;
			case "info-name":
				if (!flag)
				{
					DRLToggleView dRLToggleView4 = p_target as DRLToggleView;
					info.SetUserVisible(dRLToggleView4.toggle.isOn);
				}
				break;
			case "info-race-stats":
				if (!flag)
				{
					DRLToggleView dRLToggleView3 = p_target as DRLToggleView;
					info.SetRaceStatsVisible(dRLToggleView3.toggle.isOn);
				}
				break;
			case "info-controller":
				if (!flag)
				{
					DRLToggleView dRLToggleView2 = p_target as DRLToggleView;
					info.controller.fade.alpha = (dRLToggleView2.toggle.isOn ? 1f : (-0.1f));
				}
				break;
			case "info-motors":
				if (!flag)
				{
					DRLToggleView dRLToggleView = p_target as DRLToggleView;
					info.SetMotorsVisible(dRLToggleView.toggle.isOn);
				}
				break;
			}
		}

		public void SetCameraMode(ViewerCameraModeType p_mode)
		{
			if (p_mode != ViewerCameraModeType.None)
			{
				OnCameraModeChange(p_mode);
				bool flag = false;
				switch (p_mode)
				{
				case ViewerCameraModeType.FPV:
					flag = true;
					break;
				case ViewerCameraModeType.Orbit:
					flag = false;
					base.app.controller.SetMouseVisible(view.ControlsEnabled());
					break;
				case ViewerCameraModeType.FreeCamera:
					flag = false;
					break;
				}
				flag = flag && base.app.model.storage.state.player.settings.graphics.motionBlur;
				DroneCamera camera = base.app.model.game.camera;
				if ((bool)camera && (bool)camera.fx)
				{
					camera.fx.SetMotionBlurEnabled(flag);
				}
			}
		}

		protected virtual void OnTargetChange(int p_index)
		{
		}

		protected virtual void OnCameraModeChange(ViewerCameraModeType p_mode)
		{
		}

		protected virtual int OnKeyboardTargetSelect(int p_id)
		{
			return p_id;
		}

		protected virtual void UpdateKeyboardTargetSelection()
		{
			int num = -1;
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
				num = OnKeyboardTargetSelect(num);
				if (num >= 0)
				{
					OnTargetChange(num);
				}
			}
		}

		protected virtual void UpdateKeyboardCameraControls()
		{
			ViewerCameraModeType viewerCameraModeType = ViewerCameraModeType.None;
			if (Input.GetKeyDown(KeyCode.Z))
			{
				viewerCameraModeType = ViewerCameraModeType.FPV;
			}
			if (Input.GetKeyDown(KeyCode.X))
			{
				viewerCameraModeType = ViewerCameraModeType.Orbit;
			}
			if (Input.GetKeyDown(KeyCode.C))
			{
				viewerCameraModeType = ViewerCameraModeType.FreeCamera;
			}
			SetCameraMode(viewerCameraModeType);
			UIGameViewerControlsPlaybackPanel playback = view.controls.playback;
			if (viewerCameraModeType != ViewerCameraModeType.None)
			{
				playback.oldCameraMode = viewerCameraModeType;
			}
			DroneCamera camera = game.model.camera;
			if ((bool)camera && camera.mode == DroneCameraModeType.TPVFree)
			{
				float distanceMin = camera.orbit.constraint.distanceMin;
				float distanceMax = camera.orbit.constraint.distanceMax;
				if (Input.GetKeyDown(KeyCode.F))
				{
					camera.orbit.distance = Mathf.Lerp(distanceMin, distanceMax, 0.7f);
				}
				if (Input.GetKeyDown(KeyCode.V))
				{
					camera.orbit.distance = Mathf.Lerp(distanceMin, distanceMax, 0.1f);
				}
			}
		}

		protected virtual void UpdateKeyboardControls()
		{
			if (Input.GetKeyDown(KeyCode.H))
			{
				if (view.ControlsEnabled())
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
			}
			UIGameViewerControlsPlaybackPanel playback = view.controls.playback;
			if (Input.GetKeyDown(KeyCode.R))
			{
				playback.controllerVisible = !playback.controllerVisible;
				view.info.controller.fade.alpha = (playback.controllerVisible ? 1f : (-0.1f));
			}
		}
	}
}

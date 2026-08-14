using System.Collections.Generic;
using UnityEngine;
using drl.network;
using drl.sim;
using drl.sim.rci;
using thelab.core;

namespace drl.game
{
	public class UISpectatorController : UIGameViewerController
	{
		public new UISpectatorView view => AssertLocal<UISpectatorView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (!view.current || !view.enabled)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				base.game.ui.hud.Show();
				base.game.ui.hud.standingsFade.FadeIn(0.25f);
				base.app.view.ui.footer.droneButton.interactable = false;
				break;
			case "game.intro.animation@complete":
				view.enabled = false;
				view.info.time = 0f;
				RunOnce(0.1f, delegate
				{
					view.RefreshAndFocus();
				});
				break;
			case "game.count@complete":
				view.enabled = true;
				break;
			case "network.player.racer":
			{
				NetworkActor networkActor = (NetworkActor)p_data[0];
				if (base.app.model.network.room != null)
				{
					if (networkActor.IsLocal)
					{
						Exit();
					}
					view.RefreshAndFocus();
				}
				break;
			}
			case "network.race.end":
				Debug.Log("UISpectatorController> RaceEnd");
				if (base.app.model.network.room.Local.IsSpectator)
				{
					base.app.view.audio.StopAllGameAudio();
					base.app.view.audio.PlayMusicPostGame();
					Exit();
				}
				break;
			case "network.remote.transmitter.added":
				_ = base.app.model.network.room.Local;
				Debug.Log("UISpectatorController> update list");
				view.RefreshAndFocus();
				break;
			}
			base.OnNotification(p_event, p_target, p_data);
		}

		protected override void OnControlsFormEvent(string p_event, Object p_target, bool p_is_change)
		{
			base.OnControlsFormEvent(p_event, p_target, p_is_change);
			if (!string.IsNullOrEmpty(p_target ? p_target.name : ""))
			{
				_ = view.controls.playback;
				_ = view.info;
			}
		}

		protected override void OnCameraModeChange(ViewerCameraModeType p_mode)
		{
			if ((bool)view.focus)
			{
				Drone drone = view.focus.drone;
				base.game.replay.SetCameraMode(p_mode, drone);
				switch (p_mode)
				{
				case ViewerCameraModeType.FPV:
					SetDroneTrailDuration(1f);
					break;
				case ViewerCameraModeType.FreeCamera:
					SetDroneTrailDuration(8f);
					break;
				case ViewerCameraModeType.Orbit:
					SetDroneTrailDuration(8f);
					break;
				}
			}
		}

		protected void SetDroneTrailDuration(float p_duration)
		{
			List<DroneInputTransmitter> list = view.targets;
			if (list == null)
			{
				list = new List<DroneInputTransmitter>();
			}
			for (int i = 0; i < list.Count; i++)
			{
				DroneInputTransmitter droneInputTransmitter = list[i];
				if ((bool)droneInputTransmitter.drone && droneInputTransmitter.drone.ready)
				{
					droneInputTransmitter.drone.renderer.SetTrailsDuration(p_duration);
				}
			}
		}

		protected override int OnKeyboardTargetSelect(int p_id)
		{
			if (Input.GetKey(KeyCode.LeftAlt))
			{
				return p_id;
			}
			bool multiplayer = base.game.model.multiplayer;
			GameFlag type = base.game.model.type;
			if ((uint)(type - 14) <= 1u)
			{
				RaceController raceController = (multiplayer ? base.game.networkRace : base.game.race);
				if (!raceController)
				{
					Debug.LogWarning("UISpectatorController> Race Controller not available");
				}
				else if (raceController.model.Rankings.Count > 0)
				{
					int index = Mathf.Clamp(p_id, 0, raceController.model.Rankings.Count - 1);
					Drone drone = raceController.model.Rankings[index].drone;
					index = -1;
					for (int i = 0; i < view.targets.Count; i++)
					{
						if (view.targets[i].drone == drone)
						{
							index = i;
							break;
						}
					}
					if (index >= 0)
					{
						return index;
					}
				}
			}
			return p_id;
		}

		protected override void OnTargetChange(int p_index)
		{
			if (p_index >= 0 && p_index < view.targets.Count)
			{
				if (view.controls.playback.targetIndex != p_index)
				{
					view.controls.playback.targetIndex = p_index;
				}
				DroneInputTransmitter droneInputTransmitter = view.targets[p_index];
				if (!(droneInputTransmitter == view.focus))
				{
					view.SetFocus(droneInputTransmitter);
				}
			}
		}

		protected void Update()
		{
			if (!base.enabled || !view.current || !view.controller || !view.enabled || DRLUINavigationSystem.IsTyping)
			{
				return;
			}
			view.UpdateFocus();
			UpdateKeyboardTargetSelection();
			UpdateKeyboardCameraControls();
			UpdateKeyboardControls();
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				base.game.SwitchTabScreen();
			}
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				NetworkModel network = base.app.model.network;
				if (network.InRoom && network.room.GameMode == NetworkRoom.GameType.Freestyle && network.room.Local.IsSpectator)
				{
					network.SwitchToRacer();
				}
				Exit();
			}
			bool flag = RCI.HasNavigationController && RCI.GetButtonUp(ConsoleButtons.Center2);
			if (RCI.HasNavigationController && RCI.GetButtonUp(ConsoleButtons.ActionBottomRow1) && !view.ControlsEnabled())
			{
				flag = true;
			}
			if (Input.GetKeyUp(KeyCode.Return) && !view.ControlsEnabled())
			{
				flag = true;
			}
			if (Input.GetKeyUp(KeyCode.Space) && !view.ControlsEnabled())
			{
				flag = true;
			}
			if (flag)
			{
				bool flag2 = true;
				if (view.ControlsEnabled())
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
				UINavigation.focus = (flag2 ? view.controls.playback.targetStepper.GetComponent<UINavigation>() : null);
			}
		}
	}
}

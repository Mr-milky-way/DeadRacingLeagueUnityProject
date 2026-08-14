using System.Collections.Generic;
using UnityEngine;
using drl.sim;

namespace drl.game
{
	public class UISpectatorView : UIGameViewerView
	{
		public GameTypeController controller;

		public DroneInputTransmitter focus;

		public List<DroneInputTransmitter> targets;

		private bool m_lock_refresh;

		public float GetGameTime()
		{
			if (!controller)
			{
				return 0f;
			}
			float num = 0f;
			float num2 = 0f;
			if (controller is RaceController)
			{
				RaceController obj = controller as RaceController;
				num = obj.model.timeStart;
				num2 = obj.GetGlobalTime();
			}
			if (controller is NetworkRaceController)
			{
				NetworkRaceController obj2 = controller as NetworkRaceController;
				num = 0f;
				num2 = obj2.GetGlobalTime();
			}
			return num2 - num;
		}

		public float GetGameTime(DroneInputTransmitter p_target)
		{
			if (!controller)
			{
				return 0f;
			}
			if (controller is RaceController)
			{
				return (controller as RaceController).model.GetRaceTime(p_target ? p_target.drone : null);
			}
			if (controller is NetworkRaceController)
			{
				return (controller as NetworkRaceController).model.GetRaceTime(p_target ? p_target.drone : null);
			}
			return 0f;
		}

		public void Initialize(GameTypeController p_controller, bool p_allow_stats = true)
		{
			controller = p_controller;
			if (!controller)
			{
				Debug.LogWarning("UISpectatorView> Initialized with invalid game controller.");
				return;
			}
			if (!base.app.model.game.simulation)
			{
				Debug.LogWarning("UISpectatorView> Initialize - Failed to find the simulation");
				return;
			}
			List<DroneInputTransmitter> list = SetTargets();
			notificationLock = true;
			controls.fade.alpha = 1f;
			UIGameViewerControlsPlaybackPanel playback = controls.playback;
			playback.playerNameVisible = true;
			playback.controllerVisible = false;
			info.controller.fade.alpha = -0.1f;
			playback.raceStatsVisible = p_allow_stats;
			playback.motorsVisible = false;
			playback.oldCameraMode = ViewerCameraModeType.FPV;
			info.SetRaceStatsVisible(p_allow_stats);
			playback.raceStatsToggle.gameObject.SetActive(p_allow_stats);
			SetFocus(focus ? focus : ((list.Count > 0) ? list[0] : null));
			notificationLock = false;
		}

		public override void SetMode(ViewerModeType p_mode)
		{
			base.enabled = p_mode == mode;
			UIReplayView component = GetComponent<UIReplayView>();
			if ((bool)component)
			{
				component.enabled = !base.enabled;
			}
			if (p_mode == ViewerModeType.Spectator)
			{
				SetDirectorModeAllowed(p_flag: false);
				controls.playback.SetSpectatorMode();
			}
		}

		public List<DroneInputTransmitter> SetTargets()
		{
			DroneSimulation simulation = base.app.model.game.simulation;
			List<string> list = new List<string>();
			List<DroneInputTransmitter> list2 = simulation.transmitters.list;
			list2 = new List<DroneInputTransmitter>(list2);
			for (int i = 0; i < list2.Count; i++)
			{
				if (list2[i] is DroneRCTransmitter)
				{
					list2.RemoveAt(i--);
				}
				else if (!list2[i].drone)
				{
					list2.RemoveAt(i--);
				}
			}
			int count = list2.Count;
			Debug.Log("UISpectatorView> SetTargets - transmitters[" + count + "]\n");
			for (int j = 0; j < count; j++)
			{
				DroneInputTransmitter droneInputTransmitter = list2[j];
				if ((bool)droneInputTransmitter)
				{
					GamePlayerData playerData = base.app.model.game.GetPlayerData(droneInputTransmitter.drone);
					if (playerData != null)
					{
						list.Add(playerData.name.ToUpper());
					}
				}
			}
			controls.playback.SetTargets(list);
			targets = list2;
			return list2;
		}

		public void SetFocus(DroneInputTransmitter p_target)
		{
			UIGameViewerControlsPlaybackPanel playback = controls.playback;
			focus = p_target;
			GamePlayerData gamePlayerData = (focus ? base.app.model.game.GetPlayerData(focus.drone) : null);
			if (gamePlayerData != null)
			{
				info.SetUser(gamePlayerData);
				info.controller.SetController(focus.GetControllerType());
				if ((bool)focus.drone)
				{
					base.app.controller.game.replay.SetCameraMode(playback.oldCameraMode, focus.drone);
					if (focus.drone.body != null && focus.drone.body.frame != null && focus.drone.body.frame.camera != null)
					{
						focus.drone.body.frame.camera.tilt = gamePlayerData.cameraTilt;
					}
				}
			}
			UpdateFocus();
		}

		public void UpdateFocus()
		{
			if (!base.enabled)
			{
				return;
			}
			DroneInputTransmitter droneInputTransmitter = focus;
			if (!droneInputTransmitter)
			{
				RefreshAndFocus();
				return;
			}
			if (droneInputTransmitter is DroneGhostTransmitter)
			{
				UpdateFocusGhost(droneInputTransmitter as DroneGhostTransmitter);
			}
			if (droneInputTransmitter is DroneNetworkTransmitter)
			{
				UpdateFocusNetwork(droneInputTransmitter as DroneNetworkTransmitter);
			}
		}

		protected void UpdateFocusGhost(DroneGhostTransmitter p_target)
		{
			if ((bool)p_target)
			{
				_ = controls.playback;
				info.time = Mathf.Min(GetGameTime(), GetGameTime(p_target));
				info.controller.leftStick = p_target.leftInput;
				info.controller.rightStick = p_target.rightInput;
				info.rpm = p_target.rpm;
			}
		}

		protected void UpdateFocusNetwork(DroneNetworkTransmitter p_target)
		{
			if ((bool)p_target)
			{
				_ = controls.playback;
				Vector4 input = p_target.Input;
				info.time = Mathf.Min(GetGameTime(), GetGameTime(p_target));
				info.controller.leftStick = new Vector2(input.x, input.y);
				info.controller.rightStick = new Vector2(input.z, input.w);
				info.rpm = p_target.NetworkRPMs;
			}
		}

		public void RefreshAndFocus()
		{
			if (!m_lock_refresh)
			{
				m_lock_refresh = true;
				List<DroneInputTransmitter> list = (targets = SetTargets());
				SetFocus(focus ? focus : ((list.Count <= 0) ? null : list[0]));
				m_lock_refresh = false;
			}
		}
	}
}

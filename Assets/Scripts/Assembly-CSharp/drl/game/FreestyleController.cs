using System.Collections.Generic;
using UnityEngine;
using drl.sim;
using drl.sim.rci;

namespace drl.game
{
	public class FreestyleController : GameTypeController
	{
		private RectTransform m_standingsRect;

		private CanvasGroup m_userInfoCanvasGroup;

		public FreestyleModel model => AssertLocal<FreestyleModel>("model");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			base.OnNotification(p_event, p_target, p_data);
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
			base.game.model.level.track.pathTrace.gameObject.SetActive(value: false);
			if (!PlayTrackAnimation() && !PlayPodiumAnimation())
			{
				Debug.LogWarning("FreestyleController> Failed to play intro animation!");
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

		protected override void OnGameReady()
		{
			DroneSimulation simulation = base.game.model.simulation;
			DroneCamera droneCamera = simulation.cameras.Get(0);
			simulation.drones.SetArmed(p_flag: true);
			Drone drone = simulation.drones.Get(0);
			FCMode activeFCMode = base.app.model.storage.state.player.activeFCMode;
			SetDroneFCMode(drone, activeFCMode);
			droneCamera.SetFPV(drone);
			base.app.view.audio.ResetGameRadioSignal(drone.gameObject);
			base.game.model.level.radio.boundsSignal = 1f;
			droneCamera.fx.radio = 1f;
			base.ui.hud.Fade(1f, 0.5f, 1f);
			base.ui.hud.SetStandingsCount(24, p_has_positions: false);
			model.SetStandings(base.ui.hud.standings, model.playerStandings);
			bool num = base.app.model.storage.state.player.garage.IsOfficial(drone.rig);
			bool flag = base.app.model.storage.state.player.garage.CanUseDamage();
			RCI.SetThrottleCap((num || flag) ? 80f : (-1f));
			UnfreezeDrones();
			simulation.drones.SetReceiver(p_flag: true);
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
			{
				Debug.Log("FreestyleController> " + p_command.type);
				if (base.game.model.paused)
				{
					break;
				}
				Drone playerDrone = base.game.model.playerDrone;
				GameStateModel gameStateModel = base.app.model.storage.state.player.settings.game;
				if (playerDrone == null)
				{
					return false;
				}
				if (base.app.model.network.room != null && base.app.model.network.room.IsSpectator)
				{
					return false;
				}
				if (!gameStateModel.armAndTurtle)
				{
					playerDrone.Fix();
					if (playerDrone.body != null && playerDrone.body.frame != null && playerDrone.body.frame.batteries != null)
					{
						foreach (DroneBattery battery in playerDrone.body.frame.batteries)
						{
							if (battery != null)
							{
								battery.Recharge();
							}
						}
					}
					base.game.PodiumReset(playerDrone);
					playerDrone.fc.armed = true;
				}
				else
				{
					base.game.DroneArmDisarm(playerDrone);
				}
				return false;
			}
			}
			return base.OnGameCommand(p_command);
		}

		protected override void Update()
		{
			if (!introComplete)
			{
				return;
			}
			UpdateTabScreen();
			base.Update();
			if (m_standingsRect == null)
			{
				GameObject gameObject = base.game.ui.hud.standings.gameObject;
				if (gameObject != null)
				{
					m_standingsRect = gameObject.GetComponent<RectTransform>();
				}
			}
			if (!(m_standingsRect != null))
			{
				return;
			}
			if (m_userInfoCanvasGroup == null)
			{
				m_userInfoCanvasGroup = base.game.ui.hud.userInfo.gameObject.GetComponent<CanvasGroup>();
			}
			if (m_userInfoCanvasGroup != null)
			{
				if (m_userInfoCanvasGroup.alpha > 0f)
				{
					m_standingsRect.anchoredPosition = new Vector2(m_standingsRect.anchoredPosition.x, -175f);
				}
				else
				{
					m_standingsRect.anchoredPosition = new Vector2(m_standingsRect.anchoredPosition.x, -55f);
				}
			}
		}

		public void UpdateTabScreen()
		{
			if (base.ui.hud.standingsFade.alpha > 0f)
			{
				model.SetStandings(base.ui.hud.standings, model.playerStandings);
			}
		}
	}
}

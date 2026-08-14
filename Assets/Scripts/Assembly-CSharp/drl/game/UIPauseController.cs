using System;
using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIPauseController : Controller<DRLApp>
	{
		protected bool m_ignore_form_notification;

		private bool m_dashboardEnabled;

		private Activity m_peekTimer;

		private Activity update_tune_rating;

		public Drone currentDrone;

		private Activity update_map_rating;

		public MapData currentMap;

		public SettingsStateModel settings => base.app.model.storage.state.player.settings;

		public GameController game => base.app.controller.game;

		public UIPauseView view => AssertLocal<UIPauseView>("view");

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (this == null || base.gameObject == null || view == null || base.app.view.ui.screens.current != view.screen || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "game.pause":
				break;
			case "game.unpause":
				break;
			case "ui.screen@open":
			{
				if (p_data[0] as UIScreen != view.screen)
				{
					break;
				}
				Timer.Set(view, "ignoreReturn", 0.05f, false);
				m_ignore_form_notification = true;
				bool flag = game.model.mode == GameFlag.NetworkMultiplayer;
				bool fromEditor = game.model.fromEditor;
				view.cameraFovSlider.slider.minValue = base.app.model.storage.state.player.settings.tuning.cameraMinFOV;
				view.cameraFovSlider.slider.maxValue = base.app.model.storage.state.player.settings.tuning.cameraMaxFOV;
				view.SetGame(game.model.type, flag, fromEditor, game.model.playerDrone);
				if (flag)
				{
					bool roomStateChange = false;
					bool roomAccess = true;
					NetworkModel network2 = base.app.model.network;
					if (network2 != null && network2.room != null)
					{
						view.SetSpectator(network2.room.Local.IsSpectator);
						bool canRace = network2.room.CanRace;
						bool flag2 = network2.room.CanSpectate && network2.room.RacersCount >= 2;
						roomStateChange = (network2.room.IsSpectator ? canRace : flag2);
					}
					if (game.model.type == GameFlag.Race)
					{
						roomStateChange = false;
						roomAccess = false;
					}
					view.SetRoomStateChange(roomStateChange);
					view.SetRoomAccess(roomAccess);
				}
				DroneCamera camera = game.model.camera;
				GameCameraMode cameraMode = game.GetCameraMode(camera);
				view.SetCameraMode(cameraMode);
				view.cameraFovSlider.slider.onValueChanged.AddListener(view.OnFOVChange);
				if (cameraMode == GameCameraMode.FPV)
				{
					view.cameraFov = CameraLens.V2HFov(camera.fov);
				}
				else
				{
					FCProfileData active = base.app.model.storage.state.player.settings.tuning.GetActive();
					if (active != null)
					{
						view.cameraFov = CameraLens.V2HFov(active.fov);
					}
					else
					{
						view.cameraFov = view.cameraFovSlider.slider.minValue;
					}
				}
				if (view.IsGoldbergDrone())
				{
					view.SetProModeOnly();
				}
				else
				{
					view.SetAllModes();
				}
				if ((bool)game.model.playerDrone)
				{
					view.SetDrone(game.model.playerDrone);
				}
				m_ignore_form_notification = false;
				currentDrone = game.model.playerDrone;
				GetDroneRating();
				if (base.app.scene.map != null)
				{
					currentMap = base.app.scene.map.data;
					GetMapRating();
				}
				break;
			}
			case "game.pause.form.event@click":
				OnFormNotification(p_target, p_change: false);
				break;
			case "game.pause.form.event@change":
				OnFormNotification(p_target, p_change: true);
				break;
			case "game.pause.pro-card@click":
			{
				view.SetFCMode(FCMode.Pro);
				game.model.fcMode = base.app.model.storage.state.player.activeFCMode;
				Drone playerDrone = game.model.playerDrone;
				if ((bool)playerDrone && (bool)playerDrone.fc)
				{
					playerDrone.fc.SetMode(FlightControllerMode.Pro);
					if (base.app.model.storage.state.player.garage.CanUseDamage(playerDrone.rig))
					{
						RCI.SetThrottleCap(80f);
						GameStateModel gameStateModel = base.app.model.storage.state.player.settings.game;
						float p_resistance = (base.app.inVirtualSeason ? gameStateModel.batteryResistance : 18f);
						playerDrone.SetBatteryResistance(p_sag: true, base.app.inVirtualSeason, gameStateModel.batteryCapacity, p_resistance);
						playerDrone.crashEnabled = base.app.model.storage.state.player.activeFCMode == FCMode.DRLPilot;
						playerDrone.UseCrashDelay(game.model.type, game.model.mode == GameFlag.NetworkMultiplayer);
					}
					else
					{
						playerDrone.ResetBatteryResistance();
						playerDrone.crashEnabled = false;
						RCI.SetThrottleCap(-1f);
					}
					base.app.view.ui.game.hud.damage.Show(playerDrone.crashEnabled && base.app.model.storage.state.player.settings.game.raceStats && game.model.type != GameFlag.Mission);
					Notify("garage.drone.fc-changed", (int)base.app.model.storage.state.player.activeFCMode, playerDrone.rig.diameter);
				}
				base.app.view.audio.PlayUILoadingSuccess();
				break;
			}
			case "ui.screen.return@click":
				view.cameraFovSlider.slider.onValueChanged.RemoveListener(view.OnFOVChange);
				view.fovSliderGroup.ignoreParentGroups = false;
				view.tiltSliderGroup.ignoreParentGroups = false;
				if (!view.ignoreReturn)
				{
					Notify("game.pause.return@click");
				}
				break;
			case "network.player@update":
			{
				NetworkModel network = base.app.model.network;
				if (!(network == null) && network.room != null)
				{
					view.SetSpectator(network.room.Local.IsSpectator);
				}
				break;
			}
			case "settings.controller.disconnect":
			case "settings.controller.connect":
				view.RefreshNavigationTooltips();
				break;
			}
		}

		protected void OnFormNotification(UnityEngine.Object p_target, bool p_change)
		{
			if (m_ignore_form_notification)
			{
				return;
			}
			string text = (p_target ? p_target.name : "");
			bool flag = p_change;
			Drone d = game.model.playerDrone;
			DroneCamera camera = game.model.camera;
			if (text == null)
			{
				return;
			}
			switch (text)
			{
			case "camera-fov":
			{
				if (!flag)
				{
					break;
				}
				DRLSliderView dRLSliderView = p_target as DRLSliderView;
				if ((bool)camera)
				{
					game.UIPeek();
					view.fovSliderGroup.ignoreParentGroups = true;
					if (m_peekTimer != null)
					{
						m_peekTimer.Stop();
					}
					m_peekTimer = this.TimerRunOnce(delegate
					{
						view.fovSliderGroup.ignoreParentGroups = false;
					}, 1.2f);
					float num2 = CameraLens.H2VFov(Mathf.Round(dRLSliderView.slider.value));
					if (camera.mode == DroneCameraModeType.FPV)
					{
						camera.fov = num2;
					}
					else
					{
						camera.fov = 45f;
					}
					d.body.frame.camera.fov = num2;
					base.app.model.storage.state.player.settings.tuning.UpdateCameraDelayed(-1f, num2);
				}
				break;
			}
			case "camera-tilt":
			{
				if (!flag)
				{
					break;
				}
				DRLSliderView dRLSliderView = p_target as DRLSliderView;
				if ((bool)d)
				{
					game.UIPeek();
					view.tiltSliderGroup.ignoreParentGroups = true;
					if (m_peekTimer != null)
					{
						m_peekTimer.Stop();
					}
					m_peekTimer = this.TimerRunOnce(delegate
					{
						view.tiltSliderGroup.ignoreParentGroups = false;
					}, 1.2f);
					float num = Mathf.Round(dRLSliderView.slider.value);
					d.body.frame.camera.tilt = num;
					base.app.model.storage.state.player.settings.tuning.UpdateCameraDelayed(num);
				}
				break;
			}
			case "camera-mode":
				if (flag && (bool)camera)
				{
					DRLStepperView obj = p_target as DRLStepperView;
					game.UIPeek(1f);
					GameCameraMode index = (GameCameraMode)obj.index;
					game.SetCameraMode(d, camera, index);
					if (index == GameCameraMode.FPV)
					{
						camera.hfov = view.cameraFovSlider.slider.value;
					}
				}
				break;
			case "mode-beginner":
				if (!flag)
				{
					if ((bool)d && (bool)d.fc)
					{
						d.fc.SetMode(FlightControllerMode.Beginner);
						d.ResetBatteryResistance();
						d.crashEnabled = false;
						Notify("garage.drone.fc-changed", 1, d.rig.diameter);
						base.app.view.ui.game.hud.damage.Show(p_flag: false);
					}
					view.SetFCMode(FCMode.Beginner, p_toggleHardcore: false);
					game.model.fcMode = FCMode.Beginner;
					base.app.model.storage.state.player.activeFCMode = FCMode.Beginner;
					base.app.view.audio.PlayUILoadingSuccess();
					RCI.SetThrottleCap(-1f);
				}
				break;
			case "mode-intermediate":
				if (!flag)
				{
					if ((bool)d && (bool)d.fc)
					{
						d.fc.SetMode(FlightControllerMode.Intermediate);
						d.ResetBatteryResistance();
						d.crashEnabled = false;
						Notify("garage.drone.fc-changed", 2, d.rig.diameter);
						base.app.view.ui.game.hud.damage.Show(p_flag: false);
					}
					view.SetFCMode(FCMode.Intermediate, p_toggleHardcore: false);
					game.model.fcMode = FCMode.Intermediate;
					base.app.model.storage.state.player.activeFCMode = FCMode.Intermediate;
					base.app.view.audio.PlayUILoadingSuccess();
					RCI.SetThrottleCap(-1f);
				}
				break;
			case "settings":
				base.app.view.ui.screens.Open("settings-screen", 0f);
				break;
			case "system":
				base.app.view.ui.screens.Open("settings-system-screen", 0f);
				break;
			case "controller-setup":
				base.app.view.ui.screens.Open("calibration-menu-screen", 0f);
				break;
			case "tuning":
				if (view.tuningCard.interactable)
				{
					base.app.view.ui.screens.Open<UISettingsTuningView>("settings-tuning-screen").openedFromDashboard = false;
				}
				break;
			case "change-game":
				if (game.model.type == GameFlag.Mission)
				{
					Notify("game.change-mission@click");
				}
				else
				{
					Notify("game.change-game@click");
				}
				break;
			case "restart-game":
				if (base.app.arguments.game.quest != null)
				{
					ServiceModel service = base.app.model.service;
					if ((bool)service && base.app.arguments.game.quest.tags.Contains(GameFlag.DMVQuest))
					{
						service.StopTimer(base.app.arguments.game.mission.guid, delegate
						{
						});
					}
				}
				m_ignore_form_notification = true;
				base.app.view.audio.PlayUIGenericSuccess();
				game.Restart();
				break;
			case "exit-game":
				m_ignore_form_notification = true;
				base.app.view.audio.PlayUIGenericSuccess();
				base.app.arguments.game.tryouts = false;
				Notify("game.simulation.drone.flight-time@update", base.app.model.storage.state.player.garage.currentRigData);
				if (base.app.arguments.game.quest != null)
				{
					ServiceModel service2 = base.app.model.service;
					if ((bool)service2 && base.app.arguments.game.quest.tags.Contains(GameFlag.DMVQuest))
					{
						service2.StopTimer(base.app.arguments.game.mission.guid, delegate
						{
						});
					}
				}
				base.app.model.storage.state.player.garage.currentRigData = null;
				if (game.model.fromEditor)
				{
					game.BackToEditor();
					break;
				}
				if (base.app.arguments.game.tournamentData != null)
				{
					base.app.view.ui.screens.Open<UITournamentBracketsView>("tournament-brackets-screen");
					break;
				}
				Notify("game.pause.exit@click");
				game.Exit();
				break;
			case "change-room":
				game.OpenNetworkRoomScreen();
				break;
			case "room-player-state":
			{
				NetworkModel network = base.app.model.network;
				if (network.room == null)
				{
					base.app.view.audio.PlayUIGenericError();
					Debug.LogWarning("UIPauseController> OnFormNotification - [" + text + "] No room available.");
				}
				else if (network.room.Local.IsSpectator)
				{
					network.SwitchToRacer();
				}
				else
				{
					network.SwitchToSpectator();
				}
				break;
			}
			case "launch-dashboard":
				if (d == null || d.physics == null || d.physics.isLocked || view.ignoreReturn)
				{
					break;
				}
				if (base.app.model.game.type == GameFlag.Race && ((base.app.controller.game.race != null && !base.app.controller.game.race.customPhysics) || !m_dashboardEnabled) && base.app.model.storage.state.player.physicsTuneWarning)
				{
					UIDialogView uIDialogView = base.app.view.ui.screens.Open<UIDialogView>("dialog-screen");
					uIDialogView.Clear();
					uIDialogView.SetButtons(base.app.model.storage.locale.Get("ui.common.yes", "YES"), base.app.model.storage.locale.Get("ui.common.no", "NO"));
					uIDialogView.status.SetWarning(base.app.model.storage.locale.Get("dashboard.race-warning.message", "THIS WILL MARK YOUR LEADERBOARD ENTRY AS USING CUSTOM PHYSICS,\nARE YOU SURE YOU WANT TO LAUNCH THE DASHBOARD?"));
					uIDialogView.controller.NotificationOnCancel = "ui.screen.return@click";
					UIDialogController controller = uIDialogView.controller;
					controller.OnConfirm = (Action)Delegate.Combine(controller.OnConfirm, (Action)delegate
					{
						m_dashboardEnabled = true;
						Notify("ui.screen.return@click");
						Notify(0.2f, "game.pause.return@click");
						Notify(1f, "game.ui.dashboard@show", true);
					});
					uIDialogView.SetToggle(base.app.model.storage.locale.Get("dashboard.race-warning.toggle.label", "DON'T SHOW THIS MESSAGE AGAIN"));
					uIDialogView.SetToggleActive(p_toggle: true);
					UIDialogController controller2 = uIDialogView.controller;
					controller2.OnToggle = (Action<bool>)Delegate.Combine(controller2.OnToggle, (Action<bool>)delegate(bool p_dontshow)
					{
						base.app.model.storage.state.player.physicsTuneWarning = !p_dontshow;
					});
				}
				else
				{
					Notify("game.pause.return@click");
					Notify(0.5f, "game.ui.dashboard@show", true);
				}
				break;
			case "garage-edit-drone":
			{
				if (!view.droneEditCard.interactable || d == null || d.rig == null || d.rig.isLocked)
				{
					break;
				}
				this.TimerRunOnce(delegate
				{
					base.app.view.ui.screens.ClearStaticBackground(p_textureCleanup: true);
				}, 2f);
				StorageModel storage = base.app.model.storage;
				GameModel gm = (base.validContext ? base.app.controller.game.model : null);
				base.enabled = false;
				storage.PreloadDroneBundleData(null, null, p_ingame: true, delegate
				{
					base.enabled = true;
					UIGarageRigEditView uIGarageRigEditView = base.app.view.ui.screens.Open<UIGarageRigEditView>("garage-rig-edit-screen");
					if ((bool)gm)
					{
						uIGarageRigEditView.data = d.rig;
						uIGarageRigEditView.externalDrone = d;
					}
					uIGarageRigEditView.openedFromDashboard = false;
					uIGarageRigEditView.openedFromPause = true;
				});
				break;
			}
			case "garage-change-drone":
			{
				if (base.app == null || (base.app.scene != null && base.app.scene.track != null && base.app.scene.track.promoDrones != null && base.app.scene.track.promoDrones.Length == 1 && base.app.scene.track.promoDronesOnly) || (base.app.scene != null && base.app.scene.map != null && base.app.scene.map.promoDrones != null && base.app.scene.map.promoDrones.Length == 1 && base.app.scene.map.promoDronesOnly) || (base.app.arguments.game.campaign != null && base.app.arguments.game.campaign.drone != null))
				{
					break;
				}
				base.app.view.ui.game.hud.Hide();
				UIGarageRigSelectionView uIGarageRigSelectionView = base.app.view.ui.screens.Open<UIGarageRigSelectionView>("garage-rig-selection-screen");
				uIGarageRigSelectionView.SetCreationEnabled(p_flag: false);
				uIGarageRigSelectionView.allowCustomPhysics = true;
				uIGarageRigSelectionView.promoList = null;
				uIGarageRigSelectionView.overrideList = null;
				uIGarageRigSelectionView.overrideSizes = null;
				if (base.app.scene.track != null)
				{
					if (base.app.scene.track.promoDrones != null)
					{
						uIGarageRigSelectionView.promoList = new List<DroneRigData>(base.app.scene.track.promoDrones);
					}
					else if (base.app.scene.map != null && base.app.scene.map.promoDrones != null)
					{
						uIGarageRigSelectionView.promoList = new List<DroneRigData>(base.app.scene.map.promoDrones);
					}
					if (base.app.scene.track.promoDronesOnly && base.app.scene.track.promoDrones != null && base.app.scene.track.promoDrones.Length != 0)
					{
						uIGarageRigSelectionView.overrideList = new List<DroneRigData>();
					}
					else if (base.app.scene.map != null && base.app.scene.map.promoDronesOnly && base.app.scene.map.promoDrones != null && base.app.scene.map.promoDrones.Length != 0)
					{
						uIGarageRigSelectionView.overrideList = new List<DroneRigData>();
					}
					if (base.app.scene.track.droneSizes != null && base.app.scene.track.droneSizes.Length != 0)
					{
						uIGarageRigSelectionView.overrideSizes = new List<int>(base.app.scene.track.droneSizes);
					}
					else if (base.app.scene.map != null && base.app.scene.map.droneSizes != null && base.app.scene.map.droneSizes.Length != 0)
					{
						uIGarageRigSelectionView.overrideSizes = new List<int>(base.app.scene.map.droneSizes);
					}
				}
				uIGarageRigSelectionView.SetDroneClassEnabled(true);
				uIGarageRigSelectionView.selectionOnly = uIGarageRigSelectionView.overrideList != null || !view.droneEditCard.interactable;
				uIGarageRigSelectionView.SetCreationEnabled(!uIGarageRigSelectionView.selectionOnly);
				break;
			}
			case "rate-this-drone":
				SetDroneRating(view.droneRating.index + 1, p_update: true);
				break;
			case "rate-this-track":
				SetMapRating(view.mapRating.index + 1, p_update: true);
				break;
			}
		}

		public void GetDroneRating()
		{
			view.droneRatingCard.interactable = false;
			view.ClearDroneRating(0.0001f);
			if (!(currentDrone != null))
			{
				return;
			}
			string guid = currentDrone.rig.guid;
			ServiceModel service = base.app.model.service;
			if (!(service != null))
			{
				return;
			}
			string steamId = base.app.model.storage.state.player.playerData.playerId;
			service.GetCommunityDrone(guid, delegate(DRLCommunityDroneData p_drone)
			{
				if (!(this == null) && !(base.gameObject == null) && !(view == null))
				{
					if (p_drone != null)
					{
						Debug.Log("UIPauseController> GET drone rating = " + (int)p_drone.rating * view.droneRating.max);
					}
					if (p_drone != null && p_drone.rating > -1f && !(p_drone.playerId.ToString() == steamId))
					{
						view.droneRatingCard.interactable = true;
						view.FadeInDroneRating(0.3f, (int)(p_drone.rating * (float)view.droneRating.max));
					}
				}
			});
		}

		public void GetMapRating()
		{
			view.mapRatingCard.interactable = false;
			view.ClearMapRating(0.0001f);
			if (currentMap == null || (currentMap.mapCategoryFlag != GameFlag.MapCommon && base.app.model.game.type != GameFlag.Collectable) || !(currentMap.playerId.ToString() != base.app.model.storage.state.player.profile.playerId))
			{
				return;
			}
			view.mapRatingCard.interactable = true;
			string guid = currentMap.guid;
			ServiceModel service = base.app.model.service;
			if (!(service != null))
			{
				return;
			}
			string steamId = base.app.model.storage.state.player.playerData.playerId;
			service.GetCommunityMapRating(guid, delegate(DRLServiceResult p_result)
			{
				if (base.validContext && currentMap != null && !(view == null) && p_result != null)
				{
					float data = p_result.GetData<float>();
					Debug.Log("UIPauseController> GET map rating = " + data);
					if (data > -1f && !(currentMap.playerId.ToString() == steamId))
					{
						view.FadeInMapRating(0.3f, (int)(data * (float)view.mapRating.max));
					}
				}
			});
		}

		public void SetDroneRating(int p_rating, bool p_update)
		{
			view.droneRating.index = p_rating % (view.droneRating.max + 1);
			for (int i = 0; i < view.droneRatingStarFades.Length; i++)
			{
				view.droneRatingStarFades[i].alpha = 0.1f;
			}
			for (int j = 0; j < view.droneRating.index; j++)
			{
				view.droneRatingStarFades[j].alpha = 1f;
			}
			if (!p_update)
			{
				return;
			}
			float rating = view.droneRating.index;
			float nrating = ((view.droneRating.max <= 0) ? 1f : Mathf.Clamp01(rating / (float)view.droneRating.max));
			ServiceModel sm = base.app.model.service;
			if (!(currentDrone != null))
			{
				return;
			}
			if (update_tune_rating != null)
			{
				update_tune_rating.Stop();
			}
			string guid = currentDrone.rig.guid;
			update_tune_rating = Activity.RunOnce(delegate
			{
				Debug.Log("UIPauseController> SET drone rating = score[" + nrating + "] rating[" + rating + "]");
				if (sm != null)
				{
					sm.SetCommunityDroneRating(guid, nrating, null);
				}
			}, 2f);
		}

		public void SetMapRating(int p_rating, bool p_update)
		{
			view.mapRating.index = p_rating % (view.mapRating.max + 1);
			for (int i = 0; i < view.mapRatingStarFades.Length; i++)
			{
				view.mapRatingStarFades[i].alpha = 0.1f;
			}
			for (int j = 0; j < view.mapRating.index; j++)
			{
				view.mapRatingStarFades[j].alpha = 1f;
			}
			if (!p_update)
			{
				return;
			}
			float rating = view.mapRating.index;
			float nrating = ((view.mapRating.max <= 0) ? 1f : Mathf.Clamp01(rating / (float)view.mapRating.max));
			ServiceModel sm = base.app.model.service;
			if (currentMap == null)
			{
				return;
			}
			if (update_map_rating != null)
			{
				update_map_rating.Stop();
			}
			string guid = currentMap.guid;
			update_map_rating = Activity.RunOnce(delegate
			{
				Debug.Log("UIPauseController> SET map rating / score[" + nrating + "] rating[" + rating + "]");
				if (sm != null)
				{
					sm.SetCommunityMapRating(guid, nrating, null);
				}
			}, 2f);
		}

		private void OnDisable()
		{
			if (m_peekTimer != null)
			{
				m_peekTimer.Stop();
				m_peekTimer.manager.Remove(m_peekTimer);
				m_peekTimer = null;
			}
		}
	}
}

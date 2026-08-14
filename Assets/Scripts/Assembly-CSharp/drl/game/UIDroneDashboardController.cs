using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using drl.sim;
using drl.sim.rci;
using drl.sim.thread;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIDroneDashboardController : Controller<DRLApp>
	{
		[NonSerialized]
		public bool isSandbox;

		public TextAsset[] rigs3inch;

		public TextAsset[] rigs4inch;

		public TextAsset[] rigs5inch;

		public TextAsset[] rigs6inch;

		private List<DroneRigData> rigs = new List<DroneRigData>();

		private DroneRigData m_selectedRig;

		private DroneRigData m_activeRig;

		private string m_activeRigName = "";

		private float m_selectedRigTimer;

		private bool m_refresh;

		private bool m_initialized;

		private bool m_showing;

		public bool openedFromPause;

		public bool openingAnotherScreen;

		private UINavigation lastItem;

		private Activity update_tune_rating;

		public DRLCommunityTuneData currentTune;

		private int autohideCounter;

		protected bool m_refreshRequested;

		protected bool m_refreshNavigationRequested;

		private List<DRLToggleView> activePhysicsSubtabs = new List<DRLToggleView>();

		private List<DRLToggleView> activeDroneSubtabs = new List<DRLToggleView>();

		public UIDroneDashboardView view => AssertLocal<UIDroneDashboardView>("view");

		public bool isShowing => m_showing;

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "game.ui.dashboard.form.event@click":
				if (m_refresh)
				{
					break;
				}
				if (p_target.name.StartsWith("toggle-"))
				{
					OnToggleClick((DRLToggleView)p_target);
				}
				else if (p_target.name.StartsWith("button-"))
				{
					OnButton((UIElementView)p_target);
				}
				else if (p_target.name.Contains("tune-new"))
				{
					view.tuneSavingFeedback.gameObject.SetActive(value: true);
					view.tuneNew.gameObject.SetActive(value: false);
					SaveNewTune(GetDrone(), view.tuneName.field.text, delegate
					{
						base.app.view.audio.PlayUILoadingSuccess();
						view.tuneSavingFeedback.gameObject.SetActive(value: false);
						view.tuneSave.gameObject.SetActive(value: true);
					});
				}
				else if (p_target.name.Contains("tune-save"))
				{
					view.tuneSavingFeedback.gameObject.SetActive(value: true);
					view.tuneSave.gameObject.SetActive(value: false);
					UpdateCurrentTune(GetDrone(), view.tuneName.field.text, delegate
					{
						base.app.view.audio.PlayUILoadingSuccess();
						view.tuneSavingFeedback.gameObject.SetActive(value: false);
						view.tuneSave.gameObject.SetActive(value: true);
					});
				}
				else if (p_target.name.Contains("tune-com-tunes"))
				{
					base.app.view.ui.game.hud.Hide();
					RunOnce(0.02f, delegate
					{
						view.tunesManagerController = base.app.view.ui.screens.Open<UICommunityDronesController>("community-drones-screen");
						view.tunesManagerController.view.inGame = true;
					});
				}
				break;
			case "game.ui.dashboard.form.event@change":
				if (!m_refresh)
				{
					if (p_target.name.StartsWith("tab-"))
					{
						OnTab((DRLToggleView)p_target);
					}
					else if (p_target.name.StartsWith("subtab-"))
					{
						OnSubTab((DRLToggleView)p_target);
					}
					else if (p_target.name.StartsWith("toggle-"))
					{
						OnToggle((DRLToggleView)p_target);
					}
					else if (p_target.name.StartsWith("input-"))
					{
						OnInput((DRLInputFieldView)p_target, p_endEdit: false);
					}
					else if (p_target.name.StartsWith("stepper-"))
					{
						OnStepper((DRLStepperView)p_target);
					}
					else if (p_target.name.StartsWith("tune-rating"))
					{
						SetRating(view.tuneRating.index + 1, p_update: true);
					}
					else if (p_target.name.StartsWith("tune-name-input"))
					{
						UpdateTuneButtons();
					}
				}
				break;
			case "game.ui.dashboard.form.event@start-edit":
				if (!m_refresh && p_target.name.StartsWith("tune-name-input"))
				{
					SetIgnoredGameCommands();
				}
				break;
			case "game.ui.dashboard.form.event@end-edit":
				if (!m_refresh)
				{
					if (p_target.name.StartsWith("input-"))
					{
						OnInput((DRLInputFieldView)p_target, p_endEdit: true);
					}
					else if (p_target.name.StartsWith("tune-name-input"))
					{
						UpdateTuneButtons();
					}
					ClearIgnoredCommands();
				}
				break;
			case "game.ui.dashboard.form.event@focus":
				if (!m_refresh && !p_target.name.StartsWith("tune-name-input"))
				{
					ClearIgnoredCommands();
				}
				break;
			case "game.ui.dashboard@toggle":
				if ((base.app.view.ui.screens.current != null && base.app.view.ui.screens.current.name == "garage-rig-edit-screen") || base.app.model.game.paused)
				{
					break;
				}
				if (m_showing)
				{
					DRLInputFieldView dRLInputFieldView = ((base.app.view.ui.navigation.focus != null) ? base.app.view.ui.navigation.focus.GetComponent<DRLInputFieldView>() : null);
					if (dRLInputFieldView == null || !dRLInputFieldView.IsEditing)
					{
						Notify("game.ui.dashboard@hide");
						openedFromPause = false;
						openingAnotherScreen = false;
					}
				}
				else
				{
					Notify("game.ui.dashboard@show");
				}
				break;
			case "garage.edit.fly.ready":
			{
				if (!(p_data[0] is DroneRigData))
				{
					Debug.LogError("UIDroneDashboardController> EditFlyReady received invalid DroneRigData");
					break;
				}
				DroneRigData p_rig = (DroneRigData)p_data[0];
				Drone drone = ((p_data.Length > 1) ? ((Drone)p_data[1]) : null);
				UpdateRigData(p_rig);
				if (drone != null)
				{
					drone.receiver.channel = 0;
					drone.fc.armed = true;
					Notify("game.simulation.drone@armed", drone);
					base.app.model.game.camera.drone = drone;
					if ((bool)drone.rigidbody)
					{
						drone.rigidbody.frozen = false;
					}
				}
				break;
			}
			case "garage.edit.done":
			{
				DroneRigData p_rig3 = (DroneRigData)p_data[0];
				Drone drone2 = ((p_data.Length > 1) ? ((Drone)p_data[1]) : null);
				UpdateRigData(p_rig3);
				if (drone2 != null)
				{
					drone2.receiver.channel = 0;
					base.app.model.game.camera.drone = drone2;
				}
				break;
			}
			case "garage.edit.rig.saved":
			{
				DroneRigData p_rig2 = (DroneRigData)p_data[0];
				if (!(bool)p_data[1])
				{
					ChangeRig(p_rig2);
				}
				break;
			}
			case "ui.tooltip@show":
				if (m_showing && p_data.Length != 0)
				{
					view.Tooltip((string)p_data[0]);
				}
				else
				{
					view.Tooltip(null);
				}
				break;
			case "ui.tooltip@hide":
				view.Tooltip(null);
				break;
			}
			if (base.app.view.ui.screens.current != view.screen)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				if (!(p_data[0] as UIScreen != view.screen))
				{
					Init();
					RequestRefresh();
				}
				break;
			case "ui.screen.return@focus":
				RequestRefresh();
				break;
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			}
		}

		public bool IsCurrentTuneMine()
		{
			if (!HasCurrentTune())
			{
				return false;
			}
			return base.app.model.storage.state.player.settings.tuning.GetTune(currentTune.guid) != null;
		}

		protected void UpdateTuneButtons()
		{
			if (IsCurrentTuneMine() && currentTune.name.ToUpper() == view.tuneName.field.text.ToUpper())
			{
				view.tuneSave.gameObject.SetActive(value: true);
				view.tuneNew.gameObject.SetActive(value: false);
			}
			else
			{
				view.tuneSave.gameObject.SetActive(value: false);
				view.tuneNew.gameObject.SetActive(value: true);
			}
		}

		public void SetRating(int p_rating, bool p_update)
		{
			view.tuneRating.index = p_rating - 1;
			for (int i = 0; i < view.tuneRatingStarFades.Length; i++)
			{
				view.tuneRatingStarFades[i].alpha = 0.1f;
			}
			for (int j = 0; j < p_rating; j++)
			{
				view.tuneRatingStarFades[j].alpha = 1f;
			}
			if (!p_update)
			{
				return;
			}
			float rating = (float)p_rating / 5f;
			ServiceModel sm = base.app.model.service;
			if (currentTune == null || base.app.model.storage.state.player.settings.tuning.GetTune(currentTune.guid) != null)
			{
				return;
			}
			if (update_tune_rating != null)
			{
				update_tune_rating.Stop();
			}
			string guid = currentTune.guid;
			update_tune_rating = Activity.RunOnce(delegate
			{
				if (sm != null)
				{
					sm.SetCommunityTuneRating(guid, rating, null);
				}
			}, 2f);
		}

		public void SaveNewTune(Drone p_drone, string p_name, Action<DRLCommunityTuneData> p_callback)
		{
			if (!(this == null) && !(view == null) && !(p_drone == null) && !(base.app == null))
			{
				currentTune = new DRLCommunityTuneData();
				currentTune.name = p_name;
				currentTune.playerId = base.app.model.service.backend.playerId;
				currentTune.size = p_drone.rig.diameter;
				currentTune.weight = Mathf.RoundToInt(p_drone.body.weight);
				currentTune.thrust = Mathf.RoundToInt(4f * ((p_drone.body.frame.escs[0].motor.spec.data.thrustScale > 0f) ? p_drone.body.frame.escs[0].motor.spec.data.thrustScale : p_drone.body.frame.escs[0].motor.spec.data.GetMaxThrust()));
				if (p_drone.physics.mass > 0f)
				{
					currentTune.weight = Mathf.RoundToInt(p_drone.physics.mass * 1000f);
				}
				if (p_drone.physics.thrust > 0f)
				{
					currentTune.thrust = Mathf.RoundToInt(4f * p_drone.physics.thrust);
				}
				currentTune.SetData();
				DroneRigData droneRigData = DroneRigData.FromJson(p_drone.rig.ToJson());
				droneRigData.name = currentTune.name;
				droneRigData.tune = p_drone.physics.ToJson();
				currentTune.rig = droneRigData.ToJson();
				base.app.model.storage.state.player.settings.tuning.AddTune(currentTune);
				base.app.model.service.SetCommunityTunes(currentTune, p_callback);
			}
		}

		public void UpdateCurrentTune(Drone p_drone, string p_name, Action<DRLCommunityTuneData> p_callback)
		{
			if (this == null || view == null || p_drone == null || base.app == null)
			{
				return;
			}
			if (currentTune == null)
			{
				SaveNewTune(p_drone, p_name, p_callback);
				return;
			}
			currentTune.name = p_name;
			currentTune.playerId = base.app.model.service.backend.playerId;
			currentTune.size = p_drone.rig.diameter;
			currentTune.weight = Mathf.RoundToInt(p_drone.body.weight);
			currentTune.thrust = Mathf.RoundToInt(4f * ((p_drone.body.frame.escs[0].motor.spec.data.thrustScale > 0f) ? p_drone.body.frame.escs[0].motor.spec.data.thrustScale : p_drone.body.frame.escs[0].motor.spec.data.GetMaxThrust()));
			if (p_drone.physics.mass > 0f)
			{
				currentTune.weight = Mathf.RoundToInt(p_drone.physics.mass * 1000f);
			}
			if (p_drone.physics.thrust > 0f)
			{
				currentTune.thrust = Mathf.RoundToInt(4f * p_drone.physics.thrust);
			}
			currentTune.SetData();
			DroneRigData droneRigData = DroneRigData.FromJson(p_drone.rig.ToJson());
			droneRigData.name = currentTune.name;
			droneRigData.tune = p_drone.physics.ToJson();
			currentTune.rig = droneRigData.ToJson();
			base.app.model.storage.state.player.settings.tuning.UpdateTune(currentTune);
			base.app.model.service.SetCommunityTunes(currentTune, p_callback);
		}

		public void Show(params object[] p_data)
		{
			if (p_data != null && p_data.Length != 0)
			{
				openedFromPause = (bool)p_data[0];
			}
			else
			{
				openedFromPause = false;
			}
			base.app.controller.game.FadeBlur(1f, 0.05f);
			if ((bool)base.app.model.game.playerDrone)
			{
				if ((bool)base.app.model.game.playerDrone.fc)
				{
					base.app.model.game.playerDrone.fc.armed = false;
				}
				if ((bool)base.app.model.game.playerDrone.rigidbody && base.app.controller.game.input.pausePhysics)
				{
					base.app.model.game.playerDrone.rigidbody.frozen = true;
				}
				Notify("game.simulation.drone@disarmed", base.app.model.game.playerDrone);
			}
			Init();
			m_showing = true;
			base.app.view.ui.game.hud.physics.Show();
			base.app.view.ui.game.hud.physics.SingleColumn(p_flag: true);
			base.app.view.ui.game.hud.physics.view.ShowFooter(p_show: true);
			base.app.view.ui.footer.Hide(0.5f, 0.25f);
			base.app.view.ui.game.hud.crosshair.SetActive(value: false);
			bool interactable = base.app.arguments.game.type == GameFlag.Freestyle || base.app.arguments.game.type == GameFlag.Sandbox;
			view.buttonGarageEdit.interactable = interactable;
			view.buttonGarageCreate.interactable = interactable;
			this.TimerRunOnce(delegate
			{
				base.app.view.ui.navigation.enabled = true;
				UINavigation.Focus(base.app.view.ui.game.hud.dashboard.view.tabGeneral);
			}, 0.5f);
			base.app.controller.game.FadeBlur(1f, 0.5f);
			base.app.view.ui.dark.FadeIn();
			Drone drone = GetDrone();
			if (HasCurrentTune() && (drone == null || !drone.hasRig || !currentTune.rigData.FunctionallyIdentical(drone.rig)))
			{
				currentTune = null;
				view.tuneName.field.text = "MY TUNE";
			}
			_ = view.tunesManagerController != null;
			UpdateTuneButtons();
			view.columnGeneral.FadeIn();
			view.columnPhysics.FadeIn();
			view.columnDrone.FadeIn();
			view.columnFc.FadeIn();
			RequestRefresh();
			RedrawGraphs();
			if (base.app.model.storage.state.player.activeFCMode == FCMode.DRLPilot)
			{
				base.app.view.ui.game.hud.damage.Show(p_flag: false);
			}
			view.toggleReplay.gameObject.SetActive(value: false);
			view.toggleReplay.transform.parent.GetChild(view.toggleReplay.transform.GetSiblingIndex() - 1).gameObject.SetActive(value: true);
			RequestRefreshNavigation();
		}

		public void Close()
		{
			Hide(p_all: true);
			m_initialized = false;
			base.gameObject.SetActive(value: false);
		}

		public void Hide(bool p_all = false)
		{
			if ((bool)base.app.model.game.playerDrone)
			{
				if ((bool)base.app.model.game.playerDrone.fc)
				{
					base.app.model.game.playerDrone.fc.armed = true;
				}
				if ((bool)base.app.model.game.playerDrone.rigidbody && base.app.controller.game.input.pausePhysics)
				{
					base.app.model.game.playerDrone.rigidbody.frozen = false;
				}
				Notify("game.simulation.drone@armed", base.app.model.game.playerDrone);
				base.app.model.game.playerDrone.d_topSpeed = base.app.model.game.playerDrone.EstimateTopSpeed();
				if (base.app.model.game.camera.mode == DroneCameraModeType.FPV || base.app.model.game.camera.mode == DroneCameraModeType.FPVSmooth)
				{
					base.app.view.ui.game.hud.crosshair.SetActive(base.app.model.storage.state.player.settings.game.crosshair);
				}
			}
			lastItem = base.app.view.ui.navigation.focus;
			UINavigation.Focus(base.app.view.ui.game.hud.dashboard.view.tabGeneral);
			base.app.controller.game.FadeBlur(0f, 0.5f);
			base.app.view.ui.dark.FadeOut();
			m_showing = false;
			base.app.view.ui.game.hud.physics.SingleColumn(p_flag: false);
			if (p_all)
			{
				base.app.view.ui.game.hud.physics.Hide();
			}
			base.app.view.ui.game.hud.physics.view.ShowFooter(p_show: false);
			view.columnGeneral.FadeOut();
			view.columnPhysics.FadeOut();
			view.columnDrone.FadeOut();
			view.columnFc.FadeOut();
			view.tooltipFade.FadeOut();
			base.app.view.ui.navigation.enabled = false;
			if (base.app.model.storage.state.player.activeFCMode == FCMode.DRLPilot)
			{
				base.app.view.ui.game.hud.damage.Show(p_flag: true);
			}
		}

		private int ActiveChildCount(Transform p_transform)
		{
			int num = 0;
			for (int i = 0; i < p_transform.childCount; i++)
			{
				if (p_transform.GetChild(i).gameObject.activeSelf)
				{
					num++;
				}
			}
			return num;
		}

		public void Init()
		{
			foreach (GameObject item in view.inputWindX.GetComponent<GameObjectList>().list)
			{
				item.SetActive(isSandbox);
			}
			if (m_initialized)
			{
				return;
			}
			m_initialized = true;
			base.gameObject.SetActive(value: true);
			this.TimerRunOnce(delegate
			{
				UINavigation.Focus(view.tabGeneral);
			}, 1f);
			if (view.togglePidDebug != null)
			{
				view.togglePidDebug.gameObject.SetActive(value: false);
			}
			view.columnGeneral.alpha = 0f;
			view.columnPhysics.alpha = 0f;
			view.columnDrone.alpha = 0f;
			view.columnFc.alpha = 0f;
			view.tooltipFade.alpha = 0f;
			view.stepperCamera.labels = new string[3] { "FPV", "EXTERNAL", "ORBIT" };
			view.stepperCamera.min = 0;
			view.stepperCamera.max = view.stepperCamera.labels.Length - 1;
			view.stepperCamera.index = 0;
			view.stepperCamera.Refresh();
			view.stepperCameraTilt.labels = new string[17]
			{
				"0°", "5°", "10°", "15°", "20°", "25°", "30°", "35°", "40°", "45°",
				"50°", "55°", "60°", "65°", "70°", "75°", "80°"
			};
			view.stepperCameraTilt.min = 0;
			view.stepperCameraTilt.max = view.stepperCameraTilt.labels.Length - 1;
			view.stepperCameraTilt.index = 0;
			view.stepperCameraTilt.Refresh();
			view.stepperDroneClass.labels = new string[4] { "3\"", "4\"", "5\"", "6\"" };
			view.stepperDroneClass.min = 0;
			view.stepperDroneClass.max = view.stepperDroneClass.labels.Length - 1;
			string[] array = new string[GATechLookupStorage.DragData.Length + 1];
			array[0] = "DEFAULT";
			for (int num = 0; num < GATechLookupStorage.DragData.Length; num++)
			{
				array[num + 1] = GATechLookupStorage.DragData[num].name.ToUpperInvariant().Replace(".GATECH", "");
			}
			view.stepperDragData.labels = array;
			view.stepperDragData.min = 0;
			view.stepperDragData.max = array.Length - 1;
			view.stepperDragData.index = 0;
			view.stepperDragData.Refresh();
			string[] array2 = new string[FlightController.ActualVersions.Count];
			array2[0] = FlightController.ActualVersions[FlightController.FlightControllerVersion.Betaflight_3_4];
			array2[1] = FlightController.ActualVersions[FlightController.FlightControllerVersion.Betaflight_3_5];
			array2[2] = FlightController.ActualVersions[FlightController.FlightControllerVersion.Betaflight_4_0];
			view.stepperBetaflightVersion.labels = array2;
			view.stepperBetaflightVersion.min = 0;
			view.stepperBetaflightVersion.max = array2.Length - 1;
			view.stepperBetaflightVersion.index = 0;
			view.stepperBetaflightVersion.Refresh();
			Drone drone = GetDrone();
			if (drone != null && !drone.hasRig)
			{
				switch (drone.rig.diameter)
				{
				case 3:
					view.stepperDroneClass.index = 0;
					break;
				case 4:
					view.stepperDroneClass.index = 1;
					break;
				case 5:
					view.stepperDroneClass.index = 2;
					break;
				case 6:
					view.stepperDroneClass.index = 3;
					break;
				default:
					Debug.LogError("UIDroneDashboardController:Init: Unknown rig size class " + drone.rig.diameter);
					view.stepperDroneClass.index = 3;
					break;
				}
				view.stepperDroneClass.Refresh();
				RefreshRigs(drone.rig.diameter);
				for (int num2 = 0; num2 < rigs.Count; num2++)
				{
					if (drone.rig.guid == rigs[num2].guid)
					{
						m_selectedRig = rigs[num2];
						m_activeRig = rigs[num2];
						m_activeRigName = rigs[num2].name;
						view.stepperDroneRig.index = num2;
						view.stepperDroneRig.Refresh();
						break;
					}
				}
				view.labelDroneClass.text = drone.rig.diameter + "\"";
				view.labelDroneRig.text = drone.rig.name.ToUpper();
			}
			else
			{
				view.stepperDroneClass.index = 3;
				view.stepperDroneClass.Refresh();
				RefreshRigs(6);
				m_selectedRig = rigs[0];
				m_activeRig = rigs[0];
				m_activeRigName = rigs[0].name;
			}
			m_selectedRigTimer = 0.1f;
			LoadToggleState(view.subtabPhysicsOptions);
			LoadToggleState(view.subtabPhysicsDrag);
			LoadToggleState(view.subtabPhysicsPropDrag);
			LoadToggleState(view.subtabDroneDrones);
			LoadToggleState(view.subtabDroneSpecs);
			LoadToggleState(view.subtabFcPID);
			LoadToggleState(view.subtabFcExtras);
			LoadToggleState(view.subtabDroneBattery);
			LoadToggleState(view.tabDrone);
			LoadToggleState(view.tabFc);
			LoadToggleState(view.tabGeneral);
			LoadToggleState(view.tabPhysics);
			LoadToggleState(view.toggleGraphEfficiency, isSandbox);
			LoadToggleState(view.toggleGraphElectric, isSandbox);
			LoadToggleState(view.toggleGraphForce, isSandbox);
			LoadToggleState(view.toggleGraphMotor, isSandbox);
			LoadToggleState(view.toggleGraphPitchroll, isSandbox);
			LoadToggleState(view.toggleGraphSpeed, isSandbox);
			LoadToggleState(view.toggleGraphThrottle, isSandbox);
			LoadToggleState(view.toggleGraphYaw, isSandbox);
			LoadToggleState(view.toggleController);
			OnToggle(view.toggleController);
			OnSubTab(view.subtabPhysicsOptions);
			OnSubTab(view.subtabPhysicsDrag);
			OnSubTab(view.subtabPhysicsPropDrag);
			OnSubTab(view.subtabFcPID);
			OnSubTab(view.subtabFcExtras);
			RequestRefresh();
			bool flag = false;
			view.inputCrashEnergy.gameObject.SetActive(flag);
			view.inputDamageEnergy.gameObject.SetActive(flag);
			view.inputCrashSpinout.gameObject.SetActive(flag);
			view.inputCrashTransfer.gameObject.SetActive(flag);
			view.damageTiersInputGroup.SetActive(flag);
			view.speedTiersInputGroup.SetActive(flag);
			view.lineTiersInputGroup.SetActive(flag);
			view.inputDamageTier1.gameObject.SetActive(flag);
			view.inputDamageTier2.gameObject.SetActive(flag);
			view.inputDamageTier3.gameObject.SetActive(flag);
			view.inputSpeedReductionTier1.gameObject.SetActive(flag);
			view.inputSpeedReductionTier2.gameObject.SetActive(flag);
			view.inputSpeedReductionTier3.gameObject.SetActive(flag);
			view.inputLineDeviationTier1.gameObject.SetActive(flag);
			view.inputLineDeviationTier2.gameObject.SetActive(flag);
			view.inputLineDeviationTier3.gameObject.SetActive(flag);
			view.damageTiersTitle.SetActive(flag);
			view.speedTiersTitle.SetActive(flag);
			view.lineTiersTitle.SetActive(flag);
			view.inputPropSturdiness.gameObject.SetActive(flag);
			view.inputArmSturdiness.gameObject.SetActive(flag);
			view.inputBodySturdiness.gameObject.SetActive(flag);
			view.inputDamageThreshold.gameObject.SetActive(flag);
			for (int num3 = 0; num3 < view.vsDisabled.Count; num3++)
			{
				view.vsDisabled[num3].SetActive(!flag);
			}
			base.app.view.ui.game.hud.physics.Init();
			base.app.view.ui.game.hud.physics.Show();
			base.app.view.ui.game.hud.physics.SingleColumn(p_flag: false);
		}

		private void RedrawGraphs()
		{
			base.app.view.ui.game.hud.physics.RedrawGraphCurves();
		}

		private void RefreshRigs(int size)
		{
			rigs.Clear();
			foreach (DroneRigData originalRig in base.app.model.storage.state.player.garage.GetOriginalRigs())
			{
				if (originalRig.diameter == size)
				{
					rigs.Add(originalRig);
				}
			}
			foreach (DroneRigData rig in base.app.model.storage.state.player.garage.rigs)
			{
				if (rig.diameter == size)
				{
					rigs.Add(rig);
				}
			}
			TextAsset[] array = rigs3inch;
			switch (size)
			{
			case 4:
				array = rigs4inch;
				break;
			case 5:
				array = rigs5inch;
				break;
			case 6:
				array = rigs6inch;
				break;
			}
			for (int i = 0; i < array.Length; i++)
			{
				DroneRigData droneRigData = ScriptableObject.CreateInstance<DroneRigData>();
				droneRigData.Set(array[i].bytes);
				rigs.Add(droneRigData);
			}
			string[] array2 = new string[rigs.Count];
			for (int j = 0; j < rigs.Count; j++)
			{
				array2[j] = rigs[j].name;
			}
			view.stepperDroneRig.labels = array2;
			view.stepperDroneRig.min = 0;
			view.stepperDroneRig.max = rigs.Count - 1;
			view.stepperDroneRig.index = 0;
			view.stepperDroneRig.Refresh();
		}

		private Drone GetDrone()
		{
			Drone result = null;
			if ((bool)base.app && (bool)base.app.controller && (bool)base.app.controller.game && (bool)base.app.controller.game.model)
			{
				result = base.app.controller.game.model.playerDrone;
			}
			return result;
		}

		public bool HasCurrentTune()
		{
			return currentTune != null;
		}

		public int GetCurrentTuneRating()
		{
			if (HasCurrentTune())
			{
				if (IsCurrentTuneMine())
				{
					return Mathf.RoundToInt(currentTune.score * 5f);
				}
				return Mathf.RoundToInt(currentTune.rating * 5f);
			}
			return 0;
		}

		public string GetCurrentTuneName()
		{
			if (HasCurrentTune())
			{
				return currentTune.name.ToUpper();
			}
			return "MY TUNE";
		}

		private void LateUpdate()
		{
			if (m_refreshRequested || m_refreshNavigationRequested)
			{
				Refresh();
			}
			if (m_refreshNavigationRequested)
			{
				RefreshNavigation();
			}
			if (m_showing && base.app.model.game.playerDrone != null)
			{
				if (base.app.model.game.playerDrone.hasFc && base.app.model.game.playerDrone.fc.armed && !base.app.model.game.playerDrone.pidTuneRunning)
				{
					if (++autohideCounter > 10)
					{
						Hide();
					}
				}
				else
				{
					autohideCounter = 0;
				}
			}
			else
			{
				autohideCounter = 0;
			}
		}

		public void RequestRefresh()
		{
			m_refreshRequested = true;
		}

		public void Refresh()
		{
			m_refreshRequested = false;
			if (HasCurrentTune())
			{
				SetRating(GetCurrentTuneRating(), p_update: false);
				view.tuneRating.enabled = !IsCurrentTuneMine();
			}
			else
			{
				SetRating(0, p_update: false);
				view.tuneRating.enabled = false;
			}
			FadeComponent component = view.tuneRating.GetComponent<FadeComponent>();
			if ((bool)component)
			{
				component.Fade(view.tuneRating.enabled ? 1f : 0.2f, 0f);
			}
			view.tuneName.text = GetCurrentTuneName();
			Drone drone = GetDrone();
			if (drone == null)
			{
				Debug.LogWarning("UIDroneDashboardController:: player drone not found");
			}
			else
			{
				if (!drone.hasBody || !drone.body.hasFrame || !drone.hasPhysics || !drone.hasFc || drone.body.frame.escs == null || drone.body.frame.escs.Count == 0 || drone.body.frame.escs[0] == null || drone.body.frame.escs[0].motor == null || drone.body.frame.escs[0].motor.prop == null || drone.body.frame.escs[0].motor.spec == null || drone.body.frame.escs[0].motor.spec.data == null || drone.body.frame.batteries == null || drone.body.frame.batteries.Count == 0 || drone.body.frame.batteries[0] == null)
				{
					return;
				}
				drone.d_topSpeed = drone.EstimateTopSpeed();
				view.labelDroneClass.text = drone.rig.diameter + "\"";
				view.labelDroneRig.text = drone.rig.name.ToUpper();
				m_refresh = true;
				view.toggleController.toggle.isOn = base.app.view.ui.game.hud.controller.isActiveAndEnabled && base.app.view.ui.game.hud.controller.fade.alpha > 0.5f;
				if (view.togglePidDebug.gameObject.activeInHierarchy && view.togglePidDebug.toggle != null)
				{
					view.togglePidDebug.toggle.isOn = drone.d_debugPID;
				}
				if ((bool)view.togglePhysThreaded && (bool)view.togglePhysThreaded.toggle)
				{
					view.togglePhysThreaded.toggle.isOn = drone.physics.threaded;
				}
				if ((bool)view.toggleRealCOG && (bool)view.toggleRealCOG.toggle)
				{
					view.toggleRealCOG.toggle.isOn = drone.physics.useCOG;
				}
				view.stepperBetaflightVersion.index = (int)(FlightController.CurrentVersion - 1);
				view.stepperBetaflightVersion.Refresh();
				view.stepperBetaflightVersion.gameObject.SetActive(drone.physics.threaded);
				if ((bool)view.toggleAirmode && (bool)view.toggleAirmode.toggle)
				{
					view.toggleAirmode.toggle.isOn = drone.profile.airmode;
				}
				if ((bool)view.toggleAntigravity && (bool)view.toggleAntigravity.toggle)
				{
					view.toggleAntigravity.toggle.isOn = drone.profile.antigravity;
				}
				if ((bool)view.toggleDynamicFilter && (bool)view.toggleDynamicFilter.toggle)
				{
					view.toggleDynamicFilter.toggle.isOn = drone.profile.dynamicFilter;
				}
				if ((bool)view.toggleITermRotation && (bool)view.toggleITermRotation.toggle)
				{
					view.toggleITermRotation.toggle.isOn = drone.profile.iTermRotation;
				}
				if ((bool)view.toggleSmartFeedForward && (bool)view.toggleSmartFeedForward.toggle)
				{
					view.toggleSmartFeedForward.toggle.isOn = drone.profile.smartFeedForward;
				}
				view.toggleFcModePro.toggle.isOn = drone.hasFc && (drone.fc.mode == FlightControllerMode.Acro || drone.fc.mode == FlightControllerMode.Pro);
				view.toggleFcModeInter.toggle.isOn = drone.hasFc && drone.fc.mode == FlightControllerMode.Intermediate;
				view.toggleFcModeNoob.toggle.isOn = drone.hasFc && (drone.fc.mode == FlightControllerMode.Beginner || drone.fc.mode == FlightControllerMode.DJI || drone.fc.mode == FlightControllerMode.Stabilized || drone.fc.mode == FlightControllerMode.Horizon);
				view.togglePhysEfficiencyCurve.toggle.isOn = drone.physics != null && drone.physics.efficiency <= 0f;
				view.labelEfficiencyOverride.SetActive(!view.togglePhysEfficiencyCurve.toggle.isOn && view.togglePhysEfficiencyCurve.gameObject.activeInHierarchy);
				view.inputEfficiencyOverride.gameObject.SetActive(!view.togglePhysEfficiencyCurve.toggle.isOn && view.togglePhysEfficiencyCurve.gameObject.activeInHierarchy);
				view.labelEfficiencyCurve.SetActive(view.togglePhysEfficiencyCurve.toggle.isOn && view.togglePhysEfficiencyCurve.gameObject.activeInHierarchy);
				view.inputEfficiencyMax.gameObject.SetActive(view.togglePhysEfficiencyCurve.toggle.isOn && view.togglePhysEfficiencyCurve.gameObject.activeInHierarchy);
				view.inputEfficiencyZero.gameObject.SetActive(view.togglePhysEfficiencyCurve.toggle.isOn && view.togglePhysEfficiencyCurve.gameObject.activeInHierarchy);
				FormatField(view.inputEfficiencyOverride, drone.physics.efficiency, p_markDefault: true, 0.85f);
				drone.body.frame.escs[0].motor.prop.CheckMaximums();
				FormatField(view.inputEfficiencyMax, (drone.physics.efficiencyMax > 0f) ? drone.physics.efficiencyMax : drone.body.frame.escs[0].motor.prop.maxEfficiency, p_markDefault: true, drone.body.frame.escs[0].motor.prop.maxEfficiency);
				FormatField(view.inputEfficiencyZero, (drone.physics.efficiencyZero > 0f) ? drone.physics.efficiencyZero : drone.body.frame.escs[0].motor.prop.zeroEfficiencyAdvanceRatio, p_markDefault: true, drone.body.frame.escs[0].motor.prop.zeroEfficiencyAdvanceRatio);
				view.stepperDragData.index = 0;
				if (GATechLookupStorage.HasData(drone.physics.aerodynamicsData))
				{
					GATechLookupData data = GATechLookupStorage.GetData(drone.physics.aerodynamicsData);
					for (int i = 1; i < view.stepperDragData.labels.Length; i++)
					{
						if (data.name.ToUpperInvariant().StartsWith(view.stepperDragData.labels[i]))
						{
							view.stepperDragData.index = i;
							break;
						}
					}
				}
				view.stepperDragData.Refresh();
				view.stepperDragData.gameObject.SetActive(view.subtabPhysicsDrag.toggle.isOn && drone.physics.aerodynamicsType == DronePhysicsData.AerodynamicsModelType.GATech && !drone.physics.legacyDrag);
				FormatField(view.inputDragSurface, drone.physics.surfaceArea, p_markDefault: true, drone.body.frame.surfaceArea.y);
				FormatField(view.inputDragScaleD, drone.physics.dragScale, p_markDefault: true, drone.body.frame.dragScaling.x);
				FormatField(view.inputDragScaleL, drone.physics.liftScale, p_markDefault: true, drone.body.frame.dragScaling.y);
				FormatField(view.inputDragScaleS, drone.physics.sideScale, p_markDefault: true, drone.body.frame.dragScaling.z);
				FormatField(view.inputDragDynamicDrag, drone.physics.inertia, p_markDefault: true, DronePhysicsData.DefaultInertia(drone.body.frame.guid), p_defaultForZero: true);
				FormatField(view.inputDragDynamicLift, drone.physics.arcing, p_markDefault: true, DronePhysicsData.DefaultArcing(drone.body.frame.guid), p_defaultForZero: true);
				view.togglePhysPropBreaking.toggle.isOn = drone.physics.advancedPropLimits;
				view.inputPropTipDrag.gameObject.SetActive(view.togglePhysPropBreaking.toggle.isOn && view.togglePhysPropBreaking.gameObject.activeInHierarchy);
				view.inputPropTipSpeed.gameObject.SetActive(view.togglePhysPropBreaking.toggle.isOn && view.togglePhysPropBreaking.gameObject.activeInHierarchy);
				FormatField(view.inputPropTipDrag, drone.physics.propDragFactor, p_markDefault: true, drone.defaultphysics.propDragFactor);
				FormatField(view.inputPropTipSpeed, drone.physics.maxTipSpeed, p_markDefault: true, drone.defaultphysics.maxTipSpeed);
				FormatField(view.inputPropwashStrength, drone.propwashStrength, p_markDefault: true, 5.5f);
				FormatField(view.inputPropwashThreshold, drone.propwashThreshold, p_markDefault: true, 45f);
				FormatField(view.inputDroneWeight, drone.physics.mass * 1000f, p_markDefault: true, drone.body.weight, p_defaultForZero: true);
				FormatField(view.inputDroneThrust, drone.physics.thrust, p_markDefault: true, (drone.body.frame.escs[0].motor.spec.data.thrustScale > 0f) ? drone.body.frame.escs[0].motor.spec.data.thrustScale : drone.body.frame.escs[0].motor.spec.data.GetMaxThrust(), p_defaultForZero: true);
				FormatField(view.inputDroneTorque, drone.physics.torque, p_markDefault: true, drone.body.frame.escs[0].motor.spec.data.GetMaxTorque(), p_defaultForZero: true);
				view.inputPidPitchP.field.text = Format(drone.profile.pitchPID.p);
				view.inputPidPitchI.field.text = Format(drone.profile.pitchPID.i);
				view.inputPidPitchD.field.text = Format(drone.profile.pitchPID.d);
				view.inputPidRollP.field.text = Format(drone.profile.rollPID.p);
				view.inputPidRollI.field.text = Format(drone.profile.rollPID.i);
				view.inputPidRollD.field.text = Format(drone.profile.rollPID.d);
				view.inputPidYawP.field.text = Format(drone.profile.yawPID.p);
				view.inputPidYawI.field.text = Format(drone.profile.yawPID.i);
				view.inputPidYawD.field.text = Format(drone.profile.yawPID.d);
				view.inputPidPitchFF.field.text = Format(drone.profile.pitchFF);
				view.inputPidRollFF.field.text = Format(drone.profile.rollFF);
				view.inputPidYawFF.field.text = Format(drone.profile.yawFF);
				if ((bool)view.inputLevelAngleLimit && (bool)view.inputLevelAngleLimit.field)
				{
					view.inputLevelAngleLimit.field.text = Format((int)FlightController.LevelAngleLimit);
				}
				if ((bool)view.inputLevelFFTransition && (bool)view.inputLevelFFTransition.field)
				{
					view.inputLevelFFTransition.field.text = Format(0.01f * (float)(int)drone.profile.feedForwardTransition);
				}
				if ((bool)view.inputLevelITermRelaxValue && (bool)view.inputLevelITermRelaxValue.field)
				{
					view.inputLevelITermRelaxValue.field.text = Format(0.01f * (float)(int)drone.profile.iTermRelaxValue);
				}
				if ((bool)view.inputLevelAntigravityGain && (bool)view.inputLevelAntigravityGain.field)
				{
					view.inputLevelAntigravityGain.field.text = Format(0.01f * (float)(int)drone.profile.antigravityGain);
				}
				view.stepperITermRelax.index = drone.profile.iTermRelax;
				view.stepperITermRelax.Refresh();
				view.stepperITermRelaxType.index = drone.profile.iTermRelaxType;
				view.stepperITermRelaxType.Refresh();
				view.stepperAntigravityMode.index = drone.profile.antigravityMode;
				view.stepperAntigravityMode.Refresh();
				view.inputGroundEffectDistance.field.text = Format(drone.physics.groundeffectDistance);
				FormatField(view.inputGroundEffectStrength, drone.physics.groundEffectStrength, p_markDefault: true, drone.defaultphysics.groundEffectStrength);
				FormatField(view.inputGroundEffectDistance, drone.physics.groundeffectDistance, p_markDefault: true, drone.defaultphysics.groundeffectDistance);
				FormatField(view.inputGravityFactor, drone.physics.gravityFactor, p_markDefault: true, drone.defaultphysics.gravityFactor);
				FormatField(view.inputDelaySpinup, drone.physics.overrideSpinup ? drone.physics.spinupTime : drone.body.frame.escs[0].motor.spec.data.spinupDelay, p_markDefault: true, drone.body.frame.escs[0].motor.spec.data.spinupDelay);
				FormatField(view.inputDelaySpindown, drone.physics.overrideSpinup ? drone.physics.spindownTime : drone.body.frame.escs[0].motor.spec.data.spindownDelay, p_markDefault: true, drone.body.frame.escs[0].motor.spec.data.spindownDelay);
				if (drone.body.frame.batteries != null && drone.body.frame.batteries.Count > 0 && drone.body.frame.batteries[0] != null)
				{
					FormatField(view.inputBatteryCapacity, drone.physics.batteryCapacity, p_markDefault: true, drone.body.frame.batteries[0].defaultCapacity, p_defaultForZero: true);
					FormatField(view.inputBatteryResistance, drone.physics.batteryResistance, p_markDefault: true, drone.body.frame.batteries[0].defaultCellResistance, p_defaultForZero: true, "0.00");
				}
				FormatField(view.inputGravity, drone.physics.gravity, p_markDefault: true, 9.81f);
				FormatField(view.inputAirDensity, drone.physics.airDensity, p_markDefault: true, 1.225f, p_defaultForZero: true);
				FormatField(view.inputWindX, drone.wind.x);
				FormatField(view.inputWindY, drone.wind.y);
				FormatField(view.inputWindZ, drone.wind.z);
				if ((bool)view.toggleBatterySag.toggle)
				{
					view.toggleBatterySag.toggle.isOn = drone.physics.batterySag;
				}
				if ((bool)view.toggleBatteryDrain.toggle)
				{
					view.toggleBatteryDrain.toggle.isOn = drone.physics.batteryDrain;
				}
				view.inputBatteryCapacity.gameObject.SetActive(view.toggleBatteryDrain.gameObject.activeInHierarchy && view.toggleBatteryDrain.toggle.isOn);
				view.inputBatteryResistance.gameObject.SetActive(view.toggleBatterySag.gameObject.activeInHierarchy && view.toggleBatterySag.toggle.isOn);
				switch (base.app.model.game.camera.mode)
				{
				case DroneCameraModeType.FPV:
				case DroneCameraModeType.FPVSmooth:
					view.stepperCamera.index = 0;
					break;
				case DroneCameraModeType.TPVBack:
				case DroneCameraModeType.TPVSmooth:
					view.stepperCamera.index = 1;
					break;
				case DroneCameraModeType.TPVFree:
				case DroneCameraModeType.Follow:
					view.stepperCamera.index = 2;
					break;
				default:
					view.stepperCamera.index = 0;
					break;
				}
				view.stepperCamera.Refresh();
				view.stepperCameraTilt.index = Mathf.Clamp(Mathf.FloorToInt(drone.fc.profile.tilt / 5f), 0, view.stepperCameraTilt.labels.Length);
				view.stepperCameraTilt.Refresh();
				view.inputPitchRC.field.text = Format(drone.fc.profile.rcRate.pitch);
				view.inputPitchSuper.field.text = Format(drone.fc.profile.superRate.pitch);
				view.inputPitchExpo.field.text = Format(drone.fc.profile.expo.pitch);
				view.inputRollRC.field.text = Format(drone.fc.profile.rcRate.roll);
				view.inputRollSuper.field.text = Format(drone.fc.profile.superRate.roll);
				view.inputRollExpo.field.text = Format(drone.fc.profile.expo.roll);
				view.inputYawRC.field.text = Format(drone.fc.profile.rcRate.yaw);
				view.inputYawSuper.field.text = Format(drone.fc.profile.superRate.yaw);
				view.inputYawExpo.field.text = Format(drone.fc.profile.expo.yaw);
				view.inputThrottleMid.field.text = Format(drone.fc.profile.superRate.throttle);
				view.inputThrottleExpo.field.text = Format(drone.fc.profile.expo.throttle);
				view.inputFcMinThrottle.field.text = Format(drone.profile.minSignal * 1000f + 1000f);
				view.inputBatteryOverheat.field.text = Format(drone.profile.overheatFactor);
				FormatField(view.inputDamageEnergy, Drone.DamageEnergy, p_markDefault: true, 100f);
				FormatField(view.inputCrashEnergy, Drone.CrashEnergy, p_markDefault: true, 200f);
				FormatField(view.inputCrashSpinout, Drone.Spinout, p_markDefault: true, 0.25f);
				FormatField(view.inputCrashTransfer, Drone.CrashEnergyTransferRate, p_markDefault: true, 0.55f);
				FormatField(view.inputDamageTier1, SettingsController.damageTier1, p_markDefault: true, 0.1f);
				FormatField(view.inputDamageTier2, SettingsController.damageTier2, p_markDefault: true, 0.25f);
				FormatField(view.inputDamageTier3, SettingsController.damageTier3, p_markDefault: true, 1f);
				FormatField(view.inputSpeedReductionTier1, SettingsController.speedReduction1, p_markDefault: true, 0.15f);
				FormatField(view.inputSpeedReductionTier2, SettingsController.speedReduction2, p_markDefault: true, 0.3f);
				FormatField(view.inputSpeedReductionTier3, SettingsController.speedReduction3, p_markDefault: true, 0.5f);
				FormatField(view.inputLineDeviationTier1, SettingsController.lineDeviation1, p_markDefault: true, 0.1f);
				FormatField(view.inputLineDeviationTier2, SettingsController.lineDeviation2, p_markDefault: true, 0.1f);
				FormatField(view.inputLineDeviationTier3, SettingsController.lineDeviation3, p_markDefault: true, 0.1f);
				FormatField(view.inputLineDeviationTier3, SettingsController.lineDeviation3, p_markDefault: true, 0.1f);
				FormatField(view.inputLineDeviationTier3, SettingsController.lineDeviation3, p_markDefault: true, 0.1f);
				FormatField(view.inputPropSturdiness, Drone.PropSturdiness, p_markDefault: true, 0.2f);
				FormatField(view.inputArmSturdiness, Drone.ArmSturdiness, p_markDefault: true, 0.4f);
				FormatField(view.inputBodySturdiness, Drone.BodySturdiness, p_markDefault: true, 0.6f);
				FormatField(view.inputDamageThreshold, SettingsController.damageCrashThreshold, p_markDefault: true, 0.5f);
				m_refresh = false;
				RedrawGraphs();
			}
		}

		private void OnButton(UIElementView p_button)
		{
			Drone drone = GetDrone();
			if (drone == null)
			{
				Debug.LogWarning("UIDroneDashboardController:: player drone not found");
			}
			else if (p_button == view.buttonPreferencesLoad)
			{
				LoadPrefs();
			}
			else if (p_button == view.buttonPreferencesSave)
			{
				SavePrefs();
			}
			else if (p_button == view.buttonPreferencesClear)
			{
				ClearPrefs();
			}
			else if (p_button == view.buttonFlip)
			{
				drone.ResetOrientation();
			}
			else
			{
				if (p_button == view.buttonRecharge)
				{
					if (!(drone.body != null) || !(drone.body.frame != null) || drone.body.frame.batteries == null)
					{
						return;
					}
					{
						foreach (DroneBattery battery in drone.body.frame.batteries)
						{
							if (battery != null)
							{
								battery.Recharge();
							}
						}
						return;
					}
				}
				if (p_button == view.buttonLinkHelp)
				{
					WebBrowser.OpenURL("http://drl.io/", (base.app != null) ? base.app.model.service.platform : null);
				}
				else if (p_button == view.buttonGarageEdit)
				{
					if (!(drone == null) && !(drone.rig == null) && !drone.rig.isLocked)
					{
						UIGarageRigEditView uIGarageRigEditView = base.app.view.ui.screens.Open<UIGarageRigEditView>("garage-rig-edit-screen");
						uIGarageRigEditView.data = drone.rig;
						uIGarageRigEditView.data.isPublic = false;
						uIGarageRigEditView.externalDrone = drone;
						uIGarageRigEditView.openedFromPause = false;
						uIGarageRigEditView.openedFromDashboard = true;
						base.app.view.ui.game.hud.dashboard.Hide();
					}
				}
				else if (p_button == view.buttonGarageCreate)
				{
					if (base.app == null || (base.app.scene != null && base.app.scene.track != null && base.app.scene.track.promoDrones != null && base.app.scene.track.promoDrones.Length == 1 && base.app.scene.track.promoDronesOnly) || (base.app.scene != null && base.app.scene.map != null && base.app.scene.map.promoDrones != null && base.app.scene.map.promoDrones.Length == 1 && base.app.scene.map.promoDronesOnly) || (base.app.arguments.game.campaign != null && base.app.arguments.game.campaign.drone != null))
					{
						return;
					}
					base.app.view.ui.game.hud.Hide();
					base.app.view.ui.game.hud.dashboard.Hide();
					UIGarageRigSelectionView uIGarageRigSelectionView = base.app.view.ui.screens.Open<UIGarageRigSelectionView>("garage-rig-selection-screen");
					uIGarageRigSelectionView.openedFromDashboard = true;
					uIGarageRigSelectionView.SetCreationEnabled(p_flag: false);
					uIGarageRigSelectionView.selectionOnly = true;
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
						else if (base.app.scene.map.promoDrones != null)
						{
							uIGarageRigSelectionView.promoList = new List<DroneRigData>(base.app.scene.map.promoDrones);
						}
						if (base.app.scene.track.promoDronesOnly && base.app.scene.track.promoDrones != null && base.app.scene.track.promoDrones.Length != 0)
						{
							uIGarageRigSelectionView.overrideList = new List<DroneRigData>();
						}
						else if (base.app.scene.map.promoDronesOnly && base.app.scene.map.promoDrones != null && base.app.scene.map.promoDrones.Length != 0)
						{
							uIGarageRigSelectionView.overrideList = new List<DroneRigData>();
						}
						if (base.app.scene.track.droneSizes != null && base.app.scene.track.droneSizes.Length != 0)
						{
							uIGarageRigSelectionView.overrideSizes = new List<int>(base.app.scene.track.droneSizes);
						}
						else if (base.app.scene.map.droneSizes != null && base.app.scene.map.droneSizes.Length != 0)
						{
							uIGarageRigSelectionView.overrideSizes = new List<int>(base.app.scene.map.droneSizes);
						}
					}
					uIGarageRigSelectionView.SetDroneClassEnabled(true);
					uIGarageRigSelectionView.selectionOnly = uIGarageRigSelectionView.overrideList != null;
					uIGarageRigSelectionView.SetCreationEnabled(!uIGarageRigSelectionView.selectionOnly);
				}
				else if (p_button == view.buttonDroneSpecsReset)
				{
					drone.physics.mass = 0f;
					drone.physics.thrust = 0f;
					drone.physics.torque = 0f;
					view.inputDroneWeight.field.text = "";
					view.inputDroneThrust.field.text = "";
					view.inputDroneTorque.field.text = "";
					GreyOutRigSelector();
					RequestRefresh();
				}
				else if (p_button == view.buttonFcRates)
				{
					base.app.controller.game.input.Post(GameCommandType.SwitchPhysicsDashboard);
					openingAnotherScreen = true;
					Activity.RunOnce(delegate
					{
						base.app.controller.game.input.Post(GameCommandType.Pause);
					}, 0.05f);
					Activity.RunOnce(delegate
					{
						base.app.view.ui.screens.Open<UISettingsTuningView>("settings-tuning-screen").openedFromDashboard = true;
					}, 0.1f);
				}
			}
		}

		private void GreyOutRigSelector()
		{
			bool flag = view.inputDroneWeight.field.text == "" && view.inputDroneThrust.field.text == "" && view.inputDroneTorque.field.text == "";
			Graphic[] rigStepperGrayComponents = view.rigStepperGrayComponents;
			for (int i = 0; i < rigStepperGrayComponents.Length; i++)
			{
				rigStepperGrayComponents[i].color = (flag ? Color.white : Color.grey);
			}
		}

		private void OnTab(DRLToggleView p_tab)
		{
			SaveToggleState(p_tab);
			p_tab.transform.parent.parent.Find("body").GetComponent<FadeComponent>().Fade(p_tab.toggle.isOn ? 1f : (-0.1f));
			RequestRefreshNavigation();
		}

		private void OnSubTab(DRLToggleView p_tab)
		{
			SaveToggleState(p_tab);
			foreach (GameObject item in p_tab.GetComponent<GameObjectList>().list)
			{
				item.SetActive(p_tab.toggle.isOn);
			}
			if (p_tab == view.subtabPhysicsDrag || p_tab == view.subtabPhysicsEfficiency || p_tab == view.subtabPhysicsGroundEffect || p_tab == view.subtabPhysicsOptions || p_tab == view.subtabPhysicsPropDrag)
			{
				if (activePhysicsSubtabs.Contains(p_tab))
				{
					activePhysicsSubtabs.Remove(p_tab);
				}
				if (p_tab.toggle.isOn)
				{
					if (p_tab == view.subtabPhysicsPropDrag)
					{
						view.labelEfficiencyOverride.SetActive(!view.togglePhysEfficiencyCurve.toggle.isOn);
						view.inputEfficiencyOverride.gameObject.SetActive(!view.togglePhysEfficiencyCurve.toggle.isOn);
						view.labelEfficiencyCurve.SetActive(view.togglePhysEfficiencyCurve.toggle.isOn);
						view.inputEfficiencyMax.gameObject.SetActive(view.togglePhysEfficiencyCurve.toggle.isOn);
						view.inputEfficiencyZero.gameObject.SetActive(view.togglePhysEfficiencyCurve.toggle.isOn);
						view.inputPropTipDrag.gameObject.SetActive(view.togglePhysPropBreaking.toggle.isOn);
						view.inputPropTipSpeed.gameObject.SetActive(view.togglePhysPropBreaking.toggle.isOn);
					}
					activePhysicsSubtabs.Add(p_tab);
					if (activePhysicsSubtabs.Count > 2)
					{
						foreach (GameObject item2 in activePhysicsSubtabs[0].GetComponent<GameObjectList>().list)
						{
							item2.SetActive(value: false);
						}
						activePhysicsSubtabs[0].toggle.isOn = false;
					}
				}
			}
			if (p_tab == view.subtabDroneDrones || p_tab == view.subtabDroneSpecs || p_tab == view.subtabDroneBattery)
			{
				if (activeDroneSubtabs.Contains(p_tab))
				{
					activeDroneSubtabs.Remove(p_tab);
				}
				if (p_tab.toggle.isOn)
				{
					if (p_tab == view.subtabDroneBattery)
					{
						view.inputBatteryCapacity.gameObject.SetActive(view.toggleBatteryDrain.toggle.isOn);
						view.inputBatteryResistance.gameObject.SetActive(view.toggleBatterySag.toggle.isOn);
					}
					activeDroneSubtabs.Add(p_tab);
					if (activeDroneSubtabs.Count > 2)
					{
						foreach (GameObject item3 in activeDroneSubtabs[0].GetComponent<GameObjectList>().list)
						{
							item3.SetActive(value: false);
						}
						activeDroneSubtabs[0].toggle.isOn = false;
					}
				}
			}
			if ((p_tab == view.subtabFcPID || p_tab == view.subtabFcExtras) && p_tab.toggle.isOn)
			{
				if (p_tab == view.subtabFcPID)
				{
					foreach (GameObject item4 in view.subtabFcExtras.GetComponent<GameObjectList>().list)
					{
						item4.SetActive(value: false);
					}
					view.subtabFcExtras.toggle.isOn = false;
				}
				else if (p_tab == view.subtabFcExtras)
				{
					foreach (GameObject item5 in view.subtabFcPID.GetComponent<GameObjectList>().list)
					{
						item5.SetActive(value: false);
					}
					view.subtabFcPID.toggle.isOn = false;
				}
			}
			if (p_tab.toggle.isOn)
			{
				RequestRefresh();
			}
			RequestRefreshNavigation();
			RequestRefresh();
		}

		private void OnToggle(DRLToggleView p_toggle)
		{
			Drone drone = GetDrone();
			if (drone == null)
			{
				Debug.LogWarning("UIDroneDashboardController:: player drone not found");
				return;
			}
			if (p_toggle == view.toggleReplay)
			{
				foreach (GameObject item in view.toggleReplay.GetComponent<GameObjectList>().list)
				{
					item.SetActive(!p_toggle.toggle.isOn);
				}
				{
					foreach (GameObject item2 in view.inputReplayFilename.GetComponent<GameObjectList>().list)
					{
						item2.SetActive(p_toggle.toggle.isOn);
					}
					return;
				}
			}
			if (p_toggle == view.toggleReplayRecord)
			{
				if (p_toggle.toggle.isOn)
				{
					Debug.Log("REPLAY: start recording: " + view.inputReplayFilename.field.text);
					DebugRecordInputAndFlightpath debugRecordInputAndFlightpath = ((drone == null) ? null : drone.GetComponent<DebugRecordInputAndFlightpath>());
					if ((bool)debugRecordInputAndFlightpath)
					{
						debugRecordInputAndFlightpath.StartRecording(view.inputReplayFilename.field.text);
					}
				}
				else
				{
					Debug.Log("REPLAY: stop recording: " + view.inputReplayFilename.field.text);
					DebugRecordInputAndFlightpath debugRecordInputAndFlightpath2 = ((drone == null) ? null : drone.GetComponent<DebugRecordInputAndFlightpath>());
					if ((bool)debugRecordInputAndFlightpath2)
					{
						debugRecordInputAndFlightpath2.FinishRecording();
					}
				}
			}
			else if (p_toggle == view.toggleReplayPlay)
			{
				if (p_toggle.toggle.isOn)
				{
					Debug.Log("REPLAY: start playback: " + view.inputReplayFilename.field.text);
					DebugReplayInputAndFlightpath replay = ((drone == null) ? null : drone.GetComponent<DebugReplayInputAndFlightpath>());
					if (!replay)
					{
						return;
					}
					replay.StartPlayback(view.inputReplayFilename.field.text);
					Activity.Run((Func<bool>)delegate
					{
						if (replay.IsPlaying)
						{
							return true;
						}
						p_toggle.toggle.isOn = false;
						return false;
					}, 0f, false);
				}
				else
				{
					Debug.Log("REPLAY: playback over");
					DebugReplayInputAndFlightpath debugReplayInputAndFlightpath = ((drone == null) ? null : drone.GetComponent<DebugReplayInputAndFlightpath>());
					if ((bool)debugReplayInputAndFlightpath)
					{
						debugReplayInputAndFlightpath.StopPlayback();
					}
				}
			}
			else if (p_toggle == view.toggleReplayRun)
			{
				if (p_toggle.toggle.isOn)
				{
					Debug.Log("REPLAY: start command sequence: " + view.inputReplayFilename.field.text);
					Activity.RunOnce(delegate
					{
						view.toggleReplayRun.toggle.isOn = false;
					}, 3f);
				}
				else
				{
					Debug.Log("REPLAY: command sequence finished.");
				}
			}
			else
			{
				if (p_toggle == view.toggleAudio)
				{
					return;
				}
				if (p_toggle == view.toggleController)
				{
					SaveToggleState(p_toggle);
					if (p_toggle.toggle.isOn)
					{
						ControllerStateType controllerStateType = RCI.GetControllerStateType(ControllerStateType.Taranis);
						base.app.view.ui.game.hud.controller.gameObject.SetActive(value: true);
						base.app.view.ui.game.hud.controller.SetController(controllerStateType);
						base.app.view.ui.game.hud.controller.SetAnimation(UIControllerAnimationType.UserInput);
						base.app.view.ui.game.hud.controller.fade.FadeIn();
					}
					else
					{
						base.app.view.ui.game.hud.controller.fade.FadeOut();
					}
				}
				else if (p_toggle == view.toggleBatteryDrain)
				{
					if (drone.hasFc)
					{
						drone.physics.batteryDrain = p_toggle.toggle.isOn;
					}
					RequestRefresh();
					RequestRefreshNavigation();
					base.app.view.ui.game.hud.batteryMeterController.Toggle(p_toggle.toggle.isOn || view.toggleBatterySag.toggle.isOn);
				}
				else if (p_toggle == view.toggleBatterySag)
				{
					if (drone.hasFc)
					{
						drone.physics.batterySag = p_toggle.toggle.isOn;
					}
					RequestRefresh();
					RequestRefreshNavigation();
					base.app.view.ui.game.hud.batteryMeterController.Toggle(p_toggle.toggle.isOn || view.toggleBatteryDrain.toggle.isOn);
				}
				else
				{
					if (p_toggle == view.toggleFcModePro || p_toggle == view.toggleFcModeInter || p_toggle == view.toggleFcModeNoob || p_toggle == view.togglePidAutotune)
					{
						return;
					}
					if (p_toggle == view.toggleGraphMotor)
					{
						SaveToggleState(p_toggle, isSandbox);
						base.app.view.ui.game.hud.physics.ToggleGraph(UIHUDPhysicsController.Graph.Motors, p_toggle.toggle.isOn);
					}
					else if (p_toggle == view.toggleGraphThrottle)
					{
						SaveToggleState(p_toggle, isSandbox);
						base.app.view.ui.game.hud.physics.ToggleGraph(UIHUDPhysicsController.Graph.Throttle, p_toggle.toggle.isOn);
					}
					else if (p_toggle == view.toggleGraphPitchroll)
					{
						SaveToggleState(p_toggle, isSandbox);
						base.app.view.ui.game.hud.physics.ToggleGraph(UIHUDPhysicsController.Graph.PitchRoll, p_toggle.toggle.isOn);
					}
					else if (p_toggle == view.toggleGraphYaw)
					{
						SaveToggleState(p_toggle, isSandbox);
						base.app.view.ui.game.hud.physics.ToggleGraph(UIHUDPhysicsController.Graph.Yaw, p_toggle.toggle.isOn);
					}
					else if (p_toggle == view.toggleGraphEfficiency)
					{
						SaveToggleState(p_toggle, isSandbox);
						base.app.view.ui.game.hud.physics.ToggleGraph(UIHUDPhysicsController.Graph.Efficiency, p_toggle.toggle.isOn);
					}
					else if (p_toggle == view.toggleGraphForce)
					{
						SaveToggleState(p_toggle, isSandbox);
						base.app.view.ui.game.hud.physics.ToggleGraph(UIHUDPhysicsController.Graph.Force, p_toggle.toggle.isOn);
					}
					else if (p_toggle == view.toggleGraphSpeed)
					{
						SaveToggleState(p_toggle, isSandbox);
						base.app.view.ui.game.hud.physics.ToggleGraph(UIHUDPhysicsController.Graph.Speed, p_toggle.toggle.isOn);
					}
					else if (p_toggle == view.toggleGraphElectric)
					{
						SaveToggleState(p_toggle, isSandbox);
						base.app.view.ui.game.hud.physics.ToggleGraph(UIHUDPhysicsController.Graph.Electric, p_toggle.toggle.isOn);
					}
					else if (p_toggle == view.togglePhysRealparams)
					{
						drone.physics.arcadePhysics = !p_toggle.toggle.isOn;
					}
					else if (p_toggle == view.togglePhysEfficiencyCurve)
					{
						drone.physics.efficiency = (p_toggle.toggle.isOn ? 0f : 0.85f);
						RequestRefresh();
						RequestRefreshNavigation();
					}
					else if (p_toggle == view.togglePhysTorqueboost)
					{
						drone.physics.torqueBoost = p_toggle.toggle.isOn;
					}
					else if (p_toggle == view.togglePhysPropBreaking)
					{
						drone.physics.advancedPropLimits = p_toggle.toggle.isOn;
						RequestRefresh();
						RequestRefreshNavigation();
					}
					else if (p_toggle == view.toggleGatechCrossflow)
					{
						drone.physics.gatechUseCrossflow = p_toggle.toggle.isOn;
					}
					else if (p_toggle == view.toggleGatechUnsteady)
					{
						drone.physics.gatechUseUnsteady = p_toggle.toggle.isOn;
					}
					else if (p_toggle == view.toggleGatechShedding)
					{
						drone.physics.gatechUseShedding = p_toggle.toggle.isOn;
					}
					else if (p_toggle == view.togglePidDebug)
					{
						drone.d_debugPID = p_toggle.toggle.isOn;
					}
					else if (p_toggle == view.togglePhysThreaded)
					{
						drone.physics.threaded = p_toggle.toggle.isOn;
						if (drone.hasThreaded)
						{
							drone.threaded.ResetThreadToUnityRigidbody();
						}
						RequestRefresh();
						RequestRefreshNavigation();
					}
					else if (p_toggle == view.toggleRealCOG)
					{
						drone.physics.useCOG = p_toggle.toggle.isOn;
						drone.UpdateCenterOfMass();
					}
					else if (p_toggle == view.toggleAirmode)
					{
						drone.profile.airmode = p_toggle.toggle.isOn;
					}
					else if (p_toggle == view.toggleAntigravity)
					{
						drone.profile.antigravity = p_toggle.toggle.isOn;
					}
					else if (p_toggle == view.toggleDynamicFilter)
					{
						drone.profile.dynamicFilter = p_toggle.toggle.isOn;
					}
					else if (p_toggle == view.toggleITermRotation)
					{
						drone.profile.iTermRotation = p_toggle.toggle.isOn;
					}
					else if (p_toggle == view.toggleSmartFeedForward)
					{
						drone.profile.smartFeedForward = p_toggle.toggle.isOn;
					}
					else
					{
						Debug.LogWarning("UIDroneDashboardController:: unknown toggle clicked");
					}
				}
			}
		}

		private void SaveToggleState(DRLToggleView p_toggle, bool p_sandbox = false)
		{
			if (!(p_toggle == null) && !(p_toggle.toggle == null))
			{
				PlayerPrefs.SetInt((p_sandbox ? "sandboxtoggle-" : "dashboardtoggle-") + p_toggle.name, p_toggle.toggle.isOn ? 1 : 0);
			}
		}

		private void LoadToggleState(DRLToggleView p_toggle, bool p_sandbox = false)
		{
			if (!(p_toggle == null) && !(p_toggle.toggle == null))
			{
				p_toggle.toggle.isOn = PlayerPrefs.GetInt((p_sandbox ? "sandboxtoggle-" : "dashboardtoggle-") + p_toggle.name, (!p_sandbox) ? (p_toggle.toggle.isOn ? 1 : 0) : 0) == 1;
			}
		}

		private void OnToggleClick(DRLToggleView p_toggle)
		{
			Drone d = GetDrone();
			if (d == null)
			{
				Debug.LogWarning("UIDroneDashboardController:: player drone not found");
			}
			else if (p_toggle == view.toggleFcModePro)
			{
				if (!p_toggle.toggle.isOn)
				{
					p_toggle.toggle.isOn = true;
					return;
				}
				if (d.hasFc)
				{
					d.fc.SetMode(FlightControllerMode.Pro);
				}
				view.toggleFcModeInter.toggle.isOn = false;
				view.toggleFcModeNoob.toggle.isOn = false;
				base.app.model.storage.state.player.activeFCMode = FCMode.Pro;
				RequestRefresh();
				RequestRefreshNavigation();
			}
			else if (p_toggle == view.toggleFcModeInter)
			{
				if (!p_toggle.toggle.isOn)
				{
					p_toggle.toggle.isOn = true;
					return;
				}
				if (d.hasFc)
				{
					d.fc.SetMode(FlightControllerMode.Intermediate);
				}
				view.toggleFcModePro.toggle.isOn = false;
				view.toggleFcModeNoob.toggle.isOn = false;
				base.app.model.storage.state.player.activeFCMode = FCMode.Intermediate;
				RequestRefresh();
				RequestRefreshNavigation();
			}
			else if (p_toggle == view.toggleFcModeNoob)
			{
				if (!p_toggle.toggle.isOn)
				{
					p_toggle.toggle.isOn = true;
					return;
				}
				if (d.hasFc)
				{
					d.fc.SetMode(FlightControllerMode.Beginner);
				}
				view.toggleFcModeInter.toggle.isOn = false;
				view.toggleFcModePro.toggle.isOn = false;
				base.app.model.storage.state.player.activeFCMode = FCMode.Beginner;
				RequestRefresh();
				RequestRefreshNavigation();
			}
			else
			{
				if (!(p_toggle == view.togglePidAutotune))
				{
					return;
				}
				d.rigidbody.isKinematic = false;
				d.fc.armed = true;
				d.AutotunePid();
				Activity.Run((Func<bool>)delegate
				{
					if (d.pidTuneRunning)
					{
						return true;
					}
					view.togglePidAutotune.toggle.isOn = false;
					view.inputPidPitchP.field.text = Format(d.profile.pitchPID.p);
					view.inputPidPitchI.field.text = Format(d.profile.pitchPID.i);
					view.inputPidPitchD.field.text = Format(d.profile.pitchPID.d);
					view.inputPidRollP.field.text = Format(d.profile.rollPID.p);
					view.inputPidRollI.field.text = Format(d.profile.rollPID.i);
					view.inputPidRollD.field.text = Format(d.profile.rollPID.d);
					view.inputPidYawP.field.text = Format(d.profile.yawPID.p);
					view.inputPidYawI.field.text = Format(d.profile.yawPID.i);
					view.inputPidYawD.field.text = Format(d.profile.yawPID.d);
					d.fc.armed = false;
					d.rigidbody.isKinematic = true;
					d.UpdateCenterOfMass();
					return false;
				}, 0.1f, false);
			}
		}

		private void UpdateEfficiency(Drone d, bool p_refreshFields)
		{
			float num = Validate(view.inputEfficiencyMax, 0.01f, 1.5f, d.physics.efficiencyMax, p_refreshFields, p_allowEmpty: true, d.body.frame.escs[0].motor.prop.maxEfficiency);
			float num2 = Validate(view.inputEfficiencyZero, 0.01f, 2f, d.physics.efficiencyZero, p_refreshFields, p_allowEmpty: true, d.body.frame.escs[0].motor.prop.zeroEfficiencyAdvanceRatio);
			d.physics.efficiencyMax = num;
			d.physics.efficiencyZero = num2;
			foreach (DroneESC esc in d.body.frame.escs)
			{
				esc.motor.prop.SetEfficiency(num, num2);
			}
			if (p_refreshFields)
			{
				RedrawGraphs();
			}
		}

		private void OnInput(DRLInputFieldView p_field, bool p_endEdit)
		{
			if (p_field.field.text.EndsWith("p") || p_field.field.text.EndsWith("P"))
			{
				p_field.field.text = p_field.field.text.Substring(0, p_field.field.text.Length - 1);
				p_endEdit = true;
			}
			Drone drone = GetDrone();
			if (drone == null)
			{
				Debug.LogWarning("UIDroneDashboardController:: player drone not found");
			}
			else if (p_field == view.inputEfficiencyOverride)
			{
				if (!view.togglePhysEfficiencyCurve)
				{
					drone.physics.efficiency = Validate(p_field, 0f, 1f, drone.physics.efficiency, p_endEdit, p_allowEmpty: true, 0.85f);
				}
			}
			else if (p_field == view.inputEfficiencyMax)
			{
				if ((bool)view.togglePhysEfficiencyCurve)
				{
					UpdateEfficiency(drone, p_endEdit);
				}
			}
			else if (p_field == view.inputEfficiencyZero)
			{
				if ((bool)view.togglePhysEfficiencyCurve)
				{
					UpdateEfficiency(drone, p_endEdit);
				}
			}
			else if (p_field == view.inputDragScaleD)
			{
				drone.physics.dragScale = Validate(p_field, 0f, 20f, drone.physics.dragScale, p_endEdit, p_allowEmpty: true, drone.body.frame.dragScaling.x);
			}
			else if (p_field == view.inputDragScaleL)
			{
				drone.physics.liftScale = Validate(p_field, 0f, 20f, drone.physics.liftScale, p_endEdit, p_allowEmpty: true, drone.body.frame.dragScaling.y);
			}
			else if (p_field == view.inputDragScaleS)
			{
				drone.physics.sideScale = Validate(p_field, 0f, 20f, drone.physics.sideScale, p_endEdit, p_allowEmpty: true, drone.body.frame.dragScaling.z);
			}
			else if (p_field == view.inputDragCdMax)
			{
				drone.physics.CdMax = Validate(p_field, 0f, 20f, drone.physics.CdMax, p_endEdit, p_allowEmpty: true, drone.body.frame.cD.y);
			}
			else if (p_field == view.inputDragClMax)
			{
				drone.physics.ClMax = Validate(p_field, 0f, 20f, drone.physics.ClMax, p_endEdit, p_allowEmpty: true, drone.body.frame.cL.y);
			}
			else if (p_field == view.inputDragCdMin)
			{
				drone.physics.CdMin = Validate(p_field, 0f, 20f, drone.physics.CdMin, p_endEdit, p_allowEmpty: true, drone.body.frame.cD.x);
			}
			else if (p_field == view.inputDragClMin)
			{
				drone.physics.ClMin = Validate(p_field, 0f, 20f, drone.physics.ClMin, p_endEdit, p_allowEmpty: true, drone.body.frame.cL.x);
			}
			else if (p_field == view.inputDragSurface)
			{
				drone.physics.surfaceArea = Validate(p_field, 0f, 10f, drone.physics.surfaceArea, p_endEdit, p_allowEmpty: true, drone.body.frame.surfaceArea.y);
			}
			else if (p_field == view.inputDragDynamicDrag)
			{
				drone.physics.inertia = Validate(p_field, 0.01f, 100f, drone.physics.inertia, p_endEdit, p_allowEmpty: true, DronePhysicsData.DefaultInertia(drone.body.frame.guid));
			}
			else if (p_field == view.inputDragDynamicLift)
			{
				drone.physics.arcing = Validate(p_field, 0.01f, 10f, drone.physics.arcing, p_endEdit, p_allowEmpty: true, DronePhysicsData.DefaultArcing(drone.body.frame.guid));
			}
			else if (p_field == view.inputDroneWeight)
			{
				drone.physics.mass = Validate(p_field, 10f, 20000f, drone.physics.mass * 1000f, p_endEdit, p_allowEmpty: true, drone.body.weight) * 0.001f;
				if (Mathf.Approximately(drone.physics.mass, drone.body.weight * 0.001f))
				{
					drone.physics.mass = 0f;
				}
				drone.rigidbody.rb.mass = ((drone.physics.mass > 0.001f) ? drone.physics.mass : (drone.body.weight * 0.001f));
				GreyOutRigSelector();
			}
			else if (p_field == view.inputDroneThrust)
			{
				drone.physics.thrust = Validate(p_field, 0f, 5000f, drone.physics.thrust, p_endEdit, p_allowEmpty: true, (drone.body.frame.escs[0].motor.spec.data.thrustScale > 0f) ? drone.body.frame.escs[0].motor.spec.data.thrustScale : drone.body.frame.escs[0].motor.spec.data.GetMaxThrust());
				if (Mathf.Approximately(drone.physics.thrust, (drone.body.frame.escs[0].motor.spec.data.thrustScale > 0f) ? drone.body.frame.escs[0].motor.spec.data.thrustScale : drone.body.frame.escs[0].motor.spec.data.GetMaxThrust()))
				{
					drone.physics.thrust = 0f;
				}
				GreyOutRigSelector();
			}
			else if (p_field == view.inputDroneTorque)
			{
				drone.physics.torque = Validate(p_field, 0f, 100f, drone.physics.torque, p_endEdit, p_allowEmpty: true, drone.body.frame.escs[0].motor.spec.data.GetMaxTorque());
				if (Mathf.Approximately(drone.physics.torque, drone.body.frame.escs[0].motor.spec.data.GetMaxTorque()))
				{
					drone.physics.torque = 0f;
				}
				GreyOutRigSelector();
			}
			else if (p_field == view.inputPidPitchP)
			{
				drl.sim.PIDVector pitchPID = drone.profile.pitchPID;
				pitchPID.p = Validate(p_field, 0f, 150f, pitchPID.p, p_endEdit);
				drone.fc.profile.pid.pitch = pitchPID;
				drone.profile.pitchPID = pitchPID;
			}
			else if (p_field == view.inputPidPitchI)
			{
				drl.sim.PIDVector pitchPID2 = drone.profile.pitchPID;
				pitchPID2.i = Validate(p_field, 0f, 150f, pitchPID2.i, p_endEdit);
				drone.fc.profile.pid.pitch = pitchPID2;
				drone.profile.pitchPID = pitchPID2;
				drone.UpdateCenterOfMass();
			}
			else if (p_field == view.inputPidPitchD)
			{
				drl.sim.PIDVector pitchPID3 = drone.profile.pitchPID;
				pitchPID3.d = Validate(p_field, 0f, 150f, pitchPID3.d, p_endEdit);
				drone.fc.profile.pid.pitch = pitchPID3;
				drone.profile.pitchPID = pitchPID3;
			}
			else if (p_field == view.inputPidRollP)
			{
				drl.sim.PIDVector rollPID = drone.profile.rollPID;
				rollPID.p = Validate(p_field, 0f, 150f, rollPID.p, p_endEdit);
				drone.fc.profile.pid.roll = rollPID;
				drone.profile.rollPID = rollPID;
			}
			else if (p_field == view.inputPidRollI)
			{
				drl.sim.PIDVector rollPID2 = drone.profile.rollPID;
				rollPID2.i = Validate(p_field, 0f, 150f, rollPID2.i, p_endEdit);
				drone.fc.profile.pid.roll = rollPID2;
				drone.profile.rollPID = rollPID2;
			}
			else if (p_field == view.inputPidRollD)
			{
				drl.sim.PIDVector rollPID3 = drone.profile.rollPID;
				rollPID3.d = Validate(p_field, 0f, 150f, rollPID3.d, p_endEdit);
				drone.fc.profile.pid.roll = rollPID3;
				drone.profile.rollPID = rollPID3;
			}
			else if (p_field == view.inputPidYawP)
			{
				drl.sim.PIDVector yawPID = drone.profile.yawPID;
				yawPID.p = Validate(p_field, 0f, 150f, yawPID.p, p_endEdit);
				drone.fc.profile.pid.yaw = yawPID;
				drone.profile.yawPID = yawPID;
			}
			else if (p_field == view.inputPidYawI)
			{
				drl.sim.PIDVector yawPID2 = drone.profile.yawPID;
				yawPID2.i = Validate(p_field, 0f, 150f, yawPID2.i, p_endEdit);
				drone.fc.profile.pid.yaw = yawPID2;
				drone.profile.yawPID = yawPID2;
			}
			else if (p_field == view.inputPidYawD)
			{
				drl.sim.PIDVector yawPID3 = drone.profile.yawPID;
				yawPID3.d = Validate(p_field, 0f, 150f, yawPID3.d, p_endEdit);
				drone.fc.profile.pid.yaw = yawPID3;
				drone.profile.yawPID = yawPID3;
			}
			else if (p_field == view.inputPidPitchFF)
			{
				drone.profile.pitchFF = Validate(p_field, 0f, 150f, drone.profile.pitchFF, p_endEdit);
			}
			else if (p_field == view.inputPidRollFF)
			{
				drone.profile.rollFF = Validate(p_field, 0f, 150f, drone.profile.rollFF, p_endEdit);
			}
			else if (p_field == view.inputPidYawFF)
			{
				drone.profile.yawFF = Validate(p_field, 0f, 150f, drone.profile.yawFF, p_endEdit);
			}
			else if (p_field == view.inputPidLevelP)
			{
				drl.sim.PIDVector levelPID = drone.profile.levelPID;
				levelPID.p = Validate(p_field, 0f, 150f, levelPID.p, p_endEdit);
				drone.profile.levelPID = levelPID;
			}
			else if (p_field == view.inputPidLevelI)
			{
				drl.sim.PIDVector levelPID2 = drone.profile.levelPID;
				levelPID2.i = Validate(p_field, 0f, 150f, levelPID2.i, p_endEdit);
				drone.profile.levelPID = levelPID2;
			}
			else if (p_field == view.inputPidLevelD)
			{
				drl.sim.PIDVector levelPID3 = drone.profile.levelPID;
				levelPID3.d = Validate(p_field, 0f, 150f, levelPID3.d, p_endEdit);
				drone.profile.levelPID = levelPID3;
			}
			else if (p_field == view.inputLevelAngleLimit)
			{
				float f = Validate(p_field, 0f, 90f, (int)FlightController.LevelAngleLimit, p_endEdit, p_allowEmpty: true, 55f, p_clearOnDefault: false);
				if (drone.physics.threaded)
				{
					FlightController.LevelAngleLimit = (byte)Mathf.RoundToInt(f);
				}
			}
			else if (p_field == view.inputLevelFFTransition)
			{
				drone.profile.feedForwardTransition = (byte)Mathf.RoundToInt(Validate(p_field, 0f, 1f, 0.01f * (float)(int)drone.profile.feedForwardTransition, p_endEdit, p_allowEmpty: true, 1f, p_clearOnDefault: false) * 100f);
			}
			else if (p_field == view.inputLevelITermRelaxValue)
			{
				drone.profile.iTermRelaxValue = (byte)Mathf.RoundToInt(Validate(p_field, 0f, 1f, 0.01f * (float)(int)drone.profile.iTermRelaxValue, p_endEdit, p_allowEmpty: true, 0.11f, p_clearOnDefault: false) * 100f);
			}
			else if (p_field == view.inputLevelAntigravityGain)
			{
				drone.profile.antigravityGain = (ushort)Mathf.RoundToInt(Validate(p_field, 1f, 30f, 0.01f * (float)(int)drone.profile.antigravityGain, p_endEdit, p_allowEmpty: true, 10f, p_clearOnDefault: false) * 100f);
			}
			else if (p_field == view.inputPitchRC)
			{
				drone.fc.profile.rcRate.pitch = Validate(p_field, 0f, 2.5f, drone.fc.profile.rcRate.pitch, p_endEdit);
				RedrawGraphs();
			}
			else if (p_field == view.inputPitchSuper)
			{
				drone.fc.profile.superRate.pitch = Validate(p_field, 0f, 2.5f, drone.fc.profile.superRate.pitch, p_endEdit);
				RedrawGraphs();
			}
			else if (p_field == view.inputPitchExpo)
			{
				drone.fc.profile.expo.pitch = Validate(p_field, 0f, 1f, drone.fc.profile.expo.pitch, p_endEdit);
				RedrawGraphs();
			}
			else if (p_field == view.inputRollRC)
			{
				drone.fc.profile.rcRate.roll = Validate(p_field, 0f, 2.5f, drone.fc.profile.rcRate.roll, p_endEdit);
				RedrawGraphs();
			}
			else if (p_field == view.inputRollSuper)
			{
				drone.fc.profile.superRate.roll = Validate(p_field, 0f, 2.5f, drone.fc.profile.superRate.roll, p_endEdit);
				RedrawGraphs();
			}
			else if (p_field == view.inputRollExpo)
			{
				drone.fc.profile.expo.roll = Validate(p_field, 0f, 1f, drone.fc.profile.expo.roll, p_endEdit);
				RedrawGraphs();
			}
			else if (p_field == view.inputYawRC)
			{
				drone.fc.profile.rcRate.yaw = Validate(p_field, 0f, 2.5f, drone.fc.profile.rcRate.yaw, p_endEdit);
				RedrawGraphs();
			}
			else if (p_field == view.inputYawSuper)
			{
				drone.fc.profile.superRate.yaw = Validate(p_field, 0f, 2.5f, drone.fc.profile.superRate.yaw, p_endEdit);
				RedrawGraphs();
			}
			else if (p_field == view.inputYawExpo)
			{
				drone.fc.profile.expo.yaw = Validate(p_field, 0f, 1f, drone.fc.profile.expo.yaw, p_endEdit);
				RedrawGraphs();
			}
			else if (p_field == view.inputThrottleMid)
			{
				drone.fc.profile.superRate.throttle = Validate(p_field, 0f, 1f, drone.fc.profile.superRate.throttle, p_endEdit);
				RedrawGraphs();
			}
			else if (p_field == view.inputThrottleExpo)
			{
				drone.fc.profile.expo.throttle = Validate(p_field, 0f, 1f, drone.fc.profile.expo.throttle, p_endEdit);
				RedrawGraphs();
			}
			else if (p_field == view.inputGroundEffectStrength)
			{
				drone.physics.groundEffectStrength = Validate(p_field, 0f, 20f, drone.physics.groundEffectStrength, p_endEdit, p_allowEmpty: true, drone.defaultphysics.groundEffectStrength);
			}
			else if (p_field == view.inputGroundEffectDistance)
			{
				drone.physics.groundeffectDistance = Validate(p_field, 0f, 5f, drone.physics.groundeffectDistance, p_endEdit, p_allowEmpty: true, drone.defaultphysics.groundeffectDistance);
			}
			else if (p_field == view.inputGravityFactor)
			{
				drone.physics.gravityFactor = Validate(p_field, 0f, 50f, drone.physics.gravityFactor, p_endEdit, p_allowEmpty: true, drone.defaultphysics.gravityFactor);
			}
			else if (p_field == view.inputGravity)
			{
				drone.physics.gravity = Validate(p_field, 0f, 200f, drone.physics.gravity, p_endEdit, p_allowEmpty: true, drone.defaultphysics.gravity);
			}
			else if (p_field == view.inputAirDensity)
			{
				drone.physics.airDensity = Validate(p_field, 0f, 10f, drone.physics.airDensity, p_endEdit, p_allowEmpty: true, 1.225f);
			}
			else if (p_field == view.inputWindX)
			{
				drone.wind.x = Validate(p_field, 0f, 200f, 0f, p_endEdit);
			}
			else if (p_field == view.inputWindY)
			{
				drone.wind.y = Validate(p_field, 0f, 200f, 0f, p_endEdit);
			}
			else if (p_field == view.inputWindZ)
			{
				drone.wind.z = Validate(p_field, 0f, 200f, 0f, p_endEdit);
			}
			else if (p_field == view.inputDelaySpinup)
			{
				drone.physics.spinupTime = Validate(p_field, 0f, 5f, drone.physics.spinupTime, p_endEdit, p_allowEmpty: true, drone.body.frame.escs[0].motor.spec.data.spinupDelay);
				drone.physics.overrideSpinup = drone.physics.spinupTime >= 0f || drone.physics.spindownTime >= 0f;
			}
			else if (p_field == view.inputDelaySpindown)
			{
				drone.physics.spindownTime = Validate(p_field, 0f, 5f, drone.physics.spindownTime, p_endEdit, p_allowEmpty: true, drone.body.frame.escs[0].motor.spec.data.spindownDelay);
				drone.physics.overrideSpinup = drone.physics.spinupTime >= 0f || drone.physics.spindownTime >= 0f;
			}
			else if (p_field == view.inputBatteryCapacity)
			{
				if (drone.body.frame.batteries != null && drone.body.frame.batteries.Count != 0 && !(drone.body.frame.batteries[0] == null))
				{
					drone.physics.batteryCapacity = Validate(p_field, 0f, 100000f, drone.physics.batteryCapacity, p_endEdit, p_allowEmpty: true, drone.body.frame.batteries[0].defaultCapacity, p_clearOnDefault: false);
					if (Mathf.Approximately(drone.physics.batteryCapacity, drone.body.frame.batteries[0].defaultCapacity))
					{
						drone.physics.batteryCapacity = 0f;
					}
					drone.body.frame.batteries[0].capacity = drone.physics.batteryCapacity;
				}
			}
			else if (p_field == view.inputBatteryResistance)
			{
				if (drone.body.frame.batteries != null && drone.body.frame.batteries.Count != 0 && !(drone.body.frame.batteries[0] == null))
				{
					drone.physics.batteryResistance = Validate(p_field, 0f, 100f, drone.physics.batteryResistance, p_endEdit, p_allowEmpty: true, drone.body.frame.batteries[0].defaultCellResistance, p_clearOnDefault: false);
					if (Mathf.Approximately(drone.physics.batteryResistance, drone.body.frame.batteries[0].defaultCellResistance))
					{
						drone.physics.batteryResistance = 0f;
					}
					drone.body.frame.batteries[0].cellResistance = drone.physics.batteryResistance;
				}
			}
			else if (p_field == view.inputPropTipSpeed)
			{
				drone.physics.maxTipSpeed = Validate(p_field, 0f, 1f, drone.physics.maxTipSpeed, p_endEdit, p_allowEmpty: true, drone.defaultphysics.maxTipSpeed);
			}
			else if (p_field == view.inputPropTipDrag)
			{
				drone.physics.propDragFactor = Validate(p_field, 0f, 1f, drone.physics.propDragFactor, p_endEdit, p_allowEmpty: true, drone.defaultphysics.propDragFactor);
			}
			else if (p_field == view.inputPropwashStrength)
			{
				drone.propwashStrength = Validate(p_field, 0f, 100f, drone.propwashStrength, p_endEdit, p_allowEmpty: true, 5f);
			}
			else if (p_field == view.inputPropwashThreshold)
			{
				drone.propwashThreshold = Validate(p_field, 0f, 90f, drone.propwashThreshold, p_endEdit, p_allowEmpty: true, 40f);
			}
			else if (p_field == view.inputBatteryOverheat)
			{
				drone.profile.overheatFactor = Validate(p_field, 0f, 1000f, drone.profile.overheatFactor, p_endEdit);
			}
			else if (p_field == view.inputFcMinThrottle)
			{
				drone.profile.minSignal = Validate(p_field, 1000f, 2000f, drone.profile.minSignal * 1000f + 1000f, p_endEdit) * 0.001f - 1f;
				drone.fc.minSignal = drone.profile.minSignal;
			}
			else if (p_field == view.inputDamageEnergy)
			{
				Drone.DamageEnergy = Validate(p_field, 0f, 100000f, 100f, p_endEdit, p_allowEmpty: true, 100f);
			}
			else if (p_field == view.inputCrashEnergy)
			{
				Drone.CrashEnergy = Validate(p_field, 0f, 100000f, 200f, p_endEdit, p_allowEmpty: true, 200f);
			}
			else if (p_field == view.inputCrashSpinout)
			{
				Drone.Spinout = Validate(p_field, 0f, 100f, 0.25f, p_endEdit, p_allowEmpty: true, 0.25f);
			}
			else if (p_field == view.inputCrashTransfer)
			{
				Drone.CrashEnergyTransferRate = Validate(p_field, 0f, 100f, 0.55f, p_endEdit, p_allowEmpty: true, 0.55f);
			}
			else if (p_field == view.inputDamageTier1)
			{
				SettingsController.damageTier1 = Validate(p_field, 0f, 100f, 0.1f, p_endEdit, p_allowEmpty: true, 0.1f);
			}
			else if (p_field == view.inputDamageTier2)
			{
				SettingsController.damageTier2 = Validate(p_field, 0f, 100f, 0.25f, p_endEdit, p_allowEmpty: true, 0.25f);
			}
			else if (p_field == view.inputDamageTier3)
			{
				SettingsController.damageTier3 = Validate(p_field, 0f, 100f, 1f, p_endEdit, p_allowEmpty: true, 1f);
			}
			else if (p_field == view.inputSpeedReductionTier1)
			{
				SettingsController.speedReduction1 = Validate(p_field, 0f, 100f, 0.15f, p_endEdit, p_allowEmpty: true, 0.15f);
			}
			else if (p_field == view.inputSpeedReductionTier2)
			{
				SettingsController.speedReduction2 = Validate(p_field, 0f, 100f, 0.3f, p_endEdit, p_allowEmpty: true, 0.3f);
			}
			else if (p_field == view.inputSpeedReductionTier3)
			{
				SettingsController.speedReduction3 = Validate(p_field, 0f, 100f, 0.5f, p_endEdit, p_allowEmpty: true, 0.5f);
			}
			else if (p_field == view.inputLineDeviationTier1)
			{
				SettingsController.lineDeviation1 = Validate(p_field, 0f, 100f, 0.1f, p_endEdit, p_allowEmpty: true, 0.1f);
			}
			else if (p_field == view.inputLineDeviationTier2)
			{
				SettingsController.lineDeviation2 = Validate(p_field, 0f, 100f, 0.2f, p_endEdit, p_allowEmpty: true, 0.2f);
			}
			else if (p_field == view.inputLineDeviationTier3)
			{
				SettingsController.lineDeviation3 = Validate(p_field, 0f, 100f, 0.3f, p_endEdit, p_allowEmpty: true, 0.3f);
			}
			else if (p_field == view.inputPropSturdiness)
			{
				Drone.PropSturdiness = Validate(p_field, 0f, 1f, 0.2f, p_endEdit, p_allowEmpty: true, 0.2f);
			}
			else if (p_field == view.inputArmSturdiness)
			{
				Drone.ArmSturdiness = Validate(p_field, 0f, 1f, 0.4f, p_endEdit, p_allowEmpty: true, 0.4f);
			}
			else if (p_field == view.inputBodySturdiness)
			{
				Drone.BodySturdiness = Validate(p_field, 0f, 1f, 0.6f, p_endEdit, p_allowEmpty: true, 0.6f);
			}
			else if (p_field == view.inputDamageThreshold)
			{
				SettingsController.damageCrashThreshold = Validate(p_field, 0f, 1f, 0.5f, p_endEdit, p_allowEmpty: true, 0.5f);
			}
		}

		private void OnStepper(DRLStepperView p_stepper)
		{
			Drone drone = GetDrone();
			if (drone == null)
			{
				Debug.LogWarning("UIDroneDashboardController:: player drone not found");
			}
			else if (p_stepper == view.stepperCamera)
			{
				switch (view.stepperCamera.label)
				{
				case "FPV":
					base.app.model.game.camera.SetFPV(drone);
					break;
				case "EXTERNAL":
					base.app.model.game.camera.SetTPVBack(drone);
					break;
				case "ORBIT":
					base.app.model.game.camera.SetTPVFree(drone);
					break;
				}
			}
			else if (p_stepper == view.stepperCameraTilt)
			{
				drone.fc.profile.tilt = 5f * (float)view.stepperCameraTilt.index;
				drone.body.frame.camera.tilt = drone.fc.profile.tilt;
				base.app.model.storage.state.player.settings.tuning.UpdateCameraDelayed(drone.body.frame.camera.tilt);
			}
			else if (p_stepper == view.stepperDroneClass)
			{
				switch (p_stepper.index)
				{
				case 0:
					RefreshRigs(3);
					break;
				case 1:
					RefreshRigs(4);
					break;
				case 2:
					RefreshRigs(5);
					break;
				case 3:
					RefreshRigs(6);
					break;
				}
				m_selectedRig = rigs[0];
				m_selectedRigTimer = 2f;
			}
			else if (p_stepper == view.stepperDroneRig)
			{
				m_selectedRig = rigs[view.stepperDroneRig.index];
				m_selectedRigTimer = 2f;
			}
			else if (p_stepper == view.stepperDragMode)
			{
				drone.physics.SetAerodynamics((p_stepper.index != 1) ? DronePhysicsData.AerodynamicsModelType.Traditional : DronePhysicsData.AerodynamicsModelType.GATech, drone.body.frame.gatechDragData);
				drone.physics.legacyDrag = p_stepper.index == 2;
				RequestRefresh();
				RequestRefreshNavigation();
			}
			else if (p_stepper == view.stepperDragData)
			{
				drone.physics.aerodynamicsData = ((p_stepper.index == 0) ? null : p_stepper.label);
				if (drone.physics.aerodynamicsType == DronePhysicsData.AerodynamicsModelType.GATech && !drone.physics.legacyDrag)
				{
					GATechLookupData gATechLookupData = GATechLookupStorage.GetData(drone.physics.aerodynamicsData);
					if (gATechLookupData == null)
					{
						gATechLookupData = drone.body.frame.gatechDragData;
					}
					drone.physics.SetAerodynamics(DronePhysicsData.AerodynamicsModelType.GATech, gATechLookupData);
				}
				RequestRefresh();
			}
			else if (p_stepper == view.stepperBetaflightMode)
			{
				if (drone.physics.threaded)
				{
					switch (p_stepper.index)
					{
					case 0:
						FlightController.EnableFlightMode(FlightMode.ACRO);
						break;
					case 1:
						FlightController.EnableFlightMode(FlightMode.ANGLE);
						break;
					case 2:
						FlightController.EnableFlightMode(FlightMode.HORIZON);
						break;
					default:
						FlightController.EnableFlightMode(FlightMode.ACRO);
						break;
					}
				}
			}
			else if (p_stepper == view.stepperBetaflightVersion)
			{
				if (drone.physics.threaded)
				{
					switch (p_stepper.index)
					{
					case 0:
						drone.profile.betaflightVersion = 34;
						break;
					case 1:
						drone.profile.betaflightVersion = 35;
						break;
					case 2:
						drone.profile.betaflightVersion = 40;
						break;
					}
				}
			}
			else if (p_stepper == view.stepperITermRelax)
			{
				if (drone.physics.threaded)
				{
					drone.profile.iTermRelax = (byte)p_stepper.index;
				}
			}
			else if (p_stepper == view.stepperITermRelaxType)
			{
				if (drone.physics.threaded)
				{
					drone.profile.iTermRelaxType = (byte)p_stepper.index;
				}
			}
			else if (p_stepper == view.stepperAntigravityMode && drone.physics.threaded)
			{
				drone.profile.antigravityMode = (byte)p_stepper.index;
			}
		}

		private string Format(float f, string p_formatStyle = null)
		{
			if (!string.IsNullOrEmpty(p_formatStyle))
			{
				return f.ToString(p_formatStyle);
			}
			return f.ToString("0.######");
		}

		private void FormatField(DRLInputFieldView p_field, float p_value, bool p_markDefault = false, float p_default = 0f, bool p_defaultForZero = false, string p_formatStyle = null)
		{
			if (p_markDefault)
			{
				p_field.placeholder = "=" + Format(p_default, p_formatStyle);
				if (Mathf.Abs(p_value - p_default) < 0.0001f)
				{
					p_field.text = "";
				}
				else if (p_defaultForZero && p_value <= 0f)
				{
					p_field.text = "";
				}
				else
				{
					p_field.text = Format(p_value, p_formatStyle);
				}
			}
			else
			{
				p_field.text = Format(p_value, p_formatStyle);
			}
		}

		private float Parse(string s, float d)
		{
			float result = 0f;
			if (float.TryParse(s.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out result))
			{
				return result;
			}
			return d;
		}

		private float Validate(DRLInputFieldView p_field, float p_min, float p_max, float p_default, bool p_refreshField, bool p_allowEmpty = true, float p_emptyDefault = 0f, bool p_clearOnDefault = true)
		{
			float result = 0f;
			if (p_allowEmpty && string.IsNullOrEmpty(p_field.text))
			{
				if (p_refreshField)
				{
					p_field.field.text = "";
				}
				return p_emptyDefault;
			}
			if (!float.TryParse(p_field.field.text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out result))
			{
				if (p_refreshField)
				{
					p_field.field.text = "invalid";
				}
				return p_default;
			}
			result = ((result == 0f && p_min > 0f && p_allowEmpty) ? p_emptyDefault : ((!(result < 0f && p_min == 0f && p_allowEmpty)) ? Mathf.Clamp(result, p_min, p_max) : p_emptyDefault));
			if (p_refreshField)
			{
				p_field.field.text = ((p_allowEmpty && p_clearOnDefault && Mathf.Abs(result - p_emptyDefault) < 0.0001f) ? "" : Format(result));
			}
			return result;
		}

		public void UpdateRigData(DroneRigData p_rig)
		{
			if (!(p_rig == null))
			{
				m_activeRig = p_rig;
				m_activeRigName = p_rig.name;
			}
		}

		public void ChangeRig(DroneRigData p_rig)
		{
			if (!(p_rig == null) && !(GetDrone() == null))
			{
				if (!(p_rig == null) && p_rig.name != null && !(p_rig.name == ""))
				{
					ChangeRig(p_rig, p_threaded: true);
				}
			}
		}

		public void ChangeRig(DroneRigData p_rig, bool p_threaded)
		{
			if (p_rig == null)
			{
				return;
			}
			Drone drone = GetDrone();
			if (drone == null)
			{
				return;
			}
			if (p_rig == null || p_rig.name == null || p_rig.name == "")
			{
				return;
			}
			Drone new_drone = base.app.model.storage.factory.Instantiate(p_rig, base.transform, p_async: true, p_isUser: true);
			if (!new_drone)
			{
				return;
			}
			int channel = ((drone.receiver != null) ? drone.receiver.channel : 0);
			base.app.controller.game.ApplyCommunityDroneToDrone(new_drone);
			base.app.model.game.simulation.PlaceDrone(new_drone, 0);
			new_drone.transform.localScale = Vector3.one;
			FCProfileData fc_profile = drone.fc.profile;
			new_drone.OnEvent.AddListener(delegate(DroneEvent p_event)
			{
				if (p_event.type == DroneEventType.Ready)
				{
					new_drone.receiver.channel = channel;
					new_drone.fc.profile = fc_profile;
					new_drone.fc.armed = true;
					base.app.model.game.camera.drone = new_drone;
					Notify("game.simulation.drone@armed", new_drone);
					RunOnce(0.1f, delegate
					{
						LoadPrefs();
						new_drone.SetPropwash(base.app.model.storage.state.player.settings.game.propwash);
					});
				}
			});
			UpdateRigData(p_rig);
			GamePlayerData playerData = base.app.model.game.GetPlayerData(drone);
			if (playerData != null)
			{
				playerData.drone = new_drone;
				playerData.rig = p_rig;
			}
			Notify("game.simulation.drone@disarmed", new_drone);
			drone.gameObject.SetActive(value: false);
			UnityEngine.Object.Destroy(drone.gameObject, 5f);
			base.app.model.game.simulation.RemoveDrone(drone);
			base.app.model.storage.state.player.garage.currentRigData = p_rig;
			base.app.model.storage.state.player.garage.activeRigData = p_rig;
		}

		private void ClearPrefs()
		{
			Drone drone = GetDrone();
			if (drone != null && drone.hasRig)
			{
				drone.rig.tune = null;
			}
			base.app.model.storage.state.player.garage.UpdateRig(drone);
			if (drone != null)
			{
				DronePhysicsSettings componentInChildren = drone.GetComponentInChildren<DronePhysicsSettings>();
				if ((bool)componentInChildren)
				{
					drone.physics = componentInChildren.data;
					drone.defaultphysics = componentInChildren.data;
					drone.djiphysics = componentInChildren.djiData;
				}
			}
			view.inputDroneWeight.text = "";
			drone.physics.mass = Validate(view.inputDroneWeight, 10f, 20000f, drone.physics.mass * 1000f, p_refreshField: true, p_allowEmpty: true, drone.body.weight) * 0.001f;
			drone.rigidbody.rb.mass = ((drone.physics.mass > 0.001f) ? drone.physics.mass : (drone.body.weight * 0.001f));
			view.inputDroneThrust.text = "";
			drone.physics.thrust = Validate(view.inputDroneThrust, 0f, 5000f, drone.physics.thrust, p_refreshField: true, p_allowEmpty: true, (drone.body.frame.escs[0].motor.spec.data.thrustScale > 0f) ? drone.body.frame.escs[0].motor.spec.data.thrustScale : drone.body.frame.escs[0].motor.spec.data.GetMaxThrust());
			view.inputDroneTorque.text = "";
			drone.physics.torque = Validate(view.inputDroneTorque, 0f, 100f, drone.physics.torque, p_refreshField: true, p_allowEmpty: true, drone.body.frame.escs[0].motor.spec.data.GetMaxTorque());
			view.inputDragSurface.text = "";
			drone.physics.surfaceArea = Validate(view.inputDragSurface, 0f, 10f, drone.physics.surfaceArea, p_refreshField: true, p_allowEmpty: true, drone.body.frame.surfaceArea.y);
			view.inputDragScaleD.text = "";
			drone.physics.dragScale = Validate(view.inputDragScaleD, 0f, 20f, drone.physics.dragScale, p_refreshField: true, p_allowEmpty: true, drone.body.frame.dragScaling.x);
			view.inputDragScaleL.text = "";
			drone.physics.liftScale = Validate(view.inputDragScaleL, 0f, 20f, drone.physics.liftScale, p_refreshField: true, p_allowEmpty: true, drone.body.frame.dragScaling.y);
			view.inputDragScaleS.text = "";
			drone.physics.sideScale = Validate(view.inputDragScaleS, 0f, 20f, drone.physics.sideScale, p_refreshField: true, p_allowEmpty: true, drone.body.frame.dragScaling.z);
			view.inputDragDynamicDrag.text = "";
			drone.physics.inertia = Validate(view.inputDragDynamicDrag, 0.01f, 100f, drone.physics.inertia, p_refreshField: true, p_allowEmpty: true, DronePhysicsData.DefaultInertia(drone.body.frame.guid));
			view.inputDragDynamicLift.text = "";
			drone.physics.arcing = Validate(view.inputDragDynamicLift, 0.01f, 10f, drone.physics.arcing, p_refreshField: true, p_allowEmpty: true, DronePhysicsData.DefaultArcing(drone.body.frame.guid));
			view.inputGroundEffectStrength.text = "";
			drone.physics.groundEffectStrength = Validate(view.inputGroundEffectStrength, 0f, 20f, drone.physics.groundEffectStrength, p_refreshField: true, p_allowEmpty: true, drone.defaultphysics.groundEffectStrength);
			view.inputGroundEffectDistance.text = "";
			drone.physics.groundeffectDistance = Validate(view.inputGroundEffectDistance, 0f, 5f, drone.physics.groundeffectDistance, p_refreshField: true, p_allowEmpty: true, drone.defaultphysics.groundeffectDistance);
			view.inputGravityFactor.text = "";
			drone.physics.gravityFactor = Validate(view.inputGravityFactor, 0f, 50f, drone.physics.gravityFactor, p_refreshField: true, p_allowEmpty: true, drone.defaultphysics.gravityFactor);
			view.inputGravity.text = "";
			drone.physics.gravity = Validate(view.inputGravity, 0f, 200f, drone.physics.gravity, p_refreshField: true, p_allowEmpty: true, 9.81f);
			view.inputAirDensity.text = "";
			drone.physics.airDensity = Validate(view.inputAirDensity, 0f, 10f, drone.physics.airDensity, p_refreshField: true, p_allowEmpty: true, 1.225f);
			view.inputDelaySpinup.text = "";
			drone.physics.spinupTime = Validate(view.inputDelaySpinup, 0f, 5f, drone.physics.spinupTime, p_refreshField: true, p_allowEmpty: true, drone.body.frame.escs[0].motor.spec.data.spinupDelay);
			drone.physics.overrideSpinup = drone.physics.spinupTime > 0f || drone.physics.spindownTime > 0f;
			view.inputDelaySpindown.text = "";
			drone.physics.spindownTime = Validate(view.inputDelaySpindown, 0f, 5f, drone.physics.spindownTime, p_refreshField: true, p_allowEmpty: true, drone.body.frame.escs[0].motor.spec.data.spindownDelay);
			drone.physics.overrideSpinup = drone.physics.spinupTime > 0f || drone.physics.spindownTime > 0f;
			view.inputPropTipSpeed.text = "";
			drone.physics.maxTipSpeed = Validate(view.inputPropTipSpeed, 0f, 1f, drone.physics.maxTipSpeed, p_refreshField: true, p_allowEmpty: true, drone.defaultphysics.maxTipSpeed);
			view.inputPropTipDrag.text = "";
			drone.physics.propDragFactor = Validate(view.inputPropTipDrag, 0f, 1f, drone.physics.propDragFactor, p_refreshField: true, p_allowEmpty: true, drone.defaultphysics.propDragFactor);
			view.inputEfficiencyMax.text = "";
			drone.physics.efficiencyMax = Validate(view.inputEfficiencyMax, 0.01f, 1.5f, drone.physics.efficiencyMax, p_refreshField: true, p_allowEmpty: true, drone.body.frame.escs[0].motor.prop.maxEfficiency);
			view.inputEfficiencyZero.text = "";
			drone.physics.efficiencyZero = Validate(view.inputEfficiencyZero, 0.01f, 2f, drone.physics.efficiencyZero, p_refreshField: true, p_allowEmpty: true, drone.body.frame.escs[0].motor.prop.zeroEfficiencyAdvanceRatio);
			if (view.inputWindX != null)
			{
				view.inputWindX.text = "0.0";
			}
			if (view.inputWindY != null)
			{
				view.inputWindY.text = "0.0";
			}
			if (view.inputWindZ != null)
			{
				view.inputWindZ.text = "0.0";
			}
			drone.wind = Vector3.zero;
			RequestRefresh();
		}

		private void SavePrefs()
		{
			Drone drone = GetDrone();
			if (!(drone != null) || !drone.hasRig)
			{
				return;
			}
			bool flag = base.app.model.storage.state.player.garage.IsOriginal(drone.rig);
			if (!drone.IsCurrentPhysicsDefault)
			{
				if (flag)
				{
					drone.rig = drone.rig.Clone();
					drone.rig.name = "MY " + drone.rig.name.Replace("DRL ", "");
				}
				drone.rig.tune = drone.physics.ToJson();
			}
			else
			{
				drone.rig.tune = null;
			}
			drone.rig.profile = drone.profile.ToJson();
			if (!string.IsNullOrEmpty(drone.rig.profile))
			{
				PlayerPrefs.SetString("drone-profile-" + drone.rig.guid, drone.rig.profile);
			}
			if (drone.rig.tune != null || !flag)
			{
				base.app.model.storage.state.player.garage.UpdateRig(drone);
				if (flag)
				{
					ChangeRig(drone.rig);
				}
			}
		}

		public void ApplyCurrentTune()
		{
			Drone drone = GetDrone();
			if (drone == null)
			{
				return;
			}
			TuningStateModel tuning = base.app.model.storage.state.player.settings.tuning;
			tuning.currentTune = currentTune;
			if (tuning.currentTune != null)
			{
				DronePhysicsData dronePhysicsData = DronePhysicsData.FromSerializedData(tuning.currentTune.GetData<SerializedData>());
				if (!(dronePhysicsData == null))
				{
					drone = base.app.model.storage.factory.Replace(currentTune.rigData, drone, p_async: false);
					drone.physics = dronePhysicsData;
					drone.fc.armed = false;
					drone.rigidbody.frozen = true;
					Notify("garage.edit.fly.ready", currentTune.rigData, drone);
				}
			}
		}

		private void LoadPrefs()
		{
			Drone drone = GetDrone();
			if (drone != null && drone.hasRig)
			{
				if (drone.rig.hasCustomPhysics)
				{
					DronePhysicsData dronePhysicsData = DronePhysicsData.FromJson(drone.rig.tune);
					if (dronePhysicsData != null)
					{
						drone.physics = dronePhysicsData;
					}
				}
				if (PlayerPrefs.HasKey("drone-profile-" + drone.rig.guid))
				{
					drone.rig.profile = PlayerPrefs.GetString("drone-profile-" + drone.rig.guid);
				}
				if (drone.rig.hasCustomProfile)
				{
					DroneProfileData droneProfileData = DroneProfileData.FromJson(drone.rig.profile);
					if (droneProfileData != null)
					{
						drone.profile = droneProfileData;
					}
				}
			}
			RequestRefresh();
		}

		public string FormatNumber(float p_value, int p_decimals)
		{
			if (p_decimals < 1)
			{
				return ((int)p_value).ToString();
			}
			switch (p_decimals)
			{
			case 1:
				return ((float)(int)(p_value * 10f) * 0.1f).ToString();
			case 2:
				return ((float)(int)(p_value * 100f) * 0.01f).ToString();
			case 3:
				return ((float)(int)(p_value * 1000f) * 0.001f).ToString();
			case 4:
				return ((float)(int)(p_value * 10000f) * 0.0001f).ToString();
			default:
			{
				int num = (int)Mathf.Pow(10f, p_decimals);
				return ((float)(int)(p_value * (float)num) * (1f / (float)num)).ToString();
			}
			}
		}

		private void RequestRefreshNavigation()
		{
			m_refreshNavigationRequested = true;
		}

		private void RefreshNavigation()
		{
			m_refreshNavigationRequested = false;
			for (int i = 0; i < view.contentNodes.Length; i++)
			{
				UINavigation uINavigation = null;
				UINavigation uINavigation2 = null;
				UINavigation uINavigation3 = null;
				List<UINavigation> list = null;
				Transform transform = view.contentNodes[i];
				int num = i - 1;
				int num2 = ((i + 1 == view.contentNodes.Length) ? (-1) : (i + 1));
				int childCount = transform.childCount;
				if (num >= 0)
				{
					uINavigation2 = view.headersNavigation[num];
				}
				if (num2 >= 0)
				{
					uINavigation = view.headersNavigation[num2];
				}
				UINavigation uINavigation4 = null;
				for (int j = 0; j < childCount; j++)
				{
					Transform child = transform.GetChild(j);
					if (!child.gameObject.activeSelf)
					{
						continue;
					}
					if (child.name.Contains("group") || child.name.Contains("subtabs"))
					{
						if (uINavigation4 == null)
						{
							uINavigation3 = view.headersNavigation[i];
						}
						list = SetupGroupNavigation(child, uINavigation2, uINavigation, uINavigation3, list);
						uINavigation3 = null;
						if (uINavigation4 == null)
						{
							uINavigation4 = list[0];
						}
						continue;
					}
					UINavigation component = child.GetComponent<UINavigation>();
					if ((bool)component)
					{
						component.down = GetNearestNavDown(child);
						component.left = uINavigation2;
						component.right = uINavigation;
						if (list != null)
						{
							component.up = ((i < list.Count) ? list[i] : list[list.Count - 1]);
						}
						else
						{
							component.up = uINavigation3;
						}
						uINavigation3 = component;
						list = null;
						if (uINavigation4 == null)
						{
							uINavigation4 = component;
							component.up = view.headersNavigation[i];
						}
					}
				}
				if (i > 0)
				{
					view.headersNavigation[i].left = view.headersNavigation[num];
				}
				view.headersNavigation[i].down = uINavigation4;
				view.headersNavigation[i].up = null;
				if (num2 > 0)
				{
					view.headersNavigation[i].right = view.headersNavigation[num2];
				}
			}
		}

		public UINavigation GetNearestNavDown(Transform p_node, int p_horizontalPosition = 0)
		{
			int num = p_node.GetSiblingIndex();
			UINavigation uINavigation = null;
			while (uINavigation == null && num < p_node.parent.childCount - 1)
			{
				num++;
				Transform child = p_node.parent.GetChild(num);
				if (!child.gameObject.activeSelf)
				{
					continue;
				}
				if (child.name.Contains("group") || child.name.Contains("subtabs"))
				{
					if (p_horizontalPosition == 0)
					{
						uINavigation = child.GetComponentInChildren<UINavigation>(includeInactive: false);
						continue;
					}
					List<UINavigation> list = new List<UINavigation>();
					foreach (Transform item in child)
					{
						if (item.gameObject.activeSelf)
						{
							UINavigation component = item.GetComponent<UINavigation>();
							if ((bool)component)
							{
								list.Add(component);
							}
						}
					}
					if (list.Count != 0)
					{
						uINavigation = ((p_horizontalPosition >= list.Count) ? list[list.Count - 1] : list[p_horizontalPosition]);
					}
				}
				else
				{
					uINavigation = child.GetComponent<UINavigation>();
				}
			}
			return uINavigation;
		}

		public List<UINavigation> SetupGroupNavigation(Transform p_node, UINavigation p_left, UINavigation p_right, UINavigation p_up, List<UINavigation> p_upList = null)
		{
			List<UINavigation> list = new List<UINavigation>();
			foreach (Transform item in p_node)
			{
				if (item.gameObject.activeSelf)
				{
					UINavigation component = item.GetComponent<UINavigation>();
					if ((bool)component)
					{
						list.Add(component);
					}
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list.Count == 1)
				{
					list[i].right = p_right;
					list[i].left = p_left;
				}
				else if (i == 0)
				{
					list[i].right = list[i + 1];
					list[i].left = p_left;
				}
				else if (i == list.Count - 1)
				{
					list[i].right = p_right;
					list[i].left = list[i - 1];
				}
				else
				{
					list[i].right = list[i + 1];
					list[i].left = list[i - 1];
				}
				if (p_upList != null)
				{
					list[i].up = ((i < p_upList.Count) ? p_upList[i] : p_upList[p_upList.Count - 1]);
				}
				else
				{
					list[i].up = p_up;
				}
				list[i].down = GetNearestNavDown(p_node, i);
			}
			return list;
		}

		public void SetIgnoredGameCommands()
		{
			if (!base.app.controller)
			{
				return;
			}
			List<GameCommand> list = new List<GameCommand>();
			foreach (GameInputMapComponent map in base.app.controller.game.input.maps)
			{
				foreach (GameCommand command in map.commands)
				{
					if (command.type == GameCommandType.EditDrone || command.type == GameCommandType.ResetDrone || command.type == GameCommandType.ResetDronePodium || command.type == GameCommandType.ResetGame || command.type == GameCommandType.SwitchCameraMode || command.type == GameCommandType.SwitchDebugDashboard || command.type == GameCommandType.SwitchPhysicsDashboard)
					{
						list.Add(command);
					}
				}
			}
			base.app.controller.game.input.SetIgnoredCommands(list);
		}

		private void ClearIgnoredCommands()
		{
			if ((bool)base.app.controller)
			{
				base.app.controller.game.input.ClearIgnoredCommands();
			}
		}

		private void Update()
		{
			if (!m_showing)
			{
				return;
			}
			if (RCI.GetButtonDown(ConsoleButtons.ActionTopRow1))
			{
				Drone drone = GetDrone();
				if (drone == null)
				{
					Debug.LogWarning("UIDroneDashboardController:: player drone not found");
					return;
				}
				if (drone == null || drone.rig == null || drone.rig.isLocked)
				{
					return;
				}
				UIGarageRigEditView uIGarageRigEditView = base.app.view.ui.screens.Open<UIGarageRigEditView>("garage-rig-edit-screen");
				uIGarageRigEditView.data = drone.rig;
				uIGarageRigEditView.data.isPublic = false;
				uIGarageRigEditView.externalDrone = drone;
				uIGarageRigEditView.openedFromPause = false;
				uIGarageRigEditView.openedFromDashboard = true;
			}
			if (RCI.GetButtonDown(ConsoleButtons.ActionBottomRow2))
			{
				Notify("game.ui.dashboard@hide");
			}
		}
	}
}

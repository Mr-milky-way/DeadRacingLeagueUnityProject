using System;
using UnityEngine;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UISettingsTuningController : Controller<DRLApp>
	{
		public bool proMode = true;

		protected UIDroneOverlay m_drone;

		public UIDroneOverlay uiDronePrefab;

		[SerializeField]
		private UINavigation m_droneUINav;

		public Component[] droneUINavTargets = new Component[4];

		private Activity m_delay_save;

		private bool m_navLocked;

		public UISettingsTuningView view => AssertLocal<UISettingsTuningView>("view");

		public UIDroneOverlay drone
		{
			get
			{
				if (m_drone == null)
				{
					SpawnUiDrone();
				}
				return m_drone;
			}
		}

		public UINavigation droneUINav
		{
			get
			{
				if (m_droneUINav == null && drone != null)
				{
					m_droneUINav = drone.transform.parent.GetComponent<UINavigation>();
				}
				return m_droneUINav;
			}
		}

		public StateModel model => base.app.model.storage.state;

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "ui.screen@open":
				if (!(p_data[0] as UIScreen == view.screen))
				{
					view.pitchPlot.Fade(0f, 0.3f, 0f);
					view.rollPlot.Fade(0f, 0.3f, 0f);
					view.yawPlot.Fade(0f, 0.3f, 0f);
					view.throttlePlot.Fade(0f, 0.3f, 0f);
				}
				break;
			case "ui.screen@close":
				if (p_data[0] as UIScreen == view.screen)
				{
					view.pitchPlot.Fade(0f, 0.3f, 0f);
					view.rollPlot.Fade(0f, 0.3f, 0f);
					view.yawPlot.Fade(0f, 0.3f, 0f);
					view.throttlePlot.Fade(0f, 0.3f, 0f);
				}
				break;
			}
			if (base.app.view.ui.screens.current != view.screen || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
			{
				if (p_data[0] as UIScreen != view.screen)
				{
					break;
				}
				base.app.view.ui.screens.manager.GetComponent<UIScreenManagerController>();
				FCProfileData active = model.player.settings.tuning.GetActive();
				if (active == null)
				{
					Debug.LogWarning("UISettingsController> Invalid Profile\n" + base.app.model.storage.state.player.data.Get<string>("settings-fc-profiles"));
					break;
				}
				int activeIndex = model.player.settings.tuning.GetActiveIndex();
				Debug.Log("UISettingsTuningController> Open - idx[" + activeIndex + "] guid[" + active.guid + "] click");
				view.SetProMode(proMode);
				view.SelectProfile(activeIndex);
				switch (activeIndex)
				{
				case 1:
					view.SelectPreset("custom1");
					break;
				case 2:
					view.SelectPreset("custom2");
					break;
				case 3:
					view.SelectPreset("custom3");
					break;
				default:
					view.SelectPreset("none");
					break;
				}
				if ((bool)view.linkSlidersToggle.toggle)
				{
					view.linkSlidersToggle.toggle.isOn = model.player.settings.game.tuningPromode;
				}
				view.pitchPlot.alpha = 0f;
				view.pitchPlot.Fade(1f, 0.5f, 0.2f);
				view.rollPlot.alpha = 0f;
				if (proMode)
				{
					view.rollPlot.Fade(1f, 0.5f, 0.2f);
				}
				view.yawPlot.alpha = 0f;
				view.yawPlot.Fade(1f, 0.5f, 0.2f);
				view.throttlePlot.alpha = 0f;
				view.throttlePlot.Fade(1f, 0.5f, 0.2f);
				view.SetProfile(active);
				ControllerStateType controllerStateType = RCI.GetControllerStateType(ControllerStateType.Taranis);
				view.SetController(controllerStateType);
				drone.RefreshProfile(active);
				drone.enableDroneControl = true;
				drone.rig = base.app.model.storage.state.player.garage.currentRigData;
				if (base.app.model.game != null && base.app.model.game.playerDrone != null)
				{
					drone.rig = base.app.model.game.playerDrone.rig;
				}
				m_navLocked = false;
				view.betaflightVersion = "4.0.0";
				GameModel game = base.app.model.game;
				if ((bool)game)
				{
					GamePlayerData playerData = game.playerData;
					string text = "";
					text = base.app.model.storage.state.player.garage.defaultRig.frame;
					string text2 = ((playerData == null) ? "" : ((playerData.rig == null) ? "" : playerData.rig.frame));
					if (string.IsNullOrEmpty(text2))
					{
						Debug.LogWarning("UISettingsTuningController> Frame not found! default-frame[" + text + "]");
						text2 = text;
					}
					DronePhysicsSettings physicsSettings = base.app.model.storage.GetPhysicsSettings(text2);
					Debug.Log("UISettingsTuningController> Physics Settings - frame[" + text2 + "] physics-settings[" + (physicsSettings ? physicsSettings.guid : "") + "]");
				}
				this.TimerRunOnce(delegate
				{
					base.app.view.ui.navigation.enabled = true;
					UINavigation.Focus(view.headerNav);
				}, 0.25f);
				view.SetGraphsFormat();
				break;
			}
			case "settings.tuning.profile.preset@click":
			{
				string text3 = p_target.name;
				if (!text3.StartsWith("custom"))
				{
					ControllerStateType controllerStateType2 = RCI.GetControllerStateType(ControllerStateType.XBox);
					FCProfileData.Betaflight.Preset preset = FCProfileData.Betaflight.LowPresets[controllerStateType2];
					switch (text3)
					{
					case "medium":
						preset = FCProfileData.Betaflight.MediumPresets[controllerStateType2];
						break;
					case "high":
						preset = FCProfileData.Betaflight.HighPresets[controllerStateType2];
						break;
					default:
						Debug.LogError("UISettingsTuningController:: unknown preset [" + text3 + "]");
						break;
					case "low":
						break;
					}
					FCProfileData profile = model.player.settings.tuning.GetProfile(0);
					view.SelectProfile(0);
					model.player.settings.tuning.profileActiveGUID = profile.guid;
					view.SetProfile(preset.prRcRate, preset.prSuperRate, preset.prExpo, preset.prRcRate, preset.prSuperRate, preset.prExpo, preset.yawRcRate, preset.yawSuperRate, preset.yawExpo, preset.tMid, preset.tExpo, 0.4f);
				}
				else
				{
					int num = 0;
					switch (text3)
					{
					case "custom1":
						num = 1;
						break;
					case "custom2":
						num = 2;
						break;
					case "custom3":
						num = 3;
						break;
					default:
						Debug.LogError("UISettingsTuningController:: unknown custom preset [" + text3 + "]");
						break;
					}
					FCProfileData profile2 = model.player.settings.tuning.GetProfile(num);
					view.SelectPreset(text3);
					view.SelectProfile(num);
					view.SetProfile(profile2.rcRate.pitch, profile2.superRate.pitch, profile2.expo.pitch, profile2.rcRate.roll, profile2.superRate.roll, profile2.expo.roll, profile2.rcRate.yaw, profile2.superRate.yaw, profile2.expo.yaw, profile2.superRate.throttle, profile2.expo.throttle, 0.4f);
					model.player.settings.tuning.profileActiveGUID = profile2.guid;
				}
				base.app.view.audio.PlayUIGenericSuccess();
				break;
			}
			case "settings.tuning.profile.item@click":
			{
				FadeComponent component = ((UIElementView)p_target).GetComponent<FadeComponent>();
				int num2 = view.profileItems.IndexOf(component);
				view.SelectProfile(num2);
				FCProfileData profile3 = model.player.settings.tuning.GetProfile(num2);
				view.SetProfile(profile3.rcRate.pitch, profile3.superRate.pitch, profile3.expo.pitch, profile3.rcRate.roll, profile3.superRate.roll, profile3.expo.roll, profile3.rcRate.yaw, profile3.superRate.yaw, profile3.expo.yaw, profile3.superRate.throttle, profile3.expo.throttle, 0.4f);
				model.player.settings.tuning.profileActiveGUID = profile3.guid;
				Debug.Log("UISettingsTuningController> Profile [" + num2 + "] guid[" + profile3.guid + "] click");
				base.app.view.audio.PlayUIGenericSuccess();
				break;
			}
			case "settings.tuning.form.element@click":
				OnFormNotification(p_target, p_is_change: false);
				break;
			case "settings.tuning.form.element@change":
				OnFormNotification(p_target, p_is_change: true);
				break;
			case "settings.tuning.drone@click":
				if (drone != null)
				{
					if (m_navLocked)
					{
						view.SetDroneActive(p_active: false);
						LockUINavigation(p_flag: false);
					}
					else
					{
						view.SetDroneActive(p_active: true);
						LockUINavigation(p_flag: true);
					}
				}
				break;
			case "ui.screen.return@click":
				if (drone != null && m_navLocked)
				{
					view.SetDroneActive(p_active: false);
					LockUINavigation(p_flag: false);
					break;
				}
				view.SetDroneActive(p_active: false);
				LockUINavigation(p_flag: false);
				base.app.view.ui.screens.Return();
				if (view.openedFromDashboard)
				{
					Notify(0.2f, "game.pause.return@click");
					Notify(0.8f, "game.ui.dashboard@show", false);
					view.openedFromDashboard = false;
				}
				break;
			case "ui.screen.nav-right@click":
				LockUINavigation(p_flag: false);
				base.app.view.audio.PlayUIGenericSuccess();
				Save();
				break;
			}
		}

		protected void LockUINavigation(bool p_flag)
		{
			m_navLocked = false;
		}

		protected void DelaySave()
		{
			if (m_delay_save != null)
			{
				m_delay_save.Stop();
			}
			m_delay_save = Activity.RunOnce(Save, 2f);
		}

		protected void Save()
		{
			if (!(this == null) && !(base.gameObject == null) && !(base.app == null))
			{
				FCProfileData active = model.player.settings.tuning.GetActive();
				view.GetProfile(active);
				model.player.settings.tuning.UpdateProfile(active);
				Notify("settings.tuning.profile.save", active);
				Debug.Log("UISettingsTuningController> Profile guid[" + active.guid + "] saved!");
			}
		}

		protected void OnFormNotification(UnityEngine.Object p_target, bool p_is_change)
		{
			if (this == null || view == null || !view.isAlive)
			{
				return;
			}
			bool flag = p_is_change;
			string text = p_target.name;
			if (text == null)
			{
				return;
			}
			switch (text)
			{
			case "pitchroll-rc-rate":
			case "pitchroll-super-rate":
			case "pitchroll-expo":
			case "pitch-rc-rate":
			case "pitch-super-rate":
			case "pitch-expo":
			case "roll-rc-rate":
			case "roll-super-rate":
			case "roll-expo":
			case "yaw-rc-rate":
			case "yaw-super-rate":
			case "yaw-expo":
			case "throttle-mid":
			case "throttle-expo":
				if (proMode && view.linkSliders)
				{
					switch (text)
					{
					case "pitch-rc-rate":
						view.rollRCRateSlider.slider.value = view.pitchRCRateSlider.slider.value;
						break;
					case "roll-rc-rate":
						view.pitchRCRateSlider.slider.value = view.rollRCRateSlider.slider.value;
						break;
					case "pitch-super-rate":
						view.rollSuperRateSlider.slider.value = view.pitchSuperRateSlider.slider.value;
						break;
					case "roll-super-rate":
						view.pitchSuperRateSlider.slider.value = view.rollSuperRateSlider.slider.value;
						break;
					case "pitch-expo":
						view.rollExpoSlider.slider.value = view.pitchExpoSlider.slider.value;
						break;
					case "roll-expo":
						view.pitchExpoSlider.slider.value = view.rollExpoSlider.slider.value;
						break;
					}
				}
				if (model.player.settings.tuning.GetActiveIndex() == 0)
				{
					ControllerStateType controllerStateType = RCI.GetControllerStateType(ControllerStateType.XBox);
					if (proMode)
					{
						if (view.pitchRCRate == view.rollRCRate && view.pitchSuperRate == view.rollSuperRate && view.pitchExpo == view.rollExpo)
						{
							switch (FCProfileData.Betaflight.GetPreset(controllerStateType, view.pitchRCRate, view.pitchSuperRate, view.pitchExpo, view.yawRCRate, view.yawSuperRate, view.yawExpo, view.throttleMid, view.throttleExpo))
							{
							case FCProfileData.Betaflight.PresetType.Low:
								view.SelectPreset("low");
								break;
							case FCProfileData.Betaflight.PresetType.Medium:
								view.SelectPreset("medium");
								break;
							case FCProfileData.Betaflight.PresetType.High:
								view.SelectPreset("high");
								break;
							default:
								view.SelectPreset("none");
								break;
							}
						}
						else
						{
							view.SelectPreset("none");
						}
					}
					else
					{
						switch (FCProfileData.Betaflight.GetPreset(controllerStateType, view.pitchRollRCRate, view.pitchRollSuperRate, view.pitchRollExpo, view.yawRCRate, view.yawSuperRate, view.yawExpo, view.throttleMid, view.throttleExpo))
						{
						case FCProfileData.Betaflight.PresetType.Low:
							view.SelectPreset("low");
							break;
						case FCProfileData.Betaflight.PresetType.Medium:
							view.SelectPreset("medium");
							break;
						case FCProfileData.Betaflight.PresetType.High:
							view.SelectPreset("high");
							break;
						default:
							view.SelectPreset("none");
							break;
						}
					}
				}
				view.RefreshPlot();
				if (flag)
				{
					DelaySave();
				}
				if (drone != null)
				{
					FCProfileData active = model.player.settings.tuning.GetActive();
					view.GetProfile(active);
					drone.RefreshProfile(active);
				}
				break;
			case "fc-profiles":
				UINavigation.focus = view.profileItems[0].GetComponent<UINavigation>();
				break;
			case "header":
				UINavigation.focus = view.presetItems[0].GetComponent<UINavigation>();
				break;
			case "help":
				WebBrowser.OpenURL("https://oscarliang.com/rc-roll-pitch-yaw-rate-cleanflight/", (base.app != null) ? base.app.model.service.platform : null);
				break;
			case "link-pitch-roll":
			{
				bool linkSliders = view.linkSliders;
				base.app.model.storage.state.player.settings.game.tuningPromode = linkSliders;
				if (flag)
				{
					DelaySave();
				}
				break;
			}
			}
		}

		protected void SpawnUiDrone()
		{
			if (uiDronePrefab == null)
			{
				Debug.LogError("UISettingsTuningController> uiDrone prefab not assigned");
				return;
			}
			m_drone = UnityEngine.Object.Instantiate(uiDronePrefab);
			m_drone.rig = base.app.model.storage.state.player.garage.currentRigData;
			if (base.app.model.game != null && base.app.model.game.playerDrone != null)
			{
				m_drone.rig = base.app.model.game.playerDrone.rig;
			}
			RectTransform component = m_drone.GetComponent<RectTransform>();
			component.SetParent(view.droneOverlayHolder);
			component.offsetMin = Vector2.zero;
			component.offsetMax = Vector2.zero;
			component.localScale = Vector3.one;
			m_drone.angularVelocityField = view.droneOverlayRotationText;
			m_drone.speedField = view.droneOverlaySpeedText;
			m_drone.renderCanvas = GetComponent<Canvas>();
			Activity.Run((Func<bool>)delegate
			{
				if (m_drone.fade.alpha > 0.99f)
				{
					view.loadingMessage.alpha = 0f;
					view.loadingMessage.gameObject.SetActive(value: false);
					return false;
				}
				view.loadingMessage.alpha = 1f - m_drone.fade.alpha;
				return true;
			}, 0.01f, false);
			Activity.RunOnce(delegate
			{
				DroneRCTransmitter componentInChildren = droneUINav.GetComponentInChildren<UIDroneOverlay>().render.GetComponentInChildren<DroneRCTransmitter>();
				if (componentInChildren != null)
				{
					componentInChildren.invertRoll = true;
				}
			}, 0.5f);
		}

		private void Update()
		{
			if (drone != null && drone.drone != null && drone.drone.fc != null)
			{
				DroneFlightController fc = drone.drone.fc;
				float num = ((view.betaflightPlotter.grid.localScale.y <= 0f) ? 0f : (1f / view.betaflightPlotter.grid.localScale.y));
				view.pitchGraph.SetBounds(-1f, 1f, -500f * num, 500f * num);
				view.pitchGraph.SetCurrent(fc.rawSignal.pitch, fc.signal.pitch);
				if (proMode)
				{
					view.pitchGraph.UpdateCurveMaximum(BetaflightRates.GetMax(view.pitchSuperRate, view.pitchRCRate, view.pitchExpo));
				}
				else
				{
					view.pitchGraph.UpdateCurveMaximum(BetaflightRates.GetMax(view.pitchRollSuperRate, view.pitchRollRCRate, view.pitchRollExpo));
				}
				view.rollGraph.SetBounds(-1f, 1f, -500f * num, 500f * num);
				view.rollGraph.SetCurrent(0f - fc.rawSignal.roll, fc.signal.roll);
				view.rollGraph.UpdateCurveMaximum(BetaflightRates.GetMax(view.rollSuperRate, view.rollRCRate, view.rollExpo));
				view.yawGraph.SetBounds(-1f, 1f, -500f * num, 500f * num);
				view.yawGraph.SetCurrent(fc.rawSignal.yaw, fc.signal.yaw);
				view.yawGraph.UpdateCurveMaximum(BetaflightRates.GetMax(view.yawSuperRate, view.yawRCRate, view.yawExpo));
				view.throttleGraph.SetBounds(0f, 1f, 0f, 1f);
				view.throttleGraph.SetCurrent(fc.rawSignal.throttle, fc.signal.throttle);
			}
		}
	}
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class UISettingsTuningView : UIScreenView
	{
		private bool m_proMode = true;

		public DRLSliderView pitchRollRCRateSlider;

		public DRLSliderView pitchRollSuperRateSlider;

		public DRLSliderView pitchRollExpoSlider;

		public DRLSliderView pitchRCRateSlider;

		public DRLSliderView pitchSuperRateSlider;

		public DRLSliderView pitchExpoSlider;

		public DRLSliderView rollRCRateSlider;

		public DRLSliderView rollSuperRateSlider;

		public DRLSliderView rollExpoSlider;

		public DRLSliderView noobYawRCRateSlider;

		public DRLSliderView noobYawSuperRateSlider;

		public DRLSliderView noobYawExpoSlider;

		public DRLSliderView proYawRCRateSlider;

		public DRLSliderView proYawSuperRateSlider;

		public DRLSliderView proYawExpoSlider;

		public DRLSliderView noobThrottleMidSlider;

		public DRLSliderView noobThrottleExpoSlider;

		public DRLSliderView proThrottleMidSlider;

		public DRLSliderView proThrottleExpoSlider;

		protected DRLSliderView yawRCRateSlider;

		protected DRLSliderView yawSuperRateSlider;

		protected DRLSliderView yawExpoSlider;

		protected DRLSliderView throttleMidSlider;

		protected DRLSliderView throttleExpoSlider;

		public List<FadeComponent> profileItems;

		public List<FadeComponent> presetItems;

		public List<ControllerTypeTag> controllerItems;

		public Text controllerTypeField;

		public UINavigation headerNav;

		public Text physicsSettingsGUIDField;

		public Text betaflightVersionField;

		public FadeComponent loadingMessage;

		public RectTransform[] noobBlocks;

		public RectTransform[] proBlocks;

		public DRLToggleView linkSlidersToggle;

		public RectTransform droneOverlayHolder;

		public RawImage droneOverlayPulse;

		public Image droneOverlayOutline;

		public Text droneOverlayRotationText;

		public Text droneOverlaySpeedText;

		private Color defaultDroneOutlineColor;

		public Color droneActiveColor;

		private bool m_gotDroneColor;

		public bool openedFromDashboard;

		public BetaFlightPlotRenderer betaflightPlotter;

		public UIGraph pitchGraph;

		public UIGraph rollGraph;

		public UIGraph yawGraph;

		public UIGraph throttleGraph;

		public PlotRenderer pitchRollPlot => betaflightPlotter.renderers[0];

		public PlotRenderer pitchPlot => betaflightPlotter.renderers[0];

		public PlotRenderer yawPlot => betaflightPlotter.renderers[1];

		public PlotRenderer rollPlot => betaflightPlotter.renderers[2];

		public PlotRenderer throttlePlot => betaflightPlotter.throttle;

		public float pitchRollRCRate
		{
			get
			{
				if (pitchRollRCRateSlider == null)
				{
					return 0f;
				}
				return Mathf.Round(pitchRollRCRateSlider.value * 100f) * 0.01f;
			}
			set
			{
				if (!(pitchRollRCRateSlider == null))
				{
					pitchRollRCRateSlider.value = value;
					RefreshPlot();
				}
			}
		}

		public float pitchRCRate
		{
			get
			{
				if (pitchRCRateSlider == null)
				{
					return 0f;
				}
				return Mathf.Round(pitchRCRateSlider.value * 100f) * 0.01f;
			}
			set
			{
				if (!(pitchRCRateSlider == null))
				{
					pitchRCRateSlider.value = value;
					RefreshPlot();
				}
			}
		}

		public float rollRCRate
		{
			get
			{
				if (rollRCRateSlider == null)
				{
					return 0f;
				}
				return Mathf.Round(rollRCRateSlider.value * 100f) * 0.01f;
			}
			set
			{
				if (!(rollRCRateSlider == null))
				{
					rollRCRateSlider.value = value;
					RefreshPlot();
				}
			}
		}

		public float pitchRollSuperRate
		{
			get
			{
				if (pitchRollSuperRateSlider == null)
				{
					return 0f;
				}
				return Mathf.Round(pitchRollSuperRateSlider.value * 100f) * 0.01f;
			}
			set
			{
				if (!(pitchRollSuperRateSlider == null))
				{
					pitchRollSuperRateSlider.value = value;
					RefreshPlot();
				}
			}
		}

		public float pitchSuperRate
		{
			get
			{
				if (pitchSuperRateSlider == null)
				{
					return 0f;
				}
				return Mathf.Round(pitchSuperRateSlider.value * 100f) * 0.01f;
			}
			set
			{
				if (!(pitchSuperRateSlider == null))
				{
					pitchSuperRateSlider.value = value;
					RefreshPlot();
				}
			}
		}

		public float rollSuperRate
		{
			get
			{
				if (rollSuperRateSlider == null)
				{
					return 0f;
				}
				return Mathf.Round(rollSuperRateSlider.value * 100f) * 0.01f;
			}
			set
			{
				if (!(rollSuperRateSlider == null))
				{
					rollSuperRateSlider.value = value;
					RefreshPlot();
				}
			}
		}

		public float pitchRollExpo
		{
			get
			{
				if (pitchRollExpoSlider == null)
				{
					return 0f;
				}
				return Mathf.Round(pitchRollExpoSlider.value * 100f) * 0.01f;
			}
			set
			{
				if (!(pitchRollExpoSlider == null))
				{
					pitchRollExpoSlider.value = value;
					RefreshPlot();
				}
			}
		}

		public float pitchExpo
		{
			get
			{
				if (pitchExpoSlider == null)
				{
					return 0f;
				}
				return Mathf.Round(pitchExpoSlider.value * 100f) * 0.01f;
			}
			set
			{
				if (!(pitchExpoSlider == null))
				{
					pitchExpoSlider.value = value;
					RefreshPlot();
				}
			}
		}

		public float rollExpo
		{
			get
			{
				if (rollExpoSlider == null)
				{
					return 0f;
				}
				return Mathf.Round(rollExpoSlider.value * 100f) * 0.01f;
			}
			set
			{
				if (!(rollExpoSlider == null))
				{
					rollExpoSlider.value = value;
					RefreshPlot();
				}
			}
		}

		public float yawRCRate
		{
			get
			{
				if (yawRCRateSlider == null)
				{
					return 0f;
				}
				return Mathf.Round(yawRCRateSlider.value * 100f) * 0.01f;
			}
			set
			{
				if (!(yawRCRateSlider == null))
				{
					yawRCRateSlider.value = value;
					RefreshPlot();
				}
			}
		}

		public float yawSuperRate
		{
			get
			{
				if (yawSuperRateSlider == null)
				{
					return 0f;
				}
				return Mathf.Round(yawSuperRateSlider.value * 100f) * 0.01f;
			}
			set
			{
				if (!(yawSuperRateSlider == null))
				{
					yawSuperRateSlider.value = value;
					RefreshPlot();
				}
			}
		}

		public float yawExpo
		{
			get
			{
				if (yawExpoSlider == null)
				{
					return 0f;
				}
				return Mathf.Round(yawExpoSlider.value * 100f) * 0.01f;
			}
			set
			{
				if (!(yawExpoSlider == null))
				{
					yawExpoSlider.value = value;
					RefreshPlot();
				}
			}
		}

		public float throttleMid
		{
			get
			{
				if (throttleMidSlider == null)
				{
					return 0f;
				}
				return Mathf.Round(throttleMidSlider.value * 100f) * 0.01f;
			}
			set
			{
				if (!(throttleMidSlider == null))
				{
					throttleMidSlider.value = value;
					RefreshPlot();
				}
			}
		}

		public float throttleExpo
		{
			get
			{
				if (throttleExpoSlider == null)
				{
					return 0f;
				}
				return Mathf.Round(throttleExpoSlider.value * 100f) * 0.01f;
			}
			set
			{
				if (!(throttleExpoSlider == null))
				{
					throttleExpoSlider.value = value;
					RefreshPlot();
				}
			}
		}

		public string physicsSettingsGUID
		{
			set
			{
				physicsSettingsGUIDField.text = (string.IsNullOrEmpty(value) ? "" : ("<color=#f00>/</color>  " + value));
			}
		}

		public string betaflightVersion
		{
			set
			{
				betaflightVersionField.text = (string.IsNullOrEmpty(value) ? "" : value);
			}
		}

		public bool linkSliders
		{
			get
			{
				if (linkSlidersToggle.toggle == null)
				{
					return false;
				}
				return linkSlidersToggle.toggle.isOn;
			}
			set
			{
				if (linkSlidersToggle.toggle != null)
				{
					linkSlidersToggle.toggle.isOn = value;
				}
			}
		}

		public bool isAlive
		{
			get
			{
				if (this != null && yawRCRateSlider != null)
				{
					return yawRCRateSlider.slider != null;
				}
				return false;
			}
		}

		public void SelectProfile(int p_id)
		{
			for (int i = 0; i < profileItems.Count; i++)
			{
				FadeComponent fadeComponent = profileItems[i];
				bool flag = i == p_id;
				Transform obj = fadeComponent.transform.Find("outline");
				Transform transform = obj.Find("gray");
				Transform obj2 = obj.Find("green");
				transform.gameObject.SetActive(!flag);
				obj2.gameObject.SetActive(flag);
				fadeComponent.Fade(flag ? 1f : 0.25f, 0.3f);
			}
		}

		public void SelectPreset(string p_name)
		{
			for (int i = 0; i < presetItems.Count; i++)
			{
				FadeComponent fadeComponent = presetItems[i];
				bool flag = fadeComponent.name == p_name;
				Transform obj = fadeComponent.transform.Find("outline");
				Transform transform = obj.Find("gray");
				Transform obj2 = obj.Find("green");
				transform.gameObject.SetActive(!flag);
				obj2.gameObject.SetActive(flag);
				fadeComponent.Fade(flag ? 1f : 0.25f, 0.3f);
			}
		}

		public void SetController(ControllerStateType p_type)
		{
			for (int i = 0; i < controllerItems.Count; i++)
			{
				ControllerTypeTag controllerTypeTag = controllerItems[i];
				controllerTypeTag.gameObject.SetActive(controllerTypeTag.Match(p_type));
			}
			switch (p_type)
			{
			case ControllerStateType.XBox:
				controllerTypeField.text = base.app.model.storage.locale.Get("settings-tuning.controller-presets.label", "CONSOLE CONTROLLER RATES") + ":";
				break;
			case ControllerStateType.PS4:
				controllerTypeField.text = base.app.model.storage.locale.Get("settings-tuning.controller-presets.label", "CONSOLE CONTROLLER RATES") + ":";
				break;
			case ControllerStateType.Taranis:
				controllerTypeField.text = base.app.model.storage.locale.Get("settings-tuning.controller-presets.rc-rates", "RC RATES ") + ":";
				break;
			}
		}

		public void SetProfile(FCProfileData p_data, float p_duration = 0f)
		{
			if (p_data != null)
			{
				SetProfile(p_data.rcRate.pitch, p_data.superRate.pitch, p_data.expo.pitch, p_data.rcRate.roll, p_data.superRate.roll, p_data.expo.roll, p_data.rcRate.yaw, p_data.superRate.yaw, p_data.expo.yaw, p_data.superRate.throttle, p_data.expo.throttle, p_duration);
			}
		}

		public void SetProfile(float p_pitch_r, float p_pitch_sr, float p_pitch_e, float p_roll_r, float p_roll_sr, float p_roll_e, float p_yaw_r, float p_yaw_sr, float p_yaw_e, float p_throttle_m, float p_throttle_e, float p_duration)
		{
			if (m_proMode)
			{
				Tween.Add(this, "pitchRCRate", p_pitch_r, p_duration, Cubic.Out);
				Tween.Add(this, "pitchSuperRate", p_pitch_sr, p_duration, Cubic.Out);
				Tween.Add(this, "pitchExpo", p_pitch_e, p_duration, Cubic.Out);
				Tween.Add(this, "rollRCRate", p_roll_r, p_duration, Cubic.Out);
				Tween.Add(this, "rollSuperRate", p_roll_sr, p_duration, Cubic.Out);
				Tween.Add(this, "rollExpo", p_roll_e, p_duration, Cubic.Out);
			}
			else
			{
				Tween.Add(this, "pitchRollRCRate", p_pitch_r, p_duration, Cubic.Out);
				Tween.Add(this, "pitchRollSuperRate", p_pitch_sr, p_duration, Cubic.Out);
				Tween.Add(this, "pitchRollExpo", p_pitch_e, p_duration, Cubic.Out);
			}
			Tween.Add(this, "yawRCRate", p_yaw_r, p_duration, Cubic.Out);
			Tween.Add(this, "yawSuperRate", p_yaw_sr, p_duration, Cubic.Out);
			Tween.Add(this, "yawExpo", p_yaw_e, p_duration, Cubic.Out);
			Tween.Add(this, "throttleMid", p_throttle_m, p_duration, Cubic.Out);
			Tween.Add(this, "throttleExpo", p_throttle_e, p_duration, Cubic.Out);
		}

		public void GetProfile(FCProfileData p_data)
		{
			if (m_proMode)
			{
				p_data.rcRate.pitch = pitchRCRate;
				p_data.superRate.pitch = pitchSuperRate;
				p_data.expo.pitch = pitchExpo;
				p_data.rcRate.roll = rollRCRate;
				p_data.superRate.roll = rollSuperRate;
				p_data.expo.roll = rollExpo;
			}
			else
			{
				p_data.rcRate.pitchRoll = pitchRollRCRate;
				p_data.superRate.pitchRoll = pitchRollSuperRate;
				p_data.expo.pitchRoll = pitchRollExpo;
			}
			p_data.rcRate.yaw = yawRCRate;
			p_data.superRate.yaw = yawSuperRate;
			p_data.expo.yaw = yawExpo;
			p_data.superRate.throttle = throttleMid;
			p_data.expo.throttle = throttleExpo;
		}

		public void RefreshPitchRoll()
		{
			betaflightPlotter.Plot(0, pitchRollRCRate, pitchRollSuperRate, pitchRollExpo, "deg/s");
		}

		public void RefreshPitch()
		{
			betaflightPlotter.Plot(0, pitchRCRate, pitchSuperRate, pitchExpo, "deg/s");
		}

		public void RefreshRoll()
		{
			betaflightPlotter.Plot(2, rollRCRate, rollSuperRate, rollExpo, "deg/s");
		}

		public void RefreshYaw()
		{
			betaflightPlotter.Plot(1, yawRCRate, yawSuperRate, yawExpo, "deg/s");
		}

		public void RefreshThrottle()
		{
			betaflightPlotter.PlotThrottle(throttleMid, throttleExpo);
		}

		public void RefreshPlot()
		{
			if (m_proMode)
			{
				betaflightPlotter.Plot(0, pitchRCRate, pitchSuperRate, pitchExpo, "deg/s");
				betaflightPlotter.Plot(2, rollRCRate, rollSuperRate, rollExpo, "deg/s");
			}
			else
			{
				betaflightPlotter.Plot(0, pitchRollRCRate, pitchRollSuperRate, pitchRollExpo, "deg/s");
			}
			betaflightPlotter.Plot(1, yawRCRate, yawSuperRate, yawExpo, "deg/s");
			betaflightPlotter.PlotThrottle(throttleMid, throttleExpo);
		}

		public void SetGraphsFormat()
		{
			throttleGraph.inputFormat = "0.0000";
			throttleGraph.outputFormat = "0.0000";
			yawGraph.inputFormat = "0.0000";
			pitchGraph.inputFormat = "0.0000";
			rollGraph.inputFormat = "0.0000";
		}

		public void SetDroneActive(bool p_active)
		{
			if (p_active)
			{
				if (!m_gotDroneColor)
				{
					if (droneOverlayPulse != null)
					{
						m_gotDroneColor = true;
						defaultDroneOutlineColor = droneOverlayPulse.color;
					}
					else
					{
						if (!(droneOverlayOutline != null))
						{
							return;
						}
						m_gotDroneColor = true;
						defaultDroneOutlineColor = droneOverlayOutline.color;
					}
				}
				if (droneOverlayPulse != null)
				{
					droneOverlayPulse.color = droneActiveColor;
				}
				if (droneOverlayOutline != null)
				{
					droneOverlayOutline.color = droneActiveColor;
				}
			}
			else if (m_gotDroneColor)
			{
				if (droneOverlayPulse != null)
				{
					droneOverlayPulse.color = defaultDroneOutlineColor;
				}
				if (droneOverlayOutline != null)
				{
					droneOverlayOutline.color = defaultDroneOutlineColor;
				}
			}
		}

		public void SetProMode(bool p_mode)
		{
			m_proMode = p_mode;
			yawRCRateSlider = (p_mode ? proYawRCRateSlider : noobYawRCRateSlider);
			yawSuperRateSlider = (p_mode ? proYawSuperRateSlider : noobYawSuperRateSlider);
			yawExpoSlider = (p_mode ? proYawExpoSlider : noobYawExpoSlider);
			throttleMidSlider = (p_mode ? proThrottleMidSlider : noobThrottleMidSlider);
			throttleExpoSlider = (p_mode ? proThrottleExpoSlider : noobThrottleExpoSlider);
			RectTransform[] array = proBlocks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(p_mode);
			}
			array = noobBlocks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(!p_mode);
			}
		}
	}
}

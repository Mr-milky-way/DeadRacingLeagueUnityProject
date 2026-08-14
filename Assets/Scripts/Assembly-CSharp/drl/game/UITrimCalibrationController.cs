using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using drl.sim.rci;
using thelab.mvc;

namespace drl.game
{
	public class UITrimCalibrationController : Controller<DRLApp>
	{
		private bool initialized;

		private float sliderSnapThreshold = 0.01f;

		private float[] calibratedValues = new float[4];

		private List<UnityAction<bool>> m_toggleActions = new List<UnityAction<bool>>();

		private List<UnityAction<float>> m_midActions = new List<UnityAction<float>>();

		private List<UnityAction<float>> m_maxActions = new List<UnityAction<float>>();

		private List<UnityAction<float>> m_minActions = new List<UnityAction<float>>();

		private List<UnityAction<float>> m_deadzoneActions = new List<UnityAction<float>>();

		private UnityAction<float> zeroThrottleAction;

		private UnityAction<bool> m_midStickAction;

		public UITrimCalibrationView view => AssertLocal<UITrimCalibrationView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "input.manual-calibration.save@click":
				view.SaveData();
				initialized = false;
				RemoveListeners();
				view.manualCalibrationData = null;
				break;
			case "input.calibration-menu-panel.open@click":
				view.dataSet = false;
				initialized = false;
				RemoveListeners();
				view.manualCalibrationData = null;
				break;
			case "settings.controller.disconnect":
			case "settings.controller.connect":
				view.RefreshNavigationTooltips();
				break;
			}
		}

		private void RemoveListeners()
		{
			int index = 0;
			foreach (ActiveChannel it in view.uiChannels)
			{
				it.invertToggle.onValueChanged.RemoveListener(delegate
				{
					OnInvertToggle(it);
				});
				it.sliderCenter.onValueChanged.RemoveAllListeners();
				it.sliderDeadzone.onValueChanged.RemoveAllListeners();
				it.sliderMax.onValueChanged.RemoveAllListeners();
				it.sliderMin.onValueChanged.RemoveAllListeners();
				it.sliderZero.onValueChanged.RemoveAllListeners();
				it.invertToggle.onValueChanged.RemoveListener(m_toggleActions[index]);
			}
			view.midStickToggle.onValueChanged.RemoveListener(m_midStickAction);
			m_toggleActions.Clear();
			m_deadzoneActions.Clear();
			m_minActions.Clear();
			m_midActions.Clear();
			m_maxActions.Clear();
		}

		private void Setup()
		{
			int num = 0;
			foreach (ActiveChannel it in view.uiChannels)
			{
				AdjustMidSlider(num);
				ResetSlidersColor(it);
				UnityAction<bool> unityAction = delegate
				{
					OnInvertToggle(it);
					UpdateCalibrationData();
				};
				m_toggleActions.Add(unityAction);
				UnityAction<float> unityAction2 = delegate
				{
					UpdateDeadzoneBars(it);
					UpdateCalibrationData();
				};
				m_deadzoneActions.Add(unityAction2);
				UnityAction<float> unityAction3 = delegate
				{
					UpdateCalibrationData();
				};
				m_maxActions.Add(unityAction3);
				m_midActions.Add(unityAction3);
				m_minActions.Add(unityAction3);
				it.sliderDeadzone.onValueChanged.AddListener(unityAction2);
				it.sliderMin.onValueChanged.AddListener(unityAction3);
				it.sliderCenter.onValueChanged.AddListener(unityAction3);
				it.sliderMax.onValueChanged.AddListener(unityAction3);
				it.invertToggle.onValueChanged.AddListener(unityAction);
				if (it.axis == RawAxis.LeftStickY)
				{
					zeroThrottleAction = delegate
					{
						UpdateZeroThrottle(it);
						UpdateCalibrationData();
					};
					it.sliderZero.onValueChanged.AddListener(zeroThrottleAction);
				}
				num++;
			}
			m_midStickAction = delegate
			{
				OnMidStickToggle();
				UpdateCalibrationData();
			};
			view.midStickToggle.onValueChanged.AddListener(m_midStickAction);
			initialized = true;
		}

		private void UpdateZeroThrottle(ActiveChannel p_channel)
		{
			float f = p_channel.sliderZero.value * 2f - 1f;
			view.midStickToggle.isOn = Mathf.Abs(f) < view.zeroThrottleMidStickThreshold;
		}

		private void UpdateCalibrationData()
		{
			if (view.dataSet)
			{
				CalibrationData calibrationData = view.SaveData(snapshotOnly: true);
				if (calibrationData != null)
				{
					Notify("calibration.axis.invert", calibrationData);
				}
			}
		}

		private void Update()
		{
			if (view.dataSet)
			{
				if (view.dataSet && !initialized)
				{
					Setup();
				}
				else
				{
					UpdateUIElements();
				}
			}
		}

		private void UpdateUIElements()
		{
			UpdateRawBars();
			UpdateCalibratedBars();
		}

		private void UpdateRawBars()
		{
			for (int i = 0; i < view.uiChannels.Count; i++)
			{
				ActiveChannel activeChannel = view.uiChannels[i];
				float num = 0f;
				num = RCI.GetRawFromIndex(activeChannel.ID);
				if (num > 0f)
				{
					activeChannel.leftRawBar.fillAmount = num;
					activeChannel.rightRawBar.fillAmount = 0f;
				}
				else
				{
					activeChannel.leftRawBar.fillAmount = 0f;
					activeChannel.rightRawBar.fillAmount = Mathf.Abs(num);
				}
			}
		}

		private void UpdateCalibratedBars()
		{
			for (int i = 0; i < view.uiChannels.Count; i++)
			{
				ActiveChannel activeChannel = view.uiChannels[i];
				float min = -1f * (1f - activeChannel.sliderMin.value);
				float max = 1f - activeChannel.sliderMax.value;
				float value = activeChannel.sliderDeadzone.value;
				float centerPoint = activeChannel.sliderZero.value * 2f - 1f;
				bool isOn = activeChannel.invertToggle.isOn;
				float value2 = activeChannel.sliderCenter.value;
				float num = 0f;
				if (activeChannel.axis != RawAxis.LeftStickY)
				{
					centerPoint = -2f;
				}
				num = RCI.GetAssignedAxisValueFromIndex(activeChannel.ID, min, max, value2, value, centerPoint, isOn);
				if (num >= 0f)
				{
					activeChannel.leftCalibratedBar.fillAmount = num;
					activeChannel.rightCalibratedBar.fillAmount = 0f;
				}
				if (num <= 0f)
				{
					activeChannel.rightCalibratedBar.fillAmount = 0f - num;
					activeChannel.leftCalibratedBar.fillAmount = 0f;
				}
				calibratedValues[i] = num;
			}
		}

		private void UpdateDeadzoneBars(ActiveChannel p_channel)
		{
			float value = p_channel.sliderDeadzone.value;
			string text = (int)(value * 100f) + "%";
			p_channel.sliderDeadzoneLabel.text = view.deadzoneLocalized + " " + text;
			p_channel.deadzoneLeftBar.fillAmount = value;
			p_channel.deadzoneRightBar.fillAmount = value;
		}

		private void OnInvertToggle(ActiveChannel p_channel)
		{
		}

		private void OnMidStickToggle()
		{
			ActiveChannel activeChannel = view.uiChannels.Find((ActiveChannel o) => o.axis == RawAxis.LeftStickY);
			if (activeChannel == null)
			{
				return;
			}
			float f = activeChannel.sliderZero.value * 2f - 1f;
			if ((!view.midStickToggle.isOn || !(Mathf.Abs(f) < view.zeroThrottleMidStickThreshold)) && (view.midStickToggle.isOn || !(Mathf.Abs(f) >= view.zeroThrottleMidStickThreshold)))
			{
				if (view.midStickToggle.isOn)
				{
					activeChannel.sliderZero.value = 0.5f;
				}
				else if (activeChannel.invertToggle.isOn)
				{
					activeChannel.sliderZero.value = 1f;
				}
				else
				{
					activeChannel.sliderZero.value = 0f;
				}
			}
		}

		public void AdjustMidSlider(int idx)
		{
			ActiveChannel activeChannel = view.uiChannels[idx];
			float rawFromIndex = RCI.GetRawFromIndex(activeChannel.ID);
			Image component = activeChannel.sliderCenter.image.transform.Find("stick").GetComponent<Image>();
			if (Mathf.Abs(activeChannel.sliderCenter.value - rawFromIndex) < sliderSnapThreshold)
			{
				if (!RCI.IsRCController() && activeChannel.axis == RawAxis.LeftStickY && activeChannel.sliderZero.value > 0.1f)
				{
					if (1f - Mathf.Abs(calibratedValues[idx]) < sliderSnapThreshold)
					{
						activeChannel.sliderCenter.image.color = view.slidersOptimalColor;
						if (component != null)
						{
							component.color = view.slidersOptimalColor;
						}
					}
				}
				else if (Mathf.Abs(calibratedValues[idx]) < sliderSnapThreshold)
				{
					activeChannel.sliderCenter.image.color = view.slidersOptimalColor;
					if (component != null)
					{
						component.color = view.slidersOptimalColor;
					}
				}
			}
			else
			{
				activeChannel.sliderCenter.image.color = Color.white;
				if (component != null)
				{
					component.color = Color.white;
				}
			}
		}

		public void AdjustMinSlider(int idx)
		{
			ActiveChannel activeChannel = view.uiChannels[idx];
			float rawFromIndex = RCI.GetRawFromIndex(activeChannel.ID);
			float num = -1f * (1f - activeChannel.sliderMin.value);
			Image component = activeChannel.sliderMin.image.transform.Find("stick").GetComponent<Image>();
			if (Mathf.Abs(num - rawFromIndex) < sliderSnapThreshold && 1f - Mathf.Abs(calibratedValues[idx]) < sliderSnapThreshold)
			{
				activeChannel.sliderMin.image.color = view.slidersOptimalColor;
				if (component != null)
				{
					component.color = view.slidersOptimalColor;
				}
			}
			else
			{
				activeChannel.sliderMin.image.color = Color.white;
				if (component != null)
				{
					component.color = Color.white;
				}
			}
		}

		public void AdjustMaxSlider(int idx)
		{
			ActiveChannel activeChannel = view.uiChannels[idx];
			float rawFromIndex = RCI.GetRawFromIndex(activeChannel.ID);
			float num = 1f - activeChannel.sliderMax.value;
			Image component = activeChannel.sliderMax.image.transform.Find("stick").GetComponent<Image>();
			if (Mathf.Abs(num - rawFromIndex) < sliderSnapThreshold && 1f - Mathf.Abs(calibratedValues[idx]) < sliderSnapThreshold)
			{
				activeChannel.sliderMax.image.color = view.slidersOptimalColor;
				if (component != null)
				{
					component.color = view.slidersOptimalColor;
				}
			}
			else
			{
				activeChannel.sliderMax.image.color = Color.white;
				if (component != null)
				{
					component.color = Color.white;
				}
			}
		}

		private void ResetSlidersColor(ActiveChannel p_channel)
		{
			p_channel.sliderCenter.image.color = Color.white;
			p_channel.sliderMax.image.color = Color.white;
			p_channel.sliderMin.image.color = Color.white;
			Image component = p_channel.sliderCenter.image.transform.Find("stick").GetComponent<Image>();
			if (component != null)
			{
				component.color = Color.white;
			}
			Image component2 = p_channel.sliderMin.image.transform.Find("stick").GetComponent<Image>();
			if (component2 != null)
			{
				component2.color = Color.white;
			}
			Image component3 = p_channel.sliderMax.image.transform.Find("stick").GetComponent<Image>();
			if (component3 != null)
			{
				component3.color = Color.white;
			}
		}

		public void ResetChannelUI(int idx)
		{
			view.uiChannels[idx].Reset();
			view.uiChannels[idx].sliderDeadzoneLabel.text = view.deadzoneLocalized + " 0%";
		}
	}
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using drl.sim.rci;
using thelab.mvc;

namespace drl.game
{
	public class UIManualCalibrationController : Controller<DRLApp>
	{
		private bool initialized;

		private float[] calibratedValues = new float[4];

		private float sliderSnapThreshold = 0.05f;

		private List<UnityAction<float>> m_midActions = new List<UnityAction<float>>();

		private List<UnityAction<float>> m_maxActions = new List<UnityAction<float>>();

		private List<UnityAction<float>> m_minActions = new List<UnityAction<float>>();

		private List<UnityAction<float>> m_deadzoneActions = new List<UnityAction<float>>();

		private UnityAction<float> zeroThrottleAction;

		public UIManualCalibrationView view => AssertLocal<UIManualCalibrationView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "input.manual-calibration.save@click":
				view.SaveData();
				initialized = false;
				RemoveListeners();
				break;
			case "input.calibration-menu-panel.open@click":
				view.dataSet = false;
				initialized = false;
				RemoveListeners();
				break;
			}
		}

		private void RemoveListeners()
		{
			foreach (UIChannel it in view.uiChannels)
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
			}
			m_deadzoneActions.Clear();
			m_minActions.Clear();
			m_midActions.Clear();
			m_maxActions.Clear();
		}

		private void Setup()
		{
			int num = 0;
			foreach (UIChannel it in view.uiChannels)
			{
				it.invertToggle.onValueChanged.AddListener(delegate
				{
					OnInvertToggle(it);
				});
				AdjustMidSlider(num);
				ResetSlidersColor(it);
				UnityAction<float> unityAction = delegate
				{
					UpdateDeadzoneBars(it);
					UpdateCalibrationData();
				};
				m_deadzoneActions.Add(unityAction);
				UnityAction<float> unityAction2 = delegate
				{
					UpdateCalibrationData();
				};
				m_maxActions.Add(unityAction2);
				m_midActions.Add(unityAction2);
				m_minActions.Add(unityAction2);
				it.sliderDeadzone.onValueChanged.AddListener(unityAction);
				it.sliderMin.onValueChanged.AddListener(unityAction2);
				it.sliderCenter.onValueChanged.AddListener(unityAction2);
				it.sliderMax.onValueChanged.AddListener(unityAction2);
				if (it.channelSelection.value == 1)
				{
					zeroThrottleAction = delegate
					{
						UpdateCalibrationData();
					};
					it.sliderZero.onValueChanged.AddListener(zeroThrottleAction);
				}
				num++;
			}
			initialized = true;
		}

		private void UpdateCalibrationData()
		{
			CalibrationData calibrationData = view.SaveData(snapshotOnly: true);
			if (calibrationData != null)
			{
				Notify("calibration.axis.invert", calibrationData);
			}
			calibrationData = null;
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
				UIChannel uIChannel = view.uiChannels[i];
				if (i < view.activeChannels.Length)
				{
					float num = 0f;
					int num2 = view.activeChannels[i];
					if (num2 >= 0)
					{
						num = RCI.GetRawFromIndex(num2);
					}
					if (num > 0f)
					{
						uIChannel.leftRawBar.fillAmount = num;
						uIChannel.rightRawBar.fillAmount = 0f;
					}
					else
					{
						uIChannel.leftRawBar.fillAmount = 0f;
						uIChannel.rightRawBar.fillAmount = Mathf.Abs(num);
					}
				}
			}
		}

		private void UpdateCalibratedBars()
		{
			for (int i = 0; i < view.uiChannels.Count; i++)
			{
				UIChannel uIChannel = view.uiChannels[i];
				if (uIChannel.channelSelection.value != 0)
				{
					float min = -1f * (1f - uIChannel.sliderMin.value);
					float max = 1f - uIChannel.sliderMax.value;
					float value = uIChannel.sliderDeadzone.value;
					float num = uIChannel.sliderZero.value * 2f - 1f;
					bool isOn = uIChannel.invertToggle.isOn;
					float value2 = uIChannel.sliderCenter.value;
					float num2 = 0f;
					num2 = RCI.GetAssignedAxisValueFromIndex(centerPoint: (uIChannel.channelSelection.value == 1) ? (isOn ? (0f - num) : num) : (-2f), index: uIChannel.ID, min: min, max: max, center: value2, deadzone: value, inverted: isOn);
					if (num2 >= 0f)
					{
						uIChannel.leftCalibratedBar.fillAmount = num2;
						uIChannel.rightCalibratedBar.fillAmount = 0f;
					}
					if (num2 <= 0f)
					{
						uIChannel.rightCalibratedBar.fillAmount = 0f - num2;
						uIChannel.leftCalibratedBar.fillAmount = 0f;
					}
					calibratedValues[i] = num2;
				}
			}
		}

		private void OnDropdownValueChanged(UIChannel p_channel)
		{
			foreach (UIChannel uiChannel in view.uiChannels)
			{
				if (uiChannel.channelSelection.value == p_channel.channelSelection.value && uiChannel != p_channel)
				{
					uiChannel.channelSelection.value = 0;
					uiChannel.ID = -1;
				}
				uiChannel.invertToggle.interactable = uiChannel.channelSelection.value != 0;
			}
			switch (p_channel.channelSelection.value)
			{
			case 1:
				p_channel.ID = view.manualCalibrationData.ElementIDs[RawAxis.LeftStickY];
				break;
			case 2:
				p_channel.ID = view.manualCalibrationData.ElementIDs[RawAxis.LeftStickX];
				break;
			case 3:
				p_channel.ID = view.manualCalibrationData.ElementIDs[RawAxis.RightStickY];
				break;
			case 4:
				p_channel.ID = view.manualCalibrationData.ElementIDs[RawAxis.RightStickX];
				break;
			default:
				p_channel.ID = -1;
				break;
			}
			view.RefreshIndicatorBars();
		}

		private void UpdateDeadzoneBars(UIChannel p_channel)
		{
			float value = p_channel.sliderDeadzone.value;
			string text = (int)(value * 100f) + "%";
			p_channel.sliderDeadzoneLabel.text = "DEADZONE " + text;
			p_channel.deadzoneLeftBar.fillAmount = value;
			p_channel.deadzoneRightBar.fillAmount = value;
		}

		private void OnInvertToggle(UIChannel p_channel)
		{
			RawAxis rawAxis = RawAxis.LeftStickY;
			switch (p_channel.channelSelection.value)
			{
			case 1:
				rawAxis = RawAxis.LeftStickY;
				break;
			case 2:
				rawAxis = RawAxis.LeftStickX;
				break;
			case 3:
				rawAxis = RawAxis.RightStickY;
				break;
			case 4:
				rawAxis = RawAxis.RightStickX;
				break;
			}
			if (view.manualCalibrationData != null)
			{
				view.manualCalibrationData.Invert[rawAxis] = p_channel.invertToggle.isOn;
				Notify("calibration.axis.invert", rawAxis, view.manualCalibrationData);
			}
			if (rawAxis == RawAxis.LeftStickY)
			{
				p_channel.sliderZero.value = 1f - p_channel.sliderZero.value;
			}
		}

		public void AdjustMidSlider(int idx)
		{
			UIChannel uIChannel = view.uiChannels[idx];
			float rawFromIndex = RCI.GetRawFromIndex(uIChannel.ID);
			if (Mathf.Abs(uIChannel.sliderCenter.value - rawFromIndex) < sliderSnapThreshold)
			{
				if (!RCI.IsRCController() && uIChannel.channelSelection.value == 1 && uIChannel.sliderZero.value > 0.1f)
				{
					if (1f - Mathf.Abs(calibratedValues[idx]) < sliderSnapThreshold)
					{
						uIChannel.sliderCenter.value = rawFromIndex;
						uIChannel.sliderCenter.image.color = view.slidersOptimalColor;
					}
				}
				else if (Mathf.Abs(calibratedValues[idx]) < sliderSnapThreshold)
				{
					uIChannel.sliderCenter.value = rawFromIndex;
					uIChannel.sliderCenter.image.color = view.slidersOptimalColor;
				}
			}
			else
			{
				uIChannel.sliderCenter.image.color = Color.white;
			}
		}

		public void AdjustMinSlider(int idx)
		{
			UIChannel uIChannel = view.uiChannels[idx];
			float rawFromIndex = RCI.GetRawFromIndex(uIChannel.ID);
			if (Mathf.Abs(-1f * (1f - uIChannel.sliderMin.value) - rawFromIndex) < sliderSnapThreshold && 1f - Mathf.Abs(calibratedValues[idx]) < sliderSnapThreshold)
			{
				uIChannel.sliderMin.value = 1f + rawFromIndex;
				uIChannel.sliderMin.image.color = view.slidersOptimalColor;
			}
			else
			{
				uIChannel.sliderMin.image.color = Color.white;
			}
		}

		public void AdjustMaxSlider(int idx)
		{
			UIChannel uIChannel = view.uiChannels[idx];
			float rawFromIndex = RCI.GetRawFromIndex(uIChannel.ID);
			if (Mathf.Abs(1f - uIChannel.sliderMax.value - rawFromIndex) < sliderSnapThreshold && 1f - Mathf.Abs(calibratedValues[idx]) < sliderSnapThreshold)
			{
				uIChannel.sliderMax.value = 1f - rawFromIndex;
				uIChannel.sliderMax.image.color = view.slidersOptimalColor;
			}
			else
			{
				uIChannel.sliderMax.image.color = Color.white;
			}
		}

		private void ResetSlidersColor(UIChannel p_channel)
		{
			p_channel.sliderCenter.image.color = Color.white;
			p_channel.sliderMax.image.color = Color.white;
			p_channel.sliderMin.image.color = Color.white;
		}
	}
}

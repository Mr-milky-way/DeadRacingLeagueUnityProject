using System.Collections.Generic;
using UnityEngine;
using drl.sim.rci;

namespace drl.game
{
	public class UIManualCalibrationView : UIScreenView
	{
		[HideInInspector]
		public CalibrationData manualCalibrationData;

		[HideInInspector]
		public int[] activeChannels;

		public List<UIChannel> uiChannels;

		public Color slidersOptimalColor;

		public bool dataSet { get; set; }

		public void Setup(CalibrationData p_autoCalibrationData)
		{
			manualCalibrationData = p_autoCalibrationData;
			if (p_autoCalibrationData != null)
			{
				activeChannels = new int[4];
				activeChannels[0] = p_autoCalibrationData.ElementIDs[RawAxis.LeftStickY];
				activeChannels[1] = p_autoCalibrationData.ElementIDs[RawAxis.LeftStickX];
				activeChannels[2] = p_autoCalibrationData.ElementIDs[RawAxis.RightStickY];
				activeChannels[3] = p_autoCalibrationData.ElementIDs[RawAxis.RightStickX];
				SetChannelsCalibrationData(p_autoCalibrationData);
				RefreshIndicatorBars();
			}
			dataSet = true;
		}

		public void SetChannelsCalibrationData(CalibrationData p_autoCalibrationData)
		{
			manualCalibrationData.Centers = new float[p_autoCalibrationData.Centers.Length];
			for (int i = 0; i < uiChannels.Count; i++)
			{
				if (i < activeChannels.Length && activeChannels[i] >= 0)
				{
					int num = activeChannels[i];
					if (num < p_autoCalibrationData.Centers.Length)
					{
						uiChannels[i].sliderCenter.value = p_autoCalibrationData.Centers[num];
						uiChannels[i].channelName.text = "CHANNEL " + num + ":";
						uiChannels[i].channelSelection.value = i + 1;
						uiChannels[i].invertToggle.interactable = true;
						uiChannels[i].ID = num;
					}
				}
			}
			if (RCI.IsRCController() || !RCI.HasSavedProfile())
			{
				return;
			}
			RCDeviceData savedProfile = RCI.GetSavedProfile();
			RawAxis rawAxis = RawAxis.LeftStickY;
			foreach (UIChannel uiChannel in uiChannels)
			{
				switch (uiChannel.channelSelection.value)
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
				AssignedAxisData aAD = savedProfile.GetAAD(rawAxis);
				uiChannel.invertToggle.isOn = aAD.inverted;
				p_autoCalibrationData.Invert[rawAxis] = aAD.inverted;
				if (rawAxis == RawAxis.LeftStickY)
				{
					uiChannel.sliderZero.value = (p_autoCalibrationData.Centers[uiChannel.ID] + 1f) / 2f;
					p_autoCalibrationData.ZeroThrottle = p_autoCalibrationData.Centers[uiChannel.ID];
				}
				else
				{
					uiChannel.sliderDeadzone.value = aAD.deadzone;
					uiChannel.deadzoneRightBar.fillAmount = aAD.deadzone;
					uiChannel.deadzoneLeftBar.fillAmount = aAD.deadzone;
					uiChannel.sliderDeadzoneLabel.text = "DEADZONE " + (int)(aAD.deadzone * 100f) + "%";
					p_autoCalibrationData.Deadzone[rawAxis] = aAD.deadzone;
				}
				Notify("calibration.axis.invert", rawAxis, p_autoCalibrationData);
			}
		}

		public void RefreshIndicatorBars()
		{
			foreach (UIChannel uiChannel in uiChannels)
			{
				switch (uiChannel.channelSelection.value)
				{
				case 0:
					uiChannel.calibratedBar.SetActive(value: false);
					uiChannel.sliderZero.gameObject.SetActive(value: false);
					uiChannel.sliderDeadzone.gameObject.SetActive(value: true);
					break;
				case 1:
					uiChannel.calibratedBar.SetActive(value: true);
					uiChannel.sliderZero.gameObject.SetActive(value: true);
					uiChannel.sliderDeadzone.gameObject.SetActive(value: false);
					break;
				default:
					uiChannel.calibratedBar.SetActive(value: true);
					uiChannel.sliderZero.gameObject.SetActive(value: false);
					uiChannel.sliderDeadzone.gameObject.SetActive(value: true);
					break;
				}
			}
		}

		public CalibrationData SaveData(bool snapshotOnly = false)
		{
			if (manualCalibrationData == null)
			{
				return null;
			}
			foreach (UIChannel uiChannel in uiChannels)
			{
				if (uiChannel.channelSelection.value == 0)
				{
					return null;
				}
			}
			for (int i = 0; i < uiChannels.Count; i++)
			{
				RawAxis rawAxis = RawAxis.LeftStickY;
				switch (uiChannels[i].channelSelection.value)
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
				manualCalibrationData.ElementIDs[rawAxis] = activeChannels[i];
				int num = activeChannels[i];
				manualCalibrationData.Centers[num] = uiChannels[i].sliderCenter.value;
				float num2 = 1f - uiChannels[i].sliderMin.value;
				float value = 1f - uiChannels[i].sliderMax.value;
				if (!manualCalibrationData.RangeMin.ContainsKey(rawAxis))
				{
					manualCalibrationData.RangeMin.Add(rawAxis, 0f - num2);
				}
				else
				{
					manualCalibrationData.RangeMin[rawAxis] = 0f - num2;
				}
				if (!manualCalibrationData.RangeMax.ContainsKey(rawAxis))
				{
					manualCalibrationData.RangeMax.Add(rawAxis, value);
				}
				else
				{
					manualCalibrationData.RangeMax[rawAxis] = value;
				}
				if (!manualCalibrationData.Deadzone.ContainsKey(rawAxis))
				{
					manualCalibrationData.Deadzone.Add(rawAxis, uiChannels[i].sliderDeadzone.value);
				}
				else
				{
					manualCalibrationData.Deadzone[rawAxis] = uiChannels[i].sliderDeadzone.value;
				}
				if (!manualCalibrationData.Invert.ContainsKey(rawAxis))
				{
					manualCalibrationData.Invert.Add(rawAxis, uiChannels[i].invertToggle.isOn);
				}
				else
				{
					manualCalibrationData.Invert[rawAxis] = uiChannels[i].invertToggle.isOn;
				}
				if (rawAxis == RawAxis.LeftStickY)
				{
					float num3 = uiChannels[i].sliderZero.value * 2f - 1f;
					manualCalibrationData.ZeroThrottle = (uiChannels[i].invertToggle.isOn ? (0f - num3) : num3);
				}
			}
			if (snapshotOnly)
			{
				return manualCalibrationData;
			}
			Debug.Log("Calibration UI: manualCalibrationData.Save() from ManualView");
			RCI.SetActiveControllerFromIndex(manualCalibrationData);
			manualCalibrationData = null;
			dataSet = false;
			return manualCalibrationData;
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using drl.sim.rci;
using thelab.core;

namespace drl.game
{
	public class UITrimCalibrationView : UIScreenView
	{
		[HideInInspector]
		public CalibrationData manualCalibrationData;

		public List<ActiveChannel> uiChannels;

		public Color slidersOptimalColor;

		public Toggle midStickToggle;

		[HideInInspector]
		public float zeroThrottleMidStickThreshold = 0.05f;

		[HideInInspector]
		public string deadzoneLocalized = "";

		public UINavigation backButtonNavigation;

		public UINavigation saveButtonNavigation;

		public GameObject navigationTooltipXbox;

		public GameObject navigationTooltipPS;

		public UINavigationLinkList midStickProxy;

		public bool dataSet { get; set; }

		public void Setup()
		{
			deadzoneLocalized = base.app.model.storage.locale.Get("calibration.trim.slider-deadzone", "DEADZONE");
			ResetUI();
			if (RCI.HasSavedProfile())
			{
				LoadCalibrationData();
			}
			dataSet = true;
		}

		public void Setup(CalibrationData p_data)
		{
			if (p_data != null)
			{
				manualCalibrationData = new CalibrationData();
				ResetUI();
				LoadCalibrationData(p_data);
				dataSet = true;
				RefreshNavigationTooltips();
			}
		}

		public void ResetUI()
		{
			backButtonNavigation.right = midStickProxy;
			saveButtonNavigation.left = uiChannels[1].resetButton;
			RefreshNavigationTooltips();
			midStickToggle.isOn = false;
			foreach (ActiveChannel uiChannel in uiChannels)
			{
				if (uiChannel.axis == RawAxis.LeftStickY)
				{
					uiChannel.sliderZero.gameObject.SetActive(value: true);
					uiChannel.sliderDeadzone.gameObject.SetActive(value: false);
				}
				else
				{
					uiChannel.sliderZero.gameObject.SetActive(value: false);
					uiChannel.sliderDeadzone.gameObject.SetActive(value: true);
				}
				uiChannel.Reset();
				uiChannel.sliderDeadzoneLabel.text = deadzoneLocalized + " 0%";
			}
		}

		public CalibrationData SaveData(bool snapshotOnly = false)
		{
			if (manualCalibrationData == null)
			{
				manualCalibrationData = new CalibrationData();
				if (RCI.HasSavedProfile())
				{
					foreach (RawAxis item in (IEnumerable<RawAxis>)manualCalibrationData.ElementIDs.Keys.ToList())
					{
						AssignedAxisData aAD = RCI.GetSavedProfile().GetAAD(item);
						manualCalibrationData.ElementIDs[item] = aAD.ElementID;
						if (item == RawAxis.ToggleA || item == RawAxis.ToggleB)
						{
							manualCalibrationData.RangeMax[item] = aAD.max;
							manualCalibrationData.RangeMin[item] = aAD.min;
						}
					}
				}
			}
			manualCalibrationData.Centers = new float[RCI.GetAxisCount()];
			foreach (ActiveChannel uiChannel in uiChannels)
			{
				RawAxis axis = uiChannel.axis;
				if (uiChannel.ID >= 0 && uiChannel.ID < manualCalibrationData.Centers.Length)
				{
					manualCalibrationData.Centers[uiChannel.ID] = uiChannel.sliderCenter.value;
				}
				float num = 1f - uiChannel.sliderMin.value;
				float value = 1f - uiChannel.sliderMax.value;
				if (!manualCalibrationData.RangeMin.ContainsKey(axis))
				{
					manualCalibrationData.RangeMin.Add(axis, 0f - num);
				}
				else
				{
					manualCalibrationData.RangeMin[axis] = 0f - num;
				}
				if (!manualCalibrationData.RangeMax.ContainsKey(axis))
				{
					manualCalibrationData.RangeMax.Add(axis, value);
				}
				else
				{
					manualCalibrationData.RangeMax[axis] = value;
				}
				if (!manualCalibrationData.Deadzone.ContainsKey(axis))
				{
					manualCalibrationData.Deadzone.Add(axis, uiChannel.sliderDeadzone.value);
				}
				else
				{
					manualCalibrationData.Deadzone[axis] = uiChannel.sliderDeadzone.value;
				}
				if (!manualCalibrationData.Invert.ContainsKey(axis))
				{
					manualCalibrationData.Invert.Add(axis, uiChannel.invertToggle.isOn);
				}
				else
				{
					manualCalibrationData.Invert[axis] = uiChannel.invertToggle.isOn;
				}
				if (axis == RawAxis.LeftStickY)
				{
					float zeroThrottle = uiChannel.sliderZero.value * 2f - 1f;
					manualCalibrationData.ZeroThrottle = zeroThrottle;
				}
			}
			if (snapshotOnly)
			{
				return manualCalibrationData;
			}
			Debug.Log("Calibration UI: manualCalibrationData.Save() from TrimView");
			RCI.SetActiveControllerFromIndex(manualCalibrationData);
			manualCalibrationData = null;
			dataSet = false;
			return manualCalibrationData;
		}

		private void LoadCalibrationData()
		{
			RCDeviceData savedProfile = RCI.GetSavedProfile();
			Notify("calibration.invert.reset");
			foreach (ActiveChannel uiChannel in uiChannels)
			{
				uiChannel.invertToggle.isOn = false;
			}
			foreach (ActiveChannel uiChannel2 in uiChannels)
			{
				AssignedAxisData aAD = savedProfile.GetAAD(uiChannel2.axis);
				uiChannel2.invertToggle.isOn = aAD.inverted;
				uiChannel2.sliderMax.value = 1f - aAD.max;
				uiChannel2.sliderMin.value = 1f - Mathf.Abs(aAD.min);
				uiChannel2.sliderCenter.value = aAD.center;
				uiChannel2.sliderDeadzone.value = aAD.deadzone;
				uiChannel2.sliderDeadzoneLabel.text = deadzoneLocalized + " " + (int)(aAD.deadzone * 100f) + "%";
				uiChannel2.deadzoneRightBar.fillAmount = aAD.deadzone;
				uiChannel2.deadzoneLeftBar.fillAmount = aAD.deadzone;
				uiChannel2.ID = aAD.ElementID;
				if (uiChannel2.axis == RawAxis.LeftStickY)
				{
					float zeroThrottle = aAD.zeroThrottle;
					uiChannel2.sliderZero.value = (zeroThrottle + 1f) / 2f;
					midStickToggle.isOn = Mathf.Abs(zeroThrottle) < zeroThrottleMidStickThreshold;
				}
			}
			Notify("calibration.axis.invert", SaveData(snapshotOnly: true));
		}

		private void LoadCalibrationData(CalibrationData p_data)
		{
			if (p_data == null)
			{
				return;
			}
			Notify("calibration.invert.reset");
			foreach (ActiveChannel uiChannel in uiChannels)
			{
				uiChannel.Reset();
				uiChannel.sliderDeadzoneLabel.text = deadzoneLocalized + " 0%";
				uiChannel.ID = p_data.ElementIDs[uiChannel.axis];
				if (uiChannel.ID < RCI.GetAxisCount())
				{
					uiChannel.invertToggle.isOn = p_data.Invert[uiChannel.axis];
					uiChannel.sliderMax.value = 1f - p_data.RangeMax[uiChannel.axis];
					uiChannel.sliderMin.value = 1f - Mathf.Abs(p_data.RangeMin[uiChannel.axis]);
					if (uiChannel.ID >= 0 && uiChannel.ID < p_data.Centers.Length)
					{
						uiChannel.sliderCenter.value = p_data.Centers[uiChannel.ID];
					}
					else
					{
						uiChannel.sliderCenter.value = 0f;
					}
					uiChannel.sliderDeadzone.value = p_data.Deadzone[uiChannel.axis];
					uiChannel.sliderDeadzoneLabel.text = deadzoneLocalized + " " + (int)(p_data.Deadzone[uiChannel.axis] * 100f) + "%";
					uiChannel.deadzoneRightBar.fillAmount = p_data.Deadzone[uiChannel.axis];
					uiChannel.deadzoneLeftBar.fillAmount = p_data.Deadzone[uiChannel.axis];
					if (uiChannel.axis == RawAxis.LeftStickY)
					{
						float zeroThrottle = p_data.ZeroThrottle;
						uiChannel.sliderZero.value = (zeroThrottle + 1f) / 2f;
						midStickToggle.isOn = Mathf.Abs(zeroThrottle) < zeroThrottleMidStickThreshold;
					}
				}
			}
			if (!RCI.IsRCController() && RCI.HasSavedProfile())
			{
				RCDeviceData savedProfile = RCI.GetSavedProfile();
				foreach (ActiveChannel uiChannel2 in uiChannels)
				{
					AssignedAxisData aAD = savedProfile.GetAAD(uiChannel2.axis);
					uiChannel2.invertToggle.isOn = aAD.inverted;
					p_data.Invert[uiChannel2.axis] = aAD.inverted;
					if (uiChannel2.axis == RawAxis.LeftStickY)
					{
						if (aAD.zeroThrottle >= -1f)
						{
							uiChannel2.sliderZero.value = (aAD.zeroThrottle + 1f) / 2f;
							p_data.ZeroThrottle = aAD.zeroThrottle;
							midStickToggle.isOn = Mathf.Abs(aAD.zeroThrottle) < zeroThrottleMidStickThreshold;
						}
					}
					else
					{
						uiChannel2.sliderDeadzone.value = aAD.deadzone;
						uiChannel2.deadzoneRightBar.fillAmount = aAD.deadzone;
						uiChannel2.deadzoneLeftBar.fillAmount = aAD.deadzone;
						uiChannel2.sliderDeadzoneLabel.text = deadzoneLocalized + " " + (int)(aAD.deadzone * 100f) + "%";
						p_data.Deadzone[uiChannel2.axis] = aAD.deadzone;
					}
				}
			}
			manualCalibrationData = p_data;
			Notify("calibration.axis.invert", SaveData(snapshotOnly: true));
		}

		public void RefreshNavigationTooltips()
		{
			DefaultControllerType defaultControllerType = RCI.GetDefaultControllerType(DefaultControllerType.XBox);
			bool active = defaultControllerType == DefaultControllerType.XBox && RCI.GetActiveJoystick() != null;
			bool active2 = defaultControllerType == DefaultControllerType.PS && RCI.GetActiveJoystick() != null;
			navigationTooltipXbox.SetActive(active);
			navigationTooltipPS.SetActive(active2);
		}
	}
}

using System;
using System.Collections.Generic;
using PS4.UsbPeripherals;
using UnityEngine;
using UnityEngine.UI;

public class LibUsbInput : MonoBehaviour
{
	public List<Text> debugTexts;

	private bool m_initialized;

	private List<float> axes = new List<float>();

	private List<float> buttons = new List<float>();

	private List<float> prevAxes = new List<float>();

	private List<float> prevButtons = new List<float>();

	private List<bool> buttonFlags = new List<bool>();

	private List<bool> buttonChangedState = new List<bool>();

	private void Start()
	{
		InitTest();
	}

	public void InitTest()
	{
		Debug.Log("LibUSBInput> Init test..");
		Initialize();
	}

	public void Initialize()
	{
		if (m_initialized)
		{
			return;
		}
		Debug.Log("Initializing USBDInput plugin..");
		try
		{
			int num = USBDInputPlugin.InitializePlugin();
			m_initialized = num == 1;
		}
		catch (Exception ex)
		{
			Debug.Log("LibUSBInput init error - " + ex.Message);
		}
	}

	private void Update()
	{
		if (m_initialized)
		{
			UpdateData();
		}
	}

	private void UpdateData()
	{
		try
		{
			s_structuredDeviceData activeDeviceStructuredData = USBDInputPlugin.GetActiveDeviceStructuredData();
			s_simpleMsg rawData = activeDeviceStructuredData.rawData;
			Debug.Log("RAW DATA: " + string.Join(",", rawData.data));
			Debug.Log("DEVICE MAP: AXIS COUNT - " + activeDeviceStructuredData.deviceMap.numJoysticks + " BUTTON COUNT - " + activeDeviceStructuredData.deviceMap.numButtons);
			debugTexts[0].text = string.Join(",", rawData.data);
			int numJoysticks = activeDeviceStructuredData.deviceMap.numJoysticks;
			int numButtons = activeDeviceStructuredData.deviceMap.numButtons;
			if (numJoysticks == 0 || rawData.length == 0)
			{
				return;
			}
			for (int i = 0; i < numJoysticks; i++)
			{
				float num = (float)activeDeviceStructuredData.joysticks[i].normVal;
				if (i >= prevAxes.Count && i >= axes.Count)
				{
					prevAxes.Add(0f);
					axes.Add(num);
				}
				else
				{
					prevAxes[i] = axes[i];
					axes[i] = num;
				}
			}
			for (int j = 0; j < numButtons && j + numJoysticks < rawData.length; j++)
			{
				float num2 = activeDeviceStructuredData.buttons[j].normVal;
				if (j >= prevButtons.Count && j >= buttons.Count)
				{
					prevButtons.Add(0f);
					buttons.Add(num2);
					buttonChangedState.Add(item: false);
					buttonFlags.Add(item: false);
				}
				else
				{
					prevButtons[j] = buttons[j];
					buttons[j] = num2;
				}
				if (prevButtons[j] != buttons[j])
				{
					buttonFlags[j] = !buttonFlags[j];
				}
				buttonChangedState[j] = prevButtons[j] != buttons[j];
			}
			DebugData(rawData.length, numJoysticks, numButtons);
		}
		catch (Exception ex)
		{
			Debug.Log("RCUsbdInput> Can't update controller data - " + ex.Message);
		}
	}

	private void DebugData(int rawLength, int axisCount, int buttonCount)
	{
		Debug.Log("RAW DATA LENGTH:" + rawLength + " AXIS COUNT: " + axisCount + " BUTTON COUNT: " + buttonCount);
		string text = "CONTROLLER DATA VALUES:\n";
		text = text + "AXES: " + string.Join(",", axes) + "\n";
		text = text + "PREV AXES: " + string.Join(",", prevAxes) + "\n";
		text = text + "BUTTONS: " + string.Join(",", buttons) + "\n";
		text = text + "PREV BUTTONS: " + string.Join(",", prevButtons) + "\n";
		text = text + "CHANGED BUTTONS: " + string.Join(",", buttonChangedState) + "\n";
		text = text + "FLAG BUTTONS: " + string.Join(",", buttonFlags) + "\n";
		Debug.Log(text);
		debugTexts[1].text = text;
	}

	private void OnApplicationQuit()
	{
		m_initialized = false;
		USBDInputPlugin.FinalizePlugin();
		Debug.Log("Finalizing USBDInput plugin..");
	}
}

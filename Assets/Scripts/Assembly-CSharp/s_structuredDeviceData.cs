using System.Collections.Generic;

public struct s_structuredDeviceData
{
	public int numButtons;

	public int numJoysticks;

	public List<s_structuredButtonData> buttons;

	public List<s_structuredJoystickData> joysticks;

	public s_simpleMsg rawData;

	public s_outputMapExp deviceMap;

	public bool doubleBytePrecision;

	public s_structuredDeviceData(int local_numButtons, int local_numJoysticks, s_simpleMsg p_raw_data, s_outputMapExp p_device_map, bool p_doubleBytePrecision)
	{
		numButtons = local_numButtons;
		numJoysticks = local_numJoysticks;
		buttons = new List<s_structuredButtonData>(numButtons);
		joysticks = new List<s_structuredJoystickData>(numJoysticks);
		rawData = p_raw_data;
		deviceMap = p_device_map;
		doubleBytePrecision = p_doubleBytePrecision;
	}
}

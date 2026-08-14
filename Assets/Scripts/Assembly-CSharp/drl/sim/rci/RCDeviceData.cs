using System;
using Rewired;
using thelab.core;

namespace drl.sim.rci
{
	[Serializable]
	public class RCDeviceData
	{
		public string hardwareName;

		public string guid;

		public bool isDefault;

		public DefaultControllerType defaultControllerType;

		public bool centerPointMode;

		public bool rawInputMode;

		public AssignedAxisData[] assignedAxisData;

		public int toggleAsButtonA = -2;

		public int toggleAsButtonB = -2;

		public string customXMLmap;

		public bool usingCustomXMLmap;

		public bool usingAdapter;

		public RCDeviceData(DefaultControllerType dct, bool custom, string controllerName = "default", string id = "", bool uses_adapter = false)
		{
			guid = (string.IsNullOrEmpty(id) ? GUID.Create(12, "", 200, 0, 15, "x1") : id);
			assignedAxisData = new AssignedAxisData[6];
			RawAxis[] array = new RawAxis[6]
			{
				RawAxis.LeftStickX,
				RawAxis.LeftStickY,
				RawAxis.RightStickX,
				RawAxis.RightStickY,
				RawAxis.ToggleA,
				RawAxis.ToggleB
			};
			for (int i = 0; i < 6; i++)
			{
				assignedAxisData[i] = new AssignedAxisData();
				assignedAxisData[i].center = 0f;
				assignedAxisData[i].max = 1f;
				assignedAxisData[i].min = -1f;
				assignedAxisData[i].assignedAxis = (AssignedAxis)i;
				assignedAxisData[i].rawAxis = array[i];
				assignedAxisData[i].inverted = false;
				assignedAxisData[i].deadzone = 0f;
				assignedAxisData[i].ElementID = -1;
				assignedAxisData[i].zeroThrottle = -2f;
				if (dct == DefaultControllerType.Taranis || dct == DefaultControllerType.Nikko || custom)
				{
					continue;
				}
				ControllerMap controllerMap = RCI.GetControllerMap();
				RCI.Controller activeJoystick = RCI.GetActiveJoystick();
				if (activeJoystick == null || activeJoystick.isNetwork || controllerMap == null || array[i] == RawAxis.ToggleA || array[i] == RawAxis.ToggleB)
				{
					continue;
				}
				Joystick joystick = activeJoystick.joystick;
				ActionElementMap firstElementMapWithAction = controllerMap.GetFirstElementMapWithAction((int)array[i]);
				CalibrationMap calibrationMap = joystick.calibrationMap;
				if (firstElementMapWithAction != null)
				{
					assignedAxisData[i].ElementID = firstElementMapWithAction.elementIdentifierId;
					assignedAxisData[i].inverted = (calibrationMap?.GetAxis(firstElementMapWithAction.elementIndex).invert ?? firstElementMapWithAction.invert) && !uses_adapter;
					assignedAxisData[i].deadzone = (uses_adapter ? 0f : 0.081f);
					if (assignedAxisData[i].assignedAxis == AssignedAxis.Throttle)
					{
						assignedAxisData[i].zeroThrottle = (uses_adapter ? (-2f) : 0f);
					}
				}
			}
			defaultControllerType = dct;
			hardwareName = controllerName;
			isDefault = !custom;
			usingCustomXMLmap = false;
			customXMLmap = "";
			usingAdapter = uses_adapter;
		}

		public AssignedAxisData GetAAD(AssignedAxis aa)
		{
			for (int i = 0; i < assignedAxisData.Length; i++)
			{
				if (assignedAxisData[i].assignedAxis == aa)
				{
					return assignedAxisData[i];
				}
			}
			return null;
		}

		public AssignedAxisData GetAAD(RawAxis ra)
		{
			for (int i = 0; i < assignedAxisData.Length; i++)
			{
				if (assignedAxisData[i].rawAxis == ra)
				{
					return assignedAxisData[i];
				}
			}
			return null;
		}
	}
}

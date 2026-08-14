using System;
using System.Collections.Generic;

namespace drl.sim.rci
{
	public class CalibrationData
	{
		public enum CalibrationDataType
		{
			AutoCalibration = 0,
			ManualCalibration = 1
		}

		public Dictionary<RawAxis, int> ElementIDs = new Dictionary<RawAxis, int>
		{
			{
				RawAxis.LeftStickX,
				-1
			},
			{
				RawAxis.LeftStickY,
				-1
			},
			{
				RawAxis.RightStickX,
				-1
			},
			{
				RawAxis.RightStickY,
				-1
			},
			{
				RawAxis.ToggleA,
				-2
			},
			{
				RawAxis.ToggleB,
				-2
			}
		};

		public int[] ActiveChannels = new int[6];

		public float[] Centers = new float[6];

		public Dictionary<RawAxis, float> RangeMin = new Dictionary<RawAxis, float>
		{
			{
				RawAxis.LeftStickX,
				-1f
			},
			{
				RawAxis.LeftStickY,
				-1f
			},
			{
				RawAxis.RightStickX,
				-1f
			},
			{
				RawAxis.RightStickY,
				-1f
			},
			{
				RawAxis.ToggleA,
				-1f
			},
			{
				RawAxis.ToggleB,
				-1f
			}
		};

		public Dictionary<RawAxis, float> RangeMax = new Dictionary<RawAxis, float>
		{
			{
				RawAxis.LeftStickX,
				1f
			},
			{
				RawAxis.LeftStickY,
				1f
			},
			{
				RawAxis.RightStickX,
				1f
			},
			{
				RawAxis.RightStickY,
				1f
			},
			{
				RawAxis.ToggleA,
				1f
			},
			{
				RawAxis.ToggleB,
				1f
			}
		};

		public Dictionary<RawAxis, bool> Invert = new Dictionary<RawAxis, bool>
		{
			{
				RawAxis.LeftStickX,
				false
			},
			{
				RawAxis.LeftStickY,
				false
			},
			{
				RawAxis.RightStickX,
				false
			},
			{
				RawAxis.RightStickY,
				false
			},
			{
				RawAxis.ToggleA,
				false
			},
			{
				RawAxis.ToggleB,
				false
			}
		};

		public Dictionary<RawAxis, float> Deadzone = new Dictionary<RawAxis, float>
		{
			{
				RawAxis.LeftStickX,
				0f
			},
			{
				RawAxis.LeftStickY,
				0f
			},
			{
				RawAxis.RightStickX,
				0f
			},
			{
				RawAxis.RightStickY,
				0f
			},
			{
				RawAxis.ToggleA,
				0f
			},
			{
				RawAxis.ToggleB,
				0f
			}
		};

		public Dictionary<int, Tuple<float, float>> ChannelRange = new Dictionary<int, Tuple<float, float>>();

		public float ZeroThrottle = -1f;
	}
}

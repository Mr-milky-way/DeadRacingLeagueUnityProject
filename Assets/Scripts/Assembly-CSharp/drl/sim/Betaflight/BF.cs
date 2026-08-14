using System;
using UnityEngine;
using drl.sim.Betaflight.Types;

namespace drl.sim.Betaflight
{
	public static class BF
	{
		public const float M_PIf = (float)Math.PI;

		public const float RAD = (float)Math.PI / 180f;

		public const int RX_MAPPABLE_CHANNEL_COUNT = 8;

		public const int MAX_SUPPORTED_RC_CHANNEL_COUNT = 18;

		public const float PTERM_SCALE = 0.032029f;

		public const float ITERM_SCALE = 0.244381f;

		public const float DTERM_SCALE = 0.000529f;

		public static float sq(float f)
		{
			return f * f;
		}

		public static int sq(int i)
		{
			return i * i;
		}

		public static float power3(float f)
		{
			return f * f * f;
		}

		public static int power3(int i)
		{
			return i * i * i;
		}

		public static int constrain(int amt, int low, int high)
		{
			if (amt < low)
			{
				return low;
			}
			if (amt > high)
			{
				return high;
			}
			return amt;
		}

		public static float constrainf(float amt, float low, float high)
		{
			if (amt < low)
			{
				return low;
			}
			if (amt > high)
			{
				return high;
			}
			return amt;
		}

		public static float ABS(float f)
		{
			return Mathf.Abs(f);
		}

		public static int ABS(int i)
		{
			if (i >= 0)
			{
				return i;
			}
			return -i;
		}

		public static float cos_approx(float angle)
		{
			return Mathf.Cos(angle);
		}

		public static float sin_approx(float angle)
		{
			return Mathf.Sin(angle);
		}

		public static float MAX(float a, float b)
		{
			if (!(a > b))
			{
				return b;
			}
			return a;
		}

		public static int MAX(int a, int b)
		{
			if (a <= b)
			{
				return b;
			}
			return a;
		}

		public static float MIN(float a, float b)
		{
			if (!(a < b))
			{
				return b;
			}
			return a;
		}

		public static int MIN(int a, int b)
		{
			if (a >= b)
			{
				return b;
			}
			return a;
		}

		public static float degreesToRadians(int degrees)
		{
			return (float)degrees * ((float)Math.PI / 180f);
		}

		public static int DECIDEGREES_TO_DEGREES(int angle)
		{
			return angle / 10;
		}

		public static int qConstruct(int num, int den)
		{
			return (num << 12) / den;
		}

		public static int qMultiply(int q, int input)
		{
			return input * q >> 12;
		}

		public static bool feature(features_e f)
		{
			return f switch
			{
				features_e.FEATURE_3D => false, 
				features_e.FEATURE_ANTI_GRAVITY => false, 
				_ => false, 
			};
		}

		public static float CONVERT_PARAMETER_TO_FLOAT(int param)
		{
			return 0.001f * (float)param;
		}

		public static float CONVERT_PARAMETER_TO_PERCENT(int param)
		{
			return 0.01f * (float)param;
		}

		public static int getTaskDeltaTime()
		{
			return (int)(Time.fixedDeltaTime * 1000000f);
		}

		public static bool IS_RC_MODE_ACTIVE(boxId_e boxId)
		{
			return boxId switch
			{
				boxId_e.BOXANTIGRAVITY => false, 
				boxId_e.BOXFPVANGLEMIX => false, 
				_ => false, 
			};
		}

		public static bool isAntiGravityModeActive()
		{
			if (!IS_RC_MODE_ACTIVE(boxId_e.BOXANTIGRAVITY))
			{
				return feature(features_e.FEATURE_ANTI_GRAVITY);
			}
			return true;
		}

		public static bool FLIGHT_MODE(flightModeFlags_e mode)
		{
			_ = 64;
			return false;
		}

		public static int GET_DIRECTION(bool isReversed)
		{
			if (!isReversed)
			{
				return 1;
			}
			return -1;
		}
	}
}

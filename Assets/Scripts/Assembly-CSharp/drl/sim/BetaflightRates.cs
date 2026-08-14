using UnityEngine;
using drl.sim.Betaflight;

namespace drl.sim
{
	public class BetaflightRates
	{
		public const string Version = "4.0.0";

		public const float SETPOINT_RATE_LIMIT = 1998f;

		public const float RC_RATE_INCREMENTAL = 14.54f;

		public static float GetRate(float p_rcCommand, float p_superRate, float p_rcRate, float p_rcExpo)
		{
			return calculateSetpointRate(p_rcCommand, p_superRate, p_rcRate, p_rcExpo);
		}

		public static float ReverseRate(float p_angleRate, float p_superRate, float p_rcRate, float p_rcExpo)
		{
			float max = GetMax(p_superRate, p_rcRate, p_rcExpo);
			return p_angleRate * ((max > 1E-06f) ? (1f / max) : 0f);
		}

		public static float GetNormalizedRate(float p_rcCommand, float p_superRate, float p_rcRate, float p_rcExpo)
		{
			return calculateSetpointRate(p_rcCommand, p_superRate, p_rcRate, p_rcExpo) / 1998f;
		}

		public static float GetMin(float p_superRate, float p_rcRate, float p_rcExpo)
		{
			return calculateSetpointRate(-1f, p_superRate, p_rcRate, p_rcExpo);
		}

		public static float GetMax(float p_superRate, float p_rcRate, float p_rcExpo)
		{
			return calculateSetpointRate(1f, p_superRate, p_rcRate, p_rcExpo);
		}

		public static float GetMinNormalized(float p_superRate, float p_rcRate, float p_rcExpo)
		{
			return GetNormalizedRate(-1f, p_superRate, p_rcRate, p_rcExpo);
		}

		public static float GetMaxNormalized(float p_superRate, float p_rcRate, float p_rcExpo)
		{
			return GetNormalizedRate(1f, p_superRate, p_rcRate, p_rcExpo);
		}

		public static float[] GetRates(float[] p_inputs, float p_superRate, float p_rcRate, float p_rcExpo)
		{
			float[] array = new float[p_inputs.Length];
			for (int i = 0; i < p_inputs.Length; i++)
			{
				array[i] = calculateSetpointRate(p_inputs[i], p_superRate, p_rcRate, p_rcExpo);
			}
			return array;
		}

		public static void Plot(ref Vector2[] p_data, float p_superRate, float p_rcRate, float p_rcExpo)
		{
			for (int i = 0; i < p_data.Length; i++)
			{
				p_data[i].y = calculateSetpointRate(p_data[i].x, p_superRate, p_rcRate, p_rcExpo);
			}
		}

		public static float GetThrottle(float p_command, float p_expo, float p_mid)
		{
			float num = generateThrottleCurve(p_command, p_expo, p_mid);
			if (float.IsNaN(num))
			{
				return 0f;
			}
			if (float.IsInfinity(num))
			{
				return 1f;
			}
			return num;
		}

		public static float GetAltitude(float p_command, float p_expo)
		{
			float num = Mathf.Abs(p_command);
			float num2 = Mathf.Clamp01(num * (1f - p_expo + p_expo * num * num));
			if (float.IsNaN(num2))
			{
				return 0f;
			}
			if (float.IsInfinity(num2))
			{
				return Mathf.Sign(p_command);
			}
			return Mathf.Sign(p_command) * num2;
		}

		public static float generateThrottleCurve(float p_command, float p_expo, float p_mid)
		{
			float num = p_command - p_mid;
			float num2 = 1f;
			if (num > 0f)
			{
				num2 = 1f - p_mid;
			}
			if (num < 0f)
			{
				num2 = p_mid;
			}
			return Mathf.Clamp01(p_mid + num * (1f - p_expo + p_expo * num * num / (num2 * num2)));
		}

		public static float calculateSetpointRate(float p_rcCommand, float p_superRate, float p_rcRate, float p_rcExpo)
		{
			float num = p_rcRate;
			if (num > 2f)
			{
				num += 14.54f * (num - 2f);
			}
			float num2 = p_rcCommand;
			float num3 = BF.ABS(num2);
			if (p_rcExpo > 0f)
			{
				num2 = num2 * BF.power3(num3) * p_rcExpo + num2 * (1f - p_rcExpo);
			}
			float num4 = 200f * num * num2;
			if (p_superRate > 0f)
			{
				float num5 = 1f / BF.constrainf(1f - num3 * p_superRate, 0.01f, 1f);
				num4 *= num5;
			}
			return BF.constrainf(num4, -1998f, 1998f);
		}
	}
}

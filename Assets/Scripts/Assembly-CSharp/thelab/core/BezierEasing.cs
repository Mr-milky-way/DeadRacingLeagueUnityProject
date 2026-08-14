using UnityEngine;

namespace thelab.core
{
	public class BezierEasing
	{
		public static float precision = 0.01f;

		public static int maxIteration = 4;

		public static float[] InSineConst = new float[4] { 0.47f, 0f, 0.745f, 0.715f };

		public static float[] InQuadConst = new float[4] { 0.55f, 0.085f, 0.68f, 0.53f };

		public static float[] InCubicConst = new float[4] { 0.55f, 0.055f, 0.675f, 0.19f };

		public static float[] InQuartConst = new float[4] { 0.895f, 0.03f, 0.685f, 0.22f };

		public static float[] InQuintConst = new float[4] { 0.755f, 0.05f, 0.855f, 0.06f };

		public static float[] InCircConst = new float[4] { 0.6f, 0.04f, 0.98f, 0.335f };

		public static float[] InExpoConst = new float[4] { 0.95f, 0.05f, 0.795f, 0.035f };

		public static float[] InBackConst = new float[4] { 0.6f, -0.28f, 0.735f, 0.045f };

		public static float[] OutSineConst = new float[4] { 0.39f, 0.575f, 0.565f, 1f };

		public static float[] OutQuadConst = new float[4] { 0.25f, 0.46f, 0.45f, 0.94f };

		public static float[] OutCubicConst = new float[4] { 0.215f, 0.61f, 0.355f, 1f };

		public static float[] OutQuartConst = new float[4] { 0.165f, 0.84f, 0.44f, 1f };

		public static float[] OutQuintConst = new float[4] { 0.23f, 1f, 0.32f, 1f };

		public static float[] OutCircConst = new float[4] { 0.075f, 0.82f, 0.165f, 1f };

		public static float[] OutExpoConst = new float[4] { 0.19f, 1f, 0.22f, 1f };

		public static float[] OutBackConst = new float[4] { 0.175f, 0.885f, 0.32f, 1.275f };

		public static float[] InOutSineConst = new float[4] { 0.445f, 0.05f, 0.55f, 0.95f };

		public static float[] InOutQuadConst = new float[4] { 0.455f, 0.03f, 0.515f, 0.955f };

		public static float[] InOutCubicConst = new float[4] { 0.645f, 0.045f, 0.355f, 1f };

		public static float[] InOutQuartConst = new float[4] { 0.77f, 0f, 0.175f, 1f };

		public static float[] InOutQuintConst = new float[4] { 0.86f, 0f, 0.07f, 1f };

		public static float[] InOutCircConst = new float[4] { 0.785f, 0.135f, 0.15f, 0.86f };

		public static float[] InOutExpoConst = new float[4] { 1f, 0f, 0f, 1f };

		public static float[] InOutBackConst = new float[4] { 0.68f, -0.55f, 0.265f, 1.55f };

		public static float[] OutInSineConst = new float[4] { 0.05f, 0.445f, 0.95f, 0.55f };

		public static float[] OutInQuadConst = new float[4] { 0.03f, 0.455f, 0.955f, 0.515f };

		public static float[] OutInCubicConst = new float[4] { 0.045f, 0.645f, 1f, 0.355f };

		public static float[] OutInQuartConst = new float[4] { 0f, 0.77f, 1f, 0.175f };

		public static float[] OutInQuintConst = new float[4] { 0f, 0.86f, 1f, 0.07f };

		public static float[] OutInCircConst = new float[4] { 0.135f, 0.785f, 0.86f, 0.15f };

		public static float[] OutInExpoConst = new float[4] { 0f, 1f, 1f, 0f };

		public static float[] OutInBackConst = new float[4] { 0f, 1.25f, 1f, -0.25f };

		public static void Cubic(float p_r, float p_a, float p_b, float p_c, float p_d, out float p_value, out float p_derivative)
		{
			float num = p_r * p_r;
			float num2 = num * p_r;
			float num3 = 1f - p_r;
			float num4 = num3 * num3;
			float num5 = num4 * num3;
			p_value = num5 * p_a + 3f * p_r * num4 * p_b + 3f * num * num3 * p_c + num2 * p_d;
			p_derivative = 3f * num4 * (p_b - p_a) + 6f * num3 * p_r * (p_c - p_b) + 3f * num * (p_d - p_c);
		}

		public static void Cubic(float p_r, float p_b, float p_c, out float p_value, out float p_derivative)
		{
			Cubic(p_r, 0f, p_b, p_c, 1f, out p_value, out p_derivative);
		}

		public static float Sample(float p_r, float p_cx1, float p_cy1, float p_cx2, float p_cy2, float p_bias, int p_max_iteration)
		{
			float num = p_r;
			float p_value = 0f;
			float p_derivative = 0f;
			int num2 = 0;
			for (num2 = 0; num2 < p_max_iteration; num2++)
			{
				Cubic(num, p_cx1, p_cx2, out p_value, out p_derivative);
				float num3 = p_r - p_value;
				if (Mathf.Abs(num3) <= p_bias)
				{
					break;
				}
				float value = ((Mathf.Abs(p_derivative) <= 0.001f) ? ((p_derivative < 0f) ? (-0.001f) : 0.001f) : (num3 / p_derivative));
				value = Mathf.Clamp(value, (0f - Mathf.Abs(num3)) * 4f, Mathf.Abs(num3) * 4f);
				num += value;
			}
			float p_value2 = 0f;
			float p_derivative2 = 0f;
			Cubic(num, p_cy1, p_cy2, out p_value2, out p_derivative2);
			return p_value2;
		}

		public static float Sample(float p_r, float p_cx1, float p_cy1, float p_cx2, float p_cy2)
		{
			return Sample(p_r, p_cx1, p_cy1, p_cx2, p_cy2, precision, maxIteration);
		}

		public static float Sample(float p_r, float[] p_constants, float p_precision, int p_max_iterations)
		{
			return Sample(p_r, p_constants[0], p_constants[1], p_constants[2], p_constants[3], p_precision, p_max_iterations);
		}

		public static float Sample(float p_r, float[] p_constants)
		{
			return Sample(p_r, p_constants, precision, maxIteration);
		}

		public static float InSine(float p_r)
		{
			return Sample(p_r, InSineConst);
		}

		public static float InQuad(float p_r)
		{
			return Sample(p_r, InQuadConst);
		}

		public static float InCubic(float p_r)
		{
			return Sample(p_r, InCubicConst);
		}

		public static float InQuart(float p_r)
		{
			return Sample(p_r, InQuartConst);
		}

		public static float InQuint(float p_r)
		{
			return Sample(p_r, InQuintConst);
		}

		public static float InCirc(float p_r)
		{
			return Sample(p_r, InCircConst);
		}

		public static float InExpo(float p_r)
		{
			return Sample(p_r, InExpoConst);
		}

		public static float InBack(float p_r)
		{
			return Sample(p_r, InBackConst);
		}

		public static float OutSine(float p_r)
		{
			return Sample(p_r, OutSineConst);
		}

		public static float OutQuad(float p_r)
		{
			return Sample(p_r, OutQuadConst);
		}

		public static float OutCubic(float p_r)
		{
			return Sample(p_r, OutCubicConst);
		}

		public static float OutQuart(float p_r)
		{
			return Sample(p_r, OutQuartConst);
		}

		public static float OutQuint(float p_r)
		{
			return Sample(p_r, OutQuintConst);
		}

		public static float OutCirc(float p_r)
		{
			return Sample(p_r, OutCircConst);
		}

		public static float OutExpo(float p_r)
		{
			return Sample(p_r, OutExpoConst);
		}

		public static float OutBack(float p_r)
		{
			return Sample(p_r, OutBackConst);
		}

		public static float InOutSine(float p_r)
		{
			return Sample(p_r, InOutSineConst);
		}

		public static float InOutQuad(float p_r)
		{
			return Sample(p_r, InOutQuadConst);
		}

		public static float InOutCubic(float p_r)
		{
			return Sample(p_r, InOutCubicConst);
		}

		public static float InOutQuart(float p_r)
		{
			return Sample(p_r, InOutQuartConst);
		}

		public static float InOutQuint(float p_r)
		{
			return Sample(p_r, InOutQuintConst);
		}

		public static float InOutCirc(float p_r)
		{
			return Sample(p_r, InOutCircConst);
		}

		public static float InOutExpo(float p_r)
		{
			return Sample(p_r, InOutExpoConst);
		}

		public static float InOutBack(float p_r)
		{
			return Sample(p_r, InOutBackConst);
		}

		public static float OutInSine(float p_r)
		{
			return Sample(p_r, OutInSineConst);
		}

		public static float OutInQuad(float p_r)
		{
			return Sample(p_r, OutInQuadConst);
		}

		public static float OutInCubic(float p_r)
		{
			return Sample(p_r, OutInCubicConst);
		}

		public static float OutInQuart(float p_r)
		{
			return Sample(p_r, OutInQuartConst);
		}

		public static float OutInQuint(float p_r)
		{
			return Sample(p_r, OutInQuintConst);
		}

		public static float OutInCirc(float p_r)
		{
			return Sample(p_r, OutInCircConst);
		}

		public static float OutInExpo(float p_r)
		{
			return Sample(p_r, OutInExpoConst);
		}

		public static float OutInBack(float p_r)
		{
			return Sample(p_r, OutInBackConst);
		}
	}
}

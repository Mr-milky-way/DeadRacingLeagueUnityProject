using System;
using UnityEngine;

namespace drl.sim
{
	public class DroneProp : DronePart
	{
		public string benchId;

		public AnimationCurve response;

		public DronePropType type;

		public float diameter = 5f;

		public float pitch = 4.5f;

		[HideInInspector]
		public float constant;

		[HideInInspector]
		public float powerFactor;

		public int blades = 3;

		public bool ccw;

		public float advanceRatio;

		public DroneMotor motor;

		public AnimationCurve efficiency;

		public AnimationCurve boost;

		[NonSerialized]
		public float maxEfficiency = -1f;

		[NonSerialized]
		public float zeroEfficiencyAdvanceRatio = -1f;

		public float efficiencyOverride;

		public float zeroOverride;

		public static AnimationCurve DefaultEfficiencyCurve
		{
			get
			{
				AnimationCurve animationCurve = new AnimationCurve();
				animationCurve.AddKey(new Keyframe(0f, 0.5f)
				{
					outTangent = 0.5f
				});
				animationCurve.AddKey(new Keyframe(0.0001f, 0.85f)
				{
					inTangent = 0f,
					outTangent = 0f
				});
				animationCurve.AddKey(new Keyframe(1.99f, 0.85f)
				{
					inTangent = 0f,
					outTangent = 0f
				});
				animationCurve.AddKey(new Keyframe(2f, 0f)
				{
					inTangent = -0.5f
				});
				return animationCurve;
			}
		}

		public override string GetPrefix()
		{
			return "P";
		}

		public float AdvanceRatio(float p_rpm, float p_air_speed)
		{
			float num = diameter * 0.0254f * p_rpm / 60f;
			num = ((!(Mathf.Abs(num) <= Mathf.Epsilon)) ? (1f / num) : 0f);
			return Mathf.Clamp(p_air_speed * num, -2f, 2f);
		}

		public float Efficiency(float p_rpm, float p_air_speed)
		{
			advanceRatio = AdvanceRatio(p_rpm, p_air_speed);
			return EvaluateEfficiencyCurve(advanceRatio);
		}

		public float Boost(float p_rpm, float p_air_speed)
		{
			advanceRatio = AdvanceRatio(p_rpm, p_air_speed);
			if (boost != null && boost.length > 2)
			{
				return EvaluateBoostCurve(advanceRatio);
			}
			return EvaluateEfficiencyCurve(advanceRatio);
		}

		public float EvaluateEfficiencyCurve(float p_advanceRatio)
		{
			if (efficiencyOverride > 0f || zeroOverride > 0f)
			{
				CheckMaximums();
			}
			float num = efficiency.Evaluate((zeroOverride > 0f) ? (p_advanceRatio * zeroEfficiencyAdvanceRatio / zeroOverride) : p_advanceRatio);
			if (efficiencyOverride > 0f)
			{
				num *= efficiencyOverride / maxEfficiency;
			}
			return num;
		}

		public float EvaluateBoostCurve(float p_advanceRatio)
		{
			if (efficiencyOverride > 0f || zeroOverride > 0f)
			{
				CheckMaximums();
			}
			float num = boost.Evaluate((zeroOverride > 0f) ? (Mathf.Clamp(p_advanceRatio, 0f, 2f) * zeroEfficiencyAdvanceRatio / zeroOverride) : Mathf.Clamp(p_advanceRatio, 0f, 2f));
			float num2 = boost.Evaluate((zeroOverride > 0f) ? (Mathf.Abs(p_advanceRatio) * zeroEfficiencyAdvanceRatio / zeroOverride) : Mathf.Abs(p_advanceRatio));
			if (num2 > num)
			{
				num = num2;
			}
			if (num > 1f)
			{
				num = 1f + (num - 1f) * 0.33f;
			}
			if (efficiencyOverride > 0f)
			{
				num *= efficiencyOverride / maxEfficiency;
			}
			return num;
		}

		public void CheckMaximums()
		{
			if (maxEfficiency > 0f && zeroEfficiencyAdvanceRatio > 0f)
			{
				return;
			}
			maxEfficiency = -1f;
			zeroEfficiencyAdvanceRatio = -1f;
			for (float num = 0f; num < 2f; num += 0.05f)
			{
				if (maxEfficiency < efficiency.Evaluate(num))
				{
					maxEfficiency = efficiency.Evaluate(num);
				}
				if (zeroEfficiencyAdvanceRatio < 0f && num > 0.24f && efficiency.Evaluate(num) < 0.001f)
				{
					zeroEfficiencyAdvanceRatio = num;
					break;
				}
			}
			if (efficiencyOverride <= 0f)
			{
				efficiencyOverride = maxEfficiency;
			}
			if (zeroOverride <= 0f)
			{
				zeroOverride = zeroEfficiencyAdvanceRatio;
			}
		}

		public void SetEfficiency(float p_max, float p_zero)
		{
			CheckMaximums();
			efficiencyOverride = p_max;
			zeroOverride = p_zero;
		}

		public void SetEfficiency(AnimationCurve p_curve)
		{
			efficiency = p_curve;
			CheckMaximums();
		}

		public void ParseEfficiency(string p_filename)
		{
		}
	}
}

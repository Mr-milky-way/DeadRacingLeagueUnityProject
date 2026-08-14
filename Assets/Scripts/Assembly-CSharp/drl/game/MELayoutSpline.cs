using System;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class MELayoutSpline : MELayoutSurface
	{
		[SerializeField]
		private MASpline m_spline;

		public float marginStart;

		public float marginEnd;

		public float startRatio;

		public float endRatio;

		public float stepRatio;

		public float spacing;

		public float length;

		private List<float> m_ratios;

		public MASpline spline
		{
			get
			{
				if (!m_spline)
				{
					return m_spline = GetComponent<MASpline>();
				}
				return m_spline;
			}
		}

		public void Generate(int p_count, float p_margin_start, float p_margin_end, Action p_callback = null)
		{
			marginStart = p_margin_start;
			marginEnd = p_margin_end;
			length = spline.spline.positions.length;
			startRatio = ((length <= 0f) ? 0f : Mathf.Clamp01(marginStart / length));
			endRatio = 1f - ((length <= 0f) ? 0f : Mathf.Clamp01(marginEnd / length));
			stepRatio = ((length <= 0f) ? 0f : (1f / length));
			m_ratios = new List<float>();
			Generate(p_count, p_callback);
		}

		protected override TransformVector OnAsyncDataStep(int p_index, float p_ratio, TransformVector p_sample)
		{
			TransformVector result = p_sample;
			float num = p_index;
			float num2 = ((spacing <= 0f) ? Mathf.Lerp(startRatio, endRatio, p_ratio) : Mathf.Min(startRatio + num * stepRatio * spacing, endRatio));
			Quaternion identity = Quaternion.identity;
			Vector3 p_result;
			if (spacing > 0f && p_index > 0)
			{
				TransformVector transformVector = base.samples[p_index - 1];
				float num3 = m_ratios[p_index - 1];
				p_result = spline.spline.positions.GetNormalized(num2);
				num2 = GetClosest(transformVector.position, ref p_result, spacing, num3 + stepRatio * 0.01f, 0.01f, 0, 200);
				identity = Quaternion.Euler(spline.spline.rotations.GetNormalized(num2));
			}
			else
			{
				p_result = spline.spline.positions.GetNormalized(num2);
				identity = Quaternion.Euler(spline.spline.rotations.GetNormalized(num2));
			}
			m_ratios.Add(num2);
			result.Set(p_result, identity, Vector3.one);
			return result;
		}

		private float GetClosest(Vector3 p_origin, ref Vector3 p_result, float p_distance, float p_r, float p_precision, int p_i, int p_max)
		{
			float num = Mathf.Clamp01(p_r);
			if (p_i >= p_max)
			{
				p_result = spline.spline.positions.GetNormalized(num);
				return num;
			}
			Vector3 normalized = spline.spline.positions.GetNormalized(num);
			float num2 = Vector3.Distance(normalized, p_origin);
			float num3 = p_distance - num2;
			if (Mathf.Abs(num3) <= p_precision)
			{
				p_result = normalized;
				return num;
			}
			float num4 = num3 * 0.5f;
			if (num3 > 0f)
			{
				num += stepRatio * 0.05f * num4;
			}
			if (num3 < 0f)
			{
				num -= stepRatio * 0.05f * num4;
			}
			return GetClosest(p_origin, ref p_result, p_distance, num, p_precision, p_i + 1, p_max);
		}

		private float Approximate(Vector3 p_target, ref Vector3 p_result, float p_r0, float p_r1, float p_precision, int p_i, int p_max)
		{
			float num = Mathf.Clamp01(p_r0);
			float num2 = Mathf.Clamp01(p_r1);
			float num3 = Mathf.Abs(p_r1 - p_r0);
			if (num3 <= stepRatio * 0.1f || p_i >= p_max)
			{
				num = (num + num2) * 0.5f;
				p_result = spline.spline.positions.GetNormalized(num);
				return num;
			}
			Vector3 normalized = spline.spline.positions.GetNormalized(num);
			float num4 = Vector3.Distance(normalized, p_target);
			if (num4 <= p_precision)
			{
				p_result = normalized;
				return num;
			}
			Vector3 normalized2 = spline.spline.positions.GetNormalized(num2);
			float num5 = Vector3.Distance(normalized2, p_target);
			if (num5 <= p_precision)
			{
				p_result = normalized2;
				return num2;
			}
			if (Mathf.Abs(num4 - num5) <= p_precision)
			{
				num = (num + num2) * 0.5f;
				p_result = spline.spline.positions.GetNormalized(num);
				return num;
			}
			if (num4 < num5)
			{
				return Approximate(p_target, ref p_result, p_r0, p_r1 - num3 * 0.01f, p_precision, p_i + 1, p_max);
			}
			return Approximate(p_target, ref p_result, p_r0 + num3 * 0.01f, p_r1, p_precision, p_i + 1, p_max);
		}

		protected override void OnAsyncDataRefresh()
		{
			for (int i = 0; i < count; i++)
			{
				int num = Mathf.Clamp(i, 0, count - 1);
				int num2 = Mathf.Clamp(i + 1, 0, count - 1);
				if (i >= count - 1)
				{
					num2 = Mathf.Clamp(i - 1, 0, count - 1);
				}
				TransformVector transformVector = base.samples[num];
				Vector3 vector = (base.samples[num2].position - transformVector.position).normalized;
				if (i >= count - 1)
				{
					vector = -vector;
				}
				Vector3 upwards = ((Mathf.Abs(Vector3.Dot(vector, Vector3.up)) > 0.99f) ? (-Vector3.forward) : Vector3.up);
				Quaternion quaternion = Quaternion.LookRotation(vector, upwards);
				transformVector.rotation = ((!orientEnabled) ? Quaternion.identity : quaternion);
				base.samples[num] = transformVector;
			}
			Vector3 zero = Vector3.zero;
			for (int j = 0; j < count; j++)
			{
				TransformVector transformVector2 = base.samples[j];
				Vector3 vector2 = orientOffset + zero;
				zero += orientStep;
				Quaternion quaternion2 = Quaternion.AngleAxis(vector2.y, Vector3.up) * Quaternion.AngleAxis(vector2.x, Vector3.right) * Quaternion.AngleAxis(vector2.z, Vector3.forward);
				Vector3 vector3 = transformVector2.rotation * Vector3.right;
				Vector3 vector4 = transformVector2.rotation * Vector3.up;
				Vector3 vector5 = transformVector2.rotation * Vector3.forward;
				transformVector2.position += vector3 * offsetPosition.x;
				transformVector2.position += vector4 * offsetPosition.y;
				Vector3 vector6 = ditherPosition;
				vector6.Scale(GetRandom(j));
				transformVector2.position += vector3 * vector6.x;
				transformVector2.position += vector4 * vector6.y;
				transformVector2.position += vector5 * vector6.z;
				transformVector2.rotation *= quaternion2;
				base.samples[j] = transformVector2;
			}
		}
	}
}

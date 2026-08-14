using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class SplineTracerSectionComponent : MonoBehaviour
	{
		public SplineTracerComponent tracer;

		public int index;

		public int start;

		public int end;

		public int GetNextSampleIndex(float p_distance)
		{
			int num = index;
			if (!IsValidIndex(num))
			{
				return -1;
			}
			int count = tracer.samples.Count;
			int num2 = ((!(p_distance < 0f)) ? 1 : (-1));
			Vector3 b = tracer.samples[num].position;
			float num3 = 0f;
			float num4 = Mathf.Abs(p_distance);
			if (num4 <= 0.001f)
			{
				return num;
			}
			while ((num2 >= 0 || num > 0) && (num2 <= 0 || num < count - 1))
			{
				num += num2;
				Vector3 position = tracer.samples[num].position;
				num3 += Vector3.Distance(position, b);
				b = position;
				if (num3 >= num4)
				{
					break;
				}
			}
			return num;
		}

		public int GetClosestSampleIndex(Vector3 p_point, bool p_forward_only)
		{
			int num = start;
			if (!IsValidIndex(num))
			{
				return -1;
			}
			int num2 = end;
			if (!IsValidIndex(num2))
			{
				return -1;
			}
			int result = num;
			float num3 = Vector3.Distance(tracer.samples[num].position, p_point);
			bool flag = (p_forward_only = false);
			for (int i = num + 1; i <= num2; i++)
			{
				Vector3 position = tracer.samples[i].position;
				if (flag)
				{
					Quaternion rotation = tracer.samples[i].rotation;
					Vector3 lhs = p_point - position;
					lhs.Normalize();
					Vector3 rhs = rotation * Vector3.forward;
					if (Vector3.Dot(lhs, rhs) > 0.1f)
					{
						continue;
					}
				}
				float num4 = Vector3.Distance(tracer.samples[i].position, p_point);
				if (num4 < num3)
				{
					num3 = num4;
					result = i;
				}
			}
			return result;
		}

		public int GetClosestSampleIndex(Vector3 p_point)
		{
			return GetClosestSampleIndex(p_point, p_forward_only: false);
		}

		private bool IsValidIndex(int p_index)
		{
			if (!tracer)
			{
				return false;
			}
			if (tracer.samples == null)
			{
				return false;
			}
			int count = tracer.samples.Count;
			if (count <= 0)
			{
				return false;
			}
			if (p_index < 0)
			{
				return false;
			}
			if (p_index >= count)
			{
				return false;
			}
			return true;
		}

		[ContextMenu("Debug Path")]
		private void DebugSectionPath()
		{
			if (!tracer)
			{
				return;
			}
			List<TransformVector> samples = tracer.samples;
			if (samples.Count >= 2)
			{
				int num;
				for (num = start + 1; num <= end; num++)
				{
					num = Mathf.Clamp(num, 0, samples.Count - 1);
					int num2 = Mathf.Clamp(num - 1, 0, samples.Count - 1);
					int num3 = Mathf.Clamp(num, 0, samples.Count - 1);
					Vector3 position = samples[num2].position;
					Vector3 position2 = samples[num3].position;
					bool flag = (num & 1) != 0;
					Debug.DrawLine(position, position2, flag ? Color.yellow : Color.black, 30f, depthTest: false);
				}
			}
		}

		[ContextMenu("Debug Index")]
		private void DebugSectionIndex()
		{
			if (!tracer)
			{
				return;
			}
			List<TransformVector> samples = tracer.samples;
			if (samples.Count >= 2)
			{
				int num;
				for (num = index - 8; num <= index + 8; num++)
				{
					num = Mathf.Clamp(num, 0, samples.Count - 1);
					int num2 = Mathf.Clamp(num - 1, 0, samples.Count - 1);
					int num3 = Mathf.Clamp(num, 0, samples.Count - 1);
					Vector3 position = samples[num2].position;
					Vector3 position2 = samples[num3].position;
					bool flag = (num & 1) != 0;
					Debug.DrawLine(position, position2, flag ? Color.red : Color.black, 30f, depthTest: false);
				}
			}
		}
	}
}

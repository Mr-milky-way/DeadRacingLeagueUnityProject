using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class LineTransform : MonoBehaviour
	{
		public AnimationCurve transition = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public AnimationCurve offset = AnimationCurve.Linear(0f, 0f, 1f, 0f);

		[SerializeField]
		private List<Transform> m_anchors;

		private bool m_clamp;

		public List<Transform> anchors
		{
			get
			{
				if (m_anchors != null)
				{
					return m_anchors;
				}
				return m_anchors = new List<Transform>();
			}
		}

		public bool Clamp
		{
			get
			{
				return m_clamp;
			}
			set
			{
				m_clamp = value;
			}
		}

		public static void Evaluate(Vector3 p_target, Vector3 p_a0, Vector3 p_a1, float p_offset, ref Vector3 p_position, ref Quaternion p_rotation, Vector3 p_up)
		{
			Vector3 vector = p_a1 - p_a0;
			float magnitude = vector.magnitude;
			Vector3 vector2 = ((magnitude <= 0f) ? Vector3.zero : (vector / magnitude));
			float num = Vector3.Dot(p_target - p_a0, vector2);
			num += p_offset;
			Vector3 vector3 = p_a0 + vector2 * num;
			Quaternion quaternion = Quaternion.LookRotation(p_target - vector3, p_up);
			p_position = vector3;
			p_rotation = quaternion;
		}

		protected void Awake()
		{
			if (anchors.Count <= 0)
			{
				int childCount = base.transform.childCount;
				for (int i = 0; i < childCount; i++)
				{
					anchors.Add(base.transform.GetChild(i));
				}
			}
		}

		public void Evaluate(Vector3 p_target, ref Vector3 p_position, ref Quaternion p_rotation, Vector3 p_up)
		{
			Transform p_a = null;
			Transform p_a2 = null;
			GetClosestPair(p_target, ref p_a, ref p_a2);
			if ((bool)p_a && (bool)p_a2)
			{
				Vector3 vector = p_a2.position - p_a.position;
				float magnitude = vector.magnitude;
				Vector3 vector2 = ((magnitude <= 0f) ? Vector3.zero : (vector / magnitude));
				float num = Vector3.Dot(p_target - p_a.position, vector2);
				float num2 = ((magnitude <= 0f) ? 0f : (num / magnitude));
				float lineRatio = GetLineRatio(p_target);
				lineRatio = transition.Evaluate(lineRatio);
				float num3 = offset.Evaluate(lineRatio);
				num = (m_clamp ? Mathf.Clamp(num2 * magnitude + num3, 0f, magnitude) : (num2 * magnitude + num3));
				Vector3 vector3 = p_a.position + vector2 * num;
				Quaternion quaternion = Quaternion.LookRotation(p_target - vector3, p_up);
				p_position = vector3;
				p_rotation = quaternion;
			}
		}

		public float GetLineRatio(Vector3 p_target)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			if (anchors.Count <= 1)
			{
				return 0f;
			}
			Transform transform = null;
			Transform transform2 = null;
			for (int i = 1; i < anchors.Count; i++)
			{
				transform = anchors[i - 1];
				transform2 = anchors[i];
				num += Vector3.Distance(transform.position, transform2.position);
			}
			num2 = num;
			Transform p_a = null;
			Transform p_a2 = null;
			GetClosestPair(p_target, ref p_a, ref p_a2);
			if (!p_a)
			{
				return 0f;
			}
			if (!p_a2)
			{
				return 0f;
			}
			int num4 = anchors.IndexOf(p_a);
			int num5 = anchors.IndexOf(p_a2);
			transform = ((num4 < num5) ? p_a : p_a2);
			transform2 = ((num5 < num4) ? p_a : p_a2);
			num4 = anchors.IndexOf(transform);
			num5 = anchors.IndexOf(transform2);
			Vector3 normalized = (transform2.position - transform.position).normalized;
			num = Vector3.Dot(p_target - transform.position, normalized);
			for (int num6 = num4 - 1; num6 >= 0; num6--)
			{
				transform = anchors[num6];
				transform2 = anchors[num6 + 1];
				num += Vector3.Distance(transform.position, transform2.position);
			}
			num3 = num;
			if (!(num2 <= 0f))
			{
				return Mathf.Clamp01(num3 / num2);
			}
			return 0f;
		}

		public void GetClosestPair(Vector3 p_position, ref Transform p_a0, ref Transform p_a1)
		{
			if (anchors.Count <= 1)
			{
				p_a0 = (p_a1 = null);
				return;
			}
			if (anchors.Count == 2)
			{
				p_a0 = anchors[0];
				p_a1 = anchors[1];
				return;
			}
			int num = 0;
			float num2 = Distance(p_position, anchors[num].position, anchors[num + 1].position);
			for (int i = 2; i < anchors.Count; i++)
			{
				Transform transform = anchors[i - 1];
				Transform transform2 = anchors[i];
				float num3 = Distance(p_position, transform.position, transform2.position);
				if (num3 < num2)
				{
					num = i - 1;
					num2 = num3;
				}
			}
			int index = num;
			int index2 = num + 1;
			p_a0 = anchors[index];
			p_a1 = anchors[index2];
		}

		protected float Distance(Vector3 p, Vector3 a, Vector3 b)
		{
			Vector3 vector = b - a;
			float magnitude = vector.magnitude;
			vector.Normalize();
			Vector3 rhs = p - a;
			float value = Vector3.Dot(vector, rhs);
			value = Mathf.Clamp(value, 0f, magnitude);
			a += vector * value;
			return Vector3.Distance(p, a);
		}
	}
}

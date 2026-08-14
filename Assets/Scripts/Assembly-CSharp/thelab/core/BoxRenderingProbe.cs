using UnityEngine;

namespace thelab.core
{
	public class BoxRenderingProbe : RenderingProbe
	{
		public BoxCollider outer;

		public BoxCollider inner;

		public override float GetDistance(Vector3 p_position)
		{
			if (IsInside(p_position, outer))
			{
				return 0f;
			}
			Vector3 b = outer.ClosestPointOnBounds(p_position);
			return Vector3.Distance(p_position, b);
		}

		public bool IsInside(Vector3 p_position, BoxCollider p_box)
		{
			p_position = p_box.transform.InverseTransformPoint(p_position) - p_box.center;
			float num = p_box.size.x * 0.5f;
			if (p_position.x < 0f - num)
			{
				return false;
			}
			if (p_position.x > num)
			{
				return false;
			}
			float num2 = p_box.size.y * 0.5f;
			if (p_position.y < 0f - num2)
			{
				return false;
			}
			if (p_position.y > num2)
			{
				return false;
			}
			float num3 = p_box.size.z * 0.5f;
			if (p_position.z < 0f - num3)
			{
				return false;
			}
			if (p_position.z > num3)
			{
				return false;
			}
			return true;
		}

		public override float GetIntensity(Vector3 p_position)
		{
			if (!outer)
			{
				return 0f;
			}
			if (!inner)
			{
				return 0f;
			}
			if (!IsInside(p_position, outer))
			{
				return 0f;
			}
			if (IsInside(p_position, inner))
			{
				return 1f;
			}
			Vector3 vector = inner.ClosestPointOnBounds(p_position);
			Vector3 vector2 = p_position - vector;
			Vector3 vector3 = p_position + vector2.normalized * (inner.size - outer.size).magnitude * 0.5f;
			for (int i = 0; i < 5; i++)
			{
				vector3 = outer.ClosestPointOnBounds(vector3);
				if (i >= 4)
				{
					break;
				}
				vector3 -= vector;
				float num = Vector3.Dot(vector3, vector2.normalized);
				vector3 = vector + vector2.normalized * num;
			}
			vector2 = p_position - vector3;
			float num2 = Vector3.Distance(vector3, vector);
			float num3 = Vector3.Dot(vector2, (vector - vector3).normalized);
			if (num2 <= 0f)
			{
				return 1f;
			}
			return Mathf.Clamp01(num3 / num2);
		}

		protected override void OnDrawGizmos()
		{
		}
	}
}

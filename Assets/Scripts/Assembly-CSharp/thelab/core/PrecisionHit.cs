using System;
using UnityEngine;

namespace thelab.core
{
	[Serializable]
	public class PrecisionHit
	{
		public PrecisionCollider target;

		public Collider from;

		public Collider to;

		public Rigidbody rigidbody;

		public Vector3 enter;

		public Vector3 exit;

		public Vector3 center;

		public Vector3 size;

		public Vector3 precision;

		public Vector3 normalized;

		public Vector3 orientation;

		public float ratio;

		public float distance;

		public PrecisionHit()
		{
		}

		public PrecisionHit(PrecisionHit v)
		{
			Set(v);
		}

		public void Set(PrecisionHit v)
		{
			target = v.target;
			from = v.from;
			enter = v.enter;
			exit = v.exit;
			precision = v.precision;
			size = v.size;
		}

		public Vector3 GetNormalized(Vector3 p_precision)
		{
			return new Vector3
			{
				x = Mathf.Clamp01((Mathf.Abs(size.x) <= 1E-07f) ? 1f : (p_precision.x / size.x)),
				y = Mathf.Clamp01((Mathf.Abs(size.y) <= 1E-07f) ? 1f : (p_precision.y / size.y)),
				z = Mathf.Clamp01((Mathf.Abs(size.z) <= 1E-07f) ? 1f : (p_precision.z / size.z))
			};
		}

		public float GetRatio(Vector3 p_precision, bool p_normalized = false)
		{
			return Mathf.Clamp01((p_normalized ? p_precision : GetNormalized(p_precision)).magnitude);
		}

		public Vector3 GetOrientation()
		{
			Vector3 vector = exit - center;
			if (!from)
			{
				return Vector3.zero;
			}
			return from.transform.InverseTransformVector(vector);
		}

		public void Clear()
		{
			target = null;
			from = null;
			enter = (exit = (precision = (size = Vector3.zero)));
		}
	}
}

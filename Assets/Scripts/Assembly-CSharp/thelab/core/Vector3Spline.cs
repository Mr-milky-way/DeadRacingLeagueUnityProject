using UnityEngine;

namespace thelab.core
{
	public class Vector3Spline : Spline<Vector3>
	{
		public Vector3Spline(SplineType p_type, int p_length, float p_precision)
			: base(p_type, p_length, p_precision)
		{
		}

		public Vector3Spline(SplineType p_type, int p_length)
			: base(p_type, p_length)
		{
		}

		public Vector3Spline(Spline<Vector3> p_src)
			: base(p_src)
		{
		}

		protected override float Distance(Vector3 a, Vector3 b)
		{
			return Vector3.Distance(a, b);
		}

		protected override Vector3 Lerp(float r, Vector3[] vl)
		{
			return Spline.Lerp(base.type, r, vl);
		}

		protected override Vector3 Move(Vector3 p0, Vector3 p1, float s)
		{
			return p0 + (p1 - p0).normalized * s;
		}
	}
}

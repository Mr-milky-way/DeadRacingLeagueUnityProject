using UnityEngine;

namespace thelab.core
{
	public class Vector3Interpolator : Interpolator<Vector3>
	{
		public Vector3Interpolator(InterpolationType p_type)
			: base(p_type)
		{
		}

		protected override float Distance(Vector3 a, Vector3 b)
		{
			return Vector3.Distance(a, b);
		}

		protected override Vector3 Add(Vector3 a, Vector3 b)
		{
			return a + b;
		}

		protected override Vector3 Lerp(Vector3 a, Vector3 b, float r)
		{
			return a + (b - a) * r;
		}

		protected override Vector3 Move(Vector3 p_value, Vector3 p_vector, float p_step)
		{
			return p_value + p_vector * p_step;
		}

		protected override Vector3 Mul(Vector3 v, float n)
		{
			return v * n;
		}

		protected override Vector3 Sub(Vector3 a, Vector3 b)
		{
			return a - b;
		}

		protected override Vector3 Zero()
		{
			return Vector3.zero;
		}
	}
}

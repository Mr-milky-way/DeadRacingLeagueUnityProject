using UnityEngine;

namespace thelab.core
{
	public class FloatInterpolator : Interpolator<float>
	{
		public FloatInterpolator(InterpolationType p_type)
			: base(p_type)
		{
		}

		protected override float Distance(float a, float b)
		{
			return Mathf.Abs(b - a);
		}

		protected override float Add(float a, float b)
		{
			return a + b;
		}

		protected override float Lerp(float a, float b, float r)
		{
			return a + (b - a) * r;
		}

		protected override float Move(float p_value, float p_vector, float p_step)
		{
			return p_value + p_vector * p_step;
		}

		protected override float Mul(float v, float n)
		{
			return v * n;
		}

		protected override float Sub(float a, float b)
		{
			return a - b;
		}

		protected override float Zero()
		{
			return 0f;
		}
	}
}

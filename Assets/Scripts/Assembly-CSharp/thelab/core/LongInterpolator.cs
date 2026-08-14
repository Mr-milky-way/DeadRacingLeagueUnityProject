using UnityEngine;

namespace thelab.core
{
	public class LongInterpolator : Interpolator<long>
	{
		public LongInterpolator(InterpolationType p_type)
			: base(p_type)
		{
		}

		protected override float Distance(long a, long b)
		{
			return (long)Mathf.Abs(b - a);
		}

		protected override long Add(long a, long b)
		{
			return a + b;
		}

		protected override long Lerp(long a, long b, float r)
		{
			return (long)((float)a + (float)(b - a) * r);
		}

		protected override long Move(long p_value, long p_vector, float p_step)
		{
			return (long)((float)p_value + (float)p_vector * p_step);
		}

		protected override long Mul(long v, float n)
		{
			return (long)((float)v * n);
		}

		protected override long Sub(long a, long b)
		{
			return a - b;
		}

		protected override long Zero()
		{
			return 0L;
		}
	}
}

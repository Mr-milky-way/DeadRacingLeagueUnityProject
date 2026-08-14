using UnityEngine;

namespace thelab.mvc
{
	public class FloatStepperView : ListStepperView<float>
	{
		public float step;

		public float minValue;

		public float maxValue = 1f;

		protected override float GetValue(int p_count)
		{
			step = Mathf.Abs(max - min);
			if (step <= Mathf.Epsilon)
			{
				step = 0f;
			}
			else
			{
				step = 1f / step;
			}
			if (values.Count > 0)
			{
				return base.GetValue(p_count);
			}
			return Mathf.Clamp((float)p_count * step, minValue, maxValue);
		}

		protected override string GetValueString()
		{
			return value.ToString(format);
		}
	}
}

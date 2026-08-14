using UnityEngine;

namespace thelab.mvc
{
	public class IntStepperView : ListStepperView<int>
	{
		public int step;

		public int minValue;

		public int maxValue = 1;

		protected override int GetValue(int p_count)
		{
			step = Mathf.Abs(max - min);
			if (step <= 0)
			{
				step = 0;
			}
			else
			{
				step = 1;
			}
			if (values.Count > 0)
			{
				return base.GetValue(p_count);
			}
			return Mathf.Clamp(p_count * step, minValue, maxValue);
		}

		protected override string GetValueString()
		{
			return value.ToString(format);
		}
	}
}

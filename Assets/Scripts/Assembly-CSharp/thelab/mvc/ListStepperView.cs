using System.Collections.Generic;
using UnityEngine;

namespace thelab.mvc
{
	public class ListStepperView<T> : StepperView<T>
	{
		public List<T> values;

		public void SetValue(T v)
		{
			int num = values.IndexOf(v);
			if (num < 0)
			{
				T val = v;
				Debug.LogWarning("StepperView> Value [" + val?.ToString() + "] not found!");
			}
			else
			{
				index = num;
				Refresh();
			}
		}

		protected override T GetValue(int p_count)
		{
			if (values.Count <= 0)
			{
				return default(T);
			}
			int num = Mathf.Clamp(p_count, 0, values.Count - 1);
			return values[num];
		}

		public override string GetLabelText()
		{
			if (!showValue)
			{
				return base.GetLabelText();
			}
			return GetValueString();
		}
	}
}

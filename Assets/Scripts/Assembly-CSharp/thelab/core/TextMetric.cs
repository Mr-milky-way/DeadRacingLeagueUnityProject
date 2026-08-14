using UnityEngine;

namespace thelab.core
{
	public class TextMetric : TextNumber<float>
	{
		public enum ValueFormat
		{
			None = 0,
			MetricDistance = 1,
			ImperialDistance = 2,
			Count = 3
		}

		public enum UnitFormat
		{
			None = 0,
			LowerCase = 1,
			UpperCase = 2
		}

		public ValueFormat outputFormat = ValueFormat.MetricDistance;

		public UnitFormat unitFormat = UnitFormat.LowerCase;

		public string unitSeparator = "";

		public override string GetStringValue()
		{
			float num = base.value;
			switch (outputFormat)
			{
			case ValueFormat.ImperialDistance:
				num *= 3.28084f;
				break;
			}
			string unitFromMetric = GetUnitFromMetric(num);
			float num2 = Mathf.Abs(num);
			switch (outputFormat)
			{
			case ValueFormat.Count:
				if (num2 >= 1000f)
				{
					num /= 1000f;
				}
				if (num2 >= 1000000f)
				{
					num /= 1000f;
				}
				break;
			case ValueFormat.MetricDistance:
				if (num2 < 0.99999f)
				{
					num *= 100f;
				}
				if (num2 >= 1000f)
				{
					num /= 1000f;
				}
				break;
			case ValueFormat.ImperialDistance:
				if (num2 < 0.99999f)
				{
					num *= 12f;
				}
				if (num2 >= 5280f)
				{
					num /= 5280f;
				}
				break;
			}
			return num.ToString(format) + unitFromMetric;
		}

		private string GetUnitFromMetric(float v)
		{
			string text = "";
			float num = Mathf.Abs(v);
			switch (outputFormat)
			{
			case ValueFormat.None:
				text = "";
				break;
			case ValueFormat.Count:
				if (num > 0.004f)
				{
					if (num >= 1000000f)
					{
						text = "m";
						break;
					}
					if (num >= 1000f)
					{
						text = "k";
						break;
					}
				}
				text = "";
				break;
			case ValueFormat.MetricDistance:
				if (num > 0.004f)
				{
					if (num < 0.99999f)
					{
						text = "cm";
						break;
					}
					if (num >= 1000f)
					{
						text = "km";
						break;
					}
				}
				text = "m";
				break;
			case ValueFormat.ImperialDistance:
				if (num > 0.004f)
				{
					if (num < 0.99999f)
					{
						text = "in";
						break;
					}
					if (num >= 5280f)
					{
						text = "mi";
						break;
					}
				}
				text = "ft";
				break;
			}
			switch (unitFormat)
			{
			case UnitFormat.LowerCase:
				text = text.ToLower();
				break;
			case UnitFormat.UpperCase:
				text = text.ToUpper();
				break;
			}
			return unitSeparator + text;
		}

		protected override bool HasValueChanged(float a, float b)
		{
			return Mathf.Abs(a - b) > 0.005f;
		}
	}
}

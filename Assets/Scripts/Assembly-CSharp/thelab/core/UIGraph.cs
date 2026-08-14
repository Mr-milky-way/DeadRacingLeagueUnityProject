using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace thelab.core
{
	public class UIGraph : MonoBehaviour
	{
		public UILineRenderer curve;

		public Text maxValueField;

		public Text minValueField;

		public Text maxRangeField;

		public Text minRangeField;

		public RectTransform inputValueMarker;

		public RectTransform outputValueMarker;

		public RectTransform maxCurveValueMarker;

		public Text inputValueField;

		public Text outputValueField;

		public Text maxCurveValueField;

		public Text caption;

		public Text xAxisLabel;

		public Text yAxisLabel;

		public Text[] xAxisValues;

		public Text[] yAxisValues;

		public string minMaxFormat = "0.0";

		public string rangeFormat = "0.0";

		public string inputFormat = "0.0";

		public string outputFormat = "0.0";

		private float m_maxValue = float.MaxValue;

		private float m_minValue = float.MaxValue;

		private float m_minRange = float.MaxValue;

		private float m_maxRange = float.MaxValue;

		private Vector2 m_current = new Vector2(float.MaxValue, float.MaxValue);

		public float maxValue
		{
			get
			{
				return m_maxValue;
			}
			set
			{
				if (maxValueField != null && m_maxValue != value)
				{
					maxValueField.text = FormatNumber(value, minMaxFormat);
				}
				m_maxValue = value;
			}
		}

		public float minValue
		{
			get
			{
				return m_minValue;
			}
			set
			{
				if (minValueField != null && m_minValue != value)
				{
					minValueField.text = FormatNumber(value, minMaxFormat);
				}
				m_minValue = value;
			}
		}

		public float minRange
		{
			get
			{
				return m_minRange;
			}
			set
			{
				if (minRangeField != null && m_minRange != value)
				{
					minRangeField.text = FormatNumber(value, rangeFormat);
				}
				m_minRange = value;
			}
		}

		public float maxRange
		{
			get
			{
				return m_maxRange;
			}
			set
			{
				if (maxRangeField != null && m_maxRange != value)
				{
					maxRangeField.text = FormatNumber(value, rangeFormat);
				}
				m_maxRange = value;
			}
		}

		public Vector2 current
		{
			get
			{
				return m_current;
			}
			set
			{
				SetCurrent(value.x, value.y);
			}
		}

		public void SetCurrent(Vector2 p_current)
		{
			SetCurrent(p_current.x, p_current.y);
		}

		public void SetCurrent(Vector2 p_current, Vector2 p_labels)
		{
			SetCurrent(p_current.x, p_current.y, p_labels.x, p_labels.y);
		}

		public void SetCurrent(float p_x, float p_y)
		{
			if (inputValueField != null && m_current.x != p_x)
			{
				inputValueField.text = FormatNumber(p_x, inputFormat);
			}
			if (outputValueField != null && m_current.y != p_y)
			{
				outputValueField.text = FormatNumber(p_y, outputFormat);
			}
			if (inputValueMarker != null && m_current.x != p_x)
			{
				inputValueMarker.anchoredPosition = new Vector2(Mathf.Lerp(curve.rectTransform.rect.xMin, curve.rectTransform.rect.xMax, (p_x - minRange) / (maxRange - minRange)), 0f);
			}
			if (outputValueMarker != null && m_current.y != p_y)
			{
				outputValueMarker.anchoredPosition = new Vector2(0f, Mathf.Lerp(curve.rectTransform.rect.yMin, curve.rectTransform.rect.yMax, (p_y - minValue) / (maxValue - minValue)));
			}
			m_current = new Vector2(p_x, p_y);
		}

		public void SetCurrent(float p_x, float p_y, float p_labelX, float p_labelY)
		{
			if (inputValueField != null && m_current.x != p_x)
			{
				inputValueField.text = FormatNumber(p_labelX, inputFormat);
			}
			if (outputValueField != null && m_current.y != p_y)
			{
				outputValueField.text = FormatNumber(p_labelY, outputFormat);
			}
			if (inputValueMarker != null && m_current.x != p_x)
			{
				inputValueMarker.anchoredPosition = new Vector2(Mathf.Lerp(curve.rectTransform.rect.xMin, curve.rectTransform.rect.xMax, (p_x - minRange) / (maxRange - minRange)), 0f);
			}
			if (outputValueMarker != null && m_current.y != p_y)
			{
				outputValueMarker.anchoredPosition = new Vector2(0f, Mathf.Lerp(curve.rectTransform.rect.yMin, curve.rectTransform.rect.yMax, (p_y - minValue) / (maxValue - minValue)));
			}
			m_current = new Vector2(p_x, p_y);
		}

		public void SetCurrentRaw(float p_x, float p_y, float p_labelX, float p_labelY)
		{
			if (inputValueMarker != null && m_current.x != p_x)
			{
				inputValueMarker.anchoredPosition = new Vector2(p_x * 100f, 0f);
			}
			if (outputValueMarker != null && m_current.y != p_y)
			{
				outputValueMarker.anchoredPosition = new Vector2(0f, p_y * 100f);
			}
			if (inputValueField != null)
			{
				inputValueField.text = FormatNumber(p_labelX, inputFormat);
			}
			if (outputValueField != null)
			{
				outputValueField.text = FormatNumber(p_labelY, outputFormat);
			}
			m_current = new Vector2(p_x, p_y);
		}

		public void UpdateGraphRaw(Vector2[] p_points)
		{
			if (curve != null)
			{
				curve.Points = p_points;
				curve.SetAllDirty();
			}
		}

		public void UpdateGraph(Vector2[] p_points, Vector2 p_clampMin, Vector2 p_clampMax)
		{
			if (curve != null)
			{
				for (int i = 0; i < p_points.Length; i++)
				{
					p_points[i].x = Mathf.Clamp((p_points[i].x - minRange) / (maxRange - minRange), p_clampMin.x, p_clampMax.x);
					p_points[i].y = Mathf.Clamp((p_points[i].y - minValue) / (maxValue - minValue), p_clampMin.y, p_clampMax.y);
				}
				curve.Points = p_points;
				curve.SetAllDirty();
			}
		}

		public void UpdateGraph(Vector2[] p_points)
		{
			if (curve != null)
			{
				for (int i = 0; i < p_points.Length; i++)
				{
					p_points[i].x = (p_points[i].x - minRange) / (maxRange - minRange);
					p_points[i].y = (p_points[i].y - minValue) / (maxValue - minValue);
				}
				curve.Points = p_points;
				curve.SetAllDirty();
			}
		}

		public void SetBounds(float p_minX, float p_maxX, float p_minY, float p_maxY)
		{
			minValue = p_minY;
			maxValue = p_maxY;
			minRange = p_minX;
			maxRange = p_maxX;
		}

		public void UpdateCurveMaximum(float p_max)
		{
			if (maxCurveValueField != null)
			{
				maxCurveValueField.text = FormatNumber(p_max, outputFormat);
			}
			if (maxCurveValueMarker != null)
			{
				maxCurveValueMarker.anchoredPosition = new Vector2(0f, Mathf.Lerp(curve.rectTransform.rect.yMin, curve.rectTransform.rect.yMax, (p_max - minValue) / (maxValue - minValue)));
			}
		}

		public void SetLabels(string p_x, string p_y, float[] p_xValues, float[] p_yValues)
		{
			if (xAxisLabel != null)
			{
				xAxisLabel.text = p_x;
			}
			if (yAxisLabel != null)
			{
				yAxisLabel.text = p_y;
			}
			if (p_xValues != null && xAxisValues != null)
			{
				for (int i = 0; i < p_xValues.Length && i < xAxisValues.Length; i++)
				{
					Text obj = xAxisValues[i];
					float num = p_xValues[i];
					obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(curve.rectTransform.rect.width * (num - minRange) / (maxRange - minRange), 0f);
					obj.text = FormatNumber(num, inputFormat);
				}
			}
			if (p_yValues != null && yAxisValues != null)
			{
				for (int j = 0; j < p_yValues.Length && j < yAxisValues.Length; j++)
				{
					Text obj2 = yAxisValues[j];
					float num2 = p_yValues[j];
					obj2.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, curve.rectTransform.rect.height * (num2 - minValue) / (maxValue - minValue));
					obj2.text = FormatNumber(num2, outputFormat);
				}
			}
		}

		public void SetEndpointLabel(float p_max)
		{
			if (maxCurveValueField != null)
			{
				maxCurveValueField.text = FormatNumber(p_max, outputFormat);
			}
			if (maxCurveValueMarker != null)
			{
				maxCurveValueMarker.anchoredPosition = new Vector2(0f, curve.rectTransform.rect.height * (p_max - minValue) / (maxValue - minValue));
			}
		}

		public void SetCaption(string p_caption)
		{
			if (caption != null)
			{
				caption.text = p_caption;
			}
		}

		public string FormatNumber(float p_value, string p_format)
		{
			if (!p_format.StartsWith("0"))
			{
				return p_value.ToString();
			}
			return p_value.ToString(p_format);
		}

		public string FormatNumber(float p_value, int p_decimals)
		{
			if (p_decimals < 1)
			{
				return ((int)p_value).ToString();
			}
			switch (p_decimals)
			{
			case 1:
				return ((float)(int)(p_value * 10f) * 0.1f).ToString();
			case 2:
				return ((float)(int)(p_value * 100f) * 0.01f).ToString();
			case 3:
				return ((float)(int)(p_value * 1000f) * 0.001f).ToString();
			case 4:
				return ((float)(int)(p_value * 10000f) * 0.0001f).ToString();
			default:
			{
				int num = (int)Mathf.Pow(10f, p_decimals);
				return ((float)(int)(p_value * (float)num) * (1f / (float)num)).ToString();
			}
			}
		}
	}
}

using System;
using UnityEngine;
using UnityEngine.UI;

namespace drl.game
{
	public class UIBatteryMeter : MonoBehaviour
	{
		public Image bar1;

		public Image bar2;

		public Text valueText1;

		public RectTransform outlineRT;

		public RectTransform fillRT1;

		public RectTransform fillRT2;

		private float m_max;

		private float m_min;

		private float m_value1;

		private float m_value2;

		private float m_valueNorm1;

		private float m_valueNorm2;

		private float m_maxBarWidth;

		public float max
		{
			set
			{
				if (!(value <= 0f))
				{
					m_max = value;
				}
			}
		}

		public float min
		{
			set
			{
				if (!(value < 0f))
				{
					m_min = value;
				}
			}
		}

		public float value1
		{
			set
			{
				SetValue1(value);
			}
		}

		public float value2
		{
			set
			{
				SetValue2(value);
			}
		}

		public void Init()
		{
			if (m_max <= 0f)
			{
				m_max = 100f;
			}
			outlineRT = GetComponent<RectTransform>();
			fillRT1 = bar1.GetComponent<RectTransform>();
			fillRT2 = bar2.GetComponent<RectTransform>();
			m_maxBarWidth = outlineRT.sizeDelta.x;
		}

		public void SetValue1(float p_value)
		{
			if (!(p_value < 0f) && !(m_max <= 0f))
			{
				if (m_maxBarWidth <= 0f)
				{
					m_maxBarWidth = outlineRT.sizeDelta.x;
				}
				m_value1 = p_value;
				m_valueNorm1 = (m_value1 - m_min) / (m_max - m_min);
				float x = m_valueNorm1 * m_maxBarWidth;
				fillRT1.sizeDelta = new Vector2(x, bar1.rectTransform.sizeDelta.y);
				valueText1.text = ((float)Math.Round(m_value1, 2)).ToString();
			}
		}

		public void SetValue2(float p_value)
		{
			if (!(p_value < 0f) && !(m_max <= 0f))
			{
				if (m_maxBarWidth <= 0f)
				{
					m_maxBarWidth = outlineRT.sizeDelta.x;
				}
				m_value2 = p_value;
				m_valueNorm2 = (m_value2 - m_min) / (m_max - m_min);
				float x = m_valueNorm2 * m_maxBarWidth;
				fillRT2.sizeDelta = new Vector2(x, bar2.rectTransform.sizeDelta.y);
			}
		}
	}
}

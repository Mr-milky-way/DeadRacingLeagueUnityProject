using System;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLDroneSpecBar : UIElementView
	{
		public RectTransform barContainer;

		public Image progressField;

		public Image previewField;

		public Text previewValueField;

		public Text labelField;

		public Text valueField;

		public bool roundToInt;

		[HideInInspector]
		[SerializeField]
		private bool m_bad_preview;

		public bool negativePreviewIsGood;

		public float min;

		public float max = 1f;

		private bool m_isCustom;

		private float m_preview_value;

		public string unit;

		public Vector2 barSize
		{
			get
			{
				if (!barContainer)
				{
					return Vector2.zero;
				}
				return barContainer.sizeDelta;
			}
		}

		public Vector2 progressSize
		{
			get
			{
				return (progressField.transform as RectTransform).sizeDelta;
			}
			set
			{
				(progressField.transform as RectTransform).sizeDelta = value;
			}
		}

		public Vector2 previewSize
		{
			get
			{
				return (previewField.transform as RectTransform).sizeDelta;
			}
			set
			{
				(previewField.transform as RectTransform).sizeDelta = value;
			}
		}

		public float progress
		{
			get
			{
				float x = progressSize.x;
				float x2 = barSize.x;
				if (!(x2 <= 0f))
				{
					return Mathf.Clamp01(x / x2);
				}
				return 0f;
			}
			set
			{
				Vector2 vector = progressSize;
				vector.x = barSize.x * Mathf.Clamp01(value);
				progressSize = vector;
				Refresh();
			}
		}

		public float preview
		{
			get
			{
				float x = previewSize.x;
				float x2 = barSize.x;
				if (!(x2 <= 0f))
				{
					return Mathf.Clamp01(x / x2);
				}
				return 0f;
			}
			set
			{
				Vector2 vector = previewSize;
				vector.x = barSize.x * Mathf.Clamp01(value);
				previewSize = vector;
				Refresh();
			}
		}

		public bool badPreview
		{
			get
			{
				return m_bad_preview;
			}
			set
			{
				m_bad_preview = value;
				previewField.color = (value ? Color.red : Color.green);
				previewValueField.color = (value ? Color.red : Color.green);
				Refresh();
			}
		}

		public float value
		{
			get
			{
				return Mathf.Lerp(min, max, progress);
			}
			set
			{
				float num = Mathf.Max(0f, max - min);
				float num2 = ((num <= 0f) ? 0f : ((value - min) / num));
				progress = num2;
			}
		}

		public bool isCustom
		{
			get
			{
				return m_isCustom;
			}
			set
			{
				if (m_isCustom != value)
				{
					valueField.color = (value ? Color.yellow : Color.white);
				}
				m_isCustom = value;
			}
		}

		public float previewValue
		{
			get
			{
				return m_preview_value;
			}
			set
			{
				float num = Mathf.Max(0f, max - min);
				m_preview_value = value;
				float num2 = ((num <= 0f) ? 0f : ((Mathf.Abs(m_preview_value) - min) / num));
				preview = num2;
			}
		}

		public void SetValue(float p_value, float p_duration, float p_delay = 0f)
		{
			Tween.Kill(this, "value");
			Tween.Add(this, "value", p_value, p_duration, p_delay, Cubic.Out);
		}

		public void SetPreview(float p_value, float p_duration, float p_delay = 0f)
		{
			Tween.Kill(this, "previewValue");
			Tween.Add(this, "previewValue", p_value, p_duration, p_delay, Cubic.Out);
		}

		public void SetCurrentAndNext(float p_value, float p_next_value, float p_duration, float p_delay = 0f)
		{
			float num = p_next_value - p_value;
			float num2 = ((num >= 0f) ? p_value : (p_value + num));
			badPreview = ((num < 0f) ? (!negativePreviewIsGood) : negativePreviewIsGood);
			float num3 = Mathf.Max(0f, max - min);
			float num4 = ((num3 <= 0f) ? 0f : ((num2 - min) / num3));
			num4 = Mathf.Clamp01(num4);
			float f = num;
			float num5 = ((num3 <= 0f) ? 0f : ((Mathf.Abs(f) - min) / num3));
			num5 = Mathf.Clamp01(num5);
			float num6 = num5 + num4;
			if (num6 > 1f)
			{
				float num7 = num6 - 1f;
				num4 -= num4 * num7 / num6;
				num5 -= num5 * num7 / num6;
				num2 = num3 * num4 + min;
				num = (num5 * num3 + min) * Mathf.Sign(num);
			}
			SetValue(num2, p_duration, p_delay);
			SetPreview(num, p_duration, p_delay);
		}

		protected void Refresh()
		{
			float num = (float)Math.Round(value, 2);
			float num2 = (float)Math.Round(m_preview_value, 2);
			if (roundToInt)
			{
				num2 = Mathf.RoundToInt(m_preview_value);
				num = Mathf.RoundToInt(value);
			}
			if (num2 > 0f)
			{
				num += num2;
			}
			valueField.text = (isCustom ? ("*" + num + unit) : (num + unit));
			string text = ((m_preview_value < 0f) ? "" : "+");
			previewValueField.gameObject.SetActive(Mathf.Abs(num2) >= 0.05f);
			previewValueField.text = "(" + text + num2 + ")";
		}
	}
}

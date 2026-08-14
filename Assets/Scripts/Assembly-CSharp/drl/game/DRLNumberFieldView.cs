using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLNumberFieldView : NotificationView<DRLApp>
	{
		public DRLInputFieldView input;

		public Graphic label;

		private EventComponent m_label_ec;

		[SerializeField]
		private float m_value;

		private float m_drag_value;

		public float minValue = float.NegativeInfinity;

		public float maxValue = float.PositiveInfinity;

		[Range(0f, 1f)]
		public float precision;

		public float snap;

		public string format = "";

		public float rate = 1f;

		public float scroll = 1f;

		public string prefix = "";

		public string suffix = "";

		[Header("Conversion")]
		public float convertOffset;

		public float convertFactor = 1f;

		private bool m_ignore_input;

		private bool m_ignore_range_precision;

		public float value
		{
			get
			{
				if (float.IsNaN(m_value))
				{
					return float.NaN;
				}
				float min = Mathf.Min(minValue, maxValue);
				float max = Mathf.Max(minValue, maxValue);
				return Mathf.Clamp(m_value, min, max);
			}
			set
			{
				float min = Mathf.Min(minValue, maxValue);
				float max = Mathf.Max(minValue, maxValue);
				float num = m_value;
				m_value = Mathf.Clamp(value, min, max);
				Refresh();
				Notify(notification + "@change", num, m_value);
			}
		}

		protected void Awake()
		{
			if ((bool)input && (bool)input.field)
			{
				input.field.onEndEdit.AddListener(OnEndEdit);
				input.field.onValueChanged.AddListener(OnChange);
			}
			if ((bool)label)
			{
				m_label_ec = label.GetComponent<EventComponent>();
				if ((bool)m_label_ec)
				{
					m_label_ec.callback.AddListener(OnLabelEvent);
				}
			}
			Refresh();
		}

		protected void Start()
		{
		}

		public T Get<T>()
		{
			if (typeof(T) == typeof(int))
			{
				return (T)(object)(int)value;
			}
			if (typeof(T) == typeof(uint))
			{
				return (T)(object)(uint)value;
			}
			if (typeof(T) == typeof(float))
			{
				return (T)(object)((precision <= 0f) ? value : (Mathf.Round(value * precision) / precision));
			}
			if (typeof(T) == typeof(double))
			{
				return (T)(object)(double)((precision <= 0f) ? value : (Mathf.Round(value * precision) / precision));
			}
			return default(T);
		}

		public void Invalidate()
		{
			value = float.NaN;
		}

		public void Refresh()
		{
			float num = m_value;
			num = (num - convertOffset) * convertFactor;
			RefreshText(num, p_format: true);
		}

		private void RefreshText(float p_value, bool p_format)
		{
			if (!input)
			{
				return;
			}
			m_ignore_input = true;
			bool flag = !string.IsNullOrEmpty(format);
			float num = p_value;
			string text = "";
			if (!float.IsNaN(num))
			{
				if (p_format && precision > 0f)
				{
					num = Mathf.Round(num * precision) / precision;
				}
				text = (flag ? num.ToString(format) : num.ToString());
			}
			bool flag2 = p_format && !string.IsNullOrEmpty(text);
			input.text = (flag2 ? (prefix + text + suffix) : text);
			m_ignore_input = false;
		}

		private void OnChange(string v)
		{
			if (!base.enabled || m_ignore_input)
			{
				return;
			}
			float result = 0f;
			if (!string.IsNullOrEmpty(v))
			{
				if (!string.IsNullOrEmpty(suffix))
				{
					v = v.Replace(suffix, "");
				}
				if (!string.IsNullOrEmpty(prefix))
				{
					v = v.Replace(prefix, "");
				}
			}
			if (!float.TryParse(v, out result))
			{
				return;
			}
			if (v.Contains("."))
			{
				string[] array = v.Split('.');
				if ((array.Length != 0 && string.IsNullOrEmpty(array[0])) || (array.Length > 1 && string.IsNullOrEmpty(array[1])))
				{
					return;
				}
			}
			float num = value;
			if (convertFactor > 0f)
			{
				result = result / convertFactor + convertOffset;
			}
			m_value = result;
			Notify(notification + "@change", num, value);
		}

		private void OnEndEdit(string v)
		{
			if (!base.enabled || m_ignore_input)
			{
				return;
			}
			float result = 0f;
			if (!float.TryParse(v, out result))
			{
				value = m_value;
				return;
			}
			RefreshText(result, p_format: true);
			if (convertFactor > 0f)
			{
				result = result / convertFactor + convertOffset;
			}
			if (snap > 0f)
			{
				result = Mathf.Round(result / snap) * snap;
			}
			value = result;
			Notify(notification + "@end-edit", value);
		}

		public void OnLabelEvent(UIEvent p_event)
		{
			EventComponent label_ec = m_label_ec;
			if (!label_ec || !base.enabled)
			{
				return;
			}
			switch (p_event.type)
			{
			case UIEventType.DragStart:
				m_drag_value = m_value;
				break;
			case UIEventType.DragUpdate:
			{
				float dragFactor = label_ec.dragFactor;
				float num5 = ((snap > 0f) ? (rate * snap) : rate);
				dragFactor *= num5;
				float num6 = m_drag_value + dragFactor;
				if (snap > 0f)
				{
					num6 = Mathf.Round(num6 / snap) * snap;
				}
				value = num6;
				break;
			}
			case UIEventType.Scroll:
			{
				int num = (int)label_ec.data.scrollDelta.y;
				float num2 = value;
				float num3 = (float)num * scroll;
				if (snap > 0f)
				{
					num3 = ((num < 0) ? snap : (0f - snap));
				}
				float num4 = value - num3;
				if (snap > 0f)
				{
					num4 = Mathf.Round(num4 / snap) * snap;
				}
				value = num4;
				Notify(notification + "@end-edit", num2, value);
				break;
			}
			case UIEventType.DragOver:
			case UIEventType.DragEnd:
			case UIEventType.Drop:
				break;
			}
		}
	}
}

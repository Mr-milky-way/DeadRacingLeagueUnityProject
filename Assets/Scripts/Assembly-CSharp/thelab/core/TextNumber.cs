using UnityEngine;
using UnityEngine.UI;

namespace thelab.core
{
	public class TextNumber<T> : MonoBehaviour
	{
		[SerializeField]
		private Component m_target;

		private T m_value;

		public string text;

		public string format;

		public string prefix;

		public string postfix;

		public Component target
		{
			get
			{
				if ((bool)m_target)
				{
					return m_target;
				}
				m_target = GetComponent<Text>();
				if ((bool)m_target)
				{
					return m_target;
				}
				m_target = GetComponent<TextMesh>();
				return m_target;
			}
			set
			{
				m_target = value;
			}
		}

		public T value
		{
			get
			{
				return m_value;
			}
			set
			{
				if (HasValueChanged(m_value, value))
				{
					m_value = value;
					OnValueChange(m_value);
				}
			}
		}

		public Tween Animate(T p_to, float p_duration, float p_delay, Easing p_easing = null)
		{
			return Tween.Add(this, "value", p_to, p_duration, p_delay, p_easing);
		}

		public Tween Animate(T p_to, float p_duration, Easing p_easing = null)
		{
			return Animate(p_to, p_duration, 0f, p_easing);
		}

		public Tween Animate(T p_to, Easing p_easing = null)
		{
			return Animate(p_to, 0.2f, 0f, p_easing);
		}

		public virtual string GetStringValue()
		{
			return value.ToString();
		}

		protected virtual void OnValueChange(T p_value)
		{
			Refresh();
		}

		protected virtual bool HasValueChanged(T a, T b)
		{
			return true;
		}

		public virtual void Refresh()
		{
			if (base.enabled)
			{
				string text = "";
				text += prefix;
				text += GetStringValue();
				text += postfix;
				SetText(text);
			}
		}

		public virtual void SetText(string v)
		{
			text = v;
			if ((bool)target)
			{
				if (target is Text)
				{
					(target as Text).text = v;
				}
				if (target is TextMesh)
				{
					(target as TextMesh).text = v;
				}
			}
		}
	}
}

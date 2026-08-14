using UnityEngine;
using UnityEngine.UI;

namespace thelab.core
{
	public class FadeResizeComponent : MonoBehaviour
	{
		public enum LayoutFieldType
		{
			None = 0,
			Flexible = 1,
			Min = 2,
			Preferred = 3
		}

		public Component target;

		public LayoutFieldType field;

		public Vector2 from;

		public Vector2 to;

		public bool applyWidth = true;

		public bool applyHeight = true;

		public bool autoHide;

		[HideInInspector]
		[SerializeField]
		private float m_transition;

		public LayoutElement layout => target as LayoutElement;

		public new RectTransform transform => target as RectTransform;

		public Vector2 size
		{
			get
			{
				Vector2 zero = Vector2.zero;
				if (!target)
				{
					return zero;
				}
				if ((bool)layout)
				{
					switch (field)
					{
					case LayoutFieldType.Min:
						zero.x = layout.minWidth;
						zero.y = layout.minHeight;
						break;
					case LayoutFieldType.Flexible:
						zero.x = layout.flexibleWidth;
						zero.y = layout.flexibleHeight;
						break;
					case LayoutFieldType.Preferred:
						zero.x = layout.preferredWidth;
						zero.y = layout.preferredHeight;
						break;
					}
					return zero;
				}
				if ((bool)transform)
				{
					return transform.sizeDelta;
				}
				return zero;
			}
			set
			{
				if (!target)
				{
					return;
				}
				Vector2 sizeDelta = value;
				if ((bool)layout)
				{
					switch (field)
					{
					case LayoutFieldType.Min:
						layout.minWidth = sizeDelta.x;
						layout.minHeight = sizeDelta.y;
						break;
					case LayoutFieldType.Flexible:
						layout.flexibleWidth = sizeDelta.x;
						layout.flexibleHeight = sizeDelta.y;
						break;
					case LayoutFieldType.Preferred:
						layout.preferredWidth = sizeDelta.x;
						layout.preferredHeight = sizeDelta.y;
						break;
					case LayoutFieldType.None:
						break;
					}
				}
				else if ((bool)transform)
				{
					transform.sizeDelta = sizeDelta;
				}
			}
		}

		public float transition
		{
			get
			{
				return Mathf.Clamp(m_transition, -0.1f, 1f);
			}
			set
			{
				m_transition = Mathf.Clamp(value, -0.1f, 1f);
				if (!target)
				{
					return;
				}
				Vector2 a = from;
				Vector2 b = to;
				float t = m_transition;
				if (autoHide)
				{
					bool active = false;
					if (Mathf.Abs(a.x) > 0.01f)
					{
						active = true;
					}
					else if (Mathf.Abs(a.y) > 0.01f)
					{
						active = true;
					}
					else if (Mathf.Abs(b.x) > 0.01f)
					{
						active = true;
					}
					else if (Mathf.Abs(b.y) > 0.01f)
					{
						active = true;
					}
					target.gameObject.SetActive(active);
				}
				Vector2 vector = Vector2.Lerp(a, b, t);
				Vector2 vector2 = size;
				vector.x = (applyWidth ? vector.x : vector2.x);
				vector.y = (applyHeight ? vector.y : vector2.y);
				size = vector;
			}
		}

		public void FadeIn(float p_duration, float p_delay)
		{
			Fade(-1f, 0f, p_duration, p_delay);
		}

		public void FadeIn(float p_duration)
		{
			Fade(1f, p_duration, 0f);
		}

		public void FadeIn()
		{
			Fade(1f, 0.3f, 0f);
		}

		public void FadeOut(float p_duration, float p_delay)
		{
			Fade(-0.1f, p_duration, p_delay);
		}

		public void FadeOut(float p_duration)
		{
			Fade(-0.1f, p_duration, 0f);
		}

		public void FadeOut()
		{
			Fade(-0.1f, 0.3f, 0f);
		}

		public void Fade(float p_from, float p_to, float p_duration, float p_delay)
		{
			transition = p_from;
			Tween.Kill(this, "transition");
			Tween.Add(this, "transition", p_to, p_duration, p_delay, Cubic.Out);
		}

		public void Fade(float p_to, float p_duration, float p_delay)
		{
			Tween.Kill(this, "transition");
			Tween.Add(this, "transition", p_to, p_duration, p_delay, Cubic.Out);
		}

		public void Fade(float p_to, float p_duration)
		{
			Tween.Kill(this, "transition");
			Tween.Add(this, "transition", p_to, p_duration, 0f, Cubic.Out);
		}

		public void Move(Vector2 p_from, Vector2 p_to, float p_duration, float p_delay)
		{
			if ((bool)target)
			{
				size = p_from;
				Tween.Kill(this, "size");
				Tween.Add(this, "size", p_to, p_duration, p_delay, Cubic.Out);
			}
		}

		public void Move(Vector2 p_from, Vector2 p_to, float p_duration)
		{
			Move(p_from, p_to, p_duration, 0f);
		}
	}
}

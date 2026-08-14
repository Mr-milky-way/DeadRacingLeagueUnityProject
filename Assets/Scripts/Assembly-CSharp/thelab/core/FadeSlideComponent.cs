using UnityEngine;

namespace thelab.core
{
	public class FadeSlideComponent : MonoBehaviour
	{
		public RectTransform target;

		public Vector2 from;

		public Vector2 center;

		public Vector2 to;

		public CanvasGroup group;

		public bool autoHide = true;

		[HideInInspector]
		[SerializeField]
		private float m_transition;

		public float transition
		{
			get
			{
				return Mathf.Clamp(m_transition, -1f, 1f);
			}
			set
			{
				m_transition = Mathf.Clamp(value, -1f, 1f);
				if ((bool)target)
				{
					bool active = !autoHide || (m_transition > -1f && m_transition < 1f);
					target.gameObject.SetActive(active);
					Vector2 a = ((m_transition < 0f) ? from : center);
					Vector2 b = ((m_transition < 0f) ? center : to);
					float t = ((m_transition < 0f) ? (1f + m_transition) : m_transition);
					target.anchoredPosition = Vector2.Lerp(a, b, t);
					if (group != null)
					{
						group.alpha = 1f - Mathf.Abs(m_transition);
					}
				}
			}
		}

		public void FadeIn(float p_duration, float p_delay)
		{
			Fade(-1f, 0f, p_duration, p_delay);
		}

		public void FadeIn(float p_duration)
		{
			Fade(-1f, 0f, p_duration, 0f);
		}

		public void FadeIn()
		{
			Fade(-1f, 0f, 0.3f, 0f);
		}

		public void FadeOut(float p_duration, float p_delay)
		{
			Fade(0f, 1f, p_duration, p_delay);
		}

		public void FadeOut(float p_duration)
		{
			Fade(0f, 1f, p_duration, 0f);
		}

		public void FadeOut()
		{
			Fade(0f, 1f, 0.3f, 0f);
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

		public void Move(Vector2 p_from, Vector2 p_to, float p_duration, float p_delay)
		{
			if ((bool)target)
			{
				target.anchoredPosition = p_from;
				Tween.Add(target, "anchoredPosition", p_to, p_duration, p_delay, Cubic.Out);
			}
		}

		public void Move(Vector2 p_from, Vector2 p_to, float p_duration)
		{
			Move(p_from, p_to, p_duration, 0f);
		}

		public void Kill()
		{
			Tween.Kill(this, "transition");
		}
	}
}

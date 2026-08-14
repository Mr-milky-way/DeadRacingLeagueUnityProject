using UnityEngine;
using UnityEngine.EventSystems;
using thelab.core;

namespace drl.game
{
	public class DRLFocusTransition : MonoBehaviour, IFocusHandler, IPointerClickHandler, IEventSystemHandler
	{
		public enum PulseType
		{
			None = 0,
			__Scale_ = 10,
			Scale = 11,
			__Offset_ = 20,
			OffsetX = 21,
			OffsetY = 22
		}

		[SerializeField]
		private FadeComponent m_focus;

		private FadeComponent m_blink;

		private FadeComponent m_pulse;

		private ImageLayout m_pulse_t;

		[SerializeField]
		private Vector2 m_pulse_offset = Vector2.zero;

		private Activity m_pulse_update;

		public AnimationCurve pulseAlpha;

		public AnimationCurve pulseSize;

		public float pulseDuration = 0.5f;

		public float pulseDelay = 1f;

		public PulseType pulseType = PulseType.Scale;

		private float m_pulse_elapsed;

		internal FadeComponent focus
		{
			get
			{
				if (!this)
				{
					return null;
				}
				if ((bool)m_focus)
				{
					return m_focus;
				}
				Transform transform = base.transform.Find("focus");
				if (!transform)
				{
					return null;
				}
				return m_focus = transform.GetComponent<FadeComponent>();
			}
		}

		internal FadeComponent blink
		{
			get
			{
				if ((bool)m_blink)
				{
					return m_blink;
				}
				if (!focus)
				{
					return null;
				}
				int childCount = focus.transform.childCount;
				Transform transform = null;
				if (childCount <= 0)
				{
					return null;
				}
				transform = focus.transform.Find("blink");
				if (!transform)
				{
					transform = focus.transform.GetChild(1);
				}
				if (!transform)
				{
					return null;
				}
				return m_blink = transform.GetComponent<FadeComponent>();
			}
		}

		internal FadeComponent pulse
		{
			get
			{
				if ((bool)m_pulse)
				{
					return m_pulse;
				}
				if (!focus)
				{
					return null;
				}
				int childCount = focus.transform.childCount;
				Transform transform = null;
				if (childCount <= 0)
				{
					return null;
				}
				transform = focus.transform.Find("pulse");
				if (!transform)
				{
					transform = focus.transform.GetChild(0);
				}
				if (!transform)
				{
					return null;
				}
				m_pulse_t = transform.GetComponent<ImageLayout>();
				if ((bool)m_pulse_t)
				{
					switch (pulseType)
					{
					case PulseType.Scale:
						m_pulse_offset = m_pulse_t.scale;
						break;
					case PulseType.OffsetX:
						m_pulse_offset = m_pulse_t.offset;
						break;
					case PulseType.OffsetY:
						m_pulse_offset = m_pulse_t.offset;
						break;
					}
				}
				return m_pulse = transform.GetComponent<FadeComponent>();
			}
		}

		public void Blink()
		{
			if ((bool)blink)
			{
				blink.Fade(1f, 0f, 0f, Cubic.Out);
				blink.Fade(0f, 0.2f, 0f, Cubic.Out);
			}
		}

		public void OnFocus()
		{
			if (m_pulse_update != null)
			{
				m_pulse_update.Stop();
			}
			if ((bool)pulse)
			{
				m_pulse_update = Activity.Run(OnPulseUpdate, 0f, false);
				m_pulse_elapsed = 0f - pulseDelay;
			}
			if ((bool)focus)
			{
				focus.Fade(1f, 0f, 0f, Cubic.Out);
			}
			Blink();
		}

		public void OnUnfocus()
		{
			if (m_pulse_update != null)
			{
				m_pulse_update.Stop();
			}
			if ((bool)focus)
			{
				focus.Fade(0f, 0.1f, 0f, Cubic.Out);
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			Blink();
		}

		private bool OnPulseUpdate()
		{
			if (!this)
			{
				return false;
			}
			if (!pulse)
			{
				return false;
			}
			if (!m_pulse_t)
			{
				return false;
			}
			float pulse_elapsed = m_pulse_elapsed;
			m_pulse_elapsed += Time.unscaledDeltaTime;
			if (m_pulse_elapsed >= 1f + pulseDelay)
			{
				m_pulse_elapsed = 0f;
			}
			float time = Mathf.Clamp01((pulse_elapsed < 0f) ? 0f : ((pulseDuration <= 0f) ? 1f : (pulse_elapsed / pulseDuration)));
			if (pulse_elapsed < 0f)
			{
				time = 1f;
			}
			float alpha = pulseAlpha.Evaluate(time);
			float num = pulseSize.Evaluate(time);
			Vector2 vector = new Vector2(num, num);
			switch (pulseType)
			{
			case PulseType.Scale:
			{
				Vector3 vector2 = m_pulse_t.scale;
				vector2 = m_pulse_offset + vector;
				m_pulse_t.scale = vector2;
				break;
			}
			case PulseType.OffsetX:
			{
				Vector3 vector2 = m_pulse_t.offset;
				vector2.x = m_pulse_offset.x + vector.x;
				m_pulse_t.offset = vector2;
				break;
			}
			case PulseType.OffsetY:
			{
				Vector3 vector2 = m_pulse_t.offset;
				vector2.y = m_pulse_offset.y + vector.x;
				m_pulse_t.offset = vector2;
				break;
			}
			}
			m_pulse.alpha = alpha;
			return true;
		}
	}
}

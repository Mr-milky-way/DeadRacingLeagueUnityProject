using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIFlareProgressItem : MonoBehaviour
	{
		public RawImage backgroundField;

		public RawImage fillField;

		private RectTransform m_fill_rt;

		private FadeComponent m_fill_fade;

		public Image flareField;

		private Rotator m_flare_rotator;

		private RectTransform m_flare_rt;

		private FadeComponent m_flare_fade;

		public TweenComponent[] animation;

		private FadeComponent m_animation_fade;

		private MonoActivity m_blink_timer0;

		private MonoActivity m_blink_timer1;

		public float fillMaxWidth = 56f;

		public RectTransform fillRT
		{
			get
			{
				if (!m_fill_rt)
				{
					if (!fillField)
					{
						return null;
					}
					return m_fill_rt = (RectTransform)fillField.transform;
				}
				return m_fill_rt;
			}
		}

		public FadeComponent fillFade
		{
			get
			{
				if (!m_fill_fade)
				{
					if (!fillField)
					{
						return null;
					}
					return m_fill_fade = fillField.GetComponent<FadeComponent>();
				}
				return m_fill_fade;
			}
		}

		public Rotator flareRotator
		{
			get
			{
				if (!m_flare_rotator)
				{
					if (!flareField)
					{
						return null;
					}
					return m_flare_rotator = flareField.GetComponent<Rotator>();
				}
				return m_flare_rotator;
			}
		}

		public RectTransform flareRT
		{
			get
			{
				if (!m_flare_rt)
				{
					if (!flareField)
					{
						return null;
					}
					return m_flare_rt = (RectTransform)flareField.transform;
				}
				return m_flare_rt;
			}
		}

		public FadeComponent flareFade
		{
			get
			{
				if (!m_flare_fade)
				{
					if (!flareField)
					{
						return null;
					}
					return m_flare_fade = flareField.GetComponent<FadeComponent>();
				}
				return m_flare_fade;
			}
		}

		public FadeComponent animationFade
		{
			get
			{
				if (!m_animation_fade)
				{
					if (animation.Length == 0)
					{
						return null;
					}
					return m_animation_fade = animation[0].GetComponent<FadeComponent>();
				}
				return m_animation_fade;
			}
		}

		public Color backgroundColor
		{
			get
			{
				return backgroundField.color;
			}
			set
			{
				backgroundField.color = value;
			}
		}

		public Color fillColor
		{
			get
			{
				return fillField.color;
			}
			set
			{
				fillField.color = value;
			}
		}

		public Color flareColor
		{
			get
			{
				return flareField.color;
			}
			set
			{
				flareField.color = value;
			}
		}

		public float fillWidth
		{
			get
			{
				if (!fillRT)
				{
					return 0f;
				}
				return fillRT.sizeDelta.x;
			}
			set
			{
				if ((bool)fillRT)
				{
					Vector2 sizeDelta = fillRT.sizeDelta;
					sizeDelta.x = value;
					fillRT.sizeDelta = sizeDelta;
				}
			}
		}

		public float progress
		{
			get
			{
				if (!(fillMaxWidth <= 0f))
				{
					return Mathf.Clamp01(fillWidth / fillMaxWidth);
				}
				return 1f;
			}
			set
			{
				fillWidth = fillMaxWidth * Mathf.Clamp01(value);
			}
		}

		protected void Awake()
		{
			Image image = flareField;
			Material material = Object.Instantiate(image.material);
			material.name = image.material.name + "-copy";
			image.material = material;
		}

		public void Clear()
		{
			if (!(this == null))
			{
				progress = 0f;
				TweenComponent[] array = animation;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].progress = 0f;
				}
				if ((bool)animationFade)
				{
					animationFade.Kill();
					animationFade.alpha = 1f;
				}
				flareFade.alpha = 0f;
				flareRotator.Clear();
				flareFade.Kill();
				Tween.Kill(this);
				array = animation;
				for (int i = 0; i < array.Length; i++)
				{
					Tween.Kill(array[i]);
				}
				Tween.Kill(flareRotator);
				if (m_blink_timer0 != null)
				{
					m_blink_timer0.Stop();
				}
				if (m_blink_timer1 != null)
				{
					m_blink_timer1.Stop();
				}
			}
		}

		public float FadeProgress(float p_progress, float p_delay = 0f, bool p_clear = true)
		{
			if (p_clear)
			{
				Clear();
			}
			float num = p_delay;
			if (p_progress > 0f && animation.Length != 0)
			{
				float duration = animation[0].duration;
				TweenComponent[] array = animation;
				for (int i = 0; i < array.Length; i++)
				{
					Tween.Add(array[i], "progress", 1f, duration, num, Cubic.Out);
				}
				num += duration;
			}
			if ((bool)animationFade)
			{
				animationFade.FadeOut(0.2f, num - 0.1f, Cubic.Out);
			}
			Tween.Add(this, "progress", p_progress, 0.3f, num, Cubic.Out);
			if (p_progress >= 1f)
			{
				Blink(num + 0.1f, 0f, p_clear);
				num += 0.1f;
			}
			else
			{
				flareRotator.Clear();
			}
			return num;
		}

		public void SetProgress(float p_progress)
		{
			Clear();
			progress = p_progress;
			TweenComponent[] array = animation;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].progress = p_progress;
			}
			if ((bool)animationFade)
			{
				animationFade.alpha = ((p_progress < 1f) ? 1f : 0f);
			}
			if (p_progress >= 1f)
			{
				Blink();
			}
			else
			{
				flareRotator.Clear();
			}
		}

		public void Blink(float p_delay = 0f, float p_duration = 0f, bool p_clear = true)
		{
			FadeComponent f = flareFade;
			if (m_blink_timer0 != null)
			{
				m_blink_timer0.Stop();
			}
			if (m_blink_timer1 != null)
			{
				m_blink_timer1.Stop();
			}
			Tween.Kill(flareRotator);
			f.Kill();
			m_blink_timer0 = this.MonoActivityRunOnce(delegate
			{
				if (p_clear)
				{
					f.alpha = 1f;
				}
				f.Fade(0.3f, 0.5f, p_duration, Cubic.Out);
				m_blink_timer1 = this.MonoActivityRunOnce(delegate
				{
					f.Fade(0f, 1f, 0f, Cubic.Out);
				}, 0.7f + p_duration);
				Rotator rotator = flareRotator;
				Vector3 vector = new Vector3(0f, 0f, -90f);
				rotator.speed = vector * 5f;
				Tween.Add(rotator, "speed", vector, 1f, Cubic.Out);
			}, p_delay);
		}
	}
}

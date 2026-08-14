using UnityEngine;

namespace thelab.core
{
	public class TweenComponent : MonoBehaviour
	{
		[SerializeField]
		private RectTransform m_target;

		[SerializeField]
		private FadeComponent m_fade;

		public AnimationCurve xCurve;

		public AnimationCurve yCurve;

		public AnimationCurve zCurve;

		public AnimationCurve scaleCurve;

		public AnimationCurve alphaCurve;

		public bool xCurveEnabled = true;

		public bool yCurveEnabled = true;

		public bool zCurveEnabled = true;

		public bool scaleCurveEnabled = true;

		public bool alphaCurveEnabled = true;

		public Vector3 startPosition;

		public Vector3 endPosition;

		public Vector3 startScale;

		public Vector3 endScale;

		public float startAlpha;

		public float endAlpha;

		public Vector3 position;

		public Vector3 scale;

		public float alpha;

		public bool playing;

		public bool playOnAwake;

		public bool useTimeScale = true;

		private bool m_has_alpha;

		[SerializeField]
		private float m_duration = 1f;

		[SerializeField]
		private float m_time;

		public RectTransform target
		{
			get
			{
				if (!m_target)
				{
					return m_target = base.transform as RectTransform;
				}
				return m_target;
			}
		}

		public FadeComponent fade
		{
			get
			{
				if (!m_fade)
				{
					return m_fade = (m_target ? m_target.GetComponent<FadeComponent>() : null);
				}
				return m_fade;
			}
		}

		public float duration
		{
			get
			{
				return m_duration;
			}
			set
			{
				m_duration = value;
				Evaluate(progress);
			}
		}

		public float time
		{
			get
			{
				return m_time;
			}
			set
			{
				m_time = value;
				Evaluate(progress);
			}
		}

		public float progress
		{
			get
			{
				if (!(duration <= 0f))
				{
					return time / duration;
				}
				return 0f;
			}
			set
			{
				time = value * duration;
			}
		}

		protected void Awake()
		{
			if (playOnAwake)
			{
				Play();
			}
		}

		public void Play(float p_time, float p_duration)
		{
			duration = p_duration;
			time = p_time;
			playing = true;
		}

		public void Play(float p_time)
		{
			Play(p_time, duration);
		}

		public void Play()
		{
			Play(time, duration);
		}

		public void Stop()
		{
			playing = false;
			time = 0f;
		}

		public void Evaluate(float p_ratio)
		{
			float num = ((xCurve == null) ? 0f : xCurve.Evaluate(p_ratio));
			float num2 = ((yCurve == null) ? 0f : yCurve.Evaluate(p_ratio));
			float num3 = ((zCurve == null) ? 0f : zCurve.Evaluate(p_ratio));
			float num4 = ((scaleCurve == null) ? 0f : scaleCurve.Evaluate(p_ratio));
			float num5 = ((alphaCurve == null) ? 0f : alphaCurve.Evaluate(p_ratio));
			position.x = startPosition.x + (endPosition.x - startPosition.x) * num;
			position.y = startPosition.y + (endPosition.y - startPosition.y) * num2;
			position.z = startPosition.z + (endPosition.z - startPosition.z) * num3;
			scale.x = startScale.x + (endScale.x - startScale.x) * num4;
			scale.y = startScale.y + (endScale.y - startScale.y) * num4;
			scale.z = startScale.z + (endScale.z - startScale.z) * num4;
			alpha = startAlpha + (endAlpha - startAlpha) * num5;
			RectTransform rectTransform = target;
			if (!rectTransform)
			{
				return;
			}
			Vector3 anchoredPosition3D = new Vector3
			{
				x = (xCurveEnabled ? position.x : rectTransform.anchoredPosition3D.x),
				y = (yCurveEnabled ? position.y : rectTransform.anchoredPosition3D.y),
				z = (zCurveEnabled ? position.z : rectTransform.anchoredPosition3D.z)
			};
			Vector3 localScale = new Vector3
			{
				x = (scaleCurveEnabled ? scale.x : rectTransform.localScale.x),
				y = (scaleCurveEnabled ? scale.y : rectTransform.localScale.y),
				z = (scaleCurveEnabled ? scale.z : rectTransform.localScale.z)
			};
			rectTransform.anchoredPosition3D = anchoredPosition3D;
			rectTransform.localScale = localScale;
			if (true)
			{
				FadeComponent fadeComponent = fade;
				if ((bool)fadeComponent)
				{
					float num6 = (alphaCurveEnabled ? alpha : fadeComponent.alpha);
					fadeComponent.alpha = num6;
				}
			}
		}

		internal virtual void Refresh()
		{
			if (playing)
			{
				time += (useTimeScale ? Time.deltaTime : Time.unscaledDeltaTime);
			}
		}

		protected void Update()
		{
			Refresh();
		}
	}
}

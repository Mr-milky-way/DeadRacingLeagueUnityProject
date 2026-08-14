using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace thelab.core
{
	public class FadeComponent : MonoBehaviour, IUpdateable
	{
		[SerializeField]
		[HideInInspector]
		private float m_alpha;

		public bool autoDisable;

		public bool allowMouseInput = true;

		public float disableThreshold;

		private Graphic m_graphic;

		private CanvasGroup m_group;

		private VideoPlayer m_video;

		private AudioSource m_video_audio;

		private LineRenderer m_linerenderer;

		private Canvas m_canvas;

		private GraphicRaycaster m_raycaster;

		[SerializeField]
		private bool m_pulse;

		public AnimationCurve wave;

		private float m_elapsed;

		private GameObject m_cache_go;

		private bool m_cached_go;

		private bool is_destroyed;

		private bool m_has_init;

		public float alpha
		{
			get
			{
				return m_alpha;
			}
			set
			{
				_ = m_alpha;
				if ((bool)group)
				{
					group.alpha = (m_alpha = value);
					CanvasGroup canvasGroup = group;
					bool blocksRaycasts = (group.interactable = m_alpha >= disableThreshold && allowMouseInput);
					canvasGroup.blocksRaycasts = blocksRaycasts;
				}
				else if ((bool)graphic)
				{
					Color color = graphic.color;
					color.a = (m_alpha = value);
					graphic.color = color;
					graphic.raycastTarget = m_alpha >= 0f && allowMouseInput;
					graphic.enabled = m_alpha >= 0f;
				}
				if ((bool)video)
				{
					video.targetCameraAlpha = (m_alpha = Mathf.Clamp01(value));
					if ((bool)m_video_audio)
					{
						m_video_audio.volume = m_alpha;
					}
				}
				if ((bool)linerenderer)
				{
					Color color2 = linerenderer.material.color;
					color2.a = (m_alpha = value);
					linerenderer.material.color = color2;
					linerenderer.enabled = m_alpha >= 0f;
				}
				if ((bool)canvas)
				{
					canvas.enabled = m_alpha >= 0f;
				}
				if ((bool)raycaster)
				{
					raycaster.enabled = m_alpha >= 0f;
				}
				if (autoDisable)
				{
					base.gameObject.SetActive(m_alpha >= 0f);
				}
			}
		}

		public Graphic graphic => m_graphic ?? (m_graphic = GetComponent<Graphic>());

		public CanvasGroup group
		{
			get
			{
				if (!this)
				{
					return null;
				}
				return m_group ?? (m_group = GetComponent<CanvasGroup>());
			}
		}

		public VideoPlayer video
		{
			get
			{
				m_video = m_video ?? (m_video = GetComponent<VideoPlayer>());
				if ((bool)m_video)
				{
					m_video_audio = (m_video_audio ? m_video_audio : (m_video_audio = m_video.GetTargetAudioSource(0)));
				}
				return m_video;
			}
		}

		public LineRenderer linerenderer => m_linerenderer ?? (m_linerenderer = GetComponent<LineRenderer>());

		public Canvas canvas => m_canvas ?? (m_canvas = GetComponent<Canvas>());

		public GraphicRaycaster raycaster => m_raycaster;

		public bool pulse
		{
			get
			{
				return m_pulse;
			}
			set
			{
				m_pulse = value;
				if (m_pulse)
				{
					Activity.Add(this);
				}
				else
				{
					Activity.Remove(this);
				}
			}
		}

		private GameObject cache_go
		{
			get
			{
				if (m_cached_go)
				{
					return m_cache_go;
				}
				m_cached_go = true;
				return m_cache_go = base.gameObject;
			}
		}

		protected void Awake()
		{
			Init();
		}

		public void Init()
		{
			if (!m_has_init)
			{
				m_has_init = true;
				m_raycaster = GetComponent<GraphicRaycaster>();
				if ((bool)group)
				{
					m_alpha = group.alpha;
				}
				else if ((bool)graphic)
				{
					m_alpha = graphic.color.a;
				}
				else if ((bool)video)
				{
					m_alpha = video.targetCameraAlpha;
				}
				is_destroyed = false;
				if (m_pulse)
				{
					Activity.Add(this);
				}
			}
		}

		public void Fade(float p_alpha, float p_duration = 0.4f, float p_delay = 0f, Easing p_easing = null)
		{
			pulse = false;
			Tween.Kill(this);
			if (p_duration <= 0f)
			{
				alpha = p_alpha;
			}
			else
			{
				Tween.Add(this, "alpha", p_alpha, p_duration, p_delay, p_easing);
			}
		}

		public void FadeIn(float p_duration = 0.4f, float p_delay = 0f, Easing p_easing = null)
		{
			Fade(1f, p_duration, p_delay, p_easing);
		}

		public void FadeOut(float p_duration = 0.4f, float p_delay = 0f, Easing p_easing = null)
		{
			Fade(-0.1f, p_duration, p_delay, p_easing);
		}

		public void Kill()
		{
			Tween.Kill(this, "alpha");
		}

		public void Pulse()
		{
			pulse = true;
			m_elapsed = 0f;
		}

		public void Stop()
		{
			pulse = false;
		}

		protected void Refresh()
		{
			if (!is_destroyed && cache_go.activeInHierarchy && pulse)
			{
				m_elapsed += Time.deltaTime;
				float num = wave.Evaluate(m_elapsed);
				alpha = num;
			}
		}

		public void OnUpdate()
		{
			Refresh();
		}

		protected void OnDestroy()
		{
			is_destroyed = true;
			Activity.Remove(this);
		}
	}
}

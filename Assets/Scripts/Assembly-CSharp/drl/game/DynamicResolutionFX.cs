using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DynamicResolutionFX : View<DRLApp>
	{
		[SerializeField]
		private protected Camera m_camera;

		public List<Camera> children;

		[SerializeField]
		private Canvas m_canvas;

		[SerializeField]
		private RawImage m_background;

		public float minRatio = 0.66f;

		public float maxRatio = 1f;

		public float minFPS = 30f;

		public float maxFPS = 60f;

		public float ratioBlendDelay = 0.5f;

		public float fps;

		public float ratio;

		public bool auto;

		protected int m_frames;

		protected float m_clock;

		[SerializeField]
		protected float m_current_ratio;

		[SerializeField]
		protected RenderTexture m_buffer;

		[SerializeField]
		protected string m_os;

		[SerializeField]
		protected bool m_is_low;

		[SerializeField]
		protected bool m_init;

		public Camera camera
		{
			get
			{
				if (!m_camera)
				{
					return m_camera = GetComponent<Camera>();
				}
				return m_camera;
			}
		}

		public Canvas canvas => m_canvas;

		public RawImage background => m_background;

		protected void Awake()
		{
		}

		public void Initialize()
		{
			if (!m_init)
			{
				if (!camera)
				{
					base.enabled = false;
					return;
				}
				m_init = true;
				base.enabled = false;
				fps = maxFPS;
				ratio = 1f;
			}
		}

		public void Resize(int p_width, int p_height, bool p_force = false)
		{
			bool flag = false;
			int num = (m_buffer ? m_buffer.width : 0);
			int num2 = (m_buffer ? m_buffer.height : 0);
			if (num != p_width)
			{
				flag = true;
			}
			if (num2 != p_height)
			{
				flag = true;
			}
			if (!flag && !p_force)
			{
				return;
			}
			num = p_width;
			num2 = p_height;
			bool num3 = camera != null && camera.allowHDR;
			RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGBFloat;
			RenderTextureFormat format = (num3 ? renderTextureFormat : RenderTextureFormat.ARGB32);
			if (m_buffer != null)
			{
				RenderTexture.ReleaseTemporary(m_buffer);
				Object.Destroy(m_buffer);
				m_buffer = null;
			}
			m_buffer = RenderTexture.GetTemporary(num, num2, 24, format);
			m_buffer.name = "drone-camera-dynamic-rt";
			if (camera != null)
			{
				camera.targetTexture = m_buffer;
			}
			for (int i = 0; i < children.Count; i++)
			{
				if ((bool)children[i])
				{
					children[i].targetTexture = m_buffer;
				}
			}
			RefreshCanvas();
		}

		public void SetCanvas(Canvas p_canvas)
		{
			m_canvas = p_canvas;
			RefreshCanvas();
		}

		protected void RefreshCanvas()
		{
			if ((bool)m_background && (bool)m_background.material)
			{
				Object.Destroy(m_background.material);
			}
			if ((bool)m_background)
			{
				Object.Destroy(m_background.gameObject);
			}
			if ((bool)m_canvas)
			{
				GameObject gameObject = new GameObject("dr-background");
				gameObject.transform.SetParent(m_canvas.transform);
				gameObject.transform.SetSiblingIndex(1);
				gameObject.transform.localScale = Vector3.one;
				RawImage rawImage = (m_background = gameObject.AddComponent<RawImage>());
				rawImage.texture = m_buffer;
				RectTransform rectTransform = rawImage.rectTransform;
				rectTransform.pivot = new Vector2(0.5f, 0.5f);
				rectTransform.anchorMin = new Vector2(0f, 0f);
				rectTransform.anchorMax = new Vector2(1f, 1f);
				rectTransform.offsetMin = default(Vector2);
				rectTransform.offsetMax = default(Vector2);
				m_background.material = new Material(Shader.Find("UI/Default (No Alpha)"));
				m_background.material.name = "dr-background-material";
			}
		}

		protected void FixedUpdate()
		{
			if (base.enabled && auto)
			{
				m_clock += Time.fixedUnscaledDeltaTime;
				if (m_clock >= 0.25f)
				{
					fps = (float)m_frames / m_clock;
					m_frames = 0;
					m_clock = 0f;
				}
			}
		}

		protected void Update()
		{
			if (base.enabled && auto)
			{
				m_frames++;
				float num = ratioBlendDelay;
				float num2 = maxFPS - minFPS;
				float t = ((num2 <= 0f) ? 1f : ((fps - minFPS) / num2));
				t = Mathf.Max(0.05f, Mathf.Lerp(minRatio, maxRatio, t));
				m_current_ratio = Mathf.Lerp(m_current_ratio, t, Time.unscaledDeltaTime / num);
				if (auto)
				{
					SetRatio(m_current_ratio);
				}
			}
		}

		public void SetDynamicResolution(bool p_flag)
		{
			DynamicResolutionAssert component = GetComponent<DynamicResolutionAssert>();
			if ((bool)m_buffer)
			{
				m_buffer.useDynamicScale = p_flag;
			}
			if ((bool)component)
			{
				component.enabled = p_flag;
			}
			if ((bool)camera)
			{
				camera.allowDynamicResolution = p_flag;
			}
			for (int i = 0; i < children.Count; i++)
			{
				if ((bool)children[i])
				{
					children[i].allowDynamicResolution = p_flag;
				}
			}
			if (!p_flag)
			{
				SetRatio(1f);
			}
		}

		public void SetRatio(float p_ratio)
		{
			float num = (ratio = p_ratio);
			float widthScaleFactor = ScalableBufferManager.widthScaleFactor;
			if (Mathf.Abs(ScalableBufferManager.heightScaleFactor - num) <= 0.01f && Mathf.Abs(widthScaleFactor - num) <= 0.01f)
			{
				return;
			}
			if ((bool)camera)
			{
				camera.allowDynamicResolution = num < 1f;
			}
			for (int i = 0; i < children.Count; i++)
			{
				if ((bool)children[i])
				{
					children[i].allowDynamicResolution = num < 1f;
				}
			}
			ScalableBufferManager.ResizeBuffers(num, num);
			widthScaleFactor = ScalableBufferManager.widthScaleFactor;
			_ = ScalableBufferManager.heightScaleFactor;
		}

		public void SetRatio(float p_ratio, float p_delay)
		{
			Timer.Invoke(this, "SetRatio", p_delay, p_ratio);
		}

		protected void OnDestroy()
		{
			if ((bool)m_buffer)
			{
				if (RenderTexture.active == m_buffer)
				{
					RenderTexture.active = null;
				}
				m_buffer.DiscardContents();
				RenderTexture.ReleaseTemporary(m_buffer);
				m_buffer = null;
			}
		}
	}
}

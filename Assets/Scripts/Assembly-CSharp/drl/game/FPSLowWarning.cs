using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class FPSLowWarning : UIElementView
	{
		public Text fpsField;

		public int lowFPSThreshold = 45;

		public float lowFPSTimeout = 10f;

		public bool underWarning;

		public int fps;

		public float m_frame_elapsed_10s;

		public int m_frames_rendered_10s;

		public float m_frame_elapsed;

		public int m_frames_rendered;

		public FadeComponent fade => AssertLocal<FadeComponent>("fade");

		protected void Awake()
		{
			fps = 0;
			m_frame_elapsed_10s = -20f;
			m_frames_rendered_10s = Time.renderedFrameCount;
			m_frame_elapsed = 0f;
			m_frames_rendered = 0;
			underWarning = false;
			switch (Application.platform)
			{
			case RuntimePlatform.XboxOne:
				base.enabled = false;
				break;
			case RuntimePlatform.PS4:
				base.enabled = false;
				break;
			default:
				Activity.Run(FPSWatch, 0f, false);
				break;
			}
			fade.alpha = -0.1f;
			fade.pulse = false;
		}

		public void Restart()
		{
			fps = 0;
			m_frame_elapsed_10s = 0f;
			m_frames_rendered_10s = Time.renderedFrameCount;
			m_frame_elapsed = 0f;
			m_frames_rendered = 0;
			underWarning = false;
			fade.alpha = -0.1f;
			fade.pulse = false;
		}

		private bool FPSWatch()
		{
			if (!this)
			{
				return false;
			}
			if (!base.enabled)
			{
				return true;
			}
			m_frame_elapsed_10s += Time.deltaTime;
			if (m_frame_elapsed_10s < 0f)
			{
				m_frames_rendered_10s = Time.renderedFrameCount;
			}
			if (m_frame_elapsed_10s >= lowFPSTimeout)
			{
				bool num = (Time.renderedFrameCount - m_frames_rendered_10s) / 10 < lowFPSThreshold;
				if (num && !underWarning)
				{
					underWarning = true;
					fade.FadeIn(0.5f);
					fade.pulse = true;
				}
				if (!num && underWarning)
				{
					underWarning = false;
					fade.FadeOut(0.5f);
					fade.pulse = false;
				}
				m_frame_elapsed_10s = 0f;
				m_frames_rendered_10s = Time.renderedFrameCount;
			}
			m_frame_elapsed += Time.deltaTime;
			if (m_frame_elapsed >= 1f)
			{
				fps = Time.renderedFrameCount - m_frames_rendered;
				m_frames_rendered = Time.renderedFrameCount;
				m_frame_elapsed = 0f;
				if ((bool)fpsField)
				{
					fpsField.text = fps + " fps";
				}
			}
			return true;
		}
	}
}

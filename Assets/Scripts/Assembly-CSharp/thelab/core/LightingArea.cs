using UnityEngine;

namespace thelab.core
{
	public class LightingArea : MonoBehaviour
	{
		public enum Mode
		{
			None = 0,
			Fog = 1,
			AmbientColor = 2,
			FogAmbientColor = 3
		}

		private static int m_stack;

		public Mode mode;

		public float duration = 0.8f;

		public Color ambientColor = Color.gray;

		public Color fogColor = Color.gray;

		[Range(0f, 0.2f)]
		public float fogDensity = 0.005f;

		protected Color m_startAmbientColor;

		protected Color m_startFogColor;

		protected float m_startFogDensity;

		protected Color rsAmbientSkyColor
		{
			get
			{
				return RenderSettings.ambientSkyColor;
			}
			set
			{
				RenderSettings.ambientSkyColor = value;
			}
		}

		protected Color rsFogColor
		{
			get
			{
				return RenderSettings.fogColor;
			}
			set
			{
				RenderSettings.fogColor = value;
			}
		}

		protected float rsFogDensity
		{
			get
			{
				return RenderSettings.fogDensity;
			}
			set
			{
				RenderSettings.fogDensity = value;
			}
		}

		static LightingArea()
		{
			m_stack = 0;
		}

		protected void Awake()
		{
			m_startAmbientColor = RenderSettings.ambientSkyColor;
			m_startFogColor = RenderSettings.fogColor;
			m_startFogDensity = RenderSettings.fogDensity;
		}

		public void Clear(bool p_animate = false)
		{
			m_stack = 0;
			if (p_animate)
			{
				Transition(mode, 0f, duration);
				return;
			}
			RenderSettings.ambientSkyColor = m_startAmbientColor;
			RenderSettings.fogColor = m_startFogColor;
			RenderSettings.fogDensity = m_startFogDensity;
		}

		protected void OnTriggerEnter(Collider p_collider)
		{
			m_stack++;
			Transition(mode, 1f, duration);
		}

		protected void OnTriggerExit(Collider p_collider)
		{
			m_stack--;
			if (m_stack <= 0)
			{
				Transition(mode, 0f, duration);
				m_stack = 0;
			}
		}

		public void Transition(Mode p_mode, float p_transition, float p_duration)
		{
			if ((p_mode & Mode.Fog) != Mode.None)
			{
				Color p_to = Color.Lerp(m_startFogColor, fogColor, p_transition);
				float p_to2 = Mathf.Lerp(m_startFogDensity, fogDensity, p_transition);
				Tween.Add(typeof(RenderSettings), "fogColor", p_to, p_duration, Cubic.Out);
				Tween.Add(typeof(RenderSettings), "fogDensity", p_to2, p_duration, Cubic.Out);
			}
			if ((p_mode & Mode.AmbientColor) != Mode.None)
			{
				Color p_to = Color.Lerp(m_startAmbientColor, ambientColor, p_transition);
				Tween.Add(typeof(RenderSettings), "ambientSkyColor", p_to, p_duration, Cubic.Out);
			}
		}
	}
}

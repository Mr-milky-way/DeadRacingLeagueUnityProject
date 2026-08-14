using UnityEngine;
using thelab.core;

namespace drl
{
	[ExecuteInEditMode]
	[ImageEffectAllowedInSceneView]
	[AddComponentMenu("Image Effects/DRL/RadioFX")]
	public class CameraRadioFX : MonoBehaviour
	{
		[SerializeField]
		private Material m_material;

		protected float m_quality;

		protected float m_offset;

		public Material material
		{
			get
			{
				if ((bool)m_material)
				{
					return m_material;
				}
				m_material = new Material(Shader.Find("DRL/Image Effects/Radio FX"));
				m_material.name = "CameraRadioFXMaterial";
				m_material.hideFlags = HideFlags.HideAndDontSave;
				return m_material;
			}
		}

		public float quality
		{
			get
			{
				return m_quality;
			}
			set
			{
				if (Mathf.Abs(value - m_quality) >= 0.001f)
				{
					m_quality = value;
				}
			}
		}

		public float offset
		{
			get
			{
				return m_offset;
			}
			set
			{
				if (Mathf.Abs(value - m_offset) >= 0.001f)
				{
					m_offset = value;
					Refresh();
				}
			}
		}

		protected void Awake()
		{
			m_quality = 1f;
			m_offset = 0f;
			Refresh();
		}

		public void Shake(float p_intensity, float p_duration)
		{
			offset = 0f - Mathf.Max(0f, p_intensity);
			Tween.Kill(this, "offset");
			Tween.Add(this, "offset", 0f, p_duration, Cubic.Out);
		}

		protected void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			Graphics.Blit(source, destination, material);
		}

		protected void Refresh()
		{
			if (m_material == null)
			{
				Debug.LogError("CameraRadioFX> material not assigned");
				return;
			}
			float t = 1f - Mathf.Clamp01(m_quality + m_offset);
			m_material.SetFloat("_IntensityNoise", Mathf.Lerp(0f, 1.5f, t));
			m_material.SetFloat("_IntensityWaves", Mathf.Lerp(0f, 5f, t));
			m_material.SetFloat("_IntensityStretch", Mathf.Lerp(0f, 20f, t));
			m_material.SetFloat("_IntensityDesaturation", Mathf.Lerp(0f, 1f, t));
			m_material.SetFloat("_IntensityChromatic", Mathf.Lerp(0f, 1f, t));
		}
	}
}

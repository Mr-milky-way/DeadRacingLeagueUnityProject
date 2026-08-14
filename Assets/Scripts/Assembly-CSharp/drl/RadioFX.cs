using UnityEngine;

namespace drl
{
	[ExecuteInEditMode]
	[ImageEffectAllowedInSceneView]
	[AddComponentMenu("Image Effects/DRL/RadioFX")]
	public class RadioFX : MonoBehaviour
	{
		[SerializeField]
		private Material m_material;

		[Range(0f, 1f)]
		public float intensityNoise;

		[Range(0f, 1f)]
		public float intensityWaves;

		[Range(0f, 1f)]
		public float intensityStretch;

		[Range(0f, 1f)]
		public float intensityChromatic;

		[Range(0f, 1f)]
		public float intensityDesaturation;

		[Range(0f, 1f)]
		public float intensity;

		public Material material
		{
			get
			{
				if (m_material != null)
				{
					return m_material;
				}
				m_material = new Material(Shader.Find("DRL/Image Effects/Radio FX"));
				m_material.name = "RadioFXMaterial";
				m_material.hideFlags = HideFlags.HideAndDontSave;
				return m_material;
			}
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			material.SetFloat("_IntensityNoise", intensityNoise);
			material.SetFloat("_IntensityWaves", intensityWaves);
			material.SetFloat("_IntensityStretch", intensityStretch);
			material.SetFloat("_IntensityChromatic", intensityChromatic);
			material.SetFloat("_IntensityDesaturation", intensityDesaturation);
			material.SetFloat("_Intensity", intensity);
			Graphics.Blit(source, destination, material);
		}
	}
}

using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
	[ExecuteInEditMode]
	[AddComponentMenu("Image Effects/Color Adjustments/Grayscale")]
	[ImageEffectAllowedInSceneView]
	public class Grayscale : ImageEffectBase
	{
		public Texture textureRamp;

		[Range(-1f, 1f)]
		public float rampOffset;

		[Range(0f, 1f)]
		public float intensity;

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			base.material.SetTexture("_RampTex", textureRamp);
			base.material.SetFloat("_RampOffset", rampOffset);
			base.material.SetFloat("_Intensity", intensity);
			Graphics.Blit(source, destination, base.material);
		}
	}
}

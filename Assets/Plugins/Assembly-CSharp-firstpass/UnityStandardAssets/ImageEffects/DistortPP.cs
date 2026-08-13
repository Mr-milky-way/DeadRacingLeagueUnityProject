using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Image Effects/Displacement/Distort")]
	[ImageEffectAllowedInSceneView]
	public class DistortPP : PostEffectsBase
	{
		[Range(-100f, 100f)]
		public float intensity = 50f;

		[Range(0f, 1f)]
		public float intensityX = 1f;

		[Range(0f, 1f)]
		public float intensityY = 1f;

		[Range(-1f, 1f)]
		public float centerX;

		[Range(-1f, 1f)]
		public float centerY;

		[Range(0.01f, 5f)]
		public float scale = 1f;

		public Shader distortShader;

		private Material distortMaterial;

		public override bool CheckResources()
		{
			CheckSupport(needDepth: false);
			distortMaterial = CheckShaderAndCreateMaterial(distortShader, distortMaterial);
			if (!isSupported)
			{
				ReportAutoDisable();
			}
			return isSupported;
		}

		protected void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (!CheckResources())
			{
				Graphics.Blit(source, destination);
				return;
			}
			float val = 1.6f * Math.Max(Mathf.Abs(intensity), 1f);
			float num = (float)Math.PI / 180f * Math.Min(160f, val);
			float y = 2f * Mathf.Tan(num * 0.5f);
			Vector4 value = new Vector4(centerX, centerY, Mathf.Max(intensityX, 0.0001f), Mathf.Max(intensityY, 0.0001f));
			Vector4 value2 = new Vector4((intensity >= 0f) ? num : (1f / num), y, 1f / scale, intensity);
			if ((bool)distortMaterial)
			{
				distortMaterial.EnableKeyword("DISTORT_PP");
				distortMaterial.SetVector("DistortionCenterScale", value);
				distortMaterial.SetVector("DistortionAmount", value2);
			}
			Graphics.Blit(source, destination, distortMaterial);
		}
	}
}

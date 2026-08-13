using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Image Effects/Displacement/Distort")]
	[ImageEffectAllowedInSceneView]
	public class Distort : PostEffectsBase
	{
		[Range(0.01f, 4f)]
		public float power = 1f;

		[Range(0.01f, 4f)]
		public float scale = 1f;

		public bool showGrid;

		public bool showBounds;

		public Shader distortShader;

		public Texture2D gridTexture;

		public Texture2D vigneteTexture;

		private Material distortMaterial;

		public override bool CheckResources()
		{
			CheckSupport(needDepth: false);
			distortMaterial = CheckShaderAndCreateMaterial(distortShader, distortMaterial);
			if ((bool)distortMaterial)
			{
				distortMaterial.SetTexture("_GridTex", gridTexture);
				distortMaterial.SetTexture("_VigneteTex", vigneteTexture);
			}
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
			if ((bool)distortMaterial)
			{
				distortMaterial.SetVector("params", new Vector4(power, scale, 0f, 0f));
				distortMaterial.SetVector("debug", new Vector4(showBounds ? 1f : 0f, showGrid ? 1f : 0f, 0f, 0f));
			}
			Graphics.Blit(source, destination, distortMaterial);
		}
	}
}

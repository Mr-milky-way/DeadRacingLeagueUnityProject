using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Image Effects/ByPass")]
	[ImageEffectAllowedInSceneView]
	public class ByPass : PostEffectsBase
	{
		public Camera camera;

		protected void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if ((bool)camera)
			{
				Graphics.Blit(camera.targetTexture, destination);
			}
		}

		protected void OnPostRender()
		{
		}
	}
}

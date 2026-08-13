using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
	[ExecuteInEditMode]
	[ImageEffectAllowedInSceneView]
	public class ImageEffectsTest : MonoBehaviour
	{
		public Material material;

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			Graphics.Blit(source, destination, material);
		}
	}
}

using UnityEngine;

namespace drl
{
	[RequireComponent(typeof(Camera))]
	public class DynamicResolutionAssert : MonoBehaviour
	{
		protected void OnRenderImage(RenderTexture p_src, RenderTexture p_dst)
		{
			Graphics.Blit(p_src, p_dst);
		}
	}
}

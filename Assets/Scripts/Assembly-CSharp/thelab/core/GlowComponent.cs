using UnityEngine;

namespace thelab.core
{
	[ExecuteInEditMode]
	public class GlowComponent : MonoBehaviour
	{
		public Camera camera;

		protected Material m_clear_material;

		protected RenderTexture m_active;

		protected void OnPreRender()
		{
			m_active = RenderTexture.active;
			RenderTexture.active = camera.targetTexture;
			GL.Clear(clearDepth: false, clearColor: true, camera.backgroundColor);
			RenderTexture.active = m_active;
		}

		protected void OnPostRender()
		{
			RenderTexture.active = m_active;
		}
	}
}

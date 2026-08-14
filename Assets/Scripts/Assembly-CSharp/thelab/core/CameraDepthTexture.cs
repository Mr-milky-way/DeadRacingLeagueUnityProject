using UnityEngine;

namespace thelab.core
{
	[RequireComponent(typeof(Camera))]
	[ExecuteInEditMode]
	public class CameraDepthTexture : MonoBehaviour
	{
		public DepthTextureMode mode;

		private Camera m_camera;

		public Camera camera
		{
			get
			{
				if (!m_camera)
				{
					return m_camera = GetComponent<Camera>();
				}
				return m_camera;
			}
		}

		protected void Update()
		{
			if (mode != camera.depthTextureMode)
			{
				camera.depthTextureMode = mode;
			}
		}
	}
}

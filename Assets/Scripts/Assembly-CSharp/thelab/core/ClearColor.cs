using UnityEngine;

namespace thelab.core
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class ClearColor : MonoBehaviour
	{
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

		private void Start()
		{
		}

		protected void OnPreRender()
		{
			RenderTexture.active = camera.targetTexture;
			GL.Clear(clearDepth: false, clearColor: true, camera.backgroundColor);
		}
	}
}

using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	[ExecuteInEditMode]
	public class Composer : MonoBehaviour
	{
		public enum ClearFlag
		{
			DontClear = 0,
			Color = 1,
			Depth = 2,
			ColorDepth = 3
		}

		[RequireComponent(typeof(MeshRenderer))]
		[RequireComponent(typeof(Camera))]
		[ExecuteInEditMode]
		public class Layer : MonoBehaviour
		{
			public Camera capture;

			public Color clearColor;

			private Camera m_camera;

			public RenderTexture target;

			private Vector2 m_screen;

			private RenderTexture m_garbage;

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
				base.gameObject.layer = 2;
				camera.clearFlags = CameraClearFlags.Nothing;
				camera.cullingMask = 0;
			}

			protected void OnWillRenderObject()
			{
				if ((bool)capture)
				{
					camera.depth = capture.depth + 0.01f;
					if (!(Camera.current != camera))
					{
						AssertTexture();
						Graphics.Blit(null, target);
						Debug.Log("rendering");
					}
				}
			}

			protected void AssertTexture()
			{
				camera.depthTextureMode = DepthTextureMode.None;
				if ((bool)m_garbage)
				{
					Object.DestroyImmediate(m_garbage);
					m_garbage = null;
				}
				if ((bool)target)
				{
					Vector2 vector = new Vector2(Screen.width, Screen.height);
					if ((vector - m_screen).magnitude <= 0f)
					{
						return;
					}
					m_screen = vector;
					m_garbage = target;
				}
				int width = Mathf.Max((int)m_screen.x, 1);
				int height = Mathf.Max((int)m_screen.y, 1);
				target = new RenderTexture(width, height, 0);
				target.name = "RenderLayer" + target.GetHashCode().ToString("X6");
				target.hideFlags = HideFlags.HideAndDontSave;
			}
		}

		public bool post = true;

		public List<Layer> layers;

		protected void OnRenderImage(RenderTexture p_from, RenderTexture p_to)
		{
			if (post)
			{
				Graphics.Blit(p_from, p_to);
				Compose(p_to);
			}
			else
			{
				Compose(p_to);
				Graphics.Blit(p_from, p_to);
			}
		}

		protected void Compose(RenderTexture p_target)
		{
			for (int i = 0; i < layers.Count; i++)
			{
			}
		}
	}
}

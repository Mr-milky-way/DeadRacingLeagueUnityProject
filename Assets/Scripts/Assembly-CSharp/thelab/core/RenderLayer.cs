using UnityEngine;

namespace thelab.core
{
	[RequireComponent(typeof(Camera))]
	[ExecuteInEditMode]
	public class RenderLayer : MonoBehaviour
	{
		public enum Type
		{
			Normal = 0,
			Add = 1,
			Alpha = 2,
			Multiply = 3
		}

		public Type type;

		public Color color = Color.white;

		public Color emissive = Color.clear;

		public int blits = 1;

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

		protected void Start()
		{
		}

		protected void OnEnable()
		{
			AssertTexture();
		}

		protected void OnRenderImage(RenderTexture p_from, RenderTexture p_to)
		{
			AssertTexture();
			Graphics.Blit(p_from, p_to);
			Graphics.Blit(p_from, target);
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
}

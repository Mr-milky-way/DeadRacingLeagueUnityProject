using UnityEngine;

namespace thelab.core
{
	[RequireComponent(typeof(Camera))]
	[ExecuteInEditMode]
	public class CameraResolution : MonoBehaviour
	{
		private Camera m_camera;

		[Range(0.001f, 1f)]
		public float quality = 1f;

		public FilterMode filter;

		private float m_last_quality;

		private int m_last_sw;

		private int m_last_sh;

		public RenderTexture rtexture;

		private Camera m_dummy;

		private Rect m_current_vp;

		private Material m_material;

		private Shader m_shader;

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

		protected void OnPreRender()
		{
			if ((bool)camera)
			{
				UpdateRT();
				camera.targetTexture = rtexture;
			}
		}

		protected void OnPostRender()
		{
			if ((bool)rtexture)
			{
				camera.targetTexture = null;
				Graphics.Blit((Texture)rtexture, (RenderTexture)null);
			}
		}

		protected void AssertMaterial()
		{
			if (!m_material)
			{
				m_shader = Shader.Find("thelab/fx/AlphaBlit");
				if ((bool)m_shader)
				{
					m_material = new Material(m_shader);
					m_material.name = "AlphaBlitMaterial";
					m_material.hideFlags = HideFlags.HideAndDontSave;
				}
			}
		}

		protected void UpdateRT()
		{
			bool flag = rtexture != null;
			if (quality >= 1f)
			{
				if ((bool)rtexture)
				{
					Object.DestroyImmediate(rtexture);
					rtexture = null;
				}
				return;
			}
			int width = Screen.width;
			int height = Screen.height;
			if (flag)
			{
				rtexture.filterMode = filter;
			}
			if (!flag || !(Mathf.Abs(m_last_quality - quality) <= 0f) || m_last_sw != width || m_last_sh != height)
			{
				m_last_sw = width;
				m_last_sh = height;
				m_last_quality = quality;
				int num = Mathf.FloorToInt((float)width * quality);
				int num2 = Mathf.FloorToInt((float)height * quality);
				if ((num & 1) != 0)
				{
					num++;
				}
				if ((num2 & 1) != 0)
				{
					num2++;
				}
				if (num <= 1)
				{
					num = 2;
				}
				if (num2 <= 1)
				{
					num2 = 2;
				}
				rtexture = new RenderTexture(num, num2, 16, RenderTextureFormat.ARGB32);
				rtexture.name = "RTQuality-" + GetHashCode().ToString("X6");
				rtexture.hideFlags = HideFlags.HideAndDontSave;
			}
		}
	}
}

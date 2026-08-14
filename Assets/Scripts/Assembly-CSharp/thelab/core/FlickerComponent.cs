using UnityEngine;
using UnityEngine.UI;

namespace thelab.core
{
	public class FlickerComponent : MonoBehaviour
	{
		public int frequence = 30;

		public float min;

		public float max = 1f;

		private float m_switch;

		private int m_frame;

		private Image m_image;

		private CanvasGroup m_group;

		private MeshRenderer m_renderer;

		private string m_renderer_color_attrib;

		protected void Awake()
		{
			m_image = GetComponent<Image>();
			m_group = GetComponent<CanvasGroup>();
			if (!m_image)
			{
				m_renderer = GetComponent<MeshRenderer>();
				Material sharedMaterial = m_renderer.sharedMaterial;
				m_renderer_color_attrib = "";
				if ((bool)sharedMaterial)
				{
					if (sharedMaterial.HasProperty("_TintColor"))
					{
						m_renderer_color_attrib = "_TintColor";
					}
					else if (sharedMaterial.HasProperty("_Color"))
					{
						m_renderer_color_attrib = "_Color";
					}
					else if (sharedMaterial.HasProperty("_Tint"))
					{
						m_renderer_color_attrib = "_Tint";
					}
				}
			}
			m_switch = 0f;
			m_frame = 0;
		}

		protected void OnRenderObject()
		{
			if (Camera.current != Camera.main)
			{
				return;
			}
			m_frame++;
			int frame = m_frame;
			float num = 0f;
			int num2 = Mathf.CeilToInt(60f / (float)frequence);
			if (frame % num2 == 0)
			{
				m_switch = ((m_switch <= 0f) ? 1f : 0f);
			}
			num = Mathf.Lerp(min, max, m_switch);
			if ((bool)m_image)
			{
				Color color = m_image.color;
				color.a = num;
				m_image.color = color;
			}
			if ((bool)m_group)
			{
				m_group.alpha = num;
			}
			if ((bool)m_renderer)
			{
				Material sharedMaterial = m_renderer.sharedMaterial;
				if ((bool)sharedMaterial && !string.IsNullOrEmpty(m_renderer_color_attrib))
				{
					Color color2 = sharedMaterial.GetColor(m_renderer_color_attrib);
					color2.a = num;
					sharedMaterial.SetColor(m_renderer_color_attrib, color2);
				}
			}
		}
	}
}

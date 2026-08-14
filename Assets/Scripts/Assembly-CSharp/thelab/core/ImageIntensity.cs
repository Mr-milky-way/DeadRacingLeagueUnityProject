using UnityEngine;
using UnityEngine.UI;

namespace thelab.core
{
	public class ImageIntensity : MonoBehaviour
	{
		[SerializeField]
		internal Graphic m_target;

		internal Material m_material_unique;

		internal static int m_color_bloom_id = -1;

		internal static int m_bloom_intensity_id = -1;

		public Material material;

		[ColorUsage(true, true)]
		public Color color = Color.white;

		[SerializeField]
		[Range(0f, 100f)]
		private float m_intensity;

		public Graphic target
		{
			get
			{
				if (!m_target)
				{
					return m_target = GetComponent<Graphic>();
				}
				return m_target;
			}
			set
			{
				m_target = value;
				Refresh();
			}
		}

		public float intensity
		{
			get
			{
				return m_intensity;
			}
			set
			{
				if (!(Mathf.Abs(m_intensity - value) <= 0f))
				{
					m_intensity = value;
					Refresh();
				}
			}
		}

		protected void OnEnable()
		{
			if (!m_target)
			{
				target = GetComponent<Graphic>();
			}
			Refresh();
		}

		internal void Refresh()
		{
			if (!m_target)
			{
				return;
			}
			if (!m_material_unique)
			{
				string text = (this.material ? this.material.name : "");
				m_material_unique = (this.material ? Object.Instantiate(this.material) : null);
				if ((bool)m_material_unique)
				{
					m_material_unique.name = text + "-unique";
					if (!Application.isPlaying)
					{
						m_material_unique.hideFlags = HideFlags.HideAndDontSave;
					}
				}
			}
			if (m_color_bloom_id < 0)
			{
				m_color_bloom_id = Shader.PropertyToID("_BloomColor");
			}
			if (m_bloom_intensity_id < 0)
			{
				m_bloom_intensity_id = Shader.PropertyToID("_BloomIntensity");
			}
			Material material = ((m_intensity <= 0f) ? null : m_material_unique);
			m_target.material = material;
			if ((bool)material && material == m_material_unique)
			{
				material.SetColor(m_color_bloom_id, color);
				material.SetFloat(m_bloom_intensity_id, m_intensity);
			}
		}

		internal void Clear()
		{
			if ((bool)m_material_unique)
			{
				Object.Destroy(m_material_unique);
				if ((bool)m_target)
				{
					m_target.material = null;
				}
			}
		}
	}
}

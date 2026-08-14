using UnityEngine;
using UnityEngine.UI;

namespace thelab.core
{
	public class ImageHDR : Image, IUpdateable
	{
		internal static int m_color_bloom_id = -1;

		internal static int m_bloom_intensity_id = -1;

		internal static int m_bloom_texture_id = -1;

		internal static int m_bloom_texture_rect_id = -1;

		internal static int m_bloom_texture_debug_id = -1;

		[ColorUsage(true, true)]
		[SerializeField]
		private Color m_bloom_color = Color.white;

		[SerializeField]
		[Range(0f, 10f)]
		private float m_intensity = 1f;

		[SerializeField]
		private Texture2D m_bloom_texture;

		[SerializeField]
		private Rect m_bloom_texture_rect = new Rect(0f, 0f, 1f, 1f);

		private bool m_is_dirty;

		[SerializeField]
		private bool m_debug_bloom_texture;

		internal bool m_debug_bloom_dirty;

		private Material m_material_unique;

		private Material m_material_original;

		public Color bloomColor
		{
			get
			{
				return m_bloom_color;
			}
			set
			{
				m_bloom_color = value;
				SetDirty();
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
					SetDirty();
				}
			}
		}

		public Texture2D bloomTexture
		{
			get
			{
				return m_bloom_texture;
			}
			set
			{
				m_bloom_texture = value;
				SetDirty();
			}
		}

		public Rect bloomTextureRect
		{
			get
			{
				return m_bloom_texture_rect;
			}
			set
			{
				m_bloom_texture_rect = value;
				SetDirty();
			}
		}

		public bool debugBloomTexture
		{
			get
			{
				return m_debug_bloom_texture;
			}
			set
			{
				m_debug_bloom_dirty = value != m_debug_bloom_texture;
				m_debug_bloom_texture = value;
				SetDirty();
			}
		}

		public override Material materialForRendering
		{
			get
			{
				if (!m_material_unique)
				{
					if (!material)
					{
						goto IL_0033;
					}
				}
				else if (!(material != m_material_original))
				{
					goto IL_0033;
				}
				if ((bool)m_material_unique)
				{
					if (Application.isPlaying)
					{
						Object.Destroy(m_material_unique);
					}
					else
					{
						Object.DestroyImmediate(m_material_unique);
					}
					m_material_unique = null;
				}
				m_material_original = material;
				string text = (material ? material.name : "");
				m_material_unique = (material ? Object.Instantiate(material) : null);
				if ((bool)m_material_unique)
				{
					m_material_unique.name = text;
					if (!Application.isPlaying)
					{
						m_material_unique.hideFlags = HideFlags.HideAndDontSave;
					}
				}
				return m_material_unique;
				IL_0033:
				return m_material_unique;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if (Application.isPlaying)
			{
				Activity.Add(this);
			}
			Refresh();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if (Application.isPlaying)
			{
				Activity.Remove(this);
			}
		}

		public void OnUpdate()
		{
			if (m_is_dirty)
			{
				Refresh();
			}
		}

		internal void SetDirty()
		{
			m_is_dirty = true;
			if (!Application.isPlaying)
			{
				Refresh();
			}
		}

		internal void Refresh()
		{
			m_is_dirty = false;
			Material material = materialForRendering;
			if (m_color_bloom_id < 0)
			{
				m_color_bloom_id = Shader.PropertyToID("_BloomColor");
			}
			if (m_bloom_intensity_id < 0)
			{
				m_bloom_intensity_id = Shader.PropertyToID("_BloomIntensity");
			}
			if (m_bloom_texture_id < 0)
			{
				m_bloom_texture_id = Shader.PropertyToID("_BloomTexture");
			}
			if (m_bloom_texture_rect_id < 0)
			{
				m_bloom_texture_rect_id = Shader.PropertyToID("_BloomTextureRect");
			}
			if (m_bloom_texture_debug_id < 0)
			{
				m_bloom_texture_debug_id = Shader.PropertyToID("_DebugBloomtexture");
			}
			if ((bool)material)
			{
				material.SetColor(m_color_bloom_id, m_bloom_color);
				material.SetFloat(m_bloom_intensity_id, m_intensity);
				material.SetTexture(m_bloom_texture_id, m_bloom_texture ? m_bloom_texture : (base.sprite ? base.sprite.texture : Graphic.s_WhiteTexture));
				material.SetVector(m_bloom_texture_rect_id, new Vector4(m_bloom_texture_rect.x, m_bloom_texture_rect.y, m_bloom_texture_rect.width, m_bloom_texture_rect.height));
				material.SetFloat(m_bloom_texture_debug_id, m_debug_bloom_texture ? 1f : 0f);
				m_debug_bloom_dirty = false;
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if ((bool)m_material_unique)
			{
				if (Application.isPlaying)
				{
					Object.Destroy(m_material_unique);
				}
				else
				{
					Object.DestroyImmediate(m_material_unique);
				}
			}
		}
	}
}

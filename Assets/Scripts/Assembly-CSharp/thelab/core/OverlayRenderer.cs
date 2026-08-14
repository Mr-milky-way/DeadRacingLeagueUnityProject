using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class OverlayRenderer : MonoBehaviour
	{
		public Material material;

		private Material _uniqueMaterial;

		public bool unique = true;

		[HideInInspector]
		[SerializeField]
		private Color m_color;

		[HideInInspector]
		[SerializeField]
		private float m_width;

		[HideInInspector]
		[SerializeField]
		private float m_distance;

		[HideInInspector]
		[SerializeField]
		private Texture m_texture;

		[SerializeField]
		private Renderer[] m_renderers;

		public Renderer[] ignore;

		protected Material m_uniqueMaterial
		{
			get
			{
				if (!unique)
				{
					return material;
				}
				if ((bool)_uniqueMaterial)
				{
					return _uniqueMaterial;
				}
				if ((bool)material)
				{
					string text = material.name + "-" + GetHashCode().ToString("X6");
					_uniqueMaterial = Object.Instantiate(material);
					_uniqueMaterial.name = text;
				}
				return _uniqueMaterial;
			}
		}

		public bool active
		{
			get
			{
				if (renderers.Length == 0)
				{
					return false;
				}
				if (!material)
				{
					return false;
				}
				for (int i = 0; i < renderers.Length; i++)
				{
					Material[] sharedMaterials = renderers[i].sharedMaterials;
					for (int j = 0; j < sharedMaterials.Length; j++)
					{
						if (sharedMaterials[j] == material)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public Color color
		{
			get
			{
				return m_color;
			}
			set
			{
				m_color = value;
				if (value.a <= 0f)
				{
					Disable();
				}
				else
				{
					Enable();
				}
				Apply();
			}
		}

		public float width
		{
			get
			{
				return m_width;
			}
			set
			{
				m_width = value;
				Apply();
			}
		}

		public float distance
		{
			get
			{
				return m_distance;
			}
			set
			{
				m_distance = value;
				Apply();
			}
		}

		public Texture texture
		{
			get
			{
				return m_texture;
			}
			set
			{
				m_texture = value;
				Apply();
			}
		}

		public Renderer[] renderers
		{
			get
			{
				if (Application.isPlaying && m_renderers != null && m_renderers.Length != 0)
				{
					return m_renderers;
				}
				List<Renderer> list = new List<Renderer>();
				SearchRenderers(list, base.transform);
				return m_renderers = list.ToArray();
			}
		}

		protected void Awake()
		{
			m_renderers = null;
		}

		protected void SearchRenderers(List<Renderer> l, Transform t)
		{
			List<Renderer> list = new List<Renderer>(t.GetComponents<Renderer>());
			if (ignore != null)
			{
				for (int i = 0; i < ignore.Length; i++)
				{
					list.Remove(ignore[i]);
				}
			}
			l.AddRange(list.ToArray());
			for (int j = 0; j < t.childCount; j++)
			{
				SearchRenderers(l, t.GetChild(j));
			}
		}

		public void Enable()
		{
			Renderer[] array = renderers;
			if (array.Length == 0 || !material || color.a <= 0f)
			{
				return;
			}
			Material uniqueMaterial = m_uniqueMaterial;
			for (int i = 0; i < array.Length; i++)
			{
				List<Material> list = new List<Material>(array[i].sharedMaterials);
				if (list.IndexOf(uniqueMaterial) < 0)
				{
					for (int j = 0; j < list.Count; j++)
					{
						if (!list[j])
						{
							list[j] = uniqueMaterial;
						}
					}
				}
				if (list.IndexOf(uniqueMaterial) < 0)
				{
					list.Insert(0, uniqueMaterial);
				}
				array[i].sharedMaterials = list.ToArray();
			}
		}

		public void Disable()
		{
			Renderer[] array = renderers;
			if (array.Length != 0 && (bool)material)
			{
				Material uniqueMaterial = m_uniqueMaterial;
				for (int i = 0; i < array.Length; i++)
				{
					List<Material> list = new List<Material>(array[i].sharedMaterials);
					list.Remove(uniqueMaterial);
					array[i].sharedMaterials = list.ToArray();
				}
			}
		}

		public void Fade(float p_transition, float p_duration = 0.4f, float p_delay = 0f, Easing p_easing = null)
		{
			Color p_to = color;
			p_to.a = p_transition;
			Tween.Add(this, "color", p_to, p_duration, p_delay, p_easing);
		}

		protected void Apply()
		{
			Material uniqueMaterial = m_uniqueMaterial;
			if ((bool)uniqueMaterial)
			{
				uniqueMaterial.SetColor("_OutlineColor", m_color);
				uniqueMaterial.SetFloat("_OutlineWidth", m_width);
				uniqueMaterial.SetFloat("_FadeDistance", m_distance);
				uniqueMaterial.SetTexture("_OutlineTexture", m_texture);
			}
		}
	}
}

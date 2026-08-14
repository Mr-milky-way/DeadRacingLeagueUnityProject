using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class MARenderer : MAEntity
	{
		[SerializeField]
		private List<Renderer> m_renderers;

		private static Dictionary<string, Material> m_material_cache;

		protected List<Material> m_renderer_materials;

		[SerializeField]
		private bool m_is_layout;

		private int m_allow_unique_material = -1;

		private int m_vertex_count = -1;

		private List<MARendererMaterial> m_materials;

		[SerializeField]
		[ColorUsage(true, true, 0f, 8f, 0.125f, 3f)]
		internal Color m_emission_color = Color.black;

		[SerializeField]
		internal float m_color_intensity = 1.5f;

		[SerializeField]
		internal Color m_color0 = Color.white;

		[SerializeField]
		internal Color m_color1 = Color.white;

		[SerializeField]
		internal Color m_color2 = Color.white;

		[SerializeField]
		internal bool m_visible = true;

		public bool colorsEnabled = true;

		[SerializeField]
		internal int m_map_style0 = -1;

		[SerializeField]
		internal int m_map_style1 = -1;

		[SerializeField]
		internal int m_map_style2 = -1;

		[SerializeField]
		internal int m_style0;

		[SerializeField]
		internal int m_style1;

		[SerializeField]
		internal int m_style2;

		private int[] m_color_hashes = new int[13];

		private Activity m_refresh_timer;

		public List<Renderer> renderers
		{
			get
			{
				if (m_renderers != null)
				{
					return m_renderers;
				}
				return m_renderers = new List<Renderer>();
			}
			set
			{
				m_renderers = ((value == null) ? new List<Renderer>() : new List<Renderer>(value));
			}
		}

		public bool isVisible
		{
			get
			{
				bool flag = false;
				for (int i = 0; i < renderers.Count; i++)
				{
					flag = flag || (renderers[i].enabled && renderers[i].isVisible);
				}
				return flag;
			}
		}

		public bool isLayout
		{
			get
			{
				return m_is_layout;
			}
			set
			{
				m_is_layout = value;
				Write();
			}
		}

		public bool allowUniqueMaterial
		{
			get
			{
				if (m_allow_unique_material >= 0)
				{
					return m_allow_unique_material == 1;
				}
				bool flag = !base.tags.Contains(MapAssetType.NoUniqueMaterial);
				m_allow_unique_material = (flag ? 1 : 0);
				return flag;
			}
		}

		public int vertexCount
		{
			get
			{
				if (m_vertex_count >= 0)
				{
					return m_vertex_count;
				}
				m_vertex_count = 0;
				for (int i = 0; i < renderers.Count; i++)
				{
					Renderer renderer = renderers[i];
					MeshFilter component = renderer.GetComponent<MeshFilter>();
					if ((bool)component && (!renderer.name.Contains("lod") || renderer.name.Contains("lod0")))
					{
						Mesh sharedMesh = component.sharedMesh;
						m_vertex_count += sharedMesh.vertexCount;
					}
				}
				return m_vertex_count;
			}
		}

		public int triangleCount => vertexCount / 3;

		public List<MARendererMaterial> materials
		{
			get
			{
				RefreshRendererMaterials();
				return m_materials;
			}
		}

		public bool hasMaterials => materials.Count > 0;

		public Color[] palleteEmission => GetMaterialColors("emission");

		public Color[] pallete0 => GetMaterialColors("color0");

		public Color[] pallete1 => GetMaterialColors("color1");

		public Color[] pallete2 => GetMaterialColors("color2");

		public MARendererMaterial styleMapList0 => GetMaterialById("$map-style0");

		public MARendererMaterial styleMapList1 => GetMaterialById("$map-style1");

		public MARendererMaterial styleMapList2 => GetMaterialById("$map-style2");

		public MARendererMaterial styleList0 => GetMaterialById("style0");

		public MARendererMaterial styleList1 => GetMaterialById("style1");

		public MARendererMaterial styleList2 => GetMaterialById("style2");

		public bool hasPalletes
		{
			get
			{
				if (palleteEmission.Length != 0)
				{
					return true;
				}
				if (pallete0.Length != 0)
				{
					return true;
				}
				if (pallete1.Length != 0)
				{
					return true;
				}
				if (pallete2.Length != 0)
				{
					return true;
				}
				return false;
			}
		}

		public bool hasStyles
		{
			get
			{
				if (styleList0 != null)
				{
					return true;
				}
				if (styleList1 != null)
				{
					return true;
				}
				if (styleList2 != null)
				{
					return true;
				}
				return false;
			}
		}

		public bool hasMapStyles
		{
			get
			{
				if (styleMapList0 != null)
				{
					return true;
				}
				if (styleMapList1 != null)
				{
					return true;
				}
				if (styleMapList2 != null)
				{
					return true;
				}
				return false;
			}
		}

		public Color emissionColor
		{
			get
			{
				return m_emission_color;
			}
			set
			{
				m_emission_color = value;
				Write();
				DelayRefresh();
			}
		}

		public float colorIntensity
		{
			get
			{
				return m_color_intensity;
			}
			set
			{
				m_color_intensity = value;
				Write();
				DelayRefresh();
			}
		}

		public Color color0
		{
			get
			{
				return m_color0;
			}
			set
			{
				m_color0 = value;
				Write();
				DelayRefresh();
			}
		}

		public Color color1
		{
			get
			{
				return m_color1;
			}
			set
			{
				m_color1 = value;
				Write();
				DelayRefresh();
			}
		}

		public Color color2
		{
			get
			{
				return m_color2;
			}
			set
			{
				m_color2 = value;
				Write();
				DelayRefresh();
			}
		}

		public bool visible
		{
			get
			{
				return m_visible;
			}
			set
			{
				m_visible = value;
				Write();
			}
		}

		public int mapStyle0
		{
			get
			{
				return m_map_style0;
			}
			set
			{
				m_map_style0 = value;
				Write();
				DelayRefresh();
			}
		}

		public int mapStyle1
		{
			get
			{
				return m_map_style1;
			}
			set
			{
				m_map_style1 = value;
				Write();
				DelayRefresh();
			}
		}

		public int mapStyle2
		{
			get
			{
				return m_map_style2;
			}
			set
			{
				m_map_style2 = value;
				Write();
				DelayRefresh();
			}
		}

		public int style0
		{
			get
			{
				return m_style0;
			}
			set
			{
				m_style0 = value;
				Write();
				DelayRefresh();
			}
		}

		public int style1
		{
			get
			{
				return m_style1;
			}
			set
			{
				m_style1 = value;
				Write();
				DelayRefresh();
			}
		}

		public int style2
		{
			get
			{
				return m_style2;
			}
			set
			{
				m_style2 = value;
				Write();
				DelayRefresh();
			}
		}

		public new MDRenderer data
		{
			get
			{
				return base.data as MDRenderer;
			}
			set
			{
				base.data = value;
			}
		}

		public static void ClearCache()
		{
			if (m_material_cache == null)
			{
				return;
			}
			foreach (Material value in m_material_cache.Values)
			{
				if ((bool)value)
				{
					Object.Destroy(value);
				}
			}
			m_material_cache.Clear();
		}

		public static List<Material> GetCachedMaterials()
		{
			if (m_material_cache != null)
			{
				return new List<Material>(m_material_cache.Values);
			}
			return new List<Material>();
		}

		public List<Material> GetMaterials()
		{
			if (m_renderer_materials == null)
			{
				m_renderer_materials = new List<Material>();
			}
			return new List<Material>(m_renderer_materials);
		}

		public Mesh GetMeshSimple()
		{
			List<Mesh> meshByQuery = GetMeshByQuery("lod2");
			if (meshByQuery.Count <= 0)
			{
				meshByQuery = GetMeshByQuery("lod1");
			}
			meshByQuery.Sort((Mesh a, Mesh b) => (a.vertexCount <= b.vertexCount) ? 1 : (-1));
			if (meshByQuery.Count > 0)
			{
				return meshByQuery[0];
			}
			return null;
		}

		public List<Mesh> GetMeshes()
		{
			List<Mesh> list = new List<Mesh>();
			for (int i = 0; i < renderers.Count; i++)
			{
				MeshFilter component = renderers[i].GetComponent<MeshFilter>();
				if ((bool)component)
				{
					Mesh sharedMesh = component.sharedMesh;
					list.Add(sharedMesh);
				}
			}
			return list;
		}

		public List<Mesh> GetMeshByQuery(string p_query)
		{
			List<Mesh> list = new List<Mesh>();
			for (int i = 0; i < renderers.Count; i++)
			{
				Renderer renderer = renderers[i];
				MeshFilter component = renderer.GetComponent<MeshFilter>();
				if ((bool)component && renderer.name.Contains(p_query))
				{
					Mesh sharedMesh = component.sharedMesh;
					list.Add(sharedMesh);
				}
			}
			return list;
		}

		internal void RefreshRendererMaterials()
		{
			if (m_materials != null)
			{
				return;
			}
			m_materials = new List<MARendererMaterial>(GetComponents<MARendererMaterial>());
			List<MARendererMaterial> list = m_materials;
			for (int i = 0; i < list.Count; i++)
			{
				MARendererMaterial mARendererMaterial = list[i];
				switch (mARendererMaterial.id)
				{
				case "emission":
					mARendererMaterial.defaultColor = emissionColor;
					break;
				case "color0":
					mARendererMaterial.defaultColor = color0;
					break;
				case "color1":
					mARendererMaterial.defaultColor = color1;
					break;
				case "color2":
					mARendererMaterial.defaultColor = color2;
					break;
				}
			}
		}

		public void ResetRendererMaterials()
		{
			List<MARendererMaterial> list = materials;
			for (int i = 0; i < list.Count; i++)
			{
				MARendererMaterial mARendererMaterial = list[i];
				switch (mARendererMaterial.id)
				{
				case "emission":
					emissionColor = mARendererMaterial.defaultColor;
					break;
				case "color0":
					color0 = mARendererMaterial.defaultColor;
					break;
				case "color1":
					color1 = mARendererMaterial.defaultColor;
					break;
				case "color2":
					color2 = mARendererMaterial.defaultColor;
					break;
				}
			}
		}

		public override void Write()
		{
			base.Write();
			MDRenderer mDRenderer = data;
			if (mDRenderer != null)
			{
				mDRenderer.emissionColor = emissionColor;
				mDRenderer.colorIntensity = colorIntensity;
				mDRenderer.color0 = color0;
				mDRenderer.color1 = color1;
				mDRenderer.color2 = color2;
				mDRenderer.mapStyle0 = mapStyle0;
				mDRenderer.mapStyle1 = mapStyle1;
				mDRenderer.mapStyle2 = mapStyle2;
				mDRenderer.style0 = style0;
				mDRenderer.style1 = style1;
				mDRenderer.style2 = style2;
				mDRenderer.visible = visible;
				mDRenderer.isLayout = m_is_layout;
			}
		}

		public override void Read()
		{
			base.Read();
			if (m_data is MDRenderer mDRenderer)
			{
				RefreshRendererMaterials();
				m_emission_color = mDRenderer.emissionColor;
				m_color_intensity = mDRenderer.colorIntensity;
				m_color0 = mDRenderer.color0;
				m_color1 = mDRenderer.color1;
				m_color2 = mDRenderer.color2;
				m_map_style0 = mDRenderer.mapStyle0;
				m_map_style1 = mDRenderer.mapStyle1;
				m_map_style2 = mDRenderer.mapStyle2;
				m_style0 = mDRenderer.style0;
				m_style1 = mDRenderer.style1;
				m_style2 = mDRenderer.style2;
				m_visible = mDRenderer.visible;
				m_is_layout = mDRenderer.isLayout;
				Refresh();
			}
		}

		protected override void OnRefresh()
		{
			base.OnRefresh();
			if (m_material_cache == null)
			{
				m_material_cache = new Dictionary<string, Material>();
			}
			_ = m_material_cache;
			MARendererMaterial mARendererMaterial = styleMapList0;
			if ((bool)mARendererMaterial)
			{
				mARendererMaterial.ApplyStyle(mapStyle0);
			}
			mARendererMaterial = styleMapList1;
			if ((bool)mARendererMaterial)
			{
				mARendererMaterial.ApplyStyle(mapStyle1);
			}
			mARendererMaterial = styleMapList2;
			if ((bool)mARendererMaterial)
			{
				mARendererMaterial.ApplyStyle(mapStyle2);
			}
			mARendererMaterial = styleList0;
			if ((bool)mARendererMaterial)
			{
				mARendererMaterial.ApplyStyle(style0);
			}
			mARendererMaterial = styleList1;
			if ((bool)mARendererMaterial)
			{
				mARendererMaterial.ApplyStyle(style1);
			}
			mARendererMaterial = styleList2;
			if ((bool)mARendererMaterial)
			{
				mARendererMaterial.ApplyStyle(style2);
			}
			if (m_renderer_materials == null)
			{
				m_renderer_materials = new List<Material>();
			}
			m_renderer_materials.Clear();
			string text = "";
			int num = 0;
			m_color_hashes[num++] = (int)(emissionColor.r * 13000f);
			m_color_hashes[num++] = (int)(emissionColor.g * 13000f);
			m_color_hashes[num++] = (int)(emissionColor.b * 13000f);
			m_color_hashes[num++] = (int)(color0.r * 255f);
			m_color_hashes[num++] = (int)(color0.g * 255f);
			m_color_hashes[num++] = (int)(color0.b * 255f);
			m_color_hashes[num++] = (int)(color1.r * 255f);
			m_color_hashes[num++] = (int)(color1.g * 255f);
			m_color_hashes[num++] = (int)(color1.b * 255f);
			m_color_hashes[num++] = (int)(color2.r * 255f);
			m_color_hashes[num++] = (int)(color2.g * 255f);
			m_color_hashes[num++] = (int)(color2.b * 255f);
			m_color_hashes[num++] = (int)(colorIntensity * 100f);
			for (int i = 0; i < m_color_hashes.Length; i++)
			{
				switch (i)
				{
				case 0:
				case 1:
				case 2:
					text += ((short)m_color_hashes[i]).ToString("x");
					break;
				case 12:
				{
					int num2 = m_color_hashes[i];
					text += num2;
					break;
				}
				default:
					text += ((byte)m_color_hashes[i]).ToString("x");
					break;
				}
			}
			List<Renderer> list = renderers;
			list.RemoveAll(RemoveNullRenderer);
			for (int j = 0; j < list.Count; j++)
			{
				m_renderer_materials.AddRange(AssertUniqueMaterial(list[j], text));
				ApplyColors(list[j]);
				list[j].enabled = m_visible;
			}
		}

		protected Material[] AssertUniqueMaterial(Renderer p_renderer, string p_uid)
		{
			if (!p_renderer)
			{
				return new Material[0];
			}
			if (m_material_cache == null)
			{
				m_material_cache = new Dictionary<string, Material>();
			}
			Dictionary<string, Material> material_cache = m_material_cache;
			Material[] sharedMaterials = p_renderer.sharedMaterials;
			bool flag = false;
			for (int i = 0; i < sharedMaterials.Length; i++)
			{
				Material material = sharedMaterials[i];
				if (!material)
				{
					continue;
				}
				bool flag2 = material.name.Contains("trigger-grid");
				if (flag2 || allowUniqueMaterial)
				{
					string text = material.name;
					if (text.StartsWith("@"))
					{
						text = text.Replace("@", "");
						text = text.Substring(0, text.IndexOf("$"));
					}
					string text2 = "@" + text + "-" + p_uid;
					if (flag2)
					{
						text2 = text2 + "-fx" + p_renderer.GetInstanceID().ToString("x");
					}
					if (!material_cache.ContainsKey(text2))
					{
						material = Object.Instantiate(material);
						material.name = "@" + text + "$" + p_uid;
						material_cache[text2] = material;
					}
					else
					{
						material = material_cache[text2];
					}
					if (sharedMaterials[i] != material)
					{
						sharedMaterials[i] = material;
						flag = true;
					}
				}
			}
			if (flag)
			{
				p_renderer.sharedMaterials = sharedMaterials;
			}
			return sharedMaterials;
		}

		protected void ApplyColors(Renderer p_renderer)
		{
			if (!p_renderer)
			{
				return;
			}
			Material[] sharedMaterials = p_renderer.sharedMaterials;
			foreach (Material material in sharedMaterials)
			{
				if ((bool)material)
				{
					ApplyColors(material);
				}
			}
		}

		protected virtual void ApplyColors(Material p_material)
		{
			if (!hasMapStyles && colorsEnabled)
			{
				if (p_material.HasProperty("_ColorEmission"))
				{
					p_material.SetColor("_ColorEmission", m_emission_color);
				}
				if (p_material.HasProperty("_EmissionColor"))
				{
					p_material.SetColor("_EmissionColor", m_emission_color);
				}
				if (p_material.HasProperty("_Color1"))
				{
					p_material.SetColor("_Color1", m_color0);
				}
				if (p_material.HasProperty("_Color2"))
				{
					p_material.SetColor("_Color2", m_color1);
				}
				if (p_material.HasProperty("_Color3"))
				{
					p_material.SetColor("_Color3", m_color2);
				}
			}
		}

		public void DelayRefresh()
		{
			if (!Application.isPlaying)
			{
				Refresh();
				return;
			}
			if (m_refresh_timer != null)
			{
				m_refresh_timer.Stop();
			}
			m_refresh_timer = Activity.RunOnce(base.Refresh, 1f / 60f);
		}

		public void SetRenderersEnabled(bool p_flag, bool p_force = false)
		{
			for (int i = 0; i < renderers.Count; i++)
			{
				renderers[i].enabled = p_flag;
			}
			if (p_force)
			{
				Renderer[] array = GetComponents<Renderer>();
				for (int j = 0; j < array.Length; j++)
				{
					array[j].enabled = p_flag;
				}
			}
		}

		protected override MDObject NewData()
		{
			return new MDRenderer();
		}

		protected void OnDestroy()
		{
		}

		private bool RemoveNullRenderer(Renderer r)
		{
			return r == null;
		}

		public void SetRenderersLayer(int p_layer, float p_delay)
		{
			Activity.RunOnce(delegate
			{
				if (renderers != null)
				{
					for (int i = 0; i < renderers.Count; i++)
					{
						renderers[i].gameObject.layer = p_layer;
					}
				}
			}, p_delay);
		}

		public MARendererMaterial GetMaterialById(string p_id)
		{
			return materials.Find((MARendererMaterial it) => it.id == p_id);
		}

		public Color[] GetMaterialColors(string p_id)
		{
			MARendererMaterial materialById = GetMaterialById(p_id);
			if (!materialById)
			{
				return new Color[0];
			}
			return materialById.colors;
		}

		public Color GetMaterialColor(string p_id, int p_index, Color p_default)
		{
			MARendererMaterial materialById = GetMaterialById(p_id);
			if (!materialById)
			{
				return p_default;
			}
			return materialById.GetColor(p_index);
		}

		public Color GetMaterialColor(string p_id, int p_index)
		{
			return GetMaterialColor(p_id, p_index, Color.black);
		}

		public Texture[] GetMaterialTextures(string p_id)
		{
			MARendererMaterial materialById = GetMaterialById(p_id);
			if (!materialById)
			{
				return new Texture[0];
			}
			return materialById.textures;
		}

		public Texture GetMaterialTexture(string p_id, int p_index, Texture p_default)
		{
			MARendererMaterial materialById = GetMaterialById(p_id);
			if (!materialById)
			{
				return p_default;
			}
			return materialById.GetTexture(p_index);
		}

		public Texture GetMaterialTexture(string p_id, int p_index)
		{
			return GetMaterialTexture(p_id, p_index, null);
		}

		public string GetMaterialLabel(string p_id)
		{
			MARendererMaterial materialById = GetMaterialById(p_id);
			if (!materialById)
			{
				return "";
			}
			return materialById.label;
		}
	}
}

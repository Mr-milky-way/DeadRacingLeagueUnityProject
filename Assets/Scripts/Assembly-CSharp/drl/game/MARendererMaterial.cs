using System;
using System.Collections.Generic;
using UnityEngine;

namespace drl.game
{
	public class MARendererMaterial : MonoBehaviour
	{
		[Serializable]
		public class StyleData
		{
			public string label;

			public Material material;
		}

		public string id;

		public string label;

		[ColorUsage(true, true, 0f, 8f, 0.125f, 3f)]
		public Color[] colors;

		public Texture[] textures;

		public StyleData[] styles;

		public Material styleMaterial;

		public Color defaultColor = Color.black;

		public Texture defaultTexture;

		public string[] GetStyleLabels(bool p_uppercase = false)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < styles.Length; i++)
			{
				string text = (string.IsNullOrEmpty(styles[i].label) ? i.ToString("00") : styles[i].label);
				list.Add(p_uppercase ? text.ToUpper() : text);
			}
			return list.ToArray();
		}

		public Color GetColor(int p_index, Color p_default)
		{
			if (p_index < 0)
			{
				return p_default;
			}
			if (p_index >= colors.Length)
			{
				return p_default;
			}
			return colors[p_index];
		}

		public Color GetColor(int p_index)
		{
			return GetColor(p_index, defaultColor);
		}

		public Texture GetTexture(int p_index, Texture p_default)
		{
			if (p_index < 0)
			{
				return p_default;
			}
			if (p_index >= colors.Length)
			{
				return p_default;
			}
			return textures[p_index];
		}

		public Texture GetTexture(int p_index)
		{
			return GetTexture(p_index, defaultTexture);
		}

		public StyleData GetStyle(int p_index)
		{
			if (p_index < 0)
			{
				return null;
			}
			if (p_index >= styles.Length)
			{
				return null;
			}
			return styles[p_index];
		}

		public void ApplyStyle(int p_index, Material p_target)
		{
			StyleData style = GetStyle(p_index);
			if (style == null)
			{
				return;
			}
			Material material = style.material;
			if (!material)
			{
				return;
			}
			List<Material> currentMaterialsToStyle = GetCurrentMaterialsToStyle(p_target);
			for (int i = 0; i < currentMaterialsToStyle.Count; i++)
			{
				Material material2 = currentMaterialsToStyle[i];
				if ((bool)material2)
				{
					material2.CopyPropertiesFromMaterial(material);
				}
			}
		}

		public void ApplyStyle(int p_index)
		{
			ApplyStyle(p_index, styleMaterial);
		}

		protected List<Material> GetCurrentMaterialsToStyle(Material p_target)
		{
			MARenderer component = GetComponent<MARenderer>();
			if (!component)
			{
				return null;
			}
			if (!p_target)
			{
				return null;
			}
			List<Material> materials = component.GetMaterials();
			List<Material> list = new List<Material>();
			for (int i = 0; i < materials.Count; i++)
			{
				if (materials[i].name.Contains(p_target.name))
				{
					list.Add(materials[i]);
				}
			}
			return list;
		}
	}
}

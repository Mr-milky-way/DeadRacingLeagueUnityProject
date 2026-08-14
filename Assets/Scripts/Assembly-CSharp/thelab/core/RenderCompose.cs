using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	[RequireComponent(typeof(Camera))]
	[ExecuteInEditMode]
	public class RenderCompose : MonoBehaviour
	{
		public RenderLayer[] layers;

		public RenderTexture target;

		private Dictionary<string, Material> _materials;

		protected Dictionary<string, Material> m_materials
		{
			get
			{
				if (_materials != null)
				{
					return _materials;
				}
				return _materials = new Dictionary<string, Material>();
			}
		}

		protected void OnRenderImage(RenderTexture p_from, RenderTexture p_to)
		{
			if (layers == null)
			{
				layers = new RenderLayer[0];
			}
			RenderTexture dest = (target ? target : p_to);
			Graphics.Blit(p_from, dest);
			for (int i = 0; i < layers.Length; i++)
			{
				RenderLayer renderLayer = layers[i];
				if ((bool)renderLayer && renderLayer.gameObject.activeInHierarchy && renderLayer.enabled)
				{
					string shaderPath = GetShaderPath(renderLayer.type);
					Material material = AssertMaterial(shaderPath, renderLayer.type.ToString() + "Material");
					material.SetColor("_Color", renderLayer.color);
					material.SetColor("_Emissive", renderLayer.emissive);
					for (int j = 0; j < renderLayer.blits; j++)
					{
						Graphics.Blit(renderLayer.target, dest, material);
					}
				}
			}
		}

		protected string GetShaderPath(RenderLayer.Type p_type)
		{
			return "thelab/fx/layer/" + p_type;
		}

		protected Material AssertMaterial(string p_shader, string p_name = "InternalMaterial")
		{
			if (m_materials.ContainsKey(p_shader))
			{
				return m_materials[p_shader];
			}
			Material material = new Material(Shader.Find(p_shader));
			material.name = p_name;
			material.hideFlags = HideFlags.HideAndDontSave;
			m_materials[p_shader] = material;
			return material;
		}
	}
}

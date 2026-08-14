using System;
using System.Collections.Generic;
using UnityEngine;

namespace drl.levels
{
	public class FloatSkinManager : MonoBehaviour
	{
		[Serializable]
		public class FloatAsset
		{
			public GameObject target;

			public Texture2D skin;
		}

		[SerializeField]
		private FloatSkinLibrary m_library;

		public List<FloatAsset> targets;

		protected Dictionary<string, Material> m_material_cache;

		public FloatSkinLibrary library
		{
			get
			{
				if (!m_library)
				{
					return m_library = GetComponent<FloatSkinLibrary>();
				}
				return m_library;
			}
		}

		protected void Awake()
		{
			Apply();
		}

		public void Apply()
		{
			if (!library)
			{
				Debug.LogWarning("FloatSkinManager> Library is [nulll]");
				return;
			}
			if (m_material_cache == null)
			{
				m_material_cache = new Dictionary<string, Material>();
			}
			for (int i = 0; i < targets.Count; i++)
			{
				FloatAsset floatAsset = targets[i];
				if (floatAsset == null)
				{
					continue;
				}
				GameObject target = floatAsset.target;
				if (!target)
				{
					continue;
				}
				Transform transform = target.transform.Find("lods");
				if (!transform)
				{
					Debug.LogWarning("FloatSkinManager> Target do not contains 'lods'");
					continue;
				}
				for (int j = 0; j < transform.childCount; j++)
				{
					Transform child = transform.GetChild(j);
					if (!child.name.Contains("base"))
					{
						MeshRenderer component = child.GetComponent<MeshRenderer>();
						if ((bool)component)
						{
							Texture2D skin = floatAsset.skin;
							Material fringe = library.GetFringe(floatAsset.skin);
							SetFringeMaterial(component, fringe);
							SetSkinMaterial(component, skin);
						}
					}
				}
			}
		}

		protected void SetSkinMaterial(MeshRenderer p_target, Texture2D p_skin)
		{
			if (!p_skin || !p_target)
			{
				return;
			}
			Material[] sharedMaterials = p_target.sharedMaterials;
			string text = p_skin.name;
			for (int i = 0; i < sharedMaterials.Length; i++)
			{
				Material material = sharedMaterials[i];
				if (!material.name.Contains("fringe"))
				{
					string key = material.name + "_" + text;
					Material material2 = (m_material_cache.ContainsKey(key) ? m_material_cache[key] : UnityEngine.Object.Instantiate(material));
					material2.name = key;
					material2.SetTexture("_SkinTex", p_skin);
					m_material_cache[key] = material2;
					sharedMaterials[i] = material2;
					break;
				}
			}
			p_target.sharedMaterials = sharedMaterials;
		}

		protected void SetFringeMaterial(MeshRenderer p_target, Material p_material)
		{
			Material[] sharedMaterials = p_target.sharedMaterials;
			for (int i = 0; i < sharedMaterials.Length; i++)
			{
				if (sharedMaterials[i].name.Contains("fringe"))
				{
					sharedMaterials[i] = p_material;
					break;
				}
			}
			p_target.sharedMaterials = sharedMaterials;
		}
	}
}

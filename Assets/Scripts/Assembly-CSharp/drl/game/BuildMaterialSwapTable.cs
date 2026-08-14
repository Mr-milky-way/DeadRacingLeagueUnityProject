using System;
using System.Collections.Generic;
using UnityEngine;

namespace drl.game
{
	public class BuildMaterialSwapTable : ScriptableObject
	{
		[Serializable]
		public class Rule
		{
			public Material replacement;

			public List<Material> targets;

			public List<Material> backups;

			public void Apply()
			{
				if (backups.Count > 0)
				{
					Revert();
				}
				CreateBackups();
				for (int i = 0; i < targets.Count; i++)
				{
					Material material = targets[i];
					if ((bool)material)
					{
						Apply(replacement, material);
					}
				}
			}

			public void Revert()
			{
				if (backups.Count != targets.Count)
				{
					return;
				}
				for (int i = 0; i < targets.Count; i++)
				{
					Material material = targets[i];
					if ((bool)material)
					{
						Apply(backups[i], material);
					}
				}
				Clear();
			}

			private static void Apply(Material p_from, Material p_to)
			{
				if ((bool)p_from && (bool)p_to)
				{
					p_to.CopyPropertiesFromMaterial(p_from);
				}
			}

			public void Clear()
			{
				for (int i = 0; i < backups.Count; i++)
				{
					if ((bool)backups[i])
					{
						UnityEngine.Object.DestroyImmediate(backups[i]);
					}
				}
				backups.Clear();
			}

			protected void CreateBackups()
			{
				Clear();
				for (int i = 0; i < targets.Count; i++)
				{
					Material material = targets[i];
					if (!material)
					{
						backups.Add(null);
						continue;
					}
					material = UnityEngine.Object.Instantiate(material);
					material.name = material.name.Replace("(Clone)", "");
					material.name += "$backup";
					material.hideFlags = HideFlags.DontSave;
					backups.Add(material);
				}
			}
		}

		public List<Rule> rules;

		public List<Material> GetMaterials()
		{
			List<Material> list = new List<Material>();
			for (int i = 0; i < rules.Count; i++)
			{
				list.AddRange(rules[i].targets);
			}
			return list;
		}

		public void Apply()
		{
			for (int i = 0; i < rules.Count; i++)
			{
				rules[i].Apply();
			}
		}

		public void Revert()
		{
			for (int i = 0; i < rules.Count; i++)
			{
				rules[i].Revert();
			}
		}
	}
}

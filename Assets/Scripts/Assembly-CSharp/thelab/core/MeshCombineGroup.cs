using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace thelab.core
{
	public class MeshCombineGroup : MonoBehaviour
	{
		public List<Transform> targets;

		public List<MeshCombineComponent> groups;

		public bool enable32bitIndex;

		public bool ignoreDisabled;

		public string filterRule;

		private Dictionary<Object, string> m_hash_lut;

		public uint maxVertexIndex
		{
			get
			{
				if (!SystemInfo.supports32bitsIndexBuffer || !enable32bitIndex)
				{
					return 65535u;
				}
				return uint.MaxValue;
			}
		}

		public void Apply()
		{
			Clear();
			uint num = (uint)((double)maxVertexIndex * 0.8);
			List<MeshRenderer> list = new List<MeshRenderer>();
			for (int i = 0; i < targets.Count; i++)
			{
				Transform transform = targets[i];
				if (!transform)
				{
					continue;
				}
				List<MeshRenderer> list2 = Hierarchy.FindAll<MeshRenderer>(transform);
				for (int j = 0; j < list2.Count; j++)
				{
					if (!list.Contains(list2[j]))
					{
						list.Add(list2[j]);
					}
				}
			}
			Regex filter_rule = new Regex(filterRule);
			list.RemoveAll((MeshRenderer it) => filter_rule.IsMatch(it.name));
			Debug.Log("MeshCombineGroup> Combining [" + list.Count + "] renderers at [" + base.name + "]");
			Dictionary<string, List<MeshRenderer>> dictionary = new Dictionary<string, List<MeshRenderer>>();
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				MeshRenderer meshRenderer = list[num2];
				if ((bool)meshRenderer)
				{
					string materialsHash = GetMaterialsHash(meshRenderer);
					List<MeshRenderer> list3 = (dictionary[materialsHash] = (dictionary.ContainsKey(materialsHash) ? dictionary[materialsHash] : new List<MeshRenderer>()));
					list3.Add(meshRenderer);
				}
			}
			List<string> list5 = new List<string>();
			Dictionary<string, List<MeshRenderer>> dictionary2 = new Dictionary<string, List<MeshRenderer>>();
			foreach (KeyValuePair<string, List<MeshRenderer>> item in dictionary)
			{
				string key = item.Key;
				List<MeshRenderer> value = item.Value;
				if (GetVertexCount(value) < num)
				{
					continue;
				}
				list5.Add(key);
				int num3 = 0;
				int num4 = 0;
				for (int num5 = 0; num5 < value.Count; num5++)
				{
					if (num4 >= num / 2)
					{
						num4 = 0;
						num3++;
					}
					string key2 = key + "_" + num3;
					List<MeshRenderer> list6 = (dictionary2[key2] = (dictionary2.ContainsKey(key2) ? dictionary2[key2] : new List<MeshRenderer>()));
					MeshRenderer meshRenderer2 = value[num5];
					list6.Add(meshRenderer2);
					num4 += GetVertexCount(meshRenderer2);
				}
			}
			for (int num6 = 0; num6 < list5.Count; num6++)
			{
				dictionary.Remove(list5[num6]);
			}
			foreach (KeyValuePair<string, List<MeshRenderer>> item2 in dictionary2)
			{
				dictionary.Add(item2.Key, item2.Value);
			}
			foreach (KeyValuePair<string, List<MeshRenderer>> item3 in dictionary)
			{
				GameObject obj = new GameObject("group_" + item3.Key);
				obj.transform.parent = base.transform;
				MeshCombineComponent meshCombineComponent = obj.AddComponent<MeshCombineComponent>();
				meshCombineComponent.enable32bitIndex = enable32bitIndex;
				meshCombineComponent.ignoreDisabled = ignoreDisabled;
				meshCombineComponent.targets = new List<MeshRenderer>(item3.Value);
				meshCombineComponent.renderer.sharedMaterials = ((meshCombineComponent.targets.Count <= 0) ? new Material[0] : meshCombineComponent.targets[0].sharedMaterials);
				meshCombineComponent.Apply();
				groups.Add(meshCombineComponent);
			}
		}

		public void Clear()
		{
			for (int i = 0; i < groups.Count; i++)
			{
				MeshCombineComponent meshCombineComponent = groups[i];
				if ((bool)meshCombineComponent)
				{
					meshCombineComponent.Clear();
					Object.Destroy(meshCombineComponent.gameObject);
				}
			}
			groups.Clear();
			if (m_hash_lut != null)
			{
				m_hash_lut.Clear();
			}
		}

		public void SetTargetsEnabled(bool p_flag)
		{
			for (int i = 0; i < targets.Count; i++)
			{
				Transform transform = targets[i];
				if (!transform)
				{
					continue;
				}
				List<MeshRenderer> list = Hierarchy.FindAll<MeshRenderer>(transform);
				for (int j = 0; j < list.Count; j++)
				{
					if ((bool)list[i])
					{
						list[i].enabled = p_flag;
					}
				}
			}
		}

		protected int GetVertexCount(List<MeshRenderer> rl)
		{
			int num = 0;
			for (int i = 0; i < rl.Count; i++)
			{
				num += GetVertexCount(rl[i]);
			}
			return num;
		}

		protected int GetVertexCount(MeshRenderer r)
		{
			if (!r)
			{
				return 0;
			}
			MeshFilter component = r.GetComponent<MeshFilter>();
			if (!component)
			{
				return 0;
			}
			Mesh sharedMesh = component.sharedMesh;
			if (!sharedMesh)
			{
				return 0;
			}
			return sharedMesh.vertexCount;
		}

		protected string GetMaterialsHash(MeshRenderer m)
		{
			Material[] sharedMaterials = m.sharedMaterials;
			string text = "";
			for (int i = 0; i < sharedMaterials.Length; i++)
			{
				if ((bool)sharedMaterials[i])
				{
					text += GenerateHash(sharedMaterials[i]);
					if (i < sharedMaterials.Length - 1)
					{
						text += "_";
					}
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
			return GenerateHash(m);
		}

		protected string GenerateHash(Object t)
		{
			if (m_hash_lut == null)
			{
				m_hash_lut = new Dictionary<Object, string>();
			}
			if (!t)
			{
				return "";
			}
			if (m_hash_lut.ContainsKey(t))
			{
				return m_hash_lut[t];
			}
			string text = t.GetInstanceID().ToString("x6");
			m_hash_lut[t] = text;
			return text;
		}

		protected void OnDestroy()
		{
			Clear();
		}
	}
}

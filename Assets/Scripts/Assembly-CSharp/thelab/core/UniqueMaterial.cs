using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	public class UniqueMaterial : MonoBehaviour
	{
		private static Dictionary<string, Material[]> m_groups;

		public bool uniqueName = true;

		public string group;

		public GameObject container;

		public static Dictionary<string, Material[]> groups
		{
			get
			{
				if (m_groups != null)
				{
					return m_groups;
				}
				return m_groups = new Dictionary<string, Material[]>();
			}
		}

		public static Material[] Get(string p_group)
		{
			if (string.IsNullOrEmpty(p_group))
			{
				return null;
			}
			if (groups.ContainsKey(p_group))
			{
				return groups[p_group];
			}
			return null;
		}

		protected virtual void Awake()
		{
			Renderer component = GetComponent<Renderer>();
			if (!component)
			{
				return;
			}
			string text = (container ? container.GetHashCode().ToString("x6") : "");
			string text2 = group + text;
			Material[] array = Get(text2);
			if (array != null)
			{
				component.sharedMaterials = array;
				return;
			}
			array = component.sharedMaterials;
			for (int i = 0; i < array.Length; i++)
			{
				string text3 = array[i].name;
				array[i] = Object.Instantiate(array[i]);
				array[i].name = text3;
				if (uniqueName)
				{
					Material obj = array[i];
					obj.name = obj.name + "-" + array[i].GetHashCode().ToString("x6");
				}
			}
			component.sharedMaterials = array;
			if (!string.IsNullOrEmpty(text2))
			{
				groups[text2] = array;
			}
		}
	}
}

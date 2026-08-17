using System;
using System.Collections.Generic;
using UnityEngine;
using drl.game;
using thelab.core;

namespace drl.sim
{
	public class DRLAsset : UniqueAsset
	{
		[Serializable]
		public class Info
		{
			public string name = "Stock";

			public string caption = "";

			public Texture2D thumb;

			public string brand = "DRL";

			public Texture2D logo;
		}

		public Info info;

		public int order;

		public List<Transform> nodes;

		private DRLLibraryAsset m_filter;

		private bool m_has_init;

		public DRLLibraryAsset filter
		{
			get
			{
				if (!m_filter)
				{
					return m_filter = GetComponent<DRLLibraryAsset>();
				}
				return m_filter;
			}
		}

		public void Initialize()
		{
			if (!m_has_init)
			{
				m_has_init = true;
				RefreshNodes();
				OnInitialize();
				Hierarchy.Traverse(base.transform, (Action<DronePart>)TraversePartsInit, true, false);
			}
		}

		public void RefreshNodes()
		{
			nodes = new List<Transform>();
			Hierarchy.Traverse(base.transform, (Action<Transform>)TraverseNodes, true, false);
			nodes.Sort(SortNodes);
		}

		protected virtual void OnInitialize()
		{
		}

		public string GetPath()
		{
			return Hierarchy.Path(base.transform);
		}

		public virtual string GetPrefix()
		{
			return "G";
		}

		protected override string GetGUID()
		{
			return GetPrefix() + "-" + GUID.Create(1, "", 500, 0, 4095, "x3");
		}

		public Transform FindNode(string p_name)
		{
			Transform res = null;
			Hierarchy.Traverse(base.transform, delegate(Transform it)
			{
				if ((bool)res)
				{
					return false;
				}
				if (it.name == p_name)
				{
					res = it;
				}
				return true;
			});
			return res;
		}

		protected int GetNodeId(Transform p_node)
		{
			string[] array = (p_node ? p_node.name.Split('-') : new string[0]);
			if (array.Length <= 1)
			{
				return -1;
			}
			string s = array[array.Length - 1];
			int result = -1;
			if (int.TryParse(s, out result))
			{
				return result;
			}
			return -1;
		}

		private void TraversePartsInit(DronePart it)
		{
			it.Initialize();
		}

		private static int SortNodes(Transform a, Transform b)
		{
			bool num = a.name.IndexOf("-motor") >= 0;
			bool flag = b.name.IndexOf("-motor") >= 0;
			bool flag2 = a.name.IndexOf("-esc") >= 0;
			bool flag3 = b.name.IndexOf("-esc") >= 0;
			if (num && flag3)
			{
				return 1;
			}
			if (flag && flag2)
			{
				return -1;
			}
			return string.Compare(a.name, b.name);
		}

		private void TraverseNodes(Transform it)
		{
			if (it.name.IndexOf("node-") == 0)
			{
				nodes.Add(it);
			}
		}

		protected T CheckTags<T>(T p_ref) where T : Tag
		{
			if (!this)
			{
				return null;
			}
			T val = p_ref;
			if ((bool)val)
			{
				return val;
			}
			val = (base.gameObject ? GetComponent<T>() : null);
			if ((bool)val)
			{
				return val;
			}
			return base.gameObject ? base.gameObject.AddComponent<T>() : null;
		}
	}
}

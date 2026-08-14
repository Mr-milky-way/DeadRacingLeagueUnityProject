using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MEInspectorPanelView : View<DRLApp>
	{
		private VerticalLayoutGroup m_inspector_layout;

		public List<RectTransform> fields;

		private List<string> m_field_ids;

		[Header("Inspector")]
		public GameObject inspectorContainer;

		public MEInspector current;

		private Activity m_layout_dirty_timer;

		public MEInspectorPanelController controller => AssertLocal<MEInspectorPanelController>("controller");

		public VerticalLayoutGroup inspectorLayout
		{
			get
			{
				if (!m_inspector_layout)
				{
					return m_inspector_layout = GetComponent<VerticalLayoutGroup>();
				}
				return m_inspector_layout;
			}
		}

		public List<string> fieldIds
		{
			get
			{
				if (m_field_ids != null)
				{
					return m_field_ids;
				}
				return m_field_ids = fields.ConvertAll((RectTransform it) => it.name);
			}
		}

		public void SetFieldEnabled(string p_field, bool p_flag)
		{
			for (int i = 0; i < fields.Count; i++)
			{
				if ((bool)fields[i] && (string.IsNullOrEmpty(p_field) || !(fields[i].name != p_field)) && fields[i].gameObject.activeInHierarchy != p_flag)
				{
					fields[i].gameObject.SetActive(p_flag);
				}
			}
		}

		public void SetFieldEnabled(bool p_flag)
		{
			SetFieldEnabled("", p_flag);
		}

		public void ClearFields()
		{
			SetFieldEnabled("", p_flag: false);
		}

		public T GetField<T>(string p_field) where T : Component
		{
			for (int i = 0; i < fields.Count; i++)
			{
				if ((bool)fields[i] && fields[i].name == p_field)
				{
					return fields[i].GetComponent<T>();
				}
			}
			return null;
		}

		public void SetFieldsNotificationEnabled(bool p_flag)
		{
			for (int i = 0; i < fields.Count; i++)
			{
				View component = Hierarchy.GetComponent<View>(fields[i].gameObject);
				if ((bool)component)
				{
					component.enabled = p_flag;
				}
			}
		}

		public void SetTargets(IList p_targets)
		{
			if (p_targets == null)
			{
				p_targets = new List<MapAsset>();
			}
			ClearFields();
			List<MapAsset> list = new List<MapAsset>();
			for (int i = 0; i < p_targets.Count; i++)
			{
				if (p_targets[i] is MapAsset)
				{
					list.Add(p_targets[i] as MapAsset);
				}
			}
			list.Sort(SortByClass);
			MapAsset targetAsset = GetTargetAsset(list);
			list.Sort(SortById);
			if ((bool)current)
			{
				current.OnInspectorDisable();
				UnityEngine.Object.Destroy(current);
				current = null;
			}
			current = GetInspector(targetAsset);
			if ((bool)current)
			{
				current.panel = this;
				current.targets = list;
				current.OnInspectorCreate();
				current.OnInspectorEnable();
			}
			SetLayoutDirty();
		}

		public MEInspector GetInspector(MapAsset p_asset, GameObject p_container)
		{
			if (p_asset is MAPodium)
			{
				return p_container.AddComponent<MEPropertyInspector>();
			}
			if (p_asset is MAGate)
			{
				return p_container.AddComponent<MEPropertyInspector>();
			}
			if (p_asset is MARenderer)
			{
				return p_container.AddComponent<MEPropertyInspector>();
			}
			_ = p_asset is MAEntity;
			return p_container.AddComponent<MEPropertyInspector>();
		}

		public MEInspector GetInspector(MapAsset p_asset)
		{
			return GetInspector(p_asset, inspectorContainer);
		}

		protected MapAsset GetTargetAsset(MapAsset p_current, MapAsset p_next)
		{
			if (p_current is MAPodium && p_next is MAPodium)
			{
				return p_current;
			}
			if (p_current is MAPodium && p_next is MAGate)
			{
				return p_current;
			}
			if (p_current is MAPodium && p_next is MARenderer)
			{
				return p_next;
			}
			if (p_current is MAPodium && p_next is MAEntity)
			{
				return p_next;
			}
			if (p_current is MAGate && p_next is MAGate)
			{
				return p_current;
			}
			if (p_current is MAGate && p_next is MAPodium)
			{
				return p_current;
			}
			if (p_current is MAGate && p_next is MARenderer)
			{
				return p_next;
			}
			if (p_current is MAGate && p_next is MAEntity)
			{
				return p_next;
			}
			if (p_current is MARenderer && p_next is MARenderer)
			{
				return p_current;
			}
			if (p_current is MARenderer && p_next is MAEntity)
			{
				return p_next;
			}
			return p_current;
		}

		protected MapAsset GetTargetAsset(List<MapAsset> p_list)
		{
			MapAsset mapAsset = ((p_list.Count <= 0) ? null : p_list[0]);
			if (mapAsset == null)
			{
				return null;
			}
			for (int i = 1; i < p_list.Count; i++)
			{
				mapAsset = GetTargetAsset(mapAsset, p_list[i]);
			}
			return mapAsset;
		}

		protected int SortById(MapAsset a, MapAsset b)
		{
			return string.Compare(a.id, b.id);
		}

		protected int SortByClass(MapAsset a, MapAsset b)
		{
			int classOrder = GetClassOrder(a);
			int classOrder2 = GetClassOrder(b);
			if (classOrder >= classOrder2)
			{
				return 1;
			}
			return -1;
		}

		protected bool IsEqualClass(List<MapAsset> p_list)
		{
			if (p_list == null)
			{
				return true;
			}
			for (int i = 0; i < p_list.Count; i++)
			{
				for (int j = i; j < p_list.Count; j++)
				{
					Type type = p_list[i].GetType();
					Type type2 = p_list[j].GetType();
					if (type != type2)
					{
						return false;
					}
				}
			}
			return true;
		}

		protected int GetClassOrder(MapAsset p_asset)
		{
			if (!p_asset)
			{
				return 1000;
			}
			int num = 0;
			if (p_asset is MAPodium)
			{
				return num;
			}
			num++;
			if (p_asset is MAGate)
			{
				return num;
			}
			num++;
			if (p_asset is MARenderer)
			{
				return num;
			}
			return num + 1;
		}

		protected void SetLayoutDirty()
		{
			if ((bool)inspectorLayout)
			{
				inspectorLayout.enabled = true;
				if (m_layout_dirty_timer != null)
				{
					m_layout_dirty_timer.Stop();
				}
				m_layout_dirty_timer = Activity.RunOnce(delegate
				{
					m_layout_dirty_timer = null;
					inspectorLayout.enabled = false;
				}, 2f);
			}
		}
	}
}

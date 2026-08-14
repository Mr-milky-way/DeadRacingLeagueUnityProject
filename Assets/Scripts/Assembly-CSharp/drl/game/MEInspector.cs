using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class MEInspector : MonoBehaviour
	{
		public MEInspectorPanelView panel;

		[SerializeField]
		private List<MapAsset> m_targets;

		public List<MapAssetType> tags;

		public bool dirty;

		public MapEditorView editor => panel.controller.editor.view;

		public List<MapAsset> targets
		{
			get
			{
				if (m_targets == null)
				{
					m_targets = new List<MapAsset>();
				}
				m_targets.RemoveAll((MapAsset it) => it == null);
				return m_targets;
			}
			set
			{
				m_targets = value;
				tags = new List<MapAssetType>();
				if (m_targets == null)
				{
					return;
				}
				for (int i = 0; i < m_targets.Count; i++)
				{
					List<MapAssetType> list = m_targets[i].tags;
					for (int j = 0; j < list.Count; j++)
					{
						if (!tags.Contains(list[j]))
						{
							tags.Add(list[j]);
						}
					}
				}
			}
		}

		public MapAsset target
		{
			get
			{
				if (targets != null)
				{
					if (targets.Count <= 0)
					{
						return null;
					}
					return targets[0];
				}
				return null;
			}
		}

		public T Find<T>() where T : Component
		{
			List<MapAsset> list = targets;
			T val = null;
			for (int i = 0; i < list.Count; i++)
			{
				MapAsset mapAsset = list[i];
				if ((bool)mapAsset)
				{
					val = mapAsset.GetComponent<T>();
					if ((bool)val)
					{
						return val;
					}
				}
			}
			return val;
		}

		public List<T> FindAll<T>() where T : Component
		{
			List<MapAsset> list = targets;
			List<T> list2 = new List<T>();
			for (int i = 0; i < list.Count; i++)
			{
				MapAsset mapAsset = list[i];
				if ((bool)mapAsset)
				{
					T component = mapAsset.GetComponent<T>();
					if ((bool)component)
					{
						list2.Add(component);
					}
				}
			}
			return list2;
		}

		public virtual void OnInspectorCreate()
		{
		}

		public virtual void OnInspectorEnable()
		{
			panel.SetFieldEnabled("", p_flag: true);
		}

		public virtual void OnInspectorDisable()
		{
		}

		public virtual void OnNotification(string p_notification, Object p_target, params object[] p_data)
		{
		}

		public bool ContainsTarget<T>()
		{
			for (int i = 0; i < targets.Count; i++)
			{
				if (targets[i] is T)
				{
					return true;
				}
			}
			return false;
		}

		public T GetTarget<T>(int p_index) where T : Component
		{
			if (p_index >= 0 && p_index < targets.Count)
			{
				MapAsset mapAsset = targets[p_index];
				if (!mapAsset)
				{
					return null;
				}
				return Hierarchy.GetComponent<T>(mapAsset.gameObject);
			}
			return null;
		}

		public bool IsMultiTargetSameGUID()
		{
			for (int i = 0; i < targets.Count; i++)
			{
				for (int j = i + 1; j < targets.Count; j++)
				{
					MapAsset mapAsset = targets[i];
					MapAsset mapAsset2 = targets[j];
					if (mapAsset.guid != mapAsset2.guid)
					{
						return false;
					}
				}
			}
			return true;
		}

		public bool IsMultiTargetSameType()
		{
			for (int i = 0; i < targets.Count; i++)
			{
				for (int j = i + 1; j < targets.Count; j++)
				{
					MapAsset mapAsset = targets[i];
					MapAsset mapAsset2 = targets[j];
					if (mapAsset.GetType().Name != mapAsset2.GetType().Name)
					{
						return false;
					}
				}
			}
			return true;
		}

		protected void BeginChange(string p_event)
		{
			panel.Notify("map-editor.inspector.begin-change", this, p_event);
		}

		protected void BeginChange()
		{
			BeginChange("");
		}

		protected void EndChange(string p_event)
		{
			panel.Notify("map-editor.inspector.end-change", this, p_event);
			SetDirty();
		}

		protected void EndChange()
		{
			EndChange("");
		}

		public void SetDirty()
		{
			if (!dirty)
			{
				dirty = true;
				panel.Notify("map-editor.inspector.dirty", this);
				panel.RunOnce(delegate
				{
					dirty = false;
				}, 1f / 30f);
			}
		}
	}
}

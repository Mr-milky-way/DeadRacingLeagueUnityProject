using System;
using System.Collections.Generic;
using UnityEngine;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class MapAsset : DRLAsset
	{
		private MapAssetTag m_tags;

		internal MDObject m_data;

		[SerializeField]
		private string m_id;

		private string m_cache_id;

		[SerializeField]
		private List<MapAssetComponent> m_components;

		public Action<MapAsset, MapAssetEventType> OnEvent;

		private bool m_lock_refresh;

		private List<MapAssetAction> m_actions;

		public List<MapAssetType> tags
		{
			get
			{
				m_tags = CheckTags(m_tags);
				if (!m_tags)
				{
					return new List<MapAssetType>();
				}
				return m_tags.tags;
			}
		}

		public MDObject data
		{
			get
			{
				if (m_data != null)
				{
					return m_data;
				}
				m_data = GetData();
				Write();
				return m_data;
			}
			set
			{
				m_data = value;
				if (value != null)
				{
					string p_data = value.ToJson();
					m_data = GetData();
					Serialize.FromJson(p_data, (object)m_data, true);
				}
			}
		}

		public bool replacedGUID
		{
			get
			{
				return data.replacedGUID;
			}
			set
			{
				data.replacedGUID = value;
			}
		}

		public string id
		{
			get
			{
				if (valid && m_cache_id != m_id)
				{
					m_cache_id = m_id;
				}
				if (!string.IsNullOrEmpty(m_cache_id))
				{
					return m_cache_id;
				}
				return m_cache_id = data.id;
			}
		}

		public bool valid => m_id != "0";

		public new string name
		{
			get
			{
				return data.name;
			}
			set
			{
				string text = (data.name = value);
				base.name = text;
			}
		}

		public List<MapAssetComponent> components
		{
			get
			{
				if (m_components == null)
				{
					m_components = new List<MapAssetComponent>();
				}
				return m_components;
			}
		}

		public List<MapAssetAction> actions
		{
			get
			{
				if (m_actions != null)
				{
					return m_actions;
				}
				return m_actions = Hierarchy.FindAll<MapAssetAction>(base.transform);
			}
		}

		public override string GetPrefix()
		{
			return "DMA";
		}

		protected void Trigger(MapAssetEventType p_event)
		{
			List<MapAssetComponent> list = components;
			for (int i = 0; i < list.Count; i++)
			{
				if ((bool)list[i])
				{
					list[i].OnEvent(this, p_event);
				}
			}
			if (OnEvent != null)
			{
				OnEvent(this, p_event);
			}
		}

		protected override string GetGUID()
		{
			return GetPrefix() + "-" + GUID.Create(1, "", 500, 0, 65535, "x4");
		}

		protected virtual void Awake()
		{
			components.Clear();
			MapAssetComponent[] array = GetComponents<MapAssetComponent>();
			if (array != null)
			{
				components.AddRange(array);
			}
		}

		public virtual void Write()
		{
			MDObject mDObject = data;
			Trigger(MapAssetEventType.DataWrite);
			mDObject.name = base.name;
			string text = base.guid;
			string text2 = mDObject.guid;
			if (!string.IsNullOrEmpty(text2) && text2 != text)
			{
				Debug.LogWarning("MapAsset> GUID ovewrite attempted / from[" + text2 + "] to[" + text + "]");
				text = text2;
			}
			mDObject.guid = text;
			m_id = mDObject.id;
			m_cache_id = m_id;
		}

		public virtual void Read()
		{
			MDObject mDObject = m_data;
			if (mDObject != null)
			{
				Trigger(MapAssetEventType.DataRead);
				m_id = mDObject.id;
				m_cache_id = m_id;
				base.guid = mDObject.guid;
				base.name = mDObject.name;
			}
		}

		public void GenerateId()
		{
			MDObject mDObject = m_data;
			if (mDObject != null)
			{
				mDObject.id = MDObject.GenerateId();
				m_id = mDObject.id;
				m_cache_id = mDObject.id;
			}
		}

		public void Refresh()
		{
			if (!m_lock_refresh)
			{
				m_lock_refresh = true;
				if ((bool)this && base.enabled)
				{
					OnRefresh();
				}
				m_lock_refresh = false;
			}
		}

		protected virtual void OnRefresh()
		{
			Trigger(MapAssetEventType.Refresh);
		}

		protected virtual MDObject NewData()
		{
			return new MDObject();
		}

		public MDObject GetData()
		{
			return NewData();
		}
	}
}

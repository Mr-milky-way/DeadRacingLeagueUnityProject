using System;
using System.Collections.Generic;
using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class MESelectionModel : Model<DRLApp>
	{
		[SerializeField]
		private List<MapAsset> m_assets;

		public List<MapAssetType> assetTags;

		[SerializeField]
		private List<MAEntity> m_entities;

		private List<string> m_entity_ids;

		public List<MapAssetType> entityTags;

		public bool anyAsset => asset != null;

		public bool anyEntity => entity != null;

		public bool multiEntity => m_entities.Count > 1;

		public bool multiAsset => m_assets.Count > 1;

		public bool any
		{
			get
			{
				if (!anyAsset)
				{
					return anyEntity;
				}
				return true;
			}
		}

		public bool none
		{
			get
			{
				if (!anyAsset)
				{
					return !anyEntity;
				}
				return false;
			}
		}

		public List<MapAsset> assets
		{
			get
			{
				if (m_assets != null)
				{
					return m_assets;
				}
				return m_assets = new List<MapAsset>();
			}
			set
			{
				if (m_assets == null)
				{
					m_assets = new List<MapAsset>();
				}
				List<string> list = assetsGUIDs;
				Notify("map-editor.selection.assets@remove", m_assets);
				m_assets.Clear();
				if (value != null)
				{
					m_assets.AddRange(value);
				}
				List<string> list2 = assetsGUIDs;
				assetTags = new List<MapAssetType>();
				for (int i = 0; i < m_assets.Count; i++)
				{
					List<MapAssetType> tags = m_assets[i].tags;
					for (int j = 0; j < tags.Count; j++)
					{
						if (!assetTags.Contains(tags[j]))
						{
							assetTags.Add(tags[j]);
						}
					}
				}
				Notify("map-editor.selection.assets@add", m_assets);
				if (ValidateListChange(list, list2))
				{
					Notify("map-editor.selection.assets@change", list, list2);
				}
			}
		}

		public MapAsset asset
		{
			get
			{
				if (m_assets.Count > 0)
				{
					return m_assets[0];
				}
				return null;
			}
			set
			{
				assets = ((value == null) ? null : new List<MapAsset> { value });
			}
		}

		public List<string> assetsGUIDs => assets.ConvertAll<string>(MapAssetToGUID);

		public List<MAEntity> entities
		{
			get
			{
				if (m_entities != null)
				{
					return m_entities;
				}
				return m_entities = new List<MAEntity>();
			}
			set
			{
				if (m_entities == null)
				{
					m_entities = new List<MAEntity>();
				}
				if (m_entity_ids == null)
				{
					m_entity_ids = new List<string>();
				}
				List<string> list = new List<string>(entitiesIds);
				for (int i = 0; i < m_entities.Count; i++)
				{
					m_entities[i].OnEditorUnselect();
				}
				Notify("map-editor.selection.entities@remove", m_entities);
				m_entities.Clear();
				m_entity_ids.Clear();
				if (value != null)
				{
					m_entities.AddRange(value);
					m_entities.RemoveAll(EntityValidFilter);
					m_entity_ids = m_entities.ConvertAll<string>(EntityToId);
				}
				List<string> list2 = new List<string>(entitiesIds);
				entityTags = new List<MapAssetType>();
				for (int j = 0; j < m_entities.Count; j++)
				{
					List<MapAssetType> tags = m_entities[j].tags;
					for (int k = 0; k < tags.Count; k++)
					{
						if (!entityTags.Contains(tags[k]))
						{
							entityTags.Add(tags[k]);
						}
					}
				}
				for (int l = 0; l < m_entities.Count; l++)
				{
					m_entities[l].OnEditorSelect();
				}
				Notify("map-editor.selection.entities@add", m_entities);
				if (ValidateListChange(list, list2))
				{
					Notify("map-editor.selection.entities@change", list, list2);
				}
			}
		}

		public List<string> entitiesIds
		{
			get
			{
				if (m_entity_ids != null)
				{
					return m_entity_ids;
				}
				return m_entity_ids = new List<string>();
			}
		}

		public MAEntity entity
		{
			get
			{
				if (m_entities.Count > 0)
				{
					return m_entities[0];
				}
				return null;
			}
			set
			{
				entities = ((value == null) ? null : new List<MAEntity> { value });
			}
		}

		private string MapAssetToGUID(MapAsset it)
		{
			return it.guid;
		}

		public bool TrueForAllAssets(MapAssetType p_flag)
		{
			for (int i = 0; i < assets.Count; i++)
			{
				if ((bool)assets[i] && !assets[i].tags.Contains(p_flag))
				{
					return false;
				}
			}
			return true;
		}

		private string EntityToId(MAEntity it)
		{
			return it.id;
		}

		private int EntityIdSort(MAEntity a, MAEntity b)
		{
			return string.Compare(a.id, b.id);
		}

		private bool EntityValidFilter(MAEntity a)
		{
			if (!a)
			{
				return true;
			}
			return !a.gameObject.activeInHierarchy;
		}

		public bool TrueForAllEntities(MapAssetType p_flag)
		{
			for (int i = 0; i < entities.Count; i++)
			{
				if ((bool)entities[i] && !entities[i].tags.Contains(p_flag))
				{
					return false;
				}
			}
			return true;
		}

		public bool TrueForAnyEntities(MapAssetType p_flag)
		{
			for (int i = 0; i < entities.Count; i++)
			{
				if ((bool)entities[i] && entities[i].tags.Contains(p_flag))
				{
					return true;
				}
			}
			return false;
		}

		public void SetEntity(List<MAEntity> p_items, bool p_combine)
		{
			if (p_combine)
			{
				PushEntity(p_items);
			}
			else
			{
				entities = p_items;
			}
		}

		public void SetEntity(MAEntity p_item, bool p_combine)
		{
			if (p_combine)
			{
				PushEntity(p_item);
			}
			else
			{
				entity = p_item;
			}
		}

		public void PushEntity(MAEntity p_item)
		{
			List<MAEntity> list = new List<MAEntity>(entities);
			if (list.Contains(p_item))
			{
				list.Remove(p_item);
			}
			else
			{
				list.Add(p_item);
			}
			entities = list;
		}

		public void PushEntity(List<MAEntity> p_items)
		{
			List<MAEntity> list = new List<MAEntity>(entities);
			bool flag = false;
			for (int i = 0; i < p_items.Count; i++)
			{
				MAEntity item = p_items[i];
				if (list.Contains(item))
				{
					list.Remove(item);
				}
				else
				{
					list.Add(item);
				}
				flag = true;
			}
			if (flag)
			{
				entities = list;
			}
		}

		public void ClearEntities()
		{
			entities = null;
		}

		public void InvalidateEntities()
		{
			for (int i = 0; i < m_entities.Count; i++)
			{
				m_entities[i].OnEditorUnselect();
			}
			m_entities = new List<MAEntity>();
			entities = null;
		}

		public List<T> ConvertEntities<T>() where T : Component
		{
			List<MAEntity> list = entities;
			List<T> list2 = new List<T>();
			for (int i = 0; i < list.Count; i++)
			{
				T component = list[i].GetComponent<T>();
				if ((bool)component)
				{
					list2.Add(component);
				}
			}
			return list2;
		}

		public List<MAEntity> FilterEntities(Predicate<MAEntity> p_callback)
		{
			List<MAEntity> list = new List<MAEntity>();
			if (p_callback == null)
			{
				return list;
			}
			List<MAEntity> list2 = entities;
			for (int i = 0; i < list2.Count; i++)
			{
				if (p_callback(list2[i]))
				{
					list.Add(list2[i]);
				}
			}
			return list;
		}

		public List<MAEntity> FilterEntities(MDEntityAttribFlag p_flags)
		{
			return FilterEntities((MAEntity it) => (it.attribs & p_flags) != 0);
		}

		public void SetAsset(List<MapAsset> p_items, bool p_combine)
		{
			if (p_combine)
			{
				PushAsset(p_items);
			}
			else
			{
				assets = p_items;
			}
		}

		public void SetAsset(MapAsset p_item, bool p_combine)
		{
			if (p_combine)
			{
				PushAsset(p_item);
			}
			else
			{
				asset = ((asset == p_item) ? null : p_item);
			}
		}

		public void PushAsset(MapAsset p_item)
		{
			List<MapAsset> list = new List<MapAsset>(assets);
			if (list.Contains(p_item))
			{
				list.Remove(p_item);
			}
			else
			{
				list.Add(p_item);
			}
			assets = new List<MapAsset>(list);
		}

		public void PushAsset(List<MapAsset> p_items)
		{
			List<MapAsset> list = new List<MapAsset>(assets);
			bool flag = false;
			for (int i = 0; i < p_items.Count; i++)
			{
				MapAsset item = p_items[i];
				if (list.Contains(item))
				{
					list.Remove(item);
				}
				else
				{
					list.Add(item);
				}
				flag = true;
			}
			if (flag)
			{
				assets = list;
			}
		}

		public void ClearAssets()
		{
			assets = null;
		}

		public void InvalidateAssets()
		{
			m_assets = new List<MapAsset>();
			assets = null;
		}

		public List<T> ConvertAssets<T>() where T : Component
		{
			List<MapAsset> list = assets;
			List<T> list2 = new List<T>();
			for (int i = 0; i < list.Count; i++)
			{
				T component = list[i].GetComponent<T>();
				if ((bool)component)
				{
					list2.Add(component);
				}
			}
			return list2;
		}

		public bool CanMultiSelectAsset()
		{
			List<MapAsset> list = assets;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] is MASpline)
				{
					return false;
				}
			}
			return true;
		}

		protected bool ValidateListChange(List<string> a, List<string> b)
		{
			if (a.Count != b.Count)
			{
				return true;
			}
			for (int i = 0; i < a.Count; i++)
			{
				bool flag = false;
				for (int j = i + 1; j < b.Count; j++)
				{
					if (a[i] == b[j])
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return true;
				}
			}
			return false;
		}
	}
}

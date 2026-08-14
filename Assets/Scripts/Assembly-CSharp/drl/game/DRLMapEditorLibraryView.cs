using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class DRLMapEditorLibraryView : DRLLibraryView<UICardButtonMapEditorAssetItem>
	{
		[Header("Map Editor")]
		public List<string> libraryList;

		public string query;

		public MapAssetType[] filters = new MapAssetType[2];

		public int SetFilter(string p_query, params MapAssetType[] p_filters)
		{
			query = p_query;
			Array.Copy(p_filters, filters = new MapAssetType[p_filters.Length], p_filters.Length);
			return Filter<MapAsset>(SortAllAssets);
		}

		public int GetFilterCount(string p_query, params MapAssetType[] p_filters)
		{
			query = p_query;
			Array.Copy(p_filters, filters = new MapAssetType[p_filters.Length], p_filters.Length);
			return GetSortedAssets<MapAsset>(SortAllAssets).Count;
		}

		protected override void OnPageRefresh()
		{
			if ((bool)scroll && (bool)container)
			{
				GridLayoutGroup gridLayoutGroup = container as GridLayoutGroup;
				if ((bool)gridLayoutGroup)
				{
					int num = Mathf.CeilToInt((float)count / (float)gridLayoutGroup.constraintCount);
					float x = gridLayoutGroup.spacing.x;
					int pages = Mathf.CeilToInt(((float)num * (gridLayoutGroup.cellSize.x + x) - x) / (scroll.pageWidth + x));
					scroll.SetPages(pages);
				}
			}
		}

		protected override void OnSetCard(UICardButtonMapEditorAssetItem p_target, DRLAsset p_data)
		{
			MapAsset mapAsset = p_data as MapAsset;
			if ((bool)mapAsset)
			{
				p_target.Set(mapAsset);
			}
		}

		protected override void OnSetEmpty(UICardButtonMapEditorAssetItem p_target)
		{
			p_target.Set(null);
		}

		protected override bool OnLibraryFilter(AssetLibrary p_item)
		{
			List<string> list = libraryList;
			if (list == null)
			{
				return true;
			}
			if (list.Count <= 0)
			{
				return true;
			}
			return list.Contains(p_item.name);
		}

		protected override bool OnAssetFilter(DRLAsset p_item)
		{
			MapAsset mapAsset = p_item as MapAsset;
			if (!mapAsset)
			{
				return false;
			}
			bool isDeveloper = base.app.model.storage.state.player.profile.isDeveloper;
			bool flag = isDeveloper || !mapAsset.filter || !mapAsset.filter.inDevelopment;
			bool flag2 = true;
			for (int i = 0; i < filters.Length; i++)
			{
				MapAssetType mapAssetType = filters[i];
				if (mapAssetType != MapAssetType.None)
				{
					flag2 = flag2 && mapAsset.tags.Contains(mapAssetType);
					if (!isDeveloper && mapAssetType == MapAssetType.Debug)
					{
						flag2 = false;
					}
				}
			}
			query = query.ToLower().Trim();
			string[] array = query.Split(' ');
			string text = mapAsset.name.ToLower();
			bool flag3 = string.IsNullOrEmpty(query);
			for (int j = 0; j < array.Length; j++)
			{
				string value = array[j].Trim();
				bool flag4 = string.IsNullOrEmpty(value) || text.Contains(value);
				flag3 = flag3 || flag4;
			}
			return flag2 && flag3 && flag;
		}

		public void SetSelection(List<MapAsset> p_list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				UICardButtonMapEditorAssetItem uICardButtonMapEditorAssetItem = list.Get<UICardButtonMapEditorAssetItem>(i);
				if ((bool)uICardButtonMapEditorAssetItem)
				{
					uICardButtonMapEditorAssetItem.selected = false;
				}
			}
			if (p_list == null)
			{
				return;
			}
			for (int j = 0; j < list.Count; j++)
			{
				UICardButtonMapEditorAssetItem uICardButtonMapEditorAssetItem2 = list.Get<UICardButtonMapEditorAssetItem>(j);
				if ((bool)uICardButtonMapEditorAssetItem2 && (bool)uICardButtonMapEditorAssetItem2.data)
				{
					uICardButtonMapEditorAssetItem2.selected = p_list.Contains(uICardButtonMapEditorAssetItem2.data);
				}
			}
		}

		public void SetSelection(MapAsset p_item)
		{
			SetSelection(p_item ? new List<MapAsset> { p_item } : null);
		}

		protected int SortAllAssets(MapAsset a, MapAsset b)
		{
			MapAssetType mapAssetType = ((a.tags.Count > 0) ? a.tags[0] : MapAssetType.None);
			MapAssetType mapAssetType2 = ((b.tags.Count > 0) ? a.tags[0] : MapAssetType.None);
			if (a.tags.Count > 1 && mapAssetType == MapAssetType.Prop)
			{
				mapAssetType = a.tags[1];
			}
			if (b.tags.Count > 1 && mapAssetType2 == MapAssetType.Prop)
			{
				mapAssetType2 = b.tags[1];
			}
			if (mapAssetType == mapAssetType2)
			{
				if (a.order == b.order)
				{
					return string.Compare(a.info.name, b.info.name);
				}
				if (a.order >= b.order)
				{
					return 1;
				}
				return -1;
			}
			if (mapAssetType == MapAssetType.Debug && mapAssetType2 != MapAssetType.Debug)
			{
				return 1;
			}
			if (mapAssetType != MapAssetType.Debug && mapAssetType2 == MapAssetType.Debug)
			{
				return -1;
			}
			if (mapAssetType >= mapAssetType2)
			{
				return 1;
			}
			return -1;
		}
	}
}

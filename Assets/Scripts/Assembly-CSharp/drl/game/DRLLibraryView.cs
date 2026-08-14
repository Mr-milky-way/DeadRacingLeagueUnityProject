using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLLibraryView<T> : View<DRLApp> where T : UICardView
	{
		[Header("UI")]
		public ListComponent list;

		public Button prevPageButton;

		public Button nextPageButton;

		public DRLScrollView scroll;

		public int count;

		[Header("Layout")]
		public LayoutGroup container;

		public int minPadding;

		public int restPadding;

		public int offsetPadding;

		[Header("Navigation")]
		public Component upNavLink;

		public Component downNavLink;

		public Component leftNavLink;

		public Component rightNavLink;

		[Header("Query")]
		public List<string> groupList;

		protected virtual void Awake()
		{
			count = 0;
			if ((bool)prevPageButton)
			{
				prevPageButton.onClick.AddListener(PrevPageClick);
			}
			if ((bool)nextPageButton)
			{
				nextPageButton.onClick.AddListener(NextPageClick);
			}
		}

		protected void PrevPageClick()
		{
			if (base.enabled)
			{
				OnPrevPageClick();
			}
		}

		protected void NextPageClick()
		{
			if (base.enabled)
			{
				OnNextPageClick();
			}
		}

		protected virtual void OnPrevPageClick()
		{
			if ((bool)scroll)
			{
				scroll.PreviousPage();
			}
		}

		protected virtual void OnNextPageClick()
		{
			if ((bool)scroll)
			{
				scroll.NextPage();
			}
		}

		protected List<U> GetSortedAssets<U>(Comparison<U> p_sort) where U : DRLAsset
		{
			List<U> list = new List<U>();
			List<AssetLibrary> list2 = base.app.model.storage.library.FindAll<AssetLibrary>();
			for (int i = 0; i < list2.Count; i++)
			{
				AssetLibrary it = list2[i];
				if (!OnLibraryFilter(it))
				{
					continue;
				}
				List<U> list3 = it.FindAll<U>();
				list3.RemoveAll(delegate(U p_asset)
				{
					if (!p_asset)
					{
						return true;
					}
					if (groupList.Count <= 0)
					{
						return false;
					}
					string guid = p_asset.guid;
					if (it is DRLAssetBundleLibrary)
					{
						List<string> groups = ((DRLAssetBundleLibrary)it).GetGroups(guid);
						if (groups.Count <= 0)
						{
							return false;
						}
						for (int j = 0; j < groups.Count; j++)
						{
							string item = groups[j];
							if (groupList.Contains(item))
							{
								return false;
							}
						}
						return true;
					}
					return false;
				});
				list.AddRange(list3);
			}
			for (int num = 0; num < list.Count; num++)
			{
				U val = list[num];
				if (!OnAssetFilter(val))
				{
					list.RemoveAt(num--);
					continue;
				}
				DRLLibraryAsset filter = val.filter;
				if ((bool)filter && !filter.available)
				{
					list.RemoveAt(num--);
				}
			}
			if (p_sort != null)
			{
				list.Sort(p_sort);
			}
			return list;
		}

		public int Filter<U>(Comparison<U> p_sort) where U : DRLAsset
		{
			if (!list)
			{
				return 0;
			}
			list.Clear();
			List<U> sortedAssets = GetSortedAssets(p_sort);
			SetCards(sortedAssets);
			OnPageRefresh();
			OnRefresh();
			sortedAssets.RemoveAll((U it) => it == null);
			return sortedAssets.Count;
		}

		public void Filter<U>() where U : DRLAsset
		{
			Filter<U>(null);
		}

		public void SetCards<U>(List<U> p_assets) where U : DRLAsset
		{
			List<U> list = ((p_assets == null) ? new List<U>() : new List<U>(p_assets));
			count = list.Count;
			while (list.Count < minPadding)
			{
				list.Add(null);
			}
			for (int i = 0; i < offsetPadding; i++)
			{
				list.Add(null);
			}
			int num = ((restPadding > 0) ? (list.Count % restPadding) : 0) + restPadding;
			for (int j = 0; j < num; j++)
			{
				list.Add(null);
			}
			for (int k = 0; k < list.Count; k++)
			{
				this.list.Push<T>();
				Transform transform = this.list.Get<Transform>(k);
				OnInstanceCreate(transform.gameObject);
			}
			for (int l = 0; l < list.Count; l++)
			{
				T val = this.list.Get<T>(l);
				if ((bool)val)
				{
					U val2 = list[l];
					if (!val2)
					{
						OnSetEmpty(val);
					}
					else
					{
						OnSetCard(val, val2);
					}
				}
			}
			LayoutGroup layoutGroup = container;
			if ((bool)layoutGroup)
			{
				UINavigation.Link(layoutGroup, leftNavLink, rightNavLink, upNavLink, downNavLink);
			}
		}

		protected virtual void OnPageRefresh()
		{
		}

		protected virtual bool OnAssetFilter(DRLAsset p_item)
		{
			return true;
		}

		protected virtual bool OnLibraryFilter(AssetLibrary p_item)
		{
			return true;
		}

		protected virtual void OnInstanceCreate(GameObject p_target)
		{
		}

		protected virtual void OnSetEmpty(T p_target)
		{
		}

		protected virtual void OnSetCard(T p_target, DRLAsset p_data)
		{
		}

		protected virtual void OnRefresh()
		{
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class UICommunityMapsView : UIScreenView
	{
		public ListComponent listField;

		public UINavigation sortStepperNav;

		public DRLStepperView sortStepper;

		public DRLStepperView difficultyStepper;

		public DRLStepperView baseMapStepper;

		public List<DRLMap> baseMapList;

		public DRLInputFieldView searchInput;

		public FadeComponent feedbackFade;

		public FadeComponent listFade;

		public List<GameObject> feedbacks;

		public DRLPagePickerView pageField;

		public GameObject menuContainer;

		public UICommunityMapsShowCriteria showCriteria;

		public GameFlag showCategory = GameFlag.MapCommon;

		public GameObject exitButton;

		private UINavigation m_exitButtonNav;

		public List<GameObject> navRightGroup;

		public List<DRLCommunityMapData> maps;

		private bool m_allow_exit;

		[HideInInspector]
		public string initMapGUID;

		[HideInInspector]
		public bool isMultiGP;

		private UINavigation exitButtonNav
		{
			get
			{
				if (m_exitButtonNav == null)
				{
					m_exitButtonNav = exitButton.GetComponent<UINavigation>();
				}
				return m_exitButtonNav;
			}
		}

		public bool allowExit
		{
			get
			{
				return m_allow_exit;
			}
			set
			{
				m_allow_exit = value;
				exitButton.SetActive(value);
			}
		}

		public void InitFilter(bool p_isMultiGP, string p_mapGUID = "")
		{
			initMapGUID = p_mapGUID;
			isMultiGP = p_isMultiGP;
		}

		public void Clear()
		{
			listField.Clear();
			maps = new List<DRLCommunityMapData>();
		}

		public void InitializeSteppers()
		{
			UpdateBaseMapsStepper();
			ResetDifficultyStepper();
			ResetSortStepper();
			ClearSearch();
		}

		public void UpdateBaseMapsStepper()
		{
			List<DRLMap> list = base.app.model.storage.GetMaps(true);
			list.RemoveAll((DRLMap it) => (bool)it.tags && it.tags.Contains(GameFlag.MapEditorOnly));
			baseMapList = list;
			baseMapStepper.min = 0;
			baseMapStepper.max = list.Count;
			baseMapStepper.labels = new string[list.Count + 1];
			baseMapStepper.labels[0] = "ALL";
			for (int num = 0; num < list.Count; num++)
			{
				baseMapStepper.labels[num + 1] = list[num].label;
			}
			baseMapStepper.index = 0;
			baseMapStepper.Refresh();
		}

		public void ResetDifficultyStepper()
		{
			difficultyStepper.index = 0;
			difficultyStepper.Refresh();
		}

		public void ResetSortStepper()
		{
			sortStepper.index = 0;
			sortStepper.Refresh();
		}

		public void ClearSearch()
		{
			searchInput.text = "";
		}

		public void UpdateList(List<DRLCommunityMapData> p_maps, int p_page, int p_page_length, int p_pages_count = -1, bool p_allow_search = false)
		{
			List<DRLCommunityMapData> collection = ((p_maps == null) ? new List<DRLCommunityMapData>() : p_maps);
			collection = new List<DRLCommunityMapData>(collection);
			if (p_allow_search)
			{
				collection.RemoveAll(delegate(DRLCommunityMapData p_it)
				{
					string text = searchInput.field.text;
					text = text.Trim().ToLower();
					return !string.IsNullOrEmpty(text) && !p_it.mapTitle.ToLower().Contains(text);
				});
			}
			int num = ((p_page_length > 0) ? ((collection.Count - 1) / p_page_length) : 0) + 1;
			if (p_pages_count > 0)
			{
				num = p_pages_count;
			}
			int num2 = Mathf.Clamp(p_page, 0, num - 1);
			List<DRLCommunityMapData> list = new List<DRLCommunityMapData>();
			int num3 = ((collection.Count > p_page_length) ? Mathf.Max(0, num2 * p_page_length) : 0);
			for (int num4 = 0; num4 < p_page_length; num4++)
			{
				if (num3 >= collection.Count)
				{
					break;
				}
				DRLCommunityMapData item = collection[num3];
				list.Add(item);
				num3++;
			}
			Debug.Log("UICommunityMapsView> UpdateList - total[" + collection.Count + "] page[" + num2 + "] total-pages[" + num + "] elements[" + list.Count + "]");
			List<DRLCommunityMapData> list2 = new List<DRLCommunityMapData>();
			List<DRLCommunityMapData> list3 = new List<DRLCommunityMapData>();
			if (maps == null)
			{
				maps = new List<DRLCommunityMapData>();
			}
			for (int num5 = 0; num5 < list.Count; num5++)
			{
				if (!ContainsMap(maps, list[num5]))
				{
					list2.Add(list[num5]);
				}
			}
			for (int num6 = 0; num6 < maps.Count; num6++)
			{
				if (!ContainsMap(list, maps[num6]))
				{
					list3.Add(maps[num6]);
				}
			}
			Debug.Log("UICommunityMapsView> UpdateList - add[" + list2.Count + "] remove[" + list3.Count + "]");
			for (int num7 = 0; num7 < list3.Count; num7++)
			{
				RemoveMap(list3[num7]);
			}
			for (int num8 = 0; num8 < list2.Count; num8++)
			{
				if (maps.Count < p_page_length)
				{
					AddMap(list2[num8]);
				}
			}
			for (int num9 = 0; num9 < list.Count; num9++)
			{
				int mapIndex = GetMapIndex(list[num9]);
				if (mapIndex >= 0)
				{
					maps[mapIndex] = list[num9];
				}
			}
			for (int num10 = 0; num10 < maps.Count; num10++)
			{
				UpdateMap(maps[num10]);
			}
			UpdateNavigation();
			if (num > 1)
			{
				ShowPages();
			}
			else
			{
				HidePages();
			}
			pageField.Set(num);
			pageField.index = num2;
			UICommunityMapsFeedbackType p_type = ((maps.Count <= 0) ? UICommunityMapsFeedbackType.NoMaps : UICommunityMapsFeedbackType.None);
			SetFeedback(p_type, p_hide_list: true, 0.1f);
		}

		public void HidePages()
		{
			FadeComponent fade = pageField.fade;
			if (fade.alpha < 0f)
			{
				fade.alpha = 0f;
			}
			fade.FadeOut(0.01f);
		}

		public void ShowPages()
		{
			FadeComponent fade = pageField.fade;
			if (fade.alpha < 0f)
			{
				fade.alpha = 0f;
			}
			fade.FadeIn(0.3f);
		}

		protected void UpdateNavigation()
		{
			ListComponent listComponent = listField;
			List<UINavigation> fly_navs = new List<UINavigation>();
			List<UINavigation> data_navs = new List<UINavigation>();
			List<UINavigation> add_navs = new List<UINavigation>();
			List<UINavigation> list = new List<UINavigation>();
			List<UINavigation> del_navs = new List<UINavigation>();
			List<UINavigation> list2 = new List<UINavigation>();
			List<UINavigation> list3 = new List<UINavigation>();
			UINavigation page_nav = pageField.GetComponent<UINavigation>();
			for (int i = 0; i < listComponent.Count; i++)
			{
				UICommunityMapsItemView uICommunityMapsItemView = listComponent.Get<UICommunityMapsItemView>(i);
				if (base.app.arguments.game.type == GameFlag.MapEditor)
				{
					add_navs.Add(uICommunityMapsItemView.addNav);
					list.Add(uICommunityMapsItemView.editNav);
					del_navs.Add(uICommunityMapsItemView.delNav);
					list2.Add(uICommunityMapsItemView.cloNav);
				}
				else
				{
					fly_navs.Add(uICommunityMapsItemView.flyNav);
				}
				data_navs.Add(uICommunityMapsItemView.dataNav);
				list3.Add(uICommunityMapsItemView.dataProxyNav);
				if (i >= listComponent.Count - 1)
				{
					page_nav.up = uICommunityMapsItemView.dataNav;
				}
			}
			UINavigation uINavigation = sortStepperNav;
			UINavigation.Link(data_navs.ToArray(), 0, p_vertical: true, base.leftNavigation, null, uINavigation, page_nav);
			if (base.app.arguments.game.type == GameFlag.MapEditor)
			{
				UINavigation.Link(add_navs.ToArray(), 0, p_vertical: true, null, GetRightNavigationLink(), uINavigation, page_nav);
			}
			else
			{
				UINavigation.Link(fly_navs.ToArray(), 0, p_vertical: true, null, GetRightNavigationLink(), uINavigation, page_nav);
			}
			int navs_len = data_navs.Count;
			((Component)this).ActivityRun((Func<bool>)delegate
			{
				if (pageField.selection == null)
				{
					return true;
				}
				page_nav = pageField.selection.GetComponent<UINavigation>();
				if (navs_len <= 0)
				{
					return false;
				}
				int num2 = navs_len - 1;
				data_navs[num2].down = page_nav;
				if (base.app.arguments.game.type == GameFlag.MapEditor)
				{
					add_navs[num2].down = page_nav;
					if (del_navs.Count > 0 && num2 < del_navs.Count && del_navs[num2].gameObject.activeSelf)
					{
						del_navs[num2].down = page_nav;
					}
				}
				else if (num2 < fly_navs.Count)
				{
					fly_navs[num2].down = page_nav;
				}
				return false;
			}, 0f);
			for (int num = 0; num < navs_len; num++)
			{
				if (base.app.arguments.game.type == GameFlag.MapEditor)
				{
					if (num + 1 < add_navs.Count)
					{
						UINavigation down = (list[num + 1].gameObject.activeSelf ? list[num + 1] : add_navs[num + 1]);
						if (del_navs[num].gameObject.activeSelf)
						{
							del_navs[num].down = down;
						}
						else
						{
							add_navs[num].down = down;
						}
					}
					else if (del_navs[num].gameObject.activeSelf)
					{
						del_navs[num].down = page_nav;
					}
					list2[num].up = list[num];
					list2[num].down = del_navs[num];
					if (num > 0)
					{
						UINavigation up = (del_navs[num - 1].gameObject.activeSelf ? del_navs[num - 1] : add_navs[num - 1]);
						if (list[num].gameObject.activeSelf)
						{
							list[num].up = up;
						}
						else
						{
							add_navs[num].up = up;
						}
					}
					else if (list[num].gameObject.activeSelf)
					{
						list[num].up = uINavigation;
					}
					else
					{
						add_navs[num].up = uINavigation;
					}
					list[num].left = data_navs[num];
					del_navs[num].left = data_navs[num];
					list2[num].left = data_navs[num];
					add_navs[num].left = data_navs[num];
					del_navs[num].up = list2[num];
					list[num].down = list2[num];
					list[num].right = GetRightNavigationLink();
					list2[num].right = GetRightNavigationLink();
					del_navs[num].right = GetRightNavigationLink();
				}
				else
				{
					fly_navs[num].left = data_navs[num];
				}
				data_navs[num].right = list3[num];
			}
			if (navs_len > 0)
			{
				SetRightNavigationLeft(list3[0]);
			}
			else
			{
				SetRightNavigationLeft(uINavigation);
			}
			exitButtonNav.right = data_navs[0];
		}

		public void ResetNavigation()
		{
			base.leftNavigation.right = sortStepperNav;
			SetRightNavigationLeft(sortStepperNav);
			exitButtonNav.right = sortStepperNav;
		}

		public void AddMap(DRLCommunityMapData p_data)
		{
			maps.Add(p_data);
			DRLMap p_map = base.app.model.storage.library.FindByGUID<DRLMap>(p_data.mapId);
			Localization locale = base.app.model.storage.locale;
			UICommunityMapsItemView uICommunityMapsItemView = listField.Push<UICommunityMapsItemView>();
			bool flag = base.app.arguments.game.type == GameFlag.MapEditor;
			_ = base.app.arguments.game.mode;
			uICommunityMapsItemView.Set(p_data, showCriteria == UICommunityMapsShowCriteria.MyMaps, !flag, p_disable_privates: false, base.app.model.service.backend.playerId, p_map, locale, base.app.model.service, base.app.model.storage);
		}

		public void RemoveMap(DRLCommunityMapData p_data)
		{
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].guid == p_data.guid)
				{
					maps.RemoveAt(i);
					break;
				}
			}
			for (int j = 0; j < listField.Count; j++)
			{
				UICommunityMapsItemView uICommunityMapsItemView = listField.Get<UICommunityMapsItemView>(j);
				if ((bool)uICommunityMapsItemView && uICommunityMapsItemView.data.guid == p_data.guid)
				{
					listField.Remove(j);
					break;
				}
			}
		}

		public void UpdateMap(DRLCommunityMapData p_data)
		{
			UICommunityMapsItemView byMapId = GetByMapId(p_data.guid);
			if ((bool)byMapId)
			{
				string p_override_profile_img_url = null;
				if (string.IsNullOrEmpty(p_data.profileThumbURL))
				{
					p_override_profile_img_url = base.app.model.storage.state.player.profile.photoURL;
				}
				DRLMap p_map = base.app.model.storage.library.FindByGUID<DRLMap>(p_data.mapId);
				Localization locale = base.app.model.storage.locale;
				bool flag = base.app.arguments.game.type == GameFlag.MapEditor;
				_ = base.app.arguments.game.mode;
				byMapId.Set(p_data, showCriteria == UICommunityMapsShowCriteria.MyMaps, !flag, p_disable_privates: false, base.app.model.service.backend.playerId, p_map, locale, base.app.model.service, base.app.model.storage, p_override_profile_img_url);
				List<DRLMapFavoriteData> favoriteMaps = base.app.model.storage.state.player.favoriteMaps;
				bool flag2 = p_data.typeFlag == GameFlag.Collectable;
				bool flag3 = favoriteMaps.Any((DRLMapFavoriteData m) => m.mapId == p_data.mapId && m.trackId == p_data.guid && m.customMap);
				byMapId.SetFavoriteToggleOn(flag3 && !flag2);
				byMapId.SetFavoriteActive(!flag2);
			}
		}

		public UICommunityMapsItemView GetByMapId(string p_id)
		{
			for (int i = 0; i < listField.Count; i++)
			{
				UICommunityMapsItemView uICommunityMapsItemView = listField.Get<UICommunityMapsItemView>(i);
				if (uICommunityMapsItemView.data != null && uICommunityMapsItemView.data.guid == p_id)
				{
					return uICommunityMapsItemView;
				}
			}
			return null;
		}

		public int GetMapIndex(DRLCommunityMapData p_map)
		{
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].guid == p_map.guid)
				{
					return i;
				}
			}
			return -1;
		}

		public bool ContainsMap(List<DRLCommunityMapData> p_list, DRLCommunityMapData p_map)
		{
			if (p_map == null)
			{
				return false;
			}
			if (p_list == null)
			{
				return false;
			}
			if (p_list.Count <= 0)
			{
				return false;
			}
			for (int i = 0; i < p_list.Count; i++)
			{
				if (p_list[i].guid == p_map.guid)
				{
					return true;
				}
			}
			return false;
		}

		public void SetFeedback(UICommunityMapsFeedbackType p_type, bool p_hide_list, float p_delay)
		{
			float feedback_alpha = ((p_type == UICommunityMapsFeedbackType.None) ? (-0.1f) : 1f);
			float content_alpha = ((p_type == UICommunityMapsFeedbackType.None) ? 1f : (p_hide_list ? (-0.1f) : 1f));
			Action action = delegate
			{
				feedbackFade.Fade(feedback_alpha, 0.3f, 0.05f, Cubic.Out);
				listFade.Fade(content_alpha, 0.3f, 0f, Cubic.Out);
				if (p_type != UICommunityMapsFeedbackType.None)
				{
					int num = (int)p_type;
					for (int i = 0; i < feedbacks.Count; i++)
					{
						feedbacks[i].SetActive(i == num);
					}
				}
			};
			if (p_delay <= 0f)
			{
				action();
			}
			else
			{
				RunOnce(p_delay, action);
			}
		}

		public void SetFeedback(UICommunityMapsFeedbackType p_type, bool p_hide_list)
		{
			SetFeedback(p_type, p_hide_list, 0f);
		}

		public void SetFeedback(UICommunityMapsFeedbackType p_type)
		{
			SetFeedback(p_type, p_hide_list: true, 0f);
		}

		public void SetMenuActive(bool p_flag)
		{
			sortStepper.interactable = p_flag;
			difficultyStepper.interactable = p_flag;
			baseMapStepper.interactable = p_flag;
			searchInput.interactable = p_flag;
		}

		public void SetRightNavigationEnabled(bool p_flag)
		{
			for (int i = 0; i < navRightGroup.Count; i++)
			{
				if ((bool)navRightGroup[i])
				{
					navRightGroup[i].SetActive(p_flag);
				}
			}
		}

		public void SetRightNavigationLeft(UINavigation p_link)
		{
			for (int i = 0; i < navRightGroup.Count; i++)
			{
				UINavigation component = navRightGroup[i].GetComponent<UINavigation>();
				if ((bool)component)
				{
					component.left = p_link;
				}
			}
		}

		public UINavigation GetRightNavigationLink()
		{
			for (int i = 0; i < navRightGroup.Count; i++)
			{
				UINavigation component = navRightGroup[i].GetComponent<UINavigation>();
				if ((bool)component)
				{
					return component;
				}
			}
			return null;
		}
	}
}

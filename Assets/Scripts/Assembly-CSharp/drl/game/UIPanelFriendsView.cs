using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIPanelFriendsView : View<DRLApp>
	{
		public GameObject friendsListContainter;

		public UINavigation searchNav;

		public UINavigation sortNav;

		[HideInInspector]
		public UINavigation tabNav;

		public InputField searchInput;

		public ListComponent listField;

		private bool m_repopulateList = true;

		public ScrollRect scroll;

		private bool open;

		public Color defaultFriendItemBackgroundColor;

		public Sort sortingMode { get; set; }

		public FadeComponent fade => AssertLocal<FadeComponent>("fade");

		public void Refresh(List<GameFriendData> p_data)
		{
			Set(p_data);
			SortFriends();
			RefreshUINavigation();
			List<UIFriendItemView> list = listField.GetList<UIFriendItemView>();
			int num = 0;
			if (list.Count == 0)
			{
				return;
			}
			foreach (UIFriendItemView item in list)
			{
				item.background.color = ((num % 2 != 0) ? Color.clear : defaultFriendItemBackgroundColor);
				num++;
			}
		}

		public void Set(List<GameFriendData> p_data)
		{
			if (listField.Count > 0)
			{
				int num = p_data.Count((GameFriendData o) => o.hasGame);
				if (num != listField.Count)
				{
					RepopulateList();
					if (num > listField.Count)
					{
						List<string> list = new List<string>();
						foreach (GameFriendData p_datum in p_data)
						{
							if (p_datum.hasGame)
							{
								Debug.Log(p_datum.platformId + " PID ");
								if (FindById(p_datum.platformId) == null)
								{
									list.Add(p_datum.platformId);
								}
							}
						}
						Notify("social.friend.add-friend-button@success", list);
					}
					else
					{
						List<string> list2 = new List<string>();
						foreach (UIFriendItemView it in listField.GetList<UIFriendItemView>())
						{
							if (p_data.All((GameFriendData o) => o.platformId != it.gameFriendData.platformId))
							{
								list2.Add(it.gameFriendData.platformId);
							}
						}
						Notify("chat.friend-remove", list2);
					}
				}
			}
			if (!m_repopulateList && listField.Count > 0)
			{
				foreach (GameFriendData p_datum2 in p_data)
				{
					if (p_datum2.hasGame)
					{
						UIFriendItemView uIFriendItemView = FindById(p_datum2.platformId);
						if (uIFriendItemView != null)
						{
							uIFriendItemView.SetStatus(p_datum2.status);
						}
					}
				}
				return;
			}
			m_repopulateList = false;
			listField.Clear();
			foreach (GameFriendData p_datum3 in p_data)
			{
				if (p_datum3.hasGame)
				{
					UIFriendItemView uIFriendItemView2 = listField.Push<UIFriendItemView>();
					if (uIFriendItemView2 != null)
					{
						uIFriendItemView2.Set(p_datum3);
					}
				}
			}
		}

		private void SortFriends()
		{
			if (listField.Count >= 2)
			{
				switch (sortingMode)
				{
				case Sort.online:
					listField.Sort(StatusSort);
					break;
				case Sort.name:
					listField.Sort(UsernameSort);
					break;
				}
			}
		}

		private int StatusSort(Component x, Component y)
		{
			UIFriendItemView component = x.GetComponent<UIFriendItemView>();
			UIFriendItemView component2 = y.GetComponent<UIFriendItemView>();
			bool ingame = component.gameFriendData.ingame;
			bool ingame2 = component2.gameFriendData.ingame;
			int num = -ingame.CompareTo(ingame2);
			int result = component.gameFriendData.name.CompareTo(component2.gameFriendData.name);
			if (num != 0)
			{
				return num;
			}
			return result;
		}

		private int UsernameSort(Component x, Component y)
		{
			UIFriendItemView component = x.GetComponent<UIFriendItemView>();
			UIFriendItemView component2 = y.GetComponent<UIFriendItemView>();
			return component.gameFriendData.name.CompareTo(component2.gameFriendData.name);
		}

		private void RefreshUINavigation()
		{
			if (listField.Count == 0)
			{
				return;
			}
			List<UIFriendItemView> list = listField.GetList<UIFriendItemView>();
			searchNav.down = list[0].navigation;
			sortNav.down = list[0].navigation;
			list[0].SetNavigation(NavigationDirection.up, searchNav);
			if ((bool)tabNav)
			{
				tabNav.up = listField.Get<Component>(listField.Count - 1);
			}
			if (listField.Count < 2)
			{
				list[0].SetNavigation(NavigationDirection.down, tabNav);
				list[0].UpdateSubmenuNavigation(tabNav);
				return;
			}
			list[0].SetNavigation(NavigationDirection.down, list[1].navigation);
			list[0].UpdateSubmenuNavigation(list[1].navigation);
			list[listField.Count - 1].SetNavigation(NavigationDirection.up, list[listField.Count - 2].navigation);
			list[listField.Count - 1].SetNavigation(NavigationDirection.down, tabNav);
			list[listField.Count - 1].UpdateSubmenuNavigation(tabNav);
			for (int i = 1; i < list.Count - 1; i++)
			{
				list[i].SetNavigation(NavigationDirection.up, list[i - 1].navigation);
				list[i].SetNavigation(NavigationDirection.down, list[i + 1].navigation);
				list[i].UpdateSubmenuNavigation(list[i + 1].navigation);
			}
		}

		private UIFriendItemView FindById(string p_id)
		{
			if (listField.list == null)
			{
				return null;
			}
			if (string.IsNullOrEmpty(p_id))
			{
				return null;
			}
			return listField.GetList<UIFriendItemView>().FirstOrDefault((UIFriendItemView o) => o.gameFriendData.platformId == p_id);
		}

		public void RepopulateList()
		{
			m_repopulateList = true;
			HideSubmenus();
		}

		public void Show()
		{
			RepopulateList();
			base.app.model.service.social.friends.RefreshFriendsAPI();
			if (listField.Count > 0)
			{
				UINavigation.focus = listField.Get<UIFriendItemView>(0).navigation;
			}
			else
			{
				UINavigation.focus = searchNav;
			}
			Notify("social.badges.clear", "friends");
			fade.FadeIn(0f);
			scroll.normalizedPosition = new Vector2(0.5f, 1f);
			searchInput.text = "";
			open = true;
		}

		public void Hide()
		{
			fade.FadeOut(0f);
			HideSubmenus();
			if ((bool)tabNav)
			{
				tabNav.up = null;
			}
			open = false;
		}

		public void HideSubmenus()
		{
			Notify("social.friend.item@close");
		}

		public void CloseInactiveSubmenus(UIFriendItemView p_activeItem)
		{
			foreach (UIFriendItemView item in listField.GetList<UIFriendItemView>())
			{
				if (!(item == p_activeItem))
				{
					item.SubmenuFold();
				}
			}
		}

		public void SetSearchResults(List<UIFriendItemView> p_results)
		{
			if (p_results.Count == 0)
			{
				ClearSearchResults();
				return;
			}
			List<UIFriendItemView> list = listField.GetList<UIFriendItemView>();
			if (p_results.Count != list.Count)
			{
				ClearSearchResults();
			}
			foreach (UIFriendItemView item in list)
			{
				if (!p_results.Contains(item))
				{
					item.gameObject.SetActive(value: false);
				}
			}
		}

		public void ClearSearchResults()
		{
			foreach (UIFriendItemView item in listField.GetList<UIFriendItemView>())
			{
				item.gameObject.SetActive(value: true);
			}
		}
	}
}

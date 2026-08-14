using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UINotificationView : View<DRLApp>
	{
		public GameObject notificationCardPrefab;

		[Header("Pop-Up:")]
		public UIElementView popUp;

		public FadeSlideComponent popUpFade;

		public Transform popUpList;

		public LayoutElement popUpPusher;

		public int popUpLimit = 4;

		public List<FadeComponent> snoozeFade;

		public GameObject controllerNavHeader;

		public GameObject xboxTriggerIcons;

		public GameObject ps4TriggerIcons;

		public FadeComponent xboxLTFade;

		public FadeComponent psLTFade;

		public FadeComponent xboxRTFade;

		public FadeComponent psRTFade;

		public FadeComponent headerBackFade;

		public FadeComponent headerNotificationsFade;

		[Header("History Board:")]
		public FadeSlideComponent historyFade;

		public ListComponent invitesList;

		public ListComponent newsList;

		public GameObject scrollInvites;

		public GameObject scrollNews;

		public DRLTabGroup historyTabs;

		public int historyLimit = 50;

		public GameObject feedback;

		private bool m_popUpActive;

		private UINavigation m_lastScreenNav;

		public bool popUpPanelVisible => popUpFade.transition >= 0f;

		public bool historyPanelVisible => historyFade.transition >= 0f;

		public bool focused { get; private set; }

		public bool popUpActive
		{
			get
			{
				m_popUpActive = m_popUpActive || popUp.over;
				foreach (Transform item in popUpList.transform)
				{
					UIElementView component = item.GetComponent<UIElementView>();
					if (!(component == null))
					{
						m_popUpActive = m_popUpActive || component.over;
					}
				}
				return m_popUpActive;
			}
			set
			{
				m_popUpActive = value;
			}
		}

		public bool footerVisible { get; set; }

		public UIElementView elementView => AssertLocal<UIElementView>("elementView");

		public UINavigation lastFocusElement { get; set; }

		public void PushNotification(NotificationData p_data)
		{
			if (!historyPanelVisible)
			{
				switch (p_data.type)
				{
				case NotificationTypeFlag.RoomInvite:
				case NotificationTypeFlag.QuickMatchInvite:
					AnimatePopUp(p_data);
					break;
				case NotificationTypeFlag.Tournament:
				case NotificationTypeFlag.Message:
					AnimatePopUp(p_data);
					break;
				}
			}
			if (historyPanelVisible && p_data.type != NotificationTypeFlag.Message)
			{
				AddToHistoryBoard(p_data, p_animate: true);
			}
		}

		private void AddToHistoryBoard(NotificationData p_data, bool p_animate = false)
		{
			if (p_data.type == NotificationTypeFlag.Message)
			{
				return;
			}
			ListComponent listComponent = ((p_data.type == NotificationTypeFlag.Information) ? newsList : invitesList);
			UINotificationCardView uINotificationCardView = ((!p_animate) ? listComponent.Push<UINotificationCardView>() : listComponent.Unshift<UINotificationCardView>());
			if (uINotificationCardView == null)
			{
				return;
			}
			uINotificationCardView.Set(p_data, p_popUp: false, base.app.model.notifications.gameInviteTTL);
			uINotificationCardView.Show(p_animate ? 0.3f : 0f);
			if (listComponent.Count > historyLimit)
			{
				UINotificationCardView uINotificationCardView2 = listComponent.Get<UINotificationCardView>(listComponent.Count - 1);
				if (uINotificationCardView2 == null)
				{
					return;
				}
				PopFromHistory(uINotificationCardView2, listComponent);
			}
			feedback.SetActive(value: false);
			RefreshUINavigation(p_isPopUp: false);
			if (historyPanelVisible)
			{
				UINavigation.Focus(uINotificationCardView);
			}
		}

		public void ToggleHistoryPanel()
		{
			if (historyPanelVisible)
			{
				HideHistoryPanel();
			}
			else
			{
				ShowHistoryPanel();
			}
		}

		public void ShowHistoryPanel(float p_duration = 0.3f)
		{
			if (historyPanelVisible)
			{
				return;
			}
			RefreshCards();
			SetNotificationBadge(p_enabled: false);
			feedback.SetActive(invitesList.Count == 0);
			if (popUpPanelVisible)
			{
				HidePopUpPanel(0.3f);
			}
			UISocialView social = base.app.view.ui.social;
			if (base.transform.GetSiblingIndex() < social.transform.GetSiblingIndex())
			{
				base.transform.SetSiblingIndex(social.transform.GetSiblingIndex());
			}
			if (DRLUINavigationSystem.lastNavigationDown != null && !DRLUINavigationSystem.lastNavigationDown.transform.IsChild(invitesList.transform))
			{
				m_lastScreenNav = (social.open ? social.GetLastNavigation() : DRLUINavigationSystem.lastNavigationDown);
			}
			if (social.open)
			{
				social.Hide(p_duration);
			}
			historyFade.FadeIn(p_duration);
			if (base.app.view.ui.screens.current != null)
			{
				UINavigationScroll component = base.app.view.ui.screens.current.GetComponent<UINavigationScroll>();
				if (component != null)
				{
					component.enabled = false;
				}
			}
			RefreshUINavigation(p_isPopUp: false);
			this.TimerRunOnce(delegate
			{
				if (invitesList.Count > 0)
				{
					UINavigation.Focus(invitesList);
				}
			}, 0.3f);
			focused = true;
		}

		public void HideHistoryPanel(float p_duration = 0.3f)
		{
			if (!historyPanelVisible)
			{
				return;
			}
			historyFade.Fade(0f, -1f, p_duration, 0f);
			if (base.app.view.ui.screens.current != null)
			{
				UINavigationScroll component = base.app.view.ui.screens.current.GetComponent<UINavigationScroll>();
				if (component != null)
				{
					component.enabled = true;
				}
				if (m_lastScreenNav != null)
				{
					UIFooterView.SetNavigationTop(m_lastScreenNav);
					DRLUINavigationSystem.lastNavigationDown = m_lastScreenNav;
					UINavigation.Focus(m_lastScreenNav);
				}
				else
				{
					UIFooterView.SetNavigationTop((base.app.view.ui.screens.current != null) ? base.app.view.ui.screens.current.transform : null);
				}
			}
			focused = false;
		}

		public void SetHistoryTab(int p_tab = 0)
		{
			historyTabs.index = p_tab;
			if (p_tab < historyTabs.tabs.Count)
			{
				UINavigation n = historyTabs.tabs[p_tab].GetComponent<UINavigation>();
				if (n != null)
				{
					this.TimerRunOnce(delegate
					{
						UINavigation.focus = n;
					}, 0.4f);
				}
			}
			RefreshCards();
		}

		private void RefreshCards()
		{
			invitesList.Clear();
			newsList.Clear();
			PlatformService platform = base.app.model.service.platform;
			List<NotificationData> notifications = base.app.model.notifications.list;
			for (int i = notifications.Count - 1; i >= 0; i--)
			{
				if (notifications[i] is InviteNotificationData)
				{
					InviteNotificationData ind = (InviteNotificationData)notifications[i];
					if (platform != null)
					{
						platform.IsUserCommunicationBlocked(ind.platformId, delegate
						{
							if (!(base.app.model.network != null) || base.app.model.network.room == null || !(base.app.model.network.room.Id == ind.inviteRoomId))
							{
								List<UINotificationCardView> list2 = invitesList.GetList<UINotificationCardView>();
								bool flag2 = false;
								foreach (UINotificationCardView item in list2)
								{
									if (item.data is InviteNotificationData && !item.inactive && ((InviteNotificationData)item.data).inviteRoomId == ind.inviteRoomId)
									{
										flag2 = true;
										break;
									}
								}
								if (!flag2)
								{
									AddToHistoryBoard(notifications[i]);
								}
							}
						});
					}
				}
				if (!(notifications[i] is TournamentNotificationData))
				{
					continue;
				}
				TournamentNotificationData tournamentNotificationData = (TournamentNotificationData)notifications[i];
				if (tournamentNotificationData.status == TournamentNotificationType.Started && !tournamentNotificationData.isParticipant)
				{
					continue;
				}
				List<UINotificationCardView> list = invitesList.GetList<UINotificationCardView>();
				bool flag = false;
				foreach (UINotificationCardView item2 in list)
				{
					if (item2.data is TournamentNotificationData && !item2.inactive && ((TournamentNotificationData)item2.data).tournamentGuid == tournamentNotificationData.tournamentGuid)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					AddToHistoryBoard(notifications[i]);
				}
			}
		}

		public void OnTabChanged(int p_tab)
		{
			scrollInvites.SetActive(p_tab == 0);
			scrollNews.SetActive(p_tab != 0);
			RefreshCards();
		}

		private void AnimatePopUp(NotificationData p_data)
		{
			ShowPopUpPanel();
			Object.Instantiate(notificationCardPrefab, popUpList).GetComponent<UINotificationCardView>().Set(p_data, p_popUp: true);
			RefreshUINavigation(p_isPopUp: true);
			popUpPusher.transform.SetAsLastSibling();
			popUpPusher.preferredHeight = ((popUpList.childCount > 1) ? 140f : 0f);
			Tween.Kill(popUpPusher, "preferredHeight");
			Tween.Add(popUpPusher, "preferredHeight", 290f, 0.4f, Cubic.Out);
			if (popUpList.childCount - 2 > popUpLimit)
			{
				UINotificationCardView component = popUpList.GetChild(1).GetComponent<UINotificationCardView>();
				if (component != null)
				{
					component.Hide(0.3f);
					if (component.selected)
					{
						FocusPopup(p_updateLastNavigation: false, component);
					}
					Object.Destroy(component.gameObject, 0.4f);
				}
			}
			popUpActive = true;
			this.TimerRunOnce(delegate
			{
				popUpActive = false;
			}, 0.3f);
		}

		private void RefreshUINavigation(bool p_isPopUp)
		{
			UINotificationCardView[] array = (p_isPopUp ? popUpList.GetComponentsInChildren<UINotificationCardView>() : invitesList.transform.GetComponentsInChildren<UINotificationCardView>());
			if (array == null || array.Length == 0)
			{
				return;
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (i < array.Length - 1)
				{
					array[i].navigation.down = array[i + 1];
				}
				if (i > 0)
				{
					array[i].navigation.up = array[i - 1];
				}
			}
			if (p_isPopUp)
			{
				array[array.Length - 1].navigation.down = snoozeFade[0].GetComponent<UINavigation>();
				for (int j = 0; j < snoozeFade.Count; j++)
				{
					snoozeFade[j].GetComponent<UINavigation>().up = array[array.Length - 1].navigation;
				}
			}
		}

		public void TogglePopUpPanel()
		{
			if (popUpPanelVisible)
			{
				HidePopUpPanel(0.3f);
			}
			else
			{
				ShowPopUpPanel();
			}
		}

		public void ShowPopUpPanel(float p_duration = 0f)
		{
			if (!popUpPanelVisible)
			{
				bool flag = !RCI.IsRCController() && RCI.GetActiveJoystick() != null;
				controllerNavHeader.SetActive(flag);
				if (flag)
				{
					bool flag2 = RCI.GetDefaultControllerType(DefaultControllerType.XBox) == DefaultControllerType.PS;
					xboxTriggerIcons.SetActive(!flag2);
					ps4TriggerIcons.SetActive(flag2);
				}
				if (base.app.model.game != null)
				{
					popUpFade.from.y = (footerVisible ? 0f : (-70f));
					popUpFade.center.y = (footerVisible ? 0f : (-70f));
					popUpFade.to.y = (footerVisible ? 0f : (-70f));
				}
				HilightHeaderBack(p_flag: false);
				popUpFade.FadeIn(p_duration);
				base.app.view.ui.navigation.enabled = true;
			}
		}

		public void HidePopUpPanel(float p_duration = 0f)
		{
			ClearIgnoredCommands();
			if (popUpPanelVisible)
			{
				popUpFade.Fade(0f, -1f, p_duration, 0f);
				if (lastFocusElement != null)
				{
					UINavigation.Focus(lastFocusElement);
				}
				HilightHeaderBack(p_flag: false);
				focused = false;
				if (base.app.inGame && base.app.view.ui.screens.current == null)
				{
					base.app.view.ui.navigation.enabled = false;
				}
			}
		}

		internal void FocusPopup(bool p_updateLastNavigation = true, UINotificationCardView p_ignore = null)
		{
			if (!popUpPanelVisible || !HasActiveInvites())
			{
				return;
			}
			if (p_updateLastNavigation)
			{
				lastFocusElement = UINavigation.focus;
			}
			bool flag = false;
			foreach (Transform popUp in popUpList)
			{
				UINotificationCardView component = popUp.GetComponent<UINotificationCardView>();
				if (!(component == null) && (!(p_ignore != null) || !(p_ignore == component)))
				{
					flag = true;
					UINavigation.Focus(component.navigation);
					break;
				}
			}
			focused = flag;
			if (!flag)
			{
				UINavigation.Focus(lastFocusElement);
			}
		}

		public void RemoveFromHistory(string id)
		{
			UINotificationCardView uINotificationCardView = null;
			int num = 0;
			for (int i = 0; i < invitesList.Count; i++)
			{
				if (id == invitesList.Get<UINotificationCardView>(i).id)
				{
					uINotificationCardView = invitesList.Get<UINotificationCardView>(i);
					break;
				}
				num++;
			}
			if (!(uINotificationCardView == null))
			{
				base.app.model.notifications.Remove(uINotificationCardView.data);
				invitesList.Remove(num);
				if (invitesList.Count > 0)
				{
					UINavigation.Focus(invitesList);
					return;
				}
				feedback.SetActive(value: true);
				UINavigation.Focus(base.app.view.ui.footer.notificationsButton);
				HideHistoryPanel();
			}
		}

		public void PopFromHistory(UINotificationCardView p_nv, ListComponent p_list)
		{
			if (!(p_nv == null))
			{
				base.app.model.notifications.Remove(p_nv.data);
				p_list.Pop();
			}
		}

		public void RemoveFromPopUp(string id, bool p_delete = true)
		{
			foreach (Transform popUp in popUpList)
			{
				UINotificationCardView component = popUp.GetComponent<UINotificationCardView>();
				if (component == null || !(component.id == id))
				{
					continue;
				}
				if (p_delete)
				{
					base.app.model.notifications.Remove(component.data);
					if (base.app.model.notifications.list.Count == 0)
					{
						SetNotificationBadge(p_enabled: false);
					}
				}
				if (component.selected)
				{
					FocusPopup(p_updateLastNavigation: false, component);
				}
				Object.Destroy(component.gameObject);
				break;
			}
		}

		public void ResetSnoozeFade()
		{
			foreach (FadeComponent item in snoozeFade)
			{
				item.Fade(0.4f, 0f);
			}
		}

		public void SetNotificationBadge(bool p_enabled = true)
		{
			base.app.view.ui.footer.notificationStripe.color = (p_enabled ? Color.red : Color.white);
		}

		public void SetIgnoredGameCommands()
		{
			if (!base.validContext || !base.app.inGame || base.app.model.game.simulation == null)
			{
				return;
			}
			base.app.model.game.simulation.SetDroneTransmitter(p_active: false);
			List<GameCommand> list = new List<GameCommand>();
			if (!base.app.controller)
			{
				return;
			}
			foreach (GameInputMapComponent map in base.app.controller.game.input.maps)
			{
				foreach (GameCommand command in map.commands)
				{
					if (command.type != GameCommandType.Pause)
					{
						list.Add(command);
					}
				}
			}
			base.app.controller.game.input.SetIgnoredCommands(list);
		}

		public void ClearIgnoredCommands()
		{
			if (base.validContext && base.app.inGame && !base.app.inGarage && !(base.app.model.game.simulation == null))
			{
				base.app.model.game.simulation.SetDroneTransmitter();
				if ((bool)base.app.controller)
				{
					base.app.controller.game.input.ClearIgnoredCommands();
				}
				focused = false;
			}
		}

		public void HilightHeaderBack(bool p_flag)
		{
			headerBackFade.Fade(p_flag ? 1f : 0.15f, 0f);
			headerNotificationsFade.Fade(p_flag ? 0.15f : 1f, 0f);
			xboxLTFade.Fade(p_flag ? 1f : 0.15f, 0f);
			psLTFade.Fade(p_flag ? 1f : 0.15f, 0f);
			xboxRTFade.Fade(p_flag ? 0.15f : 1f, 0f);
			psRTFade.Fade(p_flag ? 0.15f : 1f, 0f);
		}

		public bool HasActiveInvites(bool p_popUp = true)
		{
			return (p_popUp ? popUpList : invitesList.transform).GetComponentsInChildren<UINotificationCardView>().Length != 0;
		}

		public UINavigation GetLastNavigation()
		{
			if (!popUpPanelVisible)
			{
				return m_lastScreenNav;
			}
			return lastFocusElement;
		}
	}
}

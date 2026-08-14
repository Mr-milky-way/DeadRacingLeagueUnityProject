using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.network;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIMultiplayerLobbyView : UIScreenView
	{
		public ListComponent listField;

		public DRLIntStepperView serverStepper;

		public DRLIntStepperView gameTypeStepper;

		public DRLInputFieldView searchInput;

		public FadeComponent feedbackFade;

		public FadeComponent listFade;

		public List<GameObject> feedbacks;

		public Text feedbackPlayersInQueueCount;

		public Text feedbackRaceStartsInTime;

		public Text feedbackRaceLockedWithPlayersCount;

		public GameObject quickMatchHeadersContainer;

		public Text quickMatchQuickFreestyleLabel;

		public Text quickMatchQuickRaceLabel;

		public GameObject backButtonNav;

		public GameObject exitQMButtonNav;

		public GameObject lanContainer;

		public FadeComponent lanCreateServerBtn;

		public FadeComponent lanStopServerBtn;

		public GameObject lanSepAfterBtn;

		public FadeComponent lanConnectBtn;

		public FadeComponent lanDisconnectBtn;

		public DRLInputFieldView lanCreateIpInput;

		public GameObject lanServerIp;

		public Text lanServerIpField;

		public UINavigationLinkList listNavLinks;

		public UINavigationLinkList menuNavLinks;

		public RawImage[] qualityStateFields;

		public Text qualityMsField;

		public DRLPagePickerView pageField;

		public FadeComponent createRoomBtn;

		public FadeComponent serverStepperFader;

		public FadeComponent gameTypeStepperFade;

		public FadeComponent searchInputFade;

		public FadeComponent quickRaceFade;

		public FadeComponent quickFreestyleFade;

		[Header("Top")]
		public GameObject topContainer;

		public GameObject topFeedback;

		public Text topFeedbackField;

		public UILobbyFeedbackType status;

		public List<Lobby.NetworkRoomInfo> rooms;

		public bool connecting => status == UILobbyFeedbackType.Connecting;

		public GameFlag gameType => gameTypeStepper.value switch
		{
			0 => GameFlag.Freestyle, 
			1 => GameFlag.Race, 
			2 => GameFlag.Campaign, 
			_ => GameFlag.Freestyle, 
		};

		public void Clear()
		{
			listField.Clear();
			listField.gameObject.SetActive(value: false);
			rooms = new List<Lobby.NetworkRoomInfo>();
		}

		public void Update()
		{
		}

		public void UpdateList(List<Lobby.NetworkRoomInfo> p_rooms, int p_page, int p_page_length)
		{
			List<Lobby.NetworkRoomInfo> collection = ((p_rooms == null) ? new List<Lobby.NetworkRoomInfo>() : p_rooms);
			collection = new List<Lobby.NetworkRoomInfo>(collection);
			collection.Sort(delegate(Lobby.NetworkRoomInfo a, Lobby.NetworkRoomInfo b)
			{
				if ((a.CanRace || a.CanSpectate) && !b.CanRace && !b.CanSpectate)
				{
					return -1;
				}
				if ((b.CanRace || b.CanSpectate) && !a.CanRace && !a.CanSpectate)
				{
					return 1;
				}
				if (a.Progress > b.Progress)
				{
					return -1;
				}
				return (a.Progress < b.Progress) ? 1 : 0;
			});
			List<Lobby.NetworkRoomInfo> list = new List<Lobby.NetworkRoomInfo>();
			collection.RemoveAll(delegate(Lobby.NetworkRoomInfo p_it)
			{
				string text = searchInput.field.text;
				text = text.Trim().ToLower();
				if (string.IsNullOrEmpty(text))
				{
					return false;
				}
				if (p_it == null)
				{
					return true;
				}
				return p_it.RoomTitle == null || !p_it.RoomTitle.ToLower().Contains(text);
			});
			collection.RemoveAll((Lobby.NetworkRoomInfo r) => r.MasterPlatform.Equals("playstation") && !r.Crossplay);
			int num = ((p_page_length > 0) ? ((collection.Count - 1) / p_page_length) : 0) + 1;
			int num2 = Mathf.Clamp(p_page, 0, num - 1);
			int num3 = Mathf.Max(0, num2 * p_page_length);
			for (int num4 = 0; num4 < p_page_length; num4++)
			{
				if (num3 >= collection.Count)
				{
					break;
				}
				Lobby.NetworkRoomInfo item = collection[num3];
				list.Add(item);
				num3++;
			}
			Debug.Log("UIMultiplayerLobbyView> UpdateList - total[" + collection.Count + "] page[" + num2 + "] total-pages[" + num + "] elements[" + list.Count + "]");
			List<Lobby.NetworkRoomInfo> list2 = new List<Lobby.NetworkRoomInfo>();
			List<Lobby.NetworkRoomInfo> list3 = new List<Lobby.NetworkRoomInfo>();
			for (int num5 = 0; num5 < list.Count; num5++)
			{
				if (!ContainsRoom(rooms, list[num5]) && list[num5].GameMode != NetworkRoom.GameType.Tournament)
				{
					list2.Add(list[num5]);
				}
			}
			for (int num6 = 0; num6 < rooms.Count; num6++)
			{
				if (!ContainsRoom(list, rooms[num6]))
				{
					list3.Add(rooms[num6]);
				}
			}
			Debug.Log("UIMultiplayerLobbyView> UpdateList - add[" + list2.Count + "] remove[" + list3.Count + "]");
			for (int num7 = 0; num7 < list3.Count; num7++)
			{
				RemoveRoom(list3[num7]);
			}
			for (int num8 = 0; num8 < list2.Count; num8++)
			{
				if (rooms.Count < p_page_length)
				{
					AddRoom(list2[num8]);
				}
			}
			for (int num9 = 0; num9 < list.Count; num9++)
			{
				int roomIndex = GetRoomIndex(list[num9]);
				if (roomIndex >= 0)
				{
					rooms[roomIndex] = list[num9];
				}
			}
			for (int num10 = 0; num10 < rooms.Count; num10++)
			{
				UpdateRoom(rooms[num10]);
			}
			FadeComponent fade = pageField.fade;
			if (fade.alpha < 0f)
			{
				fade.alpha = 0f;
			}
			if (num > 1)
			{
				fade.FadeIn(0.3f);
			}
			else
			{
				fade.FadeOut(0.3f);
			}
			pageField.Set(num);
			pageField.index = num2;
			UpdateNavigation();
			UILobbyFeedbackType p_type = ((rooms.Count > 0) ? UILobbyFeedbackType.None : UILobbyFeedbackType.NoRoom);
			SetFeedback(p_type, p_hide_list: true, 0.1f);
		}

		protected void UpdateNavigation()
		{
			ListComponent listComponent = listField;
			List<UINavigation> list = new List<UINavigation>();
			List<UINavigation> list2 = new List<UINavigation>();
			UINavigation component = pageField.GetComponent<UINavigation>();
			for (int i = 0; i < listComponent.Count; i++)
			{
				UIMultiplayerLobbyItemView uIMultiplayerLobbyItemView = listComponent.Get<UIMultiplayerLobbyItemView>(i);
				list.Add(uIMultiplayerLobbyItemView.entryNav);
				if (uIMultiplayerLobbyItemView.publicJoin.activeInHierarchy)
				{
					list2.Add(uIMultiplayerLobbyItemView.actionNav);
				}
				else
				{
					list2.Add(uIMultiplayerLobbyItemView.privateJoinNavigationProxy);
				}
				if (i >= listComponent.Count - 1)
				{
					component.up = uIMultiplayerLobbyItemView.entryNav;
				}
			}
			UINavigation uINavigation = null;
			if (lanContainer.activeInHierarchy)
			{
				uINavigation = lanContainer.GetComponentInChildren<UINavigation>();
			}
			if (!uINavigation)
			{
				uINavigation = gameTypeStepper.GetComponent<UINavigation>();
			}
			bool flag = pageField.gameObject.activeInHierarchy && pageField.listField.Count > 1;
			UINavigation.Link(list.ToArray(), 0, p_vertical: true, base.leftNavigation, null, uINavigation, flag ? component : null);
			UINavigation.Link(list2.ToArray(), 0, p_vertical: true, null, null, uINavigation, flag ? component : null);
			if (list.Count > 0)
			{
				UINavigation component2 = createRoomBtn.GetComponent<UINavigation>();
				UINavigation component3 = searchInputFade.GetComponent<UINavigation>();
				component2.down = list[0];
				component3.down = list2[0];
				list2[0].up = component3;
				list[0].up = component3;
			}
			int count = list.Count;
			for (int j = 0; j < count; j++)
			{
				UIMultiplayerLobbyItemView uIMultiplayerLobbyItemView2 = listComponent.Get<UIMultiplayerLobbyItemView>(j);
				UINavigation uINavigation2 = list[j];
				UINavigation uINavigation3 = (UINavigation)(uINavigation2.right = list2[j]);
				uINavigation3.left = uINavigation2;
				uIMultiplayerLobbyItemView2.privateJoinPasswordInput.GetComponent<UINavigation>().up = list2[j].up;
				uIMultiplayerLobbyItemView2.privateJoinButtonContent.GetComponent<UINavigation>().up = list2[j].up;
			}
			for (int k = 0; k < list2.Count; k++)
			{
				if (k >= 0 && k + 1 < list2.Count)
				{
					bool flag2 = list2[k] is UINavigationLinkList;
					bool flag3 = list2[k + 1] is UINavigationLinkList;
					_ = list2[k];
					UINavigationLinkList uINavigationLinkList = list2[k + 1] as UINavigationLinkList;
					if (flag2)
					{
						if (flag3)
						{
							listComponent.Get<UIMultiplayerLobbyItemView>(k).privateRoomJoin.GetComponent<UINavigation>().down = uINavigationLinkList;
							listComponent.Get<UIMultiplayerLobbyItemView>(k).privateJoinButtonContent.GetComponent<UINavigation>().down = uINavigationLinkList;
						}
						else
						{
							listComponent.Get<UIMultiplayerLobbyItemView>(k).privateRoomJoin.GetComponent<UINavigation>().down = list2[k + 1];
							listComponent.Get<UIMultiplayerLobbyItemView>(k).privateJoinButtonContent.GetComponent<UINavigation>().down = list2[k + 1];
						}
					}
					if (!flag2)
					{
						if (flag3)
						{
							if (uINavigationLinkList != null)
							{
								list2[k].transform.GetComponent<UINavigation>().down = uINavigationLinkList.GetComponent<UINavigation>();
							}
						}
						else
						{
							list2[k].GetComponent<UINavigation>().down = list2[k + 1].GetComponent<UINavigation>();
						}
					}
					if (!flag3 && !flag2)
					{
						list2[k].GetComponent<UINavigation>().down = list2[k + 1].GetComponent<UINavigation>();
					}
				}
				list2[k].left = list[k];
				list[k].right = list2[k];
			}
		}

		public void UpdateList(Lobby p_lobby, int p_page, int p_count)
		{
			UpdateList(p_lobby.Rooms, p_page, p_count);
		}

		public void AddRoom(Lobby.NetworkRoomInfo p_data)
		{
			rooms.Add(p_data);
			listField.Push<UIMultiplayerLobbyItemView>().Set(p_data);
			listField.gameObject.SetActive(listField.Count > 0);
		}

		public void RemoveRoom(Lobby.NetworkRoomInfo p_data)
		{
			for (int i = 0; i < rooms.Count; i++)
			{
				if (rooms[i].Name == p_data.Name)
				{
					rooms.RemoveAt(i);
					break;
				}
			}
			for (int j = 0; j < listField.Count; j++)
			{
				UIMultiplayerLobbyItemView uIMultiplayerLobbyItemView = listField.Get<UIMultiplayerLobbyItemView>(j);
				if ((bool)uIMultiplayerLobbyItemView && uIMultiplayerLobbyItemView.data.Name == p_data.Name)
				{
					listField.Remove(j);
					break;
				}
			}
			listField.gameObject.SetActive(listField.Count > 0);
		}

		public void UpdateRoom(Lobby.NetworkRoomInfo p_data)
		{
			UIMultiplayerLobbyItemView byRoomById = GetByRoomById(p_data.Name);
			if ((bool)byRoomById)
			{
				byRoomById.Set(p_data);
			}
		}

		public UIMultiplayerLobbyItemView GetByRoomById(string p_id)
		{
			for (int i = 0; i < listField.Count; i++)
			{
				UIMultiplayerLobbyItemView uIMultiplayerLobbyItemView = listField.Get<UIMultiplayerLobbyItemView>(i);
				if (uIMultiplayerLobbyItemView.data != null && uIMultiplayerLobbyItemView.data.Name == p_id)
				{
					return uIMultiplayerLobbyItemView;
				}
			}
			return null;
		}

		public bool ContainsRoom(List<Lobby.NetworkRoomInfo> p_list, Lobby.NetworkRoomInfo p_room)
		{
			if (p_room == null)
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
				if (p_list[i].Name == p_room.Name)
				{
					return true;
				}
			}
			return false;
		}

		public int GetRoomIndex(Lobby.NetworkRoomInfo p_room)
		{
			for (int i = 0; i < rooms.Count; i++)
			{
				if (rooms[i].Name == p_room.Name)
				{
					return i;
				}
			}
			return -1;
		}

		public void SetFeedback(UILobbyFeedbackType p_type, bool p_hide_list, float p_delay)
		{
			float feedback_alpha = ((p_type == UILobbyFeedbackType.None) ? (-0.1f) : 1f);
			float content_alpha = ((p_type == UILobbyFeedbackType.None) ? 1f : (p_hide_list ? (-0.1f) : 1f));
			status = p_type;
			Action action = delegate
			{
				feedbackFade.Fade(feedback_alpha, 0.3f, 0.05f, Cubic.Out);
				listFade.Fade(content_alpha, 0.3f, 0f, Cubic.Out);
				if (p_type != UILobbyFeedbackType.None)
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
			bool flag = p_type == UILobbyFeedbackType.Connecting || p_type == UILobbyFeedbackType.CreatingRoom || p_type == UILobbyFeedbackType.CreatingServer || p_type == UILobbyFeedbackType.StoppingServer || p_type == UILobbyFeedbackType.SearchingMatches || p_type == UILobbyFeedbackType.WaitingForPlayers || p_type == UILobbyFeedbackType.PlayersInQueue || p_type == UILobbyFeedbackType.RaceLockedWithPlayers;
			EnableCreateRoomButton(!flag);
			EnableGameTypeStepperButton(!flag);
			EnableSearchInputBox(!flag);
			EnableLanCreateServerButton(!flag);
			EnableLanStopServerButton(!flag);
			EnableLanConnectButton(!flag);
			EnableLanDisconnectButton(!flag);
			EnableQuickMatchButtons(!flag);
			if (flag)
			{
				Notify("network.footer@disable");
				Notify("network.lobby.server-list@disable");
			}
			else
			{
				Notify("network.footer@enable");
				Notify("network.lobby.server-list@enable");
			}
		}

		public void EnableCreateRoomButton(bool p_enable)
		{
			createRoomBtn.Fade(p_enable ? 1f : 0.1f, 0f);
			UIElementView component = createRoomBtn.gameObject.GetComponent<UIElementView>();
			if ((bool)component)
			{
				component.enabled = p_enable;
			}
		}

		public void EnableGameTypeStepperButton(bool p_enable)
		{
			gameTypeStepperFade.Fade(p_enable ? 1f : 0.1f, 0f);
			gameTypeStepper.enabled = p_enable;
		}

		public void EnableSearchInputBox(bool p_enable)
		{
			searchInput.enabled = p_enable;
			searchInputFade.Fade(p_enable ? 1f : 0.1f, 0f);
		}

		public void EnableLanCreateServerButton(bool p_enable)
		{
			lanCreateServerBtn.Fade(p_enable ? 1f : 0.1f, 0f);
			UIElementView component = lanCreateServerBtn.gameObject.GetComponent<UIElementView>();
			if ((bool)component)
			{
				component.enabled = p_enable;
			}
		}

		public void ShowLanCreateServerButton(bool p_show)
		{
			lanCreateServerBtn.gameObject.SetActive(p_show);
			lanSepAfterBtn.gameObject.SetActive(p_show);
		}

		public void EnableLanStopServerButton(bool p_enable)
		{
			lanStopServerBtn.Fade(p_enable ? 1f : 0.1f, 0f);
			UIElementView component = lanStopServerBtn.gameObject.GetComponent<UIElementView>();
			if ((bool)component)
			{
				component.enabled = p_enable;
			}
		}

		public void ShowLanStopServerButton(bool p_show)
		{
			lanStopServerBtn.gameObject.SetActive(p_show);
			lanSepAfterBtn.gameObject.SetActive(p_show);
		}

		public void EnableLanConnectButton(bool p_enable)
		{
			lanConnectBtn.Fade(p_enable ? 1f : 0.1f, 0f);
			UIElementView component = lanConnectBtn.gameObject.GetComponent<UIElementView>();
			if ((bool)component)
			{
				component.enabled = p_enable;
			}
		}

		public void ShowLanConnectButton(bool p_show)
		{
			lanConnectBtn.gameObject.SetActive(p_show);
		}

		public void EnableLanDisconnectButton(bool p_enable)
		{
			lanDisconnectBtn.Fade(p_enable ? 1f : 0.1f, 0f);
			UIElementView component = lanDisconnectBtn.gameObject.GetComponent<UIElementView>();
			if ((bool)component)
			{
				component.enabled = p_enable;
			}
		}

		public void EnableQuickMatchButtons(bool p_enable)
		{
			if ((bool)quickFreestyleFade)
			{
				quickFreestyleFade.Fade(p_enable ? 1f : 0.1f, 0f);
				UIElementView component = quickFreestyleFade.gameObject.GetComponent<UIElementView>();
				if ((bool)component)
				{
					component.enabled = p_enable;
				}
			}
			if ((bool)quickRaceFade)
			{
				quickRaceFade.Fade(p_enable ? 1f : 0.1f, 0f);
				UIElementView component2 = quickRaceFade.gameObject.GetComponent<UIElementView>();
				if ((bool)component2)
				{
					component2.enabled = p_enable;
				}
			}
		}

		public void ShowLanDisconnectButton(bool p_show)
		{
			lanDisconnectBtn.gameObject.SetActive(p_show);
		}

		public void ShowLanCreateIpInput(bool p_show)
		{
			lanCreateIpInput.gameObject.SetActive(p_show);
		}

		public void ShowLanServerIpLabel(bool p_show, string p_string)
		{
			lanServerIp.gameObject.SetActive(p_show);
			if (p_string != null)
			{
				lanServerIpField.text = p_string;
			}
			else
			{
				lanServerIpField.text = "";
			}
		}

		public void ShowLanControls(bool p_show)
		{
			lanContainer.SetActive(p_show);
		}

		public bool IsLanControlsVisible()
		{
			return lanContainer.gameObject.activeInHierarchy;
		}

		public void SetFeedback(UILobbyFeedbackType p_type, bool p_hide_list)
		{
			SetFeedback(p_type, p_hide_list, 0f);
		}

		public void SetFeedback(UILobbyFeedbackType p_type)
		{
			SetFeedback(p_type, p_hide_list: true, 0f);
		}

		public void SetInQuickMatchMode(bool p_inQuickMatch, bool p_isRace = true)
		{
			EnableQuickMatchButtons(!p_inQuickMatch);
			EnableCreateRoomButton(!p_inQuickMatch);
			EnableGameTypeStepperButton(!p_inQuickMatch);
			EnableSearchInputBox(!p_inQuickMatch);
			if (p_inQuickMatch)
			{
				Notify("network.lobby.server-list@disable");
			}
			else
			{
				Notify("network.lobby.server-list@enable");
			}
			if ((bool)backButtonNav)
			{
				backButtonNav.SetActive(!p_inQuickMatch);
			}
			if ((bool)exitQMButtonNav)
			{
				exitQMButtonNav.SetActive(p_inQuickMatch);
			}
			if ((bool)quickMatchHeadersContainer)
			{
				quickMatchHeadersContainer.SetActive(p_inQuickMatch);
			}
			if ((bool)quickMatchQuickFreestyleLabel)
			{
				quickMatchQuickFreestyleLabel.gameObject.SetActive(!p_isRace);
			}
			if ((bool)quickMatchQuickRaceLabel)
			{
				quickMatchQuickRaceLabel.gameObject.SetActive(p_isRace);
			}
			if (p_inQuickMatch)
			{
				SetFeedback(UILobbyFeedbackType.WaitingForPlayers, p_hide_list: true);
				return;
			}
			base.app.view.ui.screenBack = true;
			UINavigation.focus = backButtonNav.GetComponent<UINavigation>();
			SetFeedback(UILobbyFeedbackType.None, p_hide_list: false);
		}

		public void RefreshQuickMatchFeedback(int p_playersCount, int p_playersMax, int p_timeSec)
		{
			if ((bool)feedbackPlayersInQueueCount)
			{
				feedbackPlayersInQueueCount.text = p_playersCount.ToString();
			}
			if (p_timeSec < 0)
			{
				p_timeSec = 0;
			}
			if ((bool)feedbackRaceStartsInTime)
			{
				feedbackRaceStartsInTime.text = p_timeSec.ToString();
			}
			if ((bool)feedbackRaceLockedWithPlayersCount)
			{
				feedbackRaceLockedWithPlayersCount.text = p_playersCount.ToString();
			}
			if ((bool)exitQMButtonNav)
			{
				bool flag = p_playersCount < 2;
				exitQMButtonNav.SetActive(flag);
				if (flag)
				{
					UINavigation.focus = exitQMButtonNav.GetComponent<UINavigation>();
				}
				base.app.view.ui.screenBack = flag;
			}
			if (p_playersCount >= 2)
			{
				SetFeedback(UILobbyFeedbackType.PlayersInQueue, p_hide_list: true);
			}
			else
			{
				SetFeedback(UILobbyFeedbackType.WaitingForPlayers, p_hide_list: true);
			}
			if (p_timeSec == 0 || p_playersCount == p_playersMax)
			{
				SetFeedback(UILobbyFeedbackType.RaceLockedWithPlayers, p_hide_list: true);
			}
		}
	}
}

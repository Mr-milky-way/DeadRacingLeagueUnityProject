using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using drl.chat;
using drl.network;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIChatController : Controller<DRLApp>
	{
		private const string NO_THREADS_FOUND = "START PRIVATE CONVERSATIONS WITH YOUR FRIENDS";

		private const string NOT_IN_A_GAME = "NOT IN A GAME";

		private float m_keyspeed;

		private int m_spammingMsgCount;

		private float m_spammingTimer;

		private string mActiveChannel = "global-chat";

		private ChatRoom.Scope mActiveChannelScope;

		private string mCurrentThreadId;

		private string mPendingStartPrivateChatUserName;

		private string mPendingStartPrivateChatPhotoURL;

		private Color mPendingStartPrivateChatColor;

		private bool mHandleScroll;

		private bool mIsReady;

		private bool m_gameTemplate;

		private bool m_foundAutocomplete;

		private bool m_waitForNewChar;

		private int m_privateAutoLength;

		public NetworkModel roomChatModel => base.app.model.network;

		public ChatModel chatModel => base.app.model.chat;

		public SocialModel socialModel => base.app.model.service.social;

		public UIChatView view => AssertLocal<UIChatView>("view");

		protected override void Start()
		{
			base.Start();
			mIsReady = true;
			m_gameTemplate = view.social.useGameTemplate;
		}

		public override async void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (!mIsReady)
			{
				return;
			}
			Localization locale = base.app.model.storage.locale;
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "chat.block-user@click":
				view.messagesList.Clear();
				view.LoadMessages();
				break;
			case "chat.unblock-user@click":
				view.messagesList.Clear();
				view.LoadMessages();
				break;
			case "chat.server.disconnected":
			{
				string p_serviceMsg2 = locale.Get("social.label.disconnected", "DISCONNECTED");
				view.Lock(p_enable: true, p_serviceMsg2);
				view.Clear();
				break;
			}
			case "chat.server.connected":
				view.Clear();
				break;
			case "chat.server.connecting":
			{
				string p_serviceMsg = locale.Get("social.label.connecting", "CONNECTING...");
				view.Lock(p_enable: true, p_serviceMsg);
				break;
			}
			case "chat.room.joined":
				view.Clear();
				view.Lock(p_enable: false);
				view.SetPrivateChannels();
				break;
			case "network.room@enter":
				view.Clear();
				view.SetDefaultChannel("room-chat");
				view.Lock(p_enable: false);
				break;
			case "network.room@exit":
				view.Clear();
				view.SetDefaultChannel("global-chat");
				break;
			case "social.panel.hidden":
				MarkCurrentChannelAsRead();
				break;
			case "chat.panel@click":
				if (!view.social.useGameTemplate)
				{
					EnableChatScrolling(p_enable: true);
				}
				break;
			case "chat.channel@click":
				view.CycleChannels();
				break;
			case "chat.message.input@change":
			{
				if (!view.social.isActive)
				{
					break;
				}
				DRLInputFieldView dRLInputFieldView3 = p_target as DRLInputFieldView;
				if (!(dRLInputFieldView3 == null))
				{
					if (view.IsInputWaitLabelEnabled())
					{
						dRLInputFieldView3.field.text = "";
					}
					view.inputWriteLabel.gameObject.SetActive(string.IsNullOrEmpty(dRLInputFieldView3.text));
					if (dRLInputFieldView3.text.StartsWith("@"))
					{
						view.SetPrivateDefaultActive();
						dRLInputFieldView3.text = "";
						dRLInputFieldView3.field.DeactivateInputField();
						view.privateInput.Activate();
					}
					else if ((bool)base.app.model.chat)
					{
						base.app.model.chat.LastMessage = dRLInputFieldView3.text;
					}
				}
				break;
			}
			case "chat.private.invite":
				if (view.social.isActive && p_data.Length >= 2)
				{
					string text = (string)p_data[0];
					if (view.IsOnline(text))
					{
						string channelActive = "private-chat " + text;
						view.SetChannelActive(channelActive);
						UINavigation.Focus(view.input);
					}
				}
				break;
			case "chat.message.private.input@change":
				if (view.social.isActive)
				{
					DRLInputFieldView dRLInputFieldView2 = p_target as DRLInputFieldView;
					if (!(dRLInputFieldView2 == null))
					{
						AutoCompletePrivateChannel(dRLInputFieldView2);
					}
				}
				break;
			case "chat.message.private.input@end-edit":
			{
				if (!view.social.isActive)
				{
					break;
				}
				DRLInputFieldView dRLInputFieldView4 = p_target as DRLInputFieldView;
				if (dRLInputFieldView4 == null || !dRLInputFieldView4.isActiveAndEnabled)
				{
					break;
				}
				string pcs = dRLInputFieldView4.field.text;
				pcs = pcs.Trim();
				dRLInputFieldView4.text = dRLInputFieldView4.text.ToUpper();
				if (!string.IsNullOrEmpty(pcs) && !pcs.EndsWith("]"))
				{
					dRLInputFieldView4.text += "]";
					pcs += "]";
				}
				view.playerNotFoundField.SetActive(value: false);
				this.TimerRunOnce(delegate
				{
					if (!view.SetActiveAutocompleteChannel(pcs) && !string.IsNullOrEmpty(pcs))
					{
						view.playerNotFoundField.SetActive(value: true);
						view.RefreshPrivateChannel();
						this.TimerRunOnce(delegate
						{
							view.playerNotFoundField.SetActive(value: false);
						}, 1.5f);
					}
					view.privateInput.field.DeactivateInputField();
					view.input.Activate();
				}, 0.3f);
				break;
			}
			case "chat.message.input@end-edit":
			{
				if (!view.social.isActive)
				{
					break;
				}
				if (!view.social.useGameTemplate)
				{
					view.verticalScrollbar.value = 0f;
				}
				DRLInputFieldView dRLInputFieldView = p_target as DRLInputFieldView;
				if (dRLInputFieldView == null)
				{
					break;
				}
				string text2 = dRLInputFieldView.field.text;
				text2 = text2.Trim();
				view.inputWriteLabel.gameObject.SetActive(string.IsNullOrEmpty(text2));
				if (text2.StartsWith("/") && !ProcessCommands(ref text2))
				{
					dRLInputFieldView.field.text = "This command isn't available at this moment.";
				}
				else
				{
					if (string.IsNullOrEmpty(text2) || string.IsNullOrEmpty(view.activeChannel))
					{
						break;
					}
					bool flag = false;
					int num = text2.IndexOf('@');
					if (num >= 0)
					{
						string[] array = text2.Substring(num + 1).Split(' ');
						if (array.Length != 0)
						{
							string text3 = array[0];
							if (TryInitiatingPrivateChat(text3.Trim()))
							{
								flag = false;
							}
							else
							{
								for (int i = 1; i < array.Length; i++)
								{
									text3 = text3 + " " + array[i];
									if (TryInitiatingPrivateChat(text3.Trim()))
									{
										flag = false;
										break;
									}
									flag = true;
								}
							}
						}
					}
					if (flag)
					{
						break;
					}
					if (view.activeChannel.StartsWith("private-chat"))
					{
						string text4 = view.activeChannel.Split(' ')[1];
						if (string.IsNullOrEmpty(text4) || text4 == "DEFAULT")
						{
							dRLInputFieldView.ClearInputText();
							dRLInputFieldView.field.ActivateInputField();
							break;
						}
						Debug.Log("Send Private Message: " + text2 + " to: " + text4);
						chatModel.SendPrivateMessage(text4, text2, "", "", Color.black);
					}
					else
					{
						switch (view.activeChannel)
						{
						case "global-chat":
							chatModel.SendGlobalMessage(text2);
							break;
						case "tournament-chat":
							chatModel.SendMessageToChannelWithScope(text2, ChatRoom.Scope.Tournament);
							break;
						case "room-chat":
							roomChatModel.SendChatMessage(text2);
							break;
						}
					}
					CheckForMsgSpamming();
					if (string.IsNullOrEmpty(dRLInputFieldView.field.text))
					{
						view.UnfocusPanel();
					}
					else
					{
						dRLInputFieldView.field.ActivateInputField();
					}
					dRLInputFieldView.field.text = "";
					if ((bool)base.app.model.chat)
					{
						base.app.model.chat.LastMessage = "";
					}
				}
				break;
			}
			case "chat.incoming.public":
			case "chat.incoming.private":
			case "network.room.chat.incoming":
			{
				if (!view.social.isActive)
				{
					break;
				}
				PlatformService ps = base.app.model.service.platform;
				if (!ps)
				{
					break;
				}
				int num2 = p_data.Length;
				object obj2 = ((num2 > 0) ? p_data[0] : null);
				object obj3 = ((num2 > 1) ? p_data[1] : null);
				ChatRoom chatRoom = ((obj2 is ChatRoom) ? ((ChatRoom)obj2) : null);
				NetworkRoomChat.Message room_msg = ((obj2 is NetworkRoomChat.Message) ? ((NetworkRoomChat.Message)obj2) : null);
				PrivateChatMessage private_msg = ((obj3 is PrivateChatMessage) ? ((PrivateChatMessage)obj3) : null);
				PublicChatMessage chat_msg = ((obj3 is PublicChatMessage) ? ((PublicChatMessage)obj3) : null);
				NetworkActor sender_actor = ((room_msg == null) ? null : roomChatModel.GetPlayer(room_msg.SenderId));
				if (p_event == "chat.incoming.public" && view.social.useGameTemplate && base.app.model.network.room != null && chatRoom.ScopeType == ChatRoom.Scope.Global)
				{
					break;
				}
				bool flag2 = false;
				bool flag3 = false;
				if (private_msg != null)
				{
					flag2 = private_msg.IsValidated;
					flag3 = private_msg.IsBlocked;
				}
				if (chat_msg != null)
				{
					flag2 = chat_msg.IsValidated || chat_msg.IsInfo;
					flag3 = chat_msg.IsBlocked && !chat_msg.IsInfo;
				}
				if (room_msg != null)
				{
					flag2 = room_msg.IsValidated;
					flag3 = room_msg.IsBlocked;
				}
				if (flag2 && flag3)
				{
					break;
				}
				string steamId = "";
				string msg_sender_pid = "";
				string msg_sender_name = "";
				string msg_content = "";
				string msg_channel = "";
				string msg_platform = "";
				int msg_badge_level = -1;
				bool msg_from_friend = false;
				if (p_event != null)
				{
					switch (p_event)
					{
					case "network.room.chat.incoming":
						msg_sender_pid = sender_actor.PlatformId;
						steamId = sender_actor.PlatformId;
						msg_badge_level = sender_actor.BadgeLevel;
						msg_platform = sender_actor.Platform;
						msg_sender_name = room_msg.SenderName;
						msg_content = room_msg.Content;
						msg_platform = (msg_channel = "room-chat");
						break;
					case "chat.incoming.private":
						msg_sender_pid = private_msg.SenderId;
						steamId = private_msg.SenderId;
						msg_badge_level = private_msg.BadgeLevel;
						msg_sender_name = private_msg.SenderName;
						msg_platform = private_msg.Platform;
						msg_content = private_msg.Body;
						msg_channel = view.GetPrivateChannel(private_msg.IsMine ? private_msg.ReceiverId : private_msg.SenderId);
						break;
					case "chat.incoming.public":
						msg_sender_pid = chat_msg.SenderId;
						steamId = chat_msg.SenderId;
						msg_badge_level = chat_msg.BadgeLevel;
						msg_sender_name = chat_msg.SenderName;
						msg_platform = chat_msg.Platform;
						msg_content = chat_msg.Body;
						msg_channel = ((chatRoom.ScopeType == ChatRoom.Scope.Tournament) ? "tournament-chat" : "global-chat");
						break;
					}
				}
				if (string.IsNullOrEmpty(msg_channel))
				{
					break;
				}
				msg_from_friend = IsFriend(steamId);
				if (flag2 && !flag3)
				{
					if (p_event == null)
					{
						break;
					}
					switch (p_event)
					{
					case "chat.incoming.private":
						if (!base.app.model.service.platform.GetUserSessionBlocked(private_msg.SenderId) && !base.app.model.storage.state.player.blockedUsers.Contains(private_msg.SenderId))
						{
							view.Add(private_msg, msg_from_friend, msg_badge_level, msg_platform, isPrivate: true, msg_channel);
						}
						break;
					case "chat.incoming.public":
						if (!base.app.model.service.platform.GetUserSessionBlocked(chat_msg.SenderId) && !base.app.model.storage.state.player.blockedUsers.Contains(chat_msg.SenderId))
						{
							view.Add(chat_msg, msg_from_friend, msg_badge_level, msg_platform, isPrivate: false, msg_channel);
						}
						break;
					case "network.room.chat.incoming":
						if (!base.app.model.service.platform.GetUserSessionBlocked(room_msg.SenderId.ToString()) && !base.app.model.storage.state.player.blockedUsers.Contains(room_msg.SenderId.ToString()))
						{
							view.Add(sender_actor, msg_content, room_msg.Date, room_msg.IsMine, msg_from_friend, msg_badge_level, msg_platform, isPrivate: false, msg_channel);
						}
						break;
					}
					break;
				}
				ps.IsUserCommunicationBlocked(msg_sender_pid, delegate(bool p_blocked)
				{
					if (string.IsNullOrEmpty(msg_sender_pid))
					{
						p_blocked = false;
					}
					if (msg_sender_pid.Contains("info"))
					{
						p_blocked = false;
					}
					string p_input = msg_sender_name + "@" + msg_content;
					ps.TextValidate(p_input, delegate(bool p_result, string p_value)
					{
						if (private_msg != null)
						{
							private_msg.IsValidated = true;
							private_msg.IsBlocked = !p_result || p_blocked;
							if (!string.IsNullOrEmpty(p_value))
							{
								private_msg.Body = p_value;
							}
						}
						if (chat_msg != null)
						{
							chat_msg.IsValidated = true;
							chat_msg.IsBlocked = !p_result || p_blocked;
							if (!string.IsNullOrEmpty(p_value))
							{
								chat_msg.Body = p_value;
							}
						}
						if (room_msg != null)
						{
							room_msg.IsValidated = true;
							room_msg.IsBlocked = !p_result || p_blocked;
							if (!string.IsNullOrEmpty(p_value))
							{
								msg_content = p_value;
							}
						}
						if (!p_result)
						{
							Debug.LogWarning("UIChatController> " + p_event + " / TextValidate Fail");
						}
						else if (p_blocked)
						{
							Debug.LogWarning("UIChatController> " + p_event + " / User is Blocked");
						}
						else if (p_event != null)
						{
							switch (p_event)
							{
							case "chat.incoming.private":
								view.Add(private_msg, msg_from_friend, msg_badge_level, msg_platform, isPrivate: true, msg_channel);
								break;
							case "chat.incoming.public":
								view.Add(chat_msg, msg_from_friend, msg_badge_level, msg_platform, isPrivate: false, msg_channel);
								break;
							case "network.room.chat.incoming":
								view.Add(sender_actor, msg_content, room_msg.Date, room_msg.IsMine, msg_from_friend, msg_badge_level, msg_platform, isPrivate: false, msg_channel);
								break;
							}
						}
					}, p_chatMessage: true);
				});
				break;
			}
			case "chat.channnel.player.left":
			case "chat.channnel.player.joined":
				if (p_data.Length >= 2)
				{
					string obj = (string)p_data[0];
					string text5 = (string)p_data[1];
					if (!(obj != "global-chat") && p_event != null && p_event == "chat.channnel.player.left")
					{
						string text6 = "private-chat " + text5;
						view.RemoveChannel(text6);
						view.RemovePrivateChannels(text6);
					}
				}
				break;
			case "chat.info.help@click":
				WebBrowser.OpenURL("https://drlracingsimulator.zendesk.com/hc/en-us/requests/new", (base.app != null) ? base.app.model.service.platform : null);
				break;
			case "chat.info.zendesk@click":
				WebBrowser.OpenURL("https://drlracingsimulator.zendesk.com/hc/en-us", (base.app != null) ? base.app.model.service.platform : null);
				break;
			case "chat.info.steam@click":
				WebBrowser.OpenURL("https://steamcommunity.com/app/641780/discussions", (base.app != null) ? base.app.model.service.platform : null);
				break;
			case "chat.info.discord@click":
				WebBrowser.OpenURL("https://discord.gg/p7ndQHz", (base.app != null) ? base.app.model.service.platform : null);
				break;
			}
		}

		private void AutoCompletePrivateChannel(DRLInputFieldView inf)
		{
			string text = inf.text;
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			if (text.Length > m_privateAutoLength)
			{
				m_waitForNewChar = false;
			}
			m_privateAutoLength = text.Length;
			if (m_waitForNewChar)
			{
				return;
			}
			string text2 = "";
			foreach (KeyValuePair<string, string> privateChannel in UIChatView.privateChannels)
			{
				if (!string.IsNullOrEmpty(privateChannel.Value))
				{
					string text3 = privateChannel.Value.ToUpper();
					if (text3.StartsWith(text.ToUpper()))
					{
						text2 = text3;
						break;
					}
				}
			}
			if (!string.IsNullOrEmpty(text2))
			{
				inf.text = text2 + "]";
				inf.field.selectionAnchorPosition = text.Length;
				inf.field.selectionFocusPosition = text2.Length + 1;
				m_foundAutocomplete = true;
			}
		}

		private bool ProcessCommands(ref string cs)
		{
			if (cs.StartsWith("/global"))
			{
				if (UIChatView.availableChannels.ContainsKey("global-chat"))
				{
					view.SetChannelActive("global-chat");
					cs = cs.Replace("/global", "");
					cs = cs.Trim();
					return true;
				}
				return false;
			}
			if (cs.StartsWith("/game"))
			{
				if (UIChatView.availableChannels.ContainsKey("room-chat"))
				{
					view.SetChannelActive("room-chat");
					cs = cs.Replace("/game", "");
					cs = cs.Trim();
					return true;
				}
				return false;
			}
			if (cs.StartsWith("/tournament"))
			{
				if (UIChatView.availableChannels.ContainsKey("tournament-chat"))
				{
					view.SetChannelActive("tournament-chat");
					cs = cs.Replace("/tournament", "");
					cs = cs.Trim();
					return true;
				}
				return false;
			}
			return false;
		}

		private bool TryInitiatingPrivateChat(string p_username)
		{
			if (string.IsNullOrEmpty(p_username))
			{
				view.playerNotFoundField.SetActive(value: true);
				this.TimerRunOnce(delegate
				{
					view.playerNotFoundField.SetActive(value: false);
				}, 1.5f);
				return false;
			}
			foreach (KeyValuePair<string, string> privateChannel in UIChatView.privateChannels)
			{
				if (!(privateChannel.Value.ToUpper() == p_username.ToUpper()))
				{
					continue;
				}
				if (!view.SetActiveAutocompleteChannel(p_username))
				{
					view.playerNotFoundField.SetActive(value: true);
					view.RefreshPrivateChannel();
					this.TimerRunOnce(delegate
					{
						view.playerNotFoundField.SetActive(value: false);
					}, 1.5f);
					return false;
				}
				return true;
			}
			view.playerNotFoundField.SetActive(value: true);
			this.TimerRunOnce(delegate
			{
				view.playerNotFoundField.SetActive(value: false);
			}, 1.5f);
			return false;
		}

		private void MarkCurrentChannelAsRead()
		{
			switch (mActiveChannel)
			{
			case "global-chat":
				if ((bool)chatModel)
				{
					chatModel.MarkChannelAsRead(mActiveChannel);
				}
				break;
			case "tournament-chat":
				if ((bool)chatModel)
				{
					ChatRoom publicChannelByScope = chatModel.GetPublicChannelByScope(ChatRoom.Scope.Tournament);
					if (publicChannelByScope != null)
					{
						chatModel.MarkChannelAsRead(publicChannelByScope.ID);
					}
				}
				break;
			case "private-chat":
				if ((bool)chatModel && !string.IsNullOrEmpty(mCurrentThreadId))
				{
					chatModel.MarkChannelAsRead(mCurrentThreadId);
				}
				break;
			case "room-chat":
				if ((bool)roomChatModel)
				{
					roomChatModel.MarkChatAsRead();
				}
				break;
			}
		}

		private void CheckForMsgSpamming()
		{
			if (m_spammingTimer <= 0f)
			{
				m_spammingTimer = 3f;
				m_spammingMsgCount = 0;
			}
			else
			{
				m_spammingMsgCount++;
			}
			if (m_spammingMsgCount >= 3 && m_spammingTimer > 0f)
			{
				view.EnableInputWaitLabel(p_enable: true, 5);
			}
		}

		private void EnableChatScrolling(bool p_enable)
		{
			if (p_enable)
			{
				if ((bool)view.chatHandleNav)
				{
					if (UINavigation.focus != view.inputNavigation)
					{
						UINavigation.focus = view.chatHandleNav;
						base.app.view.ui.screenBack = false;
						if ((bool)view.chatScrollBarFade)
						{
							view.chatScrollBarFade.pulse = true;
						}
						Transform transform = null;
						int childCount = view.list.childCount;
						if (childCount > 0)
						{
							transform = view.list.GetChild(childCount - 1);
						}
						if ((bool)transform)
						{
							UINavigation.focus = transform.GetComponent<UINavigation>();
						}
						mHandleScroll = true;
					}
				}
				else
				{
					Debug.LogError("Can't find chat's window scrolling handle! It is not set on the view component.");
				}
				return;
			}
			RunOnce(1f / 30f, delegate
			{
				if ((bool)view.chatPanelNav)
				{
					if (UINavigation.focus != view.inputNavigation)
					{
						UINavigation.focus = view.chatPanelNav;
						mHandleScroll = false;
					}
				}
				else
				{
					Debug.LogError("Can't find chat panel navigation element to return focus to! It's not set on the view component.");
				}
				base.app.view.ui.screenBack = true;
				if ((bool)view.chatScrollBarFade)
				{
					if (view.chatScrollBarFade.pulse)
					{
						view.chatScrollBarFade.FadeOut();
					}
					view.chatScrollBarFade.pulse = false;
				}
			});
		}

		protected void Update()
		{
			if (m_spammingTimer >= 0f)
			{
				m_spammingTimer -= Time.deltaTime;
			}
			if (Input.GetKeyDown(KeyCode.AltGr) || Input.GetKeyDown(KeyCode.LeftAlt) || RCI.GetButtonDown(ConsoleButtons.RightShoulder1))
			{
				if (!view.social.isActive || !view.social.open)
				{
					return;
				}
				view.CycleChannels();
			}
			if (Input.GetKeyDown(KeyCode.Backspace))
			{
				if ((DRLUINavigationSystem.IsTyping && UINavigation.focus != null && UINavigation.focus.name != "autocomplete-input") || !m_foundAutocomplete)
				{
					return;
				}
				m_waitForNewChar = true;
				m_foundAutocomplete = false;
				view.privateInput.text = view.privateInput.text.Substring(0, view.privateInput.field.selectionAnchorPosition);
			}
			if (Input.GetKeyDown(KeyCode.Tab) && (!DRLUINavigationSystem.IsTyping || !(UINavigation.focus != null) || !(UINavigation.focus.name != "autocomplete-input")))
			{
				view.privateInput.field.caretPosition = view.privateInput.text.Length;
				view.input.Activate();
			}
		}

		private bool IsFriend(string steamId, out GameFriendData friendData)
		{
			friendData = socialModel.friends.Get(steamId);
			return friendData != null;
		}

		private bool IsFriend(string steamId)
		{
			GameFriendData friendData = null;
			return IsFriend(steamId, out friendData);
		}

		private bool GetFriend(string steamId, out GameFriendData friendData)
		{
			friendData = socialModel.friends.Get(steamId);
			return friendData != null;
		}

		private int SortByTime(ChatRoom a, ChatRoom b)
		{
			if (a.LastMessageTime == b.LastMessageTime)
			{
				return 0;
			}
			if (a.LastMessageTime > b.LastMessageTime)
			{
				return 1;
			}
			return -1;
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using drl.chat;
using drl.network;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIChatView : View<DRLApp>
	{
		public DRLInputFieldView input;

		public UINavigation inputNavigation;

		public RectTransform list;

		public UIChatMessageController messageTemplate;

		public RectTransform viewport;

		public Scrollbar verticalScrollbar;

		public UINavigation chatHandleNav;

		public UINavigation chatPanelNav;

		public FadeComponent chatScrollBarFade;

		public Text inputWriteLabel;

		public Text inputWaitLabel;

		public GameObject serviceOverlay;

		public Text serviceMessageLabel;

		public UINavigation chatTabNavigation;

		public ListComponent messagesList;

		public static DateTime infoDateTime;

		public FadeComponent panelFade;

		public FadeComponent inputFade;

		public GameObject playerNotFoundField;

		public Text channelLabel;

		[Space]
		[Header("Channel colors:")]
		public Color globalColorLight;

		public Color gameColorLight;

		public Color privateColorLight;

		public Color globalColorDark;

		public Color gameColorDark;

		public Color privateColorDark;

		public Image inputLayoutBackground;

		public Image inputOutline;

		public DRLInputFieldView privateInput;

		[SerializeField]
		private Font m_tabsPendingFont;

		[SerializeField]
		private Font m_tabsClearFont;

		[SerializeField]
		private FadeComponent m_fadeComponent;

		public UISocialView social;

		private string mLastMessageHeaderID;

		private string m_lastMessageChannel = "";

		private UIChatMessageController mLastMessage;

		public int messagesPoolSize = 30;

		public bool messagePoolComplete;

		public static Dictionary<string, string> availableChannels = new Dictionary<string, string>();

		public static Dictionary<string, string> privateChannels = new Dictionary<string, string>();

		private int mCnt;

		public string captionText
		{
			get
			{
				return inputWriteLabel.text;
			}
			set
			{
				if (availableChannels.Count <= 1)
				{
					inputWriteLabel.text = base.app.model.storage.locale.Get<string>("social.chat.channels", "TYPE HERE...");
				}
				else
				{
					inputWriteLabel.text = value;
				}
			}
		}

		public bool focused { get; set; }

		public string activeChannel { get; set; }

		protected void Awake()
		{
			int pool_step = 0;
			this.ActivityRun((Func<bool>)delegate
			{
				if (messagePoolComplete)
				{
					return false;
				}
				if (pool_step >= messagesPoolSize)
				{
					return false;
				}
				messagesList.Push<UIChatMessageController>();
				messagesList.Clear();
				pool_step++;
				if (pool_step >= messagesPoolSize)
				{
					messagePoolComplete = true;
					LoadMessages();
				}
				return true;
			}, 0f);
		}

		private void Start()
		{
			SetCaptionText();
		}

		private void SetCaptionText()
		{
			captionText = base.app.model.storage.locale.Get<string>("social.chat.channels", "PRESS ALT TO CYCLE CHANNELS");
		}

		private void SaveMessage(string m_id, string platformId, string p_player_id, string p_sender_id, Color userColor, string p_name, string p_text, DateTime msgTime, bool p_isMine, bool isFriend, int rank, string p_platform, bool isPrivate, string p_channel, bool isInfo)
		{
			UIChatMessage item = new UIChatMessage
			{
				messageId = m_id,
				platformId = platformId,
				playerId = p_player_id,
				userColor = userColor,
				name = p_name,
				text = p_text,
				msgTime = msgTime,
				left = p_isMine,
				isFriend = isFriend,
				rank = rank,
				isPrivate = isPrivate,
				channel = p_channel,
				isInfo = isInfo,
				platform = p_platform,
				senderId = p_sender_id
			};
			if (!base.app.model.chat.messages.Contains(item) && !string.IsNullOrEmpty(p_name) && !string.IsNullOrEmpty(p_text))
			{
				base.app.model.chat.messages.Add(item);
			}
		}

		public void LoadMessages()
		{
			if (base.app.model.chat == null || base.app.model.chat.messages.Count == 0)
			{
				return;
			}
			foreach (UIChatMessage message in base.app.model.chat.messages)
			{
				if (!base.app.model.service.platform.GetUserSessionBlocked(message.playerId) && !base.app.model.storage.state.player.blockedUsers.Contains(message.senderId))
				{
					Add(message.platformId, message.playerId, message.senderId, message.userColor, message.name, message.text, message.msgTime, message.left, message.isFriend, message.rank, message.platform, message.isPrivate, message.channel, isLoading: true, message.isInfo);
				}
			}
		}

		public void PruneMessages()
		{
			for (int i = 0; i < messagesList.Count; i++)
			{
				UIChatMessageController uIChatMessageController = messagesList.Get<UIChatMessageController>(i);
				if (string.IsNullOrEmpty(uIChatMessageController.view.message))
				{
					uIChatMessageController.gameObject.SetActive(value: false);
				}
			}
		}

		public void Show()
		{
			m_fadeComponent.FadeIn(0f);
		}

		public void Hide()
		{
			m_fadeComponent.FadeOut(0f);
		}

		public void FocusPanel()
		{
			if (social.useGameTemplate && social.isActive)
			{
				panelFade.FadeIn();
				input.gameObject.SetActive(value: true);
				inputFade.FadeIn();
				focused = true;
				base.app.view.ui.navigation.enabled = true;
				DRLUINavigationSystem.IsTyping = true;
				input.Activate();
				Notify("chat.panel@active");
				social.SetIgnoredGameCommands();
				if ((bool)social.graphicRaycaster)
				{
					social.graphicRaycaster.enabled = true;
				}
				if (!string.IsNullOrEmpty(base.app.model.chat.LastMessage))
				{
					input.text = base.app.model.chat.LastMessage;
				}
				SetCaptionText();
				UINavigation.Focus(inputNavigation);
			}
		}

		public void UnfocusPanel()
		{
			if (social.useGameTemplate)
			{
				panelFade.FadeOut();
				inputFade.FadeOut();
				focused = false;
				input.ClearInputText();
				Notify("chat.panel@inactive");
				social.ClearIgnoredCommands();
				this.TimerRunOnce(delegate
				{
					DRLUINavigationSystem.IsTyping = false;
					UINavigation.focus = null;
					input.gameObject.SetActive(value: false);
				}, 0.4f);
				if ((bool)social.graphicRaycaster)
				{
					social.graphicRaycaster.enabled = false;
				}
			}
		}

		public void ResetPanel()
		{
			if (social.useGameTemplate)
			{
				panelFade.FadeOut(0f);
				inputFade.FadeOut(0f);
				focused = false;
				input.ClearInputText();
				DRLUINavigationSystem.IsTyping = false;
				if (base.app.view.ui.screens.current == null)
				{
					UINavigation.focus = null;
				}
				input.gameObject.SetActive(value: false);
			}
		}

		public void ToggleFocus()
		{
			if (focused && IsInputEmpty())
			{
				UnfocusPanel();
			}
			else
			{
				FocusPanel();
			}
		}

		public void Clear()
		{
			for (int num = messagesList.Count - 1; num >= 0; num--)
			{
				UIChatMessageController uIChatMessageController = messagesList.Get<UIChatMessageController>(num);
				if (uIChatMessageController != null && !uIChatMessageController.IsInfo)
				{
					uIChatMessageController.Reset();
					messagesList.Remove(uIChatMessageController);
				}
			}
			mLastMessageHeaderID = "";
			EnableInputWaitLabel(p_enable: false, 0);
			inputWriteLabel.gameObject.SetActive(string.IsNullOrEmpty(input.text));
		}

		public void Lock(bool p_enable, string p_serviceMsg = "")
		{
			serviceOverlay.SetActive(p_enable);
			serviceMessageLabel.text = p_serviceMsg;
			input.GetComponent<InputField>().interactable = p_enable;
		}

		public void SetDefaultChannel(string p_channel)
		{
			string text = "";
			activeChannel = p_channel;
			ClearPrivateInputField();
			Localization locale = base.app.model.storage.locale;
			switch (p_channel)
			{
			case "global-chat":
				SetLayoutColor("global-chat");
				text = locale.Get<string>("social.chat.channel.global", "[GLOBAL]");
				RemoveChannel("room-chat");
				RemoveChannel("tournament-chat");
				break;
			case "tournament-chat":
				SetLayoutColor("tournament-chat");
				text = locale.Get<string>("social.chat.channel.vdrl", "[LOCAL (TOURNAMENT)]");
				RemoveChannel("room-chat");
				if (social.useGameTemplate)
				{
					RemoveChannel("global-chat");
				}
				else
				{
					AddChannel("global-chat", locale.Get<string>("social.chat.channel.global", "[GLOBAL]"));
				}
				break;
			case "room-chat":
				SetLayoutColor("room-chat");
				text = locale.Get<string>("social.chat.channel.game", "[LOCAL (GAME)]");
				RemoveChannel("tournament-chat");
				if (social.useGameTemplate)
				{
					RemoveChannel("global-chat");
				}
				else
				{
					AddChannel("global-chat", locale.Get<string>("social.chat.channel.global", "[GLOBAL]"));
				}
				break;
			default:
			{
				SetLayoutColor("private-chat");
				text = locale.Get<string>("social.chat.channel.private", "[PRIVATE @");
				string privateInputField = p_channel.Split(' ')[1];
				SetPrivateInputField(privateInputField);
				break;
			}
			}
			channelLabel.text = text;
			AddChannel(p_channel, text);
			bool flag = false;
			foreach (KeyValuePair<string, string> availableChannel in availableChannels)
			{
				if (availableChannel.Key.StartsWith("private-chat"))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				AddChannel("private-chat DEFAULT", locale.Get<string>("social.chat.channel.private", "[PRIVATE @"));
			}
		}

		public void SetChannelActive(string p_channel)
		{
			Localization locale = base.app.model.storage.locale;
			if (p_channel.StartsWith("private-chat") && !availableChannels.ContainsKey(p_channel))
			{
				ClearPrivateChannels();
				availableChannels.Add(p_channel, locale.Get<string>("social.chat.channel.private", "[PRIVATE @"));
			}
			if (availableChannels.ContainsKey(p_channel))
			{
				string text = "";
				activeChannel = p_channel;
				ClearPrivateInputField();
				input.text = "";
				switch (p_channel)
				{
				case "global-chat":
					SetLayoutColor("global-chat");
					text = locale.Get<string>("social.chat.channel.global", "[GLOBAL]");
					break;
				case "tournament-chat":
					SetLayoutColor("tournament-chat");
					text = locale.Get<string>("social.chat.channel.vdrl", "[LOCAL (TOURNAMENT)]");
					break;
				case "room-chat":
					SetLayoutColor("room-chat");
					text = locale.Get<string>("social.chat.channel.game", "[LOCAL (GAME)]");
					break;
				default:
				{
					SetLayoutColor("private-chat");
					text = locale.Get<string>("social.chat.channel.private", "[PRIVATE @");
					string privateInputField = p_channel.Split(' ')[1];
					SetPrivateInputField(privateInputField);
					break;
				}
				}
				channelLabel.text = text;
			}
		}

		public void SetPrivateDefaultActive()
		{
			AddChannel("private-chat DEFAULT", base.app.model.storage.locale.Get<string>("social.chat.channel.private", "[PRIVATE @"));
			activeChannel = "private-chat DEFAULT";
			SetLayoutColor("private-chat");
			channelLabel.text = base.app.model.storage.locale.Get<string>("social.chat.channel.private", "[PRIVATE @");
			SetPrivateInputField(activeChannel);
		}

		public string GetPrivateChannel(string p_pid, string p_name = "")
		{
			string result = "private-chat " + p_pid;
			if (!privateChannels.ContainsKey(p_pid))
			{
				if (string.IsNullOrEmpty(p_name))
				{
					return "";
				}
				privateChannels.Add(p_pid, p_name);
			}
			return result;
		}

		public void ClearPrivateChannels()
		{
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, string> availableChannel in availableChannels)
			{
				if (availableChannel.Key.StartsWith("private-chat"))
				{
					list.Add(availableChannel.Key);
				}
			}
			if (list.Count == 0)
			{
				return;
			}
			foreach (string item in list)
			{
				availableChannels.Remove(item);
			}
		}

		public void AddChannel(string p_channel, string p_label)
		{
			if (!availableChannels.ContainsKey(p_channel))
			{
				availableChannels.Add(p_channel, p_label ?? "");
				SetCaptionText();
			}
		}

		public void RemoveChannel(string p_channel)
		{
			if (!availableChannels.ContainsKey(p_channel))
			{
				return;
			}
			availableChannels.Remove(p_channel);
			if (activeChannel == p_channel)
			{
				if (availableChannels.Count > 0)
				{
					SetChannelActive(availableChannels.Last().Key);
				}
				else
				{
					channelLabel.text = "";
					activeChannel = "";
				}
			}
			SetCaptionText();
		}

		public bool SetActiveAutocompleteChannel(string p_username, bool p_has_brackets = true)
		{
			string text = (p_has_brackets ? p_username.Replace("]", "").Trim() : p_username.Trim());
			string text2 = "";
			foreach (KeyValuePair<string, string> privateChannel in privateChannels)
			{
				if (!string.IsNullOrEmpty(privateChannel.Value) && privateChannel.Value.ToUpper() == text.ToUpper())
				{
					text2 = privateChannel.Key;
					break;
				}
			}
			if (string.IsNullOrEmpty(text2))
			{
				return false;
			}
			SetChannelActive("private-chat " + text2);
			return true;
		}

		public void CycleChannels()
		{
			input.field.ActivateInputField();
			if (availableChannels.Count == 0)
			{
				activeChannel = "";
				return;
			}
			int num = 0;
			foreach (KeyValuePair<string, string> availableChannel in availableChannels)
			{
				num++;
				if (availableChannel.Key == activeChannel)
				{
					break;
				}
			}
			if (num >= availableChannels.Count)
			{
				num = 0;
			}
			List<string> list = availableChannels.Keys.ToList();
			SetChannelActive(list[num]);
		}

		public void SetPrivateChannels()
		{
			if (!base.validContext)
			{
				return;
			}
			if (!DRLBootController.ready)
			{
				Debug.LogWarning("UIChatView> SetPrivateChannels / Game Boot not Completed!");
				return;
			}
			ChatRoom globalChat = base.app.model.chat.GlobalChat;
			if (globalChat == null)
			{
				return;
			}
			HashSet<string> users = globalChat.Users;
			string mid = base.app.model.storage.state.player.profile.platformId;
			base.app.model.service.GetSocialProfile(users.ToArray(), delegate(DRLPlayerProfileData[] results)
			{
				if (base.validContext && results != null && privateChannels != null && results.Length != 0)
				{
					for (int i = 0; i < results.Length; i++)
					{
						if (results[i] != null && !string.IsNullOrEmpty(results[i].name))
						{
							string key = results[i].platformId.ToString();
							if (!privateChannels.ContainsKey(key) && mid != results[i].platformId)
							{
								privateChannels.Add(key, results[i].name.ToUpper());
							}
						}
					}
				}
			});
		}

		public void AddPrivateChannels(string p_steamId)
		{
			if (!base.validContext)
			{
				return;
			}
			if (!DRLBootController.ready)
			{
				Debug.LogWarning("UIChatView> AddPrivateChannels / Game Boot not Completed!");
				return;
			}
			base.app.model.service.GetSocialProfile(p_steamId, delegate(DRLPlayerProfileData[] results)
			{
				if (base.validContext && results != null && privateChannels != null && results.Length != 0 && !privateChannels.ContainsKey(results[0].platformId.ToString()))
				{
					privateChannels.Add(results[0].platformId.ToString(), results[0].name.ToUpper());
				}
			});
		}

		public void RemovePrivateChannels(string p_steamId)
		{
			if (!privateChannels.ContainsKey(p_steamId))
			{
				return;
			}
			privateChannels.Remove(p_steamId);
			string text = "private-chat " + p_steamId;
			if (availableChannels.ContainsKey(text))
			{
				availableChannels.Remove(text);
			}
			bool flag = false;
			foreach (KeyValuePair<string, string> availableChannel in availableChannels)
			{
				if (availableChannel.Key.StartsWith("private-chat"))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				AddChannel("private-chat DEFAULT", base.app.model.storage.locale.Get<string>("social.chat.channel.private", "[PRIVATE @"));
			}
			if (activeChannel == text)
			{
				SetPrivateDefaultActive();
			}
		}

		public bool IsOnline(string p_steamId)
		{
			return base.app.model.chat.GlobalChat?.Users.Contains(p_steamId) ?? false;
		}

		public void RefreshPrivateChannel()
		{
			foreach (KeyValuePair<string, string> availableChannel in availableChannels)
			{
				if (availableChannel.Key.StartsWith("private-chat"))
				{
					SetChannelActive(availableChannel.Key);
					break;
				}
			}
		}

		public void Add(NetworkActor p_player, string p_text, DateTime msgTime, bool p_left, bool isFriend, int rank, string p_platform, bool isPrivate = false, string p_channel = "global-chat")
		{
			if (!base.app.model.service.platform.GetUserSessionBlocked(p_player.PlayerId) && !base.app.model.storage.state.player.blockedUsers.Contains(p_player.PlayerId))
			{
				Add(p_player.PlatformId, p_player.PlayerId, p_player.PlayerId, p_player.ProfileColor, p_player.ProfileName, p_text, msgTime, p_left, isFriend, rank, p_platform, isPrivate = false, p_channel);
			}
		}

		public UIChatMessageController Add(IChatSendable msg, bool isFriend, int rank, string p_platform, bool isPrivate = false, string p_channel = "global-chat")
		{
			return Add(msg.SenderId, msg.PlayerId, msg.SenderId, msg.SenderColor, msg.SenderName, msg.Body, msg.Date, msg.IsMine, isFriend, rank, p_platform, isPrivate, p_channel, isLoading: false, msg.IsInfo);
		}

		public UIChatMessageController Add(string p_platform_id, string p_player_id, string p_sender_id, Color userColor, string p_name, string p_text, DateTime msgTime, bool p_isMine, bool isFriend, int rank, string p_platform, bool isPrivate = false, string p_channel = "global-chat", bool isLoading = false, bool isInfo = false)
		{
			if ((isInfo && (p_platform_id != base.app.model.storage.state.player.profile.platformId || social.useGameTemplate)) || string.IsNullOrEmpty(p_text))
			{
				return null;
			}
			if (isInfo)
			{
				for (int i = 0; i < messagesList.Count; i++)
				{
					UIChatMessageController uIChatMessageController = messagesList.Get<UIChatMessageController>(i);
					if (uIChatMessageController != null && uIChatMessageController.IsInfo && uIChatMessageController.gameObject.activeInHierarchy)
					{
						return null;
					}
				}
			}
			if (!p_isMine && !isInfo)
			{
				if (!privateChannels.ContainsKey(p_platform_id))
				{
					privateChannels.Add(p_platform_id, p_name.ToUpper());
				}
				if (!isLoading)
				{
					Notify("social.badges.dirty");
				}
			}
			UIChatMessageController uIChatMessageController2 = PushMessage(p_platform_id, userColor, p_name, p_text, msgTime, p_isMine, isFriend, isPrivate, rank, p_platform, p_player_id, isInfo);
			if (!isLoading)
			{
				string id = GUID.Create(24, "", 200, 0, 15, "x1");
				SaveMessage(id, p_platform_id, p_player_id, p_sender_id, userColor, p_name, p_text, msgTime, p_isMine, isFriend, rank, p_platform, isPrivate, p_channel, isInfo);
			}
			ConfigureMessageAppeareance(p_name, uIChatMessageController2, p_channel);
			Hierarchy.RefreshLayout(list);
			return uIChatMessageController2;
		}

		protected UIChatMessageController PushMessage(string steamId, Color userColor, string p_name, string p_text, DateTime msgTime, bool p_left, bool isFriend, bool isPrivate, int rank, string p_platform, string p_playerId = null, bool isInfo = false)
		{
			if (messagesList.Count > messagesPoolSize)
			{
				messagesList.Get<UIChatMessageController>(0);
				messagesList.Shift();
			}
			UIChatMessageController uIChatMessageController = messagesList.Push<UIChatMessageController>();
			uIChatMessageController.Reset();
			uIChatMessageController.transform.localScale = Vector3.one;
			uIChatMessageController.transform.SetAsLastSibling();
			if (isInfo)
			{
				p_left = false;
			}
			bool isOnline = p_left || IsOnline(steamId);
			uIChatMessageController.Init(steamId, userColor, p_name, p_text, msgTime, p_left, isFriend, isOnline, isPrivate, p_playerId, rank, p_platform, isInfo, mCnt % 2 == 0);
			if (string.IsNullOrEmpty(p_name) || string.IsNullOrEmpty(p_text))
			{
				uIChatMessageController.gameObject.SetActive(value: false);
			}
			mCnt++;
			int num = int.Parse(uIChatMessageController.gameObject.name);
			if (num > 0)
			{
				UIChatMessageView component = messagesList.Get<UIChatMessageController>(num - 1).GetComponent<UIChatMessageView>();
				if ((bool)component)
				{
					uIChatMessageController.view.uinav.up = component.uinav;
					component.uinav.down = uIChatMessageController.view.uinav;
				}
				else
				{
					uIChatMessageController.view.uinav.up = chatTabNavigation;
					if ((bool)chatTabNavigation)
					{
						chatTabNavigation.down = uIChatMessageController.view.uinav;
					}
				}
			}
			else
			{
				uIChatMessageController.view.uinav.up = chatTabNavigation;
				if ((bool)chatTabNavigation)
				{
					chatTabNavigation.down = uIChatMessageController.view.uinav;
				}
			}
			uIChatMessageController.view.uinav.down = inputNavigation;
			inputNavigation.up = uIChatMessageController.view.uinav;
			return uIChatMessageController;
		}

		private void ConfigureMessageAppeareance(string msgUserName, UIChatMessageController chatMsg, string p_channel)
		{
			if (!chatMsg.view.isMine && !social.useGameTemplate)
			{
				chatMsg.SetupSubmenu();
			}
			if (p_channel.StartsWith("private-chat"))
			{
				chatMsg.view.channelColor = privateColorLight;
				chatMsg.view.outlineColor = privateColorDark;
				string key = p_channel.Split(' ')[1].Trim();
				if (chatMsg.IsMine)
				{
					if (privateChannels[key] == "DEFAULT")
					{
						return;
					}
					UIChatMessageView view = chatMsg.view;
					view.title = view.title + " TO " + privateChannels[key];
				}
				else
				{
					chatMsg.view.title += " TO YOU";
				}
			}
			else if (p_channel != null && p_channel == "global-chat")
			{
				chatMsg.view.channelColor = globalColorLight;
				chatMsg.view.outlineColor = globalColorDark;
			}
			else
			{
				chatMsg.view.channelColor = gameColorLight;
				chatMsg.view.outlineColor = gameColorDark;
			}
			if (social.useGameTemplate)
			{
				chatMsg.view.SetupInGameLayout();
			}
			mLastMessage = chatMsg;
		}

		public void EnableInputWaitLabel(bool p_enable, int p_seconds)
		{
			inputWriteLabel.gameObject.SetActive(!p_enable);
			inputWaitLabel.gameObject.SetActive(p_enable);
			StopCoroutine("InputWaitLabelTimer");
			if (p_enable)
			{
				StartCoroutine("InputWaitLabelTimer", p_seconds);
			}
		}

		public bool IsInputWaitLabelEnabled()
		{
			return inputWaitLabel.gameObject.activeInHierarchy;
		}

		private IEnumerator InputWaitLabelTimer(int p_seconds)
		{
			for (int i = p_seconds; i > 0; i--)
			{
				inputWaitLabel.text = "PLEASE WAIT..." + i;
				yield return new WaitForSeconds(1f);
			}
			EnableInputWaitLabel(p_enable: false, 0);
		}

		private bool IsInputEmpty()
		{
			return string.IsNullOrEmpty(input.text);
		}

		private void SetLayoutColor(string p_channel)
		{
			switch (p_channel)
			{
			case "global-chat":
				inputLayoutBackground.color = globalColorDark;
				inputOutline.color = globalColorLight;
				channelLabel.color = globalColorLight;
				input.inputText.color = globalColorLight;
				break;
			case "tournament-chat":
			case "room-chat":
				inputLayoutBackground.color = gameColorDark;
				inputOutline.color = gameColorLight;
				channelLabel.color = gameColorLight;
				input.inputText.color = gameColorLight;
				break;
			case "private-chat":
				inputLayoutBackground.color = privateColorDark;
				inputOutline.color = privateColorLight;
				channelLabel.color = privateColorLight;
				input.inputText.color = privateColorLight;
				break;
			}
		}

		public void SetPrivateInputField(string p_steamID)
		{
			privateInput.gameObject.SetActive(value: true);
			privateInput.field.interactable = true;
			if (privateChannels.ContainsKey(p_steamID))
			{
				privateInput.text = privateChannels[p_steamID] + "]";
			}
		}

		public void ClearPrivateInputField()
		{
			privateInput.text = "";
			privateInput.gameObject.SetActive(value: false);
		}
	}
}

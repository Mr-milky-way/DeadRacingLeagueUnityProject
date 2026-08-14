using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIChatBlockedView : View<DRLApp>
	{
		public RectTransform list;

		public UIChatBlockedUserController blockedUserTemplate;

		public RectTransform viewport;

		public Scrollbar verticalScrollbar;

		public UINavigation chatHandleNav;

		public UINavigation chatPanelNav;

		public FadeComponent chatScrollBarFade;

		public GameObject serviceOverlay;

		public Text serviceMessageLabel;

		public UINavigation chatTabNavigation;

		public ListComponent messagesList;

		public static DateTime infoDateTime;

		public FadeComponent panelFade;

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

		private int uCnt;

		public bool focused { get; set; }

		public string activeChannel { get; set; }

		public void LoadBlockedUsers()
		{
			base.app.model.service.GetSocialProfile(base.app.model.storage.state.player.blockedUsers.ToArray(), delegate(DRLPlayerProfileData[] results)
			{
				for (int i = 0; i < results.Length; i++)
				{
					Debug.Log(results.Length + " users blocked");
					Add(results[i]);
				}
			});
		}

		public void Show()
		{
			m_fadeComponent.FadeIn(0f);
		}

		public void Hide()
		{
			m_fadeComponent.FadeOut(0f);
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
		}

		public void Add(DRLPlayerProfileData p_playerProfileData)
		{
			PushUser(p_playerProfileData);
			Hierarchy.RefreshLayout(list);
		}

		protected UIChatBlockedUserController PushUser(DRLPlayerProfileData p_playerProfile)
		{
			UIChatBlockedUserController uIChatBlockedUserController = messagesList.Push<UIChatBlockedUserController>();
			uIChatBlockedUserController.Reset();
			uIChatBlockedUserController.transform.localScale = Vector3.one;
			uIChatBlockedUserController.transform.SetAsLastSibling();
			uIChatBlockedUserController.Init(p_playerProfile, uCnt % 2 == 0);
			uCnt++;
			return uIChatBlockedUserController;
		}
	}
}

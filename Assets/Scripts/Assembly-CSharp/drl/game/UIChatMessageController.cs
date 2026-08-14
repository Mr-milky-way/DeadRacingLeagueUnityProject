using System;
using System.Collections.Generic;
using UnityEngine;
using drl.network;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIChatMessageController : Controller<DRLApp>
	{
		[Tooltip("Optional: sprites that indicate the toggle state (Open/Close)")]
		[SerializeField]
		private Sprite[] m_submenuToggleIcons = new Sprite[2];

		private float m_timeStampTimer;

		private float m_timeStampRefreshPeriod = 2f;

		public Action onToggleSubmenuPanel;

		private bool mSubmenuInitialized;

		private UIChatSubmenuController mSubMenuController;

		public GameObject submenu;

		private string mSteamId;

		private bool mIsMine;

		private bool mIsFriend;

		private bool mIsOnline;

		private string mPlayerId;

		private string mUserName;

		private Color mColor;

		private string mPlatform = "";

		private bool mInitialized;

		public UIChatMessageView view => AssertLocal<UIChatMessageView>("view");

		public float ElapsedTime => m_timeStampTimer;

		public DateTime MessageTime { get; private set; }

		public bool IsPrivate { get; private set; }

		public bool IsInfo { get; private set; }

		public bool IsMine => mIsMine;

		public string MPlayerId
		{
			get
			{
				return mPlayerId;
			}
			set
			{
				mPlayerId = value;
			}
		}

		public void Init(string steamId, Color userColor, string p_name, string p_text, DateTime msgTime, bool p_isMine, bool isFriend, bool isOnline, bool isPrivate, string p_playerId, int rank, string p_platform, bool p_isInfo = false, bool bckgColor = false)
		{
			mSteamId = steamId;
			MessageTime = msgTime;
			mIsMine = p_isMine;
			mIsFriend = isFriend;
			mIsOnline = isOnline;
			MPlayerId = p_playerId;
			mUserName = p_name;
			mColor = userColor;
			IsPrivate = isPrivate;
			mPlatform = p_platform;
			view.isMine = p_isMine;
			view.LoadPhoto(p_playerId);
			view.userColor = userColor;
			view.rankBadge = rank;
			view.title = p_name;
			view.message = p_text;
			view.time = PhotonUtils.TimeAgo(msgTime, base.app.model.storage.locale);
			view.ShowToggle(show: false);
			view.ShowTime(show: true);
			IsInfo = p_isInfo;
			if (IsInfo)
			{
				view.SetInfo();
			}
			mInitialized = true;
			SetBackgroundColor(bckgColor);
		}

		public void Reset()
		{
			mInitialized = false;
			if (submenu != null)
			{
				submenu.SetActive(value: false);
			}
			mSubMenuController = null;
			m_timeStampTimer = 0f;
			mSteamId = null;
			MessageTime = DateTime.Now;
			mIsMine = false;
			mIsFriend = false;
			mIsOnline = false;
			MPlayerId = null;
			mUserName = null;
			mColor = Color.white;
			IsPrivate = false;
			view.rankBadge = 0;
			view.isMine = false;
			view.title = null;
			view.userColor = Color.white;
			view.message = null;
			view.time = null;
			view.ShowToggle(show: false);
			view.ShowTime(show: false);
			mSubmenuInitialized = false;
			view.ClearInfo();
			if (view.messageBackground != null)
			{
				view.messageBackground.enabled = false;
			}
			SetBackgroundColor(p_active: false);
		}

		public void SetupSubmenu()
		{
			submenu.SetActive(value: true);
			mSubMenuController = submenu.GetComponent<UIChatSubmenuController>();
			UINavigation component = GetComponent<UINavigation>();
			mSubMenuController.Setup(new UIChatSubmenuData(mSteamId, mUserName, mIsFriend, mIsOnline, IsPrivate, MPlayerId, mColor, mPlatform, component, component.down, view.submenuIcon, m_submenuToggleIcons[0], m_submenuToggleIcons[1]));
			mSubMenuController.Fold(0f);
			mSubmenuInitialized = true;
		}

		private void Update()
		{
			if (mInitialized)
			{
				if (m_timeStampTimer > m_timeStampRefreshPeriod)
				{
					m_timeStampTimer = 0f;
					view.time = PhotonUtils.TimeAgo(MessageTime, base.app.model.storage.locale);
				}
				else
				{
					m_timeStampTimer += Time.deltaTime;
				}
				if (mSubmenuInitialized && !IsPrivate)
				{
					view.ShowToggle((mSubMenuController != null && (mSubMenuController.IsOpen || mSubMenuController.view.Opening)) || (UINavigation.focus != null && UINavigation.focus.gameObject == base.gameObject));
				}
			}
		}

		private void OnToggleSubMenu()
		{
			if (!IsInfo)
			{
				if (mSubMenuController.IsOpen)
				{
					mSubMenuController.Fold();
				}
				else
				{
					mSubMenuController.Unfold();
				}
			}
		}

		private void SetBackgroundColor(bool p_active)
		{
			if (!(view.messageBackground == null))
			{
				view.messageBackground.enabled = p_active;
			}
		}

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (!mInitialized)
			{
				return;
			}
			switch (p_event)
			{
			case "chat.message@click":
				try
				{
					UIElementView uIElementView = p_target as UIElementView;
					UIChatMessageView component = uIElementView.GetComponent<UIChatMessageView>();
					if (component == null || component != view)
					{
						break;
					}
					if (!component.isInfo)
					{
						if (mSubmenuInitialized && uIElementView != null && uIElementView.gameObject == base.gameObject)
						{
							OnToggleSubMenu();
						}
					}
					else
					{
						UINavigation.Focus(view.infoNav);
					}
					break;
				}
				catch (InvalidCastException ex)
				{
					Debug.LogWarning("Unable to cast " + p_target.name + " to UIElementView " + ex.Message);
					break;
				}
			case "social.friend.add-friend-button@success":
				if (mSubmenuInitialized)
				{
					List<string> newFriendsIds = p_data[0] as List<string>;
					mSubMenuController.OnFriendAddSuccess(newFriendsIds);
				}
				break;
			case "service.social.friends.invite@fail":
				if (mSubmenuInitialized)
				{
					GameFriendData gameFriendData = (GameFriendData)p_data[0];
					Debug.LogWarning("ADDInvite failed for non-friend " + gameFriendData.platformId);
					if (gameFriendData != null)
					{
						mSubMenuController.OnFriendAddFail(gameFriendData.platformId);
					}
				}
				break;
			case "chat.friend-remove":
				if (!mSubmenuInitialized)
				{
					break;
				}
				{
					foreach (string item in p_data[0] as List<string>)
					{
						Debug.LogWarning("Friend removed id=" + item);
						mSubMenuController.OnFriendRemove(item);
					}
					break;
				}
			case "chat.channnel.player.joined":
				if (mSubmenuInitialized && p_data.Length >= 2)
				{
					string obj2 = (string)p_data[0];
					string p_id2 = (string)p_data[1];
					if (!(obj2 != "global-chat"))
					{
						mSubMenuController.OnUserConnected(p_id2);
					}
				}
				break;
			case "chat.channnel.player.left":
				if (mSubmenuInitialized && p_data.Length >= 2)
				{
					string obj = (string)p_data[0];
					string p_id = (string)p_data[1];
					if (!(obj != "global-chat"))
					{
						mSubMenuController.OnUserDisconnected(p_id);
					}
				}
				break;
			}
		}
	}
}

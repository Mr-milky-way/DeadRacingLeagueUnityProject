using System.Collections.Generic;
using UnityEngine;

namespace drl.game
{
	public class UIChatSubmenuController : UIBaseSubmenuController
	{
		private UIChatSubmenuData mData;

		public new UIChatSubmenuView view => AssertLocal<UIChatSubmenuView>("view");

		public override void Setup<T>(T configData)
		{
			base.Setup(configData);
			mData = configData as UIChatSubmenuData;
			if (mData != null)
			{
				view.EnablePrivateMessageButton(mData.isOnline);
				view.EnableAddFriendButton(!mData.isFriend);
			}
		}

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "social.friend.pm-button@click":
				if (base.IsOpen)
				{
					Debug.LogWarning("Starting private msg with " + mData.steamId);
					StartPrivateChat();
					Fold();
				}
				break;
			case "social.friend.add-friend-button@click":
				if (base.IsOpen)
				{
					string text = (string)p_data[0];
					Debug.LogWarning("Starting AddFriend for userId= " + text);
					if (!string.IsNullOrEmpty(text))
					{
						base.app.model.service.social.friends.Add(text);
						view.EnableAddFriendButton(show: false);
					}
				}
				break;
			case "chat.ui.submenu.unfold":
				if (!((UIBaseSubmenuView)p_data[0] == view) && (base.IsOpen || view.Opening))
				{
					Fold(0f);
				}
				break;
			}
		}

		public void OnFriendRemove(string exFriendSteamId)
		{
			Debug.LogWarning("Friend removed id=" + exFriendSteamId);
			if (exFriendSteamId == mData.steamId)
			{
				mData.isFriend = false;
				view.EnableAddFriendButton(show: true);
			}
		}

		public void OnFriendAddFail(string noFriendSteamId)
		{
			Debug.LogWarning("ADDInvite failed for non-friend " + noFriendSteamId);
			if (noFriendSteamId == mData.steamId)
			{
				mData.isFriend = false;
				view.EnableAddFriendButton(show: true);
			}
		}

		public void OnFriendAddSuccess(List<string> newFriendsIds)
		{
			foreach (string newFriendsId in newFriendsIds)
			{
				Debug.LogWarning("FriendAddSuccess. Checking id:" + newFriendsId + " to match message id:" + mData.steamId);
				if (newFriendsId == mData.steamId)
				{
					mData.isFriend = true;
					view.EnableAddFriendButton(show: false);
					break;
				}
			}
		}

		public void OnUserConnected(string p_id)
		{
			if (!(mData.steamId != p_id))
			{
				mData.isOnline = true;
				view.EnablePrivateMessageButton(show: true);
			}
		}

		public void OnUserDisconnected(string p_id)
		{
			if (!(mData.steamId != p_id))
			{
				mData.isOnline = false;
				view.EnablePrivateMessageButton(show: false);
			}
		}

		public void StartPrivateChat()
		{
			Notify("chat.private.invite", mData.steamId, mData.userName, mData.photoURL, mData.color);
		}

		public void AddFriend()
		{
			Notify("social.friend.add-friend-button@click", mData.steamId);
		}

		public void BlockUser()
		{
			if (mData != null)
			{
				base.app.model.service.platform.SetUserSessionBlocked(mData.steamId, p_flag: true);
				SetUserPersistentBlocked(mData.steamId);
			}
		}

		public void UnblockUser()
		{
			if (mData != null)
			{
				base.app.model.service.platform.SetUserSessionBlocked(mData.steamId, p_flag: false);
				SetUserPersistentUnBlocked(mData.steamId);
			}
		}

		public void SetUserPersistentBlocked(string p_playerID)
		{
			Debug.Log("UIChatSubmenuController-> SetUserPersistentBlocked: " + p_playerID);
			base.app.model.service.platform.SetUserSessionBlocked(p_playerID, p_flag: true);
			List<string> blockedUsers = base.app.model.storage.state.player.blockedUsers;
			blockedUsers.Add(p_playerID);
			base.app.model.storage.state.player.blockedUsers = blockedUsers;
		}

		public void SetUserPersistentUnBlocked(string p_playerID)
		{
			Debug.Log("UIChatSubmenuController-> SetUserPersistentUnBlocked: " + p_playerID);
			List<string> blockedUsers = base.app.model.storage.state.player.blockedUsers;
			blockedUsers.Remove(p_playerID);
			base.app.model.storage.state.player.blockedUsers = blockedUsers;
			base.app.model.service.platform.SetUserSessionBlocked(p_playerID, p_flag: false);
		}

		public void UnblockAllUsers()
		{
			List<string> blockedUsers = base.app.model.storage.state.player.blockedUsers;
			blockedUsers.Clear();
			base.app.model.storage.state.player.blockedUsers = blockedUsers;
		}

		public bool GetUserBlocked()
		{
			if (!base.app.model.service.platform.GetUserSessionBlocked(mData.steamId))
			{
				return base.app.model.storage.state.player.blockedUsers.Contains(mData.steamId);
			}
			return true;
		}
	}
}

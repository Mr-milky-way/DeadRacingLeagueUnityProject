using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using thelab.mvc;

namespace drl.game
{
	public class UIChatBlockedUserController : Controller<DRLApp>
	{
		private string mPlayerId;

		private bool mInitialized;

		public UIChatBlockedUserView view => AssertLocal<UIChatBlockedUserView>("view");

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

		public void Init(DRLPlayerProfileData p_player, bool p_backgroundColor = false)
		{
			view.title = p_player.name;
			MPlayerId = p_player.platformId;
			view.usernameField.text = p_player.name;
			view.userColor = p_player.profileColor;
			view.LoadPhoto(p_player.playerId);
			mInitialized = true;
			SetBackgroundColor(p_backgroundColor);
		}

		public void Reset()
		{
			mInitialized = false;
			MPlayerId = null;
			view.title = null;
			view.userColor = Color.white;
			SetBackgroundColor(p_active: false);
		}

		private void SetBackgroundColor(bool p_active)
		{
			if (!(view.background == null))
			{
				view.background.enabled = p_active;
			}
		}

		public void UnblockUser()
		{
			if (MPlayerId != null)
			{
				base.app.model.service.platform.SetUserSessionBlocked(MPlayerId, p_flag: false);
				SetUserPersistentUnBlocked(MPlayerId);
			}
		}

		public void SetUserPersistentUnBlocked(string p_playerID)
		{
			List<string> blockedUsers = base.app.model.storage.state.player.blockedUsers;
			blockedUsers.Remove(p_playerID);
			base.app.model.storage.state.player.blockedUsers = blockedUsers;
		}
	}
}

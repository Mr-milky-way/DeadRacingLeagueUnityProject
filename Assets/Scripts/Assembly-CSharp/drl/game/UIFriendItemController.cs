using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class UIFriendItemController : UIUserItemController
	{
		private Component m_nextDown;

		public new UIFriendItemView view => AssertLocal<UIFriendItemView>("view");

		public RectTransform rectTransform => AssertLocal<RectTransform>("rectTransform");

		public UIPanelFriendsView friendsPanel => Hierarchy.FindReverse<UIPanelFriendsView>(base.transform);

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "social.friend.item@open":
				ShowMenu();
				break;
			case "social.friend.item@close":
				HideMenu(0.01f);
				break;
			}
		}

		public void ToggleSubmenu(float p_duration = 0.3f)
		{
			Notify("social.friend.item@click");
			if (view.submenuOpened)
			{
				view.SubmenuFold(p_duration);
				return;
			}
			friendsPanel.CloseInactiveSubmenus(view);
			view.SubmenuUnfold(p_duration);
		}

		public void PrivateChatClicked()
		{
			if (base.validContext && view.gameFriendData != null && view.gameFriendData.ingame)
			{
				HideMenu();
				Debug.Log("PrivateChatClicked()" + view.gameFriendData.name);
				Notify("social.friend.pm-button@click", view.gameFriendData.platformId, view.gameFriendData.name, view.gameFriendData.profileThumbURL, view.gameFriendData.color);
			}
		}

		public void ShowMenu(float p_duration = 0.3f)
		{
			if (!view.submenuOpened)
			{
				view.SubmenuUnfold(p_duration);
			}
		}

		public void HideMenu(float p_duration = 0.3f)
		{
			if (view.submenuOpened)
			{
				view.SubmenuFold(p_duration);
			}
		}

		public void Remove()
		{
			Notify("social.friend.remove-friend-button@click", view.gameFriendData.platformId);
		}
	}
}

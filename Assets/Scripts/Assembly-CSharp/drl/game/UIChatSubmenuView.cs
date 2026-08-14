using UnityEngine;
using UnityEngine.UI;
using thelab.mvc;

namespace drl.game
{
	public class UIChatSubmenuView : UIBaseSubmenuView
	{
		public CanvasGroup privateMessageCanvasGroup;

		public CanvasGroup addFriendCanvasGroup;

		public CanvasGroup blockCanvasGroup;

		public UIElementView privateChatButton;

		public UIElementView addFriendButton;

		public UIElementView blockButton;

		public Text blockText;

		public void EnablePrivateMessageButton(bool show)
		{
			privateMessageCanvasGroup.alpha = (show ? 1f : 0.3f);
			privateChatButton.interactable = show;
		}

		public void EnableAddFriendButton(bool show)
		{
			addFriendCanvasGroup.alpha = (show ? 1f : 0.3f);
			addFriendButton.interactable = show;
		}

		public void EnableBlockButton(bool p_flag)
		{
			blockCanvasGroup.alpha = (p_flag ? 1f : 0.3f);
			blockButton.interactable = p_flag;
		}

		public void SetBlockButton(bool p_flag)
		{
			blockText.text = base.app.model.storage.locale.Get(p_flag ? "social.chat.submenu.blocked" : "social.chat.submenu.unblocked", p_flag ? "BLOCK" : "UNBLOCK");
		}
	}
}

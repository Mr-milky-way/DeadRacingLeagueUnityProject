using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class UIChatBlockedController : Controller<DRLApp>
	{
		private bool mIsReady;

		public NetworkModel roomChatModel => base.app.model.network;

		public ChatModel chatModel => base.app.model.chat;

		public SocialModel socialModel => base.app.model.service.social;

		public UIChatBlockedView view => AssertLocal<UIChatBlockedView>("view");

		protected override void Start()
		{
			base.Start();
			mIsReady = true;
		}

		public override async void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (mIsReady && p_event != null)
			{
				switch (p_event)
				{
				case "social.panel.shown":
					Debug.Log("Chat SocialPanelShown");
					LoadBlockedUsers();
					break;
				case "chat.block-user@click":
					Debug.Log("Chat BlockUser");
					LoadBlockedUsers();
					break;
				case "chat.unblock-user@click":
					Debug.Log("Chat UnBlockUser");
					LoadBlockedUsers();
					break;
				}
			}
		}

		private void LoadBlockedUsers()
		{
			Debug.Log("UIChatBlockedController-> LoadBlockedUsers()");
			view.messagesList.Clear();
			view.LoadBlockedUsers();
		}
	}
}

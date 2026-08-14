using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class UISocialTemplatePicker : Controller<DRLApp>
	{
		public UISocialView socialGame;

		public UISocialView socialMenu;

		private bool m_communcationBlocked;

		private new void Start()
		{
			socialMenu.isActive = false;
		}

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "ui.footer@open":
				socialGame.isActive = false;
				socialMenu.gameObject.SetActive(!m_communcationBlocked);
				socialMenu.isActive = true;
				break;
			case "ui.footer@close":
				socialMenu.Hide();
				socialMenu.isActive = false;
				socialMenu.gameObject.SetActive(value: false);
				socialGame.isActive = true;
				socialGame.chat.messagesList.Clear();
				socialGame.chat.LoadMessages();
				break;
			case "game.intro.animation@start":
				socialGame.Hide();
				socialGame.isActive = false;
				break;
			case "game.intro.animation@complete":
				socialGame.Show(0f);
				socialGame.Hide(0f);
				socialGame.isActive = true;
				break;
			case "chat.toggle":
				socialGame.gameObject.SetActive(!socialGame.gameObject.activeSelf && !m_communcationBlocked);
				break;
			}
		}
	}
}

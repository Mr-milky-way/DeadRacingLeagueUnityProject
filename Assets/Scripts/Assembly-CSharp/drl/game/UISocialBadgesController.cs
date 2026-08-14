using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UISocialBadgesController : Controller<DRLApp>
	{
		private bool m_chatTabDirty;

		private bool m_friendsTabDirty;

		public UISocialBadgesView view => AssertLocal<UISocialBadgesView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (!view.socialView.isActive || view.socialView.open)
			{
				return;
			}
			switch (p_event)
			{
			case "social.badges.dirty":
				if (!view.initialized)
				{
					this.TimerRunOnce(delegate
					{
						view.initialized = true;
					}, 3f);
				}
				else
				{
					view.SetSocialButtonGlobalDirty();
				}
				break;
			case "social.panel.shown":
				view.SetSocialButtonGlobalClear();
				view.initialized = true;
				break;
			}
		}
	}
}

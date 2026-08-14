using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class UISubmenuController : Controller<DRLApp>
	{
		public virtual UISubmenuView view => AssertLocal<UISubmenuView>("view");

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

		public void ToggleSubmenu(float p_duration = 0.3f)
		{
			Notify("social.friend.item@click", view, p_duration);
		}
	}
}

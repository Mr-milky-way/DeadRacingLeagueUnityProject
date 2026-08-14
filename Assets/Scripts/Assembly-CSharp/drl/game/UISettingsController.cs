using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UISettingsController : Controller<DRLApp>
	{
		public UISettingsView view => AssertLocal<UISettingsView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event != null && p_event == "ui.screen@close")
			{
				_ = Reflection<object>.Get<UIScreen>(p_data, 0) != view.screen;
			}
			if (base.app.view.ui.screens.current != view.screen || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				if (!(p_data[0] as UIScreen != view.screen))
				{
					view.Show();
				}
				break;
			case "settings.system@click":
				base.app.view.ui.screens.Open("settings-system-screen", 0f);
				break;
			case "settings.tuning@click":
				base.app.view.ui.screens.Open("settings-tuning-screen", 0f);
				break;
			case "settings.game@click":
				base.app.view.ui.screens.Open("settings-game-screen", 0f);
				break;
			case "settings.controller@click":
				base.app.view.ui.screens.Open("calibration-menu-screen", 0f);
				break;
			case "settings.help@click":
				WebBrowser.OpenURL("https://drlracingsimulator.zendesk.com/hc/en-us", (base.app != null) ? base.app.model.service.platform : null);
				break;
			case "settings.legal@click":
				WebBrowser.OpenURL("https://thedroneracingleague.com/privacy-policy/", (base.app != null) ? base.app.model.service.platform : null);
				break;
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				if (base.app.arguments.game.type == GameFlag.MapEditor)
				{
					base.app.view.ui.screens.controller.BlockDark();
				}
				break;
			}
		}
	}
}

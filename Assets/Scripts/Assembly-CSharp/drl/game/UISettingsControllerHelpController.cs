using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UISettingsControllerHelpController : Controller<DRLApp>
	{
		public UISettingsControllerHelpView view => AssertLocal<UISettingsControllerHelpView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (view.current)
			{
				switch (p_event)
				{
				case "ui.screen@open":
					UINavigation.Link(view.GridLayoutGroup, view.leftNavigation);
					break;
				case "settings.controller.help@click":
				{
					UIElementView uIElementView = p_target as UIElementView;
					string text = ((uIElementView != null) ? uIElementView.GetComponent<StringTag>().tags[0] : null);
					WebBrowser.OpenURL(string.IsNullOrEmpty(text) ? "https://drlracingsimulator.zendesk.com/hc/en-us/sections/115000302251-Controller-List-and-Setup" : text, (base.app != null) ? base.app.model.service.platform : null);
					break;
				}
				case "ui.screen.return@click":
					base.app.view.ui.screens.Return();
					break;
				}
			}
		}
	}
}

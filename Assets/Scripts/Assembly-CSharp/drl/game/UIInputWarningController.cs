using UnityEngine;
using drl.sim.rci;
using thelab.mvc;

namespace drl.game
{
	public class UIInputWarningController : Controller<DRLApp>
	{
		public UIInputWarningView view => AssertLocal<UIInputWarningView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (!view.current)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			case "settings.controller.connect":
				if (RCI.HasControllersConnected())
				{
					base.app.view.ui.screens.Return();
				}
				break;
			case "input.help@click":
				WebBrowser.OpenURL("https://drlracingsimulator.zendesk.com/hc/en-us", (base.app != null) ? base.app.model.service.platform : null);
				break;
			}
		}
	}
}

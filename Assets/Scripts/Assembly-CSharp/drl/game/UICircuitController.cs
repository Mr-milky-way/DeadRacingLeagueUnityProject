using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UICircuitController : Controller<DRLApp>
	{
		public UICircuitView view => AssertLocal<UICircuitView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				if (!(p_data[0] as UIScreen != view.screen))
				{
					view.depth = 2;
				}
				break;
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			}
		}
	}
}

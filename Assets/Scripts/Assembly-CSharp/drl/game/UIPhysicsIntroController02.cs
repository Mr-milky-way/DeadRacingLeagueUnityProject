using System;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIPhysicsIntroController02 : Controller<DRLApp>
	{
		public UIPhysicsIntroView02 view => AssertLocal<UIPhysicsIntroView02>("view");

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (!view.current)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				base.app.view.ui.SetDark(p_flag: false);
				RunOnce(1f / 30f, delegate
				{
					base.app.view.ui.footer.Hide(0f);
					base.app.controller.game.FadeBlur(0f, 0f);
				});
				UINavigation.focus = view.rightNavigation;
				break;
			case "intro.screens.close":
				base.app.view.ui.screens.CloseAllScreens();
				break;
			case "intro.calibration@open":
				throw new Exception("UIPhysicsIntroController01 line 64,  change it over to the new calibration system");
			}
		}
	}
}

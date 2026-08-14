using System;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIPhysicsIntroController01 : Controller<DRLApp>
	{
		public UIPhysicsIntroView01 view => AssertLocal<UIPhysicsIntroView01>("view");

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (!view.current)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				RunOnce(1f / 30f, delegate
				{
					base.app.view.ui.footer.Hide(0f);
				});
				RunOnce(0.5f, delegate
				{
					UINavigation.focus = view.rightNavigation;
				});
				break;
			case "intro.screens.close":
				base.app.view.ui.screens.CloseAllScreens();
				break;
			case "intro.calibration@open":
				throw new Exception("UIPhysicsIntroController01 line 64,  change it over to the new calibration system");
			case "fn.intro.controller-store@click":
				WebBrowser.OpenURL(FindController(p_target.name).GetComponent<StringTag>().tags[0], (base.app != null) ? base.app.model.service.platform : null);
				break;
			}
		}

		private GameObject FindController(string p_name)
		{
			foreach (GameObject controller in view.controllers)
			{
				if (controller.name == p_name)
				{
					return controller;
				}
			}
			return null;
		}
	}
}

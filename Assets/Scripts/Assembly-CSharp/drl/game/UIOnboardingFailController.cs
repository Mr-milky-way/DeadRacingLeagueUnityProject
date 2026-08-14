using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class UIOnboardingFailController : Controller<DRLApp>
	{
		public UIOnboardingFailView view => AssertLocal<UIOnboardingFailView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen)
			{
				return;
			}
			switch (p_event)
			{
			case "onboarding.failed.training@click":
				Notify("onboarding.restart.training");
				break;
			case "onboarding.failed.race-restart@click":
				if (base.app.inGame)
				{
					base.app.view.audio.PlayUIGenericSuccess();
					base.app.controller.game.Restart();
				}
				break;
			}
		}
	}
}

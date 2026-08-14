using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class UIOnboardingOrientationController : Controller<DRLApp>
	{
		public UIOnboardingOrientationView view => AssertLocal<UIOnboardingOrientationView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (!(base.app.view.ui.screens.current != view.screen) && p_event != null && p_event == "onboarding.orientation.exit@click")
			{
				base.app.model.onboarding.hasFinishedOrientation = true;
				Notify("onboarding.stop");
			}
		}
	}
}

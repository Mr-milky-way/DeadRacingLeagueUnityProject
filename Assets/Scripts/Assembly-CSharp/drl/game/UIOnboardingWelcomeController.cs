using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class UIOnboardingWelcomeController : Controller<DRLApp>
	{
		public UIOnboardingWelcomeView view => AssertLocal<UIOnboardingWelcomeView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (!(base.app.view.ui.screens.current != view.screen) && base.app.inOnboarding && p_event != null && p_event == "ui.screen@open")
			{
				DRLOnboarding activeOnboarding = base.app.model.onboarding.activeOnboarding;
				if (!(activeOnboarding == null))
				{
					view.Set(activeOnboarding);
				}
			}
		}
	}
}

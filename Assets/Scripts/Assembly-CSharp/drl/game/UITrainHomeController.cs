using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class UITrainHomeController : Controller<DRLApp>
	{
		public UITrainHomeView view => AssertLocal<UITrainHomeView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (!(base.app.view.ui.screens.current != view.screen))
			{
				switch (p_event)
				{
				case "ui.screen@open":
					base.app.model.onboarding.activeOnboarding = null;
					break;
				case "ui.screen.return@click":
					base.app.view.ui.screens.Return(1);
					break;
				case "onboarding.enter.menu@click":
				{
					UIOnobardingMenuView uIOnobardingMenuView = base.app.view.ui.screens.Open<UIOnobardingMenuView>("onboarding-home-screen");
					uIOnobardingMenuView.backNav.gameObject.SetActive(value: true);
					uIOnobardingMenuView.resetButtonNav.gameObject.SetActive(base.app.model.onboarding.hasProgress);
					uIOnobardingMenuView.skipButton.gameObject.SetActive(base.app.model.onboarding.hasProgress);
					break;
				}
				case "missions.enter.menu@click":
					base.app.view.ui.screens.Open("quests-screen", 0f);
					break;
				}
			}
		}
	}
}

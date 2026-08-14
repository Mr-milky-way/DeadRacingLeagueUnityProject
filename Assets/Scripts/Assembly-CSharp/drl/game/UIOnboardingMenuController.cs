using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class UIOnboardingMenuController : Controller<DRLApp>
	{
		public UIOnobardingMenuView view => AssertLocal<UIOnobardingMenuView>("view");

		public DRLOnboardingModel model => base.app.model.onboarding;

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen)
			{
				view.videoPlayer.Stop();
				base.app.view.audio.StopOnboardingIntro();
			}
			else if (p_event != null)
			{
				switch (p_event)
				{
				case "ui.screen@open":
					model.RefreshCompletedSteps();
					view.Set(model);
					model.firstStart = false;
					break;
				case "ui.screen.return@click":
					base.app.view.ui.screens.Return(1);
					view.videoPlayer.Stop();
					break;
				case "onboarding.video.click@click":
					view.videoPlayer.Stop();
					view.PlayVideo();
					break;
				case "onboarding.progress@click":
					view.videoPlayer.Stop();
					break;
				case "onboarding.back.home@click":
					view.videoPlayer.Stop();
					base.app.view.ui.screens.Open<UITrainHomeView>("train-menu-screen", 0.3f);
					break;
				case "onboarding.start.beginner@click":
					view.videoPlayer.Stop();
					break;
				case "onboarding.start.intermediate@click":
					view.videoPlayer.Stop();
					break;
				case "onboarding.start.pro@click":
					view.videoPlayer.Stop();
					break;
				case "onboarding.skip@click":
					view.videoPlayer.Stop();
					break;
				case "onboarding.stop":
					base.app.view.ui.screens.Return();
					break;
				}
			}
		}
	}
}

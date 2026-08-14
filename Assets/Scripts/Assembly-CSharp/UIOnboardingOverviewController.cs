using UnityEngine;
using drl.game;
using thelab.core;
using thelab.mvc;

public class UIOnboardingOverviewController : Controller<DRLApp>
{
	public UIOnboardingOverviewView view => AssertLocal<UIOnboardingOverviewView>("view");

	public DRLOnboardingController controller => base.app.controller.onboarding;

	public DRLOnboardingModel model => base.app.model.onboarding;

	public override void OnNotification(string p_event, Object p_target, params object[] p_data)
	{
		if (base.app.view.ui.screens.current != view.screen)
		{
			return;
		}
		switch (p_event)
		{
		case "ui.screen@open":
		{
			view.columns.SetActive(value: true);
			UINavigationScroll component = GetComponent<UINavigationScroll>();
			controller.GetBotData(model.activeOnboarding.mode);
			model.hasFailed = false;
			if ((bool)component)
			{
				component.forceScrollX = true;
			}
			model.GetProgress();
			model.GetProgress(base.app.model.onboarding.activeOnboarding.mode);
			view.SetPage();
			view.SetStatus();
			view.roomStatusField.fade.FadeOut(0f);
			this.TimerRunOnce(delegate
			{
				UINavigation.Focus(view.missionListField.GetComponentInChildren<UINavigation>());
			}, 0.5f);
			break;
		}
		case "missions.mission-card@click":
		{
			view.roomStatusField.fade.FadeOut(0.2f, 0.5f);
			model.hasFailed = false;
			view.SetWidthZero();
			UICardButtonMission uICardButtonMission = p_target as UICardButtonMission;
			view.onboardingModel.SetOnboardingActive(uICardButtonMission.onboardingCampaignMode);
			model.currentStep = uICardButtonMission.onboardinStep - 1;
			model.SetActiveStep(model.currentStep);
			OnboardingStep stepModel = model.activeOnboarding.steps[model.currentStep];
			base.app.view.ui.screens.current.position = new Vector2(0f, base.app.view.ui.screens.current.position.y);
			view.columns.SetActive(value: false);
			if (model.IsMissionStep(model.currentStep, model.activeOnboarding))
			{
				base.app.view.ui.fade.FadeIn(0.5f, 0f);
				controller.StartStep(model.currentStep);
			}
			else
			{
				controller.GetBotData(model.activeOnboarding.mode);
				controller.LoadBots(stepModel, view.roomStatusField);
				view.SetStatus();
			}
			break;
		}
		case "onboarding.back.home@click":
			if (base.app.inGame)
			{
				base.app.view.ui.screens.Open<UIOnboardingOverviewController>("onboarding-home-screen", 0.3f);
			}
			else
			{
				Notify("ui.screen.return@click");
			}
			break;
		case "ui.screen.return@click":
			view.onboardingData = null;
			base.app.view.ui.screens.Return();
			break;
		}
	}
}

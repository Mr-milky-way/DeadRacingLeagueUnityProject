using System.Collections.Generic;
using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class UIOnboardingCompleteController : Controller<DRLApp>
	{
		public UIOnboardingCompleteView view => AssertLocal<UIOnboardingCompleteView>("view");

		public DRLOnboardingController controller => base.app.controller.onboarding;

		public DRLOnboardingModel model => base.app.model.onboarding;

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen)
			{
				base.app.view.audio.StopOnboardingEnd();
				return;
			}
			DRLOnboardingModel dRLOnboardingModel = base.app.controller.onboarding.model;
			switch (p_event)
			{
			case "ui.screen@open":
				dRLOnboardingModel.loading = false;
				view.SetUI();
				controller.GetBotData(dRLOnboardingModel.activeOnboarding.mode);
				SetStatus();
				break;
			case "onboarding.video.click@click":
				if (dRLOnboardingModel.activeOnboarding.mode == OnboardingCampaignMode.Pro)
				{
					view.videoPlayer.Stop();
					view.PlayVideo();
				}
				break;
			case "onboarding.complete.next@click":
			{
				view.videoPlayer.Stop();
				base.app.view.audio.StopOnboardingEnd();
				dRLOnboardingModel.loading = true;
				Notify("onboarding.progress@increase");
				OnboardingStep stepModel = base.app.model.onboarding.activeOnboarding.steps[base.app.model.onboarding.currentStep];
				List<OnboardingStep> steps = base.app.model.onboarding.activeOnboarding.steps;
				int currentStep = base.app.model.onboarding.currentStep;
				if (dRLOnboardingModel.activeOnboarding.steps[currentStep].type == OnboardingStep.OnboardingStepType.Race && steps.Count - 1 == currentStep && !view.isLastRace)
				{
					base.app.controller.onboarding.LoadBots(stepModel, view.roomStatusField);
					SetStatus();
					break;
				}
				base.app.view.ui.fade.FadeIn(0.5f);
				switch (dRLOnboardingModel.activeOnboarding.mode)
				{
				case OnboardingCampaignMode.Beginner:
					Notify("onboarding.start.intermediate@click");
					base.app.view.ui.fade.FadeOut(1f, 0.5f);
					break;
				case OnboardingCampaignMode.Intermediate:
					Notify("onboarding.start.pro@click");
					base.app.view.ui.fade.FadeOut(1f, 0.5f);
					break;
				case OnboardingCampaignMode.Pro:
					dRLOnboardingModel.hasFinishedOrientation = true;
					Notify("onboarding.skip@click");
					break;
				}
				break;
			}
			case "ui.screen.preview@click":
				view.videoPlayer.Stop();
				base.app.view.audio.StopOnboardingEnd();
				break;
			case "onboarding.back.overview@click":
				view.videoPlayer.Stop();
				base.app.view.audio.StopOnboardingEnd();
				break;
			}
		}

		public void SetStatus()
		{
			OpponentModel opponent = base.app.model.service.opponent;
			view.roomStatusField.fade.FadeIn(0.1f, 0.1f);
			switch (opponent.status)
			{
			case OpponentModel.Status.Error:
				view.roomStatusField.SetWarning("LOADING FAILED!");
				view.roomStatusField.fade.FadeOut(0.2f, 0.5f);
				base.app.view.audio.PlayUIGenericError();
				break;
			case OpponentModel.Status.NoResults:
				view.roomStatusField.SetWarning("NO OPPONENTS FOUND!");
				break;
			case OpponentModel.Status.Progress:
			{
				float loading = opponent.progress * 100f;
				view.roomStatusField.SetLoading(loading);
				break;
			}
			case OpponentModel.Status.Complete:
				view.roomStatusField.SetLoading(1f);
				view.roomStatusField.fade.FadeOut(0.2f, 0.5f);
				break;
			case OpponentModel.Status.ManifestSuccess:
				view.roomStatusField.SetLoading(0f);
				base.app.view.audio.PlayUIGenericSuccess();
				break;
			case OpponentModel.Status.None:
				view.roomStatusField.fade.FadeOut(0f);
				break;
			case OpponentModel.Status.ByPass:
				break;
			}
		}
	}
}

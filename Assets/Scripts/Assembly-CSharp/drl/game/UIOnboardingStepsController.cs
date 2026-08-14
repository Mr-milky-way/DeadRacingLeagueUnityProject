using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class UIOnboardingStepsController : Controller<DRLApp>
	{
		private OnboardingStep stepModel;

		public UIOnboardingStepsView view => AssertLocal<UIOnboardingStepsView>("view");

		public GameController game => base.app.controller.game;

		public DRLOnboardingModel model => base.app.model.onboarding;

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				model.SetOnboardingActive(base.app.controller.onboarding.selectedDifficulty);
				base.app.controller.onboarding.GetBotData(model.activeOnboarding.mode);
				SetStatus();
				if (base.app.model.onboarding.hasFailed)
				{
					Notify("onboarding.failed.training@click");
				}
				break;
			case "ui.screen.return@click":
				view.onboardingData = null;
				base.app.view.ui.screens.Return();
				break;
			case "onboarding.failed.training@click":
				view.SetMarkers(missionComplete: false);
				view.SetFailUI(model);
				break;
			case "onboarding.skip@click":
				view.SetButtonsInactive();
				model.hasFailed = false;
				base.app.controller.onboarding.StopOnboarding();
				break;
			case "onboarding.failed.race-restart@click":
				view.SetButtonsInactive();
				stepModel = model.activeOnboarding.steps[model.currentStep];
				base.app.controller.onboarding.LoadBots(stepModel, view.roomStatusField);
				break;
			case "game.race-complete.restart@click":
				view.SetButtonsInactive();
				game.Restart();
				break;
			case "onboarding.missions-complete.next@click":
				view.SetButtonsInactive();
				if (base.app.model.onboarding.IsMissionStep(base.app.model.onboarding.currentStep, base.app.model.onboarding.activeOnboarding))
				{
					base.app.controller.onboarding.StartStep(model.currentStep);
					break;
				}
				base.app.controller.onboarding.GetBotData(base.app.model.onboarding.activeOnboarding.mode);
				stepModel = model.activeOnboarding.steps[model.currentStep];
				base.app.controller.onboarding.LoadBots(stepModel, view.roomStatusField);
				break;
			case "onboarding.step.current@click":
				view.SetButtonsInactive();
				if (base.app.model.onboarding.IsMissionStep(base.app.model.onboarding.currentStep, base.app.model.onboarding.activeOnboarding))
				{
					base.app.controller.onboarding.StartStep(model.currentStep);
					break;
				}
				base.app.controller.onboarding.GetBotData(base.app.model.onboarding.activeOnboarding.mode);
				stepModel = model.activeOnboarding.steps[model.currentStep];
				base.app.controller.onboarding.LoadBots(stepModel, view.roomStatusField);
				break;
			}
		}

		public void SetStatus()
		{
			OpponentModel opponent = base.app.model.service.opponent;
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

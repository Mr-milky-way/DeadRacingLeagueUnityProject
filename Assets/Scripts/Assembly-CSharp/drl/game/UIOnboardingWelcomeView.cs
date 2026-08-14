using UnityEngine.UI;
using drl.sim;
using drl.sim.rci;
using thelab.core;

namespace drl.game
{
	public class UIOnboardingWelcomeView : UIScreenView
	{
		public UINPCOverlay npcOverlay;

		public Text title;

		public Text description;

		public void Set(DRLOnboarding p_onboarding)
		{
			Localization locale = base.app.model.storage.locale;
			SetNPCState(NPCStateType.Controller0, p_is_left: false, RCI.GetControllerStateType(ControllerStateType.Nikko));
			switch (p_onboarding.mode)
			{
			case OnboardingCampaignMode.Beginner:
				title.text = locale.Get("onboarding.welcome.beginner.title", "BEGINNER ONBOARDING");
				description.text = locale.Get("onboarding.welcome.beginner.description", "Welcome to beginner onboarding.");
				break;
			case OnboardingCampaignMode.Intermediate:
				title.text = locale.Get("onboarding.welcome.intermediate.title", "INTERMEDIATE ONBOARDING");
				description.text = locale.Get("onboarding.welcome.intermediate.description", "Welcome to beginner onboarding.");
				break;
			case OnboardingCampaignMode.Pro:
				title.text = locale.Get("onboarding.welcome.pro.title", "PRO ONBOARDING");
				description.text = locale.Get("onboarding.welcome.pro.description", "Welcome to beginner onboarding.");
				break;
			}
		}

		public void SetNPCState(NPCStateType p_type, bool p_is_left, ControllerStateType p_controller)
		{
			npcOverlay.controller = p_controller;
			npcOverlay.SetState(p_type, p_is_left);
		}
	}
}

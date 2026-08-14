using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using thelab.core;

namespace drl.game
{
	public class UIOnobardingMenuView : UIScreenView
	{
		public UINavigation resetButtonNav;

		public UINavigation resetButtonConfirmNav;

		public Text resetButtonConfirmText;

		public UINavigation backNav;

		public UINavigation beginnerButton;

		public UINavigation intermediateButton;

		public UINavigation proButton;

		public UINavigation skipButton;

		public LayoutGroup leftLayout;

		public LayoutGroup centerLayout;

		public LayoutGroup videoLayout;

		public LayoutGroup rightLayot;

		public VideoPlayer videoPlayer;

		public Text beginnerMissionText;

		public Text beginnerRaceText;

		public UIMarkers beginnerMarkers;

		public Text intermediateMissionText;

		public Text intermediateRaceText;

		public UIMarkers intermediateMarkers;

		public Text proRaceText;

		public Text proMissionText;

		public UIMarkers proMarkers;

		private DRLQuest m_data;

		public DRLOnboardingModel model => base.app.model.onboarding;

		public void Set(DRLOnboardingModel model)
		{
			InitBeginnerButton(model);
			InitIntermediateButton(model);
			InitProButton(model);
			HideProgress(model.hasProgressedOnboarding());
			resetButtonNav.gameObject.SetActive(model.hasProgressedOnboarding());
			if (resetButtonNav.gameObject.activeInHierarchy)
			{
				UINavigation.Link(centerLayout, videoLayout, rightLayot);
				UINavigation.Link(rightLayot, centerLayout, leftLayout);
			}
			else
			{
				UINavigation.Link(centerLayout, videoLayout, rightLayot);
				UINavigation.Link(rightLayot, centerLayout, centerLayout);
			}
		}

		public void HideProgress(bool setActive)
		{
			beginnerMarkers.gameObject.SetActive(setActive);
			intermediateMarkers.gameObject.SetActive(setActive);
			proMarkers.gameObject.SetActive(setActive);
			beginnerMissionText.gameObject.SetActive(setActive);
			beginnerRaceText.gameObject.SetActive(setActive);
			intermediateMissionText.gameObject.SetActive(setActive);
			intermediateRaceText.gameObject.SetActive(setActive);
			proRaceText.gameObject.SetActive(setActive);
			proMissionText.gameObject.SetActive(setActive);
		}

		public void InitBeginnerButton(DRLOnboardingModel model)
		{
			int raceProgress = model.GetRaceProgress(OnboardingCampaignMode.Beginner);
			int missionsProgress = model.GetMissionsProgress(OnboardingCampaignMode.Beginner);
			Localization locale = base.app.model.storage.locale;
			string text = locale.Get<string>("onboarding.missions.title", "MISSIONS");
			string text2 = locale.Get<string>("onboarding.races.title", "RACES");
			model.GetProgress(model.beginnerOnboarding.mode);
			int totalMissionSteps = model.GetTotalMissionSteps(model.beginnerOnboarding);
			beginnerMarkers.Init(model.beginnerOnboarding.steps.Count, model.GetProgress(model.beginnerOnboarding.mode));
			beginnerMissionText.text = text + " (" + missionsProgress + "/" + totalMissionSteps + ")";
			beginnerRaceText.text = text2 + "(" + raceProgress + "/" + model.GetTotalRaceSteps(model.beginnerOnboarding) + ")";
		}

		public void InitIntermediateButton(DRLOnboardingModel model)
		{
			int raceProgress = model.GetRaceProgress(OnboardingCampaignMode.Intermediate);
			int missionsProgress = model.GetMissionsProgress(OnboardingCampaignMode.Intermediate);
			model.GetProgress(model.intermediateOnboarding.mode);
			int totalMissionSteps = model.GetTotalMissionSteps(model.intermediateOnboarding);
			Localization locale = base.app.model.storage.locale;
			string text = locale.Get<string>("onboarding.missions.title", "MISSIONS");
			string text2 = locale.Get<string>("onboarding.races.title", "RACES");
			intermediateMarkers.Init(model.intermediateOnboarding.steps.Count, model.GetProgress(model.intermediateOnboarding.mode));
			intermediateMissionText.text = text + "(" + missionsProgress + "/" + totalMissionSteps + ")";
			intermediateRaceText.text = text2 + "(" + raceProgress + "/" + model.GetTotalRaceSteps(model.intermediateOnboarding) + ")";
		}

		public void InitProButton(DRLOnboardingModel model)
		{
			int raceProgress = model.GetRaceProgress(OnboardingCampaignMode.Pro);
			model.GetProgress(OnboardingCampaignMode.Pro);
			int missionsProgress = model.GetMissionsProgress(OnboardingCampaignMode.Pro);
			Localization locale = base.app.model.storage.locale;
			string text = locale.Get<string>("onboarding.missions.title", "MISSIONS");
			string text2 = locale.Get<string>("onboarding.races.title", "RACES");
			proMarkers.Init(model.proOnboarding.steps.Count + missionsProgress, model.GetProgress(model.proOnboarding.mode));
			proMissionText.text = text + "(" + missionsProgress + "/" + model.GetTotalMissionSteps(model.proOnboarding) + ")";
			proRaceText.text = text2 + "(" + raceProgress + "/" + model.GetTotalRaceSteps(model.proOnboarding) + ")";
		}

		public void HighlightButton(OnboardingCampaignMode difficulty)
		{
			switch (difficulty)
			{
			case OnboardingCampaignMode.Beginner:
				beginnerButton.GetComponent<CanvasGroup>().interactable = true;
				beginnerButton.GetComponent<CanvasGroup>().blocksRaycasts = false;
				beginnerButton.GetComponent<CanvasGroup>().alpha = 1f;
				beginnerButton.Focus();
				intermediateButton.GetComponent<CanvasGroup>().interactable = false;
				intermediateButton.GetComponent<CanvasGroup>().blocksRaycasts = false;
				intermediateButton.GetComponent<CanvasGroup>().alpha = 0.3f;
				proButton.GetComponent<CanvasGroup>().interactable = false;
				proButton.GetComponent<CanvasGroup>().blocksRaycasts = false;
				proButton.GetComponent<CanvasGroup>().alpha = 0.3f;
				break;
			case OnboardingCampaignMode.Intermediate:
				intermediateButton.GetComponent<CanvasGroup>().interactable = true;
				intermediateButton.GetComponent<CanvasGroup>().blocksRaycasts = false;
				intermediateButton.GetComponent<CanvasGroup>().alpha = 1f;
				intermediateButton.Focus();
				beginnerButton.GetComponent<CanvasGroup>().interactable = false;
				beginnerButton.GetComponent<CanvasGroup>().blocksRaycasts = false;
				beginnerButton.GetComponent<CanvasGroup>().alpha = 0.3f;
				proButton.GetComponent<CanvasGroup>().interactable = false;
				proButton.GetComponent<CanvasGroup>().blocksRaycasts = false;
				proButton.GetComponent<CanvasGroup>().alpha = 0.3f;
				break;
			case OnboardingCampaignMode.Pro:
				proButton.GetComponent<CanvasGroup>().interactable = true;
				proButton.GetComponent<CanvasGroup>().blocksRaycasts = false;
				proButton.GetComponent<CanvasGroup>().alpha = 1f;
				proButton.Focus();
				beginnerButton.GetComponent<CanvasGroup>().interactable = false;
				beginnerButton.GetComponent<CanvasGroup>().blocksRaycasts = false;
				beginnerButton.GetComponent<CanvasGroup>().alpha = 0.3f;
				intermediateButton.GetComponent<CanvasGroup>().interactable = false;
				intermediateButton.GetComponent<CanvasGroup>().blocksRaycasts = false;
				intermediateButton.GetComponent<CanvasGroup>().alpha = 0.3f;
				break;
			}
		}

		public void PlayVideo()
		{
			base.app.view.audio.StopOnboardingIntro();
			videoPlayer.GetComponent<CanvasGroup>().alpha = 1f;
			videoPlayer.Play();
			float cachedMusicVolume = base.app.view.audio.volumeMusic;
			base.app.view.audio.volumeMusic = 0f;
			base.app.view.audio.PlayOnboardingIntro();
			Activity.RunOnce(delegate
			{
				base.app.view.audio.volumeMusic = cachedMusicVolume;
			}, (float)videoPlayer.length);
			Activity.RunOnce(delegate
			{
				videoPlayer.GetComponent<CanvasGroup>().alpha = 0f;
			}, (float)videoPlayer.length);
		}
	}
}

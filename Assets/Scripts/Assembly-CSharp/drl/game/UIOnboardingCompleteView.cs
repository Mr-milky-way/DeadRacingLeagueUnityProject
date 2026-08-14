using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using thelab.core;

namespace drl.game
{
	public class UIOnboardingCompleteView : UIScreenView
	{
		public Text missionText;

		public Text nextButton;

		public UINavigation skipButton;

		public UINavigation raceButton;

		public RawImage tonyAvatarImage;

		public RawImage backgroundImage;

		public DRLOnboarding onboardingData;

		public UIStatusView roomStatusField;

		public bool isMissionCompleted;

		public bool isLastRace;

		public VideoPlayer videoPlayer;

		public VideoClip pcVideo;

		public VideoClip consoleVideo;

		public DRLOnboardingModel model => base.app.model.onboarding;

		public void SetUI()
		{
			Localization locale = base.app.model.storage.locale;
			onboardingData = base.app.model.onboarding.activeOnboarding;
			videoPlayer.gameObject.SetActive(value: false);
			if (isMissionCompleted)
			{
				switch (onboardingData.mode)
				{
				case OnboardingCampaignMode.Beginner:
					missionText.text = locale.Get<string>("onboarding.beginner.missions.complete", OnboardingStrings.onboardingBeginnerMissionsComplete);
					break;
				case OnboardingCampaignMode.Intermediate:
					missionText.text = locale.Get<string>("onboarding.intermediate.missions.complete", OnboardingStrings.onboardingIntermediateMissionsComplete);
					break;
				}
			}
			else if (base.app.model.onboarding.currentStep == onboardingData.steps.Count - 1)
			{
				switch (onboardingData.mode)
				{
				case OnboardingCampaignMode.Beginner:
					missionText.text = locale.Get<string>("onboarding.beginner.campaign.complete", OnboardingStrings.onboardingBeginnerComplete);
					break;
				case OnboardingCampaignMode.Intermediate:
					missionText.text = locale.Get<string>("onboarding.intermediate.campaign.complete", OnboardingStrings.onboardingIntermediateComplete);
					break;
				case OnboardingCampaignMode.Pro:
					missionText.text = locale.Get<string>("onboarding.pro.campaign.complete", OnboardingStrings.onboardingProComplete);
					FinishedOnboarding(locale);
					break;
				default:
					missionText.text = missionText.text;
					break;
				}
			}
		}

		public void SetNextButtonRaceText()
		{
			Localization locale = base.app.model.storage.locale;
			nextButton.text = locale.Get<string>("onboarding.race.title", "RACE");
		}

		public void SetNextButtonNextText()
		{
			Localization locale = base.app.model.storage.locale;
			nextButton.text = locale.Get<string>("ui.common.next", "NEXT");
		}

		private void FinishedOnboarding(Localization l)
		{
			model.hasFinishedOrientation = true;
			skipButton.gameObject.SetActive(value: false);
			Activity.RunOnce(delegate
			{
				videoPlayer.gameObject.SetActive(value: true);
				tonyAvatarImage.gameObject.SetActive(value: false);
				videoPlayer.GetComponent<CanvasGroup>().alpha = 1f;
				videoPlayer.clip = pcVideo;
				PlayVideo();
			}, 0.3f);
		}

		public void PlayVideo()
		{
			base.app.view.audio.StopOnboardingEnd();
			videoPlayer.GetComponent<CanvasGroup>().alpha = 1f;
			videoPlayer.Play();
			float cachedMusicVolume = base.app.view.audio.volumeMusic;
			base.app.view.audio.volumeMusic = 0f;
			base.app.view.audio.PlayOnboardingEnd();
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

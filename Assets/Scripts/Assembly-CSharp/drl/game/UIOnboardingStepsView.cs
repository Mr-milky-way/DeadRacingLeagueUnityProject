using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIOnboardingStepsView : UIScreenView
	{
		public ListComponent listField;

		public Sprite npcIdle;

		public Sprite npcFailHat;

		public Image NPCUI;

		public Text UIProgressMissionText;

		public Text UIProgressMissionCountText;

		public Text UIProgressRaceText;

		public Text UIProgressRaceCountText;

		public Text missionDescription;

		public Text missionTitle;

		public Text titleFailText;

		public Text titleCompleteText;

		public Text missionNumberText;

		public Text missionNumberCountText;

		private DRLMission om_data;

		public ListComponent racesListField;

		public UIMarkers missionMarkers;

		public UIMarkers raceMarkers;

		public UIStatusView roomStatusField;

		public GameObject avatarsGroup;

		public GameObject botWinnerIcon;

		public GameObject playerWinnerIcon;

		public UINavigation startButton;

		public UINavigation retryButton;

		public UINavigation nextButton;

		public UINavigation backButton;

		public UINavigation missionsButton;

		public UINavigation playAgainButton;

		public UINavigation exitButton;

		public GameObject missionBar;

		public DRLOnboarding onboardingData;

		public int currentStep;

		public RawImage playerAvatar;

		public RawImage playerPrize;

		public Text playerName;

		public Text playerTime;

		public Text ghostTime;

		private string beginnerTime = "01:30.000";

		private string intermediateTime = "2:00.000";

		private readonly string[] proTime = new string[3] { "1:20.000", "1:20.000", "2:30.000" };

		private string missionString;

		private string raceString;

		private int missionSteps;

		private int raceSteps;

		private int currentMissionStep;

		private int currentRaceStep;

		private DRLQuest _mQuestData;

		public DRLQuest questData
		{
			get
			{
				return _mQuestData;
			}
			set
			{
				_mQuestData = value;
			}
		}

		public DRLOnboardingModel model => base.app.model.onboarding;

		public void Set(DRLOnboarding p_model)
		{
			ClearMissions();
			onboardingData = p_model;
			if (onboardingData == null)
			{
				onboardingData = model.activeOnboarding;
			}
			DRLMission mission = onboardingData.steps[currentStep].mission;
			if (mission != null)
			{
				questData = p_model.steps[currentStep].quest;
			}
			missionDescription.enabled = true;
			titleFailText.gameObject.SetActive(value: false);
			titleCompleteText.gameObject.SetActive(value: false);
			avatarsGroup.SetActive(value: false);
			missionsButton.gameObject.SetActive(value: false);
			missionBar.SetActive(model.GetTotalMissionSteps(onboardingData) >= 1);
			OnboardingStep onboardingStep = onboardingData.steps[currentStep];
			SetStepData();
			roomStatusField.fade.alpha = -0.1f;
			foreach (OnboardingStep step in p_model.steps)
			{
				if (step.type == OnboardingStep.OnboardingStepType.Mission)
				{
					AddMission(step.mission);
				}
				else if (step.type == OnboardingStep.OnboardingStepType.Race)
				{
					AddRace(step.mission);
				}
			}
			Localization locale = base.app.model.storage.locale;
			missionString = locale.Get<string>("onboarding.mission.title", "MISSION");
			raceString = locale.Get<string>("onboarding.race.title", "RACE");
			SetStepTitle(locale, onboardingStep);
			NPCUI.sprite = npcIdle;
			if (mission != null)
			{
				missionDescription.text = mission.description;
			}
			SetBottomBarText(missionString, currentMissionStep, missionSteps, raceString, currentRaceStep, raceSteps);
			SetTrackTitle(onboardingStep);
			SetSubtitle(missionString, currentMissionStep, raceString, currentRaceStep);
			SetAvatars(model);
			SetMarkers(missionComplete: false);
			if (playAgainButton.gameObject.activeSelf)
			{
				nextButton.GetComponent<UINavigation>().down = playAgainButton;
				exitButton.GetComponent<UINavigation>().right = playAgainButton;
				exitButton.GetComponent<UINavigation>().left = playAgainButton;
				if (nextButton.gameObject.activeSelf)
				{
					playAgainButton.GetComponent<UINavigation>().right = exitButton;
					playAgainButton.GetComponent<UINavigation>().left = exitButton;
					playAgainButton.GetComponent<UINavigation>().up = nextButton;
				}
				else
				{
					playAgainButton.GetComponent<UINavigation>().right = backButton;
					playAgainButton.GetComponent<UINavigation>().left = backButton;
				}
			}
		}

		private void SetStepData()
		{
			missionSteps = base.app.controller.onboarding.model.GetTotalMissionSteps(onboardingData);
			raceSteps = base.app.controller.onboarding.model.GetTotalRaceSteps(onboardingData);
			currentMissionStep = model.GetMissionsProgress(model.activeOnboarding.mode);
			currentRaceStep = model.GetCurrentRaceStep(model.activeOnboarding, currentStep) + 1;
		}

		private void SetBottomBarText(string missionString, int currentMissionStep, int missionSteps, string raceString, int currentRaceStep, int raceSteps)
		{
			UIProgressMissionText.text = missionString;
			UIProgressMissionCountText.text = " (" + currentMissionStep + "/" + missionSteps + ")";
			UIProgressRaceText.text = raceString;
			UIProgressRaceCountText.text = " (" + model.GetRaceProgress(model.activeOnboarding.mode) + "/" + raceSteps + ")";
		}

		private void SetSubtitle(string missionString, int currentMissionStep, string raceString, int currentRaceStep)
		{
			if (model.IsMissionStep(model.currentStep, onboardingData))
			{
				missionNumberText.text = missionString;
				missionNumberCountText.text = " (" + (model.currentStep + 1) + " / " + model.GetTotalMissionSteps(model.activeOnboarding) + ")";
			}
			else
			{
				missionNumberText.text = raceString;
				missionNumberCountText.text = " (" + currentRaceStep + " / " + model.GetTotalRaceSteps(model.activeOnboarding) + ")";
			}
		}

		private void SetTrackTitle(OnboardingStep onboardingStep)
		{
			string text;
			if (onboardingStep.trackGuid == null)
			{
				text = "WIP NO GUID";
			}
			else if (onboardingStep.trackGuid.StartsWith("CMP-"))
			{
				text = base.app.model.storage.maps.FindByGUID(onboardingStep.trackGuid).mapTitle;
			}
			else
			{
				DRLMapTrack dRLMapTrack = base.app.model.storage.library.FindByGUID<DRLMapTrack>(onboardingStep.trackGuid);
				text = ((!(dRLMapTrack != null)) ? onboardingStep.mission.map.title : dRLMapTrack.map.title);
			}
			if (onboardingStep.type == OnboardingStep.OnboardingStepType.Mission)
			{
				missionTitle.text = onboardingData.steps[currentStep].mission.title;
				missionDescription.gameObject.SetActive(value: true);
				avatarsGroup.SetActive(value: false);
			}
			else
			{
				missionTitle.text = text;
				missionDescription.gameObject.SetActive(value: false);
				avatarsGroup.SetActive(value: true);
				botWinnerIcon.SetActive(value: false);
				playerWinnerIcon.SetActive(value: false);
			}
			missionTitle.text = missionTitle.text.ToUpper();
			missionTitle.text = missionTitle.text.Replace(",", "");
			missionTitle.text = missionTitle.text.Replace("\r\n", " ");
		}

		private void SetStepTitle(Localization l, OnboardingStep onboardingStep)
		{
			if (onboardingStep.type == OnboardingStep.OnboardingStepType.Mission)
			{
				titleCompleteText.text = l.Get<string>("onboarding.mission.complete", "MISSION COMPLETE");
				titleFailText.text = l.Get<string>("onboarding.mission.failed", "MISSION FAILED");
			}
			else
			{
				titleCompleteText.text = l.Get<string>("onboarding.race.complete", "RACE COMPLETE");
				titleFailText.text = l.Get<string>("onboarding.race.failed", "RACE FAILED");
			}
		}

		public void Set(DRLOnboarding p_model, int selectedIndex)
		{
			currentStep = selectedIndex;
			Set(p_model);
		}

		private void SetAvatars(DRLOnboardingModel model)
		{
			ProfileStateModel profile = base.app.model.storage.state.player.profile;
			playerAvatar.texture = profile.photo;
			playerName.text = profile.username.ToUpper();
			switch (base.app.controller.onboarding.selectedDifficulty)
			{
			case OnboardingCampaignMode.Beginner:
				ghostTime.text = beginnerTime;
				break;
			case OnboardingCampaignMode.Intermediate:
				ghostTime.text = intermediateTime;
				break;
			case OnboardingCampaignMode.Pro:
				ghostTime.text = proTime[currentRaceStep - 1];
				break;
			}
		}

		public void AddMission(DRLMission p_item)
		{
			if ((bool)listField && (bool)p_item && !p_item.gameObject.CompareTag("Intro"))
			{
				UICardButtonMission uICardButtonMission = listField.Push<UICardButtonMission>();
				_ = listField.Count;
				uICardButtonMission.notification = "missions.mission-card";
				uICardButtonMission.Set(p_item);
			}
		}

		public void AddRace(DRLMission p_item)
		{
			if ((bool)racesListField && (bool)p_item && !p_item.gameObject.CompareTag("Intro"))
			{
				UICardButtonMission uICardButtonMission = racesListField.Push<UICardButtonMission>();
				_ = racesListField.Count;
				uICardButtonMission.notification = "missions.mission-card";
				uICardButtonMission.Set(p_item);
			}
		}

		public void ClearMissions()
		{
			if ((bool)listField)
			{
				listField.Clear();
			}
		}

		public void SetFailUI(DRLOnboardingModel model)
		{
			Localization locale = base.app.model.storage.locale;
			base.app.model.onboarding.hasFailed = false;
			NPCUI.sprite = npcFailHat;
			missionDescription.enabled = false;
			titleFailText.gameObject.SetActive(value: true);
			avatarsGroup.SetActive(value: true);
			botWinnerIcon.SetActive(value: true);
			playerWinnerIcon.SetActive(value: false);
			playerName.color = Color.red;
			playerTime.color = Color.red;
			retryButton.gameObject.SetActive(value: true);
			startButton.gameObject.SetActive(value: false);
			if (base.app.controller.onboarding.model.IsMissionStep(model.currentStep, onboardingData))
			{
				missionMarkers.SetRedCurrentStep(model.currentStep);
				titleCompleteText.text = locale.Get<string>("onboarding.mission.complete");
				titleFailText.text = locale.Get<string>("onboarding.mission.failed");
			}
			else
			{
				raceMarkers.SetRedCurrentStep(currentRaceStep);
				titleCompleteText.text = locale.Get<string>("onboarding.race.complete");
			}
		}

		public void SetMarkers(bool missionComplete)
		{
			SetStepData();
			int progress = ((model.GetMissionsProgress(model.activeOnboarding.mode) > model.GetTotalMissionSteps(onboardingData)) ? model.GetTotalMissionSteps(onboardingData) : model.GetMissionsProgress(model.activeOnboarding.mode));
			int raceProgress = model.GetRaceProgress(model.activeOnboarding.mode);
			if (missionComplete)
			{
				missionMarkers.Init(model.GetTotalMissionSteps(onboardingData), progress);
				raceMarkers.Init(model.GetTotalRaceSteps(onboardingData), raceProgress);
			}
			else
			{
				if (model.GetStepType() == OnboardingStep.OnboardingStepType.Mission)
				{
					missionMarkers.Init(model.GetTotalMissionSteps(onboardingData), progress);
				}
				else
				{
					missionMarkers.Init(model.GetTotalMissionSteps(onboardingData), progress);
				}
				raceMarkers.Init(model.GetTotalRaceSteps(onboardingData), raceProgress);
			}
			titleCompleteText.gameObject.SetActive(missionComplete);
			botWinnerIcon.SetActive(!missionComplete);
			playerWinnerIcon.SetActive(missionComplete);
			SetBottomBarText(missionString, currentMissionStep, missionSteps, raceString, currentRaceStep, raceSteps);
			SetSubtitle(missionString, currentMissionStep, raceString, currentRaceStep);
			nextButton.gameObject.SetActive(missionComplete);
		}

		public void SetButtonsInactive()
		{
			startButton.GetComponent<UIElementView>().interactable = false;
			nextButton.GetComponent<UIElementView>().interactable = false;
			missionsButton.GetComponent<UIElementView>().interactable = false;
		}
	}
}

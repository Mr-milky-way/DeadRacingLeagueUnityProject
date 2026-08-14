using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIOnboardingOverviewView : UIScreenView
	{
		public UINavigation exitButtonNav;

		private DRLQuest m_data;

		public GameObject missionList;

		public ListComponent missionListField;

		public Text missionTitle;

		public Text missionCountTitle;

		public DRLOnboarding onboardingData;

		public DRLOnboardingModel onboardingModel;

		public UICardButtonQuest questCard;

		public ListComponent racesListField;

		public Text raceTitle;

		public Text raceCountTitle;

		public UIStatusView roomStatusField;

		public bool fromStartOnboarding;

		public GameObject columns;

		public GameObject separator;

		public DRLOnboardingModel model => base.app.model.onboarding;

		public DRLQuest data
		{
			get
			{
				return m_data;
			}
			set
			{
				m_data = value;
			}
		}

		private void Start()
		{
			onboardingModel = base.app.controller.onboarding.model;
		}

		public void ClearMissions()
		{
			if ((bool)missionListField)
			{
				missionListField.Clear();
				if ((bool)racesListField)
				{
					racesListField.Clear();
				}
			}
		}

		public void Set(DRLOnboarding p_model)
		{
			int num = 0;
			exitButtonNav.gameObject.SetActive(base.app.inGame);
			SetMissionListWidth();
			foreach (OnboardingStep step in p_model.steps)
			{
				if (step.type == OnboardingStep.OnboardingStepType.Mission)
				{
					AddMission(step.mission);
				}
				else if (step.type == OnboardingStep.OnboardingStepType.Race)
				{
					num++;
					AddRace(step, num);
				}
			}
			SetCountTitle();
			bool active = onboardingModel.GetTotalMissionSteps(base.app.model.onboarding.activeOnboarding) > 0 || base.app.model.onboarding.HasCompletedMissions();
			missionListField.transform.parent.gameObject.SetActive(active);
		}

		private void SetMissionListWidth()
		{
			if (onboardingModel.activeOnboarding.mode == OnboardingCampaignMode.Pro)
			{
				missionList.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(983f, missionList.transform.GetComponent<RectTransform>().sizeDelta.y);
				return;
			}
			missionList.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(1647.18f, missionList.transform.GetComponent<RectTransform>().sizeDelta.y);
			this.TimerRunOnce(delegate
			{
				GetComponent<UIScreen>().size = Vector2.right * 650f;
			}, 0.5f);
		}

		public void AddMission(DRLMission p_item)
		{
			if ((bool)missionListField && (bool)p_item && !p_item.gameObject.CompareTag("Intro"))
			{
				UICardButtonMission uICardButtonMission = missionListField.Push<UICardButtonMission>();
				uICardButtonMission.notification = "missions.mission-card";
				uICardButtonMission.Set(p_item);
			}
		}

		public void AddRace(OnboardingStep p_item, int raceIndex)
		{
			if ((bool)racesListField)
			{
				UICardButtonMission uICardButtonMission = racesListField.Push<UICardButtonMission>();
				uICardButtonMission.notification = "missions.mission-card";
				uICardButtonMission.SetRace(p_item, raceIndex);
			}
		}

		public void SetPage()
		{
			ClearMissions();
			onboardingData = model.activeOnboarding;
			Set(onboardingData);
			List<Component> list = new List<Component>();
			_ = missionListField.Count;
			_ = racesListField.Count;
			for (int i = 0; i < model.activeOnboarding.steps.Count; i++)
			{
				UICardButtonMission uICardButtonMission = SetUiCardButtonMission(i);
				list.Add(uICardButtonMission);
				ToggleButtonsInteractable(i, uICardButtonMission);
			}
			LayoutGroup component = missionListField.GetComponent<LayoutGroup>();
			LayoutGroup component2 = racesListField.GetComponent<LayoutGroup>();
			LayoutGroup componentInParent = exitButtonNav.GetComponentInParent<LayoutGroup>();
			UINavigation.Link(component, componentInParent, component2);
			if (missionListField.Count > 0)
			{
				UINavigation.Link(componentInParent, component2, component);
				UINavigation.Link(component2, component, componentInParent);
			}
			else
			{
				UINavigation.Link(componentInParent, component2, component2);
				UINavigation.Link(component2, componentInParent, componentInParent);
			}
		}

		private void SetCountTitle()
		{
			DRLOnboardingModel dRLOnboardingModel = base.app.controller.onboarding.model;
			raceTitle.enabled = !(racesListField == null) && racesListField.Count > 0;
			raceCountTitle.enabled = !(racesListField == null) && racesListField.Count > 0;
			missionTitle.enabled = !(missionListField == null) && missionListField.Count > 0;
			missionCountTitle.enabled = !(missionListField == null) && missionListField.Count > 0;
			int missionsProgress = dRLOnboardingModel.GetMissionsProgress(dRLOnboardingModel.activeOnboarding.mode);
			int totalMissionSteps = dRLOnboardingModel.GetTotalMissionSteps(dRLOnboardingModel.activeOnboarding);
			missionCountTitle.text = "(" + missionsProgress + "/" + totalMissionSteps + ")";
			raceCountTitle.text = "(" + dRLOnboardingModel.GetRaceProgress(dRLOnboardingModel.activeOnboarding.mode) + "/" + dRLOnboardingModel.GetTotalRaceSteps(dRLOnboardingModel.activeOnboarding) + ")";
		}

		private UICardButtonMission SetUiCardButtonMission(int i)
		{
			UICardButtonMission uICardButtonMission;
			if (onboardingModel.IsMissionStep(i, onboardingData))
			{
				uICardButtonMission = missionListField.Get<UICardButtonMission>(i);
				if (uICardButtonMission != null)
				{
					missionList.SetActive(value: true);
					uICardButtonMission.onboardinStep = i + 1;
					uICardButtonMission.title0 = base.app.model.storage.locale.Get("onboarding.mission.title", "MISSION") + " " + uICardButtonMission.onboardinStep;
					uICardButtonMission.onboardingCampaignMode = onboardingModel.activeOnboarding.mode;
				}
			}
			else
			{
				uICardButtonMission = racesListField.Get<UICardButtonMission>(i - onboardingModel.GetTotalMissionSteps(onboardingModel.activeOnboarding));
				if (uICardButtonMission != null)
				{
					uICardButtonMission.onboardinStep = i + 1;
					int num = uICardButtonMission.onboardinStep - missionListField.Count;
					uICardButtonMission.title0 = base.app.model.storage.locale.Get("onboarding.race.title", "RACE") + " " + num;
					uICardButtonMission.onboardingCampaignMode = onboardingModel.activeOnboarding.mode;
				}
			}
			return uICardButtonMission;
		}

		private void ToggleButtonsInteractable(int i, UICardButtonMission it)
		{
			if (!(it == null))
			{
				_ = base.app.model.storage.state.player.onboarding;
				bool flag = model.IsStepComplete(i);
				_ = new DRLOnboardingProgressionData[0];
				if (flag)
				{
					it.GetGrayMarker().SetActive(!flag);
					it.GetGreenMarker().SetActive(flag);
					it.interactable = flag;
				}
				else
				{
					it.GetGrayMarker().SetActive(!flag);
					it.GetGreenMarker().SetActive(flag);
					it.interactable = flag;
				}
				if (i > 0 && model.activeOnboarding.steps[i - 1].completed)
				{
					it.GetGrayMarker().SetActive(!flag);
					it.GetGreenMarker().SetActive(flag);
					it.interactable = true;
				}
				int firstRaceIndex = base.app.controller.onboarding.GetFirstRaceIndex(model.activeOnboarding);
				if ((i == firstRaceIndex && !model.IsStepComplete(firstRaceIndex)) || (i == 0 && !model.activeOnboarding.steps[0].completed))
				{
					it.GetGrayMarker().SetActive(value: true);
					it.GetGreenMarker().SetActive(value: false);
					it.interactable = true;
				}
			}
		}

		public void SetWidthZero()
		{
			GetComponent<UIScreen>().size = Vector2.zero;
			GetComponent<UIScreen>().position = new Vector2(0f, -180f);
			roomStatusField.GetComponent<LayoutFitter>().offset = new Vector2(350f, -800f);
		}

		public void SetStatus()
		{
			OpponentModel opponent = base.app.model.service.opponent;
			GetComponent<UINavigationScroll>().enabled = false;
			roomStatusField.fade.FadeIn(0.5f);
			SetWidthZero();
			switch (opponent.status)
			{
			case OpponentModel.Status.Error:
				roomStatusField.SetWarning("LOADING FAILED!");
				roomStatusField.fade.FadeOut(0.2f, 0.5f);
				base.app.view.audio.PlayUIGenericError();
				GetComponent<UINavigationScroll>().enabled = true;
				break;
			case OpponentModel.Status.NoResults:
				roomStatusField.SetWarning("NO OPPONENTS FOUND!");
				GetComponent<UINavigationScroll>().enabled = true;
				break;
			case OpponentModel.Status.Progress:
			{
				float loading = opponent.progress * 100f;
				roomStatusField.SetLoading(loading);
				break;
			}
			case OpponentModel.Status.Complete:
				GetComponent<UINavigationScroll>().enabled = true;
				roomStatusField.SetLoading(1f);
				roomStatusField.fade.FadeOut(0.2f, 0.5f);
				break;
			case OpponentModel.Status.ManifestSuccess:
				roomStatusField.SetLoading(0f);
				base.app.view.audio.PlayUIGenericSuccess();
				GetComponent<UINavigationScroll>().enabled = true;
				break;
			case OpponentModel.Status.None:
				roomStatusField.fade.FadeOut(0f);
				GetComponent<UINavigationScroll>().enabled = true;
				break;
			case OpponentModel.Status.ByPass:
				break;
			}
		}
	}
}

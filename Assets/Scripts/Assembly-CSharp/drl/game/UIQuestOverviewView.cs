using System.Collections.Generic;
using thelab.core;

namespace drl.game
{
	public class UIQuestOverviewView : UIScreenView
	{
		public ListComponent listField;

		public UICardButtonQuest questCard;

		private DRLQuest m_data;

		public DRLOnboarding onboardingData;

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

		public void ClearMissions()
		{
			if ((bool)listField)
			{
				listField.Clear();
			}
		}

		public void Set(DRLQuest p_quest)
		{
			ClearMissions();
			data = p_quest;
			UICardButtonQuest uICardButtonQuest = questCard;
			uICardButtonQuest.Set(data);
			uICardButtonQuest.questTitleField.enabled = false;
			uICardButtonQuest.missionCount = data.missions.Count;
			List<DRLMission> missions = data.missions;
			for (int i = 0; i < missions.Count; i++)
			{
				AddMission(missions[i]);
			}
		}

		public void Set(DRLOnboarding p_model)
		{
			ClearMissions();
			data = p_model.steps[0].quest;
			UICardButtonQuest uICardButtonQuest = questCard;
			uICardButtonQuest.Set(data);
			uICardButtonQuest.questTitleField.enabled = false;
			uICardButtonQuest.missionCount = data.missions.Count;
			foreach (OnboardingStep step in p_model.steps)
			{
				if (step.type == OnboardingStep.OnboardingStepType.Mission)
				{
					AddMission(step.mission);
				}
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
	}
}

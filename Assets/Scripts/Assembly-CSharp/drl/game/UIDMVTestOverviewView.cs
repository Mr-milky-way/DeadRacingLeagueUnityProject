using System.Collections.Generic;
using thelab.core;

namespace drl.game
{
	public class UIDMVTestOverviewView : UIScreenView
	{
		public ListComponent listField;

		public UICardButtonDmvTest questCard;

		public DRLQuest data;

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
			questCard.Set(data);
			List<DRLMission> missions = data.missions;
			for (int i = 0; i < missions.Count; i++)
			{
				AddMission(missions[i]);
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

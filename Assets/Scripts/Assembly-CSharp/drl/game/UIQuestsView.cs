using thelab.core;

namespace drl.game
{
	public class UIQuestsView : UIScreenView
	{
		public ListComponent listField;

		public void ClearQuests()
		{
			if ((bool)listField)
			{
				listField.Clear();
			}
		}

		public void AddQuest(DRLQuest p_item)
		{
			if ((bool)listField && (bool)p_item)
			{
				UICardButtonQuest uICardButtonQuest = listField.Push<UICardButtonQuest>();
				uICardButtonQuest.Set(p_item);
				uICardButtonQuest.questTitle = base.app.model.storage.locale.Get("quests.card-title.quest", "QUEST") + " " + listField.Count.ToString("00");
				uICardButtonQuest.notification = "missions.quest-card";
			}
		}
	}
}

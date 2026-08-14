using thelab.core;

namespace drl.game
{
	public class UITournamentSkillView : UIScreenView
	{
		public ListComponent listField;

		public void RefreshCards()
		{
			if (!listField || listField.Count <= 0)
			{
				return;
			}
			for (int i = 0; i < listField.Count; i++)
			{
				UICardButtonTournamentSkill uICardButtonTournamentSkill = listField.Get<UICardButtonTournamentSkill>(i);
				if ((bool)uICardButtonTournamentSkill)
				{
					uICardButtonTournamentSkill.Refresh();
				}
			}
		}
	}
}

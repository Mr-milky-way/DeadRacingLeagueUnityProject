using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UITournamentSkillController : Controller<DRLApp>
	{
		public UITournamentSkillView view => AssertLocal<UITournamentSkillView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				if (!(p_data[0] as UIScreen != view.screen))
				{
					view.RefreshCards();
				}
				break;
			case "tournament.skill-card@click":
			{
				UICardButtonTournamentSkill uICardButtonTournamentSkill = p_target as UICardButtonTournamentSkill;
				if (uICardButtonTournamentSkill.CanEnter())
				{
					UITournamentsListView uITournamentsListView = base.app.view.ui.screens.Open<UITournamentsListView>("tournaments-list-screen");
					if ((bool)uITournamentsListView)
					{
						uITournamentsListView.minimumSkill = uICardButtonTournamentSkill.skillRequired;
					}
				}
				break;
			}
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			}
		}
	}
}

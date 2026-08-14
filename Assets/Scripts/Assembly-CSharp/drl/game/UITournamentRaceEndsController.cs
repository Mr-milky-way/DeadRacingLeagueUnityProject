using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class UITournamentRaceEndsController : Controller<DRLApp>
	{
		public GameController game => base.app.controller.game;

		public UITournamentRaceEndsView view => AssertLocal<UITournamentRaceEndsView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (view.current && p_event != null && p_event == "ui.screen.return@click")
			{
				base.app.view.ui.screens.Open<UITournamentBracketsView>("tournament-brackets-screen").backButtonEnabled = false;
			}
		}
	}
}

using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class UIDMVWelcomeScreenController : Controller<DRLApp>
	{
		public UIDMVWelcomeScreenView view => AssertLocal<UIDMVWelcomeScreenView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (!(base.app.view.ui.screens.current != view.screen))
			{
				switch (p_event)
				{
				case "ui.screen.nav-right@click":
				{
					UIMissionOverviewView uIMissionOverviewView = base.app.view.ui.screens.Open<UIMissionOverviewView>("test-overview-screen");
					uIMissionOverviewView.screen.title = view.mission.title;
					uIMissionOverviewView.quest = view.quest;
					uIMissionOverviewView.mission = view.mission;
					base.app.arguments.game.mission = uIMissionOverviewView.mission;
					base.app.arguments.game.map = (uIMissionOverviewView.mission ? uIMissionOverviewView.mission.map : null);
					base.app.arguments.game.track = (uIMissionOverviewView.mission ? uIMissionOverviewView.mission.track : null);
					base.app.arguments.game.quest = uIMissionOverviewView.quest;
					break;
				}
				case "ui.screen.return@click":
					base.app.view.ui.screens.Return();
					break;
				}
			}
		}
	}
}

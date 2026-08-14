using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIRacePodiumController : Controller<DRLApp>
	{
		public GameController game => base.app.controller.game;

		public UIRacePodiumView view => AssertLocal<UIRacePodiumView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				if (p_data[0] as UIScreen != view.screen)
				{
					break;
				}
				base.app.view.ui.SetDark(p_flag: true);
				base.app.view.ui.footer.Hide(0f);
				view.SetPromoEnabled(p_flag: false);
				if ((bool)game)
				{
					view.ToggleDroneCamera(p_enabled: true);
					bool flag2 = false;
					if (game.model.type == GameFlag.Campaign)
					{
						CampaignController campaignController = view.race as CampaignController;
						flag2 = flag2 || ((bool)campaignController.model.campaign && campaignController.model.campaign.tournament);
					}
					flag2 = flag2 || base.app.arguments.game.tournamentPromo;
					view.SetPromoEnabled(flag2);
					view.Init();
				}
				break;
			case "ui.screen.return@click":
				view.ToggleDroneCamera(p_enabled: false);
				base.app.view.ui.screens.Return();
				break;
			case "ui.screen.nav-right@click":
			{
				bool flag = false;
				DRLTournamentLegacyData tournamentLegacy = base.app.arguments.game.tournamentLegacy;
				flag = tournamentLegacy != null;
				if (game.model.mode == GameFlag.SinglePlayer)
				{
					flag = false;
				}
				view.ToggleDroneCamera(p_enabled: false);
				if (flag)
				{
					base.app.view.ui.screens.Open<UIGameTournamentOverviewView>("game-tournament-overview-screen").data = tournamentLegacy;
					break;
				}
				UIRaceOverviewView uIRaceOverviewView = base.app.view.ui.screens.Open<UIRaceOverviewView>("game-race-overview-screen");
				uIRaceOverviewView.race = view.race;
				uIRaceOverviewView.title.text = view.race.GetRaceTitle().ToUpper();
				uIRaceOverviewView.Clear();
				uIRaceOverviewView.LoadRaceData();
				uIRaceOverviewView.SetTitle();
				break;
			}
			case "ui.screen@close":
				break;
			}
		}
	}
}

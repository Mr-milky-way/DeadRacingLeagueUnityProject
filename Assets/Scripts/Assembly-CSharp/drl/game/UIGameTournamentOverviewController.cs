using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIGameTournamentOverviewController : Controller<DRLApp>
	{
		public GameController game => base.app.controller.game;

		public UIGameTournamentOverviewView view => AssertLocal<UIGameTournamentOverviewView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (!view.current)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
			{
				if (p_data[0] as UIScreen != view.screen)
				{
					break;
				}
				base.app.view.ui.SetDark(p_flag: true);
				base.app.view.ui.footer.Hide(0f);
				view.ClearTable();
				GameFlag type = base.app.arguments.game.type;
				bool flag = base.app.arguments.game.mode == GameFlag.NetworkMultiplayer;
				bool promoEnabled = false || base.app.arguments.game.tournamentPromo;
				view.SetPromoEnabled(promoEnabled);
				view.SetReplayEnabled(p_flag: true);
				if (flag)
				{
					NetworkRaceController networkRaceController2 = view.race as NetworkRaceController;
					if ((bool)networkRaceController2)
					{
						Debug.Log("UITournamentOverviewController> ScreenOpen - replay-ready[" + networkRaceController2.allReplaysProcessed + "]");
						view.SetReplayEnabled(networkRaceController2.allReplaysProcessed);
					}
				}
				view.SetGameType(type, flag);
				view.SetPromoEnabled(p_flag: true);
				ServiceModel service = base.app.model.service;
				DRLTournamentLegacyData td = view.data;
				string t_guid = ((td == null) ? "" : td.guid);
				if (!string.IsNullOrEmpty(t_guid))
				{
					Debug.Log("UITournamentOverviewController> Loading Tournament - guid[" + t_guid + "]");
					view.status.SetLoading(0f);
					view.status.fade.FadeIn(0.2f);
					service.GetTournamentsLegacy(t_guid, delegate(DRLTournamentLegacyData[] p_tournaments)
					{
						if (p_tournaments.Length == 0)
						{
							Debug.LogWarning("UITournamentOverviewController> Tournament [" + t_guid + "] not found!");
							view.status.SetWarning("FAILED TO FIND TOURNAMENT!");
						}
						else
						{
							view.status.fade.FadeOut(0.2f);
							string playerId = base.app.model.service.backend.playerId;
							td = p_tournaments[0];
							Debug.Log("UITournamentOverviewController> Tournament - guid[" + t_guid + "] steam_id[" + playerId + "] Found!");
							view.Set(playerId, td);
						}
					});
				}
				else
				{
					Debug.LogWarning("UITournamentOverviewController> Tournament Data is 'null'");
					view.status.SetWarning("FAILED TO FIND TOURNAMENT!");
					view.status.fade.FadeIn(0.2f);
				}
				break;
			}
			case "network.race.replay.ready.all":
				view.SetReplayEnabled(p_flag: true);
				Debug.Log("UITournamentOverviewController> Replay Ready All");
				break;
			case "game.race-overview.replay@click":
			{
				if (base.app.arguments.game.mode == GameFlag.NetworkMultiplayer)
				{
					NetworkRaceController networkRaceController = view.race as NetworkRaceController;
					if ((bool)networkRaceController && !networkRaceController.allReplaysProcessed)
					{
						break;
					}
				}
				game.model.simulation.drones.SetVisible(p_flag: false);
				UISpectateController component = base.app.view.ui.screens.Open<UISpectateView>("game-spectate-screen").GetComponent<UISpectateController>();
				component.SetReplayClips(game.model);
				component.Initialize(GameFlag.Replay);
				break;
			}
			case "game.race-overview.room@click":
				game.OpenNetworkRoomScreen();
				base.app.model.network.StartMatchmaking();
				break;
			case "viewer.controls.nav.exit@click":
				game.SetTabScreenEnabled(p_flag: false);
				game.model.replay.player.Clear();
				game.model.simulation.drones.SetVisible(p_flag: true);
				break;
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			}
		}
	}
}

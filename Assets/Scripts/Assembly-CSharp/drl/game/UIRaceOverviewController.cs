using System;
using UnityEngine;
using drl.backend;
using drl.network;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIRaceOverviewController : Controller<DRLApp>
	{
		public GameController game => base.app.controller.game;

		public UIRaceOverviewView view => AssertLocal<UIRaceOverviewView>("view");

		public StorageModel storage => base.app.model.storage;

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (!view.current || p_event == null)
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
				bool tryouts = base.app.arguments.game.tryouts;
				bool inProgress = base.app.model.storage.state.player.circuits.inProgress;
				base.app.view.ui.screens.SetStaticBackground(view.backgroundCapture);
				base.app.view.ui.SetDark(p_flag: true);
				base.app.view.ui.footer.Hide(0f);
				base.app.view.ui.game.hud.damage.Show(p_flag: false);
				if (tryouts)
				{
					view.restartButton.gameObject.SetActive(value: false);
					view.nextTryoutsButton.SetActive(value: false);
					view.exitButton.gameObject.SetActive(value: false);
					if ((bool)base.app.model.service)
					{
						base.app.model.service.GetTryoutsHeatsFinished(delegate(int heats)
						{
							if (heats >= 3)
							{
								view.SetUserQualified();
							}
							else
							{
								view.SetHeatsFeedback(heats);
							}
						});
					}
				}
				if (!game)
				{
					break;
				}
				RefreshCards();
				GameFlag type = base.app.arguments.game.type;
				bool flag2 = base.app.arguments.game.mode == GameFlag.NetworkMultiplayer;
				bool num = type == GameFlag.Campaign;
				bool fromEditor = game.model.fromEditor;
				_ = base.app.arguments.game.tryouts;
				bool flag3 = false;
				if (num)
				{
					CampaignController campaignController = view.race as CampaignController;
					flag3 = flag3 || ((bool)campaignController.model.campaign && campaignController.model.campaign.tournament);
				}
				flag3 = flag3 || base.app.arguments.game.tournamentPromo || base.app.arguments.game.promo;
				view.SetPromoEnabled(flag3);
				if (flag2)
				{
					NetworkRaceController networkRaceController2 = view.race as NetworkRaceController;
					if ((bool)networkRaceController2)
					{
						Debug.Log("UIRaceOverviewController> ScreenOpen - replay-ready[" + networkRaceController2.allReplaysProcessed + "]");
						view.SetReplayEnabled(networkRaceController2.allReplaysProcessed);
					}
				}
				else
				{
					view.SetReplayEnabled(!game.model.replayProcessActive);
				}
				view.SetGameType(type, flag2, fromEditor, tryouts);
				if (!tryouts)
				{
					this.TimerRunOnce(delegate
					{
						if (base.validContext && !view.exitButton.gameObject.activeInHierarchy)
						{
							view.exitButton.gameObject.SetActive(value: true);
						}
					}, 20f);
				}
				if (view.isSpectator)
				{
					base.app.controller.game.ui.hud.timeout.StopTimeout();
					base.app.view.ui.game.hud.Hide(0f);
				}
				if (inProgress)
				{
					view.mapsButton.gameObject.SetActive(value: false);
					bool flag4 = view.race != null && view.race.model.status == RaceStatusType.Success;
					view.circuitsButton.gameObject.SetActive(flag4);
					view.restartButton.gameObject.SetActive(!flag4 && view.savingComplete);
				}
				view.savingComplete = storage.saveComplete;
				view.SetExitButton();
				view.SetRestartButton();
				break;
			}
			case "game.race.process-replay.complete":
			case "network.race.replay.ready.all":
				view.SetReplayEnabled(p_flag: true);
				Debug.Log("UIRaceOverviewController> Replay Ready All");
				break;
			case "game.race-overview.exit@click":
			{
				bool flag = false;
				flag = base.app.arguments.game.tournamentData != null;
				base.app.arguments.game.tryouts = false;
				if (game.model.fromEditor)
				{
					base.enabled = false;
					base.app.view.audio.PlayUIGenericSuccess();
					game.BackToEditor();
				}
				else if (flag)
				{
					base.app.view.ui.screens.Open<UITournamentBracketsView>("tournament-brackets-screen");
				}
				else
				{
					base.enabled = false;
					base.app.view.audio.PlayUIGenericSuccess();
					game.Exit();
					base.app.model.storage.state.player.garage.currentRigData = null;
				}
				base.app.model.network.Disconnect();
				break;
			}
			case "game.race.request-restart":
				if (game.model.mode != GameFlag.NetworkMultiplayer && !base.app.inCircuits)
				{
					base.enabled = false;
					base.app.view.audio.PlayUIGenericSuccess();
					game.Restart();
				}
				break;
			case "game.race-overview.restart@click":
				base.enabled = false;
				base.app.view.audio.PlayUIGenericSuccess();
				game.Restart();
				break;
			case "game.race-overview.maps@click":
				Notify("game.change-game@click");
				break;
			case "game.race-complete.settings@click":
				base.app.view.ui.screens.Open("settings-game-screen");
				break;
			case "network.room@lock":
				view.SetReplayEnabled(p_flag: false);
				break;
			case "game.race-overview.replay@click":
			{
				if (base.app.arguments.game.mode == GameFlag.NetworkMultiplayer)
				{
					NetworkRaceController networkRaceController = view.race as NetworkRaceController;
					if (((bool)networkRaceController && !networkRaceController.allReplaysProcessed) || (base.app.model.network.room != null && base.app.model.network.room.State == NetworkRoom.StateCode.MatchLocked))
					{
						break;
					}
				}
				game.model.simulation.drones.FixAll();
				game.model.simulation.drones.SetVisible(p_flag: false);
				if ((bool)view.collectable)
				{
					view.collectable.model.RestoreAll();
				}
				base.app.arguments.game.type = GameFlag.Replay;
				base.app.model.game.type = GameFlag.Replay;
				base.app.view.ui.game.hud.Hide();
				UISpectateController component = base.app.view.ui.screens.Open<UISpectateView>("game-spectate-screen").GetComponent<UISpectateController>();
				component.SetReplayClips(game.model, base.app.inCircuits);
				component.Initialize(GameFlag.Replay);
				break;
			}
			case "game.race-overview.room@click":
				if (view.isSpectator && base.app.arguments.game.tournamentData != null)
				{
					UITournamentResultsView uITournamentResultsView = base.app.view.ui.screens.Open<UITournamentResultsView>("tournament-results-screen");
					uITournamentResultsView.race = view.race;
					uITournamentResultsView.matchData = null;
				}
				else
				{
					game.OpenNetworkRoomScreen();
					base.app.model.network.StartMatchmaking();
				}
				break;
			case "game.race.replay-upload@complete":
				Debug.Log("UIRaceCompleteController> <color=#ff0>Local Player Replays UPLOAD COMPLETE!</color> " + DateTime.Now.ToString());
				storage.saveComplete = true;
				view.savingComplete = true;
				view.SetRestartButton();
				view.SetExitButton();
				break;
			case "viewer.controls.nav.exit@click":
				game.SetTabScreenEnabled(p_flag: false);
				game.model.replay.player.Clear();
				game.model.simulation.drones.SetVisible(p_flag: true);
				break;
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			case "game.race.leaderboard-set":
				if (!base.app.arguments.game.tryouts)
				{
					view.exitButton.gameObject.SetActive(value: true);
					view.exitEnabled = true;
				}
				break;
			case "game.race-overview.circuits@click":
				base.app.view.ui.screens.Open("circuits-overview-screen");
				break;
			}
		}

		protected void RefreshCards()
		{
			GamePlayerData playerData = game.model.playerData;
			DroneRigData droneRigData = null;
			if (playerData != null)
			{
				droneRigData = playerData.rig;
			}
			DRLMap map = base.app.scene.map;
			DRLMapTrack track = base.app.scene.track;
			if (map == null || track == null || base.app.model.storage == null || base.app.model.storage.state.player == null)
			{
				return;
			}
			RaceController race = view.race;
			if ((bool)race && race.model.Rankings.Count == 0)
			{
				return;
			}
			if (view.isSpectator)
			{
				playerData = race.model.Rankings[0];
				droneRigData = playerData.drone.rig;
			}
			if (droneRigData == null)
			{
				return;
			}
			if (base.app.arguments.game.tournamentData != null && view.isSpectator)
			{
				string id = base.app.arguments.game.tournamentData.GetActiveRound().GetPlayerMatch(race.model.Rankings[0].playerId).Id;
				base.app.model.service.GetLeaderboardRivals(map, track, 3, droneRigData.diameter, base.app.model.storage.state.player.garage.IsOfficial(droneRigData), droneRigData.hasCustomPhysics, id, delegate(DRLLeaderboardRivalsResult p_result)
				{
					if ((bool)this && (bool)view)
					{
						if (p_result == null)
						{
							Debug.LogWarning("UIRaceOverviewController> GetLeaderboardRivals - Failed!");
						}
						else
						{
							DRLLeaderboardData top = p_result.GetTop(0);
							Debug.Log("UIRaceOverviewController> GetLeaderboardRivals - Success - top[" + p_result.top.Length + "] player[" + p_result.player + "] rivals[" + p_result.rivals.Length + "]");
							DRLLeaderboardData[] rival = p_result.GetRival3();
							view.SetLeader(top);
							view.SetRival(0, rival[0]);
							view.SetRival(1, rival[1]);
							view.SetRival(2, rival[2]);
							view.playerTime = top.score;
							view.playerCrash = top.crashCount;
							view.playerTopSpeed = top.topSpeed;
							Debug.Log($"<color=green>RACE OVERVIEW SCREEN SAYS</color> SCORE:{top.score}, TOPSPEED:{top.topSpeed} ");
						}
					}
				});
			}
			else
			{
				base.app.model.service.GetLeaderboardRivals(map, track, 3, droneRigData.diameter, base.app.model.storage.state.player.garage.IsOfficial(droneRigData), droneRigData.hasCustomPhysics, delegate(DRLLeaderboardRivalsResult p_result)
				{
					if ((bool)this && (bool)view)
					{
						if (p_result == null)
						{
							Debug.LogWarning("UIRaceOverviewController> GetLeaderboardRivals - Failed!");
						}
						else
						{
							DRLLeaderboardData top = p_result.GetTop(0);
							Debug.Log("UIRaceOverviewController> GetLeaderboardRivals - Success - top[" + p_result.top.Length + "] player[" + p_result.player + "] rivals[" + p_result.rivals.Length + "]");
							DRLLeaderboardData[] rival = p_result.GetRival3();
							view.SetLeader(top);
							view.SetRival(0, rival[0]);
							view.SetRival(1, rival[1]);
							view.SetRival(2, rival[2]);
							if (view.isSpectator && top != null)
							{
								view.playerTime = top.score;
								view.playerCrash = top.crashCount;
								view.playerTopSpeed = top.topSpeed;
								Debug.Log($"<color=green>RACE OVERVIEW SCREEN SAYS</color> SCORE:{top.score}, TOPSPEED:{top.topSpeed} ");
							}
						}
					}
				});
			}
			view.SetDroneCard(droneRigData);
		}
	}
}

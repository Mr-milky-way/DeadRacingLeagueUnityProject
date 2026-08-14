using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UICollectablesOverviewController : Controller<DRLApp>
	{
		public GameController game => base.app.controller.game;

		public UICollectablesOverviewView view => AssertLocal<UICollectablesOverviewView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
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
				view.LoadRaceData();
				bool tryouts = base.app.arguments.game.tryouts;
				base.app.view.ui.screens.SetStaticBackground(view.backgroundCapture);
				base.app.view.ui.SetDark(p_flag: true);
				base.app.view.ui.footer.Hide(0f);
				if (!game)
				{
					break;
				}
				RefreshCards();
				GameFlag type = base.app.arguments.game.type;
				bool p_multiplayer = base.app.arguments.game.mode == GameFlag.NetworkMultiplayer;
				bool fromEditor = game.model.fromEditor;
				view.SetReplayEnabled(p_flag: true);
				view.SetGameType(type, p_multiplayer, fromEditor, tryouts);
				view.exitButton.gameObject.SetActive(view.exitEnabled);
				this.TimerRunOnce(delegate
				{
					if (base.validContext && !view.exitButton.gameObject.activeInHierarchy)
					{
						view.exitButton.gameObject.SetActive(value: true);
					}
				}, 20f);
				if (view.isSpectator)
				{
					base.app.controller.game.ui.hud.timeout.StopTimeout();
					base.app.view.ui.game.hud.Hide(0f);
				}
				break;
			}
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
				break;
			}
			case "game.race.request-restart":
				if (game.model.mode != GameFlag.NetworkMultiplayer)
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
				game.model.simulation.drones.SetVisible(p_flag: false);
				if ((bool)view.collectable)
				{
					view.collectable.model.RestoreAll();
				}
				base.app.arguments.game.type = GameFlag.Replay;
				base.app.model.game.type = GameFlag.Replay;
				base.app.view.ui.game.hud.Hide();
				UISpectateController component = base.app.view.ui.screens.Open<UISpectateView>("game-spectate-screen").GetComponent<UISpectateController>();
				component.SetReplayClips(game.model);
				component.Initialize(GameFlag.Replay);
				break;
			}
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
			if (map == null || track == null || base.app.model.storage == null || base.app.model.storage.state.player == null || droneRigData == null)
			{
				return;
			}
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
						DRLLeaderboardData top2 = p_result.GetTop(1);
						DRLLeaderboardData top3 = p_result.GetTop(2);
						Debug.Log("UIRaceOverviewController> GetLeaderboardRivals - Success - top[" + p_result.top.Length + "] player[" + p_result.player + "] rivals[" + p_result.rivals.Length + "]");
						DRLLeaderboardData[] rival = p_result.GetRival3();
						Debug.Log("UIRaceOverviewController> GetLeaderboardRivals: " + rival.Length);
						view.SetLeader(top);
						view.SetRival(0, rival[0]);
						view.SetRival(1, rival[1]);
						view.SetRival(2, rival[2]);
						DRLStandingsItemView[] componentsInChildren = view.standings.listField.GetComponentsInChildren<DRLStandingsItemView>();
						foreach (DRLStandingsItemView dRLStandingsItemView in componentsInChildren)
						{
							dRLStandingsItemView.gameObject.SetActive(dRLStandingsItemView.playerId != "");
						}
						List<UILeaderboardItemView> pPlayers = new List<UILeaderboardItemView>();
						view.standings.Refresh(pPlayers, p_clear: false, p_dnf: false, p_displayDNF: false);
						if (top != null)
						{
							view.leaders[0].Set(rival[0]);
						}
						if (top2 != null)
						{
							view.leaders[1].Set(rival[1]);
						}
						if (top3 != null)
						{
							view.leaders[2].Set(rival[2]);
						}
						if (view.isSpectator && top != null)
						{
							view.playerTime = top.score;
							view.playerCrash = top.crashCount;
							view.playerTopSpeed = top.topSpeed;
							Debug.Log($"<color=green>RACE OVERVIEW SCREEN SAYS</color> SCORE:{top.score}, TOPSPEED:{top.topSpeed} ");
						}
					}
				}
			}, -1, p_collectable: true);
			view.SetDroneCard(droneRigData);
		}
	}
}

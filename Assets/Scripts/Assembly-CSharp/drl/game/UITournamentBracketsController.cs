using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using drl.network;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UITournamentBracketsController : Controller<DRLApp>
	{
		private bool m_hasDoneCleanup;

		public UITournamentBracketsView view => AssertLocal<UITournamentBracketsView>("view");

		public NetworkModel network => base.app.model.network;

		public TournamentModel model => base.app.model.tournament;

		public DRLTournamentData tournament => model.tournament;

		private int roundIndex
		{
			get
			{
				if (model.activeRound == null)
				{
					return -1;
				}
				return model.activeRound.index;
			}
		}

		private int matchIndex
		{
			get
			{
				if (model.activeMatch == null)
				{
					return -1;
				}
				return model.activeMatch.index;
			}
		}

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "network.room@enter":
				network.room.TournamentId = model.guid;
				network.room.DroneClass = ((tournament.droneClass == 2) ? 102 : ((tournament.droneClass == 1) ? 101 : ((tournament.droneClass == 0) ? 100 : tournament.droneClass)));
				StopAll();
				break;
			case "ui.screen@close":
			{
				if (p_data.Length == 0)
				{
					break;
				}
				UIScreen uIScreen = p_data[0] as UIScreen;
				if (view.screen == uIScreen)
				{
					ClearIgnoredCommands();
					if ((bool)base.app.view.ui.header)
					{
						base.app.view.ui.header.gameObject.SetActive(value: true);
					}
				}
				break;
			}
			case "tournament.enter-match@click":
			{
				UIScreen current = base.app.view.ui.screens.current;
				if (!(current == null) && (current.name == "tournament-brackets-screen" || current.name == "garage-rig-selection-screen" || current.name == "tournament-results-screen" || current.name == "tournament-leaderboards-screen"))
				{
					EnterMatch();
					Debug.Log("UITournamentBracketsController> Trying to enter match room via UI.");
				}
				break;
			}
			case "tournament.action.match-pull":
			case "tournament.action.match-starting":
			{
				string text = (string)p_data[0];
				if (model.IsRacerInMatch(text) && (base.app.model.network.room == null || !(base.app.model.network.room.MatchId == text)))
				{
					EnterMatch();
					Debug.Log("UITournamentBracketsController> Trying to enter match room via notifications - " + p_event);
				}
				break;
			}
			case "settings.controller.disconnect":
			case "settings.controller.connect":
				view.RefreshNavigationTooltips();
				break;
			}
			if (base.app.view.ui.screens.current != view.screen || p_event == null)
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
				if ((bool)base.app.view.ui.header)
				{
					base.app.view.ui.header.gameObject.SetActive(value: false);
				}
				this.TimerRunOnce(delegate
				{
					base.app.view.ui.footer.Show(0f);
				}, 0.2f);
				view.backButtonEnabled = true;
				Notify("tournament.brackets.open");
				view.EnableMiniMap(base.app.level.IsLevelLoaded("game"));
				view.RefreshTournamentData(tournament, model.activeRound, model.activeMatch);
				view.Populate();
				UpdatePlayerDrone();
				model.RefreshData(delegate
				{
					RefreshAll();
				});
				SetIgnoredGameCommands();
				if (view.forceIntoMatch)
				{
					view.forceIntoMatch = false;
					if (!string.IsNullOrEmpty(view.forceMatchID))
					{
						ForceEnterMatch(view.forceMatchID);
					}
				}
				if ((bool)view.scroll)
				{
					view.scroll.enableJoystickPanning = false;
					Timer.Set(view.scroll, "enableJoystickPanning", 3f, true);
				}
				base.app.view.ui.screens.manager.ClearHistory("garage-rig-selection-screen");
				break;
			case "tournament.refresh.lobby":
			case "network.lobby@enter":
			case "network.lobby@exit":
			case "network.disconnect":
			case "network.connection@start":
				RefreshLobby();
				break;
			case "tournament.match-card@click":
			{
				UITournamentBracketsMatchItemView uITournamentBracketsMatchItemView = p_target as UITournamentBracketsMatchItemView;
				if (uITournamentBracketsMatchItemView == null || uITournamentBracketsMatchItemView.gameMode == TournamentRoundGameMode.leaderboard)
				{
					break;
				}
				float p_diff = 0f;
				if (uITournamentBracketsMatchItemView.IsExpanded())
				{
					uITournamentBracketsMatchItemView.Contract();
					if (uITournamentBracketsMatchItemView.data.roundIndex == 0)
					{
						p_diff = uITournamentBracketsMatchItemView.contractedSize - uITournamentBracketsMatchItemView.expandedSize;
					}
				}
				else
				{
					uITournamentBracketsMatchItemView.Expand();
					if (uITournamentBracketsMatchItemView.data.roundIndex == 0)
					{
						p_diff = uITournamentBracketsMatchItemView.expandedSize - uITournamentBracketsMatchItemView.contractedSize;
					}
				}
				view.RefreshLayout(0.3f, p_diff, 0.1f);
				break;
			}
			case "tournament.drone-card@click":
				StopAll();
				Notify("ui.footer.drone@click");
				break;
			case "tournament.match-card.form.event@click":
				OnFormNotification(p_target, p_is_change: false, p_event);
				break;
			case "tournament.exit@click":
				TournamentExit();
				break;
			case "tournament.winners@click":
				base.app.view.ui.screens.Open<UITournamentWinnersView>("tournament-leaders-screen").allowNext = false;
				view.completeWinnersScreenShown = true;
				break;
			case "tournament.standings@click":
				if (tournament != null && tournament.rankings.Length != 0)
				{
					base.app.view.ui.screens.Open<UITournamentWinnersView>("tournament-leaders-screen").allowNext = false;
				}
				break;
			case "tournament.action.refresh":
				RefreshAll();
				Activity.RunOnce(delegate
				{
					for (int i = 0; i < 3; i++)
					{
						UITournamentBracketsMatchColumnItem uITournamentBracketsMatchColumnItem = view.matchColumnsList.Get<UITournamentBracketsMatchColumnItem>(i);
						if (uITournamentBracketsMatchColumnItem != null)
						{
							HorizontalLayoutGroup component = uITournamentBracketsMatchColumnItem.GetComponent<HorizontalLayoutGroup>();
							component.childForceExpandHeight = true;
							component.childForceExpandHeight = false;
						}
					}
				}, 0.2f);
				break;
			case "tournament.action.swapped":
				model.RefreshData(delegate
				{
					RefreshAll(p_forcePopulate: true);
				});
				break;
			case "tournament.placements@click":
				base.app.view.ui.screens.Open("tournament-dawc-screen");
				break;
			case "ui.screen@close":
				StopAll();
				if ((bool)base.app.view.ui.footer)
				{
					base.app.view.ui.footer.droneButton.interactable = true;
				}
				Notify("network.footer@disable");
				break;
			case "tournament.settings@click":
				base.app.view.ui.screens.Open("settings-screen");
				break;
			case "tournametn.timer.update":
				if (p_data.Length != 0)
				{
					float num = 0f;
					try
					{
						num = (float)p_data[0];
					}
					catch
					{
						break;
					}
					view.UpdateHeaderStatus(model.isRacer, num);
				}
				break;
			}
		}

		public void TournamentExit(bool p_force = false)
		{
			bool flag = base.app.level.IsLevelLoaded("game");
			if (!flag)
			{
				base.app.view.ui.screens.Return(1);
				return;
			}
			base.enabled = false;
			base.app.controller.game.Exit();
			base.app.arguments.game.tournamentData = null;
			base.app.arguments.tournament.data = null;
			if (flag || p_force)
			{
				Notify("tournament.exited");
			}
		}

		protected void OnFormNotification(UnityEngine.Object p_target, bool p_is_change, string p_event)
		{
			switch (p_target.name)
			{
			case "enter":
			{
				Component component3 = p_target as Component;
				if ((bool)component3)
				{
					UITournamentBracketsMatchItemView uITournamentBracketsMatchItemView2 = Hierarchy.FindReverse<UITournamentBracketsMatchItemView>(component3.transform);
					if (uITournamentBracketsMatchItemView2.gameMode == TournamentRoundGameMode.leaderboard)
					{
						OpenSoloRace(uITournamentBracketsMatchItemView2);
					}
					else
					{
						CheckJoinConditions(uITournamentBracketsMatchItemView2);
					}
					Debug.Log("UITournamentBracketsController> Trying to enter match room via UI.");
				}
				break;
			}
			case "spectate":
			{
				Component component2 = p_target as Component;
				if ((bool)component2)
				{
					UITournamentBracketsMatchItemView p_match = Hierarchy.FindReverse<UITournamentBracketsMatchItemView>(component2.transform);
					CheckSpectateConditions(p_match);
				}
				break;
			}
			case "results":
			{
				Component component = p_target as Component;
				if (!component)
				{
					break;
				}
				UITournamentBracketsMatchItemView uITournamentBracketsMatchItemView = Hierarchy.FindReverse<UITournamentBracketsMatchItemView>(component.transform);
				if (uITournamentBracketsMatchItemView.interactable)
				{
					if (uITournamentBracketsMatchItemView.gameMode == TournamentRoundGameMode.leaderboard)
					{
						UITournamentLeaderboardsView uITournamentLeaderboardsView = base.app.view.ui.screens.Open<UITournamentLeaderboardsView>("tournament-leaderboards-screen");
						uITournamentLeaderboardsView.round = model.rounds[uITournamentBracketsMatchItemView.data.roundIndex];
						uITournamentLeaderboardsView.openedFromTheBrackets = true;
					}
					else
					{
						UITournamentResultsView uITournamentResultsView = base.app.view.ui.screens.Open<UITournamentResultsView>("tournament-results-screen");
						uITournamentResultsView.matchData = uITournamentBracketsMatchItemView.data;
						uITournamentResultsView.title = uITournamentBracketsMatchItemView.groupNameField.text;
						uITournamentResultsView.openedFromTheBrackets = true;
					}
					StopAll();
				}
				break;
			}
			}
		}

		private void ForceEnterMatch(string p_matchId)
		{
			if (base.app.model.network.room == null || !(base.app.model.network.room.MatchId == p_matchId))
			{
				EnterMatch();
			}
		}

		private void EnterMatch()
		{
			if (roundIndex >= 0)
			{
				UITournamentBracketsMatchColumnItem uITournamentBracketsMatchColumnItem = view.matchColumnsList.Get<UITournamentBracketsMatchColumnItem>(roundIndex);
				if (uITournamentBracketsMatchColumnItem.roundGameMode == TournamentRoundGameMode.leaderboard)
				{
					UITournamentBracketsMatchItemView p_match = uITournamentBracketsMatchColumnItem.matchesList.Get<UITournamentBracketsMatchItemView>(0);
					OpenSoloRace(p_match);
				}
				else
				{
					CheckJoinConditions(view.activeMatchItem);
				}
			}
		}

		private void OpenSoloRace(UITournamentBracketsMatchItemView p_match)
		{
			base.app.arguments.Clear();
			base.app.arguments.game.type = GameFlag.Race;
			base.app.arguments.game.mode = GameFlag.SinglePlayer;
			if ((bool)base.app.model.game)
			{
				base.app.model.game.type = GameFlag.Race;
			}
			if ((bool)base.app.model.game)
			{
				base.app.model.game.mode = GameFlag.SinglePlayer;
			}
			GamePlayerData gamePlayerData = base.app.arguments.game.AddPlayer(base.app.model.storage.state.player.playerData);
			base.app.arguments.game.tournamentData = model.tournament;
			base.app.arguments.game.tournamentMatchData = p_match.data;
			if (base.app.inVirtualSeason)
			{
				gamePlayerData.color2 = p_match.data.GetPlayerById(gamePlayerData.playerId).profileColor2;
			}
			bool isCustomMap = p_match.data.isCustomMap;
			string p_track_id = (isCustomMap ? string.Empty : p_match.data.trackId);
			string mapId = (isCustomMap ? string.Empty : p_match.data.mapId);
			DRLMap dRLMap = (isCustomMap ? null : base.app.model.storage.library.FindByGUID<DRLMap>(mapId));
			DRLMapTrack dRLMapTrack = null;
			MapData mapData = null;
			base.app.model.storage.GetMapTracks();
			base.app.view.audio.PlayUIStartGame();
			if (isCustomMap)
			{
				mapId = p_match.data.customMapId;
				if (string.IsNullOrEmpty(mapId))
				{
					return;
				}
				ServiceModel service = base.app.model.service;
				base.app.view.ui.fade.FadeIn(1.5f);
				service.GetCommunityMap(mapId, delegate(DRLCommunityMapResult p_result)
				{
					if (!(this == null))
					{
						DRLCommunityMapData dRLCommunityMapData = ((p_result.data.Length == 0) ? null : p_result.data[0]);
						if (dRLCommunityMapData == null)
						{
							Debug.LogWarning("UITournamentBracketsController> LoadCommunityMap / Failed to Load DRLCommunityMapData - guid[" + mapId + "]");
						}
						else
						{
							mapData = dRLCommunityMapData.Convert<MapData>();
							if (mapData == null)
							{
								Debug.LogWarning("UITournamentBracketsController> LoadCommunityMap / Failed to Parse MapData - guid[" + mapId + "]");
							}
							else
							{
								StopAll();
								DRLMap base_map = base.app.model.storage.library.FindByGUID<DRLMap>(mapData.mapId);
								DRLMapTrack dRLMapTrack2 = base.app.model.storage.GetMapTracks(base_map, GameFlag.Freestyle)[0];
								base_map.data = new MapData();
								base_map.data.Load(mapData.ToJson());
								base.app.arguments.game.map = base_map;
								base.app.arguments.game.track = dRLMapTrack2;
								base.app.arguments.game.podium = dRLMapTrack2.podium;
								base.app.arguments.game.fcMode = base.app.model.storage.state.player.activeFCMode;
								base.app.arguments.game.allowCrash = false;
								base.app.arguments.game.opponentType = OpponentModeType.Off;
								_ = ReplayFile.EnableVersion2;
								base.app.view.audio.SceneMainToGame(1.6f);
								Activity.RunOnce(delegate
								{
									base.app.scene.LoadCommunityMap(base_map.data.guid);
								}, 1f);
								base.app.model.storage.state.license.Poll();
							}
						}
					}
				});
			}
			else
			{
				StopAll();
				dRLMap.data = null;
				dRLMapTrack = base.app.model.storage.GetMapTrack(mapId, p_track_id, p_freestyle: false);
				base.app.arguments.game.map = dRLMap;
				base.app.arguments.game.track = dRLMapTrack;
				base.app.arguments.game.podium = dRLMapTrack.podium;
				base.app.arguments.game.fcMode = base.app.model.storage.state.player.activeFCMode;
				base.app.arguments.game.allowCrash = false;
				base.app.arguments.game.opponentType = OpponentModeType.Off;
				_ = ReplayFile.EnableVersion2;
				base.app.view.audio.SceneMainToGame(1.6f);
				base.app.view.ui.fade.FadeIn(1.5f);
				this.TimerRunOnce(delegate
				{
					base.app.scene.Load();
				}, 1f);
				base.app.model.storage.state.license.Poll();
			}
		}

		public void StopAll()
		{
			view.DisableMiniMap();
			Notify("tournament.brackets.close");
		}

		private void RefreshLobby()
		{
			view.UpdateLobbyButtons(model.CanJoin());
		}

		private void CheckJoinConditions(UITournamentBracketsMatchItemView p_matchItem)
		{
			if (p_matchItem == null)
			{
				view.enterMatchButton.interactable = false;
				view.StopRoomCountdownActivity();
			}
			else
			{
				if ((p_matchItem.data.gameMode != TournamentRoundGameMode.matchPoints && p_matchItem.data.gameMode != TournamentRoundGameMode.suddenDeath && p_matchItem.data.gameMode != TournamentRoundGameMode.goldenHeat) || !model.IsRacerInMatch(p_matchItem.data))
				{
					return;
				}
				Lobby.NetworkRoomInfo networkRoomInfo = network.lobby.Rooms.Find((Lobby.NetworkRoomInfo r) => r.MatchId == p_matchItem.data.Id);
				p_matchItem.ShowWaitOverlay();
				if (p_matchItem.data.state == TournamentMatchState.active)
				{
					if (network.connectionState == PhotonService.ServiceState.InLobby || network.connectionState == PhotonService.ServiceState.InRoom)
					{
						base.app.arguments.Clear();
						base.app.arguments.game.type = GameFlag.Race;
						base.app.arguments.game.mode = GameFlag.NetworkMultiplayer;
						if ((bool)base.app.model.game)
						{
							base.app.model.game.type = GameFlag.Race;
						}
						if ((bool)base.app.model.game)
						{
							base.app.model.game.mode = GameFlag.NetworkMultiplayer;
						}
						GamePlayerData playerData = base.app.model.storage.state.player.playerData;
						base.app.arguments.game.AddPlayer(playerData);
						base.app.arguments.game.tournamentData = view.tournament;
						base.app.arguments.game.tournamentMatchData = p_matchItem.data;
						if (base.app.inVirtualSeason)
						{
							playerData.color2 = p_matchItem.data.GetPlayerById(playerData.playerId).profileColor2;
						}
						if (playerData != null)
						{
							Debug.Log("UITournamentBracketsController> Joining match room - room id: " + p_matchItem.data.Id + " match state: " + p_matchItem.data.state.ToString() + " current heat: " + p_matchItem.data.currentHeat + " user: " + playerData.name + " player-id: " + playerData.playerId);
						}
						if (networkRoomInfo == null || networkRoomInfo.CanRace)
						{
							network.JoinTournamentMatch(p_matchItem.data);
						}
						else
						{
							view.UpdateMatchRoomData(networkRoomInfo, p_matchItem, p_joining: false, model.CanSpectate());
						}
					}
					else
					{
						view.enterMatchButton.interactable = false;
						view.StopRoomCountdownActivity();
					}
				}
				p_matchItem.HideWaitOverlay();
			}
		}

		private void CheckSpectateConditions(UITournamentBracketsMatchItemView p_match)
		{
			Lobby.NetworkRoomInfo roomInfo = network.lobby.Rooms.Find((Lobby.NetworkRoomInfo r) => r.MatchId == p_match.data.Id);
			if (roomInfo != null && !roomInfo.CanSpectate)
			{
				Debug.Log($"UITournamentBracketsController> Failed to join as spectator - MaxSpectators:[{roomInfo.MaxSpectators}] MaxRacers:[{roomInfo.MaxRacers}] MaxPlayers:[{roomInfo.MaxPlayers}] RoomState:[{roomInfo.State}] CanSpectate:[{roomInfo.CanSpectate}]");
				base.app.view.ui.dialog.Open(DialogType.Info, "ROOM FULL!", "No spectator slots available at this time.", new string[1] { "OK" });
			}
			p_match.ShowWaitOverlay();
			UpdateMatchBracket(p_match, delegate(DRLTournamentMatchData matchData)
			{
				p_match.HideWaitOverlay();
				if (matchData.state == TournamentMatchState.active)
				{
					if (network.connectionState == PhotonService.ServiceState.InLobby || network.connectionState == PhotonService.ServiceState.InRoom)
					{
						base.app.arguments.Clear();
						base.app.arguments.game.type = GameFlag.Race;
						base.app.arguments.game.mode = GameFlag.NetworkMultiplayer;
						if ((bool)base.app.model.game)
						{
							base.app.model.game.type = GameFlag.Race;
						}
						if ((bool)base.app.model.game)
						{
							base.app.model.game.mode = GameFlag.NetworkMultiplayer;
						}
						base.app.arguments.game.AddPlayer(base.app.model.storage.state.player.playerData);
						base.app.arguments.game.tournamentData = tournament;
						base.app.arguments.game.tournamentMatchData = p_match.data;
						if (roomInfo != null && roomInfo.CanSpectate && model.CanSpectate())
						{
							network.JoinTournamentMatch(p_match.data);
						}
						else
						{
							view.UpdateMatchRoomData(roomInfo, p_match);
						}
					}
					else
					{
						view.enterMatchButton.interactable = false;
						view.StopRoomCountdownActivity();
					}
				}
			});
		}

		public void RefreshAll(bool p_forcePopulate = false)
		{
			view.RefreshTournamentData(model.tournament, model.activeRound, model.activeMatch);
			if (tournament == null)
			{
				Debug.LogWarning("UITournamentBracketsController> No Tournaments Found!");
				return;
			}
			if (tournament.invalid)
			{
				Debug.LogWarning("UITournamentBracketsController> Tournament is INVALID!");
				TournamentExit(p_force: true);
				return;
			}
			UISecondaryHeaderView headerSecondary = base.app.view.ui.headerSecondary;
			if (headerSecondary != null)
			{
				headerSecondary.Refresh(view, p_is_under_review: false);
			}
			view.UpdateGeneralTournamentUI();
			view.SetMapCard(view.tournament.GetActiveRound());
			view.SetDroneCard(model.isRacer);
			if (tournament.rounds.Length != 0)
			{
				DRLTournamentRoundData dRLTournamentRoundData = tournament.rounds[0];
				DRLTournamentRoundData dRLTournamentRoundData2 = ((model.activeRound != null) ? model.activeRound : dRLTournamentRoundData);
				view.RefreshActiveRoundInfo(dRLTournamentRoundData2.totalPlayerCount, dRLTournamentRoundData.totalPlayerCount);
			}
			if (tournament.rounds.Length != view.matchColumnsList.Count)
			{
				Debug.LogWarning($"UITournamentBracketsView> Rounds Change /  from[{view.matchColumnsList.Count}] to[{tournament.rounds.Length}]");
				view.Populate(p_layoutChanged: true);
				return;
			}
			if (tournament.status == TournamentState.complete && !view.completeWinnersScreenShown && !model.isPastTournament)
			{
				if (tournament == null)
				{
					return;
				}
				StopAll();
				base.app.view.ui.screens.Open<UITournamentWinnersView>("tournament-leaders-screen", 1f).allowNext = false;
				view.completeWinnersScreenShown = true;
				view.headerNextRoundLabel.text = base.app.model.storage.locale.Get("vdrl.label.ended", "TOURNAMENT ENDED");
				view.winnersTournamentEndButton.SetActive(value: true);
			}
			if (model.isPastTournament)
			{
				view.winnersTournamentEndButton.SetActive(value: true);
			}
			for (int i = 0; i < view.matchColumnsList.Count; i++)
			{
				UITournamentBracketsMatchColumnItem uITournamentBracketsMatchColumnItem = view.matchColumnsList.Get<UITournamentBracketsMatchColumnItem>(i);
				if (i >= tournament.rounds.Length || uITournamentBracketsMatchColumnItem.matchesList.Count != tournament.rounds[i].matches.Length)
				{
					Debug.LogWarning("UITournamentBracketsView> Matches Changed /  index[" + i + "]  match-count-from[" + uITournamentBracketsMatchColumnItem.matchesList.Count + "]  match-count-to[" + tournament.rounds[i].matches.Length + "]");
					view.Populate(p_layoutChanged: true);
					return;
				}
				DRLTournamentRoundData dRLTournamentRoundData3 = tournament.rounds[i];
				if (dRLTournamentRoundData3 == null)
				{
					continue;
				}
				uITournamentBracketsMatchColumnItem.data = dRLTournamentRoundData3;
				for (int j = 0; j < uITournamentBracketsMatchColumnItem.matchesList.Count; j++)
				{
					UITournamentBracketsMatchItemView uITournamentBracketsMatchItemView = uITournamentBracketsMatchColumnItem.matchesList.Get<UITournamentBracketsMatchItemView>(j);
					DRLTournamentMatchData dRLTournamentMatchData = tournament.rounds[i].matches[j];
					bool flag = uITournamentBracketsMatchItemView.data.state != TournamentMatchState.active && dRLTournamentMatchData.state == TournamentMatchState.active;
					bool flag2 = uITournamentBracketsMatchItemView.data.state == TournamentMatchState.complete && dRLTournamentMatchData.state == TournamentMatchState.complete;
					if ((uITournamentBracketsMatchItemView.data.state == TournamentMatchState.fail && dRLTournamentMatchData.state == TournamentMatchState.fail) || flag2)
					{
						uITournamentBracketsMatchItemView.SetMatchState(uITournamentBracketsMatchItemView.data.state);
						continue;
					}
					if (uITournamentBracketsMatchItemView.gameMode == TournamentRoundGameMode.leaderboard)
					{
						view.enterMatchButton.interactable = view.activeMatchItem != null && view.activeMatchItem == uITournamentBracketsMatchItemView && model.CanJoin();
						if (dRLTournamentMatchData.state == TournamentMatchState.active)
						{
							SyncLeaderboardData();
						}
					}
					if (flag)
					{
						bool p_playersGroup = tournament.rounds[i].GetPlayerMatchIndex(base.app.model.storage.state.player.profile.playerId) == j;
						bool p_init = tournament.GetActiveRound().matches.Length > 1;
						uITournamentBracketsMatchItemView.Set(dRLTournamentMatchData, p_playersGroup, p_init);
						continue;
					}
					if (p_forcePopulate)
					{
						view.Populate();
						return;
					}
					_ = uITournamentBracketsMatchItemView.data.currentHeat;
					uITournamentBracketsMatchItemView.data = dRLTournamentMatchData;
					uITournamentBracketsMatchItemView.SetMatchState(dRLTournamentMatchData.state);
					uITournamentBracketsMatchItemView.UpdatePilotsOrder();
					if (dRLTournamentMatchData.state == TournamentMatchState.complete)
					{
						uITournamentBracketsMatchItemView.ColorWinners();
					}
				}
			}
			this.TimerRunOnce(delegate
			{
				if (base.validContext && !m_hasDoneCleanup && (!base.app.inGame || base.app.model.game.type != GameFlag.Race || !(base.app.model.game != null) || !(base.app.model.game.simulation != null) || !base.app.model.game.simulation.running))
				{
					GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
					Debug.Log("UITournamentBracketsController>  GC forced cleanup on tournaments refresh data..");
					m_hasDoneCleanup = true;
					this.TimerRunOnce(delegate
					{
						m_hasDoneCleanup = false;
					}, 3f);
				}
			}, 0.1f);
		}

		public void UpdateMatchBracket(UITournamentBracketsMatchItemView p_match, Action<DRLTournamentMatchData> callback = null)
		{
			model.RefreshMatchData(p_match.data.Id, delegate(DRLTournamentMatchData match)
			{
				if (p_match.data.state == TournamentMatchState.idle && match.state == TournamentMatchState.active)
				{
					match.Set(match);
				}
				else
				{
					p_match.data = match;
					p_match.SetMatchState(p_match.data.state);
				}
				if (callback != null)
				{
					callback(match);
				}
			});
		}

		public void SyncLeaderboardData()
		{
			if (tournament == null || view.activeMatchItem == null || model.activeMatch == null)
			{
				return;
			}
			base.app.model.service.GetTournamentResults(view.tournament.guid, model.activeMatch.roundId, delegate(DRLTournamentResultData p_result)
			{
				if (base.validContext && !(view == null))
				{
					List<string> list = new List<string>();
					if (p_result == null)
					{
						Debug.LogWarning("UITournamentBracketsController> Getting result for leaderboard mode match which appears to be a null");
						view.activeMatchItem.ColorPlayers(list);
					}
					else if (tournament != null && !(view.activeMatchItem == null) && model.activeMatch != null)
					{
						int num = 0;
						for (int i = 0; i < p_result.leaderboard.Length; i++)
						{
							if (p_result.leaderboard[i].score >= 0)
							{
								num++;
								list.Add(p_result.leaderboard[i].playerId);
							}
						}
						view.activeMatchItem.PilotCount = num + "/" + model.activeMatch.players.Length;
						view.activeMatchItem.ColorPlayers(list);
					}
				}
			});
		}

		public void SetIgnoredGameCommands()
		{
			if (base.app.model.game == null)
			{
				return;
			}
			List<GameCommand> list = new List<GameCommand>();
			if (!base.app.controller)
			{
				return;
			}
			foreach (GameInputMapComponent map in base.app.controller.game.input.maps)
			{
				foreach (GameCommand command in map.commands)
				{
					list.Add(command);
				}
			}
			base.app.controller.game.input.SetIgnoredCommands(list);
		}

		public void ClearIgnoredCommands()
		{
			if (!(base.app.model.game == null) && (bool)base.app.controller)
			{
				base.app.controller.game.input.ClearIgnoredCommands();
			}
		}

		private void UpdatePlayerDrone()
		{
			if (view.tournament == null)
			{
				return;
			}
			DRLTournamentData dRLTournamentData = view.tournament;
			GarageStateModel garage = base.app.model.storage.state.player.garage;
			DroneRigData currentRigData = garage.currentRigData;
			if (dRLTournamentData.droneClass > 0 && dRLTournamentData.droneClass != currentRigData.diameter)
			{
				foreach (DroneRigData officialRig in garage.officialRigs)
				{
					if (officialRig.diameter == dRLTournamentData.droneClass)
					{
						garage.currentRigData = officialRig;
						return;
					}
				}
			}
			if (string.IsNullOrEmpty(dRLTournamentData.droneGuid))
			{
				return;
			}
			foreach (DroneRigData officialRig2 in garage.officialRigs)
			{
				if (officialRig2.guid == dRLTournamentData.droneGuid)
				{
					garage.currentRigData = officialRig2;
					break;
				}
			}
		}
	}
}

using System;
using ExitGames.Client.Photon;
using UnityEngine;
using drl.backend;
using drl.network;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class NetworkController : Controller<DRLApp>
	{
		private string m_dialogId = "connection-disconnected";

		private bool m_sentPullUsers;

		private bool m_matchStartLocked;

		public NetworkModel model => AssertLocal<NetworkModel>("model");

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (this == null)
			{
				return;
			}
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "scene.game.scenes@start":
				Debug.Log("NetworkController> GameScenesStart");
				if (base.app.model.network.room != null)
				{
					model.StartKeepAliveLoop();
				}
				break;
			case "network.state.online":
				model.isOnline = true;
				base.app.view.ui.dialog.Close(m_dialogId);
				break;
			case "network.state.offline":
				model.isOnline = false;
				base.app.view.ui.dialog.Open(DialogTemplateType.ConnectionDisconnect, m_dialogId);
				break;
			case "network.player.all.ready":
				Debug.Log("NetworkController> PlayerReadyAll - Stopping Keep Alive Loop.");
				model.StopKeepAliveLoop(2f);
				if (model.room != null)
				{
					model.GenerateVotingTrackList();
				}
				break;
			case "network.room.load-game":
			{
				NetworkRoom room = model.room;
				if (room == null)
				{
					break;
				}
				NetworkRoom.LoadGameData loadGameData = (NetworkRoom.LoadGameData)p_data[0];
				Debug.Log("NetworkController > OnNotification - RoomLoadGame map[" + loadGameData.Map + "] track[" + loadGameData.Track + "]");
				DRLTournamentMatchData tournamentMatchData = base.app.arguments.game.tournamentMatchData;
				base.app.arguments.Clear();
				base.app.arguments.game.tournamentMatchData = tournamentMatchData;
				DRLAppArguments.Game args = base.app.arguments.game;
				args.type = model.GetGameType(loadGameData.GameType);
				args.mode = GameFlag.NetworkMultiplayer;
				if (room.IsUsingGhosts)
				{
					args.opponentType = OpponentModeType.Rival5;
					if (ReplayFile.EnableVersion2)
					{
						ReplayRecord ghostRecordsV = base.app.model.service.opponent.ghostRecordsV2;
						args.AddGhostPlayer(ghostRecordsV);
					}
					else
					{
						BlackboxRecord ghostRecords = base.app.model.service.opponent.ghostRecords;
						args.AddGhostPlayer(ghostRecords);
					}
				}
				args.map = base.app.model.storage.library.FindByGUID<DRLMap>(loadGameData.Map);
				args.track = base.app.model.storage.library.FindByGUID<DRLMapTrack>(loadGameData.Track);
				if (room.IsTournamentMatch)
				{
					RunOnce(0.1f, delegate
					{
						base.app.model.service.GetTournament(room.TournamentId, delegate(DRLTournamentResult result)
						{
							if (!(this == null) && !(base.app == null) && !(base.app.view == null) && !(base.gameObject == null) && result.tournaments.Length != 0)
							{
								DRLTournamentData dRLTournamentData2 = result.tournaments[0];
								args.tournamentData = dRLTournamentData2;
								string arg = (string.IsNullOrEmpty(room.MatchId) ? "" : room.MatchId);
								int heatIdx = room.HeatIdx;
								Debug.Log($"NetworkController> N.Network.RoomLoadGame: \nTournamentData loaded Id[{dRLTournamentData2.guid}]\nMatchId [{arg}]\nHeatIdx [{heatIdx}]");
							}
						});
					});
				}
				if (!args.map)
				{
					Debug.LogWarning("NetworkController> Map [" + loadGameData.Map + "] not found!");
					break;
				}
				if (!loadGameData.IsCustomMap && !args.track)
				{
					Debug.LogWarning("NetworkController> Track [" + loadGameData.Track + "] not found!");
					break;
				}
				NetworkActor local = model.room.Local;
				GamePlayerData gamePlayerData = args.AddPlayer(base.app.model.storage.state.player.playerData);
				gamePlayerData.SetNetwork(local);
				Debug.Log("UIMultiplayerRoomController> CreatePlayer - id[" + gamePlayerData.id + "] order[" + gamePlayerData.order + "] name[" + gamePlayerData.name + "] spectator[" + local.IsSpectator + "] created!");
				base.app.view.audio.PlayUIMultiplayerRoomStart();
				model.StartKeepAliveLoop();
				if (loadGameData.IsCustomMap)
				{
					Debug.Log("NetworkController> Will load Community Map id[" + loadGameData.CustomMapId + "]");
					Notify("network.custom-map.load@start");
					base.app.scene.LoadCommunityMap(loadGameData.CustomMapId, delegate
					{
						base.app.view.audio.SceneMainToGame(1.6f);
					});
				}
				else
				{
					Debug.Log("NetworkController> Will load standard Map id[" + loadGameData.Map + "]");
					base.app.view.audio.PlayUIStartGame();
					Activity.RunOnce(LoadRoomLevel, 0.5f);
				}
				break;
			}
			case "network.room.update":
			{
				if (model.room == null)
				{
					break;
				}
				if (model.room.IsMaster)
				{
					model.room.UpdatePlayerIds();
				}
				Hashtable hashtable = (Hashtable)p_data[0];
				if (hashtable != null && hashtable.ContainsKey("lc"))
				{
					int lobbyCountdown = model.room.LobbyCountdown;
					if (model.room.IsMaster && model.room.IsTournamentMatch && lobbyCountdown > 0 && lobbyCountdown <= 10 && !m_sentPullUsers)
					{
						m_sentPullUsers = true;
						base.app.model.service.SendMatchStartingSocketEvent(model.room.MatchId);
					}
					if (lobbyCountdown <= 5 && lobbyCountdown != 0 && model.room.State == NetworkRoom.StateCode.MatchMaking)
					{
						base.app.view.audio.PlayUIMultiplayerRoomCount(lobbyCountdown);
					}
				}
				break;
			}
			case "input.active-controller.changed":
			case "settings.controller.connect":
				if (model.room != null && model.room.Local != null)
				{
					model.room.Local.ControllerType = (int)RCI.GetControllerStateType(ControllerStateType.Taranis);
				}
				break;
			case "missions.dmv.rank.updated":
				if (model.room != null)
				{
					PlayerStateModel player2 = base.app.model.storage.state.player;
					model.room.Local.Set(player2);
				}
				break;
			case "notifications.action":
			{
				if (p_data == null)
				{
					break;
				}
				string text = "none";
				if (p_data[0] is InviteNotificationData)
				{
					text = "notification";
				}
				if (p_data[0] is PlatformGameInvite)
				{
					text = "platform";
				}
				string platformId = base.app.model.storage.state.player.profile.platformId;
				string owner_id = "";
				string invite_room = "";
				string text2 = "";
				CloudRegionCode invite_region = CloudRegionCode.us;
				NotificationTypeFlag invite_type = NotificationTypeFlag.RoomInvite;
				if (model.room != null)
				{
					text2 = model.room.Id;
				}
				switch (text)
				{
				case "none":
					Debug.LogWarning($"NetworkController> Notifications.Action / Invalid Invite Data - data[{p_data[0]}]");
					break;
				case "notification":
				{
					InviteNotificationData inviteNotificationData = p_data[0] as InviteNotificationData;
					owner_id = inviteNotificationData.platformId;
					invite_room = inviteNotificationData.inviteRoomId;
					invite_region = (CloudRegionCode)inviteNotificationData.inviteRegionCode;
					invite_type = inviteNotificationData.type;
					break;
				}
				case "platform":
				{
					PlatformGameInvite platformGameInvite = p_data[0] as PlatformGameInvite;
					owner_id = platformGameInvite.from;
					invite_room = platformGameInvite.room;
					invite_type = NotificationTypeFlag.RoomInvite;
					if (!Enum.TryParse<CloudRegionCode>(platformGameInvite.region, out invite_region))
					{
						invite_region = CloudRegionCode.us;
					}
					break;
				}
				}
				if (text == "none")
				{
					break;
				}
				if (!string.IsNullOrEmpty(text2) && text2 == invite_room)
				{
					Debug.Log("NetworkController> Notifications.Action / Already in Room");
					break;
				}
				if (owner_id == platformId)
				{
					Debug.Log("NetworkController> Notifications.Action / Ignoring Self Invite");
					break;
				}
				this.TimerRunOnce(delegate
				{
					base.app.model.service.platform.CheckPlatformMultiplayerPrivilege(delegate
					{
						if (1 == 0)
						{
							Debug.Log("NetworkController> Notifications.Action / CheckPlatformMultiplayerPrivilege - Not Allowed!");
						}
						else
						{
							bool flag = false;
							if ((bool)base.app.view.ui.screens.current)
							{
								flag = base.app.view.ui.screens.current.name == "multiplayer-room-screen";
							}
							switch (invite_type)
							{
							case NotificationTypeFlag.RoomInvite:
								Debug.Log($"NetworkController> Notifications.Action / JoinRoom - owner[{owner_id}] region[{invite_region}] room[{invite_room}] was-room-open[{flag}]");
								model.TryJoinRoomInRegion(invite_region, invite_room);
								break;
							case NotificationTypeFlag.QuickMatchInvite:
								Debug.Log($"NetworkController> Notifications.Action / JoinQuickMatch - owner[{owner_id}] region[{invite_region}] room[{invite_room}] was-room-open[{flag}]");
								model.TryJoinQuickMatchInRegion(invite_region, invite_room);
								break;
							}
						}
					});
				}, UnityEngine.Random.Range(0f, 1f));
				break;
			}
			case "network.room@enter":
			{
				if (model.room == null || model.room.IsQuickMatch)
				{
					break;
				}
				Debug.Log("NetworkController> RoomEnter / owner[" + base.app.model.service.platform.id.ToString() + "] id[" + model.room.Id + "]");
				base.app.view.audio.StopUILoadingLoop();
				string text5 = ((model.room == null) ? "NULL ROOM" : model.room.RoomTitle.ToUpper());
				text5 = model.room.GameMode.ToString().ToUpper() + "/" + text5;
				Debug.Log("NetworkController> RoomEnter / header-title[" + text5 + "]");
				float p_delay = (base.app.inGame ? 0.5f : 0f);
				UIMultiplayerRoomView uIMultiplayerRoomView = base.app.view.ui.screens.Open<UIMultiplayerRoomView>("multiplayer-room-screen", p_delay);
				uIMultiplayerRoomView.screen.title = text5;
				uIMultiplayerRoomView.leaveRoomOnExit = true;
				uIMultiplayerRoomView.Clear();
				uIMultiplayerRoomView.SetGameType(model.room.GameMode);
				uIMultiplayerRoomView.SetAvailableOptions(model.room.IsMaster, model.room.IsTournamentMatch);
				Debug.Log($"NetworkController> RoomEnter / UIMultiplayerRoomView.Open - ingame[{base.app.inGame}]");
				if (base.app.inGame)
				{
					GameController game = base.app.controller.game;
					GameTypeController gameTypeController = ((!game) ? null : (game.input ? game.input.controller : null));
					UIHUD uIHUD = ((!game) ? null : (game.ui ? game.ui.hud : null));
					if ((bool)gameTypeController)
					{
						gameTypeController.Pause(p_flag: true, p_pause_physics: true, p_open_pause_screen: false);
					}
					if ((bool)uIHUD)
					{
						uIHUD.Hide(0f);
					}
					if (!game)
					{
						Debug.Log("NetworkController> RoomEnter / GameController is <null>");
					}
					if (!gameTypeController)
					{
						Debug.Log("NetworkController> RoomEnter / GameTyprController is <null>");
					}
					if (!uIHUD)
					{
						Debug.Log("NetworkController> RoomEnter / HUD is <null>");
					}
				}
				Debug.Log($"NetworkController> RoomEnter / is-tournament[{model.room.IsTournamentMatch}]");
				if (!model.room.IsTournamentMatch)
				{
					model.room.MapId = "MP-3fd";
					model.room.CustomMapId = "CMP-af26895e90b0f65bcbc80f14";
				}
				Debug.Log("NetworkController> RoomEnter / LoadLocalTunning");
				LoadLocalTuning(base.app.model.storage.state.player.settings.tuning.GetActive());
				Debug.Log("NetworkController> RoomEnter / Notify DisableServerList");
				Notify("network.lobby.server-list@disable");
				break;
			}
			case "settings.tuning.profile.save":
			{
				FCProfileData tunningData = (FCProfileData)p_data[0];
				LoadLocalTuning(tunningData);
				break;
			}
			case "settings.profile.color@changed":
			{
				if (p_data.Length == 0)
				{
					break;
				}
				Color color = (Color)p_data[0];
				if (model.InRoom)
				{
					model.room.Local.ProfileColor = color;
					if (!model.room.AutoColor)
					{
						model.room.Local.MainColor = color;
					}
				}
				break;
			}
			case "tournament.action.start-match":
			{
				if (!base.validContext || model.room == null || model.room.GameMode != NetworkRoom.GameType.Tournament || !model.InRoom || p_data.Length == 0)
				{
					break;
				}
				string text3 = (string)p_data[0];
				Debug.Log("NetworkController> Tournament start event received for match - " + text3);
				if (!string.IsNullOrEmpty(text3) && !(model.room.MatchId != text3) && !m_matchStartLocked && model.room.State == NetworkRoom.StateCode.MatchMaking)
				{
					if (!model.room.IsMaster)
					{
						model.room.Outgoing.SendRaceReady();
						break;
					}
					m_matchStartLocked = true;
					model.room.RaceId = GUID.Create(24, "", 200, 0, 15, "x1");
					model.room.GamePlugin.StartMatch();
					Debug.Log("NetworkController> Starting tournament heat: " + model.room.HeatIdx + " at: " + DateTime.Now.ToString() + " match-id-room " + model.room.MatchId + " backend - " + text3);
				}
				break;
			}
			case "tournament.action.reset-match":
				if (base.validContext && model.room != null && model.room.GameMode == NetworkRoom.GameType.Tournament && model.InRoom && p_data.Length != 0)
				{
					string text4 = (string)p_data[0];
					Debug.Log("NetworkController> Tournament reset event received for match - " + text4);
					if (!string.IsNullOrEmpty(text4) && !(model.room.MatchId != text4) && model.room.IsMaster)
					{
						base.app.model.service.SetMatchHeat(base.app.model.tournament.guid, text4, 0);
					}
				}
				break;
			case "network.room@lock":
			{
				if (base.app.inGame)
				{
					base.app.model.game.type = GameFlag.Race;
					base.app.arguments.game.type = GameFlag.Race;
				}
				this.TimerRunOnce(delegate
				{
					m_matchStartLocked = false;
					m_sentPullUsers = false;
					Debug.Log("NetworkController> Match locked at: " + DateTime.Now.ToString());
				}, 1f / 6f);
				model.ResetDamage();
				PlayerStateModel player = base.app.model.storage.state.player;
				if (model.room.DRLPilotMode)
				{
					player.activeFCMode = FCMode.DRLPilot;
				}
				else if (player.activeFCMode == FCMode.DRLPilot)
				{
					player.activeFCMode = FCMode.Pro;
				}
				break;
			}
			case "network.player.order@update":
			{
				if (model.room == null || model.room.State == NetworkRoom.StateCode.GameRunning || !model.room.IsTournamentMatch || (base.app.inGame && base.app.arguments.game.tournamentData == null) || (base.app.inMain && base.app.arguments.tournament == null))
				{
					break;
				}
				DRLTournamentData tdata = (base.app.inGame ? base.app.arguments.game.tournamentData : base.app.arguments.tournament.data);
				if (tdata == null || tdata.GetActiveRoundMode() == TournamentRoundGameMode.leaderboard)
				{
					break;
				}
				_ = base.app.model.storage.state.player.profile.playerId;
				string mdt = model.room.MatchId;
				if (string.IsNullOrEmpty(mdt))
				{
					break;
				}
				base.app.model.tournament.RefreshMatchData(mdt, delegate(DRLTournamentMatchData p_result)
				{
					if (base.validContext)
					{
						if (p_result == null)
						{
							DRLTournamentMatchData activeMatch2 = base.app.model.tournament.activeMatch;
							model.room.HeatIdx = activeMatch2?.currentHeat ?? (model.room.HeatIdx + 1);
							base.app.model.service.SetMatchHeat(tdata.guid, mdt, model.room.HeatIdx);
						}
						else
						{
							model.tournamentMatchData = p_result;
							model.UpdateAutoColor();
							if (model.isMaster)
							{
								Debug.Log("NetworkController> playerOrder: " + string.Join(", ", p_result.playerOrder));
								for (int i = 0; i < p_result.players.Length; i++)
								{
									DRLTournamentPlayerData dRLTournamentPlayerData = p_result.players[i];
									Debug.Log($"NetworkController> index: #{i} | playerId: {dRLTournamentPlayerData.playerId} profileName: {dRLTournamentPlayerData.profileName}");
								}
								model.room.HeatIdx = p_result.currentHeat;
								base.app.model.service.SetMatchHeat(tdata.guid, mdt, model.room.HeatIdx);
								model.room.SetCustomOrder(p_result.playerOrder);
								this.TimerRunOnce(delegate
								{
									model.room.Outgoing.SendMatchLocked();
								}, 0.3f);
							}
						}
					}
				});
				break;
			}
			case "network.player.room@enter":
			case "game.race.slowmo@stop":
			case "tournament.action.refresh-racers":
			{
				Debug.Log("NetworkController> " + p_event + " / Checking Flags");
				if (model.room == null || !model.room.IsMaster || model.room.State == NetworkRoom.StateCode.GameRunning || !model.room.IsTournamentMatch)
				{
					break;
				}
				Debug.Log("NetworkController> " + p_event + " / Checking if Tournament");
				Debug.Log("NetworkController> Sudden Death/Golden Heat checking racing order!");
				if (!base.app.inTournament)
				{
					break;
				}
				Debug.Log("NetworkController> " + p_event + " / Checking Tournament Data");
				DRLTournamentData dRLTournamentData = (base.app.inGame ? base.app.arguments.game.tournamentData : base.app.arguments.tournament.data);
				if (dRLTournamentData == null)
				{
					break;
				}
				TournamentRoundGameMode mode = dRLTournamentData.GetActiveRoundMode();
				Debug.Log("NetworkController> Sudden Death/Golden checking seating for round - mode: " + mode);
				if (mode != TournamentRoundGameMode.suddenDeath && mode != TournamentRoundGameMode.goldenHeat)
				{
					break;
				}
				string mid = model.room.MatchId;
				if (string.IsNullOrEmpty(mid))
				{
					break;
				}
				Debug.Log("NetworkController> SuddenDeath/GoldenHeat fetching new match data for. Match " + mid);
				base.app.model.tournament.RefreshMatchData(mid, delegate(DRLTournamentMatchData p_result)
				{
					if (!base.validContext || p_result == null)
					{
						Notify(3f, "tournament.action.refresh-racers");
						Debug.Log("NetworkController> SuddenDeath/GoldenHeat missing results or context not valid! Context:" + base.validContext + " Results: " + (p_result != null));
						if (base.validContext && p_result != null)
						{
							Debug.Log("NetworkController> SuddenDeath/GoldenHeat missing match! -- " + mid);
						}
					}
					else if (model.room != null)
					{
						model.tournamentMatchData = p_result;
						if (p_result.playerOrder != null)
						{
							bool num = model.room.SetCustomOrder(p_result.playerOrder);
							bool flag = p_event == "network.player.room@enter";
							if (num || flag)
							{
								Notify(0.5f, "network.player@update");
							}
						}
						model.UpdateAutoColor();
						if (p_result.currentHeat >= p_result.heatCount)
						{
							Debug.Log("NetworkController> SuddenDeath/GoldenHeat trying to switch racers to spectators!");
							for (int i = 0; i < p_result.players.Length; i++)
							{
								DRLTournamentPlayerData p = p_result.players[i];
								if (p != null && ((mode == TournamentRoundGameMode.suddenDeath && p.isWinner) || (mode == TournamentRoundGameMode.goldenHeat && !p.isWinner)))
								{
									NetworkActor networkActor = model.room.Racers.Find((NetworkActor r) => r.PlayerId == p.playerId);
									if (networkActor != null && !networkActor.IsSpectator)
									{
										Debug.Log("NetworkController> Game mode: " + mode.ToString() + " switching name: " + networkActor.ProfileName + " to spectator!");
										model.room.TrySwitchToSpectator(networkActor, forced: true, p_notify: false);
									}
								}
							}
						}
					}
				});
				break;
			}
			case "tournament.action.refresh":
			{
				Debug.Log("NetworkController> " + p_event + " / Checking Flags");
				if (model.room == null || !model.room.IsMaster || model.room.State == NetworkRoom.StateCode.GameRunning || !model.room.IsTournamentMatch)
				{
					break;
				}
				Debug.Log("NetworkController> " + p_event + " / Checking if Tournament");
				Debug.Log("NetworkController> Sudden Death/Golden Heat checking racing order!");
				if (!base.app.inTournament)
				{
					break;
				}
				if (base.app.model.tournament.tournament == null || base.app.model.tournament.activeMatch == null)
				{
					Debug.LogWarning("NetworkController> " + p_event + " / Tournament data not present.");
					break;
				}
				DRLTournamentMatchData activeMatch = base.app.model.tournament.activeMatch;
				if (activeMatch.playerOrder != null && model.room.SetCustomOrder(activeMatch.playerOrder))
				{
					Notify(1f / 6f, "network.player@update");
				}
				break;
			}
			case "network.drone-damage.update":
				if (p_data.Length >= 2)
				{
					int p_networkId = (int)p_data[0];
					if (p_data[1] is NetworkRoom.DamageData damageData)
					{
						model.SetDamage(p_networkId, damageData.bodyDamage, damageData.prop0Damage, damageData.prop1Damage, damageData.prop2Damage, damageData.prop3Damage);
					}
				}
				break;
			}
		}

		protected void LoadRoomLevel()
		{
			GameModel game = base.app.model.game;
			DRLAppArguments.Game game2 = base.app.arguments.game;
			if (game != null)
			{
				game.Set(base.app.arguments);
			}
			if ((bool)game)
			{
				GameFlag type = game.type;
				if ((uint)(type - 13) <= 1u)
				{
					base.app.controller.game.StartMap(game2.map, game2.track, game2.map.data);
				}
			}
			else
			{
				base.app.view.audio.FadeStopMusicMain(1.6f);
				base.app.view.ui.fade.FadeIn(1.5f);
				Activity.RunOnce(base.app.scene.Load, 1f);
			}
		}

		private void LoadLocalTuning(FCProfileData tunningData)
		{
			if (model.InRoom && tunningData != null)
			{
				model.room.Local.CameraTilt = tunningData.tilt;
				model.room.Local.CameraFOV = tunningData.fov;
			}
		}

		public void OnPersistency()
		{
			base.app.controller.network = this;
		}
	}
}

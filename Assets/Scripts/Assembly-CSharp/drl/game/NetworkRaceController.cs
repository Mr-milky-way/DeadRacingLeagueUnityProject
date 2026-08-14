using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using drl.backend;
using drl.network;
using drl.sim;
using thelab.core;

namespace drl.game
{
	public class NetworkRaceController : RaceController
	{
		public bool allReplaysProcessed;

		private bool checkConnection;

		public bool isTournamentServerReady;

		private bool m_firstRacerFinished;

		private MonoActivity m_replay_save_loop;

		public NetworkRaceModule network => AssertLocal<NetworkRaceModule>("network");

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "game.simulation.drone.all@ready":
				return;
			case "game.pause":
				return;
			}
			base.OnNotification(p_event, p_target, p_data);
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "network.race.replay.ready.all":
				break;
			case "game.ready":
				base.app.controller.game.input.pausePhysics = false;
				allReplaysProcessed = false;
				isTournamentServerReady = false;
				m_firstRacerFinished = false;
				checkConnection = true;
				break;
			case "network.player.room@enter":
			{
				NetworkActor p_player3 = (NetworkActor)p_data[0];
				network.AddPlayer(p_player3);
				break;
			}
			case "network.player.room@exit":
			{
				int p_player_id = (int)p_data[0];
				network.RemovePlayer(p_player_id);
				RunOnce(Time.deltaTime * 2f, base.model.RefreshStandings);
				break;
			}
			case "network.room@exit":
				base.model.ClearData();
				base.game.SetGCEnabled(p_flag: true);
				break;
			case "network.instantiate.local":
				if (base.app.model.network.room != null)
				{
					NetworkActor local4 = base.app.model.network.room.Local;
					network.AddDrone(local4);
					if (local4.IsSpectator)
					{
						base.app.model.network.SendPlayerReady();
						base.app.model.network.SendPlayerCountdownReady();
					}
					if (!introComplete)
					{
						introComplete = true;
					}
				}
				break;
			case "network.instantiate.remote":
			{
				NetworkActor p_player = (NetworkActor)p_data[0];
				network.AddDrone(p_player);
				break;
			}
			case "tournament.countdown-start":
				Debug.Log("NetworkRaceController> OnNotification.TournamentCountdownStart / Server Ready!");
				isTournamentServerReady = true;
				break;
			case "network.local.transmitter.added":
			{
				LoadPlayerTuning();
				LoadCameraSettings();
				Debug.Log("NetworkRaceController> OnNotification.LocalDroneAdded / Sending Ready Signal!");
				base.app.model.network.SendPlayerReady();
				bool is_tournament = base.app.arguments.game.isTournamentActive;
				Debug.Log($"NetworkRaceController> OnNotification.LocalDroneAdded / tournament[{is_tournament}]");
				float t = 1f;
				float t_service = 3f;
				if (is_tournament)
				{
					base.game.input.SetController(this);
				}
				if (base.app.inTournament && !base.app.tournament.hasCountdown)
				{
					isTournamentServerReady = true;
				}
				bool sent_ready = false;
				((Component)this).ActivityRun((Func<bool>)delegate
				{
					if (!base.validContext || base.model.raceComplete || sent_ready)
					{
						return false;
					}
					t += Time.deltaTime;
					if (t < 1f)
					{
						return true;
					}
					t = 0f;
					if (is_tournament && !isTournamentServerReady)
					{
						Debug.Log($"NetworkRaceController> OnNotification.LocalDroneAdded / Waiting for Ready Signal - tournament[{is_tournament}] ");
						if (base.app.inTournament && base.app.model.network.room != null && !string.IsNullOrEmpty(base.app.model.network.room.MatchId))
						{
							if (t_service >= 3f)
							{
								t_service = 0f;
								base.app.model.service.GetTournamentCountdownState(base.app.tournament.guid, base.app.model.network.room.MatchId, delegate(DRLServiceResult result)
								{
									if (!(!base.validContext || sent_ready) && result != null)
									{
										bool data = result.GetData<bool>();
										if (data)
										{
											isTournamentServerReady = true;
										}
										Debug.Log("NetworkRaceController> Fetching countdown state - " + data);
									}
								});
							}
							t_service += 1f;
						}
						return true;
					}
					Debug.Log("NetworkRaceController> OnNotification.LocalDroneAdded / Sending Countdown ready Signal!");
					base.app.model.network.SendPlayerCountdownReady();
					sent_ready = true;
					return false;
				}, 0f);
				break;
			}
			case "network.player.racer":
				if (base.app.model.network.room != null && base.app.model.network.room.State != NetworkRoom.StateCode.GameRunning && base.app.model.network.room.State != NetworkRoom.StateCode.GameFinished && base.app.model.network.room.State != NetworkRoom.StateCode.MatchMaking)
				{
					NetworkActor p_player2 = (NetworkActor)p_data[0];
					network.SetPlayerState(p_player2, GamePlayerType.Network);
				}
				break;
			case "network.player.spectator":
				if (base.app.model.network.room != null)
				{
					Debug.Log("NetworkRaceController> PlayerToSpectator - room-state[" + base.app.model.network.room.State.ToString() + "]");
					if (base.app.model.network.room.State != NetworkRoom.StateCode.GameFinished && base.app.model.network.room.State != NetworkRoom.StateCode.GameRunning && base.app.model.network.room.State != NetworkRoom.StateCode.MatchMaking)
					{
						NetworkActor p_player4 = (NetworkActor)p_data[0];
						network.SetPlayerState(p_player4, GamePlayerType.Spectator);
					}
				}
				break;
			case "network.player.all.ready":
			{
				CompleteIntroAnimation();
				if (base.app.model.network.room == null)
				{
					break;
				}
				NetworkActor local = base.app.model.network.room.Local;
				if (local.IsLocal && local.IsSpectator)
				{
					UISpectateView uISpectateView = base.app.view.ui.screens.Open<UISpectateView>("game-spectate-screen");
					UISpectateController gsc = uISpectateView.GetComponent<UISpectateController>();
					gsc.Initialize();
					List<GamePlayerData> gpd = base.game.model.players;
					gsc.model.AddTargets(gpd);
					uISpectateView.tournamentContext = base.app.inTournament;
					this.TimerRunOnce(delegate
					{
						gsc.model.SetTargets(gpd);
					}, 1f / 30f);
				}
				break;
			}
			case "network.room.no.racers":
				if (base.app.model.network.room != null && base.app.model.network.room.State != NetworkRoom.StateCode.GameFinished && base.app.model.network.room.State != NetworkRoom.StateCode.MatchMaking && !base.app.inTournament)
				{
					Debug.Log("NetworkRaceController> No Racers Left");
					base.app.model.network.LeaveRoom();
					checkConnection = false;
					base.game.Exit();
				}
				break;
			case "network.race.count":
			{
				int num8 = Reflection<object>.Get<int>(p_data, 0);
				int p_max = Reflection<object>.Get<int>(p_data, 1);
				ApplyCount(num8, p_max, p_play_audio: true, num8 == 2);
				break;
			}
			case "network.race.count@complete":
				OnCountComplete();
				break;
			case "game.race.gate@step":
			{
				int num3 = Reflection<object>.Get<int>(p_data, 0);
				Drone drone2 = Reflection<object>.Get<Drone>(p_data, 3);
				if (!(drone2 == null) && num3 < base.model.gates.Count && base.app.model.game.playerDrone == drone2)
				{
					base.app.model.network.SendGateEvent(num3);
				}
				break;
			}
			case "network.race.gate@hit":
			{
				int actorId2 = Reflection<object>.Get<int>(p_data, 0);
				int num9 = Reflection<object>.Get<int>(p_data, 1);
				NetworkActor player2 = base.app.model.network.GetPlayer(actorId2);
				Drone drone4 = ((player2 == null) ? null : base.game.model.GetPlayerDataById(player2.ID))?.drone;
				if ((bool)drone4 && !player2.IsLocal && num9 < base.model.gates.Count)
				{
					ProcessGate(num9, drone4);
				}
				break;
			}
			case "network.player.completed.race":
			{
				int playerId = Reflection<object>.Get<int>(p_data, 0);
				RunOnce(Time.deltaTime * 2f, base.model.RefreshStandings);
				NetworkRoom nroom = base.app.model.network.room;
				base.model.RefreshStandings();
				if (nroom == null)
				{
					break;
				}
				GamePlayerData playerDataById = base.game.model.GetPlayerDataById(playerId);
				if (playerDataById == null)
				{
					break;
				}
				Drone drone = playerDataById.drone;
				if (drone == null)
				{
					break;
				}
				drone.invulnerable = 60f;
				drone.renderer.SetTrailsActive(p_flag: false);
				if (playerDataById.type == GamePlayerType.Network)
				{
					if (nroom.SceneRacers.ContainsKey(playerId))
					{
						float num = ((base.app.model.network.lobby != null) ? ((float)base.app.model.network.lobby.PingTime / 1000f) : 0.1f);
						this.TimerRunOnce(delegate
						{
							nroom.SceneRacers[playerId].interpolate = false;
						}, num * 2f);
					}
					Notify("network.remote.drone.finished", drone);
				}
				if (playerDataById.type != GamePlayerType.Human)
				{
					drone.fc.armed = false;
				}
				break;
			}
			case "network.room.first-racer-finshed":
			{
				NetworkRoom r = base.app.model.network.room;
				((Component)this).ActivityRun((Func<bool>)delegate
				{
					if (base.app.model.network.room == null || base.app.arguments.game.tournamentData != null || !base.validContext)
					{
						return false;
					}
					float elapsedTime = base.app.model.network.room.ElapsedTime;
					if (elapsedTime < 1f)
					{
						return true;
					}
					float num10 = ((elapsedTime > 180f) ? 120f : 60f);
					if (r.IsMaster)
					{
						r.TimeLimit = num10 + r.ElapsedTime;
					}
					StartCountdownTimer(num10);
					return false;
				}, 0f);
				break;
			}
			case "network.player.forfeit.race":
			{
				RunOnce(Time.deltaTime * 2f, base.model.RefreshStandings);
				int actorId = Reflection<object>.Get<int>(p_data, 0);
				NetworkRoom room5 = base.app.model.network.room;
				if (room5 == null)
				{
					break;
				}
				NetworkActor networkActor2 = room5.TryGetPlayer(actorId);
				if (networkActor2.IsLocal && !networkActor2.IsSpectator)
				{
					if (base.app.inGame && base.app.arguments.game.tournamentData != null)
					{
						base.app.model.service.WatchTournamentRefresh();
					}
					base.game.SetGCEnabled(p_flag: true);
					base.OnRaceComplete(networkActor2.RaceTime, RaceStatusType.Forfeit);
				}
				break;
			}
			case "network.player.crashed":
			{
				if (p_data.Length == 0)
				{
					break;
				}
				NetworkRoom room4 = base.app.model.network.room;
				if (room4 == null)
				{
					break;
				}
				NetworkRoom.DroneState crashData = p_data[0] as NetworkRoom.DroneState;
				if (crashData == null)
				{
					Debug.LogWarning("NetworkRaceController> Drone crashed but no crash data present!");
					break;
				}
				NetworkActor nact = room4.TryGetPlayer(crashData.PlayerId);
				if (nact == null || nact.IsSpectator)
				{
					break;
				}
				GamePlayerData gpdr = base.game.model.GetPlayerDataById(crashData.PlayerId);
				Drone drn = gpdr.drone;
				if (drn == null)
				{
					break;
				}
				if (nact.IsLocal)
				{
					this.TimerRunOnce(delegate
					{
						RaceStatusType raceStatusType = (base.model.IsComplete(drn) ? RaceStatusType.Success : RaceStatusType.Crash);
						gpdr.raceStatus = raceStatusType;
						OnRaceDroneComplete(drn, nact.RaceTime, raceStatusType);
						base.model.RefreshStandings();
					}, 0.1f);
					break;
				}
				if (gpdr.type != GamePlayerType.Network)
				{
					break;
				}
				float num4 = ((base.app.model.network.lobby != null) ? ((float)base.app.model.network.lobby.PingTime / 1000f) : 0.3f);
				Quaternion.Euler(crashData.Rotation);
				float num5 = num4;
				if ((bool)base.game.model.simulation)
				{
					DroneNetworkTransmitter dnt = base.game.model.simulation.transmitters.GetByDrone<DroneNetworkTransmitter>(gpdr.drone);
					Vector3 position = crashData.Position;
					NetworkRacer networkRacer = base.app.model.network.room.SceneRacers[crashData.PlayerId];
					if (networkRacer != null)
					{
						float num6 = Vector3.Distance(position, networkRacer.networkPosition);
						float magnitude = networkRacer.networkVelocity.magnitude;
						magnitude = Mathf.Clamp(magnitude, 1f, 200f);
						float num7 = num6 / magnitude;
						num5 += num7;
					}
					this.TimerRunOnce(delegate
					{
						if (dnt != null)
						{
							dnt.SetPhysics(isEnabled: true, crashData);
						}
					}, num5);
				}
				if (crashData.CrashEnergy > 0f)
				{
					gpdr.drone.CrashRemote(crashData.CrashEnergy, crashData.ContactNormal, crashData.ImpactVelocity, crashData.ContactPoint, num5);
				}
				Debug.Log("NetworkRaceController> Network user [" + gpdr.upperName + "] crashed! ");
				Notify("network.remote.drone.finished", gpdr.drone);
				break;
			}
			case "network.player.damage":
				if (p_data.Length != 0)
				{
					NetworkRoom room2 = base.app.model.network.room;
					if (room2 != null && room2.Local != null)
					{
						if (!(p_data[0] is NetworkRoom.DamageData damageData))
						{
							Debug.LogWarning("NetworkRaceController> Drone damaged but no damage data present!");
							break;
						}
						NetworkActor networkActor = room2.TryGetPlayer(damageData.NetworkID);
						if (networkActor != null && !networkActor.IsSpectator)
						{
							D.Log("NetworkRaceController> Network user [" + networkActor.ProfileName + "] damaged! ");
							Notify("network.drone-damage.update", damageData.NetworkID, damageData, networkActor);
						}
						break;
					}
					break;
				}
				break;
			case "network.race.end":
				ForceCompleteGhostDrones();
				RunOnce(Time.deltaTime * 2f, base.model.RefreshStandings);
				if (base.app.inGame && base.app.arguments.game.tournamentData != null)
				{
					base.app.model.service.WatchTournamentRefresh();
				}
				RunOnce(3f, delegate
				{
					TournamentUpdate();
				});
				base.game.SetGCEnabled(p_flag: true);
				base.game.ui.hud.timeout.StopTimeout();
				if (!base.app.model.network.room.Local.IsSpectator)
				{
					NetworkRoom.GameFinishedData gameFinishedData = Reflection<object>.Get<NetworkRoom.GameFinishedData>(p_data, 0);
					if (gameFinishedData != null)
					{
						if (gameFinishedData.FinishedReason != NetworkRoom.GameFinishedData.Reason.Timeout)
						{
							_ = 1;
							break;
						}
						NetworkActor.RacerState raceState = base.app.model.network.room.Local.RaceState;
						if (raceState == NetworkActor.RacerState.Running || raceState == NetworkActor.RacerState.Timeout)
						{
							base.OnRaceComplete(gameFinishedData.TimeElapsed, RaceStatusType.Timeout);
						}
						break;
					}
					break;
				}
				break;
			case "game.intro.animation@start":
				if (base.app.model.network.room != null && base.app.model.network.room.Local.IsSpectator && base.app.model.network.room.ServerState != NetworkRoom.StateCode.GameLoading)
				{
					RunOnce(6f, delegate
					{
						CompleteIntroAnimation();
						OnCountComplete();
					});
				}
				break;
			case "game.intro.animation@complete":
			{
				if (base.app.model.network.room == null)
				{
					break;
				}
				NetworkActor local3 = base.app.model.network.room.Local;
				RunOnce(0.1f, delegate
				{
					if (local3.IsSpectator)
					{
						network.SetPlayerState(local3, GamePlayerType.Spectator);
					}
				});
				break;
			}
			case "tournament.action.reset-heat":
			case "tournament.action.reset-match":
				if (base.app.arguments.game.tournamentData != null && base.app.model.network.room != null && p_data.Length != 0 && !((string)p_data[0] != base.app.model.network.room.MatchId))
				{
					DRLTournamentData tournamentData = base.app.arguments.game.tournamentData;
					QuitTournamentMatch(tournamentData.GetActiveRound().title, 0f, p_forceQuit: true);
				}
				break;
			case "tournament.action.quit-heat":
				if (base.app.inVirtualSeason && base.app.inTournament && base.app.model.network.room != null)
				{
					NetworkRoom room = base.app.model.network.room;
					if (p_data.Length != 0 && !((string)p_data[0] != room.MatchId))
					{
						RaceForfeit(p_force: true);
					}
				}
				break;
			case "tournament.action.quit-heat-user":
				if (base.app.inVirtualSeason && base.app.inTournament && base.app.model.network.room != null && p_data.Length != 0)
				{
					string text = (string)p_data[0];
					if (!string.IsNullOrEmpty(text) && !(text != base.app.model.storage.state.player.profile.playerId))
					{
						RaceForfeit(p_force: true);
					}
				}
				break;
			case "game.race.request-forfeit":
				RaceForfeit();
				break;
			case "game.simulation.drone@scrape":
			case "game.simulation.drone@crash":
			{
				Drone drone3 = p_data[0] as Drone;
				if (drone3 == null)
				{
					break;
				}
				NetworkRoom room3 = base.app.model.network.room;
				if (room3 == null)
				{
					break;
				}
				NetworkActor local2 = base.app.model.network.room.Local;
				if (local2 != null && room3.State == NetworkRoom.StateCode.GameRunning && !local2.IsSpectator && (local2.RaceState == NetworkActor.RacerState.Running || local2.RaceState == NetworkActor.RacerState.Crash))
				{
					if (p_event == "game.simulation.drone@crash")
					{
						room3.SendPlayerCrashed(base.model.time, drone3.position, drone3.transform.rotation, drone3.rigidbody.rb.velocity, drone3.crashData);
					}
					room3.SendPlayerDamage(drone3.crashData);
				}
				break;
			}
			case "network.race.replay.incoming":
			{
				int num2 = Reflection<object>.Get<int>(p_data, 0);
				byte[] replayData = p_data[1] as byte[];
				GamePlayerData player = base.game.model.GetPlayerDataById(num2);
				if (base.app.model.network.room == null)
				{
					Debug.LogWarning("NetworkRaceController> RaceReplayIncoming / Room doesn't exist in 'app.model.network.room'!");
				}
				else if (base.app.model.network.room.Local == null)
				{
					Debug.LogWarning("NetworkRaceController> RaceReplayIncoming / Local network actor doesn't exist in 'app.model.network.room.Local'!");
				}
				else if (base.app.model.network.room.Local.ID == num2)
				{
					Debug.Log("NetworkRaceController> RaceReplayIncoming / SUCCESS - self[true] name[" + player.upperName + "] player-id[" + player.playerId + "] platform-id[" + player.platformId + "]");
					Notify("network.race.replay.ready", player);
					break;
				}
				if (replayData == null)
				{
					Debug.LogWarning("NetworkRaceController> RaceReplayIncoming / FAIL - self[false] name[" + player.upperName + "] player-id[" + player.playerId + "] platform-id[" + player.platformId + "]");
					Notify("network.race.replay.ready", player);
					GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
					break;
				}
				if (ReplayFile.EnableVersion2)
				{
					base.game.replay.recorder.model.FromBytesAsync(replayData, delegate(ReplayFile p_replay)
					{
						if (!(this == null) && player != null)
						{
							Debug.Log("NetworkRaceController> RaceReplayIncoming / SUCCESS - self[false] name[" + player.upperName + "] player-id[" + player.playerId + "] platform-id[" + player.platformId + "]");
							player.SetReplay(p_replay);
							Notify("network.race.replay.ready", player);
							replayData = null;
						}
					});
					break;
				}
				base.game.replay.recorder.model.FromBytesAsync(replayData, delegate(BlackboxRecord p_replay)
				{
					if (!(this == null) && player != null)
					{
						Debug.Log("NetworkRaceController> RaceReplayIncoming / SUCCESS - self[false] name[" + player.upperName + "] player-id[" + player.playerId + "] platform-id[" + player.platformId + "]");
						player.SetReplay(p_replay, 0);
						Notify("network.race.replay.ready", player);
						base.app?.model?.service?.opponent?.TryAddLoadedReplay(p_replay);
						replayData = null;
						GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
					}
				});
				break;
			}
			case "network.race.replay.ready":
				if (base.app.model.network.room != null && base.app.model.network.room.Racers.TrueForAll((NetworkActor el) => el.IsReplaySent) && !allReplaysProcessed)
				{
					ReplayParseAll(delegate
					{
						allReplaysProcessed = true;
						Notify("network.race.replay.ready.all");
					});
				}
				break;
			}
		}

		protected void ReplayParseAll(Action p_oncomplete)
		{
			if (m_replay_save_loop != null)
			{
				Debug.LogWarning("NetworkRaceController> ReplayParseAll / Already Running, waiting completion!");
				return;
			}
			float replay_save_timeout = 15f;
			m_replay_save_loop = Run((Func<bool>)delegate
			{
				bool flag = false;
				replay_save_timeout -= Time.unscaledDeltaTime;
				if (replay_save_timeout <= 0f)
				{
					Debug.LogWarning("NetworkRaceController> ReplayParseAll / Replay Parse TimeOut!");
					flag = true;
				}
				if (base.game.model.HasAllReplays())
				{
					flag = true;
				}
				if (!flag)
				{
					return true;
				}
				if (!base.validContext)
				{
					return false;
				}
				string text = "NetworkRaceController> ReplayParseAll / Replays Manifest\n";
				for (int i = 0; i < base.game.model.players.Count; i++)
				{
					GamePlayerData gamePlayerData = base.game.model.players[i];
					bool flag2 = (ReplayFile.EnableVersion2 ? (gamePlayerData.replayV2 != null) : (gamePlayerData.replay != null));
					text += $"  [{gamePlayerData.name}] = {flag2}\n";
				}
				Debug.Log(text);
				if (ReplayFile.EnableVersion2)
				{
					ReplayRecord replayRecord = new ReplayRecord();
					List<ReplayFile> replaysV = base.game.model.GetReplaysV2();
					for (int j = 0; j < replaysV.Count; j++)
					{
						replayRecord.replays.Add(replaysV[j]);
					}
					base.game.model.replay.gameReplayV2 = replayRecord;
				}
				else
				{
					BlackboxRecord blackboxRecord = new BlackboxRecord();
					List<BlackboxData> replays = base.game.model.GetReplays();
					for (int k = 0; k < replays.Count; k++)
					{
						blackboxRecord.Add(replays[k]);
					}
					base.game.model.replay.gameReplay = blackboxRecord;
					base.app.model?.service?.opponent?.TryAddLoadedReplay(blackboxRecord);
					string folder = DRLPaths.Storage.replaysRoot;
					string hash = base.app.hash;
					Debug.Log("NetworkRaceController> ReplayParseAll / path[" + folder + hash + "] clip-count[" + replays.Count + "] time-out[" + replay_save_timeout + "s]");
					base.game.replay.recorder.model.ToBytesAsync(blackboxRecord, delegate(byte[] fd)
					{
						Debug.Log($"NetworkRaceController> ReplayParseAll / Parse Complete - size[{fd.Length} bytes]");
						File.WriteAllBytes(folder + hash + ".replay.bytes", fd);
					});
				}
				if (p_oncomplete != null)
				{
					p_oncomplete();
				}
				return false;
			}, 0f, false);
		}

		protected void TournamentUpdate()
		{
			if (base.app.arguments.game.mode != GameFlag.NetworkMultiplayer || base.app.model.network.room == null)
			{
				return;
			}
			NetworkActor local = base.app.model.network.room.Local;
			NetworkRoom room = base.app.model.network.room;
			if (!local.IsMaster || !room.IsTournamentMatch)
			{
				return;
			}
			string t_guid = base.app.model.network.room.TournamentId;
			if (string.IsNullOrEmpty(t_guid))
			{
				Debug.Log("NetworkRaceController> No Tournament available - skip");
				return;
			}
			int heatIdx = base.app.model.network.room.HeatIdx;
			DRLMap map = base.app.arguments.game.map;
			DRLMapTrack track = base.app.arguments.game.track;
			string log = "";
			List<GamePlayerData> rankings = base.model.Rankings;
			List<DRLRaceResultData> results = new List<DRLRaceResultData>();
			for (int i = 0; i < rankings.Count; i++)
			{
				GamePlayerData gamePlayerData = rankings[i];
				string text = (string.IsNullOrEmpty(gamePlayerData.playerId) ? "" : gamePlayerData.playerId);
				int p_score = Mathf.FloorToInt(Mathf.Round(((text == local.PlayerId) ? local.RaceTime : gamePlayerData.raceTime) * 1000f) / 1000f * 1000f);
				int p_crashes = 0;
				DRLRaceResultData dRLRaceResultData = base.app.model.storage.state.player.results.Create(t_guid, map, track, heatIdx, text, p_score, p_crashes, GameFlag.NetworkMultiplayer);
				dRLRaceResultData.status = ResultStatusType.Success;
				dRLRaceResultData.matchId = room.MatchId;
				dRLRaceResultData.heat = room.HeatIdx;
				dRLRaceResultData.raceId = room.RaceId;
				switch (gamePlayerData.raceStatus)
				{
				case RaceStatusType.Forfeit:
					dRLRaceResultData.status = ResultStatusType.Quit;
					break;
				case RaceStatusType.Success:
					dRLRaceResultData.status = ResultStatusType.Success;
					break;
				case RaceStatusType.Timeout:
					dRLRaceResultData.status = ResultStatusType.Timeout;
					break;
				case RaceStatusType.Crash:
					dRLRaceResultData.status = ResultStatusType.Crash;
					break;
				case RaceStatusType.Quit:
					dRLRaceResultData.status = ResultStatusType.Quit;
					break;
				}
				log = log + dRLRaceResultData.ToJson() + "\n";
				results.Add(dRLRaceResultData);
			}
			float fallback_timer = 20f;
			((Component)this).ActivityRun((Predicate<float>)delegate
			{
				fallback_timer -= Time.deltaTime;
				bool flag = room == null || (room.Racers != null && room.Racers.Count > 0 && room.Racers.TrueForAll((NetworkActor o) => o.HasSubmittedLeaderboard));
				if (fallback_timer <= 0f || flag)
				{
					Debug.Log("NetworkRaceController> SetResults - tournament[" + t_guid + "]\n" + log);
					SendTournamentResults(t_guid, results.ToArray());
					return false;
				}
				return true;
			}, 0f);
		}

		protected override void LoadDrones()
		{
			if (base.game == null || base.game.model == null || !base.app.model.network.InRoom)
			{
				return;
			}
			int racersCount = base.app.model.network.room.RacersCount;
			List<GamePlayerData> players = base.game.model.players;
			if (players == null)
			{
				return;
			}
			for (int i = 0; i < players.Count; i++)
			{
				if (players[i] != null && players[i].type == GamePlayerType.Ghost)
				{
					GamePlayerData gamePlayerData = players[i];
					Debug.Log($"[NetworkRaceController]@@ LoadDrones | name: {gamePlayerData.name} order before: {gamePlayerData.order} order after:{racersCount + i}");
					gamePlayerData.order = racersCount + i;
					CreatePlayer(gamePlayerData, base.model.rig);
				}
			}
			ResetGhostDrones();
		}

		protected override void ResetGhostDrones(bool p_write_podium)
		{
			DroneSimulation simulation = base.game.model.simulation;
			simulation.transmitters.ResetGhostDrones();
			simulation.transmitters.SetPhysicsOnComplete(p_flag: true);
			for (int i = 0; i < simulation.drones.list.Count; i++)
			{
				Drone p_drone = simulation.drones.list[i];
				DroneGhostTransmitter byDrone = simulation.transmitters.GetByDrone<DroneGhostTransmitter>(p_drone);
				if ((bool)byDrone && (bool)byDrone.drone)
				{
					byDrone.usePhysics = false;
					GamePlayerData playerData = base.game.model.GetPlayerData(byDrone.drone);
					DronePodium dronePodium = simulation.podiums.Get(playerData.order);
					if ((bool)dronePodium)
					{
						byDrone.podium = dronePodium.spawn.position;
						byDrone.podiumRotation = dronePodium.spawn.rotation;
					}
				}
			}
		}

		protected override bool RequestIntroAnimationSkip()
		{
			return true;
		}

		protected override void OnGameReady()
		{
			base.OnGameReady();
			base.game.model.simulation.Run(p_arm: false);
			if (base.app.model.network.room == null)
			{
				return;
			}
			foreach (KeyValuePair<int, NetworkRacer> sceneRacer in base.app.model.network.room.SceneRacers)
			{
				sceneRacer.Value.interpolate = true;
			}
		}

		protected override bool CanTabScreen()
		{
			NetworkRoom room = base.app.model.network.room;
			if (room == null)
			{
				return false;
			}
			NetworkActor local = room.Local;
			if (local == null)
			{
				return false;
			}
			if (!local.IsSpectator)
			{
				if (base.model.countActive)
				{
					return false;
				}
				if (base.model.Rankings.Count <= 1)
				{
					return false;
				}
			}
			if (room.ServerState != NetworkRoom.StateCode.GameRunning)
			{
				return false;
			}
			return true;
		}

		protected override void StartCount(bool p_fast = false)
		{
			Notify("game.count@start");
		}

		protected override void OnCountComplete()
		{
			base.OnCountComplete();
			if (base.app.model.network.room != null && base.app.model.network.room.IsSpectator)
			{
				base.ui.hud.marker.fade.Kill();
				base.ui.hud.marker.fade.alpha = -0.1f;
				base.ui.hud.race.fade.Kill();
				base.ui.hud.race.fade.alpha = -0.1f;
			}
			base.model.SortRankingsByOrder();
		}

		protected override void SetCount(int p_current, int p_max, bool p_play_audio = true, bool p_hide_title = false)
		{
		}

		protected override void OnGateEvent(ColliderEvent.Type p_type, int p_gate_id, Drone p_drone)
		{
			if ((bool)p_drone)
			{
				GamePlayerData playerData = base.game.model.GetPlayerData(p_drone);
				if (playerData != null && (base.game.model.IsPlayer(p_drone) || playerData.type == GamePlayerType.Ghost))
				{
					base.OnGateEvent(p_type, p_gate_id, p_drone);
				}
			}
		}

		public override float GetDeltaTime()
		{
			float deltaTime = base.GetDeltaTime();
			float num = base.model.time + deltaTime;
			float b = ((base.app.model.network.room != null) ? base.app.model.network.room.ElapsedTime : num);
			num = Mathf.Lerp(num, b, Time.deltaTime * 5f);
			return deltaTime;
		}

		public override float GetGlobalTime()
		{
			NetworkRoom room = base.app.model.network.room;
			if (room == null)
			{
				return base.GetGlobalTime();
			}
			float result = room.ElapsedTime;
			switch (room.State)
			{
			case NetworkRoom.StateCode.GameWarmup:
				result = 0f;
				break;
			case NetworkRoom.StateCode.GameLoading:
				result = 0f;
				break;
			}
			return result;
		}

		public override int GetRacerCount()
		{
			return base.app.model.network.room?.RacersCount ?? base.GetRacerCount();
		}

		protected override void RequestRaceReset()
		{
		}

		private void RaceForfeit(bool p_force = false)
		{
			NetworkRoom room = base.app.model.network.room;
			if (room == null)
			{
				return;
			}
			NetworkActor local = base.app.model.network.room.Local;
			if (local != null && room.State == NetworkRoom.StateCode.GameRunning && !local.IsSpectator && local.RaceState == NetworkActor.RacerState.Running && (!(base.model.time < 8f) || p_force))
			{
				float raceTime = local.RaceTime;
				if (room.IsTournamentMatch)
				{
					raceTime = base.model.time;
				}
				room.SendPlayerForfeit(raceTime);
			}
		}

		protected override void OnRaceComplete(float p_race_time, RaceStatusType p_status)
		{
			base.OnRaceComplete(p_race_time, p_status);
			Drone playerDrone = base.game.model.playerDrone;
			_ = Vector3.zero;
			_ = Quaternion.identity;
			_ = Vector3.zero;
			if (playerDrone != null)
			{
				_ = playerDrone.position;
				_ = playerDrone.transform.rotation;
				_ = playerDrone.rigidbody.rb.velocity;
			}
			if (base.app.model.network.room != null && p_status == RaceStatusType.Success)
			{
				this.TimerRunOnce(delegate
				{
					base.app.model.network.room.SendPlayerCompletedRace(p_race_time);
				}, 2f);
			}
		}

		protected override void OnReplayWrite()
		{
			base.game.replay.recorder.model.ToBytesAsync(delegate(byte[] fd)
			{
				OnReplayWriteData(fd);
			});
		}

		protected override void OnReplayWriteData(byte[] p_replay_data)
		{
			if (this == null || base.app == null || base.app.model == null)
			{
				return;
			}
			if (p_replay_data == null)
			{
				Notify("game.race.replay-storage@complete");
			}
			else
			{
				if (base.app.model.network.room == null)
				{
					return;
				}
				base.app.model.service.StorageTemp(p_replay_data, delegate(string url)
				{
					if (!(this == null) && base.app.model.network.room != null)
					{
						Debug.Log("NetworkRaceController> OnReplayWriteData / " + url);
						base.app.model.network.room.SendReplayData(url);
						Notify("game.race.replay-storage@complete");
					}
				});
			}
		}

		public override bool OnGameCommand(GameCommand p_command)
		{
			if (p_command == null)
			{
				return true;
			}
			switch (p_command.type)
			{
			case GameCommandType.Pause:
			{
				NetworkActor networkActor2 = ((base.app.model.network.room == null) ? null : base.app.model.network.room.Local);
				if (networkActor2 != null && networkActor2.IsSpectator)
				{
					return false;
				}
				break;
			}
			case GameCommandType.TabScreenEnable:
			case GameCommandType.TabScreenDisable:
			{
				NetworkActor networkActor = ((base.app.model.network.room == null) ? null : base.app.model.network.room.Local);
				if (networkActor != null)
				{
					if (CanTabScreen() && networkActor.RaceState == NetworkActor.RacerState.Running)
					{
						base.game.SwitchTabScreen();
					}
					return true;
				}
				break;
			}
			case GameCommandType.ResetDronePodium:
			{
				NetworkRoom room2 = base.app.model.network.room;
				if (room2 != null && room2.ArmAndTurtle)
				{
					Drone playerDrone = base.game.model.playerDrone;
					base.game.DroneArmDisarm(playerDrone);
					return false;
				}
				break;
			}
			case GameCommandType.ResetDrone:
			{
				NetworkRoom room3 = base.app.model.network.room;
				if (room3 != null && room3.ArmAndTurtle)
				{
					Drone playerDrone2 = base.game.model.playerDrone;
					base.game.DroneTurtle(playerDrone2);
					return false;
				}
				break;
			}
			case GameCommandType.GameForfeit:
				if (!base.game.model.paused)
				{
					NetworkRoom room = base.app.model.network.room;
					if (room != null && room.State == NetworkRoom.StateCode.GameRunning)
					{
						Notify("game.race.request-forfeit");
						return false;
					}
				}
				break;
			}
			return base.OnGameCommand(p_command);
		}

		protected override void PlayIntroAnimation()
		{
			int p_defaultPodium = 0;
			NetworkRoom room = base.app.model.network.room;
			if (room != null && !room.IsSpectator)
			{
				p_defaultPodium = base.app.model.network.room.Local.Order;
			}
			PlayPodiumAnimation(p_defaultPodium);
		}

		private void StartCountdownTimer(float p_time)
		{
			if (base.validContext)
			{
				base.game.ui.hud.timeout.StartTimeout(p_time);
			}
		}
	}
}

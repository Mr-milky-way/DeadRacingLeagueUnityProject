using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using drl.network;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIRaceCompleteController : Controller<DRLApp>
	{
		internal bool delayStandingsRefresh;

		internal bool raceComplete;

		internal bool saveComplete;

		internal bool tournamentMatchComplete;

		private Activity update_tune_rating;

		private Activity update_map_rating;

		public GameController game => base.app.controller.game;

		public UIRaceCompleteView view => AssertLocal<UIRaceCompleteView>("view");

		public StorageModel storage => base.app.model.storage;

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (!view.current || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "game.race-complete.share@click":
				break;
			case "ui.screen@open":
			{
				saveComplete = storage.saveComplete;
				bool offline = DRLApp.offline;
				view.SetSpectateEnabled(p_flag: false);
				view.SetExitEnabled(saveComplete || offline);
				view.SetNextEnabled(saveComplete || offline);
				base.app.view.ui.SetDark(p_flag: false);
				RunOnce(1f / 30f, delegate
				{
					base.app.view.ui.footer.Hide(0f);
				});
				DRLMapFavoriteData new_map_data2 = new DRLMapFavoriteData();
				new_map_data2.mapId = base.app.scene.map.guid;
				new_map_data2.customMap = !string.IsNullOrEmpty(base.app.scene.customMap);
				new_map_data2.trackId = ((!new_map_data2.customMap) ? base.app.scene.track.guid : base.app.scene.customMap);
				DRLMapFavoriteData dRLMapFavoriteData2 = base.app.model.storage.state.player.favoriteMaps.Find((DRLMapFavoriteData map) => map.trackId == new_map_data2.trackId && map.mapId == new_map_data2.mapId && map.customMap == new_map_data2.customMap);
				view.toggleView.isOn = dRLMapFavoriteData2 != null;
				base.app.view.ui.game.hud.damage.Show(p_flag: false);
				view.Set(game.model.camera);
				if (tournamentMatchComplete || view.race.tournamentMatchComplete)
				{
					view.tournamentRestartButton.gameObject.SetActive(value: false);
				}
				delayStandingsRefresh = false;
				if (view.showStandings)
				{
					view.standings.Fade(p_flag: true, 0.6f, 0.1f);
					view.showStandings = false;
					delayStandingsRefresh = true;
					thelab.core.Timer.Set(this, "delayStandingsRefresh", 2f, false);
				}
				else
				{
					view.standings.Fade(p_flag: false, 0f, 0f);
				}
				if (offline)
				{
					view.SetProgressionEnabled(p_flag: false);
				}
				else
				{
					view.SetProgressionEnabled(p_flag: true);
					SetLeaderboards(3f);
				}
				RaceController rc = view.race;
				DRLTournamentData tournamentData = base.app.arguments.game.tournamentData;
				if (view.willUpdateCircuits && rc != null && rc.model.Rankings.Count > 0)
				{
					view.willUpdateCircuits = false;
					UpdateCircuitsProgression(rc.model.GetPlayerRankings());
				}
				bool has_opponents = game.model.racerCount > 1;
				bool is_multiplayer = game.model.mode == GameFlag.NetworkMultiplayer;
				bool flag = game.model.type == GameFlag.Campaign;
				bool flag2 = tournamentData != null;
				bool is_complete = !rc || rc.model.IsComplete();
				bool flag3 = IsRestartAvailable();
				bool fromEditor = game.model.fromEditor;
				if (rc != null && !flag2)
				{
					rc.restartLocked = false;
				}
				view.SetRestartEnabled((flag3 && saveComplete) || offline);
				view.SetPromoEnabled(p_flag: false);
				if (flag2 && is_multiplayer)
				{
					VerifyUnderReview();
				}
				if ((bool)rc && has_opponents && !raceComplete)
				{
					if (!view.showStandings)
					{
						view.standings.Refresh(rc.model.Rankings, p_clear: false, p_dnf: false, p_displayDNF: false);
					}
					raceComplete = is_complete;
				}
				view.tournamentRestartButton.gameObject.SetActive(value: false);
				if (flag3 && flag2)
				{
					view.restartButtonNav.gameObject.SetActive(value: false);
					TournamentRoundGameMode activeRoundMode = tournamentData.GetActiveRoundMode();
					if ((uint)(activeRoundMode - 3) <= 1u && tournamentData.GetActiveRoundState() == TournamentRoundState.active)
					{
						view.nextButtonFade.gameObject.SetActive(value: false);
						Activity.RunOnce(delegate
						{
							if (base.validContext && view.current)
							{
								view.tournamentRestartButton.gameObject.SetActive(value: true);
								view.nextButtonFade.gameObject.SetActive(value: true);
								if (rc != null)
								{
									rc.restartLocked = false;
								}
							}
						}, 4f);
					}
					if (!view.spectateButtonNav.gameObject.activeInHierarchy)
					{
						view.tournamentRestartButton.up = view.nextButtonNav;
						view.nextButtonNav.down = view.tournamentRestartButton;
					}
				}
				if ((bool)rc)
				{
					bool flag4 = false;
					if (flag)
					{
						CampaignController campaignController = rc as CampaignController;
						flag4 = flag4 || ((bool)campaignController.model.campaign && campaignController.model.campaign.tournament);
						if (!saveComplete)
						{
							view.SetSaveFeedback(p_flag: true);
						}
						this.TimerRunOnce(delegate
						{
							saveComplete = true;
							view.saveComplete = true;
							if (base.validContext)
							{
								view.SetSaveFeedback(p_flag: false);
								view.SetNextEnabled(p_flag: true, 0.3f);
							}
						}, 100f);
					}
					flag4 = flag4 || base.app.arguments.game.tournamentPromo || base.app.arguments.game.promo;
					if (fromEditor || base.app.inCircuits)
					{
						view.SetExitEnabled(p_flag: true);
					}
					float p_delay = 10f;
					if (!base.app.inTournament || !base.app.inVirtualSeason)
					{
						this.TimerRunOnce(delegate
						{
							if (base.validContext && (!view.exitButtonNav.gameObject.activeInHierarchy || !view.nextButtonNav.gameObject.activeInHierarchy || !view.nextButton.interactable))
							{
								view.SetExitEnabled(p_flag: true);
								this.TimerRunOnce(delegate
								{
									if (base.validContext && !view.nextButton.gameObject.activeInHierarchy && !view.nextButton.interactable)
									{
										if (!is_multiplayer || (rc != null && is_complete))
										{
											view.SetNextEnabled(p_flag: true);
											view.nextButton.interactable = true;
										}
										view.SetExitEnabled(!base.app.inTournament || !base.app.inVirtualSeason);
										UINavigation.Focus(view.nextButton);
									}
								}, 2.5f);
							}
						}, p_delay);
					}
					view.SetPromoEnabled(flag4);
					if (is_multiplayer)
					{
						NetworkRoom room = base.app.model.network.room;
						List<string> list = new List<string>();
						bool flag5 = false;
						if (room != null && room.VoteTrackList != null && room.VoteTrackList.Count > 0)
						{
							list.Clear();
							list.AddRange(room.VoteTrackList);
							flag5 = room.AllowMapVoting && !room.IsTournamentMatch && !room.MapRandom;
						}
						if (flag5 && list.Count > 0 && !view.votes.HasCards)
						{
							view.votes.gameObject.SetActive(value: true);
							view.votes.Initialize(list);
							LayoutGroup component = view.votes.cardList.GetComponent<LayoutGroup>();
							Component right = view.exitButtonNav.right;
							Component left = view.nextButtonNav.left;
							UINavigation.Link(component, view.exitButtonNav, view.nextButtonNav, view.nextButtonNav);
							view.exitButtonNav.right = right;
							view.nextButtonNav.left = left;
							view.spectateButtonNav.left = view.nextButtonNav.left;
						}
						if (flag5)
						{
							RefreshVoteRoomState();
							view.votes.Refresh(room.VoteTrackTable);
						}
					}
					GetDroneRating();
					GetMapRating();
					this.TimerRunOnce(delegate
					{
						UINavigation.Focus(view.rightColumn);
					}, 0.5f);
					if (is_multiplayer)
					{
						RunOnce(3f, delegate
						{
							view.SetSpectateEnabled(!is_complete && has_opponents);
						});
					}
					if (base.app.inTournament && base.app.inVirtualSeason)
					{
						view.SetExitEnabled(p_flag: false);
						view.SetNextEnabled(p_flag: false);
						NetworkRaceController networkRaceController = ((view.race != null) ? (view.race as NetworkRaceController) : null);
						if ((bool)networkRaceController)
						{
							view.SetExitEnabled(networkRaceController.allReplaysProcessed);
							view.SetNextEnabled(networkRaceController.allReplaysProcessed);
						}
					}
				}
				if (!is_multiplayer && view.race != null && view.race.model.status != RaceStatusType.Success)
				{
					view.SetExitEnabled(p_flag: true);
					view.SetNextEnabled(p_flag: true);
					view.SetRestartEnabled(p_flag: true);
				}
				break;
			}
			case "game.standings@update":
			{
				DRLStandingsView sv = view.standings;
				RaceController rc2 = view.race;
				if (rc2 == null || !base.validContext || !view.current)
				{
					break;
				}
				int ranking_count = rc2.model.GetRacerRankingCount();
				if (ranking_count <= 0)
				{
					break;
				}
				float p_delay2 = (delayStandingsRefresh ? 1f : 0f);
				RunOnce(p_delay2, delegate
				{
					if ((bool)rc2 && base.validContext && view.current)
					{
						if (ranking_count > 1)
						{
							sv.Refresh(rc2.model.Rankings, p_clear: false, p_dnf: false, p_displayDNF: false);
						}
						rc2.model.IsComplete();
						bool flag8 = IsRestartAvailable();
						view.SetRestartEnabled(flag8 && storage.saveComplete);
					}
				});
				break;
			}
			case "game.race-complete.exit@click":
			{
				bool flag6 = false;
				flag6 = base.app.arguments.game.tournamentData != null;
				base.app.arguments.game.tryouts = false;
				if (base.app.inGame)
				{
					base.app.controller.game.ui.hud.timeout.StopTimeout();
				}
				if (base.app.inOnboarding)
				{
					Notify("onboarding.stop");
				}
				else if (game.model.fromEditor)
				{
					base.enabled = false;
					base.app.view.audio.PlayUIGenericSuccess();
					game.BackToEditor();
				}
				else if (flag6)
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
				if (game.model.mode != GameFlag.NetworkMultiplayer && !view.race.tournamentMatchComplete && (!base.app.inCircuits || IsRestartAvailable()))
				{
					base.enabled = false;
					if (base.validContext)
					{
						base.app.view.audio.PlayUIGenericSuccess();
						base.app.view.audio.StopUICounterLoop();
					}
					game.Restart();
				}
				break;
			case "game.race-complete.restart@click":
				if (view.race.tournamentMatchComplete || (base.app.inCircuits && !IsRestartAvailable()))
				{
					break;
				}
				base.enabled = false;
				if (base.validContext)
				{
					base.app.view.audio.PlayUIGenericSuccess();
					base.app.view.audio.StopUICounterLoop();
				}
				if (game.model.mode == GameFlag.NetworkMultiplayer)
				{
					NetworkModel network = base.app.model.network;
					if ((bool)network)
					{
						network.photon.ForceStartMatch();
					}
				}
				else
				{
					game.Restart();
				}
				break;
			case "game.race-complete.settings@click":
				base.app.view.ui.screens.Open("settings-game-screen");
				break;
			case "game.race.replay-upload@start":
				Debug.Log("UIRaceCompleteController> <color=#ff0>Local Player Replays UPLOAD START!</color> " + DateTime.Now.ToString());
				view.replayUploadStarted = true;
				break;
			case "game.race.replay-storage@complete":
				Debug.Log("UIRaceCompleteController> <color=#ff0>Replay storage temp complete!</color> " + DateTime.Now.ToString());
				view.saveComplete = true;
				storage.saveComplete = true;
				view.SetExitEnabled(!base.app.inTournament || !base.app.inVirtualSeason);
				view.SetRestartEnabled(IsRestartAvailable());
				view.SetNextEnabled(view.race != null && view.race.model.IsComplete());
				break;
			case "game.race.replay-upload@complete":
				Debug.Log("UIRaceCompleteController> <color=#ff0>Local Player Replays UPLOAD COMPLETE!</color> " + DateTime.Now.ToString());
				_ = game.model.mode;
				view.saveComplete = true;
				storage.saveComplete = true;
				view.SetExitEnabled(!base.app.inTournament || !base.app.inVirtualSeason);
				view.SetRestartEnabled(IsRestartAvailable());
				view.SetNextEnabled(view.race != null && view.race.model.IsComplete());
				break;
			case "game.race-complete.next@down":
				Notify("game.race-complete.next@click");
				break;
			case "game.race-complete.next@click":
				OnRaceNextClick();
				break;
			case "network.room.update":
				RefreshVoteRoomState();
				break;
			case "network.player@update":
				RefreshVoteRoomState();
				break;
			case "campaign.result.replay@complete":
				saveComplete = true;
				storage.saveComplete = true;
				view.saveComplete = true;
				view.SetSaveFeedback(p_flag: false);
				view.SetNextEnabled(p_flag: true, 0.3f);
				view.SetRestartEnabled(IsRestartAvailable());
				view.SetExitEnabled(!base.app.inTournament || !base.app.inVirtualSeason);
				break;
			case "tournament.match.complete":
				view.tournamentRestartButton.gameObject.SetActive(value: false);
				tournamentMatchComplete = true;
				view.SetSpectateEnabled(p_flag: false);
				if (storage.saveComplete)
				{
					view.SetNextEnabled(p_flag: true);
					view.SetRestartEnabled(IsRestartAvailable());
					view.SetExitEnabled(!base.app.inTournament || !base.app.inVirtualSeason);
				}
				break;
			case "tournament.action.refresh":
			{
				NetworkRoom room2 = base.app.model.network.room;
				if (room2 == null)
				{
					break;
				}
				string matchId = room2.MatchId;
				if (!string.IsNullOrEmpty(room2.MatchId))
				{
					UISecondaryHeaderView headerSecondary = base.app.view.ui.headerSecondary;
					DRLTournamentMatchData matchById = base.app.model.tournament.GetMatchById(matchId);
					bool flag7 = matchById?.isUnderReview ?? false;
					if (headerSecondary != null)
					{
						headerSecondary.Refresh(view, flag7);
					}
					Debug.Log($"UIRaceCompleteController>@@ TournamentRefreshState match_data == null:{matchById == null} is_under_review: {flag7}");
				}
				break;
			}
			case "game.race-complete.spectate@click":
			{
				UISpectateView uISpectateView = base.app.view.ui.screens.Open<UISpectateView>("game-spectate-screen");
				uISpectateView.GetComponent<UISpectateController>().Initialize();
				uISpectateView.tournamentContext = base.app.inTournament;
				break;
			}
			case "game.race-complete.map-rating@click":
				SetMapRating(view.mapRating.index + 1, p_update: true);
				break;
			case "game.race-complete.drone-rating@click":
				SetDroneRating(view.droneRating.index + 1, p_update: true);
				break;
			case "network.race.replay.ready.all":
				view.SetExitEnabled(p_flag: true);
				view.SetNextEnabled(p_flag: true);
				break;
			case "network.race.end":
				view.SetSpectateEnabled(p_flag: false);
				if (storage.saveComplete)
				{
					view.SetExitEnabled(p_flag: true);
					view.SetRestartEnabled(IsRestartAvailable());
					view.SetNextEnabled(p_flag: true);
				}
				this.TimerRunOnce(delegate
				{
					view.SetNextEnabled(p_flag: true);
					view.SetExitEnabled(p_flag: true);
					view.SetRestartEnabled(IsRestartAvailable());
				}, 5f);
				break;
			case "game.race-overview.favorite@click":
			{
				DRLToggleView dRLToggleView = p_target as DRLToggleView;
				if (dRLToggleView == null)
				{
					Debug.Log("UIRaceOverviewController> OnNotification / Can't cast p_target o Transform " + p_target.name);
					break;
				}
				DRLMapFavoriteData new_map_data = new DRLMapFavoriteData();
				new_map_data.mapId = base.app.scene.map.guid;
				new_map_data.customMap = !string.IsNullOrEmpty(base.app.scene.customMap);
				new_map_data.trackId = ((!new_map_data.customMap) ? base.app.scene.track.guid : base.app.scene.customMap);
				string track_id = new_map_data.mapId;
				List<DRLMapFavoriteData> favoriteMaps = base.app.model.storage.state.player.favoriteMaps;
				DRLMapFavoriteData dRLMapFavoriteData = favoriteMaps.Find((DRLMapFavoriteData map) => map.trackId == new_map_data.trackId && map.mapId == new_map_data.mapId && map.customMap == new_map_data.customMap);
				if (dRLToggleView.toggle.isOn)
				{
					if (dRLMapFavoriteData != null)
					{
						break;
					}
					favoriteMaps.Add(new_map_data);
					Debug.Log("UIRaceOverviewController> OnNotification / Adding " + new_map_data.mapId + " " + new_map_data.trackId);
					if (!DRLApp.offline)
					{
						DateTime t0 = DateTime.UtcNow;
						base.app.model.service.GetCommunityMap(track_id, delegate(DRLCommunityMapResult p_result)
						{
							Debug.Log("UIRaceOverviewController> Finished downloading community map - " + (DateTime.UtcNow - t0).TotalSeconds);
							DRLCommunityMapData d = ((p_result.data.Length == 0) ? null : p_result.data[0]);
							if (d == null || !base.validContext)
							{
								Debug.LogWarning("UIRaceOverviewController> Store favorite community map / Failed to Load DRLCommunityMapData - guid[" + track_id + "]");
							}
							else
							{
								new Thread((ThreadStart)delegate
								{
									MapData md = d.Convert<MapData>();
									if (md != null)
									{
										md.LoadRoot(d.root);
									}
									this.TimerRunOnce(delegate
									{
										if (md == null)
										{
											Debug.LogWarning("UIRaceOverviewController> Store favorite community map / Failed to Parse MapData - guid[" + track_id + "]");
										}
										else
										{
											base.app.model.storage.maps.SaveCommunityMap(md, delegate
											{
												Debug.Log("UIRaceOverviewController> Succesfully stored community map - guid[" + track_id + "]");
											});
										}
									}, 1f / 60f);
								}).Start();
							}
						});
					}
				}
				else if (dRLMapFavoriteData != null)
				{
					favoriteMaps.Remove(dRLMapFavoriteData);
					Debug.Log("UIRaceOverviewController> OnNotification / Removing " + dRLMapFavoriteData.mapId + " " + dRLMapFavoriteData.trackId);
					base.app.model.storage.maps.DeleteLocalCommunityMap(track_id);
				}
				base.app.model.storage.state.player.favoriteMaps = favoriteMaps;
				break;
			}
			}
		}

		private void VerifyUnderReview()
		{
			NetworkRoom room = base.app.model.network.room;
			if (room == null)
			{
				return;
			}
			string matchId = room.MatchId;
			if (string.IsNullOrEmpty(matchId))
			{
				return;
			}
			base.app.model.tournament.RefreshMatchData(matchId, delegate(DRLTournamentMatchData p_data)
			{
				if (base.validContext && p_data != null)
				{
					UISecondaryHeaderView headerSecondary = base.app.view.ui.headerSecondary;
					bool isUnderReview = p_data.isUnderReview;
					if (headerSecondary != null)
					{
						headerSecondary.Refresh(view, isUnderReview);
					}
				}
			});
		}

		private void OnRaceNextClick()
		{
			if (!view.nextEnabled)
			{
				base.app.view.audio.PlayUIGenericError();
				return;
			}
			if (base.app.arguments.game.tournamentData != null)
			{
				if (base.app.level.IsLevelLoaded("game") && base.app.arguments.game.mode == GameFlag.SinglePlayer)
				{
					base.app.view.ui.screens.Open<UITournamentLeaderboardsView>("tournament-leaderboards-screen").openedFromTheBrackets = false;
				}
				else
				{
					base.app.view.ui.screens.Open<UITournamentRaceCompleteView>("tournament-race-complete-screen").race = view.race;
				}
				return;
			}
			UIRaceOverviewView uIRaceOverviewView = base.app.view.ui.screens.Open<UIRaceOverviewView>("game-race-overview-screen");
			uIRaceOverviewView.race = view.race;
			uIRaceOverviewView.title.text = view.race.GetRaceTitle().ToUpper();
			uIRaceOverviewView.exitEnabled = view.exitButtonNav.gameObject.activeInHierarchy;
			uIRaceOverviewView.savingComplete = view.saveComplete;
			uIRaceOverviewView.restartButton.gameObject.SetActive(view.saveComplete);
			uIRaceOverviewView.Clear();
			uIRaceOverviewView.LoadRaceData();
			uIRaceOverviewView.SetTitle();
		}

		private bool IsRestartAvailable()
		{
			RaceController race = view.race;
			bool flag = game.model.mode == GameFlag.NetworkMultiplayer;
			bool tryouts = base.app.arguments.game.tryouts;
			if (base.app.inCircuits)
			{
				if (!(race == null))
				{
					return race.model.status != RaceStatusType.Success;
				}
				return true;
			}
			if (flag)
			{
				return false;
			}
			if (tryouts)
			{
				return false;
			}
			return game.model.type != GameFlag.Campaign;
		}

		protected void RefreshVoteRoomState()
		{
			NetworkModel network = base.app.model.network;
			if (!network)
			{
				return;
			}
			NetworkRoom room = network.room;
			if (room == null)
			{
				return;
			}
			NetworkRoom.StateCode state = room.State;
			if ((uint)(state - 1) <= 1u)
			{
				if (!room.IsQuickMatch && !room.IsTournamentMatch)
				{
					List<NetworkActor> list = room.Racers.FindAll((NetworkActor el) => el.IsRoomReady);
					view.votes.caption = $"{list.Count} OUT OF {room.RacersCount} PILOTS READY";
				}
				else
				{
					view.votes.caption = "RACE IN " + room.LobbyCountdown.ToString("00");
				}
			}
			else
			{
				view.votes.caption = "WAIT...";
			}
		}

		protected void SetLeaderboards(float p_delay = 0f)
		{
			if (!view.race || game.model.playerData == null)
			{
				return;
			}
			DroneRigData rd = game.model.playerData.rig;
			if (rd == null || game.model.playerDrone == null)
			{
				return;
			}
			rd.tune = ((!game.model.playerDrone.IsCurrentPhysicsDefault) ? game.model.playerDrone.physics.ToJson() : null);
			rd.profile = ((game.model.playerDrone.profile != null) ? game.model.playerDrone.profile.ToJson() : null);
			Debug.Log("UIRaceCompleteController> SetLeaderboards");
			RunOnce(p_delay, delegate
			{
				if ((bool)this && (bool)view && (bool)view.race && view.willSetLeaderboard)
				{
					view.gameMode.SetLeaderboard(OnSetLeaderboard, rd);
					view.SetExitEnabled((!base.app.inTournament || !base.app.inVirtualSeason) && storage.saveComplete);
				}
			});
		}

		protected void OnSetLeaderboard(DRLLeaderboardData p_result)
		{
			SetTournamentResults();
			if (this == null || view == null || DRLApp.offline || !view.willSetLeaderboard)
			{
				return;
			}
			if (p_result == null)
			{
				Debug.LogWarning("UIRaceCompleteController> OnSetLeaderboard - Failed to send leaderboards!");
				SetLeaderboards();
				return;
			}
			base.app.model.network.room?.SendPlayerSubmittedLeaderboard();
			if (!string.IsNullOrEmpty(p_result.group))
			{
				Debug.Log("UIRaceCompleteController> OnSetLeaderboard - CAMPAIGN Success - highscore[" + p_result.highscore + "] position[" + p_result.position + "]");
				return;
			}
			Debug.Log("UIRaceCompleteController> OnSetLeaderboard - RACE Success - highscore[" + p_result.highscore + "] position[" + p_result.position + "]");
			Notify("game.race.leaderboard-complete", p_result);
			view.SetRaceAnalytics();
			view.FadeInAnalytics(0.1f);
			if (!(base.app == null) && !(base.app.view == null) && !(base.app.view.audio == null))
			{
				OnSetProgression(p_result.progression);
				if (p_result.highscore)
				{
					base.app.view.audio.PlayUINewRecord();
					base.app.view.audio.UpdateMusicPostGameResult("new_record");
				}
				if (p_result.highscore)
				{
					view.FadeLeaderboard(p_result.position, GetLeaderboardName(p_result), 0.5f);
				}
				view.willSetLeaderboard = false;
			}
		}

		protected void OnSetProgression(DRLProgressionStateData p_next_progression)
		{
			if (p_next_progression == null)
			{
				Debug.LogWarning("UIRaceCompleteController> OnSetProgression / Next Progression is <null>");
				return;
			}
			DRLProgressionStateData dRLProgressionStateData = new DRLProgressionStateData();
			dRLProgressionStateData.Merge(base.app.model.storage.state.player.progression.state);
			DRLProgressionStateData next_progression = p_next_progression;
			Debug.Log("UIRaceCompleteController> Starting Progression Steps\nFrom: " + dRLProgressionStateData.ToJson() + "\nTo: " + next_progression.ToJson());
			base.app.model.storage.state.player.progression.Refresh();
			view.SetProgression(dRLProgressionStateData);
			bool has_progression = next_progression != null;
			bool is_level_up = has_progression && next_progression.level > dRLProgressionStateData.level;
			if (has_progression)
			{
				this.TimerRunOnce(delegate
				{
					if (base.validContext && !(base.app.view.audio == null))
					{
						if (is_level_up)
						{
							base.app.view.audio.PlayBigStepComplete();
						}
						else
						{
							base.app.view.audio.PlayUIGenericSuccess();
						}
					}
				}, 2.3f);
			}
			Activity.RunOnce(delegate
			{
				Debug.Log($"UIRaceCompleteController> Progression Start / state[{has_progression}]");
				if (has_progression && base.validContext)
				{
					view.SetProgressionNext(next_progression, 2f);
				}
			}, 2.5f);
			base.app.model.storage.state.player.progression.Refresh();
		}

		private void UpdateCircuitsProgression(GamePlayerData p_data)
		{
			if (!base.validContext)
			{
				return;
			}
			CircuitStateModel circuits = base.app.model.storage.state.player.circuits;
			if (p_data == null || !circuits.inProgress || circuits.activeCircuit == null)
			{
				return;
			}
			CircuitStateModel.CircuitsProgressData circuitsProgressData = circuits.GetCircuitProgress();
			if (circuitsProgressData == null)
			{
				circuitsProgressData = new CircuitStateModel.CircuitsProgressData();
			}
			circuitsProgressData.circuitId = circuits.activeCircuit.guid;
			circuitsProgressData.circuitName = circuits.activeCircuit.name;
			circuitsProgressData.progress++;
			List<float> times = circuitsProgressData.times;
			if (circuitsProgressData.times.Count <= circuits.circuitTrackIndex)
			{
				times.Add(p_data.raceTime);
			}
			else if (circuitsProgressData.times[circuits.circuitTrackIndex] > p_data.raceTime)
			{
				times[circuits.circuitTrackIndex] = p_data.raceTime;
			}
			circuitsProgressData.times = times;
			circuitsProgressData.drlOfficial = base.app.model.storage.state.player.garage.IsOfficial(p_data.drone.rig);
			circuits.SetCircuitProgress(circuitsProgressData);
			circuitsProgressData = circuits.GetCircuitProgress();
			if (!circuitsProgressData.finished)
			{
				return;
			}
			PlatformStateType platformStateType = PlatformStateType.Steam;
			DRLCircuitLeaderboardData d = new DRLCircuitLeaderboardData();
			d.circuitId = circuitsProgressData.circuitId;
			d.playerId = p_data.playerId;
			d.platform = platformStateType.ToString();
			d.diameter = p_data.drone.rig.diameter;
			d.droneName = p_data.drone.rig.name;
			d.droneThumb = p_data.drone.rig.thumb0;
			d.controllerType = RCI.GetControllerStateType(ControllerStateType.XBox).ToString();
			d.score = (int)(circuitsProgressData.time * 1000f);
			d.trackTimes = circuitsProgressData.timesData;
			d.drlOfficial = circuitsProgressData.drlOfficial;
			d.droneGuid = p_data.drone.rig.guid;
			d.droneRig = p_data.droneRigData;
			d.hash = game.model.playerDroneHash;
			d.customPhysics = p_data.drone.rig.hasCustomPhysics;
			Debug.Log("UIRaceCompleteController> UpdateCircuitsProgression: SetLeaderboardCircuit");
			if (!base.validContext || !(base.app.model.service != null))
			{
				return;
			}
			base.app.model.service.SetLeaderboardCircuit(d, delegate(bool success)
			{
				if (!base.validContext || !success)
				{
					view.FadeLeaderboardCircuit(-1, "00:00", 0f);
				}
				else
				{
					base.app.model.service.GetCircuitLeaderboardUser(d.circuitId, 1, d.diameter, d.drlOfficial, d.customPhysics ? 1 : 0, delegate(DRLLeaderboardResult p_result)
					{
						if (base.validContext && p_result != null && p_result.leaderboard != null && p_result.leaderboard.Length != 0)
						{
							int p_position = -1;
							for (int i = 0; i < p_result.leaderboard.Length; i++)
							{
								if (!(d.playerId != p_result.leaderboard[i].playerId))
								{
									p_position = p_result.leaderboard[i].position;
									break;
								}
							}
							view.FadeLeaderboardCircuit(p_position, Format.MsToTime(d.score, "mm\\:ss"), 0.5f);
						}
					});
				}
			});
		}

		protected string GetLeaderboardName(DRLLeaderboardData p_data)
		{
			if (p_data.drlOfficial)
			{
				return base.app.model.storage.locale.Get("leaderboard.result.drl-leaderboard", "DRL LEADERBOARD");
			}
			if (p_data.customPhysics)
			{
				return p_data.diameter + "\" " + base.app.model.storage.locale.Get("leaderboard.result.custom-physics", "CUSTOM PHYSICS");
			}
			return p_data.diameter + "\" " + base.app.model.storage.locale.Get("leaderboard.result.leaderboard", "LEADERBOARD");
		}

		protected void SetTournamentResults()
		{
			if (!DRLApp.offline)
			{
				view.race.TournamentUpdate();
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
			if (map == null || track == null || base.app.model.storage == null || base.app.model.storage.state.player == null || view.race.model.Rankings.Count == 0 || droneRigData == null)
			{
				return;
			}
			if (base.app.arguments.game.tournamentData != null && (bool)view)
			{
				string id = base.app.arguments.game.tournamentData.GetActiveRound().GetPlayerMatch(view.race.model.Rankings[0].playerId).Id;
				base.app.model.service.GetLeaderboardRivals(map, track, 3, droneRigData.diameter, base.app.model.storage.state.player.garage.IsOfficial(droneRigData), droneRigData.hasCustomPhysics, id, delegate(DRLLeaderboardRivalsResult p_result)
				{
					if ((bool)this && (bool)view)
					{
						if (p_result == null)
						{
							Debug.LogWarning("UIRaceCompleteController> GetLeaderboardRivals - Failed!");
						}
						else
						{
							DRLLeaderboardData top = p_result.GetTop(0);
							view.playerTime = top.score;
							view.playerCrash = top.crashCount;
							view.playerTopSpeed = top.topSpeed;
						}
					}
				});
				return;
			}
			base.app.model.service.GetLeaderboardRivals(map, track, 3, droneRigData.diameter, base.app.model.storage.state.player.garage.IsOfficial(droneRigData), droneRigData.hasCustomPhysics, delegate(DRLLeaderboardRivalsResult p_result)
			{
				if ((bool)this && (bool)view)
				{
					if (p_result == null)
					{
						Debug.LogWarning("UIRaceCompleteController> GetLeaderboardRivals - Failed!");
					}
					else
					{
						DRLLeaderboardData top = p_result.GetTop(0);
						if ((bool)view && top != null)
						{
							view.playerTime = top.score;
							view.playerCrash = top.crashCount;
							view.playerTopSpeed = top.topSpeed;
							Debug.Log($"<color=green>RACE COMPLETE SCREEN SAYS</color> SCORE:{top.score}, TOPSPEED:{top.topSpeed} ");
						}
					}
				}
			});
		}

		public void GetDroneRating()
		{
			view.droneRatingCard.gameObject.SetActive(value: false);
			view.ClearDroneRating(0.0001f);
			Drone playerDrone = game.model.playerDrone;
			if (!(playerDrone != null))
			{
				return;
			}
			string guid = playerDrone.rig.guid;
			ServiceModel service = base.app.model.service;
			if (!(service != null))
			{
				return;
			}
			string steamId = base.app.model.storage.state.player.playerData.playerId;
			service.GetCommunityDrone(guid, delegate(DRLCommunityDroneData p_drone)
			{
				if (!(this == null) && !(base.gameObject == null) && !(view == null))
				{
					if (p_drone != null)
					{
						Debug.Log("UIPauseController> GET drone rating = " + (int)p_drone.rating * view.droneRating.max);
					}
					if (p_drone != null && p_drone.rating > -1f && !(p_drone.playerId.ToString() == steamId))
					{
						view.droneRatingCard.gameObject.SetActive(value: true);
						view.FadeInDroneRating(0.3f, (int)(p_drone.rating * (float)view.droneRating.max));
					}
				}
			});
		}

		public void GetMapRating()
		{
			if (!base.validContext)
			{
				return;
			}
			view.mapRatingCard.gameObject.SetActive(value: false);
			view.ClearMapRating(0.0001f);
			MapData scene_md = base.app.scene.map.data;
			if (scene_md == null)
			{
				return;
			}
			bool num = scene_md.mapCategoryFlag == GameFlag.MapCommon;
			bool flag = scene_md.playerId.ToString() == base.app.model.storage.state.player.profile.playerId;
			if (!num || flag)
			{
				return;
			}
			view.mapRatingCard.gameObject.SetActive(value: true);
			UINavigation component = view.mapRatingCard.GetComponent<UINavigation>();
			component.down = view.nextButtonNav;
			view.nextButtonNav.up = component;
			if (view.restartButtonFade.gameObject.activeInHierarchy)
			{
				view.restartButtonNav.up = component;
			}
			else
			{
				view.exitButtonNav.up = component;
			}
			string guid = scene_md.guid;
			ServiceModel service = base.app.model.service;
			string sid = base.app.model.storage.state.player.playerData.playerId;
			service.GetCommunityMapRating(guid, delegate(float p_rating)
			{
				if (base.validContext && scene_md != null && !(p_rating <= -1f) && !(scene_md.playerId.ToString() == sid))
				{
					view.FadeInMapRating(0.3f, (int)(p_rating * (float)view.mapRating.max));
				}
			});
		}

		public void SetDroneRating(int p_rating, bool p_update)
		{
			Drone playerDrone = game.model.playerDrone;
			if (playerDrone == null)
			{
				return;
			}
			view.droneRating.index = p_rating % (view.droneRating.max + 1);
			for (int i = 0; i < view.droneRatingStarFades.Length; i++)
			{
				view.droneRatingStarFades[i].alpha = 0.1f;
			}
			for (int j = 0; j < view.droneRating.index; j++)
			{
				view.droneRatingStarFades[j].alpha = 1f;
			}
			if (!p_update)
			{
				return;
			}
			float rating = view.droneRating.index;
			float nrating = ((view.droneRating.max <= 0) ? 1f : Mathf.Clamp01(rating / (float)view.droneRating.max));
			ServiceModel sm = base.app.model.service;
			if (!(playerDrone != null))
			{
				return;
			}
			if (update_tune_rating != null)
			{
				update_tune_rating.Stop();
			}
			string guid = playerDrone.rig.guid;
			update_tune_rating = Activity.RunOnce(delegate
			{
				Debug.Log("UIPauseController> SET drone rating = score[" + nrating + "] rating[" + rating + "]");
				if (sm != null)
				{
					sm.SetCommunityDroneRating(guid, nrating, null);
				}
			}, 2f);
		}

		public void SetMapRating(int p_rating, bool p_update)
		{
			MapData data = base.app.scene.map.data;
			if (data == null)
			{
				return;
			}
			view.mapRating.index = p_rating % (view.mapRating.max + 1);
			for (int i = 0; i < view.mapRatingStarFades.Length; i++)
			{
				view.mapRatingStarFades[i].alpha = 0.1f;
			}
			for (int j = 0; j < view.mapRating.index; j++)
			{
				view.mapRatingStarFades[j].alpha = 1f;
			}
			if (!p_update)
			{
				return;
			}
			float rating = view.mapRating.index;
			float nrating = ((view.mapRating.max <= 0) ? 1f : Mathf.Clamp01(rating / (float)view.mapRating.max));
			ServiceModel sm = base.app.model.service;
			string guid = data.guid;
			if (update_map_rating != null)
			{
				update_map_rating.Stop();
			}
			update_map_rating = Activity.RunOnce(delegate
			{
				if (base.validContext)
				{
					Debug.Log("UIPauseController> SetMapRating / score[" + nrating + "] rating[" + rating + "]");
					sm.SetCommunityMapRating(guid, nrating, null);
				}
			}, 2f);
		}
	}
}

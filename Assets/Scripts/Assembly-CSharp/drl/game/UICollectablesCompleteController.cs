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
	public class UICollectablesCompleteController : Controller<DRLApp>
	{
		internal bool delayStandingsRefresh;

		internal bool raceComplete;

		internal bool saveComplete;

		internal bool tournamentMatchComplete;

		private DRLLeaderboardData[] leaderboard;

		private DRLLeaderboardData playerLeaderData;

		private Activity update_tune_rating;

		private Activity update_map_rating;

		public GameController game => base.app.controller.game;

		public UICollectablesCompleteView view => AssertLocal<UICollectablesCompleteView>("view");

		protected void OnLeaderboardLoaded(DRLLeaderboardResult p_result)
		{
			leaderboard = p_result.leaderboard;
			for (int i = 0; i < leaderboard.Length; i++)
			{
				if (base.app.model.storage.state.player.playerData.playerId == leaderboard[i].playerId)
				{
					playerLeaderData = leaderboard[i];
					break;
				}
			}
		}

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
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
				playerLeaderData = null;
				base.app.view.ui.SetDark(p_flag: false);
				RunOnce(1f / 30f, delegate
				{
					base.app.view.ui.footer.Hide(0f);
				});
				view.Set(game.model.camera);
				if ((bool)view.race && (tournamentMatchComplete || view.race.tournamentMatchComplete))
				{
					view.tournamentRestartButton.gameObject.SetActive(value: false);
				}
				delayStandingsRefresh = false;
				if (view.showStandings)
				{
					view.standings.Fade(p_flag: true, 0.6f, 0.1f);
					view.showStandings = false;
					delayStandingsRefresh = true;
					Timer.Set(this, "delayStandingsRefresh", 2f, false);
				}
				DroneRigData rig = game.model.playerData.rig;
				bool value = base.app.model.storage.state.player.garage.IsOfficial(rig);
				new DRLLeaderboardData();
				int p_physics = (rig.hasCustomPhysics ? 1 : 0);
				base.app.model.service.GetLeaderboard(base.app.scene.map.data.guid, 0, -1, rig.diameter, value, p_physics, delegate(DRLLeaderboardResult p_result)
				{
					OnLeaderboardLoaded(p_result);
				}, null, null, p_group: false, null, -1, GameFlag.Collectable, p_collectable: true);
				SetLeaderboards(3f);
				RaceController race = view.race;
				DRLTournamentData tournamentData = base.app.arguments.game.tournamentData;
				bool flag = game.model.racerCount > 1;
				bool flag2 = game.model.mode == GameFlag.NetworkMultiplayer;
				bool flag3 = game.model.type == GameFlag.Campaign;
				bool flag4 = tournamentData != null;
				bool is_complete = !race || race.model.IsComplete();
				bool can_reset = IsRestartAvailable();
				bool fromEditor = game.model.fromEditor;
				Activity.RunOnce(delegate
				{
					view.SetRestartEnabled(can_reset);
					view.SetNextEnabled(is_complete);
					view.SetPromoEnabled(p_flag: false);
				}, 2f);
				if ((bool)race && flag && !raceComplete)
				{
					if (!view.showStandings)
					{
						view.standings.Refresh(race.model.Rankings, p_clear: false);
					}
					raceComplete = is_complete;
				}
				view.tournamentRestartButton.gameObject.SetActive(value: false);
				if (can_reset && flag4)
				{
					view.restartButtonNav.gameObject.SetActive(value: false);
					TournamentRoundGameMode activeRoundMode = tournamentData.GetActiveRoundMode();
					if ((uint)(activeRoundMode - 3) <= 1u && tournamentData.GetActiveRoundState() == TournamentRoundState.active)
					{
						view.nextButtonFade.gameObject.SetActive(value: false);
						Activity.RunOnce(delegate
						{
							view.tournamentRestartButton.gameObject.SetActive(value: true);
							view.nextButtonFade.gameObject.SetActive(value: true);
						}, 4f);
					}
					if (!view.spectateButtonNav.gameObject.activeInHierarchy)
					{
						view.tournamentRestartButton.up = view.nextButtonNav;
						view.nextButtonNav.down = view.tournamentRestartButton;
					}
				}
				if ((bool)race)
				{
					bool flag5 = false;
					if (flag3)
					{
						CampaignController campaignController = race as CampaignController;
						flag5 = flag5 || ((bool)campaignController.model.campaign && campaignController.model.campaign.tournament);
						if (!saveComplete)
						{
							view.SetSaveFeedback(p_flag: true);
						}
						this.TimerRunOnce(delegate
						{
							saveComplete = true;
							if (base.validContext)
							{
								view.SetSaveFeedback(p_flag: false);
								view.SetNextEnabled(p_flag: true, 0.3f);
							}
						}, 100f);
					}
					flag5 = flag5 || base.app.arguments.game.tournamentPromo || base.app.arguments.game.promo;
					if (fromEditor)
					{
						view.exitButtonNav.gameObject.SetActive(value: true);
					}
					this.TimerRunOnce(delegate
					{
						if (base.validContext)
						{
							GameObject gameObject = view.exitButtonNav.gameObject;
							if (!gameObject.activeInHierarchy)
							{
								gameObject.SetActive(value: true);
							}
						}
					}, 20f);
					view.SetPromoEnabled(flag5);
					if (flag2)
					{
						NetworkRoom room = base.app.model.network.room;
						List<string> list = new List<string>();
						bool flag6 = false;
						if (room != null && room.VoteTrackList != null && room.VoteTrackList.Count > 0)
						{
							list.Clear();
							list.AddRange(room.VoteTrackList);
							flag6 = room.AllowMapVoting && !room.IsTournamentMatch;
						}
						if (flag6 && list.Count > 0 && !view.votes.HasCards)
						{
							view.votes.gameObject.SetActive(value: true);
							view.votes.Initialize(list);
							LayoutGroup component = view.votes.cardList.GetComponent<LayoutGroup>();
							view.exitButtonNav.right = null;
							view.nextButtonNav.left = null;
							UINavigation.Link(component, view.exitButtonNav, view.nextButtonNav);
							view.spectateButtonNav.left = view.nextButtonNav.left;
						}
						if (flag6)
						{
							RefreshVoteRoomState();
							view.votes.Refresh(room.VoteTrackTable);
						}
					}
					GetDroneRating();
					GetMapRating();
				}
				view.progressionCard.gameObject.SetActive(!DRLApp.offline);
				UINavigation.Focus(view.rightColumn);
				break;
			}
			case "game.standings@update":
			{
				float p_delay = (delayStandingsRefresh ? 1f : 0f);
				RunOnce(p_delay, delegate
				{
					DRLStandingsView standings = view.standings;
					RaceController race2 = view.race;
					if ((bool)race2)
					{
						if (race2.model.GetRacerRankingCount() > 1)
						{
							standings.Refresh(race2.model.Rankings, p_clear: false);
						}
						bool p_flag = race2.model.IsComplete();
						view.SetNextEnabled(p_flag);
						bool restartEnabled = IsRestartAvailable();
						view.SetRestartEnabled(restartEnabled);
					}
				});
				break;
			}
			case "game.race-complete.exit@click":
				_ = base.app.arguments.game.tournamentData;
				base.app.arguments.game.tryouts = false;
				if (base.app.inGame)
				{
					base.app.controller.game.ui.hud.timeout.StopTimeout();
				}
				if (game.model.fromEditor)
				{
					base.enabled = false;
					base.app.view.audio.PlayUIGenericSuccess();
					game.BackToEditor();
				}
				else
				{
					base.enabled = false;
					base.app.view.audio.PlayUIGenericSuccess();
					game.Exit();
					base.app.model.storage.state.player.garage.currentRigData = null;
				}
				break;
			case "game.race.request-restart":
				_ = game.model.mode;
				base.enabled = false;
				if (base.validContext)
				{
					base.app.view.audio.PlayUIGenericSuccess();
					base.app.view.audio.StopUICounterLoop();
				}
				game.Restart();
				break;
			case "game.race-complete.restart@click":
				base.enabled = false;
				if (base.validContext)
				{
					base.app.view.audio.PlayUIGenericSuccess();
					base.app.view.audio.StopUICounterLoop();
				}
				game.Restart();
				break;
			case "game.race-complete.settings@click":
				base.app.view.ui.screens.Open("settings-game-screen");
				break;
			case "game.race-complete.next@click":
			{
				if (!view.nextEnabled)
				{
					base.app.view.audio.PlayUIGenericError();
					break;
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
					break;
				}
				UICollectablesOverviewView uICollectablesOverviewView = base.app.view.ui.screens.Open<UICollectablesOverviewView>("collectables-overview-screen");
				uICollectablesOverviewView.collectable = view.collectable;
				uICollectablesOverviewView.title.text = view.gameMode.GetRaceTitle().ToUpper();
				uICollectablesOverviewView.exitEnabled = view.exitButtonNav.gameObject.activeInHierarchy;
				uICollectablesOverviewView.backgroundCapture = view.backgroundCapture;
				uICollectablesOverviewView.Clear();
				uICollectablesOverviewView.SetTitle();
				break;
			}
			case "network.room.update":
				RefreshVoteRoomState();
				break;
			case "network.player@update":
				RefreshVoteRoomState();
				break;
			case "campaign.result.replay@complete":
				saveComplete = true;
				view.SetSaveFeedback(p_flag: false);
				view.SetNextEnabled(p_flag: true, 0.3f);
				break;
			case "tournament.match.complete":
				view.tournamentRestartButton.gameObject.SetActive(value: false);
				tournamentMatchComplete = true;
				break;
			case "game.race-complete.spectate@click":
				base.app.view.ui.screens.Open<UISpectateView>("game-spectate-screen").GetComponent<UISpectateController>().Initialize();
				break;
			case "game.race-complete.map-rating@click":
				SetMapRating(view.mapRating.index + 1, p_update: true);
				break;
			case "game.race-complete.drone-rating@click":
				SetDroneRating(view.droneRating.index + 1, p_update: true);
				break;
			case "network.race.replay.ready.all":
				view.nextButtonNav.gameObject.SetActive(value: true);
				break;
			}
		}

		private bool IsRestartAvailable()
		{
			_ = view.race;
			bool num = game.model.mode == GameFlag.NetworkMultiplayer;
			bool tryouts = base.app.arguments.game.tryouts;
			if (num)
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
			if (!view.collectable || game.model.playerData == null)
			{
				return;
			}
			DroneRigData rd = game.model.playerData.rig;
			rd.tune = ((!game.model.playerDrone.IsCurrentPhysicsDefault) ? game.model.playerDrone.physics.ToJson() : null);
			rd.profile = ((game.model.playerDrone.profile != null) ? game.model.playerDrone.profile.ToJson() : null);
			Debug.Log("UICollectableCompleteController> SetLeaderboards");
			RunOnce(p_delay, delegate
			{
				if ((bool)this && (bool)view && (bool)view.collectable && view.willSetLeaderboard)
				{
					view.collectable.SetLeaderboard(OnSetLeaderboard, rd);
					view.exitButtonNav.gameObject.SetActive(value: true);
				}
			});
		}

		protected void OnSetLeaderboard(DRLLeaderboardData p_result)
		{
			if (this == null || view == null || DRLApp.offline || !view.willSetLeaderboard)
			{
				return;
			}
			if (p_result == null)
			{
				Debug.LogWarning("UICollectableCompleteController> OnSetLeaderboard - Failed to send leaderboards!");
				SetLeaderboards();
				return;
			}
			DisplayPersonalBest(p_result);
			if (!string.IsNullOrEmpty(p_result.group))
			{
				Debug.Log("UICollectableCompleteController> OnSetLeaderboard - CAMPAIGN Success - highscore[" + p_result.highscore + "] position[" + p_result.position + "]");
				return;
			}
			Debug.Log("UICollectablesCompleteController> OnSetLeaderboard - Game Success - highscore[" + p_result.highscore + "] position[" + p_result.position + "]");
			Notify("game.race.leaderboard-complete", p_result);
			view.SetRaceAnalytics();
			view.FadeInAnalytics(0.1f);
			if (!(base.app == null) && !(base.app.view == null) && !(base.app.view.audio == null))
			{
				OnSetProgression(p_result.progression);
			}
		}

		protected void OnSetProgression(DRLProgressionStateData p_next_progression)
		{
			if (p_next_progression == null)
			{
				Debug.LogWarning("UICollectablesCompleteController> OnSetProgression / Next Progression is <null>");
				return;
			}
			DRLProgressionStateData dRLProgressionStateData = new DRLProgressionStateData();
			dRLProgressionStateData.Merge(base.app.model.storage.state.player.progression.state);
			DRLProgressionStateData next_progression = p_next_progression;
			Debug.Log("UICollectablesCompleteController> Starting Progression Steps\nFrom: " + dRLProgressionStateData.ToJson() + "\nTo: " + next_progression.ToJson());
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
				Debug.Log($"UICollectablesCompleteController> Progression Start / state[{has_progression}]");
				if (has_progression && base.validContext)
				{
					view.SetProgressionNext(next_progression, 2f);
				}
			}, 2.5f);
			base.app.model.storage.state.player.progression.Refresh();
		}

		private void DisplayPersonalBest(DRLLeaderboardData p_result)
		{
			float time = view.collectable.model.time;
			time = Mathf.FloatToHalf(1000f * time) / 1000;
			if (playerLeaderData != null)
			{
				Debug.Log("PlayerLeaderData: " + playerLeaderData.profileName + " " + playerLeaderData.position);
			}
			if (leaderboard.Length != 0)
			{
				if (leaderboard[0].scoreSeconds > time)
				{
					base.app.view.audio.PlayUINewRecord();
					base.app.view.audio.UpdateMusicPostGameResult("new_record");
					view.FadeLeaderboard(p_result.position, GetLeaderboardName(p_result), 0.5f);
					view.willSetLeaderboard = false;
				}
				else if (playerLeaderData != null)
				{
					_ = (float)playerLeaderData.score * 1000f / 1000f;
					if (playerLeaderData.score > p_result.score)
					{
						int position = GetPosition(p_result.score);
						view.FadeLeaderboard(position, GetLeaderboardName(p_result), 0.5f);
						view.willSetLeaderboard = false;
					}
				}
				else
				{
					int position2 = GetPosition(p_result.score);
					view.FadeLeaderboard(position2, GetLeaderboardName(p_result), 0.5f);
					view.willSetLeaderboard = false;
				}
			}
			else
			{
				base.app.view.audio.PlayUINewRecord();
				base.app.view.audio.UpdateMusicPostGameResult("new_record");
				view.FadeLeaderboard(1, GetLeaderboardName(p_result), 0.5f);
				view.willSetLeaderboard = false;
			}
		}

		private int GetPosition(int p_score)
		{
			for (int i = 0; i < leaderboard.Length; i++)
			{
				if (leaderboard[i].score > p_score)
				{
					return i + 1;
				}
			}
			return leaderboard.Length + 1;
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

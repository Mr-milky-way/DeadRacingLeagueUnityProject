using System;
using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using drl.network;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UITournamentRaceCompleteController : Controller<DRLApp>
	{
		private float m_syncDuration = 15f;

		private float m_syncTimer = 15f;

		private Activity m_syncActivity;

		private string m_matchId = "";

		private bool m_matchStarting;

		public UITournamentRaceCompleteView view => AssertLocal<UITournamentRaceCompleteView>("view");

		private GameController game => base.app.controller.game;

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "ui.screen@close":
				if (p_data.Length != 0)
				{
					UIScreen uIScreen = p_data[0] as UIScreen;
					if (!(uIScreen == null) && !(uIScreen.name != view.screen.name))
					{
						view.StopVideo();
						StopSyncData();
					}
				}
				break;
			case "tournament.action.start-match":
			{
				if (p_data.Length == 0)
				{
					break;
				}
				NetworkRoom room = base.app.model.network.room;
				if (room != null && !string.IsNullOrEmpty(room.MatchId))
				{
					string text = p_data[0] as string;
					if (!string.IsNullOrEmpty(text) && !(text != room.MatchId))
					{
						view.watchButton.interactable = false;
						m_matchStarting = true;
					}
				}
				break;
			}
			}
			if (!view.current)
			{
				return;
			}
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
			{
				base.app.view.ui.SetDark(p_flag: false);
				base.app.view.ui.game.preventFooter = true;
				this.TimerRunOnce(delegate
				{
					base.app.view.ui.footer.Hide(0f);
				}, 0.2f);
				view.FadeOut();
				view.SetFeedback(UITournamentLeaderboardFeedbackType.Loading);
				StartSyncData();
				view.StartVideo();
				view.watchButton.interactable = false;
				view.exitButton.interactable = false;
				view.nextButton.interactable = !base.app.inVirtualSeason;
				bool is_complete = view.race != null && view.race.model.IsComplete();
				view.spectateButton.interactable = false;
				this.TimerRunOnce(delegate
				{
					base.app.model.game.camera.main.enabled = false;
				}, 2f);
				this.TimerRunOnce(delegate
				{
					if (base.validContext && view.current)
					{
						view.spectateButton.interactable = !is_complete && base.app.controller.game.model.racerCount > 1;
					}
				}, 3f);
				bool flag = base.app.arguments.game.mode == GameFlag.NetworkMultiplayer;
				view.backButton.SetActive(!flag);
				RaceController race = view.race;
				NetworkRoom room2 = base.app.model.network.room;
				if (flag && race != null)
				{
					NetworkRaceController networkRaceController = race as NetworkRaceController;
					if (networkRaceController != null)
					{
						bool flag2 = room2 != null && room2.Local != null && room2.Local.IsSpectator;
						view.watchButton.interactable = networkRaceController.allReplaysProcessed;
						view.nextButton.interactable = networkRaceController.allReplaysProcessed || !base.app.inVirtualSeason || !base.app.model.tournament.replaysProcessing;
						view.exitButton.interactable = flag2 || networkRaceController.allReplaysProcessed;
					}
				}
				if (m_matchStarting)
				{
					view.watchButton.interactable = false;
				}
				if (base.app.model.network.room != null)
				{
					m_matchId = base.app.model.network.room.MatchId;
				}
				this.TimerRunOnce(delegate
				{
					if (base.validContext && view != null && view.current)
					{
						view.exitButton.interactable = true;
						view.nextButton.interactable = true;
						base.app.model.tournament.StopReplayProcessing();
					}
				}, 60f);
				base.app.view.ui.screens.SetStaticBackground();
				if (base.app.inGame)
				{
					base.app.model.game.camera.main.enabled = false;
				}
				Debug.Log("UITournamentRaceComplete> Screen opened and ready at " + DateTime.Now.ToString());
				break;
			}
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			case "game.tournament-race-complete.next@click":
				if (base.app.arguments.game.tournamentData != null)
				{
					UITournamentResultsView uITournamentResultsView = base.app.view.ui.screens.Open<UITournamentResultsView>("tournament-results-screen");
					uITournamentResultsView.race = view.race;
					uITournamentResultsView.matchData = base.app.arguments.game.tournamentMatchData;
					StopSyncData();
				}
				break;
			case "game.race-complete.exit@click":
				if (base.app.arguments.game.tournamentData != null)
				{
					base.app.view.ui.screens.Open<UITournamentBracketsView>("tournament-brackets-screen");
					StopSyncData();
				}
				break;
			case "network.race.replay.ready.all":
				view.watchButton.interactable = true;
				view.exitButton.interactable = true;
				view.nextButton.interactable = true;
				Debug.Log("UITournamentRaceCompleteController> Replays ready! " + DateTime.Now.ToString());
				break;
			case "tournament.match.results-arrived":
			{
				if (p_data.Length == 0)
				{
					break;
				}
				string text3 = (string)p_data[0];
				if (base.app.model.network.room == null || base.app.model.network.room.MatchId != text3)
				{
					break;
				}
				this.TimerRunOnce(delegate
				{
					if (base.validContext && view.current)
					{
						RefreshData();
					}
				}, 2f);
				view.nextButton.interactable = true;
				view.allResultsReady = true;
				break;
			}
			case "game.race-overview.replay@click":
			{
				if (!base.validContext)
				{
					break;
				}
				if (base.app.arguments.game.mode == GameFlag.NetworkMultiplayer)
				{
					NetworkRaceController networkRaceController2 = view.race as NetworkRaceController;
					if (networkRaceController2 != null && !networkRaceController2.allReplaysProcessed)
					{
						break;
					}
				}
				game.model.simulation.drones.FixAll();
				game.model.simulation.drones.SetVisible(p_flag: false);
				UISecondaryHeaderView headerSecondary = base.app.view.ui.headerSecondary;
				if (headerSecondary != null)
				{
					headerSecondary.Refresh(view, p_is_under_review: false);
				}
				StopSyncData();
				base.app.arguments.game.type = GameFlag.Replay;
				base.app.model.game.type = GameFlag.Replay;
				base.app.view.ui.game.hud.Hide();
				if (base.app.inGame)
				{
					base.app.model.game.camera.main.enabled = true;
				}
				StopSyncData();
				UISpectateView uISpectateView = base.app.view.ui.screens.Open<UISpectateView>("game-spectate-screen");
				UISpectateController component = uISpectateView.GetComponent<UISpectateController>();
				uISpectateView.tournamentContext = true;
				if (ReplayFile.EnableVersion2)
				{
					List<ReplayFile> replaysV = game.model.GetReplaysV2();
					int count = replaysV.Count;
					replaysV = game.model.replay.gameReplayClipsV2;
					Debug.Log($"UITournamentResultsController> RaceOverviewReplayClick / playerdata-clips[{count}] gamereplay-clips[{replaysV.Count}]");
					component.SetReplayClips(replaysV);
				}
				else
				{
					List<BlackboxData> replays = game.model.GetReplays();
					int count2 = replays.Count;
					replays = game.model.replay.gameReplayClips;
					Debug.Log($"UITournamentResultsController> RaceOverviewReplayClick / playerdata-clips[{count2}] gamereplay-clips[{replays.Count}]");
					component.SetReplayClips(replays);
				}
				component.Initialize(GameFlag.Replay);
				break;
			}
			case "game.race-complete.spectate@click":
				this.TimerRunOnce(delegate
				{
					if (!base.validContext || view == null || !view.current || !base.app.inGame || view.race == null || view.race.model.IsComplete())
					{
						view.spectateButton.interactable = false;
					}
					else
					{
						if (base.app.inGame)
						{
							base.app.model.game.camera.main.enabled = true;
						}
						UISpectateView uISpectateView2 = base.app.view.ui.screens.Open<UISpectateView>("game-spectate-screen");
						UISpectateController component2 = uISpectateView2.GetComponent<UISpectateController>();
						uISpectateView2.tournamentContext = true;
						component2.Initialize();
					}
				}, 0.3f);
				break;
			case "network.race.end":
				view.spectateButton.interactable = false;
				break;
			case "tournament.action.reset-heat":
			case "tournament.action.reset-match":
				if (base.validContext && base.app.model.network.room != null && base.app.model.network.room.GameMode == NetworkRoom.GameType.Tournament && p_data.Length != 0)
				{
					string text2 = (string)p_data[0];
					if (!string.IsNullOrEmpty(text2) && !(base.app.model.network.room.MatchId != text2))
					{
						base.app.view.ui.screens.Open<UITournamentBracketsView>("tournament-brackets-screen");
					}
				}
				break;
			}
		}

		private void StartSyncData()
		{
			StopSyncData();
			m_syncActivity = ((Component)this).ActivityRun((Func<bool>)delegate
			{
				if (m_syncTimer >= m_syncDuration)
				{
					RefreshData();
					m_syncTimer = 0f;
				}
				m_syncTimer += Time.deltaTime;
				return true;
			}, 0f);
		}

		private void StopSyncData()
		{
			if (m_syncActivity != null)
			{
				m_syncActivity.Stop();
				m_syncActivity = null;
				m_syncTimer = m_syncDuration;
			}
		}

		private void RefreshData()
		{
			if (base.app == null || !base.validContext || !base.app.inGame || !view.current || base.app.arguments == null || base.app.arguments.game == null)
			{
				return;
			}
			DRLTournamentData td = base.app.arguments.game.tournamentData;
			if (td == null)
			{
				return;
			}
			string text = "";
			if (base.app.model.network.room != null)
			{
				text = (m_matchId = base.app.model.network.room.MatchId);
			}
			else
			{
				if (string.IsNullOrEmpty(m_matchId))
				{
					Debug.Log("UITournamentRaceCompleteController> Couldn't find matchID using active network room, defaulting to last saved matchID.");
					m_matchId = base.app.model.tournament.GetLastMatchID();
					if (string.IsNullOrEmpty(m_matchId))
					{
						Debug.LogWarning("UITournamentRaceCompleteController> Failed to find valid match ID!");
						view.SetFeedback(UITournamentLeaderboardFeedbackType.NoResult);
						return;
					}
				}
				text = m_matchId;
			}
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			base.app.model.tournament.RefreshMatchData(text, delegate(DRLTournamentMatchData p_result)
			{
				if (!base.validContext || p_result == null)
				{
					if (!(view == null) && view.current)
					{
						view.SetFeedback(UITournamentLeaderboardFeedbackType.NoResult);
					}
				}
				else
				{
					DRLTournamentMatchData md = p_result;
					UISecondaryHeaderView headerSecondary = base.app.view.ui.headerSecondary;
					bool flag = md?.isUnderReview ?? false;
					if (headerSecondary != null)
					{
						headerSecondary.Refresh(view, flag);
					}
					Debug.Log($"UITournamentRaceCompleteController>@@ TournamentRefreshState match_data == null:{md == null} is_under_review: {flag}");
					DRLTournamentRoundData activeRound = td.GetActiveRound();
					string round_title = activeRound.title;
					bool is_sudden_death = activeRound.gameMode == TournamentRoundGameMode.suddenDeath;
					bool is_golden_heat = activeRound.gameMode == TournamentRoundGameMode.goldenHeat;
					int match_idx = md.activeHeat;
					match_idx = Mathf.Clamp(match_idx, 1, md.heatCount);
					base.app.model.service.GetHeatResults(td.guid, md.Id, match_idx, delegate(DRLTournamentHeatData p_data)
					{
						if (!base.validContext || p_data == null)
						{
							view.SetFeedback(UITournamentLeaderboardFeedbackType.NoResult);
						}
						else if (p_data.results.Length == 0 && view.current)
						{
							view.SetFeedback(UITournamentLeaderboardFeedbackType.Pending);
							this.TimerRunOnce(delegate
							{
								RefreshData();
							}, 3f);
						}
						else
						{
							view.SetFeedback(UITournamentLeaderboardFeedbackType.None);
							view.SetTitle(round_title, match_idx, md.heatCount, is_sudden_death, is_golden_heat);
							view.Set(p_data);
							this.TimerRunOnce(delegate
							{
								if (base.validContext && view.current)
								{
									GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
									Debug.Log("UITournamentRaceCompleteController>  GC forced cleanup on tournaments refresh data..");
								}
							}, 0.1f);
						}
					});
				}
			});
		}

		private void OnDisable()
		{
			StopSyncData();
			view.StopVideo();
		}
	}
}

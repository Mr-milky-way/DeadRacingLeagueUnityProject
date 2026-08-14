using System;
using UnityEngine;
using drl.backend;
using drl.network;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UITournamentCollectablesCompleteController : Controller<DRLApp>
	{
		private float m_syncDuration = 15f;

		private float m_syncTimer = 15f;

		private Activity m_syncActivity;

		private string m_matchId = "";

		public UITournamentRaceCompleteView view => AssertLocal<UITournamentRaceCompleteView>("view");

		private GameController game => base.app.controller.game;

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (p_event != null && p_event == "ui.screen@close" && p_data.Length != 0)
			{
				UIScreen uIScreen = p_data[0] as UIScreen;
				if (!(uIScreen == null) && !(uIScreen != view))
				{
					view.StopVideo();
					StopSyncData();
				}
			}
			if (!view.current || p_event == null)
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
				view.nextButton.interactable = false;
				bool flag = view.race != null && view.race.model.IsComplete();
				view.spectateButton.interactable = !flag && base.app.controller.game.model.racerCount > 1;
				bool num = base.app.arguments.game.mode == GameFlag.NetworkMultiplayer;
				RaceController race = view.race;
				NetworkRoom room = base.app.model.network.room;
				if (num && race != null)
				{
					NetworkRaceController networkRaceController2 = race as NetworkRaceController;
					if (networkRaceController2 != null)
					{
						bool flag2 = room != null && room.Local != null && room.Local.IsSpectator;
						view.watchButton.interactable = networkRaceController2.allReplaysProcessed;
						view.exitButton.interactable = flag2 || networkRaceController2.allReplaysProcessed;
					}
				}
				if (room != null)
				{
					m_matchId = room.MatchId;
				}
				this.TimerRunOnce(delegate
				{
					if (base.validContext && view != null && view.current)
					{
						view.exitButton.interactable = true;
						view.nextButton.interactable = true;
					}
				}, 90f);
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
				if (p_data.Length != 0)
				{
					string text = (string)p_data[0];
					if (base.app.model.network.room != null && !(base.app.model.network.room.MatchId != text))
					{
						view.nextButton.interactable = true;
					}
				}
				break;
			case "game.race-overview.replay@click":
			{
				if (!base.validContext)
				{
					break;
				}
				if (base.app.arguments.game.mode == GameFlag.NetworkMultiplayer)
				{
					NetworkRaceController networkRaceController = view.race as NetworkRaceController;
					if ((bool)networkRaceController && !networkRaceController.allReplaysProcessed)
					{
						break;
					}
				}
				game.model.simulation.drones.SetVisible(p_flag: false);
				base.app.arguments.game.type = GameFlag.Replay;
				base.app.model.game.type = GameFlag.Replay;
				base.app.view.ui.game.hud.Hide();
				StopSyncData();
				UISpectateView uISpectateView = base.app.view.ui.screens.Open<UISpectateView>("game-spectate-screen");
				UISpectateController component = uISpectateView.GetComponent<UISpectateController>();
				uISpectateView.tournamentContext = true;
				component.SetReplayClips(game.model);
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
			case "tournament.action.refresh":
				RefreshData();
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
			if (!base.app.inGame)
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
					return;
				}
				text = m_matchId;
			}
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			base.app.model.service.GetMatch(td.guid, text, delegate(DRLTournamentMatchResult p_result)
			{
				if (base.validContext && p_result != null && p_result.matches.Length != 0)
				{
					DRLTournamentMatchData md = p_result.matches[0];
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
						else
						{
							view.SetFeedback(UITournamentLeaderboardFeedbackType.None);
							view.SetTitle(round_title, match_idx, md.heatCount, is_sudden_death, is_golden_heat);
							view.Set(p_data);
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

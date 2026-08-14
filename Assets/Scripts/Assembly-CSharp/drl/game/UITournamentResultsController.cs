using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using drl.backend;
using drl.network;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UITournamentResultsController : Controller<DRLApp>
	{
		private bool m_matchStarting;

		private WebAsyncRequest m_replayDownloader;

		private Thread m_replayProcess;

		private List<ReplayFile> m_parsedReplays = new List<ReplayFile>();

		private bool m_cancelReplayLoad;

		private bool m_replayLoading;

		public GameController game => base.app.controller.game;

		public UITournamentResultsView view => AssertLocal<UITournamentResultsView>("view");

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (p_event != null && p_event == "tournament.action.start-match" && p_data.Length != 0)
			{
				NetworkRoom room = base.app.model.network.room;
				if (room != null && !string.IsNullOrEmpty(room.MatchId))
				{
					string text = p_data[0] as string;
					if (!string.IsNullOrEmpty(text) && !(text != room.MatchId))
					{
						view.SetReplayEnabled(p_flag: false);
						m_matchStarting = true;
					}
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
				if (p_data[0] as UIScreen != view.screen)
				{
					break;
				}
				base.app.view.ui.SetDark(p_flag: true);
				if (base.app.inGame)
				{
					base.app.view.ui.game.preventFooter = true;
				}
				m_replayLoading = false;
				this.TimerRunOnce(delegate
				{
					base.app.view.ui.footer.Hide(0f);
				}, 0.2f);
				this.TimerRunOnce(delegate
				{
					if (base.app.model.game != null && base.app.model.game.camera != null)
					{
						base.app.model.game.camera.main.enabled = false;
					}
				}, 0.1f);
				view.ClearTable(p_animate: false);
				view.headerFade.FadeOut(0f);
				m_cancelReplayLoad = false;
				if (base.app.inGame)
				{
					base.app.view.ui.game.hud.Hide(0f);
				}
				view.SetFeedback(UITournamentLeaderboardFeedbackType.None);
				DRLTournamentData dRLTournamentData = (base.app.inGame ? base.app.arguments.game.tournamentData : base.app.arguments.tournament.data);
				if (dRLTournamentData == null)
				{
					Debug.LogWarning("UITournamentResultsController> No tournament data!");
					view.SetFeedback(UITournamentLeaderboardFeedbackType.NoResult);
					break;
				}
				DRLTournamentRoundData activeRound = dRLTournamentData.GetActiveRound();
				if (activeRound == null)
				{
					Debug.LogWarning("UITournamentResultsController> No round data!");
					view.SetFeedback(UITournamentLeaderboardFeedbackType.NoResult);
					break;
				}
				if (base.app.inGame && activeRound.gameMode == TournamentRoundGameMode.leaderboard)
				{
					Debug.LogWarning("UITournamentResultsController> Leaderboard game mode in results screen! Leaderboard screen should be opened.");
					view.SetFeedback(UITournamentLeaderboardFeedbackType.NoResult);
					break;
				}
				view.SetFeedback(UITournamentLeaderboardFeedbackType.Loading);
				RefreshData();
				if (base.app.inGame)
				{
					base.app.model.game.camera.main.enabled = false;
				}
				view.SetReplayActive(!view.openedFromTheBrackets);
				view.nextButton.interactable = true;
				if (view.race == null || view.openedFromTheBrackets)
				{
					break;
				}
				NetworkRaceController networkRaceController = view.race as NetworkRaceController;
				view.SetReplayEnabled(networkRaceController != null && networkRaceController.allReplaysProcessed);
				if (!base.app.inVirtualSeason)
				{
					break;
				}
				view.nextButton.interactable = (networkRaceController != null && networkRaceController.allReplaysProcessed) || !base.app.model.tournament.replaysProcessing;
				this.TimerRunOnce(delegate
				{
					if (base.validContext && !(view == null) && view.current)
					{
						view.nextButton.interactable = true;
						base.app.model.tournament.StopReplayProcessing();
					}
				}, 60f);
				break;
			}
			case "tournament.results.next@click":
			{
				DRLTournamentData data = (base.app.inGame ? base.app.arguments.game.tournamentData : base.app.arguments.tournament.data);
				if (data == null)
				{
					Debug.LogWarning("UITournamentResultsScreen> Next click: No tournament data available!");
					break;
				}
				if (base.app.controller.network.model.room == null || view.openedFromTheBrackets)
				{
					base.app.view.ui.screens.Open<UITournamentBracketsView>("tournament-brackets-screen");
					view.nextButton.interactable = true;
					view.openedFromTheBrackets = false;
					break;
				}
				if (view.matchData == null)
				{
					Debug.LogWarning("UITournamentResultsScreen> Next click: No match data available! Opening brackets screen..");
					base.app.view.ui.screens.Open<UITournamentBracketsView>("tournament-brackets-screen");
					view.nextButton.interactable = true;
					view.openedFromTheBrackets = false;
					break;
				}
				base.app.model.tournament.RefreshMatchData(view.matchData.Id, delegate(DRLTournamentMatchData p_result)
				{
					if (!base.validContext || view == null || !view.current)
					{
						Debug.LogWarning("UITournamentResultsScreen> Context not valid anymore!");
					}
					else if (p_result == null)
					{
						Debug.LogWarning("UITournamentResultsScreen> No data for given tournament ID: " + data.guid + " and match ID: " + view.matchData.Id);
					}
					else
					{
						view.matchData = p_result;
						view.openedFromTheBrackets = false;
						if (view.matchData.state == TournamentMatchState.complete || view.matchData.state == TournamentMatchState.fail || view.matchData.state == TournamentMatchState.canceled || view.matchData.currentHeat > view.matchData.heatCount)
						{
							base.app.view.ui.screens.Open<UITournamentBracketsView>("tournament-brackets-screen");
						}
						else
						{
							base.app.view.ui.screens.Open<UIMultiplayerRoomView>("multiplayer-room-screen").SetExitButtonEnabled(p_enabled: true);
						}
					}
				});
				break;
			}
			case "tournament.action.refresh":
				RefreshData(p_updateResultsOnly: true);
				break;
			case "ui.screen.return@click":
			{
				base.app.view.ui.screens.Return();
				view.openedFromTheBrackets = false;
				UISecondaryHeaderView headerSecondary = base.app.view.ui.headerSecondary;
				if (headerSecondary != null)
				{
					headerSecondary.Refresh(view, p_is_under_review: false);
				}
				break;
			}
			case "network.race.replay.ready.all":
				view.SetReplayEnabled(p_flag: true);
				view.nextButton.interactable = true;
				Debug.Log("UITournamentResultsController> Replays ready! " + DateTime.Now.ToString());
				break;
			case "game.race-overview.replay@click":
			{
				if (!base.validContext || view.race == null || view.openedFromTheBrackets)
				{
					break;
				}
				NetworkRaceController networkRaceController2 = view.race as NetworkRaceController;
				if (!(networkRaceController2 == null) && networkRaceController2.allReplaysProcessed)
				{
					game.model.simulation.drones.FixAll();
					game.model.simulation.drones.SetVisible(p_flag: false);
					base.app.arguments.game.tournamentMatchData = view.matchData;
					base.app.arguments.game.type = GameFlag.Replay;
					base.app.model.game.type = GameFlag.Replay;
					if (base.app.inGame)
					{
						base.app.model.game.camera.main.enabled = true;
					}
					base.app.view.ui.game.hud.Hide();
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
				}
				break;
			}
			case "tournament.match-heat.replay@click":
			{
				if (!base.validContext || view.matchData == null || p_target == null)
				{
					break;
				}
				UIElementView uIElementView = p_target as UIElementView;
				if (uIElementView == null || uIElementView.transform.parent == null || uIElementView.transform.parent.parent == null)
				{
					break;
				}
				Transform parent = uIElementView.transform.parent.parent;
				int result = -1;
				int.TryParse(parent.name, out result);
				if (result == -1)
				{
					break;
				}
				string[] array = base.app.model.tournament.FetchMatchHeatReplays(view.matchData.Id, result);
				if (array == null || array.Length == 0)
				{
					D.Warning("UITournamentResultsController> Replays missing from tournament model!");
					break;
				}
				D.Log(string.Join(",", array));
				if (!m_replayLoading)
				{
					m_replayLoading = true;
					LoadReplays(array);
				}
				break;
			}
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

		private void LoadReplays(string[] p_replayURLs)
		{
			if (p_replayURLs == null || p_replayURLs.Length == 0)
			{
				view.SetFeedback(UITournamentLeaderboardFeedbackType.Failed);
				this.TimerRunOnce(delegate
				{
					view.SetFeedback(UITournamentLeaderboardFeedbackType.None);
				}, 3f);
				m_replayLoading = false;
				return;
			}
			bool flag = false;
			for (int num = 0; num < p_replayURLs.Length; num++)
			{
				if (!string.IsNullOrEmpty(p_replayURLs[num]))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				view.SetFeedback(UITournamentLeaderboardFeedbackType.Failed);
				this.TimerRunOnce(delegate
				{
					view.SetFeedback(UITournamentLeaderboardFeedbackType.None);
				}, 3f);
				m_replayLoading = false;
				return;
			}
			view.SetFeedback(UITournamentLeaderboardFeedbackType.Loading);
			m_parsedReplays.Clear();
			DRLTournamentData d = base.app.model.tournament.tournament;
			D.Log("UITournamentREsultsController> Replay Load: starting replay download and parse:\n" + string.Join("\n", p_replayURLs));
			Action p_onComplete = delegate
			{
				if (m_parsedReplays.Count != 0)
				{
					D.Log("UITournamentREsultsController> Replay Load: loaded[" + m_parsedReplays.Count + "] and finished.");
					for (int i = 0; i < m_parsedReplays.Count; i++)
					{
						D.Log((m_parsedReplays[i].header != null) ? m_parsedReplays[i].header.profileName : "");
					}
					m_replayLoading = false;
					base.app.arguments.game.tournamentData = d;
					base.app.scene.Load(m_parsedReplays, view.matchData.mapId, view.matchData.trackId, view.matchData.isCustomMap ? view.matchData.customMapId : null);
				}
			};
			DownloadAndParseReplays(p_replayURLs, 0, p_onComplete);
		}

		private void DownloadAndParseReplays(string[] p_replayURLs, int p_replayIdx, Action p_onComplete = null)
		{
			if (p_replayIdx == p_replayURLs.Length)
			{
				this.TimerRunOnce(delegate
				{
					p_onComplete?.Invoke();
				}, 1f / 60f);
				return;
			}
			D.Log("UITournamentREsultsController> DownloadAndParseReplays: Checking replay " + p_replayIdx + "\n" + p_replayURLs[p_replayIdx]);
			if (string.IsNullOrEmpty(p_replayURLs[p_replayIdx]))
			{
				this.TimerRunOnce(delegate
				{
					p_replayIdx++;
					DownloadAndParseReplays(p_replayURLs, p_replayIdx, p_onComplete);
				}, 1f / 60f);
				return;
			}
			m_replayDownloader = Web.Get(p_replayURLs[p_replayIdx], delegate(byte[] data, float progress, WebAsyncRequest request)
			{
				if (!(progress < 1f))
				{
					if (request.hasError)
					{
						CancelReplayLoad();
					}
					else
					{
						p_replayIdx++;
						D.Log("UITournamentREsultsController> DownloadAndParseReplays: replay - " + p_replayURLs[p_replayIdx - 1] + " finished downloading. Starting parsing..");
						m_replayProcess = new Thread((ThreadStart)delegate
						{
							if (m_cancelReplayLoad)
							{
								m_cancelReplayLoad = false;
							}
							else
							{
								ReplayFile rf = ReplayFile.FromBytes(data);
								m_parsedReplays.Add(rf);
								data = null;
								this.TimerRunOnce(delegate
								{
									D.Log("UITournamentREsultsController> DownloadAndParseReplays: replay - " + p_replayURLs[p_replayIdx - 1] + " finished parsing.");
									base.app.model?.service?.opponent?.TryAddLoadedReplayV2(rf);
									DownloadAndParseReplays(p_replayURLs, p_replayIdx, p_onComplete);
								}, 1f / 60f);
							}
						});
						m_replayProcess.Start();
					}
				}
			});
		}

		private void CancelReplayLoad()
		{
			if (m_replayDownloader != null)
			{
				m_replayDownloader.Cancel();
			}
			m_replayDownloader = null;
			if (m_replayProcess != null)
			{
				m_replayProcess.Abort();
			}
			m_replayProcess = null;
			view.SetFeedback(UITournamentLeaderboardFeedbackType.None);
			m_cancelReplayLoad = true;
			m_replayLoading = false;
		}

		private void RefreshData(bool p_updateResultsOnly = false)
		{
			DRLAppArguments.Game args = base.app.arguments.game;
			DRLTournamentData data = (base.app.inGame ? args.tournamentData : base.app.arguments.tournament.data);
			Localization l = base.app.model.storage.locale;
			_ = base.app.model.service.backend.playerId;
			if (data == null)
			{
				Debug.LogWarning("UITournamentsResultsController> No Tournament Data!");
				view.ClearTable();
				return;
			}
			if (view.openedFromTheBrackets)
			{
				view.nextButton.gameObject.SetActive(value: false);
			}
			NetworkRoom room = base.app.controller.network.model.room;
			string tournament_guid = data.guid;
			string match_id = ((room != null) ? room.MatchId : ((view.matchData != null) ? view.matchData.Id : ""));
			if (string.IsNullOrEmpty(match_id))
			{
				Debug.LogWarning("UITournamentsResultsController> MatchId is missing / tournament[" + tournament_guid + "], fetching first available match");
				DRLTournamentRoundData activeRound = data.GetActiveRound();
				if (activeRound == null || activeRound.matches.Length == 0)
				{
					view.nextButton.interactable = true;
					return;
				}
				match_id = data.GetActiveRound().matches[0].Id;
			}
			if (data.invalid)
			{
				view.ClearTable();
				view.nextButton.interactable = true;
				return;
			}
			RunOnce(delegate
			{
				base.app.model.tournament.RefreshMatchData(match_id, delegate(DRLTournamentMatchData result)
				{
					if (base.validContext && !(view == null) && view.current)
					{
						if (result == null)
						{
							view.ClearTable();
							Debug.LogWarning("UITournamentsResultsController> No Results / tournament[" + tournament_guid + "] match[" + match_id + "]");
							view.SetFeedback(UITournamentLeaderboardFeedbackType.NoResult);
						}
						else
						{
							args.tournamentMatchData = result;
							view.matchData = result;
							DRLTournamentRoundData roundForMatch = data.GetRoundForMatch(result.Id);
							if (roundForMatch == null || roundForMatch.gameMode == TournamentRoundGameMode.leaderboard)
							{
								view.SetFeedback(UITournamentLeaderboardFeedbackType.NoResult);
								Debug.LogWarning("UITournamentResultsController> No round data or leaderboard mode active.");
							}
							else
							{
								string title = l.Get("vdrl.label.match-invalid", "MATCH INVALID");
								l.Get("vdrl.label.group", "GROUP");
								_ = result.groupNumber;
								string text = l.Get("vdrl.label.heat", "HEAT");
								int activeHeat = result.activeHeat;
								string text2 = "";
								string text3 = "";
								activeHeat = Mathf.Clamp(activeHeat, 1, result.heatCount);
								text2 = ((!string.IsNullOrEmpty(roundForMatch.title)) ? roundForMatch.title : "ROUND");
								if (result.invalid)
								{
									view.title = title;
								}
								else
								{
									TournamentRoundGameMode gameMode = result.gameMode;
									text3 = text + " " + activeHeat;
									if (activeHeat == result.heatCount && gameMode == TournamentRoundGameMode.suddenDeath)
									{
										text3 = "SUDDEN DEATH";
									}
									if (activeHeat == result.heatCount && gameMode == TournamentRoundGameMode.goldenHeat)
									{
										text3 = "GOLDEN HEAT";
									}
									view.SetTitle(text2, text3);
									view.SetMatchData(result, gameMode);
									if (!m_replayLoading)
									{
										view.SetFeedback(UITournamentLeaderboardFeedbackType.None);
									}
								}
								this.TimerRunOnce(delegate
								{
									if (base.validContext && view.current)
									{
										GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
										Debug.Log("UITournamentResultsController>  GC forced cleanup on tournaments refresh data..");
									}
								}, 0.1f);
							}
						}
					}
				});
			});
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UITournamentLeaderboardsController : Controller<DRLApp>
	{
		private string m_roundId;

		public int pageLength = 10;

		protected WebAsyncRequest m_replay_loader;

		private Thread m_replay_thread;

		protected Activity m_load_timer;

		protected bool m_ignore_page_click;

		protected bool m_ignore_replay_click;

		public float failAutoRefreshPeriod = 2f;

		private bool m_ignoreRestart;

		private float m_syncDuration = 15f;

		private float m_syncTimer;

		private Activity m_syncActivity;

		private List<UINavigation> m_replayNavs = new List<UINavigation>();

		public UITournamentLeaderboardsView view => AssertLocal<UITournamentLeaderboardsView>("view");

		public TournamentModel model => base.app.model.tournament;

		public DRLTournamentData tournament => model.tournament;

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (p_event != null && p_event == "ui.screen@close" && p_data.Length != 0 && !((p_data[0] as UIScreen).name != view.screen.name))
			{
				view.StopVideo();
				StopSyncData();
				CancelReplayLoad();
			}
			if (base.app.view.ui.screens.current != view.screen || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				if (!(p_data[0] as UIScreen != view.screen))
				{
					view.raceListFade.alpha = -0.1f;
					view.SetFeedback(UITournamentLeaderboardFeedbackType.Loading, p_hide_list: true);
					view.raceListField.Clear();
					view.StartVideo();
					m_syncTimer = 0f;
					StartSyncData();
					if (!view.openedFromTheBrackets && base.app.arguments.game.tournamentData != null && base.app.arguments.game.tournamentMatchData != null)
					{
						m_roundId = ((base.app.arguments.game.tournamentData.rounds[base.app.arguments.game.tournamentMatchData.roundIndex].gameMode == TournamentRoundGameMode.leaderboard) ? base.app.arguments.game.tournamentMatchData.roundId : null);
					}
					if (view.openedFromTheBrackets && view.round != null && view.round.matches != null && view.round.matches.Length != 0)
					{
						m_roundId = view.round.matches[0].roundId;
					}
					RefreshList(0);
					view.nextButton.SetActive(!view.openedFromTheBrackets);
					view.restartButton.SetActive(!view.openedFromTheBrackets);
					if (base.app.inGame)
					{
						base.app.view.ui.game.preventFooter = true;
						base.app.view.ui.game.hud.Hide(0f);
					}
					this.TimerRunOnce(delegate
					{
						base.app.view.ui.footer.Hide(0f);
					}, 0.2f);
					LayoutRebuilder.ForceRebuildLayoutImmediate(view.headerRect);
					UINavigation.Focus(view.backButton.activeInHierarchy ? view.backButtonNav : view.nextButtonNav);
					m_ignoreRestart = false;
				}
				break;
			case "leaderboards.page@select":
				if (!m_ignore_page_click)
				{
					int page = (int)p_data[0];
					RefreshListDelayed(page, p_is_page_change: true);
				}
				break;
			case "leaderboards.page-next@click":
				if (!m_ignore_page_click)
				{
					int index = view.racePageField.index;
					int count = view.racePageField.listField.Count;
					if (index + 1 != count)
					{
						view.racePageField.index = index + 1;
						RefreshListDelayed(view.racePageField.index, p_is_page_change: true);
					}
				}
				break;
			case "leaderboards.page-previous@click":
				if (!m_ignore_page_click)
				{
					int index2 = view.racePageField.index;
					if (index2 != 0)
					{
						view.racePageField.index = index2 - 1;
						RefreshListDelayed(view.racePageField.index, p_is_page_change: true);
					}
				}
				break;
			case "game.tournament.results@submit":
				RefreshList();
				break;
			case "tournament.action.refresh":
				RefreshList();
				break;
			case "ui.screen.return@click":
				CancelReplayLoad();
				if (base.app.view.ui.screens.manager.IsInHistory("tournament-leaders-screen"))
				{
					base.app.view.ui.screens.Return();
					break;
				}
				view.openedFromTheBrackets = false;
				SaveArgs();
				base.app.view.ui.screens.Return();
				break;
			case "tournament.leaderboards.next@click":
				CancelReplayLoad();
				base.app.view.ui.screens.Open<UITournamentBracketsView>("tournament-brackets-screen");
				view.openedFromTheBrackets = false;
				break;
			case "leaderboards.item.replay@click":
			{
				Debug.Log("UITournamentLeaderboardsController> On replay item click.");
				if (p_data.Length == 0)
				{
					base.app.view.audio.PlayUIGenericError();
					m_ignore_replay_click = false;
					break;
				}
				string text = (string)p_data[0];
				if (string.IsNullOrEmpty(text))
				{
					base.app.view.audio.PlayUIGenericError();
					m_ignore_replay_click = false;
				}
				else
				{
					if (m_ignore_replay_click || m_ignoreRestart)
					{
						break;
					}
					m_ignore_replay_click = true;
					if (tournament != null && tournament.rounds.Length != 0)
					{
						Debug.Log("UITournamentLeaderboardsController> Starting replay loading: " + text);
						DRLTournamentRoundData dRLTournamentRoundData = tournament.GetActiveRound();
						if (dRLTournamentRoundData == null)
						{
							dRLTournamentRoundData = tournament.rounds[tournament.rounds.Length - 1];
						}
						base.app.arguments.game.tournamentMatchData = dRLTournamentRoundData.matches[0];
						LoadReplay(text);
					}
				}
				break;
			}
			case "game.race.request-restart":
				if (m_ignoreRestart || m_ignore_replay_click)
				{
					break;
				}
				m_ignoreRestart = true;
				this.TimerRunOnce(delegate
				{
					if (!base.validContext || !base.app.inGame)
					{
						m_ignoreRestart = false;
					}
					else
					{
						view.isReplayLoading = false;
						RestartRace();
					}
				}, 0.5f);
				break;
			case "game.race-overview.restart@click":
				if (m_ignoreRestart || m_ignore_replay_click)
				{
					break;
				}
				m_ignoreRestart = true;
				this.TimerRunOnce(delegate
				{
					if (!base.validContext || !base.app.inGame)
					{
						m_ignoreRestart = false;
					}
					else
					{
						view.isReplayLoading = false;
						RestartRace();
					}
				}, 0.5f);
				break;
			case "game.race.replay-upload@complete":
				RefreshListDelayed();
				break;
			}
		}

		protected void SetErrorFeedback(string p_log)
		{
			Debug.LogWarning("UITournamentLeaderboardsController> " + p_log);
			base.app.view.audio.PlayUIGenericError();
			view.SetFeedback(UITournamentLeaderboardFeedbackType.NoResult, p_hide_list: false);
			this.TimerRunOnce(delegate
			{
				view.SetFeedback(UITournamentLeaderboardFeedbackType.None);
			}, 1f);
		}

		protected void SaveArgs()
		{
			DRLAppArguments.Leaderboards leaderboards = ((base.app.arguments.lastLeaderboard == DRLAppArguments.LeaderboardType.drl) ? base.app.arguments.leaderboardsDRL : ((base.app.arguments.lastLeaderboard == DRLAppArguments.LeaderboardType.open) ? base.app.arguments.leaderboardsOpen : base.app.arguments.leaderboardsCampaign));
			if (leaderboards == null)
			{
				leaderboards = new DRLAppArguments.Leaderboards();
			}
			leaderboards.racePage = view.racePageField.index;
		}

		protected void CancelReplayLoad()
		{
			view.isReplayLoading = false;
			m_ignore_replay_click = false;
			if (m_replay_loader != null)
			{
				m_replay_loader.Cancel();
			}
			if (m_replay_thread != null)
			{
				m_replay_thread.Abort();
			}
		}

		protected void RefreshListDelayed(int page = 0, bool p_is_page_change = false)
		{
			if (!p_is_page_change)
			{
				view.ClearPages();
			}
			if (m_load_timer != null)
			{
				m_load_timer.Stop();
			}
			m_load_timer = this.TimerRunOnce(delegate
			{
				RefreshList(page, p_is_page_change);
			}, 0.6f);
		}

		protected void RefreshList(int page = -1, bool p_is_page_change = false)
		{
			GetLeaderboard(page, p_is_page_change);
		}

		private void RestartRace()
		{
			if (base.app.inGame)
			{
				CancelReplayLoad();
				GameController game = base.app.controller.game;
				if (!(game == null))
				{
					base.enabled = false;
					base.app.view.audio.PlayUIGenericSuccess();
					game.Restart();
				}
			}
		}

		protected void GetLeaderboard(int p_page, bool p_is_page_change = false)
		{
			if (model.guid == null || m_roundId == null)
			{
				return;
			}
			int page = ((p_page >= 0) ? p_page : view.racePageField.index);
			page++;
			if (view.raceListField.Count == 0 || p_is_page_change)
			{
				view.SetFeedback(UITournamentLeaderboardFeedbackType.Loading, p_hide_list: true);
				view.Clear();
			}
			base.app.model.service.GetTournamentResults(model.guid, m_roundId, delegate(DRLTournamentResultData p_result)
			{
				if (base.validContext && view.current)
				{
					if (p_result == null || p_result.leaderboardParams == null || p_result.leaderboardParams.Length == 0)
					{
						Debug.LogWarning("UITournamentLeaderboardsController> Getting result for tournament which appears to be a null");
						view.SetFeedback(UITournamentLeaderboardFeedbackType.NoResult);
						this.TimerRunOnce(delegate
						{
							RefreshList();
						}, failAutoRefreshPeriod);
					}
					else
					{
						base.app.model.service.GetLeaderboard(p_result.leaderboardParams[0], page, pageLength, delegate(DRLLeaderboardResult dRLLeaderboardResult)
						{
							if (base.validContext && view.current)
							{
								if (dRLLeaderboardResult == null || dRLLeaderboardResult.leaderboard == null || dRLLeaderboardResult.leaderboard.Length == 0)
								{
									Debug.LogWarning("UITournamentLeaderboardsController> Getting result for leaderboard which appears to be a null");
									this.TimerRunOnce(delegate
									{
										RefreshList();
									}, failAutoRefreshPeriod);
								}
								OnLeaderboardRacesLoad(dRLLeaderboardResult, p_is_page_change);
								this.TimerRunOnce(delegate
								{
									if (base.validContext && view.current)
									{
										GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
										Debug.Log("UITournamentLeaderboardsController>  GC forced cleanup on tournaments refresh data..");
									}
								}, 0.1f);
							}
						});
					}
				}
			});
		}

		protected void OnLeaderboardRacesLoad(DRLLeaderboardResult p_result, bool p_is_page_change)
		{
			if (!(this == null) && !(view == null))
			{
				if (p_result == null)
				{
					Debug.LogWarning("UITournamentLeaderboardsController> OnLeaderboardRacesLoad - Error Loading the Results!");
					return;
				}
				DRLLeaderboardData[] leaderboard = p_result.leaderboard;
				ListComponent raceListField = view.raceListField;
				DRLPagePickerView racePageField = view.racePageField;
				int p_page = p_result.pagging.page - 1;
				int pageTotal = p_result.pagging.pageTotal;
				PopulateResults(leaderboard, raceListField, racePageField, p_page, pageTotal);
				LayoutRebuilder.ForceRebuildLayoutImmediate(view.standingsRect);
			}
		}

		public void PopulateResults(DRLLeaderboardData[] p_races, ListComponent p_list, DRLPagePickerView p_pages, int p_page, int p_total)
		{
			List<DRLLeaderboardData> list = new List<DRLLeaderboardData>(p_races);
			int index = p_page;
			int num = p_total;
			Debug.Log("UITournamentLeaderboardsController> PopulateResults - page[" + index + "] total[" + num + "] count[" + list.Count + "]");
			UINavigation component = p_pages.GetComponent<UINavigation>();
			p_list.Clear();
			UINavigation uINavigation = (view.backButton.activeInHierarchy ? view.backButtonNav : view.restartButtonNav);
			UINavigation uINavigation2 = (view.nextButton.activeInHierarchy ? view.nextButtonNav : null);
			m_replayNavs.Clear();
			uINavigation.down = component;
			if ((bool)uINavigation2)
			{
				uINavigation.right = uINavigation2;
				uINavigation2.left = uINavigation;
				uINavigation2.down = component;
			}
			for (int i = 0; i < list.Count; i++)
			{
				DRLTournamentRaceEndStandingsItem dRLTournamentRaceEndStandingsItem = p_list.Push<DRLTournamentRaceEndStandingsItem>();
				DRLLeaderboardData dRLLeaderboardData = list[i];
				if (i == 0)
				{
					uINavigation.right = dRLTournamentRaceEndStandingsItem.replayNavigation;
					if ((bool)uINavigation2)
					{
						uINavigation2.left = dRLTournamentRaceEndStandingsItem.replayNavigation;
					}
				}
				m_replayNavs.Add(dRLTournamentRaceEndStandingsItem.replayNavigation);
				dRLTournamentRaceEndStandingsItem.Set(dRLLeaderboardData.position.ToString(), dRLLeaderboardData.profileName, dRLLeaderboardData.crashCount.ToString(), Format.MsToTime(dRLLeaderboardData.score, "m\\:ss\\.fff"), dRLLeaderboardData.profileColor, dRLLeaderboardData.raceStatusFlag, dRLLeaderboardData.replayURL, p_useReplay: true);
			}
			UINavigation.Link(m_replayNavs.ToArray(), 0, p_vertical: true, uINavigation, uINavigation2, null, component);
			bool flag = m_replayNavs.Count > 0;
			component.up = (flag ? m_replayNavs[m_replayNavs.Count - 1] : uINavigation);
			if (!view.isReplayLoading)
			{
				UITournamentLeaderboardFeedbackType feedback = UITournamentLeaderboardFeedbackType.None;
				if (list.Count <= 0)
				{
					feedback = UITournamentLeaderboardFeedbackType.NoResult;
				}
				view.SetFeedback(feedback);
			}
			m_ignore_page_click = true;
			FadeComponent fadeComponent = (p_pages ? p_pages.fade : view.racePageField.fade);
			if (fadeComponent.alpha < 0f)
			{
				fadeComponent.alpha = 0f;
			}
			if (num > 1)
			{
				fadeComponent.FadeIn(0.3f);
			}
			else
			{
				fadeComponent.FadeOut(0.3f);
			}
			p_pages.Set(num);
			p_pages.index = index;
			m_ignore_page_click = false;
		}

		protected void LoadReplay(string p_url)
		{
			view.SetFeedback(UITournamentLeaderboardFeedbackType.Loading);
			bool has_load_start = false;
			view.isReplayLoading = true;
			if (p_url.IndexOf("@") == 0)
			{
				p_url = p_url.Substring(1);
				if (!File.Exists(p_url))
				{
					m_ignore_replay_click = false;
					base.app.view.audio.PlayUIGenericError();
					view.SetFeedback(UITournamentLeaderboardFeedbackType.None);
					Debug.LogWarning("UITournamentLeaderboardsController> Failed to load replay from local files [" + p_url + "]");
					return;
				}
				RunOnce(0.3f, delegate
				{
					has_load_start = true;
					base.app.view.audio.PlayUIGenericSuccess();
					view.progress = 0.5f;
					byte[] array = File.ReadAllBytes(p_url);
					if (array != null)
					{
						LoadReplay(array);
					}
				});
				return;
			}
			m_replay_loader = Web.Get(p_url, delegate(byte[] p_result, float p_progress, WebAsyncRequest p_request)
			{
				if (p_progress >= 1f && p_result == null)
				{
					m_ignore_replay_click = false;
					base.app.view.audio.PlayUIGenericError();
					view.SetFeedback(UITournamentLeaderboardFeedbackType.None);
					Debug.LogWarning("UITournamentLeaderboardsController> Failed to load replay from [" + p_url + "]");
				}
				else
				{
					if (!has_load_start)
					{
						has_load_start = true;
						base.app.view.audio.PlayUIGenericSuccess();
					}
					if (p_progress < 1f)
					{
						view.progress = p_progress * 0.5f;
					}
					else
					{
						view.progress = 0.5f;
						if (p_result != null)
						{
							LoadReplay(p_result);
						}
					}
				}
			});
		}

		public void LoadReplay(byte[] p_data)
		{
			float thread_progress = 0.5f;
			bool thread_complete = false;
			m_replay_thread = new Thread((ThreadStart)delegate
			{
				BlackboxRecord blackboxRecord = null;
				ReplayFile replayFile = null;
				if (ReplayFile.EnableVersion2)
				{
					replayFile = ReplayFile.FromBytes(p_data);
				}
				else
				{
					blackboxRecord = Serialize.FromBytes<BlackboxRecord>(p_data, p_unsafe: true);
					blackboxRecord.Decompress(delegate(float f)
					{
						this.TimerRunOnce(delegate
						{
							thread_progress += f / 2f;
							view.progress = thread_progress;
						}, 1f / 60f);
					});
				}
				object rpl_ref = (ReplayFile.EnableVersion2 ? ((object)replayFile) : ((object)blackboxRecord));
				thread_complete = true;
				this.TimerRunOnce(delegate
				{
					view.progress = 1f;
				}, 1f / 30f);
				this.TimerRunOnce(delegate
				{
					if (base.validContext)
					{
						view.isReplayLoading = false;
						Notify("leaderboards.replay.load@complete");
						base.app.view.ui.fade.FadeIn(1.5f);
						this.TimerRunOnce(delegate
						{
							if (base.validContext)
							{
								Debug.Log("UITournamentLeaderboardsController> Load Complete success[" + (rpl_ref != null) + "]");
								if (rpl_ref == null)
								{
									m_ignore_replay_click = false;
									base.app.view.audio.PlayUIGenericError();
									view.SetFeedback(UITournamentLeaderboardFeedbackType.None);
									base.app.view.ui.fade.FadeOut();
									Debug.LogWarning("UITournamentLeaderboardsController> Failed to load replay");
								}
								else
								{
									DRLTournamentData dRLTournamentData = tournament;
									DRLTournamentRoundData dRLTournamentRoundData = (view.openedFromTheBrackets ? view.round : model.activeRound);
									if (dRLTournamentData != null && dRLTournamentRoundData != null)
									{
										string text = (dRLTournamentRoundData.isCustomMap ? dRLTournamentRoundData.customMapId : "");
										string mapId = dRLTournamentRoundData.mapId;
										string trackId = dRLTournamentRoundData.trackId;
										Debug.LogWarning("UITournamentLeaderboardsController> Loading Replay Scene / map[" + mapId + "] track[" + trackId + "] custom-map[" + text + "] round[" + dRLTournamentRoundData.title + "]");
										base.app.arguments.game.tournamentData = dRLTournamentData;
										base.app.arguments.game.tournamentMatchData = dRLTournamentRoundData.matches[0];
										base.app.arguments.game.isFromBrackets = view.openedFromTheBrackets;
										base.app.scene.Load(rpl_ref, mapId, trackId, text);
									}
								}
							}
						}, 1f);
					}
				});
			});
			m_replay_thread.Start();
			((Component)this).ActivityRun((Func<bool>)delegate
			{
				if (!base.validContext)
				{
					return false;
				}
				if (!m_ignore_replay_click)
				{
					return false;
				}
				if (thread_complete)
				{
					return false;
				}
				float num = Mathf.Lerp(0.05f, 0.001f, Mathf.Clamp01((thread_progress - 0.5f) / 0.5f));
				thread_progress += Time.deltaTime * 0.5f * num;
				thread_progress = Mathf.Clamp(thread_progress, 0f, 0.95f);
				view.progress = thread_progress;
				return true;
			}, 0f);
		}

		private void StartSyncData()
		{
			StopSyncData();
			m_syncActivity = ((Component)this).ActivityRun((Func<bool>)delegate
			{
				if (m_syncTimer >= m_syncDuration)
				{
					RefreshList();
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

		private void OnDisable()
		{
			StopSyncData();
		}
	}
}

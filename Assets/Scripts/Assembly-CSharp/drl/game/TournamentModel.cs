using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class TournamentModel : Model<DRLApp>
	{
		[Tooltip("This property controls how often we refresh data from backend (in seconds).")]
		public float dataSyncFrequency = 10f;

		private Activity m_syncActivity;

		[Tooltip("This property controls how often we check photon network connectivity.")]
		public float networkRefreshFrequency = 3f;

		private Dictionary<string, List<DRLTournamentReplayData>> m_matchReplays;

		private Activity m_replayProcessingTimer;

		private string m_lastMatchId = "";

		private Activity m_countdownTimerActivity;

		private float m_countdownTimer;

		private float m_backendRoundTimer;

		private float m_syncTimer;

		public DRLTournamentData tournament { get; private set; }

		public string guid { get; private set; }

		public List<DRLTournamentRoundData> rounds { get; private set; }

		public List<DRLTournamentMatchData> matches { get; private set; }

		public DRLTournamentRoundData activeRound { get; private set; }

		public DRLTournamentRoundData lastRound { get; private set; }

		public DRLTournamentMatchData activeMatch { get; private set; }

		public Dictionary<string, List<DRLTournamentReplayData>> matchReplays
		{
			get
			{
				return m_matchReplays ?? (m_matchReplays = new Dictionary<string, List<DRLTournamentReplayData>>());
			}
			private set
			{
				m_matchReplays = value;
			}
		}

		public bool isRacer { get; private set; }

		public bool isSpectator => !isRacer;

		private bool isDeveloper => base.app.model.storage.state.player.profile.isDeveloper;

		private string playerId => base.app.model.storage.state.player.profile.playerId;

		public bool isTournamentActive
		{
			get
			{
				if (tournament == null)
				{
					return false;
				}
				if (tournament.status != TournamentState.active)
				{
					return tournament.status == TournamentState.idle;
				}
				return true;
			}
		}

		public bool isPastTournament { get; set; }

		public bool isRoundActive
		{
			get
			{
				if (activeRound != null)
				{
					return activeRound.state == TournamentRoundState.active;
				}
				return false;
			}
		}

		public bool isMatchActive
		{
			get
			{
				if (activeMatch != null)
				{
					return activeMatch.state == TournamentMatchState.active;
				}
				return false;
			}
		}

		public bool replaysProcessing { get; private set; }

		public float roundCountdown { get; protected set; }

		public void RefreshData(Action callback = null)
		{
			if (string.IsNullOrEmpty(guid))
			{
				Notify("tournament.action.refresh");
			}
			else
			{
				if (!base.validContext || base.app == null || base.app.model == null || base.app.model.service == null)
				{
					return;
				}
				base.app.model.service.GetTournament(guid, delegate(DRLTournamentResult p_result)
				{
					if (rounds == null)
					{
						rounds = new List<DRLTournamentRoundData>();
					}
					if (matches == null)
					{
						matches = new List<DRLTournamentMatchData>();
					}
					if (base.validContext && p_result != null && p_result.tournaments.Length != 0)
					{
						tournament = p_result.tournaments[0];
						if (tournament.rounds == null || tournament.rounds.Length == 0)
						{
							Debug.LogWarning("TournamentModel> No active rounds found for this tournament - " + guid);
						}
						else
						{
							rounds.Clear();
							matches.Clear();
							if (tournament.status != TournamentState.active || tournament.status != TournamentState.complete)
							{
								m_lastMatchId = "";
							}
							else if (base.app.model.network.room != null)
							{
								m_lastMatchId = base.app.model.network.room.MatchId;
							}
							for (int i = 0; i < tournament.rounds.Length; i++)
							{
								DRLTournamentRoundData dRLTournamentRoundData = tournament.rounds[i];
								if (dRLTournamentRoundData == null || dRLTournamentRoundData.matches == null)
								{
									Debug.LogWarning("TournamentModel> Round data is invalid - tournament guid: " + tournament.guid);
								}
								else
								{
									rounds.Add(tournament.rounds[i]);
									for (int j = 0; j < dRLTournamentRoundData.matches.Length; j++)
									{
										DRLTournamentMatchData dRLTournamentMatchData = dRLTournamentRoundData.matches[j];
										if (dRLTournamentMatchData == null)
										{
											Debug.LogWarning("TournamentModel> Match data is invalid - round index: " + dRLTournamentRoundData.index);
										}
										else
										{
											dRLTournamentMatchData.index = j;
											matches.Add(dRLTournamentMatchData);
											if (matchReplays.ContainsKey(dRLTournamentMatchData.Id))
											{
												for (int k = 0; k < dRLTournamentMatchData.replayURLs.Length; k++)
												{
													int num = dRLTournamentMatchData.replayURLs[k].heat - 1;
													if (num >= 0)
													{
														if (matchReplays[dRLTournamentMatchData.Id] == null)
														{
															matchReplays[dRLTournamentMatchData.Id] = new List<DRLTournamentReplayData>();
														}
														if (num >= matchReplays[dRLTournamentMatchData.Id].Count)
														{
															matchReplays[dRLTournamentMatchData.Id].Add(dRLTournamentMatchData.replayURLs[k]);
														}
														else
														{
															string uRLs = matchReplays[dRLTournamentMatchData.Id][num].URLs;
															string uRLs2 = dRLTournamentMatchData.replayURLs[k].URLs;
															int num2 = uRLs.Trim(';').Split(';').Length;
															int num3 = uRLs2.Trim(';').Split(';').Length;
															if (num3 >= num2)
															{
																matchReplays[dRLTournamentMatchData.Id][num].Copy(dRLTournamentMatchData.replayURLs[k]);
																if (base.app.model.network.room == null)
																{
																	matchReplays[dRLTournamentMatchData.Id][num].replaysReady = true;
																}
															}
															int num4 = Mathf.Max(num2, num3);
															if (base.app.model.network.room != null && num4 >= base.app.model.network.room.Racers.Count)
															{
																matchReplays[dRLTournamentMatchData.Id][num].replaysReady = true;
															}
														}
													}
												}
											}
											else
											{
												matchReplays.Add(dRLTournamentMatchData.Id, dRLTournamentMatchData.replayURLs.ToList());
											}
										}
									}
								}
							}
							activeRound = tournament.GetActiveRound();
							lastRound = tournament.GetLastRound();
							if (activeRound != null && activeRound.matches != null)
							{
								for (int l = 0; l < activeRound.matches.Length; l++)
								{
									DRLTournamentMatchData dRLTournamentMatchData2 = activeRound.matches[l];
									if (dRLTournamentMatchData2 == null)
									{
										Debug.LogWarning("TournamentModel> Match data is invalid - round index: " + activeRound.index);
									}
									else if (dRLTournamentMatchData2.ContainsPlayer(playerId))
									{
										activeMatch = dRLTournamentMatchData2;
										activeMatch.index = l;
										break;
									}
								}
							}
							UpdateCountdownTimer();
							Notify("tournament.action.refresh", tournament);
							callback?.Invoke();
						}
					}
				});
			}
		}

		public void SetTournamentData(string p_guid, Action callback = null)
		{
			ClearTournamentData();
			guid = p_guid;
			RefreshData(callback);
			StartAutoSync();
		}

		public void SetTournamentData(DRLTournamentData p_data)
		{
			if (p_data != null)
			{
				ClearTournamentData();
				guid = p_data.guid;
				tournament = p_data;
				rounds = tournament.rounds.ToList();
				activeRound = tournament.GetActiveRound();
				RefreshData();
				StartAutoSync();
			}
		}

		public void ClearTournamentData()
		{
			guid = null;
			tournament = null;
			matches?.Clear();
			rounds?.Clear();
			matchReplays?.Clear();
			matches = null;
			rounds = null;
			matchReplays = null;
			activeRound = null;
			activeMatch = null;
			isRacer = false;
			isPastTournament = false;
			StopAutoSync();
		}

		public void StartAutoSync()
		{
			m_syncTimer = 0f;
			StopAutoSync();
			m_syncActivity = ((Component)this).ActivityRun((Func<bool>)delegate
			{
				if (m_syncTimer >= dataSyncFrequency)
				{
					m_syncTimer = 0f;
					RefreshData();
				}
				m_syncTimer += Time.deltaTime;
				return true;
			}, 0f);
		}

		public void StopAutoSync()
		{
			if (m_syncActivity != null)
			{
				m_syncActivity.Stop();
				m_syncActivity = null;
			}
		}

		private void UpdateCountdownTimer()
		{
			StopCountdownTimer();
			if (!isRoundActive || activeRound.matches.Length == 0)
			{
				Notify("tournametn.timer.update", -1f);
			}
			else
			{
				m_backendRoundTimer = (float)(activeRound.matches[0].completeDate - activeRound.matches[0].currentTime).TotalSeconds;
				CountdownTimerStep();
			}
		}

		private void CountdownTimerStep()
		{
			Notify("tournametn.timer.update", m_backendRoundTimer - m_countdownTimer);
			m_countdownTimer += 1f;
			m_countdownTimerActivity = this.TimerRunOnce(CountdownTimerStep, 1f);
		}

		private void StopCountdownTimer()
		{
			m_countdownTimer = 0f;
			m_backendRoundTimer = 0f;
			if (m_countdownTimerActivity != null)
			{
				m_countdownTimerActivity.Stop();
				m_countdownTimerActivity = null;
			}
		}

		public void RefreshMatchData(string p_guid = null, Action<DRLTournamentMatchData> p_callback = null)
		{
			string id = ((!string.IsNullOrEmpty(p_guid)) ? p_guid : activeMatch?.Id);
			if (string.IsNullOrEmpty(id))
			{
				p_callback?.Invoke(null);
				Debug.LogWarning("TournamentModel> Couldn't fetch match data - invalid guid!");
				return;
			}
			base.app.model.service.GetMatch(guid, id, delegate(DRLTournamentMatchResult p_result)
			{
				if (!base.validContext || p_result == null || p_result.matches.Length == 0)
				{
					Debug.LogWarning("TournamentModel> Failed to fetch tournament match data for - match id: " + id);
					p_callback?.Invoke(null);
				}
				else
				{
					p_callback?.Invoke(p_result.matches[0]);
				}
			});
		}

		public bool IsRacerInMatch(DRLTournamentMatchData p_match)
		{
			if (activeMatch == null || p_match == null)
			{
				return false;
			}
			return p_match.Id == activeMatch.Id;
		}

		public bool IsRacerInMatch(string p_guid)
		{
			if (activeMatch == null || string.IsNullOrEmpty(p_guid))
			{
				return false;
			}
			return activeMatch.Id == p_guid;
		}

		public bool IsRegistered()
		{
			return tournament.IsPlayerRegistered(playerId);
		}

		public bool CanJoin()
		{
			if (activeRound == null || activeMatch == null || !isRoundActive || !isMatchActive)
			{
				return false;
			}
			if (activeRound.gameMode == TournamentRoundGameMode.leaderboard)
			{
				return activeMatch.remainingTime.TotalSeconds > 25.0;
			}
			return activeMatch.state == TournamentMatchState.active;
		}

		public bool CanSpectate()
		{
			if (!IsRegistered() || !isRacer)
			{
				if (tournament.disablePublicSpectators)
				{
					return isDeveloper;
				}
				return true;
			}
			return false;
		}

		public DRLTournamentMatchData GetMatchById(string p_matchId)
		{
			if (string.IsNullOrEmpty(p_matchId))
			{
				return null;
			}
			if (matches == null || matches.Count == 0)
			{
				return null;
			}
			return matches.Find((DRLTournamentMatchData o) => o.Id == p_matchId);
		}

		public TournamentProgression GetTournamentProgressionType()
		{
			if (tournament == null)
			{
				return TournamentProgression.auto;
			}
			return tournament.progression;
		}

		public void AddMatchReplays(string p_mid, int p_heatIdx, string p_replayURLs)
		{
			if (string.IsNullOrEmpty(p_mid) || string.IsNullOrEmpty(p_replayURLs))
			{
				return;
			}
			DRLTournamentReplayData dRLTournamentReplayData = new DRLTournamentReplayData(p_heatIdx, p_replayURLs);
			Debug.Log("TournamentModel> Replays Incoming: " + p_replayURLs);
			if (matchReplays.ContainsKey(p_mid))
			{
				if (matchReplays[p_mid] == null)
				{
					matchReplays[p_mid] = new List<DRLTournamentReplayData>();
				}
				List<DRLTournamentReplayData> list = matchReplays[p_mid];
				Predicate<DRLTournamentReplayData> match = (DRLTournamentReplayData o) => o.heat == p_heatIdx;
				DRLTournamentReplayData dRLTournamentReplayData2 = list.Find(match);
				if (dRLTournamentReplayData2 == null)
				{
					list.Add(dRLTournamentReplayData);
				}
				else
				{
					dRLTournamentReplayData2.Copy(dRLTournamentReplayData);
				}
			}
			else
			{
				List<DRLTournamentReplayData> list2 = new List<DRLTournamentReplayData>();
				list2.Add(dRLTournamentReplayData);
				matchReplays.Add(p_mid, list2);
			}
		}

		public void ClearMatchReplays(string p_mid)
		{
			if (!string.IsNullOrEmpty(p_mid) && matchReplays.ContainsKey(p_mid))
			{
				matchReplays[p_mid].Clear();
				matchReplays[p_mid] = null;
				matchReplays.Remove(p_mid);
			}
		}

		public void ClearHeatReplays(string p_mid, int p_heatIdx)
		{
			if (!string.IsNullOrEmpty(p_mid) && p_heatIdx > 0 && matchReplays.ContainsKey(p_mid))
			{
				int num = matchReplays[p_mid].FindIndex((DRLTournamentReplayData o) => o.heat == p_heatIdx);
				if (num >= 0 && num < matchReplays[p_mid].Count)
				{
					matchReplays[p_mid].RemoveAt(num);
				}
			}
		}

		public string[] FetchMatchHeatReplays(string p_mid, int p_heatIdx)
		{
			if (matchReplays == null || !matchReplays.ContainsKey(p_mid))
			{
				return null;
			}
			List<DRLTournamentReplayData> list = matchReplays[p_mid];
			Predicate<DRLTournamentReplayData> match = (DRLTournamentReplayData o) => o.heat == p_heatIdx + 1;
			return list.Find(match)?.GetHeatReplays();
		}

		private DRLTournamentReplayData FetchHeatReplay(string p_mid, int p_heatIdx)
		{
			if (matchReplays == null || !matchReplays.ContainsKey(p_mid))
			{
				return null;
			}
			List<DRLTournamentReplayData> list = matchReplays[p_mid];
			Predicate<DRLTournamentReplayData> match = (DRLTournamentReplayData o) => o.heat == p_heatIdx + 1;
			return list.Find(match);
		}

		public List<DRLTournamentReplayData> FetchMatchReplays(string p_mid)
		{
			if (matchReplays == null || !matchReplays.ContainsKey(p_mid))
			{
				return null;
			}
			for (int i = 0; i < matchReplays[p_mid].Count; i++)
			{
				if (matchReplays[p_mid][i] != null)
				{
					D.Log("HEAT: " + matchReplays[p_mid][i].heat + "\nREPLAYS: " + string.Join("\n", matchReplays[p_mid][i].URLs + "\n READY: " + matchReplays[p_mid][i].replaysReady));
				}
			}
			return matchReplays[p_mid];
		}

		public int GetMatchReplayCount(string p_mid)
		{
			if (matchReplays == null || !matchReplays.ContainsKey(p_mid))
			{
				return 0;
			}
			if (matchReplays[p_mid] == null)
			{
				return 0;
			}
			return matchReplays[p_mid].Count;
		}

		private void SetReplayReady(string p_matchId, int p_heatId)
		{
			DRLTournamentReplayData dRLTournamentReplayData = FetchHeatReplay(p_matchId, p_heatId);
			if (dRLTournamentReplayData != null)
			{
				dRLTournamentReplayData.replaysReady = true;
			}
		}

		public void StartReplayProcessing()
		{
			if (!base.app.inMultiplayer || !base.app.inTournament || !base.app.inGame || base.app.controller.game.networkRace == null)
			{
				replaysProcessing = false;
				return;
			}
			if (base.app.controller.game.networkRace as NetworkRaceController == null)
			{
				replaysProcessing = false;
				return;
			}
			replaysProcessing = true;
			m_replayProcessingTimer?.Stop();
			m_replayProcessingTimer = this.TimerRunOnce(delegate
			{
				replaysProcessing = false;
				if (activeMatch != null)
				{
					SetReplayReady(activeMatch.Id, activeMatch.activeHeat);
				}
			}, 60f);
		}

		public void StopReplayProcessing()
		{
			replaysProcessing = false;
			m_replayProcessingTimer?.Stop();
		}

		public string GetLastMatchID()
		{
			return m_lastMatchId;
		}

		private void OnDisable()
		{
			ClearTournamentData();
		}

		public void OnPersistency()
		{
			base.app.model.tournament = this;
		}
	}
}

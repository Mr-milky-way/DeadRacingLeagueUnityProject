using System;
using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using drl.network;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class TournamentController : Controller<DRLApp>
	{
		private float m_networkSyncTimer;

		private Activity m_connectionActivity;

		private Dictionary<int, string> m_replayURLs = new Dictionary<int, string>();

		public TournamentModel model => AssertLocal<TournamentModel>("model");

		public DRLTournamentData tournament => model.tournament;

		private NetworkModel network => base.app.model.network;

		private float networkSyncFrequency => model.networkRefreshFrequency;

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "tournament.brackets.open":
				StartNetworkConnection();
				model.StartAutoSync();
				StartSocketConnection();
				break;
			case "tournament.brackets.close":
				StopNetworkConnection();
				break;
			case "tournament.exited":
				StopSocketConnection();
				StopNetworkConnection();
				model.ClearTournamentData();
				break;
			case "tournament.refresh.data":
				if (!base.app.inGame || ((!base.app.inMultiplayer || !(base.app.controller.game.networkRace != null) || base.app.controller.game.networkRace.model.status != RaceStatusType.Running) && (!(base.app.controller.game.race != null) || base.app.controller.game.race.model.status != RaceStatusType.Running)))
				{
					model.RefreshData();
				}
				break;
			case "tournament.model.reset":
				if (p_data.Length != 0 && p_data[0] is DRLTournamentData tournamentData)
				{
					model.SetTournamentData(tournamentData);
				}
				break;
			case "game.race.enabled":
				model.StopAutoSync();
				m_replayURLs.Clear();
				model.StopReplayProcessing();
				break;
			case "tournament.action.reset-heat":
			case "tournament.action.reset-match":
			case "network.race.end":
			case "game.race.complete":
			{
				model.StartAutoSync();
				if (p_data.Length == 0)
				{
					break;
				}
				string text4 = p_data[0] as string;
				if (string.IsNullOrEmpty(text4))
				{
					break;
				}
				if (p_event == "tournament.action.reset-match")
				{
					model.ClearMatchReplays(text4);
					model.StopReplayProcessing();
					m_replayURLs.Clear();
				}
				if (base.app.model.network.room != null)
				{
					if (p_event == "tournament.action.reset-heat")
					{
						model.ClearHeatReplays(text4, base.app.model.network.room.HeatIdx);
						model.StopReplayProcessing();
						m_replayURLs.Clear();
					}
					else
					{
						model.StartReplayProcessing();
					}
				}
				break;
			}
			case "tournament.action.start-match":
				m_replayURLs.Clear();
				model.StopReplayProcessing();
				break;
			case "network.race.replay.ready.all":
				model.StopReplayProcessing();
				break;
			case "tournament.replay.incoming":
			{
				if (p_data.Length < 4)
				{
					break;
				}
				string text = p_data[0] as string;
				int p_heatIdx = (int)p_data[1];
				int key = (int)p_data[2];
				string text2 = p_data[3] as string;
				if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text2))
				{
					Debug.Log("TournamentController> Replay incoming: missing match id or replay URL.");
				}
				else
				{
					if (base.app.model.network.room == null)
					{
						break;
					}
					Debug.Log("TournamentController> Replay Incoming: match ID[" + text + "] heat IDX[" + p_heatIdx + "] sender ID[" + key + "] replay url[" + text2 + "]");
					if (m_replayURLs.ContainsKey(key))
					{
						m_replayURLs[key] = text2;
					}
					else
					{
						m_replayURLs.Add(key, text2);
					}
					bool num = base.app.model.network.room.Racers.Count <= m_replayURLs.Count;
					string text3 = "";
					if (!num)
					{
						break;
					}
					foreach (KeyValuePair<int, string> replayURL in m_replayURLs)
					{
						text3 = text3 + replayURL.Value + ";";
					}
					model.AddMatchReplays(text, p_heatIdx, text3);
				}
				break;
			}
			}
		}

		public void StartNetworkConnection()
		{
			Notify("network.footer@enable");
			if (network.connectionState == PhotonService.ServiceState.InRoom)
			{
				network.LeaveRoom();
			}
			m_networkSyncTimer = 0f;
			m_connectionActivity = ((Component)this).ActivityRun((Func<bool>)delegate
			{
				if (m_networkSyncTimer > 3f)
				{
					if (!base.validContext)
					{
						return false;
					}
					Notify("tournament.refresh.lobby");
					ConnectToTournamentLobby();
					m_networkSyncTimer = 0f;
				}
				m_networkSyncTimer += Time.deltaTime;
				return true;
			}, 0f);
		}

		public void StopNetworkConnection()
		{
			if (m_connectionActivity != null)
			{
				m_connectionActivity.Stop();
				m_connectionActivity = null;
			}
		}

		public void StartSocketConnection()
		{
			ServiceModel service = base.app.model.service;
			if (!(service == null) && !service.tournamentSocket.IsConnected())
			{
				service.WatchTournamentRefresh();
			}
		}

		public void StopSocketConnection()
		{
			if ((bool)base.app.model.service)
			{
				base.app.model.service.StopTournamentRefresh();
			}
		}

		private void ConnectToTournamentLobby()
		{
			if (tournament == null)
			{
				Debug.LogWarning("TournamentModel> Couldn't connect to tournament lobby - no tournament data present.");
			}
			else if (network.connectionState == PhotonService.ServiceState.Disconnected || !(network.LobbyId == tournament.guid))
			{
				network.ConnectToTournamentLobby(tournament);
			}
		}

		public void OnPersistency()
		{
			base.app.controller.tournament = this;
		}
	}
}

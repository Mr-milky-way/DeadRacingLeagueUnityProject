using System;
using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UITournamentDAWCController : Controller<DRLApp>
	{
		private float m_syncDuration = 15f;

		private float m_syncTimer = 15f;

		private Activity m_syncActivity;

		public UITournamentDAWCView view => AssertLocal<UITournamentDAWCView>("view");

		private void OnEnable()
		{
			view.SetFeedback(UITournamentLeaderboardFeedbackType.Loading);
			StartSyncData();
			view.Clear();
		}

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (view.current)
			{
				switch (p_event)
				{
				case "ui.screen@open":
					view.SetFeedback(UITournamentLeaderboardFeedbackType.Loading);
					view.StartVideo();
					StartSyncData();
					view.Clear();
					break;
				case "ui.screen@close":
					StopSyncData();
					view.StopVideo();
					view.Clear();
					break;
				case "ui.screen.return@click":
					StopSyncData();
					base.app.view.ui.screens.Return();
					break;
				}
			}
		}

		private void RefreshData()
		{
			if (!view.current)
			{
				return;
			}
			DRLTournamentData td = base.app.tournament;
			if (td == null)
			{
				Debug.LogWarning("UITournamentDAWCController> Can't display results - no tournament data.");
				view.SetFeedback(UITournamentLeaderboardFeedbackType.NoResult);
				return;
			}
			base.app.model.service.GetTournament(td.guid, delegate(DRLTournamentResult result)
			{
				if (!base.validContext || result == null || result.tournaments.Length == 0 || !result.tournaments[0].isDAWC)
				{
					Debug.LogWarning("UITournamentDAWCController> Can't display results - no tournament data.");
					view.SetFeedback(UITournamentLeaderboardFeedbackType.NoResult);
				}
				else
				{
					td = result.tournaments[0];
					if (td.rounds.Length == 0)
					{
						Debug.LogWarning("UITournamentDAWCController> Can't display results - no round data.");
						view.SetFeedback(UITournamentLeaderboardFeedbackType.NoResult);
					}
					else
					{
						base.app.model.service.GetTournamentPlacements(td.guid, delegate(DRLTournamentPlacementsData p_results)
						{
							if (p_results == null || p_results.semi1 == null || p_results.semi1.Length == 0)
							{
								Debug.LogWarning("UITournamentDAWCController> Can't display results - no placements data.");
								view.SetFeedback(UITournamentLeaderboardFeedbackType.NoResult);
							}
							else
							{
								view.Set(p_results);
								view.SetActiveRoundtitle(p_results.activeRound);
							}
						});
					}
				}
			});
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

		private void OnDisable()
		{
			StopSyncData();
			view.StopVideo();
		}
	}
}

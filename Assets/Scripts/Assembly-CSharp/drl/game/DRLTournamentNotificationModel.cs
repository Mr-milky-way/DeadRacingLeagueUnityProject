using System;
using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using thelab.mvc;

namespace drl.game
{
	public class DRLTournamentNotificationModel : Model<DRLApp>
	{
		[Tooltip("Refresh state frequency in seconds.")]
		[SerializeField]
		private float m_refreshFrequency;

		[Tooltip("Refresh state frequency in seconds.")]
		[SerializeField]
		private float m_refreshNotificationsFrequency;

		[Tooltip("Defines a time in minutes when users will be notified of soon to start tournament.")]
		[Range(1f, 30f)]
		[SerializeField]
		private int m_soonToStart;

		[Tooltip("Set to true if users should get notified of active tournamets on app start.")]
		[SerializeField]
		private bool m_notifyOnStart;

		private bool m_initialized;

		public List<TournamentNotificationState> tournamentStates = new List<TournamentNotificationState>();

		public DRLNotificationModel notifications => AssertParent<DRLNotificationModel>("notifications");

		public float RefreshFrequency => m_refreshFrequency;

		public float RefreshNotificationsFrequency => m_refreshNotificationsFrequency;

		public int SoonToStartPeriod => m_soonToStart;

		public bool NotifyOnStart => m_notifyOnStart;

		internal void UpdateTournamentStates(DRLTournamentData[] p_tournaments)
		{
			if (!base.validContext || p_tournaments == null)
			{
				return;
			}
			foreach (DRLTournamentData dRLTournamentData in p_tournaments)
			{
				bool flag = true;
				foreach (TournamentNotificationState tournamentState in tournamentStates)
				{
					if (tournamentState.guid == dRLTournamentData.guid)
					{
						UpdateState(tournamentState, dRLTournamentData);
						flag = false;
						break;
					}
				}
				if (flag)
				{
					CacheNewState(dRLTournamentData);
				}
			}
			m_initialized = true;
		}

		private void CacheNewState(DRLTournamentData p_data)
		{
			TournamentState status = p_data.status;
			tournamentStates.Add(new TournamentNotificationState(p_data.guid, p_data.status));
			switch (status)
			{
			case TournamentState.idle:
				if (m_initialized)
				{
					Notify("notifications.tournament.opened", p_data);
				}
				break;
			case TournamentState.active:
				if (m_initialized || NotifyOnStart)
				{
					Notify("notifications.tournament.started", p_data);
				}
				break;
			}
		}

		private void UpdateState(TournamentNotificationState p_tns, DRLTournamentData p_data)
		{
			string playerId = base.app.model.storage.state.player.profile.playerId;
			if (p_tns.state == TournamentState.idle && p_data.status == TournamentState.active && p_data.IsPlayerRegistered(playerId))
			{
				Notify("notifications.tournament.started", p_data);
			}
			if (p_tns.state != TournamentState.idle && p_data.status == TournamentState.idle && !p_data.IsPlayerRegistered(playerId))
			{
				Notify("notifications.tournament.opened", p_data);
			}
			if (p_data.status == TournamentState.idle)
			{
				float num = (float)(p_data.registerEndDate - DateTime.UtcNow).TotalMinutes;
				if (num > 0f && num < (float)SoonToStartPeriod && !p_tns.soonToStartNotified)
				{
					Notify("notifications.tournament.soon-to-start", p_data);
					p_tns.soonToStartNotified = true;
				}
			}
			if (p_data.status != TournamentState.idle)
			{
				p_tns.soonToStartNotified = false;
			}
			p_tns.state = p_data.status;
		}
	}
}

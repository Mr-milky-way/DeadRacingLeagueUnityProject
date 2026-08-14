using System;
using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLTournamentNotificationController : Controller<DRLApp>
	{
		private Activity m_syncTimer;

		public DRLTournamentNotificationModel model => AssertLocal<DRLTournamentNotificationModel>("model");

		public DRLNotificationController notifications => AssertParent<DRLNotificationController>("notifications");

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "boot@complete":
				StartSyncData();
				break;
			case "notifications.tournament.opened":
				if (p_data.Length != 0 && p_data[0] is DRLTournamentData dRLTournamentData2)
				{
					TournamentNotificationData tournamentNotificationData2 = new TournamentNotificationData();
					tournamentNotificationData2.tournamentGuid = dRLTournamentData2.guid;
					tournamentNotificationData2.tournamentTitle = dRLTournamentData2.title;
					tournamentNotificationData2.tournamentDescription = dRLTournamentData2.description;
					tournamentNotificationData2.isPrivate = dRLTournamentData2.isPrivate;
					tournamentNotificationData2.type = NotificationTypeFlag.Tournament;
					tournamentNotificationData2.status = TournamentNotificationType.Opened;
					tournamentNotificationData2.tournamentThumbnailURL = dRLTournamentData2.imageURL;
					tournamentNotificationData2.isParticipant = IsRegistered(dRLTournamentData2);
					Notify("notifications.receive", tournamentNotificationData2);
				}
				break;
			case "notifications.tournament.soon-to-start":
				if (p_data.Length != 0 && p_data[0] is DRLTournamentData dRLTournamentData3)
				{
					TournamentNotificationData tournamentNotificationData3 = new TournamentNotificationData();
					tournamentNotificationData3.tournamentGuid = dRLTournamentData3.guid;
					tournamentNotificationData3.tournamentTitle = dRLTournamentData3.title;
					tournamentNotificationData3.isPrivate = dRLTournamentData3.isPrivate;
					tournamentNotificationData3.tournamentDescription = dRLTournamentData3.description;
					tournamentNotificationData3.type = NotificationTypeFlag.Tournament;
					tournamentNotificationData3.status = TournamentNotificationType.SoonToStart;
					tournamentNotificationData3.tournamentThumbnailURL = dRLTournamentData3.imageURL;
					tournamentNotificationData3.isParticipant = IsRegistered(dRLTournamentData3);
					Notify("notifications.receive", tournamentNotificationData3);
				}
				break;
			case "notifications.tournament.started":
				if (p_data.Length != 0 && p_data[0] is DRLTournamentData dRLTournamentData)
				{
					TournamentNotificationData tournamentNotificationData = new TournamentNotificationData();
					tournamentNotificationData.tournamentGuid = dRLTournamentData.guid;
					tournamentNotificationData.tournamentTitle = dRLTournamentData.title;
					tournamentNotificationData.tournamentDescription = dRLTournamentData.description;
					tournamentNotificationData.isPrivate = dRLTournamentData.isPrivate;
					tournamentNotificationData.type = NotificationTypeFlag.Tournament;
					tournamentNotificationData.status = TournamentNotificationType.Started;
					tournamentNotificationData.tournamentThumbnailURL = dRLTournamentData.imageURL;
					tournamentNotificationData.isParticipant = IsRegistered(dRLTournamentData);
					Notify("notifications.receive", tournamentNotificationData);
				}
				break;
			}
		}

		private bool IsRegistered(DRLTournamentData td)
		{
			string playerId = base.app.model.storage.state.player.profile.playerId;
			if (td.playerIds == null)
			{
				return false;
			}
			for (int i = 0; i < td.playerIds.Length; i++)
			{
				if (td.playerIds[i] == playerId)
				{
					return true;
				}
			}
			return false;
		}

		private void StartSyncData()
		{
			if (m_syncTimer != null)
			{
				StopSyncData();
			}
			float timerState = 0f;
			float timerNotifications = 0f;
			SyncData();
			m_syncTimer = Activity.Run((Func<bool>)delegate
			{
				if (!model)
				{
					return false;
				}
				timerState += Time.deltaTime;
				timerNotifications += Time.deltaTime;
				if (timerState > model.RefreshFrequency)
				{
					timerState = 0f;
					SyncData();
				}
				if (timerNotifications > model.RefreshNotificationsFrequency)
				{
					timerNotifications = 0f;
					SyncNotifications();
				}
				return true;
			}, 0f, false);
		}

		private void StopSyncData()
		{
			if (m_syncTimer != null)
			{
				m_syncTimer.Stop();
				m_syncTimer = null;
			}
		}

		private void SyncData()
		{
			if (model.notifications.isSceneLoading || base.app.inTournament)
			{
				return;
			}
			base.app.model.service.GetTournaments(0, delegate(DRLTournamentResult p_result)
			{
				if (base.validContext && p_result != null && p_result.tournaments.Length != 0)
				{
					model.UpdateTournamentStates(p_result.tournaments);
				}
			});
		}

		private void SyncNotifications()
		{
			if (model.notifications.isSceneLoading)
			{
				return;
			}
			base.app.model.service.GetNotifications(delegate(DRLNotificationsData[] p_result)
			{
				if (base.validContext && p_result != null && p_result.Length != 0)
				{
					UpdateTournamentNotificationsPending(p_result);
				}
			});
		}

		internal void UpdateTournamentNotificationsPending(DRLNotificationsData[] p_notifications)
		{
			if (p_notifications == null || p_notifications.Length == 0)
			{
				return;
			}
			string sid = base.app.model.storage.state.player.profile.playerId;
			foreach (DRLNotificationsData dRLNotificationsData in p_notifications)
			{
				base.app.model.service.MarkNotificationRead(dRLNotificationsData.id);
				if (string.IsNullOrEmpty(dRLNotificationsData.guid))
				{
					continue;
				}
				foreach (TournamentNotificationState tns in model.tournamentStates)
				{
					if (!(dRLNotificationsData.guid == tns.guid))
					{
						continue;
					}
					base.app.model.service.GetTournament(dRLNotificationsData.guid, delegate(DRLTournamentResult p_result)
					{
						if (base.validContext && p_result != null && p_result.tournaments != null && p_result.tournaments.Length != 0)
						{
							DRLTournamentData dRLTournamentData = p_result.tournaments[0];
							if (dRLTournamentData.status == TournamentState.idle && !dRLTournamentData.IsPlayerRegistered(sid))
							{
								Notify("notifications.tournament.opened", dRLTournamentData);
								tns.state = TournamentState.idle;
							}
							if (dRLTournamentData.status == TournamentState.active && dRLTournamentData.IsPlayerRegistered(sid))
							{
								Notify("notifications.tournament.started", dRLTournamentData);
								tns.state = TournamentState.active;
							}
						}
					});
					break;
				}
			}
		}

		private void OnApplicationQuit()
		{
			StopSyncData();
		}

		public void OnPersistency()
		{
			base.app.controller.notifications.tournaments = this;
		}
	}
}

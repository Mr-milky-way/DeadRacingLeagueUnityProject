using System;
using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLAchievementsController : Controller<DRLApp>
	{
		[Tooltip("Achievement refresh timer in seconds.")]
		public float refreshPeriod = 60f;

		private Activity m_syncTimer;

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			_ = base.app.arguments.game;
			switch (p_event)
			{
			case "missions.mission-complete":
			case "game.race.leaderboard-complete":
				this.TimerRunOnce(delegate
				{
					if (base.validContext)
					{
						RefreshAchievements();
					}
				}, 3f);
				break;
			case "achievements.state@refresh":
				RefreshAchievements();
				break;
			}
		}

		public void RefreshAchievements()
		{
			if (!base.validContext)
			{
				return;
			}
			base.app.model.service.RefreshAchievements(delegate(DRLAchievementsResult p_result)
			{
				if (p_result == null)
				{
					Debug.LogWarning("DRLAchievementsController> RefreshAchievements / Result is <null>");
				}
				else if (p_result.list == null)
				{
					Debug.LogWarning("DRLAchievementsController> RefreshAchievements / List is <null>");
				}
				else if (p_result.list.Length != 0)
				{
					DRLAchievementsData[] list = p_result.list;
					foreach (DRLAchievementsData it in list)
					{
						base.app.model.service.platform.UpdateAchievement(it.platformId, it.progress, delegate
						{
							base.app.model.service.MarkAchievementRead(it.achievementId);
						});
					}
				}
			});
		}

		private void StartSyncData()
		{
			if (m_syncTimer != null)
			{
				StopSyncData();
			}
			float timerState = 0f;
			RefreshAchievements();
			m_syncTimer = Activity.Run((Func<bool>)delegate
			{
				if (!base.validContext)
				{
					return false;
				}
				timerState += Time.deltaTime;
				if (timerState >= refreshPeriod)
				{
					timerState = 0f;
					RefreshAchievements();
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

		private void OnApplicationQuit()
		{
			StopSyncData();
		}

		private void PushAchievementNotification(DRLAchievementsData p_achievement)
		{
			NewsNotificationData newsNotificationData = new NewsNotificationData();
			newsNotificationData.newsTitle = "ACHIEVEMENT: " + p_achievement?.title;
			Notify("notifications.push", newsNotificationData);
		}

		public void OnPersistency()
		{
			base.app.controller.achievements = this;
		}
	}
}

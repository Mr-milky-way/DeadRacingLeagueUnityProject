using System.Collections.Generic;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLNotificationModel : Model<DRLApp>
	{
		public bool isSceneLoading;

		public bool isSnoozing;

		public float gameInviteTTL = 60f;

		public float notificationRate = 1f;

		public List<NotificationData> list;

		public List<NotificationData> queue;

		[SerializeField]
		private Texture m_defaultAvatar;

		private float m_notification_elapsed;

		private Activity m_snooze_timer;

		public DRLTournamentNotificationModel tournaments => AssertFind<DRLTournamentNotificationModel>("tournaments");

		public NotificationState stateMenu
		{
			get
			{
				return base.app.model.storage.state.player.profile.notificationStateMenu;
			}
			set
			{
				base.app.model.storage.state.player.profile.notificationStateMenu = value;
			}
		}

		public NotificationState stateIngame
		{
			get
			{
				return base.app.model.storage.state.player.profile.notificationStateInGame;
			}
			set
			{
				base.app.model.storage.state.player.profile.notificationStateInGame = value;
			}
		}

		public NotificationState state
		{
			get
			{
				NotificationState result = ((base.app.model.game ? true : false) ? stateIngame : stateMenu);
				if (isSnoozing)
				{
					result = NotificationState.Off;
				}
				if (isSceneLoading)
				{
					result = NotificationState.Off;
				}
				return result;
			}
		}

		public Texture defaultAvatar => m_defaultAvatar;

		private void Start()
		{
		}

		public NotificationData FindById(string p_id)
		{
			return list.Find((NotificationData it) => it != null && it.id == p_id);
		}

		public void Push(NotificationData p_data)
		{
			if (p_data != null && FindById(p_data.id) == null)
			{
				list.Add(p_data);
				list.RemoveAll(FilterNull);
				Notify("notifications.push", p_data);
			}
		}

		public void Remove(NotificationData p_data)
		{
			if (p_data != null)
			{
				Remove(p_data.id);
			}
		}

		public void Remove(string p_id)
		{
			NotificationData notificationData = FindById(p_id);
			if (notificationData != null)
			{
				list.Remove(notificationData);
				Notify("notifications.remove", notificationData);
			}
		}

		public void Queue(NotificationData p_data)
		{
			if (p_data != null)
			{
				queue.Add(p_data);
				queue.RemoveAll(FilterNull);
				queue.Sort(SortByTimestamp);
				Notify("notifications.queue", p_data);
			}
		}

		public List<T> Filter<T>() where T : NotificationData
		{
			List<T> list = new List<T>();
			for (int i = 0; i < this.list.Count; i++)
			{
				if (this.list[i] is T)
				{
					list.Add(this.list[i] as T);
				}
			}
			return list;
		}

		public void Send(NotificationData p_data)
		{
		}

		public void Snooze(float p_time)
		{
			isSnoozing = true;
			if (m_snooze_timer != null)
			{
				m_snooze_timer.Stop();
			}
			m_snooze_timer = Activity.RunOnce(delegate
			{
				ClearSnooze();
			}, p_time);
		}

		public void ClearSnooze()
		{
			isSnoozing = false;
			if (m_snooze_timer != null)
			{
				m_snooze_timer.Stop();
			}
			Notify("notifications.ui.snooze.clear");
		}

		public void OnPersistency()
		{
			base.app.model.notifications = this;
		}

		private void Update()
		{
			PollQueue();
			UpdateNotifications();
		}

		protected void UpdateNotifications()
		{
			for (int i = 0; i < list.Count; i++)
			{
				NotificationData notificationData = list[i];
				notificationData.Update();
				if (notificationData.ttlComplete)
				{
					list.RemoveAt(i--);
					Notify("notifications.remove", notificationData);
				}
			}
		}

		protected void PollQueue()
		{
			if (queue.Count <= 0)
			{
				m_notification_elapsed = 9999f;
				return;
			}
			m_notification_elapsed += Time.unscaledDeltaTime;
			if (!(m_notification_elapsed < notificationRate))
			{
				NotificationData p_data = queue[0];
				queue.RemoveAt(0);
				Push(p_data);
				m_notification_elapsed = 0f;
			}
		}

		private bool FilterNull(NotificationData a)
		{
			return a == null;
		}

		private int SortByTimestamp(NotificationData a, NotificationData b)
		{
			return a.timestamp.CompareTo(b.timestamp);
		}
	}
}

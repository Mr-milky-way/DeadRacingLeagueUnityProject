using System;
using System.Collections.Generic;
using UnityEngine;
using drl.game;

namespace thelab.mvc
{
	public class DebugNotificationView : View
	{
		[Serializable]
		public class Notification
		{
			public string notification;

			public KeyCode key;

			public float delay;

			public float elapsed;

			public bool sent;
		}

		public List<Notification> list;

		public DRLMission uavMission;

		public DRLQuest uavQuest;

		protected void Awake()
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i].sent = false;
				if (list[i].key == KeyCode.None)
				{
					Notify(list[i].notification);
					list[i].sent = true;
				}
			}
		}

		protected void Update()
		{
			if (!this || !base.enabled || !base.gameObject.activeInHierarchy)
			{
				return;
			}
			for (int i = 0; i < list.Count; i++)
			{
				Notification notification = list[i];
				if (!Input.GetKey(notification.key))
				{
					notification.elapsed = 0f;
					notification.sent = false;
				}
				else if (!notification.sent)
				{
					notification.elapsed += Time.unscaledDeltaTime;
					if (!(notification.elapsed < notification.delay))
					{
						notification.sent = true;
						notification.elapsed = notification.delay;
						Notify(notification.notification, (!(notification.notification == "home.debug.uav@click")) ? null : new object[2] { uavMission, uavQuest });
					}
				}
			}
		}
	}
}

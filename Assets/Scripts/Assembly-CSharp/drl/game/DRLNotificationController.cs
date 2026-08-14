using UnityEngine;
using drl.backend;
using drl.chat;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLNotificationController : Controller<DRLApp>
	{
		private string m_level_name;

		private Activity m_pgi_poll;

		private float m_pgi_poll_elapsed;

		private bool m_pgi_active;

		public DRLNotificationModel model => AssertLocal<DRLNotificationModel>("model");

		public DRLTournamentNotificationController tournaments
		{
			get
			{
				return AssertFind<DRLTournamentNotificationController>("tournaments");
			}
			set
			{
			}
		}

		protected void Awake()
		{
			m_level_name = "";
		}

		protected bool PlatformInvitePoll()
		{
			m_pgi_poll_elapsed += Time.unscaledDeltaTime;
			if (m_pgi_poll_elapsed < 1f)
			{
				return true;
			}
			m_pgi_poll_elapsed = 0f;
			if (!base.validContext)
			{
				return true;
			}
			if (!model)
			{
				return true;
			}
			if (model.isSceneLoading)
			{
				return true;
			}
			if (!base.app.model)
			{
				return true;
			}
			if (!base.app.model.service)
			{
				return true;
			}
			PlatformService platform = base.app.model.service.platform;
			if (!platform.hasInvite)
			{
				return true;
			}
			if (string.IsNullOrEmpty(m_level_name))
			{
				m_level_name = base.app.scene.manager.levelName;
			}
			switch (m_level_name)
			{
			case "boot":
				return true;
			case "splash":
				return true;
			case "main":
				if (!base.app.view.ui.screens.current)
				{
					return true;
				}
				break;
			}
			PlatformGameInvite pgi = platform.GetInvite(0);
			platform.ClearInvites();
			bool flag = true;
			if (platform.ContainsFlag(PlatformServiceFlagType.XBoxCrossPlayAllowed) && pgi.args.Contains("xbox"))
			{
				flag = false;
			}
			if (!platform.ContainsFlag(PlatformServiceFlagType.XBoxCrossPlayAllowed) && pgi.args.Contains("all"))
			{
				flag = false;
			}
			if (!flag)
			{
				m_pgi_active = true;
				Notify("network.crossplay.mismatch");
				return true;
			}
			Activity.RunOnce(delegate
			{
				if (!model.isSceneLoading)
				{
					m_pgi_active = true;
					Notify("notifications.action", pgi);
				}
			}, 0.1f);
			return true;
		}

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "boot@complete":
				m_level_name = base.app.scene.manager.levelName;
				break;
			case "scene.start":
				if ((string)p_data[0] == "main")
				{
					model.ClearSnooze();
				}
				break;
			case "scene.game.scenes@start":
				model.isSceneLoading = true;
				break;
			case "scene.game.scenes@complete":
				model.isSceneLoading = false;
				break;
			case "chat.incoming.invite":
			{
				if (base.validContext && base.app.inGame)
				{
					NetworkRaceController mode = base.app.controller.game.GetMode<NetworkRaceController>();
					if ((bool)mode && mode.model.raceActive)
					{
						break;
					}
				}
				_ = p_data[0];
				GameInviteMessage gameInviteMessage = p_data[1] as GameInviteMessage;
				InviteNotificationData inviteNotificationData = new InviteNotificationData();
				inviteNotificationData.type = ((!gameInviteMessage.IsQuickMatch) ? NotificationTypeFlag.RoomInvite : NotificationTypeFlag.QuickMatchInvite);
				inviteNotificationData.platformId = gameInviteMessage.SenderId;
				inviteNotificationData.profileColor = gameInviteMessage.SenderColor;
				inviteNotificationData.profileName = gameInviteMessage.SenderName;
				inviteNotificationData.playerId = gameInviteMessage.PlayerId;
				inviteNotificationData.inviteRegionCode = gameInviteMessage.RegionCode;
				inviteNotificationData.inviteRoomId = gameInviteMessage.RoomId;
				inviteNotificationData.inviteRoomName = gameInviteMessage.RoomName;
				inviteNotificationData.inviteIsRace = gameInviteMessage.IsRace;
				inviteNotificationData.crossplay = gameInviteMessage.IsCrossplay;
				inviteNotificationData.isFriend = base.app.model.service.social.friends.IsFriend(inviteNotificationData.platformId);
				inviteNotificationData.blockedList = gameInviteMessage.blockedList;
				Debug.Log($"DRLNotificationController> N.IncomingGameInvite / type[{inviteNotificationData.type}] platform-id[{inviteNotificationData.platformId}]");
				Notify(1f / 60f, "notifications.receive", inviteNotificationData);
				break;
			}
			case "notifications.push":
				_ = p_data[0];
				break;
			case "notifications.receive":
				if (!DRLApp.offline)
				{
					NotificationData notificationData2 = p_data[0] as NotificationData;
					Debug.Log($"DRLNotificationController> N.Notifications.Receive / type[{notificationData2.type}]");
					model.Queue(notificationData2);
				}
				break;
			case "network.room@enter":
				m_pgi_active = false;
				break;
			case "network.crossplay.mismatch":
			case "network.room-enter@error":
			case "network.room.full":
			case "network.room.not-active":
			case "network.lobby.join-failed":
				Debug.Log($"DRLNotificationController> OnNotification / event[{p_event}] pgi-active[{m_pgi_active}]");
				if (m_pgi_active)
				{
					m_pgi_active = false;
					NotificationData notificationData = new NotificationData();
					notificationData.type = NotificationTypeFlag.Message;
					string text = ((p_data.Length == 0) ? "" : p_data[0].ToString().ToUpper());
					string message = (string.IsNullOrEmpty(text) ? "UNKNOWN ERROR" : text);
					switch (p_event)
					{
					case "network.crossplay.mismatch":
						message = "CROSSPLAY PRIVILEGE MISMATCH";
						break;
					case "network.room-enter@error":
						message = (string.IsNullOrEmpty(text) ? "FAILED TO ENTER ROOM" : text);
						break;
					case "network.room.full":
						message = "ROOM IS FULL";
						break;
					case "network.room.not-active":
						message = "ROOM NO LONGER ACTIVE";
						break;
					case "network.lobby.join-failed":
						message = "LOBBY CAN'T BE REACHED";
						break;
					}
					notificationData.message = message;
					Notify("notifications.push-error", notificationData);
				}
				break;
			}
		}

		public void OnPersistency()
		{
			base.app.controller.notifications = this;
		}
	}
}

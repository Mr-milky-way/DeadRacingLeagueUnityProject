using System;
using System.Text.RegularExpressions;
using UnityEngine;
using drl.backend;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UINotificationController : Controller<DRLApp>
	{
		public float autoHideDuration = 10.3f;

		private float m_autoHideTimer = 10.3f;

		private bool m_connecting;

		private UINotificationCardView m_activeCard;

		private int m_pool_step;

		private bool m_pool_complete;

		public UINotificationView view => AssertLocal<UINotificationView>("view");

		protected void Awake()
		{
			m_pool_step = 0;
			Activity.Run((Func<bool>)delegate
			{
				if (m_pool_complete)
				{
					return false;
				}
				if (m_pool_step >= view.historyLimit)
				{
					return false;
				}
				view.invitesList.Push<UINotificationCardView>();
				view.newsList.Push<UINotificationCardView>();
				view.invitesList.Clear();
				view.newsList.Clear();
				m_pool_step++;
				if (m_pool_step >= view.historyLimit)
				{
					m_pool_complete = true;
				}
				return true;
			}, 0f, false);
			view.ResetSnoozeFade();
		}

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (p_event != null)
			{
				switch (p_event)
				{
				case "notifications.ui.panel.toggle@click":
					view.ToggleHistoryPanel();
					break;
				case "notifications.ui.header.tab@change":
				{
					DRLTabGroup dRLTabGroup = p_target as DRLTabGroup;
					if (!(dRLTabGroup == null))
					{
						view.OnTabChanged(dRLTabGroup.index);
					}
					break;
				}
				case "notifications.push":
				{
					if (p_data.Length == 0)
					{
						break;
					}
					NotificationData nd = p_data[0] as NotificationData;
					if (nd == null)
					{
						break;
					}
					if (base.app.arguments.game.isTournamentActive || base.app.arguments.game.isTournamentMatchActive)
					{
						Debug.Log("UINotificationController> N.Notifications.Push / Ignoring Push - Tournament Active");
						break;
					}
					if (nd is InviteNotificationData)
					{
						InviteNotificationData ind = (InviteNotificationData)nd;
						if (ind.platformId == base.app.model.storage.state.player.profile.platformId || ((!base.app.model.network) ? "" : ((base.app.model.network.room == null) ? "" : base.app.model.network.room.Id)) == ind.inviteRoomId)
						{
							break;
						}
						PlatformService platform = base.app.model.service.platform;
						if (!platform)
						{
							break;
						}
						platform.IsUserCommunicationBlocked(ind.platformId, delegate(bool blocked)
						{
							if (blocked)
							{
								Debug.LogWarning("UINotificationController> N.Notifications.Push / IsUserCommunicationBlocked - platform-id[" + ind.platformId + "] is blocked!");
								base.app.model.notifications.Remove(nd);
							}
							else if (!FilterNotification(ind))
							{
								if (!view.historyPanelVisible)
								{
									view.SetNotificationBadge();
								}
								Debug.Log("UINotificationController> N.Notifications.Push / PushNotification");
								view.PushNotification(nd);
							}
						});
						break;
					}
					if (nd is TournamentNotificationData)
					{
						if (!(nd is TournamentNotificationData { isPrivate: false } tournamentNotificationData) || FilterNotification(tournamentNotificationData))
						{
							break;
						}
						if (!view.historyPanelVisible)
						{
							view.SetNotificationBadge();
						}
						if ((tournamentNotificationData.status == TournamentNotificationType.Started || tournamentNotificationData.status == TournamentNotificationType.SoonToStart) && IsInTournament())
						{
							base.app.model.notifications.Remove(tournamentNotificationData.id);
							break;
						}
						if (tournamentNotificationData.status == TournamentNotificationType.Started && !tournamentNotificationData.isParticipant)
						{
							base.app.model.notifications.Remove(tournamentNotificationData.id);
							break;
						}
					}
					Debug.Log("UINotificationController > Received a new notification!");
					view.PushNotification(nd);
					break;
				}
				case "notifications.push-message":
				case "notifications.push-warning":
				case "notifications.push-error":
					if (p_data.Length != 0 && p_data[0] is NotificationData notificationData)
					{
						notificationData.error = p_event == "notifications.push-error" || p_event == "notifications.push-warning";
						Debug.Log("UINotificationController > Received a new notification!");
						view.PushNotification(notificationData);
					}
					break;
				case "notifications.ui.connected":
				case "notifications.ui.expired":
				case "notifications.ui.decline@click":
					m_connecting = false;
					m_activeCard = null;
					this.TimerRunOnce(delegate
					{
						if (p_data.Length != 0)
						{
							string id = (string)p_data[0];
							view.RemoveFromHistory(id);
							if (!view.popUpPanelVisible)
							{
								if (p_event == "notifications.ui.connected")
								{
									this.TimerRunOnce(delegate
									{
										view.HideHistoryPanel();
									}, 0.4f);
								}
							}
							else
							{
								view.RemoveFromPopUp(id);
								this.TimerRunOnce(delegate
								{
									if (!view.HasActiveInvites())
									{
										view.HidePopUpPanel(0.3f);
									}
								}, 0.4f);
							}
						}
					}, 0.4f);
					break;
				case "notifications.ui.timeout":
					this.TimerRunOnce(delegate
					{
						if (p_data.Length != 0)
						{
							string id = (string)p_data[0];
							view.RemoveFromPopUp(id, p_delete: false);
							this.TimerRunOnce(delegate
							{
								if (!view.HasActiveInvites())
								{
									view.HidePopUpPanel(0.3f);
								}
							}, 0.4f);
						}
					}, 0.4f);
					break;
				case "notifications.ui.accept@click":
					m_autoHideTimer = autoHideDuration;
					m_connecting = true;
					break;
				case "notifications.ui.snooze.15@click":
				case "notifications.ui.snooze.30@click":
				case "notifications.ui.snooze.60@click":
				case "notifications.ui.snooze.90@click":
				{
					int result = 0;
					int.TryParse(Regex.Match(p_event, "\\d+").Value, out result);
					if (result != 0)
					{
						base.app.model.notifications.Snooze((float)result * 60f);
					}
					break;
				}
				case "notifications.ui.snooze.clear":
					view.ResetSnoozeFade();
					break;
				case "ui.footer@open":
					view.footerVisible = true;
					if (view.popUpPanelVisible)
					{
						view.popUpFade.from.y = 0f;
						view.popUpFade.center.y = 0f;
						view.popUpFade.to.y = 0f;
						view.popUpFade.transition = view.popUpFade.transition;
					}
					break;
				case "ui.footer@close":
					view.footerVisible = false;
					if (view.popUpPanelVisible)
					{
						view.popUpFade.from.y = -70f;
						view.popUpFade.center.y = -70f;
						view.popUpFade.to.y = -70f;
						view.popUpFade.transition = view.popUpFade.transition;
					}
					view.HideHistoryPanel();
					break;
				case "notifications.ui.card@over":
				{
					UIElementView uIElementView = p_target as UIElementView;
					if (!(uIElementView == null))
					{
						UINotificationCardView component = uIElementView.GetComponent<UINotificationCardView>();
						if (!(component == null) && !component.inactive)
						{
							m_activeCard = component;
						}
					}
					break;
				}
				case "notifications.ui.card@out":
					m_activeCard = null;
					break;
				}
			}
			if (p_event.StartsWith("scene.") && m_connecting && base.app.model.network != null)
			{
				base.app.model.network.LeaveRoom();
			}
		}

		private bool FilterNotification(InviteNotificationData p_d)
		{
			bool result = true;
			if (base.app.model.notifications.isSnoozing)
			{
				return result;
			}
			bool inGame = base.app.inGame;
			ProfileStateModel profile = base.app.model.storage.state.player.profile;
			switch (inGame ? profile.notificationStateInGame : profile.notificationStateMenu)
			{
			case NotificationState.Everyone:
				result = false;
				break;
			case NotificationState.Off:
				result = true;
				break;
			case NotificationState.Friends:
			{
				SteamService steamService = base.app.model.service.platform as SteamService;
				if (!(steamService != null) || steamService.IsFriend(p_d.platformId))
				{
					result = false;
				}
				break;
			}
			}
			return result;
		}

		private bool FilterNotification(NotificationData p_data)
		{
			bool result = true;
			if (base.app.model.notifications.isSnoozing)
			{
				return result;
			}
			bool inGame = base.app.inGame;
			ProfileStateModel profile = base.app.model.storage.state.player.profile;
			switch (inGame ? profile.notificationStateInGame : profile.notificationStateMenu)
			{
			case NotificationState.Off:
				result = true;
				break;
			case NotificationState.Everyone:
			case NotificationState.Friends:
				result = false;
				break;
			}
			return result;
		}

		private bool IsInTournament()
		{
			bool result = false;
			if (base.app.inGame && base.app.arguments.game.tournamentData != null)
			{
				result = true;
			}
			UIScreen current = base.app.view.ui.screens.current;
			string[] array = new string[7] { "tournament-overview-screen", "tournaments-list-screen", "tournament-brackets-screen", "tournament-leaderboards-screen", "tournament-leaders-screen", "tournament-results-screen", "tournament-race-ends-screen" };
			if (current != null)
			{
				string[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					if (array2[i] == current.name)
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		private void Update()
		{
			if (view.popUpPanelVisible)
			{
				if (view.popUpActive)
				{
					m_autoHideTimer = autoHideDuration;
				}
				m_autoHideTimer -= Time.deltaTime;
				if (m_autoHideTimer <= 0f)
				{
					m_autoHideTimer = autoHideDuration;
					view.HidePopUpPanel(0.3f);
				}
				if (RCI.GetButtonDown(ConsoleButtons.LeftShoulder2))
				{
					view.ClearIgnoredCommands();
					UINavigation.Focus(view.lastFocusElement);
					view.HilightHeaderBack(p_flag: false);
				}
				if (RCI.GetButtonDown(ConsoleButtons.RightShoulder2))
				{
					view.SetIgnoredGameCommands();
					view.FocusPopup();
					view.HilightHeaderBack(p_flag: true);
				}
			}
			if (Input.GetKeyDown(KeyCode.Mouse0) && !view.elementView.over && !base.app.view.ui.footer.notificationsButton.down)
			{
				view.HideHistoryPanel();
			}
			if (RCI.GetButtonDown(ConsoleButtons.ActionBottomRow1) && m_activeCard != null && m_activeCard.gameObject.activeInHierarchy)
			{
				if (m_activeCard.activeButton == null)
				{
					return;
				}
				m_activeCard.activeButton.onClick.Invoke();
			}
			if (!RCI.GetButtonDown(ConsoleButtons.ActionBottomRow2))
			{
				return;
			}
			if (m_activeCard != null && m_activeCard.gameObject.activeInHierarchy)
			{
				if (!(m_activeCard.rejectButton == null))
				{
					m_activeCard.rejectButton.onClick.Invoke();
				}
			}
			else
			{
				this.TimerRunOnce(delegate
				{
					view.HidePopUpPanel();
					view.HideHistoryPanel();
				}, 0.05f);
			}
		}
	}
}

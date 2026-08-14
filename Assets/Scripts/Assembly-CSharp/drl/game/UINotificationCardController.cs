using UnityEngine;
using drl.backend;
using drl.network;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UINotificationCardController : Controller<DRLApp>
	{
		public UINotificationCardView view => AssertLocal<UINotificationCardView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "notifications.action":
				view.SetAcceptActive(p_enabled: false);
				break;
			case "notifications.ui.connected":
			case "notifications.ui.expired":
				view.SetAcceptActive(p_enabled: true);
				break;
			case "network.room@enter":
				if (p_data.Length != 0)
				{
					string p_roomID = (string)p_data[0];
					if (view.data is InviteNotificationData && ((InviteNotificationData)view.data).type == NotificationTypeFlag.RoomInvite)
					{
						view.OnConnected(p_roomID);
					}
				}
				break;
			case "network.lobby.join-failed":
				if (view.connecting)
				{
					view.SetErrorFeedback(NotificationCardErrorType.LobbyJoinFailed);
				}
				break;
			case "network.room.not-active":
				if (view.connecting)
				{
					view.SetErrorFeedback(NotificationCardErrorType.RoomNotActive);
				}
				break;
			case "network.room.full":
				if (view.connecting)
				{
					view.SetErrorFeedback(NotificationCardErrorType.RoomFull);
				}
				break;
			case "network.qm.state.changed":
				if (p_data.Length != 0)
				{
					QuickMatchResult quickMatchResult = (QuickMatchResult)p_data[0];
					if (quickMatchResult.JoinedRoom != null)
					{
						view.OnQMStateChange(quickMatchResult.State, quickMatchResult.JoinedRoom);
					}
				}
				break;
			case "network.room-enter@error":
				view.StopConnecting();
				break;
			case "notifications.ui.card@over":
			{
				UIElementView uIElementView2 = p_target as UIElementView;
				if (!(uIElementView2 == null))
				{
					UIElementView component2 = GetComponent<UIElementView>();
					if (!(component2 == null) && !(uIElementView2 != component2))
					{
						view.SetButtonsVisible(p_flag: true);
					}
				}
				break;
			}
			case "notifications.ui.card@out":
			{
				UIElementView uIElementView = p_target as UIElementView;
				if (uIElementView == null)
				{
					break;
				}
				UIElementView component = GetComponent<UIElementView>();
				if (component == null || uIElementView != component)
				{
					break;
				}
				this.TimerRunOnce(delegate
				{
					if (!base.validContext)
					{
						view.SetButtonsVisible(p_flag: false);
					}
					else if (UINavigation.focus != null)
					{
						Transform p_child = UINavigation.focus.transform;
						if (!IsChild(p_child, base.transform))
						{
							view.SetButtonsVisible(p_flag: false);
						}
					}
					else
					{
						view.SetButtonsVisible(p_flag: false);
					}
				}, 0.1f);
				break;
			}
			}
		}

		public void Accept()
		{
			if (!view.inactive && view.type != NotificationTypeFlag.None)
			{
				view.StartConnecting();
				Notify("notifications.ui.accept@click", view);
			}
		}

		public void Decline()
		{
			if (!view.inactive)
			{
				Notify("notifications.ui.decline@click", view.id);
				view.Hide(0.3f);
			}
		}

		public void Register()
		{
			if (view.inactive)
			{
				return;
			}
			TournamentNotificationData tnd = view.data as TournamentNotificationData;
			if (tnd == null)
			{
				return;
			}
			view.SetFeedback("CONNECTING...");
			view.registerButton.interactable = false;
			base.app.model.service.GetTournament(tnd.tournamentGuid, delegate(DRLTournamentResult p_result)
			{
				if (base.validContext)
				{
					if (p_result.tournaments == null || p_result.tournaments.Length == 0)
					{
						view.SetFeedback("NO TOURNAMENT!");
						this.TimerRunOnce(delegate
						{
							if (base.validContext)
							{
								view.SetFeedback("");
								view.registerButton.interactable = true;
							}
						}, 2f);
						Debug.Log("UITournamentsListController > Unable to connect to tournament: " + tnd.id);
					}
					else
					{
						Notify("notifications.ui.connected", tnd.id);
						base.app.view.ui.notifications.HidePopUpPanel();
						UITournamentOverviewView uITournamentOverviewView = base.app.view.ui.screens.Open<UITournamentOverviewView>("tournament-overview-screen");
						if ((bool)uITournamentOverviewView)
						{
							Notify("tournament.model.reset", p_result.tournaments[0]);
							uITournamentOverviewView.Set(p_result.tournaments[0], 0);
						}
					}
				}
			});
		}

		public void Join()
		{
			if (view.inactive)
			{
				return;
			}
			TournamentNotificationData tnd = view.data as TournamentNotificationData;
			if (tnd == null || !base.validContext)
			{
				return;
			}
			view.SetFeedbackConnecting();
			TournamentModel tm = base.app.model.tournament;
			tm.SetTournamentData(tnd.tournamentGuid, delegate
			{
				if (base.validContext && tm.tournament != null)
				{
					Notify("notifications.ui.connected", tnd.id);
					base.app.model.tournament.SetTournamentData(tnd.tournamentGuid, delegate
					{
						base.app.view.ui.screens.Open<UITournamentBracketsView>("tournament-brackets-screen").backButtonEnabled = !base.app.inGame;
					});
					if (base.app.inGame)
					{
						GameTypeController mode = base.app.controller.game.GetMode<GameTypeController>();
						if (mode != null)
						{
							mode.Pause(p_flag: true, p_pause_physics: true, p_open_pause_screen: false);
						}
					}
				}
			});
		}

		public void ExitQMQueue()
		{
			if (!(base.app.model.network == null))
			{
				base.app.model.network.LeaveRoom();
				view.qmStatesText.text = "LEAVING...";
				this.TimerRunOnce(delegate
				{
					view.Hide(0.3f);
					Notify("notifications.ui.decline@click", view.id);
				}, 0.5f);
			}
		}

		private bool IsChild(Transform p_child, Transform p_parent)
		{
			if (p_child.parent == p_parent)
			{
				return true;
			}
			if (p_child == null || p_child.parent == null)
			{
				return false;
			}
			return IsChild(p_child.parent, p_parent);
		}
	}
}

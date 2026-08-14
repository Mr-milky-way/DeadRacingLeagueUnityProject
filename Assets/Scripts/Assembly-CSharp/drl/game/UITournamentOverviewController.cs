using System;
using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using drl.network;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UITournamentOverviewController : Controller<DRLApp>
	{
		private WebAsyncRequest m_tournamentGet;

		private bool m_enteringBrackets;

		private bool m_rebootRegistration;

		private bool m_rebootReminder;

		public UITournamentOverviewView view => AssertLocal<UITournamentOverviewView>("view");

		public NetworkModel network => base.app.model.network;

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (p_event != null && p_event == "tournament.drone.selected" && m_enteringBrackets)
			{
				RunOnce(delegate
				{
					base.app.view.ui.screens.Open<UITournamentBracketsView>("tournament-brackets-screen");
					Notify("tournament.brackets.open");
					m_enteringBrackets = false;
				}, 0.11f);
			}
			if (base.app.view.ui.screens.current != view.screen)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
			{
				if (p_data[0] as UIScreen != view.screen)
				{
					break;
				}
				if (base.app.inGame)
				{
					base.app.view.ui.game.hud.Hide(0f);
				}
				view.SetFeedback(UITournamentsOverviewFeedbackType.None, p_hide_list: false, 0.1f);
				view.ResetTermsAndAgeToggles();
				view.RefreshControlButtons();
				m_enteringBrackets = false;
				if (base.app.arguments.tournament != null)
				{
					base.app.arguments.tournament.data = null;
					base.app.arguments.tournament = null;
				}
				PlayerStateModel player = base.app.model.storage.state.player;
				if ((bool)player && player.profile.dataCompletion == 1f && (m_rebootRegistration || m_rebootReminder))
				{
					if (m_rebootRegistration)
					{
						StartRegistration();
					}
					if (m_rebootReminder)
					{
						SubscribeUser();
					}
				}
				m_rebootRegistration = false;
				m_rebootReminder = false;
				if (view.tournament.status == TournamentState.active)
				{
					Connect();
				}
				SyncData();
				view.OnTermsAndAgeChange();
				if (base.app.inGame)
				{
					base.app.controller.game.input.controller.Pause(p_flag: true, p_pause_physics: true, p_open_pause_screen: false);
				}
				try
				{
					if (view.tournament != null && !string.IsNullOrEmpty(view.tournament.videoURL))
					{
						if ((bool)view.videoPlayerIcon)
						{
							view.videoPlayerIcon.FadeIn();
						}
						if ((bool)view.videoPlayerFader && view.videoPlayerFader.alpha < 1f)
						{
							view.videoPlayerFader.FadeIn();
						}
						view.videoPlayer.url = view.tournament.videoURL;
						view.videoPlayer.Stop();
					}
					else
					{
						if ((bool)view.videoPlayerIcon)
						{
							view.videoPlayerIcon.FadeOut(0.01f);
						}
						if ((bool)view.videoPlayerFader)
						{
							view.videoPlayerFader.FadeOut(0.01f);
						}
					}
					break;
				}
				catch (Exception ex)
				{
					Debug.Log("UITournamentOverviewController> " + ex.Message);
					break;
				}
			}
			case "tournament.overview.form.event@click":
				OnFormNotification(p_target, p_is_change: false, p_event);
				break;
			case "tournament.overview.card@click":
			{
				string videoURL = view.tournament.videoURL;
				_ = view.tournament.imageURL;
				if (!string.IsNullOrEmpty(videoURL))
				{
					view.bigCard.StopTimerActivity();
					base.app.view.ui.screens.Open<UIVideoPlayerView>("video-player-screen").VideoURL = videoURL;
					view.videoPlayer.Pause();
				}
				break;
			}
			case "tournament.action.refresh":
				if (!view.current)
				{
					break;
				}
				if (p_data.Length != 0)
				{
					if (p_data[0] is DRLTournamentData p_data2)
					{
						SyncData(p_data2);
					}
				}
				else
				{
					SyncData();
				}
				view.RefreshControlButtons();
				break;
			case "ui.screen.return@click":
				m_enteringBrackets = false;
				m_rebootRegistration = false;
				m_rebootReminder = false;
				view.bigCard.StopTimerActivity();
				network.Disconnect();
				if (base.app.inGame)
				{
					base.app.controller.game.input.controller.Pause(p_flag: false, p_pause_physics: false, p_open_pause_screen: false);
					base.app.view.ui.screens.Close(view.screen.name);
				}
				else
				{
					base.app.view.ui.screens.Return();
				}
				break;
			case "ui.screen@close":
				m_rebootRegistration = false;
				m_rebootReminder = false;
				m_enteringBrackets = false;
				view.bigCard.StopTimerActivity();
				break;
			}
		}

		protected void OnFormNotification(UnityEngine.Object p_target, bool p_is_change, string p_event)
		{
			string text = p_target.name;
			if (text == null)
			{
				return;
			}
			switch (text)
			{
			case "standings":
				break;
			case "enter":
				if (!view.IsPlayerRegistered())
				{
					base.app.view.ui.screens.Open<UITournamentBracketsView>("tournament-brackets-screen");
					Notify("tournament.brackets.open");
					break;
				}
				m_enteringBrackets = true;
				if (view.tournament.droneClass != 1)
				{
					view.bigCard.StopTimerActivity();
					OpenDroneSelectionScreen(view.tournament.droneClass);
				}
				else if (!string.IsNullOrEmpty(view.tournament.droneGuid))
				{
					view.bigCard.StopTimerActivity();
					view.LoadCustomDrone(delegate
					{
						OpenDroneSelectionScreen();
					});
				}
				else
				{
					m_enteringBrackets = false;
				}
				break;
			case "subscribe":
			case "register":
			{
				if (text == "subscribe" && view.IsPlayerRegistered())
				{
					SubscribeUser();
					break;
				}
				PlayerStateModel player = base.app.model.storage.state.player;
				if (player == null)
				{
					break;
				}
				player.profile.CalculateDataCompletion();
				if (player.profile.dataCompletion >= 1f)
				{
					if (text == "register")
					{
						StartRegistration();
					}
					if (text == "subscribe")
					{
						SubscribeUser();
					}
				}
				else
				{
					view.bigCard.StopTimerActivity();
					base.app.view.ui.screens.Open<UISettingsProfileView>("settings-profile-screen").showMandatoryFields = true;
				}
				break;
			}
			case "unregister":
				Unregister();
				break;
			case "unsubscribe":
				UnsubscribeUser();
				break;
			case "terms-conditions":
				if (!string.IsNullOrEmpty(view.tournament.termsURL))
				{
					WebBrowser.OpenURL(view.tournament.termsURL, (base.app != null) ? base.app.model.service.platform : null);
				}
				break;
			case "over-18":
				if (view.IsPlayerRegistered())
				{
					view.ageConfirmation.SetState(p_flag: true);
				}
				else
				{
					view.OnTermsAndAgeChange();
				}
				break;
			case "accept-terms":
				if (view.IsPlayerRegistered())
				{
					view.termsAccepting.SetState(p_flag: true);
				}
				else
				{
					view.OnTermsAndAgeChange();
				}
				break;
			case "watch-stream":
				if (!string.IsNullOrEmpty(view.tournament.streamingURL))
				{
					WebBrowser.OpenURL(view.tournament.streamingURL, (base.app != null) ? base.app.model.service.platform : null);
				}
				break;
			}
		}

		public void StartRegistration()
		{
			view.SetFeedback(UITournamentsOverviewFeedbackType.Registering, p_hide_list: true, 0.1f);
			view.registerButton.interactable = false;
			view.unRegisterButton.interactable = true;
			base.app.model.service.RegisterUser(view.tournament.guid, delegate(DRLServiceResult p_result)
			{
				view.SetFeedback(UITournamentsOverviewFeedbackType.None, p_hide_list: false, 0.1f);
				if (base.validContext && !p_result.success)
				{
					Debug.Log("UITournamentsListController > Unsuccessful user registration " + p_result.message);
					view.registerButton.interactable = true;
					view.unRegisterButton.interactable = false;
				}
			});
		}

		public void Unregister()
		{
			view.SetFeedback(UITournamentsOverviewFeedbackType.Unregistering, p_hide_list: true, 0.1f);
			view.registerButton.interactable = true;
			view.unRegisterButton.interactable = false;
			base.app.model.service.UnregisterUser(view.tournament.guid, delegate(DRLServiceResult p_result)
			{
				view.SetFeedback(UITournamentsOverviewFeedbackType.None, p_hide_list: false, 0.2f);
				if (base.validContext)
				{
					if (!p_result.success)
					{
						Debug.Log("UITournamentsListController > Unsuccessful user registration");
					}
					else
					{
						Debug.Log("UITournamentsListController > Successful user unregistration");
						view.OnTermsAndAgeChange();
						SyncData();
					}
				}
			});
		}

		public void SubscribeUser()
		{
			view.SetFeedback(UITournamentsOverviewFeedbackType.Processing, p_hide_list: true, 0.1f);
			base.app.model.service.SubscribeUser(view.tournament.guid, delegate(DRLServiceResult p_result)
			{
				if (base.validContext)
				{
					view.SetFeedback(UITournamentsOverviewFeedbackType.None, p_hide_list: false, 0f);
					if (!p_result.success)
					{
						Debug.Log("UITournamentsListController > Unsuccessful user subscription " + p_result.message);
					}
					else
					{
						view.SetSubscriptionButtons(p_subscribed: true);
					}
				}
			});
		}

		public void UnsubscribeUser()
		{
			view.SetFeedback(UITournamentsOverviewFeedbackType.Processing, p_hide_list: true, 0.1f);
			base.app.model.service.UnsubscribeUser(view.tournament.guid, delegate(DRLServiceResult p_result)
			{
				if (base.validContext)
				{
					view.SetFeedback(UITournamentsOverviewFeedbackType.None, p_hide_list: false, 0f);
					if (!p_result.success)
					{
						Debug.Log("UITournamentsListController > User wasn't unsubscribed: " + p_result.message);
					}
					else
					{
						Debug.Log("UITournamentsListController > Unsubscribed user successfully ");
						view.SetSubscriptionButtons(p_subscribed: false);
					}
				}
			});
		}

		public void OpenDroneSelectionScreen(int p_droneClass)
		{
			UIGarageRigSelectionView uIGarageRigSelectionView = base.app.view.ui.screens.Open<UIGarageRigSelectionView>("garage-rig-selection-screen");
			uIGarageRigSelectionView.screen.title = base.app.model.storage.locale.Get("multiplayer.select-drone-screen.title", "Select your Drone");
			uIGarageRigSelectionView.SetCreationEnabled(p_flag: false);
			uIGarageRigSelectionView.SetDroneClassEnabled(true);
			uIGarageRigSelectionView.allowCustomPhysics = false;
			uIGarageRigSelectionView.selectionOnly = true;
			uIGarageRigSelectionView.overrideList = null;
			uIGarageRigSelectionView.backButtonDoubleReturn = true;
			uIGarageRigSelectionView.openedAsTournamentSelector = true;
			if (p_droneClass == 2)
			{
				uIGarageRigSelectionView.overrideSizes = new List<int>(1) { 0 };
			}
			else if (p_droneClass > 2)
			{
				uIGarageRigSelectionView.overrideSizes = new List<int>(1) { p_droneClass };
			}
			else
			{
				uIGarageRigSelectionView.overrideSizes = null;
			}
		}

		public void OpenDroneSelectionScreen()
		{
			Notify("tournament.drone.selected");
		}

		private void Connect()
		{
			if (view.tournament.id != network.LobbyId || network.connectionState == PhotonService.ServiceState.Disconnected)
			{
				network.ConnectToTournamentLobby(view.tournament);
			}
		}

		private void SyncData(DRLTournamentData p_data = null)
		{
			if (m_tournamentGet != null)
			{
				m_tournamentGet.Cancel();
				m_tournamentGet = null;
			}
			if (p_data != null)
			{
				view.Set(p_data, view.minimumSkill);
				return;
			}
			m_tournamentGet = base.app.model.service.GetTournament(view.tournament.guid, delegate(DRLTournamentResult p_result)
			{
				if (!(this == null) && !(base.app == null) && !(base.app.view == null) && !(view == null) && !(base.gameObject == null) && p_result.tournaments.Length != 0)
				{
					view.Set(p_result.tournaments[0], view.minimumSkill);
					view.RefreshControlButtons();
					this.TimerRunOnce(delegate
					{
						if (base.validContext && (!base.app.inGame || base.app.model.game.type != GameFlag.Race || !(base.app.model.game != null) || !(base.app.model.game.simulation != null) || !base.app.model.game.simulation.running))
						{
							GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
							Debug.Log("UITournamentOverviewController>  GC forced cleanup on tournaments refresh data..");
						}
					}, 0.1f);
				}
			});
		}
	}
}

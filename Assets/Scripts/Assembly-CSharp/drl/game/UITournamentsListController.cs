using System;
using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UITournamentsListController : Controller<DRLApp>
	{
		private MonoActivity m_syncLoop;

		private float m_syncFreq = 20f;

		private WebAsyncRequest tournamentsGet;

		public UITournamentsListView view => AssertLocal<UITournamentsListView>("view");

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				if (!(p_data[0] as UIScreen != view.screen))
				{
					view.ClearTournaments();
					StartDataSyncLoop(0f);
					base.app.model.service.WatchTournamentRefresh();
				}
				break;
			case "tournament.list.form.event@click":
				OnFormNotification(p_target, p_is_change: false, p_event);
				break;
			case "tournament-card@click":
			{
				UICardButtonTournament uICardButtonTournament2 = p_target as UICardButtonTournament;
				if (uICardButtonTournament2.tournamentData != null)
				{
					StopDataSyncLoop();
					uICardButtonTournament2.StopTimerActivity();
					UITournamentOverviewView uITournamentOverviewView = base.app.view.ui.screens.Open<UITournamentOverviewView>("tournament-overview-screen");
					if ((bool)uITournamentOverviewView)
					{
						Notify("tournament.model.reset", uICardButtonTournament2.tournamentData);
						uITournamentOverviewView.Set(uICardButtonTournament2.tournamentData, view.minimumSkill);
					}
				}
				break;
			}
			case "past-tournament-card@click":
			{
				UICardButtonTournament uICardButtonTournament = p_target as UICardButtonTournament;
				if (uICardButtonTournament.tournamentData != null)
				{
					Notify("tournament.model.reset", uICardButtonTournament.tournamentData);
					base.app.view.ui.screens.Open<UITournamentWinnersView>("tournament-leaders-screen").allowNext = true;
				}
				break;
			}
			case "ui.screen.return@click":
				StopDataSyncLoop();
				base.app.model.service.StopTournamentRefresh();
				base.app.view.ui.screens.Return();
				break;
			case "ui.screen@close":
				StopDataSyncLoop();
				break;
			}
		}

		protected void RefreshTournaments(bool p_screenOpen = false)
		{
			view.bigCardTournament = null;
			view.mediumCardTournament = null;
			UpdateCards(view.minimumSkill, p_screenOpen);
			view.RefreshNavigation();
		}

		private UITournamentsListCardType GetCardTypeForTournament(DRLTournamentData p_data)
		{
			DateTime currentTime = p_data.currentTime;
			DateTime registerStartDate = p_data.registerStartDate;
			DateTime registerEndDate = p_data.registerEndDate;
			double totalDays = (p_data.registerEndDate - p_data.currentTime).TotalDays;
			bool flag = currentTime.CompareTo(registerEndDate) < 0 && currentTime.CompareTo(registerStartDate) >= 0;
			UITournamentsListCardType result = UITournamentsListCardType.Invalid;
			if (p_data.status == TournamentState.active)
			{
				return UITournamentsListCardType.Active;
			}
			if (flag)
			{
				return UITournamentsListCardType.Registration;
			}
			if (p_data.status == TournamentState.complete)
			{
				return UITournamentsListCardType.Past;
			}
			if (totalDays > 0.0 && !flag)
			{
				return UITournamentsListCardType.Future;
			}
			return result;
		}

		private void UpdateCards(int p_minSkill, bool p_showFeedback)
		{
			if (p_showFeedback)
			{
				view.SetFeedback(UITournamentsListFeedbackType.Processing, p_hide_list: false, 0.1f);
			}
			view.ResetPastTournaments();
			view.ClearTournamentsQueues();
			if (tournamentsGet != null)
			{
				tournamentsGet.Cancel();
				tournamentsGet = null;
			}
			DRLTournamentData[] tournaments;
			tournamentsGet = base.app.model.service.GetTournaments(p_minSkill, delegate(DRLTournamentResult p_result)
			{
				if (!(this == null) && !(base.app == null) && !(base.app.view == null) && !(view == null) && !(base.gameObject == null) && base.validContext && view.current)
				{
					int num = 0;
					tournaments = p_result.tournaments;
					if (tournaments.Length != 0)
					{
						foreach (DRLTournamentData p_data in tournaments)
						{
							UITournamentsListCardType cardTypeForTournament = GetCardTypeForTournament(p_data);
							if (cardTypeForTournament != UITournamentsListCardType.Invalid)
							{
								view.AddTournament(p_data, cardTypeForTournament);
								num++;
							}
						}
						view.SetBigCard();
					}
					if (num == 0)
					{
						view.SetFeedback(UITournamentsListFeedbackType.NoTournaments, p_hide_list: false, 0.1f);
					}
					else
					{
						view.AddPads();
						view.SetFeedback(UITournamentsListFeedbackType.None, p_hide_list: false, 0.1f);
					}
					tournamentsGet = null;
					view.RefreshNavigation();
					this.TimerRunOnce(delegate
					{
						if (base.validContext && (!base.app.inGame || base.app.model.game.type != GameFlag.Race || !(base.app.model.game != null) || !(base.app.model.game.simulation != null) || !base.app.model.game.simulation.running))
						{
							GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
							Debug.Log("UITournamentsListController>  GC forced cleanup on tournaments refresh data..");
						}
					}, 0.1f);
				}
			}, -1, 12);
		}

		private void StartDataSyncLoop(float p_delay)
		{
			float time = 0f;
			SyncData(p_screenOpen: true);
			if (m_syncLoop != null)
			{
				m_syncLoop.Stop();
				m_syncLoop = null;
			}
			m_syncLoop = Run((Func<bool>)delegate
			{
				if (time > m_syncFreq)
				{
					SyncData();
					time = 0f;
				}
				time += Time.deltaTime;
				return true;
			}, p_delay, false);
		}

		private void StopDataSyncLoop()
		{
			foreach (UICardButtonTournament item in view.activeList.GetList<UICardButtonTournament>())
			{
				item.StopTimerActivity();
			}
			if (m_syncLoop != null && m_syncLoop.IsRunning)
			{
				m_syncLoop.Stop();
			}
		}

		public void SyncData(bool p_screenOpen = false)
		{
			RefreshTournaments(p_screenOpen);
		}

		protected void OnFormNotification(UnityEngine.Object p_target, bool p_is_change, string p_event)
		{
			switch (p_target.name)
			{
			case "newsletter":
				if (view.isPlayerSubscribed())
				{
					UnsubscribeUser();
				}
				else
				{
					SubscribeUser();
				}
				break;
			case "past-results":
				Debug.Log("past-results");
				break;
			}
		}

		private void SubscribeUser()
		{
			view.SetFeedback(UITournamentsListFeedbackType.Processing, p_hide_list: true, 0.1f);
			base.app.model.service.SubscribeUser(delegate(DRLServiceResult p_result)
			{
				if (!(this == null) && !(base.app == null) && !(base.app.view == null) && !(view == null) && !(base.gameObject == null))
				{
					view.SetFeedback(UITournamentsListFeedbackType.None, p_hide_list: false, 0f);
					if (p_result.success)
					{
						view.SetSubscriptionButtons(p_subscribed: true);
					}
				}
			});
		}

		private void UnsubscribeUser()
		{
			view.SetFeedback(UITournamentsListFeedbackType.Processing, p_hide_list: true, 0.1f);
			base.app.model.service.UnsubscribeUser(delegate(DRLServiceResult p_result)
			{
				if (!(this == null) && !(base.app == null) && !(base.app.view == null) && !(view == null) && !(base.gameObject == null))
				{
					view.SetFeedback(UITournamentsListFeedbackType.None, p_hide_list: false, 0f);
					if (p_result.success)
					{
						view.SetSubscriptionButtons(p_subscribed: false);
					}
				}
			});
		}
	}
}

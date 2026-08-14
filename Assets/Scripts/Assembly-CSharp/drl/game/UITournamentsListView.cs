using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;

namespace drl.game
{
	public class UITournamentsListView : UIScreenView
	{
		public Color subscribedColor;

		public Color notSubscribedColor;

		public Color stripeSubscribedColor;

		public Color stripeNotSubscribedColor;

		public Image newsletterBackground;

		public Image newsletterStripe;

		private bool newsletterState;

		[HideInInspector]
		public int minimumSkill;

		public DRLTournamentData bigCardTournament;

		public UITournamentsListCardType bigCardTType;

		public DRLTournamentData mediumCardTournament;

		public UITournamentsListCardType mediumCardTType;

		private Queue<DRLTournamentData> activeTournaments;

		private Queue<DRLTournamentData> registrationTournaments;

		private Queue<DRLTournamentData> futureTournaments;

		public UICardButtonTournament[] smallCardsList;

		private UICardButtonTournament bigCard;

		public UICardButtonTournament mediumCard;

		public ListComponent activeList;

		public LayoutGroup tournamentsLayout;

		public LayoutGroup completedLayout;

		public UINavigation backButtonNav;

		public FadeComponent listFade;

		public FadeComponent feedbackFade;

		public List<GameObject> feedbacks;

		private UITournamentsListFeedbackType status;

		private Dictionary<string, UICardButtonTournament> tournamentCardsById;

		private Dictionary<string, UICardButtonTournament> tournamentCardsForRemoval;

		private int tournamentPadsSmall;

		private int tournamentPadsMed;

		private int pastTournamentIndex;

		public void ClearTournamentsQueues()
		{
			if (activeTournaments == null)
			{
				activeTournaments = new Queue<DRLTournamentData>(2);
			}
			else
			{
				activeTournaments.Clear();
			}
			if (registrationTournaments == null)
			{
				registrationTournaments = new Queue<DRLTournamentData>(2);
			}
			else
			{
				registrationTournaments.Clear();
			}
			if (futureTournaments == null)
			{
				futureTournaments = new Queue<DRLTournamentData>(2);
			}
			else
			{
				futureTournaments.Clear();
			}
		}

		public void AddTournament(DRLTournamentData p_data, UITournamentsListCardType p_cardType)
		{
			if (p_data != null)
			{
				switch (p_cardType)
				{
				case UITournamentsListCardType.Active:
					activeTournaments.Enqueue(p_data);
					break;
				case UITournamentsListCardType.Registration:
					registrationTournaments.Enqueue(p_data);
					break;
				case UITournamentsListCardType.Past:
					SetSmallCard(p_data, p_cardType);
					break;
				case UITournamentsListCardType.Future:
					futureTournaments.Enqueue(p_data);
					break;
				}
			}
		}

		public void SetBigCard()
		{
			if (!base.validContext || activeList == null || activeTournaments == null || registrationTournaments == null || futureTournaments == null)
			{
				return;
			}
			activeList.Clear();
			if (activeTournaments.Count + registrationTournaments.Count + futureTournaments.Count == 0)
			{
				bigCard = activeList.Push<UICardButtonTournament>();
				bigCard.Set(null, UITournamentsListCardType.Invalid);
			}
			else
			{
				int count = activeTournaments.Count;
				int count2 = registrationTournaments.Count;
				int count3 = futureTournaments.Count;
				for (int i = 0; i < count; i++)
				{
					UICardButtonTournament uICardButtonTournament = activeList.Push<UICardButtonTournament>();
					if ((bool)uICardButtonTournament)
					{
						uICardButtonTournament.Set(activeTournaments.Dequeue(), UITournamentsListCardType.Active);
					}
				}
				for (int j = 0; j < count2; j++)
				{
					UICardButtonTournament uICardButtonTournament2 = activeList.Push<UICardButtonTournament>();
					if ((bool)uICardButtonTournament2)
					{
						uICardButtonTournament2.Set(registrationTournaments.Dequeue(), UITournamentsListCardType.Registration);
					}
				}
				for (int k = 0; k < count3; k++)
				{
					activeList.Push<UICardButtonTournament>().Set(futureTournaments.Dequeue(), UITournamentsListCardType.Future);
				}
			}
			RefreshNavigation();
		}

		public void SetMediumCard()
		{
			if (base.validContext && !(activeList == null) && activeTournaments != null && registrationTournaments != null && futureTournaments != null && (bool)mediumCard)
			{
				mediumCard.gameObject.SetActive(value: true);
				if (activeTournaments.Count > 0)
				{
					mediumCard.Set(activeTournaments.Dequeue(), UITournamentsListCardType.Active);
					return;
				}
				if (registrationTournaments.Count > 0)
				{
					mediumCard.Set(registrationTournaments.Dequeue(), UITournamentsListCardType.Registration);
					return;
				}
				if (futureTournaments.Count > 0)
				{
					mediumCard.Set(futureTournaments.Dequeue(), UITournamentsListCardType.Future);
					return;
				}
				mediumCard.Set(null, UITournamentsListCardType.Invalid);
				mediumCard.gameObject.SetActive(value: false);
			}
		}

		public void SetSmallCard(DRLTournamentData p_data, UITournamentsListCardType p_cardType)
		{
			if (p_data != null && base.validContext && smallCardsList != null && pastTournamentIndex < smallCardsList.Length)
			{
				_ = smallCardsList[pastTournamentIndex].tournamentData;
				smallCardsList[pastTournamentIndex].Set(p_data, p_cardType);
				pastTournamentIndex++;
			}
		}

		public void ClearTournaments()
		{
			if (tournamentCardsById == null)
			{
				tournamentCardsById = new Dictionary<string, UICardButtonTournament>();
			}
			else
			{
				tournamentCardsById.Clear();
			}
			tournamentPadsSmall = 0;
			tournamentPadsMed = 0;
		}

		public void ResetPastTournaments()
		{
			pastTournamentIndex = 0;
		}

		public void AddPads()
		{
			for (int i = pastTournamentIndex; i < smallCardsList.Length; i++)
			{
				smallCardsList[i].Set(null, UITournamentsListCardType.Invalid);
			}
		}

		public void CheckIsPlayerSubscribed()
		{
			base.app.model.service.CheckUserSubscription(delegate(DRLServiceResult p_result)
			{
				if (p_result == null || p_result.data == null)
				{
					Debug.LogWarning("UITournamentListView> User subscription result is null");
				}
				else if (Serialize.FromJson<DRLTournamentSubscription[]>(p_result.data.ToString()).Length != 0)
				{
					SetSubscriptionButtons(p_subscribed: true);
				}
				else
				{
					SetSubscriptionButtons(p_subscribed: false);
				}
			});
		}

		public bool isPlayerSubscribed()
		{
			return newsletterState;
		}

		public void SetSubscriptionButtons(bool p_subscribed)
		{
			if (p_subscribed)
			{
				if ((bool)newsletterBackground)
				{
					newsletterBackground.color = subscribedColor;
					newsletterStripe.color = stripeSubscribedColor;
				}
				newsletterState = true;
			}
			else
			{
				if ((bool)newsletterBackground)
				{
					newsletterBackground.color = notSubscribedColor;
					newsletterStripe.color = stripeNotSubscribedColor;
				}
				newsletterState = false;
			}
		}

		public void SetFeedback(UITournamentsListFeedbackType p_type, bool p_hide_list, float p_delay)
		{
			float feedback_alpha = ((p_type == UITournamentsListFeedbackType.None) ? (-0.1f) : 1f);
			float content_alpha = ((p_type == UITournamentsListFeedbackType.None) ? 1f : (p_hide_list ? (-0.1f) : 1f));
			status = p_type;
			Action action = delegate
			{
				feedbackFade.Fade(feedback_alpha, 0.2f, 0.05f, Cubic.Out);
				listFade.Fade(content_alpha, 0.3f, 0f, Cubic.Out);
				if (p_type != UITournamentsListFeedbackType.None)
				{
					int num = (int)p_type;
					for (int i = 0; i < feedbacks.Count; i++)
					{
						feedbacks[i].SetActive(i == num);
					}
				}
			};
			if (p_delay <= 0f)
			{
				action();
			}
			else
			{
				RunOnce(p_delay, action);
			}
		}

		public void RefreshNavigation()
		{
			List<UICardButtonTournament> list = activeList.GetList<UICardButtonTournament>();
			if (list.Count == 0)
			{
				return;
			}
			UINavigation nav = backButtonNav;
			if (activeList != null && activeList.Count > 0)
			{
				UINavigation.Link(tournamentsLayout, backButtonNav, completedLayout);
				nav = activeList.Get<UICardButtonTournament>(activeList.Count - 1).nav;
			}
			for (int i = 0; i < smallCardsList.Length; i++)
			{
				smallCardsList[i].nav.left = nav;
			}
			base.leftNavigation.right = list[0].nav;
			list[0].nav.left = base.leftNavigation;
			for (int j = 0; j < list.Count; j++)
			{
				if (j > 0)
				{
					list[j].nav.left = list[j - 1].nav;
				}
				if (j < list.Count - 1)
				{
					list[j].nav.right = list[j + 1].nav;
				}
				if (j == list.Count - 1)
				{
					list[j].nav.right = smallCardsList[0].nav;
				}
			}
			for (int k = 0; k < smallCardsList.Length; k++)
			{
				if (k < smallCardsList.Length - 1)
				{
					smallCardsList[k].nav.down = smallCardsList[k + 1].nav;
				}
				if (k > 0)
				{
					smallCardsList[k].nav.up = smallCardsList[k - 1].nav;
				}
				smallCardsList[k].nav.left = list[list.Count - 1].nav;
			}
		}
	}
}

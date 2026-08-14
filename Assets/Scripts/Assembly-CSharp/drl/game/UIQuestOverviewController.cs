using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIQuestOverviewController : Controller<DRLApp>
	{
		public UIQuestOverviewView view => AssertLocal<UIQuestOverviewView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
			{
				Debug.Log("222 UIQuestOverviewController.OnNotification(): p_event: " + p_event + ", p_target: " + p_target.name);
				if (p_data[0] as UIScreen != view.screen)
				{
					break;
				}
				if ((bool)view.data && view.onboardingData == null)
				{
					view.Set(view.data);
				}
				else if ((bool)view.onboardingData)
				{
					view.Set(view.onboardingData);
				}
				ServiceModel service2 = base.app.model.service;
				List<Component> list2 = new List<Component>();
				view.questCard.stars.fade.alpha = -0.1f;
				list2.Add(view.questCard);
				for (int j = 0; j < view.listField.Count; j++)
				{
					UICardButtonMission uICardButtonMission2 = view.listField.Get<UICardButtonMission>(j);
					uICardButtonMission2.stars.fade.alpha = -0.1f;
					uICardButtonMission2.stars.Clear();
					uICardButtonMission2.stars.SetProgress(0f);
					if ((bool)uICardButtonMission2)
					{
						list2.Add(uICardButtonMission2);
					}
				}
				view.questCard.stars.Clear();
				view.questCard.stars.SetProgress(0f);
				view.questCard.stars.fade.alpha = -0.1f;
				service2.PopulateLeaderboardQuest(view.questCard.data.missions, list2);
				UINavigation.Link(view.listField.GetComponent<LayoutGroup>(), view.leftNavigation);
				break;
			}
			case "missions.mission-card@click":
			{
				UICardButtonMission uICardButtonMission3 = p_target as UICardButtonMission;
				UIMissionOverviewView uIMissionOverviewView = base.app.view.ui.screens.Open<UIMissionOverviewView>("mission-overview-screen");
				uIMissionOverviewView.screen.title = uICardButtonMission3.data.title;
				uIMissionOverviewView.quest = view.data;
				uIMissionOverviewView.mission = uICardButtonMission3.data;
				base.app.arguments.game.mission = uIMissionOverviewView.mission;
				base.app.arguments.game.map = (uIMissionOverviewView.mission ? uIMissionOverviewView.mission.map : null);
				base.app.arguments.game.track = (uIMissionOverviewView.mission ? uIMissionOverviewView.mission.track : null);
				break;
			}
			case "onboarding.enter.menu@click":
			{
				if (p_data[0] as UIScreen != view.screen)
				{
					break;
				}
				view.Set(view.onboardingData);
				ServiceModel service = base.app.model.service;
				List<Component> list = new List<Component>();
				view.questCard.stars.fade.alpha = -0.1f;
				list.Add(view.questCard);
				for (int i = 0; i < view.listField.Count; i++)
				{
					UICardButtonMission uICardButtonMission = view.listField.Get<UICardButtonMission>(i);
					uICardButtonMission.stars.fade.alpha = -0.1f;
					uICardButtonMission.stars.Clear();
					uICardButtonMission.stars.SetProgress(0f);
					if ((bool)uICardButtonMission)
					{
						list.Add(uICardButtonMission);
					}
				}
				view.questCard.stars.Clear();
				view.questCard.stars.SetProgress(0f);
				view.questCard.stars.fade.alpha = -0.1f;
				service.PopulateLeaderboardQuest(view.questCard.data.missions, list);
				UINavigation.Link(view.listField.GetComponent<LayoutGroup>(), view.leftNavigation);
				break;
			}
			case "ui.screen.return@click":
				view.onboardingData = null;
				base.app.view.ui.screens.Return();
				break;
			}
		}
	}
}

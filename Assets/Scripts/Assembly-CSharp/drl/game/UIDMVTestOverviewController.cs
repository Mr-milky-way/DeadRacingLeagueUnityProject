using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIDMVTestOverviewController : Controller<DRLApp>
	{
		public UIDMVTestOverviewView view => AssertLocal<UIDMVTestOverviewView>("view");

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
				if (p_data[0] as UIScreen != view.screen)
				{
					break;
				}
				view.Set(view.data);
				ServiceModel service = base.app.model.service;
				List<Component> list = new List<Component>();
				list.Add(view.questCard);
				for (int i = 0; i < view.listField.Count; i++)
				{
					UICardButtonMission uICardButtonMission2 = view.listField.Get<UICardButtonMission>(i);
					uICardButtonMission2.stars.fade.alpha = -0.1f;
					uICardButtonMission2.stars.Clear();
					uICardButtonMission2.stars.SetProgress(0f);
					if ((bool)uICardButtonMission2)
					{
						list.Add(uICardButtonMission2);
					}
				}
				service.PopulateLeaderboardQuest(view.questCard.data.missions, list);
				UINavigation.Link(view.listField.GetComponent<LayoutGroup>(), view.leftNavigation);
				break;
			}
			case "missions.mission-card@click":
			{
				UICardButtonMission uICardButtonMission = p_target as UICardButtonMission;
				UIMissionOverviewView uIMissionOverviewView = base.app.view.ui.screens.Open<UIMissionOverviewView>("mission-overview-screen");
				uIMissionOverviewView.screen.title = uICardButtonMission.data.title;
				uIMissionOverviewView.quest = view.data;
				uIMissionOverviewView.mission = uICardButtonMission.data;
				base.app.arguments.game.mission = uIMissionOverviewView.mission;
				base.app.arguments.game.map = (uIMissionOverviewView.mission ? uIMissionOverviewView.mission.map : null);
				base.app.arguments.game.track = (uIMissionOverviewView.mission ? uIMissionOverviewView.mission.track : null);
				break;
			}
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			}
		}
	}
}

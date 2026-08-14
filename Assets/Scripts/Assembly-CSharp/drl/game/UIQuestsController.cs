using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIQuestsController : Controller<DRLApp>
	{
		public UIQuestsView view => AssertLocal<UIQuestsView>("view");

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
				List<DRLQuest> quests = base.app.model.storage.GetQuests(GameFlag.Training);
				Debug.Log("UITrainController> Open - Found> [" + quests.Count + "] Quests");
				view.ClearQuests();
				for (int i = 0; i < quests.Count; i++)
				{
					DRLQuest p_item = quests[i];
					view.AddQuest(p_item);
				}
				ServiceModel service = base.app.model.service;
				List<Component> list = new List<Component>();
				for (int j = 0; j < view.listField.Count; j++)
				{
					UICardButtonQuest uICardButtonQuest2 = view.listField.Get<UICardButtonQuest>(j);
					if ((bool)uICardButtonQuest2)
					{
						list.Clear();
						list.Add(uICardButtonQuest2);
						service.PopulateLeaderboardQuest(uICardButtonQuest2.data.missions, list);
					}
				}
				UINavigation.Link(view.listField.GetComponent<LayoutGroup>(), view.leftNavigation);
				break;
			}
			case "missions.quest-card@click":
			{
				UICardButtonQuest uICardButtonQuest = p_target as UICardButtonQuest;
				UIQuestOverviewView uIQuestOverviewView = base.app.view.ui.screens.Open<UIQuestOverviewView>("quest-overview-screen");
				uIQuestOverviewView.screen.title = uICardButtonQuest.data.title;
				uIQuestOverviewView.data = uICardButtonQuest.data;
				base.app.arguments.game.quest = uIQuestOverviewView.data;
				break;
			}
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			}
		}
	}
}

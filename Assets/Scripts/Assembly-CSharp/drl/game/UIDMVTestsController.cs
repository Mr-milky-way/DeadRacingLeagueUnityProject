using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIDMVTestsController : Controller<DRLApp>
	{
		private Vector2 m_initSize;

		public UIDMVTestsView view => AssertLocal<UIDMVTestsView>("view");

		public UINavigationScroll scroll => AssertLocal<UINavigationScroll>("scroll");

		public UIScreen screen => AssertLocal<UIScreen>("screen");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen || p_event == null)
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
				List<DRLQuest> quests = base.app.model.storage.GetQuests(GameFlag.DMVQuest);
				Debug.Log("UITrainController> Open - Found> [" + quests.Count + "] Quests");
				view.ClearQuests();
				for (int num6 = 0; num6 < quests.Count; num6++)
				{
					DRLQuest p_item = quests[num6];
					view.AddQuest(p_item);
				}
				ServiceModel service2 = base.app.model.service;
				List<Component> list4 = new List<Component>();
				List<DRLMission> list5 = new List<DRLMission>();
				for (int num7 = 0; num7 < view.listField.Count; num7++)
				{
					UICardButtonDmvTest uICardButtonDmvTest2 = view.listField.Get<UICardButtonDmvTest>(num7);
					if ((bool)uICardButtonDmvTest2)
					{
						list4.Add(uICardButtonDmvTest2);
						if (uICardButtonDmvTest2.data.testMission != null)
						{
							list5.Add(uICardButtonDmvTest2.data.testMission);
						}
					}
				}
				service2.PopulateLeaderboardQuest(list5, list4);
				list4.Clear();
				UINavigation.Link(view.listField.GetComponent<LayoutGroup>(), view.leftNavigation);
				SetTotalTime();
				SetNavigation();
				SetExitButton();
				break;
			}
			case "test.submenu@opened":
			{
				float num4 = (float)p_data[0];
				scroll.SetContentSizeChanging(num4 * 2f);
				List<UICardButtonDmvTest> list3 = view.listField.GetList<UICardButtonDmvTest>();
				for (int num5 = 0; num5 < list3.Count; num5++)
				{
					list3[num5].UpdateSubmenuNavigation((num5 < list3.Count - 1) ? list3[num5 + 1].navigation : null);
				}
				break;
			}
			case "test.submenu@closed":
			{
				float num8 = (float)p_data[0];
				scroll.SetContentSizeChanging(num8 * 2f);
				List<UICardButtonDmvTest> list6 = view.listField.GetList<UICardButtonDmvTest>();
				for (int num9 = 0; num9 < list6.Count; num9++)
				{
					list6[num9].UpdateSubmenuNavigation((num9 < list6.Count - 1) ? list6[num9 + 1].navigation : null);
				}
				break;
			}
			case "missions.mission-card@click":
			{
				UICardButtonLesson uICardButtonLesson = p_target as UICardButtonLesson;
				UIMissionOverviewView uIMissionOverviewView = base.app.view.ui.screens.Open<UIMissionOverviewView>("lesson-overview-screen");
				uIMissionOverviewView.screen.title = uICardButtonLesson.data.title;
				uIMissionOverviewView.quest = uICardButtonLesson.questData;
				uIMissionOverviewView.mission = uICardButtonLesson.data;
				base.app.arguments.game.mission = uIMissionOverviewView.mission;
				base.app.arguments.game.map = (uIMissionOverviewView.mission ? uIMissionOverviewView.mission.map : null);
				base.app.arguments.game.track = (uIMissionOverviewView.mission ? uIMissionOverviewView.mission.track : null);
				base.app.arguments.game.quest = uIMissionOverviewView.quest;
				break;
			}
			case "missions.test-card@click":
				if (p_data.Length != 0)
				{
					UICardButtonDmvTest uICardButtonDmvTest3 = p_data[0] as UICardButtonDmvTest;
					UIMissionOverviewView uIMissionOverviewView2 = base.app.view.ui.screens.Open<UIMissionOverviewView>("test-overview-screen");
					uIMissionOverviewView2.screen.title = uICardButtonDmvTest3.data.testMission.title;
					uIMissionOverviewView2.quest = uICardButtonDmvTest3.data;
					uIMissionOverviewView2.mission = uICardButtonDmvTest3.data.testMission;
					base.app.arguments.game.mission = uIMissionOverviewView2.mission;
					base.app.arguments.game.map = (uIMissionOverviewView2.mission ? uIMissionOverviewView2.mission.map : null);
					base.app.arguments.game.track = (uIMissionOverviewView2.mission ? uIMissionOverviewView2.mission.track : null);
					base.app.arguments.game.quest = uIMissionOverviewView2.quest;
				}
				break;
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				view.ClearInfo();
				{
					foreach (UICardButtonDmvTest item3 in view.listField.GetList<UICardButtonDmvTest>())
					{
						if (item3.menuOpened)
						{
							item3.FoldLessonsMenu();
						}
					}
					break;
				}
			case "ui.screen@close":
			{
				foreach (UICardButtonDmvTest item4 in view.listField.GetList<UICardButtonDmvTest>())
				{
					item4.ClearDataReceived();
				}
				break;
			}
			case "missions.dmv.total-progress":
			{
				if (p_data.Length == 0)
				{
					break;
				}
				int progress = (int)p_data[0];
				ServiceModel service = base.app.model.service;
				List<DRLMission> list = new List<DRLMission>();
				List<Component> list2 = new List<Component>();
				for (int num = 0; num < view.listField.Count; num++)
				{
					UICardButtonDmvTest uICardButtonDmvTest = view.listField.Get<UICardButtonDmvTest>(num);
					if (!uICardButtonDmvTest)
					{
						continue;
					}
					foreach (DRLMission mission in uICardButtonDmvTest.data.missions)
					{
						list.Add(mission);
					}
					for (int num2 = 0; num2 < uICardButtonDmvTest.lessonsMenuFirstRow.Count; num2++)
					{
						UICardButtonLesson item = uICardButtonDmvTest.lessonsMenuFirstRow.Get<UICardButtonLesson>(num2);
						list2.Add(item);
					}
					for (int num3 = 0; num3 < uICardButtonDmvTest.lessonsMenuSecondRow.Count; num3++)
					{
						UICardButtonLesson item2 = uICardButtonDmvTest.lessonsMenuSecondRow.Get<UICardButtonLesson>(num3);
						list2.Add(item2);
					}
				}
				service.PopulateLeaderboardQuest(list, list2);
				list.Clear();
				list2.Clear();
				SetProgress(progress);
				break;
			}
			case "missions.scoring.set":
				RunOnce(delegate
				{
					view.CheckLessonSetCompleteTestUnlock();
				}, 0.8f);
				break;
			case "test.toggle.submenu@click":
				(p_target as UIElementView).transform.parent.GetComponent<UICardButtonDmvTest>().ToggleLessonsMenu();
				break;
			case "missions.mission-complete.exit@click":
				base.enabled = false;
				base.app.view.audio.PlayUIGenericSuccess();
				base.app.controller.game.Exit();
				break;
			}
		}

		private void SetTotalTime()
		{
			if (!(base.app.model.storage.state.player == null))
			{
				view.SetTotalTime((int)base.app.model.storage.state.player.dmvUserTotalTime);
			}
		}

		private void SetProgress(int p_progress)
		{
			view.SetProgress((float)p_progress / (float)view.listField.Count);
			p_progress = Mathf.Clamp(p_progress, 0, view.listField.Count);
			view.currentLevel = p_progress;
			base.app.model.storage.state.player.userRank = p_progress;
		}

		private void SetNavigation()
		{
			List<UICardButtonDmvTest> list = view.listField.GetList<UICardButtonDmvTest>();
			if (list.Count == 0)
			{
				return;
			}
			view.backButtonNavigation.right = list[0].navigation;
			list[0].navigation.left = view.backButtonNavigation;
			if (list.Count > 1)
			{
				list[0].UpdateSubmenuNavigation(list[1].navigation);
				for (int i = 1; i < list.Count; i++)
				{
					list[i].UpdateSubmenuNavigation((i < list.Count - 1) ? list[i + 1].navigation : null);
				}
			}
			else
			{
				list[0].UpdateSubmenuNavigation(null);
			}
		}

		private void SetExitButton()
		{
			if (base.app.controller.game != null)
			{
				view.exitButton.SetActive(value: true);
				view.backButtonNavigation.down = view.exitButtonNavigation;
				view.exitButtonNavigation.up = view.backButtonNavigation;
				view.exitButtonNavigation.right = view.listField.Get<UICardButtonDmvTest>(0).navigation;
			}
			else
			{
				view.exitButton.SetActive(value: false);
				view.backButtonNavigation.down = null;
				view.exitButtonNavigation.right = null;
				view.exitButtonNavigation.up = null;
			}
		}

		private void Update()
		{
		}
	}
}

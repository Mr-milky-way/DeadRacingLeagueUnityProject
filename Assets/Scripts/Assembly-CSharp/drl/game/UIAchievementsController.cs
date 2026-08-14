using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIAchievementsController : Controller<DRLApp>
	{
		private DRLAchievementData[] allAchievements;

		public int pageLength = 7;

		private int currentPage;

		[SerializeField]
		private UINavigation leftNavigation;

		[SerializeField]
		private UINavigation bottomRightNavigation;

		[SerializeField]
		private UINavigation bottomLeftNavigation;

		public UIAchievementsView view => AssertLocal<UIAchievementsView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				view.SetFeedback(p_state: true, 0f);
				GetAchievements();
				break;
			case "ui.screen@close":
				view.Clear();
				break;
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			case "settings.profile.achievements.detail@click":
				Debug.Log("UIAchievementsController> AchievementsDetailClick");
				base.app.view.ui.screens.Open("achievement-details-screen");
				break;
			case "achievements.state@select":
			{
				view.SetFeedback(p_state: true, 0f);
				int p_page = (currentPage = (int)p_data[0]);
				Debug.Log("UIAchievementsController> Page Select [" + p_page + "]");
				UpdatePage(p_page, pageLength);
				break;
			}
			case "achievements.page-next@click":
				view.SetFeedback(p_state: true, 0f);
				if (view.pageField.index + 1 != view.pageField.listField.Count)
				{
					view.pageField.index = view.pageField.index + 1;
					currentPage = view.pageField.index;
					UpdatePage(currentPage, pageLength);
				}
				break;
			case "achievements.page-previous@click":
				view.SetFeedback(p_state: true, 0f);
				if (view.pageField.index != 0)
				{
					view.pageField.index = view.pageField.index - 1;
					currentPage = view.pageField.index;
					UpdatePage(currentPage, pageLength);
				}
				break;
			case "home.garage@click":
			{
				UICommunityDronesView uICommunityDronesView = base.app.view.ui.screens.Open<UICommunityDronesView>("community-drones-screen");
				uICommunityDronesView.inGame = false;
				uICommunityDronesView.showMyDrones = true;
				uICommunityDronesView.showCreateButton = true;
				uICommunityDronesView.screen.title = base.app.model.storage.locale.Get("garage.selection-screen.title", "Drones");
				break;
			}
			}
		}

		public void GetAchievements()
		{
			Debug.Log("UIAchievementsController> GetAchievements");
			base.app.model.service.GetAchievements(delegate(DRLAchievementResult result)
			{
				if (result.list.Length != 0)
				{
					allAchievements = result.list;
					UpdatePage(0, 7);
					view.UpdateAchievementList(allAchievements.ToList(), 0, 7);
				}
			}, base.app.model.storage.state.player.playerData.playerId);
		}

		public void UpdatePage(int p_page, int p_total, float p_refreshDelay = 0.5f)
		{
			view.Clear();
			List<DRLAchievementData> list = new List<DRLAchievementData>();
			int num = p_page * p_total;
			int num2 = p_page * p_total + p_total;
			for (int i = num; i < num2 && i < allAchievements.Length; i++)
			{
				list.Add(allAchievements[i]);
				view.AddAchievement(list[i - num]);
			}
			ConfigureNavigation();
			view.SetFeedback(p_state: false, 0.5f);
		}

		private void ConfigureNavigation()
		{
			for (int num = view.listField.Count - 1; num >= 0; num--)
			{
				if (num != 0)
				{
					view.listField[num].GetComponentInChildren<UINavigation>().up = view.listField[num - 1].GetComponentInChildren<UINavigation>();
				}
				if (num != view.listField.Count - 1)
				{
					view.listField[num].GetComponentInChildren<UINavigation>().down = view.listField[num + 1].GetComponentInChildren<UINavigation>();
				}
			}
			view.listField[0].GetComponentInChildren<UINavigation>().left = leftNavigation;
			view.listField[view.listField.Count - 1].GetComponentInChildren<UINavigation>().down = bottomLeftNavigation;
			bottomLeftNavigation.up = view.listField[view.listField.Count - 1].GetComponentInChildren<UINavigation>();
			bottomRightNavigation.up = view.listField[view.listField.Count - 1].GetComponentInChildren<UINavigation>();
			this.ActivityRunOnce(delegate
			{
				bottomLeftNavigation.right = view.pageField.listField[0].GetComponent<UINavigation>();
				view.pageField.listField[0].GetComponent<UINavigation>().left = bottomLeftNavigation;
				bottomRightNavigation.left = view.pageField.listField[view.pageField.listField.Count - 1].GetComponent<UINavigation>();
				view.pageField.listField[view.pageField.listField.Count - 1].GetComponent<UINavigation>().right = bottomRightNavigation;
				for (int i = 0; i < view.pageField.listField.Count; i++)
				{
					view.pageField.listField[i].GetComponent<UINavigation>().up = view.listField[view.listField.Count - 1].GetComponentInChildren<UINavigation>();
				}
			});
		}
	}
}

using System.Linq;
using UnityEngine;
using drl.backend;
using thelab.mvc;

namespace drl.game
{
	public class UIAchievementRequirementsController : Controller<DRLApp>
	{
		private DRLAchievementRequirementsData[] allAchievementRequirements;

		public UIAchievementRequirementsView view => AssertLocal<UIAchievementRequirementsView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				view.SetFeedback(p_state: true, 0f);
				if (view.data != null)
				{
					GetAchievementRequirements(view.data.id);
					GetComponentInChildren<AchievementCardView>().Set(view.data);
				}
				break;
			case "settings.profile.achievements.detail.update":
			{
				DRLAchievementData dRLAchievementData = (DRLAchievementData)p_data[0];
				if (dRLAchievementData.hasRequirements)
				{
					view.data = dRLAchievementData;
					GetAchievementRequirements(dRLAchievementData.id);
					GetComponentInChildren<AchievementCardView>().Set(dRLAchievementData);
				}
				break;
			}
			case "ui.screen.return@click":
				view.Reset();
				GetComponentInChildren<AchievementCardView>().Reset();
				base.app.view.ui.screens.Return();
				break;
			case "ui.screen@close":
				view.Reset();
				GetComponentInChildren<AchievementCardView>().Reset();
				break;
			}
		}

		public void GetAchievementRequirements(string p_achievementID)
		{
			Debug.Log("UIAchievementRequirementsController-> GetAchievementRequirements " + p_achievementID);
			base.app.model.service.GetAchievementRequirements(delegate(DRLAchievementRequirementsResult result)
			{
				int p_column_count = 2;
				if (result.list.Length != 0)
				{
					allAchievementRequirements = result.list;
					view.UpdateAchievementRequirementsList(allAchievementRequirements.ToList(), p_column_count);
					view.SetFeedback(p_state: false, 0.5f);
				}
			}, base.app.model.storage.state.player.playerData.playerId, p_achievementID);
		}
	}
}

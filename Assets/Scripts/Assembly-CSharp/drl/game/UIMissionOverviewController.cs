using System.Collections.Generic;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIMissionOverviewController : Controller<DRLApp>
	{
		public UIMissionOverviewView view => AssertLocal<UIMissionOverviewView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
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
					view.Set(view.quest, view.mission);
					if ((bool)view.missionCard)
					{
						view.missionCard.stars.fade.alpha = -0.1f;
						view.missionCard.stars.Clear();
						view.missionCard.stars.SetProgress(0f);
						base.app.model.service.PopulateLeaderboardQuest(p_targets: new List<Component> { view.missionCard }, p_mission: view.missionCard.data, p_delay_step: 0.2f);
					}
					else if ((bool)view.lessonCard)
					{
						view.lessonCard.SetScore(0f);
						base.app.model.service.PopulateLeaderboardQuest(p_targets: new List<Component> { view.lessonCard }, p_mission: view.lessonCard.data, p_delay_step: 0.2f);
					}
					RunOnce(0.5f, delegate
					{
						UINavigation.focus = view.rightNavigation;
					});
				}
				break;
			case "missions.mission-overview.form.event@click":
				OnFormNotification(p_target, p_change: false);
				break;
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			case "missions.mission-overview.start@click":
				if (view.mission != null && view.mission.tag == "DiagnosticTest")
				{
					base.app.model.storage.state.player.dmvWelcomeScreen = true;
				}
				break;
			}
		}

		protected void OnFormNotification(Object p_target, bool p_change)
		{
			switch (p_target ? p_target.name : "")
			{
			case "drone":
			{
				List<DRLDroneRig> drone = view.mission.drone;
				if (drone != null && drone.Count > 1)
				{
					base.app.view.ui.screens.Open<UIDroneSelectionView>("drone-selection-screen");
				}
				break;
			}
			case "mode-intermediate":
				view.modeIntermediate.GetComponent<FadeComponent>().FadeIn(0f);
				view.modePro.GetComponent<FadeComponent>().Fade(view.inactiveFMFadeAmount, 0f);
				base.app.model.storage.state.player.activeFCModeMissions = FCMode.Intermediate;
				break;
			case "mode-pro":
				view.modePro.GetComponent<FadeComponent>().FadeIn(0f);
				view.modeIntermediate.GetComponent<FadeComponent>().Fade(view.inactiveFMFadeAmount, 0f);
				base.app.model.storage.state.player.activeFCModeMissions = FCMode.Pro;
				break;
			}
		}
	}
}

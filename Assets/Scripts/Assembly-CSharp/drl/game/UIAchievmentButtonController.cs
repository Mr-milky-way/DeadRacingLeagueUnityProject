using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIAchievmentButtonController : Controller<DRLApp>
	{
		private UIHomeController home;

		public UIAchievementButtonView view => AssertLocal<UIAchievementButtonView>("view");

		protected override void Start()
		{
			base.Start();
			List<UIHomeController> list = Hierarchy.FindAll<UIHomeController>(base.app.transform);
			if (list != null && list.Count > 0)
			{
				home = list[0];
			}
		}

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "community-drones.create-new3@click":
				OpenDroneSelectionScreen(3);
				break;
			case "community-drones.create-new4@click":
				OpenDroneSelectionScreen(4);
				break;
			case "community-drones.create-new5@click":
				OpenDroneSelectionScreen(5);
				break;
			case "community-drones.create-new6@click":
				OpenDroneSelectionScreen(6);
				break;
			case "community-drones.create-new7@click":
				OpenDroneSelectionScreen(7);
				break;
			case "home.missions@click":
				base.app.arguments.Clear();
				base.app.arguments.game.type = GameFlag.Mission;
				base.app.arguments.game.mode = GameFlag.SinglePlayer;
				base.app.view.ui.screens.Open("quests-screen", 0f);
				break;
			case "settings.profile.achievements.detail@click":
			{
				DRLAchievementData achievementData = GetComponentInParent<AchievementCardView>().AchievementData;
				Debug.Log("UIAchievementButtonController-> AchievementsDetailClick " + GetComponentInParent<AchievementCardView>().gameObject.name + " " + achievementData.title);
				UIAchievementButtonView uIAchievementButtonView = p_target as UIAchievementButtonView;
				if (achievementData.id == "")
				{
					Debug.Log("DRLAchievementData is empty");
				}
				else if (uIAchievementButtonView != null && uIAchievementButtonView.AchievementID == view.AchievementID)
				{
					Debug.Log("UIAchievementButtonController-> AchievementsDetailClick: Matched Achievement");
					Notify(1f, "settings.profile.achievements.detail.update", achievementData);
				}
				else
				{
					Debug.Log("UIAchievementButtonController-> AchievementsDetailClick: Achievement Doesn't Match");
				}
				break;
			}
			case "home.race@click":
			{
				UIMapsCategoryView uIMapsCategoryView = base.app.view.ui.screens.Open<UIMapsCategoryView>("maps-category-screen");
				uIMapsCategoryView.screen.title = base.app.model.storage.locale.Get("home.card.race", "Solo Race");
				uIMapsCategoryView.caller = this;
				SetAppArguments(GameFlag.Race, GameFlag.SinglePlayer);
				break;
			}
			case "home.multiplayer@click":
				if (!IsOffline())
				{
					home.CheckMultiplayerAvailability(delegate
					{
						base.app.view.ui.screens.Open("multiplayer-lobby-screen", 0f);
					});
				}
				break;
			}
		}

		private void OpenDroneSelectionScreen(int p_droneClass)
		{
			UIGarageRigSelectionView uIGarageRigSelectionView = base.app.view.ui.screens.Open<UIGarageRigSelectionView>("garage-rig-selection-screen");
			uIGarageRigSelectionView.screen.title = base.app.model.storage.locale.Get("multiplayer.select-drone-screen.title", "Select your Drone");
			uIGarageRigSelectionView.SetCreationEnabled(p_flag: false);
			uIGarageRigSelectionView.allowCustomPhysics = true;
			uIGarageRigSelectionView.selectionOnly = true;
			uIGarageRigSelectionView.unlockedRigsOnly = true;
			uIGarageRigSelectionView.openedAsTemplateSelector = true;
			if (p_droneClass > 1)
			{
				uIGarageRigSelectionView.SetDroneClassEnabled(true);
				uIGarageRigSelectionView.overrideList = null;
				uIGarageRigSelectionView.overrideSizes = new List<int>(1) { p_droneClass };
			}
			else
			{
				uIGarageRigSelectionView.SetDroneClassEnabled(true);
				uIGarageRigSelectionView.overrideList = null;
				uIGarageRigSelectionView.overrideSizes = null;
			}
		}

		private void SetAppArguments(GameFlag p_type, GameFlag p_mode)
		{
			base.app.arguments.Clear();
			base.app.arguments.game.type = p_type;
			base.app.arguments.game.mode = p_mode;
			base.app.arguments.game.AddPlayer(base.app.model.storage.state.player.playerData);
		}

		private bool IsOffline()
		{
			bool offline = DRLApp.offline;
			if (offline)
			{
				base.app.view.ui.dialog.Open(DialogTemplateType.OfflineMode, "no-connection");
			}
			return offline;
		}
	}
}

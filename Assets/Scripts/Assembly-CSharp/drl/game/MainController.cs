using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class MainController : Controller<DRLApp>
	{
		public string startScreen;

		private static bool isFirstPaywall = true;

		protected void Awake()
		{
			base.app.view.ui.fade.FadeIn(0f);
		}

		protected void FadeIn()
		{
			base.app.view.audio.MuteFadeIn();
			base.app.view.audio.PlayMusicMain();
			string p_id = "home-screen-grid";
			if (!base.app.model.onboarding.skipOnboarding || base.app.model.onboarding.firstStart)
			{
				UIOnobardingMenuView uIOnobardingMenuView = base.app.view.ui.screens.Open<UIOnobardingMenuView>("onboarding-home-screen", 0.3f);
				uIOnobardingMenuView.backNav.gameObject.SetActive(value: false);
				base.app.view.ui.fade.FadeOut(1.5f, 0.3f);
				uIOnobardingMenuView.PlayVideo();
			}
			else
			{
				base.app.view.ui.screens.Open(p_id);
				base.app.view.ui.fade.FadeOut(1.5f, 0.3f);
			}
		}

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "storage.state@refresh":
				break;
			case "storage.state@parse":
				break;
			case "scene.start":
				base.app.view.ui.footer.SetColors(p_ingame: false);
				break;
			case "settings.ready":
				DRLApp.LogMemStats("MainController> Settings Ready", p_show_delta: true);
				if ((bool)base.app.acs)
				{
					base.app.acs.guiAllowed = base.app.model.storage.state.player.profile.isDeveloper;
				}
				Activity.RunOnce(base.app.scene.LogBundleLibraryStats, 2f);
				FadeIn();
				Activity.RunOnce(delegate
				{
					Scene garageScene = SceneManager.GetSceneByName("garage");
					if (!garageScene.IsValid())
					{
						AsyncOperation garageLoad = SceneManager.LoadSceneAsync("garage", LoadSceneMode.Additive);
						garageScene = SceneManager.GetSceneByName("garage");
						Activity.Run((Func<bool>)delegate
						{
							if (!garageLoad.isDone)
							{
								return true;
							}
							if (!garageScene.isLoaded)
							{
								return true;
							}
							GameObject[] rootGameObjects = garageScene.GetRootGameObjects();
							for (int i = 0; i < rootGameObjects.Length; i++)
							{
								if (rootGameObjects[i].name == "environment")
								{
									rootGameObjects[i].SetActive(value: false);
								}
							}
							return false;
						}, 0f, false);
					}
				}, 2f);
				base.app.model.storage.state.player.garage.ClearPhysicsOnOriginals();
				break;
			case "home.debug.dmv@click":
			case "home.DMV@click":
			case "home.missions@click":
				base.app.arguments.Clear();
				base.app.arguments.game.type = GameFlag.Mission;
				base.app.arguments.game.mode = GameFlag.SinglePlayer;
				break;
			case "missions.mission-overview.start@click":
				Debug.Log("MainController> MissionOverview - Start Click");
				if ((bool)base.app.view.ui.screens.manager.GetOpen<UIMissionOverviewView>())
				{
					base.enabled = false;
					base.app.arguments.game.players.Clear();
					base.app.arguments.game.AddPlayer(base.app.model.storage.state.player.playerData);
					base.app.view.audio.SceneMainToGame(1.6f);
					base.app.view.ui.fade.FadeIn(1.5f);
					Activity.RunOnce(base.app.scene.Load, 1.6f);
					base.app.model.storage.state.license.Poll();
				}
				break;
			case "missions.test-overview.start@click":
				base.enabled = false;
				base.app.arguments.game.players.Clear();
				base.app.arguments.game.AddPlayer(base.app.model.storage.state.player.playerData);
				base.app.view.audio.SceneMainToGame(1.6f);
				base.app.view.ui.fade.FadeIn(1.5f);
				Activity.RunOnce(base.app.scene.Load, 1.6f);
				base.app.model.storage.state.license.Poll();
				break;
			case "fly.circuits-overview.ready":
			case "fly.map-track-overview.ready":
				if (p_data.Length == 0 || !(p_data[0] is MapLoadData mapLoadData) || mapLoadData.baseMap == null)
				{
					break;
				}
				if (mapLoadData.baseTrack == null)
				{
					mapLoadData.baseTrack = base.app.model.storage.GetMapTracks(mapLoadData.baseMap, GameFlag.Freestyle)[0];
				}
				base.enabled = false;
				base.app.arguments.game.map = mapLoadData.baseMap;
				base.app.arguments.game.track = mapLoadData.baseTrack;
				base.app.arguments.game.podium = mapLoadData.baseTrack.podium;
				base.app.arguments.game.fcMode = base.app.model.storage.state.player.activeFCMode;
				base.app.arguments.game.allowCrash = false;
				base.app.arguments.game.opponentType = mapLoadData.opponentMode;
				if (ReplayFile.EnableVersion2)
				{
					if (mapLoadData.opponentRecordV2 != null)
					{
						base.app.arguments.game.AddGhostPlayer(mapLoadData.opponentRecordV2);
					}
				}
				else if (mapLoadData.opponentRecord != null)
				{
					base.app.model?.service?.opponent?.TryAddLoadedReplay(mapLoadData.opponentRecord);
					base.app.arguments.game.AddGhostPlayer(mapLoadData.opponentRecord);
				}
				if (p_data.Length != 0)
				{
					p_data[0] = null;
				}
				base.app.model.service.opponent.ghostRecords = null;
				if (base.app.model.service.opponent.ghostRecordsV2 != null)
				{
					base.app.model.service.opponent.ghostRecordsV2.Destroy();
					base.app.model.service.opponent.ghostRecordsV2 = null;
				}
				if (base.app.arguments.game.type == GameFlag.Campaign)
				{
					DRLCampaign campaign = base.app.arguments.game.campaign;
					if ((bool)campaign && !string.IsNullOrEmpty(campaign.podium))
					{
						base.app.arguments.game.podium = campaign.podium;
					}
				}
				if (mapLoadData.isCustom)
				{
					base.app.view.ui.fade.FadeIn(1.5f);
					base.app.scene.LoadCommunityMap(mapLoadData.customMap.guid, 7f, delegate
					{
						base.app.view.audio.SceneMainToGame(0f);
						base.app.view.ui.fade.FadeIn(1.5f);
					}, mapLoadData.customMap.version);
				}
				else
				{
					base.app.view.audio.SceneMainToGame(0f);
					base.app.view.ui.fade.FadeIn(1.5f);
					Activity.RunOnce(base.app.scene.Load, 3f);
				}
				base.app.view.ui.navigation.enabled = false;
				base.app.model.storage.state.license.Poll();
				break;
			case "garage.edit.fly.ready":
			{
				base.enabled = false;
				base.app.arguments.game.players.Clear();
				base.app.arguments.game.AddPlayer(base.app.model.storage.state.player.playerData);
				base.app.arguments.game.mode = GameFlag.SinglePlayer;
				base.app.arguments.game.type = GameFlag.Sandbox;
				DRLMapTrack dRLMapTrack = null;
				switch ((p_data.Length >= 3) ? ((int)p_data[2]) : 0)
				{
				case 0:
					dRLMapTrack = base.app.model.storage.library.FindByGUID<DRLMapTrack>("MT-9ea");
					break;
				case 1:
					dRLMapTrack = base.app.model.storage.library.FindByGUID<DRLMapTrack>("MT-1a7");
					break;
				}
				DRLMap map = dRLMapTrack.map;
				base.app.view.audio.PlayUIStartGame();
				base.app.arguments.game.map = map;
				base.app.arguments.game.track = dRLMapTrack;
				base.app.arguments.game.podium = dRLMapTrack.podium;
				base.app.arguments.game.fcMode = base.app.model.storage.state.player.activeFCMode;
				base.app.arguments.game.allowCrash = false;
				base.app.view.audio.SceneMainToGame(1.6f);
				base.app.view.ui.fade.FadeIn(1.5f);
				Activity.RunOnce(base.app.scene.Load, 1f);
				break;
			}
			case "leaderboards.replay.load@complete":
				base.app.view.audio.SceneMainToGame(1.6f);
				break;
			case "home.paywall.continue@click":
				base.app.view.ui.screens.CloseAllScreens();
				base.app.view.ui.screens.Open("home-screen-grid");
				break;
			case "home.paywall.dismiss@change":
			{
				DRLToggleView dRLToggleView = p_target as DRLToggleView;
				if ((bool)dRLToggleView)
				{
					Debug.Log(dRLToggleView.toggle.isOn);
					base.app.model.storage.state.player.paywallDismiss = dRLToggleView.toggle.isOn;
				}
				break;
			}
			}
		}
	}
}

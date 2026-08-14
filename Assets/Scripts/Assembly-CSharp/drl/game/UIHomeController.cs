using System;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIHomeController : Controller<DRLApp>
	{
		protected bool m_animateClickDrag;

		public UIHomeView view => AssertLocal<UIHomeView>("view");

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "ui.screen@close":
				m_animateClickDrag = false;
				break;
			case "maps.selection-complete":
				if (base.validContext && base.app.controller.AssertMapSelection(p_target, this))
				{
					base.app.controller.LoadTrackOverview(this, p_target, p_data);
				}
				break;
			case "maps.track-selection-complete":
			{
				UIMapOverviewController uIMapOverviewController = p_target as UIMapOverviewController;
				UIMapSDOverviewController uIMapSDOverviewController = p_target as UIMapSDOverviewController;
				if ((!(uIMapOverviewController == null) || !(uIMapSDOverviewController == null)) && (!(uIMapOverviewController != null) || (!(this == null) && !(uIMapOverviewController.view == null) && !(uIMapOverviewController.view.caller != this))) && (!(uIMapSDOverviewController != null) || (!(this == null) && !(uIMapSDOverviewController.view == null) && !(uIMapSDOverviewController.view.caller != this))))
				{
					_ = base.app.view.ui.screens.manager.history;
					base.app.controller.LoadTrackOverview(this, p_target, p_data);
				}
				break;
			}
			case "maps.community-map-selection-complete":
			{
				UICommunityMapsController uICommunityMapsController = p_target as UICommunityMapsController;
				if (!(this == null) && !(uICommunityMapsController == null) && !(uICommunityMapsController.view == null) && !(uICommunityMapsController.view.caller != this))
				{
					base.app.controller.LoadTrackOverview(this, p_target, p_data);
				}
				break;
			}
			}
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
				PopulateCards();
				bool p_show = GraphicsStateModel.HasLowSpec();
				if ((bool)base.app.view.ui.header)
				{
					base.app.view.ui.header.ShowLowSpecWarning(p_show, 1f);
				}
				RunOnce(0.5f, delegate
				{
					UINavigation focus = UINavigation.focus;
					if (!focus || !focus.transform.IsChildOf(view.transform))
					{
						UINavigation.Focus(view.leftList[0]);
					}
				});
				base.app.model.onboarding.SetOnboardingInactive();
				m_animateClickDrag = true;
				base.app.view.ui.footer.Show(0f);
				view.RefreshPromoBanner();
				break;
			}
			case "home.missions@click":
				base.app.view.ui.screens.Open("train-menu-screen", 0f);
				break;
			case "home.debug.dmv@click":
			case "home.DMV@click":
				if (!base.app.model.storage.state.player.dmvWelcomeScreen)
				{
					base.app.view.ui.screens.Open("dmv-welcome-screen");
				}
				else
				{
					base.app.view.ui.screens.Open("dmv-tests-screen", 0f);
				}
				break;
			case "home.sandbox@click":
			{
				UIMapTrackShortcutView component = (p_target as UICardButtonLarge).GetComponent<UIMapTrackShortcutView>();
				if (!(component == null))
				{
					base.enabled = false;
					base.app.arguments.Clear();
					base.app.arguments.game.type = GameFlag.Sandbox;
					base.app.arguments.game.mode = GameFlag.SinglePlayer;
					base.app.arguments.game.map = component.map;
					base.app.arguments.game.track = component.track;
					base.app.arguments.game.fcMode = base.app.model.storage.state.player.activeFCMode;
					base.app.arguments.game.podium = "";
					base.app.arguments.game.allowCrash = false;
					base.app.arguments.game.promo = false;
					base.app.arguments.game.players.Clear();
					base.app.arguments.game.AddPlayer(base.app.model.storage.state.player.playerData);
					base.app.view.audio.PlayUIStartGame();
					base.app.view.audio.SceneMainToGame(1.6f);
					base.app.view.ui.fade.FadeIn(1.5f);
					RunOnce(base.app.scene.Load, 1f);
					base.app.model.storage.state.license.Poll();
				}
				break;
			}
			case "home.debug.map-editor@click":
			case "home.map-editor@click":
			{
				UICommunityMapsView uICommunityMapsView = base.app.view.ui.screens.Open<UICommunityMapsView>("community-maps-screen");
				uICommunityMapsView.screen.title = base.app.model.storage.locale.Get("maps.community.title", "Community Maps");
				uICommunityMapsView.allowExit = false;
				uICommunityMapsView.caller = this;
				base.app.arguments.Clear();
				base.app.arguments.game.type = GameFlag.MapEditor;
				base.app.arguments.game.mode = GameFlag.SinglePlayer;
				uICommunityMapsView.InitFilter(p_isMultiGP: false);
				break;
			}
			case "home.tryouts@click":
				OnTryoutsClick();
				break;
			case "home.usaf@click":
				base.app.view.ui.screens.Open<UIMapsUSAFView>("maps-usaf-screen").caller = this;
				base.app.arguments.Clear();
				base.app.arguments.game.type = GameFlag.Race;
				base.app.arguments.game.mode = GameFlag.SinglePlayer;
				base.app.arguments.game.AddPlayer(base.app.model.storage.state.player.playerData);
				break;
			case "home.settings@click":
				base.app.view.ui.screens.Open<UISettingsView>("settings-screen");
				break;
			case "home.profile@click":
				base.app.view.ui.screens.Open<UISettingsProfileView>("settings-profile-screen").showMandatoryFields = false;
				break;
			case "home.community.drones@click":
				OnCommunityDroneClick();
				break;
			case "home.community.maps@click":
				OnCommunityMapClick();
				break;
			case "home.garage@click":
				if (!IsCardBlocked())
				{
					UICommunityDronesView uICommunityDronesView = base.app.view.ui.screens.Open<UICommunityDronesView>("community-drones-screen");
					uICommunityDronesView.inGame = false;
					uICommunityDronesView.showMyDrones = true;
					uICommunityDronesView.showCreateButton = true;
					uICommunityDronesView.screen.title = base.app.model.storage.locale.Get("garage.selection-screen.title", "Drones");
				}
				break;
			case "home.store@click":
				if (!IsCardBlocked())
				{
					base.app.view.ui.screens.Open<UIStoreView>("store-screen");
				}
				break;
			case "home.purchase@click":
				base.app.view.ui.screens.Open("purchase-overview-screen", 0f);
				break;
			case "home.debug.dmv@change":
			{
				UICardButtonLarge uICardButtonLarge = null;
				List<UICardButtonLarge> list = new List<UICardButtonLarge>();
				list.AddRange(view.rowBtmList.GetList<UICardButtonLarge>());
				list.AddRange(view.rowTopList.GetList<UICardButtonLarge>());
				uICardButtonLarge = list.Find((UICardButtonLarge it) => it.tag == "TrainingCard");
				if ((bool)uICardButtonLarge)
				{
					uICardButtonLarge.labelField.text = "FLIGHT\nSCHOOL";
					uICardButtonLarge.notification = "home.DMV";
					if ((bool)uICardButtonLarge.subtitle)
					{
						uICardButtonLarge.subtitle.text = "GET CERTIFIED";
					}
				}
				break;
			}
			case "home.vdrl@click":
			{
				bool flag = IsCardBlocked();
				if (flag)
				{
					break;
				}
				Debug.Log($"UIHomeController> VDRLClick - blocked[{flag}]");
				CheckMultiplayerAvailability(delegate
				{
					UITournamentsListView uITournamentsListView = base.app.view.ui.screens.Open<UITournamentsListView>("tournaments-list-screen");
					if ((bool)uITournamentsListView)
					{
						uITournamentsListView.screen.title = base.app.model.storage.locale.Get("vdrl.list.title", "Tournaments").Replace("\n", " ");
						uITournamentsListView.minimumSkill = 0;
					}
				});
				break;
			}
			case "home.allianz@click":
			{
				MapData mapData = base.app.model.storage.maps.FindByGUID("CMP-5d4a0a0c38c1b81258a29ec7");
				if (mapData == null)
				{
					Debug.LogWarning("UIHomeController> AllianzClick: missing map data for GUID: CMP-5d4a0a0c38c1b81258a29ec7");
					break;
				}
				base.app.arguments.Clear();
				base.app.arguments.game.type = GameFlag.Race;
				base.app.arguments.game.mode = GameFlag.SinglePlayer;
				base.app.arguments.game.AddPlayer(base.app.model.storage.state.player.playerData);
				RunOnce(0.5f, delegate
				{
					mapData.Load(mapData.ToJson());
					MapData p_mapData = mapData;
					base.app.controller.LoadCustomTrackOverview(p_mapData);
				});
				break;
			}
			case "home.fly@click":
				if (!base.app.view.ui.screens.Open<UIHomeFlyView>("home-fly-overview-screen"))
				{
					return;
				}
				break;
			case "home.leaders@click":
				if (!IsCardBlocked() && !base.app.view.ui.screens.Open<UIHomeLeadersView>("home-leaders-screen"))
				{
					return;
				}
				break;
			case "home.leaderboards.drl@click":
				if (!IsOffline())
				{
					UILeaderboardsView uILeaderboardsView2 = base.app.view.ui.screens.Open<UILeaderboardsView>("leaderboards-screen");
					uILeaderboardsView2.screen.title = base.app.model.storage.locale.Get("home.card.leaders.drl", "DRL LEADERS");
					uILeaderboardsView2.gameTypeFlag = GameFlag.Race;
					base.app.arguments.lastLeaderboard = DRLAppArguments.LeaderboardType.drl;
				}
				break;
			case "home.leaderboards.open@click":
				if (!IsOffline())
				{
					UILeaderboardsView uILeaderboardsView = base.app.view.ui.screens.Open<UILeaderboardsView>("leaderboards-screen");
					uILeaderboardsView.screen.title = base.app.model.storage.locale.Get("home.card.leaders.open", "OPEN CLASS LEADERS").Replace("\n", " ");
					uILeaderboardsView.gameTypeFlag = GameFlag.Race;
					base.app.arguments.lastLeaderboard = DRLAppArguments.LeaderboardType.open;
				}
				break;
			case "network.update.offline":
			case "network.state.offline":
			case "network.state.online":
				this.TimerRunOnce(delegate
				{
					view.RefreshOnlineContextCards();
					RefreshMultiplayerContext();
				}, 0.5f);
				break;
			}
			if (p_event.IndexOf("@click") >= 0 && p_event.IndexOf("@paywall") >= 0)
			{
				base.app.view.ui.screens.Open("purchase-overview-screen", 0f);
			}
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

		private void OnLeaderBoardClick()
		{
			if (!IsCardBlocked())
			{
				UILeaderboardsView uILeaderboardsView = base.app.view.ui.screens.Open<UILeaderboardsView>("leaderboards-screen");
				uILeaderboardsView.screen.title = base.app.model.storage.locale.Get("home.card.leaders.open", "OPEN CLASS LEADERS").Replace("\n", " ");
				uILeaderboardsView.gameTypeFlag = GameFlag.Race;
				base.app.arguments.lastLeaderboard = DRLAppArguments.LeaderboardType.open;
			}
		}

		private void OnDRLLeaderBoardClick()
		{
			if (!IsCardBlocked())
			{
				UILeaderboardsView uILeaderboardsView = base.app.view.ui.screens.Open<UILeaderboardsView>("leaderboards-screen");
				uILeaderboardsView.screen.title = base.app.model.storage.locale.Get("home.card.leaders.drl", "DRL LEADERS");
				uILeaderboardsView.gameTypeFlag = GameFlag.Race;
				base.app.arguments.lastLeaderboard = DRLAppArguments.LeaderboardType.drl;
			}
		}

		private void OnCommunityMapClick()
		{
			if (!IsCardBlocked())
			{
				UICommunityMapsView uICommunityMapsView = base.app.view.ui.screens.Open<UICommunityMapsView>("community-maps-screen");
				uICommunityMapsView.screen.title = base.app.model.storage.locale.Get("maps.community.title", "Community Maps");
				uICommunityMapsView.caller = this;
				base.app.arguments.Clear();
				base.app.arguments.game.type = GameFlag.Freestyle;
				base.app.arguments.game.mode = GameFlag.SinglePlayer;
				GamePlayerData playerData = base.app.model.storage.state.player.playerData;
				base.app.arguments.game.AddPlayer(playerData);
			}
		}

		private void OnCommunityDroneClick()
		{
			if (!IsCardBlocked())
			{
				UICommunityDronesView uICommunityDronesView = base.app.view.ui.screens.Open<UICommunityDronesView>("community-drones-screen");
				uICommunityDronesView.inGame = false;
				uICommunityDronesView.showMyDrones = false;
				uICommunityDronesView.showCreateButton = false;
				uICommunityDronesView.screen.title = base.app.model.storage.locale.Get("drones.title", "Community Drones");
			}
		}

		private void OnTryoutsClick()
		{
			string p_guid = "CP-58f";
			DRLCampaign data = base.app.model.storage.library.FindByGUID<DRLCampaign>(p_guid);
			base.app.arguments.Clear();
			base.app.arguments.game.type = GameFlag.Race;
			base.app.arguments.game.mode = GameFlag.SinglePlayer;
			base.app.arguments.game.AddPlayer(base.app.model.storage.state.player.playerData);
			base.app.view.ui.screens.Open<UITryoutsOnboardingView>("tryouts-onboarding-screen").data = data;
		}

		private ListComponent GetListForCardListType(UICardButtonLargeListType p_type)
		{
			return p_type switch
			{
				UICardButtonLargeListType.TopRow => view.rowTopList, 
				UICardButtonLargeListType.BottomRow => view.rowBtmList, 
				_ => view.leftList, 
			};
		}

		[ContextMenu("Refresh Cards")]
		private void ForcePopulateCards()
		{
			PopulateCards(p_force: true);
		}

		protected void PopulateCards(bool p_force = false)
		{
			view.SetUserOnlineField(0);
			view.SetUserOnlineFieldAlpha(0f);
			_ = base.app.model.storage.state.player.profile.isDeveloper;
			if (!p_force && view.leftList.Count > 0)
			{
				view.RefreshOnlineContextCards();
				RefreshMultiplayerContext();
				return;
			}
			view.rowTopList.Clear();
			view.rowBtmList.Clear();
			view.leftList.Clear();
			string text = "";
			text = "home-screen-cards";
			AssetLibrary assetLibrary = base.app.model.storage.library.FindByGUID<AssetLibrary>(text);
			GameFlag gameFlag = GameFlag.Release;
			for (int i = 0; i < assetLibrary.assets.Count; i++)
			{
				AssetLibrary component = assetLibrary.assets[i].GetComponent<AssetLibrary>();
				if (component.GetComponent<GameFlagTag>().Match(gameFlag))
				{
					assetLibrary = component;
					break;
				}
			}
			List<UICardButtonLarge> list = assetLibrary.FindAll<UICardButtonLarge>();
			view.multiplayerContextCards = new List<UICardButtonLarge>();
			view.crossplayContextCards = new List<UICardButtonLarge>();
			for (int j = 0; j < list.Count; j++)
			{
				UICardButtonLarge original = list[j];
				original = UnityEngine.Object.Instantiate(original);
				original.name = original.name.Replace("(Clone)", "").Replace("home-", "").Replace("-card", "")
					.Replace("-console", "")
					.Replace("-paywall", "")
					.Trim();
				switch (original.name)
				{
				case "leaders":
					view.multiplayerContextCards.Add(original);
					break;
				case "multiplayer":
				case "vdrl":
				case "leaderboards-open":
				case "leaderboards-drl":
				case "garage":
				case "community-drones":
				case "store":
					if (original.name == "leaderboards-open")
					{
						original.listType = UICardButtonLargeListType.BottomRow;
					}
					view.multiplayerContextCards.Add(original);
					break;
				}
				GetListForCardListType(original.listType).Push(original);
			}
			view.RefreshOnlineContextCards();
			RefreshMultiplayerContext();
			int num = 0;
			for (int k = 0; k < view.rowTopList.Count; k++)
			{
				UICardButtonLarge uICardButtonLarge = view.rowTopList.Get<UICardButtonLarge>(k);
				if (uICardButtonLarge.subType == UICardButtonLargeType.LargeHorizontal)
				{
					num++;
				}
				if (uICardButtonLarge.subType == UICardButtonLargeType.LargeVertical && uICardButtonLarge.listType != UICardButtonLargeListType.LeftMost)
				{
					GameObject gameObject = new GameObject("space");
					gameObject.transform.parent = view.rowBtmList.transform;
					gameObject.AddComponent<RectTransform>().sizeDelta = new Vector2(uICardButtonLarge.GetComponent<RectTransform>().sizeDelta.x, 5f);
					if (num < view.rowBtmList.Count)
					{
						gameObject.transform.SetSiblingIndex(num);
					}
				}
				num++;
			}
			UINavigation.Link(view.leftMostLayout, null, view.rowTopLayout);
			UINavigation.Link(view.rowTopLayout, view.leftMostLayout, null, null, view.rowBtmLayout);
			UINavigation.Link(view.rowBtmLayout, view.leftMostLayout, null, view.rowTopLayout);
			UINavigation left = view.leftList.Get<UINavigation>(view.leftList.Count - 1);
			UINavigation uINavigation = view.rowTopList.Get<UINavigation>(0);
			UINavigation uINavigation2 = view.rowBtmList.Get<UINavigation>(0);
			uINavigation.left = left;
			uINavigation2.left = left;
			int num2 = 0;
			int num3 = 0;
			float num4 = 0f;
			float num5 = 0f;
			bool flag = false;
			while (true)
			{
				UICardButtonLarge uICardButtonLarge2 = ((num2 < view.rowTopList.Count) ? view.rowTopList.Get<UICardButtonLarge>(num2) : null);
				UICardButtonLarge uICardButtonLarge3 = ((num3 < view.rowBtmList.Count) ? view.rowBtmList.Get<UICardButtonLarge>(num3) : null);
				if (uICardButtonLarge2 != null && uICardButtonLarge2.subType == UICardButtonLargeType.LargeVertical)
				{
					UICardButtonLarge uICardButtonLarge4 = ((num2 < view.rowTopList.Count) ? view.rowTopList.Get<UICardButtonLarge>(num2 - 1) : null);
					UICardButtonLarge uICardButtonLarge5 = ((num2 < view.rowTopList.Count) ? view.rowTopList.Get<UICardButtonLarge>(num2 + 1) : null);
					UICardButtonLarge uICardButtonLarge6 = ((num3 < view.rowBtmList.Count) ? view.rowBtmList.Get<UICardButtonLarge>(num3 - 1) : null);
					num2++;
					uICardButtonLarge2.GetComponent<UINavigation>().down = null;
					if (uICardButtonLarge4 != null)
					{
						if (uICardButtonLarge6 != null && uICardButtonLarge4.subType != UICardButtonLargeType.LargeVertical)
						{
							uICardButtonLarge6.GetComponent<UINavigation>().right = uICardButtonLarge2;
						}
					}
					else if (uICardButtonLarge6 != null)
					{
						uICardButtonLarge6.GetComponent<UINavigation>().right = uICardButtonLarge2;
					}
					if (uICardButtonLarge5 != null)
					{
						if (uICardButtonLarge3 != null && uICardButtonLarge5.subType != UICardButtonLargeType.LargeVertical)
						{
							uICardButtonLarge3.GetComponent<UINavigation>().left = uICardButtonLarge2;
						}
					}
					else if (uICardButtonLarge3 != null)
					{
						uICardButtonLarge3.GetComponent<UINavigation>().left = uICardButtonLarge2;
					}
					continue;
				}
				if (num2 >= view.rowTopList.Count && num3 >= view.rowBtmList.Count)
				{
					break;
				}
				if (!uICardButtonLarge2)
				{
					uICardButtonLarge2 = view.rowTopList.Get<UICardButtonLarge>(view.rowTopList.Count - 1);
				}
				if (!uICardButtonLarge3)
				{
					uICardButtonLarge3 = view.rowBtmList.Get<UICardButtonLarge>(view.rowBtmList.Count - 1);
				}
				UINavigation component2 = uICardButtonLarge2.GetComponent<UINavigation>();
				UINavigation component3 = uICardButtonLarge3.GetComponent<UINavigation>();
				if ((uICardButtonLarge2.subType == UICardButtonLargeType.LargeHorizontal || uICardButtonLarge2.subType == UICardButtonLargeType.Medium) && num2 < view.rowTopList.Count && uICardButtonLarge2.subType != uICardButtonLarge3.subType && (uICardButtonLarge3.subType == UICardButtonLargeType.Small || uICardButtonLarge3.subType == UICardButtonLargeType.Medium))
				{
					RectTransform rectTransform = (RectTransform)uICardButtonLarge2.transform;
					RectTransform rectTransform2 = (RectTransform)uICardButtonLarge3.transform;
					num4 += rectTransform2.sizeDelta.x;
					if (num4 < rectTransform.sizeDelta.x)
					{
						if (!flag)
						{
							component2.down = component3;
						}
						flag = true;
						component3.up = component2;
						num3++;
					}
					else
					{
						flag = false;
						num4 = 0f;
						num2++;
					}
				}
				else if ((uICardButtonLarge3.subType == UICardButtonLargeType.LargeHorizontal || uICardButtonLarge3.subType == UICardButtonLargeType.Medium) && num3 < view.rowBtmList.Count && uICardButtonLarge2.subType != uICardButtonLarge3.subType && (uICardButtonLarge2.subType == UICardButtonLargeType.Small || uICardButtonLarge2.subType == UICardButtonLargeType.Medium))
				{
					RectTransform rectTransform3 = (RectTransform)uICardButtonLarge2.transform;
					RectTransform rectTransform4 = (RectTransform)uICardButtonLarge3.transform;
					num5 += rectTransform3.sizeDelta.x;
					if (num5 < rectTransform4.sizeDelta.x)
					{
						component2.down = component3;
						if (!flag)
						{
							component3.up = component2;
						}
						flag = true;
						num2++;
					}
					else
					{
						flag = false;
						num5 = 0f;
						num3++;
					}
				}
				else
				{
					flag = false;
					num5 = 0f;
					num4 = 0f;
					if (num2 < view.rowTopList.Count || component2.down != view.rowBtmList.Get<UINavigation>(view.rowBtmList.Count - 2))
					{
						component2.down = component3;
					}
					if (num3 < view.rowTopList.Count || component3.up != view.rowTopList.Get<UINavigation>(view.rowTopList.Count - 2))
					{
						component3.up = component2;
					}
					num2++;
					num3++;
				}
			}
		}

		protected void RefreshMultiplayerContext(Action p_oncomplete = null)
		{
			_ = base.app.online;
			string p_caption = base.app.model.storage.locale.Get("ui.offline.status", "UNAVAILABLE (OFFLINE)");
			view.SetMultiplayerContextEnabled(!DRLApp.offline, p_interactable: true, p_caption);
		}

		public void CheckMultiplayerAvailability(Action p_oncomplete, bool delayForCheck = true)
		{
			_ = base.app.model.service.platform;
			bool online = base.app.online;
			Debug.Log($"UIHomeController> CheckMultiplayerAvailability - online[{online}]");
			if (!online)
			{
				view.SetMultiplayerContextEnabled(p_flag: false, p_interactable: false, "NETWORK OFFLINE");
				base.app.view.audio.PlayUIGenericError();
			}
			else
			{
				p_oncomplete?.Invoke();
			}
		}

		private bool IsCardBlocked()
		{
			bool offline = DRLApp.offline;
			if (offline)
			{
				base.app.view.ui.dialog.Open(DialogTemplateType.OfflineMode, "no-connection");
			}
			return offline;
		}

		protected void OnDestroy()
		{
		}
	}
}

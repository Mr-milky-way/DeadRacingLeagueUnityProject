using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using drl.backend;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UILeaderboardsController : Controller<DRLApp>
	{
		public int pageLength = 10;

		public DRLLeaderboardData campaignSelected;

		protected WebAsyncRequest m_loader;

		protected WebAsyncRequest m_user_search;

		protected WebAsyncRequest m_replay_loader;

		protected Activity m_load_timer;

		protected bool m_ignore_page_click;

		protected bool m_ignore_replay_click;

		private bool isCollectable;

		private bool m_thread_kill;

		public UILeaderboardsView view => AssertLocal<UILeaderboardsView>("view");

		private void Awake()
		{
			PopulateControllerTypeStepper();
		}

		private void PopulateControllerTypeStepper()
		{
			string[] labels = new string[4] { "ALL", "RADIO", "PS", "XBOX" };
			view.controllerTypeStepper.labels = labels;
			view.controllerTypeStepper.max = 3;
			view.controllerTypeStepper.icons = view.controllerTypeIcons_Standalone;
		}

		private void PopulatePlatformTypeStepper()
		{
			string[] labels = new string[4] { "ALL", "PC", "XBox", "PS" };
			view.platformStepper.labels = labels;
			view.platformStepper.max = 3;
			view.platformStepper.icons = view.platformTypeIcons_Standalone;
		}

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (p_event != null && p_event == "maps.selection-complete")
			{
				if (!base.app.controller.AssertMapSelection(p_target, this, p_need_return: true))
				{
					Debug.Log("UILeaderboardsController> no assert map selection");
				}
				else
				{
					_ = (string)p_data[0];
					_ = (string)p_data[1];
					_ = (string)p_data[2];
					bool num = (bool)p_data[3];
					view.showAllButton.gameObject.SetActive(value: false);
					if (num)
					{
						if (p_data[6] is MapData)
						{
							MapData customMap = (MapData)p_data[6];
							view.SetCustomMap(customMap);
							goto IL_00fc;
						}
						Debug.LogError("UILeaderboardsController> MapSelectionComplete received invalid DRLCommunityMapData");
					}
					else
					{
						if (p_data[4] is DRLMap && p_data[5] is DRLMapTrack)
						{
							DRLMap dRLMap = (DRLMap)p_data[4];
							DRLMapTrack dRLMapTrack = (DRLMapTrack)p_data[5];
							view.map = dRLMap;
							view.track = dRLMapTrack;
							view.SetMap(dRLMap, dRLMapTrack);
							goto IL_00fc;
						}
						Debug.LogError("UILeaderboardsController> MapSelectionComplete received invalid DRLMap or DRLMapTrack");
					}
				}
			}
			goto IL_012a;
			IL_012a:
			if (base.app.view.ui.screens.current != view.screen || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				if (!(p_data[0] as UIScreen != view.screen))
				{
					view.drlInputFieldView.ClearInputText();
					if (base.app.inVirtualSeason)
					{
						view.replayURLInputField.transform.parent.gameObject.SetActive(value: true);
					}
					view.regionId = "";
					view.raceListFade.alpha = -0.1f;
					view.campaignRacesListFade.alpha = -0.1f;
					view.SetFeedback(UILeaderboardFeedbackType.Loading);
					ListComponent raceListField = view.raceListField;
					raceListField.Clear();
					for (int num6 = 0; num6 < pageLength; num6++)
					{
						raceListField.Push<UILeaderboardItemView>().fade.alpha = -0.1f;
					}
					raceListField = view.campaignRacesListField;
					raceListField.Clear();
					for (int num7 = 0; num7 < pageLength; num7++)
					{
						raceListField.Push<UILeaderboardItemView>().fade.alpha = -0.1f;
					}
					view.circuitsStepper.max = (base.app.inVirtualSeason ? 1 : 2);
					if (view.circuit != null)
					{
						view.SetCircuit();
						view.circuitsStepper.index = 1;
						view.circuitsStepper.Refresh();
					}
					else if (view.map == null && view.customMap == null && view.gameTypeFlag != GameFlag.Campaign)
					{
						List<DRLMap> raceMaps = base.app.model.storage.GetRaceMaps();
						List<DRLMapTrack> mapTracks = base.app.model.storage.GetMapTracks(raceMaps[0], view.gameTypeFlag);
						view.map = raceMaps[0];
						view.track = mapTracks[0];
						view.SetMap();
					}
					view.showAllButton.gameObject.SetActive(value: false);
					view.SetTopVisible(p_flag: false);
					m_thread_kill = false;
					LoadArgs();
					CheckUserRace();
				}
				break;
			case "leaderboards.filter.form.event@submit":
			{
				string text = p_target.name;
				if (text != null && text == "replay-url")
				{
					DRLInputFieldView dRLInputFieldView = p_target as DRLInputFieldView;
					if ((bool)dRLInputFieldView)
					{
						Debug.Log("UILeaderboardsController> Loading Replay [" + dRLInputFieldView.field.text + "]");
						LoadReplay(dRLInputFieldView.field.text);
					}
				}
				break;
			}
			case "leaderboards.filter.form.event@change":
				OnFormNotification(p_target, p_is_change: true);
				break;
			case "leaderboards.filter.form.event@click":
				OnFormNotification(p_target, p_is_change: false);
				break;
			case "leaderboards.search@submit":
				SearchUserRace(view.drlInputFieldView.inputText.text);
				break;
			case "leaderboards.search@click":
				SearchUserRace(view.drlInputFieldView.inputText.text);
				break;
			case "leaderboards.search.reset@click":
				ResetLeaderboard();
				break;
			case "leaderboards.choose-map@click":
				if (view.circuitsStepper.index == 0)
				{
					UIMapsCategoryView uIMapsCategoryView = base.app.view.ui.screens.Open<UIMapsCategoryView>("maps-category-screen");
					uIMapsCategoryView.screen.title = base.app.model.storage.locale.Get("maps.choose-map", "Choose");
					uIMapsCategoryView.caller = this;
					base.app.arguments.game.type = GameFlag.Race;
				}
				else if (view.circuitsStepper.index == 2)
				{
					UIMapsSDCategoryView uIMapsSDCategoryView = base.app.view.ui.screens.Open<UIMapsSDCategoryView>("collectables-category-screen");
					uIMapsSDCategoryView.screen.title = base.app.model.storage.locale.Get("maps.sd-courses.title", "Search & Destroy Tracks");
					uIMapsSDCategoryView.caller = this;
					base.app.arguments.game.type = GameFlag.Collectable;
				}
				else
				{
					UICircuitSelectionView uICircuitSelectionView = base.app.view.ui.screens.Open<UICircuitSelectionView>("circuits-selection-screen");
					uICircuitSelectionView.screen.title = "Choose";
					uICircuitSelectionView.caller = this;
				}
				break;
			case "leaderboards.item@click":
			{
				UIElementView uIElementView = p_target as UIElementView;
				if (!uIElementView)
				{
					break;
				}
				UILeaderboardItemView componentInParent = uIElementView.GetComponentInParent<UILeaderboardItemView>();
				if (!view.isCampaignRaceMode && view.gameTypeFlag == GameFlag.Campaign)
				{
					Debug.Log("UILeaderboardsController> Clicked User Item [" + componentInParent?.ToString() + "]");
					DRLLeaderboardData p_data2 = (campaignSelected = componentInParent.data);
					view.SetCampaignRaceMode(p_flag: true);
					RefreshCampaignRacePage(p_data2, 0);
					RunOnce(0.05f, delegate
					{
						UINavigation.Focus(view.campaignItemField);
					});
				}
				break;
			}
			case "leaderboards.item.replay@click":
			{
				if (m_ignore_replay_click)
				{
					break;
				}
				m_ignore_replay_click = true;
				UIElementView uIElementView2 = p_target as UIElementView;
				if (!uIElementView2)
				{
					break;
				}
				UILeaderboardItemView componentInParent2 = uIElementView2.GetComponentInParent<UILeaderboardItemView>();
				Debug.Log("UILeaderboardsController> Clicked User Replay [" + componentInParent2?.ToString() + "] replay[" + (componentInParent2 ? componentInParent2.replayURL : "") + "]");
				if ((bool)componentInParent2)
				{
					if (!componentInParent2.hasReplay)
					{
						base.app.view.audio.PlayUIGenericError();
						m_ignore_replay_click = false;
					}
					else
					{
						m_thread_kill = false;
						string replayURL = componentInParent2.replayURL;
						LoadReplay(replayURL);
					}
				}
				break;
			}
			case "leaderboards.item.savedrone@click":
			{
				UIElementView uIElementView3 = p_target as UIElementView;
				if (!uIElementView3)
				{
					break;
				}
				UILeaderboardItemView componentInParent3 = uIElementView3.GetComponentInParent<UILeaderboardItemView>();
				if ((bool)componentInParent3)
				{
					bool flag = true;
					if (string.IsNullOrEmpty(componentInParent3.droneRigData))
					{
						flag = false;
					}
					if (!flag)
					{
						base.app.view.audio.PlayUIGenericError();
					}
					else
					{
						LoadGarage(componentInParent3.droneRigData);
					}
				}
				break;
			}
			case "leaderboards.page@select":
				if (!m_ignore_page_click)
				{
					int num2 = (int)p_data[0];
					bool isCampaignRaceMode = view.isCampaignRaceMode;
					Debug.Log("UILeaderboardController> Page Select [" + num2 + "] campaign[" + isCampaignRaceMode + "]");
					if (isCampaignRaceMode)
					{
						RefreshCampaignRacePage(campaignSelected, num2);
					}
					else
					{
						RefreshListDelayed(num2, p_is_page_change: true);
					}
				}
				break;
			case "leaderboards.page-next@click":
			{
				if (m_ignore_page_click)
				{
					break;
				}
				int num3 = (view.isCampaignRaceMode ? view.campaignRacePageField.index : view.racePageField.index);
				int num4 = (view.isCampaignRaceMode ? view.campaignRacePageField.listField.Count : view.racePageField.listField.Count);
				if (num3 + 1 != num4)
				{
					if (view.isCampaignRaceMode)
					{
						view.campaignRacePageField.index = num3 + 1;
						RefreshCampaignRacePage(campaignSelected, num3);
					}
					else
					{
						view.racePageField.index = num3 + 1;
						RefreshListDelayed(view.racePageField.index, p_is_page_change: true);
					}
				}
				break;
			}
			case "leaderboards.page-previous@click":
			{
				if (m_ignore_page_click)
				{
					break;
				}
				int num5 = (view.isCampaignRaceMode ? view.campaignRacePageField.index : view.racePageField.index);
				if (!view.isCampaignRaceMode)
				{
					_ = view.racePageField.listField.Count;
				}
				else
				{
					_ = view.campaignRacePageField.listField.Count;
				}
				if (num5 != 0)
				{
					if (view.isCampaignRaceMode)
					{
						view.campaignRacePageField.index = num5 - 1;
						RefreshCampaignRacePage(campaignSelected, num5);
					}
					else
					{
						view.racePageField.index = num5 - 1;
						RefreshListDelayed(view.racePageField.index, p_is_page_change: true);
					}
				}
				break;
			}
			case "ui.screen.return@click":
				CancelReplayLoad();
				CancelReplayParse();
				if (base.app.view.ui.screens.manager.IsInHistory("tournament-leaders-screen"))
				{
					base.app.view.ui.screens.Return();
				}
				else if (view.isCampaignRaceMode)
				{
					view.SetCampaignRaceMode(p_flag: false);
					SetMenuListNavigation(p_campaign_race: false);
					RunOnce(0.05f, delegate
					{
						UINavigation.Focus(view.gameTypeStepper);
					});
					if (view.racePageField.total <= 0)
					{
						RefreshListDelayed();
					}
				}
				else
				{
					SaveArgs();
					base.app.view.ui.screens.Return();
				}
				break;
			case "ui.screen.nav-left@click":
			{
				string text = (p_target as UIElementView).name;
				if (text != null && text == "exit" && (bool)base.app.controller.game)
				{
					base.app.controller.game.Exit();
				}
				break;
			}
			}
			return;
			IL_00fc:
			RefreshListDelayed();
			m_ignore_page_click = true;
			view.racePageField.index = 0;
			m_ignore_page_click = false;
			SaveArgs();
			goto IL_012a;
		}

		protected void OnFormNotification(UnityEngine.Object p_target, bool p_is_change)
		{
			bool flag = p_is_change;
			string text = p_target.name;
			if (text.Contains("region-"))
			{
				text = "region-item";
			}
			if (text == null)
			{
				return;
			}
			switch (text)
			{
			case "quest":
				break;
			case "mission":
				break;
			case "user":
				base.app.view.audio.PlayUIGenericSuccess();
				switch (view.gameTypeFlag)
				{
				case GameFlag.Race:
					FindRaceUser();
					break;
				case GameFlag.Campaign:
					FindCampaignUser();
					break;
				}
				break;
			case "game-type":
				if (flag)
				{
					DRLStepperView campaignStepper = view.gameTypeStepper;
					if (!campaignStepper || campaignStepper.max != 0)
					{
						base.app.view.audio.PlayUIGenericSuccess();
						view.SetGameType(campaignStepper.label);
						RefreshListDelayed();
						m_ignore_page_click = true;
						view.racePageField.index = 0;
						m_ignore_page_click = false;
					}
				}
				break;
			case "game-type-circuits":
				if (flag)
				{
					view.map = null;
					view.track = null;
					if (view.circuitsStepper.index == 0)
					{
						base.app.arguments.game.type = GameFlag.Race;
						isCollectable = false;
						view.ResetMap();
					}
					if (view.circuitsStepper.index == 1)
					{
						view.SetCircuit();
					}
					if (view.circuitsStepper.index == 2)
					{
						base.app.arguments.game.type = GameFlag.Collectable;
						view.ResetMap();
					}
					RefreshListDelayed();
					m_ignore_page_click = true;
					view.racePageField.index = 0;
					m_ignore_page_click = false;
				}
				break;
			case "platform":
				if (flag)
				{
					RefreshListDelayed();
					m_ignore_page_click = true;
					view.racePageField.index = 0;
					m_ignore_page_click = false;
				}
				break;
			case "controller-type":
				if (flag)
				{
					RefreshListDelayed();
					m_ignore_page_click = true;
					view.racePageField.index = 0;
					m_ignore_page_click = false;
				}
				break;
			case "drone-class":
				if (flag)
				{
					RefreshListDelayed();
					m_ignore_page_click = true;
					view.racePageField.index = 0;
					m_ignore_page_click = false;
				}
				break;
			case "physics":
			case "physics-stepper":
				if (flag)
				{
					RefreshListDelayed();
					m_ignore_page_click = true;
					view.racePageField.index = 0;
					m_ignore_page_click = false;
				}
				break;
			case "campaign":
				if (flag)
				{
					DRLStepperView campaignStepper = view.campaignStepper;
					view.PopulateCampaigns(campaignStepper.index);
					RefreshListDelayed();
					m_ignore_page_click = true;
					view.racePageField.index = 0;
					m_ignore_page_click = false;
				}
				break;
			case "region":
				UINavigation.focus = (p_target as UIElementView).transform.Find("content").GetComponentInChildren<UINavigation>();
				break;
			case "region-item":
			{
				string region = p_target.name.Replace("region-", "");
				view.SetRegion(region);
				RefreshListDelayed();
				m_ignore_page_click = true;
				view.racePageField.index = 0;
				m_ignore_page_click = false;
				break;
			}
			}
		}

		protected void FindRaceUser()
		{
			view.SetFeedback(UILeaderboardFeedbackType.Loading, p_hide_list: false);
			if (m_user_search != null)
			{
				m_user_search.Cancel();
			}
			isCollectable = false;
			if (view.circuitsStepper.index == 0)
			{
				if (!view.isCustomMap && !view.map)
				{
					SetErrorFeedback("Invalid Map!");
					return;
				}
				if (!view.isCustomMap && !view.track)
				{
					SetErrorFeedback("Invalid Track!");
					return;
				}
				if (view.isCustomMap && string.IsNullOrEmpty(view.customMap.guid))
				{
					SetErrorFeedback("Invalid Custom Map!");
					return;
				}
			}
			if (view.circuitsStepper.index == 1 && view.circuit == null)
			{
				SetErrorFeedback("Please select a valid circuit!");
				return;
			}
			if (view.circuitsStepper.index == 2)
			{
				if (!view.isCustomMap && !view.map)
				{
					SetErrorFeedback("Invalid Map!");
					return;
				}
				if (!view.isCustomMap && !view.track)
				{
					SetErrorFeedback("Invalid Track!");
					return;
				}
				if (view.isCustomMap && string.IsNullOrEmpty(view.customMap.guid))
				{
					SetErrorFeedback("Invalid Custom Map!");
					return;
				}
				isCollectable = true;
			}
			int p_drone_class = (view.drlOnly ? 7 : view.droneClass);
			int p_physics = ((!view.drlOnly) ? view.physics : 0);
			string controllerTypeString = view.GetControllerTypeString();
			string platformString = view.GetPlatformString();
			string p_circuitId = view.circuit?.guid;
			if (view.isCustomMap)
			{
				m_user_search = base.app.model.service.GetLeaderboardUser(view.customMap, pageLength, p_drone_class, view.drlOnly, p_physics, delegate(DRLLeaderboardResult p_result)
				{
					if (!(this == null))
					{
						if (p_result == null)
						{
							SetErrorFeedback("Failed to find player.");
						}
						else if (m_user_search.status != AsyncRequestStatus.Cancelled && m_user_search.status != AsyncRequestStatus.Created)
						{
							int page = p_result.pagging.page;
							if (page <= 0)
							{
								SetErrorFeedback("User not found!");
							}
							else
							{
								Debug.Log("UILeaderboardController> FindRaceUser - page[" + page + "]");
								view.racePageField.index = page - 1;
								RefreshListDelayed(view.racePageField.index, p_is_page_change: true);
							}
						}
					}
				}, platformString, controllerTypeString, p_circuitId, null, -1, isCollectable);
				return;
			}
			m_user_search = base.app.model.service.GetLeaderboardUser(view.map, view.track, pageLength, p_drone_class, view.drlOnly, p_physics, delegate(DRLLeaderboardResult p_result)
			{
				if (!(this == null))
				{
					if (p_result == null)
					{
						SetErrorFeedback("Failed to find player.");
					}
					else if (m_user_search.status != AsyncRequestStatus.Cancelled && m_user_search.status != AsyncRequestStatus.Created)
					{
						int page = p_result.pagging.page;
						if (page <= 0)
						{
							SetErrorFeedback("User not found!");
						}
						else
						{
							Debug.Log("UILeaderboardController> FindRaceUser - page[" + page + "]");
							view.racePageField.index = page - 1;
							RefreshListDelayed(view.racePageField.index, p_is_page_change: true);
						}
					}
				}
			}, platformString, controllerTypeString, p_circuitId, null, -1, isCollectable);
		}

		public void CheckUserRace()
		{
			isCollectable = false;
			if (view.circuitsStepper.index == 0 || view.circuitsStepper.index == 2)
			{
				if ((!view.isCustomMap && !view.map) || (!view.isCustomMap && !view.track) || (view.isCustomMap && string.IsNullOrEmpty(view.customMap.guid)))
				{
					return;
				}
			}
			else if (view.circuitsStepper.index == 1 && view.circuit == null)
			{
				return;
			}
			if (view.circuitsStepper.index == 2)
			{
				isCollectable = true;
			}
			int p_drone_class = (view.drlOnly ? 7 : view.droneClass);
			int p_physics = ((!view.drlOnly) ? view.physics : 0);
			string controllerTypeString = view.GetControllerTypeString();
			string platformString = view.GetPlatformString();
			string p_circuitId = view.circuit?.guid;
			if (view.isCustomMap)
			{
				base.app.model.service.GetLeaderboardUser(view.customMap, pageLength, p_drone_class, view.drlOnly, p_physics, delegate(DRLLeaderboardResult p_result)
				{
					if (!(this == null))
					{
						if (p_result == null)
						{
							view.SetMyPositionEnabled(p_flag: false);
						}
						else if (p_result.leaderboard == null || p_result.leaderboard.Length == 0)
						{
							view.SetMyPositionEnabled(p_flag: false);
						}
						else if (p_result.pagging.page <= 0)
						{
							view.SetMyPositionEnabled(p_flag: false);
						}
						else
						{
							view.SetMyPositionEnabled(p_flag: true);
						}
					}
				}, platformString, controllerTypeString, p_circuitId, null, -1, isCollectable);
				return;
			}
			base.app.model.service.GetLeaderboardUser(view.map, view.track, pageLength, p_drone_class, view.drlOnly, p_physics, delegate(DRLLeaderboardResult p_result)
			{
				if (!(this == null))
				{
					if (p_result == null)
					{
						view.SetMyPositionEnabled(p_flag: false);
					}
					else if (p_result.leaderboard == null || p_result.leaderboard.Length == 0)
					{
						view.SetMyPositionEnabled(p_flag: false);
					}
					else if (p_result.pagging.page <= 0)
					{
						view.SetMyPositionEnabled(p_flag: false);
					}
					else
					{
						view.SetMyPositionEnabled(p_flag: true);
					}
				}
			}, platformString, controllerTypeString, p_circuitId, null, -1, isCollectable);
		}

		public void SearchUserRace(string playerName)
		{
			if (playerName == "")
			{
				ResetLeaderboard();
				return;
			}
			view.showAllButton.gameObject.SetActive(value: true);
			view.SetFeedback(UILeaderboardFeedbackType.Loading, p_hide_list: false);
			if (m_user_search != null)
			{
				m_user_search.Cancel();
			}
			if (view.circuitsStepper.index == 0)
			{
				if (!view.isCustomMap && !view.map)
				{
					SetErrorFeedback("Invalid Map!");
					return;
				}
				if (!view.isCustomMap && !view.track)
				{
					SetErrorFeedback("Invalid Track!");
					return;
				}
				if (view.isCustomMap && string.IsNullOrEmpty(view.customMap.guid))
				{
					SetErrorFeedback("Invalid Custom Map!");
					return;
				}
			}
			if (view.circuitsStepper.index == 1 && view.circuit == null)
			{
				SetErrorFeedback("Please select a valid circuit!");
				return;
			}
			if (view.circuitsStepper.index == 2)
			{
				isCollectable = true;
			}
			int p_drone_class = (view.drlOnly ? 7 : view.droneClass);
			int p_physics = ((!view.drlOnly) ? view.physics : 0);
			string controllerTypeString = view.GetControllerTypeString();
			string platformString = view.GetPlatformString();
			string p_circuitId = view.circuit?.guid;
			WebAsyncRequest message = ((!view.isCustomMap) ? base.app.model.service.GetLeaderboardSpecificUser(playerName, view.map, view.track, view.isCustomMap, pageLength, p_drone_class, view.drlOnly, p_physics, delegate(DRLLeaderboardResult p_result)
			{
				if (!(this == null))
				{
					if (p_result == null)
					{
						view.SetMyPositionEnabled(p_flag: false);
					}
					else
					{
						_ = p_result.pagging.page;
						view.Clear();
						ListComponent raceListField = view.raceListField;
						DRLPagePickerView racePageField = view.racePageField;
						int p_page = p_result.pagging.page - 1;
						int pageTotal = p_result.pagging.pageTotal;
						DRLLeaderboardData[] p_races = (p_result.success ? p_result.leaderboard : null);
						bool p_allow_replay = view.gameTypeFlag == GameFlag.Campaign && false;
						PopulateResults(p_races, raceListField, racePageField, p_campaign_race: false, p_allow_replay, p_allow_save: true, p_page, pageTotal);
					}
				}
			}, platformString, controllerTypeString, p_circuitId, null, isCollectable) : base.app.model.service.GetLeaderboardSpecificUser(playerName, view.map, view.track, view.isCustomMap, pageLength, p_drone_class, view.drlOnly, p_physics, delegate(DRLLeaderboardResult p_result)
			{
				if (!(this == null))
				{
					if (p_result == null)
					{
						view.SetMyPositionEnabled(p_flag: false);
					}
					else if (p_result.pagging.page <= 0)
					{
						view.SetMyPositionEnabled(p_flag: false);
					}
					else
					{
						view.SetMyPositionEnabled(p_flag: true);
						view.Clear();
						ListComponent raceListField = view.raceListField;
						DRLPagePickerView racePageField = view.racePageField;
						int p_page = p_result.pagging.page - 1;
						int pageTotal = p_result.pagging.pageTotal;
						DRLLeaderboardData[] p_races = (p_result.success ? p_result.leaderboard : null);
						bool p_allow_replay = view.gameTypeFlag == GameFlag.Campaign && false;
						PopulateResults(p_races, raceListField, racePageField, p_campaign_race: false, p_allow_replay, p_allow_save: true, p_page, pageTotal);
					}
				}
			}, platformString, controllerTypeString, p_circuitId, null, isCollectable));
			Debug.Log(message);
		}

		private void ResetLeaderboard()
		{
			view.regionId = "";
			view.raceListFade.alpha = -0.1f;
			view.campaignRacesListFade.alpha = -0.1f;
			view.SetFeedback(UILeaderboardFeedbackType.Loading);
			ListComponent raceListField = view.raceListField;
			raceListField.Clear();
			for (int i = 0; i < pageLength; i++)
			{
				raceListField.Push<UILeaderboardItemView>().fade.alpha = -0.1f;
			}
			raceListField = view.campaignRacesListField;
			raceListField.Clear();
			for (int j = 0; j < pageLength; j++)
			{
				raceListField.Push<UILeaderboardItemView>().fade.alpha = -0.1f;
			}
			if (view.circuit != null)
			{
				view.SetCircuit();
				view.circuitsStepper.index = 1;
				view.circuitsStepper.Refresh();
			}
			else if (view.map == null && view.customMap == null && view.gameTypeFlag != GameFlag.Campaign)
			{
				List<DRLMap> raceMaps = base.app.model.storage.GetRaceMaps();
				List<DRLMapTrack> mapTracks = base.app.model.storage.GetMapTracks(raceMaps[0], view.gameTypeFlag);
				view.map = raceMaps[0];
				view.track = mapTracks[0];
				view.SetMap();
			}
			view.SetTopVisible(p_flag: false);
			m_thread_kill = false;
			view.showAllButton.gameObject.SetActive(value: false);
			LoadArgs();
			CheckUserRace();
		}

		protected void FindCampaignUser()
		{
			view.SetFeedback(UILeaderboardFeedbackType.Loading, p_hide_list: false);
			if (m_user_search != null)
			{
				m_user_search.Cancel();
			}
			if (!view.campaign)
			{
				SetErrorFeedback("Invalid Campaign!");
				return;
			}
			m_user_search = base.app.model.service.GetLeaderboardUser(view.campaign, pageLength, 6, delegate(DRLLeaderboardResult p_result)
			{
				if (p_result == null)
				{
					SetErrorFeedback("Failed to find player.");
				}
				else
				{
					int page = p_result.pagging.page;
					if (page <= 0)
					{
						SetErrorFeedback("User not found!");
					}
					else
					{
						Debug.Log("UILeaderboardController> FindCampaignUser - page[" + page + "]");
						view.campaignRacePageField.index = page - 1;
						RefreshListDelayed(view.campaignRacePageField.index, p_is_page_change: true);
					}
				}
			});
		}

		protected void SetErrorFeedback(string p_log)
		{
			Debug.LogWarning("UILeaderboardController> " + p_log);
			base.app.view.audio.PlayUIGenericError();
			view.SetFeedback(UILeaderboardFeedbackType.NoResult, p_hide_list: false);
			Activity.RunOnce(delegate
			{
				view.SetFeedback(UILeaderboardFeedbackType.None);
			}, 1f);
		}

		protected void LoadArgs()
		{
			DRLAppArguments.Leaderboards leaderboards = ((base.app.arguments.lastLeaderboard == DRLAppArguments.LeaderboardType.drl) ? base.app.arguments.leaderboardsDRL : ((base.app.arguments.lastLeaderboard == DRLAppArguments.LeaderboardType.open) ? base.app.arguments.leaderboardsOpen : base.app.arguments.leaderboardsCampaign));
			if (leaderboards == null)
			{
				leaderboards = new DRLAppArguments.Leaderboards();
			}
			if (leaderboards.gameType == GameFlag.None)
			{
				leaderboards.gameType = ((base.app.arguments.lastLeaderboard == DRLAppArguments.LeaderboardType.campaign) ? GameFlag.Campaign : GameFlag.Race);
			}
			view.drlOnly = base.app.arguments.lastLeaderboard == DRLAppArguments.LeaderboardType.drl;
			string text = "";
			switch (leaderboards.gameType)
			{
			case GameFlag.Campaign:
				text = "campaign";
				break;
			case GameFlag.Race:
				text = "race";
				break;
			case GameFlag.Mission:
				text = "mission";
				break;
			case GameFlag.Collectable:
				text = "collectable";
				break;
			}
			int num = Array.IndexOf(view.gameTypeStepper.labels, text.ToUpper());
			if (num >= 0)
			{
				view.gameTypeStepper.index = num;
				view.gameTypeStepper.Refresh();
			}
			view.map = leaderboards.map;
			view.track = leaderboards.track;
			view.customMap = leaderboards.customMap;
			view.isCustomMap = leaderboards.isCustomMap;
			view.campaign = leaderboards.campaign;
			view.campaignStepper.index = leaderboards.campaignIndex;
			view.campaignStepper.Refresh();
			view.mission = leaderboards.mission;
			view.missionStepper.index = leaderboards.missionIndex;
			view.missionStepper.Refresh();
			view.controllerTypeStepper.index = leaderboards.controllerTypeIndex;
			view.controllerTypeStepper.Refresh();
			view.platformStepper.index = leaderboards.platformIndex;
			view.platformStepper.Refresh();
			view.droneClassStepper.index = leaderboards.sizeIndex;
			view.droneClassStepper.Refresh();
			view.physicsStepper.index = leaderboards.physicsIndex;
			view.physicsStepper.Refresh();
			view.SetCampaignRaceMode(leaderboards.isCampaignRaceMode);
			view.SetGameType(leaderboards.gameType);
			view.SetMap(view.map, view.track);
			view.SetCustomMap(view.customMap);
			campaignSelected = leaderboards.campaignSelectd;
			if (campaignSelected == null)
			{
				leaderboards.isCampaignRaceMode = false;
			}
			if (leaderboards.isCampaignRaceMode)
			{
				view.SetCampaign(view.campaign);
				RefreshCampaignRacePage(campaignSelected, leaderboards.campaignRaceModePage);
			}
			else if (view.circuit == null)
			{
				if (view.map == null && view.customMap == null && view.gameTypeFlag != GameFlag.Campaign)
				{
					List<DRLMap> raceMaps = base.app.model.storage.GetRaceMaps();
					List<DRLMapTrack> mapTracks = base.app.model.storage.GetMapTracks(raceMaps[0], view.gameTypeFlag);
					view.map = raceMaps[0];
					view.track = mapTracks[0];
					view.SetMap();
				}
				RefreshList(leaderboards.racePage);
			}
			else
			{
				view.SetCircuit();
				RefreshList(0);
			}
		}

		private DRLAppArguments.Leaderboards SaveArgs()
		{
			DRLAppArguments.Leaderboards leaderboards = ((base.app.arguments.lastLeaderboard == DRLAppArguments.LeaderboardType.drl) ? base.app.arguments.leaderboardsDRL : ((base.app.arguments.lastLeaderboard == DRLAppArguments.LeaderboardType.open) ? base.app.arguments.leaderboardsOpen : base.app.arguments.leaderboardsCampaign));
			if (leaderboards == null)
			{
				leaderboards = new DRLAppArguments.Leaderboards();
			}
			leaderboards.map = view.map;
			leaderboards.track = view.track;
			leaderboards.customMap = view.customMap;
			leaderboards.isCustomMap = view.isCustomMap;
			leaderboards.campaign = view.campaign;
			leaderboards.campaignIndex = view.campaignStepper.index;
			leaderboards.mission = view.mission;
			leaderboards.missionIndex = view.missionStepper.index;
			leaderboards.racePage = view.racePageField.index;
			leaderboards.campaignSelectd = campaignSelected;
			leaderboards.isCampaignRaceMode = campaignSelected != null && view.isCampaignRaceMode;
			leaderboards.campaignRaceModePage = view.campaignRacePageField.index;
			leaderboards.gameType = view.gameTypeFlag;
			leaderboards.physicsIndex = view.physicsStepper.index;
			leaderboards.sizeIndex = view.droneClassStepper.index;
			leaderboards.platformIndex = view.platformStepper.index;
			leaderboards.controllerTypeIndex = view.controllerTypeStepper.index;
			switch (base.app.arguments.lastLeaderboard)
			{
			case DRLAppArguments.LeaderboardType.drl:
				base.app.arguments.leaderboardsDRL = leaderboards;
				break;
			case DRLAppArguments.LeaderboardType.open:
				base.app.arguments.leaderboardsOpen = leaderboards;
				break;
			case DRLAppArguments.LeaderboardType.campaign:
				base.app.arguments.leaderboardsCampaign = leaderboards;
				break;
			}
			return leaderboards;
		}

		protected void CancelReplayLoad()
		{
			m_ignore_replay_click = false;
			if (m_replay_loader != null)
			{
				m_replay_loader.Cancel();
			}
		}

		protected void CancelReplayParse()
		{
			Debug.Log($"UILeaderboardController> CancelReplayParse / Thread Kill [{m_thread_kill}]");
			m_ignore_replay_click = false;
			m_thread_kill = true;
			if (m_replay_loader != null)
			{
				m_replay_loader.Cancel();
			}
		}

		protected void RefreshListDelayed(int page = 0, bool p_is_page_change = false)
		{
			view.Clear();
			if (!p_is_page_change)
			{
				view.ClearPages();
			}
			if (m_load_timer != null)
			{
				m_load_timer.Stop();
			}
			m_load_timer = Activity.RunOnce(delegate
			{
				RefreshList(page, p_is_page_change);
			}, 0.6f);
			CancelReplayLoad();
		}

		protected void RefreshList(int p_page, bool p_is_page_change = false)
		{
			view.SetCampaignRaceMode(p_flag: false);
			GetLeaderboard(p_page, p_is_page_change);
			CheckUserRace();
		}

		protected void GetLeaderboard(int p_page, bool p_is_page_change = false)
		{
			if (m_loader != null)
			{
				m_loader.Cancel();
			}
			if (view.circuit != null)
			{
				GetLeaderboardCircuits(p_page);
				return;
			}
			GameFlag gameTypeFlag = view.gameTypeFlag;
			DRLMap map = view.map;
			DRLMapTrack track = view.track;
			MapData customMap = view.customMap;
			DRLCampaign campaign = view.campaign;
			int num = p_page;
			int p_drone_class = ((view.drlOnly || gameTypeFlag == GameFlag.Campaign) ? 7 : view.droneClass);
			int p_physics = ((!view.drlOnly) ? view.physics : 0);
			string controllerTypeString = view.GetControllerTypeString();
			string platformString = view.GetPlatformString();
			view.SetFeedback(UILeaderboardFeedbackType.Loading);
			switch (gameTypeFlag)
			{
			case GameFlag.Race:
				if (view.circuitsStepper.index == 2)
				{
					isCollectable = true;
				}
				view.campaign = null;
				if (map != null && track != null)
				{
					m_loader = base.app.model.service.GetLeaderboard(map, track, num + 1, pageLength, p_drone_class, view.drlOnly, p_physics, delegate(DRLLeaderboardResult p_result)
					{
						OnLeaderboardRacesLoad(p_result, p_is_page_change);
					}, platformString, controllerTypeString, p_group: false, null, -1, isCollectable);
					Debug.Log("UILeaderboardController> RefreshList - game-type[" + gameTypeFlag.ToString() + "] map[" + map.scene + "] track[" + track.label + "] page[" + num + "] drone-class[" + p_drone_class + "] physics [" + p_physics + "]");
				}
				else if (customMap != null)
				{
					m_loader = base.app.model.service.GetLeaderboard(customMap.guid, num + 1, pageLength, p_drone_class, view.drlOnly, p_physics, delegate(DRLLeaderboardResult p_result)
					{
						OnLeaderboardRacesLoad(p_result, p_is_page_change);
					}, platformString, controllerTypeString, p_group: false, null, -1, GameFlag.Race, isCollectable);
					Debug.Log("UILeaderboardController> RefreshList - game-type[" + gameTypeFlag.ToString() + "] map[MULTIGP] track[" + customMap.guid + "] page[" + num + "] drone-class[" + p_drone_class + "] physics [" + p_physics + "]");
				}
				break;
			case GameFlag.Campaign:
				if ((bool)campaign)
				{
					Debug.Log("UILeaderboardController> RefreshList - game-type[" + gameTypeFlag.ToString() + "] campaign[" + campaign.label + "] drone-class[" + p_drone_class + "]");
					m_loader = base.app.model.service.GetLeaderboard(campaign, num + 1, pageLength, p_drone_class, delegate(DRLLeaderboardResult p_result)
					{
						OnLeaderboardRacesLoad(p_result, p_is_page_change);
					});
				}
				break;
			}
			base.app.view.ui.screens.manager.GetComponent<UIScreenManagerController>().ValidatePromo();
		}

		public void GetLeaderboardCircuits(int p_page)
		{
			GameFlag gameTypeFlag = view.gameTypeFlag;
			int num = p_page;
			int p_drone_class = ((view.drlOnly || gameTypeFlag == GameFlag.Campaign) ? 7 : view.droneClass);
			int p_physics = ((!view.drlOnly) ? view.physics : 0);
			string controllerTypeString = view.GetControllerTypeString();
			string platformString = view.GetPlatformString();
			view.SetFeedback(UILeaderboardFeedbackType.Loading);
			m_loader = base.app.model.service.GetLeaderboardCircuit(view.circuit.guid, num + 1, pageLength, p_drone_class, view.drlOnly, p_physics, OnCircuitLeaderboardRacesLoad, platformString, controllerTypeString);
			Debug.Log("UILeaderboardController> RefreshList - game-type[" + gameTypeFlag.ToString() + "] circuit[" + view.circuit.name + "] page[" + num + "] drone-class[" + p_drone_class + "] physics [" + p_physics + "]");
		}

		protected void RefreshCampaignRacePage(DRLLeaderboardData p_data, int p_page)
		{
			DRLLeaderboardData[] races = p_data.races;
			int num = pageLength - 1;
			int p_total = races.Length / num + 1;
			List<DRLLeaderboardData> list = new List<DRLLeaderboardData>();
			int num2 = p_page * num;
			for (int i = 0; i < num; i++)
			{
				if (num2 >= races.Length)
				{
					break;
				}
				list.Add(races[num2]);
				num2++;
			}
			ListComponent campaignRacesListField = view.campaignRacesListField;
			DRLPagePickerView campaignRacePageField = view.campaignRacePageField;
			PopulateResults(list.ToArray(), campaignRacesListField, campaignRacePageField, p_campaign_race: true, p_allow_replay: true, p_allow_save: false, p_page, p_total, p_data);
		}

		protected void OnLeaderboardRacesLoad(DRLLeaderboardResult p_result, bool p_is_page_change)
		{
			if (!(this == null) && !(view == null) && (m_loader == null || (m_loader.status != AsyncRequestStatus.Created && m_loader.status != AsyncRequestStatus.Cancelled)))
			{
				if (p_result == null)
				{
					Debug.LogWarning("UILeaderboardController> OnLeaderboardRacesLoad - Error Loading the Results!");
					m_loader = null;
					return;
				}
				DRLLeaderboardData[] p_races = (p_result.success ? p_result.leaderboard : null);
				ListComponent raceListField = view.raceListField;
				DRLPagePickerView racePageField = view.racePageField;
				bool p_allow_replay = view.gameTypeFlag != GameFlag.Campaign;
				bool p_allow_save = true;
				int p_page = p_result.pagging.page - 1;
				int pageTotal = p_result.pagging.pageTotal;
				PopulateResults(p_races, raceListField, racePageField, p_campaign_race: false, p_allow_replay, p_allow_save, p_page, pageTotal);
			}
		}

		protected void OnCircuitLeaderboardRacesLoad(DRLCircuitsResult p_result)
		{
			if (!(this == null) && !(view == null) && (m_loader == null || (m_loader.status != AsyncRequestStatus.Created && m_loader.status != AsyncRequestStatus.Cancelled)))
			{
				if (p_result == null)
				{
					Debug.LogWarning("UILeaderboardController> OnLeaderboardRacesLoad - Error Loading the Results!");
					m_loader = null;
					return;
				}
				DRLCircuitLeaderboardData[] leaderboard = p_result.leaderboard;
				ListComponent raceListField = view.raceListField;
				DRLPagePickerView racePageField = view.racePageField;
				bool p_allow_replay = false;
				bool p_allow_save = true;
				int p_page = p_result.pagging.page - 1;
				int pageTotal = p_result.pagging.pageTotal;
				PopulateResults(leaderboard, raceListField, racePageField, p_campaign_race: false, p_allow_replay, p_allow_save, p_page, pageTotal);
			}
		}

		private void PopulateResults(DRLLeaderboardData[] p_races, ListComponent p_list, DRLPagePickerView p_pages, bool p_campaign_race, bool p_allow_replay, bool p_allow_save, int p_page, int p_total, DRLLeaderboardData p_parent = null)
		{
			bool flag = p_races == null;
			List<DRLLeaderboardData> list = new List<DRLLeaderboardData>();
			if (p_races != null)
			{
				list.AddRange(p_races);
			}
			int index = p_page;
			int num = p_total;
			float num2 = 0f;
			bool p_allow_replay2 = p_allow_replay;
			bool flag2 = p_allow_save;
			Debug.Log("UILeaderboardController> PopulateResults [" + (flag ? "FAILED" : "SUCCESS") + "] - page[" + index + "] total[" + num + "] count[" + list.Count + "] parent[" + p_parent?.ToString() + "] campaign-race[" + p_campaign_race + "]");
			List<UINavigation> list2 = new List<UINavigation>();
			List<UINavigation> list3 = new List<UINavigation>();
			List<UINavigation> list4 = new List<UINavigation>();
			List<UINavigation> list5 = new List<UINavigation>();
			List<UINavigation> list6 = new List<UINavigation>();
			List<UINavigation> list7 = new List<UINavigation>();
			UINavigation component = p_pages.GetComponent<UINavigation>();
			Component listField = p_pages.listField;
			p_list.Clear();
			list.Sort((DRLLeaderboardData lba, DRLLeaderboardData lbb) => (lba.score >= lbb.score) ? 1 : (-1));
			if (p_parent != null)
			{
				bool flag3 = false;
				if (view.campaignItemField.data == null || ((p_parent.playerId == null) ? (view.campaignItemField.data.username != p_parent.username) : (view.campaignItemField.data.playerId != p_parent.playerId)))
				{
					bool selected = p_parent.playerId == base.app.model.service.backend.playerId;
					view.campaignItemField.Set(p_parent);
					view.campaignItemField.selected = selected;
				}
			}
			for (int num3 = 0; num3 < list.Count; num3++)
			{
				DRLLeaderboardData dRLLeaderboardData = list[num3];
				UILeaderboardItemView uILeaderboardItemView = p_list.Push<UILeaderboardItemView>();
				list2.Add(uILeaderboardItemView.entryNav);
				list3.Add(uILeaderboardItemView.replayNav);
				list4.Add(flag2 ? uILeaderboardItemView.saveNav : null);
				list5.Add(uILeaderboardItemView.saveNav);
				list6.Clear();
				list7.Clear();
				list6.Add(view.userButtonNav);
				list7.Add(view.mapSelectionNav);
				uILeaderboardItemView.Set(dRLLeaderboardData, p_allow_replay2, flag2, num2);
				uILeaderboardItemView.SetCampaignRaceMode(p_campaign_race);
				uILeaderboardItemView.FlagCustomPhysics(dRLLeaderboardData.customPhysics);
				uILeaderboardItemView.selected = dRLLeaderboardData.playerId == base.app.model.service.backend.playerId && !p_campaign_race;
				DRLCampaign dRLCampaign = (p_campaign_race ? view.campaign : null);
				if ((bool)dRLCampaign)
				{
					int p_phase = 0;
					int p_heat = 0;
					DRLCampaignRace race = dRLCampaign.GetRace(dRLLeaderboardData.order, out p_phase, out p_heat);
					if (race == null)
					{
						Debug.LogWarning("UILeaderboardsController > Race with id = " + dRLLeaderboardData.order + " does not exist");
						continue;
					}
					string text = race.phaseNames[p_phase];
					DRLMapTrack track = race.track;
					string label = track.map.label;
					string p_track = (race.isCustomMap ? race.customMap.mapTitle.ToUpper() : track.label);
					int num4 = (string.IsNullOrEmpty(text) ? num3 : p_heat);
					string text2 = "Heat " + (num4 + 1).ToString("00");
					string p_title = (string.IsNullOrEmpty(text) ? text2 : (text + " - " + text2));
					uILeaderboardItemView.SetCampaignRaceTitle(label, p_track, p_title);
				}
				num2 += 0.02f;
				if (num3 >= list.Count - 1)
				{
					component.up = uILeaderboardItemView.entryNav;
				}
			}
			UINavigation leftNavigation = view.leftNavigation;
			UINavigation p_right = null;
			UINavigation p_up = (p_campaign_race ? view.campaignItemNav : view.userButtonNav);
			SetMenuListNavigation(p_campaign_race);
			if (view.drlInputFieldView != null)
			{
				UINavigation.Link(list2.ToArray(), 0, p_vertical: true, leftNavigation, p_right, p_up, listField);
				UINavigation.Link(list3.ToArray(), 0, p_vertical: true, null, null, p_up, listField);
				UINavigation.Link(list5.ToArray(), 0, p_vertical: true, null, null, p_up, listField);
				UINavigation.Link(list6.ToArray(), 0, p_vertical: true, view.backButtonNav, view.drlInputFieldView, view.raceListField, view.circuitsStepper);
				UINavigation.Link(list7.ToArray(), 0, p_vertical: true, view.physicsStepper.isActiveAndEnabled ? view.physicsStepper : view.controllerTypeStepper, view.mapNav, view.drlInputFieldView, view.raceListField);
			}
			int count = list2.Count;
			for (int num5 = 0; num5 < count; num5++)
			{
				UINavigation uINavigation = list2[num5];
				UINavigation uINavigation2 = list3[num5];
				UINavigation uINavigation3 = list4[num5];
				if (uINavigation3 == null)
				{
					uINavigation.right = uINavigation2;
					uINavigation2.left = uINavigation;
					continue;
				}
				uINavigation.right = uINavigation3;
				uINavigation3.left = uINavigation;
				uINavigation3.right = uINavigation2;
				uINavigation2.left = uINavigation3;
			}
			UILeaderboardFeedbackType p_type = UILeaderboardFeedbackType.None;
			if (list.Count <= 0)
			{
				p_type = UILeaderboardFeedbackType.NoResult;
			}
			if (flag)
			{
				p_type = UILeaderboardFeedbackType.Failure;
			}
			view.SetFeedback(p_type);
			m_ignore_page_click = true;
			FadeComponent fadeComponent = (p_pages ? p_pages.fade : view.racePageField.fade);
			if (fadeComponent.alpha < 0f)
			{
				fadeComponent.alpha = 0f;
			}
			if (num > 1)
			{
				fadeComponent.FadeIn(0.3f);
			}
			else
			{
				fadeComponent.FadeOut(0.3f);
			}
			p_pages.Set(num);
			p_pages.index = index;
			m_ignore_page_click = false;
		}

		private void PopulateResults(DRLCircuitLeaderboardData[] p_races, ListComponent p_list, DRLPagePickerView p_pages, bool p_campaign_race, bool p_allow_replay, bool p_allow_save, int p_page, int p_total, DRLLeaderboardData p_parent = null)
		{
			bool flag = p_races == null;
			List<DRLCircuitLeaderboardData> list = new List<DRLCircuitLeaderboardData>();
			if (p_races != null)
			{
				list.AddRange(p_races);
			}
			int index = p_page;
			int num = p_total;
			float num2 = 0f;
			bool p_allow_replay2 = p_allow_replay;
			bool flag2 = p_allow_save;
			Debug.Log("UILeaderboardController> PopulateResults [" + (flag ? "FAILED" : "SUCCESS") + "] - page[" + index + "] total[" + num + "] count[" + list.Count + "] parent[" + p_parent?.ToString() + "] campaign-race[" + p_campaign_race + "]");
			List<UINavigation> list2 = new List<UINavigation>();
			List<UINavigation> list3 = new List<UINavigation>();
			List<UINavigation> list4 = new List<UINavigation>();
			List<UINavigation> list5 = new List<UINavigation>();
			List<UINavigation> list6 = new List<UINavigation>();
			List<UINavigation> list7 = new List<UINavigation>();
			UINavigation component = p_pages.GetComponent<UINavigation>();
			Component listField = p_pages.listField;
			p_list.Clear();
			if (p_campaign_race)
			{
				list.Sort((DRLCircuitLeaderboardData lba, DRLCircuitLeaderboardData lbb) => (lba.score >= lbb.score) ? 1 : (-1));
				if (p_parent != null)
				{
					bool flag3 = false;
					if (view.campaignItemField.data == null || view.campaignItemField.data.playerId != p_parent.playerId)
					{
						bool selected = p_parent.playerId == base.app.model.service.backend.playerId;
						view.campaignItemField.Set(p_parent);
						view.campaignItemField.selected = selected;
					}
				}
			}
			for (int num3 = 0; num3 < list.Count; num3++)
			{
				DRLCircuitLeaderboardData dRLCircuitLeaderboardData = list[num3];
				UILeaderboardItemView uILeaderboardItemView = p_list.Push<UILeaderboardItemView>();
				list2.Add(uILeaderboardItemView.entryNav);
				list3.Add(uILeaderboardItemView.replayNav);
				list4.Add(flag2 ? uILeaderboardItemView.saveNav : null);
				list5.Add(uILeaderboardItemView.saveNav);
				list6.Add(view.userButtonNav);
				list7.Add(view.mapSelectionNav);
				uILeaderboardItemView.Set(dRLCircuitLeaderboardData, p_allow_replay2, flag2, num2, dRLCircuitLeaderboardData.position);
				uILeaderboardItemView.SetCampaignRaceMode(p_campaign_race);
				uILeaderboardItemView.FlagCustomPhysics(dRLCircuitLeaderboardData.customPhysics);
				uILeaderboardItemView.selected = dRLCircuitLeaderboardData.playerId == base.app.model.service.backend.playerId && !p_campaign_race;
				num2 += 0.02f;
				if (num3 >= list.Count - 1)
				{
					component.up = uILeaderboardItemView.entryNav;
				}
			}
			UINavigation leftNavigation = view.leftNavigation;
			UINavigation p_right = null;
			UINavigation p_up = (p_campaign_race ? view.campaignItemNav : view.userButtonNav);
			SetMenuListNavigation(p_campaign_race);
			if (view.drlInputFieldView != null)
			{
				UINavigation.Link(list2.ToArray(), 0, p_vertical: true, leftNavigation, p_right, p_up, listField);
				UINavigation.Link(list3.ToArray(), 0, p_vertical: true, null, null, p_up, listField);
				UINavigation.Link(list5.ToArray(), 0, p_vertical: true, null, null, p_up, listField);
				UINavigation.Link(list6.ToArray(), 0, p_vertical: true, view.backButtonNav, view.drlInputFieldView, view.raceListField, view.circuitsStepper);
				UINavigation.Link(list7.ToArray(), 0, p_vertical: true, view.physicsStepper.isActiveAndEnabled ? view.physicsStepper : view.controllerTypeStepper, view.mapNav, view.drlInputFieldView, view.raceListField);
			}
			int count = list2.Count;
			for (int num4 = 0; num4 < count; num4++)
			{
				UINavigation uINavigation = list2[num4];
				UINavigation uINavigation2 = list3[num4];
				UINavigation uINavigation3 = list4[num4];
				if (uINavigation3 == null)
				{
					uINavigation.right = uINavigation2;
					uINavigation2.left = uINavigation;
					continue;
				}
				uINavigation.right = uINavigation3;
				uINavigation3.left = uINavigation;
				uINavigation3.right = uINavigation2;
				uINavigation2.left = uINavigation3;
			}
			UILeaderboardFeedbackType p_type = UILeaderboardFeedbackType.None;
			if (list.Count <= 0)
			{
				p_type = UILeaderboardFeedbackType.NoResult;
			}
			if (flag)
			{
				p_type = UILeaderboardFeedbackType.Failure;
			}
			view.SetFeedback(p_type);
			m_ignore_page_click = true;
			FadeComponent fadeComponent = (p_pages ? p_pages.fade : view.racePageField.fade);
			if (fadeComponent.alpha < 0f)
			{
				fadeComponent.alpha = 0f;
			}
			if (num > 1)
			{
				fadeComponent.FadeIn(0.3f);
			}
			else
			{
				fadeComponent.FadeOut(0.3f);
			}
			p_pages.Set(num);
			p_pages.index = index;
			m_ignore_page_click = false;
		}

		private void SetMenuListNavigation(bool p_campaign_race)
		{
			UINavigation uINavigation = (p_campaign_race ? view.campaignItemNav : view.gameTypeStepper.GetComponent<UINavigation>());
			Component down = (p_campaign_race ? ((MonoBehaviour)uINavigation) : ((MonoBehaviour)view.raceListField));
			view.userButtonNav.down = down;
			view.gameTypeStepper.GetComponent<UINavigation>().down = down;
			view.mapSelectionNav.down = down;
			view.mapNav.down = down;
			view.campaignStepper.GetComponent<UINavigation>().down = down;
			view.missionStepper.GetComponent<UINavigation>().down = down;
		}

		private void LoadGarage(string p_rig_data)
		{
			if (string.IsNullOrEmpty(p_rig_data))
			{
				return;
			}
			DroneRigData rig = DroneRigData.FromJson(p_rig_data);
			if (!(rig == null) && !rig.isLocked)
			{
				rig.guid = DroneRigData.GenerateGUID();
				rig.name = "";
				bool p_ingame = (bool)base.app && base.app.inGame;
				StorageModel storage = base.app.model.storage;
				base.enabled = false;
				storage.PreloadDroneBundleData(null, null, p_ingame, delegate
				{
					base.enabled = true;
					UIGarageRigEditView uIGarageRigEditView = base.app.view.ui.screens.Open<UIGarageRigEditView>("garage-rig-edit-screen");
					uIGarageRigEditView.data = rig;
					uIGarageRigEditView.data.isPublic = false;
				});
			}
		}

		private void LoadReplay(string p_url)
		{
			string replay_url = p_url.ToLower();
			view.SetFeedback(UILeaderboardFeedbackType.Loading);
			bool has_load_start = false;
			if (replay_url.IndexOf("@") == 0)
			{
				replay_url = replay_url.Substring(1);
				if (!File.Exists(replay_url))
				{
					m_ignore_replay_click = false;
					base.app.view.audio.PlayUIGenericError();
					view.SetFeedback(UILeaderboardFeedbackType.None);
					Debug.LogWarning("UILeaderboardsController> Failed to load replay from [" + replay_url + "]");
					return;
				}
				RunOnce(0.3f, delegate
				{
					has_load_start = true;
					base.app.view.audio.PlayUIGenericSuccess();
					view.progress = 0.5f;
					byte[] array = File.ReadAllBytes(replay_url);
					if (array != null)
					{
						LoadReplay(array);
					}
				});
				return;
			}
			m_replay_loader = Web.Get(replay_url, delegate(byte[] p_result, float p_progress, WebAsyncRequest p_request)
			{
				if (p_progress >= 1f && (p_result == null || p_result.Length == 0))
				{
					m_ignore_replay_click = false;
					m_thread_kill = false;
					if (p_request.status != AsyncRequestStatus.Cancelled)
					{
						base.app.view.audio.PlayUIGenericError();
					}
					view.SetFeedback(UILeaderboardFeedbackType.None);
					Debug.LogWarning("UILeaderboardsController> Failed to load replay from [" + replay_url + "]");
				}
				else
				{
					if (!has_load_start)
					{
						has_load_start = true;
						base.app.view.audio.PlayUIGenericSuccess();
					}
					if (p_progress < 1f)
					{
						view.progress = p_progress * 0.5f;
					}
					else
					{
						view.progress = 0.5f;
						if (p_result != null)
						{
							LoadReplay(p_result);
						}
					}
				}
			});
		}

		public void LoadReplay(byte[] p_data)
		{
			float thread_progress = 0.5f;
			bool thread_complete = false;
			new Thread((ThreadStart)delegate
			{
				if (m_thread_kill)
				{
					m_thread_kill = false;
					Debug.Log("UILeaderboardsController> LoadReplay / Pre Parse Cancelled");
					view.SetFeedback(UILeaderboardFeedbackType.None);
				}
				else
				{
					BlackboxRecord rec = null;
					ReplayFile rpl = null;
					if (ReplayFile.EnableVersion2)
					{
						rpl = ReplayFile.FromBytes(p_data);
					}
					else
					{
						rec = Serialize.FromBytes<BlackboxRecord>(p_data, p_unsafe: true);
						rec.Decompress();
					}
					thread_complete = true;
					if (m_thread_kill)
					{
						m_thread_kill = false;
						Debug.Log("UILeaderboardsController> LoadReplay / Post Parse Cancelled");
						view.SetFeedback(UILeaderboardFeedbackType.None);
					}
					else
					{
						Activity.RunOnce(delegate
						{
							if (m_thread_kill)
							{
								m_thread_kill = false;
								Debug.Log("UILeaderboardsController> LoadReplay / ReplayLoad Complete Cancelled");
								view.SetFeedback(UILeaderboardFeedbackType.None);
							}
							else
							{
								view.progress = 1f;
								Notify("leaderboards.replay.load@complete");
								base.app.view.ui.fade.FadeIn(1.5f);
								Activity.RunOnce(delegate
								{
									if (m_thread_kill)
									{
										m_thread_kill = false;
										Debug.Log("UILeaderboardsController> LoadReplay / ReplayLoad Scene Load Cancelled");
										view.SetFeedback(UILeaderboardFeedbackType.None);
									}
									else
									{
										bool flag = (ReplayFile.EnableVersion2 ? (rpl == null) : (rec == null));
										Debug.Log($"UILeaderboardsController> Load Complete success / replay-valid[{!flag}]");
										if (flag)
										{
											m_ignore_replay_click = false;
											base.app.view.audio.PlayUIGenericError();
											view.SetFeedback(UILeaderboardFeedbackType.None);
											base.app.view.ui.fade.FadeOut();
											Debug.LogWarning("UILeaderboardsController> Failed to load replay");
										}
										else
										{
											DRLAppArguments.Leaderboards leaderboards = SaveArgs();
											string text = (leaderboards.isCustomMap ? leaderboards.customMap.guid : "");
											string text2 = (leaderboards.map ? leaderboards.map.guid : (leaderboards.isCustomMap ? leaderboards.customMap.mapId : ""));
											string text3 = (leaderboards.track ? leaderboards.track.guid : "");
											Debug.LogWarning("UILeaderboardsController> Loading Replay Scene / map[" + text2 + "] track[" + text3 + "] custom-map[" + text + "]");
											object p_replay = (ReplayFile.EnableVersion2 ? ((object)rpl) : ((object)rec));
											base.app.scene.Load(p_replay);
										}
									}
								}, 1f);
							}
						});
					}
				}
			}).Start();
			Activity.Run((Func<bool>)delegate
			{
				if (m_thread_kill)
				{
					return false;
				}
				if (!m_ignore_replay_click)
				{
					return false;
				}
				if (thread_complete)
				{
					return false;
				}
				float num = Mathf.Lerp(0.1f, 0.05f, Mathf.Clamp01((thread_progress - 0.5f) / 0.5f));
				thread_progress += Time.deltaTime * 0.5f * num;
				thread_progress = Mathf.Clamp01(thread_progress);
				view.progress = thread_progress;
				return true;
			}, 0f, false);
		}
	}
}

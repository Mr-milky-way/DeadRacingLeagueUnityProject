using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using drl.backend;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIOpponentSelectionController : Controller<DRLApp>
	{
		public int pageLength = 10;

		public DRLLeaderboardData campaignSelected;

		private readonly Dictionary<string, DRLLeaderboardData> m_selectedItems = new Dictionary<string, DRLLeaderboardData>();

		private const int MaxSelectItem = 5;

		private WebAsyncRequest m_loader;

		private WebAsyncRequest m_userSearch;

		private Activity m_loadTimer;

		private bool m_ignorePageClick;

		private bool m_starting;

		private const string Racer4Guid = "DRD-fc5bf84d13e5bac67957921c";

		private UIOpponentSelectionView view => AssertLocal<UIOpponentSelectionView>("view");

		private void Awake()
		{
			PopulateControllerTypeStepper();
		}

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "maps.selection-complete":
				if (!base.app.controller.AssertMapSelection(p_target, this, p_need_return: true))
				{
					break;
				}
				_ = (string)p_data[0];
				_ = (string)p_data[1];
				_ = (string)p_data[2];
				if ((bool)p_data[3])
				{
					if (!(p_data[6] is MapData))
					{
						break;
					}
					MapData customMap = (MapData)p_data[6];
					view.SetCustomMap(customMap);
				}
				else
				{
					if (!(p_data[4] is DRLMap) || !(p_data[5] is DRLMapTrack))
					{
						Debug.LogError("UIOpponentSelectionController> MapSelectionComplete received invalid DRLMap or DRLMapTrack");
						break;
					}
					DRLMap dRLMap = (DRLMap)p_data[4];
					DRLMapTrack dRLMapTrack = (DRLMapTrack)p_data[5];
					view.map = dRLMap;
					view.track = dRLMapTrack;
					view.SetMap(dRLMap, dRLMapTrack);
				}
				RefreshListDelayed();
				m_ignorePageClick = true;
				view.racePageField.index = 0;
				m_ignorePageClick = false;
				SaveArgs();
				break;
			case "ui.screen@close":
				if (p_data != null && p_data.Length != 0)
				{
					UIScreen uIScreen = p_data[0] as UIScreen;
					if (!(uIScreen == null))
					{
						_ = uIScreen != view.screen;
					}
				}
				break;
			}
			if (base.app.view.ui.screens.current != view.screen || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				if (!(p_data[0] as UIScreen != view.screen))
				{
					m_starting = false;
					m_selectedItems.Clear();
					view.droneRigData = base.app.model.storage.state.player.garage.currentRigData;
					view.regionId = "";
					view.raceListFade.alpha = -0.1f;
					view.campaignRacesListFade.alpha = -0.1f;
					view.SetFeedback(UIOpponentSelectionFeedbackType.Loading, p_hide_list: true);
					ListComponent raceListField = view.raceListField;
					raceListField.Clear();
					for (int i = 0; i < pageLength; i++)
					{
						UIOpponentSelectionItemView uIOpponentSelectionItemView = raceListField.Push<UIOpponentSelectionItemView>();
						uIOpponentSelectionItemView.fade.alpha = -0.1f;
						uIOpponentSelectionItemView.selected = false;
					}
					raceListField = view.campaignRacesListField;
					raceListField.Clear();
					for (int j = 0; j < pageLength; j++)
					{
						UIOpponentSelectionItemView uIOpponentSelectionItemView2 = raceListField.Push<UIOpponentSelectionItemView>();
						uIOpponentSelectionItemView2.fade.alpha = -0.1f;
						uIOpponentSelectionItemView2.selected = false;
					}
					view.SetTopVisible(p_flag: false);
					view.SetSelectionCount(m_selectedItems.Count);
					int p_physics = ((!view.droneRigData.hasCustomPhysics) ? 1 : 0);
					GarageStateModel garage = base.app.model.storage.state.player.garage;
					DroneRigData currentRigData = garage.currentRigData;
					DroneRigData p_base_drone;
					bool num = garage.TryGetBaseDrone(currentRigData, out p_base_drone);
					int p_drone_class = view.SpecificDroneClassIndex;
					if (!num)
					{
						p_drone_class = currentRigData.diameter - 1;
					}
					SaveArgs(p_physics, p_drone_class);
					LoadArgs();
					CheckUserRace();
				}
				break;
			case "opponent-selection.filter.form.event@change":
				OnFormNotification(p_target, p_is_change: true);
				break;
			case "opponent-selection.filter.form.event@click":
				OnFormNotification(p_target, p_is_change: false);
				break;
			case "opponent-selection.item@click":
			{
				UIElementView uIElementView2 = p_target as UIElementView;
				if ((bool)uIElementView2)
				{
					UIOpponentSelectionItemView componentInParent = uIElementView2.GetComponentInParent<UIOpponentSelectionItemView>();
					HandleItemClick(componentInParent);
				}
				break;
			}
			case "opponent-selection.start@click":
				LoadBotsAndStartRace();
				break;
			case "opponent-selection.page@select":
				if (!m_ignorePageClick)
				{
					int num5 = (int)p_data[0];
					bool isCampaignRaceMode = view.isCampaignRaceMode;
					Debug.Log($"UIOpponentSelectionController> Page Select [{num5}] campaign[{isCampaignRaceMode}]");
					if (isCampaignRaceMode)
					{
						RefreshCampaignRacePage(campaignSelected, num5);
					}
					else
					{
						RefreshListDelayed(num5, p_is_page_change: true);
					}
				}
				break;
			case "opponent-selection.page-next@click":
			{
				if (m_ignorePageClick)
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
			case "opponent-selection.page-previous@click":
			{
				if (m_ignorePageClick)
				{
					break;
				}
				int num2 = (view.isCampaignRaceMode ? view.campaignRacePageField.index : view.racePageField.index);
				if (num2 != 0)
				{
					if (view.isCampaignRaceMode)
					{
						view.campaignRacePageField.index = num2 - 1;
						RefreshCampaignRacePage(campaignSelected, num2);
					}
					else
					{
						view.racePageField.index = num2 - 1;
						RefreshListDelayed(view.racePageField.index, p_is_page_change: true);
					}
				}
				break;
			}
			case "ui.screen.return@click":
				if (base.app.view.ui.screens.manager.IsInHistory("tournament-leaders-screen"))
				{
					base.app.view.ui.screens.Return();
					break;
				}
				base.app.model.service.opponent.Cancel();
				if (view.isCampaignRaceMode)
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
					m_starting = false;
					base.app.view.ui.screens.Return();
				}
				break;
			case "ui.screen.nav-left@click":
			{
				UIElementView uIElementView = p_target as UIElementView;
				if (!(uIElementView == null))
				{
					string text = uIElementView.name;
					if (text != null && text == "exit" && (bool)base.app.controller.game)
					{
						base.app.controller.game.Exit();
					}
				}
				break;
			}
			}
		}

		private void PopulateControllerTypeStepper()
		{
			string[] labels = new string[4] { "ALL", "RADIO", "PS", "XBOX" };
			view.controllerTypeStepper.labels = labels;
			view.controllerTypeStepper.max = 3;
			view.controllerTypeStepper.icons = view.controllerTypeIcons_Standalone;
		}

		private void HandleItemClick(UIOpponentSelectionItemView p_item)
		{
			DRLLeaderboardData dRLLeaderboardData = (campaignSelected = p_item.data);
			Debug.Log(string.Format("UIOpponentSelectionController> Clicked User Item [{0}] replay[{1}] {2} {3}", p_item, p_item ? p_item.replayURL : "", dRLLeaderboardData.droneName, dRLLeaderboardData.controllerType));
			AddOrRemoveSelectedItem(p_item);
		}

		private void AddOrRemoveSelectedItem(UIOpponentSelectionItemView p_item)
		{
			DRLLeaderboardData data = p_item.data;
			string text = data?.id;
			if (data == null || string.IsNullOrEmpty(text))
			{
				Debug.LogWarning("UIOpponentSelectionController> AddOrRemoveSelectedItem / ID is null or empty");
				return;
			}
			if (m_selectedItems.ContainsKey(data.id))
			{
				m_selectedItems.Remove(text);
				p_item.selected = false;
			}
			else
			{
				if (m_selectedItems.Count >= 5)
				{
					return;
				}
				m_selectedItems.Add(text, data);
				p_item.selected = true;
			}
			base.app.view.audio.PlayUIClick();
			view.SetSelectionCount(m_selectedItems.Count);
			bool interactable = m_selectedItems.Count > 0;
			view.startNav.GetComponent<UIElementView>().interactable = interactable;
		}

		private void OnFormNotification(UnityEngine.Object p_target, bool p_is_change)
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
						m_ignorePageClick = true;
						view.racePageField.index = 0;
						m_ignorePageClick = false;
					}
				}
				break;
			case "drone-class":
				if (flag)
				{
					RefreshListDelayed();
					m_ignorePageClick = true;
					view.racePageField.index = 0;
					m_ignorePageClick = false;
				}
				break;
			case "controller-type":
				if (flag)
				{
					RefreshListDelayed();
					m_ignorePageClick = true;
					view.racePageField.index = 0;
					m_ignorePageClick = false;
				}
				break;
			case "physics":
			case "physics-stepper":
				if (flag)
				{
					RefreshListDelayed();
					m_ignorePageClick = true;
					view.racePageField.index = 0;
					m_ignorePageClick = false;
				}
				break;
			case "campaign":
				if (flag)
				{
					DRLStepperView campaignStepper = view.campaignStepper;
					view.PopulateCampaigns(campaignStepper.index);
					RefreshListDelayed();
					m_ignorePageClick = true;
					view.racePageField.index = 0;
					m_ignorePageClick = false;
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
				m_ignorePageClick = true;
				view.racePageField.index = 0;
				m_ignorePageClick = false;
				break;
			}
			}
		}

		private void FindRaceUser()
		{
			view.SetFeedback(UIOpponentSelectionFeedbackType.Loading, p_hide_list: false);
			if (m_userSearch != null)
			{
				m_userSearch.Cancel();
			}
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
			DroneRigData currentRigData = base.app.model.storage.state.player.garage.currentRigData;
			int num = view.droneClass;
			int physics = view.physics;
			GarageStateModel garage = base.app.model.storage.state.player.garage;
			DroneRigData currentRigData2 = garage.currentRigData;
			DroneRigData p_base_drone;
			bool flag = garage.TryGetBaseDrone(currentRigData2, out p_base_drone);
			bool? p_drone_official = null;
			string p_circuitId = currentRigData.guid;
			if (num == view.SpecificDroneClassIndex + 1)
			{
				num = currentRigData.diameter;
				p_circuitId = p_base_drone.guid;
				if (flag && p_base_drone.guid == "DRD-fc5bf84d13e5bac67957921c")
				{
					p_drone_official = true;
					p_circuitId = null;
				}
			}
			Debug.Log($"UIOpponentSelectionController> FindRaceUser / rig.hasCustomPhysics: {currentRigData.hasCustomPhysics} physics: {physics}");
			string controllerTypeString = GetControllerTypeString();
			if (view.isCustomMap)
			{
				m_userSearch = base.app.model.service.GetLeaderboardUser(view.customMap, pageLength, num, p_drone_official, physics, delegate(DRLLeaderboardResult p_result)
				{
					if (!(this == null))
					{
						if (p_result == null)
						{
							SetErrorFeedback("Failed to find player.");
						}
						else if (m_userSearch.status != AsyncRequestStatus.Cancelled && m_userSearch.status != AsyncRequestStatus.Created)
						{
							int page = p_result.pagging.page;
							if (page <= 0)
							{
								SetErrorFeedback("User not found!");
							}
							else
							{
								Debug.Log("UIOpponentSelectionController> FindRaceUser - page[" + page + "]");
								view.racePageField.index = page - 1;
								if (p_result.pagging.page > p_result.pagging.pageTotal)
								{
									Debug.Log($"UIOpponentSelectionController> FindRaceUser / {p_result.pagging.page}/{p_result.pagging.pageTotal}");
									view.racePageField.index = p_result.pagging.pageTotal - 1;
								}
								RefreshListDelayed(view.racePageField.index, p_is_page_change: true);
							}
						}
					}
				}, null, controllerTypeString, p_circuitId);
				return;
			}
			m_userSearch = base.app.model.service.GetLeaderboardUser(view.map, view.track, pageLength, num, p_drone_official, physics, delegate(DRLLeaderboardResult p_result)
			{
				if (!(this == null))
				{
					if (p_result == null)
					{
						SetErrorFeedback("Failed to find player.");
					}
					else if (m_userSearch.status != AsyncRequestStatus.Cancelled && m_userSearch.status != AsyncRequestStatus.Created)
					{
						int page = p_result.pagging.page;
						if (page <= 0)
						{
							SetErrorFeedback("User not found!");
						}
						else
						{
							Debug.Log("UIOpponentSelectionController> FindRaceUser - page[" + page + "]");
							view.racePageField.index = page - 1;
							if (p_result.pagging.page > p_result.pagging.pageTotal)
							{
								Debug.Log($"UIOpponentSelectionController> FindRaceUser / {p_result.pagging.page}/{p_result.pagging.pageTotal}");
								view.racePageField.index = p_result.pagging.pageTotal - 1;
							}
							RefreshListDelayed(view.racePageField.index, p_is_page_change: true);
						}
					}
				}
			}, null, controllerTypeString, p_circuitId);
		}

		private void CheckUserRace()
		{
			if ((!view.isCustomMap && !view.map) || (!view.isCustomMap && !view.track) || (view.isCustomMap && string.IsNullOrEmpty(view.customMap.guid)))
			{
				return;
			}
			DroneRigData currentRigData = base.app.model.storage.state.player.garage.currentRigData;
			int num = view.droneClass;
			GarageStateModel garage = base.app.model.storage.state.player.garage;
			DroneRigData currentRigData2 = garage.currentRigData;
			DroneRigData p_base_drone;
			bool flag = garage.TryGetBaseDrone(currentRigData2, out p_base_drone);
			bool? p_drone_official = null;
			string p_drone_guid = currentRigData.guid;
			if (num == view.SpecificDroneClassIndex + 1)
			{
				num = currentRigData.diameter;
				p_drone_guid = p_base_drone.guid;
				if (flag && p_base_drone.guid == "DRD-fc5bf84d13e5bac67957921c")
				{
					p_drone_official = true;
					p_drone_guid = null;
				}
			}
			int physics = view.physics;
			string controllerTypeString = GetControllerTypeString();
			if (view.isCustomMap)
			{
				base.app.model.service.GetLeaderboardUser(view.customMap, pageLength, num, p_drone_official, physics, delegate(DRLLeaderboardResult p_result)
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
				}, null, controllerTypeString, null, p_drone_guid);
				return;
			}
			base.app.model.service.GetLeaderboardUser(view.map, view.track, pageLength, num, p_drone_official, physics, delegate(DRLLeaderboardResult p_result)
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
			}, null, controllerTypeString, null, p_drone_guid);
		}

		private void FindCampaignUser()
		{
			view.SetFeedback(UIOpponentSelectionFeedbackType.Loading, p_hide_list: false);
			if (m_userSearch != null)
			{
				m_userSearch.Cancel();
			}
			if (!view.campaign)
			{
				SetErrorFeedback("Invalid Campaign!");
				return;
			}
			m_userSearch = base.app.model.service.GetLeaderboardUser(view.campaign, pageLength, 6, delegate(DRLLeaderboardResult p_result)
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
						Debug.Log("UIOpponentSelectionController> FindCampaignUser - page[" + page + "]");
						view.campaignRacePageField.index = page - 1;
						RefreshListDelayed(view.campaignRacePageField.index, p_is_page_change: true);
					}
				}
			}, GetControllerTypeString());
		}

		private void SetErrorFeedback(string p_log)
		{
			Debug.LogWarning("UIOpponentSelectionController> " + p_log);
			base.app.view.audio.PlayUIGenericError();
			view.SetFeedback(UIOpponentSelectionFeedbackType.NoResult, p_hide_list: false);
			Activity.RunOnce(delegate
			{
				view.SetFeedback(UIOpponentSelectionFeedbackType.None);
			}, 1f);
		}

		private void LoadArgs()
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
			view.droneClassStepper.index = leaderboards.sizeIndex;
			view.droneClassStepper.Refresh();
			view.physicsStepper.index = leaderboards.physicsIndex;
			view.physicsStepper.Refresh();
			view.controllerTypeStepper.index = leaderboards.controllerTypeIndex;
			view.controllerTypeStepper.Refresh();
			view.SetCampaignRaceMode(leaderboards.isCampaignRaceMode);
			view.SetGameType(leaderboards.gameType);
			view.SetCustomMap(view.customMap);
			view.SetMap(view.map, view.track);
			campaignSelected = leaderboards.campaignSelectd;
			if (campaignSelected == null)
			{
				leaderboards.isCampaignRaceMode = false;
			}
			if (leaderboards.isCampaignRaceMode)
			{
				view.SetCampaign(view.campaign);
				RefreshCampaignRacePage(campaignSelected, leaderboards.campaignRaceModePage);
				return;
			}
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

		private DRLAppArguments.Leaderboards SaveArgs(int p_physics, int p_drone_class, int p_controller_type = 0)
		{
			if (p_physics < 0 || p_physics > 1)
			{
				p_physics = 1;
			}
			view.physicsStepper.index = p_physics;
			view.droneClass = p_drone_class + 1;
			view.controllerTypeStepper.index = p_controller_type;
			return SaveArgs();
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

		private void RefreshListDelayed(int p_page = 0, bool p_is_page_change = false)
		{
			view.Clear();
			if (!p_is_page_change)
			{
				view.ClearPages();
			}
			if (m_loadTimer != null)
			{
				m_loadTimer.Stop();
			}
			m_loadTimer = Activity.RunOnce(delegate
			{
				RefreshList(p_page, p_is_page_change);
			}, 0.6f);
		}

		private void RefreshList(int p_page, bool p_is_page_change = false)
		{
			view.SetCampaignRaceMode(p_flag: false);
			GetLeaderboard(p_page, p_is_page_change);
			CheckUserRace();
		}

		private void GetLeaderboard(int p_page, bool p_is_page_change = false)
		{
			if (m_loader != null)
			{
				m_loader.Cancel();
			}
			GameFlag gameTypeFlag = view.gameTypeFlag;
			DRLMap map = view.map;
			DRLMapTrack track = view.track;
			MapData customMap = view.customMap;
			DRLCampaign campaign = view.campaign;
			int num = p_page;
			int num2 = view.droneClass;
			int physics = view.physics;
			string controllerTypeString = GetControllerTypeString();
			view.SetFeedback(UIOpponentSelectionFeedbackType.Loading, p_hide_list: true);
			switch (gameTypeFlag)
			{
			case GameFlag.Race:
			{
				GarageStateModel garage = base.app.model.storage.state.player.garage;
				DroneRigData currentRigData = garage.currentRigData;
				DroneRigData p_base_drone;
				bool num3 = garage.TryGetBaseDrone(currentRigData, out p_base_drone);
				string guid = currentRigData.guid;
				if (num3)
				{
					guid = p_base_drone.guid;
				}
				view.campaign = null;
				if (map != null && track != null)
				{
					if (num2 == view.SpecificDroneClassIndex + 1)
					{
						num2 = currentRigData.diameter;
						if (p_base_drone.guid == "DRD-fc5bf84d13e5bac67957921c")
						{
							m_loader = base.app.model.service.GetLeaderboard(map, track, num + 1, pageLength, num2, true, physics, delegate(DRLLeaderboardResult p_result)
							{
								OnLeaderboardRacesLoad(p_result, p_is_page_change);
							}, null, controllerTypeString);
						}
						else
						{
							m_loader = base.app.model.service.GetLeaderboard(map, track, num + 1, pageLength, num2, null, physics, delegate(DRLLeaderboardResult p_result)
							{
								OnLeaderboardRacesLoad(p_result, p_is_page_change);
							}, null, controllerTypeString, p_group: false, guid);
						}
					}
					else
					{
						m_loader = base.app.model.service.GetLeaderboard(map, track, num + 1, pageLength, num2, null, physics, delegate(DRLLeaderboardResult p_result)
						{
							OnLeaderboardRacesLoad(p_result, p_is_page_change);
						}, null, controllerTypeString);
					}
					Debug.Log("UIOpponentSelectionController> RefreshList - game-type[" + gameTypeFlag.ToString() + "] map[" + map.scene + "] track[" + track.label + "] page[" + num + "] drone-class[" + num2 + "] physics [" + physics + "]");
				}
				else
				{
					if (customMap == null)
					{
						break;
					}
					if (num2 == view.SpecificDroneClassIndex + 1)
					{
						num2 = currentRigData.diameter;
						if (p_base_drone.guid == "DRD-fc5bf84d13e5bac67957921c")
						{
							m_loader = base.app.model.service.GetLeaderboard(customMap.guid, num + 1, pageLength, num2, true, physics, delegate(DRLLeaderboardResult p_result)
							{
								OnLeaderboardRacesLoad(p_result, p_is_page_change);
							}, null, controllerTypeString);
						}
						else
						{
							m_loader = base.app.model.service.GetLeaderboard(customMap.guid, num + 1, pageLength, num2, null, physics, delegate(DRLLeaderboardResult p_result)
							{
								OnLeaderboardRacesLoad(p_result, p_is_page_change);
							}, null, controllerTypeString, p_group: false, guid);
						}
					}
					else
					{
						m_loader = base.app.model.service.GetLeaderboard(customMap.guid, num + 1, pageLength, num2, null, physics, delegate(DRLLeaderboardResult p_result)
						{
							OnLeaderboardRacesLoad(p_result, p_is_page_change);
						}, null, controllerTypeString);
					}
					Debug.Log("UIOpponentSelectionController> RefreshList - game-type[" + gameTypeFlag.ToString() + "] map[MULTIGP] track[" + customMap.guid + "] page[" + num + "] drone-class[" + num2 + "] physics [" + physics + "]");
				}
				break;
			}
			case GameFlag.Campaign:
				if ((bool)campaign)
				{
					Debug.Log("UIOpponentSelectionController> RefreshList - game-type[" + gameTypeFlag.ToString() + "] campaign[" + campaign.label + "] drone-class[" + num2 + "]");
					m_loader = base.app.model.service.GetLeaderboard(campaign, num + 1, pageLength, num2, delegate(DRLLeaderboardResult p_result)
					{
						OnLeaderboardRacesLoad(p_result, p_is_page_change);
					});
				}
				break;
			}
			base.app.view.ui.screens.manager.GetComponent<UIScreenManagerController>().ValidatePromo();
		}

		private string GetControllerTypeString()
		{
			switch (view.controllerTypeStepper.index)
			{
			case 0:
				return null;
			case 1:
				return "Taranis";
			case 2:
				return "XBox";
			case 3:
				return "PS4";
			default:
				Debug.LogWarning("DRLLeaderboardData> controllerTypeFlag / Parameter out-of-range");
				return "Taranis";
			}
		}

		private void RefreshCampaignRacePage(DRLLeaderboardData p_data, int p_page)
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
			PopulateResults(list.ToArray(), campaignRacesListField, campaignRacePageField, p_campaign_race: true, p_page, p_total, p_data);
		}

		private void OnLeaderboardRacesLoad(DRLLeaderboardResult p_result, bool p_is_page_change)
		{
			if (!(this == null) && !(view == null) && (m_loader == null || (m_loader.status != AsyncRequestStatus.Created && m_loader.status != AsyncRequestStatus.Cancelled)))
			{
				if (p_result == null)
				{
					Debug.LogWarning("UIOpponentSelectionController> OnLeaderboardRacesLoad - Error Loading the Results!");
					m_loader = null;
					return;
				}
				DRLLeaderboardData[] p_races = (p_result.success ? p_result.leaderboard : null);
				ListComponent raceListField = view.raceListField;
				DRLPagePickerView racePageField = view.racePageField;
				int p_page = p_result.pagging.page - 1;
				int pageTotal = p_result.pagging.pageTotal;
				PopulateResults(p_races, raceListField, racePageField, p_campaign_race: false, p_page, pageTotal);
			}
		}

		private void PopulateResults(DRLLeaderboardData[] p_races, ListComponent p_list, DRLPagePickerView p_pages, bool p_campaign_race, int p_page, int p_total, DRLLeaderboardData p_parent = null)
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
			Debug.Log("UIOpponentSelectionController> PopulateResults [" + (flag ? "FAILED" : "SUCCESS") + "] - page[" + index + "] total[" + num + "] count[" + list.Count + "] parent[" + p_parent?.ToString() + "] campaign-race[" + p_campaign_race + "]");
			List<UINavigation> list2 = new List<UINavigation>();
			UINavigation component = p_pages.GetComponent<UINavigation>();
			Component listField = p_pages.listField;
			p_list.Clear();
			if (p_campaign_race)
			{
				list.Sort((DRLLeaderboardData lba, DRLLeaderboardData lbb) => (lba.order >= lbb.order) ? 1 : (-1));
				if (p_parent != null)
				{
					bool flag2 = false;
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
				DRLLeaderboardData dRLLeaderboardData = list[num3];
				UIOpponentSelectionItemView uIOpponentSelectionItemView = p_list.Push<UIOpponentSelectionItemView>();
				list2.Add(uIOpponentSelectionItemView.entryNav);
				uIOpponentSelectionItemView.Set(dRLLeaderboardData, num2);
				uIOpponentSelectionItemView.selected = m_selectedItems.ContainsKey(dRLLeaderboardData.id);
				uIOpponentSelectionItemView.SetCampaignRaceMode(p_campaign_race);
				uIOpponentSelectionItemView.FlagCustomPhysics(dRLLeaderboardData.customPhysics);
				bool active = dRLLeaderboardData.playerId == base.app.model.service.backend.playerId && !p_campaign_race;
				uIOpponentSelectionItemView.backgroundGreen.gameObject.SetActive(active);
				DRLCampaign dRLCampaign = (p_campaign_race ? view.campaign : null);
				if ((bool)dRLCampaign)
				{
					int p_phase;
					int p_heat;
					DRLCampaignRace race = dRLCampaign.GetRace(dRLLeaderboardData.order, out p_phase, out p_heat);
					if (race == null)
					{
						Debug.LogWarning("UIOpponentSelectionController > Race with id = " + dRLLeaderboardData.order + " does not exist");
						continue;
					}
					string text = race.phaseNames[p_phase];
					DRLMapTrack track = race.track;
					string label = track.map.label;
					string p_track = (race.isCustomMap ? race.customMap.mapTitle.ToUpper() : track.label);
					int num4 = (string.IsNullOrEmpty(text) ? num3 : p_heat);
					string text2 = "Heat " + (num4 + 1).ToString("00");
					string p_title = (string.IsNullOrEmpty(text) ? text2 : (text + " - " + text2));
					uIOpponentSelectionItemView.SetCampaignRaceTitle(label, p_track, p_title);
				}
				num2 += 0.02f;
				if (num3 >= list.Count - 1)
				{
					component.up = uIOpponentSelectionItemView.entryNav;
				}
			}
			UINavigation leftNavigation = view.leftNavigation;
			UINavigation startNav = view.startNav;
			UINavigation p_up = (p_campaign_race ? view.campaignItemNav : view.userButtonNav);
			SetMenuListNavigation(p_campaign_race);
			UINavigation.Link(list2.ToArray(), 0, p_vertical: true, leftNavigation, startNav, p_up, listField);
			UIOpponentSelectionFeedbackType feedback = UIOpponentSelectionFeedbackType.None;
			if (list.Count <= 0)
			{
				feedback = UIOpponentSelectionFeedbackType.NoResult;
			}
			if (flag)
			{
				feedback = UIOpponentSelectionFeedbackType.Failure;
			}
			view.SetFeedback(feedback);
			m_ignorePageClick = true;
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
			m_ignorePageClick = false;
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

		private void LoadBotsAndStartRace()
		{
			if (m_starting)
			{
				Debug.Log("UIOpponentSelectionController> LoadGame / Already starting! Ignored");
				return;
			}
			m_starting = true;
			ServiceModel service = base.app.model.service;
			service.opponent.Cancel();
			DRLLeaderboardData[] array = m_selectedItems.Values.ToArray();
			string[] array2 = new string[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = array[i].replayURL;
			}
			service.opponent.Load(array2, 5, delegate
			{
				switch (service.opponent.status)
				{
				case OpponentModel.Status.Error:
					Debug.Log("UIOpponentSelectionController> LoadGame / OpponentModel.Status.Error");
					view.SetFeedback(UIOpponentSelectionFeedbackType.Failure, p_hide_list: true, 2f);
					base.app.view.audio.PlayUIGenericError();
					service.opponent.Cancel();
					break;
				case OpponentModel.Status.NoResults:
					view.SetFeedback(UIOpponentSelectionFeedbackType.NoResult, p_hide_list: true, 2f);
					service.opponent.Cancel();
					Notify(1.5f, "fly.map-track-overview.ready", new MapLoadData(view.map, view.track, view.customMap, OpponentModeType.Custom));
					break;
				case OpponentModel.Status.ByPass:
					Notify("fly.map-track-overview.ready", new MapLoadData(view.map, view.track, view.customMap, OpponentModeType.Custom));
					break;
				case OpponentModel.Status.Progress:
					view.SetFeedback(UIOpponentSelectionFeedbackType.Loading, p_hide_list: true);
					view.progress = service.opponent.progress;
					break;
				case OpponentModel.Status.Complete:
					view.SetFeedback(UIOpponentSelectionFeedbackType.None, p_hide_list: false);
					Notify(1f / 60f, "fly.map-track-overview.ready", new MapLoadData(view.map, view.track, view.customMap, OpponentModeType.Custom, service.opponent.ghostRecords, service.opponent.ghostRecordsV2));
					break;
				case OpponentModel.Status.ManifestSuccess:
					view.progress = 0f;
					base.app.view.audio.PlayUIGenericSuccess();
					break;
				}
			});
		}
	}
}

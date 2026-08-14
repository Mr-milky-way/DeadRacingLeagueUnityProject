using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UICommunityMapsController : Controller<DRLApp>
	{
		public List<DRLCommunityMapData> mapsList;

		public int pageLength = 4;

		private int m_pagesTotalCount;

		private int m_currentPage;

		private MonoActivity m_refreshTimer;

		private MonoActivity m_searchTimer;

		private bool m_showing;

		public SortType sortingCriteria = SortType.ScoreDesc;

		public MapData currentMap;

		public string searchQuery;

		public int mapDifficulty = -1;

		public string mapId = "";

		public int isRaceAllowed;

		private bool m_lockUI;

		private bool m_lockRefresh;

		protected WebAsyncRequest m_webLoader;

		protected Activity m_diskLoader;

		protected MonoActivity m_deleteButtonCoolDown;

		private string m_previousSearchQuery;

		public UICommunityMapsView view => AssertLocal<UICommunityMapsView>("view");

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@close":
				view.SetRightNavigationEnabled(p_flag: false);
				m_previousSearchQuery = "";
				view.showCategory = GameFlag.MapCommon;
				break;
			case "ui.screen@open":
			{
				if (view.caller == null)
				{
					view.showCategory = GameFlag.MapCommon;
				}
				else if (view.showCategory == GameFlag.Collectable)
				{
					base.app.arguments.game.type = GameFlag.Collectable;
				}
				else
				{
					view.showCategory = GameFlag.MapCommon;
				}
				if (p_data[0] as UIScreen != view.screen)
				{
					break;
				}
				if (m_lockRefresh)
				{
					m_lockRefresh = false;
					break;
				}
				if (mapsList == null)
				{
					mapsList = new List<DRLCommunityMapData>();
				}
				view.InitializeSteppers();
				view.listFade.FadeOut(0.001f);
				ResetAllFilters();
				GameFlag type = base.app.arguments.game.type;
				bool flag3 = type == GameFlag.MapEditor;
				switch (type)
				{
				case GameFlag.Race:
					isRaceAllowed = 1;
					break;
				case GameFlag.Freestyle:
					isRaceAllowed = -1;
					break;
				default:
					isRaceAllowed = -1;
					break;
				}
				view.SetRightNavigationEnabled(flag3);
				if (!string.IsNullOrEmpty(view.initMapGUID))
				{
					SetMapFilterByGUID(view.initMapGUID);
				}
				UICommunityMapsShowCriteria show_criteria = ((flag3 || DRLApp.offline) ? UICommunityMapsShowCriteria.MyMaps : UICommunityMapsShowCriteria.CommunityMaps);
				if (!flag3 && !DRLApp.offline)
				{
					view.sortStepper.index = 1;
					view.sortStepper.Refresh();
				}
				view.SetMenuActive(!DRLApp.offline);
				base.app.model.service.platform.RefreshFlags(delegate
				{
					Show(show_criteria);
				});
				break;
			}
			case "community-maps.new-map-race@click":
			case "community-maps.new-map-collectable@click":
			case "community-maps.new-map@click":
			{
				UIMapEditorTemplatesView uIMapEditorTemplatesView = base.app.view.ui.screens.Open<UIMapEditorTemplatesView>("map-editor-templates-screen");
				uIMapEditorTemplatesView.screen.title = base.app.model.storage.locale.Get("map-editor.choose-base-map", "Choose base map");
				switch (p_event)
				{
				case "community-maps.new-map-race@click":
					uIMapEditorTemplatesView.gameMode = GameFlag.Race;
					break;
				case "community-maps.new-map-collectable@click":
					uIMapEditorTemplatesView.gameMode = GameFlag.Collectable;
					break;
				}
				break;
			}
			case "community-maps.item.clone@click":
			case "community-maps.item.add@click":
			{
				if (m_lockUI)
				{
					break;
				}
				m_lockUI = true;
				view.SetFeedback(UICommunityMapsFeedbackType.Loading);
				Component component3 = p_target as Component;
				if (!component3)
				{
					break;
				}
				UICommunityMapsItemView it2 = Hierarchy.FindReverse<UICommunityMapsItemView>(component3.transform);
				ServiceModel service = base.app.model.service;
				bool flag2 = DRLApp.offline;
				if (base.app.model.storage.state.player.profile.isDeveloper)
				{
					flag2 = Input.GetKey(KeyCode.D) || flag2;
				}
				if (!flag2)
				{
					service.CloneCommunityMap(it2.data.guid, delegate(DRLCommunityMapData p_result)
					{
						m_lockUI = false;
						if (p_result == null)
						{
							Debug.LogWarning("UICommunityMapsController> CommunityAddItemClick / Failed to Clone Map\n" + it2.data.ToJson());
						}
						RefreshList(0.1f);
					});
					break;
				}
				MapData mapData2 = new MapData();
				mapData2.Load(it2.data.ToJson());
				mapData2.guid = MapData.GenerateGUID();
				mapData2.mapTitle += " (Copy)";
				mapData2.isPublic = false;
				mapData2.allowCopy = false;
				mapData2.mapThumbURL = "";
				base.app.model.storage.maps.SaveCommunityMap(mapData2, delegate
				{
					m_lockUI = false;
					RefreshList(0.1f);
				}, p_is_map_editor: true);
				break;
			}
			case "community-maps.item.delete@click":
			{
				if (m_lockUI)
				{
					break;
				}
				m_lockUI = true;
				Component component = p_target as Component;
				if (!component)
				{
					break;
				}
				UICommunityMapsItemView it = Hierarchy.FindReverse<UICommunityMapsItemView>(component.transform);
				if (!it.confirmDelete)
				{
					if (m_deleteButtonCoolDown != null)
					{
						m_lockUI = false;
						break;
					}
					it.ShowConfirmDelete();
					m_deleteButtonCoolDown = RunOnce(delegate
					{
						if (it != null)
						{
							it.ShowDeleteButton();
						}
						m_deleteButtonCoolDown = null;
					}, 3f);
					m_lockUI = false;
					break;
				}
				if (m_deleteButtonCoolDown != null)
				{
					m_deleteButtonCoolDown.Stop();
					m_deleteButtonCoolDown = null;
				}
				view.SetFeedback(UICommunityMapsFeedbackType.Loading);
				bool flag = DRLApp.offline;
				bool p_is_map_editor = base.app.arguments.game.type == GameFlag.MapEditor;
				if (base.app.model.storage.state.player.profile.isDeveloper)
				{
					flag = Input.GetKey(KeyCode.D) || DRLApp.offline;
				}
				if (!flag)
				{
					base.app.model.service.RemoveCommunityMaps(it.data.guid, delegate
					{
						m_lockUI = false;
						RefreshList(0.1f);
					});
				}
				else
				{
					base.app.model.storage.maps.DeleteLocalCommunityMap(it.data.guid, p_is_map_editor, delegate
					{
						m_lockUI = false;
						RefreshList(0.1f);
					});
				}
				break;
			}
			case "community-maps.item.edit@click":
				if (!m_lockUI)
				{
					view.SetFeedback(UICommunityMapsFeedbackType.Loading);
					Component component2 = p_target as Component;
					if ((bool)component2)
					{
						UICommunityMapsItemView uICommunityMapsItemView2 = Hierarchy.FindReverse<UICommunityMapsItemView>(component2.transform);
						MapData mapData = new MapData();
						mapData.Load(uICommunityMapsItemView2.data.ToJson());
						currentMap = mapData;
						base.app.view.audio.SceneMainToGame(1.6f);
						base.app.scene.LoadCommunityMap(currentMap.guid);
					}
				}
				break;
			case "community-maps.data@click":
			case "community-maps.item.fly@click":
				if (!m_lockUI && base.app.arguments.game.type != GameFlag.MapEditor)
				{
					Component component5 = p_target as Component;
					if ((bool)component5)
					{
						UICommunityMapsItemView uICommunityMapsItemView4 = Hierarchy.FindReverse<UICommunityMapsItemView>(component5.transform);
						MapData mapData3 = new MapData();
						mapData3.Load(uICommunityMapsItemView4.data.ToJson());
						currentMap = mapData3;
						Notify("maps.community-map-selection-complete", "", "", currentMap.guid, true, "", "", currentMap);
						m_lockRefresh = true;
					}
				}
				break;
			case "community-maps.form.event@click":
				OnFormNotification(p_target, p_is_change: false, p_event);
				break;
			case "community-maps.form.event@change":
				OnFormNotification(p_target, p_is_change: true, p_event);
				break;
			case "community-maps.form.event@end-edit":
				OnFormNotification(p_target, p_is_change: false, p_event);
				break;
			case "community-maps.form.event@submit":
				OnFormNotification(p_target, p_is_change: false, p_event);
				break;
			case "community-maps.page@select":
				if (!m_lockUI)
				{
					int p_page = (m_currentPage = (int)p_data[0]);
					Debug.Log("UICommunityMapsController> Page Select [" + p_page + "]");
					UpdatePage(isRaceAllowed, mapDifficulty, mapId, p_page, pageLength, sortingCriteria, searchQuery);
				}
				break;
			case "community-maps.page-next@click":
				if (!m_lockUI && view.pageField.index + 1 != view.pageField.listField.Count)
				{
					view.pageField.index = view.pageField.index + 1;
					m_currentPage = view.pageField.index;
					UpdatePage(isRaceAllowed, mapDifficulty, mapId, view.pageField.index, pageLength, sortingCriteria, searchQuery);
				}
				break;
			case "community-maps.page-previous@click":
				if (!m_lockUI && view.pageField.index != 0)
				{
					view.pageField.index = view.pageField.index - 1;
					m_currentPage = view.pageField.index;
					UpdatePage(isRaceAllowed, mapDifficulty, mapId, view.pageField.index, pageLength, sortingCriteria, searchQuery);
				}
				break;
			case "community-maps.exit@click":
				base.app.controller.game.Exit();
				break;
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				if (view.allowExit)
				{
					base.app.view.ui.screens.controller.BlockDark();
				}
				break;
			case "community-maps.data@focus":
			case "community-maps.data@unfocus":
			{
				if (m_lockUI)
				{
					break;
				}
				Component component4 = p_target as Component;
				if ((bool)component4)
				{
					UICommunityMapsItemView uICommunityMapsItemView3 = Hierarchy.FindReverse<UICommunityMapsItemView>(component4.transform);
					if ((bool)uICommunityMapsItemView3)
					{
						uICommunityMapsItemView3.mapPhotoAnimation.animationType = ((p_event == "community-maps.data@focus") ? AnimateImageLayout.AnimationType.OscilateVertical : AnimateImageLayout.AnimationType.None);
					}
				}
				break;
			}
			case "maps.track-selection.favorite@change":
			{
				DRLToggleView dRLToggleView = p_target as DRLToggleView;
				if (dRLToggleView == null)
				{
					Debug.Log("UICommunityMapsController> OnNotification / Can't cast p_target o Transform " + p_target.name);
					break;
				}
				UICommunityMapsItemView uICommunityMapsItemView = Hierarchy.FindReverse<UICommunityMapsItemView>(dRLToggleView.transform);
				if (uICommunityMapsItemView == null)
				{
					break;
				}
				string text = uICommunityMapsItemView.data.mapId;
				string track_id = uICommunityMapsItemView.data.guid;
				bool customMap = true;
				DRLMapFavoriteData new_map_data = new DRLMapFavoriteData();
				new_map_data.mapId = text;
				new_map_data.trackId = track_id;
				new_map_data.customMap = customMap;
				List<DRLMapFavoriteData> favoriteMaps = base.app.model.storage.state.player.favoriteMaps;
				DRLMapFavoriteData dRLMapFavoriteData = favoriteMaps.Find((DRLMapFavoriteData map) => map.mapId == new_map_data.mapId && map.trackId == new_map_data.trackId && map.customMap == new_map_data.customMap);
				List<DRLMapFavoriteData> list = favoriteMaps.FindAll((DRLMapFavoriteData map) => map.mapId == new_map_data.mapId && map.trackId == new_map_data.trackId && map.customMap == new_map_data.customMap);
				Debug.Log($"UICommunityMapsController> OnNotification / Find #{list.Count} occurrences of favorite map");
				if (dRLToggleView.toggle.isOn)
				{
					if (dRLMapFavoriteData != null)
					{
						break;
					}
					favoriteMaps.Add(new_map_data);
					Debug.Log("UICommunityMapsController> OnNotification / Adding " + new_map_data.mapId + " " + new_map_data.trackId);
					if (!DRLApp.offline)
					{
						view.SetFeedback(UICommunityMapsFeedbackType.MapSave);
						m_lockUI = true;
						m_lockRefresh = true;
						DateTime t0 = DateTime.UtcNow;
						base.app.model.service.GetCommunityMap(track_id, delegate(DRLCommunityMapResult p_result)
						{
							Debug.Log("UICommunityMapsController> Finished downloading community map - " + (DateTime.UtcNow - t0).TotalSeconds);
							DRLCommunityMapData d = ((p_result.data.Length == 0) ? null : p_result.data[0]);
							if (d == null || !base.validContext)
							{
								Debug.LogWarning("UICommunityMapsController> Store favorite community map / Failed to Load DRLCommunityMapData - guid[" + track_id + "]");
								m_lockRefresh = false;
								m_lockUI = false;
								view.SetFeedback(UICommunityMapsFeedbackType.None);
							}
							else
							{
								new Thread((ThreadStart)delegate
								{
									MapData md = d.Convert<MapData>();
									if (md != null)
									{
										md.LoadRoot(d.root);
									}
									this.TimerRunOnce(delegate
									{
										if (md == null)
										{
											Debug.LogWarning("UICommunityMapsController> Store favorite community map / Failed to Parse MapData - guid[" + track_id + "]");
											m_lockRefresh = false;
											m_lockUI = false;
											view.SetFeedback(UICommunityMapsFeedbackType.None);
										}
										else
										{
											base.app.model.storage.maps.SaveCommunityMap(md, delegate
											{
												m_lockRefresh = false;
												m_lockUI = false;
												view.SetFeedback(UICommunityMapsFeedbackType.None);
												Debug.Log("UICommunityMapsController> Succesfully stored community map - guid[" + track_id + "]");
											});
										}
									}, 1f / 60f);
								}).Start();
							}
						});
					}
				}
				else if (dRLMapFavoriteData != null)
				{
					favoriteMaps.Remove(dRLMapFavoriteData);
					Debug.Log("UICommunityMapsController> OnNotification / Removing " + dRLMapFavoriteData.mapId + " " + dRLMapFavoriteData.trackId);
					base.app.model.storage.maps.DeleteLocalCommunityMap(track_id);
				}
				base.app.model.storage.state.player.favoriteMaps = favoriteMaps;
				break;
			}
			}
		}

		private void Show(UICommunityMapsShowCriteria p_show_criteria)
		{
			view.Clear();
			view.showCriteria = p_show_criteria;
			m_showing = true;
			RefreshList(0.2f);
			UINavigation.Focus(view.sortStepperNav);
		}

		public void Hide()
		{
			m_showing = false;
			FadeComponent component = base.gameObject.GetComponent<FadeComponent>();
			if ((bool)component)
			{
				component.FadeOut();
			}
		}

		private void RefreshList(float p_delay, int p_page = 0)
		{
			if (!m_lockRefresh)
			{
				if (m_refreshTimer != null)
				{
					m_refreshTimer.Stop();
				}
				m_refreshTimer = RunOnce(delegate
				{
					UpdatePage(isRaceAllowed, mapDifficulty, mapId, p_page, pageLength, sortingCriteria, searchQuery);
				}, p_delay);
			}
		}

		public async void UpdatePage(int p_is_race_allowed, int p_map_difficulty, string p_map_id, int p_page, int p_total, SortType p_sort, string p_search, float p_user_maps_refresh_delay = 0.5f)
		{
			if (!base.validContext)
			{
				return;
			}
			PlatformService ps = base.app.model.service.platform;
			if (ps.ContainsFlag(PlatformServiceFlagType.XBoxUGCBlocked))
			{
				view.SetFeedback(UICommunityMapsFeedbackType.UGCBlock);
				Activity.RunOnce(delegate
				{
					ps.CheckPlatformUGCPrivilege(delegate
					{
						if (ps.ContainsFlag(PlatformServiceFlagType.XBoxUGCBlocked))
						{
							base.app.view.ui.screens.Return();
						}
						else
						{
							UpdatePage(p_is_race_allowed, p_map_difficulty, p_map_id, p_page, p_total, p_sort, p_search, p_user_maps_refresh_delay);
						}
					});
				}, 2f);
				return;
			}
			CancelWebLoad();
			view.Clear();
			if (p_user_maps_refresh_delay > 0.01f)
			{
				view.SetFeedback(UICommunityMapsFeedbackType.Loading);
			}
			string p_player_id = ((view.showCriteria == UICommunityMapsShowCriteria.CommunityMaps) ? "" : base.app.model.service.backend.playerId);
			GameFlag p_category = ((view.showCategory == GameFlag.Collectable) ? GameFlag.Collectable : ((view.showCriteria != UICommunityMapsShowCriteria.MyMaps) ? view.showCategory : GameFlag.None));
			if (view.caller != null)
			{
				if (view.caller.name == "home-screen-grid")
				{
					p_category = GameFlag.All;
				}
			}
			else
			{
				p_category = GameFlag.All;
			}
			base.app.view.audio.PlayUILoadingLoop();
			bool flag = DRLApp.offline;
			if (base.app.model.storage.state.player.profile.isDeveloper)
			{
				flag = Input.GetKey(KeyCode.D) || flag;
			}
			if (!flag)
			{
				ServiceModel service = base.app.model.service;
				m_webLoader = service.GetCommunityMaps(p_player_id, p_map_difficulty, p_is_race_allowed, p_map_id, p_page, p_total, p_category, p_sort, p_search, delegate(DRLCommunityMapResult p_result)
				{
					ApplyPageData(p_page, p_total, p_result);
				});
				return;
			}
			base.app.model.storage.maps.GetLocalMapEditorMaps(p_is_race_allowed, p_page, p_total, delegate(DRLCommunityMapResult p_result)
			{
				this.TimerRunOnce(delegate
				{
					if (base.validContext)
					{
						ApplyPageData(p_page, p_total, p_result);
					}
				}, 1f / 30f);
			});
		}

		private void ApplyPageData(int p_page, int p_total, DRLCommunityMapResult p_result)
		{
			bool flag = (bool)this && (bool)base.app && (bool)base.app.view && (bool)view && (bool)base.gameObject;
			if (!flag)
			{
				return;
			}
			if (flag && (bool)base.app.view.audio)
			{
				base.app.view.audio.StopUILoadingLoop();
			}
			if (m_webLoader != null && (m_webLoader.status == AsyncRequestStatus.Created || m_webLoader.status == AsyncRequestStatus.Cancelled))
			{
				return;
			}
			if (p_result == null)
			{
				if (!DRLApp.offline)
				{
					if ((bool)base.app.view && (bool)base.app.view.audio)
					{
						base.app.view.audio.PlayUIGenericError();
					}
					view.SetFeedback(UICommunityMapsFeedbackType.OperationFailure);
					Debug.LogWarning("UICommunityMapsController> UpdatePage - Failed!");
				}
				else
				{
					UICommunityMapsFeedbackType feedback = ((base.app.arguments.game.type == GameFlag.MapEditor) ? UICommunityMapsFeedbackType.NoMaps : UICommunityMapsFeedbackType.Offline);
					view.SetFeedback(feedback);
					view.pageField.Set(0);
					view.ResetNavigation();
				}
			}
			else
			{
				if ((bool)base.app.view && (bool)base.app.view.audio)
				{
					base.app.view.audio.PlayUILoadingSuccess();
				}
				mapsList = new List<DRLCommunityMapData>(p_result.data);
				m_pagesTotalCount = p_result.pagging.pageTotal;
				if (mapsList.Count > 0)
				{
					view.UpdateList(mapsList, p_page, p_total, p_result.pagging.pageTotal);
					return;
				}
				view.SetFeedback(UICommunityMapsFeedbackType.NoMaps);
				view.pageField.Set(0);
				view.ResetNavigation();
			}
		}

		private void CancelWebLoad()
		{
			if (m_webLoader != null)
			{
				m_webLoader.Cancel();
			}
		}

		private void OnFormNotification(UnityEngine.Object p_target, bool p_is_change, string p_event)
		{
			if (m_lockUI)
			{
				return;
			}
			bool flag = p_is_change;
			p_event.Contains("@end-edit");
			bool flag2 = p_event.Contains("@submit");
			switch (p_target.name)
			{
			case "map-sort-stepper":
				if (flag)
				{
					DRLStepperView dRLStepperView = p_target as DRLStepperView;
					UICommunityMapsShowCriteria[] array = new UICommunityMapsShowCriteria[7]
					{
						UICommunityMapsShowCriteria.MyMaps,
						UICommunityMapsShowCriteria.CommunityMaps,
						UICommunityMapsShowCriteria.CommunityMaps,
						UICommunityMapsShowCriteria.CommunityMaps,
						UICommunityMapsShowCriteria.CommunityMaps,
						UICommunityMapsShowCriteria.CommunityMaps,
						UICommunityMapsShowCriteria.CommunityMaps
					};
					SortType[] array2 = new SortType[7]
					{
						SortType.None,
						SortType.ScoreDesc,
						SortType.RatingCountDesc,
						SortType.DateAsc,
						SortType.DateDesc,
						SortType.DateUpdateDesc,
						SortType.Featured
					};
					view.showCriteria = array[dRLStepperView.index];
					sortingCriteria = array2[dRLStepperView.index];
					RefreshList(0.6f);
				}
				break;
			case "map-difficulty-stepper":
				if (flag)
				{
					DRLStepperView dRLStepperView3 = p_target as DRLStepperView;
					mapDifficulty = dRLStepperView3.index - 1;
					RefreshList(0.6f);
				}
				break;
			case "map-base-map-stepper":
				if (flag)
				{
					DRLStepperView dRLStepperView2 = p_target as DRLStepperView;
					_ = dRLStepperView2.index;
					mapId = ((dRLStepperView2.index <= 0) ? "" : view.baseMapList[dRLStepperView2.index - 1].guid);
					Debug.Log("UICommunityMapsController> Filter / Base Map / index[" + dRLStepperView2.index + "] name[" + dRLStepperView2.labels[dRLStepperView2.index] + "] guid[" + mapId + "]");
					RefreshList(0.6f);
				}
				break;
			case "map-search-input":
			{
				if (!flag2)
				{
					break;
				}
				DRLInputFieldView dRLInputFieldView = p_target as DRLInputFieldView;
				if (!(dRLInputFieldView.field.text == m_previousSearchQuery))
				{
					searchQuery = dRLInputFieldView.field.text;
					m_previousSearchQuery = searchQuery;
					if (m_searchTimer != null)
					{
						m_searchTimer.Stop();
					}
					m_searchTimer = RunOnce(delegate
					{
						UpdatePage(isRaceAllowed, mapDifficulty, mapId, 0, pageLength, sortingCriteria, searchQuery);
					}, 1f / 60f);
				}
				break;
			}
			}
		}

		private void ResetAllFilters()
		{
			searchQuery = "";
			mapId = "";
			mapDifficulty = -1;
			sortingCriteria = SortType.ScoreDesc;
		}

		private void SetMapFilterByGUID(string p_guid)
		{
			for (int i = 0; i < view.baseMapList.Count; i++)
			{
				if (!(view.baseMapList[i].guid != p_guid))
				{
					mapId = p_guid;
					view.baseMapStepper.index = i + 1;
					view.baseMapStepper.Refresh();
				}
			}
		}
	}
}

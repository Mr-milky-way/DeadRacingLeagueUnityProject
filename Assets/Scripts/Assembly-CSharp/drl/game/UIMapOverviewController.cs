using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIMapOverviewController : Controller<DRLApp>
	{
		private bool m_lockUI;

		private MonoActivity m_refreshTimer;

		public UIMapOverviewView view => AssertLocal<UIMapOverviewView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
			{
				UIScreen uIScreen = p_data[0] as UIScreen;
				if (!(uIScreen != view.screen))
				{
					LoadCards(uIScreen);
					UINavigationScroll component = GetComponent<UINavigationScroll>();
					if ((bool)component)
					{
						component.forceScrollX = true;
					}
				}
				break;
			}
			case "fly.map-track-card@click":
			{
				UICardButtonMapTrack uICardButtonMapTrack2 = p_target as UICardButtonMapTrack;
				if ((bool)uICardButtonMapTrack2)
				{
					string text4 = (uICardButtonMapTrack2.data ? uICardButtonMapTrack2.data.map.guid : uICardButtonMapTrack2.customData.mapId);
					string text5 = (uICardButtonMapTrack2.data ? uICardButtonMapTrack2.data.guid : "");
					string text6 = ((uICardButtonMapTrack2.customData == null) ? "" : uICardButtonMapTrack2.customData.guid);
					bool flag = !string.IsNullOrEmpty(text6);
					DRLMap dRLMap = (uICardButtonMapTrack2.data ? uICardButtonMapTrack2.data.map : base.app.model.storage.library.FindByGUID<DRLMap>(text4));
					DRLMapTrack data = uICardButtonMapTrack2.data;
					MapData customData = uICardButtonMapTrack2.customData;
					Notify("maps.track-selection-complete", text4, text5, text6, flag, dRLMap, data, customData);
				}
				break;
			}
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			case "maps.track-selection.favorite@change":
			{
				if (m_lockUI)
				{
					Debug.Log("UIMapOverviewController> OnNotification / UI is temporally being ignored");
					break;
				}
				DRLToggleView dRLToggleView = p_target as DRLToggleView;
				if (dRLToggleView == null)
				{
					Debug.LogWarning("UIMapOverviewController> OnNotification / Can't cast p_target to Transform " + p_target.name);
					break;
				}
				UICardButtonMapTrack uICardButtonMapTrack = Hierarchy.FindReverse<UICardButtonMapTrack>(dRLToggleView.transform);
				if (uICardButtonMapTrack == null)
				{
					break;
				}
				string text = (uICardButtonMapTrack.data ? uICardButtonMapTrack.data.map.guid : uICardButtonMapTrack.customData.mapId);
				string text2 = (uICardButtonMapTrack.data ? uICardButtonMapTrack.data.guid : "");
				string text3 = ((uICardButtonMapTrack.customData == null) ? "" : uICardButtonMapTrack.customData.guid);
				(uICardButtonMapTrack.data ? uICardButtonMapTrack.data.map : base.app.model.storage.library.FindByGUID<DRLMap>(text)).data = null;
				_ = uICardButtonMapTrack.data;
				_ = uICardButtonMapTrack.customData;
				DRLMapFavoriteData new_map_data = new DRLMapFavoriteData();
				new_map_data.mapId = text;
				new_map_data.customMap = !string.IsNullOrEmpty(text3);
				new_map_data.trackId = (new_map_data.customMap ? text3 : text2);
				List<DRLMapFavoriteData> favoriteMaps = base.app.model.storage.state.player.favoriteMaps;
				DRLMapFavoriteData dRLMapFavoriteData = favoriteMaps.Find((DRLMapFavoriteData map) => map.trackId == new_map_data.trackId && map.mapId == new_map_data.mapId && map.customMap == new_map_data.customMap);
				if (dRLToggleView.toggle.isOn)
				{
					if (dRLMapFavoriteData != null)
					{
						break;
					}
					favoriteMaps.Add(new_map_data);
				}
				else if (dRLMapFavoriteData != null)
				{
					favoriteMaps.Remove(dRLMapFavoriteData);
				}
				base.app.model.storage.state.player.favoriteMaps = favoriteMaps;
				if (view.category == GameFlag.MapFavorite)
				{
					ReloadViewAfterDelay(1f);
				}
				break;
			}
			}
		}

		private void FetchMap(string map_guid, string track_guid, string custom_map_guid, bool is_custom_map, DRLMap map_data, DRLMapTrack track_data, MapData custom_data, int retries)
		{
			int maxRetries = 2;
			string p_guid = (is_custom_map ? custom_map_guid : track_guid);
			base.app.model.service.GetCommunityMaps(p_guid, delegate(DRLCommunityMapResult p_result)
			{
				DRLCommunityMapData dRLCommunityMapData = ((p_result.data.Length == 0) ? null : p_result.data[0]);
				if ((!base.validContext || !p_result.success || dRLCommunityMapData == null) && base.app.online && is_custom_map)
				{
					if (retries == maxRetries)
					{
						Debug.LogWarning("UIMapOverviewController> OnNotification / Failed to Load DRLCommunityMapData - guid[" + track_guid + "]");
						base.app.view.ui.dialog.Open(DialogType.Warning, "FAILED TO LOAD COMMUNITY MAP DATA", "MAP COULDN'T BE DOWNLOADED AT THIS TIME. LOADING MOST RECENT LOCAL MAP", new string[1] { "OK" }, null, null, delegate
						{
							Notify("maps.track-selection-complete", map_guid, track_guid, custom_map_guid, is_custom_map, map_data, track_data, custom_data);
						});
					}
					else
					{
						FetchMap(map_guid, track_guid, custom_map_guid, is_custom_map, map_data, track_data, custom_data, retries + 1);
					}
				}
				else
				{
					MapData mapData = null;
					if (dRLCommunityMapData != null)
					{
						mapData = dRLCommunityMapData.Convert<MapData>();
					}
					mapData = ((mapData == null) ? custom_data : mapData);
					Notify("maps.track-selection-complete", map_guid, track_guid, custom_map_guid, is_custom_map, map_data, track_data, mapData);
				}
			});
		}

		private void LoadCards(UIScreen p_screen)
		{
			m_lockUI = false;
			view.HideLoadingUI(0f, p_without_animating: true);
			DRLMap d = view.data;
			GameFlag type = base.app.arguments.game.type;
			GameFlag category = view.category;
			bool is_featured = view.isFeatured;
			bool is_dev = base.app.model.storage.state.player.profile.isDeveloper;
			List<object> content_list = new List<object>();
			if ((bool)d)
			{
				content_list.AddRange(base.app.model.storage.GetMapTracks(d, type, p_filter_build: true));
			}
			content_list.AddRange(base.app.model.storage.maps.Find(type == GameFlag.Race, category));
			if (category == GameFlag.MapDRL)
			{
				content_list.AddRange(base.app.model.storage.maps.Find(type == GameFlag.Race, GameFlag.MapDRLSimCup));
				content_list.AddRange(base.app.model.storage.maps.Find(type == GameFlag.Race, GameFlag.MapFeatured));
			}
			content_list.RemoveAll((object it) => it is MapData mapData && is_featured && mapData.mapCategoryFlag != GameFlag.MapFeatured);
			if (type == GameFlag.Freestyle)
			{
				content_list.RemoveAll((object it) => (it is MapData mapData && mapData.mode.typeFlag == GameFlag.Collectable) ? true : false);
			}
			bool flag = category != GameFlag.MapMultiGP && category != GameFlag.MapSimple && category != GameFlag.MapDRLSimCup && category != GameFlag.MapVirtualSeason && category != GameFlag.MapFavorite;
			if (is_featured && (category == GameFlag.MapFeatured || category == GameFlag.MapSimple))
			{
				flag = false;
			}
			if (flag)
			{
				content_list.RemoveAll((object it) => it is MapData mapData && mapData.mapId != d.guid);
			}
			content_list.RemoveAll(delegate(object it)
			{
				if (it is MapData)
				{
					return false;
				}
				DRLMapTrack dRLMapTrack = it as DRLMapTrack;
				if (!dRLMapTrack.tags)
				{
					return false;
				}
				if (dRLMapTrack.tags.Contains(GameFlag.MapEditorOnly))
				{
					return true;
				}
				return (!is_dev && dRLMapTrack.tags.Contains(GameFlag.MapEditorDevOnly)) ? true : false;
			});
			if (category == GameFlag.MapFavorite)
			{
				List<DRLMapFavoriteData> favorite_maps = base.app.model.storage.state.player.favoriteMaps;
				if (favorite_maps.Count == 0)
				{
					base.app.view.ui.screens.Return();
				}
				view.ShowLoadingUI();
				view.Clear();
				bool p_race_only = type == GameFlag.Race;
				List<MapData> collection = base.app.model.storage.maps.Find(p_race_only);
				List<DRLMapTrack> tracks = base.app.model.storage.GetMapTracks();
				List<DRLMapFavoriteData> maps_left;
				if (DRLApp.offline)
				{
					base.app.model.storage.maps.GetLocalMaps(delegate(List<MapData> maps)
					{
						content_list.AddRange(maps);
						content_list.AddRange(tracks);
						maps_left = RemoveAllNonFavoriteMapsFromList(favorite_maps, content_list);
						view.Set(d, content_list);
						view.SetRatingsAvailable(p_available: false);
						SetupNavigationScrolling(p_screen);
						view.HideLoadingUI();
						m_lockUI = false;
						_ = maps_left.Count;
						_ = 0;
					});
				}
				else
				{
					content_list.AddRange(collection);
					content_list.AddRange(tracks);
					maps_left = RemoveAllNonFavoriteMapsFromList(favorite_maps, content_list);
					FetchCommunityMapsAndSetup(p_screen, maps_left, content_list, d);
					if (maps_left.Count > 0)
					{
						return;
					}
				}
			}
			if (!DRLApp.offline || category != GameFlag.MapFavorite)
			{
				content_list.Sort(SortTracks);
				view.Set(d, content_list);
				view.SetRatingsAvailable(p_available: false);
				SetupNavigationScrolling(p_screen);
				view.HideLoadingUI(0f);
				m_lockUI = false;
			}
		}

		private int SortTracks(object p_trackA, object p_trackB)
		{
			DRLMapTrack dRLMapTrack = null;
			DRLMapTrack dRLMapTrack2 = null;
			MapData mapData = null;
			MapData mapData2 = null;
			if (p_trackA is DRLMapTrack)
			{
				dRLMapTrack = p_trackA as DRLMapTrack;
			}
			else
			{
				mapData = p_trackA as MapData;
			}
			if (p_trackB is DRLMapTrack)
			{
				dRLMapTrack2 = p_trackB as DRLMapTrack;
			}
			else
			{
				mapData2 = p_trackB as MapData;
			}
			if ((dRLMapTrack == null && mapData == null) || (dRLMapTrack2 == null && mapData2 == null))
			{
				return 0;
			}
			int num = ((dRLMapTrack == null) ? mapData.order : dRLMapTrack.order);
			int value = ((dRLMapTrack2 == null) ? mapData2.order : dRLMapTrack2.order);
			return num.CompareTo(value);
		}

		private void FetchCommunityMapsAndSetup(UIScreen p_screen, List<DRLMapFavoriteData> p_maps_left, List<object> p_content_list, DRLMap d)
		{
			for (int i = 0; i < p_maps_left.Count; i++)
			{
				DRLMapFavoriteData maps = p_maps_left[i];
				int _i = i;
				base.app.model.service.GetCommunityMaps(maps.trackId, delegate(DRLCommunityMapResult p_result)
				{
					DRLCommunityMapData dRLCommunityMapData = ((p_result.data.Length == 0) ? null : p_result.data[0]);
					if (dRLCommunityMapData == null)
					{
						Debug.LogWarning("UIMapOverviewController> OnNotification / Failed to Load DRLCommunityMapData - guid[" + maps.trackId + "]");
						view.HideLoadingUI();
						m_lockUI = false;
					}
					else
					{
						MapData mapData = dRLCommunityMapData.Convert<MapData>();
						if (mapData.mode.typeFlag != GameFlag.Collectable)
						{
							p_content_list.Add(mapData);
						}
						view.Set(d, p_content_list);
						view.SetRatingsAvailable(p_available: false);
						SetupNavigationScrolling(p_screen);
						if (_i == p_maps_left.Count - 1)
						{
							view.HideLoadingUI();
							m_lockUI = false;
						}
					}
				});
			}
		}

		private void SetupNavigationScrolling(UIScreen p_screen)
		{
			UINavigationScroll component = p_screen.GetComponent<UINavigationScroll>();
			if ((bool)component)
			{
				component.ResetScroll(p_force: true);
			}
			if (view.category != GameFlag.MapMultiGP)
			{
				UINavigation.Link(view.listField.GetComponent<LayoutGroup>(), view.leftNavigation);
			}
		}

		private List<DRLMapFavoriteData> RemoveAllNonFavoriteMapsFromList(List<DRLMapFavoriteData> p_favorite_maps, List<object> p_content_list)
		{
			List<DRLMapFavoriteData> maps_left = new List<DRLMapFavoriteData>(p_favorite_maps);
			bool race_only = base.app.arguments.game.type == GameFlag.Race;
			p_content_list.RemoveAll(delegate(object it)
			{
				MapData mapData = it as MapData;
				if (mapData != null)
				{
					if (race_only && !mapData.mode.race.allowed)
					{
						maps_left.RemoveAll((DRLMapFavoriteData map) => map.mapId == mapData.mapId && map.trackId == mapData.guid);
						return true;
					}
					if (race_only && mapData.mode.typeFlag != GameFlag.Race)
					{
						maps_left.RemoveAll((DRLMapFavoriteData map) => map.mapId == mapData.mapId && map.trackId == mapData.guid);
						return true;
					}
					if (mapData.mode.typeFlag == GameFlag.Collectable)
					{
						maps_left.RemoveAll((DRLMapFavoriteData map) => map.mapId == mapData.mapId && map.trackId == mapData.guid);
						return true;
					}
					if (!p_favorite_maps.Any((DRLMapFavoriteData map) => map.mapId == mapData.mapId && map.trackId == mapData.guid))
					{
						return true;
					}
					maps_left.RemoveAll((DRLMapFavoriteData map) => map.mapId == mapData.mapId && map.trackId == mapData.guid);
					return false;
				}
				DRLMapTrack dRLMapTrack = it as DRLMapTrack;
				if ((object)dRLMapTrack != null)
				{
					if (!p_favorite_maps.Any((DRLMapFavoriteData map) => map.mapId == dRLMapTrack.map.guid && map.trackId == dRLMapTrack.guid))
					{
						return true;
					}
					if (race_only && dRLMapTrack.id == "freefly")
					{
						maps_left.RemoveAll((DRLMapFavoriteData map) => map.mapId == dRLMapTrack.map.guid && map.trackId == dRLMapTrack.guid);
						return true;
					}
					maps_left.RemoveAll((DRLMapFavoriteData map) => map.mapId == dRLMapTrack.map.guid && map.trackId == dRLMapTrack.guid);
					return false;
				}
				return true;
			});
			return maps_left;
		}

		private void ReloadViewAfterDelay(float p_delay)
		{
			if (m_refreshTimer != null)
			{
				m_refreshTimer.Stop();
			}
			m_refreshTimer = RunOnce(delegate
			{
				UIScreen screen = view.screen;
				LoadCards(screen);
			}, p_delay);
		}
	}
}

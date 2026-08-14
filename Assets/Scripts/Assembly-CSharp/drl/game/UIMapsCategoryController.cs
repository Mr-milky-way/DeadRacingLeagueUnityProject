using System.Collections.Generic;
using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIMapsCategoryController : Controller<DRLApp>
	{
		public UIMapsCategoryView view => AssertLocal<UIMapsCategoryView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "maps.community-map-selection-complete":
			{
				UICommunityMapsController uICommunityMapsController = p_target as UICommunityMapsController;
				if (!(this == null) && !(uICommunityMapsController == null) && !(uICommunityMapsController.view == null) && !(uICommunityMapsController.view.caller != this))
				{
					Notify("maps.selection-complete", p_data);
				}
				break;
			}
			case "maps.track-selection-complete":
			{
				Debug.Log("UIMapsCategoryController> TrackSelectionComplete");
				UIMapOverviewController uIMapOverviewController = p_target as UIMapOverviewController;
				if ((!(this == null) && !(uIMapOverviewController == null) && !(uIMapOverviewController.view == null) && !(uIMapOverviewController.view.caller != this)) || !(view.caller.name != "settings-game-screen"))
				{
					if (base.app.inMultiplayer && view.screen.open)
					{
						view.screen.Hide(0f);
					}
					Notify("maps.selection-complete", p_data);
				}
				break;
			}
			}
			if (base.app.view.ui.screens.current != view.screen || p_event == null)
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
					view.depth = 1;
					if ((bool)view.virtualSeasonContainer)
					{
						bool active = base.app.model.storage.maps.Find(false, GameFlag.MapVirtualSeason).Count > 0;
						view.virtualSeasonContainer.gameObject.SetActive(active);
					}
					if (view.screen.title == "SOLO RACE")
					{
						bool favoriteMapsCardsEnabled = CheckForRaceFavorite();
						view.SetFavoriteMapsCardsEnabled(favoriteMapsCardsEnabled);
					}
					else
					{
						view.SetFavoriteMapsCardsEnabled(base.app.model.storage.state.player.favoriteMaps.Count != 0);
					}
					if (view.caller.name == "settings-game-screen")
					{
						view.collectableCards.SetActive(value: true);
					}
					else
					{
						view.collectableCards.SetActive(value: false);
					}
					view.SetCommunityMapsCardsEnabled(!DRLApp.offline);
					UINavigationScroll component = uIScreen.GetComponent<UINavigationScroll>();
					if ((bool)component)
					{
						component.ResetScroll(p_force: true);
					}
				}
				break;
			}
			case "fly.community-maps@click":
			{
				UICommunityMapsView uICommunityMapsView2 = base.app.view.ui.screens.Open<UICommunityMapsView>("community-maps-screen");
				uICommunityMapsView2.screen.title = base.app.model.storage.locale.Get("maps.community.title", "Community Maps");
				uICommunityMapsView2.allowExit = false;
				uICommunityMapsView2.InitFilter(p_isMultiGP: false);
				uICommunityMapsView2.caller = this;
				uICommunityMapsView2.showCategory = GameFlag.MapCommon;
				view.depth = 2;
				break;
			}
			case "fly.sd-community-maps@click":
			{
				UICommunityMapsView uICommunityMapsView = base.app.view.ui.screens.Open<UICommunityMapsView>("community-maps-screen");
				uICommunityMapsView.screen.title = base.app.model.storage.locale.Get("maps.community.title", "Community Maps");
				uICommunityMapsView.allowExit = false;
				uICommunityMapsView.InitFilter(p_isMultiGP: false);
				uICommunityMapsView.caller = this;
				uICommunityMapsView.showCategory = GameFlag.Collectable;
				view.depth = 2;
				break;
			}
			case "fly.simple-courses@click":
			{
				UIMapOverviewView uIMapOverviewView7 = base.app.view.ui.screens.Open<UIMapOverviewView>("map-overview-screen");
				uIMapOverviewView7.category = GameFlag.MapSimple;
				uIMapOverviewView7.isFeatured = false;
				uIMapOverviewView7.usaf = false;
				uIMapOverviewView7.screen.title = base.app.model.storage.locale.Get("maps.simple-courses.title", "Simple Courses");
				uIMapOverviewView7.data = base.app.model.storage.library.FindByGUID<DRLMap>("MP-d3f");
				uIMapOverviewView7.caller = this;
				view.depth = 2;
				break;
			}
			case "fly.multigp@click":
			{
				UIMapOverviewView uIMapOverviewView6 = base.app.view.ui.screens.Open<UIMapOverviewView>("map-overview-screen");
				uIMapOverviewView6.category = GameFlag.MapMultiGP;
				uIMapOverviewView6.isFeatured = false;
				uIMapOverviewView6.usaf = false;
				uIMapOverviewView6.screen.title = base.app.model.storage.locale.Get("maps.multigp.title", "MultiGP");
				uIMapOverviewView6.data = base.app.model.storage.library.FindByGUID<DRLMap>("MP-7ea");
				uIMapOverviewView6.caller = this;
				view.depth = 2;
				break;
			}
			case "fly.favorite-maps@click":
			{
				UIMapOverviewView uIMapOverviewView5 = base.app.view.ui.screens.Open<UIMapOverviewView>("map-overview-screen");
				uIMapOverviewView5.category = GameFlag.MapFavorite;
				uIMapOverviewView5.isFeatured = false;
				uIMapOverviewView5.usaf = false;
				uIMapOverviewView5.screen.title = base.app.model.storage.locale.Get("maps.favorite-maps.title", "Favorite Maps");
				uIMapOverviewView5.data = base.app.model.storage.library.FindByGUID<DRLMap>("MP-27a");
				uIMapOverviewView5.caller = this;
				view.depth = 2;
				break;
			}
			case "fly.featured-tracks@click":
			{
				UIMapOverviewView uIMapOverviewView4 = base.app.view.ui.screens.Open<UIMapOverviewView>("map-overview-screen");
				uIMapOverviewView4.category = GameFlag.MapFeatured;
				uIMapOverviewView4.isFeatured = true;
				uIMapOverviewView4.usaf = false;
				uIMapOverviewView4.screen.title = base.app.model.storage.locale.Get("maps.featured-tracks.title", "Featured Tracks");
				uIMapOverviewView4.data = base.app.model.storage.library.FindByGUID<DRLMap>("MP-94f");
				uIMapOverviewView4.caller = this;
				view.depth = 2;
				break;
			}
			case "fly.sim-cup@click":
			{
				UIMapOverviewView uIMapOverviewView3 = base.app.view.ui.screens.Open<UIMapOverviewView>("map-overview-screen");
				uIMapOverviewView3.category = GameFlag.MapDRLSimCup;
				uIMapOverviewView3.isFeatured = false;
				uIMapOverviewView3.usaf = false;
				uIMapOverviewView3.screen.title = base.app.model.storage.locale.Get("maps.sim-cup.title", "Sim Cup");
				uIMapOverviewView3.data = base.app.model.storage.library.FindByGUID<DRLMap>("MP-229");
				uIMapOverviewView3.caller = this;
				view.depth = 2;
				break;
			}
			case "fly.virtual-season@click":
			{
				UIMapOverviewView uIMapOverviewView2 = base.app.view.ui.screens.Open<UIMapOverviewView>("map-overview-screen");
				uIMapOverviewView2.category = GameFlag.MapVirtualSeason;
				uIMapOverviewView2.isFeatured = false;
				uIMapOverviewView2.usaf = false;
				uIMapOverviewView2.screen.title = base.app.model.storage.locale.Get("maps.virtual-season.title", "Virtual Season");
				uIMapOverviewView2.data = base.app.model.storage.library.FindByGUID<DRLMap>("MP-df2");
				uIMapOverviewView2.caller = this;
				view.depth = 2;
				break;
			}
			case "fly.mega-maps@click":
			{
				UIMapsView v = base.app.view.ui.screens.Open<UIMapsView>("maps-screen-grid");
				RunOnce(delegate
				{
					v.screen.title = base.app.model.storage.locale.Get("maps.megamaps.title", "Originals");
				}, 0.01f);
				v.allowedMaps.Clear();
				v.allowedMaps.AddRange(new string[11]
				{
					"MP-f95", "MP-615", "MP-95a", "MP-103", "MP-409", "MP-b59", "MP-23c", "MP-b9d", "MP-50c", "MP-19c",
					"MP-2cb"
				});
				v.SetCategoriesEnabled(p_simple_courses: false, p_community_maps: false, p_multigp_maps: false, p_drl_maps: false, p_mega_maps: true);
				v.caller = this;
				view.depth = 2;
				break;
			}
			case "fly.drl-maps@click":
			{
				UIMapsView uIMapsView = base.app.view.ui.screens.Open<UIMapsView>("maps-screen-grid");
				uIMapsView.screen.title = base.app.model.storage.locale.Get("maps.drl.title", "DRL Maps");
				uIMapsView.allowedMaps.Clear();
				uIMapsView.SetCategoriesEnabled(p_simple_courses: false, p_community_maps: false, p_multigp_maps: false, p_drl_maps: true);
				uIMapsView.caller = this;
				view.depth = 2;
				break;
			}
			case "fly.sd-drl-maps@click":
			{
				UIMapSDOverviewView uIMapSDOverviewView = base.app.view.ui.screens.Open<UIMapSDOverviewView>("collectables-map-overview-screen");
				uIMapSDOverviewView.category = GameFlag.MapFeatured;
				uIMapSDOverviewView.isFeatured = true;
				uIMapSDOverviewView.usaf = false;
				uIMapSDOverviewView.screen.title = base.app.model.storage.locale.Get("maps.sd-courses.title", "Search & Destroy Tracks");
				uIMapSDOverviewView.caller = this;
				view.depth = 2;
				break;
			}
			case "fly.out-of-service@click":
			case "fly.gates-of-hell@click":
			{
				UIMapOverviewView uIMapOverviewView = base.app.view.ui.screens.Open<UIMapOverviewView>("map-overview-screen");
				uIMapOverviewView.category = GameFlag.MapDRL;
				uIMapOverviewView.isFeatured = false;
				uIMapOverviewView.usaf = false;
				string p_guid = "";
				switch (p_event)
				{
				case "fly.out-of-service@click":
					p_guid = "MP-103";
					break;
				case "fly.gates-of-hell@click":
					p_guid = "MP-95a";
					break;
				}
				DRLMap dRLMap = base.app.model.storage.library.FindByGUID<DRLMap>(p_guid);
				uIMapOverviewView.screen.title = dRLMap.label;
				uIMapOverviewView.data = dRLMap;
				uIMapOverviewView.isFeatured = false;
				uIMapOverviewView.usaf = false;
				uIMapOverviewView.caller = this;
				view.depth = 2;
				break;
			}
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			}
		}

		private bool CheckForRaceFavorite()
		{
			List<DRLMapTrack> mapTracks = base.app.model.storage.GetMapTracks();
			foreach (DRLMapFavoriteData favoriteMap in base.app.model.storage.state.player.favoriteMaps)
			{
				if (favoriteMap.customMap)
				{
					MapData mapData = base.app.model.storage.maps.FindByGUID(favoriteMap.trackId);
					if (mapData != null && mapData.mode.typeFlag == GameFlag.Race)
					{
						return true;
					}
					continue;
				}
				foreach (DRLMapTrack item in mapTracks)
				{
					if (item.guid == favoriteMap.trackId && !item.freestyleOnly)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}

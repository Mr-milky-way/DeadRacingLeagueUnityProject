using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIMapsController : Controller<DRLApp>
	{
		public UIMapsView view => AssertLocal<UIMapsView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@close":
				break;
			case "ui.screen@open":
			{
				if (p_data[0] as UIScreen != view.screen)
				{
					break;
				}
				bool p_allow_empty = base.app.arguments.game.type == GameFlag.MapEditor || !view.drlMapsCard.activeInHierarchy;
				List<DRLMap> l = base.app.model.storage.GetMaps(p_allow_empty);
				l.RemoveAll((DRLMap it) => (bool)it.tags && it.tags.Contains(GameFlag.MapEditorOnly));
				List<string> field_maps_guids = new List<string> { "MP-0c6", "MP-7ea", "MP-d3f", "MP-dec" };
				l.RemoveAll((DRLMap it) => field_maps_guids.Contains(it.guid));
				l.RemoveAll((DRLMap it) => view.allowedMaps.Count > 0 && !view.allowedMaps.Contains(it.guid));
				l.RemoveAll((DRLMap it) => it.guid == "MP-19c");
				List<string> originals_guids = new List<string> { "MP-103", "MP-409", "MP-23c", "MP-b59", "MP-b9d", "MP-615", "MP-50c", "MP-f95", "MP-2cb" };
				if (view.drlMapsCard.activeInHierarchy)
				{
					l.RemoveAll((DRLMap it) => originals_guids.Contains(it.guid));
				}
				List<string> values = l.ConvertAll((DRLMap it) => it.guid + "|" + it.name + ": " + it.title);
				Debug.Log(string.Format("UIMapsController> ScreenOpen / Found [{0}] Maps\n{1}", l.Count, string.Join("\n  ", values)));
				GameFlag type2 = base.app.arguments.game.type;
				if (type2 != GameFlag.MapEditor && view.drlMapsCard.activeInHierarchy)
				{
					int i = 0;
					while (i < l.Count)
					{
						List<object> list = new List<object>();
						list.AddRange(base.app.model.storage.GetMapTracks(l[i], type2, p_filter_build: true));
						list.AddRange(base.app.model.storage.maps.Find(type2 == GameFlag.Race, GameFlag.MapDRL));
						list.RemoveAll((object it) => it is MapData && (it as MapData).mapId != l[i].guid);
						if (list.Count == 0)
						{
							l.RemoveAt(i--);
						}
						int num = i + 1;
						i = num;
					}
				}
				view.Set(l);
				view.SetRatingsAvailable(p_available: false);
				LayoutGroup component = view.listField.GetComponent<LayoutGroup>();
				view.SetForGameType(type2, component);
				break;
			}
			case "fly.map-card@click":
			{
				UICardButtonMap uICardButtonMap = (UICardButtonMap)p_target;
				if (!uICardButtonMap)
				{
					break;
				}
				GameFlag type = base.app.arguments.game.type;
				DRLMap data = uICardButtonMap.data;
				Debug.Log("UIMapsController> Map [" + uICardButtonMap.data.scene + "] clicked - game-type[" + type.ToString() + "]");
				if (type == GameFlag.MapEditor)
				{
					base.enabled = false;
					base.app.controller.LoadMapEditor(data, null);
					break;
				}
				UIMapOverviewView uIMapOverviewView5 = base.app.view.ui.screens.Open<UIMapOverviewView>("map-overview-screen");
				uIMapOverviewView5.category = GameFlag.MapDRL;
				uIMapOverviewView5.isFeatured = false;
				uIMapOverviewView5.usaf = false;
				uIMapOverviewView5.screen.title = uICardButtonMap.data.title;
				uIMapOverviewView5.data = uICardButtonMap.data;
				uIMapOverviewView5.caller = view.caller;
				UIMapsCategoryController uIMapsCategoryController = view.caller as UIMapsCategoryController;
				if ((bool)uIMapsCategoryController)
				{
					uIMapsCategoryController.view.depth = 3;
				}
				break;
			}
			case "fly.debug.community-maps@click":
			case "fly.community-maps@click":
			{
				UICommunityMapsView uICommunityMapsView = base.app.view.ui.screens.Open<UICommunityMapsView>("community-maps-screen");
				uICommunityMapsView.screen.title = base.app.model.storage.locale.Get("maps.community.title", "Community Maps");
				uICommunityMapsView.allowExit = false;
				uICommunityMapsView.InitFilter(p_isMultiGP: false);
				break;
			}
			case "fly.simple-courses@click":
			{
				UIMapOverviewView uIMapOverviewView4 = base.app.view.ui.screens.Open<UIMapOverviewView>("map-overview-screen");
				uIMapOverviewView4.category = GameFlag.MapSimple;
				uIMapOverviewView4.screen.title = "SIMPLE COURSES";
				uIMapOverviewView4.data = base.app.model.storage.library.FindByGUID<DRLMap>("MP-d3f");
				break;
			}
			case "fly.multigp@click":
			{
				UIMapOverviewView uIMapOverviewView3 = base.app.view.ui.screens.Open<UIMapOverviewView>("map-overview-screen");
				uIMapOverviewView3.category = GameFlag.MapMultiGP;
				uIMapOverviewView3.screen.title = "MULTIGP";
				uIMapOverviewView3.data = base.app.model.storage.library.FindByGUID<DRLMap>("MP-7ea");
				break;
			}
			case "fly.favorite-maps@click":
			{
				UIMapOverviewView uIMapOverviewView2 = base.app.view.ui.screens.Open<UIMapOverviewView>("map-overview-screen");
				uIMapOverviewView2.category = GameFlag.MapFavorite;
				uIMapOverviewView2.screen.title = "FAVORITE MAPS";
				uIMapOverviewView2.data = base.app.model.storage.library.FindByGUID<DRLMap>("MP-27a");
				break;
			}
			case "fly.featured-tracks@click":
			{
				UIMapOverviewView uIMapOverviewView = base.app.view.ui.screens.Open<UIMapOverviewView>("map-overview-screen");
				uIMapOverviewView.category = GameFlag.MapCommon;
				uIMapOverviewView.isFeatured = true;
				uIMapOverviewView.usaf = false;
				uIMapOverviewView.screen.title = "FEATURED TRACKS";
				uIMapOverviewView.data = base.app.model.storage.library.FindByGUID<DRLMap>("MP-94f");
				break;
			}
			case "home.usaf@click":
				base.app.view.ui.screens.Open<UIMapsUSAFView>("maps-usaf-screen").caller = view.caller;
				break;
			case "ui.screen.return@click":
				view.allowedMaps.Clear();
				RunOnce(delegate
				{
					base.app.view.ui.screens.Return();
				}, 0.75f);
				break;
			}
		}
	}
}

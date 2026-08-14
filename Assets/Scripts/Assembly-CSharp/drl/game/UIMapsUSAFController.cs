using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIMapsUSAFController : Controller<DRLApp>
	{
		public UIMapsUSAFView view => AssertLocal<UIMapsUSAFView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event != null && p_event == "maps.track-selection-complete")
			{
				UIMapOverviewController uIMapOverviewController = p_target as UIMapOverviewController;
				if (!(this == null) && !(uIMapOverviewController == null) && !(uIMapOverviewController.view == null) && !(uIMapOverviewController.view.caller != this))
				{
					Notify("maps.selection-complete", p_data);
				}
			}
			if (base.app.view.ui.screens.current != view.screen)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				if (!(p_data[0] as UIScreen != view.screen))
				{
					view.depth = 2;
				}
				break;
			case "usaf.day@click":
			{
				DRLMap usafDay = view.usafDay;
				if (!(usafDay == null))
				{
					UIMapOverviewView uIMapOverviewView2 = base.app.view.ui.screens.Open<UIMapOverviewView>("map-overview-screen");
					uIMapOverviewView2.category = GameFlag.MapDRL;
					uIMapOverviewView2.isFeatured = false;
					uIMapOverviewView2.screen.title = usafDay.title;
					uIMapOverviewView2.data = usafDay;
					uIMapOverviewView2.caller = view.caller;
					uIMapOverviewView2.usaf = true;
					uIMapOverviewView2.usafDay = true;
					uIMapOverviewView2.usafNight = false;
				}
				break;
			}
			case "usaf.night@click":
			{
				DRLMap usafNight = view.usafNight;
				if (!(usafNight == null))
				{
					UIMapOverviewView uIMapOverviewView = base.app.view.ui.screens.Open<UIMapOverviewView>("map-overview-screen");
					uIMapOverviewView.category = GameFlag.MapDRL;
					uIMapOverviewView.isFeatured = false;
					uIMapOverviewView.screen.title = usafNight.title;
					uIMapOverviewView.data = usafNight;
					uIMapOverviewView.caller = view.caller;
					uIMapOverviewView.usaf = true;
					uIMapOverviewView.usafDay = false;
					uIMapOverviewView.usafNight = true;
				}
				break;
			}
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			}
		}
	}
}

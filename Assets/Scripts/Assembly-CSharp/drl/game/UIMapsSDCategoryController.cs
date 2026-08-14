using UnityEngine;
using UnityEngine.UI;
using thelab.mvc;

namespace drl.game
{
	public class UIMapsSDCategoryController : Controller<DRLApp>
	{
		public UICardButtonLarge[] onlineOnlyCards;

		public UIMapsSDCategoryView view => AssertLocal<UIMapsSDCategoryView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "maps.community-map-selection-complete":
			{
				Debug.Log("UIMapsSDCategoryController> CommunityMapSelectionComplete");
				UICommunityMapsController uICommunityMapsController = p_target as UICommunityMapsController;
				if (!(this == null) && !(uICommunityMapsController == null) && !(uICommunityMapsController.view == null) && !(uICommunityMapsController.view.caller != this))
				{
					Notify("maps.selection-complete", p_data);
				}
				break;
			}
			case "maps.track-selection-complete":
			{
				Debug.Log("UIMapsSDCategoryController> TrackSelectionComplete");
				UIMapSDOverviewController uIMapSDOverviewController = p_target as UIMapSDOverviewController;
				if (!(this == null) && !(uIMapSDOverviewController == null) && !(uIMapSDOverviewController.view == null) && !(uIMapSDOverviewController.view.caller != this))
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
			if (base.app.view.ui.screens.current != view.screen)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
			{
				UICardButtonLarge[] array = onlineOnlyCards;
				foreach (UICardButtonLarge uICardButtonLarge in array)
				{
					if (DRLApp.offline)
					{
						Debug.Log("UIHomeFlyController> OnNotification: N.UI.ScreenOpen: Set offline: " + uICardButtonLarge);
						uICardButtonLarge.enabled = false;
						Component component = uICardButtonLarge.Find<Component>("backgrounds.disabled");
						VerticalLayoutGroup verticalLayoutGroup = uICardButtonLarge.Find<VerticalLayoutGroup>("content.body");
						UIStatusView uIStatusView = uICardButtonLarge.Find<UIStatusView>("content.status");
						if (component != null)
						{
							component.gameObject.SetActive(value: true);
							string warning = base.app.model.storage.locale.Get("ui.offline.status", "UNAVAILABLE (OFFLINE)");
							uIStatusView.SetWarning(warning);
							uIStatusView.fade.alpha = 1f;
							RectOffset padding = verticalLayoutGroup.padding;
							verticalLayoutGroup.enabled = false;
							verticalLayoutGroup.padding = padding;
							verticalLayoutGroup.enabled = true;
						}
					}
				}
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
			case "fly.community-maps@click":
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
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			}
		}
	}
}

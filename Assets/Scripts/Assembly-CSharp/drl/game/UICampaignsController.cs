using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UICampaignsController : Controller<DRLApp>
	{
		public UICampaignsView view => AssertLocal<UICampaignsView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				if (!(p_data[0] as UIScreen != view.screen))
				{
					List<DRLCampaign> campaigns = base.app.model.storage.GetCampaigns();
					Debug.Log("UICampaignsController> Open - Found [" + campaigns.Count + "] Maps");
					view.Set(campaigns);
					UINavigation.Link(view.listField.GetComponent<LayoutGroup>(), view.leftNavigation);
				}
				break;
			case "campaign.campaign-card@click":
			{
				UICardButtonCampaign uICardButtonCampaign = (UICardButtonCampaign)p_target;
				if ((bool)uICardButtonCampaign)
				{
					base.app.arguments.game.campaign = uICardButtonCampaign.data;
					base.app.arguments.game.promo = uICardButtonCampaign.data.tournament;
					if (uICardButtonCampaign.data.tournament)
					{
						base.app.view.ui.screens.Open<UITryoutsOnboardingView>("tryouts-onboarding-screen").data = uICardButtonCampaign.data;
						break;
					}
					UICampaignOverviewView uICampaignOverviewView = base.app.view.ui.screens.Open<UICampaignOverviewView>("campaign-overview-screen");
					uICampaignOverviewView.screen.title = uICardButtonCampaign.data.title;
					uICampaignOverviewView.data = uICardButtonCampaign.data;
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

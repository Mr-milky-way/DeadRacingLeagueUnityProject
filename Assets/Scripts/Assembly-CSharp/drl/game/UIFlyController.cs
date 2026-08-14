using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIFlyController : Controller<DRLApp>
	{
		public UIFlyView view => AssertLocal<UIFlyView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event != null && p_event == "maps.selection-complete")
			{
				base.app.controller.LoadTrackOverview(this, p_target, p_data);
			}
			if (base.app.view.ui.screens.current != view.screen)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				Debug.LogError("UIFlyController is deprecated, use direct MapSelector calls from UIHomeController instead");
				if (!(p_data[0] as UIScreen != view.screen))
				{
					PopulateCards();
					view.ActivateAllTiles(p_activate: true);
					Activity.RunOnce(delegate
					{
						UINavigation.Focus(view.listField);
					}, 1f / 30f);
				}
				break;
			case "fly.freecamera@click":
			{
				UIMapsCategoryView uIMapsCategoryView = base.app.view.ui.screens.Open<UIMapsCategoryView>("maps-category-screen");
				uIMapsCategoryView.screen.title = base.app.model.storage.locale.Get("fly.screen.freecamera", "FreeCamera");
				uIMapsCategoryView.caller = this;
				base.app.arguments.Clear();
				base.app.arguments.game.type = GameFlag.FreeCamera;
				base.app.arguments.game.mode = GameFlag.SinglePlayer;
				break;
			}
			case "fly.campaign@click":
				base.app.view.ui.screens.Open<UICampaignsView>("campaigns-screen");
				base.app.arguments.Clear();
				base.app.arguments.game.type = GameFlag.Campaign;
				base.app.arguments.game.mode = GameFlag.SinglePlayer;
				base.app.arguments.game.AddPlayer(base.app.model.storage.state.player.playerData);
				break;
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			case "ui.screen@close":
				view.ActivateAllTiles(p_activate: false);
				break;
			}
			if (p_event.IndexOf("@click") >= 0 && p_event.IndexOf("@paywall") >= 0)
			{
				base.app.view.ui.screens.Open("purchase-overview-screen", 0f);
			}
		}

		protected void PopulateCards()
		{
			if (view.listField.Count > 0)
			{
				return;
			}
			AssetLibrary assetLibrary = base.app.model.storage.library.FindByGUID<AssetLibrary>("fly-screen-cards");
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
			for (int j = 0; j < list.Count; j++)
			{
				UICardButtonLarge uICardButtonLarge = Object.Instantiate(list[j]);
				int startIndex = uICardButtonLarge.name.IndexOf("-card");
				string oldValue = uICardButtonLarge.name.Substring(startIndex);
				uICardButtonLarge.name = uICardButtonLarge.name.Replace("fly-", "").Replace(oldValue, "");
				view.listField.Push(uICardButtonLarge);
			}
			UINavigation.Link(view.listField.GetComponent<LayoutGroup>(), view.leftNavigation);
		}
	}
}

using UnityEngine;
using thelab.core;

namespace drl.game
{
	public class UIHomeFlyController : UIHomeController
	{
		public UICardButtonLarge[] onlineOnlyCards;

		public new UIHomeFlyView view => AssertLocal<UIHomeFlyView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event != null && p_event == "maps.selection-complete" && base.validContext && base.app.controller.AssertMapSelection(p_target, this))
			{
				base.app.controller.LoadTrackOverview(this, p_target, p_data);
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
					UICardButtonLarge[] array = onlineOnlyCards;
					foreach (UICardButtonLarge uICardButtonLarge in array)
					{
						bool p_flag = !DRLApp.offline;
						uICardButtonLarge.notification.Contains("multiplayer");
						string p_label = base.app.model.storage.locale.Get("ui.offline.status", "UNAVAILABLE (OFFLINE)");
						view.SetCardEnabled(uICardButtonLarge, p_flag, p_label);
					}
					if (base.app.inVirtualSeason)
					{
						view.SetVirtualSeasonLayout();
					}
				}
				break;
			case "ui.screen.return@click":
				base.app.model.service.StopTournamentRefresh();
				base.app.view.ui.screens.Return();
				break;
			case "home.race@click":
			{
				UIMapsCategoryView uIMapsCategoryView2 = base.app.view.ui.screens.Open<UIMapsCategoryView>("maps-category-screen");
				uIMapsCategoryView2.screen.title = base.app.model.storage.locale.Get("home.card.race", "Solo Race");
				uIMapsCategoryView2.caller = this;
				SetAppArguments(GameFlag.Race, GameFlag.SinglePlayer);
				break;
			}
			case "home.collectable@click":
			{
				UIMapsSDCategoryView uIMapsSDCategoryView = base.app.view.ui.screens.Open<UIMapsSDCategoryView>("collectables-category-screen");
				uIMapsSDCategoryView.screen.title = base.app.model.storage.locale.Get("home.card.collectable", "Search & Destroy");
				uIMapsSDCategoryView.caller = this;
				SetAppArguments(GameFlag.Collectable, GameFlag.SinglePlayer);
				break;
			}
			case "home.freestyle@click":
			{
				UIMapsCategoryView uIMapsCategoryView = base.app.view.ui.screens.Open<UIMapsCategoryView>("maps-category-screen");
				uIMapsCategoryView.screen.title = base.app.model.storage.locale.Get("fly.card.fly-freestyle-card", "Freestyle");
				uIMapsCategoryView.caller = this;
				SetAppArguments(GameFlag.Freestyle, GameFlag.SinglePlayer);
				break;
			}
			case "home.circuits@click":
				base.app.view.ui.screens.Open<UICircuitSelectionView>("circuits-selection-screen");
				SetAppArguments(GameFlag.Race, GameFlag.SinglePlayer);
				break;
			case "home.multiplayer@click":
				if (!IsOffline())
				{
					CheckMultiplayerAvailability(delegate
					{
						base.app.view.ui.screens.Open("multiplayer-lobby-screen", 0f);
					});
				}
				break;
			}
		}

		private void SetAppArguments(GameFlag p_type, GameFlag p_mode)
		{
			base.app.arguments.Clear();
			base.app.arguments.game.type = p_type;
			base.app.arguments.game.mode = p_mode;
			base.app.arguments.game.AddPlayer(base.app.model.storage.state.player.playerData);
		}

		private bool IsOffline()
		{
			bool offline = DRLApp.offline;
			if (offline)
			{
				base.app.view.ui.dialog.Open(DialogTemplateType.OfflineMode, "no-connection");
			}
			return offline;
		}
	}
}

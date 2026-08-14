using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UICircuitSelectionController : Controller<DRLApp>
	{
		public UICircuitSelectionView view => AssertLocal<UICircuitSelectionView>("view");

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
					view.Set(base.app.model.storage.state.player.circuits.circuits);
				}
				break;
			case "circuits.circuit-card@click":
			{
				DRLCircuitData circuitData = ((UICircuitItemView)p_target).circuitData;
				if (view.caller != null)
				{
					UILeaderboardsController uILeaderboardsController = view.caller as UILeaderboardsController;
					if (uILeaderboardsController != null)
					{
						uILeaderboardsController.view.circuit = circuitData;
						view.caller = null;
						base.app.view.ui.screens.Return();
						break;
					}
				}
				UICircuitOverviewView uICircuitOverviewView = base.app.view.ui.screens.Open<UICircuitOverviewView>("circuits-overview-screen");
				if (circuitData != null)
				{
					uICircuitOverviewView.circuitData = circuitData;
				}
				uICircuitOverviewView.caller = this;
				break;
			}
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			case "circuits.circuit-selection.exit@click":
				if (base.app.inGame)
				{
					base.enabled = false;
					base.app.view.audio.PlayUIGenericSuccess();
					base.app.controller.game.Exit();
				}
				break;
			}
		}
	}
}

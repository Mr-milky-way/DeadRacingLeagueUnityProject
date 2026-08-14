using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class UIHomeLeadersController : Controller<DRLApp>
	{
		public UIHomeLeadersView view => AssertLocal<UIHomeLeadersView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen.return@click":
				base.app.model.service.StopTournamentRefresh();
				base.app.view.ui.screens.Return();
				break;
			case "home.leaderboards.drl@click":
				if (!IsOffline())
				{
					UILeaderboardsView uILeaderboardsView2 = base.app.view.ui.screens.Open<UILeaderboardsView>("leaderboards-screen");
					uILeaderboardsView2.screen.title = base.app.model.storage.locale.Get("home.card.leaders.drl", "DRL LEADERS");
					uILeaderboardsView2.gameTypeFlag = GameFlag.Race;
					base.app.arguments.lastLeaderboard = DRLAppArguments.LeaderboardType.drl;
				}
				break;
			case "home.leaderboards.open@click":
				if (!IsOffline())
				{
					UILeaderboardsView uILeaderboardsView = base.app.view.ui.screens.Open<UILeaderboardsView>("leaderboards-screen");
					uILeaderboardsView.screen.title = base.app.model.storage.locale.Get("home.card.leaders.open", "OPEN CLASS LEADERS").Replace("\n", " ");
					uILeaderboardsView.gameTypeFlag = GameFlag.Race;
					base.app.arguments.lastLeaderboard = DRLAppArguments.LeaderboardType.open;
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

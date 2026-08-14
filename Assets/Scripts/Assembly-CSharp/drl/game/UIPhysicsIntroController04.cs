using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIPhysicsIntroController04 : Controller<DRLApp>
	{
		public UIPhysicsIntroView04 view => AssertLocal<UIPhysicsIntroView04>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (!view.current)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				RunOnce(1f / 30f, delegate
				{
					base.app.view.ui.footer.Hide(0f);
				});
				UINavigation.focus = view.rightNavigation;
				if (base.app.model.game != null)
				{
					RunOnce(0.2f, delegate
					{
						base.app.model.game.simulation.cameras.Get(0).follow.enabled = false;
						base.app.model.game.simulation.cameras.Get(0).orbit.enabled = false;
					});
				}
				break;
			case "intro.graphics@open":
				base.app.view.ui.screens.Open<UISettingsSystemView>("settings-system-screen");
				break;
			case "intro.sandbox@open":
			{
				UIMapTrackShortcutView physicsSandboxShortcutView = view.physicsSandboxShortcutView;
				if (!(physicsSandboxShortcutView == null))
				{
					base.enabled = false;
					base.app.arguments.Clear();
					base.app.arguments.game.type = GameFlag.Sandbox;
					base.app.arguments.game.mode = GameFlag.SinglePlayer;
					base.app.arguments.game.map = physicsSandboxShortcutView.map;
					base.app.arguments.game.track = physicsSandboxShortcutView.track;
					base.app.arguments.game.fcMode = base.app.model.storage.state.player.activeFCMode;
					base.app.arguments.game.podium = "";
					base.app.arguments.game.allowCrash = false;
					base.app.arguments.game.promo = false;
					base.app.arguments.game.players.Clear();
					base.app.arguments.game.AddPlayer(base.app.model.storage.state.player.playerData);
					base.app.view.audio.PlayUIStartGame();
					base.app.view.audio.SceneMainToGame(1.6f);
					base.app.view.ui.fade.FadeIn(1.5f);
					RunOnce(1f, base.app.scene.Load);
					base.app.model.storage.state.license.Poll();
				}
				break;
			}
			case "intro.screens.close":
				base.app.view.ui.screens.CloseAllScreens();
				break;
			}
		}
	}
}

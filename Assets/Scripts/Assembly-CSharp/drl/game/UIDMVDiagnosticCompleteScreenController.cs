using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIDMVDiagnosticCompleteScreenController : Controller<DRLApp>
	{
		public UIDMVDiagnosticCompleteScreenView view => AssertLocal<UIDMVDiagnosticCompleteScreenView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (!(base.app.view.ui.screens.current != view.screen))
			{
				switch (p_event)
				{
				case "ui.screen@open":
					_ = p_data[0] as UIScreen != view.screen;
					break;
				case "ui.screen.nav-right@click":
					base.app.view.ui.screens.Open<UIDMVTestsView>("dmv-tests-screen");
					break;
				case "missions.mission-complete.exit@click":
					base.enabled = false;
					base.app.view.audio.PlayUIGenericSuccess();
					base.app.controller.game.Exit();
					break;
				}
			}
		}
	}
}

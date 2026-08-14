using UnityEngine;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UISettingsControllerMapController : Controller<DRLApp>
	{
		public UISettingsControllerMapView view => AssertLocal<UISettingsControllerMapView>("view");

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
					if (!RCI.HasControllersConnected())
					{
						Debug.LogWarning("UISettingsController> ScreenOpen - NO HARDWARE");
						break;
					}
					view.EnableControllerOverlay(enable: true);
					ControllerStateType controllerStateType = RCI.GetControllerStateType(ControllerStateType.Taranis);
					view.Set(controllerStateType);
				}
				break;
			case "ui.screen.return@click":
				view.EnableControllerOverlay(enable: false);
				base.app.view.ui.screens.Return();
				break;
			}
		}
	}
}

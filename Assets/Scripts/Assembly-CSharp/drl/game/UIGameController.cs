using UnityEngine;
using drl.sim;
using drl.sim.rci;
using thelab.mvc;

namespace drl.game
{
	public class UIGameController : Controller<DRLApp>
	{
		public UIGame view => AssertLocal<UIGame>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "boot@complete":
			{
				ControllerStateType controllerStateType = RCI.GetControllerStateType(ControllerStateType.Taranis);
				view.hud.controller.SetController(controllerStateType);
				break;
			}
			case "input.active-controller.changed":
			case "settings.controller.connect":
			{
				ControllerStateType controller = RCI.GetControllerStateType(ControllerStateType.Taranis);
				if (base.app.arguments.game.type == GameFlag.Mission)
				{
					ControllerTypeTag component = base.app.arguments.game.mission.GetComponent<ControllerTypeTag>();
					if ((bool)component)
					{
						controller = component.tags[0];
					}
				}
				view.hud.controller.SetController(controller);
				break;
			}
			case "game.simulation.drone.turtle@on":
				view.hud.turtleMode.SetDroneTurtle(p_flag: true);
				break;
			case "game.simulation.drone.turtle@off":
				view.hud.turtleMode.SetDroneTurtle(p_flag: false);
				break;
			case "game.simulation.arm-and-turtle@armed":
				view.hud.turtleMode.SetDroneArmed(p_flag: true);
				break;
			case "game.simulation.arm-and-turtle@disarmed":
				view.hud.turtleMode.SetDroneArmed(p_flag: false);
				break;
			}
		}
	}
}

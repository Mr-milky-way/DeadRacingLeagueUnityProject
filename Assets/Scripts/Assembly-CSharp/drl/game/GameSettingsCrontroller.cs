using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class GameSettingsCrontroller : Controller<DRLApp>
	{
		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event != null && !(p_event == "game.simulation.drone@ready"))
			{
				_ = p_event == "settings.ready";
			}
		}
	}
}

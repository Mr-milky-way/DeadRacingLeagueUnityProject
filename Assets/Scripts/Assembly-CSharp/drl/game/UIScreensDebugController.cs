using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class UIScreensDebugController : Controller<DRLApp>
	{
		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event != null)
			{
				_ = p_event == "scene.start";
			}
		}
	}
}

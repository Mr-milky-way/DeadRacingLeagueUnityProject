using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class UIControllerSelectionController : Controller<DRLApp>
	{
		public UIControllerSelectionView view => AssertLocal<UIControllerSelectionView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			_ = view.current;
		}
	}
}

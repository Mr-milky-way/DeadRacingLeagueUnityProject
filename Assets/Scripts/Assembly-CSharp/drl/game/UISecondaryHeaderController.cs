using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class UISecondaryHeaderController : Controller<DRLApp>
	{
		public UISecondaryHeaderView view => AssertLocal<UISecondaryHeaderView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event != null && p_event == "tournament.action.refresh" && !base.app.inTournament)
			{
				view.ShowUnderReviewWarning(p_show: false);
			}
		}
	}
}

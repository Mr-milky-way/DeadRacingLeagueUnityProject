using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class HeaderItemController : Controller<DRLApp>
	{
		[SerializeField]
		private int breadCrumbIndex;

		public HeaderItemView view => AssertLocal<HeaderItemView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			default:
				_ = p_event == "ui.screen@close";
				break;
			case "ui.screen.breadcrumb@click":
			{
				int.TryParse(p_data[0].ToString(), out var result);
				base.app.view.ui.screens.GoToBreadCrumbSelectedScreen(result);
				break;
			}
			case "ui.screen@open":
				break;
			}
		}
	}
}

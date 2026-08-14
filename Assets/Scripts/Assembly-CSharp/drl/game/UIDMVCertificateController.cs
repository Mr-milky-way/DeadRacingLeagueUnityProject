using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class UIDMVCertificateController : Controller<DRLApp>
	{
		private Vector2 m_initSize;

		public UIDMVCertificateView view => AssertLocal<UIDMVCertificateView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (!(base.app.view.ui.screens.current != view.screen))
			{
				switch (p_event)
				{
				case "ui.screen@open":
					view.AnimateBadge();
					break;
				case "missions.lesson-complete.tests@click":
					base.app.view.ui.screens.Open("dmv-tests-screen");
					break;
				}
			}
		}
	}
}

using UnityEngine;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIPurchaseOverviewController : Controller<DRLApp>
	{
		internal bool lockNavigation;

		internal bool hasData;

		internal string productId;

		public UIPurchaseOverviewView view => AssertLocal<UIPurchaseOverviewView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event != null && p_event == "ui.screen@close")
			{
				hasData = false;
			}
			if (base.app.view.ui.screens.current != view.screen)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				if (!(p_data[0] as UIScreen != view.screen))
				{
					lockNavigation = false;
					view.status.fade.alpha = -0.1f;
					base.app.model.service.License(delegate(DRLLicenseResult p_result)
					{
						hasData = true;
						productId = p_result.id.ToString();
						bool mode = p_result.id != 10;
						view.SetMode(mode);
						view.price = "<size=32>$ " + p_result.cost + "</size>";
					});
				}
				break;
			case "ui.screen.nav-right@click":
			{
				UIElementView uIElementView = p_target as UIElementView;
				if (uIElementView.name == "help")
				{
					WebBrowser.OpenURL("https://drlracingsimulator.zendesk.com/hc/en-us/articles/115002509112", (base.app != null) ? base.app.model.service.platform : null);
				}
				else
				{
					if (uIElementView.name != "checkout" || lockNavigation || !hasData)
					{
						break;
					}
					lockNavigation = true;
					view.status.SetLoading(0f);
					view.status.fade.FadeIn(0.2f);
					base.app.model.service.backend.Transaction(productId, 1, delegate(DRLTransactionResult p_result)
					{
						string text = ((p_result == null) ? "ERROR" : p_result.result);
						Debug.Log("UIPurchaseOverviewController> Transaction - result[" + text + "]");
						lockNavigation = false;
						switch (text)
						{
						case "OK":
							base.app.view.audio.PlayUIGenericSuccess();
							view.status.message = "TRANSACTION COMPLETED!";
							base.app.view.ui.fade.FadeIn(1.5f, 1f);
							base.app.arguments.Clear();
							Activity.RunOnce(delegate
							{
								base.app.scene.LoadMain(p_force: true);
							}, 2.7f);
							break;
						case "CANCEL":
							view.status.SetWarning("TRANSACTION CANCELLED!");
							view.status.fade.FadeOut(0.2f, 2f);
							break;
						case "ERROR":
							base.app.view.audio.PlayUIGenericError();
							view.status.SetWarning("TRANSACTION FAILED!");
							view.status.fade.FadeOut(0.2f, 2f);
							break;
						}
					});
				}
				break;
			}
			case "ui.screen.return@click":
				if (!lockNavigation)
				{
					base.app.view.ui.screens.Return();
				}
				break;
			}
		}
	}
}

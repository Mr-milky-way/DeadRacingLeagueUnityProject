using UnityEngine;
using thelab.mvc;

namespace drl.game
{
	public class UILoaderController : Controller<DRLApp>
	{
		public UILoaderView view => AssertLocal<UILoaderView>("view");

		protected override void Start()
		{
		}

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "boot.drl.bundle.load@start":
				view.background = null;
				view.progress = 0f;
				view.fade.alpha = 1f;
				break;
			case "boot.drl.content.download@progress":
			{
				float progress = (float)p_data[0];
				view.progress = progress;
				break;
			}
			case "boot.drl.offline-maps.download@start":
			case "boot.drl.offline-maps.store@start":
				view.progress = 0f;
				view.fade.alpha = 1f;
				break;
			case "boot.drl.offline-maps.download@progress":
			case "boot.drl.offline-maps.store@progress":
				if (!DRLApp.offline)
				{
					float progress3 = (float)p_data[0];
					view.progress = progress3;
				}
				break;
			case "boot.drl.bundle.load@progress":
			{
				float progress2 = (float)p_data[0];
				view.progress = progress2;
				break;
			}
			case "boot@complete":
				if (!base.app || !base.app.model.game)
				{
					view.progress = 1f;
					view.fade.FadeOut(0.3f, 0.5f);
				}
				break;
			}
		}

		public void OnPersistency()
		{
		}
	}
}

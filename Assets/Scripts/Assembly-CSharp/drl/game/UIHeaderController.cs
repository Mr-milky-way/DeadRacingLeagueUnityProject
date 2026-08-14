using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIHeaderController : Controller<DRLApp>, ILocaleElement
	{
		public UIHeaderView view => AssertLocal<UIHeaderView>("view");

		protected override void Start()
		{
			base.Start();
			Localization.Add(this);
		}

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "boot@complete":
				view.versionField.text = DRLApp.GetVersionString();
				view.SetDebug(Debug.unityLogger.logEnabled);
				break;
			case "ui.screen@change":
			{
				string path = base.app.view.ui.screens.manager.path;
				UIScreen uIScreen = (UIScreen)p_data[0];
				Notify("analytics.ui.menu.opened", uIScreen, path);
				Debug.Log("UIHeaderController> New Path [" + path + "]");
				view.Set(base.app.view.ui.screens.manager);
				break;
			}
			}
		}

		public void OnLocaleRefresh()
		{
			if (!(this == null) && !(base.app == null) && !(view == null))
			{
				view.Set(base.app.view.ui.screens.manager);
			}
		}
	}
}

using System;
using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIDialogController : Controller<DRLApp>
	{
		public string NotificationOnConfirm;

		public string NotificationOnCancel;

		public string NotificationOnToggle;

		public string NotificationOnNavLeft;

		public string NotificationOnNavRight;

		public Action OnConfirm;

		public Action OnCancel;

		public Action OnNavLeft;

		public Action OnNavRight;

		public Action<bool> OnToggle;

		public UIDialogView view => AssertLocal<UIDialogView>("view");

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			if (base.app.view.ui.screens.current != view.screen || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@close":
				RunOnce(0.05f, delegate
				{
					view.Clear();
				});
				break;
			case "ui.screen@open":
				_ = p_data[0] as UIScreen != view.screen;
				break;
			case "ui.screen.return@click":
				base.app.view.ui.screens.Return();
				break;
			case "ui.dialog.button.cancel@click":
				if (OnCancel != null)
				{
					OnCancel();
				}
				if (!string.IsNullOrEmpty(NotificationOnCancel))
				{
					Notify(NotificationOnCancel);
				}
				break;
			case "ui.dialog.button.confirm@click":
				if (OnConfirm != null)
				{
					OnConfirm();
				}
				if (!string.IsNullOrEmpty(NotificationOnConfirm))
				{
					Notify(NotificationOnConfirm);
				}
				break;
			case "ui.dialog.nav.left@click":
				if (OnNavLeft != null)
				{
					OnNavLeft();
				}
				if (!string.IsNullOrEmpty(NotificationOnNavLeft))
				{
					Notify(NotificationOnNavLeft);
				}
				break;
			case "ui.dialog.nav.right@click":
				if (OnNavRight != null)
				{
					OnNavRight();
				}
				if (!string.IsNullOrEmpty(NotificationOnNavRight))
				{
					Notify(NotificationOnNavRight);
				}
				break;
			case "ui.dialog.toggle@click":
				if (OnToggle != null)
				{
					OnToggle(view.toggle.toggle.isOn);
				}
				if (!string.IsNullOrEmpty(NotificationOnToggle))
				{
					Notify(NotificationOnToggle, view.toggle.toggle.isOn);
				}
				break;
			}
		}
	}
}

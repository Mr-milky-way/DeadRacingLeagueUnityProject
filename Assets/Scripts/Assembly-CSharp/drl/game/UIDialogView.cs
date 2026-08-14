using UnityEngine;
using UnityEngine.UI;
using thelab.mvc;

namespace drl.game
{
	public class UIDialogView : UIScreenView
	{
		public UIStatusView status;

		public UIElementView navLeft;

		public UIElementView navRight;

		public UIElementView buttonConfirm;

		public UIElementView buttonCancel;

		public DRLToggleView toggle;

		public Transform buttonConfirmBackground;

		public Transform buttonCancelBackground;

		public Text buttonConfirmText;

		public Text buttonCancelText;

		public Text toggleText;

		public Text navLeftText;

		public Text navRightText;

		public UIDialogController controller => AssertLocal<UIDialogController>("controller");

		public void SetNav(bool p_left, bool p_right)
		{
			navLeft.gameObject.SetActive(p_left);
			navRight.gameObject.SetActive(p_right);
		}

		public void SetNav(string p_left, string p_right)
		{
			navLeft.gameObject.SetActive(!string.IsNullOrEmpty(p_left));
			navRight.gameObject.SetActive(!string.IsNullOrEmpty(p_right));
			navLeftText.text = p_left;
			navRightText.text = p_right;
		}

		public void SetButtons(bool p_confirm, bool p_cancel)
		{
			buttonConfirm.gameObject.SetActive(p_confirm);
			buttonCancel.gameObject.SetActive(p_cancel);
		}

		public void SetButtons(string p_confirm, string p_cancel, string p_confirmColor = "hollow", string p_cancelColor = "hollow")
		{
			buttonConfirm.gameObject.SetActive(!string.IsNullOrEmpty(p_confirm));
			buttonCancel.gameObject.SetActive(!string.IsNullOrEmpty(p_cancel));
			buttonConfirmText.text = p_confirm;
			buttonCancelText.text = p_cancel;
			Transform transform = buttonConfirm.transform.Find("backgrounds");
			Transform transform2 = buttonCancel.transform.Find("backgrounds");
			if ((bool)transform)
			{
				for (int i = 0; i < transform.childCount; i++)
				{
					transform.GetChild(i).gameObject.SetActive(transform.GetChild(i).name == p_confirmColor);
				}
			}
			if ((bool)transform2)
			{
				for (int j = 0; j < transform2.childCount; j++)
				{
					transform2.GetChild(j).gameObject.SetActive(transform2.GetChild(j).name == p_cancelColor);
				}
			}
		}

		public void SetToggleActive(bool p_toggle)
		{
			toggle.gameObject.SetActive(p_toggle);
		}

		public void SetToggle(string p_text, bool p_isOn = false)
		{
			toggle.gameObject.SetActive(!string.IsNullOrEmpty(p_text));
			toggleText.text = p_text;
			toggle.toggle.isOn = p_isOn;
		}

		public void Clear()
		{
			SetButtons(p_confirm: false, p_cancel: false);
			SetNav(p_left: false, p_right: false);
			SetToggleActive(p_toggle: false);
			status.message = "MESSAGE";
			controller.NotificationOnCancel = null;
			controller.NotificationOnConfirm = null;
			controller.NotificationOnNavLeft = null;
			controller.NotificationOnNavRight = null;
			controller.NotificationOnToggle = null;
			controller.OnConfirm = null;
			controller.OnCancel = null;
			controller.OnNavLeft = null;
			controller.OnNavRight = null;
			controller.OnToggle = null;
		}
	}
}

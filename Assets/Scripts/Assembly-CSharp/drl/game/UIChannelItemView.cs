using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIChannelItemView : UIScreenView
	{
		[Header("Item :")]
		public int channel;

		public Text channelName;

		public DRLDropdownView axisSelection;

		public Image leftBar;

		public Image rightBar;

		public Image undetectedOverlay;

		public CanvasGroup canvasGroup;

		public Toggle midStickToggle;

		public Toggle invertToggle;

		public DRLToggleView midStickToggleView;

		public DRLToggleView invertToggleView;

		public bool isButton;

		[Header("Navigation:")]
		public UINavigation dropdownNavigation;

		public UINavigation midStickNavigation;

		public UINavigation invertNavigation;

		[HideInInspector]
		public bool preDetected;

		public void Set(int p_channelID, bool p_button = false)
		{
			channel = p_channelID;
			channelName.text = (p_button ? string.Format("{0} {1}", base.app.model.storage.locale.Get("calibration.channel.button", "BUTTON"), p_channelID) : string.Format("{0} {1}", base.app.model.storage.locale.Get("calibration.channel.channel", "CHANNEL"), p_channelID));
			isButton = p_button;
		}

		public void SetDetected(bool p_detected)
		{
			canvasGroup.alpha = ((p_detected || preDetected) ? 1f : 0.15f);
			if (!isButton)
			{
				midStickToggleView.interactable = p_detected && axisSelection.index == 1;
				invertToggleView.interactable = p_detected;
				midStickToggle.interactable = p_detected && axisSelection.index == 1;
				invertToggle.interactable = p_detected;
			}
		}
	}
}

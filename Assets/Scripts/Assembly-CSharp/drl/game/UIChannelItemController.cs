using UnityEngine.Events;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIChannelItemController : Controller<DRLApp>
	{
		private UnityAction<int> m_dropDownChangeEvent;

		private UnityAction<bool> m_invertToggleChangeEvent;

		private UnityAction<bool> m_midStickToggleChangeEvent;

		public UIChannelItemView view => AssertLocal<UIChannelItemView>("view");

		private void OnEnable()
		{
			Subscribe();
			Activity.RunOnce(delegate
			{
				view.axisSelection.dropdown.RefreshShownValue();
			}, 0.2f);
		}

		private void OnDisable()
		{
			Unsubscribe();
		}

		private void OnMidStickToggle()
		{
			Notify("calibration.channel-selection.midstick@change", view);
		}

		private void OnInvertToggle()
		{
			Notify("calibration.channel-selection.invert@change", view);
		}

		private void OnDropdownValueChanged()
		{
			Notify("calibration.channel-selection.dropdown@change", view);
		}

		public void Subscribe()
		{
			m_dropDownChangeEvent = delegate
			{
				OnDropdownValueChanged();
			};
			m_invertToggleChangeEvent = delegate
			{
				OnInvertToggle();
			};
			m_midStickToggleChangeEvent = delegate
			{
				OnMidStickToggle();
			};
			view.axisSelection.dropdown.onValueChanged.AddListener(m_dropDownChangeEvent);
			view.invertToggle.onValueChanged.AddListener(m_invertToggleChangeEvent);
			view.midStickToggle.onValueChanged.AddListener(m_midStickToggleChangeEvent);
		}

		public void Unsubscribe()
		{
			view.axisSelection.dropdown.onValueChanged.RemoveListener(m_dropDownChangeEvent);
			view.invertToggle.onValueChanged.RemoveListener(m_invertToggleChangeEvent);
			view.midStickToggle.onValueChanged.RemoveListener(m_midStickToggleChangeEvent);
			m_dropDownChangeEvent = null;
			m_invertToggleChangeEvent = null;
			m_midStickToggleChangeEvent = null;
		}
	}
}

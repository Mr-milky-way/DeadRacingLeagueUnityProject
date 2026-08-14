using UnityEngine;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UICalibrationController : Controller<DRLApp>
	{
		private UIControllerOverlay m_controllerOverlay;

		private UIDroneOverlay m_droneOverlay;

		public UICalibrationView view => AssertLocal<UICalibrationView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "settings.controller.connect":
				if (p_data.Length != 0)
				{
					string selectedName = ((string)p_data[0]).ToUpper();
					view.OnRefreshCalibrationOptions(-1, selectedName);
					view.noHardwareHelpButton.SetActive(value: false);
				}
				break;
			case "settings.controller.disconnect":
				view.OnRefreshCalibrationOptions(-1);
				view.CheckCalibrationInProgress();
				if (RCI.ControllersConnectedCount() == 0)
				{
					view.noHardwareHelpButton.SetActive(value: true);
				}
				break;
			}
			if (!view.current || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "ui.screen@open":
				Setup();
				break;
			case "ui.screen.return@click":
				view.ResetUIOverlay();
				base.app.view.ui.screens.Return();
				view.DestroyModels();
				this.TimerRunOnce(delegate
				{
					view.OpenCalibrationMenuPanel();
					DRLUINavigationSystem.controllerNavEnabled = true;
				}, 0.3f);
				break;
			case "input.help@click":
				WebBrowser.OpenURL("https://drlracingsimulator.zendesk.com/hc/en-us", (base.app != null) ? base.app.model.service.platform : null);
				break;
			case "input.help.no-hardware@click":
				WebBrowser.OpenURL("https://drlracingsimulator.zendesk.com/hc/en-us/sections/207079908-Controller-FAQ-s", (base.app != null) ? base.app.model.service.platform : null);
				break;
			case "input.auto-calibration@click":
				view.OpenAutoCalibrationPanel();
				break;
			case "input.manual-calibration@click":
				view.OpenManualCalibrationSteps();
				break;
			case "calibration.step.complete@timer.complete":
				view.CalibrationStepComplete();
				break;
			case "calibration.axis.undetected":
				view.CalibrationStepFailed();
				break;
			case "calibration.step.next@click":
				view.SetToggleSkip();
				view.CalibrationNextStep();
				break;
			case "calibration.step.back@click":
				view.CalibrationPreviousStep();
				break;
			case "input.calibration-menu-panel.open@click":
				view.OpenCalibrationMenuPanel();
				break;
			case "input.auto-calibration.save@click":
				SaveAutoCalibrationData();
				view.OpenCalibrationMenuPanel();
				view.OnCalibrationComplete();
				break;
			case "input.fine-tune@click":
				view.OpenTrimPanel();
				break;
			case "calibration.step.start@click":
				view.CalibrationStartStep();
				break;
			case "input.channel-selection@click":
				view.OpenChannelSelectionPanel(0f, p_useSavedProfile: true);
				break;
			case "calibration.axis.invert":
				if (p_data.Length == 1)
				{
					view.UpdateChannelData((CalibrationData)p_data[0]);
				}
				else if (p_data.Length == 2)
				{
					view.UpdateChannelData((RawAxis)p_data[0], (CalibrationData)p_data[1]);
				}
				else if (p_data.Length == 3)
				{
					view.UpdateChannelData((RawAxis)p_data[0], (bool)p_data[2]);
				}
				break;
			case "input.manual-calibration-panel.open":
				if (p_data.Length != 0)
				{
					if ((CalibrationData)p_data[0] != null)
					{
						view.OpenTrimPanel(0f, (CalibrationData)p_data[0]);
					}
					else
					{
						view.OpenCalibrationMenuPanel();
					}
				}
				break;
			case "calibration.channel-selection.complete":
				if (p_data.Length >= 2)
				{
					bool flag = (bool)p_data[0];
					bool flag2 = (bool)p_data[1];
					view.channelSelectionNextButton.SetActive(flag && flag2);
					view.channelsSaveButton.SetActive(flag && !flag2);
				}
				break;
			case "calibration.channel-selection.open":
				if (p_data.Length != 0)
				{
					CalibrationData calibrationData = (CalibrationData)p_data[0];
					if (calibrationData != null)
					{
						view.OpenChannelSelectionPanel(calibrationData);
					}
				}
				break;
			case "input.manual-calibration.save@click":
				view.OpenCalibrationMenuPanel();
				view.OnCalibrationComplete();
				break;
			case "calibration.invert.reset":
				view.ResetChannelData();
				break;
			case "input.sensitivity@click":
				base.app.view.ui.screens.Open<UISettingsTuningView>("settings-tuning-screen").openedFromDashboard = false;
				break;
			case "calibration.controller.dropdown@change":
				OnDropdownSelectionChange();
				break;
			case "input.transmitter-settings@change":
			{
				if (p_target == null)
				{
					break;
				}
				DRLToggleView dRLToggleView = p_target as DRLToggleView;
				if (dRLToggleView == null)
				{
					break;
				}
				if (RCI.GetActiveJoystick() == null && dRLToggleView.toggle.isOn)
				{
					dRLToggleView.SetState(p_flag: false);
					break;
				}
				if (dRLToggleView.toggle.isOn)
				{
					RCI.SetupTransmitterSettings();
				}
				else
				{
					RCI.SetupGamepadSettings();
				}
				base.app.model.storage.state.player.profile.usingTransmitterAdapter = dRLToggleView.toggle.isOn;
				break;
			}
			}
		}

		private void Setup()
		{
			view.Setup();
			if (RCI.ControllersConnectedCount() == 0)
			{
				view.noHardwareHelpButton.SetActive(value: true);
			}
		}

		private void SaveAutoCalibrationData()
		{
			if (view.CalibrationData != null)
			{
				Debug.Log("Calibration UI: auto calibration save.");
				RCI.SetActiveControllerFromIndex(view.CalibrationData);
			}
		}

		private void OnDropdownSelectionChange()
		{
			if (view.controllerSelectionDropdown.index == 0)
			{
				RCI.SetActiveController(null);
			}
			else
			{
				foreach (RCI.Controller controller in RCI.GetControllers())
				{
					if (controller.guid == view.controllerHardwareGUIDs[view.controllerSelectionDropdown.index])
					{
						RCI.SetActiveController(controller);
						break;
					}
				}
			}
			view.OnRefreshCalibrationOptions(view.controllerSelectionDropdown.index);
		}
	}
}

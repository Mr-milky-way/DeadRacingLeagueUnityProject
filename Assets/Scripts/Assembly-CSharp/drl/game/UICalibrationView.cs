using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UICalibrationView : UIScreenView
	{
		[Header("UI Drone:")]
		public GameObject uiDroneOverlay;

		public RectTransform uiDroneOverlayHolder;

		public CanvasGroup droneLoadingMessage;

		private UIDroneOverlay m_droneOverlay;

		[Header("UI Controller:")]
		public GameObject uiControllerOverlay;

		public RectTransform uiControllerOverlayHolder;

		private UIControllerOverlay m_controllerOverlay;

		[Header("Calibration options:")]
		public UIElementView autoCalibrationButton;

		public UIElementView manualCalibrationButton;

		public UIElementView fineTuneButton;

		public UIElementView channelSelectionView;

		public DRLToggleView transmitterSettingsToggle;

		public DRLDropdownView controllerSelectionDropdown;

		public GameObject noHardwareHelpButton;

		public GameObject calibrationOptions;

		public Text calibrationDescription;

		public Text noHardwareDescription;

		[Header("Panels:")]
		public UIScreen calibrationMenuPanel;

		public UIScreen autoCalibrationPanel;

		public UIScreen manualCalibrationStepsPanel;

		public UIScreen channelSelectionPanel;

		public UIScreen tuningPanel;

		[Header("Auto Calibration:")]
		public UICalibrationStepsController uiAutoCalibration;

		[Header("Manual Calibration:")]
		public UIChannelSelectionView uiChannelSelection;

		public UICalibrationStepsController uiManualCalibration;

		[Header("Trim Calibration:")]
		public UITrimCalibrationView uiTrimCalibrationView;

		[Header("Buttons:")]
		public GameObject exitButton;

		public GameObject helpButton;

		public GameObject calibNextButton;

		public GameObject calibBackButton;

		public GameObject calibMenuButton;

		public GameObject channelsSaveButton;

		public GameObject manualCalibSaveButton;

		public GameObject calibrateStartButton;

		public GameObject channelSelectionNextButton;

		public GameObject calibSkipButton;

		public GameObject finishButton;

		public FadeComponent controllerSelectionButton;

		public GameObject controllerSelectionSpace;

		[HideInInspector]
		public CalibrationData CalibrationData;

		private SimpleDroneAnimator droneAnimator;

		[HideInInspector]
		public Dictionary<int, string> controllerHardwareGUIDs = new Dictionary<int, string>();

		private UIControllerAnimationType prevAnimation;

		public void Setup()
		{
			this.TimerRunOnce(delegate
			{
				if (base.current)
				{
					UINavigation.Focus(exitButton.GetComponent<UINavigation>());
				}
			}, 0.5f);
			InstantiateControllerOverlayPrefab();
			SpawnDrone();
			SetControllerAnimation(UIControllerAnimationType.UserInput);
			OnRefreshCalibrationOptions(-1);
			UseRCChannels();
		}

		public void OnRefreshCalibrationOptions(int selectedController, string selectedName = "", bool disconnected = false)
		{
			int num = RCI.ControllersConnectedCount();
			controllerSelectionDropdown.options.Clear();
			Dropdown.OptionData item = new Dropdown.OptionData(base.app.model.storage.locale.Get("calibration.main.selection", "NONE"));
			controllerSelectionDropdown.options.Add(item);
			controllerHardwareGUIDs.Clear();
			for (int i = 0; i < num; i++)
			{
				string text = RCI.GetHardwareName(i).ToUpper();
				controllerHardwareGUIDs.Add(i + 1, RCI.GetControllerGUID(i));
				text = ((!(text == RCI.GetSimplifiedControllerName(i).ToUpper())) ? RCI.GetSimplifiedControllerName(i).ToUpper() : RCI.GetJoystickName(i));
				if (!string.IsNullOrEmpty(text))
				{
					Dropdown.OptionData p_option = new Dropdown.OptionData(text);
					controllerSelectionDropdown.Add(p_option);
				}
			}
			if (selectedController > 0 && selectedController < RCI.ControllersConnectedCount())
			{
				controllerSelectionDropdown.Select(selectedController);
			}
			else if (selectedController < 0 && RCI.HasAssignedController)
			{
				string controllerGUID = RCI.GetControllerGUID();
				foreach (KeyValuePair<int, string> controllerHardwareGUID in controllerHardwareGUIDs)
				{
					if (controllerHardwareGUID.Value == controllerGUID)
					{
						controllerSelectionDropdown.Select(controllerHardwareGUID.Key);
						break;
					}
				}
			}
			if (!RCI.HasAssignedController || RCI.ControllersConnectedCount() == 0)
			{
				controllerSelectionDropdown.Select(0);
			}
			autoCalibrationButton.interactable = RCI.HasAssignedController && controllerSelectionDropdown.index != 0;
			manualCalibrationButton.interactable = RCI.HasAssignedController && controllerSelectionDropdown.index != 0;
			fineTuneButton.interactable = RCI.HasSavedProfile() && controllerSelectionDropdown.index != 0;
			channelSelectionView.interactable = RCI.HasSavedProfile() && controllerSelectionDropdown.index != 0;
			if (RCI.HasSavedProfile())
			{
				UseRCChannels();
			}
			SetControllerType();
			controllerSelectionDropdown.Refresh();
		}

		public void EnableCalibrationOptions()
		{
			autoCalibrationButton.interactable = true;
			manualCalibrationButton.interactable = true;
			controllerSelectionDropdown.interactable = true;
			fineTuneButton.interactable = true;
			channelSelectionView.interactable = true;
			controllerSelectionButton.alpha = 1f;
		}

		public void DisableCalibrationOptions()
		{
			autoCalibrationButton.interactable = false;
			manualCalibrationButton.interactable = false;
			fineTuneButton.interactable = false;
			channelSelectionView.interactable = false;
			controllerSelectionDropdown.interactable = false;
			controllerSelectionButton.alpha = 0.25f;
		}

		public void SetTuningOptionActive(bool p_active = true)
		{
			fineTuneButton.interactable = p_active;
			channelSelectionView.interactable = p_active;
		}

		public void OpenCalibrationMenuPanel(float p_duration = 0f)
		{
			calibrationMenuPanel.gameObject.SetActive(value: true);
			controllerSelectionSpace.SetActive(value: true);
			calibrationMenuPanel.Show(p_duration);
			controllerSelectionButton.gameObject.SetActive(value: true);
			controllerSelectionButton.FadeIn(p_duration);
			autoCalibrationPanel.Hide(p_duration);
			manualCalibrationStepsPanel.Hide(p_duration);
			tuningPanel.Hide(p_duration);
			channelSelectionPanel.Hide(p_duration);
			calibBackButton.SetActive(value: false);
			calibNextButton.SetActive(value: false);
			calibrateStartButton.SetActive(value: false);
			calibMenuButton.SetActive(value: false);
			calibSkipButton.SetActive(value: false);
			channelSelectionNextButton.SetActive(value: false);
			channelsSaveButton.SetActive(value: false);
			manualCalibSaveButton.SetActive(value: false);
			exitButton.SetActive(value: true);
			finishButton.SetActive(value: true);
			helpButton.SetActive(value: true);
			if ((bool)m_controllerOverlay)
			{
				m_controllerOverlay.HideArrows(0.01f);
			}
			this.TimerRunOnce(delegate
			{
				autoCalibrationPanel.gameObject.SetActive(value: false);
				manualCalibrationStepsPanel.gameObject.SetActive(value: false);
				tuningPanel.gameObject.SetActive(value: false);
				channelSelectionPanel.gameObject.SetActive(value: false);
				if (base.current)
				{
					UINavigation.Focus(exitButton.transform);
				}
			}, p_duration);
			if (uiAutoCalibration.inProgress)
			{
				uiAutoCalibration.StopAll();
				uiAutoCalibration.inProgress = false;
			}
			if (uiManualCalibration.inProgress)
			{
				uiManualCalibration.StopAll();
				uiManualCalibration.inProgress = false;
			}
			CalibrationData = new CalibrationData();
			UseRCChannels();
			SetControllerAnimation(UIControllerAnimationType.UserInput);
		}

		public override string BackButtonPressedEvent()
		{
			if (!calibBackButton.activeInHierarchy && autoCalibrationPanel.gameObject.activeInHierarchy)
			{
				return string.Empty;
			}
			m_controllerOverlay.cameraAnimation.Reset();
			if (calibMenuButton.activeInHierarchy || autoCalibrationPanel.gameObject.activeInHierarchy)
			{
				return "input.calibration-menu-panel.open@click";
			}
			return base.BackButtonPressedEvent();
		}

		public void OpenAutoCalibrationPanel(float p_duration = 0f, bool p_startAutoCalibration = true)
		{
			calibrationMenuPanel.Hide(p_duration);
			controllerSelectionButton.FadeOut(p_duration);
			controllerSelectionButton.gameObject.SetActive(value: false);
			controllerSelectionSpace.SetActive(value: false);
			autoCalibrationPanel.gameObject.SetActive(value: true);
			autoCalibrationPanel.Show(p_duration);
			this.TimerRunOnce(delegate
			{
				calibrationMenuPanel.gameObject.SetActive(value: false);
				UINavigation.Focus(calibrateStartButton.transform);
			}, p_duration);
			exitButton.SetActive(value: false);
			finishButton.SetActive(value: false);
			helpButton.SetActive(value: false);
			calibrateStartButton.SetActive(value: true);
			calibBackButton.SetActive(value: true);
			if (p_startAutoCalibration && uiAutoCalibration.steps.Count != 0)
			{
				if (uiAutoCalibration.inProgress)
				{
					uiAutoCalibration.GetCurrentStep().StopStep();
					uiAutoCalibration.inProgress = false;
				}
				CalibrationData = new CalibrationData();
				uiAutoCalibration.StartCalibration(CalibrationData);
			}
		}

		public void OpenManualCalibrationSteps(float p_duration = 0f, bool p_startManualCalibration = true)
		{
			calibrationMenuPanel.Hide(p_duration);
			controllerSelectionButton.FadeOut(p_duration);
			controllerSelectionButton.gameObject.SetActive(value: false);
			controllerSelectionSpace.SetActive(value: false);
			manualCalibrationStepsPanel.gameObject.SetActive(value: true);
			manualCalibrationStepsPanel.Show(p_duration);
			exitButton.gameObject.SetActive(value: false);
			finishButton.SetActive(value: false);
			helpButton.gameObject.SetActive(value: false);
			calibrateStartButton.SetActive(value: true);
			calibBackButton.gameObject.SetActive(value: true);
			this.TimerRunOnce(delegate
			{
				calibrationMenuPanel.gameObject.SetActive(value: false);
				UINavigation.Focus(calibrateStartButton.transform);
			}, p_duration);
			if (p_startManualCalibration && uiManualCalibration.steps.Count != 0)
			{
				CalibrationData = new CalibrationData();
				uiManualCalibration.StartCalibration(CalibrationData);
			}
		}

		public void OpenChannelSelectionPanel(float p_duration = 0f, bool p_useSavedProfile = false)
		{
			Localization.instance.Refresh();
			channelSelectionPanel.gameObject.SetActive(value: true);
			manualCalibrationStepsPanel.Hide(p_duration);
			calibrationMenuPanel.Hide(p_duration);
			controllerSelectionButton.FadeOut(p_duration);
			channelSelectionPanel.Show(p_duration);
			calibBackButton.SetActive(value: false);
			helpButton.SetActive(value: false);
			finishButton.SetActive(value: false);
			exitButton.SetActive(value: false);
			calibNextButton.SetActive(value: false);
			calibSkipButton.SetActive(value: false);
			calibrateStartButton.SetActive(value: false);
			calibMenuButton.SetActive(value: true);
			channelsSaveButton.SetActive(value: false);
			this.TimerRunOnce(delegate
			{
				calibrationMenuPanel.gameObject.SetActive(value: false);
				manualCalibrationStepsPanel.gameObject.SetActive(value: false);
				controllerSelectionButton.gameObject.SetActive(value: false);
				controllerSelectionSpace.SetActive(value: false);
				UINavigation.Focus(calibMenuButton.transform);
			}, p_duration);
			uiChannelSelection.Setup(p_useSavedProfile ? null : CalibrationData);
			SetControllerAnimation(UIControllerAnimationType.UserInput);
		}

		public void OpenChannelSelectionPanel(CalibrationData p_data = null, float p_duration = 0f)
		{
			Localization.instance.Refresh();
			channelSelectionPanel.gameObject.SetActive(value: true);
			tuningPanel.Hide(p_duration);
			channelSelectionPanel.Show(p_duration);
			manualCalibSaveButton.SetActive(value: false);
			calibrateStartButton.SetActive(value: false);
			calibMenuButton.SetActive(value: true);
			channelsSaveButton.SetActive(value: true);
			this.TimerRunOnce(delegate
			{
				tuningPanel.gameObject.SetActive(value: false);
				UINavigation.Focus(channelsSaveButton.transform);
			}, p_duration);
			uiChannelSelection.Setup(p_data);
			SetControllerAnimation(UIControllerAnimationType.UserInput);
		}

		public void OpenTrimPanel(float p_duration = 0f, CalibrationData p_data = null)
		{
			tuningPanel.gameObject.SetActive(value: true);
			calibrationMenuPanel.Hide(p_duration);
			controllerSelectionButton.FadeOut(p_duration);
			controllerSelectionButton.gameObject.SetActive(value: false);
			controllerSelectionSpace.SetActive(value: false);
			channelSelectionPanel.Hide(p_duration);
			tuningPanel.Show(p_duration);
			calibMenuButton.SetActive(value: true);
			manualCalibSaveButton.SetActive(value: true);
			finishButton.SetActive(value: false);
			exitButton.SetActive(value: false);
			helpButton.SetActive(value: false);
			channelSelectionNextButton.SetActive(value: false);
			this.TimerRunOnce(delegate
			{
				DRLUINavigationSystem.controllerEnabled = true;
				DRLUINavigationSystem.controllerNavEnabled = true;
				UINavigation.Focus(calibMenuButton.transform);
			}, 0.3f);
			this.TimerRunOnce(delegate
			{
				calibrationMenuPanel.gameObject.SetActive(value: false);
				channelSelectionPanel.gameObject.SetActive(value: false);
			}, p_duration);
			if (p_data != null)
			{
				uiTrimCalibrationView.Setup(p_data);
			}
			else
			{
				uiTrimCalibrationView.Setup();
			}
			SetControllerAnimation(UIControllerAnimationType.UserInput);
			UseRCChannels();
		}

		public void CalibrationNextStep()
		{
			if (!uiAutoCalibration.inProgress && !uiManualCalibration.inProgress)
			{
				SetControllerAnimation(UIControllerAnimationType.UserInput);
				return;
			}
			UICalibrationStepsController uICalibrationStepsController = (uiAutoCalibration.inProgress ? uiAutoCalibration : uiManualCalibration);
			UICalibrationStep uICalibrationStep = uICalibrationStepsController.NextStep();
			if (uICalibrationStep == null)
			{
				this.TimerRunOnce(delegate
				{
					SetControllerAnimation(UIControllerAnimationType.UserInput);
				}, 0.6f);
				if (uICalibrationStepsController == uiAutoCalibration)
				{
					autoCalibrationPanel.Hide(0f);
					tuningPanel.gameObject.SetActive(value: true);
					tuningPanel.Show(0f);
					calibMenuButton.SetActive(value: true);
					manualCalibSaveButton.SetActive(value: true);
					calibBackButton.SetActive(value: false);
					calibNextButton.SetActive(value: false);
					calibSkipButton.SetActive(value: false);
					calibrateStartButton.SetActive(value: false);
					finishButton.SetActive(value: false);
					exitButton.gameObject.SetActive(value: false);
					helpButton.gameObject.SetActive(value: false);
					uiTrimCalibrationView.Setup(CalibrationData);
					UseRCChannels(CalibrationData.ElementIDs);
					this.TimerRunOnce(delegate
					{
						UINavigation.Focus(calibMenuButton.transform);
					}, 0.61f);
					this.TimerRunOnce(delegate
					{
						autoCalibrationPanel.gameObject.SetActive(value: false);
						SetControllerAnimation(UIControllerAnimationType.UserInput);
					});
				}
				else
				{
					UseRCChannels(null);
					OpenChannelSelectionPanel(0f, false);
				}
				uICalibrationStepsController.StopAll();
				if (m_controllerOverlay != null)
				{
					m_controllerOverlay.HideArrows(0.01f);
				}
			}
			else
			{
				calibNextButton.SetActive(value: false);
				if (uICalibrationStep.step == CalibrationSteps.AxisChannelDetection && (uICalibrationStep.axis == RawAxis.ToggleA || uICalibrationStep.axis == RawAxis.ToggleB))
				{
					calibSkipButton.SetActive(value: true);
				}
				SetControllerAnimation(UIControllerAnimationType.UserInput);
				CalibrationStartStep();
			}
		}

		public void SetToggleSkip()
		{
			UICalibrationStep currentStep = (uiAutoCalibration.inProgress ? uiAutoCalibration : uiManualCalibration).GetCurrentStep();
			if (currentStep != null && currentStep.step == CalibrationSteps.AxisChannelDetection && (currentStep.axis == RawAxis.ToggleA || currentStep.axis == RawAxis.ToggleB) && CalibrationData != null)
			{
				CalibrationData.ElementIDs[currentStep.axis] = -1;
			}
		}

		public void CalibrationStartStep()
		{
			if (uiAutoCalibration.inProgress || uiManualCalibration.inProgress)
			{
				UICalibrationStepsController obj = (uiAutoCalibration.inProgress ? uiAutoCalibration : uiManualCalibration);
				UICalibrationStep currentStep = obj.GetCurrentStep();
				if (currentStep != null)
				{
					CalibrationAnimateController(currentStep);
				}
				obj.StartStep();
				calibrateStartButton.SetActive(value: false);
				calibBackButton.SetActive(value: false);
			}
		}

		public void CalibrationPreviousStep()
		{
			if (!uiAutoCalibration.inProgress && !uiManualCalibration.inProgress)
			{
				return;
			}
			UICalibrationStepsController uICalibrationStepsController = (uiAutoCalibration.inProgress ? uiAutoCalibration : uiManualCalibration);
			UICalibrationStep uICalibrationStep = uICalibrationStepsController.PreviousStep();
			SetControllerAnimation(UIControllerAnimationType.StopAll);
			if (uICalibrationStep == null)
			{
				OpenCalibrationMenuPanel();
				uICalibrationStepsController.GetCurrentStep().StopStep();
				return;
			}
			if (uICalibrationStep.step == CalibrationSteps.CenterPointsDetection)
			{
				calibrateStartButton.SetActive(value: true);
				calibBackButton.SetActive(value: true);
				return;
			}
			if (uICalibrationStep.axis != RawAxis.ToggleA || uICalibrationStep.axis != RawAxis.ToggleB)
			{
				calibSkipButton.SetActive(value: false);
			}
			CalibrationStartStep();
		}

		public void CalibrationStepComplete()
		{
			calibSkipButton.SetActive(value: false);
			calibrateStartButton.SetActive(value: false);
			SetControllerAnimation(UIControllerAnimationType.StopAll);
			this.TimerRunOnce(delegate
			{
				CalibrationNextStep();
			}, 0.5f);
		}

		public void CalibrationStepFailed()
		{
			if (uiAutoCalibration.inProgress || uiManualCalibration.inProgress)
			{
				calibrateStartButton.SetActive(value: true);
				calibBackButton.SetActive(value: true);
				uiManualCalibration.StopAll();
				SetControllerAnimation(UIControllerAnimationType.StopAll);
			}
		}

		public void CalibrationAnimateController(UICalibrationStep p_step)
		{
			if (p_step == null || m_controllerOverlay == null)
			{
				return;
			}
			SetControllerAnimation(UIControllerAnimationType.StopAll);
			m_controllerOverlay.HideArrows(0.1f);
			this.TimerRunOnce(delegate
			{
				switch (p_step.step)
				{
				case CalibrationSteps.CenterPointsDetection:
				case CalibrationSteps.CenterPause:
					SetControllerAnimationInward(0.3f);
					break;
				case CalibrationSteps.ChannelFiltering:
					SetControllerAnimationAll(0.3f);
					break;
				case CalibrationSteps.AxisChannelDetection:
					switch (p_step.axis)
					{
					case RawAxis.LeftStickY:
						SetControllerAnimation((p_step.direction == UICalibrationStep.Direction.Forward) ? UIControllerAnimationType.LeftStickUp : UIControllerAnimationType.LeftStickDown);
						break;
					case RawAxis.LeftStickX:
						SetControllerAnimation((p_step.direction == UICalibrationStep.Direction.Forward) ? UIControllerAnimationType.LeftStickRight : UIControllerAnimationType.LeftStickLeft);
						break;
					case RawAxis.RightStickY:
						SetControllerAnimation((p_step.direction == UICalibrationStep.Direction.Forward) ? UIControllerAnimationType.RightStickUp : UIControllerAnimationType.RightStickDown);
						break;
					case RawAxis.RightStickX:
						SetControllerAnimation((p_step.direction == UICalibrationStep.Direction.Forward) ? UIControllerAnimationType.RightStickRight : UIControllerAnimationType.RightStickLeft);
						break;
					case RawAxis.ToggleA:
						m_controllerOverlay.cameraAnimation.Reset();
						if (RCI.GetControllerStateType(ControllerStateType.Taranis) == ControllerStateType.Taranis)
						{
							SetControllerAnimation(UIControllerAnimationType.LeftToggle);
						}
						else
						{
							m_controllerOverlay.ShowLeftToggleArrows(0.3f);
						}
						break;
					case RawAxis.ToggleB:
						m_controllerOverlay.cameraAnimation.Reset();
						if (RCI.GetControllerStateType(ControllerStateType.Taranis) == ControllerStateType.Taranis)
						{
							m_controllerOverlay.SetAnimation(UIControllerAnimationType.RightToggle);
						}
						else
						{
							m_controllerOverlay.ShowRightToggleArrows(0.3f);
						}
						break;
					}
					break;
				case CalibrationSteps.MaxAxisRange:
				case CalibrationSteps.MinAxisRange:
					break;
				}
			}, 0.6f);
		}

		public void OnCalibrationComplete()
		{
			((Component)this).ActivityRun((Func<bool>)delegate
			{
				if (RCI.HasSavedProfile())
				{
					fineTuneButton.interactable = RCI.HasSavedProfile();
					channelSelectionView.interactable = RCI.HasSavedProfile();
					Notify("calibration.save.complete");
					UseRCChannels();
					CalibrationData = new CalibrationData();
					return false;
				}
				return true;
			}, 0f);
		}

		public void CheckCalibrationInProgress()
		{
			UICalibrationStepsController uICalibrationStepsController = null;
			if (uiAutoCalibration.inProgress)
			{
				uICalibrationStepsController = uiAutoCalibration;
			}
			if (uiManualCalibration.inProgress)
			{
				uICalibrationStepsController = uiManualCalibration;
			}
			if (!(uICalibrationStepsController == null) && (RCI.ControllersConnectedCount() == 0 || RCI.GetActiveJoystick() == null))
			{
				uICalibrationStepsController.StopAll();
				uICalibrationStepsController.inProgress = false;
				OpenCalibrationMenuPanel();
			}
		}

		private void SpawnDrone()
		{
			if (uiDroneOverlay == null)
			{
				Debug.LogError("UICalibrationView> uiDrone prefab not assigned");
				return;
			}
			if (m_droneOverlay != null && base.validContext)
			{
				UnityEngine.Object.Destroy(m_droneOverlay.gameObject);
			}
			droneLoadingMessage.alpha = 1f;
			droneLoadingMessage.gameObject.SetActive(value: true);
			m_droneOverlay = UnityEngine.Object.Instantiate(uiDroneOverlay).GetComponent<UIDroneOverlay>();
			m_droneOverlay.name = "ui-drone-overlay";
			if (base.app.model.storage.state.player.garage.currentRigData != null)
			{
				m_droneOverlay.rig = base.app.model.storage.state.player.garage.currentRigData;
			}
			if (base.app.model.game != null && base.app.model.game.playerDrone != null)
			{
				m_droneOverlay.rig = base.app.model.game.playerDrone.rig;
			}
			RectTransform component = m_droneOverlay.GetComponent<RectTransform>();
			component.SetParent(uiDroneOverlayHolder);
			component.offsetMin = Vector2.zero;
			component.offsetMax = Vector2.zero;
			component.localScale = Vector3.one;
			m_droneOverlay.renderCanvas = GetComponent<Canvas>();
			((Component)this).ActivityRun((Func<bool>)delegate
			{
				if (m_droneOverlay.fade.alpha > 0.99f)
				{
					droneLoadingMessage.alpha = 0f;
					droneLoadingMessage.gameObject.SetActive(value: false);
					m_droneOverlay.drone.SetMotorRPM(1500f);
					droneAnimator = m_droneOverlay.GetComponent<SimpleDroneAnimator>();
					droneAnimator.Init();
					return false;
				}
				droneLoadingMessage.alpha = 1f - m_droneOverlay.fade.alpha;
				return true;
			}, 0.01f);
		}

		private void InstantiateControllerOverlayPrefab()
		{
			if (uiControllerOverlay == null)
			{
				Debug.LogError("UICalibrationView> uiController prefab not assigned");
			}
			else if (!(m_controllerOverlay != null))
			{
				RectTransform rectTransform = (RectTransform)UnityEngine.Object.Instantiate(uiControllerOverlay, uiControllerOverlayHolder).transform;
				m_controllerOverlay = rectTransform.GetComponent<UIControllerOverlay>();
				m_controllerOverlay.fade.FadeIn();
				m_controllerOverlay.SetAnimation(UIControllerAnimationType.UserInput);
				m_controllerOverlay.HideArrows(0f);
			}
		}

		public void DestroyModels()
		{
			if (m_droneOverlay != null)
			{
				droneLoadingMessage.alpha = 0f;
				droneLoadingMessage.gameObject.SetActive(value: false);
			}
		}

		public void SetControllerAnimation(UIControllerAnimationType p_animation)
		{
			if (!(m_controllerOverlay == null))
			{
				m_controllerOverlay.SetAnimation(p_animation);
				bool num = p_animation == UIControllerAnimationType.LeftStickUp || p_animation == UIControllerAnimationType.LeftStickRight || p_animation == UIControllerAnimationType.RightStickUp || p_animation == UIControllerAnimationType.RightStickRight;
				bool flag = p_animation == UIControllerAnimationType.LeftStickUp || p_animation == UIControllerAnimationType.LeftStickRight;
				if ((flag && (prevAnimation == UIControllerAnimationType.LeftStickUp || prevAnimation == UIControllerAnimationType.LeftStickRight)) || (!flag && (prevAnimation == UIControllerAnimationType.RightStickUp || prevAnimation == UIControllerAnimationType.RightStickRight)))
				{
					m_controllerOverlay.cameraAnimation.Reset();
				}
				if (num)
				{
					m_controllerOverlay.cameraAnimation.Animate(flag);
				}
			}
		}

		public void SetControllerAnimationInward(float p_duration = 5f)
		{
			if (!(m_controllerOverlay == null))
			{
				m_controllerOverlay.cameraAnimation.Reset();
				m_controllerOverlay.ShowInwardDirectionArrows(show: true, p_duration);
			}
		}

		public void SetControllerAnimationAll(float p_duration = 5f)
		{
			if (!(m_controllerOverlay == null))
			{
				m_controllerOverlay.cameraAnimation.Reset();
				m_controllerOverlay.ShowRotationArrows(p_duration);
				m_controllerOverlay.AnimateSticksRotating();
			}
		}

		public void SetControllerType()
		{
			if (!(m_controllerOverlay == null))
			{
				m_controllerOverlay.SetController(RCI.GetControllerStateType(ControllerStateType.Keyboard));
			}
		}

		public void UpdateChannelData(CalibrationData data)
		{
			if (data != null)
			{
				if ((bool)m_controllerOverlay)
				{
					m_controllerOverlay.UpdateChannelData(data);
				}
				if ((bool)droneAnimator)
				{
					droneAnimator.UpdateChannelData(data);
				}
			}
		}

		public void UpdateChannelData(RawAxis axis, CalibrationData data)
		{
			if (data != null)
			{
				if ((bool)m_controllerOverlay)
				{
					m_controllerOverlay.UpdateChannelData(axis, data);
				}
				if ((bool)droneAnimator)
				{
					droneAnimator.UpdateChannelData(axis, data);
				}
			}
		}

		public void UpdateChannelData(RawAxis axis, bool invert)
		{
			if ((bool)m_controllerOverlay)
			{
				m_controllerOverlay.UpdateInvert(axis, invert);
			}
			if ((bool)droneAnimator)
			{
				droneAnimator.UpdateInvert(axis, invert);
			}
		}

		public void ResetUIOverlay()
		{
			if (!(m_controllerOverlay == null) && !(droneAnimator == null))
			{
				m_controllerOverlay.ResetChannelData();
				droneAnimator.ResetChannelData();
				m_controllerOverlay.UseRawAxis();
				droneAnimator.UseRawAxis();
			}
		}

		public void ResetChannelData()
		{
			if (!(m_controllerOverlay == null) && !(droneAnimator == null))
			{
				m_controllerOverlay.ResetChannelData();
				droneAnimator.ResetChannelData();
			}
		}

		public void UseRCChannels(Dictionary<RawAxis, int> p_channels)
		{
			if ((bool)m_controllerOverlay)
			{
				m_controllerOverlay.UseRCChannels(p_channels);
			}
			if ((bool)droneAnimator)
			{
				droneAnimator.UseRCChannels(p_channels);
			}
		}

		public void UseRCChannels()
		{
			if ((bool)m_controllerOverlay)
			{
				m_controllerOverlay.UseRCChannels();
			}
			if ((bool)droneAnimator)
			{
				droneAnimator.UseRCChannels();
			}
		}
	}
}

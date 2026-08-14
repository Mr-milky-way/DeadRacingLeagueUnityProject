using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIDroneDashboardView : UIScreenView
	{
		[Header("Columns")]
		public FadeComponent columnGeneral;

		public FadeComponent columnPhysics;

		public FadeComponent columnDrone;

		public FadeComponent columnFc;

		[Header("Navigation Roots")]
		public Transform[] contentNodes;

		public UINavigation[] headersNavigation;

		public Transform[] bodyNodes;

		[Header("Tabs")]
		public DRLToggleView tabGeneral;

		public DRLToggleView tabPhysics;

		public DRLToggleView tabDrone;

		public DRLToggleView tabFc;

		public DRLToggleView subtabPhysicsOptions;

		public DRLToggleView subtabPhysicsEfficiency;

		public DRLToggleView subtabPhysicsDrag;

		public DRLToggleView subtabPhysicsGroundEffect;

		public DRLToggleView subtabPhysicsPropDrag;

		public DRLToggleView subtabDroneDrones;

		public DRLToggleView subtabDroneSpecs;

		public DRLToggleView subtabDroneBattery;

		public DRLToggleView subtabFcPID;

		public DRLToggleView subtabFcRates;

		public DRLToggleView subtabFcDelay;

		public DRLToggleView subtabFcExtras;

		[Header("Toggles")]
		public DRLToggleView toggleReplay;

		public DRLToggleView toggleReplayRecord;

		public DRLToggleView toggleReplayPlay;

		public DRLToggleView toggleReplayRun;

		public DRLToggleView toggleAudio;

		public DRLToggleView toggleController;

		public DRLToggleView toggleBatteryDrain;

		public DRLToggleView toggleBatterySag;

		public DRLToggleView toggleFcModePro;

		public DRLToggleView toggleFcModeInter;

		public DRLToggleView toggleFcModeNoob;

		public DRLToggleView togglePidAutotune;

		public DRLToggleView togglePidDebug;

		public DRLToggleView toggleGraphMotor;

		public DRLToggleView toggleGraphThrottle;

		public DRLToggleView toggleGraphPitchroll;

		public DRLToggleView toggleGraphYaw;

		public DRLToggleView toggleGraphEfficiency;

		public DRLToggleView toggleGraphForce;

		public DRLToggleView toggleGraphSpeed;

		public DRLToggleView toggleGraphElectric;

		public DRLToggleView togglePhysThreaded;

		public DRLToggleView togglePhysRealparams;

		public DRLToggleView togglePhysEfficiencyCurve;

		public DRLToggleView togglePhysTorqueboost;

		public DRLToggleView togglePhysPropBreaking;

		public DRLToggleView togglePhysPropwash;

		public DRLToggleView toggleGatechCrossflow;

		public DRLToggleView toggleGatechUnsteady;

		public DRLToggleView toggleGatechShedding;

		public DRLToggleView toggleRealCOG;

		public DRLToggleView toggleAirmode;

		public DRLToggleView toggleAntigravity;

		public DRLToggleView toggleDynamicFilter;

		public DRLToggleView toggleITermRotation;

		public DRLToggleView toggleSmartFeedForward;

		[Header("Buttons")]
		public UIElementView buttonPreferencesLoad;

		public UIElementView buttonPreferencesSave;

		public UIElementView buttonPreferencesClear;

		public UIElementView buttonFlip;

		public UIElementView buttonRecharge;

		public UIElementView buttonLinkHelp;

		public UIElementView buttonGarageEdit;

		public UIElementView buttonGarageCreate;

		public UIElementView buttonDroneSpecsReset;

		public UIElementView buttonFcRates;

		[Header("Inputs")]
		public DRLInputFieldView inputEfficiencyOverride;

		public DRLInputFieldView inputEfficiencyMax;

		public DRLInputFieldView inputEfficiencyZero;

		public DRLInputFieldView inputDragScaleD;

		public DRLInputFieldView inputDragScaleL;

		public DRLInputFieldView inputDragScaleS;

		public DRLInputFieldView inputDragMaxX;

		public DRLInputFieldView inputDragMaxY;

		public DRLInputFieldView inputDragMaxZ;

		public DRLInputFieldView inputDragCd;

		public DRLInputFieldView inputDragClMax;

		public DRLInputFieldView inputDragCdMax;

		public DRLInputFieldView inputDragClMin;

		public DRLInputFieldView inputDragCdMin;

		public DRLInputFieldView inputDragSurface;

		public DRLInputFieldView inputDragSlipThreshold;

		public DRLInputFieldView inputDragSlippery;

		public DRLInputFieldView inputDragDynamicDrag;

		public DRLInputFieldView inputDragDynamicLift;

		public DRLInputFieldView inputDroneWeight;

		public DRLInputFieldView inputDroneThrust;

		public DRLInputFieldView inputDroneTorque;

		public DRLInputFieldView inputGroundEffectStrength;

		public DRLInputFieldView inputGroundEffectDistance;

		public DRLInputFieldView inputGravity;

		public DRLInputFieldView inputAirDensity;

		public DRLInputFieldView inputWindX;

		public DRLInputFieldView inputWindY;

		public DRLInputFieldView inputWindZ;

		public DRLInputFieldView inputGravityFactor;

		public DRLInputFieldView inputPidPitchP;

		public DRLInputFieldView inputPidPitchI;

		public DRLInputFieldView inputPidPitchD;

		public DRLInputFieldView inputPidRollP;

		public DRLInputFieldView inputPidRollI;

		public DRLInputFieldView inputPidRollD;

		public DRLInputFieldView inputPidYawP;

		public DRLInputFieldView inputPidYawI;

		public DRLInputFieldView inputPidYawD;

		public DRLInputFieldView inputPidPitchFF;

		public DRLInputFieldView inputPidRollFF;

		public DRLInputFieldView inputPidYawFF;

		public DRLInputFieldView inputPidLevelP;

		public DRLInputFieldView inputPidLevelI;

		public DRLInputFieldView inputPidLevelD;

		public DRLInputFieldView inputLevelAngleLimit;

		public DRLInputFieldView inputLevelFFTransition;

		public DRLInputFieldView inputLevelITermRelaxValue;

		public DRLInputFieldView inputLevelAntigravityGain;

		public DRLInputFieldView inputPitchRC;

		public DRLInputFieldView inputPitchSuper;

		public DRLInputFieldView inputPitchExpo;

		public DRLInputFieldView inputRollRC;

		public DRLInputFieldView inputRollSuper;

		public DRLInputFieldView inputRollExpo;

		public DRLInputFieldView inputYawRC;

		public DRLInputFieldView inputYawSuper;

		public DRLInputFieldView inputYawExpo;

		public DRLInputFieldView inputThrottleMid;

		public DRLInputFieldView inputThrottleExpo;

		public DRLInputFieldView inputFcMinThrottle;

		public DRLInputFieldView inputDelaySpinup;

		public DRLInputFieldView inputDelaySpindown;

		public DRLInputFieldView inputPropTipSpeed;

		public DRLInputFieldView inputPropTipDrag;

		public DRLInputFieldView inputPropwashStrength;

		public DRLInputFieldView inputPropwashThreshold;

		public DRLInputFieldView inputBatteryCapacity;

		public DRLInputFieldView inputBatteryResistance;

		public DRLInputFieldView inputBatteryOverheat;

		public DRLInputFieldView inputReplayFilename;

		[Header("VS Disabled:")]
		public List<GameObject> vsDisabled = new List<GameObject>();

		public DRLInputFieldView inputDamageEnergy;

		public DRLInputFieldView inputCrashEnergy;

		public DRLInputFieldView inputCrashSpinout;

		public DRLInputFieldView inputCrashTransfer;

		public GameObject damageTiersTitle;

		public GameObject damageTiersInputGroup;

		public DRLInputFieldView inputDamageTier1;

		public DRLInputFieldView inputDamageTier2;

		public DRLInputFieldView inputDamageTier3;

		public GameObject speedTiersTitle;

		public GameObject speedTiersInputGroup;

		public DRLInputFieldView inputSpeedReductionTier1;

		public DRLInputFieldView inputSpeedReductionTier2;

		public DRLInputFieldView inputSpeedReductionTier3;

		public GameObject lineTiersTitle;

		public GameObject lineTiersInputGroup;

		public DRLInputFieldView inputLineDeviationTier1;

		public DRLInputFieldView inputLineDeviationTier2;

		public DRLInputFieldView inputLineDeviationTier3;

		public DRLInputFieldView inputPropSturdiness;

		public DRLInputFieldView inputArmSturdiness;

		public DRLInputFieldView inputBodySturdiness;

		public DRLInputFieldView inputDamageThreshold;

		[Header("Dummies")]
		public GameObject buttonRechargeDummy;

		[Header("Labels")]
		public GameObject labelEfficiencyOverride;

		public GameObject labelEfficiencyCurve;

		public Text labelDroneClass;

		public Text labelDroneRig;

		[Header("Steppers")]
		public DRLStepperView stepperCamera;

		public DRLStepperView stepperCameraTilt;

		public DRLStepperView stepperDroneClass;

		public DRLStepperView stepperDroneRig;

		public DRLStepperView stepperDragMode;

		public DRLStepperView stepperDragData;

		public DRLStepperView stepperBetaflightMode;

		public DRLStepperView stepperITermRelax;

		public DRLStepperView stepperITermRelaxType;

		public DRLStepperView stepperAntigravityMode;

		public DRLStepperView stepperBetaflightVersion;

		[Header("Tune Controls")]
		public DRLInputFieldView tuneName;

		public UIElementView tuneSave;

		public UIElementView tuneNew;

		public UIElementView tuneSavingFeedback;

		public DRLStepperView tuneRating;

		public FadeComponent[] tuneRatingStarFades;

		public UIElementView tuneCommunityManager;

		[NonSerialized]
		public UICommunityDronesController tunesManagerController;

		[Header("Misc")]
		public Graphic[] rigStepperGrayComponents;

		public Text tooltipText;

		public FadeComponent tooltipFade;

		private RectTransform tooltipRect;

		public Transform footer;

		public Image psSelectIcon;

		public Image psBackIcon;

		public Sprite psButtonX;

		public Sprite psButtonO;

		public void Tooltip(string p_text)
		{
			if (string.IsNullOrEmpty(p_text))
			{
				tooltipFade.FadeOut(0.15f);
				return;
			}
			if (tooltipRect == null)
			{
				tooltipRect = tooltipText.GetComponent<RectTransform>();
			}
			tooltipRect.anchoredPosition = new Vector2(0f, -35f);
			tooltipRect.sizeDelta = new Vector2(-40f, 70f);
			tooltipText.text = p_text;
			tooltipFade.FadeIn(0.15f);
		}
	}
}

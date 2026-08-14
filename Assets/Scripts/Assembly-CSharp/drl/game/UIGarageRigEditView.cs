using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.UI;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIGarageRigEditView : UIScreenView
	{
		public ListComponent listField;

		public DRLPagePickerView pageField;

		public UINavigation pagePreviousNav;

		public UINavigation pageNextNav;

		public UIElementView previousPageButton;

		public UIElementView nextPageButton;

		public DRLTabGroup tabGroup;

		public DRLInputFieldView rigNameField;

		public UIFieldErrorIndicator droneNameErr;

		public List<GameObject> applyStateList;

		public OrbitTransform orbit;

		public DRLOrbitWASDInput wasd;

		public OrbitFollowInput follow;

		public Transform droneContainer;

		public Transform dronePivot;

		public UIGarageCameraOffset cameraOffset;

		public UIGarageCOGMarker droneCOGMarker;

		public FadeComponent[] droneCOGFaders;

		public GameObject droneCOGAxes;

		public Transform droneCOGAxesParent;

		public FadeComponent droneCOGLayerFade;

		public UIElementView datasheetButton;

		public UIElementView videoButton;

		public RectTransform publicToggleIcon;

		public DRLStepperView publicToggle;

		public UIElementView clearPhysicsButton;

		public FadeComponent georgiaTechLabel;

		public UIElementView flyButtonElementView;

		public UIElementView saveButtonElementView;

		public UINavigation backButtonNavigation;

		public UINavigation saveButtonNav;

		public List<OrbitCustomSettings> orbitInitialOrientation;

		public List<OrbitConstraint> orbitConstraints;

		public DRLToggleView COGToggle;

		public DRLToggleView SpinToggle;

		public Transform viewerTransform;

		public Camera viewerCamera;

		public AkAudioListener viewerCameraListener;

		public FadeComponent backgroundFader;

		public FadeComponent pressAnyKeyMessage;

		public HideComponentsWhileDrag hideComponentWhileDrag;

		public FadeComponent enterDroneNameMessage;

		public List<GameObject> navigationHeader;

		public List<GameObject> xboxHeaderIcons;

		public List<GameObject> psHeaderIcons;

		public Image psSelectIcon;

		public Image psBackIcon;

		public Sprite psButtonX;

		public Sprite psButtonO;

		[HideInInspector]
		public bool unableToFly;

		[Header("Filters")]
		public DRLTextAssetStepperView filter0;

		public DRLTextAssetStepperView filter1;

		public DRLFilterAssetNames filterNames;

		[HideInInspector]
		public List<DroneAssetTagType> filter0list;

		[HideInInspector]
		public List<DroneAssetTagType> filter1list;

		[Header("Information")]
		public RawImage informationBrandImage;

		public AspectRatioFitter informationARBrandImage;

		public Text informationBrandText;

		public Text informationTitle;

		public Text[] informationCaption;

		public Text[] informationValue;

		public Text[] informationWide;

		public FadeComponent informationPanelFader;

		[Header("Panels")]
		public GameObject colorsPanel;

		public GameObject tilesPanel;

		public GameObject devAllowancePanel;

		public GameObject navigationXboxPanel;

		public GameObject navigationPSPanel;

		public Image navigationPSEnterButton;

		public Image navigationPSBackButton;

		[Header("Trails")]
		public GameObject trailsAnimationPrefab;

		public Vector3 trailsScale3inch;

		public Vector3 trailsScale4inch;

		public Vector3 trailsScale5inch;

		public Vector3 trailsScale6inch;

		[Header("Navigation")]
		public UINavigationLink headerDownProxy;

		public UINavigationLink headerUpProxy;

		public UINavigationLinkList gridUpProxy;

		public UINavigation[] filtersNavigation;

		public FadeComponent contentFade;

		public CanvasGroup screenCanvasGroup;

		[Header("Drone parts")]
		public GridLayoutGroup itemsGrid;

		public DRLScrollView scrollView;

		[Header("Colors and Specs")]
		public UIGarageColorsView colors;

		public UIGarageSpecsView specs;

		public UINavigation trailColorsNav;

		public UINavigation propsColorsNav;

		public UINavigation textureColorsNav;

		public UINavigation edgeColorsNav;

		[Header("Store")]
		public UINavigation storeCardNav;

		[HideInInspector]
		public Drone drone;

		[HideInInspector]
		public Drone thumbnailDrone;

		[HideInInspector]
		public Drone externalDrone;

		[HideInInspector]
		public bool usingExternalDrone;

		[HideInInspector]
		public GamePlayerData player;

		public bool showStoreOnOpen;

		public bool isOpenedFromStore;

		[HideInInspector]
		public DroneRigData data;

		[HideInInspector]
		public DroneRigData initRigData;

		private DroneRigData m_rawData;

		[HideInInspector]
		public bool openedFromPause;

		[HideInInspector]
		public bool openedFromDashboard;

		[HideInInspector]
		public bool openedFromRigSelection;

		[HideInInspector]
		public bool openedFromRigTemplateSelector;

		public Dictionary<Color, int> m_profileColorToIndex;

		public Dictionary<Color, int> m_partsColorToIndex;

		public Material activeSkybox;

		public Material garageSkybox;

		public DRLAsset currentProduct;

		private int mapSelectionIndex;

		public DRLTextAssetStepperView mapStepper;

		[HideInInspector]
		public DroneRigData rawData
		{
			get
			{
				if (m_rawData == null)
				{
					m_rawData = ScriptableObject.CreateInstance<DroneRigData>();
				}
				return m_rawData;
			}
			set
			{
				if (m_rawData == null)
				{
					m_rawData = ScriptableObject.CreateInstance<DroneRigData>();
				}
				m_rawData.antenna = ((value.antenna == "TX-000") ? "" : value.antenna);
				m_rawData.attachment0 = ((value.attachment0 == "AT-000") ? "" : value.attachment0);
				m_rawData.attachment1 = ((value.attachment1 == "AT-000") ? "" : value.attachment1);
				m_rawData.battery = ((value.battery == "B-000") ? "" : value.battery);
				m_rawData.camera = ((value.camera == "C-000") ? "" : value.camera);
				m_rawData.esc = ((value.esc == "E-000") ? "" : value.esc);
				m_rawData.fc = ((value.fc == "FC-000") ? "" : value.fc);
				m_rawData.frame = ((value.frame == "F-000") ? "" : value.frame);
				m_rawData.motor = ((value.motor == "M-000") ? "" : value.motor);
				m_rawData.physics = ((value.physics == "PH-000") ? "" : value.physics);
				m_rawData.prop = ((value.prop == "P-000") ? "" : value.prop);
				m_rawData.receiver = ((value.receiver == "RC-000") ? "" : value.receiver);
				m_rawData.skinFrame = ((value.skinFrame == "SK-000") ? "" : value.skinFrame);
				m_rawData.trail = ((value.trail == "TR-000") ? "" : value.trail);
			}
		}

		public string rigName
		{
			get
			{
				return rigNameField.field.text;
			}
			set
			{
				rigNameField.field.text = value;
			}
		}

		public int MapSelectionIndex
		{
			get
			{
				return mapSelectionIndex;
			}
			set
			{
				mapSelectionIndex = value;
				if (mapSelectionIndex > 1)
				{
					mapSelectionIndex = 0;
				}
			}
		}

		private void SetPostProcessing()
		{
			PostProcessingBehaviour component = viewerCamera.GetComponent<PostProcessingBehaviour>();
			if ((bool)component)
			{
				float exposure = base.app.controller.settings.GetExposure(component.profile);
				base.app.controller.settings.ApplyPPP(component);
				base.app.controller.settings.SetExposure(component.profile, exposure);
				base.app.controller.settings.SetDepthOfFieldEnabled(p_enabled: false, component.profile);
			}
		}

		public void RestoreMainCamera()
		{
			DroneCamera dc = base.app.model.game.camera;
			dc.inGarage = false;
			dc.main.gameObject.SetActive(value: true);
			dc.main.RemoveAllCommandBuffers();
			RunOnce(delegate
			{
				if ((bool)dc.fx.ppb && dc.fx.ppb.enabled)
				{
					dc.fx.ppb.enabled = false;
					dc.fx.ppb.enabled = true;
				}
			}, 0.1f);
		}

		private void HideHeader()
		{
			Transform transform = base.transform.parent.parent.Find("header");
			if ((bool)transform)
			{
				transform.gameObject.SetActive(value: false);
			}
		}

		public void SavingAnimation(bool p_flag)
		{
			applyStateList[0].SetActive(!p_flag);
			applyStateList[1].SetActive(p_flag);
		}

		public OrbitConstraint GetOrbitConstraint(string p_id)
		{
			for (int i = 0; i < orbitConstraints.Count; i++)
			{
				if (orbitConstraints[i].name == p_id)
				{
					return orbitConstraints[i];
				}
			}
			if (orbitConstraints.Count > 0)
			{
				return orbitConstraints[0];
			}
			return null;
		}

		public OrbitCustomSettings GetOrbitInitialRotation(string p_id)
		{
			for (int i = 0; i < orbitInitialOrientation.Count; i++)
			{
				if (!(orbitInitialOrientation[i] == null) && orbitInitialOrientation[i].name == p_id)
				{
					return orbitInitialOrientation[i];
				}
			}
			return null;
		}

		public void SetCameraPresetsInitialSettings()
		{
			for (int i = 0; i < orbitInitialOrientation.Count; i++)
			{
				orbitInitialOrientation[i].preset.initPreset();
			}
		}

		private void SetToggleSpecItemsPanel()
		{
			devAllowancePanel.SetActive(value: false);
		}

		public void GarageOpenInit()
		{
			RefreshNavigationTooltips();
			SetPostProcessing();
			HideHeader();
			specs.SetBarMaximums();
			base.app.view.ui.footer.droneButton.interactable = false;
			base.app.view.ui.footer.SetColors(base.app.level.IsLevelLoaded("game"), p_ingarage: true);
			contentFade.alpha = 1f;
			cameraOffset.aspect = (float)Screen.width / (float)Screen.height;
			hideComponentWhileDrag.enabled = true;
			pressAnyKeyMessage.Fade(0f, 0.001f);
			SetToggleSpecItemsPanel();
			SavingAnimation(p_flag: false);
			rawData = data;
			initRigData = ScriptableObject.CreateInstance<DroneRigData>();
			SaveInitRigData();
			data.Validate();
			colors.SetDRLColors();
			colors.SelectColorsFromDroneRig(data);
			publicToggle.index = (data.isPublic ? 1 : 0);
			publicToggle.Refresh();
			publicToggleIcon.Find("public").gameObject.SetActive(publicToggle.index == 1);
			publicToggleIcon.Find("private").gameObject.SetActive(publicToggle.index == 0);
			clearPhysicsButton.gameObject.SetActive(data.hasCustomPhysics);
			specs.ToggleTemperatureBar(!data.hasCustomPhysics);
		}

		public void SaveInitRigData()
		{
			initRigData.battery = data.battery;
			initRigData.antenna = data.antenna;
			initRigData.prop = data.prop;
			initRigData.motor = data.motor;
			initRigData.frame = data.frame;
			initRigData.attachment0 = data.attachment0;
			initRigData.attachment1 = data.attachment1;
			initRigData.skinFrame = data.skinFrame;
			initRigData.color0 = data.color0;
			initRigData.color1 = data.color1;
			initRigData.color2 = data.color2;
		}

		public void InitGeorgiaTech()
		{
			georgiaTechLabel.gameObject.SetActive(value: true);
			georgiaTechLabel.alpha = ((drone.body.frame.gatechDragData != null) ? 1f : 0f);
		}

		public void ToggleEnterNameMsg(bool p_enable)
		{
			if (p_enable)
			{
				enterDroneNameMessage.FadeIn();
				droneNameErr.Show(0.3f, 0.5f);
			}
			else
			{
				enterDroneNameMessage.FadeOut();
				droneNameErr.Hide();
			}
		}

		public void RefreshNavigationTooltips()
		{
			DefaultControllerType defaultControllerType = RCI.GetDefaultControllerType(DefaultControllerType.XBox);
			bool flag = defaultControllerType == DefaultControllerType.XBox && RCI.GetActiveJoystick() != null;
			bool flag2 = defaultControllerType == DefaultControllerType.PS && RCI.GetActiveJoystick() != null;
			navigationXboxPanel.SetActive(flag);
			navigationPSPanel.SetActive(flag2);
			foreach (GameObject item in navigationHeader)
			{
				item.SetActive(flag2 || flag);
			}
			if (flag2 || flag)
			{
				backButtonNavigation.right = saveButtonNav;
				saveButtonNav.left = backButtonNavigation;
				foreach (GameObject xboxHeaderIcon in xboxHeaderIcons)
				{
					xboxHeaderIcon.SetActive(flag);
				}
				{
					foreach (GameObject psHeaderIcon in psHeaderIcons)
					{
						psHeaderIcon.SetActive(flag2);
					}
					return;
				}
			}
			backButtonNavigation.right = tabGroup.tabs[0];
			saveButtonNav.left = tabGroup.tabs[tabGroup.tabs.Count - 1];
		}
	}
}

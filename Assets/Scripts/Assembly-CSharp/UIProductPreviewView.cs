using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.UI;
using drl.backend;
using drl.game;
using drl.sim;
using thelab.core;

public class UIProductPreviewView : UIScreenView
{
	public DRLStoreProductData data;

	public UIStoreProductItemView currentProductItem;

	public ListComponent listField;

	[Header("Product Information")]
	public Text productNameField;

	public Text priceText;

	public Image productCategoryImage;

	public List<DRLAsset> currentProductParts;

	public UIGarageCameraOffset cameraOffset;

	public Transform droneContainer;

	public Transform dronePivot;

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

	public DRLStoreProductData selectedCardData;

	public List<DronePart> library;

	[Header("Colors and Specs")]
	public UIProductPreviewColorsView colors;

	public UIGarageSpecsView specs;

	public UINavigation trailColorsNav;

	public UINavigation propsColorsNav;

	public UINavigation textureColorsNav;

	public UINavigation edgeColorsNav;

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

	[HideInInspector]
	public DroneRigData rigdata;

	[HideInInspector]
	public DroneRigData initRigData;

	private DroneRigData m_rawData;

	[HideInInspector]
	public bool openedFromRigSelection;

	[HideInInspector]
	public bool openedFromRigTemplateSelector;

	public Dictionary<Color, int> m_profileColorToIndex;

	public Dictionary<Color, int> m_partsColorToIndex;

	[Header("Garage")]
	public UIGarageCOGMarker droneCOGMarker;

	public FadeComponent[] droneCOGFaders;

	public GameObject droneCOGAxes;

	public Transform droneCOGAxesParent;

	public FadeComponent droneCOGLayerFade;

	public DRLToggleView COGToggle;

	public DRLToggleView SpinToggle;

	public List<OrbitCustomSettings> orbitInitialOrientation;

	public List<OrbitConstraint> orbitConstraints;

	public OrbitTransform orbit;

	public DRLOrbitWASDInput wasd;

	public OrbitFollowInput follow;

	public Material activeSkybox;

	public Material garageSkybox;

	public UICardButtonGarageEditItem testProductGarageEditItem;

	public UICardButtonGarageEditItem selectedCard;

	[Header("Testing Drone Rig Data")]
	public DroneRigData defaultRigData;

	public DroneSkin testSkin;

	public Texture2D testTexture;

	public Texture2D newerTestTexture;

	private Texture2D defaultTexture;

	[Header("Test GUIDs for Test Parts")]
	public string testSkinGUID;

	public string testPropGUID;

	public string testAttachmentGUID;

	public string testFrameGUID;

	public string testLipoGUID;

	public string testMotorGUID;

	public string testPodiumGUID;

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

	public string productName
	{
		get
		{
			return productNameField.text.ToUpper();
		}
		set
		{
			productNameField.text = value.ToUpper();
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

	private void HideHeader()
	{
		Transform transform = base.transform.parent.parent.Find("header");
		if ((bool)transform)
		{
			transform.gameObject.SetActive(value: false);
		}
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

	public void LinkUp(UINavigation p_buttonNavigation)
	{
		p_buttonNavigation.up = colors.textureColorSwatches[0];
	}

	protected void UpdateNavigation(int p_totalPages)
	{
		_ = listField;
		new List<UINavigation>();
		new List<UINavigation>();
		new List<UINavigation>();
		new List<UINavigation>();
		new List<UINavigation>();
		new List<UINavigation>();
		new List<UINavigation>();
		new List<UINavigation>();
	}

	public void ProductScreenInit(UIStoreProductItemView p_product)
	{
		RefreshNavigationTooltips();
		SetPostProcessing();
		HideHeader();
		base.app.view.ui.footer.droneButton.interactable = false;
		base.app.view.ui.footer.SetColors(base.app.level.IsLevelLoaded("game"), p_ingarage: true);
		contentFade.alpha = 1f;
		cameraOffset.aspect = (float)Screen.width / (float)Screen.height;
		currentProductItem = p_product;
		data = currentProductItem.data;
		currentProductParts = currentProductItem.productParts;
		Clear();
		SetDefaults();
		productName = data.name;
		if ((bool)productNameField)
		{
			productNameField.text = productName;
		}
		priceText.text = p_product.priceText.text;
		colors.SetDRLColors();
	}

	public void SetDefaults()
	{
		DroneRigData originalByGUID = base.app.model.storage.state.player.garage.GetOriginalByGUID("DRD-fc5bf84d13e5bac67957921c");
		rigdata = originalByGUID.Clone();
		rigdata.guid = DroneRigData.GenerateGUID();
		rigdata.name = "racer4-store";
		rawData = rigdata;
	}

	[ContextMenu("Set Card Data")]
	public void SetCardsData()
	{
		Clear();
		new List<DRLAsset>();
		for (int i = 0; i < currentProductParts.Count; i++)
		{
			Debug.Log("<color=green>UIProductPreviewView >:</color> Item type was " + data.category);
			if (!(currentProductParts[i] != null))
			{
				continue;
			}
			switch (data.category)
			{
			case "skins":
			{
				DroneSkin droneSkin = currentProductParts[i] as DroneSkin;
				if (i == 0)
				{
					AddCard(droneSkin, p_firstCard: true);
					break;
				}
				AddCard(droneSkin);
				Debug.Log(droneSkin.guid + " was applied");
				break;
			}
			case "props":
				_ = currentProductParts[i];
				break;
			case "attachments":
				_ = currentProductParts[i];
				break;
			case "frames":
				_ = currentProductParts[i];
				break;
			case "lipos":
				_ = currentProductParts[i];
				break;
			case "motors":
				_ = currentProductParts[i];
				break;
			case "podiums":
				_ = currentProductParts[i];
				break;
			}
		}
	}

	public void AddCard(DRLAsset p_asset, bool p_firstCard = false)
	{
		_ = base.app.model.storage.locale;
		UICardButtonGarageEditItem uICardButtonGarageEditItem = listField.Push<UICardButtonGarageEditItem>();
		uICardButtonGarageEditItem.Set(p_asset);
		LinkUp(uICardButtonGarageEditItem.navigation);
		if (p_firstCard)
		{
			uICardButtonGarageEditItem.Notify("garage.edit.item@click", uICardButtonGarageEditItem);
		}
	}

	public void Clear()
	{
		listField.Clear();
	}

	public void RefreshNavigationTooltips()
	{
	}
}

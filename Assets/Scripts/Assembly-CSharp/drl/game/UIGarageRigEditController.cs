using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIGarageRigEditController : Controller<DRLApp>
	{
		private GarageStateModel m_model;

		public Texture2D screenshotTop;

		public Texture2D screenshotSide;

		public List<DronePart> library;

		public List<DronePart> filterPromoted;

		public List<DronePart> filter;

		public List<DronePart> filterInDev;

		public int numberOfItemsPerPage;

		public int minNumberOfItems;

		public float previewRotationSpeed;

		public float previewDistance;

		public float propellerRotationBaseSpeed = 500f;

		public AnimationCurve propellerRotation;

		public UIGarageSpecsController specs;

		private bool areInDevelopmentItemsEnabled;

		private bool areUnallowedItemsEnabled;

		private bool allowChangeProfileColor = true;

		public RawImage debugThumbPanel1;

		public RawImage debugThumbPanel2;

		public RawImage debugThumbPanel3;

		private UICardButtonGarageEditItem m_selection;

		private MonoActivity m_preview_loop;

		private WebAsyncRequest m_upload_top;

		private WebAsyncRequest m_upload_side;

		private bool m_allow_preview;

		private bool m_allow_save;

		private bool m_allow_exit;

		private bool m_allow_name;

		private string m_previousTab;

		private MonoActivity m_propellersRotation;

		private AsyncOperation m_garageSceneLoader;

		private Scene m_garageScene;

		private Scene m_parentScene;

		private bool m_radioEnabled;

		private Transform m_externalDroneParent;

		private Vector3 m_externalDronePosition;

		private Quaternion m_externalDroneRotation;

		private float m_externalDroneCameraFov;

		private float m_externalDroneCameraAngleSpeed;

		private float m_externalDroneCameraDistanceSpeed;

		private float m_startingDistance;

		private DroneCameraModeType m_droneCameraMode;

		private int m_currentPage;

		private GameObject m_trailsObjectReference;

		private DRLFocusTransition m_lastHoveredItem;

		private bool m_tabHasColors;

		private bool m_tabHasFilters;

		private bool m_tabHasInformation;

		private Activity m_preventDroneFloating;

		private bool m_droneRigDirty;

		private bool m_returningFromVideoOrData;

		private Coroutine m_specsBarUpdateCoroutine;

		private MonoActivity m_waitingToUnlockTheUserInteraction;

		private Dictionary<int, MonoActivity> m_viewport_fade_loops;

		private bool m_is_first_capture = true;

		private bool m_lockname_change;

		private List<string> propGuids;

		private List<int> lipoCells;

		private Dictionary<int, Activity> m_viewport_fade;

		private bool m_capture_lock;

		private Activity m_garage_enable_timer;

		private int m_tabGroupCount;

		public UIGarageRigEditView view => AssertLocal<UIGarageRigEditView>("view");

		public GarageStateModel model
		{
			get
			{
				if (m_model == null && base.app != null && base.app.model != null && base.app.model.storage != null && base.app.model.storage.state != null && base.app.model.storage.state.player != null)
				{
					m_model = base.app.model.storage.state.player.garage;
				}
				return m_model;
			}
		}

		public DroneScreenshotCapture capture => DroneScreenshotCapture.instance;

		public UICardButtonGarageEditItem selection
		{
			get
			{
				return m_selection;
			}
			set
			{
				if ((bool)m_selection)
				{
					m_selection.selected = false;
				}
				m_selection = value;
				if ((bool)m_selection)
				{
					m_selection.selected = true;
				}
			}
		}

		private void Awake()
		{
			m_viewport_fade_loops = new Dictionary<int, MonoActivity>();
			view.viewerTransform.localScale = Vector3.one;
			m_tabGroupCount = view.tabGroup.tabs.Count;
			view.viewerCameraListener.enabled = base.app.inGame;
		}

		public override void OnNotification(string p_event, UnityEngine.Object p_target, params object[] p_data)
		{
			bool num = base.app.view.ui.screens.current == view.screen;
			if (p_event != null && p_event == "ui.screen@switch")
			{
				UIScreen uIScreen = ((p_data.Length != 0) ? (p_data[0] as UIScreen) : null);
				if (((p_data.Length > 1) ? (p_data[1] as UIScreen) : null) != view.screen)
				{
					if ((bool)view.viewerTransform)
					{
						view.viewerTransform.gameObject.SetActive(value: false);
					}
				}
				else if ((bool)view.viewerTransform)
				{
					view.viewerTransform.gameObject.SetActive(value: true);
				}
				if ((bool)uIScreen && uIScreen.name == view.screen.name)
				{
					RenderSettings.skybox = view.activeSkybox;
				}
				if (!(base.app.view.ui.screens.current != view.screen) && m_allow_exit)
				{
					if (m_preview_loop != null)
					{
						m_preview_loop.Stop();
					}
					if ((bool)base.app.view.ui.header)
					{
						base.app.view.ui.header.pathFade.FadeIn(0.2f);
					}
					if ((bool)base.app.view.ui.header)
					{
						base.app.view.ui.header.logoContainer.gameObject.SetActive(value: true);
					}
					if ((bool)base.app.view.ui.footer)
					{
						base.app.view.ui.footer.Show(0.2f);
					}
					specs.view.ToggleUnableToFly(p_enable: true, p_check: true);
					view.contentFade.FadeIn(0.2f);
					view.pressAnyKeyMessage.FadeOut(0.2f);
					m_allow_preview = true;
					m_allow_save = true;
					m_allow_name = true;
					Transform transform = base.transform.parent.parent.Find("header");
					if ((bool)transform)
					{
						transform.gameObject.SetActive(value: true);
					}
				}
			}
			if (!num || p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "game.unpause":
				break;
			case "garage.edit.testvideo@click":
				break;
			case "ui.screen@open":
			{
				if (p_data[0] as UIScreen != view.screen)
				{
					break;
				}
				if ((bool)base.app.view.ui.header)
				{
					base.app.view.ui.header.pathFade.Fade(0f, 0.2f);
				}
				if (m_returningFromVideoOrData)
				{
					m_returningFromVideoOrData = false;
					break;
				}
				if (m_preview_loop != null)
				{
					m_preview_loop.Stop();
				}
				view.activeSkybox = RenderSettings.skybox;
				Notify("garage.isOpen");
				view.flyButtonElementView.interactable = false;
				bool num2 = MakeCloneIfNecessary();
				view.GarageOpenInit();
				SetIgnoredGameCommands();
				LoadGarageScene(p_load: true);
				InitGarageFlags();
				SetReferentDroneSpecs();
				if (view.externalDrone != null)
				{
					LoadExternalDrone();
				}
				else
				{
					CreateNewDrone();
				}
				if (num2)
				{
					view.drone.rig = view.data;
					view.player.rig = view.data;
					view.data.name = "";
				}
				m_is_first_capture = true;
				CreateThumbnailDrone();
				LoadDroneName();
				MarkRigDirty();
				RefreshBars();
				InitLibrary();
				if (view.externalDrone == null)
				{
					RefreshCenterOfGravity();
					FadeInTheCOG();
				}
				view.InitGeorgiaTech();
				float p_delay = (base.app.level.IsLevelLoaded("game") ? 1.4f : (1f / 60f));
				EnableGarageSceneDelay(p_enable: true, p_delay, MoveDroneToGarageScene);
				ResetViewerFocus();
				if (view.data.esc == "E-000")
				{
					DroneRigData originalByFrame = model.GetOriginalByFrame(view.data.frame);
					if ((bool)originalByFrame && view.data.esc != originalByFrame.esc)
					{
						RefreshDronePart(originalByFrame.esc);
					}
				}
				view.videoButton.interactable = false;
				if (view.showStoreOnOpen)
				{
					Debug.LogWarning("UIGarageRigEditController> open store here");
				}
				view.showStoreOnOpen = false;
				if (base.app.level.IsLevelLoaded("game"))
				{
					view.mapStepper.transform.parent.gameObject.SetActive(value: false);
					if (base.app.inGame && base.app.controller.game != null)
					{
						base.app.controller.game.input.SetIgnoredCommands();
					}
				}
				break;
			}
			case "garage.store@click":
				if (view.showStoreOnOpen)
				{
					Debug.LogWarning("UIGarageRigEditController> open store here");
				}
				view.showStoreOnOpen = false;
				view.openedFromRigTemplateSelector = false;
				Debug.Log("<color=blue>Store opened from garage</color>");
				base.app.view.ui.screens.Open<UIScreen>("store-screen");
				if (base.app.inGame)
				{
					base.app.view.ui.game.hud.Hide();
				}
				break;
			case "garage.edit.item@click":
			{
				UICardButtonGarageEditItem uICardButtonGarageEditItem = p_target as UICardButtonGarageEditItem;
				if (!(uICardButtonGarageEditItem.data == null))
				{
					if (areInDevelopmentItemsEnabled || (!areInDevelopmentItemsEnabled && !uICardButtonGarageEditItem.storeData.inDevelopment))
					{
						_ = uICardButtonGarageEditItem.unallowed;
						selection = uICardButtonGarageEditItem;
						ChangePart(uICardButtonGarageEditItem);
					}
					SetViewerFocus(view.tabGroup.selection, p_center: false);
				}
				break;
			}
			case "garage.edit.item@focus":
			{
				UICardButtonGarageEditItem uICardButtonGarageEditItem2 = p_target as UICardButtonGarageEditItem;
				if (!(uICardButtonGarageEditItem2.data == null))
				{
					OnHoverPart(uICardButtonGarageEditItem2);
					m_lastHoveredItem = uICardButtonGarageEditItem2.GetComponent<DRLFocusTransition>();
				}
				break;
			}
			case "garage.edit.page-next@click":
				if (view.scrollView.NextPage())
				{
					if (view.scrollView.currentPage == 1)
					{
						view.previousPageButton.interactable = true;
					}
					if (view.scrollView.currentPage == view.scrollView.totalPages - 1)
					{
						view.nextPageButton.interactable = false;
					}
					EnableItemsInsideViewport();
					SetUpFiltersAndPaginationNavigation();
				}
				break;
			case "garage.edit.page-previous@click":
				if (view.scrollView.PreviousPage())
				{
					if (view.scrollView.currentPage == 0)
					{
						view.previousPageButton.interactable = false;
					}
					if (view.scrollView.currentPage < view.scrollView.totalPages - 1)
					{
						view.nextPageButton.interactable = true;
					}
					DisableItemsOutsideViewport();
					SetUpFiltersAndPaginationNavigation();
				}
				break;
			case "garage.edit.form.event@click":
			{
				UIElementView uIElementView = p_target as UIElementView;
				string text = uIElementView.name;
				if (text != null && text == "public-toggle")
				{
					DRLStepperView dRLStepperView = (DRLStepperView)uIElementView;
					view.publicToggleIcon.Find("public").gameObject.SetActive(dRLStepperView.index == 1);
					view.publicToggleIcon.Find("private").gameObject.SetActive(dRLStepperView.index == 0);
					view.data.isPublic = dRLStepperView.index == 1;
				}
				break;
			}
			case "garage.edit.form.event@submit":
			{
				string text = (p_target as UIElementView).name;
				if (text != null && text == "drone-name")
				{
					OnRigNameValueEntered();
					MarkRigDirty();
				}
				break;
			}
			case "garage.edit.form.event@end-edit":
			{
				string text = (p_target as UIElementView).name;
				if (text != null && text == "drone-name")
				{
					OnRigNameValueEntered();
					MarkRigDirty();
				}
				break;
			}
			case "garage.edit.tab@change":
			{
				DRLTabGroup dRLTabGroup = p_target as DRLTabGroup;
				SetTab(dRLTabGroup.selection);
				break;
			}
			case "service.state.write":
				base.app.view.audio.PlayUIGenericSuccess();
				view.SavingAnimation(p_flag: false);
				if (view.usingExternalDrone)
				{
					Notify("garage.edit.rig.saved", view.data, view.usingExternalDrone);
				}
				break;
			case "garage.edit.preview@click":
			{
				if (!m_allow_preview)
				{
					break;
				}
				m_allow_preview = false;
				bool stop_animateRotation = false;
				FadeOutTheCOG();
				view.ToggleEnterNameMsg(p_enable: false);
				view.contentFade.FadeOut(0.2f);
				if ((bool)base.app.view.ui.header)
				{
					base.app.view.ui.header.pathFade.FadeOut(0.2f);
				}
				if ((bool)base.app.view.ui.footer)
				{
					base.app.view.ui.footer.Hide(0.2f);
				}
				specs.view.ToggleUnableToFly(p_enable: false, p_check: true);
				view.pressAnyKeyMessage.FadeIn(0.2f);
				SetViewerFocus("preview", p_center: true);
				if (m_preview_loop != null)
				{
					m_preview_loop.Stop();
				}
				m_preview_loop = Run((Func<bool>)delegate
				{
					if (!view)
					{
						return false;
					}
					bool flag = false;
					m_previousTab = "preview";
					if (!stop_animateRotation)
					{
						stop_animateRotation = view.wasd.userInteracts;
					}
					if (!stop_animateRotation)
					{
						view.orbit.angle += new Vector2(previewRotationSpeed * Time.deltaTime, 0f);
					}
					if (Input.anyKey && !Input.GetKey(KeyCode.Mouse0) && !Input.GetKey(KeyCode.Mouse1))
					{
						flag = true;
					}
					if (flag)
					{
						if ((bool)base.app.view.ui.footer)
						{
							base.app.view.ui.footer.Show(0.2f);
						}
						specs.view.ToggleUnableToFly(p_enable: true, p_check: true);
						view.pressAnyKeyMessage.FadeOut(0.2f);
						view.contentFade.FadeIn(0.2f);
						FadeInTheCOG();
						OnRigNameValueEntered();
						SetViewerFocus(view.tabGroup.selection, p_center: true);
						Timer.Set(this, "m_allow_preview", 1f, true);
						return false;
					}
					return true;
				}, 0f, false);
				break;
			}
			case "garage.edit.apply@click":
				SaveRig();
				break;
			case "garage.edit.fly@click":
			{
				string.IsNullOrEmpty(view.data.name);
				base.app.model.storage.state.player.garage.currentRigData = view.data;
				Drone drone = view.drone;
				if (base.app.inGame)
				{
					drone = ChangeRig(base.app.controller.game.model.playerDrone, view.data);
				}
				FlightControllerMode flightControllerMode = GetFlightControllerMode(base.app.model.storage.state.player.activeFCMode);
				if (flightControllerMode != FlightControllerMode.Pro)
				{
					drone.fc.SetMode(flightControllerMode);
				}
				FCProfileData active = base.app.model.storage.state.player.settings.tuning.GetActive();
				if (active != null && drone.fc != null)
				{
					drone.fc.profile.SetData(active);
				}
				FadeOutTheCOG();
				StopPropellersAnimation();
				string text2 = "mainmenu";
				if (base.app.level.IsLevelLoaded("game"))
				{
					ReturnExternalDrone();
					base.app.arguments.game.garage = true;
					if (!view.openedFromDashboard)
					{
						text2 = (view.openedFromPause ? "game" : ((!(base.app.controller.game.replay != null)) ? "game" : "replay"));
					}
					else
					{
						Notify("game.ui.dashboard@hide");
						text2 = "game";
					}
					base.app.controller.game.input.controller.Pause(p_flag: false, p_pause_physics: false);
					Notify("game.unpause");
					view.openedFromDashboard = false;
					view.openedFromPause = false;
					base.app.view.ui.footer.Hide(0f);
					base.app.view.ui.screens.Close("garage-rig-edit-screen");
				}
				Notify("garage.edit.fly.ready", view.data, drone, view.mapStepper.index);
				EnableGarageScene(p_enable: false);
				Notify("garage.isClosed", text2);
				view.hideComponentWhileDrag.enabled = false;
				base.app.view.ui.footer.SetColors(base.app.level.IsLevelLoaded("game"));
				ClearIgnoredCommands();
				break;
			}
			case "garage.edit.datasheet@click":
				m_returningFromVideoOrData = true;
				base.app.view.ui.screens.Open<UIGarageChartsView>("garage-charts-screen").drone = view.drone;
				break;
			case "garage.edit.cog@click":
				if (((DRLToggleView)p_target).toggle.isOn)
				{
					for (int i = 0; i < view.droneCOGFaders.Length; i++)
					{
						view.droneCOGFaders[i].FadeIn(0.2f);
					}
					view.droneCOGLayerFade.FadeIn(0.2f);
					view.droneCOGMarker.active = true;
				}
				else
				{
					for (int j = 0; j < view.droneCOGFaders.Length; j++)
					{
						view.droneCOGFaders[j].FadeOut(0.2f);
					}
					view.droneCOGLayerFade.FadeOut(0.2f);
					view.droneCOGMarker.active = false;
				}
				break;
			case "ui.screen.return@click":
				m_previousTab = "preview";
				if (m_preview_loop != null)
				{
					m_preview_loop.Stop();
					m_preview_loop = null;
					if ((bool)base.app.view.ui.header)
					{
						base.app.view.ui.header.pathFade.FadeIn(0.2f);
					}
					if ((bool)base.app.view.ui.footer)
					{
						base.app.view.ui.footer.Show(0.2f);
					}
					specs.view.ToggleUnableToFly(p_enable: true, p_check: true);
					view.contentFade.FadeIn(0.2f);
					view.pressAnyKeyMessage.FadeOut(0.2f);
					SetViewerFocus(view.tabGroup.selection, p_center: false);
					Timer.Set(this, "m_allow_preview", 1f, true);
				}
				else
				{
					Notify("garage.edit.back@click");
				}
				break;
			case "garage.edit.back@click":
			{
				if (!m_allow_exit)
				{
					break;
				}
				if (m_specsBarUpdateCoroutine != null)
				{
					StopCoroutine(m_specsBarUpdateCoroutine);
					m_specsBarUpdateCoroutine = null;
				}
				if (!view.usingExternalDrone && view.drone != null)
				{
					view.drone.gameObject.SetActive(value: false);
				}
				if (view.thumbnailDrone != null)
				{
					view.thumbnailDrone.gameObject.SetActive(value: false);
				}
				view.viewerTransform.gameObject.SetActive(value: false);
				base.app.view.ui.footer.droneButton.interactable = true;
				FadeOutTheCOG();
				StopPropellersAnimation();
				string text3 = "mainmenu";
				DroneRigData data = view.data;
				DroneRigSpecData droneSpecData = model.GetDroneSpecData(data);
				if (!areUnallowedItemsEnabled && (droneSpecData.thrust < 10f || droneSpecData.torque < 1E-06f || droneSpecData.rpm < 1000f))
				{
					base.app.model.storage.state.player.garage.ResetCurrentRigData();
				}
				if (base.app.level.IsLevelLoaded("game"))
				{
					base.app.arguments.game.garage = true;
					if (view.drone != null)
					{
						view.drone.fc.armed = true;
					}
					if (view.openedFromPause)
					{
						if (view.openedFromRigTemplateSelector)
						{
							base.app.view.ui.screens.Return(1);
						}
						else
						{
							base.app.view.ui.screens.Return();
						}
						text3 = "pause";
					}
					else if (view.openedFromDashboard)
					{
						base.app.view.ui.screens.Close("garage-rig-edit-screen");
						Notify("game.ui.dashboard@show");
						text3 = "dashboard";
					}
					else if (base.app.controller.game.replay != null)
					{
						base.app.view.ui.screens.Close("garage-rig-edit-screen");
						Notify("garage.edit.done", view.data, view.drone);
						text3 = "replay";
					}
					else if (!view.openedFromDashboard && !view.openedFromPause)
					{
						base.app.view.ui.screens.Close("garage-rig-edit-screen");
						Notify("garage.edit.fly.ready", view.data, view.drone);
						text3 = "game";
					}
					else
					{
						Notify("garage.edit.done", view.data, view.drone);
					}
					view.openedFromPause = false;
					view.openedFromDashboard = false;
					EnableGarageScene(p_enable: false);
					ReturnExternalDrone();
					base.app.view.ui.footer.Hide(0f);
				}
				else
				{
					base.app.model.storage.state.player.garage.currentRigData = null;
					if (view.openedFromRigTemplateSelector)
					{
						base.app.view.ui.screens.Return(2);
					}
					else
					{
						base.app.view.ui.screens.Return();
					}
					EnableGarageScene(p_enable: false);
					view.openedFromRigTemplateSelector = false;
				}
				Notify("garage.isClosed", text3);
				view.hideComponentWhileDrag.enabled = false;
				base.app.view.ui.footer.SetColors(base.app.level.IsLevelLoaded("game"));
				ClearIgnoredCommands();
				break;
			}
			case "garage.edit.enableunallowed@click":
				areUnallowedItemsEnabled = ((DRLToggleView)p_target).toggle.isOn;
				RefreshLibrary();
				SetTab(view.tabGroup.selection);
				break;
			case "garage.edit.enabledev@click":
				areInDevelopmentItemsEnabled = ((DRLToggleView)p_target).toggle.isOn;
				RefreshLibrary();
				ApplyFilters();
				break;
			case "garage.edit.filter0.form.event@change":
				ApplyFilters();
				break;
			case "garage.edit.filter1.form.event@change":
				ApplyFilters();
				break;
			case "garage.edit.spin@click":
				if (view.SpinToggle.toggle.isOn)
				{
					PropellersAnimation();
				}
				else
				{
					StopPropellersAnimation();
				}
				break;
			case "garage.edit.apply@focus":
				if (view.saveButtonNav.callee != null && view.saveButtonNav.callee.transform != null && view.saveButtonNav.callee.transform.parent == view.itemsGrid.transform)
				{
					OnHoverPart(selection);
				}
				break;
			case "garage.edit.filter0.form.event@focus":
				if (view.filtersNavigation[0] != null && view.filtersNavigation[0].callee != null && view.filtersNavigation[0].callee.transform != null && view.filtersNavigation[0].callee.transform.parent == view.itemsGrid.transform)
				{
					OnHoverPart(selection);
				}
				break;
			case "garage.edit.page-previous@focus":
				if (view.pagePreviousNav.callee != null && view.pagePreviousNav.callee.transform != null && view.pagePreviousNav.callee.transform.parent == view.itemsGrid.transform)
				{
					OnHoverPart(selection);
				}
				break;
			case "garage.edit.grid@out":
				OnGridExit();
				break;
			case "garage.edit.clear-physics@click":
				if (view.data != null)
				{
					view.data.tune = null;
				}
				view.clearPhysicsButton.gameObject.SetActive(value: false);
				specs.view.ToggleTemperatureBar(p_enable: true);
				RefreshBars();
				break;
			case "settings.controller.disconnect":
			case "settings.controller.connect":
				view.RefreshNavigationTooltips();
				break;
			case "ui.footer.settings@click":
			case "ui.footer.calibrate@click":
			case "ui.footer.profile@click":
				m_returningFromVideoOrData = true;
				break;
			}
		}

		private void SetReferentDroneSpecs()
		{
			int counter = 10;
			Activity.Run((Func<bool>)delegate
			{
				if (counter-- < 0)
				{
					return false;
				}
				if (view == null || view.drone == null || model == null)
				{
					return false;
				}
				float num2 = view.drone.EstimateTopSpeed() * 3.6f;
				if (num2 < 0f)
				{
					return true;
				}
				model.lastSavedRigSpecData.topSpeed = num2;
				view.data.topSpeed = num2;
				specs.SetTopSpeed(model.lastSavedRigSpecData, model.lastSavedRigSpecData);
				return false;
			}, 0f, false);
			if ((bool)view && (bool)view.drone)
			{
				float num = view.drone.EstimateTopSpeed() * 3.6f;
				model.lastSavedRigSpecData.topSpeed = num;
				view.data.topSpeed = num;
				specs.StartUpdateSpeedBar(model.lastSavedRigSpecData.topSpeed, num);
				model.SaveCurrentRigSpecData(view.data);
			}
		}

		private bool MakeCloneIfNecessary()
		{
			if (model.GetOriginalByGUID(view.data.guid) != null || view.openedFromRigTemplateSelector)
			{
				view.data = view.data.Clone();
				view.data.guid = DroneRigData.GenerateGUID();
				view.data.name = "";
				view.rigName = "";
				return true;
			}
			return false;
		}

		private void InitGarageFlags()
		{
			m_droneRigDirty = false;
			m_allow_preview = true;
			m_allow_name = true;
			m_allow_save = true;
			m_previousTab = null;
			m_allow_exit = false;
			Timer.Set(this, "m_allow_exit", 2f, true);
		}

		public void InitCapture()
		{
			Debug.Log("UIGarageRigEditController> InitCapture");
			capture.gameObject.SetActive(value: true);
			capture.transform.position = new Vector3(100f, 100f, 100f);
			Activity.RunOnce(delegate
			{
				DroneScreenshotData component = view.drone.body.frame.transform.Find("captures/top").GetComponent<DroneScreenshotData>();
				screenshotTop = capture.Capture(1, 1, view.drone.transform, component, p_smooth: true, p_mipmap: false);
			}, 0.1f);
		}

		public async void OnRigNameValueEntered()
		{
			if (m_lockname_change)
			{
				return;
			}
			m_lockname_change = true;
			Timer.Set(this, "m_lockname_change", 1f, false);
			string text = view.rigName.Trim();
			if (string.IsNullOrEmpty(text))
			{
				view.ToggleEnterNameMsg(p_enable: true);
				m_allow_name = false;
				return;
			}
			m_allow_name = true;
			view.ToggleEnterNameMsg(p_enable: false);
			if (text.ToLower().StartsWith("drl "))
			{
				text = text.Substring(4);
			}
			bool flag = false;
			string text2 = Regex.Replace(text, "\\ \\([0-9]+\\)$", "").Trim();
			Match match = Regex.Match(text, "\\ \\(([0-9]+)\\)$");
			Dictionary<string, string> rigNames = model.GetRigNames();
			int num = (match.Success ? int.Parse(match.Groups[1].Value) : 0);
			int num2 = 0;
			bool flag2 = false;
			while (!flag)
			{
				num2++;
				if (num2 >= 10)
				{
					break;
				}
				flag = true;
				string text3 = text.ToLower();
				foreach (KeyValuePair<string, string> item in rigNames)
				{
					if (item.Value == text3 && item.Key != view.data.guid)
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					text = (text2 + " (" + ++num + ")").Trim();
					flag2 = true;
				}
			}
			view.data.name = text;
			view.rigNameField.allowValidation = !flag2;
			view.rigName = text;
			Timer.Set(view.rigNameField, "allowValidation", 1f, true);
		}

		public void SetTab(string p_id)
		{
			view.informationPanelFader.FadeOut(0.05f);
			string text = p_id;
			m_tabHasColors = false;
			m_tabHasFilters = false;
			m_tabHasInformation = true;
			bool flag = true;
			ClearFilterLabels();
			if (p_id == null)
			{
				goto IL_0245;
			}
			switch (p_id)
			{
			case "frames":
				break;
			case "motors":
				goto IL_018d;
			case "props":
				goto IL_01b4;
			case "lipos":
				goto IL_01d8;
			case "extras":
				goto IL_01fc;
			case "style":
				goto IL_020a;
			case "color":
				goto IL_0235;
			default:
				goto IL_0245;
			}
			m_tabHasFilters = true;
			flag = false;
			PopulateLibrary(p_id);
			PopulateFilters(p_id, flag);
			goto IL_0256;
			IL_0245:
			text = "frames";
			PopulateLibrary("frames");
			goto IL_0256;
			IL_0256:
			view.colorsPanel.SetActive(m_tabHasColors);
			view.tilesPanel.SetActive(!m_tabHasColors);
			view.previousPageButton.gameObject.SetActive(!m_tabHasColors);
			view.nextPageButton.gameObject.SetActive(!m_tabHasColors);
			if (m_tabHasFilters)
			{
				view.filter0.interactable = true;
				if (flag)
				{
					view.filter1.interactable = true;
					view.filter1.gameObject.SetActive(value: true);
				}
				else
				{
					view.filter1.interactable = false;
					view.filter1.gameObject.SetActive(value: false);
				}
			}
			else
			{
				view.filter0.interactable = false;
				if (flag)
				{
					view.filter1.interactable = false;
					view.filter1.gameObject.SetActive(value: false);
				}
			}
			if (view.tabGroup.selection != text)
			{
				view.tabGroup.selection = text;
			}
			SetUpGridNavigation();
			SetUpFiltersAndPaginationNavigation();
			SetViewerFocus(text, p_center: true);
			this.TimerRunOnce(delegate
			{
				if (p_id == "color")
				{
					UINavigation.Focus(view.trailColorsNav);
				}
				else
				{
					UINavigation.Focus(m_tabHasFilters ? view.filtersNavigation[0] : view.saveButtonNav);
				}
			}, 1f / 60f);
			Debug.Log("<color=blue>SET TAB CALLED!!!</color>  " + p_id + " " + StackTraceUtility.ExtractStackTrace());
			return;
			IL_018d:
			m_tabHasFilters = true;
			flag = true;
			PopulateLibrary(p_id);
			PopulateFilters(p_id, flag);
			goto IL_0256;
			IL_01b4:
			m_tabHasFilters = true;
			flag = true;
			PopulateLibrary(p_id);
			PopulateFilters(p_id, flag);
			goto IL_0256;
			IL_01d8:
			m_tabHasFilters = true;
			flag = true;
			PopulateLibrary(p_id);
			PopulateFilters(p_id, flag);
			goto IL_0256;
			IL_01fc:
			PopulateLibrary(p_id);
			goto IL_0256;
			IL_020a:
			m_tabHasFilters = true;
			flag = false;
			m_tabHasInformation = false;
			PopulateFilters(p_id, flag);
			PopulateLibrary(p_id);
			goto IL_0256;
			IL_0235:
			m_tabHasInformation = false;
			m_tabHasColors = true;
			goto IL_0256;
		}

		protected void FadeOutTheCOG()
		{
			if (view.COGToggle.toggle.isOn)
			{
				for (int i = 0; i < view.droneCOGFaders.Length; i++)
				{
					view.droneCOGFaders[i].FadeOut(0.2f);
				}
				view.droneCOGLayerFade.FadeOut(0.2f);
				view.droneCOGMarker.active = false;
			}
		}

		protected void FadeInTheCOG()
		{
			if (view.COGToggle.toggle.isOn)
			{
				for (int i = 0; i < view.droneCOGFaders.Length; i++)
				{
					view.droneCOGFaders[i].FadeIn(0.2f);
				}
				view.droneCOGLayerFade.FadeIn(0.2f);
				view.droneCOGMarker.active = true;
			}
		}

		private void ResetViewerFocus()
		{
			OrbitTransform orbit = view.orbit;
			orbit.angle = new Vector2(220f, 15f);
			orbit.distance = 0.39f;
			orbit.anchor = new Vector3(0f, 0f, 0f);
		}

		public void SetViewerFocus(string p_id, bool p_center)
		{
			if (!view.drone)
			{
				return;
			}
			DroneFrame frame = view.drone.body.frame;
			Vector3 position = view.dronePivot.position;
			OrbitTransform orbit = view.orbit;
			OrbitConstraint orbitConstraint = view.GetOrbitConstraint(p_id);
			if ((bool)orbitConstraint)
			{
				orbit.constraint.Set(orbitConstraint);
			}
			if (!orbitConstraint)
			{
				_ = orbit.angle;
			}
			else
			{
				_ = (orbitConstraint.angleMin + orbitConstraint.angleMax) * 0.5f;
			}
			if (!orbitConstraint)
			{
				_ = orbit.distance;
			}
			else
			{
				_ = orbitConstraint.distanceMin;
				_ = orbitConstraint.distanceMax;
			}
			if (p_center)
			{
				OrbitCustomSettings orbitInitialRotation = view.GetOrbitInitialRotation(p_id);
				if (m_previousTab == null)
				{
					m_previousTab = p_id;
					view.SetCameraPresetsInitialSettings();
					orbit.angle = orbitInitialRotation.preset.angle;
					orbit.distance = orbitInitialRotation.preset.distance;
					orbit.anchor = position;
					return;
				}
				OrbitCustomSettings orbitInitialRotation2 = view.GetOrbitInitialRotation(m_previousTab);
				orbitInitialRotation2.preset.angle = orbit.angle;
				orbitInitialRotation2.preset.distance = orbit.distance;
				if (!(orbitInitialRotation2.preset == orbitInitialRotation.preset))
				{
					orbit.ClampCurrentAngle();
					orbit.angle = new Vector2(orbitInitialRotation.preset.angle.x % 360f, orbitInitialRotation.preset.angle.y % 360f);
					if (p_id != "preview")
					{
						orbit.distance = orbitInitialRotation.preset.distance;
					}
				}
			}
			switch (p_id)
			{
			case "frames":
				position = view.dronePivot.position;
				break;
			case "motors":
				position = frame.escs[0].motor.transform.position;
				break;
			case "props":
				position = frame.escs[0].motor.prop.transform.position;
				break;
			case "lipos":
				position = view.dronePivot.position;
				break;
			case "extras":
				position = view.dronePivot.position;
				break;
			case "style":
				position = view.dronePivot.position;
				break;
			case "color":
				position = view.dronePivot.position;
				break;
			default:
				p_id = "frames";
				position = view.dronePivot.position;
				break;
			}
			Tween.Add(orbit, "anchor", position, 0.3f, 0f, Cubic.Out);
			m_previousTab = p_id;
		}

		public void SetViewerFocus(string p_id)
		{
			SetViewerFocus(p_id, p_center: false);
		}

		private bool HasRigAppearanceChanged()
		{
			if (view.initRigData.frame != view.data.frame)
			{
				return true;
			}
			if (view.initRigData.battery != view.data.battery)
			{
				return true;
			}
			if (view.initRigData.prop != view.data.prop)
			{
				return true;
			}
			if (view.initRigData.motor != view.data.motor)
			{
				return true;
			}
			if (view.initRigData.attachment0 != view.data.attachment0)
			{
				return true;
			}
			if (view.initRigData.attachment1 != view.data.attachment1)
			{
				return true;
			}
			if (view.initRigData.antenna != view.data.antenna)
			{
				return true;
			}
			if (view.initRigData.skinFrame != view.data.skinFrame)
			{
				return true;
			}
			if (view.initRigData.color0 != view.data.color0)
			{
				return true;
			}
			if (view.initRigData.color1 != view.data.color1)
			{
				return true;
			}
			if (view.initRigData.color2 != view.data.color2)
			{
				return true;
			}
			return false;
		}

		private IEnumerator SaveDroneThumbnails(bool p_force = false)
		{
			bool hasAppearanceChanged = p_force || HasRigAppearanceChanged();
			int screenshot_save_count = 1;
			ServiceModel sm = base.app.model.service;
			DroneRigData rd = view.data;
			Drone drone = view.drone;
			model.UpdateRig(drone);
			view.thumbnailDrone.gameObject.SetActive(value: true);
			view.thumbnailDrone.fc.armed = false;
			foreach (DroneESC esc in view.thumbnailDrone.body.frame.escs)
			{
				esc.motor.ForceStop();
			}
			view.thumbnailDrone.renderer.visible = true;
			yield return null;
			if (!view || !view.drone || !view.drone.body || !view.drone.body.frame || !view.thumbnailDrone)
			{
				yield break;
			}
			if (hasAppearanceChanged)
			{
				view.SaveInitRigData();
				Transform transform = view.drone.body.frame.transform.Find("captures/side");
				if (!transform)
				{
					yield break;
				}
				DroneScreenshotData sd = transform.GetComponent<DroneScreenshotData>();
				if (!sd)
				{
					yield break;
				}
				view.thumbnailDrone.transform.localScale = new Vector3(sd.scale, sd.scale, sd.scale);
				yield return null;
				view.thumbnailDrone.body.UpdateBatteryHooks();
				view.thumbnailDrone.body.frame.batteries[0].transform.localPosition = view.drone.body.frame.batteries[0].transform.localPosition;
				view.thumbnailDrone.body.frame.camera.transform.localRotation = view.drone.body.frame.camera.transform.localRotation;
				DroneBatteryPlacement componentInChildren = view.thumbnailDrone.GetComponentInChildren<DroneBatteryPlacement>();
				DroneBatteryPlacement componentInChildren2 = view.drone.GetComponentInChildren<DroneBatteryPlacement>();
				if (componentInChildren != null && componentInChildren2 != null)
				{
					if (componentInChildren.strapIntLeft != null && componentInChildren2.strapIntLeft != null)
					{
						componentInChildren.strapIntLeft.localPosition = componentInChildren2.strapIntLeft.localPosition;
					}
					if (componentInChildren.strapIntRight != null && componentInChildren2.strapIntRight != null)
					{
						componentInChildren.strapIntRight.localPosition = componentInChildren2.strapIntRight.localPosition;
					}
					if (componentInChildren.strapExtRight != null && componentInChildren2.strapExtRight != null)
					{
						componentInChildren.strapExtRight.localPosition = componentInChildren2.strapExtRight.localPosition;
					}
					if (componentInChildren.strapExtLeft != null && componentInChildren2.strapExtLeft != null)
					{
						componentInChildren.strapExtLeft.localPosition = componentInChildren2.strapExtLeft.localPosition;
					}
					if (componentInChildren.strapExtCenter != null && componentInChildren2.strapExtCenter != null)
					{
						componentInChildren.strapExtCenter.localPosition = componentInChildren2.strapExtCenter.localPosition;
					}
				}
				yield return null;
				if (!view || !view.drone || !view.drone.body || !view.drone.body.frame || !view.thumbnailDrone || !capture || !model)
				{
					yield break;
				}
				CaptureScreenshot(sd);
				while (!screenshotSide)
				{
					yield return null;
				}
				view.thumbnailDrone.transform.localScale = Vector3.one;
				view.thumbnailDrone.gameObject.SetActive(value: false);
				model.UpdateCachedThumbnail(rd.guid, screenshotSide);
				base.app.controller.RefreshFooterDrone(screenshotSide);
			}
			if (hasAppearanceChanged)
			{
				byte[] p_data = screenshotSide.EncodeToPNG();
				Action on_save_complete = delegate
				{
					if (screenshot_save_count <= 0)
					{
						Debug.Log("UIGarageRigEditController> Rig Saved\n" + rd.ToJson(p_indented: true));
						model.ClearCachedThumbnail(rd.guid);
						if ((bool)model)
						{
							model.UpdateRig(drone);
						}
						base.app.controller.RefreshFooterDrone();
						this.TimerRunOnce(delegate
						{
							m_allow_save = true;
							view.SavingAnimation(p_flag: false);
						}, 1f);
					}
				};
				if (m_upload_side != null)
				{
					m_upload_side.Cancel();
				}
				m_upload_side = sm.StorageImage("drone-thumb", p_data, delegate(string p_url)
				{
					rd.thumb1 = p_url;
					Debug.Log($"UIGarageRigEditController> Xbox Debug / Drone Thumb {screenshot_save_count} - {p_url}");
					int num = screenshot_save_count;
					screenshot_save_count = num - 1;
					on_save_complete();
				});
			}
			else
			{
				if ((bool)model)
				{
					model.UpdateRig(drone);
				}
				this.TimerRunOnce(delegate
				{
					m_allow_save = true;
					view.SavingAnimation(p_flag: false);
				}, 1f);
			}
		}

		protected void CaptureScreenshot(DroneScreenshotData p_dsd)
		{
			screenshotSide = null;
			InitCapture();
			CreateThumbnailDrone();
			Activity.RunOnce(delegate
			{
				view.thumbnailDrone.localPosition = new Vector3(0f, 0f, 0f);
				view.thumbnailDrone.rigidbody.frozen = true;
				view.thumbnailDrone.gameObject.SetActive(value: true);
				capture.CaptureAsync(640, 640, view.thumbnailDrone.transform, p_dsd, p_smooth: true, p_preview: false, p_mipmap: false, delegate(Texture p_result)
				{
					screenshotSide = (Texture2D)p_result;
				});
			}, 2f);
		}

		public void SaveRig(bool p_force = false)
		{
			if (!p_force && (!m_allow_save || !m_allow_name))
			{
				return;
			}
			m_allow_save = false;
			view.SavingAnimation(p_flag: true);
			MarkRigClean();
			base.app.model.storage.state.player.garage.activeRigData = view.data;
			OnRigNameValueEntered();
			RefreshBars();
			int counter = 10;
			Activity.Run((Func<bool>)delegate
			{
				if (counter-- < 0)
				{
					return false;
				}
				if (view == null || view.drone == null || model == null)
				{
					return false;
				}
				float num2 = view.drone.EstimateTopSpeed() * 3.6f;
				if (num2 < 0f)
				{
					return true;
				}
				if (model == null || view == null)
				{
					return false;
				}
				model.lastSavedRigSpecData.topSpeed = num2;
				view.data.topSpeed = num2;
				specs.SetTopSpeed(model.lastSavedRigSpecData, model.lastSavedRigSpecData);
				return false;
			}, 0f, false);
			if (view != null && view.drone != null)
			{
				float num = view.drone.EstimateTopSpeed() * 3.6f;
				model.lastSavedRigSpecData.topSpeed = num;
				view.data.topSpeed = num;
				specs.StartUpdateSpeedBar(model.lastSavedRigSpecData.topSpeed, num);
				model.SaveCurrentRigSpecData(view.data);
			}
			StartCoroutine(SaveDroneThumbnails(p_force));
		}

		public void PopulateLibrary(string p_id)
		{
			ClearFilter();
			switch (p_id)
			{
			case "frames":
				FilterByType<DroneFrame>();
				break;
			case "motors":
				FilterByType<DroneMotor>();
				break;
			case "props":
				FilterByType<DroneProp>();
				break;
			case "lipos":
				FilterByType<DroneBattery>();
				break;
			case "style":
				AddDefaultPartsFirst("SK-000");
				FilterByType<DroneSkin>();
				SortPartsByFilter(view.filter0list);
				break;
			case "extras":
				AddDefaultPartsFirst("AT-000");
				FilterByType<DroneAttachment>();
				AddDefaultPartsFirst("TX-000");
				FilterByType<DroneAntennaTx>();
				FilterByType<DroneRFCamera>();
				break;
			}
			filterPromoted.AddRange(filter);
			filter.Clear();
			filter.AddRange(filterPromoted);
			filter.AddRange(filterInDev);
			if (p_id == "style")
			{
				DronePart item = filter[0];
				filter.Sort(SortInventory);
				filter.Remove(item);
				filter.Insert(0, item);
			}
			PadFilter();
			PopulateCards();
		}

		private int SortInventory(DronePart a, DronePart b)
		{
			DRLStoreAsset component = a.GetComponent<DRLStoreAsset>();
			DRLStoreAsset component2 = b.GetComponent<DRLStoreAsset>();
			if (component == null && component2 == null)
			{
				return 0;
			}
			if (component == null)
			{
				return 1;
			}
			if (component2 == null)
			{
				return -1;
			}
			return component2.inventoryOnly.CompareTo(component.inventoryOnly);
		}

		public void ClearFilter()
		{
			filter.Clear();
			filterInDev.Clear();
			filterPromoted.Clear();
		}

		public void AddDefaultPartsFirst(params string[] p_guids)
		{
			for (int i = 0; i < library.Count; i++)
			{
				DronePart dronePart = library[i];
				if (!dronePart)
				{
					continue;
				}
				for (int j = 0; j < p_guids.Length; j++)
				{
					if (p_guids[j] == dronePart.guid)
					{
						filterPromoted.Add(dronePart);
					}
				}
			}
		}

		public void SortPartsByFilter(List<DroneAssetTagType> p_filter)
		{
			p_filter.Sort();
			List<DronePart> list = new List<DronePart>();
			list.Add(filter[0]);
			Dictionary<string, List<DronePart>> dictionary = new Dictionary<string, List<DronePart>>();
			for (int i = 0; i < p_filter.Count; i++)
			{
				for (int j = 1; j < filter.Count; j++)
				{
					if (filter[j].tags.Contains(p_filter[i]))
					{
						DRLStoreAsset component = filter[j].GetComponent<DRLStoreAsset>();
						if (component != null && component.isPromo)
						{
							list.Add(filter[j]);
							continue;
						}
						if (dictionary.ContainsKey(filter[j].info.name))
						{
							dictionary[filter[j].info.name].Add(filter[j]);
							continue;
						}
						dictionary.Add(filter[j].info.name, new List<DronePart> { filter[j] });
					}
				}
				List<string> list2 = dictionary.Keys.ToList();
				list2.Sort();
				for (int k = 0; k < list2.Count; k++)
				{
					list.AddRange(dictionary[list2[k]]);
				}
				dictionary.Clear();
			}
			filter.Clear();
			filter.AddRange(list);
		}

		public void FilterByType<T>() where T : DronePart
		{
			for (int i = 0; i < library.Count; i++)
			{
				DronePart dronePart = library[i];
				if (!dronePart)
				{
					continue;
				}
				T component = dronePart.GetComponent<T>();
				if ((bool)component && !filter.Contains(component) && !filterPromoted.Contains(component) && !filterInDev.Contains(component))
				{
					DRLStoreAsset component2 = dronePart.GetComponent<DRLStoreAsset>();
					if (component2 != null && component2.isPromo)
					{
						filterPromoted.Add(component);
					}
					else if (dronePart.GetComponent<DRLStoreAsset>().inDevelopment)
					{
						filterInDev.Add(component);
					}
					else
					{
						filter.Add(component);
					}
				}
			}
		}

		public void FilterByTag<T>(DroneAssetTagType tag1, DroneAssetTagType tag2 = DroneAssetTagType.None) where T : DronePart
		{
			for (int i = 0; i < library.Count; i++)
			{
				DronePart dronePart = library[i];
				if (!dronePart)
				{
					continue;
				}
				T component = dronePart.GetComponent<T>();
				if ((bool)component && !filter.Contains(component) && !filterPromoted.Contains(component) && !filterInDev.Contains(component) && (tag1 == DroneAssetTagType.None || dronePart.tags.Contains(tag1)) && (tag2 == DroneAssetTagType.None || dronePart.tags.Contains(tag2)))
				{
					DRLStoreAsset component2 = dronePart.GetComponent<DRLStoreAsset>();
					if (component2 != null && component2.isPromo)
					{
						filterPromoted.Add(component);
					}
					else if (dronePart.GetComponent<DRLStoreAsset>().inDevelopment)
					{
						filterInDev.Add(component);
					}
					else
					{
						filter.Add(component);
					}
				}
			}
		}

		protected FlightControllerMode GetFlightControllerMode(FCMode p_mode)
		{
			return p_mode switch
			{
				FCMode.Beginner => FlightControllerMode.Beginner, 
				FCMode.Pro => FlightControllerMode.Pro, 
				FCMode.Intermediate => FlightControllerMode.Intermediate, 
				_ => FlightControllerMode.Pro, 
			};
		}

		public void FilterFrames(DroneAssetTagType tag1)
		{
			for (int i = 0; i < library.Count; i++)
			{
				DronePart dronePart = library[i];
				if (!dronePart)
				{
					continue;
				}
				DroneFrame component = dronePart.GetComponent<DroneFrame>();
				if (!component || filter.Contains(component))
				{
					continue;
				}
				int num = 3;
				switch (tag1)
				{
				case DroneAssetTagType.None:
					num = -1;
					break;
				case DroneAssetTagType.prop3:
					num = 3;
					break;
				case DroneAssetTagType.prop4:
					num = 4;
					break;
				case DroneAssetTagType.prop5:
					num = 5;
					break;
				case DroneAssetTagType.prop6:
					num = 6;
					break;
				case DroneAssetTagType.prop7:
					num = 7;
					break;
				}
				if (component.propLimit == (float)num || num < 0)
				{
					DRLStoreAsset component2 = dronePart.GetComponent<DRLStoreAsset>();
					if (component2 != null && component2.isPromo)
					{
						filterPromoted.Add(component);
					}
					else if (dronePart.GetComponent<DRLStoreAsset>().inDevelopment)
					{
						filterInDev.Add(component);
					}
					else
					{
						filter.Add(component);
					}
				}
			}
		}

		public bool CheckLibraryAgainstTag<T>(DroneAssetTagType p_tag) where T : DronePart
		{
			for (int i = 0; i < library.Count; i++)
			{
				DronePart dronePart = library[i];
				if ((bool)dronePart && (bool)dronePart.GetComponent<T>() && dronePart.tags.Contains(p_tag))
				{
					return true;
				}
			}
			return false;
		}

		public void PadFilter()
		{
			while (filter.Count < minNumberOfItems)
			{
				filter.Add(null);
			}
			if (filter.Count % numberOfItemsPerPage > 0)
			{
				int num = numberOfItemsPerPage - filter.Count % numberOfItemsPerPage;
				for (int i = 0; i < num; i++)
				{
					filter.Add(null);
				}
			}
		}

		protected void InitLibrary()
		{
			RefreshLibrary();
			ClearFilter();
		}

		protected void RefreshLibrary()
		{
			PlayerStateModel player = base.app.model.storage.state.player;
			List<string> player_inventory = (player ? player.profile.inventory : new List<string>());
			library.Clear();
			library.AddRange(base.app.model.storage.library.FindAll<DronePart>());
			for (int i = 0; i < library.Count; i++)
			{
				for (int j = i + 1; j < library.Count; j++)
				{
					DronePart dronePart = library[i];
					DronePart dronePart2 = library[j];
					if (dronePart.guid == dronePart2.guid)
					{
						library.RemoveAt(j--);
					}
				}
			}
			library.RemoveAll(delegate(DronePart it)
			{
				DRLStoreAsset dRLStoreAsset = (it ? it.GetComponent<DRLStoreAsset>() : null);
				if (!dRLStoreAsset)
				{
					return false;
				}
				return dRLStoreAsset.inventoryOnly && !player_inventory.Contains(it.guid);
			});
			for (int num = 0; num < library.Count; num++)
			{
				switch (library[num].name)
				{
				case "frame-38":
				case "frame-39":
					library.RemoveAt(num--);
					break;
				}
			}
			DRLStoreAsset component = view.drone.body.frame.GetComponent<DRLStoreAsset>();
			DRLStoreAsset component2 = view.drone.body.frame.escs[0].motor.GetComponent<DRLStoreAsset>();
			propGuids = view.drone.body.frame.escs[0].motor.spec.AllowedProps(view.drone.body.frame.batteries[0].cells.Length);
			lipoCells = view.drone.body.frame.escs[0].motor.spec.AllowedLipos(view.drone.body.frame.escs[0].motor.prop.guid);
			string rewardParts = base.app.model.storage.state.player.profile.rewardParts;
			for (int num2 = 0; num2 < library.Count; num2++)
			{
				DronePart dronePart3 = library[num2];
				DRLStoreAsset component3 = dronePart3.GetComponent<DRLStoreAsset>();
				if (!component3)
				{
					library.RemoveAt(num2--);
					continue;
				}
				if (!string.IsNullOrEmpty(rewardParts) && rewardParts.Contains(dronePart3.guid))
				{
					component3.available = true;
					component3.inDevelopment = false;
					component3.isPromo = true;
				}
				if (!component3.available)
				{
					library.RemoveAt(num2--);
				}
				else if (component3.inDevelopment && !areInDevelopmentItemsEnabled)
				{
					library.RemoveAt(num2--);
				}
				else
				{
					if (areUnallowedItemsEnabled)
					{
						continue;
					}
					if (!DRLStoreFilter.Filter(dronePart3, component))
					{
						library.RemoveAt(num2--);
					}
					else if (!DRLStoreFilter.Filter(view.drone.body.frame, component3))
					{
						library.RemoveAt(num2--);
					}
					else if (dronePart3 is DroneProp)
					{
						if (!DRLStoreFilter.Filter(dronePart3, component2))
						{
							library.RemoveAt(num2--);
						}
						else if (!DRLStoreFilter.Filter(view.drone.body.frame.escs[0].motor, component3))
						{
							library.RemoveAt(num2--);
						}
					}
				}
			}
		}

		protected void CheckAllDronePartsAgainstLibrary(DroneRigData p_original, DroneRigData p_oldRig)
		{
			if (p_original.motor != p_oldRig.motor)
			{
				Debug.LogWarning("UIGarageRigEditController> had to replace motor because of incompatibility: " + p_oldRig.motor);
			}
			if (p_original.prop != p_oldRig.prop)
			{
				Debug.LogWarning("UIGarageRigEditController> had to replace propeller because of incompatibility: " + p_oldRig.prop);
			}
			if (p_original.battery != p_oldRig.battery)
			{
				Debug.LogWarning("UIGarageRigEditController> had to replace battery because of incompatibility: " + p_oldRig.battery);
			}
			if (p_original.attachment0 != p_oldRig.attachment0)
			{
				Debug.LogWarning("UIGarageRigEditController> had to replace attachment because of incompatibility: " + p_oldRig.attachment0);
			}
			DronePart[] parts = view.drone.body.parts;
			DroneRigData originalByFrame = model.GetOriginalByFrame(parts[0].guid);
			originalByFrame.Validate();
			if (originalByFrame == null && parts[0].guid == "F-692")
			{
				Debug.LogError("UIGarageRigEditController> F-692 frame is not in the originals!");
				return;
			}
			DroneFrame p_frame = (DroneFrame)parts[0];
			if (!CheckPartAgainstFrame(parts[1], p_frame))
			{
				RefreshDronePart(originalByFrame.motor);
				Debug.Log("UIGarageRigEditController> Motor changed, compat = " + CheckPartAgainstFrame(parts[1], p_frame));
				p_original.motor = originalByFrame.motor;
			}
			if (!CheckPartAgainstFrame(parts[2], p_frame) || !CheckPropAgainstMotor(parts[2], (DroneMotor)parts[1]))
			{
				RefreshDronePart(originalByFrame.prop);
				RefreshVideoAndDatasheet();
				Debug.Log("UIGarageRigEditController> Changed prop, compat = " + CheckPartAgainstFrame(parts[2], p_frame));
				p_original.prop = originalByFrame.prop;
			}
			if (!CheckPartAgainstFrame(parts[3], p_frame))
			{
				RefreshDronePart(originalByFrame.antenna);
				p_original.antenna = originalByFrame.antenna;
			}
			if (!CheckPartAgainstFrame(parts[4], p_frame))
			{
				RefreshDronePart(originalByFrame.battery);
				RefreshVideoAndDatasheet();
				Debug.Log("UIGarageRigEditController> Battery changed, compat = " + CheckPartAgainstFrame(parts[4], p_frame));
				p_original.battery = originalByFrame.battery;
			}
			if (!CheckPartAgainstFrame(parts[5], p_frame))
			{
				RefreshDronePart(originalByFrame.esc);
				Debug.Log("UIGarageRigEditController> Esc changed, compat = " + CheckPartAgainstFrame(parts[5], p_frame));
				p_original.esc = originalByFrame.esc;
			}
			if (!CheckPartAgainstFrame(parts[7], p_frame))
			{
				RefreshDronePart(originalByFrame.attachment0);
				Debug.Log("UIGarageRigEditController> Changed attachment, compat = " + CheckPartAgainstFrame(parts[7], p_frame));
				p_original.attachment0 = originalByFrame.attachment0;
			}
		}

		protected bool CheckPartAgainstFrame(DronePart p_part, DroneFrame p_frame)
		{
			if (areUnallowedItemsEnabled)
			{
				return true;
			}
			if (!p_part)
			{
				return true;
			}
			DRLStoreAsset component = p_part.GetComponent<DRLStoreAsset>();
			DRLStoreAsset component2 = p_frame.GetComponent<DRLStoreAsset>();
			if (!DRLStoreFilter.Filter(p_part, component2))
			{
				return false;
			}
			if (!DRLStoreFilter.Filter(p_frame, component))
			{
				return false;
			}
			return true;
		}

		protected bool CheckPropAgainstMotor(DronePart p_part, DroneMotor p_motor)
		{
			if (areUnallowedItemsEnabled)
			{
				return true;
			}
			if (!p_part)
			{
				return true;
			}
			DRLStoreAsset component = p_motor.GetComponent<DRLStoreAsset>();
			DRLStoreAsset component2 = p_part.GetComponent<DRLStoreAsset>();
			if (!DRLStoreFilter.Filter(p_part, component))
			{
				return false;
			}
			if (!DRLStoreFilter.Filter(p_motor, component2))
			{
				return false;
			}
			return true;
		}

		protected void EnableItemsInsideViewport()
		{
			DRLScrollView sv = view.scrollView;
			int currentPage = sv.currentPage;
			Dictionary<int, MonoActivity> viewport_fade_loops = m_viewport_fade_loops;
			if (viewport_fade_loops.ContainsKey(currentPage))
			{
				viewport_fade_loops[currentPage].Stop();
			}
			int start = sv.currentPage * numberOfItemsPerPage;
			float progress = 0f;
			MonoActivity value = Run(delegate(float t)
			{
				if (view == null || sv == null || view.listField == null)
				{
					return false;
				}
				progress = Mathf.Clamp01(t / sv.animationTime);
				for (int i = start; i < start + numberOfItemsPerPage; i++)
				{
					UICardButtonGarageEditItem uICardButtonGarageEditItem = view.listField.Get<UICardButtonGarageEditItem>(i);
					Hierarchy.GetComponent<CanvasGroup>(uICardButtonGarageEditItem.gameObject).alpha = Mathf.Lerp(0.25f, 1f, progress);
					if (progress >= 1f && (bool)uICardButtonGarageEditItem.storeData)
					{
						uICardButtonGarageEditItem.interactable = true;
					}
				}
				return progress < 1f;
			});
			viewport_fade_loops[currentPage] = value;
		}

		protected void DisableItemsOutsideViewport()
		{
			int currentPage = view.scrollView.currentPage;
			Dictionary<int, MonoActivity> viewport_fade_loops = m_viewport_fade_loops;
			if (viewport_fade_loops.ContainsKey(currentPage))
			{
				viewport_fade_loops[currentPage].Stop();
			}
			int start = (currentPage + 1) * numberOfItemsPerPage;
			float progress = 0f;
			MonoActivity value = Run(delegate(float t)
			{
				if (view == null)
				{
					return false;
				}
				progress = Mathf.Clamp01(t / view.scrollView.animationTime);
				for (int i = start; i < start + numberOfItemsPerPage; i++)
				{
					UICardButtonGarageEditItem uICardButtonGarageEditItem = view.listField.Get<UICardButtonGarageEditItem>(i);
					Hierarchy.GetComponent<CanvasGroup>(uICardButtonGarageEditItem.gameObject).alpha = Mathf.Lerp(1f, 0.25f, progress);
					if (progress >= 1f)
					{
						uICardButtonGarageEditItem.interactable = false;
					}
				}
				return progress < 1f;
			});
			viewport_fade_loops[currentPage] = value;
		}

		protected void PopulateCards()
		{
			if (!base.validContext)
			{
				return;
			}
			ListComponent listField = view.listField;
			listField.Clear();
			m_selection = null;
			UINavigationScroll componentInParent = listField.GetComponentInParent<UINavigationScroll>();
			if (!componentInParent)
			{
				return;
			}
			componentInParent.ResetScroll(p_force: true);
			int num = (int)Math.Ceiling((double)filter.Count / (double)numberOfItemsPerPage);
			if (num < 1)
			{
				num = 1;
			}
			int constraintCount = view.itemsGrid.constraintCount;
			int num2 = numberOfItemsPerPage / constraintCount;
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					for (int k = 0; k < constraintCount; k++)
					{
						int num3 = i * numberOfItemsPerPage + j + k * num2;
						if (num3 >= filter.Count)
						{
							continue;
						}
						UICardButtonGarageEditItem uICardButtonGarageEditItem = listField.Push<UICardButtonGarageEditItem>();
						if ((bool)uICardButtonGarageEditItem)
						{
							uICardButtonGarageEditItem.unallowed = false;
							if (filter[num3] is DroneBattery && !lipoCells.Contains(((DroneBattery)filter[num3]).cells.Length))
							{
								uICardButtonGarageEditItem.unallowed = true;
							}
							if (filter[num3] is DroneProp && !propGuids.Contains(filter[num3].guid))
							{
								uICardButtonGarageEditItem.unallowed = true;
							}
							uICardButtonGarageEditItem.Set(filter[num3]);
							if (i > 0)
							{
								uICardButtonGarageEditItem.interactable = false;
							}
						}
					}
				}
			}
			int num4 = (int)Math.Ceiling((double)filter.Count / (double)numberOfItemsPerPage);
			view.scrollView.offsetXbyPage = view.itemsGrid.spacing.x;
			view.scrollView.SetPages(num4);
			view.previousPageButton.interactable = false;
			view.nextPageButton.interactable = num4 >= 2;
			SelectPart();
		}

		protected void SetUpFiltersAndPaginationNavigation()
		{
			int num = view.scrollView.currentPage;
			if (num >= view.scrollView.totalPages)
			{
				num = view.scrollView.totalPages - 1;
			}
			if (num < 0)
			{
				num = 0;
			}
			int num2 = num * minNumberOfItems;
			UINavigation uINavigation = null;
			if (num2 < view.listField.Count && num2 >= 0 && view.listField[num2].GetComponent<UICardButtonGarageEditItem>().data != null)
			{
				uINavigation = view.listField[num2].GetComponent<UINavigation>();
			}
			UINavigation[] filtersNavigation = view.filtersNavigation;
			foreach (UINavigation uINavigation2 in filtersNavigation)
			{
				if (m_tabHasFilters)
				{
					uINavigation2.enabled = true;
					if (uINavigation != null)
					{
						uINavigation2.down = uINavigation;
					}
					else
					{
						uINavigation2.down = view.storeCardNav;
					}
				}
				else
				{
					uINavigation2.down = view.pagePreviousNav;
				}
			}
			if (view.scrollView.totalPages > 1)
			{
				view.pagePreviousNav.up = uINavigation;
				view.pageNextNav.up = uINavigation;
			}
			if (!m_tabHasFilters)
			{
				view.pagePreviousNav.up = view.filtersNavigation[0];
				view.pageNextNav.up = view.filtersNavigation[0];
			}
			if (m_tabHasColors)
			{
				view.headerDownProxy.link = view.colors.trailColorsNavigation;
			}
			else if (m_tabHasFilters)
			{
				view.headerDownProxy.link = view.filtersNavigation[0];
			}
			else
			{
				view.headerDownProxy.link = uINavigation;
			}
			if (m_tabHasColors)
			{
				view.storeCardNav.up = view.colors.edgeColorsNavigation;
			}
			else if (view.scrollView.totalPages > 1)
			{
				view.storeCardNav.up = view.pagePreviousNav;
			}
			else if (uINavigation != null)
			{
				view.storeCardNav.up = uINavigation;
			}
			else
			{
				view.storeCardNav.up = view.filtersNavigation[0];
			}
		}

		protected void SetUpGridNavigation()
		{
			ListComponent listField = view.listField;
			int num = (int)Math.Ceiling((double)filter.Count / (double)numberOfItemsPerPage);
			int constraintCount = view.itemsGrid.constraintCount;
			int num2 = numberOfItemsPerPage / constraintCount;
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					for (int k = 0; k < constraintCount; k++)
					{
						int num3 = i * numberOfItemsPerPage + j + k * num2;
						if (num3 >= listField.Count)
						{
							continue;
						}
						UICardButtonGarageEditItem uICardButtonGarageEditItem = listField.Get<UICardButtonGarageEditItem>(num3);
						listField.Get<UINavigation>(num3);
						if (!(uICardButtonGarageEditItem.data != null))
						{
							continue;
						}
						if (num3 % 3 != 0)
						{
							uICardButtonGarageEditItem.navigation.up = listField.Get<UINavigation>(num3 - 1);
						}
						else if (m_tabHasFilters)
						{
							uICardButtonGarageEditItem.navigation.up = view.filtersNavigation[0];
						}
						else
						{
							uICardButtonGarageEditItem.navigation.up = view.saveButtonNav;
						}
						if ((num3 + 1) % 3 != 0)
						{
							UINavigation down = listField.Get<UINavigation>(num3 + 1);
							if ((bool)listField.Get<UICardButtonGarageEditItem>(num3 + 1).data)
							{
								uICardButtonGarageEditItem.navigation.down = down;
							}
							else if (view.scrollView.totalPages > 1)
							{
								uICardButtonGarageEditItem.navigation.down = view.pagePreviousNav;
							}
							else
							{
								uICardButtonGarageEditItem.navigation.down = view.storeCardNav;
							}
						}
						else if (view.scrollView.totalPages > 1)
						{
							uICardButtonGarageEditItem.navigation.down = view.pagePreviousNav;
						}
						else
						{
							uICardButtonGarageEditItem.navigation.down = view.storeCardNav;
						}
						if (num3 + 3 < listField.Count)
						{
							UICardButtonGarageEditItem uICardButtonGarageEditItem2 = listField.Get<UICardButtonGarageEditItem>(num3 + 3);
							if ((bool)uICardButtonGarageEditItem2.data)
							{
								uICardButtonGarageEditItem.navigation.right = (((k + 1) % 3 == 0) ? null : uICardButtonGarageEditItem2.navigation);
							}
						}
						if (num3 / (3 + i * minNumberOfItems) > 0)
						{
							uICardButtonGarageEditItem.navigation.left = listField.Get<UINavigation>(num3 - 3);
						}
						if (uICardButtonGarageEditItem.navigation.left == null)
						{
							uICardButtonGarageEditItem.navigation.left = view.rigNameField.GetComponent<UINavigation>();
						}
					}
				}
			}
		}

		protected void OnHoverPart(UICardButtonGarageEditItem p_item)
		{
			if (p_item == null)
			{
				return;
			}
			DRLAsset data = p_item.data;
			if (!m_tabHasInformation || data == null || p_item.storeData.removeItem)
			{
				view.informationPanelFader.FadeOut();
				return;
			}
			view.informationPanelFader.FadeIn();
			view.informationTitle.text = "";
			view.informationBrandText.gameObject.SetActive(value: false);
			view.informationBrandImage.gameObject.SetActive(value: false);
			for (int i = 0; i < view.informationCaption.Length; i++)
			{
				view.informationCaption[i].text = "";
				view.informationValue[i].text = "";
				view.informationWide[i].text = "";
			}
			if (data.info.logo != null)
			{
				view.informationARBrandImage.aspectRatio = (float)Math.Round((double)(data.info.logo.width / data.info.logo.height), 3);
				view.informationBrandImage.texture = data.info.logo;
				view.informationBrandImage.gameObject.SetActive(value: true);
			}
			else
			{
				view.informationBrandText.text = data.info.brand;
				view.informationBrandText.gameObject.SetActive(value: true);
			}
			view.informationTitle.text = data.info.name;
			int num = 0;
			switch (view.tabGroup.selection)
			{
			case "frames":
				view.informationCaption[num].text = base.app.model.storage.locale.Get("garage.information.size.label", "Size:");
				view.informationValue[num++].text = ((DroneFrame)data).propLimit + "\"";
				break;
			case "motors":
				view.informationCaption[num].text = base.app.model.storage.locale.Get("garage.information.stator.label", "Stator:");
				view.informationValue[num++].text = ((DroneMotor)data).spec.statorWidth.ToString("00") + ((DroneMotor)data).spec.statorHeight.ToString("00");
				view.informationCaption[num].text = base.app.model.storage.locale.Get("garage.information.gear.label", "Gear:");
				view.informationValue[num++].text = ((DroneMotor)data).spec.kv + "kv";
				break;
			case "props":
				view.informationCaption[num].text = base.app.model.storage.locale.Get("garage.information.size.label", "Size:");
				view.informationValue[num++].text = ((DroneProp)data).diameter + "\"";
				view.informationCaption[num].text = base.app.model.storage.locale.Get("garage.information.blades.label", "Blades:");
				view.informationValue[num++].text = ((DroneProp)data).blades.ToString();
				break;
			case "lipos":
				view.informationCaption[num].text = base.app.model.storage.locale.Get("garage.information.capacity.label", "Capacity:");
				view.informationValue[num++].text = ((DroneBattery)data).capacity + " mAh";
				view.informationCaption[num].text = base.app.model.storage.locale.Get("garage.information.cells.label", "Cells:");
				view.informationValue[num++].text = ((DroneBattery)data).cells.Length.ToString();
				break;
			case "extras":
				if (((DronePart)data) is DroneAttachment)
				{
					view.informationCaption[num].text = base.app.model.storage.locale.Get("garage.information.type.label", "Type:");
					view.informationValue[num++].text = ((DroneAttachment)data).type.ToString();
				}
				break;
			}
			view.informationCaption[num].text = base.app.model.storage.locale.Get("garage.information.weight.label", "Weight:");
			view.informationValue[num++].text = ((DronePart)data).weight + " g";
			string text = view.tabGroup.selection;
			if (text != null && text == "frames" && ((DroneFrame)data).gatechDragData != null)
			{
				view.informationWide[num].text = "<size=14><color=yellow>*Advanced unsteady drag model</color></size>";
			}
		}

		protected DronePart FindDronePartInLibrary(string p_guid)
		{
			for (int i = 0; i < library.Capacity; i++)
			{
				if (library[i].guid == p_guid)
				{
					return library[i];
				}
			}
			return null;
		}

		protected void SelectPart()
		{
			ListComponent listField = view.listField;
			string text = "";
			DroneRigData data = view.data;
			if (data == null)
			{
				return;
			}
			switch (view.tabGroup.selection)
			{
			case "frames":
				text = data.frame;
				break;
			case "motors":
				text = data.motor;
				break;
			case "props":
				text = data.prop;
				break;
			case "lipos":
				text = data.battery;
				break;
			case "extras":
				text = data.attachment0;
				break;
			case "style":
				text = data.skinFrame;
				break;
			}
			for (int i = 0; i < listField.Count; i++)
			{
				UICardButtonGarageEditItem uICardButtonGarageEditItem = listField.Get<UICardButtonGarageEditItem>(i);
				if ((bool)uICardButtonGarageEditItem && (bool)uICardButtonGarageEditItem.data && uICardButtonGarageEditItem.data.guid == text)
				{
					selection = uICardButtonGarageEditItem;
					OnHoverPart(uICardButtonGarageEditItem);
					break;
				}
			}
		}

		protected void MarkRigDirty()
		{
			m_droneRigDirty = true;
			view.SavingAnimation(p_flag: false);
		}

		protected void MarkRigClean()
		{
			m_droneRigDirty = false;
		}

		protected void ChangePart(UICardButtonGarageEditItem p_part)
		{
			DroneRigData data = view.data;
			DroneRigData rawData = view.rawData;
			if (data == null)
			{
				return;
			}
			DronePart dronePart = (DronePart)p_part.data;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = true;
			switch (view.tabGroup.selection)
			{
			case "frames":
				rawData.frame = dronePart.guid;
				flag = true;
				break;
			case "motors":
				rawData.motor = (data.motor = dronePart.guid);
				RefreshDronePart(dronePart.guid);
				flag2 = true;
				Notify("garage.edit.change-part");
				break;
			case "props":
				rawData.prop = (data.prop = dronePart.guid);
				RefreshDronePart(dronePart.guid);
				flag2 = true;
				break;
			case "lipos":
				rawData.battery = (data.battery = dronePart.guid);
				RefreshDronePart(dronePart.guid);
				flag2 = true;
				Notify("garage.edit.change-part");
				break;
			case "extras":
				if (dronePart is DroneAntennaTx)
				{
					rawData.antenna = (data.antenna = dronePart.guid);
					RefreshDronePart(dronePart.guid);
					Notify("garage.edit.change-part");
				}
				else if (dronePart is DroneAttachment)
				{
					rawData.attachment0 = (data.attachment0 = dronePart.guid);
					RefreshDronePart(dronePart.guid);
					Notify("garage.edit.change-part");
				}
				else if (dronePart is DroneRFCamera)
				{
					rawData.camera = (data.camera = dronePart.guid);
					RefreshDronePart(dronePart.guid);
					Notify("garage.edit.change-part");
				}
				break;
			case "style":
				rawData.skinFrame = (data.skinFrame = dronePart.guid);
				RefreshDronePart(dronePart.guid);
				Notify("garage.edit.change-style");
				flag3 = false;
				break;
			}
			if (flag2 && !areUnallowedItemsEnabled)
			{
				DroneRigData originalByFrame = model.GetOriginalByFrame(rawData.frame);
				DroneMotor p_check = base.app.model.storage.library.FindByGUID<DroneMotor>(data.motor);
				DroneProp p_part2 = base.app.model.storage.library.FindByGUID<DroneProp>(data.prop);
				base.app.model.storage.library.FindByGUID<DroneBattery>(data.battery);
				if (!DRLStoreFilter.Filter(p_part2, p_check))
				{
					data.prop = originalByFrame.prop;
					RefreshDronePart(originalByFrame.prop);
				}
				RefreshLibrary();
				RefreshVideoAndDatasheet();
			}
			if (flag)
			{
				DroneRigData clonedOriginalbyFrame = model.GetClonedOriginalbyFrame(rawData.frame);
				if (clonedOriginalbyFrame != null)
				{
					clonedOriginalbyFrame.guid = data.guid;
					clonedOriginalbyFrame.color0 = data.color0;
					clonedOriginalbyFrame.color1 = data.color1;
					clonedOriginalbyFrame.color2 = data.color2;
					clonedOriginalbyFrame.name = view.rigName;
					clonedOriginalbyFrame.thumb0 = data.thumb0;
					clonedOriginalbyFrame.thumb1 = data.thumb1;
					if (view.drone != null && !areUnallowedItemsEnabled)
					{
						DroneFrame p_check2 = base.app.model.storage.library.FindByGUID<DroneFrame>(rawData.frame);
						DroneMotor droneMotor = base.app.model.storage.library.FindByGUID<DroneMotor>(rawData.motor);
						if (!string.IsNullOrEmpty(rawData.motor) && DRLStoreFilter.Filter(droneMotor, p_check2))
						{
							clonedOriginalbyFrame.motor = rawData.motor;
						}
						else
						{
							droneMotor = base.app.model.storage.library.FindByGUID<DroneMotor>(clonedOriginalbyFrame.motor);
						}
						DroneProp p_part3 = base.app.model.storage.library.FindByGUID<DroneProp>(rawData.prop);
						if (!string.IsNullOrEmpty(rawData.prop) && DRLStoreFilter.Filter(p_part3, p_check2) && DRLStoreFilter.Filter(p_part3, droneMotor))
						{
							clonedOriginalbyFrame.prop = rawData.prop;
						}
						DroneBattery battery = base.app.model.storage.library.FindByGUID<DroneBattery>(rawData.battery);
						DroneBattery droneBattery = base.app.model.storage.library.FindByGUID<DroneBattery>(clonedOriginalbyFrame.battery);
						if (!string.IsNullOrEmpty(rawData.battery) && DRLStoreFilter.Filter(battery, p_check2))
						{
							clonedOriginalbyFrame.battery = rawData.battery;
						}
						else if (battery.cells.Length != droneBattery.cells.Length)
						{
							List<DroneBattery> list = base.app.model.storage.library.FindAll((DroneBattery it) => it.cells.Length == battery.cells.Length);
							for (int num = 0; num < list.Count; num++)
							{
								if (DRLStoreFilter.Filter(list[num], p_check2))
								{
									clonedOriginalbyFrame.battery = list[num].guid;
									break;
								}
							}
						}
						if (!string.IsNullOrEmpty(rawData.attachment0) && DRLStoreFilter.Filter(base.app.model.storage.library.FindByGUID<DroneAttachment>(rawData.attachment0), p_check2))
						{
							clonedOriginalbyFrame.attachment0 = rawData.attachment0;
						}
					}
					else
					{
						clonedOriginalbyFrame.motor = ((string.IsNullOrEmpty(data.motor) || data.motor == "M-000") ? rawData.motor : data.motor);
						clonedOriginalbyFrame.battery = ((string.IsNullOrEmpty(data.battery) || data.battery == "B-000") ? rawData.battery : data.battery);
						clonedOriginalbyFrame.prop = ((string.IsNullOrEmpty(data.prop) || data.prop == "P-000") ? rawData.prop : data.prop);
						clonedOriginalbyFrame.attachment0 = ((string.IsNullOrEmpty(data.attachment0) || data.attachment0 == "AT-000") ? rawData.attachment0 : data.attachment0);
					}
					clonedOriginalbyFrame.skinFrame = ((string.IsNullOrEmpty(data.skinFrame) || data.skinFrame == "SK-000") ? rawData.skinFrame : data.skinFrame);
					clonedOriginalbyFrame.physics = "PH-000";
					clonedOriginalbyFrame.Validate();
					view.data = clonedOriginalbyFrame;
				}
				RefreshDroneParts(clonedOriginalbyFrame);
				view.drone.OnEvent.AddListener(OnDroneEvent);
				LoadDroneName();
				MarkRigDirty();
				RefreshLibrary();
				CheckAllDronePartsAgainstLibrary(clonedOriginalbyFrame, data);
			}
			RefreshCenterOfGravity();
			if (flag3)
			{
				RefreshBars();
			}
			if (view.tabGroup.selection == "props")
			{
				PropellersAnimation(1.2f);
			}
			MarkRigDirty();
			if (flag)
			{
				Notify("garage.edit.change-frame", view.drone.body.frame.gameObject);
			}
		}

		protected void PropellersAnimation(float p_duration = 0f)
		{
			float angle = 0f;
			if (m_propellersRotation != null)
			{
				m_propellersRotation.Stop();
			}
			m_propellersRotation = Run(delegate(float p_elapsed)
			{
				if ((p_duration > 0f && p_elapsed > p_duration) || view == null || view.drone == null || view.drone.body.frame.escs[0].motor == null || view.drone.body.frame.escs[0].motor.animation == null || view.drone.body.frame.escs[0].motor.animation.cap == null)
				{
					if (view != null && view.SpinToggle.toggle.isOn)
					{
						view.SpinToggle.SetState(p_flag: false);
						view.SpinToggle.toggle.isOn = false;
					}
					return false;
				}
				float num = 0.06f;
				if (p_duration > 0f)
				{
					num = propellerRotation.Evaluate(p_elapsed / p_duration);
				}
				for (int i = 0; i < 4; i++)
				{
					if (view.drone.body.frame.escs[i].motor.ccw)
					{
						view.drone.body.frame.escs[i].motor.animation.cap.Rotate(-Vector3.forward * (num * propellerRotationBaseSpeed - angle));
					}
					else
					{
						view.drone.body.frame.escs[i].motor.animation.cap.Rotate(Vector3.forward * (num * propellerRotationBaseSpeed - angle));
					}
				}
				if (p_duration > 0f)
				{
					angle += num * propellerRotationBaseSpeed - angle;
				}
				return true;
			});
			if (p_duration > 0f)
			{
				Notify("garage.edit.prop-spin-impulse", view.drone.body.frame.gameObject);
			}
			else
			{
				Notify("garage.edit.prop-spin-start", view.drone.body.frame.gameObject);
			}
		}

		protected void StoppingPropellersAnimation()
		{
			float angle = 0f;
			if (m_propellersRotation != null)
			{
				m_propellersRotation.Stop();
			}
			m_propellersRotation = Run(delegate(float p_elapsed)
			{
				if (p_elapsed > 1f || view == null || view.drone == null || view.drone.body == null || view.drone.body.frame == null || view.drone.body.frame.escs[0].motor == null || view.drone.body.frame.escs[0].motor.animation == null || view.drone.body.frame.escs[0].motor.animation.cap == null || propellerRotation == null)
				{
					if (view != null && view.SpinToggle != null && view.SpinToggle.toggle != null && view.SpinToggle.toggle.isOn)
					{
						view.SpinToggle.SetState(p_flag: false);
						view.SpinToggle.toggle.isOn = false;
					}
					return false;
				}
				float num = propellerRotation.Evaluate(0.03f + p_elapsed);
				for (int i = 0; i < 4; i++)
				{
					if (view.drone.body.frame.escs[i] == null || view.drone.body.frame.escs[i].motor == null)
					{
						if (view != null && view.SpinToggle != null && view.SpinToggle.toggle != null && view.SpinToggle.toggle.isOn)
						{
							view.SpinToggle.SetState(p_flag: false);
							view.SpinToggle.toggle.isOn = false;
						}
						return false;
					}
					if (view.drone.body.frame.escs[i].motor.ccw)
					{
						view.drone.body.frame.escs[i].motor.animation.cap.Rotate(-Vector3.forward * (num * propellerRotationBaseSpeed - angle));
					}
					else
					{
						view.drone.body.frame.escs[i].motor.animation.cap.Rotate(Vector3.forward * (num * propellerRotationBaseSpeed - angle));
					}
				}
				angle += num * propellerRotationBaseSpeed - angle;
				return true;
			});
			Notify("garage.edit.prop-spin-stop", view.drone.body.frame.gameObject);
		}

		protected void StopPropellersAnimation()
		{
			if (view.SpinToggle.toggle.isOn)
			{
				view.SpinToggle.SetState(p_flag: false);
				view.SpinToggle.toggle.isOn = false;
			}
			if (m_propellersRotation != null)
			{
				m_propellersRotation.Stop();
			}
			StoppingPropellersAnimation();
		}

		public bool ChangeProfileAndTrailColor(Color p_color)
		{
			bool result = allowChangeProfileColor;
			if (allowChangeProfileColor)
			{
				allowChangeProfileColor = false;
				base.app.model.storage.state.player.profile.color = p_color;
				view.drone.renderer.playerColor = p_color;
				if (base.app.model.game != null)
				{
					GamePlayerData playerData = base.app.model.game.GetPlayerData(view.drone);
					if (playerData != null)
					{
						playerData.color = p_color;
					}
				}
				CreateTrailsAnimation();
				RunOnce(2f, delegate
				{
					allowChangeProfileColor = true;
				});
				MarkRigDirty();
			}
			return result;
		}

		public static bool SameColorNoAlpha(Color c1, Color c2)
		{
			if (Mathf.Abs(c1.r - c2.r) < 0.003f && Mathf.Abs(c1.g - c2.g) < 0.003f)
			{
				return Mathf.Abs(c1.b - c2.b) < 0.003f;
			}
			return false;
		}

		protected void CreateTrailsAnimation()
		{
			Vector3 localScale = Vector3.one;
			switch (view.data.diameter)
			{
			case 3:
				localScale = view.trailsScale3inch;
				break;
			case 4:
				localScale = view.trailsScale4inch;
				break;
			case 5:
				localScale = view.trailsScale5inch;
				break;
			case 6:
				localScale = view.trailsScale6inch;
				break;
			}
			m_trailsObjectReference = UnityEngine.Object.Instantiate(view.trailsAnimationPrefab, null);
			m_trailsObjectReference.transform.localScale = localScale;
			m_trailsObjectReference.transform.SetParent(view.drone.transform.parent);
			m_trailsObjectReference.transform.localPosition = Vector3.zero;
			GameObjectList component = m_trailsObjectReference.GetComponent<GameObjectList>();
			for (int i = 0; i < component.list.Count; i++)
			{
				component.list[i].GetComponent<TrailRenderer>().material.SetColor("_TintColor", view.drone.renderer.trailsColor);
			}
		}

		public void ChangeEdgeColor(Color p_color)
		{
			view.data.color1 = p_color;
			view.drone.renderer.color1 = p_color;
			view.thumbnailDrone.renderer.color1 = p_color;
			MarkRigDirty();
		}

		public void ChangePropColors(Color p_color)
		{
			view.data.color2 = p_color;
			view.drone.renderer.color2 = p_color;
			view.thumbnailDrone.renderer.color2 = p_color;
			MarkRigDirty();
		}

		public void ChangeTextureColor(Color p_color)
		{
			view.data.color0 = p_color;
			view.drone.renderer.color0 = p_color;
			view.thumbnailDrone.renderer.color0 = p_color;
			MarkRigDirty();
		}

		protected void ChangePropColors(Color p_colorFl, Color p_colorFr, Color p_colorBl, Color p_colorBr)
		{
		}

		protected void LoadExternalDrone()
		{
			view.player = base.app.model.game.GetPlayerData(view.externalDrone);
			view.usingExternalDrone = true;
			view.externalDrone.fc.armed = false;
			Notify("game.simulation.drone@disarmed", view.externalDrone);
			view.externalDrone.rigidbody.frozen = true;
			view.drone = view.externalDrone;
			view.drone.OnEvent.AddListener(OnDroneEvent);
			base.app.model.game.camera.inGarage = true;
			foreach (DroneESC esc in view.externalDrone.body.frame.escs)
			{
				esc.motor.ForceStop();
			}
			view.drone.MakeStatic(p_flag: true);
		}

		protected void MoveDroneToGarageScene()
		{
			view.viewerCamera.gameObject.SetActive(value: true);
			GameObject gameObject = GameObject.Find("/environment/node-drone");
			if (gameObject == null)
			{
				Debug.LogError("UIGarageRigEditController> Could not find drone-node in Garage scene");
				return;
			}
			view.viewerTransform.gameObject.SetActive(value: true);
			view.viewerTransform.SetParent(base.app.transform.parent);
			view.viewerTransform.localScale = Vector3.one;
			view.viewerTransform.position = gameObject.transform.position;
			view.viewerTransform.rotation = gameObject.transform.rotation;
			SetTab("frames");
			if (view.isOpenedFromStore)
			{
				SetPurchasedItem();
			}
			if (view.externalDrone == null)
			{
				return;
			}
			m_radioEnabled = base.app.model.game.level.radio.enabled;
			base.app.model.game.level.radio.enabled = false;
			m_externalDroneParent = view.externalDrone.transform.parent;
			if (base.app.model.game != null && base.app.model.game.simulation != null && base.app.model.game.simulation.podiums != null && base.app.model.game.simulation.podiums.list != null && base.app.model.game.simulation.podiums.list.Count > 0 && base.app.model.game.simulation.podiums.list[0] != null && base.app.model.game.simulation.podiums.list[0].spawn != null)
			{
				m_externalDronePosition = base.app.model.game.simulation.podiums.list[0].spawn.position;
				m_externalDroneRotation = base.app.model.game.simulation.podiums.list[0].spawn.rotation;
			}
			else
			{
				m_externalDronePosition = view.externalDrone.position;
				m_externalDroneRotation = view.externalDrone.transform.rotation;
			}
			view.externalDrone.transform.SetParent(view.droneContainer);
			view.externalDrone.transform.localScale = Vector3.one;
			view.externalDrone.transform.rotation = view.droneContainer.rotation;
			view.externalDrone.position = view.droneContainer.position;
			m_droneCameraMode = base.app.model.game.camera.mode;
			base.app.model.game.camera.main.gameObject.SetActive(value: false);
			Activity.Run(delegate
			{
				if (view != null && view.externalDrone != null)
				{
					view.externalDrone.fc.armed = false;
					view.externalDrone.renderer.shadowsOnly = false;
					view.externalDrone.renderer.SetTrailsEnabled(p_flag: false);
					view.externalDrone.rigidbody.frozen = true;
					view.externalDrone.rigidbody.isKinematic = true;
					view.externalDrone.transform.localScale = Vector3.one;
					view.externalDrone.transform.rotation = view.droneContainer.rotation;
					view.externalDrone.position = view.droneContainer.position;
					view.externalDrone.SetMotorRPM(0f);
					RefreshCenterOfGravity();
					FadeInTheCOG();
				}
			}, 2f, 0f, false);
		}

		private void SetPurchasedItem()
		{
			this.TimerRunOnce(delegate
			{
				SetTab("style");
				for (int i = 0; i < view.listField.Count; i++)
				{
					UICardButtonGarageEditItem uICardButtonGarageEditItem = view.listField.Get<UICardButtonGarageEditItem>(i);
					if (uICardButtonGarageEditItem != null && uICardButtonGarageEditItem.data != null)
					{
						if (uICardButtonGarageEditItem.data.name == view.currentProduct.name)
						{
							UICardButtonGarageEditItem uICardButtonGarageEditItem2 = view.listField.Get<UICardButtonGarageEditItem>(i);
							Debug.Log(uICardButtonGarageEditItem2.name + " was found!!!!");
							uICardButtonGarageEditItem2.Notify("garage.edit.item@click", uICardButtonGarageEditItem2);
						}
						else
						{
							uICardButtonGarageEditItem.selected = false;
						}
					}
				}
			}, 0.05f);
		}

		protected void ReturnExternalDrone()
		{
			view.RestoreMainCamera();
			_ = view.externalDrone;
			if ((bool)view.externalDrone)
			{
				view.externalDrone.body.RecalculateWeight();
				view.externalDrone.transform.SetParent(m_externalDroneParent);
				base.app.controller.game.PodiumReset(view.externalDrone);
				FCProfileData active = base.app.model.storage.state.player.settings.tuning.GetActive();
				if (active != null)
				{
					view.externalDrone.fc.profile.SetData(active);
				}
				int counter = 10;
				DroneRigidbody rb = view.externalDrone.rigidbody;
				Activity.Run((Func<bool>)delegate
				{
					if (counter-- < 0)
					{
						return false;
					}
					if (rb != null)
					{
						rb.isKinematic = counter > 0;
					}
					return true;
				}, 0f, false);
				view.externalDrone.fc.armed = true;
				if (m_droneCameraMode == DroneCameraModeType.FPV)
				{
					view.externalDrone.renderer.shadowsOnly = true;
				}
				view.externalDrone.MakeStatic(p_flag: false);
				foreach (DroneESC esc in view.externalDrone.body.frame.escs)
				{
					esc.motor.Unlock();
				}
			}
			view.usingExternalDrone = false;
			view.externalDrone = null;
			view.drone = null;
			m_previousTab = null;
			SetTab("frames");
			view.viewerCamera.gameObject.SetActive(value: false);
			base.app.model.game.level.radio.enabled = m_radioEnabled;
		}

		protected void RefreshDronePart(string p_guid)
		{
			Drone drone = view.drone;
			view.drone = base.app.model.storage.factory.ReplacePart(view.drone, p_guid);
			if (!p_guid.StartsWith("F-"))
			{
				view.thumbnailDrone = base.app.model.storage.factory.ReplacePart(view.thumbnailDrone, p_guid);
			}
			else
			{
				view.thumbnailDrone = base.app.model.storage.factory.Replace(view.drone, view.thumbnailDrone, null, null, p_async: false);
			}
			if (view.usingExternalDrone)
			{
				view.player.drone = view.drone;
				view.externalDrone = view.drone;
			}
			if (drone != view.drone && base.app.controller.game != null && base.app.controller.game.replay != null && base.app.controller.game.replay.recorder != null && base.app.controller.game.replay.recorder.model != null)
			{
				base.app.controller.game.replay.recorder.model.Replace(drone, view.drone);
			}
			view.drone.body.RecalculateWeight();
			foreach (DroneESC esc in view.drone.body.frame.escs)
			{
				esc.motor.RefreshData();
			}
			if (view.drone.body.frame.gatechDragData != null)
			{
				view.georgiaTechLabel.FadeIn(0.2f);
			}
			else
			{
				view.georgiaTechLabel.FadeOut(0.2f);
			}
		}

		protected void RefreshDroneParts(DroneRigData p_rig)
		{
			Drone drone = view.drone;
			view.drone = base.app.model.storage.factory.UpdateRig(view.drone, p_rig);
			if (view.thumbnailDrone.rig.frame == p_rig.frame)
			{
				view.thumbnailDrone = base.app.model.storage.factory.UpdateRig(view.thumbnailDrone, p_rig);
			}
			else
			{
				view.thumbnailDrone = base.app.model.storage.factory.Replace(view.drone, view.thumbnailDrone, null, null, p_async: false);
			}
			if (view.usingExternalDrone)
			{
				view.player.drone = view.drone;
				view.externalDrone = view.drone;
			}
			if (drone != view.drone && base.app.controller.game != null && base.app.controller.game.replay != null && base.app.controller.game.replay.recorder != null && base.app.controller.game.replay.recorder.model != null)
			{
				base.app.controller.game.replay.recorder.model.Replace(drone, view.drone);
			}
			view.drone.body.RecalculateWeight();
			if (view.drone.body.frame.gatechDragData != null)
			{
				view.georgiaTechLabel.FadeIn(0.2f);
			}
			else
			{
				view.georgiaTechLabel.FadeOut(0.2f);
			}
		}

		protected void CreateNewDrone()
		{
			Drone drone = view.drone;
			if ((bool)drone)
			{
				drone.OnEvent.RemoveAllListeners();
			}
			DroneRigData data = view.data;
			Transform droneContainer = view.droneContainer;
			view.drone = base.app.model.storage.factory.Replace(data, drone, droneContainer, droneContainer, p_async: false);
			view.drone.rigidbody.isKinematic = true;
			view.drone.OnEvent.AddListener(OnDroneEvent);
			view.drone.renderer.SetTrailsEnabled(p_flag: false);
			view.drone.localPosition = Vector3.zero;
			view.drone.transform.localScale = Vector3.one;
			view.drone.transform.localRotation = Quaternion.identity;
			view.drone.MakeStatic(p_flag: true);
		}

		protected void CreateThumbnailDrone()
		{
			_ = view.data;
			Transform droneContainer = view.droneContainer;
			if ((bool)view.thumbnailDrone)
			{
				view.thumbnailDrone.OnEvent.RemoveAllListeners();
				UnityEngine.Object.Destroy(view.thumbnailDrone.gameObject);
			}
			view.thumbnailDrone = base.app.model.storage.factory.Replace(view.drone, view.thumbnailDrone, droneContainer, droneContainer, p_async: false);
			view.thumbnailDrone.OnEvent.AddListener(OnDroneEvent);
			AssertThumbnailDrone();
		}

		protected void AssertThumbnailDrone()
		{
			view.thumbnailDrone.localPosition = new Vector3(-1000f, -1000f, -1000f);
			view.thumbnailDrone.transform.localScale = Vector3.one;
			view.thumbnailDrone.MakeStatic(p_flag: true);
			view.drone.rigidbody.isKinematic = true;
		}

		protected void LoadDroneName()
		{
			view.rigName = view.data.name;
			OnRigNameValueEntered();
		}

		protected void RefreshBars()
		{
			DroneRigData data = view.data;
			DroneRigSpecData droneSpecData = model.GetDroneSpecData(data);
			bool flag = !areUnallowedItemsEnabled && (droneSpecData.thrust < 10f || droneSpecData.torque < 1E-06f || droneSpecData.rpm < 1000f);
			specs.RefreshBars(droneSpecData, model, flag, view.drone, view.data);
			if (flag)
			{
				view.flyButtonElementView.interactable = false;
				view.saveButtonElementView.interactable = false;
			}
			else
			{
				view.flyButtonElementView.interactable = true;
				view.saveButtonElementView.interactable = true;
			}
		}

		protected void RefreshVideoAndDatasheet()
		{
			DroneRigData data = view.data;
			model.GetDroneSpecData(data);
			view.datasheetButton.interactable = true;
			view.videoButton.interactable = false;
		}

		protected void RefreshCenterOfGravity()
		{
			if (!(view.drone == null))
			{
				view.droneCOGMarker.centerOfMass = view.drone.body.centerOfMassMarker;
				view.droneCOGAxes.transform.position = view.drone.body.centerOfMassMarker.position;
				view.droneCOGAxes.transform.rotation = view.drone.body.centerOfMassMarker.rotation;
			}
		}

		protected void OnDroneEvent(DroneEvent p_event)
		{
			if (p_event.type != DroneEventType.Ready)
			{
				return;
			}
			Drone target = p_event.target;
			if (target == view.drone)
			{
				target.fc.armed = false;
				target.renderer.shadowsOnly = false;
				view.drone.renderer.SetTrailsEnabled(p_flag: false);
				view.drone.rigidbody.frozen = true;
				{
					foreach (DroneESC esc in target.body.frame.escs)
					{
						esc.motor.ForceStop();
					}
					return;
				}
			}
			if (target == view.thumbnailDrone)
			{
				target.fc.armed = false;
				target.fc.allowPitch = (target.fc.allowRoll = (target.fc.allowYaw = (target.fc.allowThrottle = false)));
				target.renderer.SetTrailsEnabled(p_flag: false);
				target.renderer.visible = true;
				target.gameObject.SetActive(value: false);
				target.rigidbody.frozen = true;
			}
			else
			{
				Debug.LogError("UIGarageRigEditController> got ready event from unknown drone");
			}
		}

		protected void ApplyFilters()
		{
			ClearFilter();
			DroneAssetTagType tag = ((view.filter0.index != 0) ? view.filter0list[view.filter0.index] : DroneAssetTagType.None);
			DroneAssetTagType tag2 = ((view.filter1.index != 0) ? view.filter1list[view.filter1.index] : DroneAssetTagType.None);
			switch (view.tabGroup.selection)
			{
			case "frames":
				FilterFrames(tag);
				break;
			case "motors":
				FilterByTag<DroneMotor>(tag, tag2);
				break;
			case "props":
				FilterByTag<DroneProp>(tag, tag2);
				break;
			case "lipos":
				FilterByTag<DroneBattery>(tag, tag2);
				break;
			case "style":
				AddDefaultPartsFirst("SK-000");
				FilterByTag<DroneSkin>(tag);
				SortPartsByFilter(view.filter0list);
				break;
			}
			filterPromoted.AddRange(filter);
			filter.Clear();
			filter.AddRange(filterPromoted);
			if (areInDevelopmentItemsEnabled)
			{
				filter.AddRange(filterInDev);
			}
			PadFilter();
			PopulateCards();
			SetUpGridNavigation();
			SetUpFiltersAndPaginationNavigation();
		}

		protected void ClearFilterLabels()
		{
			view.filter0.values.Clear();
			view.filter0list.Clear();
			view.filter0.min = 0;
			view.filter0.max = 0;
			view.filter0.Refresh();
			view.filter1.values.Clear();
			view.filter1list.Clear();
			view.filter1.min = 0;
			view.filter1.max = 0;
			view.filter1.Refresh();
		}

		protected void PopulateFilters(string p_tab, bool p_both)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			switch (p_tab)
			{
			case "frames":
				num = (int)view.filterNames.frame0.x;
				num2 = (int)view.filterNames.frame0.y;
				break;
			case "motors":
				num = (int)view.filterNames.motor0.x;
				num2 = (int)view.filterNames.motor0.y;
				num3 = (int)view.filterNames.motor1.x;
				num4 = (int)view.filterNames.motor1.y;
				break;
			case "props":
				num = (int)view.filterNames.prop0.x;
				num2 = (int)view.filterNames.prop0.y;
				num3 = (int)view.filterNames.prop1.x;
				num4 = (int)view.filterNames.prop1.y;
				break;
			case "lipos":
				num = (int)view.filterNames.battery0.x;
				num2 = (int)view.filterNames.battery0.y;
				num3 = (int)view.filterNames.battery1.x;
				num4 = (int)view.filterNames.battery1.y;
				break;
			case "style":
				num = (int)view.filterNames.skins0.x;
				num2 = (int)view.filterNames.skins0.y;
				break;
			}
			view.filter0.values.Add(view.filterNames.filterNames[(DroneAssetTagType)num]);
			view.filter0list.Add((DroneAssetTagType)num);
			for (int i = num + 1; i < num2; i++)
			{
				switch (p_tab)
				{
				case "motors":
					if (!CheckLibraryAgainstTag<DroneMotor>((DroneAssetTagType)i))
					{
						continue;
					}
					break;
				case "props":
					if (!CheckLibraryAgainstTag<DroneProp>((DroneAssetTagType)i))
					{
						continue;
					}
					break;
				case "lipos":
					if (!CheckLibraryAgainstTag<DroneBattery>((DroneAssetTagType)i))
					{
						continue;
					}
					break;
				case "style":
					if (!CheckLibraryAgainstTag<DroneSkin>((DroneAssetTagType)i))
					{
						continue;
					}
					break;
				}
				view.filter0.max++;
				view.filter0.values.Add(view.filterNames.filterNames[(DroneAssetTagType)i]);
				view.filter0list.Add((DroneAssetTagType)i);
			}
			view.filter0.SetValue(view.filter0.values[0]);
			if (!p_both)
			{
				return;
			}
			view.filter1.values.Add(view.filterNames.filterNames[(DroneAssetTagType)num3]);
			view.filter1list.Add((DroneAssetTagType)num3);
			for (int j = num3 + 1; j < num4; j++)
			{
				switch (p_tab)
				{
				case "motors":
					if (!CheckLibraryAgainstTag<DroneMotor>((DroneAssetTagType)j))
					{
						continue;
					}
					break;
				case "props":
					if (!CheckLibraryAgainstTag<DroneProp>((DroneAssetTagType)j))
					{
						continue;
					}
					break;
				case "lipos":
					if (!CheckLibraryAgainstTag<DroneBattery>((DroneAssetTagType)j))
					{
						continue;
					}
					break;
				case "style":
					if (!CheckLibraryAgainstTag<DroneSkin>((DroneAssetTagType)j))
					{
						continue;
					}
					break;
				}
				view.filter1.max++;
				view.filter1.values.Add(view.filterNames.filterNames[(DroneAssetTagType)j]);
				view.filter1list.Add((DroneAssetTagType)j);
			}
			view.filter1.SetValue(view.filter1.values[0]);
		}

		protected void LockUserInteraction()
		{
			view.screenCanvasGroup.interactable = false;
			view.screenCanvasGroup.blocksRaycasts = false;
			view.wasd.enabled = false;
		}

		protected void UnlockUserInteraction()
		{
			view.screenCanvasGroup.interactable = true;
			view.screenCanvasGroup.blocksRaycasts = true;
			view.wasd.enabled = true;
		}

		protected void OnGridExit()
		{
			if (!(m_lastHoveredItem == null))
			{
				if (selection == null)
				{
					view.informationPanelFader.FadeOut();
				}
				else
				{
					UINavigation.focus = selection.GetComponent<UINavigation>();
				}
			}
		}

		protected void LoadGarageScene(bool p_load)
		{
			if (p_load)
			{
				m_garageScene = SceneManager.GetSceneByName("garage");
				if (!m_garageScene.IsValid())
				{
					SceneManager.LoadSceneAsync("garage", LoadSceneMode.Additive);
					m_garageScene = SceneManager.GetSceneByName("garage");
				}
			}
			else
			{
				SceneManager.UnloadSceneAsync("garage");
			}
		}

		protected void EnableGarageSceneDelay(bool p_enable, float p_delay, Action p_onActivated = null)
		{
			if (m_garage_enable_timer != null)
			{
				m_garage_enable_timer.Stop();
			}
			m_garage_enable_timer = Activity.RunOnce(delegate
			{
				m_garage_enable_timer = null;
				EnableGarageScene(p_enable, p_onActivated);
			}, p_delay);
		}

		protected void EnableGarageScene(bool p_enable, Action p_onActivated = null)
		{
			if (m_garage_enable_timer != null)
			{
				m_garage_enable_timer.Stop();
				m_garage_enable_timer = null;
			}
			if (p_enable)
			{
				Activity.Run((Func<bool>)delegate
				{
					if (!m_garageScene.IsValid())
					{
						LoadGarageScene(p_load: true);
						return true;
					}
					if (!m_garageScene.isLoaded)
					{
						return true;
					}
					GameObject[] rootGameObjects2 = m_garageScene.GetRootGameObjects();
					for (int i = 0; i < rootGameObjects2.Length; i++)
					{
						if (rootGameObjects2[i].name == "environment")
						{
							rootGameObjects2[i].SetActive(value: true);
						}
					}
					_ = m_parentScene;
					if (p_onActivated != null)
					{
						p_onActivated();
					}
					SceneManager.SetActiveScene(m_garageScene);
					GameObject gameObject3 = null;
					if ((bool)base.app.model.game && (bool)base.app.model.game.level.settings)
					{
						gameObject3 = base.app.model.game.level.settings.light.sunLight.transform.parent.gameObject;
					}
					if ((bool)gameObject3)
					{
						gameObject3.SetActive(value: false);
					}
					GameObject gameObject4 = GameObject.Find("/level/environment/probes");
					if ((bool)gameObject4)
					{
						gameObject4.SetActive(value: false);
					}
					view.flyButtonElementView.interactable = true;
					return false;
				}, 0f, false);
			}
			else
			{
				_ = m_parentScene;
				if (!m_parentScene.IsValid() || !m_parentScene.isLoaded)
				{
					for (int num = 0; num < SceneManager.sceneCount; num++)
					{
						m_parentScene = SceneManager.GetSceneAt(num);
						if (m_parentScene.IsValid() && m_parentScene.isLoaded && (m_parentScene.name == "main" || m_parentScene.name != "game"))
						{
							break;
						}
					}
				}
				_ = m_parentScene;
				if (m_parentScene.IsValid() && m_parentScene.isLoaded)
				{
					SceneManager.SetActiveScene(m_parentScene);
				}
				GameObject[] rootGameObjects = m_garageScene.GetRootGameObjects();
				for (int num2 = 0; num2 < rootGameObjects.Length; num2++)
				{
					if (rootGameObjects[num2].name == "environment")
					{
						rootGameObjects[num2].SetActive(value: false);
					}
				}
				GameObject gameObject = null;
				if ((bool)base.app.model.game && (bool)base.app.model.game.level.settings)
				{
					gameObject = base.app.model.game.level.settings.light.sunLight.transform.parent.gameObject;
				}
				if ((bool)gameObject)
				{
					gameObject.SetActive(value: true);
				}
				GameObject gameObject2 = GameObject.Find("/level/environment/probes");
				if ((bool)gameObject2)
				{
					gameObject2.SetActive(value: true);
				}
				view.viewerCamera.gameObject.SetActive(value: false);
			}
			RenderSettings.skybox = (p_enable ? view.garageSkybox : view.activeSkybox);
		}

		public void SetIgnoredGameCommands()
		{
			if (!base.app.level.IsLevelLoaded("game"))
			{
				return;
			}
			List<GameCommand> list = new List<GameCommand>();
			foreach (GameInputMapComponent map in base.app.controller.game.input.maps)
			{
				foreach (GameCommand command in map.commands)
				{
					if (command.type == GameCommandType.EditDrone || command.type == GameCommandType.ResetDrone || command.type == GameCommandType.ResetDronePodium || command.type == GameCommandType.ResetGame || command.type == GameCommandType.SwitchCameraMode || command.type == GameCommandType.SwitchDebugDashboard || command.type == GameCommandType.SwitchPhysicsDashboard)
					{
						list.Add(command);
					}
				}
			}
			base.app.controller.game.input.SetIgnoredCommands(list);
		}

		private void ClearIgnoredCommands()
		{
			if (base.app.level.IsLevelLoaded("game"))
			{
				base.app.controller.game.input.ClearIgnoredCommands();
			}
		}

		protected Drone ChangeRig(Drone p_old, DroneRigData p_new)
		{
			if (p_old == null || p_new == null)
			{
				return null;
			}
			if (p_old != null)
			{
				Notify("game.simulation.drone.flight-time@update", p_old.rig);
			}
			GamePlayerData playerData = base.app.model.game.GetPlayerData(p_old);
			int channel = ((p_old != null && p_old.receiver != null) ? p_old.receiver.channel : 0);
			if (p_new.isOriginal)
			{
				p_new.color0 = playerData.color;
				p_new.color2 = playerData.color;
			}
			Drone newDrone = base.app.model.storage.factory.Replace(p_new, p_old, p_old.transform.parent, p_old.transform.parent, p_async: false);
			base.app.controller.game.ApplyCommunityDroneToDrone(newDrone);
			base.app.controller.game.PodiumReset(newDrone);
			newDrone.OnEvent.AddListener(delegate(DroneEvent p_event)
			{
				if (p_event.type == DroneEventType.Ready)
				{
					newDrone.receiver.channel = channel;
					newDrone.fc.armed = true;
					base.app.model.game.camera.drone = newDrone;
					Notify("game.simulation.drone@armed", newDrone);
					RunOnce(0.1f, delegate
					{
						if (newDrone != null && newDrone.hasRig)
						{
							if (newDrone.rig.hasCustomPhysics)
							{
								DronePhysicsData dronePhysicsData = DronePhysicsData.FromJson(newDrone.rig.tune);
								if (dronePhysicsData != null)
								{
									newDrone.physics = dronePhysicsData;
								}
							}
							if (newDrone.rig.hasCustomProfile)
							{
								DroneProfileData droneProfileData = DroneProfileData.FromJson(newDrone.rig.profile);
								if (droneProfileData != null)
								{
									newDrone.profile = droneProfileData;
								}
							}
							newDrone.SetPropwash(base.app.model.storage.state.player.settings.game.propwash);
						}
					});
				}
			});
			Notify("game.simulation.drone@replace", playerData.drone, newDrone);
			playerData.drone = newDrone;
			newDrone.position = base.app.model.game.simulation.podiums.list[0].spawn.position;
			newDrone.transform.rotation = base.app.model.game.simulation.podiums.list[0].spawn.rotation;
			newDrone.fc.Reset();
			newDrone.ClearForces();
			if (newDrone.physics != null && newDrone.physics.aerodynamics != null)
			{
				newDrone.physics.aerodynamics.Reset();
			}
			newDrone.renderer.SetTrailsEnabled(p_flag: false);
			base.app.arguments.game.GetPlayerById(base.app.model.storage.state.player.profile.playerId.ToString()).rig = newDrone.rig;
			return newDrone;
		}

		private void Update()
		{
			if (view.drone != null)
			{
				view.drone.GarageUpdate();
			}
			CheckInput();
		}

		private void CheckInput()
		{
			if (!(base.app.view.ui.screens.current != view.screen))
			{
				int num = view.tabGroup.index;
				bool flag = false;
				if (RCI.GetButtonDown(ConsoleButtons.RightShoulder1))
				{
					num = ((num != m_tabGroupCount - 1) ? (num + 1) : 0);
					flag = true;
				}
				if (RCI.GetButtonDown(ConsoleButtons.LeftShoulder1))
				{
					num = ((num != 0) ? (num - 1) : (m_tabGroupCount - 1));
					flag = true;
				}
				if (flag)
				{
					view.tabGroup.index = num;
					SetTab(view.tabGroup.selection);
				}
				view.wasd.allowZoom = !base.app.view.ui.notifications.popUpPanelVisible;
			}
		}
	}
}

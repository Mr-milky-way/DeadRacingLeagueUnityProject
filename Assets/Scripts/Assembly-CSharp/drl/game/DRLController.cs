using System.Collections.Generic;
using UnityEngine;
using UnityExt.Core.UI;
using drl.analytics;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLController : Controller<DRLApp>
	{
		private static GameController m_game;

		private static ServiceController m_service;

		private static StorageController m_storage;

		private static NetworkController m_network;

		private static TournamentController m_tournament;

		private static ChatController m_chat;

		private static PLMController m_plm;

		private static DRLAnalyticsController m_analytics;

		private static DRLNotificationController m_notifications;

		private static DRLOnboardingController m_onboarding;

		private static DRLAchievementsController m_achievements;

		[SerializeField]
		private static UISystemProfiler m_fps_ui_widget;

		private static bool m_fps_enabled;

		[Header("Mouse Control")]
		public float mouseDelay = 3f;

		public float mouseElapsed;

		public bool mouseAllowed = true;

		private Vector2 m_last_mouse;

		private bool m_mouse_visible_lock;

		private bool m_mouse_visible;

		private bool m_mouse_dirty;

		private float m_rage_quit_down;

		private string m_controllerDialogId = "dialog-controller-disconnected";

		public SplashController splash => Assert<SplashController>("splash");

		public MainController main => Assert<MainController>("main");

		public GameController game
		{
			get
			{
				if (!m_game)
				{
					return m_game = Assert<GameController>("game");
				}
				return m_game;
			}
		}

		public SettingsController settings => Assert<SettingsController>("settings");

		public ServiceController service
		{
			get
			{
				return m_service;
			}
			set
			{
				m_service = value;
			}
		}

		public AudioController audio => Assert<AudioController>("audio");

		public StorageController storage
		{
			get
			{
				return m_storage;
			}
			set
			{
				m_storage = value;
			}
		}

		public NetworkController network
		{
			get
			{
				return m_network;
			}
			set
			{
				m_network = value;
			}
		}

		public TournamentController tournament
		{
			get
			{
				return m_tournament;
			}
			set
			{
				m_tournament = value;
			}
		}

		public ChatController chat
		{
			get
			{
				return m_chat;
			}
			set
			{
				m_chat = value;
			}
		}

		public PLMController plm
		{
			get
			{
				return m_plm;
			}
			set
			{
				m_plm = value;
			}
		}

		public DRLAnalyticsController analytics
		{
			get
			{
				return m_analytics;
			}
			set
			{
				m_analytics = value;
			}
		}

		public DRLNotificationController notifications
		{
			get
			{
				return m_notifications;
			}
			set
			{
				m_notifications = value;
			}
		}

		public DRLOnboardingController onboarding
		{
			get
			{
				return m_onboarding;
			}
			set
			{
				m_onboarding = value;
			}
		}

		public DRLAchievementsController achievements
		{
			get
			{
				return m_achievements;
			}
			set
			{
				m_achievements = value;
			}
		}

		public UISystemProfiler fpsTrackerWidget
		{
			get
			{
				if ((bool)m_fps_ui_widget)
				{
					return m_fps_ui_widget;
				}
				GameObject dontDestroyObject = LevelManager.GetDontDestroyObject("app.mini-profiler");
				if ((bool)dontDestroyObject)
				{
					m_fps_ui_widget = Hierarchy.Find<UISystemProfiler>(dontDestroyObject.transform);
				}
				if ((bool)m_fps_ui_widget)
				{
					return m_fps_ui_widget;
				}
				return m_fps_ui_widget = (base.app ? Hierarchy.Find<UISystemProfiler>(base.app.transform) : null);
			}
		}

		public bool fpsTrackerEnabled
		{
			get
			{
				return m_fps_enabled;
			}
			set
			{
				m_fps_enabled = value;
				if ((bool)fpsTrackerWidget)
				{
					fpsTrackerWidget.transform.parent.gameObject.SetActive(m_fps_enabled);
				}
			}
		}

		protected void Awake()
		{
			m_last_mouse = Input.mousePosition;
			if (m_fps_enabled)
			{
				UISystemProfiler uISystemProfiler = fpsTrackerWidget;
				if ((bool)uISystemProfiler)
				{
					uISystemProfiler.transform.parent.gameObject.SetActive(m_fps_enabled);
				}
			}
		}

		protected void Update()
		{
			RefreshMouseCursor();
			bool num = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
			bool flag = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
			bool key = Input.GetKey(KeyCode.Escape);
			Input.GetKeyDown(KeyCode.F9);
			if (num && flag && key)
			{
				m_rage_quit_down += Time.unscaledDeltaTime;
				if (m_rage_quit_down >= 3f)
				{
					m_rage_quit_down = -3f;
					base.app.scene.LoadMain(p_force: true);
				}
			}
		}

		private void SetFPSEnabled(bool p_flag)
		{
			GameObject gameObject = (fpsTrackerWidget ? fpsTrackerWidget.gameObject : null);
			if ((bool)gameObject)
			{
				gameObject = gameObject.transform.parent.gameObject;
			}
			if ((bool)gameObject)
			{
				gameObject.SetActive(p_flag);
				m_fps_enabled = p_flag;
			}
		}

		protected void LateUpdate()
		{
			if (Cursor.visible != m_mouse_visible)
			{
				Cursor.visible = m_mouse_visible;
			}
		}

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "scene.start":
			{
				if ((bool)base.app.time)
				{
					base.app.time.Initialize(p_use_nist_time: false);
				}
				List<Controller> list = new List<Controller>();
				foreach (GameObject item in new List<GameObject>(LevelManager.GetDontDestroyRootObjects()))
				{
					list.AddRange(Hierarchy.FindAll<Controller>(item.transform));
				}
				foreach (Controller item2 in list)
				{
					base.app.CacheController(item2);
				}
				if ((bool)base.app.view.ui && (bool)base.app.view.ui.dialog)
				{
					base.app.view.ui.dialog.Init();
				}
				UpdateOnlineComponents();
				string p_level = "";
				if (p_data.Length != 0)
				{
					p_level = p_data[0] as string;
				}
				if ((bool)base.app.controller.plm)
				{
					base.app.controller.plm.network.Init(p_level);
				}
				break;
			}
			case "settings.ready":
				RefreshFooter();
				break;
			case "input.active-controller.changed":
			case "settings.controller.connect":
				this.TimerRunOnce(UpdateControllerStatus, 1f / 60f);
				break;
			case "storage.state@refresh":
			case "storage.state@parse":
				RefreshFooter();
				break;
			case "storage.progression@refresh":
				RefreshFooterProgression();
				break;
			case "storage.drone@refresh":
				RefreshFooterDrone();
				break;
			case "calibration.save.complete":
				UpdateControllerStatus();
				break;
			case "boot@complete":
				if ((bool)base.app.view.ui && (bool)base.app.view.ui.dialog)
				{
					base.app.view.ui.dialog.Init();
				}
				mouseAllowed = true;
				UpdateControllerStatus();
				UpdateOnlineComponents();
				break;
			case "storage.localization@refresh":
				UpdateControllerStatus();
				break;
			case "settings.controller.disconnect":
				UpdateControllerStatus();
				this.TimerRunOnce(UpdateControllerStatus, 1f / 60f);
				break;
			case "ui.screen@open":
				base.app.view.ui.footer.SetSocialExpanded(p_flag: false);
				break;
			case "footer@over":
				DisableScroll();
				break;
			case "footer@out":
				if (!base.app.view.ui.social.open)
				{
					EnableScroll();
				}
				break;
			case "ui.footer@open":
			{
				UINavigation socialButtonNavigation2 = base.app.view.ui.footer.socialButtonNavigation;
				UINavigation component3 = base.app.view.ui.social.tabGroup.tabs[0].GetComponent<UINavigation>();
				UINavigation component4 = base.app.view.ui.social.tabGroup.tabs[1].GetComponent<UINavigation>();
				socialButtonNavigation2.up = component3;
				component3.down = socialButtonNavigation2;
				component4.down = socialButtonNavigation2;
				base.app.view.ui.footer.SetConnectionStatusActive(!DRLApp.offline);
				break;
			}
			case "ui.footer@close":
			{
				UINavigation socialButtonNavigation = base.app.view.ui.footer.socialButtonNavigation;
				UINavigation component = base.app.view.ui.social.tabGroup.tabs[0].GetComponent<UINavigation>();
				UINavigation component2 = base.app.view.ui.social.tabGroup.tabs[1].GetComponent<UINavigation>();
				socialButtonNavigation.up = null;
				component.down = null;
				component2.down = null;
				base.app.view.ui.footer.SetSocialExpanded(p_flag: false);
				break;
			}
			case "ui.footer.social@click":
				base.app.view.ui.footer.ToggleSocialGroup(0.2f);
				break;
			case "network.update.offline":
			{
				if (DRLApp.forceOffline || DRLApp.offline)
				{
					break;
				}
				string dialog_title = "ui.dialog.connection-lost.title@CONNECTION LOST!";
				string p_message = "ui.dialog.connection-lost.desc@Network connection lost. Starting game in offline mode.";
				string[] p_options2 = new string[1] { "ui.common.ok@OK" };
				Texture2D network_icon = base.app.view.ui.dialog.templates.Find((DialogTemplate o) => o.template == DialogTemplateType.LoadOffline).icon;
				base.app.view.ui.dialog.Open(DialogType.Warning, dialog_title, p_message, p_options2, network_icon, "connection-online-offline", delegate(string text, int p_option)
				{
					if (!(text != "connection-online-offline"))
					{
						this.TimerRunOnce(delegate
						{
							base.app.view.ui.dialog.Open(DialogType.Warning, dialog_title, "ui.dialog.load-offline.desc@ENTERING OFFLINE MODE...", null, network_icon, "load-transition");
							DRLApp.forceOffline = (DRLApp.offline = true);
							base.app.controller.plm.ForceReset();
						}, 0.6f);
					}
				});
				break;
			}
			case "ui.footer.connection@click":
				if (base.app.controller.plm.network.connected)
				{
					bool connected = !DRLApp.offline;
					DialogTemplate t = base.app.view.ui.dialog.templates.Find((DialogTemplate o) => (!connected) ? (o.template == DialogTemplateType.LoadOnline) : (o.template == DialogTemplateType.LoadOffline));
					base.app.view.ui.dialog.Open(connected ? DialogTemplateType.LoadOffline : DialogTemplateType.LoadOnline, "load-offline-online", delegate(string text, int p_option)
					{
						if (!(text != "load-offline-online") && p_option == 1)
						{
							this.TimerRunOnce(delegate
							{
								string text2 = "";
								text2 = ((!connected) ? "ui.dialog.load-online.desc@ENTERING ONLINE MODE..." : "ui.dialog.load-offline.desc@ENTERING OFFLINE MODE...");
								base.app.view.ui.dialog.Open(DialogType.Info, t.title, text2, null, t.icon, "load-transition", delegate
								{
									DRLApp.forceOffline = connected;
									base.app.controller.plm.ForceReset();
								});
							}, 0.6f);
						}
					});
				}
				else
				{
					base.app.view.ui.dialog.Open(DialogTemplateType.ConnectionDisconnect);
				}
				break;
			case "ui.footer.exit@click":
			{
				UIScreen current = base.app.view.ui.screens.current;
				if (current == null)
				{
					break;
				}
				if (current.title == "Home")
				{
					base.app.view.ui.dialog.Open(DialogTemplateType.QuitGame, "quit-game", delegate(string text, int p_option)
					{
						if (p_option == 1)
						{
							this.TimerRunOnce(delegate
							{
								Notify("home.quit@click");
							}, 0.6f);
						}
					});
					break;
				}
				string[] p_options = new string[3] { "HOME", "SYSTEM", "CANCEL" };
				DialogComponent dialog = base.app.view.ui.dialog;
				Texture2D warningIcon = dialog.warningIcon;
				string p_id = "dialog.exit.game";
				dialog.Open(DialogType.Info, "EXIT GAME", "DO YOU WANT TO EXIT THE GAME? ", p_options, warningIcon, p_id, delegate(string text, int p_option)
				{
					if (p_option == 1)
					{
						if (game != null)
						{
							game.Exit();
						}
						else
						{
							base.app.view.ui.screens.CloseAllScreens();
							base.app.view.ui.screens.manager.ClearHistory();
							base.app.view.ui.screens.Open("home-screen-grid", 0f);
							base.app.view.ui.header.Refresh();
						}
					}
					if (p_option == 2)
					{
						this.TimerRunOnce(delegate
						{
							Notify("home.quit@click");
						}, 0.1f);
					}
				});
				break;
			}
			case "social-media.link@click":
				if ((bool)p_target)
				{
					switch (p_target.name.ToLower())
					{
					case "discord":
						Application.OpenURL("https://discord.gg/p7ndQHz");
						break;
					case "facebook":
						Application.OpenURL("https://www.facebook.com/groups/drlsim/?ref=bookmarks");
						break;
					case "twitch":
						Application.OpenURL("https://www.twitch.tv/thedroneracingleague");
						break;
					case "twitter":
						Application.OpenURL("https://twitter.com/droneraceleague");
						break;
					case "instagram":
						Application.OpenURL("https://www.instagram.com/drlsim/");
						break;
					}
				}
				break;
			}
		}

		protected void RefreshMouseCursor()
		{
			mouseElapsed += Time.unscaledDeltaTime;
			Vector2 last_mouse = Input.mousePosition;
			bool flag = false;
			if (Mathf.Abs(m_last_mouse.x - last_mouse.x) > 3f)
			{
				flag = true;
			}
			else if (Mathf.Abs(m_last_mouse.y - last_mouse.y) > 3f)
			{
				flag = true;
			}
			if (flag)
			{
				mouseElapsed = 0f;
				m_last_mouse = last_mouse;
			}
			bool flag2 = mouseAllowed && mouseElapsed < mouseDelay;
			if (flag2 != m_mouse_visible)
			{
				m_mouse_visible = flag2;
				m_mouse_visible_lock = true;
				Notify(m_mouse_visible ? "input.mouse-cursor.show" : "input.mouse-cursor.hide");
				m_mouse_visible_lock = false;
			}
		}

		public void SetMouseVisible(bool p_flag)
		{
			mouseElapsed = (p_flag ? 0f : mouseDelay);
			m_last_mouse = Input.mousePosition;
			m_mouse_visible = p_flag;
			if (!m_mouse_visible_lock)
			{
				m_mouse_visible_lock = true;
				Notify(p_flag ? "input.mouse-cursor.show" : "input.mouse-cursor.hide");
				m_mouse_visible_lock = false;
			}
		}

		private void UpdateOnlineComponents()
		{
			DRLOnlineComponents component = GetComponent<DRLOnlineComponents>();
			if (!(component == null))
			{
				component.SetComponentsEnabled(!DRLApp.offline);
			}
		}

		public void UpdateControllerStatus()
		{
			if (!base.app.view.ui)
			{
				return;
			}
			UIFooterView footer = base.app.view.ui.footer;
			if ((bool)footer)
			{
				Localization locale = base.app.model.storage.locale;
				base.app.view.ui.dialog?.RefreshHotkeys();
				if (RCI.UsingKeyboardAsController && !RCI.HasAssignedController)
				{
					footer.SetCalibrationWarning(p_needs_calibration: false);
					footer.controllerField.color = Color.red;
					footer.controllerStatus = locale.Get("calibration.footer.nohardware", "NO HARDWARE");
				}
				else if (!RCI.HasControllersConnected())
				{
					footer.SetCalibrationWarning(p_needs_calibration: false);
					footer.controllerField.color = Color.red;
					footer.controllerStatus = locale.Get("calibration.footer.nohardware", "NO HARDWARE");
				}
				else if (RCI.IsCalibrated || RCI.HasSavedProfile())
				{
					footer.SetCalibrationWarning(p_needs_calibration: false);
					footer.controllerStatus = RCI.GetSimplifiedControllerName();
				}
				else
				{
					footer.SetCalibrationWarning(p_needs_calibration: true);
				}
			}
		}

		public void RefreshFooter()
		{
			UIFooterView footer = base.app.view.ui.footer;
			if ((bool)footer && (bool)base.app.model.storage)
			{
				PlayerStateModel player = base.app.model.storage.state.player;
				footer.username = player.profile.username;
				footer.userPhoto = player.profile.photo;
				footer.userColor = player.profile.color;
				footer.SetRankBadge(player.userRank);
				RefreshFooterProgression();
				RefreshFooterDrone();
			}
		}

		public void RefreshFooterProgression()
		{
			UIFooterView footer = base.app.view.ui.footer;
			if ((bool)footer && (bool)base.app.model.storage)
			{
				PlayerStateModel player = base.app.model.storage.state.player;
				footer.SetProgression(player.progression.state);
			}
		}

		public void RefreshFooterDrone()
		{
			if (base.app.inGarage)
			{
				return;
			}
			UIFooterView v = base.app.view.ui.footer;
			if (!v)
			{
				return;
			}
			DroneRigData currentRigData = base.app.model.storage.state.player.garage.currentRigData;
			if (currentRigData == null)
			{
				return;
			}
			v.droneName = currentRigData.diameter + "\" " + currentRigData.name.ToUpper() + (currentRigData.hasCustomPhysics ? " *" : "");
			v.droneNameField.color = (currentRigData.hasCustomPhysics ? Color.yellow : Color.white);
			base.app.model.storage.state.player.garage.GetRigThumbnail(currentRigData, 320, 0, delegate(Texture2D p_result)
			{
				if (p_result != null && p_result.width > 128 && v != null && v.droneImageField != null)
				{
					v.droneImage = p_result;
				}
			});
		}

		public void RefreshFooterDrone(Texture2D p_texture)
		{
			UIFooterView footer = base.app.view.ui.footer;
			if (!(footer == null) && !(p_texture == null))
			{
				footer.droneImage = p_texture;
			}
		}

		public void LoadMapEditor(DRLMap p_map, MapData p_data, GameFlag p_mode = GameFlag.None)
		{
			base.app.arguments.game.players.Clear();
			base.app.arguments.game.mode = GameFlag.SinglePlayer;
			base.app.arguments.game.type = GameFlag.MapEditor;
			List<DRLMapTrack> mapTracks = base.app.model.storage.GetMapTracks(p_map, GameFlag.MapEditor, p_filter_build: true);
			DRLMapTrack dRLMapTrack = null;
			for (int i = 0; i < mapTracks.Count; i++)
			{
				if (mapTracks[i].freestyleOnly)
				{
					dRLMapTrack = mapTracks[i];
					break;
				}
			}
			if (mapTracks.Count <= 0)
			{
				Debug.Log("DRLController> LoadMapEditor - No Freestyle tracks found - scene[" + p_map.scene + "]");
				base.app.view.audio.PlayUIGenericError();
				return;
			}
			MapData mapData = ((p_data == null) ? new MapData() : p_data);
			mapData.playerId = base.app.model.service.backend.playerId;
			mapData.mapTitle = "NEW MAP";
			mapData.mapDirty = false;
			switch (p_mode)
			{
			case GameFlag.Race:
				mapData.mode.typeFlag = GameFlag.Race;
				break;
			case GameFlag.Collectable:
				mapData.mode.typeFlag = GameFlag.Collectable;
				break;
			}
			Debug.Log($"DRLController> LoadMapEditor - scene[{p_map.scene}] data[{mapData.guid}] map-mode[{p_mode}]");
			p_map = (dRLMapTrack ? dRLMapTrack.map : null);
			p_map.data = mapData;
			base.app.view.audio.PlayUIStartGame();
			base.app.arguments.game.map = p_map;
			base.app.arguments.game.track = dRLMapTrack;
			base.app.arguments.game.podium = dRLMapTrack.podium;
			base.app.view.audio.SceneMainToGame(1.6f);
			base.app.view.ui.fade.FadeIn(1.5f);
			Activity.RunOnce(base.app.scene.Load, 1f);
		}

		public void LoadTrackOverview(MonoBehaviour p_caller, Object p_target, params object[] p_data)
		{
			bool num = (bool)p_data[3];
			DRLMap dRLMap = (num ? null : ((DRLMap)p_data[4]));
			DRLMapTrack dRLMapTrack = (num ? null : ((DRLMapTrack)p_data[5]));
			MapData mapData = (num ? ((MapData)p_data[6]) : null);
			UIMapTrackOverviewView uIMapTrackOverviewView = base.app.view.ui.screens.Open<UIMapTrackOverviewView>("track-overview-screen");
			if (num)
			{
				if (mapData == null || base.app.model.storage == null || base.app.model.storage.library == null)
				{
					Debug.LogError("DRLController> MapSelectionComplete received invalid MapData");
					return;
				}
				uIMapTrackOverviewView.screen.title = mapData.mapTitle.ToUpper();
				uIMapTrackOverviewView.Set(mapData);
			}
			else if (!dRLMap || !dRLMapTrack)
			{
				Debug.LogError("DRLController> MapSelectionComplete received invalid DRLMap or DRLMapTrack");
			}
			else
			{
				uIMapTrackOverviewView.screen.title = dRLMapTrack.title;
				uIMapTrackOverviewView.Set(dRLMap);
				uIMapTrackOverviewView.Set(dRLMapTrack);
			}
		}

		public void LoadCustomTrackOverview(MapData p_mapData)
		{
			if (p_mapData == null || base.app.model.storage == null || base.app.model.storage.library == null)
			{
				Debug.LogError("DRLController> MapSelectionComplete received invalid MapData");
			}
			else
			{
				base.app.view.ui.screens.Open<UIMapTrackOverviewView>("track-overview-screen").Set(p_mapData);
			}
		}

		public bool AssertMapSelection()
		{
			return true;
		}

		public bool AssertMapSelection(Object p_target, MonoBehaviour p_caller, bool p_need_return = false)
		{
			UIMapsCategoryController uIMapsCategoryController = p_target as UIMapsCategoryController;
			UIMapsUSAFController uIMapsUSAFController = p_target as UIMapsUSAFController;
			UIMapsSDCategoryController uIMapsSDCategoryController = p_target as UIMapsSDCategoryController;
			if ((bool)uIMapsCategoryController)
			{
				UIMapsCategoryController uIMapsCategoryController2 = uIMapsCategoryController;
				if (uIMapsCategoryController2 == null || uIMapsCategoryController2.view == null || uIMapsCategoryController2.view.caller != p_caller)
				{
					return false;
				}
				if (p_need_return)
				{
					int num = uIMapsCategoryController2.view.depth;
					if (base.app.view.ui.screens.manager.InHistory("maps-usaf-screen"))
					{
						num += 2;
					}
					base.app.view.ui.screens.Return(num);
				}
			}
			if ((bool)uIMapsSDCategoryController)
			{
				UIMapsSDCategoryController uIMapsSDCategoryController2 = uIMapsSDCategoryController;
				if (uIMapsSDCategoryController2 == null || uIMapsSDCategoryController2.view == null || uIMapsSDCategoryController2.view.caller != p_caller)
				{
					return false;
				}
				if (p_need_return)
				{
					int depth = uIMapsSDCategoryController2.view.depth;
					base.app.view.ui.screens.Return(depth);
				}
			}
			if ((bool)uIMapsUSAFController)
			{
				UIMapsUSAFController uIMapsUSAFController2 = uIMapsUSAFController;
				if (uIMapsUSAFController2 == null || uIMapsUSAFController2.view == null || uIMapsUSAFController2.view.caller != p_caller)
				{
					return false;
				}
				if (p_need_return)
				{
					base.app.view.ui.screens.Return(uIMapsUSAFController2.view.depth);
				}
			}
			return true;
		}

		private void EnableScroll()
		{
			if (!(base.app.view.ui.screens.current == null))
			{
				UINavigationScroll component = base.app.view.ui.screens.current.GetComponent<UINavigationScroll>();
				if (component != null)
				{
					component.enabled = true;
				}
			}
		}

		private void DisableScroll()
		{
			if (!(base.app.view.ui.screens.current == null))
			{
				UINavigationScroll component = base.app.view.ui.screens.current.GetComponent<UINavigationScroll>();
				if (component != null)
				{
					component.enabled = false;
				}
			}
		}
	}
}

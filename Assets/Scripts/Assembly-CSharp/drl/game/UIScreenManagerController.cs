using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIScreenManagerController : Controller<DRLApp>
	{
		public bool fadeBackground;

		public NavigationModeType navigationMode;

		private MonoActivity m_promo_timer;

		private string[] m_promo_screens;

		private Vector2 m_last_mouse;

		private float m_mouse_accum;

		private MonoActivity m_setmode_timer;

		private Activity m_reinforce_focus_timer;

		public UIScreenManagerView view => AssertLocal<UIScreenManagerView>("view");

		protected override void Start()
		{
			m_promo_screens = new string[9] { "tryouts-onboarding-screen", "tryouts-leaders-screen", "tryouts-register-screen", "campaign-overview-screen", "campaign-results-screen", "track-overview-screen", "multiplayer-room-screen", "leaderboards-screen", "tournament-brackets-screen" };
			m_last_mouse = Input.mousePosition;
			UINavigationSystem.OnValidateFocus = delegate
			{
				if (!view)
				{
					return (UINavigation)null;
				}
				if (view.current == null)
				{
					return (UINavigation)null;
				}
				if (UINavigation.focus != null && UINavigation.focus.transform.IsChild(view.current.transform))
				{
					return UINavigation.focus;
				}
				Transform transform = view.TryGetFirstLeftNavigationLink(view.current);
				if (transform == null)
				{
					return (UINavigation)null;
				}
				UINavigation uINavigation = transform.GetComponent<UINavigation>();
				if (!uINavigation)
				{
					uINavigation = Hierarchy.Find<UINavigation>(transform);
				}
				return uINavigation;
			};
		}

		public void ValidatePromo()
		{
			bool flag = false;
			if (!this || !base.app)
			{
				return;
			}
			if ((bool)base.app.arguments.game.campaign)
			{
				flag = base.app.arguments.game.campaign.tournament;
			}
			bool flag2 = false;
			for (int i = 0; i < m_promo_screens.Length; i++)
			{
				string text = m_promo_screens[i];
				if (!view.manager.InHistory(text))
				{
					continue;
				}
				switch (text)
				{
				case "leaderboards-screen":
				{
					UILeaderboardsView open3 = view.manager.GetOpen<UILeaderboardsView>();
					if ((bool)open3 && (bool)open3.campaign && open3.campaign.tournament)
					{
						flag = (flag2 = true);
					}
					break;
				}
				case "multiplayer-room-screen":
					if ((bool)view.manager.GetOpen<UIMultiplayerRoomView>() && base.app.arguments.game.tournamentPromo)
					{
						flag = (flag2 = true);
					}
					break;
				case "tournament-brackets-screen":
				{
					UITournamentBracketsView open2 = view.manager.GetOpen<UITournamentBracketsView>();
					flag2 = true;
					if ((bool)open2)
					{
						flag = false;
						base.app.view.ui.SetPromoEnabled(p_flag: false, UIView.PromoType.Promo);
						base.app.view.ui.SetPromoEnabled(p_flag: true, UIView.PromoType.vDRL);
					}
					break;
				}
				case "tryouts-onboarding-screen":
				{
					UITryoutsOnboardingView open = view.manager.GetOpen<UITryoutsOnboardingView>();
					flag2 = true;
					if ((bool)open)
					{
						flag = false;
						base.app.view.ui.SetPromoEnabled(p_flag: false, UIView.PromoType.vDRL);
						base.app.view.ui.SetPromoEnabled(p_flag: true, UIView.PromoType.Promo);
					}
					break;
				}
				default:
					flag2 = true;
					break;
				}
			}
			if (base.app.view.ui.screens.current != null && (base.app.view.ui.screens.current.name == "tournament-leaderboards-screen" || base.app.view.ui.screens.current.name == "tournament-results-screen"))
			{
				flag2 = true;
				flag = false;
				base.app.view.ui.SetPromoEnabled(p_flag: false, UIView.PromoType.Promo);
				base.app.view.ui.SetPromoEnabled(p_flag: true, UIView.PromoType.vDRL);
			}
			if (!flag2)
			{
				base.app.view.ui.SetPromoEnabled(p_flag: false, UIView.PromoType.Promo);
				base.app.view.ui.SetPromoEnabled(p_flag: false, UIView.PromoType.vDRL);
				if ((bool)base.app.view.ui.header)
				{
					base.app.view.ui.header.FadeLogo(p_flag: false, 1f, 0.3f);
				}
			}
			else if (flag)
			{
				base.app.view.ui.SetPromoEnabled(p_flag: false, UIView.PromoType.vDRL);
				base.app.view.ui.SetPromoEnabled(p_flag: true, UIView.PromoType.Promo);
				if ((bool)base.app.view.ui.header)
				{
					base.app.view.ui.header.FadeLogo(p_flag: true, 1f, 0.3f);
				}
			}
		}

		public void BlockDark()
		{
			fadeBackground = false;
		}

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "ui.screen@open":
			{
				UIScreen scr = p_data[0] as UIScreen;
				Debug.Log("UIScreenManagerController> Open [" + scr?.ToString() + "] stack[" + view.manager.count + "]");
				if ((bool)scr)
				{
					if (fadeBackground)
					{
						base.app.view.ui.SetDark(p_flag: true);
					}
					fadeBackground = true;
					base.app.view.ui.navigation.enabled = true;
					if (m_promo_timer != null)
					{
						m_promo_timer.Stop();
					}
					m_promo_timer = RunOnce(ValidatePromo, 0.05f);
					if (m_setmode_timer != null)
					{
						m_setmode_timer.Stop();
					}
					m_setmode_timer = RunOnce(delegate
					{
						SetNavigationMode(navigationMode, scr);
					}, 1f / 15f);
				}
				break;
			}
			case "ui.screen@close":
			{
				UIScreen uIScreen = p_data[0] as UIScreen;
				int count = view.manager.count;
				Debug.Log("UIScreenManagerController> Close [" + uIScreen?.ToString() + "] stack[" + count + "]");
				if ((bool)uIScreen)
				{
					base.app.view.ui.SetDark(p_flag: false);
					UINavigation.focus = null;
					base.app.view.ui.navigation.enabled = false;
					if (m_promo_timer != null)
					{
						m_promo_timer.Stop();
					}
					m_promo_timer = RunOnce(ValidatePromo, 0.05f);
					DisableDragScrollNavigation(uIScreen);
					DisableMouseWheelScrollNavigation(uIScreen);
				}
				break;
			}
			case "ui.screen.video-player@open":
				if (p_data.Length != 0)
				{
					VideoClip videoClip = p_data[0] as VideoClip;
					if (videoClip != null)
					{
						view.Open<UIDMVVideoPlayerView>("dmv-video-player-screen").videoPlayer.clip = videoClip;
					}
					else if (!string.IsNullOrEmpty((string)p_data[0]))
					{
						UIVideoPlayerView uIVideoPlayerView = view.Open<UIVideoPlayerView>("video-player-screen");
						uIVideoPlayerView.VideoURL = (string)p_data[0];
						uIVideoPlayerView.ToggleBackCloseButton();
					}
				}
				break;
			case "settings.controller.connect":
			case "settings.controller.disconnect":
				view.RefreshNavigationTooltips();
				break;
			}
		}

		protected void Update()
		{
			Vector2 last_mouse = m_last_mouse;
			Vector2 vector = Input.mousePosition;
			Vector2 vector2 = vector - last_mouse;
			m_last_mouse = vector;
			switch (navigationMode)
			{
			case NavigationModeType.Focus:
			case NavigationModeType.Controller:
			case NavigationModeType.Keyboard:
			{
				bool flag = false;
				if (view.current != null && view.current.GetComponent<UINavigationScroll>() != null)
				{
					flag = view.current.GetComponent<UINavigationScroll>().enableJoystickPanning;
				}
				if (vector2.magnitude > 3f)
				{
					m_mouse_accum += vector2.magnitude;
				}
				if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0f)
				{
					m_mouse_accum += 32f;
				}
				if (m_mouse_accum > 16f)
				{
					m_mouse_accum = 0f;
					SetNavigationMode(NavigationModeType.Mouse);
				}
				else if (navigationMode != NavigationModeType.Keyboard && DRLUINavigationSystem.IsKeyboard())
				{
					SetNavigationMode(NavigationModeType.Keyboard);
				}
				else if (navigationMode != NavigationModeType.Controller && !DRLUINavigationSystem.IsKeyboard() && (DRLUINavigationSystem.IsNavigation() || DRLUINavigationSystem.IsButton()))
				{
					SetNavigationMode(NavigationModeType.Controller);
				}
				if (flag)
				{
					EnableDragScrollNavigation(view.current);
				}
				break;
			}
			case NavigationModeType.Mouse:
				if (DRLUINavigationSystem.IsNavigation() || DRLUINavigationSystem.IsButton())
				{
					if (DRLUINavigationSystem.IsKeyboard())
					{
						SetNavigationMode(NavigationModeType.Keyboard);
					}
					else
					{
						SetNavigationMode(NavigationModeType.Controller);
					}
				}
				else
				{
					_ = vector2.magnitude;
					EnableDragScrollNavigation(view.current);
				}
				break;
			}
		}

		public void SetNavigationMode(NavigationModeType p_type)
		{
			if (view.current == null)
			{
				return;
			}
			Debug.Log("UIScreenManagerController> SetNavigationMode - mode[" + p_type.ToString() + "]");
			if (navigationMode == p_type)
			{
				return;
			}
			navigationMode = p_type;
			Notify(1f / 60f, "ui.screen.navigation-mode@change", navigationMode);
			DisableDragScrollNavigation(view.current);
			DisableMouseWheelScrollNavigation(view.current);
			switch (p_type)
			{
			case NavigationModeType.Focus:
			case NavigationModeType.Controller:
			case NavigationModeType.Keyboard:
			{
				UINavigation.StopUnfocus();
				List<UIScreen> screens2 = view.manager.GetScreens();
				for (int j = 0; j < screens2.Count; j++)
				{
					SetNavigationMode(p_type, screens2[j]);
				}
				break;
			}
			case NavigationModeType.Mouse:
			{
				List<UIScreen> screens = view.manager.GetScreens();
				UINavigation.ClearFocus(p_useDelay: true);
				for (int i = 0; i < screens.Count; i++)
				{
					SetNavigationMode(p_type, screens[i]);
				}
				break;
			}
			}
		}

		public void EnableDragScrollNavigation(UIScreen p_screen, bool p_force = false)
		{
			if (view.dragScrollingEnabled && (bool)p_screen)
			{
				UINavigationScroll component = p_screen.GetComponent<UINavigationScroll>();
				ScrollRect dragScroller = view.dragScroller;
				if ((bool)component && (bool)dragScroller && (!dragScroller.gameObject.activeInHierarchy || p_force))
				{
					component.StartDragScrollNavigation();
				}
			}
		}

		public void DisableDragScrollNavigation(UIScreen p_screen)
		{
			if (view.dragScrollingEnabled && (bool)p_screen)
			{
				UINavigationScroll component = p_screen.GetComponent<UINavigationScroll>();
				if ((bool)component)
				{
					component.StopDragScrollNavigation();
				}
			}
		}

		public void DisableMouseWheelScrollNavigation(UIScreen p_screen)
		{
			if ((bool)p_screen)
			{
				UINavigationScroll component = p_screen.GetComponent<UINavigationScroll>();
				if ((bool)component)
				{
					component.StopMouseWheelScrollNavigation();
				}
			}
		}

		public void SetNavigationMode(NavigationModeType p_type, UIScreen p_screen)
		{
			Debug.Log($"UIScreenManagerController> SetNavigationMode - mode[{p_type}] screen[{p_screen}]");
			if (m_reinforce_focus_timer != null)
			{
				m_reinforce_focus_timer.Stop();
				m_reinforce_focus_timer = null;
			}
			switch (p_type)
			{
			case NavigationModeType.Focus:
			case NavigationModeType.Controller:
			case NavigationModeType.Keyboard:
			{
				if ((bool)p_screen)
				{
					Hierarchy.Traverse(p_screen.transform, delegate(GraphicRaycaster p_grc)
					{
						if ((bool)p_grc)
						{
							p_grc.enabled = false;
						}
					});
					Hierarchy.Traverse(p_screen.transform, delegate(UINavigationScroll p_uns)
					{
						if ((bool)p_uns)
						{
							p_uns.mode = p_type;
						}
					});
					if (!DRLUINavigationSystem.IsTyping)
					{
						m_reinforce_focus_timer = Activity.RunOnce(delegate
						{
							if (!(UINavigation.focus == null) && !(UINavigation.focus.GetComponent<DRLInputFieldView>() != null) && !(UINavigation.focus.GetComponent<InputField>() != null) && !UINavigation.focus.transform.IsChildOf(p_screen.transform))
							{
								UINavigation.Focus(p_screen);
							}
						}, 1f / 12f);
					}
				}
				if (!base.app || !base.app.view.ui.social)
				{
					break;
				}
				GraphicRaycaster component2 = base.app.view.ui.social.GetComponent<GraphicRaycaster>();
				if ((bool)component2)
				{
					component2.enabled = false;
				}
				Activity.RunOnce(delegate
				{
					if (p_screen != null && UINavigation.focus != null && !UINavigation.focus.transform.IsChildOf(p_screen.transform))
					{
						UINavigation.Focus(p_screen);
					}
				}, 0.5f);
				break;
			}
			case NavigationModeType.Mouse:
				if ((bool)p_screen)
				{
					Hierarchy.Traverse(p_screen.transform, delegate(GraphicRaycaster p_grc)
					{
						if ((bool)p_grc)
						{
							p_grc.enabled = true;
						}
					});
					Hierarchy.Traverse(p_screen.transform, delegate(UINavigationScroll p_uns)
					{
						if ((bool)p_uns)
						{
							p_uns.mode = p_type;
						}
					});
				}
				if ((bool)base.app && (bool)base.app.view.ui.social)
				{
					GraphicRaycaster component = base.app.view.ui.social.GetComponent<GraphicRaycaster>();
					if ((bool)component)
					{
						component.enabled = true;
					}
				}
				break;
			}
		}
	}
}

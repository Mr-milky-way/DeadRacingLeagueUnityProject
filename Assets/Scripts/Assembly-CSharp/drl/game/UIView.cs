using UnityEngine;
using UnityEngine.UI;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIView : View<DRLApp>
	{
		public enum PromoType
		{
			Promo = 0,
			vDRL = 1
		}

		public RawImage cameraBackground;

		private static DialogComponent m_dialog;

		private UISplash m_splash;

		private static UILoaderView m_loader;

		public bool screenBack = true;

		public GameObject COXLogo;

		public GameObject promo;

		public GameObject vdrl;

		public GameObject vdrlPanel;

		public FadeComponent promoFade;

		public RawImage promoImage;

		public float promoImageAlpha;

		private Activity m_promo_timer;

		public UIScreenManagerView screens => AssertFind<UIScreenManagerView>("screens");

		public UIHeaderView header => AssertFind<UIHeaderView>("header");

		public UISecondaryHeaderView headerSecondary => AssertFind<UISecondaryHeaderView>("header-secondary");

		public UIFooterView footer => AssertFind<UIFooterView>("footer");

		public UISocialView social => AssertFind<UISocialView>("social");

		public UINotificationView notifications => AssertFind<UINotificationView>("notifications");

		public FadeSlideComponent fade => AssertFind<FadeSlideComponent>("fade");

		public DialogComponent dialog
		{
			get
			{
				return m_dialog;
			}
			set
			{
				m_dialog = value;
			}
		}

		public UINavigationSystem navigation => AssertLocal<UINavigationSystem>("navigation");

		public Canvas canvas => AssertLocal<Canvas>("canvas");

		public CanvasScaler canvasScaler => AssertLocal<CanvasScaler>("canvas-scaler");

		public UIGame game => AssertFind<UIGame>("game");

		public UISplash splash
		{
			get
			{
				if (!m_splash)
				{
					return m_splash = GetScreenView<UISplash>("splash-screen");
				}
				return m_splash;
			}
		}

		public UILoaderView loader
		{
			get
			{
				return m_loader;
			}
			set
			{
				m_loader = value;
			}
		}

		public FadeComponent dark => AssertFind<FadeComponent>("dark");

		public void SetDark(bool p_flag, float p_delay)
		{
			if ((bool)dark)
			{
				dark.Fade(p_flag ? 1f : (-0.1f), p_delay);
			}
		}

		public void SetDark(bool p_flag)
		{
			SetDark(p_flag, 0.3f);
		}

		public Vector2 GetMousePosition(RectTransform p_target)
		{
			if (!canvas)
			{
				return Input.mousePosition;
			}
			Vector2 localPoint = Vector3.zero;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(p_target, Input.mousePosition, canvas.worldCamera, out localPoint);
			return localPoint;
		}

		public void SetPromoEnabled(bool p_flag, PromoType p_type)
		{
			if (m_promo_timer != null)
			{
				m_promo_timer.Stop();
			}
			if (!promo || !vdrl)
			{
				return;
			}
			_ = 1;
			GameObject gameObject = null;
			switch (p_type)
			{
			case PromoType.Promo:
				gameObject = promo;
				if (p_flag)
				{
					vdrl.SetActive(value: false);
					vdrlPanel.SetActive(value: false);
				}
				break;
			case PromoType.vDRL:
				gameObject = vdrl;
				if (p_flag)
				{
					promo.SetActive(value: false);
				}
				break;
			}
			if (p_flag)
			{
				gameObject.SetActive(value: true);
				if (p_type == PromoType.vDRL)
				{
					vdrlPanel.SetActive(value: true);
				}
				if ((bool)promoFade)
				{
					promoFade.FadeIn(1f, 0.3f, Tween.Linear);
				}
				return;
			}
			if ((bool)promoFade)
			{
				promoFade.FadeOut(0.3f, 0.15f, Tween.Linear);
			}
			m_promo_timer = Activity.RunOnce(delegate
			{
				if (base.validContext)
				{
					if ((bool)promo)
					{
						promo.SetActive(value: false);
					}
					if ((bool)vdrl)
					{
						vdrl.SetActive(value: false);
						vdrlPanel.SetActive(value: false);
					}
				}
			}, 1.5f);
		}

		public void SetPromoImageAlpha(float p_alpha = -1f)
		{
			Color color = promoImage.color;
			color.a = ((p_alpha == -1f) ? promoImageAlpha : p_alpha);
			promoImage.color = color;
		}

		public T GetScreenView<T>(string p_id) where T : UIScreenView
		{
			UIScreen uIScreen = screens.manager.Get(p_id, p_create: false);
			if (!uIScreen)
			{
				return null;
			}
			return uIScreen.GetComponent<T>();
		}

		public bool IsBackPressedKeyboard()
		{
			if (DRLUINavigationSystem.IsTyping || base.app.view.ui.dialog.isVisible)
			{
				return false;
			}
			bool result = false;
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				result = true;
			}
			if (Input.GetKeyDown(KeyCode.Backspace))
			{
				result = true;
			}
			return result;
		}

		public bool IsBackPressedController()
		{
			bool result = false;
			if (!DRLUINavigationSystem.controllerEnabled || base.app.view.ui.dialog.isVisible || DRLUINavigationSystem.IsTyping)
			{
				return false;
			}
			if (RCI.GetButtonDown(ConsoleButtons.ActionBottomRow2))
			{
				result = true;
			}
			return result;
		}

		private void Awake()
		{
		}

		private void Update()
		{
			if (!screenBack || !screens || !screens.current)
			{
				return;
			}
			bool num = IsBackPressedController();
			bool flag = IsBackPressedKeyboard();
			if (flag)
			{
				UINavigation focus = UINavigation.focus;
				InputField inputField = null;
				if ((bool)focus)
				{
					inputField = Hierarchy.FindReverse<InputField>(focus.transform);
				}
				if ((bool)inputField)
				{
					flag = false;
				}
			}
			if ((!num && (!flag || DRLUINavigationSystem.IsTyping)) || base.app.view.ui.notifications.focused || base.app.view.ui.social.open || DRLUINavigationSystem.IsLoading)
			{
				return;
			}
			Notify(screens.BackButtonPressedEvent());
			this.TimerRunOnce(delegate
			{
				if (base.validContext && !(base.app.view.ui.screens.current == null))
				{
					UINavigation focus2 = UINavigation.focus;
					if (!focus2 || !focus2.transform.IsChild(base.app.view.ui.screens.current.transform))
					{
						UINavigation.Focus(base.app.view.ui.screens.current.transform);
					}
				}
			}, 1f / 30f);
		}
	}
}

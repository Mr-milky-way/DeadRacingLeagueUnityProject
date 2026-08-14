using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLGamepadHotkey : View<DRLApp>, IUpdateable
	{
		public UIElementView button;

		public Image buttonIcon;

		public ConsoleButtons hotkey;

		private Sprite m_defaultButtonImage;

		private Vector2 m_defaultImageSize;

		private UIScreen m_screen;

		[Header("Icons:")]
		public Sprite xboxButtonA;

		public Sprite xboxButtonB;

		public Sprite xboxButtonX;

		public Sprite xboxButtonY;

		public Sprite xboxButtonLB;

		public Sprite xboxButtonLT;

		public Sprite xboxButtonRB;

		public Sprite xboxButtonRT;

		public Sprite psButtonA;

		public Sprite psButtonB;

		public Sprite psButtonX;

		public Sprite psButtonY;

		public Sprite psButtonLB;

		public Sprite psButtonLT;

		public Sprite psButtonRB;

		public Sprite psButtonRT;

		private static DRLApp m_app_cache;

		private static string[] filteredEvents = new string[2] { "ui.screen.return", "garage.edit.back" };

		private DialogComponent m_dialog;

		private bool m_dialog_check;

		private static DRLApp app_cache
		{
			get
			{
				if (!m_app_cache)
				{
					return m_app_cache = Object.FindObjectOfType<DRLApp>();
				}
				return m_app_cache;
			}
		}

		private bool m_isDialog => m_dialog != null;

		private bool IsButtonInvert => false;

		public static void RefreshAll()
		{
			DRLGamepadHotkey[] array = Object.FindObjectsOfType<DRLGamepadHotkey>();
			if (array.Length != 0)
			{
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Refresh();
				}
			}
		}

		protected void Start()
		{
			Init();
		}

		protected void OnEnable()
		{
			Init();
		}

		public void Init()
		{
			Activity.Add(this);
			if (!m_app_cache)
			{
				m_app_cache = base.app;
			}
			if (!button)
			{
				button = Hierarchy.FindReverse<UIElementView>(base.transform);
			}
			if (!buttonIcon)
			{
				buttonIcon = GetComponent<Image>();
			}
			if (!buttonIcon)
			{
				Transform transform = base.transform.Find("icon");
				if ((bool)transform)
				{
					buttonIcon = transform.GetComponent<Image>();
				}
			}
			if ((bool)buttonIcon && m_defaultButtonImage == null)
			{
				m_defaultButtonImage = buttonIcon.sprite;
				m_defaultImageSize = buttonIcon.rectTransform.sizeDelta;
			}
			if (!m_screen)
			{
				m_screen = Hierarchy.FindReverse<UIScreen>(base.transform);
			}
			if (!m_dialog && !m_dialog_check)
			{
				m_dialog = Hierarchy.FindReverse<DialogComponent>(base.transform);
				m_dialog_check = true;
			}
			Refresh();
		}

		protected void OnDisable()
		{
			Activity.Remove(this);
		}

		public void Refresh()
		{
			if (buttonIcon == null)
			{
				return;
			}
			DefaultControllerType defaultControllerType = RCI.GetDefaultControllerType(DefaultControllerType.XBox);
			RCI.Controller activeJoystick = RCI.GetActiveJoystick();
			bool flag = defaultControllerType == DefaultControllerType.XBox && activeJoystick != null;
			bool flag2 = defaultControllerType == DefaultControllerType.PS && activeJoystick != null;
			if (!flag && !flag2)
			{
				buttonIcon.sprite = m_defaultButtonImage;
				buttonIcon.rectTransform.sizeDelta = m_defaultImageSize;
				return;
			}
			buttonIcon.enabled = true;
			switch (hotkey)
			{
			case ConsoleButtons.ActionBottomRow1:
				buttonIcon.sprite = (flag ? xboxButtonA : (IsButtonInvert ? psButtonB : psButtonA));
				break;
			case ConsoleButtons.ActionBottomRow2:
				buttonIcon.sprite = (flag ? xboxButtonB : (IsButtonInvert ? psButtonA : psButtonB));
				break;
			case ConsoleButtons.ActionTopRow1:
				buttonIcon.sprite = (flag ? xboxButtonX : psButtonX);
				break;
			case ConsoleButtons.ActionTopRow2:
				buttonIcon.sprite = (flag ? xboxButtonY : psButtonY);
				break;
			case ConsoleButtons.LeftShoulder1:
				buttonIcon.sprite = (flag ? xboxButtonLB : psButtonLB);
				break;
			case ConsoleButtons.LeftShoulder2:
				buttonIcon.sprite = (flag ? xboxButtonLT : psButtonLT);
				break;
			case ConsoleButtons.RightShoulder1:
				buttonIcon.sprite = (flag ? xboxButtonRB : psButtonRB);
				break;
			case ConsoleButtons.RightShoulder2:
				buttonIcon.sprite = (flag ? xboxButtonRT : psButtonRT);
				break;
			}
			buttonIcon.SetNativeSize();
		}

		public void OnUpdate()
		{
			if (!app_cache)
			{
				return;
			}
			if (app_cache.view.ui.dialog != null && app_cache.view.ui.dialog.isVisible)
			{
				if (!m_isDialog)
				{
					return;
				}
				if (RCI.GetButtonDown(hotkey))
				{
					Debug.LogWarning($"DRLGamepadHotkey> Hotkey [{hotkey}] HIT");
				}
				CheckInput();
			}
			if ((bool)button && button.gameObject.activeInHierarchy && (bool)buttonIcon && (bool)m_screen && !(m_screen.alpha <= 0f))
			{
				ConsoleButtons consoleButtons = hotkey;
				if ((uint)(consoleButtons - 16) > 1u || !IsFilteredEvent())
				{
					CheckInput();
				}
			}
		}

		private bool IsFilteredEvent()
		{
			for (int i = 0; i < filteredEvents.Length; i++)
			{
				string value = filteredEvents[i];
				if (button.notification.StartsWith(value))
				{
					return true;
				}
			}
			return false;
		}

		private void CheckInput()
		{
			if (!RCI.GetButtonDown(hotkey))
			{
				return;
			}
			UINavigation.Focus(button);
			this.TimerRunOnce(delegate
			{
				if (button is DRLToggleView dRLToggleView)
				{
					dRLToggleView.isOn = !dRLToggleView.isOn;
				}
				else if (!string.IsNullOrEmpty(button.notification))
				{
					PointerEventData p_event_data = new PointerEventData(EventSystem.current);
					button.OnPointerClick(p_event_data);
				}
				else
				{
					Button component = button.GetComponent<Button>();
					if ((bool)component && component.interactable)
					{
						if ((bool)base.app && (bool)base.app.view && (bool)base.app.view.audio && m_isDialog)
						{
							base.app.view.audio.PlayUIClick();
						}
						PointerEventData p_event_data = new PointerEventData(EventSystem.current);
						component.OnPointerClick(p_event_data);
					}
				}
			}, 0.1f);
		}
	}
}

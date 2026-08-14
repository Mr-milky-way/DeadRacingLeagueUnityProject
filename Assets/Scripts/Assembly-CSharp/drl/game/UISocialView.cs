using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UISocialView : View<DRLApp>
	{
		public float panelWidth;

		public DRLTabGroup tabGroup;

		public GameObject body;

		public UIChatView chat;

		public UIPanelFriendsView friends;

		public GameObject friendsHeader;

		public UIChatBlockedView blockedView;

		public DRLTabGroup chatTabGroup;

		public UINavigation m_lastGlobalNavigation;

		public GraphicRaycaster graphicRaycaster;

		private Vector2 m_chatLayoutDefaultPosition;

		[SerializeField]
		[HideInInspector]
		private float m_transition;

		public bool useGameTemplate;

		private bool m_isActive = true;

		public bool open { get; set; }

		public UIElementView elementView => AssertLocal<UIElementView>("elementView");

		public RectTransform rectTransform => AssertLocal<RectTransform>("rectTransform");

		public Vector2 position
		{
			get
			{
				return rectTransform.anchoredPosition;
			}
			set
			{
				rectTransform.anchoredPosition = value;
			}
		}

		public float x
		{
			get
			{
				return position.x;
			}
			set
			{
				Vector2 vector = position;
				vector.x = value;
				position = vector;
			}
		}

		public float startingPosition => rectTransform.sizeDelta.x + 20f;

		public CanvasGroup canvasGroup => AssertLocal<CanvasGroup>("canvasGroup");

		public float transition
		{
			get
			{
				return m_transition;
			}
			set
			{
				m_transition = value;
				OnTransition(m_transition);
			}
		}

		public float alpha
		{
			get
			{
				return canvasGroup.alpha;
			}
			set
			{
				canvasGroup.alpha = value;
			}
		}

		public bool isActive
		{
			get
			{
				return m_isActive;
			}
			set
			{
				m_isActive = value;
			}
		}

		private void Start()
		{
			m_chatLayoutDefaultPosition = rectTransform.anchoredPosition;
		}

		private void OnTransition(float p_value)
		{
			if (p_value >= 0f && !useGameTemplate)
			{
				x = startingPosition - p_value * startingPosition;
			}
			alpha = p_value;
		}

		private void Fade(float p_transition, float p_duration = 0.3f, float p_delay = 0f, Easing p_easing = null)
		{
			Tween.Kill(this, "transition");
			Tween.Add(this, "transition", p_transition, p_duration, p_delay, (p_easing == null) ? new Easing(Cubic.Out) : p_easing);
		}

		public void Show(float p_duration = 0.3f)
		{
			if (isActive)
			{
				UINotificationView notifications = base.app.view.ui.notifications;
				if (base.transform.GetSiblingIndex() < notifications.transform.GetSiblingIndex())
				{
					base.transform.SetSiblingIndex(notifications.transform.GetSiblingIndex());
				}
				if (DRLUINavigationSystem.lastNavigationDown != null && !DRLUINavigationSystem.lastNavigationDown.transform.IsChild(base.transform))
				{
					m_lastGlobalNavigation = (notifications.focused ? notifications.GetLastNavigation() : DRLUINavigationSystem.lastNavigationDown);
				}
				notifications.HideHistoryPanel(p_duration);
				Fade(1f, p_duration);
				Notify("social.panel.shown");
				open = true;
				if (useGameTemplate)
				{
					base.app.view.ui.navigation.enabled = true;
					chat.PruneMessages();
				}
				this.TimerRunOnce(delegate
				{
					chat.input.field.ActivateInputField();
					UINavigation.Focus(chat.inputNavigation);
				}, p_duration);
				if ((bool)graphicRaycaster)
				{
					graphicRaycaster.enabled = true;
				}
				ToggleChatPosition();
			}
		}

		public void Hide(float p_duration = 0.3f)
		{
			if (!base.validContext)
			{
				return;
			}
			Fade(0f, p_duration);
			open = false;
			Notify("social.panel.hidden");
			if (base.app.model.game != null && base.app.model.game.simulation != null)
			{
				if (base.app.view.ui.screens.current == null)
				{
					base.app.view.ui.navigation.enabled = false;
				}
				if (!base.app.inGarage)
				{
					ClearIgnoredCommands();
				}
			}
			this.TimerRunOnce(delegate
			{
				UINavigation component = base.app.view.ui.footer.notificationsButton.GetComponent<UINavigation>();
				if (m_lastGlobalNavigation != null)
				{
					UIFooterView.SetNavigationTop(m_lastGlobalNavigation);
					DRLUINavigationSystem.lastNavigationDown = m_lastGlobalNavigation;
				}
				else
				{
					UIFooterView.SetNavigationTop((base.app.view.ui.screens.current != null) ? base.app.view.ui.screens.current.transform : null);
				}
				if (UINavigation.focus != component && UIFooterView.buttonNavs.Count > 0)
				{
					UINavigation.Focus(UIFooterView.buttonNavs[0].up);
				}
			}, 0.05f);
			if ((bool)graphicRaycaster)
			{
				graphicRaycaster.enabled = false;
			}
		}

		public void ToggleChatPosition()
		{
			if (base.app.inGame && useGameTemplate)
			{
				if (!base.app.view.ui.game.hud.damage.isVisible)
				{
					rectTransform.anchoredPosition = m_chatLayoutDefaultPosition;
				}
				else
				{
					rectTransform.anchoredPosition = new Vector3(rectTransform.anchoredPosition.x, -175f);
				}
			}
		}

		public void ExpandSocialPanel(float p_duration = 0.25f, float p_delay = 0.2f)
		{
			Tween.Kill(rectTransform, "offsetMin");
			Tween.Add(rectTransform, "offsetMin", new Vector2(rectTransform.offsetMin.x, 0f), p_duration, p_delay, Cubic.Out);
		}

		public void ContractSocialPanel(float p_duration = 0.3f, float p_delay = 0.2f)
		{
			Tween.Kill(rectTransform, "offsetMin");
			Tween.Add(rectTransform, "offsetMin", new Vector2(rectTransform.offsetMin.x, 70f), p_duration, p_delay, Cubic.Out);
		}

		public void SetIgnoredGameCommands()
		{
			List<GameCommand> list = new List<GameCommand>();
			if (!base.app.controller)
			{
				return;
			}
			foreach (GameInputMapComponent map in base.app.controller.game.input.maps)
			{
				foreach (GameCommand command in map.commands)
				{
					if (command.type != GameCommandType.Pause)
					{
						list.Add(command);
					}
				}
			}
			base.app.controller.game.input.SetIgnoredCommands(list);
		}

		public void ClearIgnoredCommands()
		{
			if ((bool)base.app.controller)
			{
				base.app.controller.game.input.ClearIgnoredCommands();
			}
		}

		public UINavigation GetLastNavigation()
		{
			return m_lastGlobalNavigation;
		}
	}
}

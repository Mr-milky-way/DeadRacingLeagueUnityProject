using UnityEngine;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UISocialController : Controller<DRLApp>
	{
		private UIChatView m_chat;

		private UIPanelFriendsView m_friends;

		private UIChatBlockedView m_blocked;

		private GameObject m_clan;

		private Component m_socialButtonLastReference;

		public SocialShortcuts SocialShortcuts;

		private bool m_panelAnimating;

		private bool m_focusAnimating;

		public static bool firstTimeOpened = true;

		private bool m_inGame;

		private bool m_is_game_scene;

		private Activity m_inactiveTimer;

		public UISocialView view => AssertLocal<UISocialView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			if (p_event == null)
			{
				return;
			}
			switch (p_event)
			{
			case "scene.load":
				m_is_game_scene = base.app.level.IsLevelLoaded("game");
				break;
			case "social.panel.toggle@click":
				TogglePanel();
				m_chat?.FocusPanel();
				break;
			case "social.panel.tab@change":
			{
				DRLTabGroup dRLTabGroup = p_target as DRLTabGroup;
				if ((bool)dRLTabGroup)
				{
					SetTab(dRLTabGroup.selection);
				}
				break;
			}
			case "social.friend.pm-button@click":
				if (p_data.Length != 0)
				{
					view.tabGroup.index = 0;
					Notify("chat.private.invite", p_data);
				}
				break;
			case "ui.screen@open":
				view.Hide();
				break;
			case "game.race.slowmo@start":
				view.Hide();
				break;
			case "chat.panel@active":
			case "chat.panel@inactive":
				if (m_inGame)
				{
					if (!view.open && base.app.view.ui.screens.current == null)
					{
						Show(0.3f);
					}
					OnChatActivity();
				}
				break;
			case "chat.incoming.public":
			case "network.room.chat.incoming":
			case "chat.incoming.private":
				if (m_inGame && base.app.model.storage.state.player.settings.game.chat && (!(p_event == "chat.incoming.public") || base.app.model.network.room == null))
				{
					UIScreen current = base.app.view.ui.screens.current;
					string text = "game-spectate-screen";
					string text2 = "game-race-complete-screen";
					if (!view.open && (current == null || (current != null && (current.name == text || current.name == text2))))
					{
						Show(0.3f, showWarning: false);
					}
					OnChatActivity();
				}
				break;
			case "chat.panel@click":
				m_chat.FocusPanel();
				ClearTimer();
				break;
			case "chat.toggle.height":
				view.ToggleChatPosition();
				break;
			}
		}

		private void Awake()
		{
			m_chat = view.chat;
			m_friends = view.friends;
			m_blocked = view.blockedView;
			if ((bool)m_friends)
			{
				m_friends.gameObject.name = view.friends.name;
			}
			view.open = false;
			m_inGame = view.useGameTemplate;
			Hide(0f, p_holdScroll: false);
		}

		protected void Update()
		{
			if (view.open)
			{
				if (Input.GetKeyDown(KeyCode.Mouse0) && !view.elementView.over && !base.app.view.ui.footer.socialButtonView.down)
				{
					if (!m_inGame || base.app.view.ui.screens.current != null)
					{
						view.Hide();
					}
					if (m_chat != null)
					{
						m_chat.input.ClearInputText();
						m_chat.input.field.DeactivateInputField();
						m_chat.UnfocusPanel();
						OnChatActivity();
						DRLUINavigationSystem.IsTyping = false;
						UINavigation.focus = null;
					}
					EnableScroll();
				}
				if (RCI.GetButtonDown(ConsoleButtons.ActionBottomRow2))
				{
					this.TimerRunOnce(delegate
					{
						if (base.validContext)
						{
							Hide(0.3f, p_holdScroll: false);
						}
					}, 0.05f);
				}
			}
			UIScreen current = base.app.view.ui.screens.current;
			bool flag = current;
			string text = "game-spectate-screen";
			string text2 = "game-race-complete-screen";
			bool flag2 = false;
			if ((bool)current)
			{
				flag2 = current.name == text || current.name == text2;
			}
			if ((!flag2 && flag) || m_is_game_scene)
			{
				return;
			}
			if (Input.GetKeyDown(SocialShortcuts.keyboard) && (SocialShortcuts.keyboard != KeyCode.Y || !view.open))
			{
				if (DRLUINavigationSystem.IsTyping)
				{
					return;
				}
				TogglePanel();
				if (view.open)
				{
					m_chat?.FocusPanel();
				}
			}
			if (Input.GetKeyDown(SocialShortcuts.chatFocus) || RCI.GetButtonDown(SocialShortcuts.chatFocusGamepad))
			{
				if (m_chat == null || !m_inGame || m_focusAnimating || base.app.view.ui.dialog.isVisible)
				{
					return;
				}
				ClearTimer();
				if (!view.open)
				{
					TogglePanel();
					m_chat.FocusPanel();
					m_focusAnimating = true;
					this.TimerRunOnce(delegate
					{
						m_focusAnimating = false;
					}, 0.5f);
					return;
				}
				if (UINavigation.focus != null && UINavigation.focus.name == "autocomplete-input")
				{
					return;
				}
				m_chat.ToggleFocus();
				m_focusAnimating = true;
				this.TimerRunOnce(delegate
				{
					m_focusAnimating = false;
				}, 0.5f);
			}
			if (Input.GetKeyDown(KeyCode.Escape) && !(m_chat == null) && m_inGame && !m_focusAnimating)
			{
				ClearTimer();
				m_chat.UnfocusPanel();
				m_chat.input.ClearInputText();
				m_chat.input.field.DeactivateInputField();
				m_focusAnimating = true;
				this.TimerRunOnce(delegate
				{
					m_focusAnimating = false;
				}, 0.5f);
			}
		}

		private void TogglePanel(float p_duration = 0.3f, bool p_holdScroll = false)
		{
			if (view.isActive && !m_panelAnimating)
			{
				m_panelAnimating = true;
				this.TimerRunOnce(delegate
				{
					m_panelAnimating = false;
				}, p_duration + 0.2f);
				if (view.open)
				{
					Hide(p_duration, p_holdScroll);
				}
				else
				{
					Show(p_duration);
				}
			}
		}

		public void Show(float p_duration, bool showWarning = true)
		{
			if (!base.validContext || !view || !view.isActive)
			{
				return;
			}
			bool flag = false;
			view.Show(p_duration);
			if (!m_chat)
			{
				return;
			}
			m_chat.SetPrivateChannels();
			if (!flag)
			{
				if (!view.useGameTemplate)
				{
					view.tabGroup.index = 0;
					UINavigation.focus = view.tabGroup.tabs[0].GetComponent<UINavigation>();
					SetTab("chat");
				}
				DisableScroll();
			}
			else
			{
				UINavigation[] componentsInChildren = base.transform.GetComponentsInChildren<UINavigation>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].disableNavigation = true;
				}
			}
			NetworkModel network = base.app.model.network;
			if ((bool)network && network.room != null)
			{
				m_chat.SetDefaultChannel("room-chat");
			}
			else
			{
				m_chat.SetDefaultChannel("global-chat");
			}
			if (m_chat.activeChannel == "global-chat" && base.app.model.game == null && firstTimeOpened)
			{
				base.app.model.chat.SendInfoMessage();
				firstTimeOpened = false;
			}
			if (!view.useGameTemplate)
			{
				m_chat.messagesList.Clear();
				m_chat.LoadMessages();
			}
			else
			{
				m_chat.PruneMessages();
			}
			m_chat?.input.Activate();
		}

		public void Hide(float p_duration, bool p_holdScroll)
		{
			m_chat.input.ClearInputText();
			m_chat.input.field.CancelInvoke();
			m_chat.input.field.DeactivateInputField();
			DRLUINavigationSystem.IsTyping = false;
			UINavigation.focus = null;
			view.Hide(p_duration);
			this.TimerRunOnce(delegate
			{
				base.app.view.ui.footer.socialButtonNavigation.up = null;
				m_chat.ResetPanel();
			}, p_duration);
			ClearTimer();
			if (!p_holdScroll)
			{
				EnableScroll();
			}
		}

		private void SetTab(string p_id)
		{
			switch (p_id)
			{
			case "chat":
				m_friends?.Hide();
				m_blocked?.Hide();
				break;
			case "friends":
				m_friends?.Show();
				m_blocked?.Hide();
				break;
			case "blocked":
				m_blocked?.Show();
				break;
			}
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

		protected bool GetInput(ConsoleButtons k, bool d)
		{
			if (k < (ConsoleButtons)0)
			{
				return false;
			}
			if (!d)
			{
				return RCI.GetButtonUp(k);
			}
			return RCI.GetButtonDown(k);
		}

		public void OnChatActivity()
		{
			ClearTimer();
			if (!m_inGame || m_chat.focused || !view.open)
			{
				return;
			}
			m_inactiveTimer = this.TimerRunOnce(delegate
			{
				if (view.open)
				{
					Hide(0.3f, p_holdScroll: false);
				}
			}, 10f);
		}

		private void ClearTimer()
		{
			if (m_inactiveTimer != null)
			{
				m_inactiveTimer.Stop();
				m_inactiveTimer.manager.Remove(m_inactiveTimer);
				m_inactiveTimer = null;
			}
		}
	}
}

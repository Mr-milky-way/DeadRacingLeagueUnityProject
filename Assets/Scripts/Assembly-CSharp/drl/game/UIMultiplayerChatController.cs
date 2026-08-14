using UnityEngine;
using drl.network;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIMultiplayerChatController : Controller<DRLApp>
	{
		private float m_keyspeed;

		private int m_spammingMsgCount;

		private float m_spammingTimer;

		public NetworkModel model => base.app.model.network;

		public UIMultiplayerChatView view => AssertLocal<UIMultiplayerChatView>("view");

		public override void OnNotification(string p_event, Object p_target, params object[] p_data)
		{
			switch (p_event)
			{
			case "network.room@enter":
				base.app.view.ui.screenBack = true;
				break;
			case "network.room@exit":
				base.app.view.ui.screenBack = true;
				view.Clear();
				break;
			case "multiplayer.chat.panel@click":
				EnableChatScrolling(p_enable: true);
				break;
			case "multiplayer.chat.input@change":
			{
				DRLInputFieldView dRLInputFieldView = p_target as DRLInputFieldView;
				if (view.IsInputWaitLabelEnabled())
				{
					dRLInputFieldView.field.text = "";
				}
				break;
			}
			case "multiplayer.chat.input@end-edit":
			{
				view.verticalScrollbar.value = 0f;
				DRLInputFieldView dRLInputFieldView2 = p_target as DRLInputFieldView;
				string text = dRLInputFieldView2.field.text;
				text = text.Trim();
				if (!string.IsNullOrEmpty(text))
				{
					Debug.Log("UIMultiplayerChatController> ChatInputEndEdit\n" + text);
					model.SendChatMessage(text);
					dRLInputFieldView2.field.text = "";
					CheckForMsgSpamming();
				}
				break;
			}
			case "network.room.chat.incoming":
			{
				NetworkRoomChat.Message message = (NetworkRoomChat.Message)p_data[0];
				view.Add(message.PlayerId, message.SenderName, message.Content, message.IsMine ? true : false);
				break;
			}
			}
		}

		private void CheckForMsgSpamming()
		{
			if (m_spammingTimer <= 0f)
			{
				m_spammingTimer = 3f;
				m_spammingMsgCount = 0;
			}
			else
			{
				m_spammingMsgCount++;
			}
			if (m_spammingMsgCount >= 3 && m_spammingTimer > 0f)
			{
				view.EnableInputWaitLabel(p_enable: true, 5);
			}
		}

		private void EnableChatScrolling(bool p_enable)
		{
			if (p_enable)
			{
				if ((bool)view.chatHandleNav)
				{
					UINavigation.focus = view.chatHandleNav;
					base.app.view.ui.screenBack = false;
					if ((bool)view.chatScrollBarFade)
					{
						view.chatScrollBarFade.pulse = true;
					}
				}
				else
				{
					Debug.LogError("Can't find chat's window scrolling handle! It is not set on the view component.");
				}
				return;
			}
			RunOnce(1f / 30f, delegate
			{
				if ((bool)view.chatPanelNav)
				{
					UINavigation.focus = view.chatPanelNav;
				}
				else
				{
					Debug.LogError("Can't find chat panel navigation element to return focus to! It's not set on the view component.");
				}
				base.app.view.ui.screenBack = true;
				if ((bool)view.chatScrollBarFade)
				{
					if (view.chatScrollBarFade.pulse)
					{
						view.chatScrollBarFade.FadeOut();
					}
					view.chatScrollBarFade.pulse = false;
				}
			});
		}

		protected void Update()
		{
			if (m_spammingTimer >= 0f)
			{
				m_spammingTimer -= Time.deltaTime;
			}
			if (!view.chatHandleNav || !(UINavigation.focus == view.chatHandleNav))
			{
				return;
			}
			bool flag = false;
			if (DRLUINavigationSystem.IsButton() || base.app.view.ui.IsBackPressedController() || base.app.view.ui.IsBackPressedKeyboard())
			{
				flag = true;
			}
			if (flag)
			{
				EnableChatScrolling(p_enable: false);
				return;
			}
			float num = 0f;
			float num2 = -1f;
			float p = 8f;
			float num3 = 1f;
			float num4 = 1f;
			bool flag2 = false;
			if (!RCI.IsRCController() && RCI.HasNavigationController)
			{
				num = 0f - RCI.GetRawAxis(RawAxis.LeftStickY, RCI.navigationController);
			}
			if (Input.GetKey(KeyCode.UpArrow))
			{
				num = (0f - num4) * m_keyspeed;
				p = 1f;
				flag2 = true;
			}
			if (Input.GetKey(KeyCode.DownArrow))
			{
				num = num4 * m_keyspeed;
				p = 1f;
				flag2 = true;
			}
			if (flag2)
			{
				m_keyspeed += Time.unscaledDeltaTime;
				m_keyspeed = Mathf.Clamp01(m_keyspeed);
			}
			else
			{
				m_keyspeed = 0f;
			}
			float num5 = ((num < 0f) ? (-1f) : 1f);
			num = Mathf.Pow(Mathf.Abs(num), p);
			float num6 = num5 * num * num2 * 2f * Time.unscaledDeltaTime * num3;
			if (Mathf.Abs(num) <= 0.001f)
			{
				num6 = 0f;
			}
			view.verticalScrollbar.value += num6;
		}
	}
}

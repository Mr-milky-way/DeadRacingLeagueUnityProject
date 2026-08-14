using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using drl.network;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIMultiplayerChatView : View<DRLApp>
	{
		public DRLInputFieldView input;

		public RectTransform list;

		public UIMultiplayerChatItemHeaderView headerTemplate;

		public Text textTemplate;

		public Canvas canvas;

		public Scrollbar verticalScrollbar;

		public UINavigation chatHandleNav;

		public UINavigation chatPanelNav;

		public FadeComponent chatScrollBarFade;

		public Text inputWriteLabel;

		public Text inputWaitLabel;

		private MonoActivity m_canvas_hide_timer;

		public void Clear()
		{
			canvas.enabled = false;
			float num = 0f;
			for (int i = 0; i < list.childCount; i++)
			{
				Object.Destroy(list.GetChild(i).gameObject, num);
				num += 0.0016666667f;
			}
			EnableInputWaitLabel(p_enable: false, 0);
			if (m_canvas_hide_timer != null)
			{
				m_canvas_hide_timer.Stop();
			}
			m_canvas_hide_timer = RunOnce(delegate
			{
				canvas.enabled = true;
			}, num);
		}

		public void EnableInputWaitLabel(bool p_enable, int p_seconds)
		{
			inputWriteLabel.gameObject.SetActive(!p_enable);
			inputWaitLabel.gameObject.SetActive(p_enable);
			StopCoroutine("InputWaitLabelTimer");
			if (p_enable)
			{
				StartCoroutine("InputWaitLabelTimer", p_seconds);
			}
		}

		public bool IsInputWaitLabelEnabled()
		{
			return inputWaitLabel.gameObject.activeInHierarchy;
		}

		private IEnumerator InputWaitLabelTimer(int p_seconds)
		{
			for (int i = p_seconds; i > 0; i--)
			{
				inputWaitLabel.text = "PLEASE WAIT..." + i;
				yield return new WaitForSeconds(1f);
			}
			EnableInputWaitLabel(p_enable: false, 0);
		}

		public void Add(string p_player_id, string p_name, string p_text, bool p_left)
		{
			UIMultiplayerChatItemHeaderView uIMultiplayerChatItemHeaderView = PushHeader();
			Text text = PushMessage();
			uIMultiplayerChatItemHeaderView.isLeft = p_left;
			uIMultiplayerChatItemHeaderView.title = p_name.ToUpper();
			uIMultiplayerChatItemHeaderView.LoadPhoto(p_player_id);
			text.text = p_text.Trim();
			text.alignment = ((!p_left) ? TextAnchor.UpperRight : TextAnchor.UpperLeft);
			Hierarchy.RefreshLayout(list);
		}

		public void Add(NetworkActor p_player, string p_text, bool p_left)
		{
			Add(p_player.PlayerId, p_player.ProfileName, p_text, p_left);
		}

		public void Add(Texture p_photo, string p_name, string p_text, bool p_left)
		{
			UIMultiplayerChatItemHeaderView uIMultiplayerChatItemHeaderView = PushHeader();
			Text text = PushMessage();
			uIMultiplayerChatItemHeaderView.isLeft = p_left;
			uIMultiplayerChatItemHeaderView.title = p_name.ToUpper();
			uIMultiplayerChatItemHeaderView.photo = p_photo;
			text.text = p_text.Trim();
			Hierarchy.RefreshLayout(list);
		}

		public void Add(GamePlayerData p_player, string p_text, bool p_left)
		{
			Add(p_player.photo, p_player.name, p_text, p_left);
		}

		protected UIMultiplayerChatItemHeaderView PushHeader()
		{
			UIMultiplayerChatItemHeaderView result = Object.Instantiate(headerTemplate, list);
			canvas.enabled = false;
			canvas.enabled = true;
			RefreshListHierarchy();
			return result;
		}

		protected Text PushMessage()
		{
			Text result = Object.Instantiate(textTemplate, list);
			canvas.enabled = false;
			canvas.enabled = true;
			RefreshListHierarchy();
			return result;
		}

		protected void RefreshListHierarchy()
		{
			bool flag = true;
			int num = 0;
			for (int i = 0; i < list.childCount; i++)
			{
				list.GetChild(i).name = (flag ? ("m" + num) : ("h" + num));
				flag = !flag;
				if (flag)
				{
					num++;
				}
			}
		}
	}
}

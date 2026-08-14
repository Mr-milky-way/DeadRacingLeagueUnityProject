using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UINetworkView : View<DRLApp>
	{
		public UIFooterView footer;

		public GameObject lobbyContainer;

		public FadeComponent lobbyFader;

		public Text lobbyServerField;

		public Image lobbyServerIcon;

		public Text lobbyRoomCountField;

		public FadeComponent separatorFade;

		public UIElementView lobbyServerButton;

		public GameObject lobbyServerButtonFocus;

		public Image lobbyServerButtonStripe;

		public RectTransform lobbyServerListRect;

		public UIElementView lobbyServerListLANButton;

		public float lobbyServerListHeightMax = 280f;

		public float lobbyServerListItemHeight = 50f;

		public Color lobbyServerButtonEnabledColor = new Color32(80, 227, 194, byte.MaxValue);

		public Color lobbyServerButtonDisabledColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		[HideInInspector]
		public float lobbyServerListHeightOpen;

		public RawImage[] lobbyServerQualityStateFields;

		public Text lobbyServerQualityMsField;

		public GameObject lanStartButton;

		public GameObject lanConnectButton;

		public DRLInputFieldView lanIPInputField;

		public GameObject lanDisconnectButton;

		public GameObject lanConnectingButton;

		public List<GameObject> servers;

		private string footerLobbyOverrideString;

		public float lobbyServerListWidth
		{
			get
			{
				return lobbyServerListRect.sizeDelta.x;
			}
			set
			{
				Vector2 sizeDelta = lobbyServerListRect.sizeDelta;
				sizeDelta.x = value;
				lobbyServerListRect.sizeDelta = sizeDelta;
			}
		}

		public float lobbyServerListHeight
		{
			get
			{
				return lobbyServerListRect.sizeDelta.y;
			}
			set
			{
				Vector2 sizeDelta = lobbyServerListRect.sizeDelta;
				sizeDelta.y = value;
				lobbyServerListRect.sizeDelta = sizeDelta;
			}
		}

		public float pingQuality
		{
			set
			{
				int num = Mathf.RoundToInt(value * 2f);
				for (int i = 0; i < lobbyServerQualityStateFields.Length; i++)
				{
					lobbyServerQualityStateFields[i].gameObject.SetActive(i == num);
				}
			}
		}

		public int pingMs
		{
			set
			{
				lobbyServerQualityMsField.text = ((value < 0) ? "--- ms" : (Mathf.Min(value, 999) + "ms"));
			}
		}

		public void EnableNetworkInFooter(bool p_enabled)
		{
			if ((bool)lobbyContainer)
			{
				lobbyContainer.SetActive(p_enabled);
				if ((bool)footer)
				{
					footer.socialGroupLayout.gameObject.SetActive(!p_enabled);
					footer.socialToggle.gameObject.SetActive(!p_enabled);
					footer.SetSocialExpanded(p_flag: false);
				}
				if (!p_enabled)
				{
					ShowNetworkInFooter(p_visible: false);
				}
			}
		}

		public void ShowNetworkInFooter(bool p_visible)
		{
			if (p_visible)
			{
				lobbyFader.FadeIn();
			}
			else
			{
				lobbyFader.FadeOut(0.001f);
			}
		}

		public void SetLobbyServerButtonEnabled(bool p_enable)
		{
			if ((int)lobbyServerListHeightOpen == 0)
			{
				lobbyServerListHeightOpen = lobbyServerListHeightMax;
			}
			bool flag = false;
			flag = true;
			if (Application.platform == RuntimePlatform.OSXPlayer)
			{
				flag = false;
			}
			SetLobbyServerListLANEnabled(flag);
			if (lobbyServerButton != null)
			{
				lobbyServerButton.enabled = p_enable;
			}
			if (lobbyServerListRect != null)
			{
				lobbyServerListHeight = 0f;
				lobbyServerListRect.gameObject.SetActive(p_enable);
			}
			Color color = (p_enable ? lobbyServerButtonEnabledColor : lobbyServerButtonDisabledColor);
			if (lobbyServerField != null)
			{
				lobbyServerField.color = color;
			}
			if (lobbyServerIcon != null)
			{
				lobbyServerIcon.color = color;
			}
			if (lobbyRoomCountField != null)
			{
				lobbyRoomCountField.color = color;
			}
			if (lobbyServerButtonStripe != null)
			{
				lobbyServerButtonStripe.color = color;
			}
			base.app.view.ui.footer.RefreshNavigationButtons();
			base.app.view.ui.footer.SetConnectionButtonActive(!p_enable);
		}

		public void SetLobbyServerListLANEnabled(bool p_lanEnabled)
		{
			if (!base.app.model.storage.state.player.profile.isDeveloper)
			{
				p_lanEnabled = false;
			}
			if ((int)(lobbyServerListHeightOpen = (p_lanEnabled ? lobbyServerListHeightMax : (lobbyServerListHeightMax - lobbyServerListItemHeight))) > 0)
			{
				lobbyServerListHeight = lobbyServerListHeightOpen;
			}
			if (lobbyServerListLANButton != null)
			{
				lobbyServerListLANButton.gameObject.SetActive(p_lanEnabled);
			}
		}

		public void ToggleLobbyServerList()
		{
			int num = (int)lobbyServerListHeight;
			ShowLobbyServerList(num <= 0);
		}

		public void ShowLobbyServerList(bool p_show, bool p_force = false)
		{
			int num = (int)lobbyServerListHeight;
			if (p_show && num > 0)
			{
				return;
			}
			int num2 = (int)lobbyServerListHeightOpen;
			if (!p_show && !p_force && num < num2 - 2)
			{
				return;
			}
			lobbyServerListHeight = (p_show ? 0f : ((float)num2));
			float p_to = (p_show ? ((float)num2) : 0f);
			Tween.Kill(this, "lobbyServerListHeight");
			Tween.Add(this, "lobbyServerListHeight", p_to, 0.3f, 0f, Cubic.Out);
			if (p_show)
			{
				RectTransform component = lobbyServerListRect.parent.gameObject.GetComponent<RectTransform>();
				if (component != null)
				{
					lobbyServerListWidth = component.sizeDelta.x;
				}
			}
			base.app.view.ui.footer.SetupLobbyNavigation(p_show);
		}

		public void UpdateLobby(string p_server, int p_room_count)
		{
			lobbyServerField.text = p_server;
			lobbyRoomCountField.text = p_room_count.ToString() ?? "";
		}

		public void RefreshFooterLobby(string p_region, int p_roomsCount)
		{
			UpdateLobby((footerLobbyOverrideString != "") ? footerLobbyOverrideString : p_region, p_roomsCount);
		}
	}
}

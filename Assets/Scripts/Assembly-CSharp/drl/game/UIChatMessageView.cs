using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIChatMessageView : View<DRLApp>
	{
		internal static Dictionary<string, Texture> m_cache;

		[SerializeField]
		public List<Sprite> badges;

		public UINavigation uinav;

		public GameObject leftContent;

		public GameObject rightContent;

		public GameObject leftMessage;

		public GameObject messageContent;

		public GameObject infoContent;

		public GameObject infoBackground;

		public GameObject infoSpaceTop;

		public GameObject infoSpaceBottom;

		public GameObject focus;

		public RawImage leftPhotoField;

		public Image leftColorField;

		public Text leftHeaderField;

		public Text leftMsgField;

		public Text leftTimeStamp;

		public Image leftRankBadge;

		public RawImage rightPhotoField;

		public Image rightColorField;

		public Text rightHeaderField;

		public Text rightMsgField;

		public Text rightTimneStamp;

		public Image rightRankBadge;

		public Image submenuIcon;

		public GameObject leftPhotoContainer;

		public GameObject rightPhotoContainer;

		public Outline titleOutline;

		public Outline messageOutline;

		public Outline timeStampOutline;

		public GameObject indent;

		public Text indentField;

		[Header("Subpanel")]
		[Tooltip("Optional: container of the Image of the toggle")]
		[SerializeField]
		private GameObject m_submenuToggleContainer;

		public Image messageBackground;

		public UINavigation infoNav;

		[Header("PS4")]
		public GameObject steamButton;

		public GameObject discordIcon;

		public GameObject discordBaseText;

		public GameObject discordPs4text;

		public GameObject zendeskIcon;

		public GameObject zendeskBaseText;

		public GameObject zendeskPs4text;

		private Color m_channelColor;

		private string m_title;

		private bool m_isMine;

		internal static Dictionary<string, Texture> cache => Reflection<object>.Assert(ref m_cache);

		public bool isInfo { get; private set; }

		private Texture photo
		{
			get
			{
				if (leftPhotoField == null || rightPhotoField == null)
				{
					return null;
				}
				if (!isMine)
				{
					return rightPhotoField.texture;
				}
				return leftPhotoField.texture;
			}
			set
			{
				if (leftPhotoField != null && rightPhotoField != null)
				{
					RawImage rawImage = leftPhotoField;
					Texture texture = (rightPhotoField.texture = value);
					rawImage.texture = texture;
				}
			}
		}

		public Color userColor
		{
			get
			{
				if (!isMine)
				{
					return rightColorField.color;
				}
				return leftColorField.color;
			}
			set
			{
				if (!(leftColorField == null))
				{
					Image image = leftColorField;
					Color color = (rightColorField.color = value);
					image.color = color;
				}
			}
		}

		public Color channelColor
		{
			get
			{
				return m_channelColor;
			}
			set
			{
				Color color = value;
				if (leftHeaderField != null)
				{
					leftHeaderField.color = color;
					rightHeaderField.color = color;
					leftMsgField.color = color;
					submenuIcon.color = color;
					color = color * 155f / 255f;
					color.a = 1f;
					leftTimeStamp.color = color;
					rightTimneStamp.color = color;
				}
				m_channelColor = color;
				indentField.color = color;
				rightMsgField.color = color;
			}
		}

		public Color outlineColor
		{
			set
			{
				if ((bool)timeStampOutline)
				{
					timeStampOutline.effectColor = value;
				}
				if ((bool)titleOutline)
				{
					titleOutline.effectColor = value;
				}
				messageOutline.effectColor = value;
			}
		}

		public string time
		{
			set
			{
				if (!string.IsNullOrEmpty(value) && !(leftTimeStamp == null))
				{
					Text text = leftTimeStamp;
					string text2 = (rightTimneStamp.text = value.ToUpper());
					text.text = text2;
				}
			}
		}

		public string title
		{
			get
			{
				if (!leftHeaderField)
				{
					return m_title;
				}
				return leftHeaderField.text;
			}
			set
			{
				if (leftHeaderField != null)
				{
					Text text = leftHeaderField;
					string text2 = (rightHeaderField.text = value);
					text.text = text2;
				}
				m_title = value;
			}
		}

		public string message
		{
			get
			{
				return rightMsgField.text;
			}
			set
			{
				rightMsgField.text = value;
			}
		}

		public int rankBadge
		{
			set
			{
				if (leftRankBadge == null || rightRankBadge == null)
				{
					return;
				}
				if (value > 0 && value <= badges.Count)
				{
					Sprite sprite = badges[value - 1];
					Image image = leftRankBadge;
					Sprite sprite2 = (rightRankBadge.sprite = sprite);
					image.sprite = sprite2;
					if (sprite == null)
					{
						leftRankBadge.enabled = false;
						rightRankBadge.enabled = false;
					}
					else
					{
						leftRankBadge.enabled = true;
						rightRankBadge.enabled = true;
					}
				}
				else
				{
					leftRankBadge.enabled = false;
					rightRankBadge.enabled = false;
				}
			}
		}

		public bool outlineText
		{
			set
			{
				if ((bool)titleOutline)
				{
					titleOutline.enabled = value;
				}
				if ((bool)timeStampOutline)
				{
					timeStampOutline.enabled = value;
				}
				messageOutline.enabled = value;
			}
		}

		public bool isMine
		{
			get
			{
				return m_isMine;
			}
			set
			{
				m_isMine = value;
			}
		}

		public void LoadPhoto(string p_player_id)
		{
			if (leftPhotoField == null || rightPhotoField == null)
			{
				return;
			}
			ProfileStateModel profile = base.app.model.storage.state.player.profile;
			if (isMine && profile.photo != null)
			{
				photo = profile.photo;
				if (!string.IsNullOrEmpty(p_player_id))
				{
					cache[p_player_id] = photo;
				}
				return;
			}
			if (!string.IsNullOrEmpty(p_player_id) && cache.ContainsKey(p_player_id))
			{
				photo = cache[p_player_id];
				return;
			}
			if (photo != null)
			{
				photo = null;
			}
			Action<Texture2D> on_texture_load = delegate(Texture2D p_result)
			{
				if ((bool)p_result)
				{
					Dictionary<string, Texture> dictionary = cache;
					string key = p_player_id;
					Texture value = (photo = p_result);
					dictionary[key] = value;
				}
			};
			if (p_player_id != null && p_player_id == "drl-sim-info-message")
			{
				Web.Get(DRLService.baseUri + "/images/avatar/drl-avatar.png", delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
				{
					if (!(p_progress < 1f))
					{
						on_texture_load(p_result);
					}
				});
			}
			else
			{
				base.app.model.service.GetPlayerAvatar(p_player_id, on_texture_load);
			}
		}

		public void ShowToggle(bool show)
		{
			if (!(title == "DRLSIM") && m_submenuToggleContainer != null)
			{
				m_submenuToggleContainer.SetActive(show);
			}
		}

		public void ShowHeader(bool show)
		{
			leftColorField.gameObject.SetActive(show);
			leftPhotoField.gameObject.SetActive(show);
			leftHeaderField.gameObject.SetActive(show);
			leftRankBadge.gameObject.SetActive(show);
			if (m_submenuToggleContainer != null)
			{
				m_submenuToggleContainer.SetActive(show && !isMine);
			}
		}

		public void ShowTime(bool show)
		{
			if (!(leftTimeStamp == null))
			{
				leftTimeStamp.gameObject.SetActive(show);
				rightTimneStamp.gameObject.SetActive(show);
			}
		}

		public void SetInfo()
		{
			if (!(infoContent == null))
			{
				messageContent.SetActive(value: false);
				infoBackground.SetActive(value: true);
				infoContent.SetActive(value: true);
				infoSpaceBottom.SetActive(value: true);
				infoSpaceTop.SetActive(value: true);
				submenuIcon.gameObject.SetActive(value: false);
				isInfo = true;
			}
		}

		public void ClearInfo()
		{
			if (!(infoContent == null))
			{
				messageContent.SetActive(value: true);
				infoContent.SetActive(value: false);
				infoBackground.SetActive(value: false);
				infoSpaceBottom.SetActive(value: false);
				infoSpaceTop.SetActive(value: false);
				submenuIcon.gameObject.SetActive(value: true);
				isInfo = false;
			}
		}

		public void SteamHelp()
		{
			WebBrowser.OpenURL("https://steamcommunity.com/app/641780/discussions", (base.app != null) ? base.app.model.service.platform : null);
		}

		public void ZendeskHelp()
		{
			WebBrowser.OpenURL("https://drlracingsimulator.zendesk.com/hc/en-us", (base.app != null) ? base.app.model.service.platform : null);
		}

		public void ShowPhoto(bool p_show)
		{
			if (!(leftPhotoContainer == null))
			{
				leftPhotoContainer.SetActive(p_show);
				rightPhotoContainer.SetActive(p_show);
			}
		}

		public void SetupInGameLayout()
		{
			title = "[" + title + "]: ";
			message = title + message;
			outlineText = true;
			indent.SetActive(value: true);
		}
	}
}

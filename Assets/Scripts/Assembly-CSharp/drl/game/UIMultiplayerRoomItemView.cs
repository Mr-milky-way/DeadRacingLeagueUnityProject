using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.network;
using thelab.core;

namespace drl.game
{
	public class UIMultiplayerRoomItemView : UICardView
	{
		private static Dictionary<string, Texture> m_cache;

		private INetworkPlayer m_data;

		public Image icon;

		public Image iconBot;

		public GameObject droneOverlay;

		public RawImage profilePhotoField;

		public RectTransform profilePhotoRT;

		public Material normalPlayerMaterial;

		public Material ghostPlayerMaterial;

		public Text profileNameField;

		public Image profileColorField;

		public Image profileBackgroundField;

		public Image positionBackground;

		public Text positionText;

		public Image masterBackground;

		public Image masterIcon;

		public Image readyBackground;

		public Image readyIcon;

		public FadeComponent readyOutline;

		public FadeComponent outline;

		public Image badgeIcon;

		public RectTransform footerRT;

		public Image coverBackground;

		public Image coverSwapBackground;

		public Image footerBackground;

		public FadeComponent cardFade;

		public float disabledAlpha = 0.4f;

		public UINavigation contextMenuSpectateRace;

		public UINavigation contextMenuSwap;

		public UINavigation contextMenuKick;

		public Color profileDefaultColor;

		public Color swapFontColor;

		public Color swapColor;

		public Color readyColor;

		public Color readyFontColor;

		private bool m_ready;

		private bool m_swap;

		private bool m_potentialSwap;

		private WebAsyncRequest m_droneThumbLoader;

		[HideInInspector]
		public string droneThumbURL;

		public bool isHorizontal;

		private bool isSpectator;

		private bool m_reportError = true;

		private Color m_profileColor;

		public RawImage _droneImage;

		private bool loadingAvatar;

		private float textSize;

		private RectScroller nicknameRectScroller;

		private RectTransform nicknameTextParentRect;

		public float footerHeightMax = 45f;

		private AsyncRequest m_photo_loader;

		private bool m_contextMenuEnabled = true;

		public override UICardType type => UICardType.MultiplayerRoomUserItem;

		internal static Dictionary<string, Texture> cache => Reflection<object>.Assert(ref m_cache);

		public new INetworkPlayer data
		{
			get
			{
				return m_data;
			}
			set
			{
				m_data = value;
				if (m_data != null)
				{
					coverBackground?.gameObject.SetActive(!m_data.IsSpectator);
				}
			}
		}

		public bool valid
		{
			get
			{
				if (data != null)
				{
					return data.ID >= 0;
				}
				return false;
			}
		}

		public bool isBot { get; private set; }

		public Texture profilePhoto
		{
			get
			{
				return profilePhoto;
			}
			set
			{
				try
				{
					profilePhotoField.texture = value;
					profilePhotoField.enabled = value != null;
				}
				catch (NullReferenceException)
				{
					if (m_reportError)
					{
						m_reportError = false;
						Debug.LogError("UIMultiplayerRoomItemView:: NullReferenceException in profilePhoto=value");
					}
				}
			}
		}

		public float profilePhotoX
		{
			get
			{
				return profilePhotoRT.anchoredPosition.x;
			}
			set
			{
				Vector2 anchoredPosition = profilePhotoRT.anchoredPosition;
				anchoredPosition.x = value;
				profilePhotoRT.anchoredPosition = anchoredPosition;
			}
		}

		public string profileName
		{
			get
			{
				return profileNameField.text;
			}
			set
			{
				profileNameField.text = value;
			}
		}

		public Color profileColor
		{
			get
			{
				return m_profileColor;
			}
			set
			{
				Color color = value * 0.35f;
				color.a = 1f;
				if (!(value == m_profileColor))
				{
					if ((bool)profileColorField)
					{
						profileColorField.color = value;
					}
					if ((bool)profileBackgroundField)
					{
						profileBackgroundField.color = value;
					}
					if ((bool)coverBackground)
					{
						coverBackground.GetComponent<Image>().color = value;
					}
					if ((bool)droneOverlay)
					{
						droneOverlay.GetComponent<Image>().color = color;
					}
					if ((bool)footerBackground)
					{
						footerBackground.color = color;
					}
					if ((bool)positionBackground)
					{
						positionBackground.color = value;
					}
					if ((bool)masterBackground)
					{
						masterBackground.color = value;
					}
					Color fontColorByProfileColor = DRLColor.GetFontColorByProfileColor(value);
					if ((bool)positionText)
					{
						positionText.color = fontColorByProfileColor;
					}
					if ((bool)masterIcon)
					{
						masterIcon.color = fontColorByProfileColor;
					}
					if ((bool)profileNameField)
					{
						profileNameField.color = Color.white;
					}
					m_profileColor = value;
				}
			}
		}

		public float footerHeight
		{
			get
			{
				return footerRT.sizeDelta.y;
			}
			set
			{
				Vector2 sizeDelta = footerRT.sizeDelta;
				sizeDelta.y = value;
				footerRT.sizeDelta = sizeDelta;
			}
		}

		public RawImage droneImage
		{
			get
			{
				if (isSpectator)
				{
					_droneImage = null;
					return null;
				}
				if (isBot)
				{
					droneImageAlpha = 0f;
					return null;
				}
				if (_droneImage == null)
				{
					_droneImage = GetComponentInChildren<RawImage>();
				}
				droneImageAlpha = ((_droneImage.texture != null) ? 1 : 0);
				if (loadingAvatar)
				{
					return _droneImage;
				}
				if (_droneImage != null && _droneImage.texture == null)
				{
					if (cache.ContainsKey(droneThumbURL) && cache[droneThumbURL] != null)
					{
						_droneImage.texture = cache[droneThumbURL];
						Tween.Add(this, "droneImageAlpha", 1f, 1f, 0f, Cubic.Out);
					}
					if (!cache.ContainsKey(droneThumbURL) || !(cache[droneThumbURL] != null))
					{
						if (string.IsNullOrEmpty(droneThumbURL))
						{
							droneThumbURL = base.app.model.storage.state.player.garage.currentRigData.thumb1;
						}
						if (!string.IsNullOrEmpty(droneThumbURL))
						{
							loadingAvatar = true;
							base.app.model.service.GetImage(droneThumbURL, 224, 0, delegate(Texture p_result)
							{
								Debug.LogWarning(droneThumbURL);
								if (!(p_result == null))
								{
									if (cache != null)
									{
										cache[droneThumbURL] = p_result;
									}
									_droneImage.texture = p_result;
									Tween.Add(this, "droneImageAlpha", 1f, 1f, 0f, Cubic.Out);
									loadingAvatar = false;
								}
							});
						}
					}
				}
				return _droneImage;
			}
			set
			{
				_droneImage = value;
			}
		}

		public float droneImageAlpha
		{
			get
			{
				if (!_droneImage)
				{
					return 0f;
				}
				CanvasGroup component = _droneImage.GetComponent<CanvasGroup>();
				if (!component)
				{
					return 0f;
				}
				return component.alpha;
			}
			set
			{
				if ((bool)_droneImage)
				{
					CanvasGroup component = _droneImage.GetComponent<CanvasGroup>();
					if ((bool)component)
					{
						component.alpha = value;
					}
				}
			}
		}

		public bool isMaster { get; set; }

		public static void ClearCache()
		{
			foreach (KeyValuePair<string, Texture> item in cache)
			{
				UnityEngine.Object.DestroyImmediate(item.Value, allowDestroyingAssets: true);
			}
			cache.Clear();
		}

		private void OnNicknameFieldUpdated()
		{
			if (nicknameRectScroller == null || nicknameTextParentRect == null)
			{
				RectTransform component = profileNameField.GetComponent<RectTransform>();
				nicknameTextParentRect = UnityEngine.Object.Instantiate(component.gameObject, component.parent).GetComponent<RectTransform>();
				textSize = (isHorizontal ? 183f : 251f);
				nicknameTextParentRect.anchorMin = component.anchorMin;
				nicknameTextParentRect.anchorMax = component.anchorMax;
				nicknameTextParentRect.pivot = component.pivot;
				nicknameTextParentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textSize);
				nicknameTextParentRect.offsetMin = component.offsetMin;
				nicknameTextParentRect.offsetMax = component.offsetMax;
				UnityEngine.Object.Destroy(nicknameTextParentRect.GetComponent<Text>());
				component.SetParent(nicknameTextParentRect);
				component.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
				component.gameObject.AddComponent<LayoutElement>().minWidth = textSize;
				nicknameRectScroller = component.gameObject.AddComponent<RectScroller>();
				component.anchorMin = Vector2.zero;
				component.anchorMax = Vector2.up;
				component.pivot = Vector2.up / 2f;
				component.anchoredPosition = Vector2.zero;
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(nicknameTextParentRect);
			Action callback = delegate
			{
				float num = nicknameRectScroller.Rect.sizeDelta.x - textSize;
				bool scrollActive = num > 0f;
				nicknameRectScroller.SetScrollState(scrollActive, Mathf.Abs(num), isHorizontal);
			};
			RunOnce(callback, 0.25f, unscaledTime: true);
		}

		public void Clear()
		{
			Tween.Kill(this);
			if ((bool)_droneImage && _droneImage.texture != null)
			{
				UnityEngine.Object.DestroyImmediate(_droneImage.texture, allowDestroyingAssets: true);
			}
			if ((bool)profilePhotoField && profilePhotoField.texture != null)
			{
				UnityEngine.Object.DestroyImmediate(profilePhotoField.texture, allowDestroyingAssets: true);
			}
			if ((bool)droneOverlay)
			{
				droneOverlay.GetComponent<CanvasGroup>().alpha = 0f;
			}
			SetMaster(p_master: false);
			SetReady(p_ready: false);
			SetGhost(p_isGhost: false, profileDefaultColor);
			SetForSwapping(p_yes: false);
			SetAsPotentialSwapSlot(p_yes: false);
			m_contextMenuEnabled = true;
			if (m_photo_loader != null)
			{
				m_photo_loader.Cancel();
			}
			if (isHorizontal)
			{
				ClearHorizontal();
			}
			else
			{
				ClearVertical();
			}
			if (badgeIcon != null)
			{
				badgeIcon.sprite = null;
			}
			if ((bool)positionBackground)
			{
				positionBackground.color = profileDefaultColor;
			}
			if ((bool)masterBackground)
			{
				masterBackground.color = profileDefaultColor;
			}
			if ((bool)positionText)
			{
				positionText.color = Color.white;
			}
			if ((bool)masterIcon)
			{
				masterIcon.color = Color.white;
			}
			m_profileColor = profileDefaultColor;
			CloseMenu();
		}

		public void Show(float p_delay)
		{
			if (isHorizontal)
			{
				ShowHorizontal(p_delay);
			}
			else
			{
				ShowVertical(p_delay);
			}
		}

		public void Set(string p_name, Color p_color, bool p_master, bool p_ready, object p_photo, bool p_animate, bool p_newThumbnails, Sprite p_badge = null, bool p_is_ghost = false, string p_platform = "undefined", bool p_isSpectator = false)
		{
			string text = p_name;
			text = text.Replace("(BOT)", "");
			text = ((text.Length > 12) ? (text.Substring(0, 12) + "...") : text);
			text = (p_is_ghost ? (text + " (BOT)") : text);
			text = text.ToUpper();
			if (!string.IsNullOrEmpty(text))
			{
				profileName = text;
			}
			isSpectator = p_isSpectator;
			p_newThumbnails = false;
			profileColor = p_color;
			SetMaster(p_master);
			SetReady(p_ready);
			SetGhost(p_is_ghost, p_color);
			coverBackground?.gameObject.SetActive(!p_isSpectator);
			Texture texture = ((p_photo is Texture texture2) ? texture2 : null);
			if ((bool)texture)
			{
				profilePhoto = texture;
			}
			if (badgeIcon != null)
			{
				if ((bool)p_badge)
				{
					badgeIcon.gameObject.SetActive(value: true);
					badgeIcon.sprite = p_badge;
				}
				else
				{
					badgeIcon.gameObject.SetActive(value: false);
				}
			}
			ServiceModel service = base.app.model.service;
			ImageLayout imageLayout = (droneImage ? _droneImage.gameObject.GetComponent<ImageLayout>() : null);
			if ((bool)imageLayout)
			{
				imageLayout.offset = (p_newThumbnails ? new Vector2(0f, 0f) : new Vector2(0f, -0.5f));
			}
			string rig_thumb_url = (string.IsNullOrEmpty(droneThumbURL) ? base.app.model.storage.state.player.garage.currentRigData.thumb1 : droneThumbURL);
			StartCoroutine(LoadTexture(rig_thumb_url));
			string photo_url = ((p_photo is string text2) ? text2 : "");
			if (string.IsNullOrEmpty(photo_url))
			{
				SetVisible(p_animate);
				return;
			}
			m_photo_loader = service.GetPlayerAvatar(photo_url, delegate(Texture2D p_result)
			{
				if (!(this == null) && !(p_result == null))
				{
					profilePhoto = p_result;
					if (!cache.ContainsKey(photo_url))
					{
						cache.Add(photo_url, p_result);
					}
					SetVisible(p_animate);
				}
			});
		}

		public bool IsCacheLoaded()
		{
			if (cache == null)
			{
				return true;
			}
			if (cache.Count == 0)
			{
				return true;
			}
			if (cache.ContainsKey(droneThumbURL))
			{
				return !(cache[droneThumbURL] == null);
			}
			return true;
		}

		public IEnumerator LoadTexture(string rig_thumb_url)
		{
			if (_droneImage == null)
			{
				yield break;
			}
			yield return new WaitUntil(IsCacheLoaded);
			if (cache.ContainsKey(rig_thumb_url))
			{
				_droneImage.texture = cache[rig_thumb_url];
				if (_droneImage.texture != null)
				{
					Tween.Add(this, "droneImageAlpha", 1f, 1f, 0f, Cubic.Out);
					Debug.Log("UIMultiplayerRoomItemView> LoadTexture> Cache hit: " + rig_thumb_url);
					yield break;
				}
			}
			if (!string.IsNullOrEmpty(rig_thumb_url))
			{
				base.app.model.service.GetImage(rig_thumb_url, 224, 0, delegate(Texture p_result)
				{
					if (!(p_result == null))
					{
						if (cache != null)
						{
							cache[rig_thumb_url] = p_result;
						}
						_droneImage.texture = p_result;
						Tween.Add(this, "droneImageAlpha", 1f, 1f, 0f, Cubic.Out);
					}
				});
			}
			if (_droneImage == null)
			{
				droneImageAlpha = 0f;
			}
			else
			{
				Tween.Add(this, "droneImageAlpha", 1f, 1f, 0f, Cubic.Out);
			}
		}

		protected override bool CanOpenMenu()
		{
			if (m_contextMenuEnabled && base.CanOpenMenu())
			{
				return IsTaken();
			}
			return false;
		}

		protected void SetVisible(bool p_animate)
		{
			if (p_animate)
			{
				Show(0f);
			}
			else if (isHorizontal)
			{
				profilePhotoX = 0f;
				if (icon.gameObject.activeInHierarchy)
				{
					icon.gameObject.SetActive(value: false);
				}
			}
			else
			{
				footerHeight = footerHeightMax;
			}
		}

		public bool IsTaken()
		{
			return !string.IsNullOrEmpty(profileName);
		}

		public bool IsAvailable()
		{
			return base.interactable;
		}

		public void SetContextMenuEnabled(bool p_enabled)
		{
			m_contextMenuEnabled = p_enabled;
		}

		public bool IsContextMenuEnabled()
		{
			return m_contextMenuEnabled;
		}

		public void SetAvailable(bool p_available)
		{
			if ((bool)cardFade)
			{
				base.interactable = p_available;
			}
		}

		public void SetMaster(bool p_master)
		{
			isMaster = p_master;
			if ((bool)masterBackground)
			{
				masterBackground.gameObject.SetActive(p_master);
			}
		}

		public void SetGhost(bool p_isGhost, Color profileColor)
		{
			if (ghostPlayerMaterial == null || iconBot == null || normalPlayerMaterial == null)
			{
				return;
			}
			if ((bool)coverBackground && !isMaster)
			{
				coverBackground.gameObject.SetActive(p_isGhost);
			}
			iconBot.gameObject.SetActive(p_isGhost);
			iconBot.color = profileColor;
			droneImageAlpha = 0f;
			if (p_isGhost)
			{
				droneOverlay.GetComponent<CanvasGroup>().alpha = 0.65f;
				if (_droneImage != null)
				{
					_droneImage.material = ghostPlayerMaterial;
					_droneImage.color = profileColor;
				}
			}
			else
			{
				droneOverlay.GetComponent<CanvasGroup>().alpha = 0f;
				if (_droneImage != null)
				{
					_droneImage.material = null;
					_droneImage.color = Color.white;
				}
			}
			isBot = p_isGhost;
		}

		public void SetReady(bool p_ready)
		{
			if (isHorizontal || p_ready == m_ready)
			{
				return;
			}
			m_ready = p_ready;
			if ((bool)readyBackground)
			{
				readyBackground.gameObject.SetActive(p_ready);
				if (p_ready)
				{
					outline.FadeOut(0.001f);
					readyOutline.FadeIn(0.001f);
				}
				else
				{
					outline.FadeIn(0.001f);
					readyOutline.FadeOut(0.001f);
				}
			}
			if ((bool)positionBackground)
			{
				positionBackground.color = (p_ready ? readyColor : profileColor);
			}
			if ((bool)positionText)
			{
				positionText.color = (p_ready ? readyFontColor : ((profileColor.a == 0f) ? Color.white : DRLColor.GetFontColorByProfileColor(profileColor)));
			}
			if ((bool)masterBackground)
			{
				masterBackground.color = (p_ready ? readyColor : profileColor);
			}
			if ((bool)masterIcon)
			{
				masterIcon.color = (p_ready ? readyFontColor : ((profileColor.a == 0f) ? Color.white : DRLColor.GetFontColorByProfileColor(profileColor)));
			}
		}

		public void SetForSwapping(bool p_yes)
		{
			SetAsPotentialSwapSlot(p_yes: false);
			if (m_swap != p_yes)
			{
				m_swap = p_yes;
				Transform transform = base.gameObject.transform.Find("outline-swap");
				Transform transform2 = base.gameObject.transform.Find("outline");
				if ((bool)transform)
				{
					transform.gameObject.SetActive(p_yes);
				}
				if ((bool)transform2)
				{
					transform2.gameObject.SetActive(!p_yes);
				}
				if ((bool)positionBackground)
				{
					positionBackground.color = (p_yes ? swapColor : ((profileColor.a == 0f) ? profileDefaultColor : profileColor));
				}
				if ((bool)positionText)
				{
					positionText.color = (p_yes ? swapFontColor : ((profileColor.a == 0f) ? Color.white : DRLColor.GetFontColorByProfileColor(profileColor)));
				}
				if ((bool)masterBackground)
				{
					masterBackground.color = (p_yes ? swapColor : ((profileColor.a == 0f) ? profileDefaultColor : profileColor));
				}
				if ((bool)readyBackground)
				{
					readyBackground.color = (p_yes ? swapColor : readyColor);
				}
				if ((bool)masterIcon)
				{
					masterIcon.color = (p_yes ? swapFontColor : ((profileColor.a == 0f) ? Color.white : DRLColor.GetFontColorByProfileColor(profileColor)));
				}
			}
		}

		public void SetAsPotentialSwapSlot(bool p_yes)
		{
			if (m_potentialSwap != p_yes)
			{
				m_potentialSwap = p_yes;
				if ((bool)coverSwapBackground)
				{
					coverSwapBackground.gameObject.SetActive(p_yes);
				}
				if ((bool)positionBackground)
				{
					positionBackground.color = (p_yes ? profileDefaultColor : ((profileColor.a == 0f) ? profileDefaultColor : profileColor));
				}
				if ((bool)positionText)
				{
					positionText.color = (p_yes ? swapFontColor : ((profileColor.a == 0f) ? Color.white : DRLColor.GetFontColorByProfileColor(profileColor)));
				}
				if ((bool)masterBackground)
				{
					masterBackground.color = (p_yes ? profileDefaultColor : ((profileColor.a == 0f) ? profileDefaultColor : profileColor));
				}
				if ((bool)readyBackground)
				{
					readyBackground.color = (p_yes ? profileDefaultColor : readyColor);
				}
				if ((bool)masterIcon)
				{
					masterIcon.color = (p_yes ? swapFontColor : ((profileColor.a == 0f) ? Color.white : DRLColor.GetFontColorByProfileColor(profileColor)));
				}
			}
		}

		public bool IsPotentialSwapSlot()
		{
			if ((bool)coverSwapBackground)
			{
				return coverSwapBackground.gameObject.activeInHierarchy;
			}
			return false;
		}

		public void RequestMasterReassignment()
		{
			Notify("network.player.master@click", this);
		}

		public void ClearHorizontal()
		{
			profilePhotoX = -60f;
			profileName = "";
		}

		public void ShowHorizontal(float p_delay)
		{
			Tween.Kill(this);
			if ((bool)icon)
			{
				icon.gameObject.SetActive(value: false);
			}
			Tween.Add(this, "profilePhotoX", 0f, 0.25f, 0f, Cubic.Out);
		}

		public void ClearVertical()
		{
			Tween.Kill(this);
			footerHeight = 0f;
			profileName = "";
		}

		public void ShowVertical(float p_delay)
		{
			Tween.Kill(this);
			if ((bool)icon)
			{
				icon.gameObject.SetActive(value: false);
			}
			Tween.Add(this, "footerHeight", footerHeightMax, 0.25f, 0f, Cubic.Out);
		}
	}
}

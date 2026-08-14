using System;
using System.IO;
using Rewired;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using drl.sim;
using drl.sim.rci;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UICommunityMapsItemView : UIElementView
	{
		public AnimateImageLayout mapPhotoAnimation;

		public RawImage mapPreviewPhotoField;

		public RawImage mapPhotoField;

		public GameObject privateIcon;

		public GameObject publicIcon;

		public Text mapField;

		public Text baseMapField;

		public RawImage creatorPhotoField;

		public FadeComponent creatorThumbFade;

		public Image creatorColorField;

		public Text creatorField;

		public RawImage leaderPhotoField;

		public FadeComponent leaderThumbFade;

		public Image leaderColorField;

		public Text leaderField;

		public GameObject leaderIcon;

		public LayoutElement leaderThumbContainer;

		public bool leaderThumbContainerAnimFromBtm;

		public float leaderThumbContainerStartRectTop = 200f;

		public FadeComponent leaderFade;

		public GameObject leaderPlaceholder;

		public GameObject leader;

		public float leaderFieldWidth = 320f;

		public FadeComponent[] starFades;

		public Text ratingsCount;

		public Text difficultyField;

		public UIElementView deleteButton;

		public UIElementView editButton;

		public UIElementView addButton;

		public UIElementView flyButton;

		public UIElementView cloneButton;

		public UIElementView lockButton;

		[Header("Navigation")]
		public UINavigation dataNav;

		public UINavigation editNav;

		public UINavigation delNav;

		public UINavigation addNav;

		public UINavigation flyNav;

		public UINavigation cloNav;

		public UINavigation dataProxyNav;

		public GameObject trashIcon;

		public GameObject applyDeleteIcon;

		public FadeComponent redBackground;

		[HideInInspector]
		public bool confirmDelete;

		[Header("Favorite")]
		[SerializeField]
		private RectTransform favoriteContainer;

		[SerializeField]
		private FadeComponent heartIconFade;

		[SerializeField]
		private FadeComponent favoriteHotkeyFade;

		[SerializeField]
		private DRLToggleView favoriteToggleView;

		[SerializeField]
		private DRLGamepadHotkey favoriteGamepadHotkey;

		[SerializeField]
		private UIElementView favoriteParentView;

		[SerializeField]
		private LayoutElement favoriteIconLayoutElement;

		[Space]
		public new DRLCommunityMapData data;

		[SerializeField]
		private FadeComponent m_mapPhotoFade;

		private AsyncRequest m_photoLoader;

		private AsyncRequest m_thumbnailLoader;

		private AsyncRequest m_leaderLoader;

		private Texture2D m_thumbFileTexture;

		public Texture mapPhoto
		{
			set
			{
				UIReflection.Set(mapPhotoField, value);
				if ((bool)mapPhotoField)
				{
					mapPhotoField.enabled = value;
				}
			}
		}

		public Texture mapPreviewPhoto
		{
			set
			{
				UIReflection.Set(mapPreviewPhotoField, value);
				if ((bool)mapPreviewPhotoField)
				{
					mapPreviewPhotoField.enabled = value;
				}
			}
		}

		public FadeComponent mapPhotoFade
		{
			get
			{
				if (!m_mapPhotoFade)
				{
					return m_mapPhotoFade = (mapPhotoField ? mapPhotoField.GetComponent<FadeComponent>() : null);
				}
				return m_mapPhotoFade;
			}
		}

		public string mapName
		{
			get
			{
				return mapField.text;
			}
			set
			{
				mapField.text = value;
			}
		}

		public string baseMapName
		{
			get
			{
				return baseMapField.text;
			}
			set
			{
				baseMapField.text = value;
			}
		}

		public Texture leaderThumb
		{
			set
			{
				leaderPhotoField.texture = value;
				leaderPhotoField.enabled = value != null;
				if (value != null)
				{
					leaderThumbFade.FadeIn();
				}
				else
				{
					leaderThumbFade.alpha = 0f;
				}
			}
		}

		public float leaderThumbContainerTop
		{
			get
			{
				return leaderThumbContainer.gameObject.GetComponent<RectTransform>().offsetMax.y;
			}
			set
			{
				RectTransform component = leaderThumbContainer.gameObject.GetComponent<RectTransform>();
				Vector2 offsetMax = component.offsetMax;
				offsetMax.y = value;
				component.offsetMax = offsetMax;
			}
		}

		public Texture creatorThumb
		{
			set
			{
				creatorPhotoField.texture = value;
				creatorPhotoField.enabled = value != null;
				if (value != null)
				{
					creatorThumbFade.FadeIn();
				}
				else
				{
					creatorThumbFade.alpha = 0f;
				}
			}
		}

		public Color creatorColor
		{
			set
			{
				creatorColorField.color = value;
			}
		}

		public Color leaderColor
		{
			set
			{
				leaderColorField.color = value;
			}
		}

		public string leaderName
		{
			get
			{
				return leaderField.text;
			}
			set
			{
				if (!(leaderPlaceholder == null) && !(leader == null) && !(leaderField == null) && !(leaderPhotoField == null) && !(leaderPhotoField.transform.parent == null) && !(leaderIcon == null))
				{
					leaderPlaceholder.SetActive(value: false);
					leader.SetActive(value: true);
					leaderField.text = value;
					leaderPhotoField.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(value));
					leaderIcon.SetActive(!string.IsNullOrEmpty(value));
				}
			}
		}

		public string creatorName
		{
			get
			{
				return creatorField.text;
			}
			set
			{
				creatorField.text = value;
			}
		}

		public string difficulty
		{
			get
			{
				return difficultyField.text;
			}
			set
			{
				difficultyField.text = value;
			}
		}

		private void Start()
		{
			SetFavoriteGamepadIconActive(HasXboxOrPSControllerConnected());
		}

		private void OnEnable()
		{
			favoriteParentView.OnEvent.AddListener(HandleFavoriteParentViewEvent);
			ReInput.ControllerConnectedEvent += OnConnectedEvent;
			ReInput.ControllerDisconnectedEvent += OnDisconnectedEvent;
		}

		private void OnDisable()
		{
			favoriteParentView.OnEvent.RemoveListener(HandleFavoriteParentViewEvent);
			ReInput.ControllerConnectedEvent -= OnConnectedEvent;
			ReInput.ControllerDisconnectedEvent -= OnDisconnectedEvent;
		}

		private void OnConnectedEvent(ControllerStatusChangedEventArgs p_args)
		{
			SetFavoriteGamepadIconActive(HasXboxOrPSControllerConnected());
		}

		private void OnDisconnectedEvent(ControllerStatusChangedEventArgs p_args)
		{
			if (!HasXboxOrPSControllerConnected())
			{
				SetFavoriteGamepadIconActive(p_active: false);
			}
		}

		private static bool HasXboxOrPSControllerConnected()
		{
			DefaultControllerType defaultControllerType = RCI.GetDefaultControllerType(DefaultControllerType.XBox);
			RCI.Controller activeJoystick = RCI.GetActiveJoystick();
			bool flag = defaultControllerType == DefaultControllerType.XBox && activeJoystick != null;
			bool flag2 = defaultControllerType == DefaultControllerType.PS && activeJoystick != null;
			return flag || flag2;
		}

		private void ClearLeader()
		{
			leaderPlaceholder.SetActive(value: true);
			leader.SetActive(value: false);
		}

		private void SetStarsRating(float p_rating, int p_count)
		{
			for (int i = 0; i < starFades.Length; i++)
			{
				starFades[i].alpha = 0.1f;
			}
			int num = Mathf.RoundToInt(p_rating * 5f);
			for (int j = 0; j < num; j++)
			{
				starFades[j].alpha = 1f;
			}
			if (num == 0)
			{
				p_count = 0;
			}
			if ((bool)ratingsCount)
			{
				ratingsCount.text = "(" + p_count + ")";
			}
		}

		private void SetButtons(bool p_add, bool p_edit, bool p_delete, bool p_in_game, bool p_disable)
		{
			flyButton.gameObject.SetActive(p_in_game);
			flyButton.interactable = !p_disable;
			addButton.gameObject.SetActive(!p_in_game && p_add);
			addButton.interactable = !p_disable;
			lockButton.gameObject.SetActive(!p_in_game && !p_add && !p_edit);
			lockButton.interactable = false;
			editButton.gameObject.SetActive(!p_in_game && p_edit);
			editButton.interactable = !p_disable;
			deleteButton.gameObject.SetActive(!p_in_game && p_delete);
			deleteButton.interactable = !p_disable;
			cloneButton.gameObject.SetActive(!p_in_game && !p_add && p_edit);
			cloneButton.interactable = !p_disable;
		}

		public void Set(DRLCommunityMapData p_data, bool p_my_map, bool p_in_game, bool p_disable_privates, string p_player_id, DRLMap p_map, Localization p_locale, ServiceModel p_service_model, StorageModel p_storage_model, string p_override_profile_img_url = null)
		{
			if (!base.validContext)
			{
				return;
			}
			data = p_data;
			if (data == null)
			{
				return;
			}
			ShowDeleteButton(0.001f);
			if (m_photoLoader != null)
			{
				m_photoLoader.Cancel();
			}
			if (m_thumbnailLoader != null)
			{
				m_thumbnailLoader.Cancel();
			}
			if (m_leaderLoader != null)
			{
				m_leaderLoader.Cancel();
			}
			string text = (string.IsNullOrEmpty(data.mapTitle) ? "" : data.mapTitle);
			text = text.ToUpper();
			mapName = text;
			if (p_map != null)
			{
				string text2 = (string.IsNullOrEmpty(p_map.label) ? "" : p_map.label);
				text2 = text2.ToUpper();
				baseMapName = text2;
			}
			string text3 = (string.IsNullOrEmpty(data.profileName) ? "" : data.profileName);
			text3 = text3.ToUpper();
			creatorName = text3;
			Action p_callback = delegate
			{
				if (base.validContext)
				{
					UITruncateText component = mapField.GetComponent<UITruncateText>();
					if ((bool)component)
					{
						component.Refresh();
					}
					UITruncateText component2 = creatorField.GetComponent<UITruncateText>();
					if ((bool)component2)
					{
						component2.Refresh();
					}
					UITruncateText component3 = baseMapField.GetComponent<UITruncateText>();
					if ((bool)component3)
					{
						component3.Refresh();
					}
				}
			};
			RunOnce(0.1f, p_callback);
			string text4 = "";
			if (p_locale != null)
			{
				switch (data.mapDifficulty)
				{
				case 0:
					text4 = p_locale.Get("map.map-track-cards.difficulty.basic", "BASIC");
					break;
				case 1:
					text4 = p_locale.Get("map.map-track-cards.difficulty.easy", "EASY");
					break;
				case 2:
					text4 = p_locale.Get("map.map-track-cards.difficulty.medium", "MEDIUM");
					break;
				case 3:
					text4 = p_locale.Get("map.map-track-cards.difficulty.hard", "HARD");
					break;
				}
			}
			else
			{
				switch (data.mapDifficulty)
				{
				case 0:
					text4 = "BASIC";
					break;
				case 1:
					text4 = "EASY";
					break;
				case 2:
					text4 = "MEDIUM";
					break;
				case 3:
					text4 = "HARD";
					break;
				}
			}
			difficulty = text4.ToUpper();
			SetStarsRating(data.score, data.ratingCount);
			creatorThumb = null;
			string p_url = data.profileThumbURL;
			if (p_my_map && p_override_profile_img_url != null)
			{
				p_url = p_override_profile_img_url;
			}
			m_photoLoader = Web.Load(p_url, "GET", delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
			{
				if (base.validContext && !(this == null) && !(base.gameObject == null) && !(creatorPhotoField == null) && !(p_result == null) && !(p_progress < 1f))
				{
					creatorThumb = p_result;
				}
			});
			creatorColor = data.profileColor;
			mapPhotoFade.FadeOut(0.01f);
			if (p_map != null)
			{
				mapPreviewPhoto = p_map.preview;
			}
			if (mapPhotoAnimation != null)
			{
				mapPhotoAnimation.ResetAnimation();
				mapPhotoAnimation.animationType = AnimateImageLayout.AnimationType.None;
			}
			string mapThumbURL = data.mapThumbURL;
			bool flag = !string.IsNullOrEmpty(mapThumbURL) && mapThumbURL.IndexOf("$") == 0;
			byte[] array = null;
			if (flag)
			{
				string path = mapThumbURL.Substring(1);
				array = (File.Exists(path) ? File.ReadAllBytes(path) : null);
			}
			mapThumbURL = data.GetThumbURL(DRLCommunityMapData.ThumbSize.Small);
			if (!(flag ? (array != null) : (!string.IsNullOrEmpty(mapThumbURL))))
			{
				if (p_map != null)
				{
					mapPhoto = p_map.preview;
					mapPhotoFade.FadeIn();
				}
			}
			else if (flag)
			{
				if ((bool)m_thumbFileTexture)
				{
					UnityEngine.Object.Destroy(m_thumbFileTexture);
				}
				m_thumbFileTexture = new Texture2D(1, 1);
				m_thumbFileTexture.LoadImage(array, markNonReadable: false);
				mapPhoto = m_thumbFileTexture;
				mapPhotoFade.FadeIn();
			}
			else if (p_service_model != null)
			{
				m_thumbnailLoader = Web.Get(mapThumbURL, delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
				{
					if (!(p_progress < 1f) && this != null && p_result != null)
					{
						mapPhoto = p_result;
						mapPhotoFade.FadeIn();
					}
				});
			}
			if (p_service_model != null && p_storage_model != null)
			{
				UpdateLeader(p_service_model, p_storage_model, p_data);
			}
			string.IsNullOrEmpty(data.root);
			bool flag2 = p_data.IsOwner(p_player_id);
			bool allowCopy = p_data.allowCopy;
			bool p_add = !flag2 && allowCopy;
			SetButtons(p_add, flag2, flag2, p_in_game, p_disable_privates && !data.isPublic);
			publicIcon.SetActive(data.isPublic);
			privateIcon.SetActive(!data.isPublic);
			if (favoriteGamepadHotkey != null)
			{
				favoriteGamepadHotkey.enabled = false;
			}
		}

		private void UpdateLeader(ServiceModel p_service_model, StorageModel p_storage_model, DRLCommunityMapData p_map)
		{
			ClearLeader();
			leaderFade.alpha = 0f;
			MapData mapData = new MapData();
			mapData.Merge(p_map);
			DroneRigData currentRigData = p_storage_model.state.player.garage.currentRigData;
			bool p_official = p_storage_model.state.player.garage.IsOfficial(currentRigData);
			bool p_customPhysics = currentRigData != null && !string.IsNullOrEmpty(currentRigData.tune);
			m_leaderLoader = p_service_model.GetLeaderboardRivals("", mapData, 1, currentRigData.diameter, p_official, p_customPhysics, delegate(DRLLeaderboardRivalsResult p_result)
			{
				if (!(this == null))
				{
					if (p_result == null)
					{
						Debug.LogWarning("UICommunityMapsItemView> GetLeaderboardRivals - Failed!");
					}
					else if (p_result.top.Length != 0)
					{
						DRLLeaderboardData leaderData = p_result.top[0];
						SetLeaderData(leaderData);
					}
				}
			});
		}

		private void SetLeaderData(DRLLeaderboardData p_data)
		{
			if (!base.validContext)
			{
				return;
			}
			leaderName = p_data.profileName.ToUpper();
			leaderColor = p_data.profileColor;
			float num = 0f;
			leaderFade.alpha = 0f;
			num += 0.2f;
			leaderThumbContainer.preferredWidth = 0f;
			leaderField.color = Colorf.transparent;
			Tween.Kill(leaderThumbContainer);
			Tween.Kill(leaderField);
			Tween.Kill(this);
			leaderFade.FadeIn(0.2f, num);
			num += 0.15f;
			Tween.Add(leaderField, "color", Color.white, 0.3f, num, Cubic.Out);
			num += 0.1f;
			if (leaderThumbContainerAnimFromBtm)
			{
				leaderThumbContainerTop = 0f - leaderThumbContainerStartRectTop;
				Tween.Add(this, "leaderThumbContainerTop", 0f, 0.3f, num, Cubic.Out);
			}
			else
			{
				Tween.Add(leaderThumbContainer, "preferredWidth", 55f, 0.3f, num, Cubic.Out);
			}
			m_photoLoader = Web.Load(p_data.profileThumbURL, "GET", delegate(Texture2D p_res, float p_progress, WebAsyncRequest p_req)
			{
				if (!(this == null) && !(base.gameObject == null) && !(leaderPhotoField == null) && !(p_progress < 1f))
				{
					leaderThumb = p_res;
				}
			});
		}

		public void ShowDeleteButton(float p_time = 0.4f)
		{
			if (confirmDelete)
			{
				confirmDelete = false;
				trashIcon.SetActive(value: true);
				applyDeleteIcon.SetActive(value: false);
				redBackground.Fade(0f, p_time);
			}
		}

		public void ShowConfirmDelete()
		{
			if (!confirmDelete)
			{
				confirmDelete = true;
				trashIcon.SetActive(value: false);
				applyDeleteIcon.SetActive(value: true);
				redBackground.FadeIn();
			}
		}

		private void HandleFavoriteOnFocus()
		{
			SetFavoriteFocus(p_focus: true);
		}

		private void HandleFavoriteOnUnfocus()
		{
			SetFavoriteFocus(p_focus: false);
		}

		public void SetFavoriteToggleOn(bool p_on)
		{
			favoriteToggleView.isOn = p_on;
		}

		public void SetFavoriteFocus(bool p_focus)
		{
			if (!favoriteContainer.gameObject.activeSelf)
			{
				return;
			}
			favoriteGamepadHotkey.enabled = p_focus;
			if (p_focus)
			{
				favoriteHotkeyFade.FadeIn();
				heartIconFade.FadeIn();
				return;
			}
			favoriteHotkeyFade.Fade(0.2f);
			if (favoriteToggleView.isOn)
			{
				heartIconFade.FadeIn();
			}
			else
			{
				heartIconFade.Fade(0.2f);
			}
		}

		public void SetFavoriteActive(bool p_active)
		{
			favoriteContainer.gameObject.SetActive(p_active);
			SetFavoriteFocus(p_focus: false);
		}

		private void SetFavoriteGamepadIconActive(bool p_active)
		{
			favoriteIconLayoutElement.ignoreLayout = !p_active;
			favoriteGamepadHotkey.buttonIcon.enabled = p_active;
		}

		private void HandleFavoriteParentViewEvent(NotificationEvent p_event)
		{
			if (p_event.notification.Contains("@focus"))
			{
				HandleFavoriteOnFocus();
			}
			else if (p_event.notification.Contains("@unfocus"))
			{
				HandleFavoriteOnUnfocus();
			}
		}
	}
}

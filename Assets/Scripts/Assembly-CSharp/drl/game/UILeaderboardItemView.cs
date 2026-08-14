using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using drl.sim;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UILeaderboardItemView : UIElementView<DRLApp>
	{
		public Text positionField;

		public bool profilePhotoContainerAnimFromBtm;

		public float profilePhotoContainerStartRectTop = 200f;

		public LayoutElement profilePhotoContainer;

		public RawImage profilePhotoField;

		public Image profileColorField;

		public Text profileNameField;

		public Text timeField;

		public Image timeIcon;

		public Image personIcon;

		[Header("platform icons")]
		public GameObject platformIconsStandaloneContainer;

		public GameObject platformIconsXboxContainer;

		public GameObject platformIconsPSContainer;

		public List<PlatformTypeTag> platformTagsStandalone;

		public List<PlatformTypeTag> platformTagsXbox;

		public List<PlatformTypeTag> platformTagsPS;

		[Header("controller icons")]
		public GameObject controllerIconsStandaloneContainer;

		public GameObject controllerIconsXboxContainer;

		public GameObject controllerIconsPSContainer;

		public List<ControllerTypeTag> controllerTagsStandalone;

		public List<ControllerTypeTag> controllerTagsXbox;

		public List<ControllerTypeTag> controllerTagsPS;

		[Space]
		public GameObject replayButton;

		public GameObject saveDroneButton;

		public UINavigation replayNav;

		public UINavigation entryNav;

		public UINavigation saveNav;

		public Image selectionOutline;

		public RawImage flagImage;

		public GameObject flagContainer;

		public LayoutElement droneThumbContainer;

		public RawImage droneThumbField;

		public Text droneNameField;

		public Image droneClassBackground;

		public Text droneClassField;

		public float droneThumbWidth = 135f;

		public new DRLLeaderboardData data;

		public string replayURL;

		public string droneRigData;

		private float m_time;

		private float m_points;

		public Texture2D defaultFlag;

		private WebAsyncRequest m_photo_loader;

		private WebAsyncRequest m_drone_thumb_loader;

		private WebAsyncRequest m_flag_loader;

		private bool m_customPhysics;

		public bool hasReplay => !string.IsNullOrEmpty(replayURL);

		public FadeComponent fade => AssertLocal<FadeComponent>("fade");

		public int position
		{
			set
			{
				positionField.text = value.ToString() ?? "";
			}
		}

		public bool positionEnabled
		{
			set
			{
				positionField.gameObject.SetActive(value);
			}
		}

		public Texture profilePhoto
		{
			set
			{
				profilePhotoField.texture = value;
				profilePhotoField.color = ((value != null) ? Color.white : Colorf.transparent);
			}
		}

		public float profilePhotoContainerTop
		{
			get
			{
				return profilePhotoContainer.gameObject.GetComponent<RectTransform>().offsetMax.y;
			}
			set
			{
				RectTransform component = profilePhotoContainer.gameObject.GetComponent<RectTransform>();
				Vector2 offsetMax = component.offsetMax;
				offsetMax.y = value;
				component.offsetMax = offsetMax;
			}
		}

		public bool profilePhotoEnabled
		{
			set
			{
				profilePhotoField.transform.parent.gameObject.SetActive(value);
			}
		}

		public string profileName
		{
			set
			{
				profileNameField.text = value;
				UITruncateText component = profileNameField.GetComponent<UITruncateText>();
				if (component != null)
				{
					component.Refresh();
				}
			}
		}

		public Color profileColor
		{
			set
			{
				profileColorField.color = value;
			}
		}

		public bool profileColorEnabled
		{
			set
			{
				profileColorField.gameObject.SetActive(value);
			}
		}

		public Texture droneThumb
		{
			set
			{
				if ((bool)droneThumbField)
				{
					droneThumbField.texture = value;
				}
				if ((bool)droneThumbField)
				{
					droneThumbField.color = ((value != null) ? Color.white : Colorf.transparent);
				}
			}
		}

		public string droneName
		{
			set
			{
				droneNameField.text = value;
			}
		}

		public int droneClass
		{
			set
			{
				droneClassField.text = value + "\"";
				droneClassBackground.color = DRLColor.classColors[value];
			}
		}

		public bool droneClassEnabled
		{
			set
			{
				if ((bool)droneThumbContainer)
				{
					droneThumbContainer.gameObject.SetActive(value);
				}
				if ((bool)droneNameField)
				{
					droneNameField.gameObject.SetActive(value);
				}
			}
		}

		public Texture flagPhoto
		{
			set
			{
				flagImage.texture = value;
				flagImage.color = ((value != null) ? Color.white : Colorf.transparent);
			}
		}

		public bool flagPhotoEnabled
		{
			set
			{
				flagImage.transform.parent.gameObject.SetActive(value);
			}
		}

		public float time
		{
			get
			{
				return m_time;
			}
			set
			{
				m_time = value;
				timeField.text = Format.SecondsToMMSSFFF(value);
			}
		}

		public float points
		{
			get
			{
				return m_points;
			}
			set
			{
				m_points = value;
				timeField.text = m_points.ToString();
			}
		}

		public bool selected
		{
			get
			{
				return selectionOutline.enabled;
			}
			set
			{
				selectionOutline.enabled = value;
			}
		}

		public void SetController(ControllerStateType p_state)
		{
			if (!(controllerIconsStandaloneContainer == null) && !(controllerIconsXboxContainer == null) && !(controllerIconsPSContainer == null))
			{
				controllerIconsXboxContainer.SetActive(value: false);
				controllerIconsStandaloneContainer.SetActive(value: true);
				controllerIconsPSContainer.SetActive(value: false);
				for (int i = 0; i < controllerTagsStandalone.Count; i++)
				{
					ControllerTypeTag controllerTypeTag = controllerTagsStandalone[i];
					controllerTypeTag.gameObject.SetActive(controllerTypeTag.Contains(p_state));
				}
			}
		}

		public void SetPlatform(PlatformStateType p_state)
		{
			if (!(platformIconsStandaloneContainer == null) && !(platformIconsXboxContainer == null) && !(platformIconsPSContainer == null))
			{
				platformIconsXboxContainer.SetActive(value: false);
				platformIconsStandaloneContainer.SetActive(value: true);
				platformIconsPSContainer.SetActive(value: false);
				for (int i = 0; i < platformTagsStandalone.Count; i++)
				{
					PlatformTypeTag platformTypeTag = platformTagsStandalone[i];
					platformTypeTag.gameObject.SetActive(platformTypeTag.Contains(p_state));
				}
			}
		}

		public void SetCampaignRaceMode(bool p_flag)
		{
			flagPhotoEnabled = !p_flag;
			positionEnabled = !p_flag;
			profilePhotoEnabled = !p_flag;
			profileColorEnabled = !p_flag;
		}

		public void SetCampaignRaceTitle(string p_map, string p_track, string p_title)
		{
			string text = "<color=#f00> / </color>";
			profileName = p_map.ToUpper() + text + p_track.ToUpper() + text + p_title.ToUpper();
		}

		public void SetTimeVisible()
		{
			if ((bool)timeIcon)
			{
				timeIcon.gameObject.SetActive(value: true);
			}
			if ((bool)personIcon)
			{
				personIcon.gameObject.SetActive(value: false);
			}
		}

		public void SetPointsVisible()
		{
			if ((bool)timeIcon)
			{
				timeIcon.gameObject.SetActive(value: false);
			}
			if ((bool)personIcon)
			{
				personIcon.gameObject.SetActive(value: true);
			}
		}

		private float InitFieldsAnimation(float p_delay)
		{
			float num = p_delay;
			num += 0.2f;
			positionField.color = Colorf.transparent;
			profilePhotoContainer.preferredWidth = 0f;
			profileNameField.color = Colorf.transparent;
			Tween.Kill(positionField);
			Tween.Kill(profilePhotoContainer);
			Tween.Kill(profileNameField);
			Tween.Kill(this);
			time = 0f;
			SetController(ControllerStateType.XBox);
			SetPlatform(PlatformStateType.Steam);
			fade.FadeIn(0.2f, num);
			num += 0.15f;
			entryNav.GetComponent<UIElementView>().interactable = true;
			Tween.Add(positionField, "color", Color.white, 0.3f, num, Cubic.Out);
			Tween.Add(profileNameField, "color", Color.white, 0.3f, num, Cubic.Out);
			num += 0.1f;
			if (profilePhotoContainerAnimFromBtm)
			{
				profilePhotoContainerTop = 0f - profilePhotoContainerStartRectTop;
				Tween.Add(this, "profilePhotoContainerTop", 0f, 0.3f, num, Cubic.Out);
			}
			else
			{
				Tween.Add(profilePhotoContainer, "preferredWidth", 56f, 0.3f, num, Cubic.Out);
			}
			return num;
		}

		public void Set(DRLLeaderboardData p_data, bool p_allow_replay, bool p_allow_save, float p_delay = 0f)
		{
			ServiceModel service = base.app.model.service;
			fade.alpha = 0f;
			data = p_data;
			if (m_photo_loader != null)
			{
				m_photo_loader.Cancel();
			}
			if (m_flag_loader != null)
			{
				m_flag_loader.Cancel();
			}
			if (m_drone_thumb_loader != null)
			{
				m_drone_thumb_loader.Cancel();
			}
			if (p_data == null)
			{
				return;
			}
			float dl = InitFieldsAnimation(p_delay);
			bool flag = p_data.ContainsKey("diameter") && p_data.ContainsKey("drone-name") && (bool)droneThumbContainer;
			if ((bool)droneThumbContainer)
			{
				droneThumbContainer.preferredWidth = 1f;
			}
			if ((bool)droneNameField)
			{
				droneNameField.color = Colorf.transparent;
			}
			droneClassEnabled = flag && p_data.diameter >= 0;
			droneThumb = null;
			SetTimeVisible();
			Tween.Add(this, "time", p_data.scoreSeconds, 0.3f, dl, Cubic.Out);
			if ((bool)timeIcon)
			{
				timeIcon.color = Color.white;
			}
			if (replayButton != null)
			{
				replayButton.SetActive(p_allow_replay);
				replayURL = data.replayURL;
				FadeComponent component = replayButton.GetComponent<FadeComponent>();
				if ((bool)component)
				{
					component.alpha = (hasReplay ? 1f : 0.25f);
				}
			}
			if (saveDroneButton != null)
			{
				if (p_allow_save)
				{
					saveDroneButton.SetActive(value: true);
					this.droneRigData = data.droneRig;
					UIElementView component2 = saveDroneButton.GetComponent<UIElementView>();
					if ((bool)component2)
					{
						if (string.IsNullOrEmpty(this.droneRigData))
						{
							component2.interactable = false;
						}
						else
						{
							DroneRigData droneRigData = DroneRigData.FromJson(this.droneRigData);
							component2.interactable = !droneRigData.isLocked && (droneRigData.isPublic || base.app.model.storage.state.player.garage.IsOriginal(droneRigData));
						}
					}
				}
				else
				{
					saveDroneButton.SetActive(value: false);
				}
			}
			else
			{
				this.droneRigData = null;
			}
			position = p_data.position;
			profileName = (string.IsNullOrEmpty(p_data.profileName) ? "UNDEFINED" : p_data.profileName.ToUpper());
			profileColor = p_data.profileColor;
			profilePhoto = null;
			m_photo_loader = Web.Load(p_data.profileThumbURL, "GET", delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
			{
				if (!(p_progress < 1f) && !(this == null))
				{
					profilePhoto = p_result;
				}
			});
			if ((bool)flagImage)
			{
				flagImage.texture = defaultFlag;
				flagContainer.SetActive(defaultFlag != null);
				if (!string.IsNullOrEmpty(p_data.flagThumbURL))
				{
					Web.Get(p_data.flagThumbURL, delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
					{
						if (!(p_progress < 1f) && p_req.code == 200 && !(this == null))
						{
							if ((bool)p_result)
							{
								flagImage.texture = p_result;
							}
							flagImage.color = Color.white;
						}
					});
				}
			}
			if (flag)
			{
				m_drone_thumb_loader = service.GetImage(p_data.droneThumb, 92, 0, delegate(Texture2D p_result)
				{
					if (!(this == null))
					{
						droneThumb = p_result;
						droneClassEnabled = p_data.diameter >= 0;
						droneClass = p_data.diameter;
						droneName = p_data.droneName.ToUpper() + (m_customPhysics ? " *" : "");
						Tween.Add(droneNameField, "color", m_customPhysics ? Color.yellow : Color.white, 0.3f, dl, Cubic.Out);
						Tween.Add(droneThumbContainer, "preferredWidth", droneThumbWidth, 0.3f, dl, Cubic.Out);
					}
				});
			}
			ControllerStateType controller = Reflection<object>.GetEnum<ControllerStateType>(p_data.controllerType);
			PlatformStateType platform = Reflection<object>.GetEnum<PlatformStateType>(p_data.platform);
			SetController(controller);
			SetPlatform(platform);
		}

		public void Set(DRLCircuitLeaderboardData p_data, bool p_allow_replay, bool p_allow_save, float p_delay = 0f, int p_position = 0)
		{
			ServiceModel service = base.app.model.service;
			if (m_photo_loader != null)
			{
				m_photo_loader.Cancel();
			}
			if (m_flag_loader != null)
			{
				m_flag_loader.Cancel();
			}
			if (m_drone_thumb_loader != null)
			{
				m_drone_thumb_loader.Cancel();
			}
			if (p_data == null)
			{
				return;
			}
			fade.alpha = 0f;
			data = new DRLLeaderboardData();
			data.playerId = p_data.playerId;
			data.replayURL = null;
			data.droneRig = p_data.droneRig;
			float dl = InitFieldsAnimation(p_delay);
			bool flag = p_data.ContainsKey("diameter") && p_data.ContainsKey("drone-name") && (bool)droneThumbContainer;
			if ((bool)droneThumbContainer)
			{
				droneThumbContainer.preferredWidth = 1f;
			}
			if ((bool)droneNameField)
			{
				droneNameField.color = Colorf.transparent;
			}
			droneClassEnabled = flag && p_data.diameter >= 0;
			droneThumb = null;
			SetTimeVisible();
			Tween.Add(this, "time", p_data.scoreSeconds, 0.3f, dl, Cubic.Out);
			if ((bool)timeIcon)
			{
				timeIcon.color = Color.white;
			}
			if (replayButton != null)
			{
				replayButton.SetActive(p_allow_replay);
				replayURL = data.replayURL;
				FadeComponent component = replayButton.GetComponent<FadeComponent>();
				if ((bool)component)
				{
					component.alpha = (hasReplay ? 1f : 0.25f);
				}
			}
			if (saveDroneButton != null)
			{
				if (p_allow_save)
				{
					saveDroneButton.SetActive(value: true);
					this.droneRigData = data.droneRig;
					UIElementView component2 = saveDroneButton.GetComponent<UIElementView>();
					if ((bool)component2)
					{
						if (string.IsNullOrEmpty(this.droneRigData))
						{
							component2.interactable = false;
						}
						else
						{
							DroneRigData droneRigData = DroneRigData.FromJson(this.droneRigData);
							component2.interactable = !droneRigData.isLocked && (droneRigData.isPublic || base.app.model.storage.state.player.garage.IsOriginal(droneRigData));
						}
					}
				}
				else
				{
					saveDroneButton.SetActive(value: false);
				}
			}
			else
			{
				this.droneRigData = null;
			}
			position = p_position;
			profileName = (string.IsNullOrEmpty(p_data.profileName) ? "UNDEFINED" : p_data.profileName.ToUpper());
			profileColor = p_data.profileColor;
			profilePhoto = null;
			m_photo_loader = Web.Load(p_data.profileThumbURL, "GET", delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
			{
				if (!(p_progress < 1f) && !(this == null))
				{
					profilePhoto = p_result;
				}
			});
			if ((bool)flagImage)
			{
				flagImage.texture = defaultFlag;
				flagContainer.SetActive(defaultFlag != null);
				if (!string.IsNullOrEmpty(p_data.flagThumbURL))
				{
					Web.Get(p_data.flagThumbURL, delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
					{
						if (!(p_progress < 1f) && p_req.code == 200 && !(this == null))
						{
							if ((bool)p_result)
							{
								flagImage.texture = p_result;
							}
							flagImage.color = Color.white;
						}
					});
				}
			}
			if (flag)
			{
				m_drone_thumb_loader = service.GetImage(p_data.droneThumb, 92, 0, delegate(Texture2D p_result)
				{
					if (!(this == null))
					{
						droneThumb = p_result;
						droneClassEnabled = p_data.diameter >= 0;
						droneClass = p_data.diameter;
						droneName = p_data.droneName.ToUpper() + (m_customPhysics ? " *" : "");
						Tween.Add(droneNameField, "color", m_customPhysics ? Color.yellow : Color.white, 0.3f, dl, Cubic.Out);
						Tween.Add(droneThumbContainer, "preferredWidth", droneThumbWidth, 0.3f, dl, Cubic.Out);
					}
				});
			}
			ControllerStateType controller = Reflection<object>.GetEnum<ControllerStateType>(p_data.controllerType);
			PlatformStateType platform = Reflection<object>.GetEnum<PlatformStateType>(p_data.platform);
			SetController(controller);
			SetPlatform(platform);
		}

		public void Set(DRLLeaderboardData p_data, float p_delay = 0f)
		{
			Set(p_data, p_allow_replay: false, p_allow_save: false, p_delay);
		}

		public void Set(DRLTournamentPlayerData p_data, float p_delay = 0f)
		{
			fade.alpha = 0f;
			if (m_photo_loader != null)
			{
				m_photo_loader.Cancel();
			}
			if (m_flag_loader != null)
			{
				m_flag_loader.Cancel();
			}
			if (m_drone_thumb_loader != null)
			{
				m_drone_thumb_loader.Cancel();
			}
			if (p_data == null)
			{
				return;
			}
			float p_delay2 = InitFieldsAnimation(p_delay);
			droneClassEnabled = false;
			droneThumb = null;
			Tween.Add((object)this, "points", (float)p_data.points, 0.3f, p_delay2, (Easing)Cubic.Out);
			SetPointsVisible();
			droneRigData = null;
			position = 0;
			profileName = (string.IsNullOrEmpty(p_data.profileName) ? "UNDEFINED" : p_data.profileName.ToUpper());
			profileColor = p_data.profileColor;
			profilePhoto = null;
			m_photo_loader = Web.Load(p_data.profileThumbURL, "GET", delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
			{
				if (!(p_progress < 1f) && !(this == null))
				{
					profilePhoto = p_result;
				}
			});
		}

		public void Clear(float p_delay = 0f, bool fadeOut = true)
		{
			float num = p_delay;
			if (!fadeOut)
			{
				fade.alpha = 1f;
			}
			else
			{
				fade.alpha = 0f;
			}
			num += 0.2f;
			if (m_photo_loader != null)
			{
				m_photo_loader.Cancel();
			}
			if (m_flag_loader != null)
			{
				m_flag_loader.Cancel();
			}
			if (m_drone_thumb_loader != null)
			{
				m_drone_thumb_loader.Cancel();
			}
			if (fadeOut)
			{
				positionField.color = Colorf.transparent;
				profileNameField.color = Colorf.transparent;
			}
			Tween.Kill(positionField);
			Tween.Kill(profilePhotoContainer);
			Tween.Kill(profileNameField);
			Tween.Kill(this);
			timeField.text = "          ";
			if ((bool)timeIcon)
			{
				timeIcon.color = Colorf.transparent;
			}
			SetController(ControllerStateType.XBox);
			SetPlatform(PlatformStateType.Steam);
			if (fadeOut)
			{
				fade.Fade(0.1f, 0.2f, num);
			}
			num += 0.15f;
			entryNav.GetComponent<UIElementView>().interactable = false;
			if ((bool)droneThumbContainer)
			{
				droneThumbContainer.preferredWidth = 1f;
			}
			if ((bool)droneNameField)
			{
				droneNameField.color = Colorf.transparent;
			}
			droneClassEnabled = false;
			droneThumb = null;
			time = 0f;
			if (replayButton != null)
			{
				replayButton.SetActive(value: false);
				FadeComponent component = replayButton.GetComponent<FadeComponent>();
				if ((bool)component)
				{
					component.alpha = 0.25f;
				}
			}
			if (saveDroneButton != null)
			{
				saveDroneButton.SetActive(value: false);
			}
			droneRigData = null;
			positionField.text = " ";
			profileName = " ";
			profileColor = Colorf.transparent;
			profilePhoto = null;
			selected = false;
			if ((bool)flagImage)
			{
				flagImage.color = Colorf.transparent;
			}
		}

		public void FlagCustomPhysics(bool p_flag)
		{
			m_customPhysics = p_flag;
			Tween.Add(timeField, "color", p_flag ? Color.yellow : Color.white, 0.1f, 0f, Cubic.Out);
			Tween.Add(timeIcon, "color", p_flag ? Color.yellow : Color.white, 0.1f, 0f, Cubic.Out);
		}
	}
}

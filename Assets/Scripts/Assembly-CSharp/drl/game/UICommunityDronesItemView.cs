using System;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UICommunityDronesItemView : UIElementView
	{
		public RawImage photoField;

		public RawImage droneThumbnailField;

		public FadeComponent dronePreviewPhotoFade;

		public AnimateImageLayout droneThumbnailAnimation;

		public FadeComponent droneThumbnailFade;

		public Image colorField;

		public Image thrustBar;

		public Image weightBar;

		public Text positionField;

		public Text usernameField;

		public Text nameField;

		public Text timeField;

		public Text thrustField;

		public Text weightField;

		public FadeComponent[] starFades;

		public Text ratingsCount;

		public UINavigation flyButton;

		public UINavigation saveButton;

		public UINavigation cloneButton;

		public UINavigation editButton;

		public UINavigation deleteButton;

		public UINavigation dataGroup;

		public UINavigation buttonsTopProxy;

		public UINavigation buttonsBottomProxy;

		public GarageStateModel garage;

		public GameObject trashIcon;

		public GameObject applyDeleteIcon;

		public FadeComponent redBackground;

		[HideInInspector]
		public bool confirmDelete;

		public new DRLCommunityDroneData data;

		private AsyncRequest m_photo_loader;

		public Texture photo
		{
			set
			{
				if (SetField(photoField, value))
				{
					photoField.enabled = value != null;
				}
			}
		}

		public Texture thumbnail
		{
			set
			{
				if (SetField(droneThumbnailField, value))
				{
					droneThumbnailField.enabled = value != null;
					if (value != null)
					{
						droneThumbnailFade.FadeIn();
						dronePreviewPhotoFade.FadeOut();
					}
					else
					{
						droneThumbnailFade.alpha = 0f;
					}
				}
			}
		}

		private bool SetField(Text p_field, string p_value)
		{
			if (p_field == null)
			{
				return false;
			}
			p_field.text = p_value;
			return true;
		}

		private bool SetField(RawImage p_field, Texture p_value)
		{
			if (!p_field)
			{
				return false;
			}
			p_field.texture = p_value;
			return true;
		}

		private bool SetColor(Image p_field, Color p_value)
		{
			if (p_field == null)
			{
				return false;
			}
			p_field.color = p_value;
			return true;
		}

		public void SetRating(float p_rating, int p_count)
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

		public void SetButtons(bool p_isMine)
		{
			if ((bool)editButton)
			{
				editButton.gameObject.SetActive(p_isMine);
			}
			if ((bool)deleteButton)
			{
				deleteButton.gameObject.SetActive(p_isMine);
			}
			if ((bool)cloneButton)
			{
				cloneButton.gameObject.SetActive(p_isMine);
			}
			if ((bool)saveButton)
			{
				saveButton.gameObject.SetActive(!p_isMine);
			}
		}

		public void Set(DRLCommunityDroneData p_data, bool p_myDrone, string p_steamId, string p_overrideProfileImgUrl = null, string p_playerName = "")
		{
			data = p_data;
			if (data == null)
			{
				return;
			}
			ShowDeleteButton(0.001f);
			if (m_photo_loader != null)
			{
				m_photo_loader.Cancel();
			}
			string text = (string.IsNullOrEmpty(data.droneName) ? "" : data.droneName);
			text = text.ToUpper();
			if (SetField(nameField, data.droneSize + "\" " + text + (data.isCustomPhysics ? " *" : "")))
			{
				nameField.color = (data.isCustomPhysics ? Color.yellow : Color.white);
			}
			string text2 = (string.IsNullOrEmpty(data.profileName) ? "" : data.profileName);
			text2 = text2.ToUpper();
			SetField(usernameField, text2);
			Action p_callback = delegate
			{
				UITruncateText component = nameField.GetComponent<UITruncateText>();
				if ((bool)component)
				{
					component.Refresh();
				}
				UITruncateText component2 = usernameField.GetComponent<UITruncateText>();
				if ((bool)component2)
				{
					component2.Refresh();
				}
			};
			RunOnce(0.1f, p_callback);
			string p_url = data.profileThumbURL;
			if (p_myDrone && p_overrideProfileImgUrl != null)
			{
				p_url = p_overrideProfileImgUrl;
			}
			if (SetColor(colorField, data.profileColor))
			{
				colorField.gameObject.SetActive(!p_myDrone);
			}
			photo = null;
			if ((bool)photoField)
			{
				m_photo_loader = Web.Load(p_url, "GET", delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
				{
					if (!(this == null) && !(base.gameObject == null) && !(photoField == null) && !(p_progress < 1f))
					{
						photo = p_result;
					}
				});
			}
			bool buttons = p_myDrone || p_steamId == p_data.playerId.ToString();
			thumbnail = null;
			dronePreviewPhotoFade.FadeIn(0.001f);
			string thumbURL = data.GetThumbURL(DRLCommunityDroneData.ThumbSize.Medium);
			if (droneThumbnailAnimation != null)
			{
				droneThumbnailAnimation.ResetAnimation();
				droneThumbnailAnimation.animationType = AnimateImageLayout.AnimationType.None;
			}
			if ((bool)droneThumbnailField)
			{
				garage.GetRigThumbnail(data.guid, thumbURL, 0, 0, delegate(Texture2D p_result)
				{
					if (!(this == null) && !(base.gameObject == null) && !(droneThumbnailField == null))
					{
						thumbnail = p_result;
					}
				});
			}
			SetRating(data.score, data.ratingCount);
			SetButtons(buttons);
			SetField(thrustField, Mathf.RoundToInt(data.droneThrust) + "g");
			if ((bool)thrustBar)
			{
				thrustBar.fillAmount = Mathf.Clamp01(data.droneThrust / 10000f);
			}
			SetField(weightField, Mathf.RoundToInt(data.droneWeight) + "g");
			if ((bool)weightBar)
			{
				weightBar.fillAmount = Mathf.Clamp01(data.droneWeight / 1500f);
			}
			SetField(timeField, FormatFlightTime(data.droneFlightTime) + " / " + FormatFlightTime(data.droneFlightTotal));
		}

		public string FormatFlightTime(float p_time)
		{
			if (p_time < 90f)
			{
				return Mathf.RoundToInt(p_time) + "M";
			}
			if (p_time > 5760f)
			{
				return Mathf.RoundToInt(p_time / 1440f) + "D";
			}
			return Mathf.RoundToInt(p_time / 60f) + "H";
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
	}
}

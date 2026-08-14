using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UIProgressionRankItemView : UIElementView<DRLApp>
	{
		public string type;

		public string leagueGUID;

		public Text positionField;

		public LayoutElement profilePhotoContainer;

		public RawImage profilePhotoField;

		public FadeComponent profilePhotoFade;

		public Image profileColorField;

		public Text profileNameField;

		public RawImage profileFlagField;

		public Texture2D defaultFlagImage;

		public Graphic selectionOutlineField;

		public Text xpField;

		public List<ImageClip> progressionRankFields;

		public List<RectTransform> layoutStates;

		public new DRLProgressionRankData data;

		private Texture2D m_profile_photo;

		private WebAsyncRequest m_photo_loader;

		private WebAsyncRequest m_flag_loader;

		public FadeComponent fade => AssertLocal<FadeComponent>("fade");

		public int position
		{
			set
			{
				int num = value;
				int num2 = ((num <= 13) ? num : (num % 10));
				string text = "th";
				switch (num2)
				{
				case 1:
					text = "st";
					break;
				case 2:
					text = "nd";
					break;
				case 3:
					text = "rd";
					break;
				}
				positionField.text = num + text;
			}
		}

		public int xp
		{
			set
			{
				if ((bool)xpField)
				{
					xpField.text = value + "XP";
					xpField.enabled = value >= 0;
				}
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

		public bool selected
		{
			get
			{
				return selectionOutlineField.enabled;
			}
			set
			{
				selectionOutlineField.enabled = value;
			}
		}

		public Texture2D profilePhoto
		{
			get
			{
				return m_profile_photo;
			}
			set
			{
				if ((bool)profilePhotoField)
				{
					profilePhotoField.texture = (m_profile_photo = value);
					profilePhotoField.enabled = m_profile_photo != null;
				}
			}
		}

		public void SetState(string p_state)
		{
			int num = 0;
			switch (p_state)
			{
			case "player":
				num = 0;
				break;
			case "promotion-separator":
				num = 1;
				break;
			case "demotion-separator":
				num = 2;
				break;
			}
			for (int i = 0; i < layoutStates.Count; i++)
			{
				layoutStates[i].gameObject.SetActive(i == num);
			}
			type = p_state;
		}

		public void SetRank(string p_league_id)
		{
			int num = base.app.model.storage.state.player.progression.GetLeagueIndexByGUID(p_league_id);
			switch (type)
			{
			case "promotion-separator":
				num++;
				break;
			case "demotion-separator":
				num--;
				break;
			}
			Sprite[] frames = base.app.model.storage.state.player.progression.GetLeagueThumbSprites().ToArray();
			for (int i = 0; i < progressionRankFields.Count; i++)
			{
				progressionRankFields[i].frames = frames;
				progressionRankFields[i].frame = num;
			}
			base.gameObject.SetActive(value: true);
			if (type != "player" && num < 0)
			{
				base.gameObject.SetActive(value: false);
			}
			if (type != "player" && num > progressionRankFields[0].count - 1)
			{
				base.gameObject.SetActive(value: false);
			}
		}

		public void Set(DRLProgressionRankData p_data)
		{
			_ = base.app.model.service;
			_ = base.app.model.storage.state.player.profile;
			data = p_data;
			if (m_photo_loader != null)
			{
				m_photo_loader.Cancel();
				m_photo_loader = null;
			}
			if (m_flag_loader != null)
			{
				m_flag_loader.Cancel();
				m_flag_loader = null;
			}
			if (p_data == null)
			{
				return;
			}
			position = p_data.position;
			profileName = (string.IsNullOrEmpty(p_data.profileName) ? "UNDEFINED" : p_data.profileName.ToUpper());
			profileColor = p_data.profileColor;
			profilePhoto = null;
			profilePhotoFade.alpha = 0f;
			string profileThumbURL = p_data.profileThumbURL;
			if (!string.IsNullOrEmpty(profileThumbURL))
			{
				m_photo_loader = Web.Load(profileThumbURL, "GET", delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
				{
					if (!(p_progress < 1f) && !(this == null) && (bool)p_result)
					{
						if ((bool)m_profile_photo)
						{
							Object.Destroy(m_profile_photo);
						}
						profilePhoto = p_result;
						profilePhotoFade.FadeIn(0.3f);
					}
				});
			}
			profileFlagField.texture = defaultFlagImage;
			string flagThumbURL = p_data.flagThumbURL;
			if (!string.IsNullOrEmpty(flagThumbURL))
			{
				m_flag_loader = Web.Load(flagThumbURL, "GET", delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
				{
					if (!(p_progress < 1f) && !(this == null) && (bool)p_result)
					{
						if ((bool)profileFlagField.texture && profileFlagField.texture != defaultFlagImage)
						{
							Object.Destroy(profileFlagField.texture);
						}
						profileFlagField.texture = p_result;
					}
				});
			}
			xp = p_data.weekXP;
			selected = p_data.isPlayer;
			SetState(p_data.type);
		}
	}
}

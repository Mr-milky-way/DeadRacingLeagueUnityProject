using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class AchievementCardView : UICardView
	{
		private int id;

		[SerializeField]
		private RawImage achievementImage;

		[SerializeField]
		private Text achievementTitle;

		[SerializeField]
		private Text achievementDescription;

		[SerializeField]
		private Image achievementScoreImage;

		[SerializeField]
		private RawImage achievementCompleteImage;

		[SerializeField]
		private UIAchievementButtonView achievementButtonView;

		[SerializeField]
		private UIElementView actionUIElementView;

		private DRLAchievementData achievementData;

		[SerializeField]
		private UINavigation achievementNav;

		[SerializeField]
		private UINavigation achievementButtonNav;

		private int xpBonus;

		public Text XpBonusText;

		private Color textDarkGreenColor = new Color(0.5088413f, 0.872f, 0f);

		public int Id
		{
			get
			{
				return id;
			}
			set
			{
				id = value;
			}
		}

		public RawImage AchievementImage
		{
			get
			{
				return achievementImage;
			}
			set
			{
				achievementImage = value;
			}
		}

		public Text AchievementTitle
		{
			get
			{
				return achievementTitle;
			}
			set
			{
				achievementTitle = value;
			}
		}

		public Text AchievementDescription
		{
			get
			{
				return achievementDescription;
			}
			set
			{
				achievementDescription = value;
			}
		}

		public Image AchievementScoreImage
		{
			get
			{
				return achievementScoreImage;
			}
			set
			{
				achievementScoreImage = value;
			}
		}

		public RawImage AchievementCompleteImage
		{
			get
			{
				return achievementCompleteImage;
			}
			set
			{
				achievementCompleteImage = value;
			}
		}

		public UIAchievementButtonView AchievementButtonView
		{
			get
			{
				return achievementButtonView;
			}
			set
			{
				achievementButtonView = value;
			}
		}

		public UIElementView ActionUIElementView
		{
			get
			{
				return actionUIElementView;
			}
			set
			{
				actionUIElementView = value;
			}
		}

		public DRLAchievementData AchievementData
		{
			get
			{
				return achievementData;
			}
			set
			{
				achievementData = value;
			}
		}

		public UINavigation AchievementNav
		{
			get
			{
				return AchievementNav;
			}
			set
			{
				AchievementNav = value;
			}
		}

		public UINavigation AchievementButtonNav
		{
			get
			{
				return AchievementButtonNav;
			}
			set
			{
				AchievementButtonNav = value;
			}
		}

		public void Set(string p_title, string p_description, float p_score, Texture p_texture, string notification)
		{
			AchievementTitle.text = p_title;
			AchievementDescription.text = p_description;
			AchievementScoreImage.fillAmount = p_score;
			AchievementImage.texture = p_texture;
			AchievementImage.enabled = false;
			if (ActionUIElementView != null)
			{
				ActionUIElementView.notification = notification;
			}
		}

		public void Set(DRLAchievementData p_data)
		{
			if (p_data != null)
			{
				AchievementData = p_data;
			}
			AchievementImage.enabled = false;
			if (p_data.progression < 1f)
			{
				GetTexture(p_data.lockedImageURL);
				AchievementDescription.text = p_data.lockedMessage;
				if (AchievementCompleteImage != null)
				{
					AchievementCompleteImage.enabled = false;
				}
			}
			else
			{
				GetTexture(p_data.unlockedImageURL);
				AchievementDescription.text = p_data.unlockedMessage;
				if (AchievementCompleteImage != null)
				{
					AchievementCompleteImage.enabled = true;
				}
			}
			AchievementTitle.text = p_data.title.ToUpper();
			AchievementScoreImage.fillAmount = p_data.progression;
			if (achievementButtonView != null)
			{
				if (p_data.hasRequirements && p_data.progression != 1f)
				{
					achievementButtonView.interactable = true;
					actionUIElementView.interactable = false;
				}
				else
				{
					achievementButtonView.interactable = false;
				}
			}
			SetXPMarkers(p_data);
			switch (p_data.id)
			{
			case "60c28e3e8948a90ada6014ee":
				actionUIElementView.interactable = false;
				break;
			case "60c28ec38948a90ada6014ef":
				actionUIElementView.interactable = false;
				break;
			case "5e8f70dfd03067aaa20c0800":
				actionUIElementView.notification = "community-drones.create-new3";
				actionUIElementView.interactable = true;
				break;
			case "5e8f70dfd03067aaa20c0801":
				actionUIElementView.notification = "community-drones.create-new4";
				actionUIElementView.interactable = true;
				break;
			case "5e8f70dfd03067aaa20c0802":
				actionUIElementView.notification = "community-drones.create-new5";
				actionUIElementView.interactable = true;
				break;
			case "5e8f70dfd03067aaa20c0803":
				actionUIElementView.notification = "community-drones.create-new6";
				actionUIElementView.interactable = true;
				break;
			case "5e8f70dfd03067aaa20c0804":
				actionUIElementView.notification = "community-drones.create-new7";
				actionUIElementView.interactable = true;
				break;
			case "5e8f70dfd03067aaa20c080a":
				actionUIElementView.notification = "home.missions";
				actionUIElementView.interactable = true;
				achievementButtonView.interactable = false;
				break;
			case "5e8f70dfd03067aaa20c080b":
				actionUIElementView.notification = "home.missions";
				actionUIElementView.interactable = true;
				achievementButtonView.interactable = false;
				break;
			case "5e8f70dfd03067aaa20c080c":
				actionUIElementView.notification = "home.missions";
				actionUIElementView.interactable = true;
				achievementButtonView.interactable = false;
				break;
			case "5e8f70dfd03067aaa20c080d":
				actionUIElementView.notification = "home.missions";
				actionUIElementView.interactable = true;
				achievementButtonView.interactable = false;
				break;
			case "5e8f70dfd03067aaa20c07fe":
				actionUIElementView.notification = "home.race";
				actionUIElementView.interactable = true;
				achievementButtonView.interactable = false;
				break;
			case "5e8f70dfd03067aaa20c0808":
				actionUIElementView.notification = "home.multiplayer";
				actionUIElementView.interactable = true;
				achievementButtonView.interactable = false;
				break;
			case "5e8f70dfd03067aaa20c0809":
				actionUIElementView.notification = "home.multiplayer";
				actionUIElementView.interactable = true;
				achievementButtonView.interactable = false;
				break;
			case "60be989c020282108a3f478c":
				achievementButtonView.interactable = false;
				break;
			case "60be98de020282108a3f478e":
				achievementButtonView.interactable = false;
				break;
			default:
				if (actionUIElementView != null)
				{
					actionUIElementView.notification = "settings.profile.achievements.detail";
				}
				break;
			}
		}

		private void SetXPMarkers(DRLAchievementData p_data)
		{
			xpBonus = p_data.xpBonus;
			XpBonusText.text = "(" + xpBonus + " XP)";
			achievementCompleteImage.color = textDarkGreenColor;
			achievementCompleteImage.gameObject.SetActive(value: false);
			if (p_data.progression >= 1f)
			{
				XpBonusText.color = textDarkGreenColor;
				achievementTitle.color = textDarkGreenColor;
			}
			else
			{
				XpBonusText.color = Color.grey;
				achievementTitle.color = Color.grey;
			}
		}

		public void Reset()
		{
			Set("", "", 0f, null, "");
		}

		public void GetTexture(string imageURL)
		{
			if (!AchievementImage)
			{
				return;
			}
			Web.Load(imageURL, "GET", delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
			{
				if (!(this == null) && !(base.gameObject == null) && !(AchievementImage == null) && !(p_progress < 1f) && !(p_result == null))
				{
					AchievementImage.texture = p_result;
					AchievementImage.enabled = true;
				}
			});
		}
	}
}

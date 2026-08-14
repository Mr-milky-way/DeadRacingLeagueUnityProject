using UnityEngine;
using UnityEngine.UI;
using drl.backend;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UITournamentBracketsPilotItemView : UIElementView
	{
		public new DRLTournamentPlayerData data;

		[Header("Body")]
		public RectTransform rect;

		public Image profileStripe;

		public RawImage profileImage;

		public Text profileName;

		public FadeComponent profileImageFader;

		public GameObject background;

		private AsyncRequest m_photo_loader;

		public Texture profilePhoto
		{
			set
			{
				if ((bool)profileImage)
				{
					profileImage.texture = value;
					profileImage.enabled = value != null;
				}
			}
		}

		public float Position
		{
			get
			{
				return rect.anchoredPosition.y;
			}
			set
			{
				Vector2 anchoredPosition = rect.anchoredPosition;
				anchoredPosition.y = value;
				rect.anchoredPosition = anchoredPosition;
			}
		}

		public void Clear()
		{
		}

		public void Set(DRLTournamentPlayerData p_data, Texture p_profileImage = null)
		{
			if (p_data == null)
			{
				return;
			}
			data = p_data;
			if ((bool)profileName)
			{
				profileName.text = data.profileName.ToUpper();
				UITruncateText component = profileName.GetComponent<UITruncateText>();
				if ((bool)component)
				{
					component.Refresh();
				}
			}
			if ((bool)profileStripe)
			{
				profileStripe.color = data.profileColor;
			}
			if (p_profileImage != null)
			{
				profilePhoto = p_profileImage;
				if ((bool)profileImageFader && profileImageFader.alpha < 0.99f)
				{
					profileImageFader.FadeIn(0.1f);
				}
				profileImage.material = Object.Instantiate(profileImage.material);
			}
			else
			{
				m_photo_loader = Web.Load(data.profileThumbURL, "GET", delegate(Texture2D p_result, float p_progress, WebAsyncRequest p_req)
				{
					if (!(p_result == null))
					{
						profilePhoto = p_result;
						if ((bool)profileImageFader && profileImageFader.alpha < 0.99f)
						{
							profileImageFader.FadeIn(0.1f);
						}
					}
				});
			}
			int siblingIndex = base.transform.GetSiblingIndex();
			background.SetActive(siblingIndex % 2 == 0);
		}
	}
}

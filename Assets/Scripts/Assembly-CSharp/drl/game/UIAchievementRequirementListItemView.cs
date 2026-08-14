using UnityEngine;
using UnityEngine.UI;
using thelab.mvc;

namespace drl.game
{
	public class UIAchievementRequirementListItemView : UIElementView<DRLApp>
	{
		[SerializeField]
		private Text titleText;

		[SerializeField]
		private RawImage completedImage;

		[SerializeField]
		private Text detailText;

		public string parentScreenName = "achievement-details-screen";

		public DRLMapTrack mapTrack;

		public MapData mapData;

		public string mapGUID;

		public int xp;

		private Color textDarkGreenColor = new Color(0.5088413f, 0.872f, 0f);

		public void Set(string p_title, string p_detail, bool p_isComplete, string p_notification = "")
		{
			titleText.text = p_title;
			detailText.text = p_detail;
			if (p_notification != "")
			{
				notification = p_notification;
			}
			if (p_isComplete)
			{
				completedImage.gameObject.SetActive(value: true);
				titleText.color = textDarkGreenColor;
			}
			else
			{
				completedImage.gameObject.SetActive(value: false);
				titleText.color = Color.gray;
			}
		}

		public void Set(string p_title, string p_detail, bool p_isComplete, DRLMapTrack p_mapTrack, string p_notification = "")
		{
			mapTrack = p_mapTrack;
			if (p_notification != "")
			{
				notification = p_notification;
			}
			Set(p_title, p_detail, p_isComplete);
		}

		public void Set(string p_title, string p_detail, bool p_isComplete, string p_mapGUID, string p_notification = "")
		{
			mapGUID = p_mapGUID;
			if (p_notification != "")
			{
				notification = p_notification;
			}
			Set(p_title, p_detail, p_isComplete);
		}

		public void Reset()
		{
			Set("", "", p_isComplete: false);
		}
	}
}

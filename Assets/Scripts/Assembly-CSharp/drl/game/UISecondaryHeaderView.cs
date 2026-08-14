using UnityEngine;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UISecondaryHeaderView : View<DRLApp>
	{
		public RectTransform underReviewWarningContent;

		[Header("UnderReview adjustments")]
		private const float UnderReviewWarningContentWidth = 400f;

		private const float UnderReviewWarningContent_Height1 = -45f;

		private const float UnderReviewWarningContent_Height2 = -145f;

		private const float UnderReviewWarningContent_Height3 = -197f;

		private const float UnderReviewWarningContent_Height4 = -170f;

		private float underReviewWarningWidth
		{
			get
			{
				return underReviewWarningContent.sizeDelta.x;
			}
			set
			{
				Vector2 sizeDelta = underReviewWarningContent.sizeDelta;
				sizeDelta.x = value;
				underReviewWarningContent.sizeDelta = sizeDelta;
			}
		}

		private float underReviewWarningAnchoredPosition_Y
		{
			get
			{
				return underReviewWarningContent.anchoredPosition.y;
			}
			set
			{
				Vector2 anchoredPosition = underReviewWarningContent.anchoredPosition;
				anchoredPosition.y = value;
				underReviewWarningContent.anchoredPosition = anchoredPosition;
			}
		}

		public UIScreen screen => AssertLocal<UIScreen>("screen");

		public void ShowUnderReviewWarning(bool p_show, float p_delay = 0f)
		{
			if (!(underReviewWarningContent == null) && (!p_show || (int)underReviewWarningWidth <= 0) && (p_show || (int)underReviewWarningWidth >= 398))
			{
				Tween.Kill(this, "underReviewWarningWidth");
				if (p_show)
				{
					underReviewWarningContent.transform.parent.gameObject.SetActive(value: true);
					underReviewWarningWidth = 0f;
					Tween.Add(this, "underReviewWarningWidth", 400f, 0.4f, p_delay, Cubic.Out);
				}
				else
				{
					underReviewWarningWidth = 400f;
					Tween.Add(this, "underReviewWarningWidth", 0f, 0f, p_delay, Cubic.Out);
				}
			}
		}

		public void Refresh(UIScreen p_screen, bool p_is_under_review)
		{
			if (base.app.model.service.platform != null && base.app.inVirtualSeason)
			{
				AdjustUnderReviewWarningOffsetYByScreen(p_screen);
				ShowUnderReviewWarning(p_is_under_review);
			}
			else
			{
				ShowUnderReviewWarning(p_show: false);
			}
		}

		public void AdjustUnderReviewWarningOffsetYByScreen(UIScreen p_screen)
		{
			switch (p_screen.name)
			{
			case "tournament-results-screen":
				underReviewWarningAnchoredPosition_Y = -145f;
				break;
			case "multiplayer-room-screen":
				underReviewWarningAnchoredPosition_Y = -197f;
				break;
			case "tournament-race-complete-screen":
				underReviewWarningAnchoredPosition_Y = -170f;
				break;
			default:
				underReviewWarningAnchoredPosition_Y = -45f;
				break;
			}
			underReviewWarningContent.transform.localScale = Vector3.one;
		}
	}
}

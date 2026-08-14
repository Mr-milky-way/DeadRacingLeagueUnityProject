using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UITournamentBracketsHeaderItem : MonoBehaviour
	{
		public Text levelField;

		public UITruncateText levelTrunc;

		public Text countDownField;

		public Text pilotsCountField;

		public Text matchesCountField;

		public Text winnersCountField;

		public Image bodyImage;

		public Image colorStripeImage;

		public Image playerIcon;

		public Image heatIcon;

		public Image winnerIcon;

		public RectTransform rect;

		public RectTransform spaceRect;

		public Sprite leftMostSprite;

		public Sprite midSprite;

		public Sprite rightMostSprite;

		public Sprite leftMostStripeSprite;

		public Sprite midStripeSprite;

		public Sprite rightMostStripeSprite;

		public Vector2 leftMostSizeSpace = new Vector2(350f, 2f);

		public Vector2 midSizeSpace = new Vector2(341f, 2f);

		public Vector2 rightMostSizeSpace = new Vector2(341f, 2f);

		public Vector2 leftMostSize = new Vector2(311f, 100f);

		public Vector2 midSize = new Vector2(311f, 100f);

		public Vector2 rightMostSize = new Vector2(300f, 100f);

		public Vector2 stripeSizeDeltaInit = new Vector2(8.3f, 6f);

		public void Set(string p_level, string p_count, int p_pilotsCount, int p_matchesCount, int p_winnersCount, float p_delay = 0f)
		{
			this.TimerRunOnce(delegate
			{
				if ((bool)levelField)
				{
					levelField.text = p_level.ToUpper();
					levelTrunc.Refresh();
				}
				if ((bool)countDownField)
				{
					countDownField.text = p_count;
				}
				if ((bool)pilotsCountField)
				{
					pilotsCountField.text = p_pilotsCount.ToString();
				}
				if ((bool)matchesCountField)
				{
					matchesCountField.text = ((p_matchesCount < 0) ? "∞" : p_matchesCount.ToString());
				}
				if ((bool)winnersCountField)
				{
					winnersCountField.text = p_winnersCount.ToString();
				}
			}, p_delay);
		}

		public void SetCountdown(string p_count)
		{
			if ((bool)countDownField)
			{
				countDownField.text = p_count;
			}
		}

		public void SetColor(Color p_color)
		{
			if ((bool)levelField)
			{
				levelField.color = p_color;
			}
			if ((bool)countDownField)
			{
				countDownField.color = p_color;
			}
			if ((bool)pilotsCountField)
			{
				pilotsCountField.color = p_color;
			}
			if ((bool)matchesCountField)
			{
				matchesCountField.color = p_color;
			}
			if ((bool)winnersCountField)
			{
				winnersCountField.color = p_color;
			}
			if ((bool)colorStripeImage)
			{
				colorStripeImage.color = p_color;
			}
			if ((bool)playerIcon)
			{
				playerIcon.color = p_color;
			}
			if ((bool)heatIcon)
			{
				heatIcon.color = p_color;
			}
			if ((bool)winnerIcon)
			{
				winnerIcon.color = p_color;
			}
		}

		public void UpdateBodyImage(bool p_left, bool p_middle, bool p_right)
		{
			if ((bool)bodyImage)
			{
				GetComponent<RectTransform>();
				RectTransform component = colorStripeImage.GetComponent<RectTransform>();
				if (p_left && (bool)leftMostSprite)
				{
					colorStripeImage.sprite = leftMostStripeSprite;
					bodyImage.sprite = leftMostSprite;
					spaceRect.sizeDelta = leftMostSizeSpace;
					rect.sizeDelta = leftMostSize;
					component.sizeDelta = stripeSizeDeltaInit;
				}
				if (p_middle && (bool)midSprite)
				{
					colorStripeImage.sprite = midStripeSprite;
					bodyImage.sprite = midSprite;
					spaceRect.sizeDelta = midSizeSpace;
					rect.sizeDelta = midSize;
					component.sizeDelta = stripeSizeDeltaInit;
				}
				if (p_right && (bool)rightMostSprite)
				{
					colorStripeImage.sprite = rightMostStripeSprite;
					bodyImage.sprite = rightMostSprite;
					spaceRect.sizeDelta = rightMostSizeSpace;
					rect.sizeDelta = rightMostSize;
					component.sizeDelta = new Vector2(0f, stripeSizeDeltaInit.y);
				}
			}
		}
	}
}

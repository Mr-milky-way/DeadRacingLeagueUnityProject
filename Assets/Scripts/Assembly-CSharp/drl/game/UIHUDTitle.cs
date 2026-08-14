using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIHUDTitle : MonoBehaviour
	{
		public FadeComponent fade;

		public Text leftTitle;

		public RectTransform leftTitleRT;

		public Text leftCaption;

		public RectTransform leftCaptionRT;

		public Text rightTitle;

		public RectTransform rightTitleRT;

		public Text rightCaption;

		public RectTransform rightCaptionRT;

		public RectTransform separatorRT;

		public float leftTitleStartX;

		public float leftCaptionStartX;

		public float rightTitleStartX;

		public float rightCaptionStartX;

		public RectTransform leftContainer;

		public RectTransform rightContainer;

		private Activity m_text_size_timer;

		private float m_separator_angle;

		public float separatorAngle
		{
			get
			{
				return m_separator_angle;
			}
			set
			{
				Vector3 localEulerAngles = separatorRT.localEulerAngles;
				localEulerAngles.z = (m_separator_angle = value);
				separatorRT.localEulerAngles = localEulerAngles;
			}
		}

		public float separatorScaleY
		{
			get
			{
				return separatorRT.localScale.y;
			}
			set
			{
				Vector3 localScale = separatorRT.localScale;
				localScale.y = value;
				separatorRT.localScale = localScale;
			}
		}

		public float leftTitleX
		{
			get
			{
				return leftTitleRT.anchoredPosition.x;
			}
			set
			{
				Vector2 anchoredPosition = leftTitleRT.anchoredPosition;
				anchoredPosition.x = value;
				leftTitleRT.anchoredPosition = anchoredPosition;
			}
		}

		public float rightTitleX
		{
			get
			{
				return rightTitleRT.anchoredPosition.x;
			}
			set
			{
				Vector2 anchoredPosition = rightTitleRT.anchoredPosition;
				anchoredPosition.x = value;
				rightTitleRT.anchoredPosition = anchoredPosition;
			}
		}

		public float leftCaptionX
		{
			get
			{
				return leftCaptionRT.anchoredPosition.x;
			}
			set
			{
				Vector2 anchoredPosition = leftCaptionRT.anchoredPosition;
				anchoredPosition.x = value;
				leftCaptionRT.anchoredPosition = anchoredPosition;
			}
		}

		public float rightCaptionX
		{
			get
			{
				return rightCaptionRT.anchoredPosition.x;
			}
			set
			{
				Vector2 anchoredPosition = rightCaptionRT.anchoredPosition;
				anchoredPosition.x = value;
				rightCaptionRT.anchoredPosition = anchoredPosition;
			}
		}

		protected void Awake()
		{
			leftTitleStartX = leftTitleX;
			leftCaptionStartX = leftCaptionX;
			rightTitleStartX = rightTitleX;
			rightCaptionStartX = rightCaptionX;
		}

		public void Clear()
		{
			float num = 1500f;
			Set();
			separatorAngle = 348f;
			separatorScaleY = 0f;
			leftTitleX = num;
			leftCaptionX = num;
			rightTitleX = 0f - num;
			rightCaptionX = 0f - num;
		}

		public void Set(string p_title_left = "", string p_caption_left = "", string p_title_right = "", string p_caption_right = "")
		{
			leftTitle.text = p_title_left;
			rightTitle.text = p_title_right;
			leftCaption.text = p_caption_left;
			rightCaption.text = p_caption_right;
			Transform p_container = base.transform.Find("content");
			Text[] array = new Text[4] { leftTitle, rightTitle, leftCaption, rightCaption };
			for (int i = 0; i < array.Length; i++)
			{
				Hierarchy.RefreshLayout(array[i], p_container);
			}
			Vector2 sizeDelta = leftContainer.sizeDelta;
			sizeDelta.x = Mathf.Max(leftTitleRT.sizeDelta.x, leftCaptionRT.sizeDelta.x) + 60f;
			leftContainer.sizeDelta = sizeDelta;
			sizeDelta = rightContainer.sizeDelta;
			sizeDelta.x = Mathf.Max(rightTitleRT.sizeDelta.x, rightCaptionRT.sizeDelta.x) + 60f;
			rightContainer.sizeDelta = sizeDelta;
		}

		public void Show(float p_delay = 0f, float p_speed = 1f)
		{
			float num = p_delay;
			float num2 = 0.05f;
			float num3 = 1000f;
			Tween.Kill(this);
			separatorAngle = 168f;
			separatorScaleY = 0f;
			leftTitleX = num3;
			leftCaptionX = num3;
			rightTitleX = 0f - num3;
			rightCaptionX = 0f - num3;
			float num4 = 0.4f * p_speed;
			Tween.Add(this, "separatorAngle", -12f, num4, num, Cubic.Out);
			Tween.Add(this, "separatorScaleY", 1f, num4, num, Cubic.Out);
			num += num4 * 0.7f + num2 * 2f;
			num4 = 0.6f * p_speed;
			Tween.Add(this, "leftTitleX", leftTitleStartX, num4, num, Cubic.Out);
			num += num2;
			Tween.Add(this, "rightTitleX", rightTitleStartX, num4, num, Cubic.Out);
			num += num2;
			Tween.Add(this, "leftCaptionX", leftCaptionStartX, num4, num, Cubic.Out);
			num += num2;
			Tween.Add(this, "rightCaptionX", rightCaptionStartX, num4, num, Cubic.Out);
			num += num2;
		}

		public void Hide(float p_delay = 0f)
		{
			float num = p_delay;
			float num2 = 1000f;
			float num3 = 0.02f;
			Tween.Kill(this);
			separatorAngle = -12f;
			separatorScaleY = 1f;
			leftTitleX = leftTitleStartX;
			leftCaptionX = leftCaptionStartX;
			rightTitleX = rightTitleStartX;
			rightCaptionX = rightCaptionStartX;
			Tween.Add(this, "leftTitleX", num2, 0.4f, num, Cubic.Out);
			num += num3;
			Tween.Add(this, "rightTitleX", 0f - num2, 0.4f, num, Cubic.Out);
			num += num3;
			Tween.Add(this, "leftCaptionX", num2, 0.4f, num, Cubic.Out);
			num += num3;
			Tween.Add(this, "rightCaptionX", 0f - num2, 0.4f, num, Cubic.Out);
			num += num3;
			num += 0.2f + num3 * 2f;
			Tween.Add(this, "separatorAngle", 168f, 0.4f, num, Cubic.Out);
			Tween.Add(this, "separatorScaleY", 0f, 0.4f, num, Cubic.Out);
		}
	}
}

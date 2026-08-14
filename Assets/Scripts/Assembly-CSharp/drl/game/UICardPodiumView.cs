using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UICardPodiumView : UIElementView
	{
		public Image backgroundField;

		public Image gradientField;

		public RawImage photoField;

		public RectTransform footerContainer;

		public Image footerBackgroundField;

		public Text nameField;

		[SerializeField]
		private FadeComponent m_fade;

		public float cardXOffset = -300f;

		public float cardYOffset = -1000f;

		public float cardHeight = 420f;

		public float scaleLarge = 2f;

		public float scaleNormal = 1f;

		public FadeComponent fade
		{
			get
			{
				if (!m_fade)
				{
					return m_fade = GetComponent<FadeComponent>();
				}
				return m_fade;
			}
		}

		public Color color
		{
			set
			{
				Image image = backgroundField;
				Color color = (footerBackgroundField.color = value);
				image.color = color;
				gradientField.color = value;
			}
		}

		public Texture photo
		{
			set
			{
				photoField.texture = value;
				photoField.enabled = value != null;
			}
		}

		public string profileName
		{
			set
			{
				nameField.text = value;
				UITruncateText component = nameField.GetComponent<UITruncateText>();
				if ((bool)component)
				{
					component.Refresh();
				}
			}
		}

		public float x
		{
			get
			{
				return ((RectTransform)base.transform).anchoredPosition.x;
			}
			set
			{
				RectTransform obj = (RectTransform)base.transform;
				Vector2 anchoredPosition = obj.anchoredPosition;
				anchoredPosition.x = value;
				obj.anchoredPosition = anchoredPosition;
			}
		}

		public float y
		{
			get
			{
				return ((RectTransform)base.transform).anchoredPosition.y;
			}
			set
			{
				RectTransform obj = (RectTransform)base.transform;
				Vector2 anchoredPosition = obj.anchoredPosition;
				anchoredPosition.y = value;
				obj.anchoredPosition = anchoredPosition;
			}
		}

		public float height
		{
			get
			{
				return ((RectTransform)base.transform).sizeDelta.y;
			}
			set
			{
				RectTransform obj = (RectTransform)base.transform;
				Vector2 sizeDelta = obj.sizeDelta;
				sizeDelta.y = value;
				obj.sizeDelta = sizeDelta;
			}
		}

		public float footerHeight
		{
			get
			{
				return footerContainer.sizeDelta.y;
			}
			set
			{
				Vector2 sizeDelta = footerContainer.sizeDelta;
				sizeDelta.y = value;
				footerContainer.sizeDelta = sizeDelta;
			}
		}

		public float photoHeight
		{
			get
			{
				return ((RectTransform)photoField.transform).sizeDelta.y;
			}
			set
			{
				RectTransform obj = (RectTransform)photoField.transform;
				Vector2 sizeDelta = obj.sizeDelta;
				sizeDelta.y = value;
				obj.sizeDelta = sizeDelta;
			}
		}

		public float scaleXY
		{
			get
			{
				return ((RectTransform)base.transform).localScale.x;
			}
			set
			{
				RectTransform obj = (RectTransform)base.transform;
				Vector3 localScale = obj.localScale;
				localScale.x = value;
				localScale.y = value;
				obj.localScale = localScale;
			}
		}

		public void Show(float p_delay, float p_time = 0.75f)
		{
			height = 10f;
			footerHeight = 0f;
			photoHeight = 0f;
			float num = p_delay;
			float p_duration = p_time / 3f;
			Tween.Kill(this, "height");
			Tween.Kill(this, "photoHeight");
			Tween.Kill(this, "footerHeight");
			Tween.Add(this, "height", cardHeight, p_duration, num, Cubic.Out);
			num += ((p_time <= 0f) ? 0f : 0.05f);
			Tween.Add(this, "photoHeight", cardHeight - 10f, p_duration, num, Cubic.Out);
			num += ((p_time <= 0f) ? 0f : 0.05f);
			Tween.Add(this, "footerHeight", 80f, p_duration, num, Cubic.Out);
		}

		public void ShowFadeIn(float p_delay, float p_time = 0.6f)
		{
			Show(p_delay, 0f);
			MoveIn(p_delay, 0f);
			fade.FadeOut(0f);
			fade.FadeIn(p_time, p_delay);
		}

		public void Hide(float p_delay, float p_time = 0.75f)
		{
			height = cardHeight;
			footerHeight = 80f;
			photoHeight = cardHeight - 10f;
			float num = p_delay;
			float p_duration = p_time / 3f;
			Tween.Kill(this, "height");
			Tween.Kill(this, "photoHeight");
			Tween.Kill(this, "footerHeight");
			Tween.Add(this, "height", 10f, p_duration, num, Cubic.Out);
			num += ((p_time <= 0f) ? 0f : 0.05f);
			Tween.Add(this, "photoHeight", 0f, p_duration, num, Cubic.Out);
			num += ((p_time <= 0f) ? 0f : 0.05f);
			Tween.Add(this, "footerHeight", 0f, p_duration, num, Cubic.Out);
		}

		public void HideFadeOut(float p_delay, float p_time = 0.6f)
		{
			Show(p_delay, 0f);
			MoveIn(p_delay, 0f);
			fade.FadeIn(0f);
			fade.FadeOut(p_time, p_delay);
		}

		public void MoveX(float p_x, float p_delay, float p_time = 0.6f)
		{
			Tween.Kill(this, "x");
			Tween.Add(this, "x", p_x, p_time, p_delay, Cubic.Out);
		}

		public void MoveY(float p_y, float p_delay, float p_time = 0.6f)
		{
			Tween.Kill(this, "y");
			Tween.Add(this, "y", p_y, p_time, p_delay, Cubic.Out);
		}

		public void MoveIn(float p_delay, float p_time = 0.6f)
		{
			y = cardYOffset;
			MoveY(0f, p_delay, p_time);
		}

		public void MoveInX(float p_delay, float p_time = 0.6f)
		{
			x = cardXOffset;
			MoveX(0f, p_delay, p_time);
		}

		public void MoveOut(float p_delay, float p_time = 0.6f)
		{
			y = 0f;
			MoveY(cardYOffset, p_delay, p_time);
		}

		public void MoveOutX(float p_delay, float p_time = 0.6f)
		{
			x = 0f;
			MoveX(cardXOffset, p_delay, p_time);
		}

		public void Scale(float p_scale, float p_delay, float p_time = 0.6f)
		{
			Tween.Kill(this, "scaleXY");
			Tween.Add(this, "scaleXY", p_scale, p_time, p_delay, Cubic.Out);
		}

		public void ScaleDown(float p_delay, float p_time = 0.6f)
		{
			scaleXY = scaleLarge;
			Scale(scaleNormal, p_delay, p_time);
		}

		public void ScaleUp(float p_delay, float p_time = 0.6f)
		{
			scaleXY = scaleNormal;
			Scale(scaleLarge, p_delay, p_time);
		}
	}
}

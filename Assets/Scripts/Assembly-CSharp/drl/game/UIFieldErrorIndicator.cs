using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIFieldErrorIndicator : MonoBehaviour
	{
		public float stripeSize = 8f;

		public RectTransform stripe;

		public Image outline;

		public Color outlineNormalColor = new Color(0.57f, 0.57f, 0.57f, 1f);

		public Color outlineErrorColor = Color.red;

		public float stripeWidth
		{
			get
			{
				return stripe.sizeDelta.x;
			}
			set
			{
				Vector2 sizeDelta = stripe.sizeDelta;
				sizeDelta.x = value;
				stripe.sizeDelta = sizeDelta;
			}
		}

		public Color outlineColor
		{
			get
			{
				return outline.color;
			}
			set
			{
				outline.color = value;
			}
		}

		private void Start()
		{
		}

		public void Show(float p_delay, float p_time = 0.3f)
		{
			if ((bool)stripe)
			{
				stripe.gameObject.SetActive(value: true);
				stripeWidth = 0f;
			}
			if ((bool)outline)
			{
				outlineColor = outlineNormalColor;
			}
			Tween.Kill(this, "stripeWidth");
			Tween.Kill(this, "outlineColor");
			if ((bool)stripe)
			{
				Tween.Add(this, "stripeWidth", stripeSize, p_time, p_delay, Cubic.Out);
			}
			if ((bool)outline)
			{
				Tween.Add(this, "outlineColor", outlineErrorColor, p_time, p_delay, Cubic.Out);
			}
		}

		public bool IsOn()
		{
			if ((bool)stripe)
			{
				return stripe.gameObject.activeInHierarchy;
			}
			if ((bool)outline)
			{
				return object.Equals(outlineColor, outlineErrorColor);
			}
			return false;
		}

		public void Hide()
		{
			if ((bool)stripe)
			{
				stripe.gameObject.SetActive(value: false);
			}
			if ((bool)outline)
			{
				outlineColor = outlineNormalColor;
			}
			Tween.Kill(this, "stripeWidth");
			Tween.Kill(this, "outlineColor");
		}
	}
}

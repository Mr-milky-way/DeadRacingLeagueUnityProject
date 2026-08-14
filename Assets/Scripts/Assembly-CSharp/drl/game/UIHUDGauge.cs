using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIHUDGauge : MonoBehaviour
	{
		public Text titleField;

		public Text labelTopField;

		public Text labelMiddleField;

		public Text labelBottomField;

		public RectTransform barFill;

		public Image barFillImage;

		public FadeComponent fade;

		public ParticleSystem hilight;

		public bool locked;

		public string title
		{
			set
			{
				titleField.text = value;
			}
		}

		public string labelTop
		{
			set
			{
				labelTopField.text = value;
			}
		}

		public string labelMiddle
		{
			set
			{
				labelMiddleField.text = value;
			}
		}

		public string labelBottom
		{
			set
			{
				labelBottomField.text = value;
			}
		}

		public float barRatio
		{
			set
			{
				if (!locked)
				{
					barFillImage.fillAmount = Mathf.Clamp01(value);
				}
			}
		}

		public void Hilight()
		{
			if ((bool)hilight)
			{
				hilight.Play(withChildren: true);
			}
		}

		public void SetLock(bool f, float p_duration = 0.8f)
		{
			if (locked != f)
			{
				locked = f;
				fade.Fade(f ? 0.5f : 1f, p_duration);
			}
		}
	}
}

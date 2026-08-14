using UnityEngine;
using UnityEngine.UI;
using thelab.core;

namespace drl.game
{
	public class UIHUDPrecisionGauge : MonoBehaviour
	{
		public Text titleField;

		public Text labelTopField;

		public Text labelMiddleField;

		public Text labelBottomField;

		public LayoutElement barMiddleFill;

		public RectTransform barDrag;

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

		public float barMiddleRatio
		{
			set
			{
				barMiddleFill.preferredHeight = Mathf.Lerp(0f, 360f, Mathf.Clamp01(value));
			}
		}

		public float barDragRatio
		{
			set
			{
				barDrag.anchoredPosition = new Vector2(0f, Mathf.Lerp(-190f, 190f, Mathf.Clamp01(value)));
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

using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class UISpectateTargetButton : UIElementView<DRLApp>
	{
		public RectTransform contentContainer;

		public FadeComponent focusFade;

		public FadeComponent focusOutlineFade;

		public Text numberField;

		public FadeComponent fade;

		public int index;

		public UINavigation navigation => AssertLocal<UINavigation>("navigation");

		public void SetLabel(string p_value)
		{
			numberField.text = p_value;
		}

		public void SetEnabled(bool p_flag)
		{
			fade.disableThreshold = 0.21f;
			fade.alpha = (p_flag ? 1f : 0.2f);
		}

		public void SetFocus(bool p_flag)
		{
			focusFade.alpha = (p_flag ? 1f : 0f);
		}

		public void Blink(float p_duration, float p_delay = 0f)
		{
			FadeComponent fadeComponent = focusFade;
			fadeComponent.alpha = 1f;
			fadeComponent.Fade(0f, p_duration, p_delay);
			FadeComponent fadeComponent2 = focusOutlineFade;
			fadeComponent2.alpha = 1f;
			fadeComponent2.Fade(0f, p_duration, p_delay);
		}
	}
}

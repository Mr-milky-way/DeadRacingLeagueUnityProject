using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLToggleExpandView : ToggleView
	{
		public FadeComponent fade;

		public LayoutElement layout;

		public Text field;

		protected RectTransform m_field_rt;

		protected RectTransform m_rt;

		public float minWidth = 32f;

		public float minAlpha = 0.1f;

		public float margin = 6f;

		public bool toggleFade = true;

		protected override void Awake()
		{
			m_field_rt = field.transform as RectTransform;
			m_rt = base.transform as RectTransform;
			base.Awake();
			if ((bool)toggle)
			{
				SetState(toggle.isOn);
			}
		}

		protected override void OnChange(bool v)
		{
			base.OnChange(v);
			if (base.enabled)
			{
				SetState(v);
			}
		}

		public override void SetState(bool p_flag)
		{
			base.SetState(p_flag);
			float num = minWidth;
			float p_alpha = (toggleFade ? minAlpha : 1f);
			if (p_flag)
			{
				num += m_field_rt.sizeDelta.x + margin;
				p_alpha = 1f;
			}
			if ((bool)layout)
			{
				Tween.Kill(layout);
				Tween.Add(layout, "minWidth", num, 0.3f, Cubic.Out);
			}
			else
			{
				Vector2 sizeDelta = m_rt.sizeDelta;
				sizeDelta.x = num;
				Tween.Kill(m_rt);
				Tween.Add(m_rt, "sizeDelta", sizeDelta, 0.3f, Cubic.Out);
			}
			if (toggleFade && (bool)fade)
			{
				fade.Fade(p_alpha, 0.3f, 0f, Cubic.Out);
			}
		}
	}
}

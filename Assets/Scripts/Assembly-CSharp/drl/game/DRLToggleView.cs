using UnityEngine;
using UnityEngine.UI;
using thelab.core;
using thelab.mvc;

namespace drl.game
{
	public class DRLToggleView : ToggleView
	{
		public RawImage tick;

		[Range(0f, 1f)]
		public float duration = 0.2f;

		public bool isSwitch;

		protected override void Awake()
		{
			base.Awake();
			if ((bool)toggle && (bool)tick)
			{
				SetState(toggle.isOn);
			}
		}

		protected override void OnChange(bool v)
		{
			base.OnChange(v);
			if (base.enabled && (bool)tick)
			{
				SetState(v);
			}
		}

		public override void SetState(bool p_flag)
		{
			base.SetState(p_flag);
			bool flag = p_flag;
			Tween.Kill(tick);
			if (!isSwitch)
			{
				Rect uvRect = tick.uvRect;
				uvRect.x = (flag ? (1f - uvRect.width) : 0f);
				Tween.Add(tick, "uvRect", uvRect, duration, Cubic.Out);
				return;
			}
			RectTransform obj = tick.transform as RectTransform;
			Vector2 anchoredPosition = obj.anchoredPosition;
			anchoredPosition.x = (flag ? 28f : 0f);
			Tween.Kill(obj);
			Tween.Add(obj, "anchoredPosition", anchoredPosition, duration, Cubic.Out);
			Tween.Add(tick, "color", flag ? Colorf.RGBToColor(8311585u) : Color.red, duration, Cubic.Out);
		}
	}
}

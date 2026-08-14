using System;
using UnityEngine;

namespace thelab.core
{
	[Serializable]
	public class RectTween : Tween<Rect>
	{
		public RectTween(object p_target, string p_property, Rect p_to, float p_duration, float p_delay, Easing p_easing, bool p_threaded)
			: base(p_target, p_property, p_to, p_duration, p_delay, p_easing, p_threaded)
		{
		}

		public RectTween(object p_target, string p_property, Rect p_to, float p_duration, float p_delay, Easing p_easing)
			: this(p_target, p_property, p_to, p_duration, p_delay, p_easing, p_threaded: false)
		{
		}

		public RectTween(object p_target, string p_property, Rect p_to, float p_duration, Easing p_easing)
			: this(p_target, p_property, p_to, p_duration, 0f, p_easing, p_threaded: false)
		{
		}

		public RectTween(object p_target, string p_property, Rect p_to, Easing p_easing)
			: this(p_target, p_property, p_to, 0.25f, 0f, p_easing, p_threaded: false)
		{
		}

		public override void Lerp(float p_ratio)
		{
			Rect rect = from;
			float num = easing(p_ratio);
			rect.xMin += (to.xMin - rect.xMin) * num;
			rect.xMax += (to.xMax - rect.xMax) * num;
			rect.yMin += (to.yMin - rect.yMin) * num;
			rect.yMax += (to.yMax - rect.yMax) * num;
			switch (type)
			{
			case Target.Type:
				Reflection<object>.SetStatic((Type)target, property, rect);
				break;
			case Target.Material:
				((Material)target).SetVector(property, new Vector4(rect.xMin, rect.yMin, rect.xMax, rect.yMax));
				break;
			default:
				Reflection<object>.Set(target, property, rect);
				break;
			}
		}
	}
}

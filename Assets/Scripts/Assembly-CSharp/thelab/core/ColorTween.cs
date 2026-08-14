using System;
using UnityEngine;

namespace thelab.core
{
	[Serializable]
	public class ColorTween : Tween<Color>
	{
		public ColorTween(object p_target, string p_property, Color p_to, float p_duration, float p_delay, Easing p_easing, bool p_threaded)
			: base(p_target, p_property, p_to, p_duration, p_delay, p_easing, p_threaded)
		{
		}

		public ColorTween(object p_target, string p_property, Color p_to, float p_duration, float p_delay, Easing p_easing)
			: this(p_target, p_property, p_to, p_duration, p_delay, p_easing, p_threaded: false)
		{
		}

		public ColorTween(object p_target, string p_property, Color p_to, float p_duration, Easing p_easing)
			: this(p_target, p_property, p_to, p_duration, 0f, p_easing, p_threaded: false)
		{
		}

		public ColorTween(object p_target, string p_property, Color p_to, Easing p_easing)
			: this(p_target, p_property, p_to, 0.25f, 0f, p_easing, p_threaded: false)
		{
		}

		public override void Lerp(float p_ratio)
		{
			Color color = from + (to - from) * easing(p_ratio);
			switch (type)
			{
			case Target.Type:
				Reflection<object>.SetStatic((Type)target, property, color);
				break;
			case Target.Material:
				((Material)target).SetColor(property, color);
				break;
			default:
				Reflection<object>.Set(target, property, color);
				break;
			}
		}
	}
}

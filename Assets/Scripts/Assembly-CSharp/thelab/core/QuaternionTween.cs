using System;
using UnityEngine;

namespace thelab.core
{
	[Serializable]
	public class QuaternionTween : Tween<Quaternion>
	{
		public QuaternionTween(object p_target, string p_property, Quaternion p_to, float p_duration, float p_delay, Easing p_easing, bool p_threaded)
			: base(p_target, p_property, p_to, p_duration, p_delay, p_easing, p_threaded)
		{
		}

		public QuaternionTween(object p_target, string p_property, Quaternion p_to, float p_duration, float p_delay, Easing p_easing)
			: this(p_target, p_property, p_to, p_duration, p_delay, p_easing, p_threaded: false)
		{
		}

		public QuaternionTween(object p_target, string p_property, Quaternion p_to, float p_duration, Easing p_easing)
			: this(p_target, p_property, p_to, p_duration, 0f, p_easing, p_threaded: false)
		{
		}

		public QuaternionTween(object p_target, string p_property, Quaternion p_to, Easing p_easing)
			: this(p_target, p_property, p_to, 0.25f, 0f, p_easing, p_threaded: false)
		{
		}

		public override void Lerp(float p_ratio)
		{
			Quaternion quaternion = Quaternion.Slerp(from, to, easing(base.progress));
			if (type == Target.Type)
			{
				Reflection<object>.SetStatic((Type)target, property, quaternion);
			}
			else
			{
				Reflection<object>.Set(target, property, quaternion);
			}
		}
	}
}

using System;
using UnityEngine;

namespace thelab.core
{
	[Serializable]
	public class Vector3Tween : Tween<Vector3>
	{
		public Vector3Tween(object p_target, string p_property, Vector3 p_to, float p_duration, float p_delay, Easing p_easing, bool p_threaded)
			: base(p_target, p_property, p_to, p_duration, p_delay, p_easing, p_threaded)
		{
		}

		public Vector3Tween(object p_target, string p_property, Vector3 p_to, float p_duration, float p_delay, Easing p_easing)
			: this(p_target, p_property, p_to, p_duration, p_delay, p_easing, p_threaded: false)
		{
		}

		public Vector3Tween(object p_target, string p_property, Vector3 p_to, float p_duration, Easing p_easing)
			: this(p_target, p_property, p_to, p_duration, 0f, p_easing, p_threaded: false)
		{
		}

		public Vector3Tween(object p_target, string p_property, Vector3 p_to, Easing p_easing)
			: this(p_target, p_property, p_to, 0.25f, 0f, p_easing, p_threaded: false)
		{
		}

		public override void Lerp(float p_ratio)
		{
			Vector3 vector = from + (to - from) * easing(p_ratio);
			switch (type)
			{
			case Target.Type:
				Reflection<object>.SetStatic((Type)target, property, vector);
				break;
			case Target.Material:
				((Material)target).SetVector(property, vector);
				break;
			default:
				Reflection<object>.Set(target, property, vector);
				break;
			}
		}
	}
}

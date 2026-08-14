using System;
using UnityEngine;

namespace thelab.core
{
	[Serializable]
	public class TransformTween : Tween<Transform>
	{
		private Vector3 p;

		private Quaternion r;

		private Vector3 s;

		public TransformTween(object p_target, string p_property, Transform p_to, float p_duration, float p_delay, Easing p_easing, bool p_threaded)
			: base(p_target, p_property, p_to, p_duration, p_delay, p_easing, p_threaded)
		{
		}

		public TransformTween(object p_target, string p_property, Transform p_to, float p_duration, float p_delay, Easing p_easing)
			: this(p_target, p_property, p_to, p_duration, p_delay, p_easing, p_threaded: false)
		{
		}

		public TransformTween(object p_target, string p_property, Transform p_to, float p_duration, Easing p_easing)
			: this(p_target, p_property, p_to, p_duration, 0f, p_easing, p_threaded: false)
		{
		}

		public TransformTween(object p_target, string p_property, Transform p_to, Easing p_easing)
			: this(p_target, p_property, p_to, 0.25f, 0f, p_easing, p_threaded: false)
		{
		}

		protected override void OnStart()
		{
			base.OnStart();
			if (valid)
			{
				p = from.position;
				r = from.rotation;
				s = from.localScale;
			}
		}

		public override void Lerp(float p_ratio)
		{
			Transform transform = from;
			Vector3 position = to.position;
			Quaternion rotation = to.rotation;
			Vector3 localScale = to.localScale;
			float num = easing(base.progress);
			transform.position = p + (position - p) * num;
			transform.localScale = s + (localScale - s) * num;
			transform.rotation = Quaternion.Slerp(r, rotation, num);
		}
	}
}

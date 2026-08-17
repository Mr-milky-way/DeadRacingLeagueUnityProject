using System;
using System.Collections.Generic;
using UnityEngine;

namespace thelab.core
{
	[Serializable]
	public class Tween<T> : Tween
	{
		public T from;

		public T to;

		internal Format format;

		public Tween(object p_target, string p_property, T p_to, float p_duration, float p_delay, Easing p_easing, bool p_threaded)
			: base(p_target, p_property, p_duration, p_delay, p_easing, p_threaded)
		{
			to = p_to;
			if (typeof(T) == typeof(float))
			{
				format = Format.Float;
			}
			else if (typeof(T) == typeof(Color))
			{
				format = Format.Color;
			}
			else if (typeof(T) == typeof(Vector3))
			{
				format = Format.Vector3;
			}
			else if (typeof(T) == typeof(Vector2))
			{
				format = Format.Vector2;
			}
			else if (typeof(T) == typeof(Quaternion))
			{
				format = Format.Quaternion;
			}
			else if (typeof(T) == typeof(Transform))
			{
				format = Format.Transform;
			}
			else if (typeof(T) == typeof(Rect))
			{
				format = Format.Rect;
			}
			else if (typeof(T) == typeof(Vector4))
			{
				format = Format.Vector4;
			}
			else if (typeof(T) == typeof(int))
			{
				format = Format.Int;
			}
		}

		public Tween(object p_target, string p_property, T p_to, float p_duration, float p_delay, Easing p_easing)
			: this(p_target, p_property, p_to, p_duration, p_delay, p_easing, false)
		{
		}

		public Tween(object p_target, string p_property, T p_to, float p_duration, Easing p_easing)
			: this(p_target, p_property, p_to, p_duration, 0f, p_easing, false)
		{
		}

		public Tween(object p_target, string p_property, T p_to, Easing p_easing)
			: this(p_target, p_property, p_to, 0.25f, 0f, p_easing, false)
		{
		}

		protected override void OnStart()
		{
			base.OnStart();
			if (!valid)
			{
				return;
			}
			Tween.KillDuplicates(this);
			switch (type)
			{
			case Target.Default:
			case Target.Transform:
			case Target.Component:
				from = Reflection<object>.Get<T>(target, property);
				break;
			case Target.Type:
				from = Reflection<object>.GetStatic<T>((Type)target, property);
				break;
			case Target.Material:
			{
				Material material = (Material)target;
				switch (format)
				{
				case Format.Float:
					from = (T)(object)material.GetFloat(property);
					break;
				case Format.Int:
					from = (T)(object)material.GetInt(property);
					break;
				case Format.Color:
					from = (T)(object)material.GetColor(property);
					break;
				case Format.Vector4:
					from = (T)(object)material.GetVector(property);
					break;
				case Format.Vector2:
				case Format.Vector3:
					break;
				}
				break;
			}
			}
		}
	}
	[Serializable]
	public class Tween : Timer
	{
		internal enum Target
		{
			Default = 0,
			Transform = 1,
			Component = 2,
			Material = 3,
			Type = 4
		}

		internal enum Format
		{
			Float = 0,
			Int = 1,
			Vector2 = 2,
			Vector3 = 3,
			Vector4 = 4,
			Color = 5,
			Quaternion = 6,
			Transform = 7,
			Rect = 8
		}

		internal const float DefaultDuration = 0.25f;

		public static Easing Linear = (float r) => r;

		public object target;

		internal Target type;

		public string property;

		public new bool valid;

		public Easing easing;

		public Action<Tween> onComplete;

		public static List<Tween> all
		{
			get
			{
				List<Activity> activities = ActivityManager.instance.activities;
				List<Tween> list = new List<Tween>();
				for (int i = 0; i < activities.Count; i++)
				{
					if (activities[i] is Tween)
					{
						list.Add((Tween)activities[i]);
					}
				}
				return list;
			}
		}

		public static List<Tween> allRunning
		{
			get
			{
				List<Activity> activities = ActivityManager.instance.activities;
				List<Tween> list = new List<Tween>();
				for (int i = 0; i < activities.Count; i++)
				{
					if (activities[i] is Tween)
					{
						Tween tween = (Tween)activities[i];
						if (tween.elapsed >= 0f)
						{
							list.Add(tween);
						}
					}
				}
				return list;
			}
		}

		public static AnimationCurve GetAnimationCurve(Func<float, float> p_easing, int p_keyframes)
		{
			Keyframe[] array = new Keyframe[p_keyframes];
			for (int i = 0; i < array.Length; i++)
			{
				float num = (float)i / (float)(p_keyframes - 1);
				Keyframe keyframe = array[i];
				keyframe.time = num;
				keyframe.value = p_easing(num);
				array[i] = keyframe;
			}
			return new AnimationCurve(array);
		}

		public static AnimationCurve GetAnimationCurve(Func<float, Vector2> p_easing, int p_keyframes)
		{
			Keyframe[] array = new Keyframe[Mathf.Min(p_keyframes, 225)];
			for (int i = 0; i < array.Length; i++)
			{
				float arg = (float)i / (float)(p_keyframes - 1);
				Keyframe keyframe = array[i];
				Vector2 vector = p_easing(arg);
				keyframe.time = vector.x;
				keyframe.value = vector.y;
				array[i] = keyframe;
			}
			return new AnimationCurve(array);
		}

		public static AnimationCurve GetAnimationCurve(Func<float, float> p_easing, float p_bias = 0.05f)
		{
			List<float> list = new List<float>();
			GetAnimationCurveRecursive(list, p_easing, p_bias, 0f, 1f, 0, 500);
			if (list.Count <= 1)
			{
				return AnimationCurve.Linear(0f, 0f, 1f, 1f);
			}
			for (int i = 0; i < list.Count; i++)
			{
				for (int j = i + 1; j < list.Count; j++)
				{
					float num = list[i];
					float num2 = list[j];
					if (Mathf.Abs(num - num2) <= 0.005f)
					{
						list.RemoveAt(j--);
					}
				}
			}
			list.Add(1f);
			if (list.Count > 225)
			{
				List<float> list2 = new List<float>();
				for (int k = 0; k < 225; k++)
				{
					float num3 = (float)k / 224f;
					float num4 = (float)(list.Count - 1) * num3;
					int num5 = (int)num4;
					int index = Mathf.Min(num5 + 1, list.Count - 1);
					num3 = num4 - Mathf.Floor(num4);
					float item = Mathf.Lerp(list[num5], list[index], num3);
					list2.Add(item);
				}
				list = list2;
			}
			Keyframe[] array = new Keyframe[list.Count];
			for (int l = 0; l < array.Length; l++)
			{
				_ = (float)l / (float)(list.Count - 1);
				Keyframe keyframe = array[l];
				keyframe.time = list[l];
				keyframe.value = p_easing(list[l]);
				array[l] = keyframe;
			}
			return new AnimationCurve(array);
		}

		public static AnimationCurve GetBezierAnimationCurve(float[] p_parameters, float p_precision, int p_samples, float p_bias)
		{
			return GetAnimationCurve((float v) => BezierEasing.Sample(v, p_parameters, p_precision, p_samples), p_bias);
		}

		public static AnimationCurve GetBezierAnimationCurve(float[] p_parameters, int p_keyframes)
		{
			return GetAnimationCurve(delegate(float v)
			{
				Vector2 result = new Vector2(v, v);
				BezierEasing.Cubic(v, p_parameters[0], p_parameters[2], out var p_value, out var p_derivative);
				result.x = p_value;
				BezierEasing.Cubic(v, p_parameters[1], p_parameters[3], out p_value, out p_derivative);
				result.y = p_value;
				return result;
			}, p_keyframes);
		}

		private static void GetAnimationCurveRecursive(List<float> p_values, Func<float, float> p_easing, float p_bias, float p_left, float p_right, int p_depth, int p_max_depth)
		{
			float num = p_easing(p_left);
			float num2 = p_easing(p_right);
			if (Mathf.Abs(num - num2) <= p_bias)
			{
				p_values.Add(p_left);
				return;
			}
			if (p_depth >= p_max_depth)
			{
				p_values.Add(p_left);
				return;
			}
			Mathf.Abs(p_left - p_right);
			float num3 = (p_left + p_right) * 0.5f;
			GetAnimationCurveRecursive(p_values, p_easing, p_bias, p_left, num3, p_depth + 1, p_max_depth);
			GetAnimationCurveRecursive(p_values, p_easing, p_bias, num3, p_right, p_depth + 1, p_max_depth);
		}

		public static AnimationCurve GetBezierAnimationCurve(float[] p_parameters, float p_precision, int p_samples, int p_keyframes)
		{
			return GetAnimationCurve((float v) => BezierEasing.Sample(v, p_parameters, p_precision, p_samples), p_keyframes);
		}

		public static Tween Add<T>(object p_target, string p_property, T p_to, float p_duration, float p_delay, Easing p_easing, bool p_threaded)
		{
			Tween tween = null;
			if (typeof(T) == typeof(float))
			{
				tween = new FloatTween(p_target, p_property, (float)(object)p_to, p_duration, p_delay, p_easing, p_threaded);
			}
			else if (typeof(T) == typeof(Color))
			{
				tween = new ColorTween(p_target, p_property, (Color)(object)p_to, p_duration, p_delay, p_easing, p_threaded);
			}
			else if (typeof(T) == typeof(Vector3))
			{
				tween = new Vector3Tween(p_target, p_property, (Vector3)(object)p_to, p_duration, p_delay, p_easing, p_threaded);
			}
			else if (typeof(T) == typeof(Vector2))
			{
				tween = new Vector2Tween(p_target, p_property, (Vector2)(object)p_to, p_duration, p_delay, p_easing, p_threaded);
			}
			else if (typeof(T) == typeof(Quaternion))
			{
				tween = new QuaternionTween(p_target, p_property, (Quaternion)(object)p_to, p_duration, p_delay, p_easing, p_threaded);
			}
			else if (typeof(T) == typeof(Transform))
			{
				tween = new TransformTween(p_target, p_property, (Transform)(object)p_to, p_duration, p_delay, p_easing, p_threaded);
			}
			else if (typeof(T) == typeof(Rect))
			{
				tween = new RectTween(p_target, p_property, (Rect)(object)p_to, p_duration, p_delay, p_easing, p_threaded);
			}
			else if (typeof(T) == typeof(Vector4))
			{
				tween = new Vector4Tween(p_target, p_property, (Vector4)(object)p_to, p_duration, p_delay, p_easing, p_threaded);
			}
			else if (typeof(T) == typeof(int))
			{
				tween = new IntTween(p_target, p_property, (int)(object)p_to, p_duration, p_delay, p_easing, p_threaded);
			}
			if (tween != null)
			{
				tween.Start();
				if (p_delay <= 0f && p_duration <= 0f)
				{
					tween.OnStart();
					tween.OnComplete();
				}
			}
			else
			{
				Debug.LogWarning("Tween> Failed to create tween! type[" + typeof(T).Name + "] target[" + p_target.GetType().Name + "]");
			}
			return tween;
		}

		public static Tween Add<T>(object p_target, string p_property, T p_to, float p_duration, float p_delay, Easing p_easing)
		{
			return Add(p_target, p_property, p_to, p_duration, p_delay, p_easing, p_threaded: false);
		}

		public static Tween Add<T>(object p_target, string p_property, T p_to, float p_duration, Easing p_easing)
		{
			return Add(p_target, p_property, p_to, p_duration, 0f, p_easing, p_threaded: false);
		}

		public static Tween Add<T>(object p_target, string p_property, T p_to, Easing p_easing)
		{
			return Add(p_target, p_property, p_to, 0.25f, 0f, p_easing, p_threaded: false);
		}

		public static void Kill(object p_target = null, string p_property = "")
		{
			List<Activity> activities = ActivityManager.instance.activities;
			for (int i = 0; i < activities.Count; i++)
			{
				if (activities[i] is Tween)
				{
					Tween tween = activities[i] as Tween;
					if ((p_target == null || tween.target == p_target) && (!(p_property != "") || !(tween.property != p_property)))
					{
						tween.Stop();
					}
				}
			}
		}

		public static void KillDuplicates(Tween p_target)
		{
			List<Activity> activities = ActivityManager.instance.activities;
			for (int i = 0; i < activities.Count; i++)
			{
				if (activities[i] is Tween)
				{
					Tween tween = activities[i] as Tween;
					if (p_target != tween && p_target.target == tween.target && !(p_target.property != tween.property) && tween.elapsed >= 0f)
					{
						tween.Stop();
					}
				}
			}
		}

		public Tween(object p_target, string p_property, float p_duration, float p_delay, Easing p_easing, bool p_threaded)
			: base(p_duration, p_delay, 0, p_threaded)
		{
			target = p_target;
			context = p_target;
			property = p_property;
			easing = ((p_easing == null) ? Linear : p_easing);
			type = Target.Default;
			if (target is Transform)
			{
				type = Target.Transform;
			}
			else if (target is Component)
			{
				type = Target.Component;
			}
			else if (target is Material)
			{
				type = Target.Material;
			}
			else if (target is Type)
			{
				type = Target.Type;
			}
			valid = true;
		}

		public Tween(object p_target, string p_property, float p_duration, float p_delay, Easing p_easing)
			: this(p_target, p_property, p_duration, p_delay, p_easing, p_threaded: false)
		{
		}

		public Tween(object p_target, string p_property, float p_duration, Easing p_easing)
			: this(p_target, p_property, p_duration, 0f, p_easing, p_threaded: false)
		{
		}

		public Tween(object p_target, string p_property, Easing p_easing)
			: this(p_target, p_property, 0.25f, 0f, p_easing, p_threaded: false)
		{
		}

		protected override void OnStart()
		{
			bool flag = target != null && (!(target is Component) || (bool)(target as Component));
			bool flag2 = false;
			switch (type)
			{
			case Target.Default:
			case Target.Transform:
			case Target.Component:
				flag2 = Reflection<object>.Contains(target, property);
				break;
			case Target.Type:
				flag2 = Reflection<object>.ContainsStatic((Type)target, property);
				break;
			case Target.Material:
				flag2 = ((Material)target).HasProperty(property);
				break;
			}
			valid = flag2 && flag;
			if (!valid)
			{
				Stop();
				Debug.LogWarning(GetType().Name + "> Invalid! instance[" + flag + "] property[" + property + "][" + flag2 + "]");
			}
		}

		protected override void OnExecute()
		{
			if (target != null && (!(target is Component) || (bool)(target as Component)))
			{
				Lerp(base.progress * 0.999f);
			}
			base.OnExecute();
		}

		protected override void OnComplete()
		{
			if (target != null && (!(target is Component) || (bool)(target as Component)))
			{
				Lerp(1f);
			}
			if (onComplete != null)
			{
				onComplete(this);
			}
		}

		public virtual void Lerp(float p_ratio)
		{
		}
	}
}

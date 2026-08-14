using System;
using System.Collections.Generic;
using UnityEngine;

namespace thelab.mvc
{
	public class AnimationView : NotificationView
	{
		[Serializable]
		public class Argument
		{
			public enum Type
			{
				Int = 0,
				Float = 1,
				String = 2,
				Curve = 3,
				Vector2 = 4,
				Vector3 = 5,
				Vector4 = 6,
				Rect = 7,
				Color = 8,
				Object = 9
			}

			public Type type;

			public int aInt;

			public float aFloat;

			public string aString;

			public AnimationCurve aCurve;

			public Vector2 aVector2;

			public Vector3 aVector3;

			public Vector4 aVector4;

			public Rect aRect;

			public Color aColor;

			public UnityEngine.Object aObject;
		}

		[Serializable]
		public class Event
		{
			public AnimationClip clip;

			public List<Callback> callbacks;

			[NonSerialized]
			private AnimationView m_view;

			public AnimationView view => m_view;

			internal void Init(AnimationView p_view)
			{
				m_view = p_view;
				for (int i = 0; i < callbacks.Count; i++)
				{
					callbacks[i].Init(this);
				}
			}

			internal void Update()
			{
				if (!(clip == null) && !(view == null) && !(view.animation == null))
				{
					for (int i = 0; i < callbacks.Count; i++)
					{
						callbacks[i].Update();
					}
				}
			}
		}

		[Serializable]
		public struct Interval
		{
			public float min;

			public float max;
		}

		[Serializable]
		public class Callback
		{
			[NonSerialized]
			private Event parent;

			public string notification;

			public Interval interval;

			public bool useFrame = true;

			public bool active;

			public bool continuous;

			public List<Argument> args;

			internal float m_last_time;

			public AnimationState state;

			public float time
			{
				get
				{
					AnimationClip clip = parent.clip;
					AnimationState animationState = parent.view.animation[clip.name];
					return (animationState.normalizedTime - Mathf.Floor(animationState.normalizedTime)) * animationState.length;
				}
			}

			public int frame
			{
				get
				{
					AnimationClip clip = parent.clip;
					return (int)(time * clip.frameRate);
				}
			}

			public float progress
			{
				get
				{
					float num = Mathf.Abs(interval.max - interval.min);
					float num2 = 1f;
					if (useFrame)
					{
						float num3 = frame;
						num2 = ((num <= 0f) ? 1f : ((num3 - interval.min) / num));
					}
					else
					{
						num2 = ((num <= 0f) ? 1f : ((time - interval.min) / num));
					}
					return Mathf.Clamp01(num2);
				}
			}

			internal void Init(Event p_event)
			{
				parent = p_event;
				if (!(parent.clip == null) && !(parent.view.animation == null))
				{
					m_last_time = time;
				}
			}

			internal void Update()
			{
				AnimationClip clip = parent.clip;
				Animation animation = parent.view.animation;
				if (!animation.IsPlaying(clip.name))
				{
					return;
				}
				state = animation[clip.name];
				float num = m_last_time;
				float num2 = (m_last_time = time);
				if (useFrame)
				{
					num = Mathf.Floor(num * clip.frameRate);
					num2 = Mathf.Floor(num2 * clip.frameRate);
				}
				float min = interval.min;
				float max = interval.max;
				bool flag = false;
				flag = (num2 >= min && num2 < max) || (num >= min && num < max);
				if (!flag)
				{
					flag = num2 >= max && num <= min;
				}
				if (flag)
				{
					if ((!continuous || !(Mathf.Abs(interval.max - interval.min) > 0f)) && active)
					{
						return;
					}
					active = true;
					parent.view.callback = this;
					if (string.IsNullOrEmpty(notification))
					{
						parent.view.OnAnimationEvent(args);
						return;
					}
					string text = parent.view.notification;
					if (!string.IsNullOrEmpty(text))
					{
						parent.view.OnAnimationEvent(text + "." + notification, args);
					}
				}
				else
				{
					active = false;
				}
			}
		}

		public List<Event> events;

		public Callback callback;

		internal Animation animation => AssertLocal<Animation>("animation");

		private void Awake()
		{
			for (int i = 0; i < events.Count; i++)
			{
				events[i].Init(this);
			}
		}

		public void OnAnimationEvent(string p_event, List<Argument> p_args)
		{
			object[] array = new object[p_args.Count];
			for (int i = 0; i < p_args.Count; i++)
			{
				Argument argument = p_args[i];
				switch (argument.type)
				{
				case Argument.Type.Int:
					array[i] = argument.aInt;
					break;
				case Argument.Type.Float:
					array[i] = argument.aFloat;
					break;
				case Argument.Type.String:
					array[i] = argument.aString;
					break;
				case Argument.Type.Curve:
					array[i] = argument.aCurve;
					break;
				case Argument.Type.Vector2:
					array[i] = argument.aVector2;
					break;
				case Argument.Type.Vector3:
					array[i] = argument.aVector3;
					break;
				case Argument.Type.Vector4:
					array[i] = argument.aVector4;
					break;
				case Argument.Type.Rect:
					array[i] = argument.aRect;
					break;
				case Argument.Type.Color:
					array[i] = argument.aColor;
					break;
				case Argument.Type.Object:
					array[i] = argument.aObject;
					break;
				default:
					array[i] = null;
					break;
				}
			}
			Notify(p_event, array);
		}

		public void OnAnimationEvent(List<Argument> p_args)
		{
			OnAnimationEvent(notification, p_args);
		}

		private void Update()
		{
			for (int i = 0; i < events.Count; i++)
			{
				events[i].Update();
			}
		}
	}
}

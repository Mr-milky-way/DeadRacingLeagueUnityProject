using System;
using UnityEngine;

namespace thelab.core
{
	[Serializable]
	public class Timer : Activity
	{
		public Action<Timer> OnTimerStep;

		public Action<Timer> OnTimerComplete;

		public float duration { get; set; }

		public int step { get; set; }

		public int count { get; set; }

		public float progress
		{
			get
			{
				float num = ((!(duration <= 0f)) ? (base.elapsed / duration) : ((!base.running) ? 0f : ((base.elapsed >= 0f) ? 1f : 0f)));
				if (!(num < 0f))
				{
					if (!(num > 1f))
					{
						return num;
					}
					return 1f;
				}
				return 0f;
			}
		}

		public static Timer Run(Action<int> p_callback, float p_duration, float p_delay = 0f, int p_steps = 0, bool p_threaded = false)
		{
			Timer timer = new Timer(p_duration, p_delay, p_steps, p_threaded);
			timer.OnTimerStep = delegate(Timer t)
			{
				if (p_callback != null)
				{
					p_callback(t.step);
				}
			};
			timer.Start();
			return timer;
		}

		public static Timer Invoke(object p_target, string p_method, float p_delay, params object[] p_args)
		{
			Timer timer = new Timer(p_delay);
			timer.OnTimerComplete = delegate
			{
				if (p_target != null)
				{
					Reflection<object>.Invoke(p_target, p_method, p_args);
				}
			};
			timer.Start();
			return timer;
		}

		public static Timer InvokeThread(object p_target, string p_method, float p_delay, params object[] p_args)
		{
			Timer timer = new Timer(p_delay, 0f, 0, p_threaded: true);
			timer.OnTimerComplete = delegate
			{
				if (p_target != null)
				{
					Reflection<object>.Invoke(p_target, p_method, p_args);
				}
			};
			timer.Start();
			return timer;
		}

		public static Timer Set(object p_target, string p_property, float p_delay, object p_value)
		{
			Timer timer = new Timer(p_delay);
			timer.OnTimerComplete = delegate
			{
				if (p_target is UnityEngine.Object)
				{
					UnityEngine.Object obj = p_target as UnityEngine.Object;
					if ((bool)obj)
					{
						Reflection<object>.Set(obj, p_property, p_value);
					}
				}
				else if (p_target != null)
				{
					Reflection<object>.Set(p_target, p_property, p_value);
				}
			};
			timer.Start();
			return timer;
		}

		public static Timer Get<T>(object p_target, string p_property, float p_delay, Action<T> p_callback)
		{
			Timer timer = new Timer(p_delay);
			timer.OnTimerComplete = delegate
			{
				if (p_target != null && p_callback != null)
				{
					p_callback(Reflection<object>.Get<T>(p_target, p_property));
				}
			};
			timer.Start();
			return timer;
		}

		public Timer(float p_duration, float p_delay = 0f, int p_steps = 0, bool p_threaded = false)
			: base(p_delay, p_threaded)
		{
			duration = p_duration;
			step = 0;
			count = p_steps;
		}

		protected override void OnExecute()
		{
			if (!(base.elapsed >= duration))
			{
				return;
			}
			base.elapsed = duration;
			if (OnTimerStep != null)
			{
				OnTimerStep(this);
			}
			step++;
			if (step >= count)
			{
				OnComplete();
				if (OnTimerComplete != null)
				{
					OnTimerComplete(this);
				}
				Stop();
			}
			else
			{
				base.elapsed = 0f;
				OnStep();
			}
		}

		protected virtual void OnStep()
		{
		}

		protected virtual void OnComplete()
		{
		}
	}
}

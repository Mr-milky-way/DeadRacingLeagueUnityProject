using System;
using UnityEngine;

namespace thelab.core
{
	public static class TimerComponentExt
	{
		public static Activity TimerRun(this Component v, Action<Activity> a, float p_delay = 0f)
		{
			Activity activity = Activity.Run(a, p_delay);
			activity.context = v;
			return activity;
		}

		public static Activity TimerRun(this Component v, Func<bool> a, float p_delay = 0f)
		{
			Activity activity = Activity.Run(a, p_delay);
			activity.context = v;
			return activity;
		}

		public static Activity TimerRun(this Component v, Predicate<float> a, float p_delay = 0f)
		{
			Activity activity = Activity.Run(a, p_delay);
			activity.context = v;
			return activity;
		}

		public static Activity TimerRun(this Component v, Action a, float p_duration, float p_delay = 0f)
		{
			Activity activity = Activity.Run(a, p_duration, p_delay);
			activity.context = v;
			return activity;
		}

		public static Activity TimerRunOnce(this Component v, Action a, float p_delay = 0f)
		{
			Activity activity = Activity.RunOnce(a, p_delay);
			activity.context = v;
			return activity;
		}

		public static Activity TimerRun(this Component v, Action<int> a, float p_duration, float p_delay = 0f, int p_steps = 0)
		{
			Timer timer = Timer.Run(a, p_duration, p_delay, p_steps);
			timer.context = v;
			return timer;
		}
	}
}

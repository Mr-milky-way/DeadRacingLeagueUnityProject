using System;
using UnityEngine;

namespace thelab.core
{
	public static class ActivityComponentExt
	{
		public static Activity ActivityRun(this Component v, Action<Activity> a, float p_delay = 0f)
		{
			Activity activity = Activity.Run(a, p_delay);
			activity.context = v;
			return activity;
		}

		public static Activity ActivityRun(this Component v, Func<bool> a, float p_delay = 0f)
		{
			Activity activity = Activity.Run(a, p_delay);
			activity.context = v;
			return activity;
		}

		public static Activity ActivityRun(this Component v, Predicate<float> a, float p_delay = 0f)
		{
			Activity activity = Activity.Run(a, p_delay);
			activity.context = v;
			return activity;
		}

		public static Activity ActivityRun(this Component v, Action a, float p_duration, float p_delay = 0f)
		{
			Activity activity = Activity.Run(a, p_duration, p_delay);
			activity.context = v;
			return activity;
		}

		public static Activity ActivityRunOnce(this Component v, Action a, float p_delay = 0f)
		{
			Activity activity = Activity.RunOnce(a, p_delay);
			activity.context = v;
			return activity;
		}
	}
}

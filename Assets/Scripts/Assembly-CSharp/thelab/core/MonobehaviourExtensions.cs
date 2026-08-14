using System;
using UnityEngine;

namespace thelab.core
{
	public static class MonobehaviourExtensions
	{
		public static MonoActivity MonoActivityRun(this MonoBehaviour mb, Predicate<float> callback, float delay, bool unscaledTime = false)
		{
			MonoActivity a = new MonoActivity(unscaledTime);
			a.Delay = delay;
			a.OnExecute = delegate(MonoActivity activity)
			{
				if (callback == null || !callback(activity.Elapsed))
				{
					a.Stop();
				}
			};
			a.Parent = mb;
			if (mb != null)
			{
				mb.StartCoroutine(a.RunExecution());
			}
			return a;
		}

		public static MonoActivity MonoActivityRun(this MonoBehaviour mb, Func<bool> callback, float delay, bool unscaledTime = false)
		{
			MonoActivity a = new MonoActivity(unscaledTime);
			a.Delay = delay;
			a.OnExecute = delegate
			{
				if (callback == null || !callback())
				{
					a.Stop();
				}
			};
			a.Parent = mb;
			if (mb != null)
			{
				mb.StartCoroutine(a.RunExecution());
			}
			return a;
		}

		public static MonoActivity MonoActivityRunOnce(this MonoBehaviour mb, Action callback, float delay, bool unscaledTime = false)
		{
			MonoActivity a = new MonoActivity(unscaledTime);
			a.Delay = delay;
			a.OnExecute = delegate
			{
				a.Stop();
				callback?.Invoke();
			};
			a.Parent = mb;
			if (mb != null)
			{
				mb.StartCoroutine(a.RunExecution());
			}
			return a;
		}
	}
}

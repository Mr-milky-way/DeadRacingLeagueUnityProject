using System;
using System.Collections;
using UnityEngine;

namespace thelab.core
{
	public class MonoActivity
	{
		private bool isRunning;

		public Action<MonoActivity> OnExecute;

		public Action<MonoActivity> OnFinished;

		public MonoBehaviour Parent;

		private bool canLoop;

		public float Delay { get; set; }

		public float Elapsed { get; private set; }

		public bool IsRunning
		{
			get
			{
				if (isRunning && Parent != null)
				{
					return Parent.isActiveAndEnabled;
				}
				return false;
			}
		}

		public bool UnscaledTime { get; private set; }

		public MonoActivity(bool unscaledTime = false)
		{
			UnscaledTime = unscaledTime;
		}

		public void Stop()
		{
			canLoop = false;
		}

		public IEnumerator RunExecution()
		{
			canLoop = true;
			if (UnscaledTime)
			{
				yield return new WaitForSecondsRealtime(Delay);
			}
			else
			{
				yield return new WaitForSeconds(Delay);
			}
			while (canLoop && Parent != null)
			{
				isRunning = true;
				Elapsed += (UnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
				OnExecute?.Invoke(this);
				yield return null;
			}
			isRunning = false;
			OnFinished?.Invoke(this);
		}
	}
}

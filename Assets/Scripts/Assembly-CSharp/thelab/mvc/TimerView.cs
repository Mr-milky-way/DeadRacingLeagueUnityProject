using UnityEngine;

namespace thelab.mvc
{
	public class TimerView : NotificationView
	{
		public bool scale = true;

		public bool active = true;

		public float duration;

		public int count;

		public float elapsed;

		public int step;

		public void Restart()
		{
			elapsed = 0f;
			step = 0;
		}

		public void Play()
		{
			active = true;
		}

		public void Stop()
		{
			active = false;
			Restart();
		}

		private void Update()
		{
			if (!active)
			{
				return;
			}
			elapsed += (scale ? Time.deltaTime : Time.unscaledDeltaTime);
			if (elapsed >= duration)
			{
				elapsed = 0f;
				Notify(notification + "@timer.step");
				step++;
				if (step >= count)
				{
					Notify(notification + "@timer.complete");
					active = false;
				}
			}
		}
	}
}

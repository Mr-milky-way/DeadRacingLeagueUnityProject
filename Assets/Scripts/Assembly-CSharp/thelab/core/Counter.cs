using UnityEngine;

namespace thelab.core
{
	public class Counter : MonoBehaviour
	{
		public enum Mode
		{
			Count = 0,
			Time = 1,
			CountLimit = 2,
			TimeLimit = 3
		}

		public Mode mode;

		public bool completed;

		public bool paused;

		public float count;

		public float step = 1f;

		public float min = float.NegativeInfinity;

		public float max = float.PositiveInfinity;

		protected void Awake()
		{
		}

		public void Step()
		{
			if (!paused)
			{
				float num = 1f;
				if (mode == Mode.Time)
				{
					num = Time.unscaledDeltaTime;
				}
				if (mode == Mode.TimeLimit)
				{
					num = Time.unscaledDeltaTime;
				}
				float num2 = step * num;
				count += num2;
				count = Mathf.Clamp(count, min, max);
			}
		}

		public void SetCount(float v)
		{
			count = v;
		}

		protected void Update()
		{
			switch (mode)
			{
			case Mode.Time:
				Step();
				break;
			case Mode.TimeLimit:
				Step();
				break;
			}
			completed = false;
			if (mode == Mode.CountLimit)
			{
				completed = count > min && count < max;
			}
			if (mode == Mode.TimeLimit)
			{
				completed = count > min && count < max;
			}
		}
	}
}

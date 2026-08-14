using System;

namespace thelab.core
{
	[Serializable]
	public class LevelEvent
	{
		public LevelEventType type;

		public LevelManager target;

		public string name;

		public float progress;

		public LevelEvent(string p_name, LevelEventType p_type, LevelManager p_target, float p_progress = 0f)
		{
			name = p_name;
			type = p_type;
			target = p_target;
			progress = p_progress;
		}
	}
}

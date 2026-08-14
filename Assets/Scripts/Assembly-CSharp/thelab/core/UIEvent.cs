using System;

namespace thelab.core
{
	[Serializable]
	public class UIEvent
	{
		public UIEventType type;

		public EventComponent target;
	}
}

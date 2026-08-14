using System;
using UnityEngine.Events;

namespace thelab.core
{
	[Serializable]
	public class ListEvent : UnityEvent<ListEventData>
	{
		public enum Type
		{
			Added = 0,
			Removed = 1,
			Filter = 2,
			Sort = 3,
			Layout = 4
		}
	}
}

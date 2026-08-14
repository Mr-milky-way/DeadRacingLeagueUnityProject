using System;
using UnityEngine;

namespace thelab.core
{
	[Serializable]
	public class ListEventData
	{
		public ListEvent.Type type;

		public BaseListComponent target;

		public GameObject item;
	}
}

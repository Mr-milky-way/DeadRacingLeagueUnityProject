using System;
using UnityEngine.Events;

namespace thelab.core
{
	[Serializable]
	public class AsyncRequestCallback : UnityEvent<AsyncRequestEvent>
	{
	}
}

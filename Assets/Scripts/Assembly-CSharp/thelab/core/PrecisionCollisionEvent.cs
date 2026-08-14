using System;

namespace thelab.core
{
	[Serializable]
	public class PrecisionCollisionEvent
	{
		public PrecisionCollisionEventType type;

		public PrecisionCollider target;

		public object[] args;
	}
}

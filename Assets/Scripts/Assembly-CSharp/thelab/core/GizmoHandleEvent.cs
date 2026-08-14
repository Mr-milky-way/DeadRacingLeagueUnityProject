using System;

namespace thelab.core
{
	[Serializable]
	public class GizmoHandleEvent
	{
		public HandleEventType type;

		public GizmoHandle target;

		public GizmoHandleEvent()
		{
			type = HandleEventType.None;
		}

		public GizmoHandleEvent(HandleEventType p_type, GizmoHandle p_target)
		{
			type = p_type;
			target = p_target;
		}
	}
}

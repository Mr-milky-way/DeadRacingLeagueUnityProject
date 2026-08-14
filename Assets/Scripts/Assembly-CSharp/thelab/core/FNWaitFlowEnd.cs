using System;

namespace thelab.core
{
	[Serializable]
	public class FNWaitFlowEnd : FlowNode
	{
		public Flow m_targetFlow;

		internal override bool hasContent => true;

		internal override FlowStatus OnUpdate()
		{
			if (m_targetFlow.complete)
			{
				return FlowStatus.Complete;
			}
			return FlowStatus.Running;
		}
	}
}

using System;

namespace thelab.core
{
	[Serializable]
	public class FNMethod : FlowNode
	{
		public SerializedMethod method;

		internal override bool hasContent
		{
			get
			{
				if (method != null)
				{
					return method.target;
				}
				return false;
			}
		}

		internal override FlowStatus OnUpdate()
		{
			if (method != null)
			{
				method.Invoke();
			}
			return FlowStatus.Complete;
		}
	}
}

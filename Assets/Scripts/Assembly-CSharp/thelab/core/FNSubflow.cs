using System;

namespace thelab.core
{
	[Serializable]
	public class FNSubflow : FlowNode
	{
		public enum Action
		{
			Start = 0,
			Stop = 1,
			Restart = 2
		}

		public Action action;

		internal override bool hasContent => true;

		internal override FlowStatus OnUpdate()
		{
			for (int i = 0; i < base.transform.childCount; i++)
			{
				Flow component = base.transform.GetChild(i).GetComponent<Flow>();
				if (component != null)
				{
					switch (action)
					{
					case Action.Start:
						component.Run();
						break;
					case Action.Restart:
						component.Restart();
						break;
					case Action.Stop:
						component.Stop();
						break;
					}
				}
			}
			return FlowStatus.Complete;
		}
	}
}

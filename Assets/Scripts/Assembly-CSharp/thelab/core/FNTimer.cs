using System;
using UnityEngine;
using drl.sim;
using drl.sim.rci;

namespace thelab.core
{
	[Serializable]
	public class FNTimer : FlowNode
	{
		public float elapsed;

		public float timeout;

		public bool skippable = true;

		internal override bool hasContent => false;

		internal override void OnInitialize()
		{
			elapsed = 0f;
		}

		internal override FlowStatus OnUpdate()
		{
			if (RCI.GetAnyButtonUp() && skippable)
			{
				return FlowStatus.Complete;
			}
			elapsed += Time.deltaTime;
			if (timeout > 0f && elapsed >= timeout)
			{
				elapsed = timeout;
				return FlowStatus.Complete;
			}
			return FlowStatus.Running;
		}

		public override FlowStatus OnSkip()
		{
			int num = flow.nodes.IndexOf(this);
			num--;
			if (num >= 0)
			{
				FNSimulationModule fNSimulationModule = flow.nodes[num] as FNSimulationModule;
				if ((bool)fNSimulationModule && fNSimulationModule.mode == FNSimulationModule.Mode.CameraMove)
				{
					fNSimulationModule.OnSkip();
				}
			}
			return FlowStatus.Complete;
		}
	}
}

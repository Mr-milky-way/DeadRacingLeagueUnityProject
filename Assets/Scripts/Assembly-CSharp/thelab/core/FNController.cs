using System;
using drl.sim;

namespace thelab.core
{
	[Serializable]
	public class FNController : FlowNode
	{
		public enum StickIcon
		{
			leftStick_up = 0,
			leftStick_down = 1,
			leftStick_left = 2,
			leftStick_right = 3,
			rightStick_up = 4,
			rightStick_down = 5,
			rightStick_left = 6,
			rightStick_right = 7,
			throttle = 8,
			yaw = 9,
			pitch = 10,
			roll = 11
		}

		private SimulationFlowModule m_module;

		public StickIcon stickIcon;

		public SimulationFlowModule module
		{
			get
			{
				if (!m_module)
				{
					return Hierarchy.FindReverse<SimulationFlowModule>(base.transform);
				}
				return m_module;
			}
		}

		internal override FlowStatus OnUpdate()
		{
			if (!module)
			{
				return FlowStatus.Fail;
			}
			((DebugFlowModuleUI)module.ui).ShowControllerIcon(stickIcon);
			return FlowStatus.Complete;
		}
	}
}

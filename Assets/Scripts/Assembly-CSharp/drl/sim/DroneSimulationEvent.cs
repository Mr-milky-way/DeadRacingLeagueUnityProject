using System;

namespace drl.sim
{
	[Serializable]
	public class DroneSimulationEvent
	{
		public DroneSimulationEventType type;

		public DroneSimulation target;

		public object[] args;
	}
}

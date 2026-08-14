using System;

namespace drl.sim
{
	[Serializable]
	public class DroneEvent
	{
		public DroneEventType type;

		public Drone target;
	}
}

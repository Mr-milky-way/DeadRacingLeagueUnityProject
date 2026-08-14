using System;
using UnityEngine.Events;

namespace drl.sim
{
	[Serializable]
	public class DroneSimulationEventCallback : UnityEvent<DroneSimulationEvent>
	{
	}
}

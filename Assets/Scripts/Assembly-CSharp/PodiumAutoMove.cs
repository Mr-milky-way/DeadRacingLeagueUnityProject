using UnityEngine;
using drl.sim;
using thelab.core;

public class PodiumAutoMove : MonoBehaviour
{
	private DroneSimulation simulation;

	private Drone drone;

	private void OnEnable()
	{
		simulation = Hierarchy.FindReverse<DroneSimulation>(base.transform);
		PodiumMoveTrigger.OnTriggerPodiumMove += PodiumMoveTrigger_OnTriggerPodiumMove;
	}

	private void OnDisable()
	{
		PodiumMoveTrigger.OnTriggerPodiumMove -= PodiumMoveTrigger_OnTriggerPodiumMove;
	}

	private void OnDestroy()
	{
		PodiumMoveTrigger.OnTriggerPodiumMove -= PodiumMoveTrigger_OnTriggerPodiumMove;
	}

	private void PodiumMoveTrigger_OnTriggerPodiumMove(Transform t)
	{
		drone = simulation.drones.Get(0);
		Quaternion rotation = t.rotation;
		if (drone != null)
		{
			rotation = Quaternion.Euler(0f, drone.transform.eulerAngles.y, 0f);
		}
		base.transform.position = t.position;
		base.transform.rotation = rotation;
	}
}

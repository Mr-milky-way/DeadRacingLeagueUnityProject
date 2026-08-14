using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PodiumMoveTrigger : MonoBehaviour
{
	public static event Action<Transform> OnTriggerPodiumMove;

	public void TriggerPodiumMove(Transform t)
	{
		if (PodiumMoveTrigger.OnTriggerPodiumMove != null)
		{
			PodiumMoveTrigger.OnTriggerPodiumMove(t);
		}
	}

	public void TriggerPodiumMove()
	{
		if (PodiumMoveTrigger.OnTriggerPodiumMove != null)
		{
			PodiumMoveTrigger.OnTriggerPodiumMove(base.transform);
		}
	}

	private void OnTriggerEnter()
	{
		TriggerPodiumMove(base.transform);
	}
}

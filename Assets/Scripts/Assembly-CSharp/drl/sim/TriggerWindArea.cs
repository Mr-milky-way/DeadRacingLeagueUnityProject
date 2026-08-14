using UnityEngine;

namespace drl.sim
{
	public class TriggerWindArea : MonoBehaviour
	{
		public Vector3 velocity;

		public void OnTriggerEnter(Collider p_collider)
		{
			Drone drone = p_collider.GetComponent<Drone>();
			if (drone == null)
			{
				drone = p_collider.GetComponentInParent<Drone>();
			}
			if (drone != null)
			{
				drone.wind = velocity;
			}
		}

		public void OnTriggerExit(Collider p_collider)
		{
			Drone drone = p_collider.GetComponent<Drone>();
			if (drone == null)
			{
				drone = p_collider.GetComponentInParent<Drone>();
			}
			if (drone != null)
			{
				drone.wind = Vector3.zero;
			}
		}
	}
}

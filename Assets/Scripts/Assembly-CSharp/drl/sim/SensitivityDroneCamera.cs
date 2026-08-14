using UnityEngine;

namespace drl.sim
{
	public class SensitivityDroneCamera : MonoBehaviour
	{
		[SerializeField]
		private Transform droneParent;

		[SerializeField]
		private DroneRigidbody droneRigidbody;

		[SerializeField]
		private Vector3 cameraPosition;

		private void Update()
		{
			if (droneRigidbody == null)
			{
				droneRigidbody = droneParent.GetComponentInChildren<DroneRigidbody>();
			}
			else
			{
				base.transform.position = cameraPosition + droneRigidbody.transform.position;
			}
		}
	}
}

using UnityEngine;

namespace UnitySampleAssetsModified
{
	public class Camera2DFollow : MonoBehaviour
	{
		public Transform target;

		public float damping = 1f;

		public float lookAheadFactor = 3f;

		public float lookAheadReturnSpeed = 0.5f;

		public float lookAheadMoveThreshold = 0.1f;

		private Vector3 offsetY;

		private float offsetZ;

		private Vector3 lastTargetPosition;

		private Vector3 currentVelocity;

		private Vector3 lookAheadPos;

		private void Awake()
		{
			lastTargetPosition = target.position;
			offsetY = base.transform.position - target.position;
			offsetY.x = 0f;
			offsetY.z = 0f;
			offsetZ = (base.transform.position - target.position).z;
			base.transform.parent = null;
		}

		private void Update()
		{
			float x = (target.position - lastTargetPosition).x;
			if (Mathf.Abs(x) > lookAheadMoveThreshold)
			{
				lookAheadPos = lookAheadFactor * Vector3.right * Mathf.Sign(x);
			}
			else
			{
				lookAheadPos = Vector3.MoveTowards(lookAheadPos, Vector3.zero, Time.deltaTime * lookAheadReturnSpeed);
			}
			Vector3 vector = target.position + lookAheadPos + Vector3.forward * offsetZ + offsetY;
			Vector3 position = Vector3.SmoothDamp(base.transform.position, vector, ref currentVelocity, damping);
			base.transform.position = position;
			lastTargetPosition = target.position;
		}
	}
}

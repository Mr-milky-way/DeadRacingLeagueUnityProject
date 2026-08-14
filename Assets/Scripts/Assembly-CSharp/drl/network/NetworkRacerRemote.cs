using UnityEngine;

namespace drl.network
{
	public class NetworkRacerRemote : NetworkRacer
	{
		[Header("Teleport")]
		[SerializeField]
		private float teleportMovingDistance = 400f;

		[SerializeField]
		private float teleportStaticDistance = 25f;

		[SerializeField]
		private float currentTeleportThreshold;

		[Header("Deviation")]
		[SerializeField]
		private float staticDeviation = 2f;

		[SerializeField]
		private float movingDeviation = 15f;

		[SerializeField]
		private float currentDeviation;

		private int m_initialSampleDrop = 15;

		private Vector3 previousPosition;

		public static NetworkRacerRemote Create(NetworkActor remoteActor, INetworkObservable observedObject, NetworkRoom room)
		{
			NetworkRacerRemote networkRacerRemote = observedObject.gameObject.GetComponent<NetworkRacerRemote>();
			if (networkRacerRemote == null)
			{
				networkRacerRemote = observedObject.gameObject.AddComponent<NetworkRacerRemote>();
			}
			networkRacerRemote.SetRacer(remoteActor, room);
			observedObject.NetworkObject = networkRacerRemote;
			return networkRacerRemote;
		}

		protected void LateUpdate()
		{
			if (base.Observed == null || !(base.Observed.NetworkTransform != null) || !base.Observed.CanSync || base.room == null)
			{
				return;
			}
			if (!base.Observed.NetworkRigidbody.isKinematic)
			{
				base.Observed.NetworkRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
				base.Observed.NetworkRigidbody.isKinematic = true;
			}
			Vector3 vector = (NetworkRacer.interpolationV2 ? networkPosition : base.Observed.NetworkTransform.localPosition);
			bool flag = (vector - previousPosition).sqrMagnitude < 0.0001f;
			if (!NetworkRacer.interpolationV2)
			{
				currentDeviation = (flag ? staticDeviation : movingDeviation);
				positionController.Deviation = currentDeviation;
			}
			Vector3 vector2 = ((interpolate && !NetworkRacer.interpolationV2) ? positionController.UpdateValue(vector) : vector);
			if (m_initialSampleDrop > 0 && vector2 == Vector3.zero)
			{
				m_initialSampleDrop--;
				return;
			}
			float squareDeltaPosition = (NetworkRacer.interpolationV2 ? (vector - previousPosition).sqrMagnitude : (vector2 - vector).sqrMagnitude);
			currentTeleportThreshold = (flag ? teleportStaticDistance : teleportMovingDistance);
			if (!IsTeleporting(squareDeltaPosition, currentTeleportThreshold))
			{
				float maxDistanceDelta = Vector3.Distance(vector2, vector) / (float)PhotonNetwork.sendRateOnSerialize;
				vector2 = Vector3.MoveTowards(vector, vector2, maxDistanceDelta);
			}
			if (syncEnabled && interpolate)
			{
				float t = ((!NetworkRacer.interpolationV2) ? 1f : ((syncEasing <= 1E-07f) ? 1f : (Time.deltaTime / syncEasing)));
				base.Observed.NetworkTransform.localPosition = Vector3.Lerp(base.Observed.NetworkTransform.localPosition, vector2, t);
				base.Observed.NetworkTransform.localRotation = (NetworkRacer.interpolationV2 ? networkRotation : rotationController.GetRotation(base.Observed.NetworkTransform.localRotation));
				base.Observed.PackedInputAndRPM = inputAndRPMInterpolator.UpdateValue(base.Observed.PackedInputAndRPM);
			}
			previousPosition = vector;
		}

		private bool IsTeleporting(float squareDeltaPosition, float squareThreshold)
		{
			if (!interpolate)
			{
				return true;
			}
			if (squareDeltaPosition > squareThreshold)
			{
				positionController.Clear();
				base.Observed.OnTeleport(squareDeltaPosition);
				return true;
			}
			return false;
		}
	}
}

using UnityEngine;
using drl.sim;

namespace drl.network
{
	public class PhotonDebugDrone : MonoBehaviour, INetworkObservable
	{
		[Range(-10f, 10f)]
		[SerializeField]
		private float throttle;

		[Range(-10f, 10f)]
		[SerializeField]
		private float pitch;

		[Range(-10f, 10f)]
		[SerializeField]
		private float roll;

		[Range(-10f, 10f)]
		[SerializeField]
		private float yaw;

		[SerializeField]
		private float[] localRPMs = new float[4];

		[SerializeField]
		private float[] networkRPMs;

		[SerializeField]
		private Rigidbody rb;

		[SerializeField]
		private long packedInputAndRPM;

		public Transform NetworkTransform => base.transform;

		public float LeftStickX
		{
			get
			{
				return throttle;
			}
			set
			{
				throttle = value;
			}
		}

		public float LeftStickY
		{
			get
			{
				return pitch;
			}
			set
			{
				pitch = value;
			}
		}

		public float RighStickX
		{
			get
			{
				return roll;
			}
			set
			{
				roll = value;
			}
		}

		public float RightStickY
		{
			get
			{
				return yaw;
			}
			set
			{
				yaw = value;
			}
		}

		public float[] NetworkRPMs
		{
			get
			{
				return networkRPMs;
			}
			set
			{
				networkRPMs = value;
			}
		}

		public Rigidbody NetworkRigidbody => rb;

		public DroneBatteryPowerData BatteryPowerData { get; } = new DroneBatteryPowerData();

		public bool IsReady
		{
			get
			{
				if (base.transform != null)
				{
					return rb != null;
				}
				return false;
			}
		}

		public INetworkObject NetworkObject { get; set; }

		public bool CanSync => true;

		public long PackedInputAndRPM
		{
			get
			{
				return packedInputAndRPM;
			}
			set
			{
				packedInputAndRPM = value;
			}
		}

		GameObject INetworkObservable.gameObject => base.gameObject;

		private void Awake()
		{
			rb = base.gameObject.AddComponent<Rigidbody>();
			rb.isKinematic = true;
			rb.useGravity = false;
		}

		public void OnTeleport(float squaredDeltaDistance)
		{
		}
	}
}

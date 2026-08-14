using UnityEngine;

namespace drl.sim
{
	public class DSGyro : DroneSensor
	{
		[SerializeField]
		private Vector3 m_local;

		[SerializeField]
		private Vector3 m_delta;

		[SerializeField]
		private Vector3 m_velocity;

		[SerializeField]
		private Vector3 m_averageVelocity;

		public float flipThreshold = 50f;

		public bool flipped;

		public Vector3 spin;

		private Quaternion lastRotation = Quaternion.identity;

		public Vector3 local
		{
			get
			{
				if (!base.enabled)
				{
					return Vector3.zero;
				}
				return m_local;
			}
		}

		public Vector3 delta
		{
			get
			{
				if (!base.enabled)
				{
					return Vector3.zero;
				}
				return m_delta;
			}
		}

		public Vector3 velocity
		{
			get
			{
				if (!base.enabled)
				{
					return Vector3.zero;
				}
				return m_velocity;
			}
		}

		public Vector3 averageVelocity
		{
			get
			{
				if (!base.enabled)
				{
					return Vector3.zero;
				}
				return m_averageVelocity;
			}
		}

		public void ResetSpin()
		{
			spin = Vector3.zero;
		}

		protected override void Refresh(float p_dt)
		{
			Transform transform = base.droneTransform;
			Vector3 eulerAngles = (Quaternion.Inverse(lastRotation) * transform.rotation).eulerAngles;
			if (eulerAngles.x > 180f)
			{
				eulerAngles.x -= 360f;
			}
			if (eulerAngles.y > 180f)
			{
				eulerAngles.y -= 360f;
			}
			if (eulerAngles.z > 180f)
			{
				eulerAngles.z -= 360f;
			}
			lastRotation = transform.rotation;
			Vector3 vector = m_local;
			m_local = transform.localEulerAngles;
			m_delta = m_local - vector;
			m_velocity = eulerAngles / p_dt;
			m_averageVelocity = transform.InverseTransformDirection(base.drone.rigidbody.rb.angularVelocity) * 57.29578f;
			m_averageVelocity.x *= 0.998f;
			m_averageVelocity.z *= 0.998f;
			spin += m_velocity * p_dt;
			Vector3 eulerAngles2 = transform.eulerAngles;
			float z = eulerAngles2.z;
			float x = eulerAngles2.x;
			flipped = (z > flipThreshold && z < 360f - flipThreshold) || (x > flipThreshold && x < 360f - flipThreshold);
		}

		public override void Reset()
		{
			lastRotation = base.drone.transform.rotation;
			m_local = base.drone.transform.localEulerAngles;
			m_delta = Vector3.zero;
			m_velocity = Vector3.zero;
			spin = Vector3.zero;
			flipped = false;
		}
	}
}

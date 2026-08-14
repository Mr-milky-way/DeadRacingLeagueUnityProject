using UnityEngine;

namespace drl.sim.thread
{
	public class DroneIntertial : MonoBehaviour
	{
		private Vector3 velocityY;

		private Vector3 m_actualVelocity;

		private Vector3 m_localVelocity;

		private Vector3 m_lwp;

		public float speed => m_actualVelocity.magnitude;

		public Vector3 ActualVelocity => m_actualVelocity;

		public Vector3 LocalVelocity => m_localVelocity;

		public Vector3 VelocityY => velocityY;

		public Vector3 groundVelocity
		{
			get
			{
				Vector3 localVelocity = m_localVelocity;
				localVelocity.y = 0f;
				return localVelocity;
			}
		}

		public float groundSpeed => groundVelocity.magnitude;

		public float groundSpeedKph => groundSpeed * 3.6f;

		public void Refresh(float d_dt, Vector3 threadedVelocity, Quaternion virtualRotation)
		{
			m_actualVelocity = threadedVelocity;
			m_localVelocity = Matrix4x4.Rotate(virtualRotation).inverse.MultiplyPoint(m_actualVelocity);
			Vector3 vector = virtualRotation * Vector3.up;
			float num = Vector3.Dot(vector, m_actualVelocity.normalized);
			velocityY = vector * num * m_actualVelocity.magnitude;
		}
	}
}

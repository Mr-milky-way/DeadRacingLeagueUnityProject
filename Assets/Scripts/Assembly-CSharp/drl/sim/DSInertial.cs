using System.Collections.Generic;
using UnityEngine;

namespace drl.sim
{
	public class DSInertial : DroneSensor
	{
		[SerializeField]
		private Vector3 m_actualVelocity;

		[SerializeField]
		private float m_averageSpeed;

		private Vector3 m_lwp;

		private float m_speedSampleCount = 4f;

		private List<Vector3> m_velocitySamples = new List<Vector3>();

		public Vector3 velocity
		{
			get
			{
				if (!base.enabled)
				{
					return Vector3.zero;
				}
				return base.drone.rigidbody.rb.velocity;
			}
		}

		public Vector3 actualVelocity
		{
			get
			{
				if (!base.enabled)
				{
					return Vector3.zero;
				}
				return m_actualVelocity;
			}
		}

		public Vector3 velocityX
		{
			get
			{
				Vector3 right = base.drone.transform.right;
				float num = Vector3.Dot(right, velocity.normalized);
				return right * speed * num;
			}
		}

		public Vector3 velocityY
		{
			get
			{
				Vector3 up = base.drone.transform.up;
				float num = Vector3.Dot(up, velocity.normalized);
				return up * speed * num;
			}
		}

		public Vector3 velocityZ
		{
			get
			{
				Vector3 forward = base.drone.transform.forward;
				float num = Vector3.Dot(forward, velocity.normalized);
				return forward * speed * num;
			}
		}

		public float speed => velocity.magnitude;

		public float actualSpeed => m_actualVelocity.magnitude;

		public float speedKph => speed * 3.6f;

		public float averageSpeed => m_averageSpeed;

		public float averageSpeedKph => averageSpeed * 3.6f;

		public Vector3 speeds => new Vector3
		{
			x = velocityX.magnitude,
			y = velocityY.magnitude,
			z = velocityZ.magnitude
		};

		public float fallSpeed => Mathf.Max(0f - velocity.y, 0f);

		public Vector3 groundVelocity
		{
			get
			{
				Vector3 result = velocity;
				result.y = 0f;
				return result;
			}
		}

		public float groundForward
		{
			get
			{
				Vector3 forward = base.drone.transform.forward;
				forward.y = 0f;
				float num = Vector3.Dot(forward, velocity.normalized);
				return speed * num;
			}
		}

		public float groundSideways
		{
			get
			{
				Vector3 right = base.drone.transform.right;
				right.y = 0f;
				float num = Vector3.Dot(right, velocity.normalized);
				return speed * num;
			}
		}

		public float groundSpeed => groundVelocity.magnitude;

		public float groundSpeedKph => groundSpeed * 3.6f;

		protected override void Refresh(float p_dt)
		{
			UpdateSpeed(p_dt);
		}

		private void UpdateSpeed(float p_dt)
		{
			float num = ((p_dt != 0f) ? (1f / p_dt) : 0f);
			if ((float)m_velocitySamples.Count > m_speedSampleCount)
			{
				m_velocitySamples.RemoveAt(0);
			}
			m_velocitySamples.Add((base.drone.position - m_lwp) * num);
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < m_velocitySamples.Count; i++)
			{
				zero += m_velocitySamples[i];
			}
			m_actualVelocity = zero / m_velocitySamples.Count;
			m_lwp = base.drone.position;
			m_averageSpeed = Mathf.Lerp(m_averageSpeed, speed, p_dt * 10f);
		}

		public void SetActualVelocity(Vector3 p_actualVelocity)
		{
			m_actualVelocity = p_actualVelocity;
		}

		public override void Reset()
		{
			m_actualVelocity = Vector3.zero;
			m_lwp = base.drone.position;
		}
	}
}

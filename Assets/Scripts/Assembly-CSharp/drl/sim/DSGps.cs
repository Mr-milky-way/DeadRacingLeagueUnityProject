using UnityEngine;

namespace drl.sim
{
	public class DSGps : DroneSensor
	{
		[SerializeField]
		private Vector3 m_position;

		[SerializeField]
		private Vector3 m_groundPosition;

		[SerializeField]
		private float m_longitude;

		[SerializeField]
		private float m_latitude;

		public Vector3 position
		{
			get
			{
				if (!base.enabled)
				{
					return Vector3.zero;
				}
				return m_position;
			}
		}

		public Vector3 groundPosition
		{
			get
			{
				if (!base.enabled)
				{
					return Vector3.zero;
				}
				return m_groundPosition;
			}
		}

		public float longitude
		{
			get
			{
				if (!base.enabled)
				{
					return 0f;
				}
				return m_longitude;
			}
		}

		public float latitude
		{
			get
			{
				if (!base.enabled)
				{
					return 0f;
				}
				return m_latitude;
			}
		}

		protected override void Refresh(float p_dt)
		{
			m_position = base.drone.position;
			m_groundPosition = m_position;
			m_groundPosition.y = 0f;
			m_longitude = m_groundPosition.x;
			m_latitude = m_groundPosition.z;
		}
	}
}

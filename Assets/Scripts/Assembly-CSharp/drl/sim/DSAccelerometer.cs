using UnityEngine;

namespace drl.sim
{
	public class DSAccelerometer : DroneSensor
	{
		[SerializeField]
		private Vector3 m_local;

		[SerializeField]
		private Vector3 m_world;

		private Vector3 m_llp;

		private Vector3 m_lwp;

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

		public Vector3 world
		{
			get
			{
				if (!base.enabled)
				{
					return Vector3.zero;
				}
				return m_world;
			}
		}

		protected override void OnInitialize()
		{
			if ((bool)base.drone)
			{
				m_llp = base.drone.localPosition;
			}
			if ((bool)base.drone)
			{
				m_lwp = base.drone.position;
			}
		}

		protected override void Refresh(float p_dt)
		{
			Vector3 localPosition = base.droneTransform.localPosition;
			m_local = localPosition - m_llp;
			m_llp = localPosition;
			localPosition = base.drone.position;
			m_world = localPosition - m_lwp;
			m_lwp = localPosition;
		}

		public override void Reset()
		{
			m_local = Vector3.zero;
			m_world = Vector3.zero;
			m_llp = base.drone.localPosition;
			m_lwp = base.drone.position;
		}
	}
}

using UnityEngine;

namespace drl.sim
{
	public class DroneRFCamera : DronePart
	{
		[SerializeField]
		private DroneCamera m_target;

		private Vector3 m_tilt_euler;

		public Transform pivot;

		[SerializeField]
		private float m_fov;

		public DroneCamera target
		{
			get
			{
				return m_target;
			}
			set
			{
				m_target = value;
				Refresh();
			}
		}

		public float tilt
		{
			get
			{
				return 90f - m_tilt_euler.x;
			}
			set
			{
				Vector3 tilt_euler = m_tilt_euler;
				if (!base.drone)
				{
					Debug.LogWarning("DroneRFCamera> Drone is null!");
					return;
				}
				if (!base.drone.body)
				{
					Debug.LogWarning("DroneRFCamera> Drone.Body is null!");
					return;
				}
				if (!base.drone.body.frame)
				{
					Debug.LogWarning("DroneRFCamera> Drone.Body.Frame is null!");
					return;
				}
				Vector2 vector = base.drone.body.frame.tilt;
				float num = Mathf.Clamp(value, vector.x, vector.y);
				tilt_euler.x = 90f - num;
				base.transform.localEulerAngles = tilt_euler;
				tilt_euler = m_tilt_euler;
				tilt_euler.x = 90f - value;
				m_tilt_euler = tilt_euler;
				Transform transform = pivot;
				if ((bool)transform && !base.drone.isBroken)
				{
					if (transform.parent != base.transform.parent && base.transform.parent != null)
					{
						transform.transform.SetParent(base.transform.parent);
						transform.transform.position = base.transform.position;
						transform.transform.rotation = base.transform.rotation;
					}
					transform.localEulerAngles = m_tilt_euler;
				}
			}
		}

		public float fov
		{
			get
			{
				return m_fov;
			}
			set
			{
				m_fov = value;
				Refresh();
			}
		}

		public void Build()
		{
			Transform transform = new GameObject(base.name + "-pivot").transform;
			pivot = transform;
			m_tilt_euler = base.transform.localEulerAngles;
			tilt = tilt;
		}

		protected void Refresh()
		{
			if ((bool)target)
			{
				target.fov = fov;
			}
		}

		public override string GetPrefix()
		{
			return "C";
		}
	}
}

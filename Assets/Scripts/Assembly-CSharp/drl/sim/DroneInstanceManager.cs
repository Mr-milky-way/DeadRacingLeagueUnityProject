using System.Collections.Generic;
using UnityEngine;

namespace drl.sim
{
	public class DroneInstanceManager : DroneSimulationManager<Drone>
	{
		protected Dictionary<Drone, bool> m_armed_table;

		public bool ready
		{
			get
			{
				bool result = true;
				for (int i = 0; i < base.list.Count; i++)
				{
					if (!base.list[i].ready)
					{
						result = false;
					}
				}
				return result;
			}
		}

		protected void Awake()
		{
			m_armed_table = new Dictionary<Drone, bool>();
		}

		public void SetEnabled(bool p_flag)
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				if ((bool)base.list[i])
				{
					base.list[i].SetEnabled(p_flag);
				}
			}
		}

		public void SetVisible(bool p_flag)
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				if ((bool)base.list[i] && (bool)base.list[i].gameObject)
				{
					base.list[i].gameObject.SetActive(p_flag);
				}
			}
		}

		public void FixAll()
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				if ((bool)base.list[i] && (bool)base.list[i].gameObject)
				{
					base.list[i].Fix();
				}
			}
		}

		public void SetArmed(bool p_flag)
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				if ((bool)base.list[i] && (bool)base.list[i].fc)
				{
					base.list[i].fc.armed = p_flag;
				}
			}
		}

		public void SetReceiver(bool p_flag)
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				if ((bool)base.list[i] && (bool)base.list[i].receiver)
				{
					base.list[i].receiver.enabled = p_flag;
				}
			}
		}

		public void SaveArmed(Drone p_drone)
		{
			if ((bool)p_drone && (bool)p_drone.fc)
			{
				SaveArmed(p_drone, p_drone.fc.armed);
			}
		}

		public void SaveArmed(Drone p_drone, bool p_flag)
		{
			if ((bool)p_drone)
			{
				m_armed_table[p_drone] = p_flag;
			}
		}

		public void SaveArmed()
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				SaveArmed(base.list[i]);
			}
		}

		public void LoadArmed(Drone p_drone)
		{
			if ((bool)p_drone && (bool)p_drone.fc)
			{
				bool flag = m_armed_table.ContainsKey(p_drone);
				p_drone.fc.armed = flag && m_armed_table[p_drone];
			}
		}

		public void LoadArmed()
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				LoadArmed(base.list[i]);
			}
		}

		public void SetRigidbodyConstraint(RigidbodyConstraints p_flag)
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				if ((bool)base.list[i] && (bool)base.list[i].rigidbody && (bool)base.list[i].rigidbody.rb)
				{
					base.list[i].rigidbody.rb.constraints = p_flag;
				}
			}
		}

		public void SetRigidbody(bool p_flag)
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				if ((bool)base.list[i] && (bool)base.list[i].rigidbody)
				{
					base.list[i].rigidbody.enabled = p_flag;
				}
			}
		}

		public void ClearForces()
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				if ((bool)base.list[i])
				{
					base.list[i].ClearForces();
				}
			}
		}

		public void ResetFlightControllers()
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				if ((bool)base.list[i] && (bool)base.list[i].fc)
				{
					base.list[i].fc.Reset();
				}
			}
		}

		public void SetPause(Drone p_drone, bool p_flag)
		{
			if ((bool)p_drone)
			{
				p_drone.SetPaused(p_flag);
			}
		}

		public void SetPause(bool p_flag)
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				SetPause(base.list[i], p_flag);
			}
		}

		public void SetMotorSpinSpeed(float p_speed)
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				if ((bool)base.list[i])
				{
					base.list[i].SetMotorSpinSpeed(p_speed);
				}
			}
		}

		public Drone GetClosest(Vector3 p_position, float p_max_distance)
		{
			if (base.list.Count <= 0)
			{
				return null;
			}
			if (base.list.Count <= 1)
			{
				return base.list[0];
			}
			int index = 0;
			float num = Vector3.Distance(p_position, base.list[0].position);
			for (int i = 1; i < base.list.Count; i++)
			{
				float num2 = Vector3.Distance(p_position, base.list[i].position);
				if (!(num2 >= num))
				{
					num = num2;
					index = i;
				}
			}
			if (num > p_max_distance)
			{
				return null;
			}
			return base.list[index];
		}

		public Drone GetClosest(Vector3 p_position)
		{
			return GetClosest(p_position, float.PositiveInfinity);
		}

		public override string GetContainerName()
		{
			return "drones";
		}

		public void Replace(Drone p_old, Drone p_new)
		{
			if (!(p_old == null) && !(p_new == null) && !base.list.Contains(p_new) && base.list.Contains(p_old))
			{
				int index = base.list.IndexOf(p_old);
				base.list[index] = p_new;
				if (m_armed_table.ContainsKey(p_old))
				{
					m_armed_table.Add(p_new, m_armed_table[p_old]);
					m_armed_table.Remove(p_old);
				}
				p_new.lastState = p_old.lastState;
			}
		}

		public void SetCustomReflections(bool p_flag)
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				if (base.list[i] != null)
				{
					Transform transform = base.list[i].transform.Find("reflection");
					if (transform != null)
					{
						transform.gameObject.SetActive(p_flag);
					}
				}
			}
		}
	}
}

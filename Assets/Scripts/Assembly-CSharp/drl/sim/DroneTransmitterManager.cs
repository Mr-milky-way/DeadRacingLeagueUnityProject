using System.Collections.Generic;
using UnityEngine;

namespace drl.sim
{
	public class DroneTransmitterManager : DroneSimulationManager<DroneInputTransmitter>
	{
		protected List<int> m_ids = new List<int>(new int[25]
		{
			0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
			10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
			20, 21, 22, 23, 24
		});

		public T Add<T>() where T : DroneInputTransmitter
		{
			T val = new GameObject("new-transmitter").AddComponent<T>();
			val.order = ((m_ids.Count > 0) ? m_ids[0] : (-1));
			if (m_ids.Count > 0)
			{
				m_ids.RemoveAt(0);
			}
			val.channel = -1;
			val.name = val.GetPrefix() + "-" + val.order;
			val.transform.parent = base.container;
			val.transform.localPosition = Vector3.zero;
			val.transform.localEulerAngles = Vector3.zero;
			base.list.Add(val);
			return val;
		}

		public T GetByChannel<T>(int p_id) where T : DroneInputTransmitter
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				DroneInputTransmitter droneInputTransmitter = base.list[i];
				if (droneInputTransmitter.channel == p_id)
				{
					if (!(droneInputTransmitter is T))
					{
						return null;
					}
					return (T)droneInputTransmitter;
				}
			}
			return null;
		}

		public T GetByDrone<T>(Drone p_drone) where T : DroneInputTransmitter
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				DroneInputTransmitter droneInputTransmitter = base.list[i];
				if (droneInputTransmitter.drone == p_drone)
				{
					if (!(droneInputTransmitter is T))
					{
						return null;
					}
					return (T)droneInputTransmitter;
				}
			}
			return null;
		}

		public List<T> FindAll<T>() where T : DroneInputTransmitter
		{
			List<T> list = new List<T>();
			for (int i = 0; i < base.list.Count; i++)
			{
				if (base.list[i] is T)
				{
					list.Add((T)base.list[i]);
				}
			}
			return list;
		}

		public void SetEnabled<T>(bool p_flag) where T : DroneInputTransmitter
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				if (base.list[i] is T)
				{
					((T)base.list[i]).enabled = p_flag;
				}
			}
		}

		public void ResetGhostDrones()
		{
			List<DroneGhostTransmitter> list = FindAll<DroneGhostTransmitter>();
			for (int i = 0; i < list.Count; i++)
			{
				list[i].enabled = false;
				list[i].elapsed = 0f;
				list[i].Seek(0f);
				list[i].usePhysics = false;
				list[i].enablePhysicsOnComplete = false;
			}
		}

		public void SetPhysicsOnComplete(bool p_flag)
		{
			List<DroneGhostTransmitter> list = FindAll<DroneGhostTransmitter>();
			for (int i = 0; i < list.Count; i++)
			{
				list[i].enablePhysicsOnComplete = p_flag;
			}
		}

		public void RemoveGhostDrones()
		{
			List<DroneGhostTransmitter> list = FindAll<DroneGhostTransmitter>();
			for (int i = 0; i < list.Count; i++)
			{
				list[i].enabled = false;
				Remove(list[i]);
			}
		}

		public void SetGhostDronesSpeed(float p_speed)
		{
			List<DroneGhostTransmitter> list = FindAll<DroneGhostTransmitter>();
			for (int i = 0; i < list.Count; i++)
			{
				list[i].speed = p_speed;
			}
		}

		public new void Remove(DroneInputTransmitter p_target)
		{
			if (base.list.Contains(p_target))
			{
				base.list.Remove(p_target);
				if ((bool)p_target)
				{
					m_ids.Add(p_target.order);
					Object.Destroy(p_target.gameObject);
				}
			}
		}

		public void Step(float p_dt)
		{
			if (!base.simulation.drones || !base.enabled)
			{
				return;
			}
			List<Drone> list = base.simulation.drones.list;
			for (int i = 0; i < base.list.Count; i++)
			{
				DroneInputTransmitter droneInputTransmitter = base.list[i];
				if (!droneInputTransmitter)
				{
					base.list.RemoveAt(i--);
					continue;
				}
				droneInputTransmitter.drone = null;
				if (droneInputTransmitter.channel >= 0)
				{
					for (int j = 0; j < list.Count; j++)
					{
						Drone drone = list[j];
						if (drone.hasReceiver && drone.receiver.channel >= 0 && droneInputTransmitter.channel == drone.receiver.channel)
						{
							droneInputTransmitter.drone = drone;
							break;
						}
					}
				}
				droneInputTransmitter.Step(p_dt);
			}
		}

		public override string GetContainerName()
		{
			return "transmitters";
		}
	}
}

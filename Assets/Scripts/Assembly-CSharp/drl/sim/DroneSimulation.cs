using System;
using System.Collections.Generic;
using UnityEngine;
using drl.game;
using thelab.core;

namespace drl.sim
{
	[RequireComponent(typeof(DroneModuleManager))]
	[RequireComponent(typeof(DronePodiumManager))]
	[RequireComponent(typeof(DroneCameraManager))]
	[RequireComponent(typeof(DroneInstanceManager))]
	[RequireComponent(typeof(DroneTransmitterManager))]
	public class DroneSimulation : MonoBehaviour
	{
		public float elapsed;

		public float deltaTime;

		[Range(0f, 1f)]
		public float speed = 1f;

		public bool running;

		[SerializeField]
		private DroneSimulationPauseMode m_pause;

		[SerializeField]
		[HideInInspector]
		private DroneModuleManager m_modules;

		[SerializeField]
		[HideInInspector]
		private DronePodiumManager m_podiums;

		[SerializeField]
		[HideInInspector]
		private DroneCameraManager m_cameras;

		[SerializeField]
		[HideInInspector]
		private DroneInstanceManager m_drones;

		[SerializeField]
		[HideInInspector]
		private DroneTransmitterManager m_transmitters;

		[HideInInspector]
		public UIHUDRequirements UIRequirements;

		public DroneSimulationEventCallback OnEvent;

		public static List<DroneSimulation> instances = new List<DroneSimulation>();

		private float m_last_timescale;

		private bool m_countdown_finished;

		private bool m_all_ready;

		public DroneSimulationPauseMode pause
		{
			get
			{
				return m_pause;
			}
			set
			{
				if (m_pause != value)
				{
					OnPauseChange(value);
					m_pause = value;
				}
			}
		}

		public bool isPaused
		{
			get
			{
				return (pause & DroneSimulationPauseMode.Pause) != 0;
			}
			set
			{
				pause = (value ? (pause | DroneSimulationPauseMode.Pause) : DroneSimulationPauseMode.Unpause);
			}
		}

		public DroneModuleManager modules
		{
			get
			{
				if (!(m_modules == null))
				{
					return m_modules;
				}
				return m_modules = GetComponent<DroneModuleManager>();
			}
		}

		public DronePodiumManager podiums
		{
			get
			{
				if (!m_podiums)
				{
					return m_podiums = GetComponent<DronePodiumManager>();
				}
				return m_podiums;
			}
		}

		public DroneCameraManager cameras
		{
			get
			{
				if (!m_cameras)
				{
					return m_cameras = GetComponent<DroneCameraManager>();
				}
				return m_cameras;
			}
		}

		public DroneInstanceManager drones
		{
			get
			{
				if (!m_drones)
				{
					return m_drones = GetComponent<DroneInstanceManager>();
				}
				return m_drones;
			}
		}

		public DroneTransmitterManager transmitters
		{
			get
			{
				if (!m_transmitters)
				{
					return m_transmitters = GetComponent<DroneTransmitterManager>();
				}
				return m_transmitters;
			}
		}

		public List<Drone> list => null;

		public void Dispatch(DroneSimulationEventType p_type, params object[] p_args)
		{
			if (OnEvent != null)
			{
				OnEvent.Invoke(new DroneSimulationEvent
				{
					type = p_type,
					target = this,
					args = p_args
				});
			}
		}

		protected void Awake()
		{
			UIRequirements = GetComponentInChildren<UIHUDRequirements>();
			if (!instances.Contains(this))
			{
				instances.Add(this);
			}
			Physics.gravity = new Vector3(0f, -9.81f, 0f);
		}

		protected void OnDestroy()
		{
			if (instances.Contains(this))
			{
				instances.Remove(this);
			}
		}

		public void Initialize()
		{
			Debug.Log("DroneSimulation> Initialize");
			modules.OnInitialize();
			Dispatch(DroneSimulationEventType.Initialize);
		}

		public void Run(bool p_arm)
		{
			if (running)
			{
				Debug.LogWarning("DroneSimulation> Simulation already Running! Try using 'Stop'.");
				return;
			}
			Debug.Log("DroneSimulation> Run - arm[" + p_arm + "]");
			elapsed = 0f;
			m_pause = DroneSimulationPauseMode.Unpause;
			running = false;
			m_countdown_finished = false;
			m_all_ready = false;
			float ts = speed;
			m_last_timescale = ts;
			Activity.Run((Func<bool>)delegate
			{
				if (!drones.ready)
				{
					return true;
				}
				drones.SetEnabled(p_flag: false);
				drones.SetMotorSpinSpeed(ts);
				running = true;
				Dispatch(DroneSimulationEventType.Run);
				drones.SetArmed(p_arm);
				return false;
			}, 0f, false);
		}

		public void Run()
		{
			Run(p_arm: true);
		}

		public void Stop()
		{
			running = false;
			drones.SetArmed(p_flag: false);
			drones.SetEnabled(p_flag: false);
			drones.SetMotorSpinSpeed(0f);
			modules.OnStop();
			Dispatch(DroneSimulationEventType.Stop);
			PlaceDrones();
		}

		public void BlendTimeScale(float p_speed, float p_duration)
		{
			Tween.Add(typeof(Time), "timeScale", p_speed, p_duration, Cubic.Out);
		}

		public void ReplaceDrone(Drone p_old, Drone p_new)
		{
			if (p_old == null || p_new == null || drones.list.Contains(p_new) || !drones.list.Contains(p_old))
			{
				return;
			}
			drones.Replace(p_old, p_new);
			Dispatch(DroneSimulationEventType.DroneRemove, p_old);
			Dispatch(DroneSimulationEventType.DroneAdd, p_new);
			foreach (DroneCamera item in cameras.list)
			{
				if (item.drone == p_old)
				{
					item.drone = p_new;
				}
			}
			if (p_old.receiver != null && p_new.receiver != null)
			{
				p_new.receiver.channel = p_old.receiver.channel;
			}
			Activity.Run((Func<bool>)delegate
			{
				if (!p_new)
				{
					return false;
				}
				if (!p_new.ready)
				{
					return true;
				}
				Dispatch(DroneSimulationEventType.DroneReady, p_new);
				Activity.RunOnce(delegate
				{
					if (!m_all_ready && drones.ready)
					{
						Dispatch(DroneSimulationEventType.AllDronesReady);
						m_all_ready = true;
					}
				}, 0.1f);
				return false;
			}, 0f, false);
		}

		public void RegisterDrone(Drone p_drone)
		{
			if (!p_drone)
			{
				return;
			}
			m_all_ready = false;
			drones.Add(p_drone);
			Dispatch(DroneSimulationEventType.DroneAdd, p_drone);
			Activity.Run((Func<bool>)delegate
			{
				if (!p_drone)
				{
					return false;
				}
				if (!p_drone.ready)
				{
					return true;
				}
				Dispatch(DroneSimulationEventType.DroneReady, p_drone);
				Activity.RunOnce(delegate
				{
					if (!m_all_ready && drones.ready)
					{
						Dispatch(DroneSimulationEventType.AllDronesReady);
						m_all_ready = true;
					}
				}, 0.1f);
				return false;
			}, 0f, false);
		}

		public void PlaceDrone(Drone p_drone, int p_index = -1, bool p_force_podium = false, bool p_recover = true)
		{
			if (!p_drone)
			{
				return;
			}
			List<DronePodium> list = podiums.list;
			int num = 0;
			int count = list.Count;
			List<Drone> list2 = drones.list;
			if (p_index > 0)
			{
				Debug.Log($"DroneSimulation> PlaceDrone index: {p_index} drone.name: {p_drone.name}");
			}
			num = ((p_index < 0) ? list2.Count : p_index);
			Transform transform = ((count <= 0) ? null : list[num % count].spawn);
			Vector3 p_target = (transform ? transform.position : new Vector3(0f, 5f, 0f));
			Quaternion rotation = (transform ? transform.rotation : Quaternion.Euler(30f, 0f, 0f));
			Transform container = drones.container;
			p_drone.transform.rotation = rotation;
			p_drone.ResetPosition(p_target, p_force_podium);
			p_drone.transform.rotation = rotation;
			p_drone.renderer.ClearTrails();
			if (p_drone.transform.parent != container)
			{
				p_drone.transform.SetParent(container, worldPositionStays: true);
			}
			if (!list2.Contains(p_drone))
			{
				list2.Add(p_drone);
			}
			if ((bool)p_drone.fc)
			{
				p_drone.fc.Reset();
			}
			if (running)
			{
				p_drone.SetEnabled(p_flag: true);
				if (p_recover)
				{
					p_drone.Fix();
				}
				p_drone.ClearForces();
				if (p_drone.physics != null && p_drone.physics.aerodynamics != null)
				{
					p_drone.physics.aerodynamics.Reset();
				}
			}
			p_drone.StabilizeDroneOnGround(p_flag: true);
		}

		public void PlaceDrone(Drone p_drone, Transform p_transform)
		{
			if ((bool)p_drone)
			{
				p_drone.transform.localRotation = p_transform.localRotation;
				p_drone.ResetPosition(p_transform.position);
				p_drone.transform.localRotation = p_transform.localRotation;
				p_drone.fc.Reset();
				p_drone.ClearForces();
				if (p_drone.physics != null && p_drone.physics.aerodynamics != null)
				{
					p_drone.physics.aerodynamics.Reset();
				}
			}
		}

		public void PlaceDrone(int p_index, int p_podium = -1)
		{
			Drone drone = drones.Get(p_index);
			if ((bool)drone)
			{
				PlaceDrone(drone, p_podium);
			}
		}

		public void PlaceDrones()
		{
			List<Drone> list = drones.list;
			for (int i = 0; i < list.Count; i++)
			{
				PlaceDrone(list[i], i);
			}
		}

		public void RemoveDrone(Drone p_drone)
		{
			Dispatch(DroneSimulationEventType.DroneRemove, p_drone);
			drones.Remove(p_drone);
			p_drone.Destroy(p_async: true);
		}

		public void RemoveDroneWithoutDestroy(Drone p_drone)
		{
			Dispatch(DroneSimulationEventType.DroneRemove, p_drone);
			drones.Remove(p_drone);
		}

		public void SetDroneTransmitter(int p_id, bool p_active = true)
		{
			DroneInputTransmitter byChannel = transmitters.GetByChannel<DroneInputTransmitter>(p_id);
			if ((bool)byChannel)
			{
				byChannel.enabled = p_active;
			}
		}

		public void SetDroneTransmitter(Drone p_drone, bool p_active = true)
		{
			if (p_drone == null)
			{
				Debug.LogWarning("DroneSimulation>SetDroneTransmitter - No drone selected! ");
				return;
			}
			DroneInputTransmitter byDrone = transmitters.GetByDrone<DroneInputTransmitter>(p_drone);
			if ((bool)byDrone)
			{
				byDrone.enabled = p_active;
			}
		}

		public void SetDroneTransmitter(bool p_active = true)
		{
			Drone p_drone = drones.Get(0);
			SetDroneTransmitter(p_drone, p_active);
		}

		protected void Update()
		{
			if (running)
			{
				float num = speed;
				if (Mathf.Abs(num - m_last_timescale) > 0f)
				{
					m_last_timescale = num;
					drones.SetMotorSpinSpeed(num);
					Dispatch(DroneSimulationEventType.ChangeSpeed);
				}
				float num2 = Time.unscaledDeltaTime * speed;
				deltaTime = num2;
				if (m_countdown_finished)
				{
					transmitters.Step(deltaTime);
					modules.OnUpdate();
					elapsed += deltaTime;
				}
				else
				{
					drones.SetEnabled(p_flag: true);
					drones.ClearForces();
					m_countdown_finished = true;
				}
			}
		}

		protected void FixedUpdate()
		{
			if (running)
			{
				float num = Time.fixedDeltaTime * speed;
				deltaTime = num;
				modules.OnFixedUpdate();
			}
		}

		public bool IsPause(DroneSimulationPauseMode p_flag)
		{
			if (pause == p_flag)
			{
				return true;
			}
			return (pause & p_flag) != 0;
		}

		protected virtual void OnPauseChange(DroneSimulationPauseMode p_new_mode)
		{
			bool flag = running;
			switch (p_new_mode & DroneSimulationPauseMode.Pause)
			{
			case DroneSimulationPauseMode.Unpause:
				running = true;
				drones.SetPause(p_flag: false);
				break;
			case DroneSimulationPauseMode.Pause:
				running = false;
				drones.SetPause(p_flag: true);
				if (IsPause(DroneSimulationPauseMode.PauseKeepPhysics))
				{
					drones.SetRigidbody(p_flag: true);
				}
				if (IsPause(DroneSimulationPauseMode.PauseKeepRunning))
				{
					running = flag;
				}
				break;
			}
			modules.OnPauseChange(pause, p_new_mode);
			Dispatch(DroneSimulationEventType.PauseChange);
		}
	}
}

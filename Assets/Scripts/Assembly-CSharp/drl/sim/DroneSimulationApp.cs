using UnityEngine;
using UnityEngine.SceneManagement;
using thelab.core;

namespace drl.sim
{
	public class DroneSimulationApp : MonoBehaviour
	{
		public DroneFactory factory;

		public DroneSimulation simulation;

		public DroneCamera camera;

		private Vector3 p_los;

		private Vector3 r_los;

		public TextAsset[] rigs;

		protected void Awake()
		{
			simulation.podiums.Build();
			AddDrone(0);
			simulation.Initialize();
			Activity.RunOnce(simulation.Run, 0.3f);
			Activity.RunOnce(SetCameraFPV, 0.3f);
			p_los = camera.transform.position;
			r_los = camera.transform.localEulerAngles;
			camera.SetFreeCamera();
			camera.orbit.Snap();
		}

		protected void AddDrone(int p_id)
		{
			Drone d = CreateRig(p_id);
			if (!d)
			{
				return;
			}
			simulation.PlaceDrone(d);
			d.OnEvent.AddListener(delegate(DroneEvent p_event)
			{
				if (p_event.type == DroneEventType.Ready)
				{
					d.receiver.channel = p_id;
				}
			});
			((p_id <= 0) ? simulation.transmitters.Add<DroneRCTransmitter>() : simulation.transmitters.Add<DroneInputTransmitter>()).channel = p_id;
		}

		protected Drone CreateRig(int p_index)
		{
			if (rigs.Length == 0)
			{
				return null;
			}
			DroneRigData droneRigData = ScriptableObject.CreateInstance<DroneRigData>();
			droneRigData.Set(rigs[p_index].bytes);
			Debug.Log("Creating " + droneRigData.name);
			Drone drone = factory.Instantiate(droneRigData, base.transform);
			if (droneRigData.allowDynamicColor)
			{
				drone.renderer.color0 = droneRigData.color0;
				drone.renderer.color1 = droneRigData.color1;
				drone.renderer.color2 = droneRigData.color2;
			}
			return drone;
		}

		public void SetCameraFollow()
		{
			camera.SetTPVBack(simulation.drones.list[0]);
		}

		public void SetCameraOrbit()
		{
			camera.SetTPVFree(simulation.drones.list[0]);
		}

		public void SetCameraFPV()
		{
			camera.SetFPV(simulation.drones.list[0]);
		}

		public void SetCameraLOS()
		{
			camera.SetLOS(simulation.drones.list[0], p_los, 30f);
		}

		public void SetCameraFree()
		{
			camera.SetFreeCamera();
			camera.orbit.anchorEulerAngles = r_los;
			camera.transform.position = p_los;
			camera.transform.localEulerAngles = r_los;
			camera.orbit.angle = Vector3.zero;
			camera.orbit.Snap();
		}

		public void SetDroneProcess(string p_process, bool p_value)
		{
			switch (p_process.ToLower())
			{
			case "altitude":
				simulation.drones.list[0].fc.SetProcess(FlightControllerProcess.Altitude, p_value);
				break;
			case "level":
				simulation.drones.list[0].fc.SetProcess(FlightControllerProcess.Level, p_value);
				break;
			case "limiter":
				simulation.drones.list[0].fc.SetProcess(FlightControllerProcess.Limiter, p_value);
				break;
			case "lock":
				simulation.drones.list[0].fc.SetProcess(FlightControllerProcess.Lock, p_value);
				break;
			case "training":
				simulation.drones.list[0].fc.SetProcess(FlightControllerProcess.Training, p_value);
				break;
			}
		}

		public void SetDroneMode(string p_mode)
		{
			switch (p_mode.ToLower())
			{
			case "beginner":
				simulation.drones.list[0].fc.SetMode(FlightControllerMode.Beginner);
				break;
			case "intermediate":
				simulation.drones.list[0].fc.SetMode(FlightControllerMode.Intermediate);
				break;
			case "pro":
				simulation.drones.list[0].fc.SetMode(FlightControllerMode.Acro);
				break;
			case "acro":
				simulation.drones.list[0].fc.SetMode(FlightControllerMode.Acro);
				break;
			case "air":
				simulation.drones.list[0].fc.SetMode(FlightControllerMode.Air);
				break;
			case "level":
				simulation.drones.list[0].fc.SetMode(FlightControllerMode.Level);
				break;
			case "horizon":
				simulation.drones.list[0].fc.SetMode(FlightControllerMode.Horizon);
				break;
			case "baro":
				simulation.drones.list[0].fc.SetMode(FlightControllerMode.Baro);
				break;
			case "dji":
				simulation.drones.list[0].fc.SetMode(FlightControllerMode.DJI);
				break;
			case "target":
				simulation.drones.list[0].fc.SetMode(FlightControllerMode.Target);
				break;
			case "bypass":
				simulation.drones.list[0].fc.SetMode(FlightControllerMode.Bypass);
				break;
			case "debug":
				simulation.drones.list[0].fc.SetMode(FlightControllerMode.Debug);
				break;
			case "free":
				simulation.drones.list[0].fc.SetMode(FlightControllerMode.Free);
				break;
			case "training":
				simulation.drones.list[0].fc.SetMode(FlightControllerMode.Training);
				break;
			case "stabilized":
				simulation.drones.list[0].fc.SetMode(FlightControllerMode.Stabilized);
				break;
			default:
				Debug.LogError("unknown flight mode: " + p_mode);
				break;
			}
		}

		public void Restart()
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}

		public void Quit()
		{
			Application.Quit();
		}

		public void QuitToMenu()
		{
			SceneManager.LoadScene("main", LoadSceneMode.Single);
		}

		public void Reset()
		{
			simulation.drones.list[0].ResetPosition();
		}

		public void Recharge()
		{
			foreach (DroneBattery battery in simulation.drones.list[0].body.frame.batteries)
			{
				battery.Recharge();
			}
		}

		public void ChangeRig(int rig)
		{
			if (rig < 0 || rig >= rigs.Length)
			{
				return;
			}
			Drone drone = simulation.drones.list[0];
			simulation.drones.list.Remove(drone);
			drone.gameObject.SetActive(value: false);
			Object.Destroy(drone.gameObject, 5f);
			Drone d = CreateRig(rig);
			if (!d)
			{
				return;
			}
			simulation.PlaceDrone(d, 0);
			d.OnEvent.AddListener(delegate(DroneEvent p_event)
			{
				if (p_event.type == DroneEventType.Ready)
				{
					d.receiver.channel = 0;
				}
				d.fc.armed = true;
				camera.drone = d;
			});
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				Reset();
			}
		}
	}
}

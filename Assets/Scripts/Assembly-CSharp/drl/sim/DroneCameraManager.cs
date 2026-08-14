using UnityEngine;
using thelab.core;

namespace drl.sim
{
	public class DroneCameraManager : DroneSimulationManager<DroneCamera>
	{
		public DroneCamera template;

		public DroneCamera GetCameraByDrone(Drone p_drone)
		{
			for (int i = 0; i < base.list.Count; i++)
			{
				if (base.list[i].drone == p_drone)
				{
					return base.list[i];
				}
			}
			return null;
		}

		public void SetFPV(int p_camera_id, int p_drone_id)
		{
			DroneCamera droneCamera = Get(p_camera_id);
			if (!droneCamera)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Camera " + p_camera_id);
				return;
			}
			Drone drone = base.simulation.drones.Get(p_drone_id);
			if (!drone)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Drone " + p_drone_id);
				return;
			}
			droneCamera.SetFPV(drone);
			drone.renderer.shadowsOnly = true;
		}

		public void SetFPVSmooth(int p_camera_id, int p_drone_id, float p_transition_time)
		{
			DroneCamera droneCamera = Get(p_camera_id);
			if (!droneCamera)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Camera " + p_camera_id);
				return;
			}
			Drone drone = base.simulation.drones.Get(p_drone_id);
			if (!drone)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Drone");
			}
			else
			{
				droneCamera.SetFPVSmooth(drone, p_transition_time);
			}
		}

		public void SetLOS(int p_camera_id, int p_drone_id, float p_cameraSpeed = 3f)
		{
			DroneCamera droneCamera = Get(p_camera_id);
			if (!droneCamera)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Camera " + p_camera_id);
				return;
			}
			Drone drone = base.simulation.drones.Get(p_drone_id);
			if (!drone)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Drone");
				return;
			}
			droneCamera.SetLOS(drone, droneCamera.transform, p_cameraSpeed);
			drone.renderer.shadowsOnly = false;
		}

		public void SetLOS(int p_camera_id, int p_drone_id, Vector3 p_anchor, float p_cameraSpeed = 3f)
		{
			DroneCamera droneCamera = Get(p_camera_id);
			if (!droneCamera)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Camera " + p_camera_id);
				return;
			}
			Drone drone = base.simulation.drones.Get(p_drone_id);
			if (!drone)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Drone");
				return;
			}
			if (p_cameraSpeed >= 0f)
			{
				droneCamera.SetLOS(drone, p_anchor, p_cameraSpeed);
			}
			else
			{
				droneCamera.SetLOSFast(drone, p_anchor);
			}
			drone.renderer.shadowsOnly = false;
		}

		public void SetTPV(int p_camera_id, int p_drone_id, bool p_back = true, bool smooth = false)
		{
			DroneCamera droneCamera = Get(p_camera_id);
			if (!droneCamera)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Camera " + p_camera_id);
				return;
			}
			Drone drone = base.simulation.drones.Get(p_drone_id);
			if (!drone)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Drone");
				return;
			}
			drone.renderer.shadowsOnly = false;
			if (smooth)
			{
				droneCamera.SetTPVSmooth(drone);
			}
			else if (p_back)
			{
				droneCamera.SetTPVBack(drone);
			}
			else
			{
				droneCamera.SetTPVFree(drone);
			}
		}

		public void SetTPVSmooth(int p_camera_id, int p_drone_id)
		{
			DroneCamera droneCamera = Get(p_camera_id);
			if (!droneCamera)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Camera " + p_camera_id);
				return;
			}
			Drone drone = base.simulation.drones.Get(p_drone_id);
			if (!drone)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Drone");
				return;
			}
			droneCamera.SetTPVSmooth(drone);
			drone.renderer.shadowsOnly = false;
		}

		public void SetTPVSide(int p_camera_id, int p_drone_id)
		{
			DroneCamera droneCamera = Get(p_camera_id);
			if (!droneCamera)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Camera " + p_camera_id);
				return;
			}
			Drone drone = base.simulation.drones.Get(p_drone_id);
			if (!drone)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Drone");
				return;
			}
			drone.renderer.shadowsOnly = false;
			droneCamera.follow.target = drone.transform;
			droneCamera.follow.flags = OrbitFollowInput.Flag.PositionXYZ;
			droneCamera.follow.offset = new Vector3(0f, 0f, -1.5f);
			foreach (DroneTrail trail in drone.renderer.trails)
			{
				trail.gameObject.SetActive(value: true);
			}
		}

		public void SetTPVCUAV(int p_camera_id, int p_drone_id)
		{
			DroneCamera droneCamera = Get(p_camera_id);
			if (!droneCamera)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Camera " + p_camera_id);
				return;
			}
			Drone drone = base.simulation.drones.Get(p_drone_id);
			if (!drone)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Drone");
				return;
			}
			drone.renderer.shadowsOnly = false;
			droneCamera.SetTPVBack(drone, 1.4f);
			droneCamera.follow.SetOffset(new Vector3(0f, -1f, 0f));
		}

		public void SetLineCamera(int p_camera_id, int p_drone_id, LineTransform p_line, float p_speed = 3f, bool betweenAnchors = false)
		{
			DroneCamera droneCamera = Get(p_camera_id);
			if (!droneCamera)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Camera " + p_camera_id);
				return;
			}
			Drone drone = base.simulation.drones.Get(p_drone_id);
			if (!drone)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Drone");
				return;
			}
			droneCamera.SetLineCamera(drone, p_line, p_speed, betweenAnchors);
			drone.renderer.shadowsOnly = false;
		}

		public void SetLineCamera(int p_camera_id, int p_drone_id, Transform p_a0, Transform p_a1, float p_speed = 3f)
		{
			DroneCamera droneCamera = Get(p_camera_id);
			if (!droneCamera)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Camera " + p_camera_id);
				return;
			}
			Drone drone = base.simulation.drones.Get(p_drone_id);
			if (!drone)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Drone");
				return;
			}
			droneCamera.SetLineCamera(drone, p_a0, p_a1, p_speed);
			drone.renderer.shadowsOnly = false;
		}

		public void SetLineCamera(int p_camera_id, int p_drone_id, Vector3 p_a0, Vector3 p_a1, float p_speed = 3f)
		{
			DroneCamera droneCamera = Get(p_camera_id);
			if (!droneCamera)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Camera " + p_camera_id);
				return;
			}
			Drone drone = base.simulation.drones.Get(p_drone_id);
			if (!drone)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Drone");
				return;
			}
			droneCamera.SetLineCamera(drone, p_a0, p_a1, p_speed);
			drone.renderer.shadowsOnly = false;
		}

		public void SetTransitions(int p_camera_id, OrbitTransform.Transition mask)
		{
			DroneCamera droneCamera = Get(p_camera_id);
			if ((bool)droneCamera)
			{
				droneCamera.orbit.SetTransition(mask);
			}
		}

		public void SetFree(int p_camera_id)
		{
			DroneCamera droneCamera = Get(p_camera_id);
			if (!droneCamera)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Camera " + p_camera_id);
			}
			else
			{
				droneCamera.SetFreeCamera();
			}
		}

		public void SetNone(int p_camera_id)
		{
			DroneCamera droneCamera = Get(p_camera_id);
			if (!droneCamera)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Camera " + p_camera_id);
			}
			else
			{
				droneCamera.SetNone();
			}
		}

		public DroneCamera Push()
		{
			DroneCamera droneCamera = Instantiate(template);
			if ((bool)droneCamera)
			{
				base.simulation.Dispatch(DroneSimulationEventType.CameraAdd, droneCamera);
			}
			return droneCamera;
		}

		public void SetOrbitDistance(int p_camera_id, float p_distance)
		{
			DroneCamera droneCamera = Get(p_camera_id);
			if ((bool)droneCamera)
			{
				droneCamera.orbit.distance = p_distance;
			}
		}

		public void SetOrbitAngle(int p_camera_id, Vector2 p_angles)
		{
			DroneCamera droneCamera = Get(p_camera_id);
			if ((bool)droneCamera)
			{
				droneCamera.orbit.angle = p_angles;
			}
		}

		public void SetOther(int p_camera_id, int p_drone_id)
		{
			if (!Get(p_camera_id))
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Camera " + p_camera_id);
				return;
			}
			Drone drone = base.simulation.drones.Get(p_drone_id);
			if (!drone)
			{
				Debug.LogWarning("DroneCameraManager> Failed to locate Drone " + p_drone_id);
			}
			else
			{
				drone.renderer.shadowsOnly = false;
			}
		}

		public override string GetContainerName()
		{
			return "cameras";
		}
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.sim
{
	public class SimulationFlowModule : DroneSimulationModule
	{
		[SerializeField]
		private FlowModuleUI m_ui;

		public Flow main;

		public Flow steps;

		[HideInInspector]
		public GameObject missionModule;

		[HideInInspector]
		public GameObject missionModuleDuplicate;

		public Transform cameraStart;

		[HideInInspector]
		public DroneCameraModeType activeCameraMode;

		[HideInInspector]
		public GameObject droneStart;

		[HideInInspector]
		public bool resetAvailable;

		internal Dictionary<Flow, bool> activeFlows;

		[SerializeField]
		private DataFlow m_data;

		[SerializeField]
		private AssetLibrary m_library;

		public DroneFactory factory;

		public Drone playerDrone;

		private Vector3 startingPosition;

		private Quaternion startingRotation;

		private Activity[] m_activity_list;

		public FlowModuleUI ui
		{
			get
			{
				if (!m_ui)
				{
					return m_ui = GetComponent<FlowModuleUI>();
				}
				return m_ui;
			}
			set
			{
				m_ui = value;
			}
		}

		public DataFlow data => Reflection<object>.Assert(ref m_data, base.gameObject);

		public AssetLibrary library => Reflection<object>.Assert(ref m_library, base.gameObject);

		private void OnDisable()
		{
			if (steps != null)
			{
				Flow flow = steps;
				flow.ProgressUpdate = (Action)Delegate.Remove(flow.ProgressUpdate, new Action(ProgressUpdate));
			}
		}

		private void Start()
		{
			simulation.Initialize();
			simulation.Run();
			m_activity_list = new Activity[2];
			activeFlows = new Dictionary<Flow, bool>();
			missionModule = base.transform.GetChild(0).gameObject;
			DuplicateMission();
			GetSteps();
			Flow flow = steps;
			flow.ProgressUpdate = (Action)Delegate.Combine(flow.ProgressUpdate, new Action(ProgressUpdate));
			resetAvailable = true;
		}

		public void CreateDrone(DroneRigData p_rig, int p_id = 0)
		{
			Drone d = factory.Instantiate(p_rig, p_async: true, p_isUser: true);
			simulation.RegisterDrone(d);
			simulation.PlaceDrone(d, p_id);
			if (playerDrone == null)
			{
				playerDrone = d;
				startingPosition = d.position;
				startingRotation = d.transform.rotation;
			}
			d.OnEvent.AddListener(delegate(DroneEvent p_event)
			{
				if (p_event.type == DroneEventType.Ready)
				{
					d.receiver.channel = p_id;
					EmitParticle("simulation-drone-create", d.position);
					DroneTrail[] componentsInChildren = d.GetComponentsInChildren<DroneTrail>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].gameObject.SetActive(value: false);
					}
					d.fc.allowPitch = false;
					d.fc.allowThrottle = false;
					d.fc.allowRoll = false;
					d.fc.allowYaw = false;
				}
			});
			((p_id <= 0) ? simulation.transmitters.Add<DroneRCTransmitter>() : simulation.transmitters.Add<DroneInputTransmitter>()).channel = p_id;
		}

		public void CreateDrone(TextAsset p_rig, int p_id = 0)
		{
			DroneRigData droneRigData = ScriptableObject.CreateInstance<DroneRigData>();
			droneRigData.Set(p_rig.bytes);
			CreateDrone(droneRigData, p_id);
		}

		public void RescueDrone(int p_id, int p_podium, List<object> p_al, List<UnityEngine.Object> p_ual)
		{
			Drone d = simulation.drones.Get(p_id);
			if (!d)
			{
				Debug.LogWarning("SimulationFlowModule> Failed to get drone [" + p_id + "]");
				return;
			}
			bool usePodium = Reflection<object>.Get<bool>(p_al, 0);
			Transform customLocation = Reflection<object>.Get<Transform>(p_ual, 0);
			d.gameObject.SetActive(value: false);
			EmitParticle("simulation-drone-create", d.position);
			Vector3 position = d.position;
			Vector3 b = (usePodium ? simulation.podiums.Get(p_podium).transform.position : customLocation.position);
			position.y = 0f;
			b.y = 0f;
			if (Vector3.Distance(position, b) > 0.1f)
			{
				Activity.RunOnce(delegate
				{
					if (usePodium || customLocation == null)
					{
						simulation.PlaceDrone(d, p_podium);
					}
					else
					{
						simulation.PlaceDrone(d, customLocation);
					}
					d.gameObject.SetActive(value: true);
				}, 0.1f);
				Activity.RunOnce(delegate
				{
					EmitParticle("simulation-drone-create", d.position);
				}, 0.5f);
			}
			else
			{
				if (usePodium || customLocation == null)
				{
					simulation.PlaceDrone(d, p_podium);
				}
				else
				{
					simulation.PlaceDrone(d, customLocation);
				}
				d.gameObject.SetActive(value: true);
			}
		}

		public void ActivateDrone(bool p_on)
		{
			if ((bool)playerDrone)
			{
				playerDrone.SetEnabled(p_on);
				playerDrone.fc.armed = p_on;
			}
		}

		public void EnableDroneControl(bool p_on)
		{
			if ((bool)playerDrone && (bool)playerDrone.fc)
			{
				playerDrone.fc.allowThrottle = p_on;
				playerDrone.fc.allowPitch = p_on;
				playerDrone.fc.allowYaw = p_on;
				playerDrone.fc.allowRoll = p_on;
			}
		}

		public void EnableDroneControl(bool p_throttle, bool p_pitch, bool p_yaw, bool p_roll)
		{
			if ((bool)playerDrone && (bool)playerDrone.fc)
			{
				playerDrone.fc.allowThrottle = p_throttle;
				playerDrone.fc.allowPitch = p_pitch;
				playerDrone.fc.allowYaw = p_yaw;
				playerDrone.fc.allowRoll = p_roll;
			}
		}

		public void DroneHover(float p_altitude)
		{
			if ((bool)playerDrone)
			{
				playerDrone.fc.armed = true;
				playerDrone.fc.SetMode(FlightControllerMode.Baro);
				playerDrone.fc.process.altitude.targetAltitude = p_altitude;
			}
		}

		public void DroneFree()
		{
			if ((bool)playerDrone)
			{
				playerDrone.fc.armed = true;
				playerDrone.fc.SetMode(FlightControllerMode.Acro);
			}
		}

		public void ResetDrone()
		{
			if ((bool)playerDrone)
			{
				playerDrone.transform.rotation = startingRotation;
				playerDrone.ResetPosition(startingPosition);
				playerDrone.transform.rotation = startingRotation;
				playerDrone.fc.allowPitch = false;
				playerDrone.fc.allowThrottle = false;
				playerDrone.fc.allowRoll = false;
				playerDrone.fc.allowYaw = false;
				playerDrone.fc.Reset();
				playerDrone.fc.armed = false;
			}
		}

		public void ReturnDrone()
		{
			if ((bool)playerDrone)
			{
				playerDrone.fc.armed = true;
				playerDrone.fc.SetMode(FlightControllerMode.Target);
				playerDrone.fc.modeProcess.target.targetPosition = startingPosition;
			}
		}

		public void SetDronePosition(Vector3 pos, Vector3 rot, float scale = 1f)
		{
			playerDrone.transform.rotation = Quaternion.Euler(rot);
			playerDrone.ResetPosition(pos);
			playerDrone.transform.rotation = Quaternion.Euler(rot);
			if (scale == 0f)
			{
				scale = 1f;
			}
			playerDrone.transform.localScale = new Vector3(scale, scale, scale);
		}

		public void SetDronePosition(Transform p_droneTransform)
		{
			playerDrone.transform.rotation = p_droneTransform.rotation;
			playerDrone.ResetPosition(p_droneTransform.position);
			playerDrone.transform.rotation = p_droneTransform.rotation;
			playerDrone.transform.localScale = p_droneTransform.localScale;
		}

		public void SetDJIModeRate(float p_Rate)
		{
			if ((bool)playerDrone)
			{
				playerDrone.fc.process.training.scale = p_Rate;
			}
		}

		public void StartTimer(int p_position, string p_label, string p_id)
		{
			if (!data)
			{
				Debug.LogWarning("SimulationFlowModule> Data not found!");
				return;
			}
			Activity activity = ((m_activity_list[p_position] == null) ? null : m_activity_list[p_position]);
			float t = (data.Contains(p_id) ? data.Get<float>(p_id) : 0f);
			ui.SetTimer(p_position, p_label, t);
			activity?.Stop();
			activity = ((Component)this).ActivityRun((Func<bool>)delegate
			{
				t += Time.deltaTime;
				ui.SetTimer(p_position, p_label, t);
				data.SetFloat(p_id, t);
				return true;
			}, 0f);
			m_activity_list[p_position] = activity;
		}

		public void ClearTimer(int p_position, string p_id)
		{
			if (!data)
			{
				Debug.LogWarning("SimulationFlowModule> Data not found!");
				return;
			}
			data.SetFloat(p_id, 0f);
			((m_activity_list[p_position] == null) ? null : m_activity_list[p_position])?.Stop();
			ui.ClearTimer(p_position);
		}

		public void StopTimer(int p_position)
		{
			if (p_position < m_activity_list.Length)
			{
				((m_activity_list[p_position] == null) ? null : m_activity_list[p_position])?.Stop();
			}
		}

		public void AddTimer(int p_position, Activity a)
		{
			m_activity_list[p_position] = a;
		}

		public void RemoveTimer(int p_position)
		{
			m_activity_list[p_position] = null;
		}

		public void CameraStart(Transform p_anchor = null)
		{
			Transform transform = (p_anchor ? p_anchor : cameraStart);
			if ((bool)transform && (bool)simulation)
			{
				DroneCamera droneCamera = simulation.cameras.Get(0);
				if ((bool)droneCamera)
				{
					droneCamera.SetNone();
					droneCamera.wasd.usePhysics = false;
					droneCamera.orbit.anchorRotation = transform.transform.localRotation;
					droneCamera.orbit.anchor = transform.transform.position;
					droneCamera.main.tag = "MainCamera";
				}
			}
		}

		public void DebugLerpDrone()
		{
			Drone drone = simulation.drones.Get(0);
			Tween.Add(p_to: new Vector3(132.7f, 12.8f, -120f), p_target: drone.transform, p_property: "position", p_duration: 4f, p_easing: Cubic.Out);
		}

		public void EmitParticle(string p_id, Vector3 p_position)
		{
			ParticleSystem particleSystem = library.Find<ParticleSystem>(p_id);
			if ((bool)particleSystem)
			{
				particleSystem = UnityEngine.Object.Instantiate(particleSystem);
				particleSystem.name = p_id;
				particleSystem.Play(withChildren: true);
				particleSystem.transform.parent = base.transform;
				particleSystem.transform.position = p_position;
				UnityEngine.Object.Destroy(particleSystem.gameObject, 2f);
			}
		}

		public override void OnPauseChange(DroneSimulationPauseMode p_from, DroneSimulationPauseMode p_to)
		{
			if (p_from != DroneSimulationPauseMode.Unpause)
			{
				_ = 1;
			}
			switch (p_to)
			{
			case DroneSimulationPauseMode.Unpause:
			{
				for (int k = 0; k < m_activity_list.Length; k++)
				{
					Activity activity2 = m_activity_list[k];
					if (activity2 != null)
					{
						activity2.paused = false;
					}
				}
				{
					foreach (KeyValuePair<Flow, bool> activeFlow in activeFlows)
					{
						activeFlow.Key.SetPause(f: false);
					}
					break;
				}
			}
			case DroneSimulationPauseMode.Pause:
			{
				for (int i = 0; i < m_activity_list.Length; i++)
				{
					Activity activity = m_activity_list[i];
					if (activity != null)
					{
						activity.paused = true;
					}
				}
				activeFlows.Clear();
				List<Flow> list = Hierarchy.FindAll<Flow>(base.transform);
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].active)
					{
						activeFlows[list[j]] = list[j].active;
						list[j].active = false;
						list[j].SetPause(f: true);
					}
				}
				break;
			}
			}
		}

		public void DuplicateMission()
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(missionModule, base.transform);
			gameObject.SetActive(value: false);
			gameObject.name = missionModule.gameObject.name;
			missionModuleDuplicate = gameObject;
		}

		public int ActiveStepIdx()
		{
			if (steps == null)
			{
				return -1;
			}
			int result = steps.pointer;
			if (steps.pointer >= steps.nodes.Count)
			{
				result = steps.nodes.Count - 1;
			}
			return result;
		}

		public Flow GetSteps()
		{
			if (missionModule == null)
			{
				return null;
			}
			steps = missionModule.transform.GetComponentInChildren<Flow>();
			main = steps;
			return steps;
		}

		public void ProgressUpdate()
		{
			ui.SaveStepTimes();
			activeCameraMode = simulation.cameras.Get(0).mode;
		}

		public void SetObjectives(string[] p_labels)
		{
			if (p_labels != null && p_labels.Length != 0)
			{
				ui.SetObjectives(p_labels);
			}
		}

		public void PauseSplineActor()
		{
			SplineActor splineActor = Hierarchy.Find<SplineActor>(base.transform);
			if (splineActor != null)
			{
				splineActor.Pause();
			}
		}

		public void UnpauseSplineActor()
		{
			SplineActor splineActor = Hierarchy.Find<SplineActor>(base.transform);
			if (splineActor != null)
			{
				splineActor.Resume();
			}
		}

		public void LoadVideos(Transform root)
		{
			foreach (Transform item in root)
			{
				LoadVideos(item);
			}
			Flow component = root.GetComponent<Flow>();
			if (!(component != null))
			{
				return;
			}
			foreach (FlowNode node in component.nodes)
			{
				FNVideoPlayer fNVideoPlayer = node as FNVideoPlayer;
				if (!(fNVideoPlayer == null))
				{
					ui.Notify("ui.screen.video-player@open", 0f, fNVideoPlayer.URL);
				}
			}
		}
	}
}

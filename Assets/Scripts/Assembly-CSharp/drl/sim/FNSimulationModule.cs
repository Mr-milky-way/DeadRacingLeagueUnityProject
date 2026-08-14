using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using drl.game;
using drl.sim.rci;
using thelab.core;

namespace drl.sim
{
	public class FNSimulationModule : FlowNode
	{
		public enum Mode
		{
			SimulationResume = 0,
			SimulationPause = 1,
			CreateDrone = 2,
			MoveDrone = 3,
			LockDrone = 4,
			AnimateDrone = 5,
			WaitDroneEuler = 6,
			WaitDronePrecision = 7,
			WatchDronePrecision = 8,
			WatchDroneEuler = 9,
			StartDroneBasicRotation = 10,
			WaitDroneDistance = 11,
			WatchDroneDistance = 12,
			PlaySound = 13,
			PlayParticles = 14,
			WatchDroneHeight = 15,
			BasicFlightController = 16,
			FlightControllerMode = 17,
			DronePower = 18,
			DroneControl = 19,
			ResetDrone = 20,
			ResetWaitDroneEuler = 21,
			SoftLockDrone = 22,
			FlightControllerParameters = 23,
			FlightControllerProcess = 24,
			StoreTime = 25,
			StoreCount = 26,
			SplineCondition = 27,
			SplineDroneReset = 28,
			MarkClosestObject = 29,
			ControllerTuningPreset = 30,
			WaitDroneTarget = 31,
			ResetAvailable = 32,
			SetActivePodium = 33,
			SetData = 34,
			__UI_ = 1000,
			UIFadeIn = 1001,
			UIFadeOut = 1002,
			UINPCOverlayShow = 1003,
			UINPCOverlayHide = 1004,
			UIWatchColliders = 1005,
			UIClearMarkers = 1006,
			UIHilightStepProgress = 1007,
			UIShowButtonNext = 1008,
			UIHideButtonNext = 1009,
			UISetTimer = 1010,
			UIWatchCollectables = 1011,
			UIIndicator = 1012,
			UINextObjective = 1013,
			UIStartCountdown = 1014,
			__Mission_ = 1200,
			MissionComplete = 1201,
			MissionStartTimer = 1202,
			MissionStopTimer = 1203,
			MissionClearTimers = 1204,
			MissionRescueDrone = 1205,
			MissionPlaceDrone = 1206,
			MissionMovePodium = 1207,
			MissionSetStep = 1208,
			MissionCompleteAudio = 1209,
			MissionTimerCondition = 1210,
			MissionStopPrecisionAudio = 1211,
			MissionResetBalloons = 1212,
			MissionFail = 1213,
			__Camera_ = 1400,
			CameraMove = 1401,
			CameraLOS = 1402,
			CameraLine = 1403,
			CameraFree = 1404,
			CameraFPV = 1405,
			CameraTPV = 1406,
			CameraTPVFree = 1407,
			CameraNone = 1408,
			SetTransitionFlags = 1409,
			CameraOrbit = 1410,
			CameraTPVSmooth = 1411,
			CameraOrbitMove = 1412,
			CameraFPVSmooth = 1413,
			CameraSplineAnimation = 1414,
			CameraTPVSideView = 1415,
			CameraTPVCuav = 1416,
			__Watch_ = 1600,
			WatchDroneFlip = 1601,
			EulerWatch = 1602,
			PoleWatch = 1603
		}

		[SerializeField]
		private SimulationFlowModule m_module;

		private float[] m_cumulativeDroneRotation = new float[3];

		private Quaternion m_droneRotationState;

		private float m_timer;

		private float m_rangeTimer;

		private Vector3 m_droneStartingPosition;

		private bool m_moduleStarted;

		private bool m_animationStarted;

		private float m_axisMeter;

		private bool m_init;

		private float m_cameraAnimationDuration = 3f;

		private bool m_updateUIMarker;

		private float m_conditionTimer;

		private Dictionary<object, bool> table;

		private bool m_conditionTimerFinished;

		private bool m_conditionTimerStopped;

		private float m_splineAnimationTimer;

		private float m_droneLastPoleAngle;

		private float m_cumulativePoleAngle;

		private static GameObject m_currentPole;

		private float m_distToEngage = 10f;

		private bool m_initialized;

		private bool m_startedTargetMode;

		private bool m_startedCameraAnimation;

		private List<Tween> m_CameraTweens = new List<Tween>();

		private float m_fastForwardAnimationDuration = 0.3f;

		private float m_fastForwardSpeed = 15f;

		private bool m_fastForwardStarted;

		private MotionBlurModel.Settings m_fastCameraBlurSettings;

		private bool m_skipStarted;

		private Transform m_closestObject;

		private FNCollider.Trigger m_closestTrigger = new FNCollider.Trigger();

		private Vector4 m_defaultTransitionSpeed;

		private Activity m_runningActivity;

		private bool m_droneInstantiationStarted;

		private bool m_startCountdownFinished;

		private static DRLApp m_app;

		public Mode mode;

		[SerializeField]
		private byte[] m_args_data;

		private List<object> m_args;

		[SerializeField]
		private List<UnityEngine.Object> m_argsUnity;

		private bool m_tpvSkippingToFPV;

		private Vector3 m_poleInitialForward;

		private bool m_trackPoleAngle;

		private Vector3 lastDronePos;

		private Vector3 lastDirToDrone;

		private Vector3 m_poleStartDir;

		public SimulationFlowModule module
		{
			get
			{
				if (!m_module)
				{
					return Hierarchy.FindReverse<SimulationFlowModule>(base.transform);
				}
				return m_module;
			}
		}

		public DRLApp app
		{
			get
			{
				if (!m_app)
				{
					return m_app = UnityEngine.Object.FindObjectOfType<DRLApp>();
				}
				return m_app;
			}
		}

		public List<object> args
		{
			get
			{
				if (m_args != null)
				{
					return m_args;
				}
				byte[] args_data = m_args_data;
				m_args = ((args_data == null) ? new List<object>() : ((args_data.Length == 0) ? new List<object>() : Serialize.FromBytes<List<object>>(args_data)));
				return m_args;
			}
			set
			{
				m_args = value;
				if (m_args == null)
				{
					m_args = new List<object>();
				}
				m_args_data = Serialize.ToBytes(m_args);
			}
		}

		public List<UnityEngine.Object> argsUnity
		{
			get
			{
				if (m_argsUnity != null)
				{
					return m_argsUnity;
				}
				return m_argsUnity = new List<UnityEngine.Object>();
			}
		}

		internal override bool hasContent => true;

		public void Notify(float p_delay, string p_event, params object[] p_args)
		{
			if ((bool)module && (bool)module.ui)
			{
				module.ui.Notify(p_event, p_delay, p_args);
			}
		}

		public void Notify(string p_event, params object[] p_args)
		{
			Notify(0f, p_event, p_args);
		}

		internal override void OnInitialize()
		{
			List<object> list = args;
			List<UnityEngine.Object> list2 = argsUnity;
			m_module = module;
			m_rangeTimer = 0f;
			m_timer = 0f;
			m_axisMeter = 0f;
			m_fastCameraBlurSettings.frameBlending = 1f;
			m_fastCameraBlurSettings.shutterAngle = 270f;
			m_fastCameraBlurSettings.sampleCount = 10;
			if (m_moduleStarted)
			{
				if (mode == Mode.WaitDroneEuler || mode == Mode.WatchDroneEuler)
				{
					m_droneRotationState = m_module.simulation.drones.Get(0).transform.localRotation;
				}
				m_moduleStarted = false;
			}
			if (mode == Mode.WatchDroneDistance || mode == Mode.WaitDroneDistance || mode == Mode.PoleWatch)
			{
				Drone drone = m_module.simulation.drones.Get(0);
				if ((bool)drone)
				{
					m_droneStartingPosition = drone.position;
				}
			}
			if (mode == Mode.PlaySound)
			{
				AudioClip clip = Reflection<object>.Get<AudioClip>(argsUnity, 0);
				bool num = Reflection<object>.Get<bool>(args, 0);
				AudioSource audio = ((DebugFlowModuleUI)module.ui).audio;
				audio.clip = clip;
				if (num)
				{
					audio.Play();
				}
			}
			switch (mode)
			{
			case Mode.WatchDronePrecision:
				if ((bool)module)
				{
					Gauge num3 = Reflection<object>.Get<Gauge>(list, 3);
					int p_position2 = ((num3 != Gauge.LeftGauge) ? 1 : 0);
					if (num3 == Gauge.None)
					{
						p_position2 = -1;
					}
					string p_up = app.model.storage.locale.Get("race-hud.training.gauge.perfect", "PERFECT");
					string p_middle = app.model.storage.locale.Get("race-hud.training.gauge.distance", "DISTANCE");
					string p_down = app.model.storage.locale.Get("race-hud.training.gauge.toofar", "TOO FAR");
					Reflection<object>.Get<SphereCollider>(list2, 0, null).enabled = false;
					Notify("fn.mission.precision@start");
					module.ui.SetGauge(p_position2, p_up, p_middle, p_down);
					module.ui.SetGauge(p_position2, 0f);
				}
				break;
			case Mode.WatchDroneDistance:
				if ((bool)module)
				{
					int p_position4 = Reflection<object>.Get<int>(list, 0);
					module.ui.SetGauge(p_position4, GetGoalDistance(list2, list) + "m", "", "0m");
					module.ui.SetGauge(p_position4, 0f);
					module.ui.SetGauge(p_position4, p_flag: true);
				}
				break;
			case Mode.WaitDroneEuler:
				if ((bool)module && Reflection<object>.Get<bool>(list, 15))
				{
					switch (Reflection<object>.Get<Gauge>(list, 16))
					{
					case Gauge.LeftGauge:
						module.ui.SetGauge(0, 0f);
						module.ui.SetGauge(0, p_flag: true);
						break;
					case Gauge.RightGauge:
						module.ui.SetGauge(1, 0f);
						module.ui.SetGauge(1, p_flag: true);
						break;
					case Gauge.LeftPrecisionGauge:
						module.ui.SetPrecisionGauge(0, GetGoalAngle(list) * 2 + "°", "", "0°", GetAnglePercisionMiddleRatio(list));
						module.ui.SetPrecisionGauge(0, 0f);
						module.ui.SetPrecisionGauge(0, p_flag: true);
						break;
					case Gauge.RightPrecisionGauge:
						module.ui.SetPrecisionGauge(1, GetGoalAngle(list) * 2 + "°", "", "0°", GetAnglePercisionMiddleRatio(list));
						module.ui.SetPrecisionGauge(1, 0f);
						module.ui.SetPrecisionGauge(1, p_flag: true);
						break;
					}
				}
				break;
			case Mode.WaitDroneDistance:
				if ((bool)module && Reflection<object>.Get<bool>(list, 13))
				{
					switch (Reflection<object>.Get<Gauge>(list, 14))
					{
					case Gauge.LeftGauge:
						module.ui.SetGauge(0, GetGoalDistance(list2, list) + "m", "", "0m");
						module.ui.SetGauge(0, 0f);
						module.ui.SetGauge(0, p_flag: true);
						break;
					case Gauge.RightGauge:
						module.ui.SetGauge(1, GetGoalDistance(list2, list) + "m", "", "0m");
						module.ui.SetGauge(1, 0f);
						module.ui.SetGauge(1, p_flag: true);
						break;
					case Gauge.LeftPrecisionGauge:
						module.ui.SetPrecisionGauge(0, GetGoalDistance(list2, list) * 2 + "m", GetGoalDistance(list2, list) + "m", "0m", GetDistancePercisionMiddleRatio(list, list2));
						module.ui.SetPrecisionGauge(0, 0f);
						module.ui.SetPrecisionGauge(0, p_flag: true);
						break;
					case Gauge.RightPrecisionGauge:
						module.ui.SetPrecisionGauge(1, GetGoalDistance(list2, list) * 2 + "m", GetGoalDistance(list2, list) + "m", "0m", GetDistancePercisionMiddleRatio(list, list2));
						module.ui.SetPrecisionGauge(1, 0f);
						module.ui.SetPrecisionGauge(1, p_flag: true);
						break;
					}
				}
				break;
			case Mode.WatchDroneHeight:
				if ((bool)module)
				{
					module.ui.SetPrecisionGauge(1, p_flag: true);
					module.ui.SetPrecisionGauge(1, "LOW", "PERFECT", "HIGH", 0f);
				}
				break;
			case Mode.UIWatchColliders:
			{
				if (!module || !module.ui)
				{
					break;
				}
				object obj = Reflection<object>.Get(list, list.Count - 1);
				bool flag = Reflection<object>.Get(list, 4, p_default: false);
				Reflection<object>.Get(list, 6, 1);
				Dictionary<object, bool> dictionary;
				if (!(obj is Dictionary<object, bool>))
				{
					list.Add(dictionary = new Dictionary<object, bool>());
				}
				else
				{
					dictionary = obj as Dictionary<object, bool>;
				}
				dictionary.Clear();
				int num4 = flow.nodes.IndexOf(this);
				num4++;
				if (num4 < 0 || num4 >= flow.nodes.Count)
				{
					break;
				}
				FNCollider fNCollider = flow.nodes[num4] as FNCollider;
				module.ui.ClearMarkers();
				m_updateUIMarker = false;
				if ((bool)fNCollider)
				{
					fNCollider.Reset();
					if (flag && fNCollider.criteria == BatchCriteria.Forward)
					{
						bool flag2 = true;
						foreach (FNCollider.Trigger trigger in fNCollider.triggers)
						{
							if (flag2)
							{
								flag2 = false;
								continue;
							}
							GameObject gameObject = trigger.target.gameObject;
							IActivable activable = gameObject.GetComponent<IActivable>();
							if (activable == null)
							{
								TrainingElementChild component = gameObject.GetComponent<TrainingElementChild>();
								if (!component)
								{
									Debug.LogWarning("No reliable activable object found");
								}
								else
								{
									activable = component.TrainingElement;
								}
							}
							if (activable != null)
							{
								activable.SetActive(active: false);
							}
							else
							{
								trigger.target.gameObject.SetActive(value: false);
							}
						}
					}
				}
				table = dictionary;
				break;
			}
			case Mode.UIStartCountdown:
				m_startCountdownFinished = false;
				break;
			case Mode.CameraSplineAnimation:
			{
				SplineActor splineActor = Reflection<object>.Get<SplineActor>(list2, 0);
				module.simulation.cameras.Get(0).follow.target = splineActor.transform;
				splineActor.auto = true;
				module.simulation.cameras.SetOther(0, 0);
				module.ui.ShowSkip(dmv: true);
				break;
			}
			case Mode.CameraFPV:
			case Mode.CameraTPV:
			case Mode.CameraTPVSmooth:
			case Mode.CameraFPVSmooth:
			{
				DroneCamera droneCamera = module.simulation.cameras.Get(0);
				m_defaultTransitionSpeed = new Vector4(droneCamera.orbit.speed.distance, droneCamera.orbit.speed.angle, droneCamera.orbit.speed.anchor, droneCamera.orbit.speed.anchor);
				break;
			}
			case Mode.MissionCompleteAudio:
				if ((bool)module)
				{
					module.ui.PlayMissionCompleteAudio();
				}
				break;
			case Mode.MissionResetBalloons:
			{
				Transform trainingElements = Reflection<object>.Get<Transform>(list2, 0, null);
				bool onlyFirstActive = Reflection<object>.Get(list, 0, p_default: false);
				ResetBalloons(trainingElements, onlyFirstActive);
				break;
			}
			case Mode.UISetTimer:
				if ((bool)module)
				{
					int num2 = Reflection<object>.Get<int>(list, 0);
					int p_position = Reflection<object>.Get<int>(list, 1);
					string p_label = Reflection<object>.Get<string>(list, 2);
					module.ui.SetTimer(p_position, p_label, num2);
				}
				break;
			case Mode.MissionTimerCondition:
			{
				if (!module)
				{
					break;
				}
				m_conditionTimer = 0f;
				m_conditionTimerFinished = false;
				m_conditionTimerStopped = false;
				float duration = Reflection<object>.Get<float>(list, 0);
				bool countdown = Reflection<object>.Get<bool>(list, 1);
				int position = (countdown ? 1 : 0);
				m_runningActivity = Activity.Run((Func<bool>)delegate
				{
					if (!module.ui)
					{
						return false;
					}
					m_conditionTimer += Time.deltaTime;
					float num5 = (countdown ? (duration - m_conditionTimer) : m_conditionTimer);
					if (num5 < 0.1f)
					{
						num5 = 0f;
					}
					module.ui.SetTimer(position, "", num5);
					if (m_conditionTimer >= duration)
					{
						m_conditionTimerFinished = true;
						module.ui.SetTimer(position, "", countdown ? 0f : duration);
					}
					if (m_conditionTimerStopped)
					{
						m_runningActivity.Stop();
						module.RemoveTimer(position);
						return false;
					}
					return m_conditionTimer < duration;
				}, 0f, false);
				module.AddTimer(position, m_runningActivity);
				break;
			}
			case Mode.MissionStopPrecisionAudio:
				Notify("fn.mission.precision@stop");
				break;
			case Mode.StoreTime:
			{
				FNMissionScoreType fNMissionScoreType2 = Reflection<object>.Get<FNMissionScoreType>(list, 0);
				UIHUDRequirements uIRequirements = module.simulation.UIRequirements;
				if (uIRequirements == null)
				{
					Debug.LogError("Can't find UIHUDRequirments!");
					break;
				}
				int p_position3 = 0;
				if (fNMissionScoreType2 >= FNMissionScoreType.TimeOut0 && uIRequirements.timer2Required)
				{
					p_position3 = 1;
				}
				module.data.Set(fNMissionScoreType2.ToString(), module.ui.GetTimerValue(p_position3));
				break;
			}
			case Mode.StoreCount:
			{
				FNMissionScoreType fNMissionScoreType = Reflection<object>.Get<FNMissionScoreType>(list, 0);
				module.data.Set(fNMissionScoreType.ToString(), (float)module.ui.GetSteps());
				break;
			}
			case Mode.MissionRescueDrone:
				m_timer = 0.5f;
				break;
			case Mode.SplineDroneReset:
			{
				SplineComponent splineComponent = Reflection<object>.Get<SplineComponent>(list2, 0);
				Drone drone2 = m_module.simulation.drones.Get(0);
				float p_length = 0f;
				if ((bool)drone2)
				{
					drone2.position = splineComponent.positions.GetClosestValue(drone2.position, ref p_length, 0.2f);
				}
				break;
			}
			case Mode.ResetAvailable:
				if ((bool)module)
				{
					bool resetAvailable = Reflection<object>.Get(list, 0, p_default: true);
					module.resetAvailable = resetAvailable;
				}
				break;
			case Mode.SetActivePodium:
				if ((bool)module)
				{
					int p_index = Reflection<object>.Get(list, 0, 0);
					module.droneStart.transform.position = module.simulation.podiums.Get(p_index).spawn.transform.position;
					module.droneStart.transform.rotation = module.simulation.podiums.Get(p_index).spawn.transform.rotation;
				}
				break;
			case Mode.SetData:
				if ((bool)module)
				{
					string k = Reflection<object>.Get(list, 0, "");
					int v = Reflection<object>.Get(list, 1, 0);
					module.data.SetInt(k, v);
				}
				break;
			}
		}

		public override FlowStatus OnSkip()
		{
			FlowStatus result = FlowStatus.Complete;
			List<object> list = args;
			if (list == null)
			{
				list = new List<object>();
			}
			List<UnityEngine.Object> p_list = argsUnity;
			DroneSimulation simulation = m_module.simulation;
			Drone drone = simulation.drones.Get(0);
			switch (mode)
			{
			case Mode.AnimateDrone:
			{
				if (!drone)
				{
					break;
				}
				Transform transform2 = Reflection<object>.Get<Transform>(p_list, 0);
				Vector3 position = Reflection<object>.Get<Vector3>(list, 0);
				Quaternion rotation = Quaternion.identity;
				if ((bool)transform2)
				{
					position = transform2.position;
					if (Reflection<object>.Get<bool>(list, 2))
					{
						rotation = transform2.localRotation;
					}
				}
				drone.position = position;
				drone.transform.rotation = rotation;
				return result;
			}
			case Mode.CameraTPVSmooth:
			{
				if (!module)
				{
					break;
				}
				DroneCamera droneCamera2 = module.simulation.cameras.Get(0);
				Tween.Kill(droneCamera2.orbit, "distance");
				Tween.Kill(droneCamera2.orbit, "anchor");
				Tween.Kill(droneCamera2.orbit, "angle");
				Tween.Kill(droneCamera2.orbit, "anchorRotation");
				droneCamera2.mode = DroneCameraModeType.TPVMissions;
				foreach (FlowNode node in flow.nodes)
				{
					FNSimulationModule fNSimulationModule = node as FNSimulationModule;
					if ((bool)fNSimulationModule && (fNSimulationModule.mode == Mode.CameraFPVSmooth || fNSimulationModule.mode == Mode.CameraFPV))
					{
						m_tpvSkippingToFPV = true;
						droneCamera2.orbit.SetTransitionMask(OrbitTransform.TransitionMask.Snap);
						return result;
					}
				}
				droneCamera2.orbit.SetTransitionMask(OrbitTransform.TransitionMask.Snap);
				droneCamera2.orbit.distance = 0.4f;
				droneCamera2.orbit.anchor = new Vector3(drone.position.x, drone.position.y + droneCamera2.follow.offset.y, drone.position.z);
				droneCamera2.orbit.angle = new Vector2(0f, 12f);
				droneCamera2.orbit.anchorRotation = Quaternion.Euler(new Vector3(0f, drone.transform.rotation.eulerAngles.y, 0f));
				droneCamera2.follow.target = drone.transform;
				droneCamera2.follow.offset = new Vector3(0f, 0.1f, 0f);
				droneCamera2.orbit.SetTransitionMask(OrbitTransform.TransitionMask.SmoothTPV);
				droneCamera2.follow.flags = (OrbitFollowInput.Flag)23;
				droneCamera2.fx.ppb.profile.motionBlur.Reset();
				droneCamera2.orbit.SetTransitionSpeed(m_defaultTransitionSpeed);
				return result;
			}
			case Mode.CameraFPVSmooth:
			{
				module.simulation.cameras.SetFPVSmooth(0, 0, 0f);
				DroneCamera camera = module.simulation.cameras.Get(0);
				camera.follow.flags = OrbitFollowInput.Flag.All;
				camera.follow.target = drone.body.frame.camera.pivot;
				camera.fx.ppb.profile.motionBlur.Reset();
				camera.orbit.SetTransitionSpeed(m_defaultTransitionSpeed);
				this.TimerRunOnce(delegate
				{
					camera.orbit.SetTransitionMask(OrbitTransform.TransitionMask.Snap);
				}, 0.8f);
				return result;
			}
			case Mode.CameraMove:
			{
				DroneCamera droneCamera = simulation.cameras.Get(0);
				if ((bool)droneCamera)
				{
					Tween.Kill(droneCamera.orbit, "anchorRotation");
					Tween.Kill(droneCamera.orbit, "angle");
					Tween.Kill(droneCamera.orbit, "anchor");
				}
				Transform transform = Reflection<object>.Get<Transform>(p_list, 0, null);
				Vector3 anchor = (transform ? transform.position : droneCamera.orbit.anchor);
				Quaternion anchorRotation = (transform ? transform.rotation : droneCamera.orbit.anchorRotation);
				droneCamera.orbit.SetTransitionMask(OrbitTransform.TransitionMask.Snap);
				droneCamera.follow.offset = new Vector3(0f, 0.1f, 0f);
				droneCamera.orbit.anchorRotation = anchorRotation;
				droneCamera.orbit.angle = Vector3.zero;
				droneCamera.orbit.anchor = anchor;
				droneCamera.fx.ppb.profile.motionBlur.Reset();
				return result;
			}
			case Mode.CameraSplineAnimation:
				Reflection<object>.Get<SplineActor>(p_list, 0, null).auto = false;
				return result;
			}
			return OnUpdate();
		}

		internal override FlowStatus OnUpdate()
		{
			if (!m_module)
			{
				Debug.LogError("FNSimulationModule: Simulation flow module not found!\n from: " + base.transform.name + ", node index: " + flow.pointer + ", mode: " + mode);
				return FlowStatus.Complete;
			}
			FlowStatus result = FlowStatus.Complete;
			List<object> list = args;
			if (list == null)
			{
				list = new List<object>();
			}
			List<UnityEngine.Object> list2 = argsUnity;
			DroneSimulation simulation = m_module.simulation;
			Drone drone = simulation.drones.Get(0);
			DronePhysicsSettings dronePhysicsSettings = null;
			switch (mode)
			{
			case Mode.CreateDrone:
				if (drone != null)
				{
					dronePhysicsSettings = drone.GetComponentInChildren<DronePhysicsSettings>();
				}
				if (drone != null && drone.fc != null && dronePhysicsSettings != null)
				{
					FlightControllerMode flightControllerMode = FlightControllerMode.Beginner;
					List<FCMode> flightModes = app.arguments.game.mission.flightModes;
					if (flightModes.Count != 0)
					{
						flightControllerMode = flightModes[0] switch
						{
							FCMode.Beginner => FlightControllerMode.Beginner, 
							FCMode.Intermediate => FlightControllerMode.Pro, 
							FCMode.Pro => FlightControllerMode.Pro, 
							_ => FlightControllerMode.Beginner, 
						};
						dronePhysicsSettings.InitializeSettings(flightControllerMode == FlightControllerMode.Beginner);
						drone.fc.SetMode(flightControllerMode);
					}
					break;
				}
				if (!m_droneInstantiationStarted)
				{
					m_droneInstantiationStarted = true;
					TextAsset textAsset = Reflection<object>.Get<TextAsset>(list2, 0);
					if ((bool)textAsset)
					{
						module.CreateDrone(textAsset);
						Notify("fn.mission.drone.spawn");
					}
					GameObject gameObject = new GameObject();
					gameObject.transform.parent = module.transform;
					gameObject.name = "drone-start";
					gameObject.transform.position = module.simulation.podiums.Get(0).spawn.transform.position;
					gameObject.transform.rotation = module.simulation.podiums.Get(0).spawn.transform.rotation;
					module.droneStart = gameObject;
				}
				if (drone == null || drone.fc == null || dronePhysicsSettings == null)
				{
					return FlowStatus.Running;
				}
				return FlowStatus.Complete;
			case Mode.DronePower:
				if ((bool)drone)
				{
					if (!drone.ready)
					{
						return FlowStatus.Running;
					}
					drone.SetEnabled((bool)list[0]);
					drone.fc.armed = (bool)list[0];
					if (drone.fc.armed)
					{
						drone.fc.Reset();
					}
				}
				break;
			case Mode.DroneControl:
				if ((bool)drone && (bool)drone.fc)
				{
					drone.fc.allowThrottle = (bool)list[0];
					drone.fc.allowPitch = (bool)list[1];
					drone.fc.allowYaw = (bool)list[2];
					drone.fc.allowRoll = (bool)list[3];
					drone.fc.debugPitch = ((list.Count > 4) ? ((float)list[4]) : 0f);
					drone.fc.debugRoll = ((list.Count > 5) ? ((float)list[5]) : 0f);
					drone.fc.debugYaw = ((list.Count > 6) ? ((float)list[6]) : 0f);
					drone.fc.debugThrottle = ((list.Count > 7) ? ((float)list[7]) : 0f);
				}
				break;
			case Mode.FlightControllerMode:
			{
				if (!drone)
				{
					break;
				}
				if (m_startedTargetMode)
				{
					if ((Transform)list2[0] == null)
					{
						return FlowStatus.Complete;
					}
					if (Vector3.Distance(drone.position, ((Transform)list2[0]).position) < 0.4f && Quaternion.Angle(drone.transform.rotation, ((Transform)list2[0]).rotation) < 5f)
					{
						m_startedTargetMode = false;
						return FlowStatus.Complete;
					}
					return FlowStatus.Running;
				}
				FlightControllerMode flightControllerMode2 = Reflection<object>.Get<FlightControllerMode>(list, 0);
				switch (flightControllerMode2)
				{
				case FlightControllerMode.Baro:
					drone.fc.process.altitude.targetAltitude = (float)list[1];
					break;
				case FlightControllerMode.Target:
				{
					drone.fc.modeProcess.target.target = (Transform)list2[0];
					float num5 = Reflection<object>.Get(list, 7, 1f);
					float num6 = Reflection<object>.Get(list, 8, 0f);
					m_startedTargetMode = true;
					if (num6 <= 0f)
					{
						drone.fc.modeProcess.target.overrideSignalWeight = num5;
						break;
					}
					drone.fc.modeProcess.target.overrideSignalWeight = 0f;
					Tween.Add(drone.fc.modeProcess.target, "overrideSignalWeight", num5, num6, Cubic.InOut);
					break;
				}
				}
				if (app.model.storage.state.player.activeFCModeMissions != FCMode.None && app.arguments.game.mission.flightModes.Contains(FCMode.Intermediate) && app.arguments.game.mission.flightModes.Contains(FCMode.Pro))
				{
					switch (app.model.storage.state.player.activeFCModeMissions)
					{
					case FCMode.Beginner:
						flightControllerMode2 = FlightControllerMode.Beginner;
						break;
					case FCMode.Intermediate:
						flightControllerMode2 = FlightControllerMode.Intermediate;
						break;
					case FCMode.Pro:
						flightControllerMode2 = FlightControllerMode.Pro;
						break;
					}
				}
				drone.fc.SetMode(flightControllerMode2);
				if (flightControllerMode2 == FlightControllerMode.Intermediate || flightControllerMode2 == FlightControllerMode.Pro)
				{
					this.TimerRunOnce(delegate
					{
						Time.fixedDeltaTime = 0.02f;
					}, 0.2f);
				}
				if (m_startedTargetMode)
				{
					return FlowStatus.Running;
				}
				break;
			}
			case Mode.FlightControllerParameters:
			{
				if (!drone)
				{
					break;
				}
				bool num7 = Reflection<object>.Get(list, 1, p_default: false);
				bool flag3 = Reflection<object>.Get(list, 2, p_default: false);
				bool flag4 = Reflection<object>.Get(list, 7, p_default: false);
				Reflection<object>.Get(list, 8, p_default: false);
				if (num7)
				{
					drone.fc.parameters.altitudeAngle = 30f;
					drone.fc.parameters.altitudeSpeed = 2f;
					drone.fc.parameters.djiAngleMin = 30f;
					drone.fc.parameters.djiAngleMax = 30f;
					drone.fc.parameters.djiSpeedMin = 9f;
					drone.fc.parameters.djiSpeedMax = 9f;
					drone.fc.parameters.targetAngle = 60f;
					drone.fc.parameters.targetError = 0.05f;
					drone.fc.parameters.targetSpeed = 8f;
					drone.fc.parameters.targetScale = 1.6f;
					drone.fc.parameters.trainingScale = 1f;
					drone.fc.parameters.limiterAngle = 45f;
				}
				else if (flag3)
				{
					drone.fc.parameters.altitudeAngle = 30f;
					drone.fc.parameters.altitudeSpeed = 0f;
					drone.fc.parameters.djiAngleMin = 50f;
					drone.fc.parameters.djiAngleMax = 50f;
					drone.fc.parameters.djiSpeedMin = 45f;
					drone.fc.parameters.djiSpeedMax = 45f;
					drone.fc.parameters.targetAngle = 80f;
					drone.fc.parameters.targetError = 0.05f;
					drone.fc.parameters.targetSpeed = 20f;
					drone.fc.parameters.targetScale = 1.6f;
					drone.fc.parameters.trainingScale = 1f;
					drone.fc.parameters.limiterAngle = 45f;
				}
				else if (flag4)
				{
					drone.fc.parameters.altitudeAngle = 30f;
					drone.fc.parameters.altitudeSpeed = 4f;
					drone.fc.parameters.djiAngleMin = 40f;
					drone.fc.parameters.djiAngleMax = 40f;
					drone.fc.parameters.djiSpeedMin = 18f;
					drone.fc.parameters.djiSpeedMax = 18f;
					drone.fc.parameters.targetAngle = 70f;
					drone.fc.parameters.targetError = 0.05f;
					drone.fc.parameters.targetSpeed = 14f;
					drone.fc.parameters.targetScale = 1.6f;
					drone.fc.parameters.trainingScale = 1f;
					drone.fc.parameters.limiterAngle = 45f;
				}
				else
				{
					switch (Reflection<object>.Get<FlightControllerMode>(list, 0))
					{
					case FlightControllerMode.Baro:
						drone.fc.parameters.altitudeAngle = (float)list[4];
						drone.fc.parameters.altitudeSpeed = ((list.Count > 5) ? ((float)list[5]) : 0f);
						drone.fc.parameters.trainingScale = (float)list[3];
						break;
					case FlightControllerMode.Target:
						drone.fc.parameters.targetSpeed = ((list.Count > 5) ? ((float)list[5]) : 0f);
						drone.fc.parameters.targetError = ((list.Count > 6) ? ((float)list[6]) : 0.05f);
						drone.fc.parameters.targetScale = ((list.Count > 9) ? ((float)list[9]) : 1f);
						break;
					case FlightControllerMode.Training:
						drone.fc.parameters.trainingScale = (float)list[3];
						break;
					case FlightControllerMode.DJI:
					case FlightControllerMode.Stabilized:
						drone.fc.parameters.trainingScale = (float)list[3];
						drone.fc.parameters.djiAngleMin = (float)list[4];
						drone.fc.parameters.djiSpeedMin = (float)list[5];
						drone.fc.parameters.djiAngleMax = (float)list[4];
						drone.fc.parameters.djiSpeedMax = (float)list[5];
						break;
					case FlightControllerMode.Intermediate:
						drone.fc.parameters.limiterAngle = (float)list[4];
						break;
					case FlightControllerMode.Horizon:
						drone.fc.parameters.trainingScale = (float)list[3];
						drone.fc.parameters.djiAngleMin = 0f;
						drone.fc.parameters.djiSpeedMin = (float)list[5];
						drone.fc.parameters.djiAngleMax = 80f;
						drone.fc.parameters.djiSpeedMax = (float)list[5];
						break;
					}
				}
				drone.fc.ApplyParameters();
				break;
			}
			case Mode.FlightControllerProcess:
				if ((bool)drone)
				{
					FlightControllerProcess flightControllerProcess = Reflection<object>.Get<FlightControllerProcess>(list, 0);
					switch (flightControllerProcess)
					{
					case FlightControllerProcess.Altitude:
						drone.fc.process.altitude.targetAltitude = Reflection<object>.Get(list, 2, 0f);
						drone.fc.process.altitude.target = (Transform)list2[0];
						break;
					case FlightControllerProcess.Limiter:
						drone.fc.process.limiter.limit = Reflection<object>.Get(list, 3, 0f);
						drone.fc.process.limiter.lookahead = Reflection<object>.Get(list, 4, 0f);
						break;
					case FlightControllerProcess.Level:
						drone.fc.process.level.delay = Reflection<object>.Get(list, 5, 0f);
						break;
					}
					drone.fc.SetProcess(flightControllerProcess, Reflection<object>.Get(list, 1, p_default: false));
				}
				break;
			case Mode.ControllerTuningPreset:
				if ((bool)drone)
				{
					ControllerStateType controllerStateType = RCI.GetControllerStateType(ControllerStateType.XBox);
					switch (Reflection<object>.Get<FCProfileData.Betaflight.PresetType>(list, 0))
					{
					case FCProfileData.Betaflight.PresetType.Low:
						drone.fc.profile.SetPreset(FCProfileData.Betaflight.LowPresets[controllerStateType]);
						break;
					case FCProfileData.Betaflight.PresetType.Medium:
						drone.fc.profile.SetPreset(FCProfileData.Betaflight.MediumPresets[controllerStateType]);
						break;
					case FCProfileData.Betaflight.PresetType.High:
						drone.fc.profile.SetPreset(FCProfileData.Betaflight.HighPresets[controllerStateType]);
						break;
					case FCProfileData.Betaflight.PresetType.Training:
						drone.fc.profile.SetPreset(FCProfileData.Betaflight.TrainingPresets[controllerStateType]);
						break;
					default:
					{
						drone.fc.profile.SetPreset(FCProfileData.Betaflight.TrainingPresets[controllerStateType]);
						FCProfileData active = app.model.storage.state.player.settings.tuning.GetActive();
						drone.fc.profile = active;
						break;
					}
					}
				}
				break;
			case Mode.LockDrone:
				if ((bool)drone)
				{
					RigidbodyConstraints constraints = (RigidbodyConstraints)Reflection<object>.Get<int>(list, 0);
					drone.rigidbody.rb.constraints = constraints;
				}
				break;
			case Mode.SoftLockDrone:
			{
				if (!drone)
				{
					break;
				}
				FCSoftlockProcess softlock = drone.fc.process.softlock;
				if ((bool)softlock)
				{
					softlock.lockAltitude = (bool)list[0];
					softlock.lockHeading = (bool)list[1];
					softlock.lockGlobalX = (bool)list[2];
					softlock.lockGlobalZ = (bool)list[3];
					softlock.lockLocalX = (bool)list[4];
					softlock.lockLocalZ = (bool)list[5];
					drone.fc.softLock = softlock.lockAltitude || softlock.lockHeading || softlock.lockGlobalX || softlock.lockGlobalZ || softlock.lockLocalX || softlock.lockLocalZ;
					drone.fc.softLockOffset = (float)list[6];
					softlock.speedLimit = ((list.Count > 7) ? ((float)list[7]) : 0f);
					softlock.target = ((list2.Count < 1) ? null : (list2[0] as Transform));
					if (softlock.target == null)
					{
						softlock.LockToCurrent();
					}
				}
				break;
			}
			case Mode.SimulationResume:
				if ((bool)simulation)
				{
					simulation.pause = DroneSimulationPauseMode.Unpause;
				}
				break;
			case Mode.SimulationPause:
				if ((bool)simulation)
				{
					DroneSimulationPauseMode pause = Reflection<object>.Get<DroneSimulationPauseMode>(list, 0);
					simulation.pause = pause;
				}
				break;
			case Mode.AnimateDrone:
			{
				if (!drone)
				{
					break;
				}
				if (RCI.GetAnyButtonUp())
				{
					OnSkip();
					return FlowStatus.Complete;
				}
				Transform transform4 = Reflection<object>.Get<Transform>(list2, 0);
				Vector3 vector = Reflection<object>.Get<Vector3>(list, 0);
				float num2 = Reflection<object>.Get<float>(list, 1);
				bool flag2 = Reflection<object>.Get<bool>(list, 2);
				bool num3 = Reflection<object>.Get<bool>(list, 3);
				if (!transform4)
				{
					flag2 = false;
				}
				vector = (transform4 ? transform4.position : vector);
				Quaternion p_to = (flag2 ? transform4.localRotation : Quaternion.identity);
				if (!m_animationStarted)
				{
					Tween tween = Tween.Add(drone.transform, "position", vector, num2, Cubic.InOut);
					tween.onComplete = (Action<Tween>)Delegate.Combine(tween.onComplete, (Action<Tween>)delegate
					{
						m_animationStarted = false;
					});
					if (flag2)
					{
						Tween tween2 = Tween.Add(drone.transform, "localRotation", p_to, num2, Cubic.InOut);
						tween2.onComplete = (Action<Tween>)Delegate.Combine(tween2.onComplete, (Action<Tween>)delegate
						{
							m_animationStarted = false;
						});
					}
					m_animationStarted = true;
				}
				if (num3 && m_timer < num2)
				{
					m_timer += Time.deltaTime;
					if (m_timer > num2)
					{
						m_animationStarted = false;
					}
					return FlowStatus.Running;
				}
				break;
			}
			case Mode.WaitDroneEuler:
				if ((bool)drone)
				{
					return WaitDroneEuler(list, drone);
				}
				break;
			case Mode.WatchDroneEuler:
				if ((bool)drone)
				{
					SetDroneRotationMeter(list, drone);
					return FlowStatus.Running;
				}
				break;
			case Mode.WatchDroneDistance:
				if ((bool)drone)
				{
					if (!SetDroneDistanceMeter(list, list2, drone))
					{
						return FlowStatus.Running;
					}
					return FlowStatus.Complete;
				}
				break;
			case Mode.WaitDroneDistance:
				if ((bool)drone)
				{
					return WaitDroneDistance(list2, list, drone);
				}
				break;
			case Mode.PlaySound:
			{
				bool num4 = Reflection<object>.Get<bool>(list, 0);
				AudioSource audio = ((DebugFlowModuleUI)module.ui).audio;
				if (!num4)
				{
					audio.Play();
					return FlowStatus.Complete;
				}
				if (!audio.isPlaying)
				{
					return FlowStatus.Complete;
				}
				return FlowStatus.Running;
			}
			case Mode.PlayParticles:
				if ((bool)drone)
				{
					Transform transform = Reflection<object>.Get<Transform>(list2, 0);
					string p_id = Reflection<object>.Get<string>(list, 0);
					bool flag = Reflection<object>.Get<bool>(list, 1);
					if (transform == null || flag)
					{
						transform = drone.transform;
					}
					module.EmitParticle(p_id, transform.position);
				}
				break;
			case Mode.WatchDroneHeight:
				if ((bool)drone)
				{
					Transform p_lower = Reflection<object>.Get<Transform>(list2, 0);
					Transform p_upper = Reflection<object>.Get<Transform>(list2, 1);
					SetDroneHeightMeter(p_lower, p_upper, drone);
					return FlowStatus.Running;
				}
				break;
			case Mode.ResetDrone:
				if ((bool)drone)
				{
					drone.ResetPosition();
				}
				break;
			case Mode.ResetWaitDroneEuler:
			{
				if (!drone)
				{
					break;
				}
				FNSimulationModule[] componentsInChildren = module.transform.GetComponentsInChildren<FNSimulationModule>();
				foreach (FNSimulationModule fNSimulationModule in componentsInChildren)
				{
					if (fNSimulationModule.mode == Mode.WaitDroneEuler)
					{
						fNSimulationModule.ResetDroneRotationAccumulators(drone);
					}
				}
				break;
			}
			case Mode.MarkClosestObject:
			{
				Transform transform2 = Reflection<object>.Get<Transform>(list2, 0);
				float num = Reflection<object>.Get<float>(list, 0);
				if ((bool)m_closestTrigger.target && m_closestTrigger.target.gameObject.activeInHierarchy)
				{
					module.ui.UpdateMarker(m_closestTrigger, 2);
				}
				if (!transform2)
				{
					return FlowStatus.Running;
				}
				if (m_timer <= num)
				{
					m_timer += Time.deltaTime;
				}
				else
				{
					ColliderEventComponent component = transform2.GetChild(0).GetComponent<ColliderEventComponent>();
					float sqrMagnitude = (transform2.GetChild(0).position - drone.transform.position).sqrMagnitude;
					foreach (Transform item in transform2)
					{
						if (item.gameObject.activeInHierarchy && (item.position - drone.transform.position).sqrMagnitude < sqrMagnitude)
						{
							sqrMagnitude = (item.position - drone.transform.position).sqrMagnitude;
							component = item.GetComponent<ColliderEventComponent>();
						}
					}
					if (m_closestObject == null)
					{
						m_closestObject = component.transform;
					}
					else if (m_closestObject == component.transform)
					{
						m_timer = 0f;
						return FlowStatus.Running;
					}
					module.ui.ClearMarkers();
					m_closestObject = component.transform;
					m_closestTrigger.mode = FNCollider.Trigger.Mode.Trigger;
					m_closestTrigger.target = component;
					m_closestTrigger.type = FNTriggerType.SwitchOn;
					m_timer = 0f;
				}
				return FlowStatus.Running;
			}
			case Mode.WaitDroneTarget:
				if ((bool)drone)
				{
					return WaitDroneTarget(list, list2, drone.transform);
				}
				break;
			case Mode.WatchDronePrecision:
				return OnWatchUpdate();
			}
			if (mode >= Mode.__Watch_)
			{
				return OnWatchUpdate();
			}
			if (mode >= Mode.__Camera_)
			{
				return OnCameraUpdate();
			}
			if (mode >= Mode.__Mission_)
			{
				return OnMissionUpdate();
			}
			if (mode >= Mode.__UI_)
			{
				return OnUIUpdate();
			}
			return result;
		}

		protected FlowStatus OnMissionUpdate()
		{
			FlowStatus result = FlowStatus.Complete;
			List<object> list = args;
			List<UnityEngine.Object> list2 = argsUnity;
			Drone drone = m_module.simulation.drones.Get(0);
			switch (mode)
			{
			case Mode.MissionComplete:
				Notify(0.2f, "fn.mission@complete");
				break;
			case Mode.MissionFail:
				flow.Stop();
				Notify(0.2f, "fn.mission@fail");
				break;
			case Mode.MissionStartTimer:
				module.StartTimer(0, "ELAPSED", "mission-time");
				break;
			case Mode.MissionStopTimer:
				module.StopTimer(0);
				break;
			case Mode.MissionClearTimers:
				module.ClearTimer(0, "mission-time");
				module.ClearTimer(1, "mission-time");
				module.ui.Hide(FlowModuleUI.ElementType.HeaderStep);
				module.ui.SetStepTimes(new Vector2(0f, 0f));
				break;
			case Mode.MissionRescueDrone:
			{
				bool flag = Reflection<object>.Get<bool>(list, 3);
				if (m_initialized)
				{
					if (flag && m_timer > 0f)
					{
						m_timer -= Time.deltaTime;
						return FlowStatus.Running;
					}
					m_initialized = false;
					return FlowStatus.Complete;
				}
				CameraFX component = module.simulation.cameras.Get(0).GetComponent<CameraFX>();
				int podium_id = Reflection<object>.Get<int>(list, 1);
				bool num = Reflection<object>.Get<bool>(list, 2);
				if (num)
				{
					component.radio = 0.3f;
					Tween.Add(component, "radio", 1f, 1f, Elastic.InSmall);
					Notify("fn.mission.drone.rescue");
				}
				if ((bool)drone && drone.fc.softLock)
				{
					drone.fc.softLock = false;
				}
				module.RescueDrone(0, podium_id, list, list2);
				m_initialized = true;
				if (!num)
				{
					Activity.RunOnce(delegate
					{
						module.droneStart.transform.position = module.simulation.podiums.Get(podium_id).spawn.transform.position;
						module.droneStart.transform.rotation = module.simulation.podiums.Get(podium_id).spawn.transform.rotation;
					}, 1.5f);
				}
				return FlowStatus.Running;
			}
			case Mode.MissionPlaceDrone:
				module.simulation.PlaceDrone(0, 0);
				break;
			case Mode.MissionMovePodium:
			{
				Transform transform = Reflection<object>.Get<Transform>(list2, 0, null);
				bool flag2 = false;
				if (!transform)
				{
					if (!drone)
					{
						break;
					}
					transform = drone.transform;
					flag2 = true;
				}
				int p_index = Reflection<object>.Get<int>(list, 0);
				DronePodium dronePodium = module.simulation.podiums.Get(p_index);
				if ((bool)dronePodium)
				{
					dronePodium.transform.position = transform.position;
					if (!flag2)
					{
						dronePodium.transform.rotation = transform.rotation;
					}
				}
				break;
			}
			case Mode.MissionSetStep:
			{
				int p_step = Reflection<object>.Get<int>(list, 0);
				int p_total = Reflection<object>.Get<int>(list, 1);
				module.ui.SetProgressBar(p_step, p_total);
				break;
			}
			case Mode.MissionTimerCondition:
				if (m_conditionTimerFinished)
				{
					module.RemoveTimer(0);
					return FlowStatus.Complete;
				}
				return FlowStatus.Running;
			}
			return result;
		}

		protected FlowStatus OnUIUpdate()
		{
			FlowStatus result = FlowStatus.Complete;
			List<object> list = args;
			_ = argsUnity;
			m_module.simulation.drones.Get(0);
			switch (mode)
			{
			case Mode.UIFadeIn:
				module.ui.FadeIn(0f);
				break;
			case Mode.UIFadeOut:
				module.ui.FadeOut(0f);
				break;
			case Mode.UINPCOverlayShow:
				module.ui.ShowNPCOverlay((NPCStateType)list[0], (string)list[1]);
				break;
			case Mode.UINPCOverlayHide:
				module.ui.HideNPCOverlay();
				break;
			case Mode.UIShowButtonNext:
				module.ui.ShowButtonNext();
				break;
			case Mode.UIHideButtonNext:
				module.ui.HideButtonNext();
				break;
			case Mode.UIHilightStepProgress:
				module.ui.HilightStepProgress();
				break;
			case Mode.UIWatchColliders:
			{
				int idx = flow.nodes.IndexOf(this);
				idx++;
				if (idx < 0 || idx >= flow.nodes.Count)
				{
					break;
				}
				FNCollider nc2 = flow.nodes[idx] as FNCollider;
				if (!nc2)
				{
					break;
				}
				bool disableOnHit = Reflection<object>.Get<bool>(list, 0);
				bool onlyNActive = Reflection<object>.Get(list, 4, p_default: false);
				int activeAmount = Reflection<object>.Get(list, 6, 1);
				bool playSoundOnEnd = Reflection<object>.Get(list, 5, p_default: true);
				bool set_steps = false;
				bool switch_on_landing = false;
				try
				{
					set_steps = Reflection<object>.Get<bool>(list, 1);
				}
				catch (Exception ex)
				{
					ex.GetHashCode();
				}
				switch_on_landing = Reflection<object>.Get<bool>(list, 3);
				int step_current = 0;
				int step_last = 0;
				int step_total = nc2.triggers.Count;
				if (set_steps && step_total >= 0)
				{
					module.ui.SetStep(step_current, step_total);
				}
				int markerTemplate = Reflection<object>.Get(list, 2, 0);
				m_runningActivity = Activity.Run((Func<bool>)delegate
				{
					if (m_runningActivity == null)
					{
						return false;
					}
					if (!nc2.flow.active)
					{
						return true;
					}
					if (nc2.status == FlowStatus.Idle)
					{
						return true;
					}
					if (nc2.status != FlowStatus.Running)
					{
						return false;
					}
					nc2.OnUpdate();
					step_current = 0;
					if (onlyNActive && nc2.criteria == BatchCriteria.Forward && idx == nc2.GetNextTrigger())
					{
						int i = idx;
						if (activeAmount > 1)
						{
							for (; i < idx + activeAmount && i < nc2.triggers.Count; i++)
							{
								if (!m_runningActivity.active)
								{
									break;
								}
								GameObject gameObject = nc2.triggers[i].target.gameObject;
								IActivable activable = gameObject.GetComponent<IActivable>();
								if (activable == null)
								{
									TrainingElementChild component = gameObject.GetComponent<TrainingElementChild>();
									if ((bool)component)
									{
										activable = component.TrainingElement;
									}
								}
								activable?.SetActive(active: true);
							}
						}
						else
						{
							GameObject gameObject2 = nc2.triggers[idx].target.gameObject;
							IActivable activable2 = gameObject2.GetComponent<IActivable>();
							if (activable2 == null)
							{
								TrainingElementChild component2 = gameObject2.GetComponent<TrainingElementChild>();
								if ((bool)component2)
								{
									activable2 = component2.TrainingElement;
								}
							}
							if (activable2 != null)
							{
								activable2.SetActive(active: true);
							}
							else
							{
								nc2.triggers[idx].target.gameObject.SetActive(value: true);
							}
						}
					}
					for (int j = 0; j < nc2.triggers.Count; j++)
					{
						FNCollider.Trigger trigger = nc2.triggers[j];
						if ((bool)trigger.target)
						{
							if (trigger.completed && switch_on_landing == m_updateUIMarker)
							{
								step_current++;
								if (set_steps && step_current >= step_total && playSoundOnEnd)
								{
									module.ui.PlayMissionCompleteAudio();
								}
								if (!table.ContainsKey(trigger) || !table[trigger])
								{
									table[trigger] = true;
									Notify("fn.mission.target@hit", trigger.target);
									if (disableOnHit)
									{
										IPoppable poppable = trigger.target.GetComponent<IPoppable>();
										if (poppable == null)
										{
											TrainingElementChild component3 = trigger.target.GetComponent<TrainingElementChild>();
											if ((bool)component3)
											{
												poppable = component3.TrainingElement as IPoppable;
											}
										}
										poppable?.Pop();
									}
									module.ui.UpdateMarker(trigger, markerTemplate);
									m_updateUIMarker = false;
								}
							}
							switch (nc2.criteria)
							{
							case BatchCriteria.All:
							case BatchCriteria.Any:
								if ((bool)module && (bool)module.ui && trigger.target.gameObject.activeInHierarchy)
								{
									module.ui.UpdateMarker(trigger, markerTemplate);
								}
								break;
							case BatchCriteria.Forward:
							case BatchCriteria.Backward:
								idx = nc2.GetNextTrigger();
								if (idx == j && (bool)module && (bool)module.ui && trigger.target.gameObject.activeInHierarchy)
								{
									module.ui.UpdateMarker(trigger, markerTemplate);
									int index = idx;
									GameObject gameObject3 = nc2.triggers[index].target.gameObject;
									TrainingElement trainingElement = gameObject3.GetComponent<TrainingElement>();
									if (trainingElement == null)
									{
										TrainingElementChild component4 = gameObject3.GetComponent<TrainingElementChild>();
										if ((bool)component4)
										{
											trainingElement = component4.TrainingElement;
										}
									}
									if (trainingElement != null && !trainingElement.IsRunning)
									{
										trainingElement.Run();
									}
								}
								break;
							}
						}
					}
					switch (nc2.criteria)
					{
					case BatchCriteria.All:
					case BatchCriteria.Any:
						if (set_steps && step_total > 0 && step_last < step_current)
						{
							module.ui.SetStep(step_current, step_total);
						}
						break;
					case BatchCriteria.Forward:
					case BatchCriteria.Backward:
						if (set_steps && step_total > 0 && step_last < step_current)
						{
							module.ui.SetStep(step_current, step_total);
						}
						break;
					}
					step_last = step_current;
					return true;
				}, 0f, false);
				break;
			}
			case Mode.UIWatchCollectables:
			{
				int num3 = flow.nodes.IndexOf(this);
				num3++;
				if (num3 < 0 || num3 >= flow.nodes.Count)
				{
					break;
				}
				FNCollector nc = flow.nodes[num3] as FNCollector;
				if (!nc)
				{
					break;
				}
				module.ui.SetCount(nc.Collected);
				Activity.Run((Func<bool>)delegate
				{
					if (!nc.flow.active)
					{
						return true;
					}
					if (nc.status == FlowStatus.Idle)
					{
						return true;
					}
					if (nc.status != FlowStatus.Running)
					{
						return false;
					}
					module.ui.SetCount(nc.Collected);
					return true;
				}, 0f, false);
				break;
			}
			case Mode.UIClearMarkers:
				module.ui.ClearMarkers();
				break;
			case Mode.UIIndicator:
			{
				int num = Reflection<object>.Get(list, 0, 0);
				Indicator indicator = Reflection<object>.Get(list, 1, Indicator.RollRight);
				float num2 = Reflection<object>.Get(list, 2, 0f);
				switch (num)
				{
				case 0:
					module.ui.ShowIndicator(indicator);
					if (num2 > 0f)
					{
						Activity.RunOnce(delegate
						{
							module.ui.HideIndicator(indicator);
						}, num2);
					}
					break;
				case 1:
					module.ui.HideIndicator(indicator);
					break;
				default:
					module.ui.ClearIndicators();
					break;
				}
				break;
			}
			case Mode.UINextObjective:
				module.ui.NextObjective();
				break;
			case Mode.UIStartCountdown:
				module.ui.FadeInCounter(0);
				Activity.RunOnce(delegate
				{
					module.ui.FadeInCounter(1);
				}, 1.4f);
				Activity.RunOnce(delegate
				{
					module.ui.FadeInCounter(2);
					m_startCountdownFinished = true;
				}, 2.4f);
				if (!m_startCountdownFinished)
				{
					return FlowStatus.Running;
				}
				return FlowStatus.Complete;
			}
			return result;
		}

		protected FlowStatus OnCameraUpdate()
		{
			FlowStatus result = FlowStatus.Complete;
			List<object> p_list = args;
			List<UnityEngine.Object> p_list2 = argsUnity;
			DroneSimulation simulation = m_module.simulation;
			Drone drone = simulation.drones.Get(0);
			DroneCamera camera = simulation.cameras.Get(0);
			if (!module)
			{
				return FlowStatus.Complete;
			}
			switch (mode)
			{
			case Mode.CameraMove:
			{
				if (!camera)
				{
					break;
				}
				Transform transform2 = Reflection<object>.Get<Transform>(p_list2, 0, null);
				float num4 = Reflection<object>.Get(p_list, 0, 0f);
				bool tweenDone = false;
				Tween tween2 = null;
				Vector3 vector2 = (transform2 ? transform2.position : camera.orbit.anchor);
				Quaternion quaternion = (transform2 ? transform2.rotation : camera.orbit.anchorRotation);
				camera.follow.target = null;
				camera.follow.offset = Vector3.zero;
				if ((bool)camera.wasd)
				{
					camera.wasd.usePhysics = false;
				}
				if (!m_startedCameraAnimation)
				{
					m_cameraAnimationDuration = num4;
					if (num4 <= 0f)
					{
						camera.orbit.anchorRotation = quaternion;
						camera.orbit.angle = Vector3.zero;
						camera.orbit.anchor = vector2;
					}
					else
					{
						tween2 = Tween.Add(camera.orbit, "anchorRotation", quaternion, num4, 0f, Cubic.InOut);
						Tween tween3 = tween2;
						tween3.onComplete = (Action<Tween>)Delegate.Combine(tween3.onComplete, (Action<Tween>)delegate
						{
							tweenDone = true;
						});
						m_CameraTweens.Add(tween2);
						m_CameraTweens.Add(Tween.Add(camera.orbit, "angle", Vector2.zero, num4, Cubic.InOut));
						m_CameraTweens.Add(Tween.Add(camera.orbit, "anchor", vector2, num4, 0f, Cubic.InOut));
					}
					m_startedCameraAnimation = true;
				}
				module.simulation.cameras.SetOther(0, 0);
				bool flag6 = !RCI.HasNavigationController && RCI.GetAxisTrigger(RawAxis.RightStickX, isPositiveSign: true);
				if ((RCI.GetAnyButtonUp() || flag6 || Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.Space)) && !m_fastForwardStarted)
				{
					module.ui.PlayFastForwardCamera();
					camera.orbit.SetTransitionMask(OrbitTransform.TransitionMask.Snap);
					camera.fx.ppb.profile.motionBlur.settings = m_fastCameraBlurSettings;
					foreach (Tween cameraTween in m_CameraTweens)
					{
						cameraTween.duration = m_fastForwardAnimationDuration;
					}
					m_cameraAnimationDuration = 0f;
					m_fastForwardStarted = true;
				}
				if (m_cameraAnimationDuration > 0f)
				{
					m_cameraAnimationDuration -= Time.deltaTime;
					return FlowStatus.Running;
				}
				m_runningActivity = Activity.RunOnce(delegate
				{
					camera.orbit.SetTransitionMask(OrbitTransform.TransitionMask.SmoothTPV);
					camera.fx.ppb.profile.motionBlur.Reset();
				}, m_fastForwardAnimationDuration);
				m_startedCameraAnimation = false;
				m_fastForwardStarted = false;
				if (!tweenDone)
				{
					return FlowStatus.Complete;
				}
				return FlowStatus.Running;
			}
			case Mode.CameraLOS:
				if ((bool)camera)
				{
					Transform transform = Reflection<object>.Get<Transform>(p_list2, 0);
					if (transform == null)
					{
						transform = camera.transform;
					}
					float p_cameraSpeed = Reflection<object>.Get(p_list, 0, 3f);
					module.simulation.cameras.SetLOS(0, 0, transform.position, p_cameraSpeed);
				}
				break;
			case Mode.CameraLine:
				if ((bool)camera)
				{
					LineTransform lineTransform = Reflection<object>.Get<LineTransform>(p_list2, 0, null);
					if ((bool)lineTransform)
					{
						float p_speed = Reflection<object>.Get(p_list, 0, 0f);
						bool betweenAnchors = Reflection<object>.Get(p_list, 1, p_default: false);
						module.simulation.cameras.SetLineCamera(0, 0, lineTransform, p_speed, betweenAnchors);
					}
				}
				break;
			case Mode.CameraFree:
				module.simulation.cameras.SetFree(0);
				break;
			case Mode.CameraFPV:
				if ((bool)camera)
				{
					module.simulation.cameras.SetFPV(0, 0);
				}
				break;
			case Mode.CameraTPV:
				if ((bool)camera)
				{
					module.simulation.cameras.SetTPV(0, 0);
				}
				break;
			case Mode.CameraTPVFree:
				if ((bool)camera)
				{
					module.simulation.cameras.SetTPV(0, 0, p_back: false);
				}
				break;
			case Mode.CameraTPVSideView:
				if ((bool)camera)
				{
					module.simulation.cameras.SetTPVSide(0, 0);
				}
				break;
			case Mode.CameraTPVCuav:
				if ((bool)camera)
				{
					module.simulation.cameras.SetTPVCUAV(0, 0);
				}
				break;
			case Mode.CameraNone:
				if (!camera)
				{
					Debug.LogWarning("FNSimulationModule> CameraNone - Camera not Found!!!");
					break;
				}
				camera.SetNone();
				if ((bool)camera.wasd)
				{
					camera.wasd.usePhysics = false;
				}
				module.simulation.cameras.SetOther(0, 0);
				break;
			case Mode.SetTransitionFlags:
				if ((bool)camera)
				{
					OrbitTransform.Transition mask = Reflection<object>.Get<OrbitTransform.Transition>(p_list, 0);
					module.simulation.cameras.SetTransitions(0, mask);
				}
				break;
			case Mode.CameraOrbit:
				if ((bool)camera)
				{
					bool flag4 = Reflection<object>.Get<bool>(p_list, 0);
					bool flag5 = Reflection<object>.Get<bool>(p_list, 1);
					float num3 = 0f;
					float p_duration = 0f;
					Vector2 vector = new Vector2(0f, 0f);
					if (flag4 || flag5)
					{
						p_duration = Reflection<object>.Get<float>(p_list, 4);
					}
					if (flag4)
					{
						num3 = Reflection<object>.Get<float>(p_list, 2);
						Tween.Add(camera.orbit, "distance", num3, p_duration, Cubic.InOut);
					}
					if (flag5)
					{
						vector = Reflection<object>.Get<Vector2>(p_list, 3);
						Tween.Add(camera.orbit, "angle", vector, p_duration, Cubic.InOut);
					}
					module.simulation.cameras.SetOther(0, 0);
				}
				break;
			case Mode.CameraTPVSmooth:
			{
				if (!camera || !module || !drone.ready)
				{
					return FlowStatus.Running;
				}
				Tween tween4 = null;
				Quaternion targetAnchorRotation = Quaternion.Euler(new Vector3(0f, drone.transform.rotation.eulerAngles.y, 0f));
				Vector3 p_to = new Vector3(drone.position.x, drone.position.y + 0.1f, drone.position.z);
				bool tweenDone2 = false;
				float num5 = Reflection<object>.Get(p_list, 0, 3f);
				bool has_fpv = false;
				if (!m_startedCameraAnimation)
				{
					camera.mode = DroneCameraModeType.TPVMissions;
					m_cameraAnimationDuration = num5;
					camera.follow.target = null;
					camera.orbit.constraint.distanceMin = float.MinValue;
					camera.orbit.constraint.distanceMax = float.MaxValue;
					camera.orbit.SetTransitionSpeed(3f, 3f, 3f, 3f);
					if (num5 <= 0f)
					{
						camera.orbit.anchorRotation = targetAnchorRotation;
						camera.orbit.angle = new Vector2(0f, 12f);
						camera.follow.offset = new Vector3(0f, 0.1f, 0f);
						camera.orbit.anchor = drone.position;
					}
					else
					{
						tween4 = Tween.Add(camera.orbit, "anchorRotation", targetAnchorRotation, num5, 0f, Cubic.InOut);
						Tween tween5 = tween4;
						tween5.onComplete = (Action<Tween>)Delegate.Combine(tween5.onComplete, (Action<Tween>)delegate
						{
							tweenDone2 = true;
							camera.follow.target = drone.transform;
							camera.orbit.distance = 0.4f;
							camera.orbit.angle = new Vector2(0f, 12f);
							camera.orbit.anchorRotation = targetAnchorRotation;
							camera.follow.offset = new Vector3(0f, 0.1f, 0f);
							camera.orbit.SetTransitionSpeed(m_defaultTransitionSpeed);
							foreach (FlowNode node in flow.nodes)
							{
								FNSimulationModule fNSimulationModule = node as FNSimulationModule;
								if ((bool)fNSimulationModule && (fNSimulationModule.mode == Mode.CameraFPVSmooth || fNSimulationModule.mode == Mode.CameraFPV))
								{
									has_fpv = true;
									break;
								}
							}
							camera.orbit.SetTransitionMask((has_fpv || m_tpvSkippingToFPV) ? OrbitTransform.TransitionMask.Snap : OrbitTransform.TransitionMask.SmoothTPV);
							camera.follow.flags = (OrbitFollowInput.Flag)23;
							drone.renderer.shadowsOnly = false;
						});
						m_CameraTweens.Add(tween4);
						m_CameraTweens.Add(Tween.Add(camera.orbit, "angle", new Vector2(0f, 12f), num5, Cubic.InOut));
						m_CameraTweens.Add(Tween.Add(camera.orbit, "anchor", p_to, num5, 0f, Cubic.InOut));
						m_CameraTweens.Add(Tween.Add(camera.orbit, "distance", 0.4f, num5, 0f, Cubic.InOut));
					}
					m_startedCameraAnimation = true;
				}
				module.simulation.cameras.SetOther(0, 0);
				bool flag7 = !RCI.HasNavigationController && RCI.GetAxisTrigger(RawAxis.RightStickX, isPositiveSign: true);
				if ((RCI.GetAnyButtonUp() || flag7 || Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.Space)) && !m_fastForwardStarted)
				{
					module.ui.PlayFastForwardCamera();
					camera.orbit.SetTransitionMask(OrbitTransform.TransitionMask.Snap);
					camera.fx.ppb.profile.motionBlur.settings = m_fastCameraBlurSettings;
					foreach (Tween cameraTween2 in m_CameraTweens)
					{
						cameraTween2.duration = m_fastForwardAnimationDuration;
					}
					m_cameraAnimationDuration = 0f;
					m_fastForwardStarted = true;
				}
				if (m_cameraAnimationDuration > 0f)
				{
					m_cameraAnimationDuration -= Time.deltaTime;
					return FlowStatus.Running;
				}
				this.ActivityRunOnce(delegate
				{
					foreach (FlowNode node2 in flow.nodes)
					{
						FNSimulationModule fNSimulationModule = node2 as FNSimulationModule;
						if ((bool)fNSimulationModule && (fNSimulationModule.mode == Mode.CameraFPVSmooth || fNSimulationModule.mode == Mode.CameraFPV))
						{
							has_fpv = true;
							break;
						}
					}
					camera.orbit.SetTransitionMask((has_fpv || m_tpvSkippingToFPV) ? OrbitTransform.TransitionMask.Snap : OrbitTransform.TransitionMask.SmoothTPV);
					camera.fx.ppb.profile.motionBlur.Reset();
				}, m_fastForwardAnimationDuration);
				m_startedCameraAnimation = false;
				m_fastForwardStarted = false;
				if (!tweenDone2)
				{
					return FlowStatus.Complete;
				}
				return FlowStatus.Running;
			}
			case Mode.CameraFPVSmooth:
				if (!camera)
				{
					break;
				}
				if (!m_startedCameraAnimation)
				{
					float num2 = Reflection<object>.Get(p_list, 0, 1.5f);
					Tween tween = Tween.Add(camera.orbit, "distance", 0.1f, num2, 0f, Cubic.Out);
					tween.onComplete = (Action<Tween>)Delegate.Combine(tween.onComplete, (Action<Tween>)delegate
					{
						module.simulation.cameras.SetFPV(0, 0);
						drone.renderer.shadowsOnly = true;
						camera.follow.flags = OrbitFollowInput.Flag.All;
					});
					m_startedCameraAnimation = true;
					m_cameraAnimationDuration = num2 + 0.2f;
				}
				if (m_cameraAnimationDuration > 0f)
				{
					m_cameraAnimationDuration -= Time.deltaTime;
					return FlowStatus.Running;
				}
				camera.follow.target = drone.body.frame.camera.pivot;
				camera.orbit.distance = -0.02f;
				camera.orbit.angle = Vector2.zero;
				return FlowStatus.Complete;
			case Mode.CameraOrbitMove:
			{
				if (!camera)
				{
					break;
				}
				Transform transform3 = Reflection<object>.Get<Transform>(p_list2, 0);
				if (transform3 == null)
				{
					if (!drone)
					{
						break;
					}
					transform3 = drone.transform;
				}
				Vector2 p_to2 = Reflection<object>.Get<Vector2>(p_list, 0);
				float num6 = Reflection<object>.Get(p_list, 1, 1.5f);
				float num7 = Reflection<object>.Get<float>(p_list, 2);
				if (num6 == 0f)
				{
					num6 = 1.5f;
				}
				camera.SetNone();
				Tween.Add(camera.orbit, "distance", num6, num7, 0f, Cubic.InOut);
				Tween.Add(camera.orbit, "angle", p_to2, num7, 0f, Cubic.InOut);
				Tween.Add(camera.orbit, "anchor", transform3.position, num7, 0f, Cubic.InOut);
				Tween.Add(camera.orbit, "anchorRotation", transform3.rotation, num7, num7, Cubic.InOut);
				module.simulation.cameras.SetOther(0, 0);
				break;
			}
			case Mode.CameraSplineAnimation:
			{
				SplineActor splineActor = Reflection<object>.Get<SplineActor>(p_list2, 0, null);
				Transform lookAtObject = Reflection<object>.Get<Transform>(p_list2, 1, null);
				float num = Reflection<object>.Get(p_list, 0, 0f);
				Reflection<object>.Get(p_list, 1, p_default: true);
				bool flag = Reflection<object>.Get(p_list, 2, p_default: false);
				bool flag2 = Reflection<object>.Get(p_list, 3, p_default: false);
				if (flag)
				{
					splineActor.lookAtObject = drone.transform;
				}
				else if (flag2)
				{
					splineActor.lookAtObject = lookAtObject;
				}
				bool flag3 = RCI.HasNavigationController && RCI.GetAxisTrigger(RawAxis.RightStickX, isPositiveSign: true);
				if (RCI.GetAnyButtonUp() || flag3 || Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.Space))
				{
					module.ui.HideSkip(dmv: true);
					splineActor.auto = false;
					return FlowStatus.Complete;
				}
				if (num > 0f)
				{
					if (m_splineAnimationTimer < num)
					{
						m_splineAnimationTimer += Time.deltaTime;
						return FlowStatus.Running;
					}
					module.ui.HideSkip(dmv: true);
					splineActor.auto = false;
					return FlowStatus.Complete;
				}
				if (splineActor.wrap == WrapMode.Once)
				{
					if (splineActor.auto)
					{
						return FlowStatus.Running;
					}
					module.ui.HideSkip(dmv: true);
					return FlowStatus.Complete;
				}
				if (splineActor.wrap != WrapMode.Once)
				{
					if (!splineActor.reverse && splineActor.progress < 0.99f)
					{
						return FlowStatus.Running;
					}
					if (splineActor.reverse && splineActor.progress > 0.01f)
					{
						return FlowStatus.Running;
					}
					module.ui.HideSkip(dmv: true);
					splineActor.auto = false;
					return FlowStatus.Complete;
				}
				break;
			}
			}
			return result;
		}

		protected FlowStatus OnWatchUpdate()
		{
			FlowStatus result = FlowStatus.Complete;
			List<object> p_list = args;
			List<UnityEngine.Object> p_list2 = argsUnity;
			Drone drone = m_module.simulation.drones.Get(0);
			switch (mode)
			{
			case Mode.WatchDroneFlip:
				if (drone.fc.sensor.stuck)
				{
					m_timer += Time.deltaTime;
				}
				else
				{
					m_timer = 0f;
				}
				if (m_timer >= 2f)
				{
					m_timer = 0f;
					break;
				}
				return FlowStatus.Running;
			case Mode.WatchDronePrecision:
			{
				SphereCollider sphereCollider = Reflection<object>.Get<SphereCollider>(p_list2, 0, null);
				SphereCollider sphereCollider2 = Reflection<object>.Get<SphereCollider>(p_list2, 1, null);
				bool flag5 = Reflection<object>.Get<bool>(p_list, 2);
				Gauge gauge2 = Reflection<object>.Get<Gauge>(p_list, 3);
				int p_position2 = ((Gauge.LeftGauge != gauge2) ? 1 : 0);
				if (gauge2 == Gauge.None)
				{
					p_position2 = -1;
				}
				if (module == null || module.ui == null)
				{
					break;
				}
				if (!sphereCollider)
				{
					module.ui.SetGauge(0, p_flag: false);
					module.ui.SetTimer(1, "HOLD", -1f);
					break;
				}
				AnimationCurve animationCurve = Reflection<object>.Get(p_list, 0, AnimationCurve.Linear(0f, 1f, 1f, 0f));
				float num4 = Reflection<object>.Get(p_list, 1, 3f);
				Vector3 position = sphereCollider.transform.position;
				Vector3 position2 = drone.position;
				float num5 = Mathf.Abs(position.y - position2.y);
				if (flag5)
				{
					position.y = 0f;
					position2.y = 0f;
				}
				float num6 = Vector3.Distance(position2, position);
				float radius = sphereCollider.radius;
				bool flag6 = num6 <= radius * 2f;
				module.ui.SetGauge(p_position2, flag6);
				if (flag6 && !flag5)
				{
					module.ui.SetTimer(1, "HOLD", Mathf.Max(num4 - m_timer, 0f));
				}
				float time = Mathf.Clamp01((radius <= 0f) ? num6 : (num6 / radius));
				time = Mathf.Clamp01(animationCurve.Evaluate(time));
				float num7 = time;
				if ((bool)sphereCollider2)
				{
					Vector3 position3 = drone.position;
					Vector3 position4 = sphereCollider2.transform.position;
					if (flag5)
					{
						position3.y = 0f;
						position4.y = 0f;
					}
					float num8 = Vector3.Distance(position3, position4);
					num7 = Mathf.Clamp01(1f - Mathf.Pow(num8 / sphereCollider2.radius, 2f));
				}
				Notify("fn.mission.precision@update", sphereCollider, num7);
				module.ui.SetGauge(p_position2, time);
				if (flag5)
				{
					if (time >= 1f && num5 <= 0.05f)
					{
						module.ui.HighlightGauge(p_position2);
						ColliderEventComponent component4 = sphereCollider.GetComponent<ColliderEventComponent>();
						if ((bool)component4)
						{
							component4.enabled = true;
						}
						UpdateWatchColliderMarkers();
						sphereCollider.enabled = true;
						Notify("fn.mission.precision@stop");
						module.ui.SetGauge(p_position2, p_flag: false);
						return FlowStatus.Complete;
					}
					return FlowStatus.Running;
				}
				if (time >= 1f)
				{
					m_timer += Time.deltaTime;
				}
				else
				{
					m_timer = 0f;
				}
				if (m_timer >= num4)
				{
					m_timer = 0f;
					module.ui.SetTimer(1, "HOLD", 0f);
					module.ui.HilightTimer(1);
					module.ui.HighlightGauge(p_position2);
					UpdateWatchColliderMarkers();
					sphereCollider.enabled = true;
					Notify("fn.mission.precision@stop");
					module.ui.SetGauge(p_position2, p_flag: false);
					return FlowStatus.Complete;
				}
				return FlowStatus.Running;
			}
			case Mode.PoleWatch:
			{
				if (!drone)
				{
					if (!m_initialized)
					{
						return FlowStatus.Idle;
					}
					return FlowStatus.Complete;
				}
				PoleWatch component = GetComponent<PoleWatch>();
				if (!m_currentPole && component.poleTag == PoleWatch.poleTags.FirstPole)
				{
					m_currentPole = base.gameObject;
				}
				if (m_currentPole != base.gameObject)
				{
					return FlowStatus.Idle;
				}
				Gauge gauge = component.gauge;
				bool showGauge = component.showGauge;
				float distToEngage = component.distToEngage;
				if (Vector3.Distance(drone.transform.position, base.transform.position) <= distToEngage)
				{
					Transform target = component.target;
					float goalAngle = component.goalAngle;
					if (!m_initialized)
					{
						Debug.Log("Enaged to pole: " + base.gameObject.name + " :: Initialize");
						m_poleInitialForward = target.forward;
						m_poleStartDir = Quaternion.AngleAxis(90f * Mathf.Sign(goalAngle), target.up) * target.forward;
						Vector3 vector = Quaternion.AngleAxis(goalAngle, target.up) * m_poleStartDir;
						Debug.DrawRay(target.transform.position, m_poleStartDir * 2f, Color.yellow, 120f);
						Debug.DrawRay(target.transform.position, vector * 2f, Color.black, 120f);
						if (showGauge)
						{
							int p_position = ((gauge != Gauge.LeftGauge) ? 1 : 0);
							module.ui.SetGauge(p_position, goalAngle + "°", Mathf.FloorToInt(goalAngle * 0.5f) + "°", "0°");
							module.ui.SetGauge(p_position, 0f);
							module.ui.SetGauge(p_position, p_flag: true);
						}
						module.ui.ShowIndicator((Mathf.Sign(goalAngle) > 0f) ? Indicator.PoleRight : Indicator.PoleLeft);
						m_initialized = true;
					}
					Vector3 vector2 = new Vector3(drone.transform.position.x, target.position.y, drone.transform.position.z);
					Vector3 rhs = vector2 - target.transform.position;
					if (!m_trackPoleAngle)
					{
						float num = Mathf.Sign(Vector3.Cross(m_poleInitialForward, rhs).normalized.y);
						bool flag = Vector3.Dot(m_poleInitialForward.normalized * -1f, lastDirToDrone.normalized) <= 0f;
						bool flag2 = num == (float)Math.Sign(goalAngle);
						bool flag3 = Vector3.Dot(m_poleInitialForward.normalized, rhs.normalized) <= 0f;
						m_trackPoleAngle = flag && flag2 && flag3;
						Debug.DrawRay(target.transform.position, lastDirToDrone, Color.red);
					}
					float num2 = 0f;
					bool flag4 = false;
					float num3 = 0f;
					if (m_trackPoleAngle)
					{
						target.LookAt(vector2);
						num3 = SignedAngleBetween(m_poleStartDir, target.forward, target.up);
						if (goalAngle > 0f)
						{
							if (num3 < 0f)
							{
								num3 += 360f;
							}
						}
						else if (num3 > 0f)
						{
							num3 -= 360f;
						}
						num2 = num3 / goalAngle;
						if (num2 < 0f || num2 > 1.1f)
						{
							m_trackPoleAngle = false;
						}
						num2 = Mathf.Max(0f, num2);
						if (num2 >= 1f && num2 <= 1.1f)
						{
							flag4 = true;
						}
					}
					lastDirToDrone = rhs;
					if (showGauge)
					{
						switch (gauge)
						{
						case Gauge.LeftGauge:
							if (num2 > 0.97f)
							{
								module.ui.SetGauge(0, p_flag: false);
								break;
							}
							module.ui.SetGauge(0, p_flag: true);
							module.ui.SetGauge(0, num2);
							break;
						case Gauge.RightGauge:
							if (num2 > 0.97f)
							{
								module.ui.SetGauge(1, p_flag: false);
								break;
							}
							module.ui.SetGauge(1, p_flag: true);
							module.ui.SetGauge(1, num2);
							break;
						}
					}
					if (flag4)
					{
						module.ui.HideIndicator((Mathf.Sign(goalAngle) > 0f) ? Indicator.PoleRight : Indicator.PoleLeft);
						ColliderEventComponent componentInChildren = target.GetComponentInChildren<ColliderEventComponent>();
						if ((bool)componentInChildren)
						{
							Collider component2 = componentInChildren.GetComponent<Collider>();
							if ((bool)component2)
							{
								componentInChildren.Trigger(component2);
							}
						}
						if (component.poleTag != PoleWatch.poleTags.LastPole)
						{
							for (int i = base.transform.GetSiblingIndex() + 1; i < base.transform.parent.childCount; i++)
							{
								Transform child = base.transform.parent.GetChild(i);
								PoleWatch component3 = child.gameObject.GetComponent<PoleWatch>();
								if ((bool)component3 && (component3.poleTag == PoleWatch.poleTags.Pole || component3.poleTag == PoleWatch.poleTags.LastPole))
								{
									m_currentPole = child.gameObject;
									break;
								}
							}
						}
						else
						{
							m_currentPole = null;
						}
						target.gameObject.SetActive(value: false);
					}
					if (flag4)
					{
						PodiumMoveTrigger podiumMoveTrigger = target.gameObject.GetComponent<PodiumMoveTrigger>();
						if (!podiumMoveTrigger)
						{
							podiumMoveTrigger = target.gameObject.AddComponent<PodiumMoveTrigger>();
						}
						podiumMoveTrigger.TriggerPodiumMove(drone.transform);
					}
					if (!flag4)
					{
						return FlowStatus.Running;
					}
					return FlowStatus.Complete;
				}
				if (showGauge)
				{
					switch (gauge)
					{
					case Gauge.LeftGauge:
						module.ui.SetGauge(0, p_flag: false);
						break;
					case Gauge.RightGauge:
						module.ui.SetGauge(1, p_flag: false);
						break;
					}
				}
				lastDronePos = drone.transform.position;
				return FlowStatus.Idle;
			}
			}
			return result;
		}

		public void SetMode(Mode p_mode)
		{
			args = null;
			if (argsUnity != null)
			{
				argsUnity.Clear();
			}
			List<object> list = args;
			List<UnityEngine.Object> list2 = argsUnity;
			switch (p_mode)
			{
			case Mode.AnimateDrone:
				list2.Add(null);
				list.Add(Vector3.zero);
				list.Add(0f);
				list.Add(true);
				list.Add(false);
				break;
			case Mode.CreateDrone:
				list2.Add(null);
				break;
			case Mode.LockDrone:
				list.Add(126);
				break;
			case Mode.SoftLockDrone:
				list.Add(false);
				list.Add(false);
				list.Add(false);
				list.Add(false);
				list.Add(false);
				list.Add(false);
				list.Add(0.05f);
				list.Add(0f);
				list2.Add(null);
				break;
			case Mode.SimulationPause:
				list.Add(DroneSimulationPauseMode.Pause);
				break;
			case Mode.WaitDroneEuler:
				list.Add(false);
				list.Add(0f);
				list.Add(0f);
				list.Add(false);
				list.Add(false);
				list.Add(0f);
				list.Add(0f);
				list.Add(false);
				list.Add(false);
				list.Add(0f);
				list.Add(0f);
				list.Add(false);
				list.Add(false);
				list.Add(0f);
				list.Add(0f);
				list.Add(false);
				list.Add(Gauge.LeftGauge);
				list.Add(0f);
				break;
			case Mode.WatchDroneEuler:
				list.Add(DroneAxis.Yaw);
				break;
			case Mode.WaitDroneDistance:
				list2.Add(null);
				list.Add(false);
				list.Add(0f);
				list.Add(0f);
				list.Add(false);
				list.Add(0f);
				list.Add(0f);
				list.Add(false);
				list.Add(0f);
				list.Add(0f);
				list.Add(0f);
				list.Add(false);
				list.Add(0f);
				list.Add(true);
				list.Add(false);
				list.Add(Gauge.LeftGauge);
				list.Add(false);
				break;
			case Mode.PlaySound:
			{
				AudioClip audioClip = null;
				audioClip = null;
				list2.Add(audioClip);
				list.Add(false);
				break;
			}
			case Mode.PlayParticles:
				list2.Add(null);
				list.Add("");
				list.Add(false);
				break;
			case Mode.WatchDroneDistance:
				list2.Add(null);
				list.Add(0);
				break;
			case Mode.WatchDroneHeight:
				list2.Add(null);
				list2.Add(null);
				break;
			case Mode.DronePower:
				list.Add(true);
				break;
			case Mode.DroneControl:
				list.Add(true);
				list.Add(true);
				list.Add(true);
				list.Add(true);
				list.Add(0f);
				list.Add(0f);
				list.Add(0f);
				list.Add(0f);
				break;
			case Mode.FlightControllerMode:
				list.Add(FlightControllerMode.Acro);
				list.Add(0f);
				list.Add(Vector3.zero);
				list.Add(0.2f);
				list.Add(22f);
				list.Add(0f);
				list.Add(0.05f);
				list.Add(1f);
				list.Add(0f);
				list2.Add(null);
				break;
			case Mode.FlightControllerParameters:
				list.Add(FlightControllerMode.Bypass);
				list.Add(true);
				list.Add(false);
				list.Add(1f);
				list.Add(30f);
				list.Add(9f);
				list.Add(0.05f);
				list.Add(false);
				list.Add(false);
				list.Add(1.6f);
				break;
			case Mode.FlightControllerProcess:
				list.Add(FlightControllerProcess.Altitude);
				list.Add(false);
				list.Add(0f);
				list.Add(45f);
				list.Add(10f);
				list.Add(0f);
				list2.Add(null);
				break;
			case Mode.ControllerTuningPreset:
				list.Add(FCProfileData.Betaflight.PresetType.Training);
				break;
			case Mode.WaitDroneTarget:
				list2.Add(null);
				list.Add(false);
				list.Add(false);
				list.Add(0f);
				break;
			case Mode.StoreCount:
				list.Add(FNMissionScoreType.Count0);
				break;
			case Mode.StoreTime:
				list.Add(FNMissionScoreType.TimeMin0);
				break;
			case Mode.SplineCondition:
				list2.Add(null);
				list.Add(0f);
				break;
			case Mode.SplineDroneReset:
				list2.Add(null);
				break;
			case Mode.MarkClosestObject:
				list2.Add(null);
				list2.Add(null);
				list.Add(1f);
				break;
			case Mode.ResetAvailable:
				list.Add(true);
				break;
			case Mode.SetActivePodium:
				list.Add(0);
				break;
			case Mode.SetData:
				list.Add("");
				list.Add(0);
				break;
			case Mode.UINPCOverlayShow:
				list.Add(NPCStateType.Drone0);
				list.Add("");
				break;
			case Mode.UIWatchColliders:
				list.Add(false);
				list.Add(true);
				list.Add(0);
				list.Add(false);
				list.Add(false);
				list.Add(true);
				list.Add(1);
				break;
			case Mode.UISetTimer:
				list.Add(0);
				list.Add(0);
				list.Add("");
				break;
			case Mode.UIIndicator:
				list.Add(0);
				list.Add(Indicator.RollRight);
				list.Add(0f);
				break;
			case Mode.CameraMove:
				list2.Add(null);
				list.Add(0f);
				break;
			case Mode.CameraLOS:
				list2.Add(null);
				list.Add(3f);
				break;
			case Mode.CameraLine:
				list2.Add(null);
				list.Add(0f);
				list.Add(false);
				list.Add(AnimationCurve.Linear(0f, 60f, 1f, 60f));
				break;
			case Mode.SetTransitionFlags:
				list.Add(OrbitTransform.Transition.DistanceLerp);
				break;
			case Mode.CameraOrbit:
				list.Add(false);
				list.Add(false);
				list.Add(0f);
				list.Add(new Vector2(0f, 0f));
				list.Add(0f);
				break;
			case Mode.CameraOrbitMove:
				list2.Add(null);
				list.Add(new Vector2(0f, 0f));
				list.Add(0f);
				list.Add(0f);
				break;
			case Mode.CameraTPVSmooth:
				list.Add(3f);
				break;
			case Mode.CameraFPVSmooth:
				list.Add(0f);
				break;
			case Mode.CameraSplineAnimation:
				list2.Add(null);
				list2.Add(null);
				list.Add(0f);
				list.Add(true);
				list.Add(false);
				list.Add(false);
				break;
			case Mode.WatchDronePrecision:
				list2.Add(null);
				list2.Add(null);
				list.Add(AnimationCurve.Linear(0f, 1f, 1f, 0f));
				list.Add(3f);
				list.Add(false);
				list.Add(Gauge.LeftGauge);
				break;
			case Mode.PoleWatch:
				list2.Add(null);
				list.Add(Gauge.RightGauge);
				list.Add(0f);
				list.Add(true);
				list.Add(10f);
				break;
			case Mode.MissionRescueDrone:
				list.Add(true);
				list.Add(0);
				list.Add(true);
				list2.Add(null);
				list.Add(false);
				break;
			case Mode.MissionMovePodium:
				list2.Add(null);
				list.Add(0);
				break;
			case Mode.MissionSetStep:
				list.Add(1);
				list.Add(1);
				break;
			case Mode.MissionTimerCondition:
				list.Add(0f);
				list.Add(false);
				break;
			case Mode.MissionResetBalloons:
				list2.Add(null);
				list.Add(false);
				break;
			}
			args = list;
		}

		protected FlowStatus WaitDroneEuler(List<object> al, Drone drone)
		{
			bool[] array = new bool[3];
			float[] array2 = new float[2];
			float[] array3 = new float[2];
			float[] array4 = new float[2];
			bool[] array5 = new bool[3];
			array[0] = Reflection<object>.Get<bool>(al, 0);
			array2[0] = Reflection<object>.Get<float>(al, 1);
			array2[1] = Reflection<object>.Get<float>(al, 2);
			array5[0] = Reflection<object>.Get<bool>(al, 3);
			array[1] = Reflection<object>.Get<bool>(al, 4);
			array3[0] = Reflection<object>.Get<float>(al, 5);
			array3[1] = Reflection<object>.Get<float>(al, 6);
			array5[1] = Reflection<object>.Get<bool>(al, 7);
			array[2] = Reflection<object>.Get<bool>(al, 8);
			array4[0] = Reflection<object>.Get<float>(al, 9);
			array4[1] = Reflection<object>.Get<float>(al, 10);
			array5[2] = Reflection<object>.Get<bool>(al, 11);
			bool flag = Reflection<object>.Get<bool>(al, 12);
			float num = Reflection<object>.Get<float>(al, 13);
			float num2 = Reflection<object>.Get<float>(al, 14);
			bool flag2 = Reflection<object>.Get<bool>(al, 15);
			Gauge gauge = Reflection<object>.Get<Gauge>(al, 16);
			float num3 = GetGoalAngle(al);
			if (gauge == Gauge.LeftPrecisionGauge || gauge == Gauge.RightPrecisionGauge)
			{
				num3 *= 2f;
			}
			bool[] array6 = new bool[3];
			bool flag3 = true;
			if (num2 > 0f)
			{
				if (m_timer > num2)
				{
					m_timer = 0f;
					return FlowStatus.Complete;
				}
				m_timer += Time.deltaTime;
			}
			UpdateAngleDifferential(drone);
			if (array[0])
			{
				if (!array5[0])
				{
					if (array2[0] > m_cumulativeDroneRotation[0] && m_cumulativeDroneRotation[0] > array2[1])
					{
						array6[0] = true;
					}
				}
				else if (m_cumulativeDroneRotation[0] > 180f || m_cumulativeDroneRotation[0] < -180f)
				{
					array6[0] = true;
				}
				float p_ratio = m_cumulativeDroneRotation[0] / num3;
				if ((m_cumulativeDroneRotation[0] < 0f && array2[0] > 0f) || (m_cumulativeDroneRotation[0] > 0f && array2[0] < 0f))
				{
					p_ratio = 0f;
				}
				if (flag2)
				{
					switch (gauge)
					{
					case Gauge.LeftGauge:
						module.ui.SetGauge(0, p_ratio);
						break;
					case Gauge.RightGauge:
						module.ui.SetGauge(1, p_ratio);
						break;
					case Gauge.LeftPrecisionGauge:
						module.ui.SetPrecisionGauge(0, p_ratio);
						break;
					case Gauge.RightPrecisionGauge:
						module.ui.SetPrecisionGauge(1, p_ratio);
						break;
					}
				}
				flag3 = array6[0];
			}
			if (array[1])
			{
				if (!array5[1])
				{
					if (array3[0] > m_cumulativeDroneRotation[1] && m_cumulativeDroneRotation[1] > array3[1])
					{
						array6[1] = true;
					}
				}
				else if (m_cumulativeDroneRotation[1] > 180f || m_cumulativeDroneRotation[1] < -180f)
				{
					array6[1] = true;
				}
				float p_ratio2 = Mathf.Abs(m_cumulativeDroneRotation[1] / num3);
				if ((m_cumulativeDroneRotation[1] < 0f && array3[0] > 0f) || (m_cumulativeDroneRotation[1] > 0f && array3[0] < 0f))
				{
					p_ratio2 = 0f;
				}
				if (flag2)
				{
					switch (gauge)
					{
					case Gauge.LeftGauge:
						module.ui.SetGauge(0, p_ratio2);
						break;
					case Gauge.RightGauge:
						module.ui.SetGauge(1, p_ratio2);
						break;
					case Gauge.LeftPrecisionGauge:
						module.ui.SetPrecisionGauge(0, p_ratio2);
						break;
					case Gauge.RightPrecisionGauge:
						module.ui.SetPrecisionGauge(1, p_ratio2);
						break;
					}
				}
				flag3 = array6[1];
			}
			if (array[2])
			{
				if (!array5[2])
				{
					if (array4[0] > m_cumulativeDroneRotation[2] && m_cumulativeDroneRotation[2] > array4[1])
					{
						array6[2] = true;
					}
				}
				else if (m_cumulativeDroneRotation[2] > 180f || m_cumulativeDroneRotation[2] < -180f)
				{
					array6[2] = true;
				}
				float p_ratio3 = Mathf.Abs(m_cumulativeDroneRotation[2] / num3);
				if ((m_cumulativeDroneRotation[2] < 0f && array4[0] > 0f) || (m_cumulativeDroneRotation[2] > 0f && array4[0] < 0f))
				{
					p_ratio3 = 0f;
				}
				if (flag2)
				{
					switch (gauge)
					{
					case Gauge.LeftGauge:
						module.ui.SetGauge(0, p_ratio3);
						break;
					case Gauge.RightGauge:
						module.ui.SetGauge(1, p_ratio3);
						break;
					case Gauge.LeftPrecisionGauge:
						module.ui.SetPrecisionGauge(0, p_ratio3);
						break;
					case Gauge.RightPrecisionGauge:
						module.ui.SetPrecisionGauge(1, p_ratio3);
						break;
					}
				}
				flag3 = array6[2];
			}
			if (flag && num > 0f)
			{
				if (flag2)
				{
					module.ui.SetTimer(1, "HOLD", num - m_rangeTimer);
				}
				if (flag3)
				{
					m_rangeTimer += Time.deltaTime;
					if (m_rangeTimer > num)
					{
						module.ui.HilightTimer(1);
						if (flag2)
						{
							module.ui.SetTimer(1, "HOLD", 0f);
						}
						return FlowStatus.Complete;
					}
					return FlowStatus.Running;
				}
				m_rangeTimer = 0f;
				return FlowStatus.Running;
			}
			if (!flag3)
			{
				return FlowStatus.Running;
			}
			return FlowStatus.Complete;
		}

		protected FlowStatus WaitDroneDistance(List<UnityEngine.Object> p_ual, List<object> p_al, Drone p_drone)
		{
			Vector3 vector = m_droneStartingPosition;
			bool[] array = new bool[3];
			float[] array2 = new float[2];
			float[] array3 = new float[2];
			float[] array4 = new float[2];
			Transform transform = Reflection<object>.Get<Transform>(p_ual, 0);
			array[0] = Reflection<object>.Get<bool>(p_al, 0);
			array2[0] = Reflection<object>.Get<float>(p_al, 1);
			array2[1] = Reflection<object>.Get<float>(p_al, 2);
			array[1] = Reflection<object>.Get<bool>(p_al, 3);
			array3[0] = Reflection<object>.Get<float>(p_al, 4);
			array3[1] = Reflection<object>.Get<float>(p_al, 5);
			array[2] = Reflection<object>.Get<bool>(p_al, 6);
			array4[0] = Reflection<object>.Get<float>(p_al, 7);
			array4[1] = Reflection<object>.Get<float>(p_al, 8);
			float num = Reflection<object>.Get<float>(p_al, 9);
			bool flag = Reflection<object>.Get<bool>(p_al, 10);
			float num2 = Reflection<object>.Get<float>(p_al, 11);
			bool flag2 = Reflection<object>.Get<bool>(p_al, 13);
			Gauge gauge = Reflection<object>.Get<Gauge>(p_al, 14);
			if (Reflection<object>.Get<bool>(p_al, 15))
			{
				vector = module.simulation.podiums.list[0].transform.position;
			}
			bool[] array5 = new bool[3];
			bool flag3 = true;
			Vector3 vector2 = transform.position - p_drone.transform.position;
			if (num > 0f)
			{
				if (m_timer > num)
				{
					m_timer = 0f;
					return FlowStatus.Complete;
				}
				m_timer += Time.deltaTime;
			}
			float num3 = 0f;
			if (array[0])
			{
				if (Mathf.Abs(vector2.x) >= array2[0] && vector2.x <= array2[1])
				{
					array5[0] = true;
				}
				flag3 = array5[0];
				float num4 = transform.position.x - vector.x;
				float num5 = (transform.position.x - p_drone.transform.position.x) / num4;
				num3 = ((!(num5 < 0f)) ? (1f - num5) : (Mathf.Abs(num5) + 1f));
			}
			if (array[1])
			{
				float num6 = Mathf.Abs(vector2.y);
				if (num6 >= array3[0] && num6 <= array3[1])
				{
					array5[1] = true;
				}
				flag3 = array5[1];
				float num7 = Mathf.Abs(transform.position.y - vector.y);
				float num8 = transform.position.y - p_drone.transform.position.y;
				float num9 = num8 / num7;
				num3 = ((!(num9 < 0f)) ? (1f - num8 / num7) : (Mathf.Abs(num9) + 1f));
			}
			if (array[2])
			{
				float num10 = Mathf.Abs(vector2.z);
				if (num10 >= array4[0] && num10 <= array4[1])
				{
					array5[2] = true;
				}
				flag3 = array5[2];
				float num11 = transform.position.z - vector.z;
				float num12 = transform.position.z - p_drone.transform.position.z;
				float num13 = num12 / num11;
				num3 = ((!(num13 < 0f)) ? (1f - num12 / num11) : (Mathf.Abs(num13) + 1f));
			}
			if (flag2)
			{
				switch (gauge)
				{
				case Gauge.LeftGauge:
					module.ui.SetGauge(0, num3);
					break;
				case Gauge.RightGauge:
					module.ui.SetGauge(1, num3);
					break;
				case Gauge.LeftPrecisionGauge:
					module.ui.SetPrecisionGauge(0, num3 / 2f);
					break;
				case Gauge.RightPrecisionGauge:
					module.ui.SetPrecisionGauge(1, num3 / 2f);
					break;
				}
			}
			if (flag && num2 > 0f)
			{
				if (flag2)
				{
					module.ui.SetTimer(1, "HOLD", num2 - m_rangeTimer);
				}
				if (flag3)
				{
					m_rangeTimer += Time.deltaTime;
					if (m_rangeTimer > num2)
					{
						module.ui.HilightTimer(1);
						if (flag2)
						{
							module.ui.SetTimer(1, "HOLD", 0f);
							module.ui.SetPrecisionGauge(0, p_flag: false);
							module.ui.SetPrecisionGauge(1, p_flag: false);
							module.ui.SetGauge(0, p_flag: false);
							module.ui.SetGauge(1, p_flag: false);
						}
						return FlowStatus.Complete;
					}
					return FlowStatus.Running;
				}
				m_rangeTimer = 0f;
				return FlowStatus.Running;
			}
			if (flag3)
			{
				module.ui.SetPrecisionGauge(0, p_flag: false);
				module.ui.SetPrecisionGauge(1, p_flag: false);
				module.ui.SetGauge(0, p_flag: false);
				module.ui.SetGauge(1, p_flag: false);
			}
			if (!flag3)
			{
				return FlowStatus.Running;
			}
			return FlowStatus.Complete;
		}

		protected FlowStatus WaitDroneTarget(List<object> p_al, List<UnityEngine.Object> p_ual, Transform p_drone)
		{
			Transform transform = Reflection<object>.Get<Transform>(p_ual, 0);
			bool num = Reflection<object>.Get<bool>(p_al, 0);
			bool flag = Reflection<object>.Get<bool>(p_al, 1);
			float num2 = Reflection<object>.Get<float>(p_al, 2);
			bool flag2 = true;
			if (num && Vector3.Distance(transform.position, p_drone.position) > num2)
			{
				flag2 = false;
			}
			if (flag && Quaternion.Angle(transform.rotation, p_drone.rotation) > num2)
			{
				flag2 = false;
			}
			if (!flag2)
			{
				return FlowStatus.Running;
			}
			return FlowStatus.Complete;
		}

		protected void SetDroneRotationMeter(List<object> p_al, Drone p_drone)
		{
			DebugFlowModuleUI debugFlowModuleUI = (DebugFlowModuleUI)module.ui;
			debugFlowModuleUI.rightMeter.GetComponent<FadeComponent>().FadeIn(0.3f);
			debugFlowModuleUI.leftMeter.GetComponent<FadeComponent>().FadeIn(0.3f);
			_ = new float[3];
			DroneAxis droneAxis = Reflection<object>.Get<DroneAxis>(p_al, 0);
			UpdateAngleDifferential(p_drone);
			switch (droneAxis)
			{
			case DroneAxis.Pitch:
				if (m_cumulativeDroneRotation[0] < 0f)
				{
					debugFlowModuleUI.SetLeftGauge((0f - m_cumulativeDroneRotation[0]) / 178f);
				}
				else
				{
					debugFlowModuleUI.SetRightGauge(m_cumulativeDroneRotation[0] / 178f);
				}
				break;
			case DroneAxis.Yaw:
				if (m_cumulativeDroneRotation[1] < 0f)
				{
					debugFlowModuleUI.SetLeftGauge((0f - m_cumulativeDroneRotation[1]) / 178f);
				}
				else
				{
					debugFlowModuleUI.SetRightGauge(m_cumulativeDroneRotation[1] / 178f);
				}
				break;
			case DroneAxis.Roll:
				if (m_cumulativeDroneRotation[2] < 0f)
				{
					debugFlowModuleUI.SetLeftGauge((0f - m_cumulativeDroneRotation[2]) / 178f);
				}
				else
				{
					debugFlowModuleUI.SetRightGauge(m_cumulativeDroneRotation[2] / 178f);
				}
				break;
			}
		}

		protected bool SetDroneDistanceMeter(List<object> p_al, List<UnityEngine.Object> p_ual, Drone p_drone)
		{
			Transform obj = Reflection<object>.Get<Transform>(p_ual, 0);
			int p_position = Reflection<object>.Get<int>(p_al, 0);
			float num = Vector3.Distance(obj.position, m_droneStartingPosition);
			float num2 = Vector3.Distance(obj.position, p_drone.transform.position);
			float num3 = 1f - num2 / num;
			if (num3 < 1f && num3 > 0f)
			{
				module.ui.SetGauge(p_position, num3);
			}
			return num3 > 0.995f;
		}

		public void SetDroneHeightMeter(Transform p_lower, Transform p_upper, Drone p_drone)
		{
			float num = (p_upper.position.y - p_lower.position.y) * 1.3f;
			float num2 = (p_upper.position.y + p_lower.position.y) / 2f;
			float num3 = (p_drone.transform.position.y - num2) / num;
			num3 += 0.5f;
			module.ui.SetPrecisionGauge(1, num3);
		}

		private void UpdateAngleDifferential(Drone p_drone)
		{
			float[] array = new float[3];
			Quaternion quaternion = Quaternion.Inverse(m_droneRotationState) * p_drone.transform.localRotation;
			array[0] = quaternion.eulerAngles.x;
			array[0] = NormalizeRotation(array[0]);
			m_cumulativeDroneRotation[0] += array[0];
			array[1] = quaternion.eulerAngles.y;
			array[1] = NormalizeRotation(array[1]);
			m_cumulativeDroneRotation[1] += array[1];
			array[2] = quaternion.eulerAngles.z;
			array[2] = NormalizeRotation(array[2]);
			m_cumulativeDroneRotation[2] += array[2];
			m_droneRotationState = p_drone.transform.localRotation;
		}

		private float NormalizeRotation(float angle)
		{
			while (angle > 180f)
			{
				angle -= 360f;
			}
			while (angle < -180f)
			{
				angle += 360f;
			}
			return angle;
		}

		private int GetGoalAngle(List<object> al)
		{
			bool[] array = new bool[3];
			float[] array2 = new float[2];
			float[] array3 = new float[2];
			float[] array4 = new float[2];
			bool[] array5 = new bool[3];
			array[0] = Reflection<object>.Get<bool>(al, 0);
			array2[0] = Reflection<object>.Get<float>(al, 1);
			array2[1] = Reflection<object>.Get<float>(al, 2);
			array5[0] = Reflection<object>.Get<bool>(al, 3);
			array[1] = Reflection<object>.Get<bool>(al, 4);
			array3[0] = Reflection<object>.Get<float>(al, 5);
			array3[1] = Reflection<object>.Get<float>(al, 6);
			array5[1] = Reflection<object>.Get<bool>(al, 7);
			array[2] = Reflection<object>.Get<bool>(al, 8);
			array4[0] = Reflection<object>.Get<float>(al, 9);
			array4[1] = Reflection<object>.Get<float>(al, 10);
			array5[2] = Reflection<object>.Get<bool>(al, 11);
			Gauge gauge = Reflection<object>.Get<Gauge>(al, 16);
			if (gauge != Gauge.LeftPrecisionGauge && gauge != Gauge.RightPrecisionGauge)
			{
				if (array[0])
				{
					return Mathf.Abs((int)((array2[0] > 0f) ? array2[1] : array2[0]));
				}
				if (array[1])
				{
					return Mathf.Abs((int)((array3[0] > 0f) ? array3[1] : array3[0]));
				}
				if (array[2])
				{
					return Mathf.Abs((int)((array4[0] > 0f) ? array4[1] : array4[0]));
				}
				return 180;
			}
			return (int)Reflection<object>.Get<float>(al, 17);
		}

		private int GetGoalDistance(List<UnityEngine.Object> p_ual, List<object> p_al)
		{
			Transform transform = Reflection<object>.Get<Transform>(p_ual, 0);
			Vector3 vector = m_droneStartingPosition;
			float num = Vector3.Distance(m_droneStartingPosition, transform.position);
			bool[] array = new bool[3];
			if (mode == Mode.WaitDroneDistance)
			{
				array[0] = Reflection<object>.Get<bool>(p_al, 0);
				array[1] = Reflection<object>.Get<bool>(p_al, 3);
				array[2] = Reflection<object>.Get<bool>(p_al, 6);
				if (Reflection<object>.Get<bool>(p_al, 15))
				{
					vector = module.simulation.podiums.list[0].transform.position;
				}
				if (array[0])
				{
					num = Mathf.Abs(vector.x - transform.position.x);
				}
				if (array[1])
				{
					num = Mathf.Abs(vector.y - transform.position.y);
				}
				if (array[2])
				{
					num = Mathf.Abs(vector.z - transform.position.z);
				}
				return Mathf.Abs((int)num);
			}
			return Mathf.Abs((int)num);
		}

		private float GetAnglePercisionMiddleRatio(List<object> al)
		{
			bool[] array = new bool[3];
			float[] array2 = new float[2];
			float[] array3 = new float[2];
			float[] array4 = new float[2];
			array[0] = Reflection<object>.Get<bool>(al, 0);
			array2[0] = Reflection<object>.Get<float>(al, 1);
			array2[1] = Reflection<object>.Get<float>(al, 2);
			array[1] = Reflection<object>.Get<bool>(al, 4);
			array3[0] = Reflection<object>.Get<float>(al, 5);
			array3[1] = Reflection<object>.Get<float>(al, 6);
			array[2] = Reflection<object>.Get<bool>(al, 8);
			array4[0] = Reflection<object>.Get<float>(al, 9);
			array4[1] = Reflection<object>.Get<float>(al, 10);
			Gauge gauge = Reflection<object>.Get<Gauge>(al, 16);
			float f = 0.3f;
			if ((gauge == Gauge.LeftPrecisionGauge || gauge == Gauge.RightPrecisionGauge) && mode == Mode.WaitDroneEuler)
			{
				float num = Reflection<object>.Get<float>(al, 17) * 2f;
				if (array[0])
				{
					f = Mathf.Abs(array2[1] - array2[0]) / num;
				}
				if (array[1])
				{
					f = Mathf.Abs(array3[1] - array3[0]) / num;
				}
				if (array[2])
				{
					f = Mathf.Abs(array4[1] - array4[0]) / num;
				}
			}
			return Mathf.Abs(f);
		}

		private float GetDistancePercisionMiddleRatio(List<object> p_al, List<UnityEngine.Object> p_ual)
		{
			Vector3 vector = m_droneStartingPosition;
			bool[] array = new bool[3];
			float[] array2 = new float[2];
			float[] array3 = new float[2];
			float[] array4 = new float[2];
			Transform transform = Reflection<object>.Get<Transform>(p_ual, 0);
			array[0] = Reflection<object>.Get<bool>(p_al, 0);
			array2[0] = Reflection<object>.Get<float>(p_al, 1);
			array2[1] = Reflection<object>.Get<float>(p_al, 2);
			array[1] = Reflection<object>.Get<bool>(p_al, 3);
			array3[0] = Reflection<object>.Get<float>(p_al, 4);
			array3[1] = Reflection<object>.Get<float>(p_al, 5);
			array[2] = Reflection<object>.Get<bool>(p_al, 6);
			array4[0] = Reflection<object>.Get<float>(p_al, 7);
			array4[1] = Reflection<object>.Get<float>(p_al, 8);
			Gauge gauge = Reflection<object>.Get<Gauge>(p_al, 14);
			if (Reflection<object>.Get<bool>(p_al, 15))
			{
				vector = module.simulation.podiums.list[0].transform.position;
			}
			if (gauge != Gauge.LeftPrecisionGauge && gauge != Gauge.RightPrecisionGauge)
			{
				return 0f;
			}
			float num = 0.3f;
			if (array[0])
			{
				num = Mathf.Abs(array2[0] - array2[1]) / (Mathf.Abs(vector.x - transform.position.x) / 2f);
			}
			if (array[1])
			{
				num = Mathf.Abs(array3[0] - array3[1]) / (Mathf.Abs(vector.y - transform.position.y) / 2f);
			}
			if (array[2])
			{
				num = Mathf.Abs(array4[0] - array4[1]) / (Mathf.Abs(vector.z - transform.position.z) / 2f);
			}
			return num - num * 0.45f;
		}

		public void ResetDroneRotationAccumulators(Drone drone)
		{
			m_cumulativeDroneRotation[0] = 0f;
			m_cumulativeDroneRotation[1] = 0f;
			m_cumulativeDroneRotation[2] = 0f;
			base.OnInitialize();
			m_droneRotationState = drone.transform.localRotation;
		}

		public void UpdateWatchColliderMarkers()
		{
			FNSimulationModule[] componentsInChildren = module.transform.GetComponentsInChildren<FNSimulationModule>();
			foreach (FNSimulationModule fNSimulationModule in componentsInChildren)
			{
				if (fNSimulationModule.mode == Mode.UIWatchColliders)
				{
					fNSimulationModule.UpdateMarkerStep();
				}
			}
		}

		public void UpdateMarkerStep()
		{
			m_updateUIMarker = true;
		}

		public void ResetBalloons(Transform trainingElements, bool onlyFirstActive = false)
		{
			foreach (Transform trainingElement in trainingElements)
			{
				TrainingElement component = trainingElement.GetComponent<TrainingElement>();
				if (component != null)
				{
					if (component is Balloon)
					{
						((Balloon)component).playSpawnAnimation = false;
					}
					component.Reset(!onlyFirstActive);
				}
				else
				{
					trainingElement.gameObject.SetActive(value: true);
				}
			}
			trainingElements.gameObject.SetActive(value: true);
			trainingElements.GetChild(0).gameObject.SetActive(value: true);
		}

		public void StopConditionTimer()
		{
			m_conditionTimerStopped = true;
		}

		private void DisableTrails(Drone drone)
		{
			if (!drone)
			{
				Debug.Log("NO DRONE");
				return;
			}
			DroneTrail[] componentsInChildren = drone.GetComponentsInChildren<DroneTrail>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.SetActive(value: false);
			}
		}

		private float SignedAngleBetween(Vector3 a, Vector3 b, Vector3 n)
		{
			float num = Vector3.Angle(a, b);
			float num2 = Mathf.Sign(Vector3.Dot(n, Vector3.Cross(a, b)));
			return num * num2;
		}

		private void OnSkipStopHandler()
		{
			m_skipStarted = false;
			FNSkip.OnSkipStart -= OnSkipStartHandler;
			FNSkip.OnSkipStop -= OnSkipStopHandler;
		}

		private void OnSkipStartHandler()
		{
			m_skipStarted = true;
		}

		public void StartBalloonRadarAudio(Transform p_balloonsPlaceholder)
		{
			foreach (Transform item in p_balloonsPlaceholder)
			{
				module.ui.PlayBalloonRadarAudio(item.gameObject);
			}
		}

		public void StopBalloonRadarAudio(Balloon p_balloon)
		{
			module.ui.StopBalloonRadarAudio(p_balloon.gameObject);
		}

		public void SetTPVBack(float distance = 0.4f)
		{
			module.simulation.cameras.Get(0).SetTPVSmooth(module.simulation.drones.Get(0), distance);
		}

		private void OnDestroy()
		{
			if (m_runningActivity != null)
			{
				m_runningActivity.Stop();
				m_runningActivity = null;
			}
		}
	}
}

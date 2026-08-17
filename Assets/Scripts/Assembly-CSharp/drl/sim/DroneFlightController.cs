using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using drl.sim.rci;
using drl.sim.thread;
using thelab.core;

namespace drl.sim
{
	public class DroneFlightController : DronePart
	{
		[Serializable]
		public class Sensor
		{
			public DSAccelerometer accelerometer;

			public DSBarometer barometer;

			public DSGyro gyro;

			public DSInertial inertial;

			public DSGps gps;

			public DSCollision collision;

			public DSElectrical electrical;

			public bool stuck
			{
				get
				{
					if (!gyro || !accelerometer)
					{
						return false;
					}
					if (!gyro.flipped)
					{
						return false;
					}
					if (accelerometer.local.magnitude > 0.05f)
					{
						return false;
					}
					return true;
				}
			}

			public Sensor(DroneFlightController p_fc)
			{
				foreach (DroneSensor sensor in p_fc.sensors)
				{
					if (sensor is DSAccelerometer)
					{
						accelerometer = (DSAccelerometer)sensor;
					}
					if (sensor is DSBarometer)
					{
						barometer = (DSBarometer)sensor;
					}
					if (sensor is DSGyro)
					{
						gyro = (DSGyro)sensor;
					}
					if (sensor is DSInertial)
					{
						inertial = (DSInertial)sensor;
					}
					if (sensor is DSGps)
					{
						gps = (DSGps)sensor;
					}
					if (sensor is DSCollision)
					{
						collision = (DSCollision)sensor;
					}
					if (sensor is DSElectrical)
					{
						electrical = (DSElectrical)sensor;
					}
				}
			}
		}

		[Serializable]
		public class Mode
		{
			public FCTargetProcess target;

			public FCDJIProcess dji;

			public FCAcroProcess acro;

			public FCArcadeProcess arcade;

			public Mode(DroneFlightController p_fc)
			{
				foreach (FCProcess mode in p_fc.modes)
				{
					if (mode is FCTargetProcess)
					{
						target = (FCTargetProcess)mode;
					}
					if (mode is FCDJIProcess)
					{
						dji = (FCDJIProcess)mode;
					}
					if (mode is FCAcroProcess)
					{
						acro = (FCAcroProcess)mode;
					}
					if (mode is FCArcadeProcess)
					{
						arcade = (FCArcadeProcess)mode;
					}
				}
			}
		}

		[Serializable]
		public class Process
		{
			public FCThrustProcess thrust;

			public FCYawProcess yaw;

			public FCPitchProcess pitch;

			public FCRollProcess roll;

			public FCAltitudeProcess altitude;

			public FCLevelProcess level;

			public FCLimiterProcess limiter;

			public FCStabilityProcess stability;

			public FCBalanceProcess balance;

			public FCTrainingProcess training;

			public FCSoftlockProcess softlock;

			public FlightControllerProcess active;

			public Process(DroneFlightController p_fc)
			{
				p_fc.m_processListInitialized = false;
				foreach (FCProcess process in p_fc.processes)
				{
					if (process is FCThrustProcess)
					{
						thrust = (FCThrustProcess)process;
					}
					if (process is FCYawProcess)
					{
						yaw = (FCYawProcess)process;
					}
					if (process is FCPitchProcess)
					{
						pitch = (FCPitchProcess)process;
					}
					if (process is FCRollProcess)
					{
						roll = (FCRollProcess)process;
					}
					if (process is FCAltitudeProcess)
					{
						altitude = (FCAltitudeProcess)process;
					}
					if (process is FCLevelProcess)
					{
						level = (FCLevelProcess)process;
					}
					if (process is FCLimiterProcess)
					{
						limiter = (FCLimiterProcess)process;
					}
					if (process is FCStabilityProcess)
					{
						stability = (FCStabilityProcess)process;
					}
					if (process is FCBalanceProcess)
					{
						balance = (FCBalanceProcess)process;
					}
					if (process is FCTrainingProcess)
					{
						training = (FCTrainingProcess)process;
					}
					if (process is FCSoftlockProcess)
					{
						softlock = (FCSoftlockProcess)process;
					}
				}
			}
		}

		[Serializable]
		public class FrameLayoutEntry
		{
			public FrameLayoutType type;

			public int[] indexes;

			public bool[] spins;
		}

		public static class Defaults
		{
			public static class Fast
			{
				public const float djiAngleMin = 50f;

				public const float djiAngleMax = 50f;

				public const float djiSpeedMin = 45f;

				public const float djiSpeedMax = 45f;

				public const float trainingScale = 1f;

				public const float altitudeSpeed = 0f;

				public const float altitudeAngle = 30f;

				public const float targetAngle = 80f;

				public const float targetSpeed = 20f;

				public const float targetError = 0.05f;

				public const float targetScale = 1.6f;

				public const float limiterAngle = 45f;
			}

			public static class Medium
			{
				public const float djiAngleMin = 40f;

				public const float djiAngleMax = 40f;

				public const float djiSpeedMin = 18f;

				public const float djiSpeedMax = 18f;

				public const float trainingScale = 1f;

				public const float altitudeSpeed = 4f;

				public const float altitudeAngle = 30f;

				public const float targetAngle = 70f;

				public const float targetSpeed = 14f;

				public const float targetError = 0.05f;

				public const float targetScale = 1.6f;

				public const float limiterAngle = 45f;
			}

			public static class Slow
			{
				public const float djiAngleMin = 30f;

				public const float djiAngleMax = 30f;

				public const float djiSpeedMin = 9f;

				public const float djiSpeedMax = 9f;

				public const float trainingScale = 1f;

				public const float altitudeSpeed = 2f;

				public const float altitudeAngle = 30f;

				public const float targetAngle = 60f;

				public const float targetSpeed = 8f;

				public const float targetError = 0.05f;

				public const float targetScale = 1.6f;

				public const float limiterAngle = 45f;
			}

			public const float djiAngleMin = 5f;

			public const float djiAngleMax = 53f;

			public const float djiSpeedMin = 4f;

			public const float djiSpeedMax = 45f;

			public const float trainingScale = 0.5f;

			public const float altitudeSpeed = 0f;

			public const float altitudeAngle = 30f;

			public const float targetAngle = 80f;

			public const float targetSpeed = 20f;

			public const float targetError = 0.05f;

			public const float targetScale = 1.6f;

			public const float limiterAngle = 45f;
		}

		public struct Current
		{
			public float djiAngleMin;

			public float djiAngleMax;

			public float djiSpeedMin;

			public float djiSpeedMax;

			public float trainingScale;

			public float altitudeSpeed;

			public float altitudeAngle;

			public float targetAngle;

			public float targetSpeed;

			public float targetError;

			public float targetScale;

			public float limiterAngle;
		}

		public Current parameters;

		public float[] wattDrop = new float[4];

		public DroneMotorSpec.BenchData[] mbd = new DroneMotorSpec.BenchData[4];

		private bool m_enabled;

		[HideInInspector]
		public FlightControllerMode mode = FlightControllerMode.Acro;

		[SerializeField]
		private bool m_armed;

		[SerializeField]
		private bool m_turtle;

		public bool external;

		[SerializeField]
		private bool m_threaded;

		[SerializeField]
		private List<FrameLayoutEntry> m_layouts = new List<FrameLayoutEntry>(new FrameLayoutEntry[1]
		{
			new FrameLayoutEntry
			{
				type = FrameLayoutType.QuadX,
				indexes = new int[4] { 1, 0, 2, 3 },
				spins = new bool[4] { false, true, true, false }
			}
		});

		private List<DroneSensor> m_sensors;

		private bool m_sensorListInitialized;

		private Sensor m_sensor;

		private bool m_hasSensor;

		private List<FCProcess> m_processes;

		private bool m_processListInitialized;

		private Process m_process;

		private bool m_hasProcess;

		private List<FCProcess> m_modes;

		private bool m_modeListInitialized;

		private Mode m_modeProcess;

		private bool m_hasMode;

		public float minSignal = 0.05f;

		public SignalVector m_rawSignal;

		public SignalVector m_signal;

		public SignalVector m_normalizedSignal;

		public bool allowThrottle = true;

		public bool allowPitch = true;

		public bool allowYaw = true;

		public bool allowRoll = true;

		public List<float> inputs;

		[Range(-1f, 1f)]
		public float debugThrottle;

		[Range(-1f, 1f)]
		public float debugPitch;

		[Range(-1f, 1f)]
		public float debugYaw;

		[Range(-1f, 1f)]
		public float debugRoll;

		internal FCProfileData m_profile;

		private bool m_profile_dirty;

		private bool m_hasProfile;

		private List<float> m_inputs_back;

		private int correctionId;

		private Stopwatch stopwatch;

		private long startTime;

		private long timeSpentInside;

		private long oneSecondPeriod;

		private long totalTimeInside;

		public float percentageTimeSpentInAThreadPerSecond;

		private bool isLegacy;

		private bool m_landed;

		private int loopExceptionCount;

		public int RunningFrequency;

		[SerializeField]
		private int targetLoopFrequency = 10000;

		private int IterationNumber = 50;

		[HideInInspector]
		public bool AllowThreadRun;

		private bool calculateFC;

		private float originalFixedDeltaTime;

		private float m_lastFixedStepTime = float.NegativeInfinity;

		public DroneThreadedMixer threadedMixer;

		public new bool enabled
		{
			get
			{
				return m_enabled;
			}
			set
			{
				base.enabled = (m_enabled = value);
			}
		}

		public bool armed
		{
			get
			{
				return m_armed;
			}
			set
			{
				if (m_armed != value)
				{
					m_armed = value;
					if (value)
					{
						Reset();
					}
					if ((bool)base.drone)
					{
						base.drone.Dispatch(m_armed ? DroneEventType.Armed : DroneEventType.Disarmed);
					}
				}
			}
		}

		public bool turtle
		{
			get
			{
				return m_turtle;
			}
			set
			{
				m_turtle = value;
				if ((bool)base.drone)
				{
					base.drone.Dispatch(m_turtle ? DroneEventType.TurtleOn : DroneEventType.TurtleOff);
				}
			}
		}

		public bool threaded
		{
			get
			{
				return m_threaded;
			}
			set
			{
				m_threaded = value;
			}
		}

		public List<FrameLayoutEntry> layouts => m_layouts;

		public List<DroneSensor> sensors
		{
			get
			{
				if (m_sensorListInitialized && m_sensors != null && m_sensors.Count > 0)
				{
					return m_sensors;
				}
				m_sensorListInitialized = true;
				m_sensors = new List<DroneSensor>();
				Transform transform = base.transform.Find("sensors");
				for (int i = 0; i < transform.childCount; i++)
				{
					m_sensors.Add(transform.GetChild(i).GetComponent<DroneSensor>());
				}
				if (transform.Find("electrical") == null)
				{
					GameObject gameObject = new GameObject("electrical");
					gameObject.transform.parent = transform;
					gameObject.transform.localPosition = Vector3.zero;
					m_sensors.Add(gameObject.AddComponent<DSElectrical>());
				}
				return m_sensors;
			}
		}

		public Sensor sensor
		{
			get
			{
				if (m_hasSensor)
				{
					return m_sensor;
				}
				if (m_sensor == null)
				{
					m_sensor = new Sensor(this);
				}
				m_hasSensor = true;
				return m_sensor;
			}
		}

		public List<FCProcess> processes
		{
			get
			{
				if (m_processListInitialized && m_processes != null && m_processes.Count > 0)
				{
					return m_processes;
				}
				m_processListInitialized = true;
				m_processes = new List<FCProcess>();
				Transform transform = base.transform.Find("processes");
				for (int i = 0; i < transform.childCount; i++)
				{
					m_processes.Add(transform.GetChild(i).GetComponent<FCProcess>());
				}
				return m_processes;
			}
		}

		public Process process
		{
			get
			{
				if (m_hasProcess)
				{
					return m_process;
				}
				if (m_process == null)
				{
					m_process = new Process(this);
				}
				m_hasProcess = true;
				return m_process;
			}
		}

		public List<FCProcess> modes
		{
			get
			{
				if (m_modeListInitialized && m_modes != null && m_modes.Count > 0)
				{
					return m_modes;
				}
				m_modeListInitialized = true;
				m_modes = new List<FCProcess>();
				Transform transform = base.transform.Find("modes");
				for (int i = 0; i < transform.childCount; i++)
				{
					m_modes.Add(transform.GetChild(i).GetComponent<FCProcess>());
				}
				return m_modes;
			}
		}

		public Mode modeProcess
		{
			get
			{
				if (m_hasMode)
				{
					return m_modeProcess;
				}
				if (m_modeProcess == null)
				{
					m_modeProcess = new Mode(this);
				}
				m_hasMode = true;
				return m_modeProcess;
			}
		}

		public bool drainBatteries
		{
			get
			{
				if (!base.attached || !m_drone.hasPhysics)
				{
					return false;
				}
				return m_drone.physics.batteryDrain;
			}
		}

		public bool batterySag
		{
			get
			{
				if (!base.attached || !m_drone.hasPhysics)
				{
					return false;
				}
				return m_drone.physics.batterySag;
			}
		}

		public SignalVector rawSignal
		{
			get
			{
				return m_rawSignal;
			}
			set
			{
				m_rawSignal = value;
				m_signal = TransformSignal(value, base.drone.profile);
				m_normalizedSignal = NormalizeSignal(value);
			}
		}

		public SignalVector signal
		{
			get
			{
				return m_signal;
			}
			set
			{
				m_signal = value;
			}
		}

		public SignalVector normalizedSignal => m_normalizedSignal;

		public FCProfileData profile
		{
			get
			{
				if (m_hasProfile)
				{
					return m_profile;
				}
				if (m_profile == null)
				{
					m_profile_dirty = true;
					m_profile = new FCProfileData();
				}
				m_hasProfile = true;
				return m_profile;
			}
			set
			{
				m_profile = value;
				m_profile_dirty = true;
				m_hasProfile = value != null;
			}
		}

		public bool softLock
		{
			get
			{
				return IsProcessActive(FlightControllerProcess.Lock);
			}
			set
			{
				SetProcess(FlightControllerProcess.Lock, value);
			}
		}

		public float softLockOffset
		{
			get
			{
				return process.softlock.allowedError;
			}
			set
			{
				process.softlock.allowedError = value;
			}
		}

		public bool landed
		{
			get
			{
				return m_landed;
			}
			set
			{
				m_landed = value;
			}
		}

		public int LoopFrequency => IterationNumber;

		private void OnDisable()
		{
		}

		public void Reset()
		{
			for (int i = 0; i < processes.Count; i++)
			{
				processes[i].Reset();
			}
			for (int j = 0; j < modes.Count; j++)
			{
				modes[j].Reset();
			}
			for (int k = 0; k < sensors.Count; k++)
			{
				sensors[k].Reset();
			}
			for (int l = 0; l < base.drone.body.frame.escs.Count; l++)
			{
				DroneESC droneESC = base.drone.body.frame.escs[l];
				if ((bool)droneESC)
				{
					droneESC.input = 0f;
					if (droneESC.hasMotor)
					{
						droneESC.motor.rpm = 0f;
					}
				}
			}
		}

		public void Boot()
		{
			m_processListInitialized = false;
			parameters.djiAngleMin = 5f;
			parameters.djiAngleMax = 53f;
			parameters.djiSpeedMin = 4f;
			parameters.djiSpeedMax = 45f;
			parameters.trainingScale = 0.5f;
			parameters.altitudeSpeed = 0f;
			parameters.altitudeAngle = 30f;
			parameters.targetError = 0.05f;
			Transform p_target = base.transform.Find("sensors");
			m_sensors = Hierarchy.FindAll<DroneSensor>(p_target);
			m_sensor = new Sensor(this);
			Transform p_target2 = base.transform.Find("processes");
			m_processes = Hierarchy.FindAll<FCProcess>(p_target2);
			for (int i = 0; i < processes.Count; i++)
			{
				processes[i].fc = this;
				processes[i].Boot();
				processes[i].SetLayout(FrameLayoutType.QuadX);
			}
			Transform p_target3 = base.transform.Find("modes");
			m_modes = Hierarchy.FindAll<FCProcess>(p_target3);
			for (int j = 0; j < modes.Count; j++)
			{
				modes[j].fc = this;
				modes[j].Boot();
				modes[j].SetLayout(FrameLayoutType.QuadX);
			}
			inputs = new List<float>();
			m_inputs_back = new List<float>();
			m_enabled = base.enabled && base.gameObject.activeInHierarchy;
			_ = layouts[0];
			int count = base.drone.body.frame.escs.Count;
			for (int k = 0; k < count; k++)
			{
				inputs.Add(0f);
				m_inputs_back.Add(0f);
			}
			m_profile_dirty = true;
			isLegacy = true;
			Validate();
		}

		public bool Validate()
		{
			int num = 1 & ((process != null) ? 1 : 0);
			if (num == 0)
			{
				UnityEngine.Debug.LogError("DroneFlightController.Validate> Missing 'process' at [" + Hierarchy.Path(base.transform) + "]");
			}
			int num2 = num & ((modeProcess != null) ? 1 : 0);
			if (num2 == 0)
			{
				UnityEngine.Debug.LogError("DroneFlightController.Validate> Missing 'modeProcess' at [" + Hierarchy.Path(base.transform) + "]");
			}
			int num3 = num2 & ((sensor != null) ? 1 : 0);
			if (num3 == 0)
			{
				UnityEngine.Debug.LogError("DroneFlightController.Validate> Missing 'sensor' at [" + Hierarchy.Path(base.transform) + "]");
			}
			int num4 = num3 & ((profile != null) ? 1 : 0);
			if (num4 == 0)
			{
				UnityEngine.Debug.LogError("DroneFlightController.Validate> Missing 'profile' at [" + Hierarchy.Path(base.transform) + "]");
			}
			return (byte)num4 != 0;
		}

		public float PitchCorrection(float p_value)
		{
			return PidCorrection(0, p_value);
		}

		public float RollCorrection(float p_value)
		{
			return PidCorrection(1, p_value);
		}

		public float YawCorrection(float p_value)
		{
			return PidCorrection(2, p_value);
		}

		public float PidCorrection(int p_pid, float p_value)
		{
			if (!base.drone.hasPhysics)
			{
				return 0f;
			}
			float[] array = null;
			switch (p_pid)
			{
			case 0:
				array = base.drone.profile.pidCorrectionP;
				break;
			case 1:
				array = base.drone.profile.pidCorrectionR;
				break;
			case 2:
				array = base.drone.profile.pidCorrectionY;
				break;
			}
			if (array == null || array.Length == 0)
			{
				return 1f;
			}
			correctionId = Mathf.FloorToInt(rawSignal.throttle * 10f);
			if (correctionId < 0)
			{
				return array[0];
			}
			if (correctionId >= array.Length - 1)
			{
				return array[array.Length - 1];
			}
			return Mathf.Lerp(array[correctionId], array[correctionId + 1], rawSignal.throttle * 10f - (float)correctionId);
		}

		public void SetLayout(FrameLayoutType p_type)
		{
			FrameLayoutEntry frameLayoutEntry = null;
			for (int i = 0; i < layouts.Count; i++)
			{
				if (layouts[i].type == p_type)
				{
					frameLayoutEntry = layouts[i];
					break;
				}
			}
			if (frameLayoutEntry == null)
			{
				UnityEngine.Debug.LogWarning("DroneFlightController> Layout [" + p_type.ToString() + "] not found!");
				return;
			}
			int count = base.drone.body.frame.escs.Count;
			for (int j = 0; j < count; j++)
			{
				int index = frameLayoutEntry.indexes[j];
				DroneESC droneESC = base.drone.body.frame.escs[index];
				if ((bool)droneESC && (bool)droneESC.motor)
				{
					droneESC.motor.ccw = frameLayoutEntry.spins[j];
					if ((bool)droneESC.motor.prop)
					{
						droneESC.motor.prop.ccw = droneESC.motor.ccw;
						droneESC.motor.prop.transform.localScale = new Vector3(droneESC.motor.ccw ? (-1f) : 1f, 1f, 1f);
					}
				}
			}
		}

		public void FCThread()
		{
			stopwatch = new Stopwatch();
			stopwatch.Start();
			while (AllowThreadRun)
			{
				if (calculateFC)
				{
					startTime = stopwatch.ElapsedMilliseconds;
					if (oneSecondPeriod == 0L)
					{
						oneSecondPeriod = startTime;
					}
					calculateFC = false;
					for (int i = 0; i < IterationNumber; i++)
					{
						Loop(originalFixedDeltaTime / (float)IterationNumber, p_thread: true);
					}
					long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
					timeSpentInside = elapsedMilliseconds - startTime;
					totalTimeInside += timeSpentInside;
					if (elapsedMilliseconds - oneSecondPeriod >= 1000)
					{
						percentageTimeSpentInAThreadPerSecond = (float)totalTimeInside / 10f;
						oneSecondPeriod = 0L;
						totalTimeInside = 0L;
					}
					if (timeSpentInside >= (int)(originalFixedDeltaTime * 1000f))
					{
						IterationNumber /= 2;
					}
				}
			}
			stopwatch.Stop();
		}

		public void FCThreadSingle()
		{
			stopwatch = new Stopwatch();
			stopwatch.Start();
			if (calculateFC)
			{
				startTime = stopwatch.ElapsedMilliseconds;
				if (oneSecondPeriod == 0L)
				{
					oneSecondPeriod = startTime;
				}
				calculateFC = false;
				for (int i = 0; i < IterationNumber; i++)
				{
					Loop(originalFixedDeltaTime / (float)IterationNumber, p_thread: true);
				}
				long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
				timeSpentInside = elapsedMilliseconds - startTime;
				totalTimeInside += timeSpentInside;
				if (elapsedMilliseconds - oneSecondPeriod >= 1000)
				{
					percentageTimeSpentInAThreadPerSecond = (float)totalTimeInside / 10f;
					oneSecondPeriod = 0L;
					totalTimeInside = 0L;
				}
				if (timeSpentInside >= (int)(originalFixedDeltaTime * 1000f))
				{
					IterationNumber /= 2;
				}
			}
			stopwatch.Stop();
		}

		public bool IsProcessActive(FlightControllerProcess p_process)
		{
			return (process.active & p_process) != 0;
		}

		public void SetProcess(FlightControllerProcess p_process, bool p_flag, bool p_cleanSwitch = true)
		{
			process.active = (p_flag ? (process.active | p_process) : (process.active & ~p_process));
			switch (p_process)
			{
			default:
				_ = 8192;
				break;
			case FlightControllerProcess.Altitude:
				process.altitude.enabled = p_flag;
				if (p_flag && p_cleanSwitch)
				{
					process.altitude.Lock();
				}
				break;
			case FlightControllerProcess.Level:
				process.level.enabled = p_flag;
				break;
			case FlightControllerProcess.Limiter:
				process.limiter.enabled = p_flag;
				break;
			case FlightControllerProcess.Training:
				process.training.enabled = p_flag;
				break;
			case FlightControllerProcess.Lock:
				process.softlock.enabled = p_flag;
				break;
			case FlightControllerProcess.None:
			case (FlightControllerProcess)3:
			case (FlightControllerProcess)5:
			case (FlightControllerProcess)6:
			case (FlightControllerProcess)7:
				break;
			}
		}

		public void ApplyParameters(bool p_applyPhysics = true)
		{
			if (IsProcessActive(FlightControllerProcess.Training))
			{
				process.training.scale = parameters.trainingScale;
			}
			if (IsProcessActive(FlightControllerProcess.Limiter))
			{
				process.limiter.limit = ((parameters.limiterAngle < 1f) ? 45f : parameters.limiterAngle);
			}
			if (IsProcessActive(FlightControllerProcess.Altitude))
			{
				process.altitude.angleLimit = parameters.altitudeAngle;
				process.altitude.speedLimit = parameters.altitudeSpeed;
			}
			if (IsProcessActive(FlightControllerProcess.Level))
			{
				process.level.affectYaw = true;
			}
			switch (mode)
			{
			case FlightControllerMode.Target:
				process.altitude.angleLimit = parameters.targetAngle;
				process.altitude.speedLimit = parameters.targetSpeed;
				modeProcess.dji.param_minAngle = parameters.targetAngle;
				modeProcess.dji.param_maxAngle = parameters.targetAngle;
				modeProcess.dji.param_minSpeed = parameters.targetSpeed;
				modeProcess.dji.param_maxSpeed = parameters.targetSpeed;
				modeProcess.target.speedLimit = parameters.targetSpeed;
				modeProcess.target.outputScale = parameters.targetScale;
				break;
			case FlightControllerMode.Beginner:
			case FlightControllerMode.DJI:
				modeProcess.dji.param_minAngle = parameters.djiAngleMin;
				modeProcess.dji.param_maxAngle = parameters.djiAngleMax;
				modeProcess.dji.param_minSpeed = parameters.djiSpeedMin;
				modeProcess.dji.param_maxSpeed = parameters.djiSpeedMax;
				break;
			}
			if (!p_applyPhysics)
			{
				return;
			}
			if (isLegacy)
			{
				DronePhysics simulation = base.drone.simulation;
				if (simulation != null)
				{
					bool flag = mode == FlightControllerMode.Beginner || mode == FlightControllerMode.DJI || mode == FlightControllerMode.Target;
					simulation.SetBeginner(flag);
					m_drone.physics = (flag ? m_drone.djiphysics : m_drone.defaultphysics);
					m_drone.profile = (flag ? m_drone.djiprofile : m_drone.defaultprofile);
				}
			}
			else
			{
				SetPhysicsSettings(mode == FlightControllerMode.Beginner || mode == FlightControllerMode.DJI || mode == FlightControllerMode.Target);
			}
		}

		public void SetPhysicsSettings(bool dji)
		{
			UnityEngine.Debug.Log("Setting physics settings for new drone...");
			if (!m_drone)
			{
				return;
			}
			m_drone.physics = (dji ? m_drone.djiphysics : m_drone.defaultphysics);
			m_drone.profile = (dji ? m_drone.djiprofile : m_drone.defaultprofile);
			if (dji && m_drone.physics.overrideMaxSpeed)
			{
				modeProcess.dji.param_maxSpeed = m_drone.physics.maxSpeedOverride;
			}
			if (m_drone.physics == null)
			{
				return;
			}
			if (m_drone.hasBody && m_drone.body.hasFrame && m_drone.body.frame.escs != null && m_drone.body.frame.escs.Count > 0)
			{
				for (int i = 0; i < m_drone.body.frame.escs.Count; i++)
				{
					DroneESC droneESC = m_drone.body.frame.escs[i];
					if (droneESC != null && droneESC.motor != null && droneESC.motor.prop != null)
					{
						droneESC.motor.prop.SetEfficiency(m_drone.physics.efficiencyMax, m_drone.physics.efficiencyZero);
					}
				}
			}
			minSignal = m_drone.profile.minSignal;
			if (m_drone.physics.mass > 0.001f)
			{
				m_drone.rigidbody.rb.mass = m_drone.physics.mass;
			}
			profile.pid.pitch = m_drone.profile.pitchPID;
			profile.pid.roll = m_drone.profile.rollPID;
			profile.pid.yaw = m_drone.profile.yawPID;
		}

		public void SetMode(FlightControllerMode p_mode)
		{
			foreach (FCProcess mode in modes)
			{
				mode.enabled = false;
			}
			foreach (FCProcess process in processes)
			{
				process.enabled = false;
			}
			this.process.active = FlightControllerProcess.None;
			RCI.ClearYawDeadzoneOverride();
			FlightControllerMode flightControllerMode = this.mode;
			this.mode = p_mode;
			switch (p_mode)
			{
			case FlightControllerMode.AcroClassic:
				modeProcess.acro.enabled = true;
				this.process.thrust.enabled = true;
				this.process.yaw.enabled = true;
				this.process.pitch.enabled = true;
				this.process.roll.enabled = true;
				this.process.balance.enabled = true;
				break;
			case FlightControllerMode.Arcade:
				modeProcess.acro.enabled = true;
				this.process.thrust.enabled = true;
				this.process.yaw.enabled = false;
				this.process.pitch.enabled = true;
				this.process.roll.enabled = false;
				this.process.balance.enabled = true;
				break;
			case FlightControllerMode.Acro:
				modeProcess.acro.enabled = true;
				this.process.thrust.enabled = true;
				this.process.yaw.enabled = true;
				this.process.pitch.enabled = true;
				this.process.roll.enabled = true;
				this.process.balance.enabled = true;
				break;
			case FlightControllerMode.Intermediate:
			case FlightControllerMode.Training:
				modeProcess.acro.enabled = true;
				this.process.thrust.enabled = true;
				this.process.yaw.enabled = true;
				this.process.pitch.enabled = true;
				this.process.roll.enabled = true;
				this.process.balance.enabled = true;
				if (p_mode == FlightControllerMode.Intermediate)
				{
					SetProcess(FlightControllerProcess.Limiter, p_flag: true);
				}
				break;
			case FlightControllerMode.Baro:
				modeProcess.arcade.enabled = true;
				this.process.thrust.enabled = true;
				this.process.balance.enabled = true;
				SetProcess(FlightControllerProcess.Altitude, p_flag: true);
				SetProcess(FlightControllerProcess.Level, p_flag: true);
				SetProcess(FlightControllerProcess.Training, p_flag: true);
				break;
			case FlightControllerMode.Speed:
				UnityEngine.Debug.LogError("FC:SetMode: mode Speed is deprecated, use Target instead");
				break;
			case FlightControllerMode.Lock:
			case FlightControllerMode.Target:
				modeProcess.target.enabled = true;
				this.process.balance.enabled = true;
				SetProcess(FlightControllerProcess.Altitude, p_flag: true, flightControllerMode != FlightControllerMode.DJI && flightControllerMode != FlightControllerMode.Beginner && flightControllerMode != FlightControllerMode.Target);
				modeProcess.dji.enabled = true;
				if (p_mode == FlightControllerMode.Lock)
				{
					this.mode = FlightControllerMode.Target;
					modeProcess.target.LockToCurrent();
				}
				break;
			case FlightControllerMode.Level:
				modeProcess.arcade.enabled = true;
				this.process.thrust.enabled = true;
				this.process.balance.enabled = true;
				SetProcess(FlightControllerProcess.Level, p_flag: true);
				break;
			case FlightControllerMode.Beginner:
			case FlightControllerMode.DJI:
				modeProcess.dji.enabled = true;
				this.process.balance.enabled = true;
				RCI.OverrideYawDeadzone();
				SetProcess(FlightControllerProcess.Altitude, p_flag: true, flightControllerMode != FlightControllerMode.DJI && flightControllerMode != FlightControllerMode.Beginner && flightControllerMode != FlightControllerMode.Target);
				break;
			default:
				modeProcess.arcade.enabled = true;
				this.process.thrust.enabled = true;
				this.process.balance.enabled = true;
				break;
			}
			ApplyParameters(flightControllerMode != p_mode);
		}

		public void ReadSignal()
		{
			if ((bool)m_drone && !m_drone.ready)
			{
				loopExceptionCount = 0;
				m_rawSignal.Set(0f, 0f, 0f, 0f);
				m_signal.Set(0f, 0f, 0f, 0f);
				m_normalizedSignal.Set(0f, 0f, 0f, 0f);
				return;
			}
			if (!enabled)
			{
				loopExceptionCount = 0;
				m_rawSignal.Set(0f, 0f, 0f, 0f);
				m_signal.Set(0f, 0f, 0f, 0f);
				m_normalizedSignal.Set(0f, 0f, 0f, 0f);
				return;
			}
			if (!armed)
			{
				if (!external)
				{
					m_rawSignal.Set(0f, 0f, 0f, 0f);
					m_signal.Set(0f, 0f, 0f, 0f);
					m_normalizedSignal.Set(0f, 0f, 0f, 0f);
				}
				loopExceptionCount = 0;
				return;
			}
			IsProcessActive(FlightControllerProcess.Debug);
			m_rawSignal = base.drone.receiver.signal;
			if (!allowThrottle)
			{
				m_rawSignal.throttle = Mathf.Clamp(debugThrottle, 0f, 1f);
				m_rawSignal.altitude = Mathf.Clamp(debugThrottle, -1f, 1f);
			}
			if (!allowPitch)
			{
				m_rawSignal.pitch = Mathf.Clamp(debugPitch, -1f, 1f);
			}
			if (!allowYaw)
			{
				m_rawSignal.yaw = Mathf.Clamp(debugYaw, -1f, 1f);
			}
			if (!allowRoll)
			{
				m_rawSignal.roll = Mathf.Clamp(debugRoll, -1f, 1f);
			}
			if (IsProcessActive(FlightControllerProcess.Limiter))
			{
				m_rawSignal.roll = Mathf.Clamp(m_rawSignal.roll, -0.9f, 0.9f);
				m_rawSignal.pitch = Mathf.Clamp(m_rawSignal.pitch, -0.9f, 0.9f);
			}
			if (turtle)
			{
				m_rawSignal.roll = Mathf.Clamp(m_rawSignal.roll, -0.9f, 0.9f);
				m_rawSignal.pitch = Mathf.Clamp(m_rawSignal.pitch, -0.9f, 0.9f);
				m_rawSignal.throttle = 0f;
				m_rawSignal.yaw = 0f;
			}
			m_signal = TransformSignal(m_rawSignal, base.drone.profile);
			m_normalizedSignal = NormalizeSignal(m_rawSignal);
		}

		public void Loop(float p_dt, bool p_thread = false)
		{
			if ((bool)m_drone && !m_drone.ready)
			{
				loopExceptionCount = 0;
				return;
			}
			if (!enabled)
			{
				loopExceptionCount = 0;
				return;
			}
			List<DroneESC> escs;
			if (!armed)
			{
				if (!external)
				{
					escs = base.drone.body.frame.escs;
					for (int i = 0; i < escs.Count; i++)
					{
						DroneESC droneESC = escs[i];
						droneESC.input = 0f;
						droneESC.motor.rpm = 0f;
						droneESC.motor.rpmAudio = 0f;
					}
				}
				loopExceptionCount = 0;
				return;
			}
			if (landed && sensor.inertial != null && sensor.inertial.speed > 1f && base.drone.FarFromResetPosition)
			{
				landed = false;
				base.drone.StabilizeDroneOnGround(p_flag: false);
				_ = base.drone.hasThreaded;
			}
			bool num = HasPower();
			bool flag = mode == FlightControllerMode.Bypass;
			IsProcessActive(FlightControllerProcess.Debug);
			bool flag2 = num && !flag;
			ReadSignal();
			if (!p_thread)
			{
				for (int j = 0; j < m_inputs_back.Count; j++)
				{
					inputs[j] = 0f;
				}
				if (flag2)
				{
					for (int k = 0; k < modes.Count; k++)
					{
						modes[k].Loop(p_dt);
					}
				}
				if (flag2)
				{
					for (int l = 0; l < processes.Count; l++)
					{
						processes[l].Loop(p_dt);
					}
				}
			}
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			float num5 = base.drone.body.frame.batteries.Count;
			float num6 = ((num5 <= 0f) ? 0f : (1f / num5));
			for (int m = 0; m < base.drone.body.frame.batteries.Count; m++)
			{
				DroneBattery droneBattery = base.drone.body.frame.batteries[m];
				num2 += (base.drone.physics.batteryDrain ? droneBattery.voltage : droneBattery.max) * num6;
				num3 += droneBattery.resistance;
				num4 += (base.drone.physics.batteryDrain ? droneBattery.mah : droneBattery.capacity);
			}
			if (!base.drone.physics.batterySag)
			{
				num3 = 0.0001f;
			}
			num2 = ((num5 <= 0f) ? 16.8f : (num2 / num5));
			escs = base.drone.body.frame.escs;
			for (int n = 0; n < escs.Count; n++)
			{
				DroneESC droneESC2 = escs[n];
				if (p_thread)
				{
					droneESC2.voltage = num2;
					mbd[n] = (droneESC2.hasMotor ? droneESC2.motor.spec.data : null);
					droneESC2.amperes = ((mbd == null) ? 0f : mbd[n].amperes.Evaluate(droneESC2.input));
					droneESC2.motor.amperes = droneESC2.amperes;
					float voltage = droneESC2.voltage;
					float num7 = droneESC2.amperes * num3 * 0.001f;
					droneESC2.motor.voltage = voltage - num7;
					float a = num4 * 1E-06f * droneESC2.motor.voltage;
					droneESC2.motor.watts = ((mbd == null) ? 0f : mbd[n].watts.Evaluate(droneESC2.amperes));
					float num8 = p_dt * 0.000277777f;
					float b = droneESC2.motor.watts * num8;
					b = Mathf.Min(a, b);
					float num9 = ((num8 <= 0f) ? 0f : (b / num8));
					wattDrop[n] = ((droneESC2.motor.watts <= 0f) ? 0f : (num9 / droneESC2.motor.watts));
					droneESC2.motor.watts = num9;
					if ((bool)base.drone.physics && (bool)base.drone.fc && (base.drone.physics.linearThrust || base.drone.physics.linearTorque))
					{
						_ = base.drone.fc.batterySag;
					}
				}
				if (drainBatteries)
				{
					DischargeBatteries(droneESC2.amperes, p_dt);
				}
			}
			List<float> inputs_back = inputs;
			inputs = m_inputs_back;
			m_inputs_back = inputs_back;
			loopExceptionCount = 0;
		}

		public SignalVector TransformSignal(SignalVector p_signal, DroneProfileData p_profile)
		{
			SignalVector result = p_signal;
			FCProfileData fCProfileData = profile;
			result.yaw = BetaflightRates.GetRate(result.yaw, fCProfileData.superRate.yaw, fCProfileData.rcRate.yaw, fCProfileData.expo.yaw);
			result.pitch = BetaflightRates.GetRate(result.pitch, fCProfileData.superRate.pitch, fCProfileData.rcRate.pitch, fCProfileData.expo.pitch);
			result.roll = BetaflightRates.GetRate(result.roll, fCProfileData.superRate.roll, fCProfileData.rcRate.roll, fCProfileData.expo.roll);
			result.throttle = BetaflightRates.GetThrottle(result.throttle, fCProfileData.expo.throttle, fCProfileData.superRate.throttle);
			result.altitude = BetaflightRates.GetAltitude(result.altitude, fCProfileData.expo.throttle);
			float throttleCap = RCI.throttleCap;
			if (throttleCap > 0f)
			{
				float throttle = BetaflightRates.GetThrottle(throttleCap, fCProfileData.expo.throttle, fCProfileData.superRate.throttle);
				result.throttle = 1f / throttle * throttleCap * result.throttle;
			}
			return result;
		}

		public SignalVector InverseTransformSignal(SignalVector p_signal)
		{
			SignalVector result = p_signal;
			FCProfileData fCProfileData = profile;
			result.yaw = BetaflightRates.ReverseRate(result.yaw, fCProfileData.superRate.yaw, fCProfileData.rcRate.yaw, fCProfileData.expo.yaw);
			result.pitch = BetaflightRates.ReverseRate(result.pitch, fCProfileData.superRate.pitch, fCProfileData.rcRate.pitch, fCProfileData.expo.pitch);
			result.roll = BetaflightRates.ReverseRate(result.roll, fCProfileData.superRate.roll, fCProfileData.rcRate.roll, fCProfileData.expo.roll);
			result.altitude = BetaflightRates.ReverseRate(result.altitude, fCProfileData.superRate.throttle, fCProfileData.rcRate.throttle, fCProfileData.expo.throttle);
			return result;
		}

		public SignalVector NormalizeSignal(SignalVector p_signal)
		{
			SignalVector result = p_signal;
			FCProfileData fCProfileData = profile;
			float max = BetaflightRates.GetMax(fCProfileData.superRate.yaw, fCProfileData.rcRate.yaw, fCProfileData.expo.yaw);
			result.yaw = ((max <= 0f) ? 0f : (BetaflightRates.GetRate(result.yaw, fCProfileData.superRate.yaw, fCProfileData.rcRate.yaw, fCProfileData.expo.yaw) / max));
			float max2 = BetaflightRates.GetMax(fCProfileData.superRate.pitch, fCProfileData.rcRate.pitch, fCProfileData.expo.pitch);
			float max3 = BetaflightRates.GetMax(fCProfileData.superRate.roll, fCProfileData.rcRate.roll, fCProfileData.expo.roll);
			result.pitch = ((max2 <= 0f) ? 0f : (BetaflightRates.GetRate(result.pitch, fCProfileData.superRate.pitch, fCProfileData.rcRate.pitch, fCProfileData.expo.pitch) / max2));
			result.roll = ((max3 <= 0f) ? 0f : (BetaflightRates.GetRate(result.roll, fCProfileData.superRate.roll, fCProfileData.rcRate.roll, fCProfileData.expo.roll) / max3));
			float max4 = BetaflightRates.GetMax(fCProfileData.superRate.throttle, fCProfileData.rcRate.throttle, fCProfileData.expo.throttle);
			if (max4 <= 0f)
			{
				result.throttle = 0f;
				result.altitude = 0f;
			}
			else
			{
				result.throttle = result.throttle * 2f - 1f;
				result.throttle = BetaflightRates.GetRate(result.throttle, fCProfileData.superRate.throttle, fCProfileData.rcRate.throttle, fCProfileData.expo.throttle) / max4;
				result.throttle = (result.throttle + 1f) / 2f;
				result.altitude = BetaflightRates.GetRate(result.altitude, fCProfileData.superRate.throttle, fCProfileData.rcRate.throttle, fCProfileData.expo.throttle) / max4;
			}
			return result;
		}

		public bool HasPower()
		{
			if (!base.drone.physics.batteryDrain)
			{
				return true;
			}
			if (base.drone.body.frame.batteries == null)
			{
				return false;
			}
			for (int i = 0; i < base.drone.body.frame.batteries.Count; i++)
			{
				if (base.drone.body.frame.batteries[i] != null && base.drone.body.frame.batteries[i].ratio > 0.001f)
				{
					return true;
				}
			}
			return false;
		}

		protected void DischargeBatteries(float p_ampere, float p_dt)
		{
			float num = base.drone.body.frame.batteries.Count;
			float p_amperes = ((num <= 0f) ? 0f : (p_ampere / num));
			for (int i = 0; i < base.drone.body.frame.batteries.Count; i++)
			{
				base.drone.body.frame.batteries[i].Discharge(p_amperes, p_dt);
			}
		}

		public override string GetPrefix()
		{
			return "FC";
		}

		protected void UpdateProfile()
		{
			if (!m_profile_dirty || processes == null || processes.Count == 0 || profile.pid == null || base.drone == null || !base.drone.hasBody || !base.drone.body.hasFrame || base.drone.body.frame.camera == null)
			{
				return;
			}
			for (int i = 0; i < processes.Count; i++)
			{
				FCProcess fCProcess = processes[i];
				PID pID = null;
				PIDVector constants = PIDVector.zero;
				switch (fCProcess.name)
				{
				case "yaw":
					pID = fCProcess.pid;
					constants = profile.pid.yaw;
					break;
				case "pitch":
					pID = fCProcess.pid;
					constants = profile.pid.pitch;
					break;
				case "roll":
					pID = fCProcess.pid;
					constants = profile.pid.roll;
					break;
				}
				if (pID != null)
				{
					pID.constants = constants;
				}
			}
			if (profile.tilt >= 0f)
			{
				base.drone.body.frame.camera.tilt = profile.tilt;
			}
			if (profile.fov >= 0f)
			{
				base.drone.body.frame.camera.fov = profile.fov;
			}
			if (m_drone != null && m_drone.hasProfile)
			{
				m_drone.profile.SuperRates.Pitch = Mathf.RoundToInt(100f * profile.superRate.pitch);
				m_drone.profile.SuperRates.Roll = Mathf.RoundToInt(100f * profile.superRate.roll);
				m_drone.profile.SuperRates.Yaw = Mathf.RoundToInt(100f * profile.superRate.yaw);
				m_drone.profile.RcRates.Pitch = Mathf.RoundToInt(100f * profile.rcRate.pitch);
				m_drone.profile.RcRates.Roll = Mathf.RoundToInt(100f * profile.rcRate.roll);
				m_drone.profile.RcRates.Yaw = Mathf.RoundToInt(100f * profile.rcRate.yaw);
				m_drone.profile.RcExpoRates.Pitch = Mathf.RoundToInt(100f * profile.expo.pitch);
				m_drone.profile.RcExpoRates.Roll = Mathf.RoundToInt(100f * profile.expo.roll);
				m_drone.profile.RcExpoRates.Yaw = Mathf.RoundToInt(100f * profile.expo.yaw);
			}
			m_profile_dirty = false;
		}

		protected virtual void Start()
		{
			StartCoroutine(Init());
		}

		private IEnumerator Init()
		{
			while (base.drone == null)
			{
				yield return null;
			}
			while (!threadedMixer)
			{
				threadedMixer = base.drone.GetComponentInChildren<DroneThreadedMixer>();
				yield return null;
			}
			yield return new WaitForSeconds(0.5f);
			threaded = m_drone.physics.threaded;
		}

		protected virtual void Update()
		{
			UpdateProfile();
		}

		public void FixedStep(float p_deltaTime)
		{
			if (m_lastFixedStepTime == Time.fixedTime)
			{
				return;
			}
			m_lastFixedStepTime = Time.fixedTime;
			originalFixedDeltaTime = p_deltaTime;
			Loop(originalFixedDeltaTime);
			calculateFC = true;
		}

		protected virtual void FixedUpdate()
		{
			FixedStep(Time.fixedDeltaTime);
		}
	}
}

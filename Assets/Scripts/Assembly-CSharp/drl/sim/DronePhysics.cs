using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using thelab.core;

namespace drl.sim
{
	[RequireComponent(typeof(Drone))]
	public class DronePhysics : MonoBehaviour
	{
		public float currentAirSpeed;

		public float topSpeed = -1f;

		public float[] currentInput = new float[4];

		private float[] targetInput = new float[4];

		private float minTargetInput;

		private float targetInputSum;

		private float[] currentMotorThust = new float[4];

		private float totalMotorThrust;

		public bool externalOverrideEsc;

		public Transform[] thrustPoints;

		private const bool manualPhysics = false;

		private Transform m_drone_transform;

		private Transform m_rb_transform;

		[SerializeField]
		private Drone m_drone;

		private bool m_hasDrone;

		private bool m_setBeginnerOnDroneSet;

		private bool m_setBeginnerFlag;

		public bool pidTuneRunning;

		private float lastAvgSignal;

		public float d_machRatio;

		public float d_dragRatio;

		public float d_propwash;

		public float d_avgThrottle;

		public float d_throttleDelta;

		public float propwashRpmOscilation = 0.22f;

		public float propwashFrameOscilation = 0.18f;

		public float propwashFrameRollOscilation = 0.22f;

		private Transform frameNode;

		private Transform frameNodeParent;

		private Transform propwashNode;

		private Quaternion fn_rotation;

		private RaycastHit[] hits;

		private DroneMotor[] motors = new DroneMotor[4];

		private int loopExceptionCount;

		private float arcing = 0.75f;

		private float inertia = 1f;

		public Vector3 requestedRates = Vector3.zero;

		public Vector3 currentRates = Vector3.zero;

		public Vector3 lastRates = Vector3.zero;

		private Quaternion lastRotation = Quaternion.identity;

		private Vector3 m_lastDragScale;

		private float _pitchP;

		private float _rollP;

		private float _yawP;

		private Vector3 _velocity;

		private Vector3 _local;

		private float _y;

		private float _drop;

		private float _slip;

		public float maxThrust { get; private set; }

		public float maxTorque { get; private set; }

		public Transform droneTransform
		{
			get
			{
				if (!m_drone_transform)
				{
					return m_drone_transform = drone.transform;
				}
				return m_drone_transform;
			}
		}

		public Transform rbTransform
		{
			get
			{
				if (!m_rb_transform)
				{
					return m_rb_transform = drone.rigidbody.rb.transform;
				}
				return m_rb_transform;
			}
		}

		public Drone drone
		{
			get
			{
				if (m_hasDrone)
				{
					return m_drone;
				}
				if ((bool)m_drone)
				{
					m_hasDrone = true;
					return m_drone;
				}
				m_drone = GetComponent<Drone>();
				if ((bool)m_drone)
				{
					m_hasDrone = true;
					return m_drone;
				}
				return null;
			}
			set
			{
				m_drone = value;
				m_hasDrone = m_drone != null;
			}
		}

		public bool hasDrone => m_hasDrone;

		public float BalanceSignalToTorque(float signal)
		{
			if (drone == null)
			{
				return signal;
			}
			if (!drone.hasBody || !drone.body.hasFrame || drone.body.frame.escs == null || drone.body.frame.escs.Count < 1)
			{
				return signal;
			}
			if (!drone.body.frame.escs[0].hasMotor)
			{
				return signal;
			}
			DroneMotorSpec.BenchData data = drone.body.frame.escs[0].motor.spec.data;
			if (drone.physics.linearTorque)
			{
				float num = data.watts.Evaluate(data.amperes.Evaluate(1f)) * Time.fixedDeltaTime;
				return Mathf.Clamp01(signal) * num;
			}
			return data.watts.Evaluate(data.amperes.Evaluate(signal)) / data.watts.Evaluate(data.amperes.Evaluate(1f));
		}

		public float BalanceTorqueToSignal(float torque)
		{
			if (drone == null)
			{
				return torque;
			}
			if (!drone.hasBody || !drone.body.hasFrame || drone.body.frame.escs == null || drone.body.frame.escs.Count < 1)
			{
				return torque;
			}
			if (!drone.body.frame.escs[0].hasMotor)
			{
				return torque;
			}
			DroneMotorSpec.BenchData data = drone.body.frame.escs[0].motor.spec.data;
			if (drone.physics.linearTorque)
			{
				float num = data.watts.Evaluate(data.amperes.Evaluate(1f)) * Time.fixedDeltaTime;
				if (num != 0f)
				{
					return torque / num;
				}
				return 0f;
			}
			return data.torqueToSignal.Evaluate(torque);
		}

		public virtual void Initialize()
		{
			currentInput[0] = (currentInput[1] = (currentInput[2] = (currentInput[3] = 0f)));
			SetBeginner(p_flag: false);
			Transform transform = base.transform.Find("thrust");
			thrustPoints = new Transform[transform.childCount];
			for (int i = 0; i < transform.childCount; i++)
			{
				thrustPoints[i] = transform.GetChild(i);
			}
		}

		public void SetBeginner(bool p_flag)
		{
			if (this == null || base.gameObject == null)
			{
				return;
			}
			if (drone == null)
			{
				m_setBeginnerOnDroneSet = true;
				m_setBeginnerFlag = p_flag;
			}
			else if (p_flag)
			{
				Activity.Run(() => PhysOverrideBeginner(p_flag));
			}
			else
			{
				Activity.Run(() => PhysOverrides(p_flag));
			}
		}

		private bool PhysOverrideBeginner(bool p_flag)
		{
			if (this == null || base.gameObject == null)
			{
				return false;
			}
			if (drone == null)
			{
				return true;
			}
			if (drone.fc == null)
			{
				return true;
			}
			if (drone.fc.process == null)
			{
				return true;
			}
			if (drone.fc.process.altitude == null)
			{
				return true;
			}
			if (drone.fc.process.softlock == null)
			{
				return true;
			}
			if (drone.fc.process.softlock.altitude == null)
			{
				return true;
			}
			if (drone.body == null)
			{
				return true;
			}
			if (drone.body.frame == null)
			{
				return true;
			}
			if (drone.body.frame.info == null)
			{
				return true;
			}
			topSpeed = -1f;
			drone.fc.process.altitude.gravityCompensation = 1.01f;
			if (drone.body.frame.guid == "F-775")
			{
				drone.fc.process.altitude.pids[0].constants.p = 0.25f;
				drone.fc.process.altitude.pids[0].constants.i = 0f;
				drone.fc.process.altitude.pids[0].constants.d = 0.01f;
				drone.fc.process.altitude.pids[1].constants.p = 0.25f;
				drone.fc.process.altitude.pids[1].constants.i = 0f;
				drone.fc.process.altitude.pids[1].constants.d = 0.01f;
				drone.fc.process.softlock.altitude.gravityCompensation = 1.01f;
				drone.fc.process.softlock.altitude.pids[0].constants.p = 0.15f;
				drone.fc.process.softlock.altitude.pids[0].constants.i = 0f;
				drone.fc.process.softlock.altitude.pids[0].constants.d = 0f;
				drone.fc.process.softlock.altitude.pids[1].constants.p = 0.25f;
				drone.fc.process.softlock.altitude.pids[1].constants.i = 0f;
				drone.fc.process.softlock.altitude.pids[1].constants.d = 0.01f;
			}
			else
			{
				drone.fc.process.altitude.pids[0].constants.p = 0.1f;
				drone.fc.process.altitude.pids[0].constants.i = 0f;
				drone.fc.process.altitude.pids[0].constants.d = 0.0001f;
				drone.fc.process.altitude.pids[1].constants.p = 0.15f;
				drone.fc.process.altitude.pids[1].constants.i = 0f;
				drone.fc.process.altitude.pids[1].constants.d = 0.001f;
				drone.fc.process.softlock.altitude.gravityCompensation = 1.01f;
				drone.fc.process.softlock.altitude.pids[0].constants.p = 0.1f;
				drone.fc.process.softlock.altitude.pids[0].constants.i = 0f;
				drone.fc.process.softlock.altitude.pids[0].constants.d = 0f;
				drone.fc.process.softlock.altitude.pids[1].constants.p = 0.15f;
				drone.fc.process.softlock.altitude.pids[1].constants.i = 0f;
				drone.fc.process.softlock.altitude.pids[1].constants.d = 0.001f;
			}
			if (drone.GetComponentInChildren<DronePhysicsSettings>() == null)
			{
				Debug.LogError("DroneSimulationDeprecated:: DronePhysicsSettings not found!");
			}
			else
			{
				SetPhysicsSettings(p_flag);
			}
			return false;
		}

		private bool PhysOverrides(bool p_flag)
		{
			if (this == null || base.gameObject == null)
			{
				return false;
			}
			if (drone == null)
			{
				return true;
			}
			if (drone.fc == null)
			{
				return true;
			}
			if (drone.fc.process == null)
			{
				return true;
			}
			if (drone.fc.process.altitude == null)
			{
				return true;
			}
			if (drone.fc.process.softlock == null)
			{
				return true;
			}
			if (drone.fc.process.softlock.altitude == null)
			{
				return true;
			}
			if (drone.body == null)
			{
				return true;
			}
			if (drone.body.frame == null)
			{
				return true;
			}
			if (drone.body.frame.info == null)
			{
				return true;
			}
			topSpeed = -1f;
			drone.fc.process.altitude.gravityCompensation = 0.62f;
			drone.fc.process.altitude.pids[0].constants.p = 0.15f;
			drone.fc.process.altitude.pids[0].constants.i = 0f;
			drone.fc.process.altitude.pids[0].constants.d = 0f;
			drone.fc.process.altitude.pids[1].constants.p = 0.25f;
			drone.fc.process.altitude.pids[1].constants.i = 0f;
			drone.fc.process.altitude.pids[1].constants.d = 0f;
			drone.fc.process.softlock.altitude.gravityCompensation = 0.62f;
			drone.fc.process.softlock.altitude.pids[0].constants.p = 0.15f;
			drone.fc.process.softlock.altitude.pids[0].constants.i = 0f;
			drone.fc.process.softlock.altitude.pids[0].constants.d = 0f;
			drone.fc.process.softlock.altitude.pids[1].constants.p = 0.25f;
			drone.fc.process.softlock.altitude.pids[1].constants.i = 0f;
			drone.fc.process.softlock.altitude.pids[1].constants.d = 0f;
			if (drone.GetComponentInChildren<DronePhysicsSettings>() == null)
			{
				Debug.LogError("DroneSimulationDeprecated:: DronePhysicsSettings not found!");
			}
			else
			{
				SetPhysicsSettings(p_flag);
			}
			return false;
		}

		private void SetPhysicsSettings(bool dji)
		{
			drone.physics = (dji ? drone.djiphysics : drone.defaultphysics);
			drone.profile = (dji ? drone.djiprofile : drone.defaultprofile);
			if (!(drone.physics == null))
			{
				if (dji && drone.physics.overrideMaxSpeed)
				{
					drone.fc.modeProcess.dji.param_maxSpeed = drone.physics.maxSpeedOverride;
				}
				if (drone.body.frame.escs != null && drone.body.frame.escs.Count > 0 && drone.body.frame.escs[0] != null && drone.body.frame.escs[0].motor != null && drone.body.frame.escs[0].motor.prop != null)
				{
					drone.body.frame.escs[0].motor.prop.SetEfficiency(drone.physics.efficiencyMax, drone.physics.efficiencyZero);
					drone.body.frame.escs[1].motor.prop.SetEfficiency(drone.physics.efficiencyMax, drone.physics.efficiencyZero);
					drone.body.frame.escs[2].motor.prop.SetEfficiency(drone.physics.efficiencyMax, drone.physics.efficiencyZero);
					drone.body.frame.escs[3].motor.prop.SetEfficiency(drone.physics.efficiencyMax, drone.physics.efficiencyZero);
				}
				if ((bool)drone.fc)
				{
					drone.fc.minSignal = drone.profile.minSignal;
					drone.fc.profile.pid.pitch = drone.profile.pitchPID;
					drone.fc.profile.pid.roll = drone.profile.rollPID;
					drone.fc.profile.pid.yaw = drone.profile.yawPID;
				}
				if (drone.physics.mass > 0.001f)
				{
					drone.rigidbody.rb.mass = drone.physics.mass;
				}
				topSpeed = -1f;
				drone.physics.gravityScale = (dji ? 6.3765f : 0f);
			}
		}

		public void AutotunePid()
		{
			if (!pidTuneRunning)
			{
				StartCoroutine(CalculatePidCorrection());
			}
		}

		private IEnumerator CalculatePidCorrection(int count = 1)
		{
			pidTuneRunning = true;
			drone.invulnerable = 2f;
			bool thread = drone.physics.threaded;
			drone.physics.threaded = false;
			yield return null;
			drone.fc.SetProcess(FlightControllerProcess.Debug, p_flag: true);
			drone.fc.allowThrottle = false;
			drone.fc.allowPitch = false;
			drone.fc.allowRoll = false;
			drone.fc.allowYaw = false;
			drone.fc.debugThrottle = 0.3f;
			bool correctR = drone.physics.correctRates;
			drone.physics.correctRates = false;
			float effOvr = drone.physics.efficiency;
			drone.physics.efficiency = 1f;
			float oldYawCorrection = ((drone.profile.pidCorrectionY == null || drone.profile.pidCorrectionY.Length < 1) ? 1.01f : drone.profile.pidCorrectionY[0]);
			drone.profile.pidCorrectionP = new float[10] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f };
			drone.profile.pidCorrectionR = new float[10] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f };
			drone.profile.pidCorrectionY = new float[10] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f };
			float _yaw = drone.body.frame.propLimit * 10f + 10f;
			float _pitch = drone.body.frame.propLimit * 10f;
			float _roll = drone.body.frame.propLimit * 10f;
			drone.profile.yawPID.p = _yaw;
			drone.profile.pitchPID.p = _pitch;
			drone.profile.rollPID.p = _roll;
			drone.profile.yawPID.i = 0f;
			drone.profile.pitchPID.i = 0f;
			drone.profile.rollPID.i = 0f;
			drone.profile.yawPID.d = 0f;
			drone.profile.pitchPID.d = 0f;
			drone.profile.rollPID.d = 0f;
			Quaternion _rotation = droneTransform.rotation;
			Vector3 _position = drone.position;
			drone.position = new Vector3(drone.position.x, 100f, drone.position.z);
			drone.rigidbody.ResetBacktrace();
			DroneMixer.DroneManeauverControl py = drone.mixer.yaw;
			droneTransform.rotation = Quaternion.identity;
			drone.rigidbody.ClearForces();
			for (int j = 1; j < 6; j++)
			{
				drone.profile.yawPID.p = 10f * (float)j;
				drone.position = new Vector3(drone.position.x, 100f, drone.position.z);
				drone.fc.debugYaw = 1f;
				yield return new WaitForSeconds(0.1f);
				float sum = 0f;
				int k = 0;
				while (k < 3)
				{
					sum += py.current;
					yield return null;
					int num = k + 1;
					k = num;
				}
				sum /= 3f;
				drone.profile.pidCorrectionY[j] = py.target / sum;
			}
			drone.profile.yawPID.p = _yaw;
			drone.profile.pidCorrectionY[0] = drone.profile.pidCorrectionY[1] + (drone.profile.pidCorrectionY[1] - drone.profile.pidCorrectionY[2]) / 2f;
			drone.fc.debugYaw = 0f;
			DroneMixer.DroneManeauverControl pc = drone.mixer.pitch;
			droneTransform.rotation = Quaternion.identity;
			drone.rigidbody.ClearForces();
			for (int j = 1; j < 10; j++)
			{
				drone.profile.pitchPID.p = 5f * (float)j;
				drone.position = new Vector3(drone.position.x, 100f, drone.position.z);
				drone.fc.debugPitch = 1f;
				yield return new WaitForSeconds(0.1f);
				float sum = 0f;
				int k = 0;
				while (k < 3)
				{
					sum += pc.current;
					yield return null;
					int num = k + 1;
					k = num;
				}
				_ = sum / 3f;
			}
			drone.profile.pitchPID.p = _pitch;
			drone.profile.pidCorrectionP[0] = drone.profile.pidCorrectionP[1] + (drone.profile.pidCorrectionP[1] - drone.profile.pidCorrectionP[2]) / 2f;
			drone.fc.debugPitch = 0f;
			DroneMixer.DroneManeauverControl pr = drone.mixer.roll;
			droneTransform.rotation = Quaternion.identity;
			drone.rigidbody.ClearForces();
			for (int j = 1; j < 10; j++)
			{
				drone.profile.rollPID.p = 5f * (float)j;
				drone.position = new Vector3(drone.position.x, 100f, drone.position.z);
				drone.fc.debugRoll = 1f;
				yield return new WaitForSeconds(0.1f);
				float sum = 0f;
				int k = 0;
				while (k < 3)
				{
					sum += pr.current;
					yield return null;
					int num = k + 1;
					k = num;
				}
				_ = sum / 3f;
			}
			drone.profile.rollPID.p = _roll;
			drone.profile.pidCorrectionR[0] = drone.profile.pidCorrectionR[1] + (drone.profile.pidCorrectionR[1] - drone.profile.pidCorrectionR[2]) / 2f;
			drone.fc.debugRoll = 0f;
			drone.fc.debugThrottle = 0f;
			drone.fc.allowThrottle = true;
			drone.fc.allowPitch = true;
			drone.fc.allowRoll = true;
			drone.fc.allowYaw = true;
			drone.fc.SetProcess(FlightControllerProcess.Debug, p_flag: false);
			drone.physics.efficiency = effOvr;
			drone.physics.correctRates = correctR;
			drone.profile.yawPID.p = Mathf.RoundToInt(_yaw + drone.profile.pidCorrectionY[0] * 3f);
			drone.profile.pitchPID.p = Mathf.RoundToInt(_pitch + drone.profile.pidCorrectionP[0] * 3f);
			drone.profile.rollPID.p = Mathf.RoundToInt(_roll + drone.profile.pidCorrectionR[0] * 3f);
			if (drone.profile.yawPID.p > 90f)
			{
				drone.profile.yawPID.p = Mathf.RoundToInt(_yaw + drone.profile.pidCorrectionY[0]);
			}
			if (drone.profile.pitchPID.p > 40f)
			{
				drone.profile.pitchPID.p = Mathf.RoundToInt(_pitch + drone.profile.pidCorrectionP[0]);
			}
			if (drone.profile.rollPID.p > 40f)
			{
				drone.profile.rollPID.p = Mathf.RoundToInt(_roll + drone.profile.pidCorrectionR[0]);
			}
			drone.profile.yawPID.d = 0f;
			drone.profile.pitchPID.d = Mathf.RoundToInt(_pitch * 0.51f + drone.profile.pidCorrectionP[0] * 2f);
			drone.profile.rollPID.d = Mathf.RoundToInt(_roll * 1.02f + drone.profile.pidCorrectionR[0] * 2f);
			if (drone.profile.pitchPID.d > 30f)
			{
				drone.profile.pitchPID.d = Mathf.RoundToInt(_pitch * 0.51f + drone.profile.pidCorrectionP[0]);
			}
			if (drone.profile.rollPID.d > 50f)
			{
				drone.profile.rollPID.d = Mathf.RoundToInt(_roll * 1.02f + drone.profile.pidCorrectionR[0]);
			}
			drone.profile.pitchPID.p = _pitch + drone.profile.pitchPID.p % 10f - 5f;
			drone.profile.rollPID.p = _roll + drone.profile.rollPID.p % 10f - 5f;
			drone.profile.yawPID.p = _yaw + drone.profile.yawPID.p % 10f;
			drone.profile.pitchPID.i = Mathf.RoundToInt(Mathf.Abs(drone.body.centerOfMass.z) * 2000f);
			drone.profile.rollPID.i = (float)Mathf.RoundToInt(drone.profile.pitchPID.i / 2f) + drone.profile.rollPID.p % 3f;
			drone.profile.yawPID.i = 0f;
			drone.profile.pitchPID.d = _pitch + drone.profile.pitchPID.d % 7f;
			drone.profile.rollPID.d = _roll + drone.profile.rollPID.d % 7f;
			drone.profile.yawPID.d = 0f;
			drone.profile.pitchPID.d = ((drone.profile.pitchPID.d < 2f) ? 30f : drone.profile.pitchPID.d);
			drone.profile.rollPID.d = ((drone.profile.rollPID.d < 2f) ? 30f : drone.profile.rollPID.d);
			droneTransform.rotation = _rotation;
			drone.position = _position;
			drone.rigidbody.ResetBacktrace();
			drone.profile.pidCorrectionP = new float[1] { 1f };
			drone.profile.pidCorrectionR = new float[1] { 1f };
			if (oldYawCorrection > 1.05f)
			{
				oldYawCorrection = drone.rig.frame switch
				{
					"F-1d2" => 1.012f, 
					"F-c2d" => 1.018f, 
					_ => 1.01f, 
				};
			}
			drone.profile.pidCorrectionY = new float[1] { oldYawCorrection };
			drone.profile.CheckAutotune();
			drone.profile.SavePID();
			drone.physics.threaded = thread;
			if (count > 0)
			{
				StartCoroutine(CalculatePidCorrection(count - 1));
			}
			else
			{
				pidTuneRunning = false;
			}
		}

		public virtual void OnUpdate()
		{
		}

		public void ClearForces()
		{
			lastAvgSignal = 0f;
			requestedRates = Vector3.zero;
			currentRates = Vector3.zero;
			lastRates = Vector3.zero;
		}

		private void PropwashSetup()
		{
			if (frameNode == null)
			{
				frameNode = drone.body.frame.transform.Find("render");
				fn_rotation = frameNode.localRotation;
			}
		}

		private void PropwashCalculate()
		{
			d_propwash = 0f;
			if (drone.propwash && !(drone.propwashStrength <= 0f))
			{
				Vector3 velocity = drone.fc.sensor.inertial.velocity;
				Vector3 up = droneTransform.up;
				float num = Vector3.Angle(velocity, -up);
				float propwashThreshold = drone.propwashThreshold;
				float num2 = propwashThreshold * 0.5f;
				float num3 = ((propwashThreshold > 1f) ? ((num - num2) / num2) : num);
				float throttle = drone.fc.rawSignal.throttle;
				d_propwash = Mathf.Clamp01(1f - num3);
				d_throttleDelta = (throttle - d_avgThrottle) * 5f;
				d_propwash *= throttle * (drone.fc.sensor.inertial.velocityY.magnitude / drone.d_topSpeed * 2f) * drone.propwashStrength * Mathf.Clamp01(d_throttleDelta);
				d_avgThrottle = Mathf.Lerp(d_avgThrottle, throttle, 0.2f);
			}
		}

		private void PropwashApply()
		{
			bool flag = d_propwash > 0f;
			if ((bool)frameNode)
			{
				Quaternion localRotation = fn_rotation;
				if (flag)
				{
					localRotation *= Quaternion.Euler(UnityEngine.Random.Range((0f - d_propwash) * propwashFrameOscilation, d_propwash * propwashFrameOscilation), UnityEngine.Random.Range((0f - d_propwash) * propwashFrameOscilation, d_propwash * propwashFrameOscilation), UnityEngine.Random.Range((0f - d_propwash) * propwashFrameRollOscilation, d_propwash * propwashFrameRollOscilation));
				}
				frameNode.localRotation = localRotation;
			}
			if (!flag || !drone.hasBody || !drone.body.hasFrame || drone.body.frame.escs == null)
			{
				return;
			}
			float num = d_propwash * propwashRpmOscilation;
			for (int i = 0; i < drone.body.frame.escs.Count; i++)
			{
				DroneESC droneESC = drone.body.frame.escs[i];
				if ((bool)droneESC && droneESC.hasMotor)
				{
					float num2 = UnityEngine.Random.Range(0f - num, num);
					droneESC.motor.rpmAudio = droneESC.motor.rpm * (1f + num2);
					drone.d_rpm[i] = droneESC.motor.rpmAudio;
					drone.d_ratio[i] = droneESC.motor.rpmAudioRatio;
				}
			}
		}

		public void OnFixedUpdate()
		{
			if (this == null)
			{
				return;
			}
			if (m_setBeginnerOnDroneSet)
			{
				m_setBeginnerOnDroneSet = false;
				Activity.RunOnce(delegate
				{
					SetBeginner(m_setBeginnerFlag);
				}, 0.1f);
			}
			if (!drone.hasPhysics || !drone.hasRigidbody || !drone.rigidbody.hasRb)
			{
				return;
			}
			Rigidbody rb = drone.rigidbody.rb;
			if (++loopExceptionCount > 10)
			{
				Debug.LogError("DroneSimulationDeprecated> too many exceptions, disabling.");
				loopExceptionCount = 0;
				drone.fc.armed = false;
				base.enabled = false;
			}
			else
			{
				if (drone.hasNaN)
				{
					return;
				}
				DronePhysicsData physics = drone.physics;
				if (physics.realisticTorque)
				{
					rb.angularDrag = 0.02f;
				}
				else
				{
					rb.angularDrag = 10f;
				}
				if (physics.airDensity <= 0f)
				{
					physics.airDensity = 1.225f;
				}
				drone.d_globalForce = Vector3.zero;
				drone.d_localForce = Vector3.zero;
				drone.d_dragForce = Vector3.zero;
				drone.d_dynDragForce = Vector3.zero;
				drone.d_windLoad = Vector3.zero;
				drone.d_drag = 0f;
				drone.d_lift = 0f;
				float num = 1f;
				if (topSpeed < 0f || drone.d_topSpeed < 0f)
				{
					topSpeed = EstimatedTopSpeed();
				}
				drone.d_topSpeed = topSpeed;
				for (int num2 = 0; num2 < 4; num2++)
				{
					motors[num2] = drone.body.frame.escs[num2].motor;
					drone.d_rpm[num2] = 0f;
					drone.d_ratio[num2] = 0f;
					motors[num2].rpmAudio = motors[num2].rpm;
				}
				if (drone.hasFc && drone.fc.armed && !drone.fc.turtle)
				{
					float num3 = 1f;
					if (!float.IsNaN(physics.groundeffectDistance) && physics.groundEffectStrength > 0f && physics.groundeffectDistance > 0.01f)
					{
						float num4 = physics.groundeffectDistance;
						if (hits == null || hits.Length < 20)
						{
							hits = new RaycastHit[20];
						}
						int num5 = Physics.RaycastNonAlloc(drone.position, DRLPhysics.Direction.down, hits, physics.groundeffectDistance, DRLPhysics.Layers.Raycast_GroundEffect, QueryTriggerInteraction.Ignore);
						for (int num6 = 0; num6 < hits.Length && num6 < num5; num6++)
						{
							RaycastHit raycastHit = hits[num6];
							if (raycastHit.transform.gameObject.layer == LayerMask.NameToLayer("DroneAsset"))
							{
								num4 = 1000f;
								break;
							}
							if (!raycastHit.transform.IsChildOf(rbTransform))
							{
								num4 = raycastHit.distance;
								break;
							}
						}
						num3 = Mathf.Lerp(1f + physics.groundEffectStrength, 1f, num4 / physics.groundeffectDistance * (num4 / physics.groundeffectDistance));
					}
					Vector3 up = rbTransform.up;
					float num7 = 0f;
					float num8 = drone.fc.sensor.inertial.velocityY.magnitude * Mathf.Sign(Vector3.Dot(drone.fc.sensor.inertial.velocity, droneTransform.up));
					drone.d_trueAirspeed = num8;
					drone.d_currentThrust = 0f;
					drone.d_propEfficiency = 0f;
					drone.d_advanceRatio = 0f;
					drone.d_dynamicDragWeight = 0f;
					drone.d_temperature = 0f;
					List<DroneESC> escs = drone.body.frame.escs;
					drone.rigidbody.CheckMotorCount(escs.Count);
					if (!physics.arcadePhysics)
					{
						arcing = ((physics.arcing > 0f) ? physics.arcing : DronePhysicsData.DefaultArcing(drone.body.frame.guid));
						inertia = ((physics.inertia > 0f) ? physics.inertia : DronePhysicsData.DefaultInertia(drone.body.frame.guid));
						float num9 = 0f;
						float num10 = 0f;
						float num11 = 0f;
						float num12 = 0f;
						float num13 = 0f;
						float num14 = 0f;
						float num15 = 0f;
						float num16 = drone.body.frame.batteries.Count;
						float num17 = ((num16 <= 0f) ? 0f : (1f / num16));
						if (physics.batteryCapacity > 0f)
						{
							drone.body.frame.batteries[0].capacity = physics.batteryCapacity;
						}
						if (physics.batteryResistance > 0f)
						{
							drone.body.frame.batteries[0].cellResistance = physics.batteryResistance;
						}
						for (int num18 = 0; num18 < drone.body.frame.batteries.Count; num18++)
						{
							DroneBattery droneBattery = drone.body.frame.batteries[num18];
							num10 += (physics.batteryDrain ? droneBattery.voltage : droneBattery.max) * num17;
							num11 += droneBattery.max * num17;
							num12 += droneBattery.min * num17;
							num13 += droneBattery.resistance;
							num14 += (physics.batteryDrain ? droneBattery.mah : droneBattery.capacity);
							num15 += droneBattery.capacity;
						}
						if (!physics.batterySag)
						{
							num13 = 0.0001f;
						}
						num = ((num15 > 0f) ? (num14 / num15) : 0f);
						float num19 = ((num11 > 0f) ? (num10 / num11) : 0f) * ((num > 0.1f) ? 1f : Mathf.Clamp01(1f - Mathf.Pow(0.2f, 100f * num)));
						float num20 = 0f;
						float num21 = 0f;
						float num22 = 0f;
						float num23 = 0f;
						bool flag = false;
						num10 = ((num16 <= 0f) ? 16.8f : num10);
						drone.fc.sensor.electrical.m_voltageMax = num11;
						drone.fc.sensor.electrical.m_voltageMin = num12;
						drone.fc.sensor.electrical.m_voltageAvailable = num10;
						drone.fc.sensor.electrical.m_remainingCharge = num14;
						drone.fc.sensor.electrical.m_totalCapacity = num15;
						for (int num24 = 0; num24 < escs.Count; num24++)
						{
							drone.rigidbody.currentThrust[num24] = 0f;
						}
						float num25 = 0f;
						if (!drone.fc.HasPower())
						{
							for (int num26 = 0; num26 < escs.Count; num26++)
							{
								escs[num26].legacyInput = 0f;
							}
						}
						else
						{
							for (int num27 = 0; num27 < escs.Count; num27++)
							{
								if (physics.batterySag)
								{
									escs[num27].legacyInput *= num19;
								}
								num25 += escs[num27].legacyInput;
							}
						}
						num25 /= (float)escs.Count;
						float num28 = num10;
						if (physics.batterySag)
						{
							num10 = Mathf.Clamp(num10 + 0.03f * num10 * (lastAvgSignal - num25), 0f, num28);
							lastAvgSignal = Mathf.Lerp(lastAvgSignal, num25, Time.fixedDeltaTime * 20f);
						}
						PropwashCalculate();
						for (int num29 = 0; num29 < escs.Count; num29++)
						{
							_ = (num29 + 1) % 4;
							DroneESC droneESC = escs[num29];
							droneESC.motor.esc = droneESC;
							droneESC.input = escs[num29].legacyInput;
							droneESC.amperes = (droneESC.hasMotor ? droneESC.motor.spec.data : null)?.amperes.Evaluate(droneESC.input) ?? 0f;
							droneESC.motor.amperes = droneESC.amperes;
							num9 += droneESC.amperes;
						}
						float num30 = num9 * num13 * 0.001f;
						num10 -= num30 * 0.25f;
						drone.fc.sensor.electrical.m_voltage = num10;
						float num31 = ((num28 > 0f) ? Mathf.Clamp01(num10 / num28) : 0f);
						if (physics.batterySag)
						{
							for (int num32 = 0; num32 < escs.Count; num32++)
							{
								escs[num32].legacyInput *= num31;
							}
							num25 *= num31;
						}
						float num33 = 0f;
						for (int num34 = 0; num34 < escs.Count; num34++)
						{
							num33 += motors[num34].rpm;
						}
						num33 /= (float)escs.Count;
						d_dragRatio = 0f;
						d_machRatio = 0f;
						if (physics.advancedPropLimits)
						{
							float num35 = (float)Math.PI * (motors[0].prop.diameter * 0.0254f) * (num33 / 60f);
							if (physics.maxTipSpeed > 0f)
							{
								float speed = drone.fc.sensor.inertial.speed;
								_ = Mathf.Sqrt(num35 * num35 + speed * speed) / 343f;
								float max = (d_machRatio = Mathf.Clamp01((343f * (physics.maxTipSpeed + 0.1f) - drone.fc.sensor.inertial.speed) / ((float)Math.PI * (motors[0].prop.diameter * 0.0254f)) * 60f / motors[0].spec.data.GetMaxRPM()));
								for (int num36 = 0; num36 < escs.Count; num36++)
								{
									escs[num36].legacyInput = Mathf.Clamp(escs[num36].legacyInput, 0f, max);
								}
							}
							if (physics.propDragFactor > 0f)
							{
								float num37 = motors[0].spec.data.GetMaxTorque() / (motors[0].prop.diameter * 0.0127f);
								float num38 = Mathf.Sqrt(num35 * num35 + drone.fc.sensor.inertial.speed * drone.fc.sensor.inertial.speed);
								if (0.5f * physics.airDensity * (num38 * num38) * physics.propDragFactor * (motors[0].prop.diameter * 0.0254f * 0.005f) > 0f)
								{
									float max2 = (d_dragRatio = Mathf.Clamp01((Mathf.Sqrt(num37 * 2f / (physics.airDensity * physics.propDragFactor * (motors[0].prop.diameter * 0.0254f * 0.005f))) - drone.fc.sensor.inertial.speed) / ((float)Math.PI * (motors[0].prop.diameter * 0.0254f)) * 60f / motors[0].spec.data.GetMaxRPM()));
									for (int num39 = 0; num39 < escs.Count; num39++)
									{
										escs[num39].legacyInput = Mathf.Clamp(escs[num39].legacyInput, 0f, max2);
									}
								}
							}
						}
						PropwashSetup();
						for (int num40 = 0; num40 < escs.Count; num40++)
						{
							_ = (num40 + 1) % 4;
							DroneESC droneESC2 = escs[num40];
							if (!droneESC2.hasMotor)
							{
								continue;
							}
							DroneMotor motor = droneESC2.motor;
							if (!motor.hasProp)
							{
								continue;
							}
							DroneProp prop = motor.prop;
							droneESC2.input = escs[num40].legacyInput;
							targetInput[num40] = droneESC2.input;
							if (num40 == 0)
							{
								minTargetInput = droneESC2.input;
								targetInputSum = droneESC2.input;
							}
							else
							{
								targetInputSum += droneESC2.input;
								if (minTargetInput > droneESC2.input)
								{
									minTargetInput = droneESC2.input;
								}
							}
							DroneMotorSpec.BenchData data = motor.spec.data;
							droneESC2.amperes = data?.amperes.Evaluate(droneESC2.input) ?? 0f;
							motor.amperes = droneESC2.amperes;
							droneESC2.temperature = Mathf.Clamp(droneESC2.temperature + (droneESC2.amperes - droneESC2.maxAmpere) * Time.fixedDeltaTime * 0.02f * drone.profile.overheatFactor, 0f, 2f);
							motor.temperature = Mathf.Clamp(motor.temperature + (motor.amperes - motor.maxAmpere) * Time.fixedDeltaTime * 0.02f * drone.profile.overheatFactor, 0f, 2f);
							if (droneESC2.temperature > 1f)
							{
								drone.fc.armed = false;
							}
							if (motor.temperature > 1f)
							{
								drone.fc.armed = false;
							}
							drone.d_temperature = Mathf.Max(drone.d_temperature, Mathf.Max(droneESC2.temperature, motor.temperature));
							droneESC2.voltage = num10;
							motor.voltage = droneESC2.voltage;
							float a = num14 * 1E-06f * motor.voltage;
							motor.watts = data?.watts.Evaluate(droneESC2.amperes) ?? 0f;
							float num41 = Time.fixedDeltaTime * 0.000277777f;
							float b = motor.watts * num41;
							b = Mathf.Min(a, b);
							float num42 = ((num41 <= 0f) ? 0f : (b / num41));
							float num43 = 1f;
							if (physics.batterySag)
							{
								num43 = ((motor.watts <= 0f) ? 0f : Mathf.Clamp01(num42 / motor.watts));
							}
							motor.watts = num42;
							motor.overrideRpm = physics.overrideSpinup;
							motor.Step(Time.fixedDeltaTime);
							if (!externalOverrideEsc)
							{
								if (physics.overrideSpinup)
								{
									float maxDelta = ((physics.spindownTime > 0f) ? (Time.fixedDeltaTime / physics.spindownTime * 40f) : 1f);
									if (droneESC2.input > currentInput[num40])
									{
										maxDelta = ((physics.spinupTime > 0f) ? (Time.fixedDeltaTime / physics.spinupTime * 2f) : 1f);
									}
									currentInput[num40] = Mathf.MoveTowards(currentInput[num40], droneESC2.input, maxDelta);
								}
								else
								{
									float maxDelta2 = ((motors[0].spec.data.spindownDelay > 0f) ? (Time.fixedDeltaTime / motors[0].spec.data.spindownDelay * 40f) : 1f);
									if (droneESC2.input > currentInput[num40])
									{
										maxDelta2 = ((motors[0].spec.data.spinupDelay > 0f) ? (Time.fixedDeltaTime / motors[0].spec.data.spinupDelay * 2f) : 1f);
									}
									currentInput[num40] = Mathf.MoveTowards(currentInput[num40], droneESC2.input, maxDelta2);
								}
							}
							motor.rpm = data.rpm.Evaluate(data.watts.Evaluate(data.amperes.Evaluate(currentInput[num40]))) * num43;
							drone.d_rpm[num40] = motor.rpm;
							drone.d_ratio[num40] = motor.rpmRatio;
							float num44 = motor.thrustNewton;
							maxThrust = data.thrust.Evaluate(data.rpm.Evaluate(data.watts.Evaluate(data.amperes.Evaluate(1f))));
							if (physics.linearThrust)
							{
								num44 = Mathf.Clamp01(currentInput[num40]) * maxThrust * 0.001f * 9.80665f;
								if (drone.fc.batterySag)
								{
									num44 *= num43;
								}
								if (physics.thrust > 0f)
								{
									num44 *= physics.thrust / maxThrust;
								}
								else if (data.thrustScale > 0f)
								{
									num44 *= data.thrustScale / maxThrust;
								}
							}
							float num45 = Mathf.Abs(motor.torque);
							maxTorque = data.watts.Evaluate(data.amperes.Evaluate(1f)) * Time.fixedDeltaTime;
							if (physics.realisticTorque)
							{
								num45 = data.torque.Evaluate(motor.watts);
								maxTorque = data.torque.Evaluate(data.watts.Evaluate(data.amperes.Evaluate(1f)));
							}
							if (physics.linearTorque)
							{
								num45 = Mathf.Clamp01(currentInput[num40]) * maxTorque;
								if (drone.fc.batterySag)
								{
									num45 *= num43;
								}
							}
							if (physics.torque > 0f)
							{
								num45 *= physics.torque / maxTorque;
							}
							if (escs[num40].motor.ccw)
							{
								num45 = 0f - num45;
							}
							float num46;
							if ((num46 = physics.efficiency) <= 0f)
							{
								flag = true;
								float num47 = prop.AdvanceRatio(motor.rpm, num8);
								num46 = Mathf.Lerp(drone.d_efficiencyAtTopSpeed, prop.Boost(motor.rpm, num8), (num47 < 0.15f) ? (arcing * 2f) : (arcing * arcing));
								num22 += num46;
								drone.d_advanceRatio += Mathf.Clamp(num47, 0f, 2f);
							}
							float num48 = num44 * num46;
							drone.d_torqueBoost = physics.torqueBoostWeight * Mathf.Clamp01((10f - num8) / 80f) * Mathf.Clamp01(physics.torque / 35f) * drone.fc.rawSignal.throttle;
							if (physics.torqueBoost)
							{
								num20 += num48 * drone.d_torqueBoost * Mathf.Clamp01(1f - physics.torqueBoostBalance);
							}
							if (physics.groundEffectStrength > 0f)
							{
								num21 += num48 * (num3 - 1f);
							}
							currentMotorThust[num40] = (flag ? num44 : num48);
							currentMotorThust[num40] *= 1f - drone.damageReduction;
							if (num40 == 0)
							{
								totalMotorThrust = currentMotorThust[num40];
							}
							else
							{
								totalMotorThrust += currentMotorThust[num40];
							}
							num23 += num44;
							motor.rpmAudio = motor.rpm;
							num7 += num45;
							drone.d_currentThrust += num48;
							drone.rigidbody.currentThrust[num40] = num48;
							drone.d_dynamicDragWeight = motor.rpmRatio;
						}
						for (int num49 = 0; num49 < 4; num49++)
						{
							if ((!drone.isThreaded || !drone.hasThreaded || !drone.threaded.wasInCollision) && physics.correctRates && num > 0.1f)
							{
								float num50 = targetInput[num49] / targetInputSum;
								ApplyForceAtPoint(rb, up * Mathf.Lerp(currentMotorThust[num49], totalMotorThrust * num50, drone.fc.rawSignal.throttle), thrustPoints[num49].position);
							}
							else
							{
								ApplyForceAtPoint(rb, up * currentMotorThust[num49], thrustPoints[num49].position);
							}
						}
						if (physics.torqueBoost)
						{
							ApplyForce(rb, up * num20);
							for (int num51 = 0; num51 < drone.rigidbody.currentThrust.Length; num51++)
							{
								drone.rigidbody.currentThrust[num51] += num20 / (float)drone.rigidbody.currentThrust.Length;
							}
						}
						if (physics.groundEffectStrength > 0f)
						{
							ApplyForce(rb, up * num21);
							for (int num52 = 0; num52 < drone.rigidbody.currentThrust.Length; num52++)
							{
								drone.rigidbody.currentThrust[num52] += num21 / (float)drone.rigidbody.currentThrust.Length;
							}
						}
						if (flag)
						{
							num22 /= 4f;
							num22 -= 1f;
							num22 *= ((num22 < 0f) ? 4f : 0.25f);
							ApplyForce(rb, up * (num23 * Mathf.Clamp(num22, -0.9f, 1.5f)));
						}
						drone.fc.sensor.electrical.m_currentDraw = num9;
						drone.fc.sensor.electrical.m_currentMax = motors[0].spec.data.GetMaxAmperes() * 4f;
						for (int num53 = 0; num53 < drone.rigidbody.currentThrust.Length; num53++)
						{
							drone.rigidbody.currentMotorThrust[num53] = drone.rigidbody.currentThrust[num53] * 1000f / 9.80665f;
						}
						PropwashApply();
					}
					else
					{
						Debug.LogError("ARCADE PHYSICS NOT IMPLEMENTED");
					}
					if (!drone.fc.HasPower())
					{
						for (int num54 = 0; num54 < escs.Count; num54++)
						{
							motors[num54].SetRPM(0f, Time.fixedDeltaTime);
						}
					}
					drone.d_currentThrust *= 101.97162f;
					drone.d_advanceRatio /= 4f;
					drone.d_propEfficiency = (physics.arcadePhysics ? 0.85f : ((physics.efficiency > 0f) ? physics.efficiency : motors[0].prop.EvaluateEfficiencyCurve(drone.d_advanceRatio)));
					drone.d_dynamicDragWeight /= 4f;
					drone.d_temperature = Mathf.Lerp(30f, 100f, drone.d_temperature);
					ApplyRelativeTorque(rb, 0f, (0f - num7) * 1.25f, 0f);
					drone.rigidbody.currentTorque = num7;
					m_lastDragScale.x = physics.dragScale;
					m_lastDragScale.y = physics.liftScale;
					m_lastDragScale.z = physics.sideScale;
					physics.dragScale *= Mathf.Lerp(Mathf.Clamp((inertia < 0.01f) ? 100f : (1f / inertia), 0.01f, 100f), 1f, drone.fc.rawSignal.throttle * 3f * Mathf.Abs(Vector3.Dot(droneTransform.up, drone.fc.sensor.inertial.velocity)));
					physics.liftScale *= Mathf.Lerp(1f, arcing * arcing, drone.fc.rawSignal.throttle * 3f);
					physics.sideScale *= Mathf.Lerp(1f, arcing * arcing, drone.fc.rawSignal.throttle * 3f) * 0.1f;
					physics.aerodynamics.Step(drone, Time.fixedDeltaTime, rb.mass, droneTransform.up, droneTransform.InverseTransformVector(drone.wind - rb.velocity), droneTransform.InverseTransformDirection(rb.angularVelocity), droneTransform.rotation);
					physics.dragScale = m_lastDragScale.x;
					physics.liftScale = m_lastDragScale.y;
					physics.sideScale = m_lastDragScale.z;
					ApplyWind(drone);
					ApplyDrag(drone);
					ApplyGravity(drone);
					ApplyVelocities(drone);
					requestedRates.x = drone.fc.signal.pitch;
					requestedRates.y = drone.fc.signal.yaw;
					requestedRates.z = 0f - drone.fc.signal.roll;
					currentRates = rbTransform.InverseTransformDirection(rb.angularVelocity) * 57.29578f;
					if (drone.isThreaded)
					{
						_pitchP = drone.profile.pitchPID.p;
						_rollP = drone.profile.rollPID.p;
						_yawP = drone.profile.yawPID.p;
					}
					else
					{
						_pitchP = drone.profile.pitchPID.p + drone.profile.pitchFF * 0.5f;
						_rollP = drone.profile.rollPID.p + drone.profile.rollFF * 0.5f;
						_yawP = drone.profile.yawPID.p + drone.profile.yawFF;
					}
					lastRates.x = Mathf.Lerp(currentRates.x, requestedRates.x, Mathf.Clamp(Mathf.Abs(drone.fc.rawSignal.pitch), _pitchP / 200f, _pitchP / 100f));
					lastRates.y = Mathf.Lerp(currentRates.y, requestedRates.y, Mathf.Clamp(Mathf.Abs(drone.fc.rawSignal.yaw), _yawP / 200f, _yawP / 100f));
					lastRates.z = Mathf.Lerp(currentRates.z, requestedRates.z, Mathf.Clamp(Mathf.Abs(drone.fc.rawSignal.roll), _rollP / 200f, _rollP / 100f));
					if ((!drone.isThreaded || !drone.hasThreaded || drone.threaded.wasInCollision) && physics.correctRates && num > 0.1f)
					{
						Vector3 vector = lastRates;
						if (physics.realisticTorque)
						{
							vector.y = currentRates.y;
						}
						if (drone.hasProfile && !drone.d_debugPID)
						{
							if (_pitchP < 30f)
							{
								vector.x = Mathf.Lerp(vector.x, currentRates.x, (30f - _pitchP) / 15f);
							}
							if (_rollP < 30f)
							{
								vector.z = Mathf.Lerp(vector.z, currentRates.z, (30f - _rollP) / 15f);
							}
						}
						rb.angularVelocity = rbTransform.TransformDirection(vector * ((float)Math.PI / 180f));
					}
				}
				else
				{
					if (drone.hasFc && drone.fc.armed && drone.fc.turtle && drone.fc.HasPower())
					{
						List<DroneESC> escs2 = drone.body.frame.escs;
						float num55 = drone.fc.rawSignal.roll * Mathf.Clamp01(1f - Mathf.Abs(drone.fc.sensor.gyro.velocity.z / 1200f));
						float num56 = drone.fc.rawSignal.pitch * Mathf.Clamp01(1f - Mathf.Abs(drone.fc.sensor.gyro.velocity.x / 1200f));
						escs2[0].input = Mathf.Clamp(num55 + num56 - 0.05f, 0f, 0.9f) * 0.5f;
						escs2[1].input = Mathf.Clamp(num55 - num56 - 0.05f, 0f, 0.9f) * 0.5f;
						escs2[2].input = Mathf.Clamp(0f - num55 - num56 - 0.05f, 0f, 0.9f) * 0.5f;
						escs2[3].input = Mathf.Clamp(0f - num55 + num56 - 0.05f, 0f, 0.9f) * 0.5f;
						for (int num57 = 0; num57 < 4; num57++)
						{
							DroneMotorSpec.BenchData benchData = (escs2[num57].hasMotor ? motors[num57].spec.data : null);
							float num58 = benchData.rpm.Evaluate(benchData.watts.Evaluate(benchData.amperes.Evaluate(escs2[num57].input)));
							float num59 = benchData.thrust.Evaluate(num58) * 0.001f * 9.80665f * ((Vector3.Dot(droneTransform.up, Vector3.up) < 0f) ? 1f : 0.1f);
							motors[num57].rpm = num58;
							motors[num57].rpmAudio = num58;
							Vector3 force = -droneTransform.up * num59;
							ApplyForceAtPoint(rb, force, thrustPoints[num57].position);
							drone.d_rpm[num57] = motors[num57].rpm;
							drone.d_ratio[num57] = motors[num57].rpmRatio;
						}
					}
					rb.angularDrag = 1f;
					drone.d_dynamicDragWeight = 0f;
					ApplyWind(drone);
					physics.aerodynamics.Step(drone, Time.fixedDeltaTime, rb.mass, droneTransform.up, droneTransform.InverseTransformVector(drone.wind - rb.velocity), droneTransform.InverseTransformDirection(rb.angularVelocity), droneTransform.rotation);
					ApplyDrag(drone);
					ApplyGravity(drone);
					ApplyVelocities(drone);
				}
				loopExceptionCount = 0;
				if (drone.hasThreaded)
				{
					drone.threaded.wasInCollision = false;
				}
			}
		}

		protected void ApplyWind(Drone p_drone)
		{
		}

		protected void ApplyDrag(Drone p_drone)
		{
			Rigidbody rb = p_drone.rigidbody.rb;
			p_drone.rigidbody.currentDragForce = drone.physics.aerodynamics.dragForce;
			p_drone.rigidbody.currentLiftForce = drone.physics.aerodynamics.liftForce;
			ApplyForce(rb, drone.physics.aerodynamics.totalForce);
			ApplyRelativeTorque(rb, drone.physics.aerodynamics.moment.x, drone.physics.aerodynamics.moment.y, drone.physics.aerodynamics.moment.z);
			p_drone.d_dragForce = drone.physics.aerodynamics.totalForce;
			p_drone.d_drag = p_drone.rigidbody.currentDragForce.magnitude;
			p_drone.d_lift = p_drone.rigidbody.currentLiftForce.magnitude;
		}

		public void ApplyForce(Rigidbody rb, Vector3 force)
		{
			drone.d_globalForce += force;
			drone.d_localForce += droneTransform.InverseTransformVector(force);
			if (float.IsNaN(force.x))
			{
				force.x = 0f;
			}
			if (float.IsNaN(force.y))
			{
				force.y = 0f;
			}
			if (float.IsNaN(force.z))
			{
				force.z = 0f;
			}
			rb.AddForce(force);
		}

		public void ApplyForceAtPoint(Rigidbody rb, Vector3 force, Vector3 point)
		{
			if (float.IsNaN(force.x))
			{
				force.x = 0f;
			}
			if (float.IsNaN(force.y))
			{
				force.y = 0f;
			}
			if (float.IsNaN(force.z))
			{
				force.z = 0f;
			}
			if (drone.isThreaded && drone.hasFc && !drone.fc.turtle && drone.hasThreaded && !drone.threaded.wasInCollision)
			{
				rb.AddForce(force);
			}
			else
			{
				rb.AddForceAtPosition(force, point);
			}
		}

		public void ApplyRelativeTorque(Rigidbody rb, float x, float y, float z)
		{
			if (float.IsNaN(x))
			{
				x = 0f;
			}
			if (float.IsNaN(y))
			{
				y = 0f;
			}
			if (float.IsNaN(z))
			{
				z = 0f;
			}
			rb.AddRelativeTorque(x, y, z);
		}

		public void ApplyGravity(Drone p_drone)
		{
			Rigidbody rb = p_drone.rigidbody.rb;
			if (drone.physics.gravity != 9.81f)
			{
				rb.velocity += Physics.gravity * (drone.physics.gravity / 9.81f - 1f) * Time.fixedDeltaTime;
			}
			if (drone.physics.gravityScale > 0f)
			{
				rb.velocity += Vector3.down * drone.physics.gravityScale * Time.fixedDeltaTime;
			}
		}

		public void ApplyVelocities(Drone p_drone)
		{
			_ = p_drone.rigidbody.rb;
		}

		public float EstimatedTopSpeed()
		{
			topSpeed = CalculateTopSpeed(drone);
			float num = 0.8333334f;
			topSpeed = Mathf.Floor(topSpeed / num) * num;
			if (topSpeed < 0f)
			{
				return -1f;
			}
			Debug.Log("DroneSimulationDeprecated> Estimated top speed:" + topSpeed + " m/s " + topSpeed * 3.6f + " km/h with " + m_drone.d_efficiencyAtTopSpeed + " efficiency");
			drone.d_topSpeed = topSpeed;
			return topSpeed;
		}

		public static float CalculateTopSpeed(Drone p_drone)
		{
			if (p_drone == null)
			{
				return -1f;
			}
			if (p_drone.physics == null)
			{
				p_drone.physics = p_drone.defaultphysics;
			}
			if (p_drone.physics == null)
			{
				return -1f;
			}
			if (p_drone.body == null || p_drone.body.frame == null || p_drone.physics == null || p_drone.fc == null || p_drone.body.frame.escs == null || p_drone.body.frame.escs.Count == 0 || p_drone.body.frame.escs[0] == null || !p_drone.body.frame.escs[0].hasMotor || !p_drone.body.frame.escs[0].motor.hasProp || !p_drone.body.frame.escs[0].motor.hasSpec || p_drone.body.frame.escs[0].motor.spec.data == null || p_drone.body.frame.batteries == null || p_drone.body.frame.batteries.Count == 0 || p_drone.body.frame.batteries[0] == null)
			{
				return -1f;
			}
			DroneMotorSpec.BenchData data = p_drone.body.frame.escs[0].motor.spec.data;
			DroneProp prop = p_drone.body.frame.escs[0].motor.prop;
			float num = data.rpm.Evaluate(data.watts.Evaluate(data.amperes.Evaluate(1f)));
			float num2 = ((p_drone.physics.thrust > 0f) ? p_drone.physics.thrust : ((data.thrustScale > 0f) ? data.thrustScale : data.thrust.Evaluate(num))) * 0.001f * 9.80665f;
			float num3 = ((p_drone.physics.gravity <= 0f) ? (0f - Physics.gravity.y) : p_drone.physics.gravity) * p_drone.rigidbody.rb.mass;
			float num4 = ((p_drone.physics.airDensity <= 0f) ? 1.225f : p_drone.physics.airDensity);
			float num5 = 0f;
			float num6 = 45f;
			float num7 = 10f;
			float num8 = 100f;
			int num9 = 0;
			bool flag = false;
			while (Mathf.Abs(num8) > 0.1f)
			{
				float num10 = Mathf.Sin((float)Math.PI / 180f * num6);
				float num11 = Mathf.Cos((float)Math.PI / 180f * num6);
				float num12 = 0f;
				float num13 = 0f;
				float num14;
				if (p_drone.physics.aerodynamics is AeroModelGATech)
				{
					AeroModelGATech aeroModelGATech = (AeroModelGATech)p_drone.physics.aerodynamics;
					if (p_drone.physics.gatechUseShedding)
					{
						for (int i = 0; i < 10; i++)
						{
							num12 += aeroModelGATech.GetCdAtAngle(p_drone, num6);
							num13 += aeroModelGATech.Cl;
						}
						num12 *= 0.1f;
						num13 *= 0.1f;
					}
					else
					{
						num12 = aeroModelGATech.GetCdAtAngle(p_drone, num6);
						num13 = aeroModelGATech.Cl;
					}
					num14 = aeroModelGATech.EffectiveSurface(p_drone);
				}
				else
				{
					float num15 = ((p_drone.physics.CdMin > 0f) ? p_drone.physics.CdMin : p_drone.body.frame.cD.x);
					float num16 = ((p_drone.physics.CdMax > 0f) ? p_drone.physics.CdMax : p_drone.body.frame.cD.y);
					float num17 = ((p_drone.physics.ClMin > 0f) ? p_drone.physics.ClMin : p_drone.body.frame.cL.x);
					float num18 = ((p_drone.physics.ClMax > 0f) ? p_drone.physics.ClMax : p_drone.body.frame.cL.y);
					num14 = ((p_drone.physics.surfaceArea > 0f) ? p_drone.physics.surfaceArea : p_drone.body.frame.surfaceArea.y);
					num12 = num15 + 2f * (num16 - num15) * num10 * num10;
					float num19 = Mathf.Sin((float)Math.PI / 90f * num6);
					num13 = num17 + (num18 - num17) * num19;
				}
				float num20 = 30f;
				float num21 = 15f;
				bool flag2 = true;
				int num22 = 0;
				float num23 = 0f;
				while (flag2)
				{
					float p_air_speed = num20 * Vector3.Dot(Vector3.up, (Mathf.Cos((float)Math.PI / 180f * num6) * Vector3.forward + Mathf.Sin((float)Math.PI / 180f * num6) * Vector3.up).normalized);
					float num24 = prop.Boost(num, p_air_speed) - 1f;
					num24 *= ((num24 < 0f) ? 4f : 0.25f);
					num24 = Mathf.Clamp(num24, -0.9f, 1.5f);
					float num25 = 0.5f * num4 * num20 * num20 * num12 * num14;
					float num26 = 4f * (num2 + num2 * num24) * num10;
					if (Mathf.Abs(num25 - num26) < 0.1f)
					{
						flag2 = false;
					}
					if (++num22 > 50)
					{
						flag2 = false;
					}
					if (!flag2)
					{
						num5 = num20;
						num23 = 4f * (num2 + num2 * num24) * num11;
						p_drone.d_efficiencyAtTopSpeed = prop.Boost(num, p_air_speed);
						p_drone.d_topSpeed = num5;
						continue;
					}
					if (num25 < num26)
					{
						if (num21 < 0f)
						{
							num21 *= -0.5f;
						}
					}
					else if (num21 > 0f)
					{
						num21 *= -0.5f;
					}
					num20 += num21;
				}
				if (flag)
				{
					num8 = 0f;
					continue;
				}
				if (++num9 > 50)
				{
					num8 = 0f;
					continue;
				}
				num8 = num23 - num3 - 0.5f * num4 * num5 * num5 * num13 * num14;
				if (num8 > 0f)
				{
					if (num7 < 0f)
					{
						num7 *= -0.5f;
					}
				}
				else if (num7 > 0f)
				{
					num7 *= -0.5f;
				}
				num6 += num7;
				if (num6 < 5f || num6 > 89f)
				{
					num6 = 45f;
					flag = true;
				}
			}
			return num5;
		}
	}
}

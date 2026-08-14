using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using thelab.core;

namespace drl.sim.thread
{
	[RequireComponent(typeof(Drone))]
	public class DroneThreaded : MonoBehaviour
	{
		[Serializable]
		public struct ForcePoints
		{
			public Transform RearRight;

			public Transform FrontRight;

			public Transform RearLeft;

			public Transform FrontLeft;
		}

		[Serializable]
		public struct ForcePointsAsVector3
		{
			public Vector3 RearRight;

			public Vector3 FrontRight;

			public Vector3 RearLeft;

			public Vector3 FrontLeft;
		}

		[Serializable]
		public struct ESCs
		{
			public DroneESC RearRight;

			public DroneESC FrontRight;

			public DroneESC RearLeft;

			public DroneESC FrontLeft;
		}

		[Serializable]
		public struct ForceValues
		{
			public Vector3 RearRight;

			public Vector3 FrontRight;

			public Vector3 RearLeft;

			public Vector3 FrontLeft;
		}

		public GyroscopeSensor Gyroscope;

		public DroneIntertial Intertial;

		public Transform ThrustPoints;

		public AutoTune AutoTunePID;

		[Space(10f)]
		[HideInInspector]
		public Drone Drone;

		[HideInInspector]
		public DroneThreadedMixer mixer;

		[HideInInspector]
		public DroneFlightController DroneFC;

		[HideInInspector]
		public PowerTrain PowerTrain;

		[HideInInspector]
		public DroneThreadedDrag DroneDragModel;

		private ESCs Frame_ESC;

		private ForcePoints Points;

		private ForcePointsAsVector3 PointsVector3;

		public Rigidbody DroneRigidbody;

		private ForceValues ForcePerPropeller;

		private bool initialized;

		public bool realThreadEnabled;

		private bool _threaded = true;

		private Thread thread;

		private float fixedDeltaTime;

		private bool allowThreadRun;

		private bool runPhysics;

		[NonSerialized]
		public bool calculateDF;

		public int NumberOfIterationsPerFixedFrame = 100;

		[HideInInspector]
		public int loopsSinceLastFixedFrame;

		[HideInInspector]
		public int dragLoopsSinceLastFixedFrame;

		[Header("Thread info...")]
		public int RunningFrequency;

		[SerializeField]
		private int _targetRefreshRate = 5000;

		[SerializeField]
		private int _targetRefreshRateDrag;

		private Stopwatch stopwatch;

		private long startTime;

		private long timeSpentInside;

		private long oneSecondPeriod;

		private long totalTimeInside;

		public float PercentageTimeSpentInAThreadPerSecond;

		[HideInInspector]
		public Vector3 VirtualPoint;

		private Vector3 PreviousFrame_VirtualPoint;

		private Vector3 threadedVelocity;

		public Quaternion VirtualRotation;

		private Vector3 RotationDelta;

		private Vector3 localAngular;

		public PID pidR;

		public PID pidP;

		public PID pidY;

		public PID pidL;

		private Vector3 handAngulVel;

		private Vector3 handVel;

		private float mss;

		private float refreshRateHZ;

		private Vector3 inertiaTensor;

		private Vector3 wrldCntrMass;

		private float yawCalc;

		private Vector3 RearRightPropellerPosition_InThread;

		private Vector3 FrontRightPropellerPosition_InThread;

		private Vector3 RearleftPropellerPosition_InThread;

		private Vector3 FrontLeftPropellerPosition_InThread;

		public Vector3 COM;

		public float angularDrag;

		private Vector3 dragReferenceVelocity;

		private Vector3 dragReferenceAngularVelocity;

		public float d_propwash;

		public float d_avgThrottle;

		public float d_throttleDelta;

		public float propwashRpmOscilation = 0.22f;

		public float propwashFrameOscilation = 0.18f;

		public float propwashFrameRollOscilation = 0.22f;

		private Transform frameNode;

		private Transform propwashNode;

		private float fixedTime;

		private float renderTime;

		private float loopTime;

		public float PhysicsFrameRate;

		public static bool calculateInFixed = false;

		public static bool calculateInUpdate = true;

		public static bool applyInFixed = false;

		public static bool applyInUpdate = true;

		public static bool interpolatePosition = true;

		private Thread m_physics_update_loop;

		private bool m_physics_update_loop_enabled;

		private bool m_physics_update_loop_kill;

		private bool m_airmode = true;

		private bool m_antigravity = true;

		private bool m_dynamicFilter = true;

		private byte m_feedForwardTransition = 100;

		private bool m_iTermRotation = true;

		private bool m_smartFeedForward;

		private byte m_iTermRelax;

		private byte m_iTermRelaxValue = 11;

		private byte m_iTermRelaxType = 1;

		private byte m_antigravityMode;

		private ushort m_antigravityGain = 1000;

		private bool m_profileInitialized;

		private float ffP;

		private float ffR;

		private float ffY;

		private SignalVector avgSignal;

		private Vector3[] calculatedForce;

		private DroneESC[] escs = new DroneESC[4];

		private Vector3 yawTorqueVector;

		private float heading;

		private const float Pscale = 0.05f;

		private const float Iscale = 0.001f;

		private const float Dscale = 0.1f;

		private const float Cscale = 0.2f;

		private const float Cspeed = 0.02f;

		private const float CYscale = 0.1f;

		private const float CYspeed = 0.1f;

		private const float COMspeed = 10000000f;

		private Vector3 control;

		private float angularDragCalculated;

		public bool inCollision;

		public bool wasInCollision;

		private float crashTimer;

		private RaycastHit[] hits;

		private float groundEffectScale;

		private float topSpeed;

		public Vector3 HandCalculatedVelocity
		{
			get
			{
				return handVel;
			}
			set
			{
				handVel = value;
			}
		}

		public Vector3 HandCalculatedAngularVelocity
		{
			get
			{
				return handAngulVel;
			}
			set
			{
				handAngulVel = value;
				localAngular = Quaternion.Inverse(VirtualRotation) * handAngulVel * 57.29578f;
			}
		}

		public Vector3 rbAngularVelocity => handAngulVel;

		public Vector3 rbVelocity => handVel;

		public Quaternion rbRotation => VirtualRotation;

		public bool Threaded => _threaded;

		public bool AllowThreadRun
		{
			get
			{
				return allowThreadRun;
			}
			set
			{
				allowThreadRun = value;
			}
		}

		public Vector3 ThreadedVelocity
		{
			get
			{
				return threadedVelocity;
			}
			set
			{
				threadedVelocity = value;
			}
		}

		public bool profilerEnabled
		{
			get
			{
				if (!Drone)
				{
					return false;
				}
				return Drone.profilerEnabled;
			}
		}

		private void StartThread()
		{
			if (Drone.isThreaded)
			{
				if (thread == null)
				{
					thread = new Thread(DFThread);
					thread.Name = "DFThread:" + ((Drone == null) ? "null" : Drone.name);
				}
				AllowThreadRun = true;
				if (!thread.IsAlive)
				{
					thread.Start();
				}
			}
		}

		private void StopThread()
		{
			if (thread == null)
			{
				return;
			}
			allowThreadRun = false;
			calculateDF = false;
			runPhysics = false;
			m_physics_update_loop_kill = true;
			Activity.RunOnce(delegate
			{
				if (thread != null && thread.IsAlive)
				{
					thread.Abort();
				}
				m_physics_update_loop_kill = false;
				thread = null;
			}, 0.05f);
		}

		private void OnDestroy()
		{
			m_physics_update_loop_enabled = false;
			m_physics_update_loop_kill = true;
			StopThread();
		}

		public void DFThread()
		{
		}

		public void Initialize()
		{
			Frame_ESC = default(ESCs);
			Points = default(ForcePoints);
			PointsVector3 = default(ForcePointsAsVector3);
			DroneRigidbody = GetComponent<Rigidbody>();
			realThreadEnabled = false;
			m_physics_update_loop_kill = false;
			if (realThreadEnabled)
			{
				AssertPhysicsUpdateThread();
			}
			StartCoroutine(FindDroneParts());
		}

		private void PropwashSetup()
		{
			if (frameNode == null)
			{
				frameNode = Drone.body.frame.transform.Find("render");
			}
			if (propwashNode == null && frameNode != null)
			{
				propwashNode = new GameObject("render").transform;
				propwashNode.parent = frameNode.parent;
				propwashNode.localPosition = Vector3.zero;
				propwashNode.localRotation = Quaternion.identity;
				propwashNode.localScale = Vector3.one;
			}
			if (frameNode != null && propwashNode != null)
			{
				frameNode.parent = propwashNode;
				propwashNode.localRotation = Quaternion.identity;
			}
		}

		private void PropwashCalculate()
		{
			if (Drone.propwash && Drone.propwashStrength > 0f)
			{
				float num = Vector3.Angle(Drone.fc.sensor.inertial.velocity, -Drone.transform.up);
				if (Drone.propwashThreshold > 1f)
				{
					d_propwash = Mathf.Clamp01(1f - (num - Drone.propwashThreshold * 0.5f) / (Drone.propwashThreshold * 0.5f));
				}
				else
				{
					d_propwash = Mathf.Clamp01(1f - num);
				}
				d_throttleDelta = (Drone.fc.rawSignal.throttle - d_avgThrottle) * 5f;
				d_propwash *= Drone.fc.rawSignal.throttle * (Drone.fc.sensor.inertial.velocityY.magnitude / Drone.d_topSpeed * 2f) * Drone.propwashStrength * Mathf.Clamp01(d_throttleDelta);
				d_avgThrottle = Mathf.Lerp(d_avgThrottle, Drone.fc.rawSignal.throttle, 0.2f);
			}
			else
			{
				d_propwash = 0f;
			}
		}

		private void PropwashApply()
		{
			if (frameNode != null && propwashNode != null)
			{
				if (d_propwash > 0f)
				{
					propwashNode.localRotation = Quaternion.Euler(UnityEngine.Random.Range((0f - d_propwash) * propwashFrameOscilation, d_propwash * propwashFrameOscilation), UnityEngine.Random.Range((0f - d_propwash) * propwashFrameOscilation, d_propwash * propwashFrameOscilation), UnityEngine.Random.Range((0f - d_propwash) * propwashFrameRollOscilation, d_propwash * propwashFrameRollOscilation));
				}
				else
				{
					propwashNode.localRotation = Quaternion.identity;
				}
				frameNode.parent = propwashNode.parent;
			}
			if (!(d_propwash > 0f) || !Drone.hasBody || !Drone.body.hasFrame || Drone.body.frame.escs == null)
			{
				return;
			}
			for (int i = 0; i < Drone.body.frame.escs.Count; i++)
			{
				DroneESC droneESC = Drone.body.frame.escs[i];
				if (droneESC != null && droneESC.hasMotor)
				{
					droneESC.motor.rpmAudio = droneESC.motor.rpm * (1f + UnityEngine.Random.Range((0f - d_propwash) * propwashRpmOscilation, d_propwash * propwashRpmOscilation)) * 1.1f;
					Drone.d_rpm[i] = droneESC.motor.rpmAudio;
					Drone.d_ratio[i] = droneESC.motor.rpmAudioRatio;
				}
			}
		}

		protected bool IsPhysicsUpdateThreadValid()
		{
			if (m_physics_update_loop == null)
			{
				return false;
			}
			System.Threading.ThreadState threadState = m_physics_update_loop.ThreadState;
			if (threadState == System.Threading.ThreadState.Running || threadState == System.Threading.ThreadState.Background || threadState == System.Threading.ThreadState.WaitSleepJoin)
			{
				return true;
			}
			return false;
		}

		protected void AssertPhysicsUpdateThread()
		{
			if (m_physics_update_loop != null)
			{
				System.Threading.ThreadState threadState = m_physics_update_loop.ThreadState;
				if (threadState == System.Threading.ThreadState.Running || threadState == System.Threading.ThreadState.Background || threadState == System.Threading.ThreadState.WaitSleepJoin)
				{
					return;
				}
			}
			m_physics_update_loop_enabled = true;
			int max_steps = 0;
			string drone_name = base.name;
			m_physics_update_loop = new Thread((ThreadStart)delegate
			{
				while (m_physics_update_loop_enabled && !m_physics_update_loop_kill)
				{
					if (!runPhysics)
					{
						max_steps = 0;
						Thread.Sleep(0);
					}
					else if (loopTime >= renderTime)
					{
						max_steps = 0;
						Thread.Sleep(0);
					}
					else
					{
						loopTime += refreshRateHZ;
						PhysicsUpdate();
						if (max_steps++ >= 200)
						{
							max_steps = 0;
							Thread.Sleep(10);
						}
						else
						{
							Thread.Sleep(0);
						}
					}
				}
				UnityEngine.Debug.LogWarning("DroneThreaded> Thread for " + drone_name + " finished!");
			});
			m_physics_update_loop.Name = "drone-physics-thread";
			m_physics_update_loop.Priority = System.Threading.ThreadPriority.Normal;
			m_physics_update_loop.Start();
			UnityEngine.Debug.LogWarning("DroneThreaded> Created New Thread for " + drone_name);
		}

		public void OnUpdate()
		{
			if (!initialized)
			{
				return;
			}
			if (!Drone || !Drone.isThreaded)
			{
				StopThread();
				return;
			}
			runPhysics = Drone.isThreaded && !DroneRigidbody.isKinematic && DroneFC.armed;
			if (!runPhysics)
			{
				fixedTime = 0f;
				renderTime = 0f;
				loopTime = 0f;
				return;
			}
			if (realThreadEnabled && !IsPhysicsUpdateThreadValid())
			{
				AssertPhysicsUpdateThread();
			}
			renderTime += Time.deltaTime;
			if (calculateInUpdate && !realThreadEnabled)
			{
				int num = 0;
				while (loopTime < renderTime && num++ < 200)
				{
					loopTime += refreshRateHZ;
					PhysicsUpdate();
				}
			}
			if (inCollision)
			{
				VirtualPoint = DroneRigidbody.position;
			}
			else if (applyInUpdate)
			{
				Drone.transform.rotation = ((!Drone.profile.iTermRotation) ? VirtualRotation : Quaternion.Slerp(DroneRigidbody.rotation, VirtualRotation, 0.9f));
				if (float.IsNaN(handAngulVel.x))
				{
					handAngulVel.x = 0f;
				}
				if (float.IsNaN(handAngulVel.y))
				{
					handAngulVel.y = 0f;
				}
				if (float.IsNaN(handAngulVel.z))
				{
					handAngulVel.z = 0f;
				}
				DroneRigidbody.angularVelocity = ((!Drone.profile.iTermRotation) ? handAngulVel : Vector3.Lerp(DroneRigidbody.angularVelocity, handAngulVel, 0.98f));
				VirtualPoint += DroneRigidbody.velocity * Time.deltaTime;
				if (interpolatePosition)
				{
					handVel = DroneRigidbody.velocity;
					Drone.transform.position = VirtualPoint;
					DroneRigidbody.velocity = handVel;
				}
			}
			handVel = DroneRigidbody.velocity;
		}

		public void OnFixedUpdate()
		{
			if (!initialized)
			{
				return;
			}
			if (!Drone || !Drone.isThreaded)
			{
				StopThread();
				return;
			}
			if (Drone.hasPhysics)
			{
				_targetRefreshRate = Drone.physics.threadTargetFrequency;
			}
			PhysicsFrameRate = 1f / Time.fixedDeltaTime;
			if ((float)NumberOfIterationsPerFixedFrame * PhysicsFrameRate < (float)_targetRefreshRate)
			{
				NumberOfIterationsPerFixedFrame++;
			}
			if ((float)NumberOfIterationsPerFixedFrame * PhysicsFrameRate > (float)_targetRefreshRate)
			{
				NumberOfIterationsPerFixedFrame /= 50;
			}
			RunningFrequency = NumberOfIterationsPerFixedFrame * (int)PhysicsFrameRate;
			angularDrag = DroneRigidbody.angularDrag;
			mss = DroneRigidbody.mass;
			fixedDeltaTime = Time.fixedDeltaTime;
			refreshRateHZ = ((!_threaded) ? fixedDeltaTime : ((RunningFrequency > 0) ? (1f / (float)RunningFrequency) : 0f));
			inertiaTensor = DroneRigidbody.inertiaTensor;
			if (AutoTunePID.TuneInProgress)
			{
				calculateDF = true;
				return;
			}
			runPhysics = Drone.isThreaded && !DroneRigidbody.isKinematic && DroneFC.armed;
			if (!runPhysics)
			{
				fixedTime = 0f;
				renderTime = 0f;
				loopTime = 0f;
				return;
			}
			GroundEffect();
			fixedTime += Time.fixedDeltaTime;
			_ = calculateInFixed;
			if (inCollision)
			{
				VirtualPoint = DroneRigidbody.position;
			}
			else if (applyInFixed)
			{
				DroneRigidbody.MoveRotation((!Drone.profile.iTermRotation) ? VirtualRotation : Quaternion.Slerp(DroneRigidbody.rotation, VirtualRotation, 0.9f));
				DroneRigidbody.angularVelocity = ((!Drone.profile.iTermRotation) ? handAngulVel : Vector3.Lerp(DroneRigidbody.angularVelocity, handAngulVel, 0.98f));
			}
			handVel = DroneRigidbody.velocity;
			inCollision = false;
			if (_threaded)
			{
				dragLoopsSinceLastFixedFrame = 0;
				loopsSinceLastFixedFrame = 0;
				calculateDF = true;
			}
			if (fixedTime > 60f || renderTime > 60f || loopTime > 60f)
			{
				fixedTime -= 60f;
				renderTime -= 60f;
				loopTime -= 60f;
			}
			COM = Vector3.Lerp(Vector3.zero, Drone.body.centerOfMass, Drone.physics.useCOG ? ((Mathf.Abs(Drone.body.centerOfMass.z) * 2000f - Drone.profile.pitchPID.i) * 0.001f) : 0f);
			CheckAndUpdateProfileParameters();
			if (!(crashTimer > 0f))
			{
				return;
			}
			crashTimer -= Time.fixedDeltaTime;
			if (crashTimer < 0f)
			{
				if (pidP != null)
				{
					pidP.Reset();
				}
				if (pidR != null)
				{
					pidR.Reset();
				}
				if (pidY != null)
				{
					pidY.Reset();
				}
			}
		}

		private void CheckAndUpdateProfileParameters()
		{
			DroneProfileData profile = Drone.profile;
			if (!m_profileInitialized)
			{
				m_airmode = FlightController.Airmode;
				m_antigravity = FlightController.Antigravity;
				m_dynamicFilter = FlightController.DynamicFilter;
				m_iTermRotation = FlightController.ItermRotation == 1;
				m_smartFeedForward = FlightController.SmartFeedforward == 1;
				m_feedForwardTransition = FlightController.FeedForwardTransition;
				m_iTermRelax = FlightController.ItermRelax;
				m_iTermRelaxValue = FlightController.ItermRelaxCutoff;
				m_iTermRelaxType = FlightController.ItermRelaxType;
				m_antigravityMode = FlightController.AntiGravityMode;
				m_antigravityGain = FlightController.ItermAcceleratorGain;
				m_profileInitialized = true;
			}
			if (profile.airmode != m_airmode)
			{
				m_profileInitialized = false;
				FlightController.Airmode = (m_airmode = profile.airmode);
			}
			if (profile.antigravity != m_antigravity)
			{
				m_profileInitialized = false;
				FlightController.Antigravity = (m_antigravity = profile.antigravity);
			}
			if (profile.dynamicFilter != m_dynamicFilter)
			{
				m_profileInitialized = false;
				FlightController.DynamicFilter = (m_dynamicFilter = profile.dynamicFilter);
			}
			if (profile.iTermRotation != m_iTermRotation)
			{
				m_profileInitialized = false;
				m_iTermRotation = profile.iTermRotation;
				FlightController.ItermRotation = (byte)(m_iTermRotation ? 1u : 0u);
			}
			if (profile.smartFeedForward != m_smartFeedForward)
			{
				m_profileInitialized = false;
				m_smartFeedForward = profile.smartFeedForward;
				FlightController.SmartFeedforward = (byte)(m_smartFeedForward ? 1u : 0u);
			}
			if (profile.feedForwardTransition != m_feedForwardTransition)
			{
				m_profileInitialized = false;
				m_feedForwardTransition = profile.feedForwardTransition;
				FlightController.FeedForwardTransition = m_feedForwardTransition;
			}
			if (profile.iTermRelax != m_iTermRelax)
			{
				m_profileInitialized = false;
				m_iTermRelax = profile.feedForwardTransition;
				FlightController.ItermRelax = m_iTermRelax;
			}
			if (profile.iTermRelaxValue != m_iTermRelaxValue)
			{
				m_profileInitialized = false;
				m_iTermRelaxValue = profile.feedForwardTransition;
				FlightController.ItermRelaxCutoff = m_iTermRelaxValue;
			}
			if (profile.iTermRelaxType != m_iTermRelaxType)
			{
				m_profileInitialized = false;
				m_iTermRelaxType = profile.feedForwardTransition;
				FlightController.ItermRelaxType = m_iTermRelaxType;
			}
			if (profile.antigravityMode != m_antigravityMode)
			{
				m_profileInitialized = false;
				m_antigravityMode = profile.feedForwardTransition;
				FlightController.AntiGravityMode = m_antigravityMode;
			}
			if (profile.antigravityGain != m_antigravityGain)
			{
				m_profileInitialized = false;
				m_antigravityGain = profile.feedForwardTransition;
				FlightController.ItermAcceleratorGain = m_antigravityGain;
			}
			if (profile.betaflightVersion != FlightController.CurrentVersionInt)
			{
				FlightController.CurrentVersionInt = profile.betaflightVersion;
			}
			if (pidP == null)
			{
				pidP = new PID();
			}
			if (pidR == null)
			{
				pidR = new PID();
			}
			if (pidY == null)
			{
				pidY = new PID();
			}
			float num = ((profile.antigravity && Drone.fc.rawSignal.throttle > avgSignal.throttle + 0.2f) ? ((float)(int)profile.antigravityGain * 0.01f) : 1f);
			ffP = ((Mathf.Abs(Drone.fc.rawSignal.pitch - avgSignal.pitch) > 0.1f) ? profile.pitchFF : 0f);
			ffR = ((Mathf.Abs(Drone.fc.rawSignal.roll - avgSignal.roll) > 0.1f) ? profile.rollFF : 0f);
			ffY = profile.yawFF * 2f;
			pidP.constants.p = 0.00160145f * (profile.pitchPID.p + ffP) * (profile.airmode ? 1f : Mathf.Lerp(0.01f, 1f, Drone.fc.rawSignal.throttle * 3f));
			pidP.constants.i = 0.000244381f * profile.pitchPID.i * num;
			pidP.constants.d = 5.2900003E-05f * (profile.pitchPID.d + ffP);
			pidR.constants.p = 0.00160145f * (profile.rollPID.p + ffR) * (profile.airmode ? 1f : Mathf.Lerp(0.01f, 1f, Drone.fc.rawSignal.throttle * 3f));
			pidR.constants.i = 0.000244381f * profile.rollPID.i * num;
			pidR.constants.d = 5.2900003E-05f * (profile.rollPID.d + ffR);
			pidY.constants.p = 0.00160145f * (profile.yawPID.p + ffY);
			pidY.constants.i = 0.000244381f * profile.yawPID.i;
			pidY.constants.d = 5.2900003E-05f * profile.yawPID.d;
			avgSignal.throttle = Mathf.Lerp(avgSignal.throttle, Drone.fc.rawSignal.throttle, 0.5f);
			avgSignal.pitch = Mathf.Lerp(avgSignal.pitch, Drone.fc.rawSignal.pitch, 0.3f);
			avgSignal.roll = Mathf.Lerp(avgSignal.roll, Drone.fc.rawSignal.roll, 0.3f);
			avgSignal.yaw = Mathf.Lerp(avgSignal.yaw, Drone.fc.rawSignal.yaw, 0.1f);
		}

		private void DragUpdate(float deltaTime)
		{
			dragLoopsSinceLastFixedFrame++;
		}

		private void DragForceUpdate(float deltaTime)
		{
		}

		private void PhysicsUpdate()
		{
			if (AutoTunePID.TuneInProgress)
			{
				if (!AutoTunePID.FinishedRoll)
				{
					AutoTunePID.Refresh_Roll(refreshRateHZ);
				}
				if (!AutoTunePID.FinishedPitch)
				{
					AutoTunePID.Refresh_Pitch(refreshRateHZ);
				}
				if (!AutoTunePID.FinishedYaw)
				{
					AutoTunePID.Refresh_Yaw(refreshRateHZ);
				}
			}
			VelocityCalculations(refreshRateHZ, mss, VirtualRotation * Vector3.up, VirtualRotation, inertiaTensor);
			RotationDelta = handAngulVel * refreshRateHZ * 57.29578f;
			if (float.IsNaN(RotationDelta.x))
			{
				RotationDelta.x = 0f;
			}
			if (float.IsNaN(RotationDelta.y))
			{
				RotationDelta.y = 0f;
			}
			if (float.IsNaN(RotationDelta.z))
			{
				RotationDelta.z = 0f;
			}
			if (float.IsNaN(localAngular.x))
			{
				localAngular.x = 0f;
			}
			if (float.IsNaN(localAngular.y))
			{
				localAngular.y = 0f;
			}
			if (float.IsNaN(localAngular.z))
			{
				localAngular.z = 0f;
			}
			if (float.IsNaN(VirtualRotation.x) || float.IsNaN(VirtualRotation.y) || float.IsNaN(VirtualRotation.z) || float.IsNaN(VirtualRotation.w))
			{
				VirtualRotation = Drone.transform.rotation;
			}
			VirtualRotation = Quaternion.Euler(RotationDelta) * VirtualRotation;
		}

		private void PowerTrainMethod(float d_td, Vector3 d_up, Vector3 d_pos)
		{
			calculatedForce = PowerTrain.OnUpdate(d_td, Drone, d_up, d_pos, DroneRigidbody, escs, mixer.DroneIntertial, groundEffectScale);
			ForcePerPropeller.RearRight = calculatedForce[0];
			ForcePerPropeller.FrontRight = calculatedForce[1];
			ForcePerPropeller.RearLeft = calculatedForce[2];
			ForcePerPropeller.FrontLeft = calculatedForce[3];
			yawCalc = calculatedForce[4].y;
		}

		private void VelocityCalculations(float d_td, float d_mass, Vector3 d_up, Quaternion d_rot, Vector3 d_inertiaTensor)
		{
			yawTorqueVector = VirtualRotation * Vector3.up * yawCalc * refreshRateHZ;
			yawTorqueVector = VirtualRotation * Div(Quaternion.Inverse(VirtualRotation) * yawTorqueVector, inertiaTensor);
			localAngular += Quaternion.Inverse(VirtualRotation) * yawTorqueVector * 57.29578f;
			if (COM.z != 0f)
			{
				localAngular.x += COM.z * (Drone.hasReceiver ? Drone.receiver.signal.throttle : 0f) * 10000000f * refreshRateHZ;
			}
			if (Drone.hasFc)
			{
				if (FlightController.flightMode == FlightMode.HORIZON)
				{
					Vector3 vector = VirtualRotation * Vector3.up;
					pidP.Update(localAngular.x, Drone.fc.signal.pitch + ((Drone.fc.signal.pitch < 10f && Drone.fc.signal.roll < 10f) ? (10f * Drone.profile.levelPID.p * ((vector.y > 0f) ? vector.x : Mathf.Sign(vector.x))) : 0f), refreshRateHZ);
					pidR.Update(localAngular.z, 0f - Drone.fc.signal.roll + ((Drone.fc.signal.pitch < 10f && Drone.fc.signal.roll < 10f) ? (10f * Drone.profile.levelPID.p * ((vector.y > 0f) ? vector.z : Mathf.Sign(vector.z))) : 0f), refreshRateHZ);
					pidY.Update(localAngular.y, Drone.fc.signal.yaw, refreshRateHZ);
				}
				else if (FlightController.flightMode == FlightMode.ANGLE)
				{
					float num = (int)FlightController.LevelAngleLimit;
					Vector3 vector2 = VirtualRotation * Quaternion.Euler(Drone.fc.rawSignal.pitch * num, 0f, (0f - Drone.fc.rawSignal.roll) * num) * Vector3.up;
					pidP.Update(localAngular.x, (Mathf.Abs(Drone.fc.rawSignal.pitch) < 0.95f) ? (10f * Drone.profile.levelPID.p * ((vector2.y > 0f) ? vector2.x : Mathf.Sign(vector2.x))) : Drone.fc.signal.pitch, refreshRateHZ);
					pidR.Update(localAngular.z, (Mathf.Abs(Drone.fc.rawSignal.roll) < 0.95f) ? (10f * Drone.profile.levelPID.p * ((vector2.y > 0f) ? vector2.z : Mathf.Sign(vector2.z))) : (0f - Drone.fc.signal.roll), refreshRateHZ);
					pidY.Update(localAngular.y, Drone.fc.signal.yaw, refreshRateHZ);
				}
				else
				{
					pidP.Update(localAngular.x, Drone.fc.signal.pitch, refreshRateHZ);
					pidR.Update(localAngular.z, 0f - Drone.fc.signal.roll, refreshRateHZ);
					pidY.Update(localAngular.y, Drone.fc.signal.yaw, refreshRateHZ);
				}
			}
			else
			{
				pidP.Update(localAngular.x, mixer.SetPoint.Pitch, refreshRateHZ);
				pidR.Update(localAngular.z, 0f - mixer.SetPoint.Roll, refreshRateHZ);
				pidY.Update(localAngular.y, mixer.SetPoint.Yaw, refreshRateHZ);
			}
			control.x = Mathf.Lerp(control.x, pidP.control, 0.02f);
			control.y = Mathf.Lerp(control.y, pidY.control, 0.1f);
			control.z = Mathf.Lerp(control.z, pidR.control, 0.02f);
			localAngular.x = Mathf.Clamp(localAngular.x + 0.2f * control.x, -2500f, 2500f);
			localAngular.y = Mathf.Clamp(localAngular.y + 0.1f * control.y, -2500f, 2500f);
			localAngular.z = Mathf.Clamp(localAngular.z + 0.2f * control.z, -2500f, 2500f);
			handAngulVel = VirtualRotation * localAngular * ((float)Math.PI / 180f);
			if (angularDrag > 0f)
			{
				angularDragCalculated = angularDrag * d_td;
				handAngulVel -= handAngulVel * angularDragCalculated;
			}
		}

		public void ResetThreadToUnityRigidbody()
		{
			VirtualPoint = DroneRigidbody.position;
			VirtualRotation = DroneRigidbody.rotation;
			handVel = DroneRigidbody.velocity;
			handAngulVel = DroneRigidbody.angularVelocity;
			localAngular = Quaternion.Inverse(VirtualRotation) * handAngulVel * 57.29578f;
			control = Vector3.zero;
			if (pidP != null)
			{
				pidP.Reset();
			}
			if (pidR != null)
			{
				pidR.Reset();
			}
			if (pidY != null)
			{
				pidY.Reset();
			}
			crashTimer = 0.5f;
		}

		public void Reset()
		{
			VirtualPoint = new Vector3(0f, 0f, 0f);
			VirtualRotation = Quaternion.Euler(0f, 0f, 0f);
			handVel = new Vector3(0f, 0f, 0f);
			handAngulVel = new Vector3(0f, 0f, 0f);
			localAngular = Vector3.zero;
			base.transform.position = VirtualPoint;
			base.transform.rotation = VirtualRotation;
			GetComponent<Rigidbody>().velocity = handVel;
			GetComponent<Rigidbody>().angularVelocity = handAngulVel;
			control = Vector3.zero;
			if (pidP != null)
			{
				pidP.Reset();
			}
			if (pidR != null)
			{
				pidR.Reset();
			}
			if (pidY != null)
			{
				pidY.Reset();
			}
		}

		public void Reset(Vector3 pos, Quaternion rot, Vector3 vel, Vector3 angularVel)
		{
			VirtualPoint = pos;
			VirtualRotation = rot;
			handVel = vel;
			handAngulVel = angularVel;
			localAngular = Quaternion.Inverse(VirtualRotation) * handAngulVel * 57.29578f;
			base.transform.position = VirtualPoint;
			base.transform.rotation = VirtualRotation;
			GetComponent<Rigidbody>().velocity = handVel;
			GetComponent<Rigidbody>().angularVelocity = handAngulVel;
			control = Vector3.zero;
		}

		public void ClearForces()
		{
			ResetThreadToUnityRigidbody();
			handAngulVel = Vector3.zero;
			handVel = Vector3.zero;
			localAngular = Vector3.zero;
			GetComponent<Rigidbody>().velocity = handVel;
			GetComponent<Rigidbody>().angularVelocity = handAngulVel;
			control = Vector3.zero;
			if (pidP != null)
			{
				pidP.Reset();
			}
			if (pidR != null)
			{
				pidR.Reset();
			}
			if (pidY != null)
			{
				pidY.Reset();
			}
		}

		private IEnumerator FindDroneParts()
		{
			yield return null;
			while (!DroneDragModel)
			{
				DroneDragModel = GetComponentInChildren<DroneThreadedDrag>();
				yield return null;
			}
			while (!mixer)
			{
				mixer = GetComponentInChildren<DroneThreadedMixer>();
				yield return null;
			}
			while (!PowerTrain)
			{
				PowerTrain = GetComponentInChildren<PowerTrain>();
				yield return null;
			}
			while (!Drone)
			{
				Drone = GetComponent<Drone>();
				yield return null;
			}
			while (!DroneFC)
			{
				DroneFC = GetComponentInChildren<DroneFlightController>();
				yield return null;
			}
			while (!Drone.hasBody || !Drone.body.hasFrame || Drone.body.frame.escs == null || Drone.body.frame.escs.Count == 0)
			{
				yield return null;
			}
			Frame_ESC.RearRight = Drone.body.frame.escs[2];
			Frame_ESC.FrontRight = Drone.body.frame.escs[1];
			Frame_ESC.RearLeft = Drone.body.frame.escs[3];
			Frame_ESC.FrontLeft = Drone.body.frame.escs[0];
			while (!Drone.physics)
			{
				yield return null;
			}
			escs = new DroneESC[4] { Frame_ESC.RearRight, Frame_ESC.FrontRight, Frame_ESC.RearLeft, Frame_ESC.FrontLeft };
			calculatedForce = new Vector3[5];
			_threaded = Drone.physics.threaded;
			yield return null;
			VirtualPoint = DroneRigidbody.transform.localPosition;
			VirtualRotation = DroneRigidbody.transform.localRotation;
			wrldCntrMass = VirtualPoint + VirtualRotation * Vector3.right * COM.x + VirtualRotation * Vector3.up * COM.y + VirtualRotation * Vector3.forward * COM.z;
			Points.RearRight = ThrustPoints.Find("RearRight").transform;
			Points.FrontRight = ThrustPoints.Find("FrontRight").transform;
			Points.RearLeft = ThrustPoints.Find("RearLeft").transform;
			Points.FrontLeft = ThrustPoints.Find("FrontLeft").transform;
			PointsVector3.RearRight = Points.RearRight.localPosition;
			PointsVector3.FrontRight = Points.FrontRight.localPosition;
			PointsVector3.RearLeft = Points.RearLeft.localPosition;
			PointsVector3.FrontLeft = Points.FrontLeft.localPosition;
			initialized = true;
		}

		private Vector3 ForceToTorque(Vector3 force, Vector3 position, Vector3 worldCenterOfMass, ForceMode forceMode, float deltaTime, Quaternion rotation, Vector3 inertiaTensor)
		{
			Vector3 torque = Vector3.Cross(position - worldCenterOfMass, force);
			ToDeltaTorque(ref torque, forceMode, deltaTime, rotation, inertiaTensor);
			return torque;
		}

		private void ToDeltaTorque(ref Vector3 torque, ForceMode forceMode, float deltaTime, Quaternion rotation, Vector3 inertiaTensor)
		{
			bool num = forceMode == ForceMode.Force || forceMode == ForceMode.Acceleration;
			bool flag = forceMode == ForceMode.Force || forceMode == ForceMode.Impulse;
			if (num)
			{
				torque *= deltaTime;
			}
			if (flag)
			{
				ApplyInertiaTensor(ref torque, rotation, inertiaTensor);
			}
		}

		private void ApplyInertiaTensor(ref Vector3 v, Quaternion rotation, Vector3 inertiaTensor)
		{
			v = rotation * Div(Quaternion.Inverse(rotation) * v, inertiaTensor);
		}

		private static Vector3 Div(Vector3 v, Vector3 v2)
		{
			return new Vector3(v.x / v2.x, v.y / v2.y, v.z / v2.z);
		}

		private void GroundEffect()
		{
		}

		public float EstimatedTopSpeed()
		{
			topSpeed = CalculateTopSpeed(Drone);
			if (topSpeed < 0f)
			{
				return -1f;
			}
			Drone.d_topSpeed = topSpeed;
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

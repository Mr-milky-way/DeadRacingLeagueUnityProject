using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using drl.sim.thread;
using thelab.core;

namespace drl.sim
{
	[RequireComponent(typeof(DroneRigidbody))]
	[RequireComponent(typeof(DroneRenderer))]
	[RequireComponent(typeof(DronePhysics))]
	public class Drone : MonoBehaviour
	{
		public struct State
		{
			public bool fcEnabled;

			public bool rigidbodyEnabled;

			public bool rigidbodyKinematic;

			public float[] motorSpeeds;

			public Vector3 velocity;

			public Vector3 angularVelocity;
		}

		public float d_trueAirspeed;

		public Vector3 d_globalForce;

		public Vector3 d_localForce;

		public Vector3 d_dragForce;

		public Vector3 d_dynDragForce;

		public Vector3 d_windLoad;

		public float d_torqueBoost;

		public float d_drag;

		public float d_lift;

		public float d_currentThrust;

		public float d_propEfficiency;

		public float d_advanceRatio;

		public float d_dynamicDragWeight;

		public float d_topSpeed;

		public float d_efficiencyAtTopSpeed;

		public float d_temperature;

		public float[] d_rpm = new float[4];

		public float[] d_ratio = new float[4];

		public bool propwash;

		public float propwashStrength = 5f;

		public float propwashThreshold = 45f;

		public float damage;

		private float m_damageReduction;

		public bool profilerEnabled;

		public State lastState;

		[SerializeField]
		private DroneRigData m_rig;

		private bool m_hasRig;

		[SerializeField]
		private DroneBody m_body;

		private bool m_hasBody;

		[SerializeField]
		private DroneFlightController m_fc;

		private bool m_hasFc;

		[SerializeField]
		private DroneReceiver m_receiver;

		private bool m_hasReceiver;

		[SerializeField]
		private DroneRigidbody m_rigidbody;

		private bool m_hasRigidbody;

		[SerializeField]
		private DroneThreaded m_threaded;

		private bool m_hasThreaded;

		[SerializeField]
		private DronePhysics m_simulation;

		private bool m_hasSimulation;

		[SerializeField]
		private DroneMixer m_mixer;

		private bool m_hasMixer;

		[SerializeField]
		private DroneRenderer m_renderer;

		private bool m_hasRenderer;

		[SerializeField]
		private DronePhysicsData m_physics;

		private bool m_hasPhysics;

		[SerializeField]
		private DroneProfileData m_profile;

		private bool m_hasProfile;

		[NonSerialized]
		public bool d_debugPID;

		[SerializeField]
		private FCProfileData m_fcProfileData;

		public Vector3 rootOffset = Vector3.zero;

		[NonSerialized]
		public bool isGhost;

		[NonSerialized]
		public bool isRemote;

		public bool m_isStatic;

		protected bool m_ready;

		public DroneEventCallback OnEvent;

		protected Transform collidersNode;

		protected static float m_crashEnergy = 200f;

		protected static float m_damageEnergy = 25f;

		protected static float m_spinout = 0.25f;

		protected static float m_crashEnergyTransferRate = 0.5f;

		protected static float m_propSturdiness = 0.1f;

		protected static float m_armSturdiness = 0.3f;

		protected static float m_bodySturdiness = 0.5f;

		protected CrashData m_crashData;

		[HideInInspector]
		public float invulnerable = 2f;

		[HideInInspector]
		public bool crashEnabled;

		private float crashForceFactor = 20f;

		private float crashDelay = 0.24f;

		protected bool m_isBroken;

		protected Activity m_spinoutActivity;

		protected float m_lastResetTime;

		protected Vector3 m_lastPLacedPosition = Vector3.zero;

		protected bool m_speedEstimatedThisFrame;

		public AutoTune m_pidAutoTuner;

		[NonSerialized]
		public Vector3 wind;

		protected bool m_hasNaN;

		protected Vector3 m_nanPosition = Vector3.zero;

		protected Vector3 m_nanVelocity = Vector3.zero;

		protected Vector3 m_nanAngular = Vector3.zero;

		protected bool m_landed;

		protected bool m_updateLanded;

		public float damageReduction
		{
			get
			{
				return m_damageReduction;
			}
			set
			{
				m_damageReduction = Mathf.Clamp(value, 0f, 1f);
			}
		}

		public DroneRigData rig
		{
			get
			{
				if (m_hasRig)
				{
					return m_rig;
				}
				if ((bool)m_rig)
				{
					m_hasRig = true;
					return m_rig;
				}
				return null;
			}
			set
			{
				m_rig = value;
				m_hasRig = m_rig != null;
			}
		}

		public bool hasRig => m_hasRig;

		public DroneBody body
		{
			get
			{
				if (m_hasBody)
				{
					return m_body;
				}
				if ((bool)m_body)
				{
					m_hasBody = true;
					return m_body;
				}
				m_body = GetComponent<DroneBody>();
				if ((bool)m_body)
				{
					m_hasBody = true;
					return m_body;
				}
				return null;
			}
			set
			{
				m_body = value;
				m_hasBody = m_body != null;
			}
		}

		public bool hasBody => m_hasBody;

		public DroneFlightController fc
		{
			get
			{
				if (m_hasFc)
				{
					return m_fc;
				}
				if ((bool)m_fc)
				{
					m_hasFc = true;
					return m_fc;
				}
				m_fc = GetComponentInChildren<DroneFlightController>();
				if ((bool)m_fc)
				{
					m_hasFc = true;
					return m_fc;
				}
				return null;
			}
			set
			{
				m_fc = value;
				m_hasFc = m_fc != null;
			}
		}

		public bool hasFc => m_hasFc;

		public DroneReceiver receiver
		{
			get
			{
				if (m_hasReceiver)
				{
					return m_receiver;
				}
				if ((bool)m_receiver)
				{
					m_hasReceiver = true;
					return m_receiver;
				}
				m_receiver = GetComponentInChildren<DroneReceiver>();
				if ((bool)m_receiver)
				{
					m_hasReceiver = true;
					return m_receiver;
				}
				return null;
			}
			set
			{
				m_receiver = value;
				m_hasReceiver = m_receiver != null;
			}
		}

		public bool hasReceiver => m_hasReceiver;

		public DroneRigidbody rigidbody
		{
			get
			{
				if (m_hasRigidbody)
				{
					return m_rigidbody;
				}
				if ((bool)m_rigidbody)
				{
					m_hasRigidbody = true;
					return m_rigidbody;
				}
				m_rigidbody = GetComponent<DroneRigidbody>();
				if ((bool)m_rigidbody)
				{
					m_hasRigidbody = true;
					return m_rigidbody;
				}
				return null;
			}
			set
			{
				m_rigidbody = value;
				m_hasRigidbody = m_rigidbody != null;
			}
		}

		public bool hasRigidbody => m_hasRigidbody;

		public DroneThreaded threaded
		{
			get
			{
				if (isGhost || isRemote)
				{
					return null;
				}
				if (m_hasThreaded)
				{
					return m_threaded;
				}
				if ((bool)m_threaded)
				{
					m_hasThreaded = true;
					return m_threaded;
				}
				m_threaded = GetComponent<DroneThreaded>();
				if ((bool)m_threaded)
				{
					m_hasThreaded = true;
					return m_threaded;
				}
				return null;
			}
			set
			{
				m_threaded = value;
				m_hasThreaded = m_threaded != null;
			}
		}

		public bool hasThreaded
		{
			get
			{
				if (!isGhost && !isRemote)
				{
					return m_hasThreaded;
				}
				return false;
			}
		}

		public DronePhysics simulation
		{
			get
			{
				if (m_hasSimulation)
				{
					return m_simulation;
				}
				if ((bool)m_simulation)
				{
					m_hasSimulation = true;
					return m_simulation;
				}
				m_simulation = GetComponent<DronePhysics>();
				if ((bool)m_simulation)
				{
					m_hasSimulation = true;
					return m_simulation;
				}
				return null;
			}
			set
			{
				m_simulation = value;
				m_hasSimulation = m_threaded != null;
			}
		}

		public bool hasSimulation => m_hasSimulation;

		public DroneMixer mixer
		{
			get
			{
				if (m_hasMixer)
				{
					return m_mixer;
				}
				if ((bool)m_mixer)
				{
					m_hasMixer = true;
					return m_mixer;
				}
				m_mixer = GetComponent<DroneMixer>();
				if ((bool)m_mixer)
				{
					m_hasMixer = true;
					return m_mixer;
				}
				return null;
			}
			set
			{
				m_mixer = value;
				m_hasMixer = m_mixer != null;
			}
		}

		public bool hasMixer => m_hasMixer;

		public DroneRenderer renderer
		{
			get
			{
				if (m_hasRenderer)
				{
					return m_renderer;
				}
				if ((bool)m_renderer)
				{
					m_hasRenderer = true;
					return m_renderer;
				}
				m_renderer = GetComponent<DroneRenderer>();
				if ((bool)m_renderer)
				{
					m_hasRenderer = true;
					return m_renderer;
				}
				return null;
			}
			set
			{
				m_renderer = value;
				m_hasRenderer = m_renderer != null;
			}
		}

		public bool hasRenderer => m_hasRenderer;

		public DronePhysicsData physics
		{
			get
			{
				return m_physics;
			}
			set
			{
				if (value == null)
				{
					m_hasPhysics = false;
					m_physics = null;
					return;
				}
				m_hasPhysics = true;
				m_physics = value.Clone();
				if (rig != null && rig.hasCustomPhysics)
				{
					m_physics = DronePhysicsData.FromJson(rig.tune);
				}
				m_physics.gatechDataAvailable = hasBody && body.hasFrame && body.frame.gatechDragData != null;
				m_physics.gatechDataAvailable |= GATechLookupStorage.HasData(m_physics.aerodynamicsData);
				if (m_physics.aerodynamicsType == DronePhysicsData.AerodynamicsModelType.GATech && m_physics.gatechDataAvailable)
				{
					if (hasBody && body.hasFrame)
					{
						m_physics.SetAerodynamics(DronePhysicsData.AerodynamicsModelType.GATech, (!string.IsNullOrEmpty(m_physics.aerodynamicsData) && GATechLookupStorage.HasData(m_physics.aerodynamicsData)) ? GATechLookupStorage.GetData(m_physics.aerodynamicsData) : body.frame.gatechDragData, body.frame.guid);
					}
					else
					{
						m_physics.SetAerodynamics(DronePhysicsData.AerodynamicsModelType.GATech, (!string.IsNullOrEmpty(m_physics.aerodynamicsData) && GATechLookupStorage.HasData(m_physics.aerodynamicsData)) ? GATechLookupStorage.GetData(m_physics.aerodynamicsData) : null);
					}
				}
			}
		}

		public bool hasPhysics => m_hasPhysics;

		public DronePhysicsData defaultphysics { get; set; }

		public DronePhysicsData djiphysics { get; set; }

		public bool IsCurrentPhysicsDefault
		{
			get
			{
				if (!m_hasPhysics)
				{
					return false;
				}
				DronePhysicsData dronePhysicsData = ((fc == null) ? defaultphysics : ((fc.mode == FlightControllerMode.Pro || fc.mode == FlightControllerMode.Acro || fc.mode == FlightControllerMode.Intermediate) ? defaultphysics : djiphysics));
				if (dronePhysicsData == null)
				{
					return false;
				}
				if (!Equal(physics.gravity, dronePhysicsData.gravity))
				{
					return false;
				}
				if (!EqualOrDefault(physics.airDensity, dronePhysicsData.airDensity, 1.225f, 0f))
				{
					return false;
				}
				if (physics.efficiency <= 0f && dronePhysicsData.efficiency <= 0f)
				{
					if (!EqualOrDefault(physics.efficiencyMax, dronePhysicsData.efficiencyMax, body.frame.escs[0].motor.prop.maxEfficiency, 0f))
					{
						return false;
					}
					if (!EqualOrDefault(physics.efficiencyZero, dronePhysicsData.efficiencyZero, body.frame.escs[0].motor.prop.zeroEfficiencyAdvanceRatio, 0f))
					{
						return false;
					}
				}
				else if (!Equal(physics.efficiency, dronePhysicsData.efficiency))
				{
					return false;
				}
				if (!Equal(physics.gravityFactor, dronePhysicsData.gravityFactor))
				{
					return false;
				}
				if (!Equal(physics.groundEffectStrength, dronePhysicsData.groundEffectStrength))
				{
					return false;
				}
				if (!Equal(physics.groundeffectDistance, dronePhysicsData.groundeffectDistance))
				{
					return false;
				}
				if (!EqualOrDefault(physics.thrust, dronePhysicsData.thrust, body.frame.escs[0].motor.spec.data.GetMaxThrust(), 0f))
				{
					return false;
				}
				if (!EqualOrDefault(physics.torque, dronePhysicsData.torque, body.frame.escs[0].motor.spec.data.GetMaxTorque(), 0f))
				{
					return false;
				}
				if (!EqualOrDefault(physics.mass, dronePhysicsData.mass, body.weight * 0.001f, 0f))
				{
					return false;
				}
				if (!Equal(physics.torqueBoost, dronePhysicsData.torqueBoost))
				{
					return false;
				}
				if (physics.torqueBoost)
				{
					if (!Equal(physics.torqueBoostWeight, dronePhysicsData.torqueBoostWeight))
					{
						return false;
					}
					if (!Equal(physics.torqueBoostBalance, dronePhysicsData.torqueBoostBalance))
					{
						return false;
					}
				}
				if (physics.overrideSpinup)
				{
					if (!EqualOrDefault(physics.spinupTime, dronePhysicsData.spinupTime, body.frame.escs[0].motor.spec.data.spinupDelay, 0.1f))
					{
						return false;
					}
					if (!EqualOrDefault(physics.spindownTime, dronePhysicsData.spindownTime, body.frame.escs[0].motor.spec.data.spindownDelay, 0.01f))
					{
						return false;
					}
				}
				if (!Equal(physics.advancedPropLimits, dronePhysicsData.advancedPropLimits))
				{
					return false;
				}
				if (physics.advancedPropLimits)
				{
					if (!Equal(physics.maxTipSpeed, dronePhysicsData.maxTipSpeed))
					{
						return false;
					}
					if (!Equal(physics.propDragFactor, dronePhysicsData.propDragFactor))
					{
						return false;
					}
				}
				if (!Equal(physics.batterySag, dronePhysicsData.batterySag))
				{
					return false;
				}
				if (!Equal(physics.batteryDrain, dronePhysicsData.batteryDrain))
				{
					return false;
				}
				if (physics.batterySag && !EqualOrDefault(physics.batteryResistance, dronePhysicsData.batteryResistance, 8f, 0f))
				{
					return false;
				}
				if (physics.batteryDrain && !EqualOrDefault(physics.batteryCapacity, dronePhysicsData.batteryCapacity, 1000f, 0f))
				{
					return false;
				}
				if (!Equal(physics.arcadePhysics, dronePhysicsData.arcadePhysics))
				{
					return false;
				}
				if (!Equal(physics.linearTorque, dronePhysicsData.linearTorque))
				{
					return false;
				}
				if (!Equal(physics.linearThrust, dronePhysicsData.linearThrust))
				{
					return false;
				}
				if (!Equal(physics.realisticTorque, dronePhysicsData.realisticTorque))
				{
					return false;
				}
				if (!Equal(physics.correctRates, dronePhysicsData.correctRates))
				{
					return false;
				}
				if (physics.correctRates && !Equal(physics.overrideAirmode, dronePhysicsData.overrideAirmode))
				{
					return false;
				}
				if (!EqualOrDefault(physics.inertia, dronePhysicsData.inertia, DronePhysicsData.DefaultInertia(body.frame.guid), 0f))
				{
					return false;
				}
				if (!EqualOrDefault(physics.arcing, dronePhysicsData.arcing, DronePhysicsData.DefaultArcing(body.frame.guid), 0f))
				{
					return false;
				}
				if (!Equal(physics.legacyDrag, dronePhysicsData.legacyDrag))
				{
					return false;
				}
				if (!EqualOrDefault(physics.surfaceArea, dronePhysicsData.surfaceArea, body.frame.surfaceArea.y, -1f))
				{
					return false;
				}
				if ((bool)body && (bool)body.frame && body.frame.gatechDragData != null && physics.aerodynamicsType != dronePhysicsData.aerodynamicsType)
				{
					return false;
				}
				if (physics.aerodynamicsType == DronePhysicsData.AerodynamicsModelType.GATech)
				{
					if (!Equal(physics.gatechUseCrossflow, dronePhysicsData.gatechUseCrossflow))
					{
						return false;
					}
					if (!Equal(physics.gatechUseUnsteady, dronePhysicsData.gatechUseUnsteady))
					{
						return false;
					}
					if (!Equal(physics.gatechUseShedding, dronePhysicsData.gatechUseShedding))
					{
						return false;
					}
					if (string.IsNullOrEmpty(physics.aerodynamicsData) != string.IsNullOrEmpty(dronePhysicsData.aerodynamicsData))
					{
						return false;
					}
					if (physics.aerodynamicsData != dronePhysicsData.aerodynamicsData)
					{
						return false;
					}
					if (!EqualOrDefault(physics.dragScale, dronePhysicsData.dragScale, body.frame.dragScaling.x, -1f))
					{
						return false;
					}
					if (!EqualOrDefault(physics.liftScale, dronePhysicsData.liftScale, body.frame.dragScaling.y, -1f))
					{
						return false;
					}
					if (!EqualOrDefault(physics.sideScale, dronePhysicsData.sideScale, body.frame.dragScaling.z, -1f))
					{
						return false;
					}
				}
				else
				{
					if (!EqualOrDefault(physics.ClMin, dronePhysicsData.ClMin, body.frame.cL.x, -1f))
					{
						return false;
					}
					if (!EqualOrDefault(physics.ClMax, dronePhysicsData.ClMax, body.frame.cL.y, -1f))
					{
						return false;
					}
					if (!EqualOrDefault(physics.CdMin, dronePhysicsData.CdMin, body.frame.cD.x, -1f))
					{
						return false;
					}
					if (!EqualOrDefault(physics.CdMax, dronePhysicsData.CdMax, body.frame.cD.y, -1f))
					{
						return false;
					}
				}
				return true;
			}
		}

		public DroneProfileData profile
		{
			get
			{
				return m_profile;
			}
			set
			{
				if (value == null)
				{
					m_hasProfile = false;
					m_profile = null;
					return;
				}
				m_hasProfile = true;
				m_profile = value.Clone();
				bool flag = fc != null && (fc.mode == FlightControllerMode.Beginner || fc.mode == FlightControllerMode.DJI || fc.mode == FlightControllerMode.Target);
				if (rig != null && rig.hasCustomProfile && !flag)
				{
					m_profile = DroneProfileData.FromJson(rig.profile);
				}
			}
		}

		public bool hasProfile => m_hasProfile;

		public DroneProfileData defaultprofile { get; set; }

		public DroneProfileData djiprofile { get; set; }

		[Obsolete]
		public FCProfileData fcProfileData
		{
			get
			{
				if (hasFc)
				{
					return fc.profile;
				}
				if (fc != null)
				{
					return fc.profile;
				}
				if (m_fcProfileData == null)
				{
					m_fcProfileData = new FCProfileData();
				}
				return m_fcProfileData;
			}
			set
			{
				if (hasFc)
				{
					fc.profile = value;
				}
				else if (fc != null)
				{
					fc.profile = value;
				}
				m_fcProfileData = value;
			}
		}

		public virtual Vector3 position
		{
			get
			{
				if (!base.gameObject)
				{
					return Vector3.zero;
				}
				return base.transform.TransformPoint(-rootOffset);
			}
			set
			{
				base.transform.position = value;
				base.transform.position = base.transform.TransformPoint(rootOffset);
				if (!isGhost && !isRemote)
				{
					if (hasRigidbody)
					{
						rigidbody.rb.position = base.transform.position;
					}
					rigidbody.ResetBacktrace();
					if (hasThreaded)
					{
						threaded.ResetThreadToUnityRigidbody();
					}
				}
			}
		}

		public Vector3 localPosition
		{
			get
			{
				return base.transform.localPosition - rootOffset;
			}
			set
			{
				base.transform.localPosition = value + rootOffset;
			}
		}

		public bool isThreaded
		{
			get
			{
				if (isGhost || isRemote || m_isStatic)
				{
					return false;
				}
				if (m_hasPhysics && m_hasFc)
				{
					if (m_physics.threaded)
					{
						if (m_fc.mode != FlightControllerMode.Pro)
						{
							return m_fc.mode == FlightControllerMode.Acro;
						}
						return true;
					}
					return false;
				}
				return false;
			}
			set
			{
				if (m_hasPhysics)
				{
					m_physics.threaded = value;
				}
			}
		}

		public bool ready
		{
			get
			{
				if (!hasBody)
				{
					return false;
				}
				if (!m_body.hasFrame)
				{
					return false;
				}
				if (!m_body.frame.hasCamera)
				{
					return false;
				}
				if (!hasFc)
				{
					return false;
				}
				if (!hasRenderer)
				{
					return false;
				}
				if (!hasRigidbody)
				{
					return false;
				}
				return m_ready;
			}
		}

		public static float CrashEnergy
		{
			get
			{
				return m_crashEnergy;
			}
			set
			{
				m_crashEnergy = value;
			}
		}

		public static float DamageEnergy
		{
			get
			{
				return m_damageEnergy;
			}
			set
			{
				m_damageEnergy = value;
			}
		}

		public static float Spinout
		{
			get
			{
				return m_spinout;
			}
			set
			{
				m_spinout = value;
			}
		}

		public static float CrashEnergyTransferRate
		{
			get
			{
				return m_crashEnergyTransferRate;
			}
			set
			{
				m_crashEnergyTransferRate = value;
			}
		}

		public static float PropSturdiness
		{
			get
			{
				return m_propSturdiness;
			}
			set
			{
				m_propSturdiness = value;
			}
		}

		public static float ArmSturdiness
		{
			get
			{
				return m_armSturdiness;
			}
			set
			{
				m_armSturdiness = value;
			}
		}

		public static float BodySturdiness
		{
			get
			{
				return m_bodySturdiness;
			}
			set
			{
				m_bodySturdiness = value;
			}
		}

		public CrashData crashData => m_crashData;

		public bool isBroken => m_isBroken;

		public float lastResetTime => m_lastResetTime;

		public bool FarFromResetPosition => (m_lastPLacedPosition - position).sqrMagnitude > 0.4f;

		public virtual bool pidTuneRunning
		{
			get
			{
				if ((bool)simulation)
				{
					return simulation.pidTuneRunning;
				}
				if ((bool)m_pidAutoTuner)
				{
					return m_pidAutoTuner.TuneInProgress;
				}
				return false;
			}
		}

		public bool hasNaN => false;

		private void LinkParameters()
		{
			if (!(this == null) && !(base.gameObject == null) && hasFc && hasProfile)
			{
				if (fc.profile.pid.pitch != profile.pitchPID)
				{
					fc.profile.pid.pitch = profile.pitchPID;
				}
				if (fc.profile.pid.roll != profile.rollPID)
				{
					fc.profile.pid.roll = profile.rollPID;
				}
				if (fc.profile.pid.yaw != profile.yawPID)
				{
					fc.profile.pid.yaw = profile.yawPID;
				}
				mixer.pitch.pid.constants = profile.pitchPID;
				mixer.roll.pid.constants = profile.rollPID;
				mixer.yaw.pid.constants = profile.yawPID;
			}
		}

		private void LinkFlightModes()
		{
			if (!hasFc && fc == null)
			{
				return;
			}
			fc.ReadSignal();
			SignalVector signalVector = fc.signal;
			if (!hasPhysics && physics == null)
			{
				return;
			}
			if (!physics.legacyDrag && (fc.mode == FlightControllerMode.DJI || fc.mode == FlightControllerMode.Beginner || fc.mode == FlightControllerMode.Target))
			{
				physics = djiphysics;
				profile = djiprofile;
			}
			if (physics.legacyDrag && fc.mode != FlightControllerMode.DJI && fc.mode != FlightControllerMode.Beginner && fc.mode != FlightControllerMode.Target)
			{
				physics = defaultphysics;
				profile = defaultprofile;
			}
			if (physics.overrideAirmode > 0f && fc.mode != FlightControllerMode.DJI && fc.mode != FlightControllerMode.Beginner && fc.mode != FlightControllerMode.Target)
			{
				signalVector.throttle = Mathf.Lerp(signalVector.throttle, Mathf.Max(signalVector.throttle, physics.overrideAirmode), (Mathf.Abs(fc.rawSignal.pitch) + Mathf.Abs(fc.rawSignal.roll)) * 0.3f);
			}
			if (fc.mode == FlightControllerMode.Training || fc.IsProcessActive(FlightControllerProcess.Training))
			{
				signalVector = fc.process.training.TransfromSignal(signalVector);
			}
			if (fc.IsProcessActive(FlightControllerProcess.Limiter))
			{
				signalVector.roll = fc.process.limiter.DampenRoll(signalVector.roll);
				signalVector.pitch = fc.process.limiter.DampenPitch(signalVector.pitch);
			}
			if ((fc.mode == FlightControllerMode.Baro || fc.IsProcessActive(FlightControllerProcess.Altitude)) && signalVector.throttle < 0.02f)
			{
				signalVector.throttle = Mathf.Clamp01(fc.process.altitude.hoverThrottle);
			}
			if ((fc.mode == FlightControllerMode.Baro || fc.mode == FlightControllerMode.Level || fc.IsProcessActive(FlightControllerProcess.Level)) && Mathf.Abs(signalVector.pitch) < fc.process.level.limit && Mathf.Abs(signalVector.roll) < fc.process.level.limit)
			{
				signalVector.pitch = fc.process.level.outputSignal.pitch;
				signalVector.roll = fc.process.level.outputSignal.roll;
				if (fc.process.level.affectYaw)
				{
					signalVector.yaw += fc.process.level.outputSignal.yaw;
				}
			}
			if (fc.mode == FlightControllerMode.DJI || fc.mode == FlightControllerMode.Beginner)
			{
				signalVector.throttle = fc.modeProcess.dji.outputSignal.throttle;
				signalVector.pitch = fc.modeProcess.dji.outputSignal.pitch;
				signalVector.roll = fc.modeProcess.dji.outputSignal.roll;
				signalVector.yaw *= fc.modeProcess.dji.dampenYaw;
				signalVector.yaw += fc.modeProcess.dji.outputSignal.yaw;
			}
			if (fc.mode == FlightControllerMode.Target)
			{
				signalVector.throttle = fc.modeProcess.dji.outputSignal.throttle;
				signalVector.pitch = fc.modeProcess.dji.outputSignal.pitch;
				signalVector.roll = fc.modeProcess.dji.outputSignal.roll;
				signalVector.yaw = fc.modeProcess.target.outputSignal.yaw * fc.modeProcess.dji.dampenYaw + fc.modeProcess.dji.outputSignal.yaw;
			}
			if (fc.IsProcessActive(FlightControllerProcess.Lock))
			{
				signalVector.throttle += fc.process.softlock.outputSignal.throttle;
				signalVector.altitude += fc.process.softlock.outputSignal.altitude;
				signalVector.pitch += fc.process.softlock.outputSignal.pitch;
				signalVector.roll += fc.process.softlock.outputSignal.roll;
				signalVector.yaw += fc.process.softlock.outputSignal.yaw;
			}
			fc.process.pitch.pid.control = mixer.pitch.pid.control;
			fc.process.yaw.pid.control = mixer.yaw.pid.control;
			fc.process.roll.pid.control = mixer.roll.pid.control;
			fc.signal = signalVector;
		}

		public void SetPropwash(int p_strength)
		{
			switch (p_strength)
			{
			case 0:
				propwash = true;
				propwashStrength = 0f;
				propwashThreshold = 0f;
				break;
			case 1:
				propwash = true;
				propwashStrength = 5f;
				propwashThreshold = 60f;
				break;
			case 2:
				propwash = true;
				propwashStrength = 10f;
				propwashThreshold = 60f;
				break;
			case 3:
				propwash = true;
				propwashStrength = 15f;
				propwashThreshold = 60f;
				break;
			default:
				propwash = true;
				propwashStrength = 0f;
				propwashThreshold = 0f;
				break;
			}
		}

		public static bool EqualOrDefault(float v1, float v2, float defaultValue, float defaultMarker, float delta = 1E-06f)
		{
			if (Equal(v2, defaultMarker) && Equal(v1, defaultValue, delta))
			{
				return true;
			}
			if (Equal(v1, defaultMarker) && Equal(v2, defaultValue, delta))
			{
				return true;
			}
			if (Equal(v1, v2, delta))
			{
				return true;
			}
			return false;
		}

		public static bool Equal(float v1, float v2, float delta = 1E-06f)
		{
			return Mathf.Abs(v1 - v2) < delta;
		}

		public static bool Equal(bool v1, bool v2)
		{
			return v1 == v2;
		}

		public void MakeStatic(bool p_flag)
		{
			m_isStatic = p_flag;
		}

		public void Dispatch(DroneEventType p_type)
		{
			if (OnEvent != null)
			{
				OnEvent.Invoke(new DroneEvent
				{
					type = p_type,
					target = this
				});
			}
		}

		public void Initialize()
		{
			IEnumerator enumerator = InitializeAsync();
			while (enumerator.MoveNext())
			{
			}
		}

		public virtual IEnumerator InitializeAsync()
		{
			yield return null;
			if (!body)
			{
				Debug.LogWarning("Drone> Missing 'body' at [" + Hierarchy.Path(base.transform) + "]");
			}
			else
			{
				if (!body.frame)
				{
					throw new NullReferenceException("no drone frame");
				}
				body.Build();
				yield return null;
				renderer.Build();
				yield return null;
				body.LinkSkins();
				yield return null;
				rigidbody.Build();
				yield return null;
				if ((bool)simulation)
				{
					simulation.Initialize();
				}
				yield return null;
				if ((bool)threaded)
				{
					threaded.Initialize();
				}
				yield return null;
			}
			if (!fc)
			{
				Debug.LogWarning("Drone> Missing 'flight-controller' at [" + Hierarchy.Path(base.transform) + "]");
			}
			else
			{
				fc.Boot();
				fc.SetLayout(FrameLayoutType.QuadX);
				yield return null;
			}
			if (!renderer)
			{
				Debug.LogWarning("Drone> Missing 'renderer' at [" + Hierarchy.Path(base.transform) + "]");
			}
			else
			{
				renderer.ClearTrails();
			}
			yield return null;
			if ((bool)body && (bool)body.frame && (bool)body.frame.crash)
			{
				body.frame.crash.Link();
			}
			yield return null;
			if ((bool)fc)
			{
				fc.SetMode(FlightControllerMode.Acro);
			}
			Validate();
			rigidbody.ResetBacktrace();
			rigidbody.rb.maxAngularVelocity = 125.663704f;
			m_ready = true;
			this.TimerRunOnce(delegate
			{
				Dispatch(DroneEventType.Ready);
			}, 1f / 12f);
		}

		public bool Validate()
		{
			int num = 1 & (((bool)rig) ? 1 : 0);
			if (num == 0)
			{
				Debug.LogError("Drone.Validate> Missing 'rig' at [" + Hierarchy.Path(base.transform) + "]");
			}
			int num2 = num & (((bool)body) ? 1 : 0);
			if (num2 == 0)
			{
				Debug.LogError("Drone.Validate> Missing 'body' at [" + Hierarchy.Path(base.transform) + "]");
			}
			int num3 = num2 & (((bool)fc) ? 1 : 0);
			if (num3 == 0)
			{
				Debug.LogError("Drone.Validate> Missing 'fc' at [" + Hierarchy.Path(base.transform) + "]");
			}
			int num4 = num3 & (((bool)receiver) ? 1 : 0);
			if (num4 == 0)
			{
				Debug.LogError("Drone.Validate> Missing 'receiver' at [" + Hierarchy.Path(base.transform) + "]");
			}
			int num5 = num4 & (((bool)rigidbody) ? 1 : 0);
			if (num5 == 0)
			{
				Debug.LogError("Drone.Validate> Missing 'rigidbody' at [" + Hierarchy.Path(base.transform) + "]");
			}
			int num6 = num5 & (((bool)renderer) ? 1 : 0);
			if (num6 == 0)
			{
				Debug.LogError("Drone.Validate> Missing 'renderer' at [" + Hierarchy.Path(base.transform) + "]");
			}
			int num7 = num6 & (((bool)body.frame) ? 1 : 0);
			if (num7 == 0)
			{
				Debug.LogError("Drone.Validate> Missing 'frame' at [" + Hierarchy.Path(base.transform) + "]");
			}
			int num8 = num7 & (((bool)body.frame.camera) ? 1 : 0);
			if (num8 == 0)
			{
				Debug.LogError("Drone.Validate> Missing 'camera' at [" + Hierarchy.Path(base.transform) + "]");
			}
			return (byte)num8 != 0;
		}

		public void UseCrashDelay(GameFlag p_type, bool p_multiplayer)
		{
			switch (p_type)
			{
			case GameFlag.Race:
				crashDelay = (p_multiplayer ? 0.33f : 0.22f);
				crashForceFactor = 10f;
				break;
			case GameFlag.Collectable:
				crashDelay = 0.1f;
				crashForceFactor = 20f;
				break;
			default:
				crashDelay = 0f;
				crashForceFactor = 20f;
				break;
			}
		}

		public void Crash(float p_crashEnergy, Vector3 p_contactNormal, Vector3 p_impactVelocity, Vector3 p_contactPoint, float p_energyTransferRate = -1f)
		{
			if (!crashEnabled || invulnerable > 0f || m_isBroken)
			{
				return;
			}
			m_isBroken = true;
			if (hasFc)
			{
				fc.armed = false;
			}
			if (!hasBody || !body.hasFrame || body.frame == null || body.frame.crash == null)
			{
				return;
			}
			this.TimerRunOnce(delegate
			{
				if (m_crashData == null)
				{
					m_crashData = new CrashData(DroneEventType.Crash, p_crashEnergy, p_contactNormal, p_impactVelocity, p_contactPoint, p_isBroken: true, body.frame.crash.nodes);
				}
				else
				{
					m_crashData.type = DroneEventType.Crash;
					m_crashData.crashEnergy = p_crashEnergy;
					m_crashData.contactNormal = p_contactNormal;
					m_crashData.impactVelocity = p_impactVelocity;
					m_crashData.contactPoint = p_contactPoint;
					m_crashData.nodes = body.frame.crash.nodes;
					m_crashData.isBroken = true;
				}
				Dispatch(DroneEventType.Crash);
				List<DroneCrashNode> nodes = body.frame.crash.nodes;
				float num = float.MaxValue;
				DroneCrashNode impact_node = null;
				for (int i = 0; i < nodes.Count; i++)
				{
					DroneCrashNode droneCrashNode = nodes[i];
					if (!(droneCrashNode == null))
					{
						float num2 = Vector3.Distance(droneCrashNode.transform.position, p_contactPoint);
						if (num2 < num)
						{
							num = num2;
							impact_node = droneCrashNode;
						}
					}
				}
				float transfer_rate = ((p_energyTransferRate > 0f) ? p_energyTransferRate : CrashEnergyTransferRate);
				this.TimerRunOnce(delegate
				{
					if (impact_node != null)
					{
						impact_node.integrity = 0f;
						impact_node.Break(p_crashEnergy, p_impactVelocity, CrashEnergy, transfer_rate, crashForceFactor, body.frame.transform.position);
					}
					if (body.frame.transform.GetComponentsInChildren<DroneCrashNode>().Length <= 1)
					{
						if (collidersNode == null)
						{
							collidersNode = body.frame.transform.Find("colliders");
						}
						if ((bool)collidersNode)
						{
							for (int j = 0; j < collidersNode.childCount; j++)
							{
								collidersNode.GetChild(j).gameObject.SetActive(value: false);
							}
							BoxCollider boxCollider = GetComponent<BoxCollider>();
							if (!boxCollider)
							{
								boxCollider = base.gameObject.AddComponent<BoxCollider>();
							}
							boxCollider.center = new Vector3(0f, 0.018f, -0.02f);
							boxCollider.size = new Vector3(0.1f, 0.03f, 0.15f);
							boxCollider.enabled = true;
						}
					}
				}, crashDelay);
			}, 1f / 60f);
		}

		public void CrashRemote(float p_crashEnergy, Vector3 p_contactNormal, Vector3 p_impactVelocity, Vector3 p_contactPoint, float p_ping)
		{
			if (m_isBroken || !isRemote)
			{
				return;
			}
			m_isBroken = true;
			this.TimerRunOnce(delegate
			{
				if (hasFc)
				{
					fc.armed = false;
				}
				if (hasBody && body.hasFrame && !(body.frame == null) && !(body.frame.crash == null))
				{
					List<DroneCrashNode> nodes = body.frame.crash.nodes;
					DroneCrashNode closestCrashNode = GetClosestCrashNode(p_contactPoint, nodes);
					if (closestCrashNode != null)
					{
						closestCrashNode.integrity = 0f;
						closestCrashNode.Break(p_crashEnergy, p_impactVelocity, CrashEnergy, CrashEnergyTransferRate, crashForceFactor, body.frame.transform.position);
					}
					if (body.frame.transform.GetComponentsInChildren<DroneCrashNode>().Length <= 1)
					{
						if (collidersNode == null)
						{
							collidersNode = body.frame.transform.Find("colliders");
						}
						if ((bool)collidersNode)
						{
							for (int i = 0; i < collidersNode.childCount; i++)
							{
								collidersNode.GetChild(i).gameObject.SetActive(value: false);
							}
							BoxCollider boxCollider = GetComponent<BoxCollider>();
							if (!boxCollider)
							{
								boxCollider = base.gameObject.AddComponent<BoxCollider>();
							}
							boxCollider.center = new Vector3(0f, 0.018f, -0.02f);
							boxCollider.size = new Vector3(0.1f, 0.03f, 0.15f);
							boxCollider.enabled = true;
						}
					}
				}
			}, p_ping / 2f);
		}

		public void Crash()
		{
			Crash(100f, new Vector3(0f, 0f, 10f), fc.sensor.inertial.actualVelocity / 2f, Vector3.zero, 2f);
		}

		public void Damage(float p_damage, Vector3 p_contactNormal, Vector3 p_impactVelocity, Vector3 p_contactPoint, float p_energy, DroneQuadrantRegion p_region)
		{
			if (!crashEnabled || invulnerable > 0f || !hasBody || !body.hasFrame || body.frame == null || body.frame.crash == null)
			{
				return;
			}
			List<DroneCrashNode> nodes = body.frame.crash.nodes;
			damage = p_damage;
			float num = 0f;
			float[] array = new float[4];
			if (m_isBroken)
			{
				damage = 0f;
				return;
			}
			DroneCrashNode closestCrashNode = GetClosestCrashNode(p_contactPoint, nodes);
			float num2 = 0f;
			if (closestCrashNode != null)
			{
				closestCrashNode.integrity = 0f;
				num2 = closestCrashNode.CalculateTotalSturdiness(nodes.Count);
			}
			damage -= damage * num2;
			if (damage < 0f)
			{
				damage = 0f;
			}
			if (closestCrashNode.tags.Count > 0 && closestCrashNode.tags.Contains(CrashNodeType.Prop0))
			{
				switch (p_region)
				{
				case DroneQuadrantRegion.UpperLeftArm:
					array[0] = damage;
					break;
				case DroneQuadrantRegion.UpperRightArm:
					array[1] = damage;
					break;
				case DroneQuadrantRegion.LowerLeftArm:
					array[2] = damage;
					break;
				case DroneQuadrantRegion.LowerRightArm:
					array[3] = damage;
					break;
				}
			}
			else
			{
				num = damage;
			}
			for (int i = 0; i < nodes.Count; i++)
			{
				if (!(nodes[i] == null))
				{
					nodes[i].ResetSturdinessReduction();
				}
			}
			if (m_crashData == null)
			{
				m_crashData = new CrashData(DroneEventType.Scrape, p_energy, p_contactNormal, p_impactVelocity, p_contactPoint, p_isBroken: false, nodes, num, array);
			}
			else
			{
				m_crashData.type = DroneEventType.Scrape;
				m_crashData.crashEnergy = p_energy;
				m_crashData.contactNormal = p_contactNormal;
				m_crashData.impactVelocity = p_impactVelocity;
				m_crashData.contactPoint = p_contactPoint;
				m_crashData.isBroken = false;
				m_crashData.nodes = nodes;
				m_crashData.bodyDamage = num;
				m_crashData.propsDamage = array;
			}
			Dispatch(DroneEventType.Scrape);
			Debug.Log("Drone> Drone damage occurred: " + damage);
			damage = 0f;
		}

		public void Scrape(float p_energy)
		{
			if (m_crashData == null)
			{
				m_crashData = new CrashData(DroneEventType.ScrapeAudio, p_energy, Vector3.zero, Vector3.zero, Vector3.zero, p_isBroken: false, null);
			}
			else
			{
				m_crashData.type = DroneEventType.ScrapeAudio;
				m_crashData.crashEnergy = p_energy;
			}
			if (!(p_energy < 2f))
			{
				Dispatch(DroneEventType.ScrapeAudio);
			}
		}

		public void Fix()
		{
			invulnerable = 2f;
			damage = 0f;
			damageReduction = 0f;
			if (hasFc && m_isBroken)
			{
				fc.armed = true;
			}
			m_isBroken = false;
			if (!hasBody || !body.hasFrame || body.frame == null || body.frame.crash == null)
			{
				return;
			}
			List<DroneCrashNode> nodes = body.frame.crash.nodes;
			if (m_spinoutActivity != null)
			{
				m_spinoutActivity.Stop();
				m_spinoutActivity = null;
			}
			if (!isRemote)
			{
				Dispatch(DroneEventType.Recover);
			}
			if (nodes != null)
			{
				foreach (DroneCrashNode item in nodes)
				{
					item.Fix();
				}
			}
			if ((bool)collidersNode)
			{
				for (int i = 0; i < collidersNode.childCount; i++)
				{
					collidersNode.GetChild(i).gameObject.SetActive(value: true);
				}
				BoxCollider component = GetComponent<BoxCollider>();
				if ((bool)component)
				{
					component.enabled = false;
				}
			}
		}

		[ContextMenu("FixSnap")]
		public void FixSnap()
		{
			if (hasBody && body.hasFrame && !(body.frame == null) && !(body.frame.crash == null))
			{
				body.frame.crash.FixSnap();
			}
		}

		private DroneCrashNode GetClosestCrashNode(Vector3 p_impactPoint, List<DroneCrashNode> p_crashNodes)
		{
			DroneCrashNode result = null;
			float num = float.MaxValue;
			for (int i = 0; i < p_crashNodes.Count; i++)
			{
				DroneCrashNode droneCrashNode = p_crashNodes[i];
				if (!(droneCrashNode == null))
				{
					float num2 = Vector3.Distance(droneCrashNode.transform.position, p_impactPoint);
					if (num2 < num)
					{
						num = num2;
						result = droneCrashNode;
					}
				}
			}
			return result;
		}

		public void WaterImpact()
		{
		}

		public void ApplySpinout(float p_deviationFactor)
		{
			if (!(Spinout > 0f))
			{
				return;
			}
			if (m_spinoutActivity != null)
			{
				m_spinoutActivity.Stop();
				m_spinoutActivity = null;
			}
			m_spinoutActivity = ((Component)this).TimerRun((Action)delegate
			{
				if (!m_isBroken)
				{
					rigidbody.rb.AddTorque(rigidbody.ForceToTorque(m_crashData.contactNormal * m_crashData.impactVelocity.magnitude * Spinout, m_crashData.contactPoint));
				}
			}, p_deviationFactor, 0f);
		}

		public void SetEnabled(bool p_flag)
		{
			if ((bool)rigidbody)
			{
				rigidbody.enabled = p_flag;
				rigidbody.isKinematic = !p_flag;
			}
			if ((bool)fc)
			{
				fc.enabled = p_flag;
			}
		}

		public void ClearLastState()
		{
			lastState.angularVelocity = Vector3.zero;
			lastState.fcEnabled = true;
			lastState.motorSpeeds = new float[4];
			lastState.rigidbodyEnabled = true;
			lastState.rigidbodyKinematic = false;
			lastState.velocity = Vector3.zero;
		}

		public void SetPaused(bool p_flag)
		{
			if (p_flag)
			{
				if (ready)
				{
					List<DroneESC> escs = body.frame.escs;
					if (lastState.motorSpeeds == null || lastState.motorSpeeds.Length != escs.Count)
					{
						lastState.motorSpeeds = new float[escs.Count];
					}
					for (int i = 0; i < escs.Count; i++)
					{
						if (!escs[i].motor)
						{
							lastState.motorSpeeds[i] = 0f;
							continue;
						}
						lastState.motorSpeeds[i] = escs[i].motor.rpm;
						escs[i].motor.rpm = 0f;
						escs[i].motor.rpmAudio = 0f;
						if ((bool)escs[i].motor.animation)
						{
							escs[i].motor.animation.rpm = 0f;
							escs[i].motor.animation.ForceUpdate(p_immediate: true);
						}
					}
				}
				if ((bool)rigidbody)
				{
					lastState.velocity = rigidbody.rb.velocity;
					lastState.angularVelocity = rigidbody.rb.angularVelocity;
					lastState.rigidbodyEnabled = rigidbody.enabled;
					lastState.rigidbodyKinematic = rigidbody.isKinematic;
					rigidbody.enabled = false;
					rigidbody.isKinematic = true;
				}
				if ((bool)fc)
				{
					lastState.fcEnabled = fc.enabled;
					fc.enabled = false;
				}
				return;
			}
			if (ready)
			{
				List<DroneESC> escs2 = body.frame.escs;
				if (lastState.motorSpeeds != null && lastState.motorSpeeds.Length == escs2.Count)
				{
					for (int j = 0; j < escs2.Count; j++)
					{
						if ((bool)escs2[j].motor)
						{
							escs2[j].motor.rpm = lastState.motorSpeeds[j];
							escs2[j].motor.rpmAudio = lastState.motorSpeeds[j];
							if ((bool)escs2[j].motor.animation)
							{
								escs2[j].motor.animation.rpm = lastState.motorSpeeds[j];
								escs2[j].motor.animation.ForceUpdate(p_immediate: true);
							}
						}
					}
				}
			}
			if ((bool)rigidbody)
			{
				rigidbody.enabled = lastState.rigidbodyEnabled;
				rigidbody.isKinematic = lastState.rigidbodyKinematic;
				rigidbody.rb.velocity = lastState.velocity;
				rigidbody.rb.angularVelocity = lastState.angularVelocity;
			}
			if ((bool)fc)
			{
				fc.enabled = lastState.fcEnabled;
			}
		}

		public void SetMotorRPM(float p_rpm)
		{
			if (!ready)
			{
				return;
			}
			List<DroneESC> escs = body.frame.escs;
			for (int i = 0; i < escs.Count; i++)
			{
				if ((bool)escs[i] && (bool)escs[i].motor && (bool)escs[i].motor.animation)
				{
					escs[i].motor.rpm = p_rpm;
					escs[i].motor.animation.rpm = p_rpm;
					escs[i].motor.rpmAudio = p_rpm;
				}
			}
		}

		public void SetMotorSpinSpeed(float p_speed, float p_duration = 0f)
		{
			if (!ready)
			{
				return;
			}
			List<DroneESC> escs = body.frame.escs;
			for (int i = 0; i < escs.Count; i++)
			{
				if ((bool)escs[i] && (bool)escs[i].motor)
				{
					DroneMotorAnimation animation = escs[i].motor.animation;
					if ((bool)animation)
					{
						animation.FadeSpeed(p_speed, p_duration);
					}
				}
			}
		}

		public void ResetOrientation()
		{
			Vector3 forward = base.transform.forward;
			forward.y = 0f;
			base.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
		}

		public virtual void ClearForces()
		{
			if ((bool)rigidbody)
			{
				rigidbody.ClearForces();
			}
			if ((bool)threaded)
			{
				threaded.ClearForces();
			}
			if ((bool)simulation)
			{
				simulation.ClearForces();
			}
			wind = Vector3.zero;
		}

		public void ResetPosition(Vector3 p_target, bool p_forcePodium = false)
		{
			m_lastResetTime = Time.time;
			ResetOrientation();
			Vector3 vector = (p_forcePodium ? Vector3.zero : (Vector3.up * 0.25f));
			position = p_target + vector;
			ResetOrientation();
			ClearForces();
			if ((bool)fc)
			{
				fc.Reset();
			}
			if (physics != null && physics.aerodynamics != null)
			{
				physics.aerodynamics.Reset();
			}
			m_lastPLacedPosition = p_target;
		}

		public void ResetPosition()
		{
			m_lastResetTime = Time.time;
			ResetOrientation();
			float[] array = new float[3] { 5f, 10f, 50f };
			for (int i = 0; i < array.Length; i++)
			{
				NavMesh.SamplePosition(position, out var hit, array[i], -1);
				if (hit.hit)
				{
					position = hit.position;
					break;
				}
			}
			ResetOrientation();
			ClearForces();
			if ((bool)fc)
			{
				fc.Reset();
			}
			if (physics != null && physics.aerodynamics != null)
			{
				physics.aerodynamics.Reset();
			}
		}

		public virtual float EstimateTopSpeed()
		{
			if (m_speedEstimatedThisFrame && d_topSpeed > 0f)
			{
				return d_topSpeed;
			}
			m_speedEstimatedThisFrame = true;
			if ((bool)simulation)
			{
				return d_topSpeed = Mathf.Round(simulation.EstimatedTopSpeed() / 3f) * 3f;
			}
			m_speedEstimatedThisFrame = false;
			return 0f;
		}

		public virtual void AutotunePid()
		{
			if ((bool)simulation)
			{
				simulation.AutotunePid();
			}
			else if ((bool)m_pidAutoTuner)
			{
				m_pidAutoTuner.StartAutoTune();
			}
		}

		public void SetNaN()
		{
			m_hasNaN = true;
		}

		public void CheckNaN()
		{
			if (!m_hasNaN)
			{
				return;
			}
			m_hasNaN = false;
			if (float.IsNaN(rootOffset.x) || float.IsNaN(rootOffset.y) || float.IsNaN(rootOffset.z))
			{
				rootOffset = Vector3.zero;
			}
			if (float.IsNaN(base.transform.position.x) || float.IsNaN(base.transform.position.y) || float.IsNaN(base.transform.position.z))
			{
				ResetPosition();
			}
			if (float.IsNaN(base.transform.localPosition.x) || float.IsNaN(base.transform.localPosition.y) || float.IsNaN(base.transform.localPosition.z))
			{
				ResetPosition();
			}
			if (hasPhysics && float.IsNaN(physics.groundeffectDistance))
			{
				physics.groundeffectDistance = 0.3f;
			}
			if (base.transform.localScale.x <= 0.0001f || base.transform.localScale.y <= 0.0001f || base.transform.localScale.z <= 0.0001f)
			{
				base.transform.localScale = Vector3.one;
			}
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>(includeInactive: true);
			foreach (Collider collider in componentsInChildren)
			{
				if (float.IsNaN(collider.transform.localScale.x) || Mathf.Abs(collider.transform.localScale.x) <= 1E-06f)
				{
					collider.transform.localScale = Vector3.one;
					throw new InvalidOperationException(collider.transform.name + (float.IsNaN(collider.transform.localScale.x) ? " x scale NaN" : " x scale zero"));
				}
				if (float.IsNaN(collider.transform.localScale.y) || Mathf.Abs(collider.transform.localScale.y) <= 1E-06f)
				{
					collider.transform.localScale = Vector3.one;
					throw new InvalidOperationException(collider.transform.name + (float.IsNaN(collider.transform.localScale.x) ? " y scale NaN" : " y scale zero"));
				}
				if (float.IsNaN(collider.transform.localScale.z) || Mathf.Abs(collider.transform.localScale.z) <= 1E-06f)
				{
					collider.transform.localScale = Vector3.one;
					throw new InvalidOperationException(collider.transform.name + (float.IsNaN(collider.transform.localScale.x) ? " z scale NaN" : " z scale zero"));
				}
			}
		}

		public void FixNaN()
		{
			base.transform.position = Vector3.zero;
			if (hasRigidbody && rigidbody.rb != null)
			{
				rigidbody.rb.position = Vector3.zero;
				rigidbody.rb.velocity = Vector3.zero;
				rigidbody.rb.angularVelocity = Vector3.zero;
			}
			if (hasThreaded)
			{
				threaded.VirtualPoint = Vector3.zero;
				threaded.HandCalculatedVelocity = Vector3.zero;
				threaded.HandCalculatedAngularVelocity = Vector3.zero;
			}
			if (hasMixer)
			{
				mixer.yaw.pid.Reset();
				mixer.pitch.pid.Reset();
				mixer.roll.pid.Reset();
			}
			Dispatch(DroneEventType.NanRecover);
		}

		protected virtual void FixedUpdate()
		{
			if (!ready || isRemote)
			{
				return;
			}
			_ = profilerEnabled;
			if (isGhost && hasFc && fc.armed && rigidbody.backtraceTriggers)
			{
				rigidbody.BackTraceTriggers();
			}
			_ = profilerEnabled;
			if (!isGhost)
			{
				if (hasFc && fc.armed)
				{
					_ = profilerEnabled;
					LinkParameters();
					_ = profilerEnabled;
					_ = profilerEnabled;
					LinkFlightModes();
					_ = profilerEnabled;
					_ = profilerEnabled;
					mixer.OnUpdate(this, Time.fixedDeltaTime);
					_ = profilerEnabled;
				}
				_ = profilerEnabled;
				if (hasSimulation)
				{
					simulation.OnFixedUpdate();
				}
				_ = profilerEnabled;
				_ = profilerEnabled;
				if (hasRigidbody)
				{
					rigidbody.OnFixedUpdate();
				}
				_ = profilerEnabled;
				_ = profilerEnabled;
				if (hasThreaded)
				{
					threaded.OnFixedUpdate();
				}
				_ = profilerEnabled;
				_ = profilerEnabled;
				if (hasFc && fc.armed && rigidbody.backtraceTriggers)
				{
					rigidbody.BackTraceTriggers();
				}
				_ = profilerEnabled;
				_ = profilerEnabled;
				if (rigidbody.backtraceCollisions)
				{
					rigidbody.BackTraceCollisions();
				}
				_ = profilerEnabled;
				if (m_updateLanded)
				{
					m_updateLanded = false;
					SetDroneLandedFlags(m_landed);
				}
				_ = profilerEnabled;
				if (hasNaN)
				{
					CheckNaN();
				}
				_ = profilerEnabled;
			}
		}

		protected virtual void Update()
		{
			if (!ready)
			{
				return;
			}
			m_speedEstimatedThisFrame = false;
			if (hasThreaded)
			{
				if (float.IsNaN(threaded.VirtualPoint.x) || float.IsNaN(threaded.VirtualPoint.y) || float.IsNaN(threaded.VirtualPoint.z))
				{
					FixNaN();
				}
				else
				{
					m_nanPosition = threaded.VirtualPoint;
				}
				if (float.IsNaN(threaded.HandCalculatedVelocity.x) || float.IsNaN(threaded.HandCalculatedVelocity.y) || float.IsNaN(threaded.HandCalculatedVelocity.z))
				{
					FixNaN();
				}
				else
				{
					m_nanVelocity = threaded.HandCalculatedVelocity;
				}
				if (float.IsNaN(threaded.HandCalculatedAngularVelocity.x) || float.IsNaN(threaded.HandCalculatedAngularVelocity.y) || float.IsNaN(threaded.HandCalculatedAngularVelocity.z))
				{
					FixNaN();
				}
				else
				{
					m_nanAngular = threaded.HandCalculatedAngularVelocity;
				}
			}
			if (pidTuneRunning)
			{
				invulnerable = 2f;
			}
			else if (invulnerable > 0f)
			{
				invulnerable -= Time.deltaTime;
			}
			m_speedEstimatedThisFrame = false;
			if (hasFc && fc.armed)
			{
				_ = profilerEnabled;
				if (hasSimulation)
				{
					simulation.OnUpdate();
				}
				_ = profilerEnabled;
				_ = profilerEnabled;
				if (hasThreaded)
				{
					threaded.OnUpdate();
				}
				_ = profilerEnabled;
				_ = profilerEnabled;
				if (!isGhost && !isRemote && rigidbody.backtraceTriggers)
				{
					rigidbody.BackTraceTriggers();
				}
				_ = profilerEnabled;
			}
		}

		public void GarageUpdate()
		{
			m_speedEstimatedThisFrame = false;
		}

		public void Destroy(bool p_async = false)
		{
			base.transform.SetParent(null, worldPositionStays: true);
			base.gameObject.hideFlags = HideFlags.HideInHierarchy;
			if (p_async)
			{
				_ = (float)Hierarchy.TraverseDestroy(base.transform);
			}
			else
			{
				Destroy(base.gameObject);
			}
			m_ready = false;
		}

		public void StabilizeDroneOnGround(bool p_flag)
		{
			m_landed = p_flag;
			m_updateLanded = true;
		}

		protected void SetDroneLandedFlags(bool p_flag)
		{
			if (p_flag)
			{
				if (fc != null)
				{
					fc.landed = true;
				}
				if (rigidbody.rb.centerOfMass != -rootOffset)
				{
					rigidbody.rb.centerOfMass = -rootOffset;
				}
			}
			else if (rigidbody.rb.centerOfMass != Vector3.Lerp(Vector3.zero, body.centerOfMass, (physics.useCOG && !isThreaded) ? ((Mathf.Abs(body.centerOfMass.z) * 2000f - profile.pitchPID.i) * 0.001f) : 0f))
			{
				rigidbody.rb.centerOfMass = Vector3.Lerp(Vector3.zero, body.centerOfMass, (physics.useCOG && !isThreaded) ? ((Mathf.Abs(body.centerOfMass.z) * 2000f - profile.pitchPID.i) * 0.001f) : 0f);
			}
			if (body != null)
			{
				body.SetLandingGear(p_flag);
			}
		}

		public void UpdateCenterOfMass()
		{
			m_updateLanded = true;
		}

		public void DumpState()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("drone:").Append(rig.name).Append(" [gameobject ")
				.Append(base.name)
				.Append("]")
				.AppendLine();
			stringBuilder.Append("rig:").Append(rig.ToJson()).AppendLine();
			stringBuilder.Append("transform: position [").Append(base.transform.position.ToString()).Append("] rotation [")
				.Append(base.transform.rotation.ToString())
				.Append("] scale [")
				.Append(base.transform.lossyScale.ToString())
				.Append("]")
				.AppendLine();
			stringBuilder.Append("rigidbody: position [").Append(rigidbody.rb.position.ToString()).Append("] rotation [")
				.Append(rigidbody.rb.rotation.ToString())
				.Append("] velocity [")
				.Append(rigidbody.rb.velocity.ToString())
				.Append("] angular [")
				.Append(rigidbody.rb.angularVelocity.ToString())
				.Append("]")
				.AppendLine();
			stringBuilder.Append("signals: throttle [").Append(fc.signal.throttle.ToString()).Append("] pitch [")
				.Append(fc.signal.pitch.ToString())
				.Append("] roll [")
				.Append(fc.signal.roll.ToString())
				.Append("] yaw [")
				.Append(fc.signal.yaw.ToString())
				.Append("]")
				.AppendLine();
			stringBuilder.Append("forces: position [").Append(threaded.VirtualPoint.ToString()).Append("] rotation [")
				.Append(threaded.VirtualRotation.ToString())
				.Append("] velocity [")
				.Append(threaded.HandCalculatedVelocity.ToString())
				.Append("] angular [")
				.Append(threaded.HandCalculatedAngularVelocity.ToString())
				.Append("]")
				.AppendLine();
			stringBuilder.Append("drag: cd [").Append(physics.aerodynamics.Cd.ToString()).Append("] cl [")
				.Append(physics.aerodynamics.Cl.ToString())
				.Append("] force [")
				.Append(physics.aerodynamics.totalForce.ToString())
				.Append("]")
				.AppendLine();
			Debug.LogError(stringBuilder.ToString());
			stringBuilder.Length = 0;
			stringBuilder.Append("drone:").Append(rig.name).Append(" [gameobject ")
				.Append(base.name)
				.Append("]")
				.AppendLine();
			stringBuilder.Append("colliders------------------------------------------------------------").AppendLine();
			foreach (Collider collider in rigidbody.colliders)
			{
				stringBuilder.Append("collider ");
				FullHierarchy(stringBuilder, collider.transform, base.transform);
				stringBuilder.Append(collider.enabled ? " [enabled]" : "[disabled]").Append(" position [").Append(collider.transform.position.ToString())
					.Append("] rotation [")
					.Append(collider.transform.rotation.ToString())
					.Append("] scale [")
					.Append(collider.transform.lossyScale.ToString())
					.Append("]")
					.AppendLine();
				if (stringBuilder.Length > 10240)
				{
					Debug.LogError(stringBuilder.ToString());
					stringBuilder.Length = 0;
					stringBuilder.Append("...continued:").AppendLine();
				}
			}
			Debug.LogError(stringBuilder.ToString());
		}

		public void FullHierarchy(StringBuilder sb, Transform p_child, Transform p_ancestor)
		{
			while (p_child != p_ancestor && p_child != null)
			{
				sb.Append(p_child.name).Append("/");
			}
			if (p_child == null)
			{
				sb.Append("{root}");
			}
			else
			{
				sb.Append(p_child.name);
			}
		}

		public void SetBatteryResistance(bool p_sag = true, bool p_drain = false, float p_capacity = 0f, float p_resistance = 8f)
		{
			defaultphysics.batterySag = p_sag;
			defaultphysics.batteryDrain = p_drain;
			defaultphysics.batteryCapacity = p_capacity;
			defaultphysics.batteryResistance = p_resistance;
			physics.batterySag = p_sag;
			physics.batteryDrain = p_drain;
			physics.batteryCapacity = p_capacity;
			physics.batteryResistance = p_resistance;
		}

		public void ResetBatteryResistance()
		{
			SetBatteryResistance();
		}
	}
}

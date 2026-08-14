using System.Collections.Generic;
using UnityEngine;
using drl.network;
using thelab.core;

namespace drl.sim
{
	public class DroneNetworkTransmitter : DroneInputTransmitter, INetworkObservable
	{
		private DroneBatteryPowerData m_batteryPowerData;

		private NetworkRacer m_network_racer;

		private long m_PackedInputAndRPM;

		public Vector4 Input;

		private readonly FloatInterpolator rpmInterpolator = new FloatInterpolator(InterpolationType.Lerp);

		private readonly FloatInterpolator inputXInterpolator = new FloatInterpolator(InterpolationType.Lerp);

		private readonly FloatInterpolator inputYInterpolator = new FloatInterpolator(InterpolationType.Lerp);

		private readonly FloatInterpolator inputZInterpolator = new FloatInterpolator(InterpolationType.Lerp);

		private readonly FloatInterpolator inputWInterpolator = new FloatInterpolator(InterpolationType.Lerp);

		private readonly float[] cachedNetworkRPM = new float[4];

		private bool canSync = true;

		private bool m_use_physics;

		public bool IsReady
		{
			get
			{
				if (drone != null && drone.transform != null && drone.hasBody)
				{
					return drone.body.hasFrame;
				}
				return false;
			}
		}

		public Transform NetworkTransform
		{
			get
			{
				if (!(drone == null))
				{
					return drone.transform;
				}
				return null;
			}
		}

		public Rigidbody NetworkRigidbody => drone.rigidbody.rb;

		public DroneBatteryPowerData BatteryPowerData
		{
			get
			{
				if (m_batteryPowerData == null)
				{
					m_batteryPowerData = new DroneBatteryPowerData();
				}
				DSElectrical electrical = drone.fc.sensor.electrical;
				m_batteryPowerData.voltage = electrical.voltage;
				m_batteryPowerData.voltageMin = electrical.voltageMin;
				m_batteryPowerData.voltageMax = electrical.voltageMax;
				m_batteryPowerData.voltageAvailable = electrical.voltageAvailable;
				m_batteryPowerData.remainingCharge = electrical.remainingCharge;
				m_batteryPowerData.currentDraw = electrical.currentDraw;
				m_batteryPowerData.currentMax = electrical.currentMax;
				m_batteryPowerData.totalCapacity = electrical.totalCapacity;
				return m_batteryPowerData;
			}
		}

		public NetworkRacer networkRacer
		{
			get
			{
				if (!m_network_racer)
				{
					return m_network_racer = GetComponent<NetworkRacer>();
				}
				return m_network_racer;
			}
		}

		public long PackedInputAndRPM
		{
			get
			{
				return m_PackedInputAndRPM;
			}
			set
			{
				m_PackedInputAndRPM = value;
				float p_rpm = 0f;
				float p_throttle = 0f;
				float p_yaw = 0f;
				float p_pitch = 0f;
				float p_roll = 0f;
				FromU64(m_PackedInputAndRPM, 1f, ref p_rpm, ref p_throttle, ref p_yaw, ref p_pitch, ref p_roll);
				p_yaw = inputXInterpolator.Evaluate(p_yaw, 0.5f);
				p_throttle = inputYInterpolator.Evaluate(p_throttle, 0.5f);
				p_roll = inputZInterpolator.Evaluate(p_roll, 0.5f);
				p_pitch = inputWInterpolator.Evaluate(p_pitch, 0.5f);
				Input = new Vector4(p_yaw, p_throttle, p_roll, p_pitch);
				p_rpm = rpmInterpolator.Evaluate(p_rpm);
				cachedNetworkRPM[0] = p_rpm;
				cachedNetworkRPM[1] = p_rpm;
				cachedNetworkRPM[2] = p_rpm;
				cachedNetworkRPM[3] = p_rpm;
			}
		}

		public INetworkObject NetworkObject { get; set; }

		public NetworkActor Actor
		{
			get
			{
				if (NetworkObject != null)
				{
					return NetworkObject.Actor;
				}
				return null;
			}
		}

		public ControllerStateType ControllerType
		{
			get
			{
				if (Actor != null)
				{
					return (ControllerStateType)Actor.ControllerType;
				}
				return ControllerStateType.Taranis;
			}
		}

		public float[] NetworkRPMs
		{
			get
			{
				return cachedNetworkRPM;
			}
			set
			{
			}
		}

		public bool CanSync => canSync;

		public bool UsePhysics
		{
			get
			{
				return m_use_physics;
			}
			set
			{
				m_use_physics = value;
				Drone drone = base.drone;
				if ((bool)drone && drone.ready)
				{
					drone.enabled = !m_use_physics;
					drone.rigidbody.rb.constraints = ((!m_use_physics) ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None);
					drone.rigidbody.isKinematic = !m_use_physics;
					drone.rigidbody.rb.useGravity = m_use_physics;
					drone.rigidbody.SetCollisionEnabled(m_use_physics);
				}
			}
		}

		GameObject INetworkObservable.gameObject => base.gameObject;

		public static long ToU64(float p_rpm, float p_max_rpm, float p_throttle, float p_yaw, float p_pitch, float p_roll)
		{
			long num = (long)(Mathf.Min(p_rpm, p_max_rpm) / p_max_rpm * 32767f);
			long num2 = (long)((p_throttle + 1f) * 0.5f * 4095f);
			long num3 = (long)((p_yaw + 1f) * 0.5f * 4095f);
			long num4 = (long)((p_pitch + 1f) * 0.5f * 4095f);
			long num5 = (long)((p_roll + 1f) * 0.5f * 4095f);
			return num2 | (num3 * 4096) | (num4 * 16777216) | (num5 * 68719476736L) | (num * 281474976710656L);
		}

		public static void FromU64(long p_data, float p_max_rpm, ref float p_rpm, ref float p_throttle, ref float p_yaw, ref float p_pitch, ref float p_roll)
		{
			float num = p_data & 0xFFF;
			float num2 = (p_data / 4096) & 0xFFF;
			float num3 = (p_data / 16777216) & 0xFFF;
			float num4 = (p_data / 68719476736L) & 0xFFF;
			float num5 = (p_data / 281474976710656L) & 0xFFFF;
			p_rpm = num5 / 32767f * p_max_rpm;
			p_throttle = (num / 4095f - 0.5f) * 2f;
			p_yaw = (num2 / 4095f - 0.5f) * 2f;
			p_pitch = (num3 / 4095f - 0.5f) * 2f;
			p_roll = (num4 / 4095f - 0.5f) * 2f;
		}

		protected override void OnUpdate(float p_dt)
		{
			if (this == null || !IsReady || !drone.ready)
			{
				return;
			}
			List<DroneESC> escs = drone.body.frame.escs;
			if (escs == null || escs.Count == 0 || NetworkRPMs == null)
			{
				return;
			}
			float[] networkRPMs = NetworkRPMs;
			int num = Mathf.Min(networkRPMs.Length, escs.Count);
			for (int i = 0; i < num; i++)
			{
				if ((bool)escs[i] && escs[i].hasMotor && escs[i].motor.hasAnimation)
				{
					if (UsePhysics)
					{
						escs[i].motor.rpm = Mathf.MoveTowards(escs[i].motor.rpm, 0f, p_dt * 30000f);
						escs[i].motor.rpmAudio = escs[i].motor.rpm;
						escs[i].motor.animation.rpm = escs[i].motor.rpm;
					}
					else
					{
						float num2 = networkRPMs[i] * 30000f;
						escs[i].motor.rpm = num2;
						escs[i].motor.rpmAudio = num2;
						escs[i].motor.animation.rpm = num2;
					}
				}
			}
		}

		public override ControllerStateType GetControllerType()
		{
			return ControllerType;
		}

		public override string GetPrefix()
		{
			return "nt";
		}

		public void SetPhysics(bool isEnabled, NetworkRoom.DroneState droneState)
		{
			UsePhysics = isEnabled;
			if (!(drone == null) && drone.ready)
			{
				canSync = !isEnabled;
				drone.rigidbody.rb.velocity = droneState.Velocity;
				drone.position = droneState.Position;
				drone.transform.rotation = Quaternion.Euler(droneState.Rotation);
			}
		}

		public void OnTeleport(float squaredDeltaDistance)
		{
			if (IsReady)
			{
				drone.renderer.ClearTrails();
			}
		}
	}
}

using UnityEngine;
using drl.network;
using drl.sim.rci;

namespace drl.sim
{
	public class DroneRCTransmitter : DroneInputTransmitter, INetworkObservable
	{
		public float scale = 1f;

		[Range(0f, 1f)]
		public float throttle;

		[Range(-1f, 1f)]
		public float altitude;

		[Range(-1f, 1f)]
		public float yaw;

		[Range(-1f, 1f)]
		public float pitch;

		[Range(-1f, 1f)]
		public float roll;

		public bool _debugOverride;

		private SignalVector m_kb_signal;

		public bool invertRoll;

		private DroneBatteryPowerData m_batteryPowerData;

		public Vector4 Input;

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

		public Rigidbody NetworkRigidbody
		{
			get
			{
				if (!drone)
				{
					return null;
				}
				if (!drone.rigidbody)
				{
					return null;
				}
				return drone.rigidbody.rb;
			}
		}

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

		public long PackedInputAndRPM
		{
			get
			{
				if (drone == null)
				{
					return 0L;
				}
				float p_rpm = drone.body.frame.GetRPMRatios()[0];
				float rawAxis = RCI.GetRawAxis(RawAxis.LeftStickY);
				float rawAxis2 = RCI.GetRawAxis(RawAxis.LeftStickX);
				float rawAxis3 = RCI.GetRawAxis(RawAxis.RightStickY);
				float rawAxis4 = RCI.GetRawAxis(RawAxis.RightStickX);
				Input = new Vector4(rawAxis2, rawAxis, rawAxis4, rawAxis3);
				return DroneNetworkTransmitter.ToU64(p_rpm, 1f, rawAxis, rawAxis2, rawAxis3, rawAxis4);
			}
			set
			{
			}
		}

		public float[] NetworkRPMs { get; set; }

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

		public bool CanSync => true;

		GameObject INetworkObservable.gameObject => base.gameObject;

		protected override void OnUpdate(float p_dt)
		{
			SignalVector signal = drone.receiver.signal;
			if (_debugOverride)
			{
				signal.throttle = throttle;
				signal.altitude = altitude;
				signal.yaw = yaw;
				signal.roll = roll;
				signal.pitch = pitch;
			}
			else if (!RCI.UsingKeyboardAsController)
			{
				float assignedAxis = RCI.GetAssignedAxis(AssignedAxis.Throttle, null, excludeZeroThrottle: false, useThrottleCap: true);
				signal.throttle = (assignedAxis + 1f) / 2f;
				signal.yaw = RCI.GetAssignedAxis(AssignedAxis.Yaw);
				signal.roll = (invertRoll ? (0f - RCI.GetAssignedAxis(AssignedAxis.Roll)) : RCI.GetAssignedAxis(AssignedAxis.Roll));
				signal.pitch = RCI.GetAssignedAxis(AssignedAxis.Pitch);
				signal.altitude = RCI.GetDJIModeThrottle();
			}
			else
			{
				SignalVector signalVector = new SignalVector
				{
					throttle = RCI.GetAssignedAxis(AssignedAxis.Throttle),
					yaw = RCI.GetAssignedAxis(AssignedAxis.Yaw),
					roll = (invertRoll ? (0f - RCI.GetAssignedAxis(AssignedAxis.Roll)) : RCI.GetAssignedAxis(AssignedAxis.Roll)),
					pitch = RCI.GetAssignedAxis(AssignedAxis.Pitch),
					altitude = RCI.GetDJIModeThrottle()
				};
				bool num = signalVector.magnitude > 0f;
				float deltaTime = Time.deltaTime;
				deltaTime = ((Mathf.Abs(signalVector.altitude) <= 1E-07f) ? 1f : Time.deltaTime);
				m_kb_signal.altitude = Mathf.Lerp(m_kb_signal.altitude, signalVector.altitude, Mathf.Clamp01(deltaTime * 6f));
				deltaTime = ((Mathf.Abs(signalVector.throttle) <= 1E-07f) ? 1f : Time.deltaTime);
				m_kb_signal.throttle = Mathf.Lerp(m_kb_signal.throttle, signalVector.throttle, Mathf.Clamp01(deltaTime * 6f));
				deltaTime = ((Mathf.Abs(signalVector.yaw) <= 1E-07f) ? 1f : Time.deltaTime);
				m_kb_signal.yaw = Mathf.Lerp(m_kb_signal.yaw, signalVector.yaw, Mathf.Clamp01(deltaTime * 6f));
				deltaTime = ((Mathf.Abs(signalVector.pitch) <= 1E-07f) ? 1f : Time.deltaTime);
				m_kb_signal.pitch = Mathf.Lerp(m_kb_signal.pitch, signalVector.pitch, Mathf.Clamp01(deltaTime * 6f));
				deltaTime = ((Mathf.Abs(signalVector.roll) <= 1E-07f) ? 1f : Time.deltaTime);
				m_kb_signal.roll = Mathf.Lerp(m_kb_signal.roll, signalVector.roll, Mathf.Clamp01(deltaTime * 6f));
				if (num)
				{
					signal.altitude = m_kb_signal.altitude;
					signal.throttle = m_kb_signal.throttle;
					signal.yaw = m_kb_signal.yaw;
					signal.pitch = m_kb_signal.pitch;
					signal.roll = m_kb_signal.roll;
				}
			}
			signal.Scale(scale);
			throttle = signal.throttle;
			altitude = signal.altitude;
			yaw = signal.yaw;
			roll = signal.roll;
			pitch = signal.pitch;
			if (drone.fc.mode == FlightControllerMode.Arcade)
			{
				signal.pitch = signal.roll;
				signal.throttle *= 0.5f;
			}
			drone.receiver.signal = signal;
		}

		public override ControllerStateType GetControllerType()
		{
			return RCI.GetControllerStateType(ControllerStateType.Taranis);
		}

		public override string GetPrefix()
		{
			return "rc";
		}

		public void OnTeleport(float squaredDeltaDistance)
		{
		}
	}
}

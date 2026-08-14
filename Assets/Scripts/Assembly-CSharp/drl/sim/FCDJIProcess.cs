using UnityEngine;

namespace drl.sim
{
	public class FCDJIProcess : FCProcess
	{
		public bool angleMode;

		public SignalVector m_outputSignal;

		public float dampenYaw = 1f;

		public bool overrideInput;

		public SignalVector inputSignal;

		public float angleRateMultiplier = 1f;

		public float param_maxRate = 600f;

		public float param_maxAngle = 53f;

		public float param_minAngle = 5f;

		public float param_minTrainingScale = 0.1f;

		public float param_maxTrainingScale = 1.1f;

		public float param_maxSpeed = 45f;

		public float param_minSpeed = 4f;

		public float rateWeight;

		private float m_targetPitch;

		private float m_targetRoll;

		private float m_heading;

		private float m_targetUpward;

		private float m_targetForward;

		private float m_targetSideways;

		private Vector3 m_direction;

		private readonly float m_deadzone = Mathf.Epsilon * 2f;

		private int changingAltitude;

		public float altitudeChangeScale = 3f;

		public float correctionScale = 0.1f;

		public float targetVerticalSpeed;

		public float targetSidewaysSpeed;

		public float targetForwardSpeed;

		public float deltaVerticalSpeed;

		public float deltaSidewaysSpeed;

		public float deltaForwardSpeed;

		private float clampedTopSpeed
		{
			get
			{
				if (topSpeed <= 0f)
				{
					return 20f;
				}
				return Mathf.Clamp(topSpeed * 0.5f, 0f, 20f);
			}
		}

		public float topSpeed { get; private set; }

		public float angleLimit { get; private set; }

		public SignalVector outputSignal => m_outputSignal;

		public float heading => m_heading;

		private PID upwardsPID => pids[0];

		private PID sidewaysPID => pids[1];

		private PID forwardPID => pids[2];

		private PID rollPID => pids[3];

		private PID pitchPID => pids[4];

		private PID yawPID => pids[5];

		protected override void OnPIDUpdate(PID p_pid)
		{
			if (p_pid == upwardsPID)
			{
				float y = base.fc.sensor.inertial.velocity.y;
				float targetUpward = m_targetUpward;
				p_pid.Update(y, targetUpward, deltaTime);
			}
			else if (p_pid == sidewaysPID)
			{
				float groundSideways = base.fc.sensor.inertial.groundSideways;
				float targetSideways = m_targetSideways;
				p_pid.Update(groundSideways, targetSideways, deltaTime);
			}
			else if (p_pid == rollPID)
			{
				float num = base.fc.sensor.gyro.local.z / 180f;
				if (num > 1f)
				{
					num -= 2f;
				}
				float targetRoll = m_targetRoll;
				p_pid.Update(num, targetRoll, deltaTime);
			}
			else if (p_pid == forwardPID)
			{
				float groundForward = base.fc.sensor.inertial.groundForward;
				float targetForward = m_targetForward;
				p_pid.Update(groundForward, targetForward, deltaTime);
			}
			else if (p_pid == pitchPID)
			{
				float num2 = base.fc.sensor.gyro.local.x / 180f;
				if (num2 > 1f)
				{
					num2 -= 2f;
				}
				float targetPitch = m_targetPitch;
				p_pid.Update(num2, targetPitch, deltaTime);
			}
			else if (p_pid == yawPID)
			{
				float num3 = base.fc.sensor.gyro.local.y;
				float num4 = m_heading;
				if (num3 - num4 > 180f)
				{
					num3 -= 360f;
				}
				else if (num3 - num4 < -180f)
				{
					num4 -= 360f;
				}
				p_pid.Update(num3, num4, deltaTime);
			}
		}

		public void SetHeading(float p_heading)
		{
			m_heading = p_heading;
		}

		protected override void OnUpdate()
		{
			rateWeight = ((param_maxRate > 0f) ? Mathf.Clamp01(base.fc.profile.max.pitchRoll / param_maxRate) : 0f);
			angleLimit = Mathf.Lerp(param_minAngle, param_maxAngle, rateWeight);
			topSpeed = Mathf.Lerp(param_minSpeed, param_maxSpeed, rateWeight);
			float num = Mathf.Lerp(param_minTrainingScale, param_maxTrainingScale, rateWeight);
			if (!overrideInput)
			{
				inputSignal = base.fc.normalizedSignal;
			}
			m_direction.x = inputSignal.roll;
			m_direction.z = inputSignal.pitch;
			m_direction.y = 0f;
			if (m_direction.sqrMagnitude > 1f)
			{
				m_direction.Normalize();
			}
			if (!overrideInput)
			{
				base.fc.process.altitude.speedLimit = topSpeed * 0.8f;
				base.fc.process.altitude.angleLimit = angleLimit;
			}
			if (Mathf.Abs(inputSignal.roll) > m_deadzone)
			{
				m_targetRoll = (0f - m_direction.x) * angleLimit / 180f;
			}
			else
			{
				m_targetRoll = 0f;
			}
			if (Mathf.Abs(inputSignal.pitch) > m_deadzone)
			{
				m_targetPitch = m_direction.z * angleLimit / 180f;
			}
			else
			{
				m_targetPitch = 0f;
			}
			if (Mathf.Abs(inputSignal.altitude) > m_deadzone)
			{
				m_targetUpward = inputSignal.altitude * num * clampedTopSpeed;
			}
			else
			{
				m_targetUpward = 0f;
			}
			if (Mathf.Abs(inputSignal.roll) > m_deadzone)
			{
				m_targetSideways = (0f - inputSignal.roll) * clampedTopSpeed;
			}
			else
			{
				m_targetSideways = 0f;
			}
			if (Mathf.Abs(inputSignal.pitch) > m_deadzone)
			{
				m_targetForward = inputSignal.pitch * clampedTopSpeed;
			}
			else
			{
				m_targetForward = 0f;
			}
			Vector3 local = base.fc.sensor.gyro.local;
			local.y = 0f;
			if (local.x > 180f)
			{
				local.x -= 360f;
			}
			if (local.z > 180f)
			{
				local.z -= 360f;
			}
			float magnitude = local.magnitude;
			if (!overrideInput)
			{
				m_outputSignal.throttle = Mathf.Clamp01(base.fc.process.altitude.hoverThrottle);
				if (Mathf.Abs(inputSignal.altitude) > m_deadzone)
				{
					base.fc.process.altitude.targetAltitude = base.fc.sensor.barometer.height + base.fc.process.altitude.overshoot + inputSignal.altitude * num;
					if (inputSignal.altitude > 0f)
					{
						if (changingAltitude != 1)
						{
							base.fc.process.altitude.Lock();
						}
						changingAltitude = 1;
					}
					else
					{
						if (changingAltitude != -1)
						{
							base.fc.process.altitude.Lock();
						}
						changingAltitude = -1;
					}
					m_outputSignal.throttle += inputSignal.altitude * num * Mathf.Clamp01(clampedTopSpeed / 20f) * altitudeChangeScale;
					if (base.fc.rawSignal.altitude < -0.9f)
					{
						m_outputSignal.throttle = 0f;
						base.fc.process.altitude.targetAltitude = base.fc.sensor.barometer.height - 1f;
					}
				}
				else
				{
					changingAltitude = 0;
				}
			}
			m_outputSignal.pitch = pitchPID.control * base.fc.profile.max.pitchRoll * angleRateMultiplier;
			m_outputSignal.roll = (0f - rollPID.control) * base.fc.profile.max.pitchRoll * angleRateMultiplier;
			if (base.fc.mode == FlightControllerMode.Horizon)
			{
				if (Mathf.Abs(base.fc.rawSignal.roll) < Mathf.Epsilon * 2f && Mathf.Abs(base.fc.rawSignal.pitch) > Mathf.Epsilon * 2f)
				{
					m_outputSignal.roll = base.fc.signal.roll;
				}
				if (Mathf.Abs(base.fc.rawSignal.pitch) < Mathf.Epsilon * 2f && Mathf.Abs(base.fc.rawSignal.roll) > Mathf.Epsilon * 2f)
				{
					m_outputSignal.pitch = base.fc.signal.pitch;
				}
			}
			dampenYaw = Mathf.Clamp(Mathf.Clamp01(clampedTopSpeed / 10f) * Mathf.Clamp01(angleLimit / 35f) * 2f, 0.2f, 1f);
			if (!overrideInput && Mathf.Abs(base.fc.rawSignal.yaw) > Mathf.Epsilon * 2f)
			{
				m_heading = base.fc.sensor.gyro.local.y + base.fc.signal.yaw * deltaTime * dampenYaw;
				m_outputSignal.yaw = 0f;
			}
			else
			{
				m_outputSignal.yaw = yawPID.control * clampedTopSpeed;
			}
			if (magnitude > 50f)
			{
				m_outputSignal.yaw *= Mathf.Clamp01(1f - (magnitude - 50f) / 35f);
			}
			targetVerticalSpeed = inputSignal.altitude * clampedTopSpeed;
			targetSidewaysSpeed = inputSignal.roll * clampedTopSpeed;
			targetForwardSpeed = inputSignal.pitch * clampedTopSpeed;
			deltaVerticalSpeed = targetVerticalSpeed - base.fc.sensor.inertial.velocity.y;
			deltaSidewaysSpeed = targetSidewaysSpeed - base.fc.sensor.inertial.groundSideways;
			deltaForwardSpeed = targetForwardSpeed - base.fc.sensor.inertial.groundForward;
			Vector3 forward = base.fc.drone.transform.forward;
			forward.y = 0f;
			Vector3 right = base.fc.drone.transform.right;
			right.y = 0f;
			Vector3 vector = Vector3.up * deltaVerticalSpeed + (angleMode ? Vector3.zero : (right * deltaSidewaysSpeed + forward * deltaForwardSpeed));
			base.fc.drone.rigidbody.rb.AddForce(vector * correctionScale);
			if (!base.fc.process.altitude.enabled)
			{
				m_outputSignal.throttle = base.fc.signal.throttle;
			}
		}

		public override void Reset()
		{
			base.Reset();
			m_heading = base.fc.drone.transform.localEulerAngles.y;
			if (base.gameObject.activeSelf && !overrideInput)
			{
				base.fc.process.altitude.Lock();
			}
		}
	}
}

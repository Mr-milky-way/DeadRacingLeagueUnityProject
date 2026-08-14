using UnityEngine;

namespace drl.sim
{
	public class FCSoftlockProcess : FCProcess
	{
		public FCDJIProcess dji;

		public FCAltitudeProcess altitude;

		public float targetHeading;

		public Vector3 targetPosition;

		private Vector3 m_localTarget;

		public float allowedError = 0.1f;

		public float speedLimit;

		public float directionScale = 1f;

		public float outputScale = 1f;

		public float overrideSignalWeight = 1f;

		public Transform target;

		public bool lockHeading;

		public bool lockAltitude;

		public bool lockGlobalX;

		public bool lockGlobalZ;

		public bool lockLocalX;

		public bool lockLocalZ;

		private Transform m_lockTarget;

		public SignalVector outputSignal;

		private PID altitudePID => pids[0];

		private PID yawPID => pids[1];

		private PID forwardPID => pids[2];

		private PID sidewaysPID => pids[3];

		private Transform lockTarget
		{
			get
			{
				if (m_lockTarget == null)
				{
					m_lockTarget = new GameObject("soft lock target").transform;
				}
				return m_lockTarget;
			}
		}

		public override void Boot()
		{
			base.Boot();
			if (dji == null)
			{
				dji = GetComponentInChildren<FCDJIProcess>(includeInactive: true);
			}
			if (altitude == null)
			{
				altitude = GetComponentInChildren<FCAltitudeProcess>(includeInactive: true);
			}
		}

		private void OnDestroy()
		{
			if (m_lockTarget != null)
			{
				Object.Destroy(m_lockTarget.gameObject);
			}
		}

		public override void Reset()
		{
			base.Reset();
			if ((bool)dji)
			{
				dji.Reset();
			}
			if ((bool)altitude)
			{
				altitude.Reset();
			}
			LockToCurrent();
		}

		protected override void OnPIDUpdate(PID p_pid)
		{
			if (p_pid == altitudePID)
			{
				float num = ((target == null) ? targetPosition.y : target.position.y) - base.fc.sensor.barometer.height;
				if (num >= 0f)
				{
					float y = base.fc.sensor.inertial.velocity.y;
					float p_target = Mathf.Sqrt(num * 2f * Mathf.Abs(Physics.gravity.y));
					p_pid.Update(y, p_target, deltaTime);
				}
				else
				{
					float y2 = base.fc.sensor.inertial.velocity.y;
					float p_target2 = 0f - Mathf.Sqrt(Mathf.Abs(num) * 2f * Mathf.Abs(Physics.gravity.y));
					p_pid.Update(y2, p_target2, deltaTime);
				}
			}
			else if (p_pid == yawPID)
			{
				float num2 = base.fc.sensor.gyro.local.y;
				float num3 = ((target == null) ? targetHeading : target.eulerAngles.y);
				if (num2 - num3 > 180f)
				{
					num2 -= 360f;
				}
				else if (num2 - num3 < -180f)
				{
					num3 -= 360f;
				}
				p_pid.Update(num2, num3, deltaTime);
			}
			else if (p_pid == forwardPID)
			{
				float p_current = 0f;
				float num4 = m_localTarget.z * directionScale;
				if (Mathf.Abs(num4) < allowedError)
				{
					num4 = 0f;
				}
				p_pid.Update(p_current, num4, deltaTime);
			}
			else if (p_pid == sidewaysPID)
			{
				float p_current2 = 0f;
				float num5 = m_localTarget.x * directionScale;
				if (Mathf.Abs(num5) < allowedError)
				{
					num5 = 0f;
				}
				p_pid.Update(p_current2, num5, deltaTime);
			}
		}

		public void LockToCurrent()
		{
			target = null;
			targetPosition = base.fc.drone.position;
			targetHeading = base.fc.drone.transform.eulerAngles.y;
		}

		protected override void OnUpdate()
		{
			if (dji == null)
			{
				dji = GetComponentInChildren<FCDJIProcess>(includeInactive: true);
			}
			if (altitude == null)
			{
				altitude = GetComponentInChildren<FCAltitudeProcess>(includeInactive: true);
			}
			if (dji == null || altitude == null)
			{
				return;
			}
			dji.param_maxAngle = base.fc.modeProcess.dji.param_maxAngle;
			dji.param_maxRate = base.fc.modeProcess.dji.param_maxRate;
			dji.param_maxSpeed = base.fc.modeProcess.dji.param_maxSpeed;
			dji.param_maxTrainingScale = base.fc.modeProcess.dji.param_maxTrainingScale;
			dji.param_minAngle = base.fc.modeProcess.dji.param_minAngle;
			dji.param_minSpeed = base.fc.modeProcess.dji.param_minSpeed;
			dji.param_minTrainingScale = base.fc.modeProcess.dji.param_minTrainingScale;
			altitude.speedLimit = dji.topSpeed * 0.8f;
			altitude.angleLimit = dji.angleLimit;
			dji.overrideInput = true;
			outputSignal.throttle = 0f;
			outputSignal.altitude = 0f;
			outputSignal.yaw = 0f;
			outputSignal.pitch = 0f;
			outputSignal.roll = 0f;
			Vector3 position = ((target == null) ? targetPosition : target.position);
			float num = ((target == null) ? targetHeading : target.eulerAngles.y);
			lockTarget.position = base.fc.drone.position;
			lockTarget.eulerAngles = new Vector3(0f, num, 0f);
			if (lockGlobalX)
			{
				lockTarget.position = new Vector3(position.x, lockTarget.position.y, lockTarget.position.z);
			}
			if (lockGlobalZ)
			{
				lockTarget.position = new Vector3(lockTarget.position.x, lockTarget.position.y, position.z);
			}
			if (lockLocalX)
			{
				lockTarget.Translate(lockTarget.InverseTransformPoint(position).x, 0f, 0f, Space.Self);
			}
			if (lockLocalZ)
			{
				lockTarget.Translate(0f, 0f, lockTarget.InverseTransformPoint(position).z, Space.Self);
			}
			m_localTarget = base.fc.drone.transform.InverseTransformPoint(lockTarget.position);
			if (lockAltitude)
			{
				if (!altitude.enabled)
				{
					altitude.enabled = true;
					altitude.Lock();
				}
				altitude.targetAltitude = position.y;
				outputSignal.throttle = Mathf.Clamp01(altitude.hoverThrottle);
				outputSignal.altitude = altitude.hoverThrottle;
			}
			else
			{
				altitude.enabled = false;
			}
			dji.enabled = lockHeading || lockLocalX || lockLocalZ || lockGlobalX || lockGlobalZ;
			if (lockHeading)
			{
				dji.SetHeading(num);
				base.fc.modeProcess.dji.SetHeading(num);
				outputSignal.yaw = dji.outputSignal.yaw;
			}
			if (lockLocalZ || lockGlobalX || lockGlobalZ)
			{
				dji.inputSignal.pitch = Mathf.Clamp(forwardPID.control, -1f, 1f);
				outputSignal.pitch = dji.outputSignal.pitch;
			}
			if (lockLocalX || lockGlobalX || lockGlobalZ)
			{
				dji.inputSignal.roll = Mathf.Clamp(sidewaysPID.control, -1f, 1f);
				outputSignal.roll = dji.outputSignal.roll;
			}
			dji.Loop(deltaTime);
			altitude.Loop(deltaTime);
		}
	}
}

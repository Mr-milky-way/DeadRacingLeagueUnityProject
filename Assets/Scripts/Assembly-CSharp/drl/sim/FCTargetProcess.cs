using UnityEngine;

namespace drl.sim
{
	public class FCTargetProcess : FCProcess
	{
		public float targetHeading;

		public Vector3 targetPosition;

		private Vector3 m_localTarget;

		public float allowedError = 0.1f;

		public float speedLimit;

		public float directionScale = 1f;

		public float outputScale = 1f;

		public float overrideSignalWeight = 1f;

		public Transform target;

		public SignalVector outputSignal;

		private PID altitudePID => pids[0];

		private PID yawPID => pids[1];

		private PID forwardPID => pids[2];

		private PID sidewaysPID => pids[3];

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

		public override void Reset()
		{
			base.Reset();
			LockToCurrent();
		}

		public void LockToCurrent()
		{
			target = null;
			targetPosition = base.fc.drone.position;
			targetHeading = base.fc.drone.transform.eulerAngles.y;
		}

		protected override void OnUpdate()
		{
			outputSignal.throttle = 0f;
			outputSignal.altitude = 0f;
			outputSignal.yaw = 0f;
			outputSignal.pitch = 0f;
			outputSignal.roll = 0f;
			base.fc.process.altitude.speedLimit = speedLimit * 0.8f;
			base.fc.modeProcess.dji.param_minSpeed = speedLimit;
			base.fc.modeProcess.dji.param_maxSpeed = speedLimit;
			base.fc.modeProcess.dji.angleMode = false;
			Vector3 position = ((target == null) ? targetPosition : target.position);
			m_localTarget = base.fc.drone.transform.InverseTransformPoint(position);
			if (target == null)
			{
				if (overrideSignalWeight > 0.9f)
				{
					base.fc.process.altitude.targetAltitude = position.y;
				}
				else
				{
					base.fc.process.altitude.targetAltitude = Mathf.Lerp(base.fc.process.altitude.targetAltitude, position.y, deltaTime * overrideSignalWeight * 4f);
				}
			}
			else if (overrideSignalWeight > 0.9f)
			{
				base.fc.process.altitude.target = target;
			}
			else
			{
				base.fc.process.altitude.target = null;
				base.fc.process.altitude.targetAltitude = Mathf.Lerp(base.fc.process.altitude.targetAltitude, target.position.y, deltaTime * overrideSignalWeight * 4f);
			}
			float num = base.fc.modeProcess.dji.heading;
			float num2 = ((target == null) ? targetHeading : target.eulerAngles.y);
			if (num - num2 > 180f)
			{
				num -= 360f;
			}
			else if (num - num2 < -180f)
			{
				num2 -= 360f;
			}
			if (overrideSignalWeight > 0.9f)
			{
				base.fc.modeProcess.dji.SetHeading(num2);
			}
			else
			{
				base.fc.modeProcess.dji.SetHeading(Mathf.Lerp(num, num2, deltaTime * overrideSignalWeight * 4f));
			}
			Vector2 vector = new Vector2(forwardPID.control, sidewaysPID.control);
			if (vector.sqrMagnitude > 1f)
			{
				vector.Normalize();
			}
			outputSignal.pitch = vector.x * outputScale;
			outputSignal.roll = vector.y * outputScale;
			if (speedLimit > 0f)
			{
				outputSignal.pitch *= Mathf.Clamp01(speedLimit / 15f);
				outputSignal.roll *= Mathf.Clamp01(speedLimit / 15f);
			}
			base.fc.rawSignal = SignalVector.Lerp(base.fc.rawSignal, outputSignal, overrideSignalWeight);
			if (m_localTarget.sqrMagnitude > 1f)
			{
				m_localTarget.Normalize();
			}
		}
	}
}

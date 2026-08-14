using UnityEngine;

namespace drl.sim
{
	public class FCLevelProcess : FCProcess
	{
		public float pitchOffset;

		public float rollOffset;

		public float heading;

		public float delay;

		public float rate = 400f;

		public float limit = 0.05f;

		public float inputTime;

		public bool affectYaw;

		private SignalVector m_outputSignal;

		private PID pitchPID => pids[0];

		private PID rollPID => pids[1];

		private PID yawPID => pids[2];

		public SignalVector outputSignal => m_outputSignal;

		protected override void OnPIDUpdate(PID p_pid)
		{
			if (p_pid == pitchPID)
			{
				float num = base.fc.sensor.gyro.local.x;
				if (num > 180f)
				{
					num -= 360f;
				}
				float p_target = pitchOffset;
				p_pid.Update(num, p_target, deltaTime);
			}
			else if (p_pid == rollPID)
			{
				float num2 = base.fc.sensor.gyro.local.z;
				if (num2 > 180f)
				{
					num2 -= 360f;
				}
				float p_target2 = rollOffset;
				p_pid.Update(num2, p_target2, deltaTime);
			}
			else if (p_pid == yawPID)
			{
				float num3 = base.fc.sensor.gyro.local.y;
				float num4 = heading;
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

		public override void Reset()
		{
			base.Reset();
			heading = base.fc.drone.transform.localEulerAngles.y;
		}

		protected override void OnUpdate()
		{
			if (Mathf.Abs(base.fc.rawSignal.pitch) > limit || Mathf.Abs(base.fc.rawSignal.roll) > limit)
			{
				inputTime = Time.time + delay;
			}
			if (Time.time < inputTime)
			{
				m_outputSignal.pitch = 0f;
				m_outputSignal.yaw = 0f;
				m_outputSignal.roll = 0f;
				return;
			}
			m_outputSignal.pitch = pitchPID.control * rate;
			m_outputSignal.roll = (0f - rollPID.control) * rate;
			if (affectYaw)
			{
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
				if (Mathf.Abs(base.fc.signal.yaw) > Mathf.Epsilon)
				{
					heading = base.fc.sensor.gyro.local.y + base.fc.signal.yaw * deltaTime * 3f;
					m_outputSignal.yaw = 0f;
				}
				else
				{
					m_outputSignal.yaw = yawPID.control * rate;
				}
				if (magnitude > 45f)
				{
					m_outputSignal.yaw *= Mathf.Clamp01(1f - (magnitude - 45f) / 30f);
				}
			}
			else
			{
				m_outputSignal.yaw = 0f;
			}
			for (int i = 0; i < base.fc.inputs.Count; i++)
			{
			}
		}
	}
}

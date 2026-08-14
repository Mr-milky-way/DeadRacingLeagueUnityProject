using UnityEngine;

namespace drl.sim
{
	public class FCLimiterProcess : FCProcess
	{
		public float rate = 400f;

		public float limit = 45f;

		public float lookahead = 10f;

		public float m_rollSignal;

		public float m_pitchSignal;

		public float dampenRoll;

		public float dampenPitch;

		private PID pitchPID => pids[0];

		private PID rollPID => pids[1];

		public float rollSignal => m_rollSignal;

		public float pitchSignal => m_pitchSignal;

		protected override void OnPIDUpdate(PID p_pid)
		{
			if (p_pid == pitchPID)
			{
				float num = base.fc.sensor.gyro.local.x;
				if (num > 180f)
				{
					num -= 360f;
				}
				float p_target = Mathf.Clamp(num, 0f - limit, limit);
				p_pid.Update(num, p_target, deltaTime);
			}
			else if (p_pid == rollPID)
			{
				float num2 = base.fc.sensor.gyro.local.z;
				if (num2 > 180f)
				{
					num2 -= 360f;
				}
				float p_target2 = Mathf.Clamp(num2, 0f - limit, limit);
				p_pid.Update(num2, p_target2, deltaTime);
			}
		}

		protected override void OnUpdate()
		{
			float num = ((Mathf.Abs(base.fc.profile.max.pitchRoll) <= 0f) ? 0f : (1f / base.fc.profile.max.pitchRoll));
			m_pitchSignal = pitchPID.control * num * rate / 800f;
			m_rollSignal = (0f - rollPID.control) * num * rate / 800f;
			for (int i = 0; i < base.fc.inputs.Count; i++)
			{
			}
		}

		public float DampenPitch(float p_input)
		{
			float num = base.fc.sensor.gyro.local.x;
			if (num > 180f)
			{
				num -= 360f;
			}
			dampenPitch = 1f - Mathf.Clamp01((Mathf.Abs(num) + lookahead - limit) / lookahead);
			if ((num > 0f && p_input > 0f) || (num < 0f && p_input < 0f))
			{
				return dampenPitch * p_input;
			}
			return p_input;
		}

		public float DampenRoll(float p_input)
		{
			float num = base.fc.sensor.gyro.local.z;
			if (num > 180f)
			{
				num -= 360f;
			}
			dampenRoll = 1f - Mathf.Clamp01((Mathf.Abs(num) + lookahead - limit) / lookahead);
			if ((num > 0f && p_input < 0f) || (num < 0f && p_input > 0f))
			{
				return dampenRoll * p_input;
			}
			return p_input;
		}
	}
}

using UnityEngine;

namespace drl.sim
{
	public class FCSetDirectionProcess : FCManeauverProcess
	{
		public float range = 90f;

		private PID pitchPID => pids[0];

		private PID yawPID => pids[1];

		private PID rollPID => pids[2];

		protected override void OnPIDUpdate(PID p_pid)
		{
			_ = base.fc.drone;
			if (p_pid == rollPID)
			{
				float num = base.fc.sensor.gyro.local.z;
				if (num > 180f)
				{
					num -= 360f;
				}
				float p_target = (0f - base.fc.signal.roll) / 800f * range;
				p_pid.Update(num, p_target, deltaTime);
			}
			else if (p_pid == pitchPID)
			{
				float num2 = base.fc.sensor.gyro.local.x;
				if (num2 > 180f)
				{
					num2 -= 360f;
				}
				float p_target2 = base.fc.signal.pitch / 800f * range;
				p_pid.Update(num2, p_target2, deltaTime);
			}
		}

		public override void Boot()
		{
			if (pids.Length < 3)
			{
				pids = new PID[3];
				for (int i = 0; i < 3; i++)
				{
					pids[i] = new PID();
					pids[i].constants.p = 0.5f;
					pids[i].constants.d = 0.04f;
				}
				pids[0].name = "pitch";
				pids[1].name = "yaw";
				pids[2].name = "roll";
			}
		}

		protected override void OnUpdate()
		{
			if (!(Mathf.Abs(range) <= 0f))
			{
				_ = 1f / range;
			}
		}
	}
}

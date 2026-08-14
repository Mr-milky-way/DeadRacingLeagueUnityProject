using System;
using UnityEngine;

namespace drl.sim
{
	public class FCYawProcess : FCManeauverProcess
	{
		private float p_correction = 1f;

		public override float rate => base.fc.profile.max.yaw;

		protected override void OnPIDUpdate(PID p_pid)
		{
			_ = base.fc.drone;
			PIDVector constants = p_pid.constants;
			if (p_pid.constants.p > 1f)
			{
				p_pid.constants.p *= 0.0091969995f;
				p_pid.constants.i *= 7.900838E-07f;
				p_pid.constants.d *= 1.0919618E-05f;
			}
			float p = p_pid.constants.p;
			p_pid.constants.p /= 4f;
			p_correction = base.fc.YawCorrection(p);
			float p_current = base.fc.sensor.gyro.velocity.y * ((float)Math.PI / 180f);
			float p_target = base.fc.signal.yaw * p_correction;
			p_pid.Update(p_current, p_target, deltaTime);
			p_pid.constants = constants;
		}

		protected override void OnUpdate()
		{
			if (base.fc.mode != FlightControllerMode.AcroClassic)
			{
				float num = ((Mathf.Abs(base.fc.profile.max.yaw) <= 0f) ? 0f : (1f / base.fc.profile.max.yaw));
				for (int i = 0; i < base.fc.inputs.Count; i++)
				{
					base.fc.inputs[i] += base.pid.control * layout[i] * num;
				}
			}
		}

		public override void SetLayout(FrameLayoutType p_type)
		{
			if (p_type == FrameLayoutType.QuadX)
			{
				layout = new float[4] { 1f, -1f, 1f, -1f };
			}
		}
	}
}

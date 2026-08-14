using UnityEngine;

namespace drl.sim
{
	public class FCStabilityProcess : FCProcess
	{
		public float stabilityTime;

		public AnimationCurve stabilityAuthority;

		public bool hover;

		public bool hoverOnly;

		public bool maintainAltitude;

		public float altitudeToMaintain;

		public bool speed;

		public bool stabilized;

		public float m_stabilization_angle;

		public float stabilizationAngleRange = 80f;

		public float altitudeRange = 10f;

		protected float m_hover_thrust;

		protected float m_elapsed;

		protected PID m_roll_correction;

		protected PID m_pitch_correction;

		public override void Boot()
		{
			_ = base.transform.parent;
			m_elapsed = 0f;
			m_stabilization_angle = 0f;
			m_hover_thrust = 0f;
			stabilized = false;
		}

		protected override void OnPIDUpdate(PID p_pid)
		{
			Drone drone = base.fc.drone;
			float p_dt = deltaTime;
			float num = 0f;
			float num2 = 0f;
			switch (p_pid.name)
			{
			case "roll-correction":
				num = base.fc.sensor.gyro.local.z;
				num2 = ((num > 180f) ? 360f : 0f);
				p_pid.Update(num, num2, p_dt);
				break;
			case "pitch-correction":
				num = base.fc.sensor.gyro.local.x;
				num2 = ((num > 180f) ? 360f : 0f);
				p_pid.Update(num, num2, p_dt);
				break;
			case "hover":
				num = base.fc.sensor.inertial.velocity.z;
				num2 = (hoverOnly ? 0f : (-0.2f));
				p_pid.Update(num, num2, p_dt);
				break;
			case "altitude":
				num = drone.position.y;
				num2 = altitudeToMaintain;
				p_pid.Update(num, num2, p_dt);
				break;
			case "x-speed":
			{
				Vector3 right = drone.transform.right;
				Vector3 velocityX = base.fc.sensor.inertial.velocityX;
				float z = base.fc.sensor.gyro.local.z;
				StabilizeSpeed(right, velocityX, 5f, 95f, z, base.fc.process.roll.pid, p_pid, p_dt);
				break;
			}
			case "z-speed":
			{
				Vector3 forward = drone.transform.forward;
				Vector3 velocityZ = base.fc.sensor.inertial.velocityZ;
				float x = base.fc.sensor.gyro.local.x;
				StabilizeSpeed(forward, velocityZ, -5f, 95f, x, base.fc.process.pitch.pid, p_pid, p_dt);
				break;
			}
			}
		}

		protected void StabilizeSpeed(Vector3 p_axis, Vector3 p_velocity, float p_speed_range, float p_max_angle, float p_current_angle, PID p_rpid, PID p_pid, float p_dt)
		{
			float num = Vector3.Dot(p_axis, p_velocity) / p_speed_range;
			float num2 = Mathf.Pow(Mathf.Clamp01(Mathf.Abs(num)), 2f) * p_max_angle;
			if (num < 0f)
			{
				num2 = 0f - num2;
			}
			float num3 = p_current_angle;
			num3 = ((num3 >= 180f) ? (num3 - 360f) : num3);
			float p_target = num2;
			p_pid.Update(num3, p_target, p_dt);
		}

		protected override void OnUpdate()
		{
			float num = deltaTime;
			Drone drone = base.fc.drone;
			m_elapsed += num;
			float maneauver = drone.receiver.signal.maneauver;
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
			m_stabilization_angle = local.magnitude;
			stabilized = m_stabilization_angle <= 0.2f;
			if (!hoverOnly)
			{
				if (maneauver > 0.01f)
				{
					m_elapsed = 0f;
				}
				float time = m_elapsed / stabilityTime;
				time = stabilityAuthority.Evaluate(time);
				PID pID = pids[0];
				PID pID2 = pids[1];
				PID pID3 = (maintainAltitude ? pids[5] : pids[2]);
				PID pID4 = pids[3];
				PID pID5 = pids[4];
				float num2 = 0f;
				bool flag = speed;
				time = (flag ? 1f : time);
				if (flag)
				{
					base.fc.process.roll.pid.control += pID4.control;
					num2 = 1f - Mathf.Clamp01(base.fc.sensor.inertial.velocityX.magnitude / 5f);
					base.fc.process.pitch.pid.control += pID5.control * num2;
					num2 = Mathf.Clamp01(base.fc.sensor.inertial.speed / 10f);
					base.fc.process.thrust.throttle += Mathf.Abs(pID4.control + pID5.control);
				}
				base.fc.process.pitch.pid.control += pID2.control * time;
				num2 = 1f - Mathf.Clamp01(Mathf.Abs(local.x / 8f));
				base.fc.process.roll.pid.control += pID.control * num2 * time;
				bool num3 = hover || flag;
				num2 = 1f - Mathf.Clamp01(m_stabilization_angle / 20f);
				num2 = pID3.control * num2;
				num2 = (flag ? pID3.control : num2);
				float b = (num3 ? num2 : 0f);
				m_hover_thrust = Mathf.Lerp(m_hover_thrust, b, num * 5f);
				if (num3)
				{
					base.fc.process.thrust.throttle += m_hover_thrust * time;
				}
			}
			else if (maintainAltitude)
			{
				PID pID6 = pids[5];
				PID obj = pids[2];
				float b2 = Mathf.Lerp(t: Mathf.Clamp01(Mathf.Abs(altitudeToMaintain - drone.position.y) / altitudeRange), a: obj.control, b: pID6.control) * Mathf.Clamp01(-5f * m_stabilization_angle / stabilizationAngleRange + 5f);
				m_hover_thrust = Mathf.Lerp(m_hover_thrust, b2, num * 5f);
				base.fc.process.thrust.throttle += m_hover_thrust;
			}
			else
			{
				float b3 = pids[2].control * (1f - Mathf.Clamp01(m_stabilization_angle / 45f));
				m_hover_thrust = Mathf.Lerp(m_hover_thrust, b3, num * 5f);
				base.fc.process.thrust.throttle += m_hover_thrust;
			}
		}
	}
}

using System;
using UnityEngine;

namespace drl.sim
{
	public class FCAltitudeProcess : FCProcess
	{
		public float targetAltitude;

		private RaycastHit[] hits;

		public Transform m_target;

		public bool targetOverHardSurface;

		public float thrustLerp = 5f;

		private float delta;

		public float speedLimit;

		public float lockOvershoot = 0.25f;

		public float lockUndershoot = 0.25f;

		public float gravityCompensation = 1f;

		public float m_hover_throttle;

		public float angleLimit = 22f;

		public float overshoot = 0.04f;

		public float overshootRange = 1f;

		public Transform target
		{
			get
			{
				return m_target;
			}
			set
			{
				if (m_target == value)
				{
					return;
				}
				m_target = value;
				targetOverHardSurface = false;
				if (!(m_target != null))
				{
					return;
				}
				if (hits == null || hits.Length < 20)
				{
					hits = new RaycastHit[20];
				}
				int num = Physics.RaycastNonAlloc(m_target.position + DRLPhysics.Direction.up * 0.05f, DRLPhysics.Direction.down, hits, 0.35f, DRLPhysics.Layers.Raycast_Everything, QueryTriggerInteraction.Ignore);
				for (int i = 0; i < hits.Length && i < num; i++)
				{
					RaycastHit raycastHit = hits[i];
					if (!raycastHit.transform.IsChildOf(base.fc.drone.transform) && !(raycastHit.transform.GetComponent<Drone>() != null))
					{
						targetOverHardSurface = true;
						break;
					}
				}
			}
		}

		private PID upwardsPID => pids[0];

		private PID breakPID => pids[1];

		private PID overshootPID => pids[2];

		private float clampedSpeedLimit => Mathf.Clamp(speedLimit * 0.5f, 1.6f, speedLimit);

		public float hoverThrottle => m_hover_throttle;

		public override void Reset()
		{
			base.Reset();
			Lock();
		}

		protected override void OnPIDUpdate(PID p_pid)
		{
			if (p_pid == upwardsPID)
			{
				if (delta > 0f)
				{
					float y = base.fc.sensor.inertial.velocity.y;
					float num = Mathf.Sqrt(delta * 2f * Mathf.Abs(Physics.gravity.y));
					if (speedLimit > 0f)
					{
						num = Mathf.Clamp(num, 0f - clampedSpeedLimit, clampedSpeedLimit);
					}
					p_pid.Update(y, num, deltaTime);
				}
				else
				{
					p_pid.Update(0f, 0f, deltaTime);
				}
			}
			else if (p_pid == breakPID)
			{
				if (delta > 0f)
				{
					p_pid.Update(0f, 0f, deltaTime);
					return;
				}
				float y2 = base.fc.sensor.inertial.velocity.y;
				float num2 = 0f - Mathf.Sqrt(Mathf.Abs(delta) * 2f * Mathf.Abs(Physics.gravity.y * gravityCompensation));
				if (speedLimit > 0f)
				{
					num2 = Mathf.Clamp(num2, 0f - clampedSpeedLimit, clampedSpeedLimit);
				}
				p_pid.Update(y2, num2, deltaTime);
			}
			else if (p_pid == overshootPID)
			{
				float num3 = ((target == null) ? targetAltitude : target.position.y) - base.fc.sensor.barometer.height;
				float num4 = overshoot;
				if (Mathf.Abs(num3) < ((speedLimit <= 0f) ? overshootRange : Mathf.Min(overshootRange, clampedSpeedLimit / 10f)))
				{
					num4 += num3;
				}
				p_pid.Update(overshoot, num4, deltaTime);
			}
		}

		protected override void OnUpdate()
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
			overshoot = Mathf.Clamp(overshoot + overshootPID.control, 0f, overshootRange);
			bool flag = base.fc.sensor.collision.RestingOnSurface && base.fc.normalizedSignal.altitude < Mathf.Epsilon * 2f && base.fc.normalizedSignal.throttle < Mathf.Epsilon * 2f;
			if (flag)
			{
				targetAltitude = ((target == null) ? base.fc.sensor.barometer.height : target.position.y) - 0.2f;
			}
			delta = ((target == null || flag) ? targetAltitude : target.position.y) - base.fc.sensor.barometer.height;
			delta += (flag ? 0f : overshoot);
			if (target != null && targetOverHardSurface && base.fc.sensor.barometer.height + overshoot * 2f > target.position.y && (base.fc.drone.position - target.position).sqrMagnitude < 1f)
			{
				delta = target.position.y - base.fc.sensor.barometer.height - 0.5f;
			}
			float num = ((angleLimit > 1f) ? angleLimit : 60f);
			float num2 = ((local.magnitude < num) ? (1f / Mathf.Cos(local.magnitude * ((float)Math.PI / 180f))) : (1f / Mathf.Cos(num * ((float)Math.PI / 180f)) * (1f - Mathf.Clamp01((local.magnitude - num) / num))));
			float num3 = Mathf.Clamp((upwardsPID.control + breakPID.control) * num2, -1f, 1f);
			if ((base.fc.drone.rigidbody.rb.constraints & RigidbodyConstraints.FreezePositionY) != RigidbodyConstraints.None)
			{
				num3 = Mathf.Clamp(num3, -0.25f, 0.25f);
			}
			m_hover_throttle = Mathf.Lerp(m_hover_throttle, num3, deltaTime * thrustLerp);
		}

		public void Lock()
		{
			if (base.fc.mode == FlightControllerMode.Target)
			{
				return;
			}
			target = null;
			if (base.fc.sensor == null || base.fc.sensor.inertial == null)
			{
				targetAltitude = base.fc.drone.position.y;
				return;
			}
			float y = base.fc.sensor.inertial.actualVelocity.y;
			if (y >= 0f)
			{
				targetAltitude = base.fc.drone.position.y + y * y * lockOvershoot / Mathf.Abs(Physics.gravity.y * gravityCompensation);
				return;
			}
			targetAltitude = base.fc.drone.position.y - y * y * lockUndershoot / Mathf.Abs(Physics.gravity.y * gravityCompensation);
			overshoot = 0.01f;
		}

		protected void Apply(float[] p_list)
		{
			for (int i = 0; i < base.fc.inputs.Count; i++)
			{
				base.fc.inputs[i] = p_list[i];
			}
		}

		protected void GetControls(float[] p_list)
		{
			for (int i = 0; i < base.fc.inputs.Count; i++)
			{
				p_list[i] = base.fc.inputs[i];
			}
		}

		protected string ToString(float[] p_list, string p_format)
		{
			string text = "";
			for (int i = 0; i < p_list.Length; i++)
			{
				text = text + ((i > 0) ? "," : "") + p_list[i].ToString(p_format);
			}
			return "[" + text + "]";
		}
	}
}

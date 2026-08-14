using UnityEngine;

namespace drl.sim
{
	public class FCDirectionProcess : FCProcess
	{
		private float[] m_inputs;

		private float[] m_weights;

		public Vector3 targetVelocity;

		private Vector3 localTargetVelocity;

		private Vector3 localCurrentVelocity;

		public float controlScale;

		public float yawThreshold = 10f;

		private Quaternion targetAngle = Quaternion.identity;

		public SignalVector outputSignal;

		public float[] inputs;

		private PID forwardPID => pids[0];

		private PID sidewaysPID => pids[1];

		private PID upwardsPID => pids[2];

		private PID yawPID => pids[3];

		public override void Boot()
		{
			m_inputs = new float[4];
			m_weights = new float[4];
			_ = base.transform.parent;
		}

		protected override void OnPIDUpdate(PID p_pid)
		{
			if (p_pid == forwardPID)
			{
				float z = localCurrentVelocity.z;
				float z2 = localTargetVelocity.z;
				p_pid.Update(z, z2, deltaTime);
			}
			else if (p_pid == sidewaysPID)
			{
				float x = localCurrentVelocity.x;
				float x2 = localTargetVelocity.x;
				p_pid.Update(x, x2, deltaTime);
			}
			else if (p_pid == upwardsPID)
			{
				float y = localCurrentVelocity.y;
				float y2 = localTargetVelocity.y;
				p_pid.Update(y, y2, deltaTime);
			}
			else
			{
				if (p_pid != yawPID)
				{
					return;
				}
				Vector3 forward = base.fc.drone.transform.forward;
				Vector3 fromDirection = targetVelocity;
				if (Mathf.Abs(forward.y) < 0.99f && Mathf.Abs(fromDirection.y) < 0.99f)
				{
					forward.y = 0f;
					fromDirection.y = 0f;
					float num = Quaternion.FromToRotation(fromDirection, forward).eulerAngles.y;
					if (num > 180f)
					{
						num -= 360f;
					}
					float p_current = num;
					float p_target = 0f;
					p_pid.Update(p_current, p_target, deltaTime);
				}
				else
				{
					float p_current2 = 0f;
					float p_target2 = 0f;
					p_pid.Update(p_current2, p_target2, deltaTime);
				}
			}
		}

		protected override void OnUpdate()
		{
			localTargetVelocity = base.fc.drone.transform.InverseTransformDirection(targetVelocity);
			localCurrentVelocity = base.fc.drone.transform.InverseTransformDirection(base.fc.sensor.inertial.velocity);
			outputSignal = base.fc.rawSignal;
			outputSignal.yaw += yawPID.control * controlScale;
			if (Mathf.Abs(yawPID.control) < yawThreshold)
			{
				outputSignal.pitch += forwardPID.control * controlScale;
				outputSignal.roll += sidewaysPID.control * controlScale;
			}
			base.fc.rawSignal = outputSignal;
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

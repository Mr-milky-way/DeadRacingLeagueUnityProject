using System;
using UnityEngine;

namespace drl.sim
{
	public class FCArcadeProcess : FCProcess
	{
		private float[] m_inputs;

		private float[] m_weights;

		[NonSerialized]
		public float rateScale = 1f;

		public AnimationCurve thrustTrainingCurve;

		private Quaternion targetAngle = Quaternion.identity;

		private Vector3 deltaRotation;

		private PID pitchPID => pids[0];

		private PID yawPID => pids[1];

		private PID rollPID => pids[2];

		public override void Boot()
		{
			m_inputs = new float[4];
			m_weights = new float[4];
			_ = base.transform.parent;
		}

		public override void SetLayout(FrameLayoutType p_type)
		{
			if (p_type == FrameLayoutType.QuadX)
			{
				m_inputs = new float[4];
				m_weights = new float[4];
			}
		}

		protected override void OnPIDUpdate(PID p_pid)
		{
			float p_current = 0f;
			float num = 0f;
			num = ((p_pid == pitchPID) ? deltaRotation.x : ((p_pid != yawPID) ? deltaRotation.z : deltaRotation.y));
			p_pid.Update(p_current, num, deltaTime);
		}

		protected override void OnUpdate()
		{
			deltaRotation = Vector3.zero;
			deltaRotation.x = base.fc.profile.max.pitch * deltaTime * base.fc.signal.pitch * 0.03f * rateScale;
			deltaRotation.y = base.fc.profile.max.yaw * deltaTime * base.fc.signal.yaw * 0.003f * rateScale;
			deltaRotation.z = (0f - base.fc.profile.max.roll) * deltaTime * base.fc.signal.roll * 0.02f * rateScale;
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

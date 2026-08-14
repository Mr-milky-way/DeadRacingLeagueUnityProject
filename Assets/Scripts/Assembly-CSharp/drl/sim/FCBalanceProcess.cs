using System.Collections.Generic;
using UnityEngine;

namespace drl.sim
{
	public class FCBalanceProcess : FCProcess
	{
		public AnimationCurve yawAuthority;

		public bool yawInputAuthority = true;

		public bool balanced;

		public float torqueSum;

		private float[] m_inputs;

		private float[] m_weights;

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

		protected override void OnUpdate()
		{
			Drone drone = base.fc.drone;
			ValidateNaN(m_inputs, "FCBalanceProcess> OnUpdate [1] - NaN Detected!");
			GetControls(m_inputs);
			Rail(m_inputs);
			Fit01(m_inputs);
			float num = 0f;
			num = ((!yawInputAuthority) ? Mathf.Clamp01(Mathf.Abs(base.fc.sensor.gyro.velocity.y / 90f)) : Mathf.Abs(base.fc.rawSignal.yaw));
			num = Mathf.Clamp01(yawAuthority.Evaluate(num));
			ValidateNaN(m_inputs, "FCBalanceProcess> OnUpdate [2] - NaN Detected!");
			if (num >= 1f)
			{
				Apply(m_inputs);
				return;
			}
			float num2 = 0f;
			num2 = GetTorqueSum(m_inputs);
			num2 = (torqueSum = num2 * (1f - num));
			ValidateNaN(m_inputs, "FCBalanceProcess> OnUpdate [3] - NaN Detected!");
			if (balanced = Mathf.Abs(num2) <= 2f * Mathf.Epsilon)
			{
				Apply(m_inputs);
				return;
			}
			ValidateNaN(m_inputs, "FCBalanceProcess> OnUpdate [4] - NaN Detected!");
			GetWeights(m_inputs, m_weights);
			ValidateNaN(m_inputs, "FCBalanceProcess> OnUpdate [5] - NaN Detected!");
			ValidateNaN(m_weights, "FCBalanceProcess> OnUpdate Weights [1] - NaN Detected!");
			Balance(drone, num2, m_inputs, m_weights);
			ValidateNaN(m_inputs, "FCBalanceProcess> OnUpdate [5] - NaN Detected!");
			Apply(m_inputs);
			ValidateNaN(m_inputs, "FCBalanceProcess> OnUpdate [6] - NaN Detected!");
		}

		protected void ValidateNaN(float[] p_list, string p_message)
		{
			bool flag = false;
			for (int i = 0; i < p_list.Length; i++)
			{
				if (float.IsNaN(p_list[i]))
				{
					flag = true;
					p_list[i] = 0f;
				}
			}
			if (flag)
			{
				Debug.LogError(p_message);
			}
		}

		protected void Apply(float[] p_list)
		{
		}

		protected void GetControls(float[] p_list)
		{
			if (base.fc.mode == FlightControllerMode.AcroClassic)
			{
				for (int i = 0; i < base.fc.inputs.Count; i++)
				{
					float num = 0f;
					if (base.fc.process.thrust.enabled)
					{
						num += base.fc.process.thrust.throttle;
					}
					FCManeauverProcess yaw = base.fc.process.yaw;
					if (yaw.enabled)
					{
						num += yaw.pid.control * yaw.layout[i] * yaw.irate;
					}
					yaw = base.fc.process.pitch;
					if (yaw.enabled)
					{
						num += yaw.pid.control * yaw.layout[i] * yaw.irate;
					}
					yaw = base.fc.process.roll;
					if (yaw.enabled)
					{
						num += yaw.pid.control * yaw.layout[i] * yaw.irate;
					}
					p_list[i] = num;
				}
			}
			else
			{
				for (int j = 0; j < base.fc.inputs.Count; j++)
				{
					p_list[j] = base.fc.inputs[j];
				}
			}
		}

		protected void Lerp(float[] a, float[] b, float r)
		{
			for (int i = 0; i < a.Length; i++)
			{
				a[i] = Mathf.Lerp(a[i], b[i], r);
			}
		}

		protected void Rail(float[] p_list)
		{
			Vector4 vector = MinMaxLenMid(p_list);
			float num = 0f;
			if (vector.x < 0f)
			{
				num = 0f - vector.x;
			}
			else if (vector.y > 1f)
			{
				num = 0f - (vector.y - 1f);
			}
			if (Mathf.Abs(num) > 0f)
			{
				Offset(p_list, num);
			}
		}

		protected void GetWeights(float[] p_list, float[] p_weights, float p_force = -1f)
		{
			if (p_force >= 0f)
			{
				for (int i = 0; i < p_weights.Length; i++)
				{
					p_weights[i] = p_force;
				}
				return;
			}
			float num = Sum(p_list);
			float num2 = (((double)Mathf.Abs(num) < 1E-12) ? 0f : (1f / num));
			for (int j = 0; j < p_list.Length; j++)
			{
				p_weights[j] = p_list[j] * num2;
			}
		}

		protected float GetTorqueSum(float[] p_list)
		{
			List<DroneESC> escs = base.fc.drone.body.frame.escs;
			float num = 0f;
			for (int i = 0; i < escs.Count; i++)
			{
				num += (escs[i].motor.ccw ? (0f - p_list[i]) : p_list[i]);
			}
			return num;
		}

		protected Vector4 MinMaxLenMid(float[] p_list)
		{
			Vector4 result = new Vector2(p_list[0], p_list[0]);
			for (int i = 1; i < p_list.Length; i++)
			{
				result.x = Mathf.Min(result.x, p_list[i]);
				result.y = Mathf.Max(result.y, p_list[i]);
			}
			result.z = result.y - result.x;
			result.w = (result.x + result.y) * 0.5f;
			return result;
		}

		protected float Sum(float[] p_list)
		{
			float num = 0f;
			for (int i = 0; i < p_list.Length; i++)
			{
				num += p_list[i];
			}
			return num;
		}

		protected void Copy(float[] a, float[] b)
		{
			for (int i = 0; i < a.Length; i++)
			{
				b[i] = a[i];
			}
		}

		protected void Clamp01(float[] p_list)
		{
			for (int i = 0; i < p_list.Length; i++)
			{
				p_list[i] = Mathf.Clamp01(p_list[i]);
			}
		}

		protected bool Balance(Drone p_drone, float p_sum, float[] p_list, float[] p_weights)
		{
			List<DroneESC> escs = base.fc.drone.body.frame.escs;
			bool result = true;
			for (int i = 0; i < escs.Count; i++)
			{
				bool ccw = escs[i].motor.ccw;
				float num = p_sum * p_weights[i];
				p_list[i] += (ccw ? num : (0f - num));
				if (p_list[i] > 1f)
				{
					result = false;
				}
				else if (p_list[i] < 0f)
				{
					result = false;
				}
			}
			return result;
		}

		protected void Fit01(float[] p_list)
		{
			Vector4 vector = MinMaxLenMid(p_list);
			float z = vector.z;
			if (!(z < 1f))
			{
				float w = vector.w;
				float num = 1f / z;
				for (int i = 0; i < p_list.Length; i++)
				{
					p_list[i] = (p_list[i] - w) * num + 0.5f;
				}
			}
		}

		protected void Offset(float[] p_list, float p_off)
		{
			for (int i = 0; i < p_list.Length; i++)
			{
				p_list[i] += p_off;
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

using System;
using UnityEngine;

namespace drl.sim
{
	public class DroneMixer : MonoBehaviour
	{
		[Serializable]
		public class DroneManeauverControl
		{
			public PID pid;

			public float target;

			public float current;

			public float stability = 1f;

			public float[] layout = new float[4] { 1f, 1f, 1f, 1f };
		}

		public DronePhysics sim;

		public DroneManeauverControl yaw;

		public DroneManeauverControl pitch;

		public DroneManeauverControl roll;

		public AnimationCurve yawAuthority;

		public bool yawInputAuthority = true;

		private float[] m_signals;

		private float[] m_weights;

		protected void Awake()
		{
			m_signals = new float[4];
			m_weights = new float[4];
			_ = base.transform.parent;
			sim = GetComponent<DronePhysics>();
		}

		protected void UpdateYaw(Drone p_drone, PID p_pid, float p_dt)
		{
			yaw.current = p_drone.fc.sensor.gyro.velocity.y;
			PIDVector constants = p_pid.constants;
			if (!p_drone.isThreaded)
			{
				p_pid.constants.p += p_drone.profile.rollFF;
			}
			if (p_drone.fc.mode == FlightControllerMode.Beginner || p_drone.fc.mode == FlightControllerMode.DJI || p_drone.fc.mode == FlightControllerMode.Target)
			{
				p_pid.constants.p *= 0.25f;
			}
			p_pid.constants.p *= 0.0091969995f * (p_drone.d_debugPID ? 1.1f : ((p_pid.constants.p < 60f) ? 1f : Mathf.Lerp(1f, 1.3f, (p_pid.constants.p - 60f) / 80f)));
			p_pid.constants.i *= 7.900838E-07f;
			p_pid.constants.d *= 1.0919618E-05f * (p_drone.d_debugPID ? 0.5f : ((p_pid.constants.d < 80f) ? 0.5f : Mathf.Lerp(0.5f, 1f, (p_pid.constants.d - 80f) / 50f)));
			float p = p_pid.constants.p;
			if (p_drone.hasPhysics && p_drone.physics.realisticTorque)
			{
				p_pid.constants.p *= 2.5f;
			}
			else
			{
				p_pid.constants.p /= 4f;
			}
			float p_target = p_drone.fc.signal.yaw * p_drone.fc.YawCorrection(p);
			p_pid.Update(yaw.current, p_target, p_dt);
			p_pid.constants = constants;
			yaw.target = p_drone.fc.signal.yaw;
		}

		protected void UpdateRoll(Drone p_drone, PID p_pid, float p_dt)
		{
			roll.current = p_drone.fc.sensor.gyro.velocity.z;
			PIDVector constants = p_pid.constants;
			if (!p_drone.isThreaded)
			{
				p_pid.constants.p += p_drone.profile.rollFF * 0.5f;
				p_pid.constants.d += p_drone.profile.rollFF * 0.5f;
			}
			roll.stability = (p_pid.constants.d + 20f) / p_pid.constants.p;
			p_pid.constants.p *= 0.0091969995f * Mathf.Lerp(5f, (p_pid.constants.p < 80f) ? 0.25f : Mathf.Lerp(0.25f, 1f, (p_pid.constants.p - 80f) / 50f), roll.stability);
			p_pid.constants.i *= 7.900838E-07f;
			p_pid.constants.d *= 1.0919618E-05f * Mathf.Lerp(5f, (p_pid.constants.d < 80f) ? 0.5f : Mathf.Lerp(0.5f, 1f, (p_pid.constants.d - 80f) / 50f), roll.stability);
			float p_target = (0f - p_drone.fc.signal.roll) * p_drone.fc.RollCorrection(p_pid.constants.p);
			p_pid.Update(roll.current, p_target, p_dt);
			p_pid.constants = constants;
			roll.target = 0f - p_drone.fc.signal.roll;
		}

		protected void UpdatePitch(Drone p_drone, PID p_pid, float p_dt)
		{
			pitch.current = p_drone.fc.sensor.gyro.velocity.x;
			PIDVector constants = p_pid.constants;
			if (!p_drone.isThreaded)
			{
				p_pid.constants.p += p_drone.profile.pitchFF * 0.5f;
				p_pid.constants.d += p_drone.profile.pitchFF * 0.5f;
			}
			pitch.stability = (p_pid.constants.d + 20f) / p_pid.constants.p;
			p_pid.constants.p *= 0.0091969995f * Mathf.Lerp(5f, (p_pid.constants.p < 80f) ? 0.25f : Mathf.Lerp(0.25f, 1f, (p_pid.constants.p - 80f) / 50f), pitch.stability);
			p_pid.constants.i *= 7.900838E-07f;
			p_pid.constants.d *= 1.0919618E-05f * Mathf.Lerp(5f, (p_pid.constants.d < 80f) ? 0.5f : Mathf.Lerp(0.5f, 1f, (p_pid.constants.d - 80f) / 50f), pitch.stability);
			float p_target = p_drone.fc.signal.pitch * p_drone.fc.PitchCorrection(p_pid.constants.p);
			p_pid.Update(pitch.current, p_target, p_dt);
			p_pid.constants = constants;
		}

		public void OnUpdate(Drone p_drone, float p_dt)
		{
			if (p_drone == null)
			{
				Debug.LogError("DroneMixer> OnUpdate: sim.Drone is null");
				return;
			}
			UpdateYaw(p_drone, yaw.pid, p_dt);
			UpdateRoll(p_drone, roll.pid, p_dt);
			UpdatePitch(p_drone, pitch.pid, p_dt);
			if (ValidateNaN(m_signals, "DroneBalanceControl> OnUpdate [1] - NaN Detected!"))
			{
				p_drone.FixNaN();
				return;
			}
			GetControls(p_drone, m_signals);
			if (!p_drone.hasFc && p_drone.fc == null)
			{
				Debug.LogError("DroneMixer> OnUpdate: Drone flight controller missing");
				return;
			}
			if (!p_drone.hasPhysics && p_drone.physics == null)
			{
				Debug.LogError("DroneMixer> OnUpdate: Drone physics missing");
				return;
			}
			Rail(m_signals, p_drone.fc.minSignal);
			Fit01(m_signals);
			float num = 0f;
			if (yawInputAuthority)
			{
				num = Mathf.Abs(p_drone.fc.InverseTransformSignal(p_drone.fc.signal).yaw);
				if (p_drone.physics != null && p_drone.physics.realisticTorque)
				{
					SignalVector p_signal = new SignalVector
					{
						yaw = yaw.pid.control
					};
					num = Mathf.Abs(p_drone.fc.InverseTransformSignal(p_signal).yaw);
				}
			}
			else
			{
				num = Mathf.Clamp01(Mathf.Abs(p_drone.fc.sensor.gyro.velocity.y / 90f));
			}
			num = Mathf.Clamp01(yawAuthority.Evaluate(num));
			if (ValidateNaN(m_signals, "DroneBalanceControl> OnUpdate [2] - NaN Detected!"))
			{
				p_drone.FixNaN();
				return;
			}
			if (num >= 1f)
			{
				Apply(p_drone, m_signals);
				return;
			}
			float num2 = 0f;
			SignalToTorque(p_drone, m_signals);
			num2 = GetTorqueSum(p_drone, m_signals);
			num2 *= 1f - num;
			if (ValidateNaN(m_signals, "DroneBalanceControl> OnUpdate [3] - NaN Detected!"))
			{
				p_drone.FixNaN();
				return;
			}
			if (Mathf.Abs(num2) <= 2f * Mathf.Epsilon)
			{
				TorqueToSignal(p_drone, m_signals);
				Apply(p_drone, m_signals);
				return;
			}
			if (ValidateNaN(m_signals, "DroneBalanceControl> OnUpdate [4] - NaN Detected!"))
			{
				p_drone.FixNaN();
				return;
			}
			GetWeights(m_signals, m_weights);
			if (ValidateNaN(m_signals, "DroneBalanceControl> OnUpdate [5] - NaN Detected!"))
			{
				p_drone.FixNaN();
				return;
			}
			if (ValidateNaN(m_weights, "DroneBalanceControl> OnUpdate Weights [1] - NaN Detected!"))
			{
				p_drone.FixNaN();
				return;
			}
			Balance(p_drone, num2, m_signals, m_weights);
			if (ValidateNaN(m_signals, "DroneBalanceControl> OnUpdate [5] - NaN Detected!"))
			{
				p_drone.FixNaN();
				return;
			}
			SignalToTorque(p_drone, m_signals);
			Apply(p_drone, m_signals);
			if (ValidateNaN(m_signals, "DroneBalanceControl> OnUpdate [6] - NaN Detected!"))
			{
				p_drone.FixNaN();
			}
		}

		public void SignalToTorque(Drone p_drone, float[] p_list)
		{
			if (p_drone.physics.torqueBoost && !p_drone.physics.linearTorque)
			{
				for (int i = 0; i < p_list.Length; i++)
				{
					p_list[i] = sim.BalanceSignalToTorque(p_list[i]);
				}
			}
		}

		public void TorqueToSignal(Drone p_drone, float[] p_list)
		{
			if (p_drone.physics.torqueBoost && !p_drone.physics.linearTorque)
			{
				for (int i = 0; i < p_list.Length; i++)
				{
					p_list[i] = sim.BalanceTorqueToSignal(p_list[i]);
				}
			}
		}

		protected bool ValidateNaN(float[] p_list, string p_message)
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
			return flag;
		}

		protected void Apply(Drone p_drone, float[] p_list)
		{
			for (int i = 0; i < p_drone.body.frame.escs.Count; i++)
			{
				p_drone.body.frame.escs[i].legacyInput = p_list[i];
			}
		}

		protected void GetControls(Drone p_drone, float[] p_list)
		{
			for (int i = 0; i < p_drone.body.frame.escs.Count; i++)
			{
				float throttle = p_drone.fc.signal.throttle;
				throttle += yaw.pid.control * yaw.layout[i] / 2000f;
				throttle += pitch.pid.control * pitch.layout[i] / 2000f;
				throttle += roll.pid.control * roll.layout[i] / 2000f;
				p_list[i] = throttle;
			}
		}

		protected void Lerp(float[] a, float[] b, float r)
		{
			for (int i = 0; i < a.Length; i++)
			{
				a[i] = Mathf.Lerp(a[i], b[i], r);
			}
		}

		protected void Rail(float[] p_list, float p_min = 0f)
		{
			Vector4 vector = MinMaxLenMid(p_list);
			float num = 0f;
			if (vector.x < p_min)
			{
				num = 0f - vector.x + p_min;
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

		protected float GetTorqueSum(Drone p_drone, float[] p_list)
		{
			float num = 0f;
			for (int i = 0; i < p_drone.body.frame.escs.Count; i++)
			{
				num += (p_drone.body.frame.escs[i].motor.ccw ? (0f - p_list[i]) : p_list[i]);
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
			bool result = true;
			for (int i = 0; i < p_drone.body.frame.escs.Count; i++)
			{
				bool ccw = p_drone.body.frame.escs[i].motor.ccw;
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
			if (z < 1f)
			{
				return;
			}
			float w = vector.w;
			bool flag = (double)Mathf.Abs(z) <= 1E-08;
			if (flag)
			{
				Debug.LogWarning("DroneBalanceControl> Fit01 / length is zero, potential NaN - 'return'");
				return;
			}
			float num = (flag ? 0f : (1f / z));
			for (int i = 0; i < p_list.Length; i++)
			{
				p_list[i] = (p_list[i] - w) * num + 0.5f;
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

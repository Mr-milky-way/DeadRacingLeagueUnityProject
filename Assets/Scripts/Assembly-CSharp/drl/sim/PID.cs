using System;
using UnityEngine;

namespace drl.sim
{
	[Serializable]
	public class PID
	{
		public string name = "pid";

		public PIDVector constants;

		public PIDVector error;

		public PIDVector gain;

		public float guard;

		public float control { get; set; }

		public float target { get; set; }

		public float current { get; set; }

		public void Update(float p_current, float p_target, float p_dt)
		{
			float num = ((Mathf.Abs(p_dt) <= 0f) ? 0f : (1f / p_dt));
			current = p_current;
			target = p_target;
			float num2 = p_target - p_current;
			float p = error.p;
			float num3 = num2 - p;
			error.p = num2;
			error.i += error.p * p_dt;
			error.d = num3 * num;
			gain.p = constants.p * error.p;
			gain.i = constants.i * error.i;
			gain.d = constants.d * error.d;
			control = gain.p + gain.i + gain.d;
		}

		public void Reset()
		{
			gain.p = 0f;
			gain.d = 0f;
			gain.i = 0f;
			error.p = 0f;
			error.d = 0f;
			error.i = 0f;
			control = 0f;
			target = 0f;
			current = 0f;
		}
	}
}

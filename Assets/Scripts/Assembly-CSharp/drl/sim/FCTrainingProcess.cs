using System;
using UnityEngine;

namespace drl.sim
{
	public class FCTrainingProcess : FCProcess
	{
		[NonSerialized]
		public float scale = 1f;

		public AnimationCurve thrustCurve;

		public SignalVector TransfromSignal(SignalVector p_signal)
		{
			return new SignalVector
			{
				throttle = Mathf.Lerp(thrustCurve.Evaluate(p_signal.throttle), p_signal.throttle, scale),
				altitude = p_signal.altitude * scale,
				yaw = p_signal.yaw * 1.4f,
				pitch = p_signal.pitch * scale,
				roll = p_signal.roll * scale
			};
		}
	}
}

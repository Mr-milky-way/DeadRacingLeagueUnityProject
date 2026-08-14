using System;
using UnityEngine;

namespace drl.sim.rci
{
	[Serializable]
	public class AssignedAxisData
	{
		public int ElementID;

		public RawAxis rawAxis;

		public AssignedAxis assignedAxis;

		[Range(-1f, 1f)]
		public float center;

		[Range(-1f, 1f)]
		public float min;

		[Range(-1f, 1f)]
		public float max;

		public float zeroThrottle = -2f;

		public bool inverted;

		[Range(0f, 1f)]
		public float deadzone;
	}
}

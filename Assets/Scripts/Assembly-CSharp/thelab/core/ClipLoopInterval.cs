using System;

namespace thelab.core
{
	[Serializable]
	public struct ClipLoopInterval
	{
		public bool time;

		public float start;

		public float end;

		public int count;

		public bool pingpong;
	}
}

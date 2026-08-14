using System;

namespace drl
{
	[Serializable]
	public class Packet
	{
		public string name;

		public ulong id;

		public object data;

		public float rtt;
	}
}

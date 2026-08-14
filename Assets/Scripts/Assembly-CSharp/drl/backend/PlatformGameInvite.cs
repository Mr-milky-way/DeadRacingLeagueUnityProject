using System;

namespace drl.backend
{
	[Serializable]
	public class PlatformGameInvite
	{
		public string from;

		public string to;

		public string region;

		public string room;

		public string args;
	}
}

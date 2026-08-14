using System;

namespace drl.backend
{
	[Serializable]
	public class SteamEventData
	{
		public SteamEventType type;

		public SteamService target;
	}
}

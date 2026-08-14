using System;
using Epic.OnlineServices.AntiCheatCommon;

namespace Epic.OnlineServices.AntiCheatClient
{
	public class RegisterPeerOptions
	{
		public IntPtr PeerHandle { get; set; }

		public AntiCheatCommonClientType ClientType { get; set; }

		public AntiCheatCommonClientPlatform ClientPlatform { get; set; }

		public string AccountId { get; set; }

		public string IpAddress { get; set; }
	}
}

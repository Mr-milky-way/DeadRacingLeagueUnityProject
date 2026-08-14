using System;
using Epic.OnlineServices.AntiCheatCommon;

namespace Epic.OnlineServices.AntiCheatServer
{
	public class RegisterClientOptions
	{
		public IntPtr ClientHandle { get; set; }

		public AntiCheatCommonClientType ClientType { get; set; }

		public AntiCheatCommonClientPlatform ClientPlatform { get; set; }

		public string AccountId { get; set; }

		public string IpAddress { get; set; }
	}
}

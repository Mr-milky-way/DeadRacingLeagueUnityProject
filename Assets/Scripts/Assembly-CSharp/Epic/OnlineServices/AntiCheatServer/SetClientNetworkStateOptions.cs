using System;

namespace Epic.OnlineServices.AntiCheatServer
{
	public class SetClientNetworkStateOptions
	{
		public IntPtr ClientHandle { get; set; }

		public bool IsNetworkActive { get; set; }
	}
}

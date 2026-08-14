using System;

namespace Epic.OnlineServices.AntiCheatServer
{
	public class ReceiveMessageFromClientOptions
	{
		public IntPtr ClientHandle { get; set; }

		public byte[] Data { get; set; }
	}
}

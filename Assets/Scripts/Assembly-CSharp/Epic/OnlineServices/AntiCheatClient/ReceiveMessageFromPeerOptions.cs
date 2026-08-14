using System;

namespace Epic.OnlineServices.AntiCheatClient
{
	public class ReceiveMessageFromPeerOptions
	{
		public IntPtr PeerHandle { get; set; }

		public byte[] Data { get; set; }
	}
}

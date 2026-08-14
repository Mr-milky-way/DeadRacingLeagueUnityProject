using System;

namespace Epic.OnlineServices.AntiCheatServer
{
	public class ProtectMessageOptions
	{
		public IntPtr ClientHandle { get; set; }

		public byte[] Data { get; set; }

		public uint OutBufferSizeBytes { get; set; }
	}
}

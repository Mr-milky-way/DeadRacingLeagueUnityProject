using System;

namespace Epic.OnlineServices.AntiCheatServer
{
	public class UnprotectMessageOptions
	{
		public IntPtr ClientHandle { get; set; }

		public byte[] Data { get; set; }

		public uint OutBufferSizeBytes { get; set; }
	}
}

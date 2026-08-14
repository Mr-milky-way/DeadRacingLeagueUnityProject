using System;

namespace Epic.OnlineServices.Platform
{
	public class InitializeOptions
	{
		public IntPtr AllocateMemoryFunction { get; set; }

		public IntPtr ReallocateMemoryFunction { get; set; }

		public IntPtr ReleaseMemoryFunction { get; set; }

		public string ProductName { get; set; }

		public string ProductVersion { get; set; }

		public IntPtr SystemInitializeOptions { get; set; }

		public InitializeThreadAffinity OverrideThreadAffinity { get; set; }
	}
}

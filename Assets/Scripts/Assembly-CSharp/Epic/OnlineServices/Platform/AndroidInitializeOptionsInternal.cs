using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Platform
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct AndroidInitializeOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_AllocateMemoryFunction;

		private IntPtr m_ReallocateMemoryFunction;

		private IntPtr m_ReleaseMemoryFunction;

		private IntPtr m_ProductName;

		private IntPtr m_ProductVersion;

		private IntPtr m_Reserved;

		private IntPtr m_SystemInitializeOptions;

		private IntPtr m_OverrideThreadAffinity;

		public IntPtr AllocateMemoryFunction
		{
			set
			{
				m_AllocateMemoryFunction = value;
			}
		}

		public IntPtr ReallocateMemoryFunction
		{
			set
			{
				m_ReallocateMemoryFunction = value;
			}
		}

		public IntPtr ReleaseMemoryFunction
		{
			set
			{
				m_ReleaseMemoryFunction = value;
			}
		}

		public string ProductName
		{
			set
			{
				Helper.TryMarshalSet(ref m_ProductName, value);
			}
		}

		public string ProductVersion
		{
			set
			{
				Helper.TryMarshalSet(ref m_ProductVersion, value);
			}
		}

		public IntPtr Reserved
		{
			set
			{
				m_Reserved = value;
			}
		}

		public AndroidInitializeOptionsSystemInitializeOptions SystemInitializeOptions
		{
			set
			{
				Helper.TryMarshalSet<AndroidInitializeOptionsSystemInitializeOptionsInternal, AndroidInitializeOptionsSystemInitializeOptions>(ref m_SystemInitializeOptions, value);
			}
		}

		public InitializeThreadAffinity OverrideThreadAffinity
		{
			set
			{
				Helper.TryMarshalSet<InitializeThreadAffinityInternal, InitializeThreadAffinity>(ref m_OverrideThreadAffinity, value);
			}
		}

		public void Set(AndroidInitializeOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 4;
				AllocateMemoryFunction = other.AllocateMemoryFunction;
				ReallocateMemoryFunction = other.ReallocateMemoryFunction;
				ReleaseMemoryFunction = other.ReleaseMemoryFunction;
				ProductName = other.ProductName;
				ProductVersion = other.ProductVersion;
				Reserved = other.Reserved;
				SystemInitializeOptions = other.SystemInitializeOptions;
				OverrideThreadAffinity = other.OverrideThreadAffinity;
			}
		}

		public void Set(object other)
		{
			Set(other as AndroidInitializeOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_AllocateMemoryFunction);
			Helper.TryMarshalDispose(ref m_ReallocateMemoryFunction);
			Helper.TryMarshalDispose(ref m_ReleaseMemoryFunction);
			Helper.TryMarshalDispose(ref m_ProductName);
			Helper.TryMarshalDispose(ref m_ProductVersion);
			Helper.TryMarshalDispose(ref m_Reserved);
			Helper.TryMarshalDispose(ref m_SystemInitializeOptions);
			Helper.TryMarshalDispose(ref m_OverrideThreadAffinity);
		}
	}
}

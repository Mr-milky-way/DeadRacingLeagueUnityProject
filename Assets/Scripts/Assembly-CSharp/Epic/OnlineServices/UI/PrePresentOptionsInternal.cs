using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.UI
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct PrePresentOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_PlatformSpecificData;

		public IntPtr PlatformSpecificData
		{
			set
			{
				m_PlatformSpecificData = value;
			}
		}

		public void Set(PrePresentOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				PlatformSpecificData = other.PlatformSpecificData;
			}
		}

		public void Set(object other)
		{
			Set(other as PrePresentOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_PlatformSpecificData);
		}
	}
}

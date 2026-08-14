using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.UI
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct ReportKeyEventOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_PlatformSpecificInputData;

		public IntPtr PlatformSpecificInputData
		{
			set
			{
				m_PlatformSpecificInputData = value;
			}
		}

		public void Set(ReportKeyEventOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				PlatformSpecificInputData = other.PlatformSpecificInputData;
			}
		}

		public void Set(object other)
		{
			Set(other as ReportKeyEventOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_PlatformSpecificInputData);
		}
	}
}

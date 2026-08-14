using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct StartSessionOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_SessionName;

		public string SessionName
		{
			set
			{
				Helper.TryMarshalSet(ref m_SessionName, value);
			}
		}

		public void Set(StartSessionOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				SessionName = other.SessionName;
			}
		}

		public void Set(object other)
		{
			Set(other as StartSessionOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_SessionName);
		}
	}
}

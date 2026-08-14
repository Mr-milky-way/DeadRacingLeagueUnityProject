using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SessionModificationSetHostAddressOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_HostAddress;

		public string HostAddress
		{
			set
			{
				Helper.TryMarshalSet(ref m_HostAddress, value);
			}
		}

		public void Set(SessionModificationSetHostAddressOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				HostAddress = other.HostAddress;
			}
		}

		public void Set(object other)
		{
			Set(other as SessionModificationSetHostAddressOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_HostAddress);
		}
	}
}

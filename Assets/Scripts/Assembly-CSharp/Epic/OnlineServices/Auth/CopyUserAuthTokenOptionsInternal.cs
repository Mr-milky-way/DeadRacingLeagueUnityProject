using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyUserAuthTokenOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		public void Set(CopyUserAuthTokenOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyUserAuthTokenOptions);
		}

		public void Dispose()
		{
		}
	}
}

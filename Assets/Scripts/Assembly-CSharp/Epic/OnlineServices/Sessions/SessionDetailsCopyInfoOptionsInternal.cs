using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SessionDetailsCopyInfoOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		public void Set(SessionDetailsCopyInfoOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
			}
		}

		public void Set(object other)
		{
			Set(other as SessionDetailsCopyInfoOptions);
		}

		public void Dispose()
		{
		}
	}
}

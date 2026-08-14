using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SessionSearchGetSearchResultCountOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		public void Set(SessionSearchGetSearchResultCountOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
			}
		}

		public void Set(object other)
		{
			Set(other as SessionSearchGetSearchResultCountOptions);
		}

		public void Dispose()
		{
		}
	}
}

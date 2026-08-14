using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SessionSearchCopySearchResultByIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_SessionIndex;

		public uint SessionIndex
		{
			set
			{
				m_SessionIndex = value;
			}
		}

		public void Set(SessionSearchCopySearchResultByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				SessionIndex = other.SessionIndex;
			}
		}

		public void Set(object other)
		{
			Set(other as SessionSearchCopySearchResultByIndexOptions);
		}

		public void Dispose()
		{
		}
	}
}

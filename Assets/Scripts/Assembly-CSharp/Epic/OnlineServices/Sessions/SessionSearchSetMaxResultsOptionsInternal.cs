using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SessionSearchSetMaxResultsOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_MaxSearchResults;

		public uint MaxSearchResults
		{
			set
			{
				m_MaxSearchResults = value;
			}
		}

		public void Set(SessionSearchSetMaxResultsOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				MaxSearchResults = other.MaxSearchResults;
			}
		}

		public void Set(object other)
		{
			Set(other as SessionSearchSetMaxResultsOptions);
		}

		public void Dispose()
		{
		}
	}
}

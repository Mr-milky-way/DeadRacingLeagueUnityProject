using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CreateLobbySearchOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_MaxResults;

		public uint MaxResults
		{
			set
			{
				m_MaxResults = value;
			}
		}

		public void Set(CreateLobbySearchOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				MaxResults = other.MaxResults;
			}
		}

		public void Set(object other)
		{
			Set(other as CreateLobbySearchOptions);
		}

		public void Dispose()
		{
		}
	}
}

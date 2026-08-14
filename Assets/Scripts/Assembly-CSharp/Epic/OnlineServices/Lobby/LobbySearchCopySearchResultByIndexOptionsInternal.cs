using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LobbySearchCopySearchResultByIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_LobbyIndex;

		public uint LobbyIndex
		{
			set
			{
				m_LobbyIndex = value;
			}
		}

		public void Set(LobbySearchCopySearchResultByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LobbyIndex = other.LobbyIndex;
			}
		}

		public void Set(object other)
		{
			Set(other as LobbySearchCopySearchResultByIndexOptions);
		}

		public void Dispose()
		{
		}
	}
}

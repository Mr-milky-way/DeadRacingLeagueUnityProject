using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LobbyDetailsCopyAttributeByIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_AttrIndex;

		public uint AttrIndex
		{
			set
			{
				m_AttrIndex = value;
			}
		}

		public void Set(LobbyDetailsCopyAttributeByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				AttrIndex = other.AttrIndex;
			}
		}

		public void Set(object other)
		{
			Set(other as LobbyDetailsCopyAttributeByIndexOptions);
		}

		public void Dispose()
		{
		}
	}
}

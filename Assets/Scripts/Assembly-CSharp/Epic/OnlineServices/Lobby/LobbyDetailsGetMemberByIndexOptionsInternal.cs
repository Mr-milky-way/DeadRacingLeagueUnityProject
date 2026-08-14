using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LobbyDetailsGetMemberByIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_MemberIndex;

		public uint MemberIndex
		{
			set
			{
				m_MemberIndex = value;
			}
		}

		public void Set(LobbyDetailsGetMemberByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				MemberIndex = other.MemberIndex;
			}
		}

		public void Set(object other)
		{
			Set(other as LobbyDetailsGetMemberByIndexOptions);
		}

		public void Dispose()
		{
		}
	}
}

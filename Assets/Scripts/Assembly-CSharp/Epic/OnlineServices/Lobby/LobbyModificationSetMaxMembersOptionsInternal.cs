using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LobbyModificationSetMaxMembersOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_MaxMembers;

		public uint MaxMembers
		{
			set
			{
				m_MaxMembers = value;
			}
		}

		public void Set(LobbyModificationSetMaxMembersOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				MaxMembers = other.MaxMembers;
			}
		}

		public void Set(object other)
		{
			Set(other as LobbyModificationSetMaxMembersOptions);
		}

		public void Dispose()
		{
		}
	}
}

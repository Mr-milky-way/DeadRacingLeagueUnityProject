using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LobbyModificationSetPermissionLevelOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private LobbyPermissionLevel m_PermissionLevel;

		public LobbyPermissionLevel PermissionLevel
		{
			set
			{
				m_PermissionLevel = value;
			}
		}

		public void Set(LobbyModificationSetPermissionLevelOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				PermissionLevel = other.PermissionLevel;
			}
		}

		public void Set(object other)
		{
			Set(other as LobbyModificationSetPermissionLevelOptions);
		}

		public void Dispose()
		{
		}
	}
}

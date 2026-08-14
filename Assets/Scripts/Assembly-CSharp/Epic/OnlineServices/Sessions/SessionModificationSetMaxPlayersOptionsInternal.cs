using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SessionModificationSetMaxPlayersOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_MaxPlayers;

		public uint MaxPlayers
		{
			set
			{
				m_MaxPlayers = value;
			}
		}

		public void Set(SessionModificationSetMaxPlayersOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				MaxPlayers = other.MaxPlayers;
			}
		}

		public void Set(object other)
		{
			Set(other as SessionModificationSetMaxPlayersOptions);
		}

		public void Dispose()
		{
		}
	}
}

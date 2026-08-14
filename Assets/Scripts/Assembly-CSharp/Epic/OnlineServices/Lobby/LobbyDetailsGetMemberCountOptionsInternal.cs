using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LobbyDetailsGetMemberCountOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		public void Set(LobbyDetailsGetMemberCountOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
			}
		}

		public void Set(object other)
		{
			Set(other as LobbyDetailsGetMemberCountOptions);
		}

		public void Dispose()
		{
		}
	}
}

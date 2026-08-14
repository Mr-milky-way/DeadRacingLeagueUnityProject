using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LobbySearchSetLobbyIdOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LobbyId;

		public string LobbyId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LobbyId, value);
			}
		}

		public void Set(LobbySearchSetLobbyIdOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LobbyId = other.LobbyId;
			}
		}

		public void Set(object other)
		{
			Set(other as LobbySearchSetLobbyIdOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LobbyId);
		}
	}
}

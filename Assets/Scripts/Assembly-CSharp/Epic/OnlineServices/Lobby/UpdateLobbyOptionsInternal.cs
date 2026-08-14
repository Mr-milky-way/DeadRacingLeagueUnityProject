using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct UpdateLobbyOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LobbyModificationHandle;

		public LobbyModification LobbyModificationHandle
		{
			set
			{
				Helper.TryMarshalSet(ref m_LobbyModificationHandle, value);
			}
		}

		public void Set(UpdateLobbyOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LobbyModificationHandle = other.LobbyModificationHandle;
			}
		}

		public void Set(object other)
		{
			Set(other as UpdateLobbyOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LobbyModificationHandle);
		}
	}
}

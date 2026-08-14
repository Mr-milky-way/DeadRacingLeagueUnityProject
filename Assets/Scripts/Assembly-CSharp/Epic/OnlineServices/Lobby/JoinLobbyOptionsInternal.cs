using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct JoinLobbyOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LobbyDetailsHandle;

		private IntPtr m_LocalUserId;

		private int m_PresenceEnabled;

		public LobbyDetails LobbyDetailsHandle
		{
			set
			{
				Helper.TryMarshalSet(ref m_LobbyDetailsHandle, value);
			}
		}

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public bool PresenceEnabled
		{
			set
			{
				Helper.TryMarshalSet(ref m_PresenceEnabled, value);
			}
		}

		public void Set(JoinLobbyOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				LobbyDetailsHandle = other.LobbyDetailsHandle;
				LocalUserId = other.LocalUserId;
				PresenceEnabled = other.PresenceEnabled;
			}
		}

		public void Set(object other)
		{
			Set(other as JoinLobbyOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LobbyDetailsHandle);
			Helper.TryMarshalDispose(ref m_LocalUserId);
		}
	}
}

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyLobbyDetailsHandleByInviteIdOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_InviteId;

		public string InviteId
		{
			set
			{
				Helper.TryMarshalSet(ref m_InviteId, value);
			}
		}

		public void Set(CopyLobbyDetailsHandleByInviteIdOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				InviteId = other.InviteId;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyLobbyDetailsHandleByInviteIdOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_InviteId);
		}
	}
}

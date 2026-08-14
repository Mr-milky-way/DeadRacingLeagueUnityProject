using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LobbyModificationSetInvitesAllowedOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private int m_InvitesAllowed;

		public bool InvitesAllowed
		{
			set
			{
				Helper.TryMarshalSet(ref m_InvitesAllowed, value);
			}
		}

		public void Set(LobbyModificationSetInvitesAllowedOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				InvitesAllowed = other.InvitesAllowed;
			}
		}

		public void Set(object other)
		{
			Set(other as LobbyModificationSetInvitesAllowedOptions);
		}

		public void Dispose()
		{
		}
	}
}

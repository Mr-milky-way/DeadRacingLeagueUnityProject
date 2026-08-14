using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct KickMemberOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LobbyId;

		private IntPtr m_LocalUserId;

		private IntPtr m_TargetUserId;

		public string LobbyId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LobbyId, value);
			}
		}

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public ProductUserId TargetUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_TargetUserId, value);
			}
		}

		public void Set(KickMemberOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LobbyId = other.LobbyId;
				LocalUserId = other.LocalUserId;
				TargetUserId = other.TargetUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as KickMemberOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LobbyId);
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_TargetUserId);
		}
	}
}

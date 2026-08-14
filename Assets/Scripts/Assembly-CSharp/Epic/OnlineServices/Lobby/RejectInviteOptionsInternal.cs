using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Lobby
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct RejectInviteOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_InviteId;

		private IntPtr m_LocalUserId;

		public string InviteId
		{
			set
			{
				Helper.TryMarshalSet(ref m_InviteId, value);
			}
		}

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public void Set(RejectInviteOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				InviteId = other.InviteId;
				LocalUserId = other.LocalUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as RejectInviteOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_InviteId);
			Helper.TryMarshalDispose(ref m_LocalUserId);
		}
	}
}

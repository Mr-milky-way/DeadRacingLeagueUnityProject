using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CloseConnectionOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_RemoteUserId;

		private IntPtr m_SocketId;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public ProductUserId RemoteUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_RemoteUserId, value);
			}
		}

		public SocketId SocketId
		{
			set
			{
				Helper.TryMarshalSet<SocketIdInternal, SocketId>(ref m_SocketId, value);
			}
		}

		public void Set(CloseConnectionOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				RemoteUserId = other.RemoteUserId;
				SocketId = other.SocketId;
			}
		}

		public void Set(object other)
		{
			Set(other as CloseConnectionOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_RemoteUserId);
			Helper.TryMarshalDispose(ref m_SocketId);
		}
	}
}

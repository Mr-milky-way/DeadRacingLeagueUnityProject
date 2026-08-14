using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct AddNotifyPeerConnectionRequestOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_SocketId;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public SocketId SocketId
		{
			set
			{
				Helper.TryMarshalSet<SocketIdInternal, SocketId>(ref m_SocketId, value);
			}
		}

		public void Set(AddNotifyPeerConnectionRequestOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				SocketId = other.SocketId;
			}
		}

		public void Set(object other)
		{
			Set(other as AddNotifyPeerConnectionRequestOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_SocketId);
		}
	}
}

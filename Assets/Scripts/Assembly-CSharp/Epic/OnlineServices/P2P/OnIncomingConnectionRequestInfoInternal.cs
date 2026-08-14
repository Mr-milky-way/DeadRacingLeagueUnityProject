using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct OnIncomingConnectionRequestInfoInternal : ICallbackInfoInternal
	{
		private IntPtr m_ClientData;

		private IntPtr m_LocalUserId;

		private IntPtr m_RemoteUserId;

		private IntPtr m_SocketId;

		public object ClientData
		{
			get
			{
				Helper.TryMarshalGet(m_ClientData, out object target);
				return target;
			}
		}

		public IntPtr ClientDataAddress => m_ClientData;

		public ProductUserId LocalUserId
		{
			get
			{
				Helper.TryMarshalGet(m_LocalUserId, out ProductUserId target);
				return target;
			}
		}

		public ProductUserId RemoteUserId
		{
			get
			{
				Helper.TryMarshalGet(m_RemoteUserId, out ProductUserId target);
				return target;
			}
		}

		public SocketId SocketId
		{
			get
			{
				Helper.TryMarshalGet<SocketIdInternal, SocketId>(m_SocketId, out var target);
				return target;
			}
		}
	}
}

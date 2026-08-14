using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct GetNextReceivedPacketSizeOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_RequestedChannel;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public byte? RequestedChannel
		{
			set
			{
				Helper.TryMarshalSet(ref m_RequestedChannel, value);
			}
		}

		public void Set(GetNextReceivedPacketSizeOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				LocalUserId = other.LocalUserId;
				RequestedChannel = other.RequestedChannel;
			}
		}

		public void Set(object other)
		{
			Set(other as GetNextReceivedPacketSizeOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_RequestedChannel);
		}
	}
}

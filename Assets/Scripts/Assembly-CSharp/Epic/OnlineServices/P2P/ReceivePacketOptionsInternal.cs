using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct ReceivePacketOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private uint m_MaxDataSizeBytes;

		private IntPtr m_RequestedChannel;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public uint MaxDataSizeBytes
		{
			set
			{
				m_MaxDataSizeBytes = value;
			}
		}

		public byte? RequestedChannel
		{
			set
			{
				Helper.TryMarshalSet(ref m_RequestedChannel, value);
			}
		}

		public void Set(ReceivePacketOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				LocalUserId = other.LocalUserId;
				MaxDataSizeBytes = other.MaxDataSizeBytes;
				RequestedChannel = other.RequestedChannel;
			}
		}

		public void Set(object other)
		{
			Set(other as ReceivePacketOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_RequestedChannel);
		}
	}
}

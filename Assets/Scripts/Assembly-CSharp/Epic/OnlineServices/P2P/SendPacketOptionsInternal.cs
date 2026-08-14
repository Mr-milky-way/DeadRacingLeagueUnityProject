using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SendPacketOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_RemoteUserId;

		private IntPtr m_SocketId;

		private byte m_Channel;

		private uint m_DataLengthBytes;

		private IntPtr m_Data;

		private int m_AllowDelayedDelivery;

		private PacketReliability m_Reliability;

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

		public byte Channel
		{
			set
			{
				m_Channel = value;
			}
		}

		public byte[] Data
		{
			set
			{
				Helper.TryMarshalSet(ref m_Data, value, out m_DataLengthBytes);
			}
		}

		public bool AllowDelayedDelivery
		{
			set
			{
				Helper.TryMarshalSet(ref m_AllowDelayedDelivery, value);
			}
		}

		public PacketReliability Reliability
		{
			set
			{
				m_Reliability = value;
			}
		}

		public void Set(SendPacketOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				LocalUserId = other.LocalUserId;
				RemoteUserId = other.RemoteUserId;
				SocketId = other.SocketId;
				Channel = other.Channel;
				Data = other.Data;
				AllowDelayedDelivery = other.AllowDelayedDelivery;
				Reliability = other.Reliability;
			}
		}

		public void Set(object other)
		{
			Set(other as SendPacketOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_RemoteUserId);
			Helper.TryMarshalDispose(ref m_SocketId);
			Helper.TryMarshalDispose(ref m_Data);
		}
	}
}

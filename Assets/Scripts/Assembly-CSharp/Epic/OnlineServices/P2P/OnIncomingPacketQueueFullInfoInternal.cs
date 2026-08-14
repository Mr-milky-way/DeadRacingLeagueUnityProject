using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.P2P
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct OnIncomingPacketQueueFullInfoInternal : ICallbackInfoInternal
	{
		private IntPtr m_ClientData;

		private ulong m_PacketQueueMaxSizeBytes;

		private ulong m_PacketQueueCurrentSizeBytes;

		private IntPtr m_OverflowPacketLocalUserId;

		private byte m_OverflowPacketChannel;

		private uint m_OverflowPacketSizeBytes;

		public object ClientData
		{
			get
			{
				Helper.TryMarshalGet(m_ClientData, out object target);
				return target;
			}
		}

		public IntPtr ClientDataAddress => m_ClientData;

		public ulong PacketQueueMaxSizeBytes => m_PacketQueueMaxSizeBytes;

		public ulong PacketQueueCurrentSizeBytes => m_PacketQueueCurrentSizeBytes;

		public ProductUserId OverflowPacketLocalUserId
		{
			get
			{
				Helper.TryMarshalGet(m_OverflowPacketLocalUserId, out ProductUserId target);
				return target;
			}
		}

		public byte OverflowPacketChannel => m_OverflowPacketChannel;

		public uint OverflowPacketSizeBytes => m_OverflowPacketSizeBytes;
	}
}

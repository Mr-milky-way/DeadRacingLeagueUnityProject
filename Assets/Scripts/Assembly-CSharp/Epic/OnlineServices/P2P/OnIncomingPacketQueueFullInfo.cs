namespace Epic.OnlineServices.P2P
{
	public class OnIncomingPacketQueueFullInfo : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public ulong PacketQueueMaxSizeBytes { get; private set; }

		public ulong PacketQueueCurrentSizeBytes { get; private set; }

		public ProductUserId OverflowPacketLocalUserId { get; private set; }

		public byte OverflowPacketChannel { get; private set; }

		public uint OverflowPacketSizeBytes { get; private set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(OnIncomingPacketQueueFullInfoInternal? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				PacketQueueMaxSizeBytes = other.Value.PacketQueueMaxSizeBytes;
				PacketQueueCurrentSizeBytes = other.Value.PacketQueueCurrentSizeBytes;
				OverflowPacketLocalUserId = other.Value.OverflowPacketLocalUserId;
				OverflowPacketChannel = other.Value.OverflowPacketChannel;
				OverflowPacketSizeBytes = other.Value.OverflowPacketSizeBytes;
			}
		}

		public void Set(object other)
		{
			Set(other as OnIncomingPacketQueueFullInfoInternal?);
		}
	}
}

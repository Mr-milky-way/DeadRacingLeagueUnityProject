namespace Epic.OnlineServices.P2P
{
	public class PacketQueueInfo : ISettable
	{
		public ulong IncomingPacketQueueMaxSizeBytes { get; set; }

		public ulong IncomingPacketQueueCurrentSizeBytes { get; set; }

		public ulong IncomingPacketQueueCurrentPacketCount { get; set; }

		public ulong OutgoingPacketQueueMaxSizeBytes { get; set; }

		public ulong OutgoingPacketQueueCurrentSizeBytes { get; set; }

		public ulong OutgoingPacketQueueCurrentPacketCount { get; set; }

		internal void Set(PacketQueueInfoInternal? other)
		{
			if (other.HasValue)
			{
				IncomingPacketQueueMaxSizeBytes = other.Value.IncomingPacketQueueMaxSizeBytes;
				IncomingPacketQueueCurrentSizeBytes = other.Value.IncomingPacketQueueCurrentSizeBytes;
				IncomingPacketQueueCurrentPacketCount = other.Value.IncomingPacketQueueCurrentPacketCount;
				OutgoingPacketQueueMaxSizeBytes = other.Value.OutgoingPacketQueueMaxSizeBytes;
				OutgoingPacketQueueCurrentSizeBytes = other.Value.OutgoingPacketQueueCurrentSizeBytes;
				OutgoingPacketQueueCurrentPacketCount = other.Value.OutgoingPacketQueueCurrentPacketCount;
			}
		}

		public void Set(object other)
		{
			Set(other as PacketQueueInfoInternal?);
		}
	}
}

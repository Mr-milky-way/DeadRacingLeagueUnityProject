namespace Epic.OnlineServices.P2P
{
	public class SetPacketQueueSizeOptions
	{
		public ulong IncomingPacketQueueMaxSizeBytes { get; set; }

		public ulong OutgoingPacketQueueMaxSizeBytes { get; set; }
	}
}

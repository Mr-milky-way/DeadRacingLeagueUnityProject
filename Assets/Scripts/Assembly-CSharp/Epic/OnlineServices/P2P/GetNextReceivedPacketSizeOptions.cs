namespace Epic.OnlineServices.P2P
{
	public class GetNextReceivedPacketSizeOptions
	{
		public ProductUserId LocalUserId { get; set; }

		public byte? RequestedChannel { get; set; }
	}
}

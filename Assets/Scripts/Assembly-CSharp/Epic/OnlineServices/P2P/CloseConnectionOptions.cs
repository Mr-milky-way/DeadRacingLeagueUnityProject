namespace Epic.OnlineServices.P2P
{
	public class CloseConnectionOptions
	{
		public ProductUserId LocalUserId { get; set; }

		public ProductUserId RemoteUserId { get; set; }

		public SocketId SocketId { get; set; }
	}
}

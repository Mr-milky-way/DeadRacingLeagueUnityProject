namespace Epic.OnlineServices.P2P
{
	public class OnIncomingConnectionRequestInfo : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public ProductUserId LocalUserId { get; private set; }

		public ProductUserId RemoteUserId { get; private set; }

		public SocketId SocketId { get; private set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(OnIncomingConnectionRequestInfoInternal? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
				RemoteUserId = other.Value.RemoteUserId;
				SocketId = other.Value.SocketId;
			}
		}

		public void Set(object other)
		{
			Set(other as OnIncomingConnectionRequestInfoInternal?);
		}
	}
}

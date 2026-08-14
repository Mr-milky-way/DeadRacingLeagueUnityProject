namespace Epic.OnlineServices.P2P
{
	public class OnRemoteConnectionClosedInfo : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public ProductUserId LocalUserId { get; private set; }

		public ProductUserId RemoteUserId { get; private set; }

		public SocketId SocketId { get; private set; }

		public ConnectionClosedReason Reason { get; private set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(OnRemoteConnectionClosedInfoInternal? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
				RemoteUserId = other.Value.RemoteUserId;
				SocketId = other.Value.SocketId;
				Reason = other.Value.Reason;
			}
		}

		public void Set(object other)
		{
			Set(other as OnRemoteConnectionClosedInfoInternal?);
		}
	}
}

namespace Epic.OnlineServices.Lobby
{
	public class JoinLobbyAcceptedCallbackInfo : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public ProductUserId LocalUserId { get; private set; }

		public ulong UiEventId { get; private set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(JoinLobbyAcceptedCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
				UiEventId = other.Value.UiEventId;
			}
		}

		public void Set(object other)
		{
			Set(other as JoinLobbyAcceptedCallbackInfoInternal?);
		}
	}
}

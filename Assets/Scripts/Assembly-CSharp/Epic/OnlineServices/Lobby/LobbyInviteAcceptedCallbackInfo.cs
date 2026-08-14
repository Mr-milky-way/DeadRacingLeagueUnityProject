namespace Epic.OnlineServices.Lobby
{
	public class LobbyInviteAcceptedCallbackInfo : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public string InviteId { get; private set; }

		public ProductUserId LocalUserId { get; private set; }

		public ProductUserId TargetUserId { get; private set; }

		public string LobbyId { get; private set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(LobbyInviteAcceptedCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				InviteId = other.Value.InviteId;
				LocalUserId = other.Value.LocalUserId;
				TargetUserId = other.Value.TargetUserId;
				LobbyId = other.Value.LobbyId;
			}
		}

		public void Set(object other)
		{
			Set(other as LobbyInviteAcceptedCallbackInfoInternal?);
		}
	}
}

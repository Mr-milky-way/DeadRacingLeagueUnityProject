namespace Epic.OnlineServices.Lobby
{
	public class LobbyMemberUpdateReceivedCallbackInfo : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public string LobbyId { get; private set; }

		public ProductUserId TargetUserId { get; private set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(LobbyMemberUpdateReceivedCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				LobbyId = other.Value.LobbyId;
				TargetUserId = other.Value.TargetUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as LobbyMemberUpdateReceivedCallbackInfoInternal?);
		}
	}
}

namespace Epic.OnlineServices.Lobby
{
	public class LobbyMemberStatusReceivedCallbackInfo : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public string LobbyId { get; private set; }

		public ProductUserId TargetUserId { get; private set; }

		public LobbyMemberStatus CurrentStatus { get; private set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(LobbyMemberStatusReceivedCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				LobbyId = other.Value.LobbyId;
				TargetUserId = other.Value.TargetUserId;
				CurrentStatus = other.Value.CurrentStatus;
			}
		}

		public void Set(object other)
		{
			Set(other as LobbyMemberStatusReceivedCallbackInfoInternal?);
		}
	}
}

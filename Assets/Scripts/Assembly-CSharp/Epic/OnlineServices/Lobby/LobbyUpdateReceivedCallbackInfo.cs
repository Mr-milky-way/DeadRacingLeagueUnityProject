namespace Epic.OnlineServices.Lobby
{
	public class LobbyUpdateReceivedCallbackInfo : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public string LobbyId { get; private set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(LobbyUpdateReceivedCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				LobbyId = other.Value.LobbyId;
			}
		}

		public void Set(object other)
		{
			Set(other as LobbyUpdateReceivedCallbackInfoInternal?);
		}
	}
}

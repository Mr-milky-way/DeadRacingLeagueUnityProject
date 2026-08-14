namespace Epic.OnlineServices.Lobby
{
	public class DestroyLobbyCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public string LobbyId { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(DestroyLobbyCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
				LobbyId = other.Value.LobbyId;
			}
		}

		public void Set(object other)
		{
			Set(other as DestroyLobbyCallbackInfoInternal?);
		}
	}
}

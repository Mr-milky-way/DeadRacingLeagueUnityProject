namespace Epic.OnlineServices.Lobby
{
	public class RejectInviteCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public string InviteId { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(RejectInviteCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
				InviteId = other.Value.InviteId;
			}
		}

		public void Set(object other)
		{
			Set(other as RejectInviteCallbackInfoInternal?);
		}
	}
}

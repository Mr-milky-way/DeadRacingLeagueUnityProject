namespace Epic.OnlineServices.Sessions
{
	public class UpdateSessionCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public string SessionName { get; private set; }

		public string SessionId { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(UpdateSessionCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
				SessionName = other.Value.SessionName;
				SessionId = other.Value.SessionId;
			}
		}

		public void Set(object other)
		{
			Set(other as UpdateSessionCallbackInfoInternal?);
		}
	}
}

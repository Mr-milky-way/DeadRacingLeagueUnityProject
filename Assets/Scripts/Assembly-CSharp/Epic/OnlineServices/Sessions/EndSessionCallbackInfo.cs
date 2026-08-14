namespace Epic.OnlineServices.Sessions
{
	public class EndSessionCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(EndSessionCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
			}
		}

		public void Set(object other)
		{
			Set(other as EndSessionCallbackInfoInternal?);
		}
	}
}

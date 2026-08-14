namespace Epic.OnlineServices.Auth
{
	public class LinkAccountCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public EpicAccountId LocalUserId { get; private set; }

		public PinGrantInfo PinGrantInfo { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(LinkAccountCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
				PinGrantInfo = other.Value.PinGrantInfo;
			}
		}

		public void Set(object other)
		{
			Set(other as LinkAccountCallbackInfoInternal?);
		}
	}
}

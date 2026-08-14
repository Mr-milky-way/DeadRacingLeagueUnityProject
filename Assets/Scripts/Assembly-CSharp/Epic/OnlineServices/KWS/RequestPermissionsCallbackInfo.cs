namespace Epic.OnlineServices.KWS
{
	public class RequestPermissionsCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public ProductUserId LocalUserId { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(RequestPermissionsCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as RequestPermissionsCallbackInfoInternal?);
		}
	}
}

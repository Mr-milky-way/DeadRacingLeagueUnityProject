namespace Epic.OnlineServices.Connect
{
	public class LoginCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public ProductUserId LocalUserId { get; private set; }

		public ContinuanceToken ContinuanceToken { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(LoginCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
				ContinuanceToken = other.Value.ContinuanceToken;
			}
		}

		public void Set(object other)
		{
			Set(other as LoginCallbackInfoInternal?);
		}
	}
}

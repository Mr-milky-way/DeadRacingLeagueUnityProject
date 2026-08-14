namespace Epic.OnlineServices.Auth
{
	public class LoginCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public EpicAccountId LocalUserId { get; private set; }

		public PinGrantInfo PinGrantInfo { get; private set; }

		public ContinuanceToken ContinuanceToken { get; private set; }

		public AccountFeatureRestrictedInfo AccountFeatureRestrictedInfo { get; private set; }

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
				PinGrantInfo = other.Value.PinGrantInfo;
				ContinuanceToken = other.Value.ContinuanceToken;
				AccountFeatureRestrictedInfo = other.Value.AccountFeatureRestrictedInfo;
			}
		}

		public void Set(object other)
		{
			Set(other as LoginCallbackInfoInternal?);
		}
	}
}

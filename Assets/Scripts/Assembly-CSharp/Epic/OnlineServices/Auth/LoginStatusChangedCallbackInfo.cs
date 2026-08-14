namespace Epic.OnlineServices.Auth
{
	public class LoginStatusChangedCallbackInfo : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public EpicAccountId LocalUserId { get; private set; }

		public LoginStatus PrevStatus { get; private set; }

		public LoginStatus CurrentStatus { get; private set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(LoginStatusChangedCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
				PrevStatus = other.Value.PrevStatus;
				CurrentStatus = other.Value.CurrentStatus;
			}
		}

		public void Set(object other)
		{
			Set(other as LoginStatusChangedCallbackInfoInternal?);
		}
	}
}

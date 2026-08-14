namespace Epic.OnlineServices.Connect
{
	public class LoginStatusChangedCallbackInfo : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public ProductUserId LocalUserId { get; private set; }

		public LoginStatus PreviousStatus { get; private set; }

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
				PreviousStatus = other.Value.PreviousStatus;
				CurrentStatus = other.Value.CurrentStatus;
			}
		}

		public void Set(object other)
		{
			Set(other as LoginStatusChangedCallbackInfoInternal?);
		}
	}
}

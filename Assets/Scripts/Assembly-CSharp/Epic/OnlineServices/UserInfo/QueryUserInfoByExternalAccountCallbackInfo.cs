namespace Epic.OnlineServices.UserInfo
{
	public class QueryUserInfoByExternalAccountCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public EpicAccountId LocalUserId { get; private set; }

		public string ExternalAccountId { get; private set; }

		public ExternalAccountType AccountType { get; private set; }

		public EpicAccountId TargetUserId { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(QueryUserInfoByExternalAccountCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
				ExternalAccountId = other.Value.ExternalAccountId;
				AccountType = other.Value.AccountType;
				TargetUserId = other.Value.TargetUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryUserInfoByExternalAccountCallbackInfoInternal?);
		}
	}
}

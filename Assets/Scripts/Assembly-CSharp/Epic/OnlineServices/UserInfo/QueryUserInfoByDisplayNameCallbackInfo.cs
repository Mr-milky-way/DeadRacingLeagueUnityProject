namespace Epic.OnlineServices.UserInfo
{
	public class QueryUserInfoByDisplayNameCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public EpicAccountId LocalUserId { get; private set; }

		public EpicAccountId TargetUserId { get; private set; }

		public string DisplayName { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(QueryUserInfoByDisplayNameCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
				TargetUserId = other.Value.TargetUserId;
				DisplayName = other.Value.DisplayName;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryUserInfoByDisplayNameCallbackInfoInternal?);
		}
	}
}

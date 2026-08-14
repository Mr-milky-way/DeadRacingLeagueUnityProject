namespace Epic.OnlineServices.UserInfo
{
	public class QueryUserInfoCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public EpicAccountId LocalUserId { get; private set; }

		public EpicAccountId TargetUserId { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(QueryUserInfoCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
				TargetUserId = other.Value.TargetUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryUserInfoCallbackInfoInternal?);
		}
	}
}

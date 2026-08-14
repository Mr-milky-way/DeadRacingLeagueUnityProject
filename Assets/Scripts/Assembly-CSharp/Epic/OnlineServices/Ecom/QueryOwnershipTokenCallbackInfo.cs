namespace Epic.OnlineServices.Ecom
{
	public class QueryOwnershipTokenCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public EpicAccountId LocalUserId { get; private set; }

		public string OwnershipToken { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(QueryOwnershipTokenCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
				OwnershipToken = other.Value.OwnershipToken;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryOwnershipTokenCallbackInfoInternal?);
		}
	}
}

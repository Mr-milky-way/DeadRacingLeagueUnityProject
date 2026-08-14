namespace Epic.OnlineServices.Ecom
{
	public class QueryOwnershipCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public EpicAccountId LocalUserId { get; private set; }

		public ItemOwnership[] ItemOwnership { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(QueryOwnershipCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
				ItemOwnership = other.Value.ItemOwnership;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryOwnershipCallbackInfoInternal?);
		}
	}
}

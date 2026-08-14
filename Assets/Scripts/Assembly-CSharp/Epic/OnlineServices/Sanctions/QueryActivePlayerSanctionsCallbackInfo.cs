namespace Epic.OnlineServices.Sanctions
{
	public class QueryActivePlayerSanctionsCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public ProductUserId TargetUserId { get; private set; }

		public ProductUserId LocalUserId { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(QueryActivePlayerSanctionsCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
				TargetUserId = other.Value.TargetUserId;
				LocalUserId = other.Value.LocalUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryActivePlayerSanctionsCallbackInfoInternal?);
		}
	}
}

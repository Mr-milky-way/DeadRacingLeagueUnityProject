namespace Epic.OnlineServices.TitleStorage
{
	public class QueryFileListCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public ProductUserId LocalUserId { get; private set; }

		public uint FileCount { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(QueryFileListCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
				FileCount = other.Value.FileCount;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryFileListCallbackInfoInternal?);
		}
	}
}

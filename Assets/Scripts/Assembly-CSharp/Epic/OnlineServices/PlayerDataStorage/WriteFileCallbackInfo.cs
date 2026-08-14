namespace Epic.OnlineServices.PlayerDataStorage
{
	public class WriteFileCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public ProductUserId LocalUserId { get; private set; }

		public string Filename { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(WriteFileCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
				Filename = other.Value.Filename;
			}
		}

		public void Set(object other)
		{
			Set(other as WriteFileCallbackInfoInternal?);
		}
	}
}

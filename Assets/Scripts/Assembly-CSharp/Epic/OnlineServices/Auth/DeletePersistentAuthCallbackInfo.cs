namespace Epic.OnlineServices.Auth
{
	public class DeletePersistentAuthCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(DeletePersistentAuthCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
			}
		}

		public void Set(object other)
		{
			Set(other as DeletePersistentAuthCallbackInfoInternal?);
		}
	}
}

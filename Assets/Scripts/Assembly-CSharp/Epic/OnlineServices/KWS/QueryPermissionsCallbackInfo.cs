namespace Epic.OnlineServices.KWS
{
	public class QueryPermissionsCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public ProductUserId LocalUserId { get; private set; }

		public string KWSUserId { get; private set; }

		public string DateOfBirth { get; private set; }

		public bool IsMinor { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(QueryPermissionsCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
				KWSUserId = other.Value.KWSUserId;
				DateOfBirth = other.Value.DateOfBirth;
				IsMinor = other.Value.IsMinor;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryPermissionsCallbackInfoInternal?);
		}
	}
}

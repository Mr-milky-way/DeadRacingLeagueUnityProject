namespace Epic.OnlineServices.KWS
{
	public class PermissionsUpdateReceivedCallbackInfo : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public ProductUserId LocalUserId { get; private set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(PermissionsUpdateReceivedCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as PermissionsUpdateReceivedCallbackInfoInternal?);
		}
	}
}

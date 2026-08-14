namespace Epic.OnlineServices.Sessions
{
	public class JoinSessionAcceptedCallbackInfo : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public ProductUserId LocalUserId { get; private set; }

		public ulong UiEventId { get; private set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(JoinSessionAcceptedCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
				UiEventId = other.Value.UiEventId;
			}
		}

		public void Set(object other)
		{
			Set(other as JoinSessionAcceptedCallbackInfoInternal?);
		}
	}
}

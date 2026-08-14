namespace Epic.OnlineServices.Sessions
{
	public class SessionInviteAcceptedCallbackInfo : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public string SessionId { get; private set; }

		public ProductUserId LocalUserId { get; private set; }

		public ProductUserId TargetUserId { get; private set; }

		public string InviteId { get; private set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(SessionInviteAcceptedCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				SessionId = other.Value.SessionId;
				LocalUserId = other.Value.LocalUserId;
				TargetUserId = other.Value.TargetUserId;
				InviteId = other.Value.InviteId;
			}
		}

		public void Set(object other)
		{
			Set(other as SessionInviteAcceptedCallbackInfoInternal?);
		}
	}
}

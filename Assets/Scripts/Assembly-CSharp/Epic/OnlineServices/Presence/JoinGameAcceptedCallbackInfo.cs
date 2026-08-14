namespace Epic.OnlineServices.Presence
{
	public class JoinGameAcceptedCallbackInfo : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public string JoinInfo { get; private set; }

		public EpicAccountId LocalUserId { get; private set; }

		public EpicAccountId TargetUserId { get; private set; }

		public ulong UiEventId { get; private set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(JoinGameAcceptedCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				JoinInfo = other.Value.JoinInfo;
				LocalUserId = other.Value.LocalUserId;
				TargetUserId = other.Value.TargetUserId;
				UiEventId = other.Value.UiEventId;
			}
		}

		public void Set(object other)
		{
			Set(other as JoinGameAcceptedCallbackInfoInternal?);
		}
	}
}

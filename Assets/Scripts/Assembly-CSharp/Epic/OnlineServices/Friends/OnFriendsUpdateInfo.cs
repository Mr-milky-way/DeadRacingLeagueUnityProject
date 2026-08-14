namespace Epic.OnlineServices.Friends
{
	public class OnFriendsUpdateInfo : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public EpicAccountId LocalUserId { get; private set; }

		public EpicAccountId TargetUserId { get; private set; }

		public FriendsStatus PreviousStatus { get; private set; }

		public FriendsStatus CurrentStatus { get; private set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(OnFriendsUpdateInfoInternal? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				LocalUserId = other.Value.LocalUserId;
				TargetUserId = other.Value.TargetUserId;
				PreviousStatus = other.Value.PreviousStatus;
				CurrentStatus = other.Value.CurrentStatus;
			}
		}

		public void Set(object other)
		{
			Set(other as OnFriendsUpdateInfoInternal?);
		}
	}
}

namespace Epic.OnlineServices.Achievements
{
	public class OnQueryPlayerAchievementsCompleteCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public ProductUserId UserId { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(OnQueryPlayerAchievementsCompleteCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
				UserId = other.Value.UserId;
			}
		}

		public void Set(object other)
		{
			Set(other as OnQueryPlayerAchievementsCompleteCallbackInfoInternal?);
		}
	}
}

namespace Epic.OnlineServices.Achievements
{
	public class OnUnlockAchievementsCompleteCallbackInfo : ICallbackInfo, ISettable
	{
		public Result ResultCode { get; private set; }

		public object ClientData { get; private set; }

		public ProductUserId UserId { get; private set; }

		public uint AchievementsCount { get; private set; }

		public Result? GetResultCode()
		{
			return ResultCode;
		}

		internal void Set(OnUnlockAchievementsCompleteCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ResultCode = other.Value.ResultCode;
				ClientData = other.Value.ClientData;
				UserId = other.Value.UserId;
				AchievementsCount = other.Value.AchievementsCount;
			}
		}

		public void Set(object other)
		{
			Set(other as OnUnlockAchievementsCompleteCallbackInfoInternal?);
		}
	}
}

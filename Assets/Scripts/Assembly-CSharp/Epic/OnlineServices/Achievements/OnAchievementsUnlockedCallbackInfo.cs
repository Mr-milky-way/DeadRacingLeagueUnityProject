namespace Epic.OnlineServices.Achievements
{
	public class OnAchievementsUnlockedCallbackInfo : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public ProductUserId UserId { get; private set; }

		public string[] AchievementIds { get; private set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(OnAchievementsUnlockedCallbackInfoInternal? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				UserId = other.Value.UserId;
				AchievementIds = other.Value.AchievementIds;
			}
		}

		public void Set(object other)
		{
			Set(other as OnAchievementsUnlockedCallbackInfoInternal?);
		}
	}
}

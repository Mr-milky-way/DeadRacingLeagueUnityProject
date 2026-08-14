using System;

namespace Epic.OnlineServices.Achievements
{
	public class OnAchievementsUnlockedCallbackV2Info : ICallbackInfo, ISettable
	{
		public object ClientData { get; private set; }

		public ProductUserId UserId { get; private set; }

		public string AchievementId { get; private set; }

		public DateTimeOffset? UnlockTime { get; private set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(OnAchievementsUnlockedCallbackV2InfoInternal? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				UserId = other.Value.UserId;
				AchievementId = other.Value.AchievementId;
				UnlockTime = other.Value.UnlockTime;
			}
		}

		public void Set(object other)
		{
			Set(other as OnAchievementsUnlockedCallbackV2InfoInternal?);
		}
	}
}

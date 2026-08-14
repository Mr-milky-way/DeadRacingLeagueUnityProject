using System;

namespace Epic.OnlineServices.Achievements
{
	public class UnlockedAchievement : ISettable
	{
		public string AchievementId { get; set; }

		public DateTimeOffset? UnlockTime { get; set; }

		internal void Set(UnlockedAchievementInternal? other)
		{
			if (other.HasValue)
			{
				AchievementId = other.Value.AchievementId;
				UnlockTime = other.Value.UnlockTime;
			}
		}

		public void Set(object other)
		{
			Set(other as UnlockedAchievementInternal?);
		}
	}
}

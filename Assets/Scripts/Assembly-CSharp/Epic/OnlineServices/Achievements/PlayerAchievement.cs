using System;

namespace Epic.OnlineServices.Achievements
{
	public class PlayerAchievement : ISettable
	{
		public string AchievementId { get; set; }

		public double Progress { get; set; }

		public DateTimeOffset? UnlockTime { get; set; }

		public PlayerStatInfo[] StatInfo { get; set; }

		public string DisplayName { get; set; }

		public string Description { get; set; }

		public string IconURL { get; set; }

		public string FlavorText { get; set; }

		internal void Set(PlayerAchievementInternal? other)
		{
			if (other.HasValue)
			{
				AchievementId = other.Value.AchievementId;
				Progress = other.Value.Progress;
				UnlockTime = other.Value.UnlockTime;
				StatInfo = other.Value.StatInfo;
				DisplayName = other.Value.DisplayName;
				Description = other.Value.Description;
				IconURL = other.Value.IconURL;
				FlavorText = other.Value.FlavorText;
			}
		}

		public void Set(object other)
		{
			Set(other as PlayerAchievementInternal?);
		}
	}
}

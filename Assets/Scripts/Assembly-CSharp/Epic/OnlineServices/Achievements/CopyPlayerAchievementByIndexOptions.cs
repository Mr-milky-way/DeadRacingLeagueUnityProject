namespace Epic.OnlineServices.Achievements
{
	public class CopyPlayerAchievementByIndexOptions
	{
		public ProductUserId TargetUserId { get; set; }

		public uint AchievementIndex { get; set; }

		public ProductUserId LocalUserId { get; set; }
	}
}

namespace Epic.OnlineServices.Achievements
{
	public class CopyPlayerAchievementByAchievementIdOptions
	{
		public ProductUserId TargetUserId { get; set; }

		public string AchievementId { get; set; }

		public ProductUserId LocalUserId { get; set; }
	}
}

namespace Epic.OnlineServices.Achievements
{
	public class QueryDefinitionsOptions
	{
		public ProductUserId LocalUserId { get; set; }

		public EpicAccountId EpicUserId_DEPRECATED { get; set; }

		public string[] HiddenAchievementIds_DEPRECATED { get; set; }
	}
}

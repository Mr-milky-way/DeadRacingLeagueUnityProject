namespace Epic.OnlineServices.Leaderboards
{
	public class UserScoresQueryStatInfo : ISettable
	{
		public string StatName { get; set; }

		public LeaderboardAggregation Aggregation { get; set; }

		internal void Set(UserScoresQueryStatInfoInternal? other)
		{
			if (other.HasValue)
			{
				StatName = other.Value.StatName;
				Aggregation = other.Value.Aggregation;
			}
		}

		public void Set(object other)
		{
			Set(other as UserScoresQueryStatInfoInternal?);
		}
	}
}

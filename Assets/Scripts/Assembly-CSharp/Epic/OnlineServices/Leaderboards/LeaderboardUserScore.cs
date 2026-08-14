namespace Epic.OnlineServices.Leaderboards
{
	public class LeaderboardUserScore : ISettable
	{
		public ProductUserId UserId { get; set; }

		public int Score { get; set; }

		internal void Set(LeaderboardUserScoreInternal? other)
		{
			if (other.HasValue)
			{
				UserId = other.Value.UserId;
				Score = other.Value.Score;
			}
		}

		public void Set(object other)
		{
			Set(other as LeaderboardUserScoreInternal?);
		}
	}
}

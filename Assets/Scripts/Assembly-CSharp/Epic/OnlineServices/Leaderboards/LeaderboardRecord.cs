namespace Epic.OnlineServices.Leaderboards
{
	public class LeaderboardRecord : ISettable
	{
		public ProductUserId UserId { get; set; }

		public uint Rank { get; set; }

		public int Score { get; set; }

		public string UserDisplayName { get; set; }

		internal void Set(LeaderboardRecordInternal? other)
		{
			if (other.HasValue)
			{
				UserId = other.Value.UserId;
				Rank = other.Value.Rank;
				Score = other.Value.Score;
				UserDisplayName = other.Value.UserDisplayName;
			}
		}

		public void Set(object other)
		{
			Set(other as LeaderboardRecordInternal?);
		}
	}
}

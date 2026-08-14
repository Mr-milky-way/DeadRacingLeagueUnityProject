using System;

namespace Epic.OnlineServices.Leaderboards
{
	public class QueryLeaderboardDefinitionsOptions
	{
		public DateTimeOffset? StartTime { get; set; }

		public DateTimeOffset? EndTime { get; set; }

		public ProductUserId LocalUserId { get; set; }
	}
}

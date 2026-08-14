using System;

namespace Epic.OnlineServices.Leaderboards
{
	public class Definition : ISettable
	{
		public string LeaderboardId { get; set; }

		public string StatName { get; set; }

		public LeaderboardAggregation Aggregation { get; set; }

		public DateTimeOffset? StartTime { get; set; }

		public DateTimeOffset? EndTime { get; set; }

		internal void Set(DefinitionInternal? other)
		{
			if (other.HasValue)
			{
				LeaderboardId = other.Value.LeaderboardId;
				StatName = other.Value.StatName;
				Aggregation = other.Value.Aggregation;
				StartTime = other.Value.StartTime;
				EndTime = other.Value.EndTime;
			}
		}

		public void Set(object other)
		{
			Set(other as DefinitionInternal?);
		}
	}
}

using System;

namespace Epic.OnlineServices.Stats
{
	public class Stat : ISettable
	{
		public string Name { get; set; }

		public DateTimeOffset? StartTime { get; set; }

		public DateTimeOffset? EndTime { get; set; }

		public int Value { get; set; }

		internal void Set(StatInternal? other)
		{
			if (other.HasValue)
			{
				Name = other.Value.Name;
				StartTime = other.Value.StartTime;
				EndTime = other.Value.EndTime;
				Value = other.Value.Value;
			}
		}

		public void Set(object other)
		{
			Set(other as StatInternal?);
		}
	}
}

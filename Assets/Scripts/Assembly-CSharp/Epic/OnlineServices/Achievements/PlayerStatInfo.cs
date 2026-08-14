namespace Epic.OnlineServices.Achievements
{
	public class PlayerStatInfo : ISettable
	{
		public string Name { get; set; }

		public int CurrentValue { get; set; }

		public int ThresholdValue { get; set; }

		internal void Set(PlayerStatInfoInternal? other)
		{
			if (other.HasValue)
			{
				Name = other.Value.Name;
				CurrentValue = other.Value.CurrentValue;
				ThresholdValue = other.Value.ThresholdValue;
			}
		}

		public void Set(object other)
		{
			Set(other as PlayerStatInfoInternal?);
		}
	}
}

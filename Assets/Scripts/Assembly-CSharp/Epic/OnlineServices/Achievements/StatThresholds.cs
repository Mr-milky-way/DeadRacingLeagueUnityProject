namespace Epic.OnlineServices.Achievements
{
	public class StatThresholds : ISettable
	{
		public string Name { get; set; }

		public int Threshold { get; set; }

		internal void Set(StatThresholdsInternal? other)
		{
			if (other.HasValue)
			{
				Name = other.Value.Name;
				Threshold = other.Value.Threshold;
			}
		}

		public void Set(object other)
		{
			Set(other as StatThresholdsInternal?);
		}
	}
}

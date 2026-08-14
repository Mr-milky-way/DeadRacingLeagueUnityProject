namespace Epic.OnlineServices.Stats
{
	public class IngestData : ISettable
	{
		public string StatName { get; set; }

		public int IngestAmount { get; set; }

		internal void Set(IngestDataInternal? other)
		{
			if (other.HasValue)
			{
				StatName = other.Value.StatName;
				IngestAmount = other.Value.IngestAmount;
			}
		}

		public void Set(object other)
		{
			Set(other as IngestDataInternal?);
		}
	}
}

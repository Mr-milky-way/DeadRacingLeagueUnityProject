namespace Epic.OnlineServices.Presence
{
	public class DataRecord : ISettable
	{
		public string Key { get; set; }

		public string Value { get; set; }

		internal void Set(DataRecordInternal? other)
		{
			if (other.HasValue)
			{
				Key = other.Value.Key;
				Value = other.Value.Value;
			}
		}

		public void Set(object other)
		{
			Set(other as DataRecordInternal?);
		}
	}
}

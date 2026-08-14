namespace Epic.OnlineServices.Sessions
{
	public class AttributeData : ISettable
	{
		public string Key { get; set; }

		public AttributeDataValue Value { get; set; }

		internal void Set(AttributeDataInternal? other)
		{
			if (other.HasValue)
			{
				Key = other.Value.Key;
				Value = other.Value.Value;
			}
		}

		public void Set(object other)
		{
			Set(other as AttributeDataInternal?);
		}
	}
}

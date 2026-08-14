namespace Epic.OnlineServices.Sessions
{
	public class SessionDetailsAttribute : ISettable
	{
		public AttributeData Data { get; set; }

		public SessionAttributeAdvertisementType AdvertisementType { get; set; }

		internal void Set(SessionDetailsAttributeInternal? other)
		{
			if (other.HasValue)
			{
				Data = other.Value.Data;
				AdvertisementType = other.Value.AdvertisementType;
			}
		}

		public void Set(object other)
		{
			Set(other as SessionDetailsAttributeInternal?);
		}
	}
}

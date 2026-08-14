namespace Epic.OnlineServices.Presence
{
	public class PresenceModificationDataRecordId : ISettable
	{
		public string Key { get; set; }

		internal void Set(PresenceModificationDataRecordIdInternal? other)
		{
			if (other.HasValue)
			{
				Key = other.Value.Key;
			}
		}

		public void Set(object other)
		{
			Set(other as PresenceModificationDataRecordIdInternal?);
		}
	}
}

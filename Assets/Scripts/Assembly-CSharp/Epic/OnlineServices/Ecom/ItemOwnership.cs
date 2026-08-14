namespace Epic.OnlineServices.Ecom
{
	public class ItemOwnership : ISettable
	{
		public string Id { get; set; }

		public OwnershipStatus OwnershipStatus { get; set; }

		internal void Set(ItemOwnershipInternal? other)
		{
			if (other.HasValue)
			{
				Id = other.Value.Id;
				OwnershipStatus = other.Value.OwnershipStatus;
			}
		}

		public void Set(object other)
		{
			Set(other as ItemOwnershipInternal?);
		}
	}
}

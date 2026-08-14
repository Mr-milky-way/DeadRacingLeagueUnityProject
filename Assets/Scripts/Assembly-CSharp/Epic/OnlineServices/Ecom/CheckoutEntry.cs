namespace Epic.OnlineServices.Ecom
{
	public class CheckoutEntry : ISettable
	{
		public string OfferId { get; set; }

		internal void Set(CheckoutEntryInternal? other)
		{
			if (other.HasValue)
			{
				OfferId = other.Value.OfferId;
			}
		}

		public void Set(object other)
		{
			Set(other as CheckoutEntryInternal?);
		}
	}
}

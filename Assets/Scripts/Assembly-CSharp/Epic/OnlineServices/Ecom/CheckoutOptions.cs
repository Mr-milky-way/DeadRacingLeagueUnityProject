namespace Epic.OnlineServices.Ecom
{
	public class CheckoutOptions
	{
		public EpicAccountId LocalUserId { get; set; }

		public string OverrideCatalogNamespace { get; set; }

		public CheckoutEntry[] Entries { get; set; }
	}
}

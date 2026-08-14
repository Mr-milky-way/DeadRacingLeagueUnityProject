namespace Epic.OnlineServices.Ecom
{
	public class QueryEntitlementsOptions
	{
		public EpicAccountId LocalUserId { get; set; }

		public string[] EntitlementNames { get; set; }

		public bool IncludeRedeemed { get; set; }
	}
}

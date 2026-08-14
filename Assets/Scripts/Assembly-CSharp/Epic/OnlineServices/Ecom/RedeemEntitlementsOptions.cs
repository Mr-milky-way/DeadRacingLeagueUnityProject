namespace Epic.OnlineServices.Ecom
{
	public class RedeemEntitlementsOptions
	{
		public EpicAccountId LocalUserId { get; set; }

		public string[] EntitlementIds { get; set; }
	}
}

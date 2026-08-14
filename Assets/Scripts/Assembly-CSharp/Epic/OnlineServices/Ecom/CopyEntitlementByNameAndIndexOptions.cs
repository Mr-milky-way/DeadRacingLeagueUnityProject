namespace Epic.OnlineServices.Ecom
{
	public class CopyEntitlementByNameAndIndexOptions
	{
		public EpicAccountId LocalUserId { get; set; }

		public string EntitlementName { get; set; }

		public uint Index { get; set; }
	}
}

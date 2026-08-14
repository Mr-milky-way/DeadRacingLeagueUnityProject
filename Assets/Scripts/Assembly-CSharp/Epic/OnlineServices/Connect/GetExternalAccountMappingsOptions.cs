namespace Epic.OnlineServices.Connect
{
	public class GetExternalAccountMappingsOptions
	{
		public ProductUserId LocalUserId { get; set; }

		public ExternalAccountType AccountIdType { get; set; }

		public string TargetExternalUserId { get; set; }
	}
}

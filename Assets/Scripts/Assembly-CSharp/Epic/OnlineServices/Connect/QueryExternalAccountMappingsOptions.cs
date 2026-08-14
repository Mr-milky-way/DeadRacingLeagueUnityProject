namespace Epic.OnlineServices.Connect
{
	public class QueryExternalAccountMappingsOptions
	{
		public ProductUserId LocalUserId { get; set; }

		public ExternalAccountType AccountIdType { get; set; }

		public string[] ExternalAccountIds { get; set; }
	}
}

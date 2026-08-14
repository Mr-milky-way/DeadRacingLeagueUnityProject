namespace Epic.OnlineServices.Connect
{
	public class QueryProductUserIdMappingsOptions
	{
		public ProductUserId LocalUserId { get; set; }

		public ExternalAccountType AccountIdType_DEPRECATED { get; set; }

		public ProductUserId[] ProductUserIds { get; set; }
	}
}

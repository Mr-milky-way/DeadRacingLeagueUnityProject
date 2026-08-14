namespace Epic.OnlineServices.Connect
{
	public class TransferDeviceIdAccountOptions
	{
		public ProductUserId PrimaryLocalUserId { get; set; }

		public ProductUserId LocalDeviceUserId { get; set; }

		public ProductUserId ProductUserIdToPreserve { get; set; }
	}
}

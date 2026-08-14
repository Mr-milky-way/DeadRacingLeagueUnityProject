namespace Epic.OnlineServices.KWS
{
	public class RequestPermissionsOptions
	{
		public ProductUserId LocalUserId { get; set; }

		public string[] PermissionKeys { get; set; }
	}
}

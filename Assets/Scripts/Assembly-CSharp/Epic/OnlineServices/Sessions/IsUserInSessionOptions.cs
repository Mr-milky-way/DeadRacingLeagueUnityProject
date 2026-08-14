namespace Epic.OnlineServices.Sessions
{
	public class IsUserInSessionOptions
	{
		public string SessionName { get; set; }

		public ProductUserId TargetUserId { get; set; }
	}
}

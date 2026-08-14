namespace Epic.OnlineServices.Sessions
{
	public class JoinSessionOptions
	{
		public string SessionName { get; set; }

		public SessionDetails SessionHandle { get; set; }

		public ProductUserId LocalUserId { get; set; }

		public bool PresenceEnabled { get; set; }
	}
}

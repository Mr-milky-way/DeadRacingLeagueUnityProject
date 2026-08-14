namespace Epic.OnlineServices.Auth
{
	public class IOSLoginOptions
	{
		public IOSCredentials Credentials { get; set; }

		public AuthScopeFlags ScopeFlags { get; set; }
	}
}

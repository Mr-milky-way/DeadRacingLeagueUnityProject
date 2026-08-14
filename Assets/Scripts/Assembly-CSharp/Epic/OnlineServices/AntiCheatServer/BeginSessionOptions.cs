namespace Epic.OnlineServices.AntiCheatServer
{
	public class BeginSessionOptions
	{
		public uint RegisterTimeoutSeconds { get; set; }

		public string ServerName { get; set; }

		public bool EnableGameplayData { get; set; }

		public ProductUserId LocalUserId { get; set; }
	}
}

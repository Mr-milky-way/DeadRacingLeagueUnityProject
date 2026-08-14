namespace Epic.OnlineServices.Auth
{
	public class IOSCredentials : ISettable
	{
		public string Id { get; set; }

		public string Token { get; set; }

		public LoginCredentialType Type { get; set; }

		public IOSCredentialsSystemAuthCredentialsOptions SystemAuthCredentialsOptions { get; set; }

		public ExternalCredentialType ExternalType { get; set; }

		internal void Set(IOSCredentialsInternal? other)
		{
			if (other.HasValue)
			{
				Id = other.Value.Id;
				Token = other.Value.Token;
				Type = other.Value.Type;
				SystemAuthCredentialsOptions = other.Value.SystemAuthCredentialsOptions;
				ExternalType = other.Value.ExternalType;
			}
		}

		public void Set(object other)
		{
			Set(other as IOSCredentialsInternal?);
		}
	}
}

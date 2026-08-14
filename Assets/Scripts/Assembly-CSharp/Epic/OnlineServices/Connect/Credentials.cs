namespace Epic.OnlineServices.Connect
{
	public class Credentials : ISettable
	{
		public string Token { get; set; }

		public ExternalCredentialType Type { get; set; }

		internal void Set(CredentialsInternal? other)
		{
			if (other.HasValue)
			{
				Token = other.Value.Token;
				Type = other.Value.Type;
			}
		}

		public void Set(object other)
		{
			Set(other as CredentialsInternal?);
		}
	}
}

namespace Epic.OnlineServices.Platform
{
	public class ClientCredentials : ISettable
	{
		public string ClientId { get; set; }

		public string ClientSecret { get; set; }

		internal void Set(ClientCredentialsInternal? other)
		{
			if (other.HasValue)
			{
				ClientId = other.Value.ClientId;
				ClientSecret = other.Value.ClientSecret;
			}
		}

		public void Set(object other)
		{
			Set(other as ClientCredentialsInternal?);
		}
	}
}

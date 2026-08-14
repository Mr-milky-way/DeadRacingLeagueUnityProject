namespace Epic.OnlineServices.Auth
{
	public class Token : ISettable
	{
		public string App { get; set; }

		public string ClientId { get; set; }

		public EpicAccountId AccountId { get; set; }

		public string AccessToken { get; set; }

		public double ExpiresIn { get; set; }

		public string ExpiresAt { get; set; }

		public AuthTokenType AuthType { get; set; }

		public string RefreshToken { get; set; }

		public double RefreshExpiresIn { get; set; }

		public string RefreshExpiresAt { get; set; }

		internal void Set(TokenInternal? other)
		{
			if (other.HasValue)
			{
				App = other.Value.App;
				ClientId = other.Value.ClientId;
				AccountId = other.Value.AccountId;
				AccessToken = other.Value.AccessToken;
				ExpiresIn = other.Value.ExpiresIn;
				ExpiresAt = other.Value.ExpiresAt;
				AuthType = other.Value.AuthType;
				RefreshToken = other.Value.RefreshToken;
				RefreshExpiresIn = other.Value.RefreshExpiresIn;
				RefreshExpiresAt = other.Value.RefreshExpiresAt;
			}
		}

		public void Set(object other)
		{
			Set(other as TokenInternal?);
		}
	}
}

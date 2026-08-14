namespace Epic.OnlineServices.Auth
{
	public class PinGrantInfo : ISettable
	{
		public string UserCode { get; set; }

		public string VerificationURI { get; set; }

		public int ExpiresIn { get; set; }

		public string VerificationURIComplete { get; set; }

		internal void Set(PinGrantInfoInternal? other)
		{
			if (other.HasValue)
			{
				UserCode = other.Value.UserCode;
				VerificationURI = other.Value.VerificationURI;
				ExpiresIn = other.Value.ExpiresIn;
				VerificationURIComplete = other.Value.VerificationURIComplete;
			}
		}

		public void Set(object other)
		{
			Set(other as PinGrantInfoInternal?);
		}
	}
}

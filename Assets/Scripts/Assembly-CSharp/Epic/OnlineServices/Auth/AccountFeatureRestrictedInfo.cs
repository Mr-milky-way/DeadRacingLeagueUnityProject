namespace Epic.OnlineServices.Auth
{
	public class AccountFeatureRestrictedInfo : ISettable
	{
		public string VerificationURI { get; set; }

		internal void Set(AccountFeatureRestrictedInfoInternal? other)
		{
			if (other.HasValue)
			{
				VerificationURI = other.Value.VerificationURI;
			}
		}

		public void Set(object other)
		{
			Set(other as AccountFeatureRestrictedInfoInternal?);
		}
	}
}

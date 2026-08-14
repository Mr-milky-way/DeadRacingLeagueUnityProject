namespace Epic.OnlineServices.UserInfo
{
	public class ExternalUserInfo : ISettable
	{
		public ExternalAccountType AccountType { get; set; }

		public string AccountId { get; set; }

		public string DisplayName { get; set; }

		internal void Set(ExternalUserInfoInternal? other)
		{
			if (other.HasValue)
			{
				AccountType = other.Value.AccountType;
				AccountId = other.Value.AccountId;
				DisplayName = other.Value.DisplayName;
			}
		}

		public void Set(object other)
		{
			Set(other as ExternalUserInfoInternal?);
		}
	}
}

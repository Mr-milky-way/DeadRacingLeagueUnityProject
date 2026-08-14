namespace Epic.OnlineServices.UserInfo
{
	public class UserInfoData : ISettable
	{
		public EpicAccountId UserId { get; set; }

		public string Country { get; set; }

		public string DisplayName { get; set; }

		public string PreferredLanguage { get; set; }

		public string Nickname { get; set; }

		internal void Set(UserInfoDataInternal? other)
		{
			if (other.HasValue)
			{
				UserId = other.Value.UserId;
				Country = other.Value.Country;
				DisplayName = other.Value.DisplayName;
				PreferredLanguage = other.Value.PreferredLanguage;
				Nickname = other.Value.Nickname;
			}
		}

		public void Set(object other)
		{
			Set(other as UserInfoDataInternal?);
		}
	}
}

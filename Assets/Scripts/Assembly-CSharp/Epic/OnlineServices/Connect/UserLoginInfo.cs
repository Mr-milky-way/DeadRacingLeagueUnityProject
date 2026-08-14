namespace Epic.OnlineServices.Connect
{
	public class UserLoginInfo : ISettable
	{
		public string DisplayName { get; set; }

		internal void Set(UserLoginInfoInternal? other)
		{
			if (other.HasValue)
			{
				DisplayName = other.Value.DisplayName;
			}
		}

		public void Set(object other)
		{
			Set(other as UserLoginInfoInternal?);
		}
	}
}

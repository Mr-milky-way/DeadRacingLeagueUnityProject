namespace Epic.OnlineServices.UserInfo
{
	public class GetExternalUserInfoCountOptions
	{
		public EpicAccountId LocalUserId { get; set; }

		public EpicAccountId TargetUserId { get; set; }
	}
}

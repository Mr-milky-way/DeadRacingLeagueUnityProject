namespace Epic.OnlineServices.UserInfo
{
	public class CopyExternalUserInfoByIndexOptions
	{
		public EpicAccountId LocalUserId { get; set; }

		public EpicAccountId TargetUserId { get; set; }

		public uint Index { get; set; }
	}
}

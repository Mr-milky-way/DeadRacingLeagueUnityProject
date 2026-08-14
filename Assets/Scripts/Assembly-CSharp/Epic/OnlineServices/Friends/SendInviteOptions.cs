namespace Epic.OnlineServices.Friends
{
	public class SendInviteOptions
	{
		public EpicAccountId LocalUserId { get; set; }

		public EpicAccountId TargetUserId { get; set; }
	}
}

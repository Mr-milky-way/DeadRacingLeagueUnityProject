namespace Epic.OnlineServices.Friends
{
	public class RejectInviteOptions
	{
		public EpicAccountId LocalUserId { get; set; }

		public EpicAccountId TargetUserId { get; set; }
	}
}

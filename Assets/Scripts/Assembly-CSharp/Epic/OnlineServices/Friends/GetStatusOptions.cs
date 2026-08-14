namespace Epic.OnlineServices.Friends
{
	public class GetStatusOptions
	{
		public EpicAccountId LocalUserId { get; set; }

		public EpicAccountId TargetUserId { get; set; }
	}
}

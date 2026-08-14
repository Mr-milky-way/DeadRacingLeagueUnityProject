namespace Epic.OnlineServices.Presence
{
	public class GetJoinInfoOptions
	{
		public EpicAccountId LocalUserId { get; set; }

		public EpicAccountId TargetUserId { get; set; }
	}
}

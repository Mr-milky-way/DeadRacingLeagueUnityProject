namespace Epic.OnlineServices.Presence
{
	public class QueryPresenceOptions
	{
		public EpicAccountId LocalUserId { get; set; }

		public EpicAccountId TargetUserId { get; set; }
	}
}

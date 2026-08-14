namespace Epic.OnlineServices.Presence
{
	public class SetPresenceOptions
	{
		public EpicAccountId LocalUserId { get; set; }

		public PresenceModification PresenceModificationHandle { get; set; }
	}
}

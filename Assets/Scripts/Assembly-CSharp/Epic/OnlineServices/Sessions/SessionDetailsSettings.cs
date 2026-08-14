namespace Epic.OnlineServices.Sessions
{
	public class SessionDetailsSettings : ISettable
	{
		public string BucketId { get; set; }

		public uint NumPublicConnections { get; set; }

		public bool AllowJoinInProgress { get; set; }

		public OnlineSessionPermissionLevel PermissionLevel { get; set; }

		public bool InvitesAllowed { get; set; }

		internal void Set(SessionDetailsSettingsInternal? other)
		{
			if (other.HasValue)
			{
				BucketId = other.Value.BucketId;
				NumPublicConnections = other.Value.NumPublicConnections;
				AllowJoinInProgress = other.Value.AllowJoinInProgress;
				PermissionLevel = other.Value.PermissionLevel;
				InvitesAllowed = other.Value.InvitesAllowed;
			}
		}

		public void Set(object other)
		{
			Set(other as SessionDetailsSettingsInternal?);
		}
	}
}

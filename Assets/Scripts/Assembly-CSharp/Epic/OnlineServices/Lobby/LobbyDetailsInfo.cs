namespace Epic.OnlineServices.Lobby
{
	public class LobbyDetailsInfo : ISettable
	{
		public string LobbyId { get; set; }

		public ProductUserId LobbyOwnerUserId { get; set; }

		public LobbyPermissionLevel PermissionLevel { get; set; }

		public uint AvailableSlots { get; set; }

		public uint MaxMembers { get; set; }

		public bool AllowInvites { get; set; }

		public string BucketId { get; set; }

		public bool AllowHostMigration { get; set; }

		internal void Set(LobbyDetailsInfoInternal? other)
		{
			if (other.HasValue)
			{
				LobbyId = other.Value.LobbyId;
				LobbyOwnerUserId = other.Value.LobbyOwnerUserId;
				PermissionLevel = other.Value.PermissionLevel;
				AvailableSlots = other.Value.AvailableSlots;
				MaxMembers = other.Value.MaxMembers;
				AllowInvites = other.Value.AllowInvites;
				BucketId = other.Value.BucketId;
				AllowHostMigration = other.Value.AllowHostMigration;
			}
		}

		public void Set(object other)
		{
			Set(other as LobbyDetailsInfoInternal?);
		}
	}
}

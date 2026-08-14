namespace Epic.OnlineServices.Sessions
{
	public class ActiveSessionInfo : ISettable
	{
		public string SessionName { get; set; }

		public ProductUserId LocalUserId { get; set; }

		public OnlineSessionState State { get; set; }

		public SessionDetailsInfo SessionDetails { get; set; }

		internal void Set(ActiveSessionInfoInternal? other)
		{
			if (other.HasValue)
			{
				SessionName = other.Value.SessionName;
				LocalUserId = other.Value.LocalUserId;
				State = other.Value.State;
				SessionDetails = other.Value.SessionDetails;
			}
		}

		public void Set(object other)
		{
			Set(other as ActiveSessionInfoInternal?);
		}
	}
}

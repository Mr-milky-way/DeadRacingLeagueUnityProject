namespace Epic.OnlineServices.Sessions
{
	public class SessionDetailsInfo : ISettable
	{
		public string SessionId { get; set; }

		public string HostAddress { get; set; }

		public uint NumOpenPublicConnections { get; set; }

		public SessionDetailsSettings Settings { get; set; }

		internal void Set(SessionDetailsInfoInternal? other)
		{
			if (other.HasValue)
			{
				SessionId = other.Value.SessionId;
				HostAddress = other.Value.HostAddress;
				NumOpenPublicConnections = other.Value.NumOpenPublicConnections;
				Settings = other.Value.Settings;
			}
		}

		public void Set(object other)
		{
			Set(other as SessionDetailsInfoInternal?);
		}
	}
}

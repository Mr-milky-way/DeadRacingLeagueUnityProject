namespace Epic.OnlineServices.Platform
{
	public class InitializeThreadAffinity : ISettable
	{
		public ulong NetworkWork { get; set; }

		public ulong StorageIo { get; set; }

		public ulong WebSocketIo { get; set; }

		public ulong P2PIo { get; set; }

		public ulong HttpRequestIo { get; set; }

		internal void Set(InitializeThreadAffinityInternal? other)
		{
			if (other.HasValue)
			{
				NetworkWork = other.Value.NetworkWork;
				StorageIo = other.Value.StorageIo;
				WebSocketIo = other.Value.WebSocketIo;
				P2PIo = other.Value.P2PIo;
				HttpRequestIo = other.Value.HttpRequestIo;
			}
		}

		public void Set(object other)
		{
			Set(other as InitializeThreadAffinityInternal?);
		}
	}
}

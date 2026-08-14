namespace Epic.OnlineServices.P2P
{
	public class SocketId : ISettable
	{
		public string SocketName { get; set; }

		internal void Set(SocketIdInternal? other)
		{
			if (other.HasValue)
			{
				SocketName = other.Value.SocketName;
			}
		}

		public void Set(object other)
		{
			Set(other as SocketIdInternal?);
		}
	}
}

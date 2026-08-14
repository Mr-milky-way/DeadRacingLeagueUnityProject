namespace Epic.OnlineServices.Lobby
{
	public class Attribute : ISettable
	{
		public AttributeData Data { get; set; }

		public LobbyAttributeVisibility Visibility { get; set; }

		internal void Set(AttributeInternal? other)
		{
			if (other.HasValue)
			{
				Data = other.Value.Data;
				Visibility = other.Value.Visibility;
			}
		}

		public void Set(object other)
		{
			Set(other as AttributeInternal?);
		}
	}
}

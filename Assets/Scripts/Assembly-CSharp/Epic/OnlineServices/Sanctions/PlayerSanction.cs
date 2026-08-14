namespace Epic.OnlineServices.Sanctions
{
	public class PlayerSanction : ISettable
	{
		public long TimePlaced { get; set; }

		public string Action { get; set; }

		internal void Set(PlayerSanctionInternal? other)
		{
			if (other.HasValue)
			{
				TimePlaced = other.Value.TimePlaced;
				Action = other.Value.Action;
			}
		}

		public void Set(object other)
		{
			Set(other as PlayerSanctionInternal?);
		}
	}
}

namespace Epic.OnlineServices.Mods
{
	public class ModInfo : ISettable
	{
		public ModIdentifier[] Mods { get; set; }

		public ModEnumerationType Type { get; set; }

		internal void Set(ModInfoInternal? other)
		{
			if (other.HasValue)
			{
				Mods = other.Value.Mods;
				Type = other.Value.Type;
			}
		}

		public void Set(object other)
		{
			Set(other as ModInfoInternal?);
		}
	}
}

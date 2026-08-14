namespace Epic.OnlineServices.Mods
{
	public class ModIdentifier : ISettable
	{
		public string NamespaceId { get; set; }

		public string ItemId { get; set; }

		public string ArtifactId { get; set; }

		public string Title { get; set; }

		public string Version { get; set; }

		internal void Set(ModIdentifierInternal? other)
		{
			if (other.HasValue)
			{
				NamespaceId = other.Value.NamespaceId;
				ItemId = other.Value.ItemId;
				ArtifactId = other.Value.ArtifactId;
				Title = other.Value.Title;
				Version = other.Value.Version;
			}
		}

		public void Set(object other)
		{
			Set(other as ModIdentifierInternal?);
		}
	}
}

namespace Epic.OnlineServices.Ecom
{
	public class CatalogRelease : ISettable
	{
		public string[] CompatibleAppIds { get; set; }

		public string[] CompatiblePlatforms { get; set; }

		public string ReleaseNote { get; set; }

		internal void Set(CatalogReleaseInternal? other)
		{
			if (other.HasValue)
			{
				CompatibleAppIds = other.Value.CompatibleAppIds;
				CompatiblePlatforms = other.Value.CompatiblePlatforms;
				ReleaseNote = other.Value.ReleaseNote;
			}
		}

		public void Set(object other)
		{
			Set(other as CatalogReleaseInternal?);
		}
	}
}

namespace Epic.OnlineServices.Ecom
{
	public class CatalogItem : ISettable
	{
		public string CatalogNamespace { get; set; }

		public string Id { get; set; }

		public string EntitlementName { get; set; }

		public string TitleText { get; set; }

		public string DescriptionText { get; set; }

		public string LongDescriptionText { get; set; }

		public string TechnicalDetailsText { get; set; }

		public string DeveloperText { get; set; }

		public EcomItemType ItemType { get; set; }

		public long EntitlementEndTimestamp { get; set; }

		internal void Set(CatalogItemInternal? other)
		{
			if (other.HasValue)
			{
				CatalogNamespace = other.Value.CatalogNamespace;
				Id = other.Value.Id;
				EntitlementName = other.Value.EntitlementName;
				TitleText = other.Value.TitleText;
				DescriptionText = other.Value.DescriptionText;
				LongDescriptionText = other.Value.LongDescriptionText;
				TechnicalDetailsText = other.Value.TechnicalDetailsText;
				DeveloperText = other.Value.DeveloperText;
				ItemType = other.Value.ItemType;
				EntitlementEndTimestamp = other.Value.EntitlementEndTimestamp;
			}
		}

		public void Set(object other)
		{
			Set(other as CatalogItemInternal?);
		}
	}
}

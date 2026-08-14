namespace Epic.OnlineServices.Ecom
{
	public class CatalogOffer : ISettable
	{
		public int ServerIndex { get; set; }

		public string CatalogNamespace { get; set; }

		public string Id { get; set; }

		public string TitleText { get; set; }

		public string DescriptionText { get; set; }

		public string LongDescriptionText { get; set; }

		public string TechnicalDetailsText_DEPRECATED { get; set; }

		public string CurrencyCode { get; set; }

		public Result PriceResult { get; set; }

		public uint OriginalPrice_DEPRECATED { get; set; }

		public uint CurrentPrice_DEPRECATED { get; set; }

		public byte DiscountPercentage { get; set; }

		public long ExpirationTimestamp { get; set; }

		public uint PurchasedCount { get; set; }

		public int PurchaseLimit { get; set; }

		public bool AvailableForPurchase { get; set; }

		public ulong OriginalPrice64 { get; set; }

		public ulong CurrentPrice64 { get; set; }

		internal void Set(CatalogOfferInternal? other)
		{
			if (other.HasValue)
			{
				ServerIndex = other.Value.ServerIndex;
				CatalogNamespace = other.Value.CatalogNamespace;
				Id = other.Value.Id;
				TitleText = other.Value.TitleText;
				DescriptionText = other.Value.DescriptionText;
				LongDescriptionText = other.Value.LongDescriptionText;
				TechnicalDetailsText_DEPRECATED = other.Value.TechnicalDetailsText_DEPRECATED;
				CurrencyCode = other.Value.CurrencyCode;
				PriceResult = other.Value.PriceResult;
				OriginalPrice_DEPRECATED = other.Value.OriginalPrice_DEPRECATED;
				CurrentPrice_DEPRECATED = other.Value.CurrentPrice_DEPRECATED;
				DiscountPercentage = other.Value.DiscountPercentage;
				ExpirationTimestamp = other.Value.ExpirationTimestamp;
				PurchasedCount = other.Value.PurchasedCount;
				PurchaseLimit = other.Value.PurchaseLimit;
				AvailableForPurchase = other.Value.AvailableForPurchase;
				OriginalPrice64 = other.Value.OriginalPrice64;
				CurrentPrice64 = other.Value.CurrentPrice64;
			}
		}

		public void Set(object other)
		{
			Set(other as CatalogOfferInternal?);
		}
	}
}

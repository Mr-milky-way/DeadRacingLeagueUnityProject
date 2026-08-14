using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CatalogOfferInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private int m_ServerIndex;

		private IntPtr m_CatalogNamespace;

		private IntPtr m_Id;

		private IntPtr m_TitleText;

		private IntPtr m_DescriptionText;

		private IntPtr m_LongDescriptionText;

		private IntPtr m_TechnicalDetailsText_DEPRECATED;

		private IntPtr m_CurrencyCode;

		private Result m_PriceResult;

		private uint m_OriginalPrice_DEPRECATED;

		private uint m_CurrentPrice_DEPRECATED;

		private byte m_DiscountPercentage;

		private long m_ExpirationTimestamp;

		private uint m_PurchasedCount;

		private int m_PurchaseLimit;

		private int m_AvailableForPurchase;

		private ulong m_OriginalPrice64;

		private ulong m_CurrentPrice64;

		public int ServerIndex
		{
			get
			{
				return m_ServerIndex;
			}
			set
			{
				m_ServerIndex = value;
			}
		}

		public string CatalogNamespace
		{
			get
			{
				Helper.TryMarshalGet(m_CatalogNamespace, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_CatalogNamespace, value);
			}
		}

		public string Id
		{
			get
			{
				Helper.TryMarshalGet(m_Id, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_Id, value);
			}
		}

		public string TitleText
		{
			get
			{
				Helper.TryMarshalGet(m_TitleText, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_TitleText, value);
			}
		}

		public string DescriptionText
		{
			get
			{
				Helper.TryMarshalGet(m_DescriptionText, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_DescriptionText, value);
			}
		}

		public string LongDescriptionText
		{
			get
			{
				Helper.TryMarshalGet(m_LongDescriptionText, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_LongDescriptionText, value);
			}
		}

		public string TechnicalDetailsText_DEPRECATED
		{
			get
			{
				Helper.TryMarshalGet(m_TechnicalDetailsText_DEPRECATED, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_TechnicalDetailsText_DEPRECATED, value);
			}
		}

		public string CurrencyCode
		{
			get
			{
				Helper.TryMarshalGet(m_CurrencyCode, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_CurrencyCode, value);
			}
		}

		public Result PriceResult
		{
			get
			{
				return m_PriceResult;
			}
			set
			{
				m_PriceResult = value;
			}
		}

		public uint OriginalPrice_DEPRECATED
		{
			get
			{
				return m_OriginalPrice_DEPRECATED;
			}
			set
			{
				m_OriginalPrice_DEPRECATED = value;
			}
		}

		public uint CurrentPrice_DEPRECATED
		{
			get
			{
				return m_CurrentPrice_DEPRECATED;
			}
			set
			{
				m_CurrentPrice_DEPRECATED = value;
			}
		}

		public byte DiscountPercentage
		{
			get
			{
				return m_DiscountPercentage;
			}
			set
			{
				m_DiscountPercentage = value;
			}
		}

		public long ExpirationTimestamp
		{
			get
			{
				return m_ExpirationTimestamp;
			}
			set
			{
				m_ExpirationTimestamp = value;
			}
		}

		public uint PurchasedCount
		{
			get
			{
				return m_PurchasedCount;
			}
			set
			{
				m_PurchasedCount = value;
			}
		}

		public int PurchaseLimit
		{
			get
			{
				return m_PurchaseLimit;
			}
			set
			{
				m_PurchaseLimit = value;
			}
		}

		public bool AvailableForPurchase
		{
			get
			{
				Helper.TryMarshalGet(m_AvailableForPurchase, out var target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_AvailableForPurchase, value);
			}
		}

		public ulong OriginalPrice64
		{
			get
			{
				return m_OriginalPrice64;
			}
			set
			{
				m_OriginalPrice64 = value;
			}
		}

		public ulong CurrentPrice64
		{
			get
			{
				return m_CurrentPrice64;
			}
			set
			{
				m_CurrentPrice64 = value;
			}
		}

		public void Set(CatalogOffer other)
		{
			if (other != null)
			{
				m_ApiVersion = 3;
				ServerIndex = other.ServerIndex;
				CatalogNamespace = other.CatalogNamespace;
				Id = other.Id;
				TitleText = other.TitleText;
				DescriptionText = other.DescriptionText;
				LongDescriptionText = other.LongDescriptionText;
				TechnicalDetailsText_DEPRECATED = other.TechnicalDetailsText_DEPRECATED;
				CurrencyCode = other.CurrencyCode;
				PriceResult = other.PriceResult;
				OriginalPrice_DEPRECATED = other.OriginalPrice_DEPRECATED;
				CurrentPrice_DEPRECATED = other.CurrentPrice_DEPRECATED;
				DiscountPercentage = other.DiscountPercentage;
				ExpirationTimestamp = other.ExpirationTimestamp;
				PurchasedCount = other.PurchasedCount;
				PurchaseLimit = other.PurchaseLimit;
				AvailableForPurchase = other.AvailableForPurchase;
				OriginalPrice64 = other.OriginalPrice64;
				CurrentPrice64 = other.CurrentPrice64;
			}
		}

		public void Set(object other)
		{
			Set(other as CatalogOffer);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_CatalogNamespace);
			Helper.TryMarshalDispose(ref m_Id);
			Helper.TryMarshalDispose(ref m_TitleText);
			Helper.TryMarshalDispose(ref m_DescriptionText);
			Helper.TryMarshalDispose(ref m_LongDescriptionText);
			Helper.TryMarshalDispose(ref m_TechnicalDetailsText_DEPRECATED);
			Helper.TryMarshalDispose(ref m_CurrencyCode);
		}
	}
}

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CatalogItemInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_CatalogNamespace;

		private IntPtr m_Id;

		private IntPtr m_EntitlementName;

		private IntPtr m_TitleText;

		private IntPtr m_DescriptionText;

		private IntPtr m_LongDescriptionText;

		private IntPtr m_TechnicalDetailsText;

		private IntPtr m_DeveloperText;

		private EcomItemType m_ItemType;

		private long m_EntitlementEndTimestamp;

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

		public string EntitlementName
		{
			get
			{
				Helper.TryMarshalGet(m_EntitlementName, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_EntitlementName, value);
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

		public string TechnicalDetailsText
		{
			get
			{
				Helper.TryMarshalGet(m_TechnicalDetailsText, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_TechnicalDetailsText, value);
			}
		}

		public string DeveloperText
		{
			get
			{
				Helper.TryMarshalGet(m_DeveloperText, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_DeveloperText, value);
			}
		}

		public EcomItemType ItemType
		{
			get
			{
				return m_ItemType;
			}
			set
			{
				m_ItemType = value;
			}
		}

		public long EntitlementEndTimestamp
		{
			get
			{
				return m_EntitlementEndTimestamp;
			}
			set
			{
				m_EntitlementEndTimestamp = value;
			}
		}

		public void Set(CatalogItem other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				CatalogNamespace = other.CatalogNamespace;
				Id = other.Id;
				EntitlementName = other.EntitlementName;
				TitleText = other.TitleText;
				DescriptionText = other.DescriptionText;
				LongDescriptionText = other.LongDescriptionText;
				TechnicalDetailsText = other.TechnicalDetailsText;
				DeveloperText = other.DeveloperText;
				ItemType = other.ItemType;
				EntitlementEndTimestamp = other.EntitlementEndTimestamp;
			}
		}

		public void Set(object other)
		{
			Set(other as CatalogItem);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_CatalogNamespace);
			Helper.TryMarshalDispose(ref m_Id);
			Helper.TryMarshalDispose(ref m_EntitlementName);
			Helper.TryMarshalDispose(ref m_TitleText);
			Helper.TryMarshalDispose(ref m_DescriptionText);
			Helper.TryMarshalDispose(ref m_LongDescriptionText);
			Helper.TryMarshalDispose(ref m_TechnicalDetailsText);
			Helper.TryMarshalDispose(ref m_DeveloperText);
		}
	}
}

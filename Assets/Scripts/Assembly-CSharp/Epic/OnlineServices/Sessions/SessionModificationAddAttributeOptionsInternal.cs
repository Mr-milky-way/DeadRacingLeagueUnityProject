using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SessionModificationAddAttributeOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_SessionAttribute;

		private SessionAttributeAdvertisementType m_AdvertisementType;

		public AttributeData SessionAttribute
		{
			set
			{
				Helper.TryMarshalSet<AttributeDataInternal, AttributeData>(ref m_SessionAttribute, value);
			}
		}

		public SessionAttributeAdvertisementType AdvertisementType
		{
			set
			{
				m_AdvertisementType = value;
			}
		}

		public void Set(SessionModificationAddAttributeOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				SessionAttribute = other.SessionAttribute;
				AdvertisementType = other.AdvertisementType;
			}
		}

		public void Set(object other)
		{
			Set(other as SessionModificationAddAttributeOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_SessionAttribute);
		}
	}
}

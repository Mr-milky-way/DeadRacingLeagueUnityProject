using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sessions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct SessionDetailsAttributeInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_Data;

		private SessionAttributeAdvertisementType m_AdvertisementType;

		public AttributeData Data
		{
			get
			{
				Helper.TryMarshalGet<AttributeDataInternal, AttributeData>(m_Data, out var target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet<AttributeDataInternal, AttributeData>(ref m_Data, value);
			}
		}

		public SessionAttributeAdvertisementType AdvertisementType
		{
			get
			{
				return m_AdvertisementType;
			}
			set
			{
				m_AdvertisementType = value;
			}
		}

		public void Set(SessionDetailsAttribute other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				Data = other.Data;
				AdvertisementType = other.AdvertisementType;
			}
		}

		public void Set(object other)
		{
			Set(other as SessionDetailsAttribute);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Data);
		}
	}
}

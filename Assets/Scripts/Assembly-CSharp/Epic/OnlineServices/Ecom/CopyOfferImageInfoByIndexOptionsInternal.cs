using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyOfferImageInfoByIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_OfferId;

		private uint m_ImageInfoIndex;

		public EpicAccountId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public string OfferId
		{
			set
			{
				Helper.TryMarshalSet(ref m_OfferId, value);
			}
		}

		public uint ImageInfoIndex
		{
			set
			{
				m_ImageInfoIndex = value;
			}
		}

		public void Set(CopyOfferImageInfoByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				OfferId = other.OfferId;
				ImageInfoIndex = other.ImageInfoIndex;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyOfferImageInfoByIndexOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_OfferId);
		}
	}
}

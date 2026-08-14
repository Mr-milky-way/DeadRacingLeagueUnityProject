using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyOfferItemByIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_OfferId;

		private uint m_ItemIndex;

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

		public uint ItemIndex
		{
			set
			{
				m_ItemIndex = value;
			}
		}

		public void Set(CopyOfferItemByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				OfferId = other.OfferId;
				ItemIndex = other.ItemIndex;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyOfferItemByIndexOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_OfferId);
		}
	}
}

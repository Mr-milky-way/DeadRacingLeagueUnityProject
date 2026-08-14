using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyOfferByIdOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_OfferId;

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

		public void Set(CopyOfferByIdOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				LocalUserId = other.LocalUserId;
				OfferId = other.OfferId;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyOfferByIdOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_OfferId);
		}
	}
}

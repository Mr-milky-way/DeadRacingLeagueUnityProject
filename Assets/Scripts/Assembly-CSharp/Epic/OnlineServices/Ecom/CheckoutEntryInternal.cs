using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CheckoutEntryInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_OfferId;

		public string OfferId
		{
			get
			{
				Helper.TryMarshalGet(m_OfferId, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_OfferId, value);
			}
		}

		public void Set(CheckoutEntry other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				OfferId = other.OfferId;
			}
		}

		public void Set(object other)
		{
			Set(other as CheckoutEntry);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_OfferId);
		}
	}
}

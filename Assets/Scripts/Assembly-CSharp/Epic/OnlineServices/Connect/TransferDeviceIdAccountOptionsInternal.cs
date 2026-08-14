using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct TransferDeviceIdAccountOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_PrimaryLocalUserId;

		private IntPtr m_LocalDeviceUserId;

		private IntPtr m_ProductUserIdToPreserve;

		public ProductUserId PrimaryLocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_PrimaryLocalUserId, value);
			}
		}

		public ProductUserId LocalDeviceUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalDeviceUserId, value);
			}
		}

		public ProductUserId ProductUserIdToPreserve
		{
			set
			{
				Helper.TryMarshalSet(ref m_ProductUserIdToPreserve, value);
			}
		}

		public void Set(TransferDeviceIdAccountOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				PrimaryLocalUserId = other.PrimaryLocalUserId;
				LocalDeviceUserId = other.LocalDeviceUserId;
				ProductUserIdToPreserve = other.ProductUserIdToPreserve;
			}
		}

		public void Set(object other)
		{
			Set(other as TransferDeviceIdAccountOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_PrimaryLocalUserId);
			Helper.TryMarshalDispose(ref m_LocalDeviceUserId);
			Helper.TryMarshalDispose(ref m_ProductUserIdToPreserve);
		}
	}
}

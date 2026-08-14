using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct QueryProductUserIdMappingsOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private ExternalAccountType m_AccountIdType_DEPRECATED;

		private IntPtr m_ProductUserIds;

		private uint m_ProductUserIdCount;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public ExternalAccountType AccountIdType_DEPRECATED
		{
			set
			{
				m_AccountIdType_DEPRECATED = value;
			}
		}

		public ProductUserId[] ProductUserIds
		{
			set
			{
				Helper.TryMarshalSet(ref m_ProductUserIds, value, out m_ProductUserIdCount);
			}
		}

		public void Set(QueryProductUserIdMappingsOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				LocalUserId = other.LocalUserId;
				AccountIdType_DEPRECATED = other.AccountIdType_DEPRECATED;
				ProductUserIds = other.ProductUserIds;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryProductUserIdMappingsOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_ProductUserIds);
		}
	}
}

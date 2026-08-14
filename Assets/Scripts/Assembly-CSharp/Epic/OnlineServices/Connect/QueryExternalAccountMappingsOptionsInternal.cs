using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct QueryExternalAccountMappingsOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private ExternalAccountType m_AccountIdType;

		private IntPtr m_ExternalAccountIds;

		private uint m_ExternalAccountIdCount;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public ExternalAccountType AccountIdType
		{
			set
			{
				m_AccountIdType = value;
			}
		}

		public string[] ExternalAccountIds
		{
			set
			{
				Helper.TryMarshalSet(ref m_ExternalAccountIds, value, out m_ExternalAccountIdCount, true);
			}
		}

		public void Set(QueryExternalAccountMappingsOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				AccountIdType = other.AccountIdType;
				ExternalAccountIds = other.ExternalAccountIds;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryExternalAccountMappingsOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_ExternalAccountIds);
		}
	}
}

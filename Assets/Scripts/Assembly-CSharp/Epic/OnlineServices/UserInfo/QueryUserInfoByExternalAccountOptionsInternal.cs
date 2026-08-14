using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.UserInfo
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct QueryUserInfoByExternalAccountOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_ExternalAccountId;

		private ExternalAccountType m_AccountType;

		public EpicAccountId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public string ExternalAccountId
		{
			set
			{
				Helper.TryMarshalSet(ref m_ExternalAccountId, value);
			}
		}

		public ExternalAccountType AccountType
		{
			set
			{
				m_AccountType = value;
			}
		}

		public void Set(QueryUserInfoByExternalAccountOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				ExternalAccountId = other.ExternalAccountId;
				AccountType = other.AccountType;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryUserInfoByExternalAccountOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_ExternalAccountId);
		}
	}
}

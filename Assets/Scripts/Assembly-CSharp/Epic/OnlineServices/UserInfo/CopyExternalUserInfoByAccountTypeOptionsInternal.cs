using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.UserInfo
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyExternalUserInfoByAccountTypeOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_TargetUserId;

		private ExternalAccountType m_AccountType;

		public EpicAccountId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public EpicAccountId TargetUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_TargetUserId, value);
			}
		}

		public ExternalAccountType AccountType
		{
			set
			{
				m_AccountType = value;
			}
		}

		public void Set(CopyExternalUserInfoByAccountTypeOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				TargetUserId = other.TargetUserId;
				AccountType = other.AccountType;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyExternalUserInfoByAccountTypeOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_TargetUserId);
		}
	}
}

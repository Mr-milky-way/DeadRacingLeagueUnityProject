using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.UserInfo
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct ExternalUserInfoInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private ExternalAccountType m_AccountType;

		private IntPtr m_AccountId;

		private IntPtr m_DisplayName;

		public ExternalAccountType AccountType
		{
			get
			{
				return m_AccountType;
			}
			set
			{
				m_AccountType = value;
			}
		}

		public string AccountId
		{
			get
			{
				Helper.TryMarshalGet(m_AccountId, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_AccountId, value);
			}
		}

		public string DisplayName
		{
			get
			{
				Helper.TryMarshalGet(m_DisplayName, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_DisplayName, value);
			}
		}

		public void Set(ExternalUserInfo other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				AccountType = other.AccountType;
				AccountId = other.AccountId;
				DisplayName = other.DisplayName;
			}
		}

		public void Set(object other)
		{
			Set(other as ExternalUserInfo);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_AccountId);
			Helper.TryMarshalDispose(ref m_DisplayName);
		}
	}
}

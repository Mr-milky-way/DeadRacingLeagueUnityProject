using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyProductUserExternalAccountByAccountTypeOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_TargetUserId;

		private ExternalAccountType m_AccountIdType;

		public ProductUserId TargetUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_TargetUserId, value);
			}
		}

		public ExternalAccountType AccountIdType
		{
			set
			{
				m_AccountIdType = value;
			}
		}

		public void Set(CopyProductUserExternalAccountByAccountTypeOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				TargetUserId = other.TargetUserId;
				AccountIdType = other.AccountIdType;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyProductUserExternalAccountByAccountTypeOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_TargetUserId);
		}
	}
}

using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct GetExternalAccountMappingsOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private ExternalAccountType m_AccountIdType;

		private IntPtr m_TargetExternalUserId;

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

		public string TargetExternalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_TargetExternalUserId, value);
			}
		}

		public void Set(GetExternalAccountMappingsOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				AccountIdType = other.AccountIdType;
				TargetExternalUserId = other.TargetExternalUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as GetExternalAccountMappingsOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_TargetExternalUserId);
		}
	}
}

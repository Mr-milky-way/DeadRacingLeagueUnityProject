using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct GetProductUserIdMappingOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private ExternalAccountType m_AccountIdType;

		private IntPtr m_TargetProductUserId;

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

		public ProductUserId TargetProductUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_TargetProductUserId, value);
			}
		}

		public void Set(GetProductUserIdMappingOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				AccountIdType = other.AccountIdType;
				TargetProductUserId = other.TargetProductUserId;
			}
		}

		public void Set(object other)
		{
			Set(other as GetProductUserIdMappingOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_TargetProductUserId);
		}
	}
}

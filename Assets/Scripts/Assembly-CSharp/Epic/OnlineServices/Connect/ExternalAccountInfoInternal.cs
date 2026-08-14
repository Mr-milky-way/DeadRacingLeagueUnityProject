using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct ExternalAccountInfoInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_ProductUserId;

		private IntPtr m_DisplayName;

		private IntPtr m_AccountId;

		private ExternalAccountType m_AccountIdType;

		private long m_LastLoginTime;

		public ProductUserId ProductUserId
		{
			get
			{
				Helper.TryMarshalGet(m_ProductUserId, out ProductUserId target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_ProductUserId, value);
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

		public ExternalAccountType AccountIdType
		{
			get
			{
				return m_AccountIdType;
			}
			set
			{
				m_AccountIdType = value;
			}
		}

		public DateTimeOffset? LastLoginTime
		{
			get
			{
				Helper.TryMarshalGet(m_LastLoginTime, out var target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_LastLoginTime, value);
			}
		}

		public void Set(ExternalAccountInfo other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				ProductUserId = other.ProductUserId;
				DisplayName = other.DisplayName;
				AccountId = other.AccountId;
				AccountIdType = other.AccountIdType;
				LastLoginTime = other.LastLoginTime;
			}
		}

		public void Set(object other)
		{
			Set(other as ExternalAccountInfo);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_ProductUserId);
			Helper.TryMarshalDispose(ref m_DisplayName);
			Helper.TryMarshalDispose(ref m_AccountId);
		}
	}
}

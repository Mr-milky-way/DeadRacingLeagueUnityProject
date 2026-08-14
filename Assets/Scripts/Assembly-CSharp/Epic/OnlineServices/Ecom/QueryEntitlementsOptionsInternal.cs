using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct QueryEntitlementsOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_EntitlementNames;

		private uint m_EntitlementNameCount;

		private int m_IncludeRedeemed;

		public EpicAccountId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public string[] EntitlementNames
		{
			set
			{
				Helper.TryMarshalSet(ref m_EntitlementNames, value, out m_EntitlementNameCount);
			}
		}

		public bool IncludeRedeemed
		{
			set
			{
				Helper.TryMarshalSet(ref m_IncludeRedeemed, value);
			}
		}

		public void Set(QueryEntitlementsOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				LocalUserId = other.LocalUserId;
				EntitlementNames = other.EntitlementNames;
				IncludeRedeemed = other.IncludeRedeemed;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryEntitlementsOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_EntitlementNames);
		}
	}
}

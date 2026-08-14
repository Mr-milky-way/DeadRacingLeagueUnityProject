using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct RedeemEntitlementsOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private uint m_EntitlementIdCount;

		private IntPtr m_EntitlementIds;

		public EpicAccountId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public string[] EntitlementIds
		{
			set
			{
				Helper.TryMarshalSet(ref m_EntitlementIds, value, out m_EntitlementIdCount);
			}
		}

		public void Set(RedeemEntitlementsOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				EntitlementIds = other.EntitlementIds;
			}
		}

		public void Set(object other)
		{
			Set(other as RedeemEntitlementsOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_EntitlementIds);
		}
	}
}

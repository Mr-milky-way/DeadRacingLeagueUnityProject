using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyEntitlementByIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private uint m_EntitlementIndex;

		public EpicAccountId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public uint EntitlementIndex
		{
			set
			{
				m_EntitlementIndex = value;
			}
		}

		public void Set(CopyEntitlementByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				EntitlementIndex = other.EntitlementIndex;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyEntitlementByIndexOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
		}
	}
}

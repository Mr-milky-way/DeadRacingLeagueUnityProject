using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct TransactionCopyEntitlementByIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_EntitlementIndex;

		public uint EntitlementIndex
		{
			set
			{
				m_EntitlementIndex = value;
			}
		}

		public void Set(TransactionCopyEntitlementByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				EntitlementIndex = other.EntitlementIndex;
			}
		}

		public void Set(object other)
		{
			Set(other as TransactionCopyEntitlementByIndexOptions);
		}

		public void Dispose()
		{
		}
	}
}

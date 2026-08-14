using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct TransactionGetEntitlementsCountOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		public void Set(TransactionGetEntitlementsCountOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
			}
		}

		public void Set(object other)
		{
			Set(other as TransactionGetEntitlementsCountOptions);
		}

		public void Dispose()
		{
		}
	}
}

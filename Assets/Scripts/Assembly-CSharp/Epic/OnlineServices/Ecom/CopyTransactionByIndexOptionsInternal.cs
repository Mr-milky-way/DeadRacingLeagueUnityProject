using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyTransactionByIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private uint m_TransactionIndex;

		public EpicAccountId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public uint TransactionIndex
		{
			set
			{
				m_TransactionIndex = value;
			}
		}

		public void Set(CopyTransactionByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				TransactionIndex = other.TransactionIndex;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyTransactionByIndexOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
		}
	}
}

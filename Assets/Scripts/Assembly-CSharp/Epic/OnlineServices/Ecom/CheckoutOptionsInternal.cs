using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CheckoutOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_OverrideCatalogNamespace;

		private uint m_EntryCount;

		private IntPtr m_Entries;

		public EpicAccountId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public string OverrideCatalogNamespace
		{
			set
			{
				Helper.TryMarshalSet(ref m_OverrideCatalogNamespace, value);
			}
		}

		public CheckoutEntry[] Entries
		{
			set
			{
				Helper.TryMarshalSet<CheckoutEntryInternal, CheckoutEntry>(ref m_Entries, value, out m_EntryCount);
			}
		}

		public void Set(CheckoutOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				OverrideCatalogNamespace = other.OverrideCatalogNamespace;
				Entries = other.Entries;
			}
		}

		public void Set(object other)
		{
			Set(other as CheckoutOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_OverrideCatalogNamespace);
			Helper.TryMarshalDispose(ref m_Entries);
		}
	}
}

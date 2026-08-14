using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct QueryOwnershipOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_CatalogItemIds;

		private uint m_CatalogItemIdCount;

		private IntPtr m_CatalogNamespace;

		public EpicAccountId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public string[] CatalogItemIds
		{
			set
			{
				Helper.TryMarshalSet(ref m_CatalogItemIds, value, out m_CatalogItemIdCount);
			}
		}

		public string CatalogNamespace
		{
			set
			{
				Helper.TryMarshalSet(ref m_CatalogNamespace, value);
			}
		}

		public void Set(QueryOwnershipOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				LocalUserId = other.LocalUserId;
				CatalogItemIds = other.CatalogItemIds;
				CatalogNamespace = other.CatalogNamespace;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryOwnershipOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_CatalogItemIds);
			Helper.TryMarshalDispose(ref m_CatalogNamespace);
		}
	}
}

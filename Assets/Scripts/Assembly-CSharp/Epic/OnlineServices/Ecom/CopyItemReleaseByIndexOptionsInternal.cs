using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyItemReleaseByIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_ItemId;

		private uint m_ReleaseIndex;

		public EpicAccountId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public string ItemId
		{
			set
			{
				Helper.TryMarshalSet(ref m_ItemId, value);
			}
		}

		public uint ReleaseIndex
		{
			set
			{
				m_ReleaseIndex = value;
			}
		}

		public void Set(CopyItemReleaseByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				ItemId = other.ItemId;
				ReleaseIndex = other.ReleaseIndex;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyItemReleaseByIndexOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_ItemId);
		}
	}
}

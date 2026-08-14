using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.TitleStorage
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct QueryFileListOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_ListOfTags;

		private uint m_ListOfTagsCount;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public string[] ListOfTags
		{
			set
			{
				Helper.TryMarshalSet(ref m_ListOfTags, value, out m_ListOfTagsCount);
			}
		}

		public void Set(QueryFileListOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				ListOfTags = other.ListOfTags;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryFileListOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_ListOfTags);
		}
	}
}

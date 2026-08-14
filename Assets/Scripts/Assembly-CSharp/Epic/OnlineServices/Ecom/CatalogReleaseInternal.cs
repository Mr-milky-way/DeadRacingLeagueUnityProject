using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CatalogReleaseInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private uint m_CompatibleAppIdCount;

		private IntPtr m_CompatibleAppIds;

		private uint m_CompatiblePlatformCount;

		private IntPtr m_CompatiblePlatforms;

		private IntPtr m_ReleaseNote;

		public string[] CompatibleAppIds
		{
			get
			{
				Helper.TryMarshalGet<string>(m_CompatibleAppIds, out var target, m_CompatibleAppIdCount, isElementAllocated: true);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_CompatibleAppIds, value, out m_CompatibleAppIdCount, true);
			}
		}

		public string[] CompatiblePlatforms
		{
			get
			{
				Helper.TryMarshalGet<string>(m_CompatiblePlatforms, out var target, m_CompatiblePlatformCount, isElementAllocated: true);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_CompatiblePlatforms, value, out m_CompatiblePlatformCount, true);
			}
		}

		public string ReleaseNote
		{
			get
			{
				Helper.TryMarshalGet(m_ReleaseNote, out string target);
				return target;
			}
			set
			{
				Helper.TryMarshalSet(ref m_ReleaseNote, value);
			}
		}

		public void Set(CatalogRelease other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				CompatibleAppIds = other.CompatibleAppIds;
				CompatiblePlatforms = other.CompatiblePlatforms;
				ReleaseNote = other.ReleaseNote;
			}
		}

		public void Set(object other)
		{
			Set(other as CatalogRelease);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_CompatibleAppIds);
			Helper.TryMarshalDispose(ref m_CompatiblePlatforms);
			Helper.TryMarshalDispose(ref m_ReleaseNote);
		}
	}
}

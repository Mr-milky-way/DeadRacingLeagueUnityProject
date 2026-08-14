using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Stats
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyStatByIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_TargetUserId;

		private uint m_StatIndex;

		public ProductUserId TargetUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_TargetUserId, value);
			}
		}

		public uint StatIndex
		{
			set
			{
				m_StatIndex = value;
			}
		}

		public void Set(CopyStatByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				TargetUserId = other.TargetUserId;
				StatIndex = other.StatIndex;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyStatByIndexOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_TargetUserId);
		}
	}
}

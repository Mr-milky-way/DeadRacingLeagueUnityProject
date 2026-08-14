using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Sanctions
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyPlayerSanctionByIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_TargetUserId;

		private uint m_SanctionIndex;

		public ProductUserId TargetUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_TargetUserId, value);
			}
		}

		public uint SanctionIndex
		{
			set
			{
				m_SanctionIndex = value;
			}
		}

		public void Set(CopyPlayerSanctionByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				TargetUserId = other.TargetUserId;
				SanctionIndex = other.SanctionIndex;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyPlayerSanctionByIndexOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_TargetUserId);
		}
	}
}

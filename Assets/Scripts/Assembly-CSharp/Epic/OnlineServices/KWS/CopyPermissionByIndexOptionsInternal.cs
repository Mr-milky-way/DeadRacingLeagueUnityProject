using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.KWS
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyPermissionByIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private uint m_Index;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public uint Index
		{
			set
			{
				m_Index = value;
			}
		}

		public void Set(CopyPermissionByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				Index = other.Index;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyPermissionByIndexOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
		}
	}
}

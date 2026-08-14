using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyProductUserExternalAccountByIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_TargetUserId;

		private uint m_ExternalAccountInfoIndex;

		public ProductUserId TargetUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_TargetUserId, value);
			}
		}

		public uint ExternalAccountInfoIndex
		{
			set
			{
				m_ExternalAccountInfoIndex = value;
			}
		}

		public void Set(CopyProductUserExternalAccountByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				TargetUserId = other.TargetUserId;
				ExternalAccountInfoIndex = other.ExternalAccountInfoIndex;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyProductUserExternalAccountByIndexOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_TargetUserId);
		}
	}
}

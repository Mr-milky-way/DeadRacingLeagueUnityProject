using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Ecom
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct CopyEntitlementByNameAndIndexOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_EntitlementName;

		private uint m_Index;

		public EpicAccountId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public string EntitlementName
		{
			set
			{
				Helper.TryMarshalSet(ref m_EntitlementName, value);
			}
		}

		public uint Index
		{
			set
			{
				m_Index = value;
			}
		}

		public void Set(CopyEntitlementByNameAndIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				EntitlementName = other.EntitlementName;
				Index = other.Index;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyEntitlementByNameAndIndexOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_EntitlementName);
		}
	}
}

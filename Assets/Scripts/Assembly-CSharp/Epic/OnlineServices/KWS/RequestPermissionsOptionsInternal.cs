using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.KWS
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct RequestPermissionsOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private uint m_PermissionKeyCount;

		private IntPtr m_PermissionKeys;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public string[] PermissionKeys
		{
			set
			{
				Helper.TryMarshalSet(ref m_PermissionKeys, value, out m_PermissionKeyCount, true);
			}
		}

		public void Set(RequestPermissionsOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				PermissionKeys = other.PermissionKeys;
			}
		}

		public void Set(object other)
		{
			Set(other as RequestPermissionsOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_PermissionKeys);
		}
	}
}

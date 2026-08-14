using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.KWS
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct GetPermissionByKeyOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_Key;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public string Key
		{
			set
			{
				Helper.TryMarshalSet(ref m_Key, value);
			}
		}

		public void Set(GetPermissionByKeyOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				Key = other.Key;
			}
		}

		public void Set(object other)
		{
			Set(other as GetPermissionByKeyOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_Key);
		}
	}
}

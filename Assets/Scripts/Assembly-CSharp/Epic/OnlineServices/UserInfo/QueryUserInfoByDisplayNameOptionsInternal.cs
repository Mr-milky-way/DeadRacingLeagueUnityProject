using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.UserInfo
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct QueryUserInfoByDisplayNameOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_LocalUserId;

		private IntPtr m_DisplayName;

		public EpicAccountId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public string DisplayName
		{
			set
			{
				Helper.TryMarshalSet(ref m_DisplayName, value);
			}
		}

		public void Set(QueryUserInfoByDisplayNameOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				LocalUserId = other.LocalUserId;
				DisplayName = other.DisplayName;
			}
		}

		public void Set(object other)
		{
			Set(other as QueryUserInfoByDisplayNameOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_DisplayName);
		}
	}
}

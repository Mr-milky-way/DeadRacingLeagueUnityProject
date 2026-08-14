using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Connect
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct LoginOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_Credentials;

		private IntPtr m_UserLoginInfo;

		public Credentials Credentials
		{
			set
			{
				Helper.TryMarshalSet<CredentialsInternal, Credentials>(ref m_Credentials, value);
			}
		}

		public UserLoginInfo UserLoginInfo
		{
			set
			{
				Helper.TryMarshalSet<UserLoginInfoInternal, UserLoginInfo>(ref m_UserLoginInfo, value);
			}
		}

		public void Set(LoginOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				Credentials = other.Credentials;
				UserLoginInfo = other.UserLoginInfo;
			}
		}

		public void Set(object other)
		{
			Set(other as LoginOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Credentials);
			Helper.TryMarshalDispose(ref m_UserLoginInfo);
		}
	}
}

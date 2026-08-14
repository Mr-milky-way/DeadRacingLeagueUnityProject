using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct IOSLoginOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_Credentials;

		private AuthScopeFlags m_ScopeFlags;

		public IOSCredentials Credentials
		{
			set
			{
				Helper.TryMarshalSet<IOSCredentialsInternal, IOSCredentials>(ref m_Credentials, value);
			}
		}

		public AuthScopeFlags ScopeFlags
		{
			set
			{
				m_ScopeFlags = value;
			}
		}

		public void Set(IOSLoginOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 2;
				Credentials = other.Credentials;
				ScopeFlags = other.ScopeFlags;
			}
		}

		public void Set(object other)
		{
			Set(other as IOSLoginOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_Credentials);
		}
	}
}

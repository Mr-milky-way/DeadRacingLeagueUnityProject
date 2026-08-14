using System;
using System.Runtime.InteropServices;

namespace Epic.OnlineServices.Auth
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct VerifyUserAuthOptionsInternal : ISettable, IDisposable
	{
		private int m_ApiVersion;

		private IntPtr m_AuthToken;

		public Token AuthToken
		{
			set
			{
				Helper.TryMarshalSet<TokenInternal, Token>(ref m_AuthToken, value);
			}
		}

		public void Set(VerifyUserAuthOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = 1;
				AuthToken = other.AuthToken;
			}
		}

		public void Set(object other)
		{
			Set(other as VerifyUserAuthOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_AuthToken);
		}
	}
}
